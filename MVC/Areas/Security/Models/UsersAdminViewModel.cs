using MVC.Security.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using MvcHtmlHelpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC.Areas.Security.Models
{
    public class UsersAdminViewModel
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [Display(Name = "Nombre")]
        [StringLength(50, ErrorMessage = "El campo {0} no puede superar los {1} caracteres")]
        public string Name { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [RegularExpression(@"^\S+$", ErrorMessage = "El campo {0} no puede tener espacios en blanco")]
        [Display(Name = "Usuario")]
        [Remote("IsUserNameAvailable", "UsersAdmin", "Security", AdditionalFields = "Id")]
        [StringLength(50, ErrorMessage = "El campo {0} no puede superar los {1} caracteres")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [Display(Name = "Apellido")]
        [StringLength(50, ErrorMessage = "El campo {0} no puede superar los {1} caracteres")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [Display(Name = "Correo electrónico")]
        [RegularExpression(@"^([\w\!\#$\%\&\'\*\+\-\/\=\?\^\`{\|\}\~]+\.)*[\w\!\#$\%\&\'\*\+\-\/\=\?\^\`{\|\}\~]+@((((([a-zA-Z0-9]{1}[a-zA-Z0-9\-]{0,62}[a-zA-Z0-9]{1})|[a-zA-Z])\.)+[a-zA-Z]{2,6})|(\d{1,3}\.){3}\d{1,3}(\:\d{1,5})?)$", ErrorMessage = "Ingrese un Email válido")]
        [Remote("IsEmailAvailable", "UsersAdmin", "Security", AdditionalFields = "Id")]
        public string Email { get; set; }

        [Display(Name = "Teléfono")]
        [StringLength(50, ErrorMessage = "El campo {0} no puede superar los {1} caracteres")]
        public string PhoneNumber { get; set; }
    }

    public class AssignRolesViewModel
    {
        public string Id { get; set; }
        public List<IdentityRole> Roles { get; set; }
        public List<string> RolesInUser { get; set; }
    }
}