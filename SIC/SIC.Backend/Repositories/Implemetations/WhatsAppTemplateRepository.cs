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

        public async Task<ActionResponse<IEnumerable<WhatsAppTemplate?>>> GetAllAsync()
        {
            var entities = await _context.WhatsAppTemplates.ToListAsync();

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