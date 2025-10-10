using Microsoft.EntityFrameworkCore;
using SIC.Backend.Data;
using SIC.Backend.Helpers;
using SIC.Backend.Repositories.Interfaces;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.Repositories.Implemetations;

public class PlanItemRepository : GenericRepository<PlanItem>, IPlanItemRepository
{
    private readonly DataContext _context;

    public PlanItemRepository(DataContext context) : base(context)
    {
        _context = context;
    }

    public override async Task<ActionResponse<IEnumerable<PlanItem>>> GetAsync(PaginationDTO pagination)
    {
        var queryable = _context.PlanItems
            .Include(p => p.Plan)
            .Include(p => p.Item)
            .Where(p => p.PlanId == pagination.Id)
            .AsQueryable();

        return new ActionResponse<IEnumerable<PlanItem>>
        {
            Success = true,
            Result = await queryable.OrderBy(x => x.Item!.Name).Paginate(pagination).ToListAsync()
        };
    }

    public override async Task<ActionResponse<int>> GetTotalRecordAsync(PaginationDTO pagination)
    {
        var queryable = _context.PlanItems
            .Where(p => p.PlanId == pagination.Id)
            .AsQueryable();

        double count = await queryable.CountAsync();
        return new ActionResponse<int>
        {
            Success = true,
            Result = (int)count
        };
    }

    public override async Task<ActionResponse<IEnumerable<PlanItem>>> GetAsync()
    {
        var entities = await _context.PlanItems.Include(p => p.Plan).Include(p => p.Item).ToListAsync();
        return new ActionResponse<IEnumerable<PlanItem>>
        {
            Success = true,
            Result = entities
        };
    }

    public async Task<ActionResponse<IEnumerable<PlanItem>>> GetByIdAsync(int id)
    {
        var entities = await _context.PlanItems.Where(x => x.PlanId == id).ToListAsync();
        if (entities == null)
        {
            return new ActionResponse<IEnumerable<PlanItem>>
            {
                Message = "El registro no existe."
            };
        }
        return new ActionResponse<IEnumerable<PlanItem>>
        {
            Success = true,
            Result = entities
        };
    }

    public async Task<ActionResponse<PlanItem>> AddOrUpdateFullAsync(List<int> itemIds, int planId)
    {
        // Obtener los ítems actuales del plan
        var existingPlanItems = await _context.PlanItems
            .Where(p => p.PlanId == planId)
            .ToListAsync();

        // Obtener los ítems reales desde la BD
        var validItems = await _context.Items
            .Where(i => itemIds.Contains(i.Id))
            .ToListAsync();

        // Si no se encontraron ítems válidos
        if (!validItems.Any())
        {
            return new ActionResponse<PlanItem>
            {
                Success = false,
                Message = "No se encontraron ítems válidos para asignar al plan."
            };
        }

        // Calcular cuáles agregar y cuáles eliminar
        var existingItemIds = existingPlanItems.Select(pi => pi.ItemId).ToList();

        var itemsToAdd = validItems
            .Where(i => !existingItemIds.Contains(i.Id))
            .Select(i => new PlanItem
            {
                PlanId = planId,
                ItemId = i.Id
            })
            .ToList();

        var itemsToRemove = existingPlanItems
            .Where(pi => !itemIds.Contains(pi.ItemId))
            .ToList();

        // Aplicar los cambios
        if (itemsToRemove.Any())
            _context.PlanItems.RemoveRange(itemsToRemove);

        if (itemsToAdd.Any())
            await _context.PlanItems.AddRangeAsync(itemsToAdd);

        await _context.SaveChangesAsync();

        return new ActionResponse<PlanItem>
        {
            Success = true,
            Message = "Los ítems se han asignado o actualizado correctamente."
        };
    }
}