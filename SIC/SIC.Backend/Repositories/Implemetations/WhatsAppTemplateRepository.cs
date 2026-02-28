using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using SIC.Backend.Data;
using SIC.Backend.Repositories.Interfaces;
using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.Repositories.Implemetations
{
    public class WhatsAppTemplateRepository : IWhatsAppTemplateRepository
    {
        private readonly DataContext _context;

        public WhatsAppTemplateRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<ActionResponse<bool>> AddSentTemplateAsync(int templateNumber, int id)
        {
            var exists = await _context.TemplateSents.AnyAsync(t => t.InvitationId == id && t.TemplateNumber == templateNumber);

            if (exists)
            {
                return new ActionResponse<bool>
                {
                    Success = true,
                    Message = "Ya se envio esta plantilla.",
                    Result = true
                };
            }
            else
            {
                var entity = new TemplateSent
                {
                    TemplateNumber = templateNumber,
                    InvitationId = id
                };
                _context.TemplateSents.Add(entity);
                await _context.SaveChangesAsync();
                return new ActionResponse<bool>
                {
                    Success = true,
                    Result = true
                };
            }
        }

        public async Task<ActionResponse<bool>> CreateTemplates(WhatsAppTemplate entity)
        {
            bool exists = await _context.WhatsAppTemplates.AnyAsync(t => t.Name == entity.Name);
            if (exists)
            {
                return new ActionResponse<bool>
                {
                    Success = false,
                    Message = "Ya existe una plantilla con ese nombre."
                };
            }
            else
            {
                _context.WhatsAppTemplates.Add(entity);
                await _context.SaveChangesAsync();
                return new ActionResponse<bool>
                {
                    Success = true,
                    Result = true
                };
            }
        }

        public async Task<ActionResponse<IEnumerable<WhatsAppTemplate?>>> GetAllAsync()
        {
            var entities = await _context.WhatsAppTemplates.OrderBy(o => o.OrderTemplate).ToListAsync();

            return new ActionResponse<IEnumerable<WhatsAppTemplate?>>
            {
                Success = true,
                Result = entities
            };
        }

        public async Task<WhatsAppTemplate?> GetByNameAsync(string name)
        {
            return await _context.WhatsAppTemplates
                .FirstOrDefaultAsync(t => t.Name == name);
        }
    }
}