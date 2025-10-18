using SIC.Shared.Entities;
using System.Globalization;
using System.Reflection;

namespace SIC.Backend.Helpers
{
    public static class MessageFormatter
    {
        public static Message FormatMessage(Message message, Invitation invitation, List<MessageKey> messageKeys)
        {
            if (message == null || invitation == null || messageKeys == null)
                throw new ArgumentNullException("message, invitation o messageKeys no puede ser null");

            // 🔹 Método auxiliar para obtener propiedades incluso anidadas (ej: "Event.Name")
            string GetNestedPropertyValue(object? obj, string propertyPath)
            {
                if (obj == null || string.IsNullOrEmpty(propertyPath))
                    return string.Empty;

                var parts = propertyPath.Split('.');
                object? currentValue = obj;

                foreach (var part in parts)
                {
                    if (currentValue == null)
                        return string.Empty;

                    var prop = currentValue.GetType().GetProperty(part, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    if (prop == null)
                        return string.Empty;

                    currentValue = prop.GetValue(currentValue);
                }

                // 🔸 Formatear valores especiales
                if (currentValue is DateTime date)
                    return date.ToString("dddd dd 'de' MMMM 'de' yyyy", new CultureInfo("es-ES"));

                if (currentValue is TimeSpan time)
                    return DateTime.Today.Add(time).ToString("hh:mm tt", new CultureInfo("es-ES"));

                return currentValue?.ToString() ?? string.Empty;
            }

            // 🔹 Propiedades calculadas o personalizadas
            string GetCustomValue(string key)
            {
                switch (key.ToLower())
                {
                    case "linkinvitation":
                        {
                            // Construye la URL dinámica: miapp.com/mievento/codigoevento/codigoinvitacion
                            var baseUrl = invitation.Event?.Url?.TrimEnd('/') ?? "https://miapp.com";
                            var eventCode = invitation.Event?.Code ?? string.Empty;
                            var invitationCode = invitation.Code ?? string.Empty;

                            // Construcción final
                            return $"{baseUrl}/?e={eventCode}&i={invitationCode}";
                        }
                    default:
                        return string.Empty;
                }
            }

            // 🔹 Reemplazar tokens en el texto
            string ReplaceTokens(string text)
            {
                if (string.IsNullOrEmpty(text))
                    return string.Empty;

                foreach (var key in messageKeys)
                {
                    string replacement;

                    // Si es una propiedad calculada como {linkinvitacion}
                    if (key.PropertyName.Equals("LinkInvitation", StringComparison.OrdinalIgnoreCase))
                    {
                        replacement = GetCustomValue("LinkInvitation");
                    }
                    else
                    {
                        // Propiedades normales (incluso anidadas)
                        replacement = GetNestedPropertyValue(invitation, key.PropertyName);
                    }

                    text = text.Replace(key.Key, replacement ?? string.Empty);
                }

                return text;
            }

            // 🔹 Retornar el mensaje formateado
            return new Message
            {
                Id = message.Id,
                Title = ReplaceTokens(message.Title),
                SubTitle = ReplaceTokens(message.SubTitle),
                MessageInvitation = ReplaceTokens(message.MessageInvitation),
                MessageConfirmation = ReplaceTokens(message.MessageConfirmation),
                CreatedDate = message.CreatedDate,
                EventId = message.EventId,
                Event = message.Event
            };
        }
    }
}