using MVC.Security.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using MVC.Security.Controllers;
using MVC.Areas.Security.Models;
using System.Web.UI;

namespace MVC.Areas.Security.Controllers
{
    [OutputCache(Location = OutputCacheLocation.None, NoStore = true)]

    public class UsersAdminController : BaseController
    {
        public UsersAdminController()
        {
        }

        public UsersAdminController(ApplicationUserManager userManager, ApplicationRoleManager roleManager)
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
            private set
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
        /// Busca Usuario según una descripción en la base de datos, y los devuelve paginados
        /// según los parámetros propuestos por la jqgrid
        /// </summary>  
        [Authorize]
        public ActionResult SearchUsers(string sidx, string sord, int page, int rows, string desc)
        {
            IQueryable<ApplicationUser> result = UserManager.Users.Where(u => u.UserName.Contains(desc) || u.Email.Contains(desc));
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
                                        c.UserName,
                                        c.Name,
                                        c.LastName,
                                        c.Email,
                                        GetCurrentState(c),
                                        UserManager.IsLockedOut(c.Id).ToString()
                                   }
                       }
            };
            return Json(data);
        }

        private string GetCurrentState(ApplicationUser c)
        {
            if (UserManager.IsLockedOut(c.Id))
                return "Bloqueado";

            if (!UserManager.IsEmailConfirmed(c.Id))
                return "Pendiente de Verificación";

            return "Activo";
        }

        /// <summary>
        /// PartialView utilizada para la Inserción/Edición de un Usuario
        /// </summary>
        /// <param name="id">Parámetro opcional. Ingresar un Id para la edición, o dejar vacío para la inserción</param>        
        [Authorize]
        public async Task<ActionResult> _UserEdit(string id = null)
        {
            UsersAdminViewModel user = new UsersAdminViewModel();
            if (id != null)
            {
                var u = await UserManager.FindByIdAsync(id);
                user.Id = u.Id;
                user.Name = u.Name;
                user.LastName = u.LastName;
                user.UserName = u.UserName;
                user.Email = u.Email;
                user.PhoneNumber = u.PhoneNumber;
            }
            return View(user);
        }

        /// <summary>
        /// Partial View para la asignación de un usuario a un rol
        /// </summary>
        /// <param name="id">Id del usuario</param>
        [Authorize]
        public ActionResult _AssignRoles(string id)
        {
            AssignRolesViewModel model = new AssignRolesViewModel();
            var user = UserManager.FindById(id);
            if (user == null) return View("Error");
            model.Roles = RoleManager.Roles.ToList();
            model.RolesInUser = user.Roles.Select(r => r.RoleId).ToList();
            model.Id = id;

            return View(model);
        }

        /// <summary>
        /// Actualiza los roles a los que el usuario está asignado
        /// </summary>
        [Authorize]
        public async Task<ActionResult> AssignRoles(AssignRolesViewModel model)
        {
            var user = UserManager.FindById(model.Id);
            List<string> roles = new List<string>();
            if (model.RolesInUser != null)
                roles = RoleManager.Roles.Where(r => model.RolesInUser.Contains(r.Id)).Select(rn => rn.Name).ToList();
            List<string> rolesInUserIds = user.Roles.Select(ur => ur.RoleId).ToList();
            List<string> rolesInUserNames = RoleManager.Roles.Where(r => rolesInUserIds.Contains(r.Id)).Select(rn => rn.Name).ToList();
            rolesInUserNames.ForEach(r => UserManager.RemoveFromRole(user.Id, r));
            var result = await UserManager.AddUserToRolesAsync(user.Id, roles);
            if (!result.Succeeded)
                throw new Exception(result.Errors.First());
            return Json(true);
        }

        /// <summary>
        /// Inserción de un nuevo usuario: envía un mail para que confirme su correo electrónico.
        /// Hasta que no lo haga, no podrá acceder con su cuenta.
        /// </summary>
        [Authorize]
        [HttpPost]
        public async Task<ActionResult> InsertUser(UsersAdminViewModel model)
        {
            var user = new ApplicationUser { UserName = model.UserName, Email = model.Email, PhoneNumber = model.PhoneNumber, Name = model.Name, LastName = model.LastName, EmailConfirmed = false, LockoutEnabled = true };
            var result = await UserManager.CreateAsync(user);
            if (result.Succeeded)
            {
                var code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                var callbackUrl = Url.Action("ConfirmEmailAndChangePassword", "Account", new { Area = "", userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                await UserManager.SendEmailAsync(user.Id, "Bienvenido a MVC!", "Por favor confirme la creación de su cuenta dirigiéndose a este link: <a href=\"" + callbackUrl + "\">Click Aquí</a>");
                return Json(true);
            }

            return Json(true);
        }

        /// <summary>
        /// Verifica si el correo electrónico ingresado no está siendo
        /// utilizado por otra cuenta
        /// </summary>
        /// <param name="Email">Correo electrónico</param>
        /// <param name="Id">Parámetro opcional. Es null si es una inserción, y contiene un Id en el caso de una actualización</param>
        [AllowAnonymous]
        public JsonResult IsEmailAvailable(string Email, string Id)
        {
            var user = UserManager.FindByEmail(Email);
            if (Id == null)
            {
                if (user == null)
                {
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    string errorMessage = "El Email se encuentra en uso.";
                    return Json(errorMessage, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                if (user == null)
                {
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    if (user.Id != Id)
                    {
                        string errorMessage = "El Email se encuentra en uso.";
                        return Json(errorMessage, JsonRequestBehavior.AllowGet);
                    }
                    else
                        return Json(true, JsonRequestBehavior.AllowGet);
                }
            }
        }

        /// <summary>
        /// Verifica si el nombre de usuario ingresado no está siendo
        /// utilizado por otra cuenta
        /// </summary>
        /// <param name="UserName">Nombre de usuario</param>
        /// <param name="Id">Parámetro opcional. Es null si es una inserción, y contiene un Id en el caso de una actualización</param>
        [AllowAnonymous]
        public JsonResult IsUserNameAvailable(string UserName, string Id)
        {
            var user = UserManager.FindByName(UserName);
            if (Id == null)
            {
                if (user == null)
                {
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    string errorMessage = "El Nombre de Usuario se encuentra en uso.";
                    return Json(errorMessage, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                if (user == null)
                {
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    if (user.Id != Id)
                    {
                        string errorMessage = "El Nombre de Usuario se encuentra en uso.";
                        return Json(errorMessage, JsonRequestBehavior.AllowGet);
                    }
                    else
                        return Json(true, JsonRequestBehavior.AllowGet);
                }
            }
        }


        /// <summary>
        /// Actualiza un usuario en la base de datos
        /// </summary>
        [Authorize]
        [HttpPost]
        public async Task<ActionResult> UpdateUser(UsersAdminViewModel model)
        {
            bool reconfirmMail;
            ApplicationUser user = await UserManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                return HttpNotFound();
            }

            reconfirmMail = user.Email == model.Email ? false : true;

            user.Email = model.Email;
            user.Name = model.Name;
            user.UserName = model.UserName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber;
            if (reconfirmMail) user.EmailConfirmed = false;
            UserManager.Update(user);

            if (reconfirmMail)
            {
                var code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                var callbackUrl = Url.Action("ConfirmEmailAndChangePassword", "Account", new { Area = "", userId = user.Id, code = code, changePassword = false }, protocol: Request.Url.Scheme);
                await UserManager.SendEmailAsync(user.Id, "Confirmación de Correo Electrónico!", "Por favor confirme su correo electrónico dirigiéndose a este link: <a href=\"" + callbackUrl + "\">Click Aquí</a>");
            }

            return Json(true);
        }

        /// <summary>
        /// Elminar un usuario de la base de datos
        /// </summary>
        /// <param name="id">Id del usuario</param>
        [Authorize]
        public async Task<ActionResult> DeleteUser(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = await UserManager.FindByIdAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            user.AspNetUserPassword.RemoveAll(p => true == true);
            user.AspNetUsersNoAccess.RemoveAll(p => true == true);
            user.AspNetUsersAccess.RemoveAll(p => true == true);
            var result = await UserManager.DeleteAsync(user);

            return Json(true);
        }

        /// <summary>
        /// Desbloquea un usuario
        /// </summary>
        /// <param name="id">Id de usuario</param>
        [Authorize]
        public ActionResult UnlockUser(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserManager.SetLockoutEndDate(id, DateTimeOffset.Now);

            return Json(true);
        }

        /// <summary>
        /// Bloquea un usuario. El tiempo que utiliza es el mismo definido en la configuración
        /// para el bloqueo por intento fallido en caso de estar configurado.
        /// Sino, emula un bloqueo permanente bloqueando al usuario por 999 años.
        /// </summary>
        /// <param name="id">Id de usuario</param>
        [Authorize]
        public ActionResult LockUser(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetGeneralConfiguration entity;

            using (var dbContext = new ApplicationDbContext())
            {
                entity = dbContext.AspNetGeneralConfiguration.FirstOrDefault();
            }
            if (entity == null)
                throw new Exception("Debe configurarse el tiempo de bloqueo");

            if (entity.ExpirationTimeDays != 0)
                UserManager.SetLockoutEndDate(id, DateTimeOffset.Now.AddMinutes(entity.ExpirationTimeDays));
            else
                UserManager.SetLockoutEndDate(id, DateTimeOffset.Now.AddYears(999));

            return Json(true);
        }

        /// <summary>
        /// Setea que el usuario debe cambiar la password y le da 72 horas al código que le envía por mail para efectuar dicho cambio.
        /// </summary>
        /// <param name="id">Id de usuario</param>
        [Authorize]
        [HttpPost]
        public async Task<ActionResult> ResetPassword(string id)
        {
            var user = await UserManager.FindByIdAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            user.PasswordChangeExpiration = DateTime.Now.AddHours(72);
            await UserManager.RemovePasswordAsync(user.Id);
            await UserManager.UpdateAsync(user);

            string token = UserManager.GeneratePasswordResetToken(id);
            var callbackUrl = Url.Action("ResetPassword", "Account", new { Area = "", userId = user.Id, code = token }, protocol: Request.Url.Scheme);
            await UserManager.SendEmailAsync(user.Id, "MVC - Cambio de contraseña", "Para cambiar su contraseña,   <a href=\"" + callbackUrl + "\">Ingrese aquí</a></br>Este link será válido hasta " + user.PasswordChangeExpiration.ToString());
            return Json(true);
        }
    }
}
