using MVC.Areas.Security.Models;
using MVC.Security.Controllers;
using MVC.Security.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC.Areas.Security.Controllers
{
    /// <summary>
    /// Control para la creacion del menu principal de la aplicacion. 
    /// </summary>
    public class MenuController : BaseController
    {
        [AllowAnonymous]
        public ActionResult NavigationMenu()
        {
            //Si hay sesion de usuario el menu se toma de ahi. 
            if (Session["user"] != null)
            {
                List<MenuItem> menu = new List<MenuItem>();
                menu = ((ApplicationUser)Session["user"]).Menus;
                return PartialView("_Menu", menu);

            }
            else
            {
                //Si no hay sesion de usuario el menu solo contenra el acceso a Inicio
                List<MenuItem> menu = new List<MenuItem>();
                menu = new List<MenuItem>();
                menu.Add(new MenuItem { Id = 1, Name = "Inicio", IdParent = null, Action = "Index", Controller = "Home" });
                return PartialView("_Menu", menu);
            }
        }

        [Authorize]
        public ActionResult Index()
        {
            MVC.Areas.Security.Models.MenuListViewModel model = new MVC.Areas.Security.Models.MenuListViewModel();
            model.dataTree = GetMenus();
            return View(model);
        }

        [Authorize]
        public ActionResult RefreshTreeData()
        {
            return Json(GetMenus());
        }


        /// <summary>
        /// Retorna la estructura de menús ordenados
        /// </summary>
        private List<MVC.Areas.Security.Models.TreeData> GetMenus()
        {
            ApplicationDbContext db = new ApplicationDbContext();

            // Lista todos los menús
            List<AspNetMenu> Menus = db.AspNetMenu.ToList();

            // Lista los menús que son jerárquicamente superiores a todos
            List<AspNetMenu> parentMenus = Menus.Where(m => m.IdParent == 0).OrderBy(mo => mo.Order).ToList();
            List<MVC.Areas.Security.Models.TreeData> treeData = new List<MVC.Areas.Security.Models.TreeData>();
            foreach (AspNetMenu m in parentMenus)
            {
                // Por cada menú superior, genera la estructura necesaria para él y sus menús descendientes
                treeData.Add(GetTreeData(m, Menus));

            }
            return treeData;
        }

        /// <summary>
        /// Función recursiva que retorna la estructura necesaria para la vista de un menú y sus descendientes
        /// </summary>
        /// <param name="parentMenu">Menú padre</param>
        /// <param name="Menus">Lista de todos los menús</param>
        private MVC.Areas.Security.Models.TreeData GetTreeData(AspNetMenu parentMenu, List<AspNetMenu> Menus)
        {
            MVC.Areas.Security.Models.TreeData result = new MVC.Areas.Security.Models.TreeData();

            result.key = parentMenu.Id;
            result.title = parentMenu.Name;

            result.children = new List<MVC.Areas.Security.Models.TreeData>();

            //Agrega los Menús hijos
            Menus.Where(cm => cm.IdParent == parentMenu.Id).OrderBy(o => o.Order).ToList().ForEach(cmf => result.children.Add(GetTreeData(cmf, Menus)));

            return result;
        }

        /// <summary>
        /// Actualiza el orden y la jerarquía de todos los menús
        /// </summary>        
        [Authorize]
        public ActionResult UpdateMenusHierarchy(List<MVC.Areas.Security.Models.TreeData> treeData)
        {
            int orden = 0;
            ApplicationDbContext db = new ApplicationDbContext();
            List<AspNetMenu> menus = db.AspNetMenu.ToList();
            foreach (MVC.Areas.Security.Models.TreeData TreeNode in treeData)
            {
                UpdateMenuItemFromTreeData(TreeNode, ref orden, ref menus);
            }
            menus.ForEach(m => db.Entry(m).State = System.Data.Entity.EntityState.Modified);
            db.SaveChanges();
            return Json(true);
        }

        /// <summary>
        /// Función recursiva que actualiza una entidad AspNetMenu y sus menús descendientes
        /// </summary>
        /// <param name="tree">Datos provenientes de la pantalla</param>
        /// <param name="orden">Lista de orden. Se va actualizando el número</param>
        /// <param name="menus">Lista de menús. Retorna actualizada</param>
        /// <param name="idparent">ID del menú padre</param>
        private void UpdateMenuItemFromTreeData(MVC.Areas.Security.Models.TreeData tree, ref int orden, ref List<AspNetMenu> menus, int? idparent = 0)
        {
            AspNetMenu m = menus.Where(menu => menu.Id == tree.key).FirstOrDefault();
            int ord = orden;
            List<AspNetMenu> menusToSave = menus;
            if (m != null)
            {
                m.IdParent = idparent;
                m.Order = ord++;
                if (tree.children != null)
                    tree.children.ForEach(t => UpdateMenuItemFromTreeData(t, ref ord, ref menusToSave, tree.key));
            }
            orden = ord;
            menus = menusToSave;
        }

        /// <summary>
        /// Partial View utilizado para la Inserción/Edición de un menú
        /// </summary>
        /// <param name="id">Parametro opcional: Id de menú para edición, o dejar vacío para inserción</param>        
        [Authorize]
        public ActionResult _MenuEdit(int id = 0, int idParent = 0)
        {
            try
            {
                MenuViewModel menu = new MenuViewModel();
                if (id != 0)
                {
                    ApplicationDbContext db = new ApplicationDbContext();
                    AspNetMenu m = db.AspNetMenu.Where(me => me.Id == id).FirstOrDefault();
                    menu.Id = m.Id;
                    menu.Action = db.AspNetActions.Find(m.IdAction).Route;
                    menu.IdParent = m.IdParent == null ? 0 : m.IdParent.Value;
                    menu.Parent = menu.Parents.Find(p => p.Value == menu.IdParent.ToString()).Text;
                    menu.Name = m.Name;
                }
                else
                {
                    menu.IdParent = idParent;
                    menu.Parent = menu.Parents.Find(p => p.Value == menu.IdParent.ToString()).Text;
                }
                return PartialView("_MenuEdit", menu);
            }
            catch (Exception ex)
            {
                return ReturnException(ex);
            }
        }

        /// <summary>
        /// Inserta un nuevo menú de la base de datos
        /// </summary>
        [Authorize]
        public ActionResult InsertMenu(MenuViewModel menu)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            AspNetMenu m = new AspNetMenu();
            AspNetActions action = db.AspNetActions.Where(a => a.Route.Equals(menu.Action)).FirstOrDefault();
            if (action == null)
                return ReturnMessage("La acción no existe.");
            m.IdAction = action.Id;
            m.IdParent = menu.IdParent;
            m.Name = menu.Name;
            db.AspNetMenu.Add(m);
            db.Entry(m).State = System.Data.Entity.EntityState.Added;
            db.SaveChangesAsync();
            return Json(true);
        }

        /// <summary>
        /// Actualiza un menú de la base de datos
        /// </summary>        
        [Authorize]
        public ActionResult UpdateMenu(MenuViewModel menu)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            AspNetMenu m = db.AspNetMenu.Where(me => me.Id == menu.Id).FirstOrDefault();
            AspNetActions action = db.AspNetActions.Where(a => a.Route.Equals(menu.Action)).FirstOrDefault();
            if (action == null)
                return ReturnMessage("La acción no existe.");
            m.IdAction = action.Id;
            m.IdParent = menu.IdParent;
            m.Name = menu.Name;
            db.Entry(m).State = System.Data.Entity.EntityState.Modified;
            db.SaveChangesAsync();
            return Json(true);
        }

        [Authorize]
        public ActionResult CanDeleteMenu(int id)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            int children = db.AspNetMenu.Where(me => me.IdParent == id).Count();
            if (children > 0)
                return ReturnMessage("Atención, no se puede eliminar el menú ya que tiene menús asociados. Quite los submenús y vuelva a intentarlo");
            return Json(true);
        }


        /// <summary>
        /// Elimina un menú de la base de datos
        /// </summary>        
        [Authorize]
        public ActionResult DeleteMenu(int id)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            AspNetMenu m = db.AspNetMenu.Where(me => me.Id == id).FirstOrDefault();
            db.AspNetMenu.Remove(m);
            db.SaveChangesAsync();
            return Json(true);
        }
    }
}
