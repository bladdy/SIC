using Newtonsoft.Json.Linq;

namespace SIC.Shared.Helpers
{
    public static class WhatsAppTemplateParser
    {
        public static string ReplaceTemplateVariables(string jsonPayload, string templateText)
        {
            if (string.IsNullOrWhiteSpace(jsonPayload) || string.IsNullOrWhiteSpace(templateText))
                return templateText;

            var jObject = JObject.Parse(jsonPayload);

            var bodyComponent = jObject["template"]?["components"]?
                .FirstOrDefault(c => c["type"]?.ToString().ToLower() == "body");

            if (bodyComponent == null)
                return templateText;

            var parameters = bodyComponent["parameters"];
            if (parameters == null)
                return templateText;

            int index = 1;

            foreach (var param in parameters)
            {
                if (param["type"]?.ToString() == "text")
                {
                    var value = param["text"]?.ToString() ?? string.Empty;

                    templateText = templateText.Replace($"{{{{{index}}}}}", value);
                    index++;
                }
            }

            return templateText;
        }
    }
}