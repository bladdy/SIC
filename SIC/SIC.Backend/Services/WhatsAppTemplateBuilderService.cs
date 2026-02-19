using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using SIC.Shared.Helpers;
using SIC.Shared.Response;
using System.Text.Json;

namespace SIC.Backend.Services
{
    public class WhatsAppTemplateBuilderService : IWhatsAppTemplateBuilderService
    {
        public List<TemplateComponentRequest> BuildComponents(
            WhatsAppTemplate template,
            Invitation invitation,
            Event ev,
            string code)
        {
            var components = new List<TemplateComponentRequest>();

            var structure = JsonSerializer.Deserialize<TemplateStructure>(
            template.StructureJson,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (structure == null)
                return components;

            // HEADER
            if (structure.Header != null)
            {
                var headerValue = GetDynamicValue(structure.Header.Source, invitation, ev, code);

                components.Add(new TemplateComponentRequest
                {
                    Type = "header",
                    Parameters = new List<TemplateParameterRequest>
                    {
                        new TemplateParameterRequest
                        {
                            Type = structure.Header.Type,
                            Link = headerValue
                        }
                    }
                });
            }

            // BODY
            if (structure.Body?.Any() == true)
            {
                var bodyParams = structure.Body.Select(b => new TemplateParameterRequest
                {
                    Type = b.Type,
                    Text = GetDynamicValue(b.Source, invitation, ev, code)
                }).ToList();

                components.Add(new TemplateComponentRequest
                {
                    Type = "body",
                    Parameters = bodyParams
                });
            }

            // BUTTONS
            if (structure.Buttons?.Any() == true)
            {
                foreach (var btn in structure.Buttons)
                {
                    components.Add(new TemplateComponentRequest
                    {
                        Type = "button",
                        SubType = btn.Type,
                        Index = btn.Index,
                        Parameters = new List<TemplateParameterRequest>
                        {
                            new TemplateParameterRequest
                            {
                                Type = "text",
                                Text = GetDynamicValue(btn.Source, invitation, ev, code)
                            }
                        }
                    });
                }
            }

            return components;
        }

        private string GetDynamicValue(string source, Invitation invitation, Event ev, string code)
        {
            return source switch
            {
                "Invitation.Name" => invitation.Name,
                "Invitation.Code" => invitation.Code!,
                "Event.Name" => ev.Name,
                "Event.SubTitle" => ev.SubTitle,
                "Event.CoverImageUrl" => ev.CoverImageUrl ?? "",
                "Invitation.Table" => invitation.Table ?? "",
                "Event.DateFormatted" => FechaHelper.FormatearFechaLargaEspanol(ev.Date),
                "Event.Url" => $"{ev.Url}?codigo={code}",
                "Event.UrlConfirmation" => code,
                _ => ""
            };
        }
    }
}