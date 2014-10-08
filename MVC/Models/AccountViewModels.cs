using MvcHtmlHelpers;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MVC.Security.Models
{

    public class LoginViewModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "El campo {0} es requerido")]
        [Display(Name = "Usuario / Correo electrónico")]
        public string Email { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "El campo {0} es requerido")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        [StringLength(140, ErrorMessage = "{0} debes ser de {1} caracteres como máximo.")]
        public string Password { get; set; }

        [Display(Name = "Recordar contraseña")]
        public bool RememberMe { get; set; }
    }

    public class PasswordChangeRequiredViewModel
    {
        public string Email { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "El campo {0} es requerido")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        [StringLength(140, ErrorMessage = "{0} debes ser de {1} caracteres como máximo.")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Contraseña")]
        [Compare("Password", ErrorMessage = "La contraseña y la confirmación no coinciden")]
        public string ConfirmPassword { get; set; }

        public string CurrentPassword { get; set; }
    }

    public class RegisterViewModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "El campo {0} es requerido")]
        [StringLength(50, ErrorMessage = "{0} debes ser de {1} caracteres como máximo.")]
        [Display(Name = "Usuario")]
        [HFXRemote("IsUserNameAvailable", "UsersAdmin", "Security")]
        public string UserName { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "El campo {0} es requerido")]
        [RegularExpression(@"^([\w\!\#$\%\&\'\*\+\-\/\=\?\^\`{\|\}\~]+\.)*[\w\!\#$\%\&\'\*\+\-\/\=\?\^\`{\|\}\~]+@((((([a-zA-Z0-9]{1}[a-zA-Z0-9\-]{0,62}[a-zA-Z0-9]{1})|[a-zA-Z])\.)+[a-zA-Z]{2,6})|(\d{1,3}\.){3}\d{1,3}(\:\d{1,5})?)$", ErrorMessage = "Ingrese un Email válido")]
        [Display(Name = "Correo electrónico")]
        [HFXRemote("IsEmailAvailable", "UsersAdmin", "Security")]
        public string Email { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "El campo {0} es requerido")]
        [Display(Name = "Contraseña")]
        [StringLength(140, ErrorMessage = "{0} debes ser de {1} caracteres como máximo.")]
        public string Password { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "El campo {0} es requerido")]
        [Display(Name = "Confirmar Contraseña")]
        [Compare("Password", ErrorMessage = "La contraseña y la confirmación no coinciden")]
        public string ConfirmPassword { get; set; }
    }

    public class ResetPasswordViewModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "El campo {0} es requerido")]
        [EmailAddress]
        [Display(Name = "Correo electrónico")]
        public string Email { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "El campo {0} es requerido")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña Actual")]
        [StringLength(140, ErrorMessage = "{0} debes ser de {1} caracteres como máximo.")]
        public string ActualPassword { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "El campo {0} es requerido")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        [HFXRemote("IsPasswordvalid", "Account", "")]
        [StringLength(140, ErrorMessage = "{0} debes ser de {1} caracteres como máximo.")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Contraseña")]
        [Compare("Password", ErrorMessage = "La contraseña y la confirmación no coinciden")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
        public string Id { get; set; }

        public bool HasToChangePassword { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [Display(Name = "Correo electrónico")]
        [RegularExpression(@"^([\w\!\#$\%\&\'\*\+\-\/\=\?\^\`{\|\}\~]+\.)*[\w\!\#$\%\&\'\*\+\-\/\=\?\^\`{\|\}\~]+@((((([a-zA-Z0-9]{1}[a-zA-Z0-9\-]{0,62}[a-zA-Z0-9]{1})|[a-zA-Z])\.)+[a-zA-Z]{2,6})|(\d{1,3}\.){3}\d{1,3}(\:\d{1,5})?)$", ErrorMessage = "Ingrese un Email válido")]
        [HFXRemote("EmailExists", "Account", "")]
        public string Email { get; set; }
    }
}