using System.Collections.Generic;

namespace WebApp.Models.Components
{
    public class FormConfiguration
    {
        public string ActionUrl { get; set; }
        public string Method { get; set; } = "post";
        public string Title { get; set; }
        

        public List<InputConfiguration> Inputs { get; set; } = new List<InputConfiguration>();

        public ButtonConfiguration Button { get; set; } = new ButtonConfiguration();
    }

    public class InputConfiguration
    {
        public string Name { get; set; }
        public string Label { get; set; }
        public string Type { get; set; } = "text";
        public string Placeholder { get; set; }
        public string Value { get; set; }
    }

    public class ButtonConfiguration
    {
        public string Text { get; set; } = "Enviar";
        public string Variant { get; set; } = "primary";
        public string Icon { get; set; }
        public string Type { get; set; } = "submit";
    }
}
