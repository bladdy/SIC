using Microsoft.EntityFrameworkCore;
using SIC.Backend.Data;
using SIC.Backend.Repositories.Interfaces;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using SIC.Shared.Enums;
using SIC.Shared.Response;

namespace SIC.Backend.Repositories.Implemetations;

public class EventRequirementAnswersRepository : GenericRepository<EventRequirementAnswer>, IEventRequirementAnswersRepository
{
    private readonly DataContext _context;

    public EventRequirementAnswersRepository(DataContext context) : base(context)
    {
        _context = context;
    }

    public async Task<ActionResponse<IEnumerable<EventRequirementAnswer>>> GetByEventIdAsync(int eventId)
    {
        var entities = await _context.EventRequirementAnswers
            .Include(x => x.Requirement)
            .Include(x => x.Images)
            .Where(x => x.EventId == eventId)
            .ToListAsync();

        return new ActionResponse<IEnumerable<EventRequirementAnswer>>
        {
            Success = true,
            Result = entities
        };
    }

    public async Task<ActionResponse<bool>> SaveAllAsync(int eventId, List<EventRequirementAnswerDTO> answers)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            //Si existe que guarde la fecha de modificacion en UpdateAt, y que mantenga la fecha de creacion del existente,
            //y que solo cambien los campos que se modifico porque no se eliminen la fotos por si solo modifican un campo
            var existing = await _context.EventRequirementAnswers
                .Where(x => x.EventId == eventId)
                .ToListAsync();

            _context.EventRequirementAnswers.RemoveRange(existing);

            foreach (var answer in answers)
            {
                var entity = new EventRequirementAnswer
                {
                    EventId = eventId,
                    RequirementId = answer.RequirementId,
                    Value = answer.Value,
                    CreatedAt = DateTime.UtcNow
                };
                _context.EventRequirementAnswers.Add(entity);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new ActionResponse<bool>
            {
                Success = true,
                Result = true
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new ActionResponse<bool>
            {
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<EventRequirementAnswer>> GetByEventAndRequirementAsync(int eventId, int requirementId)
    {
        var entity = await _context.EventRequirementAnswers
            .Include(x => x.Images)
            .FirstOrDefaultAsync(x => x.EventId == eventId && x.RequirementId == requirementId);

        return new ActionResponse<EventRequirementAnswer>
        {
            Success = true,
            Result = entity
        };
    }

    public async Task<ActionResponse<bool>> ClearFieldAsync(int eventId, int requirementId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var answer = await _context.EventRequirementAnswers
                .Include(x => x.Images)
                .FirstOrDefaultAsync(x => x.EventId == eventId && x.RequirementId == requirementId);

            if (answer == null)
            {
                return new ActionResponse<bool>
                {
                    Success = true,
                    Result = true
                };
            }

            _context.EventRequirementImages.RemoveRange(answer.Images);
            await _context.SaveChangesAsync();

            _context.EventRequirementAnswers.Remove(answer);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return new ActionResponse<bool>
            {
                Success = true,
                Result = true
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new ActionResponse<bool>
            {
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<SaveFormResponseDTO>> SaveFormAsync(int eventId, List<EventRequirementAnswerDTO> answers, List<EventRequirementImageDTO> images)
    {
        var ev = await _context.Events.FirstOrDefaultAsync(e => e.Id == eventId);
        if (ev?.EventTypeId == null)
        {
            return new ActionResponse<SaveFormResponseDTO>
            {
                Message = "El evento no tiene tipo asignado."
            };
        }

        var configs = await _context.EventTypeRequirements
            .Include(x => x.Requirement)
            .Where(x => x.EventTypeId == ev.EventTypeId)
            .ToListAsync();

        var imageCounts = images
            .GroupBy(i => i.RequirementId)
            .ToDictionary(g => g.Key, g => g.Count());

        foreach (var config in configs)
        {
            if (config.Requirement?.InputType != RequirementInputType.Image) continue;

            var count = imageCounts.GetValueOrDefault(config.RequirementId);
            var max = config.Requirement.MaxImages;

            if (max > 0 && count > max)
            {
                return new ActionResponse<SaveFormResponseDTO>
                {
                    Message = $"El requisito '{config.Requirement.Name}' solo admite hasta {max} imágenes."
                };
            }
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var existingImages = await _context.EventRequirementImages
                .Include(x => x.RequirementAnswer)
                .Where(x => x.RequirementAnswer!.EventId == eventId)
                .ToListAsync();
            _context.EventRequirementImages.RemoveRange(existingImages);
            await _context.SaveChangesAsync();

            var existingAnswers = await _context.EventRequirementAnswers
                .Where(x => x.EventId == eventId)
                .ToListAsync();
            _context.EventRequirementAnswers.RemoveRange(existingAnswers);
            await _context.SaveChangesAsync();

            var savedAnswers = new List<EventRequirementAnswer>();
            foreach (var answer in answers)
            {
                var entity = new EventRequirementAnswer
                {
                    EventId = eventId,
                    RequirementId = answer.RequirementId,
                    Value = answer.Value,
                    CreatedAt = DateTime.UtcNow
                };
                _context.EventRequirementAnswers.Add(entity);
                savedAnswers.Add(entity);
            }
            await _context.SaveChangesAsync();

            var savedImages = new List<EventRequirementImage>();
            foreach (var imgDto in images)
            {
                var answer = savedAnswers.FirstOrDefault(a => a.RequirementId == imgDto.RequirementId);
                if (answer == null) continue;

                var entity = new EventRequirementImage
                {
                    RequirementAnswerId = answer.Id,
                    FileName = imgDto.FileName,
                    OriginalName = imgDto.OriginalName,
                    Path = imgDto.Path,
                    Order = imgDto.Order
                };
                _context.EventRequirementImages.Add(entity);
                savedImages.Add(entity);
            }
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            var answerMap = savedAnswers.ToDictionary(a => a.Id, a => a.RequirementId);

            var response = new SaveFormResponseDTO
            {
                Answers = savedAnswers.Select(a => new EventRequirementAnswerDTO
                {
                    Id = a.Id,
                    EventId = a.EventId,
                    RequirementId = a.RequirementId,
                    Value = a.Value
                }).ToList(),
                Images = savedImages.Select(i => new EventRequirementImageDTO
                {
                    Id = i.Id,
                    RequirementAnswerId = i.RequirementAnswerId,
                    RequirementId = answerMap.GetValueOrDefault(i.RequirementAnswerId),
                    FileName = i.FileName,
                    OriginalName = i.OriginalName,
                    Path = i.Path,
                    Order = i.Order
                }).ToList()
            };

            return new ActionResponse<SaveFormResponseDTO>
            {
                Success = true,
                Result = response
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new ActionResponse<SaveFormResponseDTO>
            {
                Message = ex.Message
            };
        }
    }
}