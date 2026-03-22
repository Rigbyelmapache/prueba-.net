using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Threading.Tasks;

namespace WebApp.TagHelpers
{
    // COMPONENTE BOTÓN
    // Uso: <ui-button icon="bi bi-star" variant="primary">Texto del botón</ui-button>
    [HtmlTargetElement("ui-button")]
    public class UiButtonTagHelper : TagHelper
    {
        public string Icon { get; set; }
        public string Variant { get; set; } = "primary"; // primary, secondary, danger, outline-primary...
        public string Type { get; set; } = "button";
        
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "button";
            output.Attributes.SetAttribute("type", Type);
            
     
            var existingClasses = output.Attributes["class"]?.Value.ToString() ?? "";
            output.Attributes.SetAttribute("class", $"btn btn-{Variant} {existingClasses}".Trim());

         
            var childContent = await output.GetChildContentAsync();
            var content = childContent.GetContent();

          
            if (!string.IsNullOrEmpty(Icon))
            {
                if (string.IsNullOrWhiteSpace(content))
                {
                    output.Content.SetHtmlContent($"<i class=\"{Icon}\"></i>");
                }
                else
                {
                    output.Content.SetHtmlContent($"<i class=\"{Icon} me-2\"></i>{content}");
                }
            }
            else
            {
                output.Content.SetHtmlContent(content);
            }
        }
    }

    // COMPONENTE ENLACE
    // Uso: <ui-link href="/home">Ir al inicio</ui-link>
    // Uso anidado: <ui-link href="/home"><ui-button>Ir al inicio</ui-button></ui-link>
    [HtmlTargetElement("ui-link")]
    public class UiLinkTagHelper : TagHelper
    {
        public string Href { get; set; }
        
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "a";
            output.Attributes.SetAttribute("href", Href ?? "#");
            
            var existingClasses = output.Attributes["class"]?.Value.ToString() ?? "";
            output.Attributes.SetAttribute("class", $"text-decoration-none {existingClasses}".Trim());

            var childContent = await output.GetChildContentAsync();
            output.Content.SetHtmlContent(childContent.GetContent());
        }
    }

    // COMPONENTE INPUT
    // Uso: <ui-input type="email" name="userEmail" label="Correo Electrónico" placeholder="ejemplo@correo.com"></ui-input>
    [HtmlTargetElement("ui-input")]
    public class UiInputTagHelper : TagHelper
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public string Label { get; set; }
        public string Type { get; set; } = "text";
        public string Placeholder { get; set; }
        public string Value { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            
            var existingClasses = output.Attributes["class"]?.Value.ToString() ?? "";
            output.Attributes.SetAttribute("class", $"mb-3 {existingClasses}".Trim());

            var inputId = Id ?? Name ?? "input_" + System.Guid.NewGuid().ToString("N").Substring(0, 6);

            var labelHtml = string.IsNullOrEmpty(Label) 
                ? "" 
                : $"<label for=\"{inputId}\" class=\"form-label fw-bold\">{Label}</label>";

            var placeholderAttr = string.IsNullOrEmpty(Placeholder) ? "" : $"placeholder=\"{Placeholder}\"";
            var nameAttr = string.IsNullOrEmpty(Name) ? "" : $"name=\"{Name}\"";
            var valueAttr = string.IsNullOrEmpty(Value) ? "" : $"value=\"{Value}\"";


            var inputHtml = $"<input type=\"{Type}\" class=\"form-control form-control-lg\" id=\"{inputId}\" {nameAttr} {placeholderAttr} {valueAttr}>";

            output.Content.SetHtmlContent($"{labelHtml}\n{inputHtml}");
        }
    }

    // COMPONENTE SESSION TIMEOUT
    // Uso: <ui-session-timeout idle-minutes="2" countdown-seconds="49"></ui-session-timeout>
    [HtmlTargetElement("ui-session-timeout")]
    public class UiSessionTimeoutTagHelper : TagHelper
    {
        public int IdleMinutes { get; set; } = 20;
        public int CountdownSeconds { get; set; } = 49;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            output.Attributes.SetAttribute("class", "ui-session-timeout-wrapper");

            var modalHtml = $@"
<div class=""modal fade"" id=""sessionTimeoutModal"" tabindex=""-1"" data-bs-backdrop=""static"" data-bs-keyboard=""false"">
  <div class=""modal-dialog modal-dialog-centered"">
    <div class=""modal-content border-0 shadow-lg rounded-4"">
      <div class=""modal-body p-4 p-md-5"">
       <div class=""row align-items-center"">
          
     
          <div class=""col-auto"">
            <div class=""d-flex justify-content-center align-items-center rounded-circle shadow-sm""
                 style=""width: 60px; height: 60px; background: #ffc107;"">
              <i class=""  bi bi-exclamation-triangle-fill text-white fs-3""></i>
            </div>
          </div>

          <!-- Columna 2: Texto -->
          <div class=""col"">
            <h5 class=""fw-bold mb-2"">
              Su sesión está a punto de expirar
            </h5>
            <p class=""text-muted mb-0"">
              Por seguridad, su sesión expirará en 
              <strong id=""sessionCountdown"" class=""text-danger"">{CountdownSeconds}</strong> segundos. 
              Para continuar, seleccione <strong>Extender sesión</strong>.
            </p>
          </div>

        </div>
       
           <div class=""row mt-4"">
               <div class=""col text-end"">
               <button id=""btnExtendSession""
                    class=""btn fw-bold px-4 py-2 shadow-sm""
                    style=""background: #ffc107; color: #000; border-radius: 8px;"">
              Extender sesión
            </button>
                   
               </div>
           </div>



      </div>
    </div>
  </div>
</div>

<script>
document.addEventListener('DOMContentLoaded', function () {{
    let idleTime = 0;
    let countdownInterval;
    let secondsLeft = {CountdownSeconds};
    let isModalOpen = false;
    let sessionModal = null;
    
    // Timer principal de inactividad
    setInterval(function() {{
        if (isModalOpen) return;
        
        idleTime++;
        if (idleTime >= ({IdleMinutes} * 60)) {{ 
            showTimeoutModal();
        }}
    }}, 1000);

    // Escuchar interacciones para reiniciar
    ['mousemove', 'keydown', 'scroll', 'click'].forEach(evt => 
        document.addEventListener(evt, resetTimer)
    );

    function resetTimer() {{
        if (!isModalOpen) {{
            idleTime = 0;
        }}
    }}

    function showTimeoutModal() {{
        isModalOpen = true;
        secondsLeft = {CountdownSeconds};
        document.getElementById('sessionCountdown').innerText = secondsLeft;
        
        if (!sessionModal) {{
            sessionModal = new bootstrap.Modal(document.getElementById('sessionTimeoutModal'));
        }}
        sessionModal.show();
        
        countdownInterval = setInterval(function() {{
            secondsLeft--;
            document.getElementById('sessionCountdown').innerText = secondsLeft;
            if (secondsLeft <= 0) {{
                clearInterval(countdownInterval);
                fetch('/Auth/Logout', {{ method: 'POST' }}).then(() => {{
                    window.location.href = '/Auth/Login?expired=true';
                }});
            }}
        }}, 1000);
    }}
    
    document.getElementById('btnExtendSession').addEventListener('click', function() {{
        clearInterval(countdownInterval);
        isModalOpen = false;
        idleTime = 0;
        if (sessionModal) sessionModal.hide();
    }});
}});
</script>
";
            output.Content.SetHtmlContent(modalHtml);
        }
    }
}
