using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SIC.Backend.Data;
using SIC.Backend.Repositories.Interfaces;
using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.Repositories.Implemetations
{
    public class WhatsAppConfigRepository : GenericRepository<UsuarioWhatsAppConfig>, IWhatsAppConfigRepository
    {
        private readonly DataContext _context;

        public WhatsAppConfigRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public async Task<ActionResponse<UsuarioWhatsAppConfig>> GetByUserIdAsync(string userId)
        {
            var events = await _context.UsuarioWhatsAppConfigs.FirstOrDefaultAsync(x => x.UsuarioId!.Contains(userId));
            if (events == null)
            {
                return new ActionResponse<UsuarioWhatsAppConfig>
                {
                    Success = true,
                    Message = "Usuario WhatsApp Config no existe."
                };
            }
            return new ActionResponse<UsuarioWhatsAppConfig>
            {
                Success = true,
                Result = events
            };
        }

        public async Task<ActionResponse<UsuarioWhatsAppConfig>> AddFullAsync(UsuarioWhatsAppConfig whatsAppConfig)
        {
            try
            {
                // 1️⃣ Validar usuario
                var user = await _context.Users.FindAsync(whatsAppConfig.UsuarioId);
                if (user == null)
                {
                    return new ActionResponse<UsuarioWhatsAppConfig>
                    {
                        Success = false,
                        Message = "Usuario no existe."
                    };
                }

                // 2️⃣ Buscar configuración existente
                var existingConfig = await _context.UsuarioWhatsAppConfigs
                    .FirstOrDefaultAsync(x => x.UsuarioId == whatsAppConfig.UsuarioId);

                if (existingConfig != null)
                {
                    // 🔁 ACTUALIZAR
                    existingConfig.BusinessId = whatsAppConfig.BusinessId;
                    existingConfig.WabaId = whatsAppConfig.WabaId;
                    existingConfig.PhoneNumberId = whatsAppConfig.PhoneNumberId;
                    existingConfig.SystemUserId = whatsAppConfig.SystemUserId;
                    existingConfig.AccessToken = whatsAppConfig.AccessToken;
                    existingConfig.PhoneNumber = whatsAppConfig.PhoneNumber;
                    existingConfig.IsActive = true;
                    existingConfig.TokenExpiresAt = whatsAppConfig.TokenExpiresAt;
                    existingConfig.RevokedAt = null;

                    _context.Update(existingConfig);
                }
                else
                {
                    // ➕ CREAR
                    whatsAppConfig.Usuario = user;
                    whatsAppConfig.IsActive = true;
                    whatsAppConfig.CreatedAt = DateTime.UtcNow;
                    whatsAppConfig.SystemUserId = "";
                    _context.UsuarioWhatsAppConfigs.Add(whatsAppConfig);
                }

                await _context.SaveChangesAsync();

                return new ActionResponse<UsuarioWhatsAppConfig>
                {
                    Success = true,
                    Result = existingConfig ?? whatsAppConfig
                };
            }
            catch (Exception ex)
            {
                return new ActionResponse<UsuarioWhatsAppConfig>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ActionResponse<UsuarioWhatsAppConfig>> UpdateFullAsync(UsuarioWhatsAppConfig whatsAppConfig)
        {
            try
            {
                var user = await _context.Users.FindAsync(whatsAppConfig.UsuarioId);
                if (user == null)
                {
                    return new ActionResponse<UsuarioWhatsAppConfig>
                    {
                        Success = false,
                        Message = "Usuario no existe."
                    };
                }
                whatsAppConfig.Usuario = user;
                _context.Update(whatsAppConfig);
                await _context.SaveChangesAsync();
                return new ActionResponse<UsuarioWhatsAppConfig>
                {
                    Success = true,
                    Result = null
                };
            }
            catch (Exception exception)
            {
                return new ActionResponse<UsuarioWhatsAppConfig>
                {
                    Success = false,
                    Message = exception.Message
                };
            }
        }

        public async Task<ActionResponse<MassiveShippingProgress?>> GetByEventoUserAsync(int eventoId)
        {
            var progress = await _context.MassiveShippingProgresses.FirstOrDefaultAsync(x => x.EventoId == eventoId);
            if (progress == null)
            {
                return new ActionResponse<MassiveShippingProgress?>
                {
                    Success = true,
                    Message = "No hay registros."
                };
            }
            return new ActionResponse<MassiveShippingProgress?>
            {
                Success = true,
                Result = progress
            };
        }

        public async Task<ActionResponse<StripeSettings?>> GetStripeConfig(string env)
        {
            var stripe = await _context.StripeSettings.FirstOrDefaultAsync(x => x.ENVIRONMENT ==env);
            if (stripe == null)
            {
                return new ActionResponse<StripeSettings?>
                {
                    Success = true,
                    Message = "No hay registros."
                };
            }
            return new ActionResponse<StripeSettings?>
            {
                Success = true,
                Result = stripe
            };
        }
    }
}