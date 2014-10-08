using MVC.Security.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC.Areas.Security.Models
{
    public class MenuListViewModel
    {
        public List<TreeData> dataTree { get; set; }
    }

    public class MenuViewModel
    {
        public MenuViewModel()
        {
            Parents = GetParents();
        }
        public int Id { get; set; }

        [Display(Name = "Nombre")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "El campo {0} es requerido")]
        [StringLength(140, ErrorMessage = "El campo {0} no puede superar los {1} caracteres")]
        public string Name { get; set; }

        [Display(Name = "Acción")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "El campo {0} es requerido")]
        [StringLength(140, ErrorMessage = "El campo {0} no puede superar los {1} caracteres")]
        public string Action { get; set; }

        [Display(Name = "Menu padre")]
        public string Parent { get; set; }

        public int IdParent { get; set; }
        public Nullable<int> Order { get; set; }

        public List<SelectListItem> Parents { get; set; }

        private List<SelectListItem> GetParents()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            using (var db = new ApplicationDbContext())
            {
                list.Insert(0, new SelectListItem { Text = "Raiz", Value = "0" });
                List<AspNetMenu> Menu = db.AspNetMenu.ToList();
                Menu.ForEach(c => list.Add(new SelectListItem { Text = c.Name, Value = c.Id.ToString() }));

            }
            return list;
        }
    }

}