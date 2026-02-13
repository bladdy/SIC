using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.Services
{
    public interface IWhatsAppTemplateBuilderService
    {
        List<TemplateComponentRequest> BuildComponents(
            WhatsAppTemplate template,
            Invitation invitation,
            Event ev,
            string code);
    }
}