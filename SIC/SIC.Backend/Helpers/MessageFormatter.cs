using SIC.Shared.Entities;
using System.Reflection;

namespace SIC.Backend.Helpers
{
    public static class MessageFormatter
    {
        public static Message FormatMessage(Message message, Invitation invitation, List<MessageKey> messageKeys)
        {
            if (message == null || invitation == null || messageKeys == null)
                throw new ArgumentNullException("message, invitation o messageKeys no puede ser null");

            // Crear diccionario <PropertyName, valor> desde la invitación
            var invitationDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var prop in invitation.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var value = prop.GetValue(invitation);
                invitationDict[prop.Name] = value?.ToString() ?? string.Empty;
            }
            //ToDo: chequear el formato de las fechas y horas en los mensajes para que sean legibles EN ESPAÑOL

            // Método para reemplazar tokens en un texto
            string ReplaceTokens(string text)
            {
                if (string.IsNullOrEmpty(text))
                    return string.Empty;

                foreach (var key in messageKeys)
                {
                    if (invitationDict.TryGetValue(key.PropertyName, out var replacement))
                    {
                        text = text.Replace(key.Key, replacement);
                    }
                }

                return text;
            }
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