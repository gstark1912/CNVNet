using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace MVC.Security.Models
{
    public class EditUserViewModel
    {
        public EditUserViewModel()
        {
            RolesList = new List<SelectListItem>();
        }
        public string Id { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "El campo {0} es requerido")]
        [StringLength(50, ErrorMessage = "{0} debes ser de {1} caracteres como máximo.")]
        [Display(Name = "Nombre de Usuario")]
        public string UserName { get; set; }

        [Required(AllowEmptyStrings = false)]
        [Display(Name = "Correo electrónico")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(AllowEmptyStrings = false)]
        [Display(Name = "Nombre")]
        public string Name { get; set; }

        [Required(AllowEmptyStrings = false)]
        [Display(Name = "Apellido")]
        public string LastName { get; set; }

        public IEnumerable<SelectListItem> RolesList { get; set; }
    }


}