using Microsoft.EntityFrameworkCore;
using SIC.Backend.Data;
using SIC.Backend.Repositories.Interfaces;
using SIC.Shared.Entities;

namespace SIC.Backend.Repositories.Implemetations
{
    public class UsuarioWhatsAppConfigRepository
    : IUsuarioWhatsAppConfigRepository
    {
        public readonly DataContext _context;

        public UsuarioWhatsAppConfigRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<UsuarioWhatsAppConfig?> GetByPhoneNumberIdAsync(string phoneNumberId)
        {
            return await _context.UsuarioWhatsAppConfigs
                .FirstOrDefaultAsync(x => x.PhoneNumberId == phoneNumberId);
        }

        public async Task AddAsync(UsuarioWhatsAppConfig config)
        {
            await _context.UsuarioWhatsAppConfigs.AddAsync(config);
        }
    }
}

/*
IUsuarioWhatsAppConfigUnitOfWork
    UsuarioWhatsAppConfigUnitOfWork*/