namespace SIC.Shared.Request
{
    // Modelo principal de la plantilla
    public class CreateTemplateModel
    {
        public string Name { get; set; } = string.Empty;
        public string Language { get; set; } = "es_MX";
        public string Category { get; set; } = "UTILITY"; // MARKETING / UTILITY / AUTHENTICATION / SERVICE
        public HeaderModel? Header { get; set; }
        public List<ComponentRequest> Components { get; set; } = new(); // Lista de componentes
        public string? Footer { get; set; }
        public List<string> BodyExamples { get; set; } = new();
        public List<string> BodyExampleTypes { get; set; } = new();
        public List<ButtonModel> Buttons { get; set; } = new();
        public string MediaType { get; set; } = ""; // IMAGE / VIDEO / DOCUMENT
        public string? MediaUrl { get; set; } = "https://invboxv.com/wp-content/uploads/2026/05/WhatsApp-Image-2026-05-28-at-7.22.03-PM.jpeg";
        public string? MediaCaption { get; set; }
    }

    public class BodyExampleParam
    {
        public string ParamName { get; set; } = "";
        public string ExampleValue { get; set; } = "";
    }

    public class HeaderModel
    {
        public string Type { get; set; } = "TEXT"; // TEXT / IMAGE / VIDEO / DOCUMENT
        public string? Text { get; set; }
    }

    public class ButtonModel
    {
        public string Type { get; set; } = "URL"; // URL / QUICK_REPLY
        public string Text { get; set; } = "";
        public string UrlType { get; set; } = "STATIC"; // STATIC / DYNAMIC
        public string? Url { get; set; }
        public string? PhoneNumber { get; set; }
        public string? UrlBase { get; set; }
        public string? DynamicExample { get; set; }
    }

    public class ComponentRequest
    {
        public string Type { get; set; } = ""; // HEADER / BODY / FOOTER / BUTTONS
        public string? Format { get; set; } // TEXT / IMAGE / VIDEO / DOCUMENT
        public string? Text { get; set; } // Para BODY, FOOTER, HEADER TEXT

        //public BodyExample? Example { get; set; } // Para BODY
        public List<BodyExampleParam>? BodyExampleParams { get; set; } // Para BODY

        public List<ButtonRequest>? Buttons { get; set; } // Para BUTTONS
    }

    public class BodyExample
    {
        public List<List<string>> BodyText { get; set; } = new();
    }

    public class ButtonRequest
    {
        public string Type { get; set; } = ""; // URL / QUICK_REPLY
        public string Text { get; set; } = "";
        public string? Url { get; set; }
        public string? UrlType { get; set; } // STATIC / DYNAMIC
        public string? UrlBase { get; set; }
        public string? DynamicExample { get; set; }
        public List<string>? Example { get; set; }
    }
}

/*
 var templateExample = new CreateTemplateModel
{
    Name = "confirmacioncita",
    Language = "es_ES",
    Category = "MARKETING",
    Header = new HeaderModel
    {
        Type = "text",
        Text = "Recordatorio de tu cita"
    },
    Components = new List<ComponentRequest>
    {
        new ComponentRequest
        {
            Type = "body",
            Format = "text",
            Text = "Hola {{nombre}}, tu cita con {{profesional}} es el {{fecha}} a las {{hora}}. Para que estés pendiente de tu cita, te enviaremos un mensaje cuando se acerque la hora.",
            BodyExampleParams = new List<BodyExampleParam>
            {
                new BodyExampleParam { ParamName = "nombre", ExampleValue = "Juan" },
                new BodyExampleParam { ParamName = "profesional", ExampleValue = "Dra. Martínez" },
                new BodyExampleParam { ParamName = "fecha", ExampleValue = "25 de febrero de 2026" },
                new BodyExampleParam { ParamName = "hora", ExampleValue = "10:00 AM" }
            }
        },
        new ComponentRequest
        {
            Type = "buttons",
            Buttons = new List<ButtonRequest>
            {
                new ButtonRequest
                {
                    Type = "url",
                    Text = "Ver ubicación",
                    Url = "https://maps.example.com/salon123",
                    UrlType = "STATIC",
                    UrlBase = "maps.example.com",
                    DynamicExample = "https://maps.example.com/salon123",
                    Example = new List<string> { "https://maps.example.com/salon123" }
                },
                new ButtonRequest
                {
                    Type = "phone_number",
                    Text = "Llamar al salón",
                    UrlType = "STATIC",
                    PhoneNumber = "+5215551234567"
                },
                new ButtonRequest
                {
                    Type = "quick_reply",
                    Text = "Reagendar cita",
                    UrlType = "STATIC"
                }
            }
        }
    },
    Footer = "Gracias por confiar en nosotros",
    MediaType = "image",
    MediaUrl = "https://invboxv.com/wp-content/uploads/2025/10/Save-the-Date-en-la-playa-de-Yucatan.jpg",
    MediaCaption = "Tu cita está confirmada"
};
 */