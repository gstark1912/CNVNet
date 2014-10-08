using MVC.Areas.Security.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity.EntityFramework;
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
    /// Controlador utilizado para la edición de datos o cambio de contraseña
    /// de parte del propio de usuario.
    /// </summary>
    [Authorize]
    public class UsersController : BaseController
    {
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

        public ActionResult ChangePassword()
        {
            ResetPasswordViewModel model = new ResetPasswordViewModel();
            string username = HttpContext.User.Identity.Name;
            var user = UserManager.FindByName(username);
            model.Id = user.Id;
            return View(model);
        }

        /// <summary>
        /// Realiza el cambio de contraseña al usuario validando antes formato 
        /// y repeticiones de contraseña admitidas
        /// </summary>        
        [HttpPost]
        public ActionResult ChangePassword(ResetPasswordViewModel model)
        {
            var user = UserManager.FindById(model.Id);
            if (user == null)
            {
                ModelState.AddModelError("", "El usuario no existe");
                return View(model);
            }
            if (!UserManager.CheckPassword(user, model.ActualPassword))
            {
                ModelState.AddModelError("", "La contraseña actual no es correcta");
                return View(model);
            }
            var passwordValidation = UserManager.PasswordValidator.ValidateAsync(model.Password);
            if (!passwordValidation.Result.Succeeded)
            {
                foreach (var error in passwordValidation.Result.Errors)
                {
                    ModelState.AddModelError("", error);
                }
                return View(model);
            }
            if (UserManager.PasswordIsRepeated(user.Id, model.Password))
            {
                ModelState.AddModelError("", "Password Repetida");
                return View(model);
            }
            var result = UserManager.ResetPassword(user.Id, UserManager.GeneratePasswordResetToken(user.Id), model.Password);
            if (result.Succeeded)
            {
                UserManager.SavePassword(user.Id);
                return RedirectToAction("Index", "Home", new { Area = "" });
            }
            foreach (var error in passwordValidation.Result.Errors)
            {
                ModelState.AddModelError("", error);
            }
            return View();
        }

        public ActionResult ChangePersonalData()
        {
            UsersAdminViewModel model = new UsersAdminViewModel();
            string username = HttpContext.User.Identity.GetUserId();
            var user = UserManager.FindById(username);
            model.Id = user.Id;
            model.LastName = user.LastName;
            model.Name = user.Name;
            model.UserName = user.UserName;
            model.PhoneNumber = user.PhoneNumber;
            model.Email = user.Email;
            return View(model);
        }

        /// <summary>
        /// Permite editar los datos personales del usuario
        /// </summary>
        public ActionResult SavePersonalData(UsersAdminViewModel model)
        {
            bool reconfirmMail;
            ApplicationUser user = UserManager.FindById(model.Id);
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
                var code = UserManager.GenerateEmailConfirmationToken(user.Id);
                var callbackUrl = Url.Action("ConfirmEmailAndChangePassword", "Account", new { Area = "", userId = user.Id, code = code, changePassword = false }, protocol: Request.Url.Scheme);
                UserManager.SendEmail(user.Id, "Confirmación de Correo Electrónico!", "Por favor confirme su correo electrónico dirigiéndose a este link: <a href=\"" + callbackUrl + "\">Click Aquí</a>");
            }

            return Json(true);
        }
    }
}