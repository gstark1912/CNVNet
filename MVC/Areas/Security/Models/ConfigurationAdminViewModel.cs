using MvcHtmlHelpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MVC.Areas.Security.Models
{
    public class ConfigurationViewModel
    {
        public int Id { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "El campo {0} es requerido")]
        [Display(Name = "Correo Electrónico")]
        [RegularExpression(@"^([\w\!\#$\%\&\'\*\+\-\/\=\?\^\`{\|\}\~]+\.)*[\w\!\#$\%\&\'\*\+\-\/\=\?\^\`{\|\}\~]+@((((([a-zA-Z0-9]{1}[a-zA-Z0-9\-]{0,62}[a-zA-Z0-9]{1})|[a-zA-Z])\.)+[a-zA-Z]{2,6})|(\d{1,3}\.){3}\d{1,3}(\:\d{1,5})?)$", ErrorMessage = "Ingrese un Correo Electrónico válido")]
        public string CredentialUserName { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "El campo {0} es requerido")]
        [Display(Name = "Contraseña")]
        public string EmailPassword { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "El campo {0} es requerido")]
        [Display(Name = "Servidor SMTP")]
        public string Client { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "El campo {0} es requerido")]
        [Display(Name = "Activar SSL")]
        public bool EnableSSL { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "El campo {0} es requerido")]
        [HFXInteger]
        [Range(1, 65535, ErrorMessage = "El campo {0} debe estar entre {1} y {2}")]
        public int Port { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "El campo {0} es requerido")]
        [Display(Name = "Default Credentials")]
        public bool DefaultCredentials { get; set; }

        [Display(Name = "Caducidad (Días)")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "El campo {0} es requerido")]
        [HFXInteger]
        [Range(0, 999, ErrorMessage = "El campo {0} debe estar entre {1} y {2}")]
        public int ExpirationTimeDays { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "El campo {0} es requerido")]
        [Display(Name = "Cantidad de caracteres mínimo")]
        [Range(6, 999, ErrorMessage = "El campo {0} debe estar entre {1} y {2}")]
        [HFXInteger]
        public int PasswordRequiredLength { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "El campo {0} es requerido")]
        [Display(Name = "Requiere Número")]
        public bool PasswordRequireDigit { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "El campo {0} es requerido")]
        [Display(Name = "Requiere Minúscula")]
        public bool PasswordRequireLowercase { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "El campo {0} es requerido")]
        [Display(Name = "Requiere Mayúscula")]
        public bool PasswordRequireUppercase { get; set; }


        [Display(Name = "Cantidad de Intentos Fallidos Permitidos")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "El campo {0} es requerido")]
        [HFXInteger]
        [Range(0, 999, ErrorMessage = "El campo {0} debe estar entre {1} y {2}")]
        public int MaxFailedAccessCount { get; set; }


        [Display(Name = "Cantidad de Contraseñas Anteriores no Permitidas")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "El campo {0} es requerido")]
        [HFXInteger]
        [Range(0, 999, ErrorMessage = "El campo {0} debe estar entre {1} y {2}")]
        public int PasswordRepeat { get; set; }
    }
}