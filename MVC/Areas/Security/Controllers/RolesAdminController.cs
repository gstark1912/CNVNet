using MVC.Areas.Security.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Collections.Generic;
using MVC.Models;
using System.Web.Script.Serialization;
using System;
using MVC.Security.Models;
using System.Web.UI;
using MVC.Security.Controllers;

namespace MVC.Areas.Security.Controllers
{
    [OutputCache(Location = OutputCacheLocation.None, NoStore = true)]
    [Authorize]
    public class RolesAdminController : BaseController
    {
        public RolesAdminController()
        {
        }

        public RolesAdminController(ApplicationUserManager userManager,
            ApplicationRoleManager roleManager)
        {
            UserManager = userManager;
            RoleManager = roleManager;
        }

        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            set
            {
                _userManager = value;
            }
        }

        private ApplicationRoleManager _roleManager;
        public ApplicationRoleManager RoleManager
        {
            get
            {
                return _roleManager ?? HttpContext.GetOwinContext().Get<ApplicationRoleManager>();
            }
            private set
            {
                _roleManager = value;
            }
        }


        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Busca Roles según descripción en la base de datos, y los devuelve paginados
        /// según los parámetros propuestos por la jqgrid
        /// </summary>        
        public ActionResult SearchRoles(string sidx, string sord, int page, int rows, string desc)
        {
            IQueryable<IdentityRole> result = RoleManager.Roles.Where(r => r.Name.Contains(desc));
            int total = result.Count();
            result = result.PagedData(sidx, sord, page, rows);
            int totalPages = (int)Math.Ceiling((decimal)total / (decimal)rows);
            var data = new
            {
                total = totalPages,
                page = page,
                records = total,
                rows = from c in result.ToList()
                       select new
                       {
                           cell = new string[] 
                                   {                                       
                                        c.Id,
                                        c.Name
                                   }
                       }
            };
            return Json(data);
        }

        /// <summary>
        /// PartialView utilizada para la Inserción/Edición de un Rol
        /// </summary>
        /// <param name="id">Parámetro opcional. Ingresar un Id para la edición, o dejar vacío para la inserción</param>        
        public async Task<ActionResult> _RoleEdit(string id = null)
        {
            try
            {
                RoleViewModel role = new RoleViewModel();
                if (id != null)
                {
                    var r = await RoleManager.FindByIdAsync(id);
                    role.Id = r.Id;
                    role.Name = r.Name;
                }
                return View(role);
            }
            catch (Exception ex)
            {
                return ReturnException(ex);
            }
        }

        /// <summary>
        /// Inserta un nuevo rol en la base de datos
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> InsertRole(RoleViewModel roleViewModel)
        {
            var role = new IdentityRole(roleViewModel.Name);
            var roleresult = await RoleManager.CreateAsync(role);
            if (!roleresult.Succeeded)
            {
                return ReturnMessage(roleresult.Errors.First());
            }
            return Json(true);
        }

        /// <summary>
        /// Actualiza un rol existente en la base de datos
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> UpdateRole(RoleViewModel roleViewModel)
        {
            var role = await RoleManager.FindByIdAsync(roleViewModel.Id);
            role.Name = roleViewModel.Name;
            var roleresult = await RoleManager.UpdateAsync(role);
            if (!roleresult.Succeeded)
            {
                return ReturnMessage(roleresult.Errors.First());
            }
            return Json(true);
        }

        /// <summary>
        /// Verifica si se puede eliminar un rol de la base de datos
        /// </summary>
        public ActionResult CanDeleteRole(string id)
        {
            var role = RoleManager.FindById(id);
            if (role.Users.Count() > 0)
                return ReturnMessage("Atención, no se puede eliminar el rol ya que tiene usuarios asignados. Quite los usuarios asignados y vuelva a intentarlo");
            return Json(true);
        }

        /// <summary>
        /// Elimina un rol de la base de datos
        /// </summary>
        public async Task<ActionResult> DeleteRole(string id)
        {
            var role = await RoleManager.FindByIdAsync(id);
            var roleresult = await RoleManager.DeleteAsync(role);
            if (!roleresult.Succeeded)
            {
                return ReturnMessage(roleresult.Errors.First());
            }
            return Json(true);
        }

        /// <summary>
        /// Retorna la Vista con la estructura necesaria para la Asignación de Acciones a un Rol
        /// </summary>
        /// <param name="id">Id de rol</param>
        public ActionResult AssignActions(string id)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            MVC.Areas.Security.Models.PermissionsViewModels model = new MVC.Areas.Security.Models.PermissionsViewModels();
            model.Id = db.Roles.Where(r => r.Id == id).FirstOrDefault().Id;

            // Obtiene las acciones ya asignadas al rol
            List<AspNetRoleAction> roleActions = db.AspNetRoleAction.Where(a => a.IdRole == id).ToList();

            // Obtiene el total de las acciones
            List<AspNetActions> actions = db.AspNetActions.ToList();

            // Obtiene la jerarquía de acciones por menú
            List<AspNetActionInMenu> actionInMenus = db.AspNetActionInMenu.ToList();

            // Obtiene todos los menús
            List<AspNetMenu> Menus = db.AspNetMenu.ToList();

            // Obtiene los menús que son jerárquicamente superiores
            List<AspNetMenu> parentMenus = Menus.Where(m => m.IdParent == 0).OrderBy(mo => mo.Order).ToList();
            List<MVC.Areas.Security.Models.TreeData> treeData = new List<MVC.Areas.Security.Models.TreeData>();

            foreach (AspNetMenu m in parentMenus)
            {
                // Por cada menú padre, devuelve la estructura necesaria del mismo y de 
                // sus menús y acciones descendientes
                treeData.Add(GetTreeData(m, Menus, actions, roleActions, actionInMenus));

            }
            model.dataTree = treeData;
            return View(model);
        }

        /// <summary>
        /// Método recursivo para la obtención de la estructura requerida para la vista de un menú padre
        /// y de sus acciones y menús descendientes
        /// </summary>
        /// <param name="m">Menú padre</param>
        /// <param name="Menus">Lista de todos los menús</param>
        /// <param name="actions">Lista de todas las acciones</param>
        /// <param name="roleActions">Lista de acciones asignadas al rol</param>
        /// <param name="actionInMenus">Jerarquía de Acciones por Menú</param>
        /// <returns></returns>
        private MVC.Areas.Security.Models.TreeData GetTreeData(AspNetMenu m, List<AspNetMenu> Menus, List<AspNetActions> actions, List<AspNetRoleAction> roleActions, List<AspNetActionInMenu> actionInMenus)
        {
            MVC.Areas.Security.Models.TreeData result = new MVC.Areas.Security.Models.TreeData();

            //Acción del Menú
            AspNetActions menuAction = actions.Where(a => a.Id == m.IdAction).FirstOrDefault();
            result.key = menuAction.Id;
            result.title = menuAction.Name;
            result.select = roleActions.Where(r => r.IdAction == menuAction.Id).Count() > 0 ? true : false;
            result.isFolder = true; //Se setea en true por ser Menú

            result.children = new List<MVC.Areas.Security.Models.TreeData>();

            //Agrega los Menús hijos
            Menus.Where(cm => cm.IdParent == m.Id).ToList().ForEach(cmf => result.children.Add(GetTreeData(cmf, Menus, actions, roleActions, actionInMenus)));

            //Agrega las Acciones hijas
            List<AspNetActionInMenu> childActions = actionInMenus.Where(am => am.IdMenu == m.Id && am.IdAction != menuAction.Id).ToList();
            childActions.ForEach(ca => result.children.Add(GetTreeData(actions.Where(act => act.Id == ca.IdAction).FirstOrDefault(), roleActions)));

            return result;
        }

        /// <summary>
        /// Sobrecarga del método GetTreeData para obtener la estructura necesaria
        /// en el caso de las acciones
        /// </summary>
        /// <param name="ca">Acción</param>
        /// <param name="roleActions">Lista de acciones asignadas al rol</param>
        /// <returns></returns>
        private MVC.Areas.Security.Models.TreeData GetTreeData(AspNetActions ca, List<AspNetRoleAction> roleActions)
        {
            MVC.Areas.Security.Models.TreeData result = new MVC.Areas.Security.Models.TreeData();

            //Acción del Menú            
            result.key = ca.Id;
            result.title = ca.Name;
            //result.menuDescription 
            result.select = roleActions.Where(r => r.IdAction == ca.Id).Count() > 0 ? true : false;
            result.isFolder = false; //true por ser acción

            result.children = new List<MVC.Areas.Security.Models.TreeData>();
            return result;
        }

        /// <summary>
        /// Actualiza la asignación de acciones al Rol
        /// </summary>        
        public ActionResult SaveAssignActions(MVC.Areas.Security.Models.PermissionsViewModels model)
        {
            string idRol = model.Id;
            List<int> selectedKeys = new List<int>();

            //Se obtiene la nueva lista de acciones guardadas para el rol
            //y se guardan en selectedKeys
            string json = model.selectedKeys;
            var jss = new JavaScriptSerializer();
            dynamic dynamicData = jss.Deserialize<dynamic>(json);
            foreach (var item in dynamicData)
            {
                selectedKeys.Add(Convert.ToInt32(item));
            }

            selectedKeys = selectedKeys.OrderBy(a => a).ToList();

            ApplicationDbContext db = new ApplicationDbContext();

            //Elimina todas las acciones asignadas al rol
            List<AspNetRoleAction> toDelete = db.AspNetRoleAction.Where(x => x.IdRole == idRol).ToList();
            toDelete.ForEach(x => db.AspNetRoleAction.Remove(x));

            //Asigna las acciones determinadas en pantalla al rol
            selectedKeys.ForEach(x => db.AspNetRoleAction.Add(new AspNetRoleAction { IdAction = x, IdRole = idRol }));

            db.SaveChanges();

            return Json(true);
        }

        /// <summary>
        /// Verifica si el nombre de un rol se encuentra disponible para la inserción o edicion.
        /// </summary>
        public JsonResult IsRoleAvailable(string Name, string Id)
        {
            var role = RoleManager.FindByName(Name);
            if (Id == null)
            {
                if (role == null)
                {
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    string errorMessage = "El Nombre de Rol se encuentra en uso.";
                    return Json(errorMessage, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                if (role == null)
                {
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    if (role.Id != Id)
                    {
                        string errorMessage = "El Nombre de Rol se encuentra en uso.";
                        return Json(errorMessage, JsonRequestBehavior.AllowGet);
                    }
                    else
                        return Json(true, JsonRequestBehavior.AllowGet);
                }
            }
        }
    }
}
