using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC.Areas.Security.Models
{
    public class RoleViewModel
    {
        public string Id { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "El campo {0} es requerido")]
        [Display(Name = "Descripción")]
        [Remote("IsRoleAvailable", "RolesAdmin", "Security", AdditionalFields = "Id")]
        [StringLength(140, ErrorMessage = "El campo {0} no puede superar los {1} caracteres")]
        public string Name { get; set; }
    }

    public class PermissionsViewModels
    {
        public string Id { get; set; }
        public List<TreeData> dataTree { get; set; }
        public string selectedKeys { get; set; }
    }

    public class TreeData
    {
        public int key { get; set; }
        public string title { get; set; }
        public string menuDescription { get; set; }
        public bool select { get; set; }
        public bool isFolder { get; set; }
        public List<TreeData> children { get; set; }
    }


}