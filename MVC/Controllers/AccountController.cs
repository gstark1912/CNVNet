using System.Globalization;
using MVC.Security.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MVC.Security.Controllers
{
    public class AccountController : BaseController
    {
        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
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

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        private SignInHelper _helper;

        private SignInHelper SignInHelper
        {
            get
            {
                if (_helper == null)
                {
                    _helper = new SignInHelper(UserManager, AuthenticationManager);
                }
                return _helper;
            }
        }

        /// <summary>
        /// Realiza un intento de login basado en la información ingresada
        /// </summary>        
        /// <param name="returnUrl">Url a la que se debe redireccionar en caso de un Login exitoso</param>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            // Realiza el intento de Login y recibe uno de varios resultados posibles detallados a continuación
            var result = await SignInHelper.PasswordSignIn(model.Email, model.Password, model.RememberMe);
            switch (result)
            {
                //Login Exitoso: 
                // Registra que se ha realizado un login exitoso y setea en la base de datos que el usuario está logueado
                // Luego redirecciona a la Url pedida, o en su defecto al Home
                case SignInStatus.Success:
                    LogSuccessLogin(model);
                    SetStatusUserLogged(model.Email, true);
                    return RedirectToLocal(returnUrl);

                // Requiere cambio de Contraseña:
                // Sea porque la contraseña caducó o porque fue reseteada por el Administrador, redirecciona a la vista para el cambio de la misma
                case SignInStatus.RequiresPasswordChange:
                    return View("PasswordChangeRequired", new PasswordChangeRequiredViewModel { Email = model.Email, CurrentPassword = model.Password });

                // Cuenta Bloqueada por intentos máximos permitidos.
                case SignInStatus.LockedOutForAttempts:
                    ModelState.AddModelError("", "Ha excedido el número de intentos permitidos para introducir tu contraseña, la cuenta será bloqueada.");
                    LogFailLogin(model);
                    return View(model);
                // Cuenta Bloqueada: redirecciona a la vista que informa el estado de la cuenta
                case SignInStatus.LockedOut:
                    ModelState.AddModelError("", "La cuenta del usuario se encuentra bloqueada.");
                    LogFailLogin(model);
                    return View(model);

                // Confirmación de Email pendiente: redirecciona a la vista que informa al usuario que revise su correo para confirmar su cuenta
                case SignInStatus.RequiresEmailConfirmation:
                    ModelState.AddModelError("", "No se ha verificado el correo electrónico de la cuenta, debe verificarlo para poder ingresar.");
                    return View(model);

                // Intento de Login fallido: Registra el intento fallido y redirecciona al login para informar y proponer un nuevo intento
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Usuario y contraseña inválidos");
                    LogFailLogin(model);
                    return View(model);
            }
        }

        /// <summary>
        /// Registra un logueo correcto de usuario en la base de datos
        /// </summary>
        /// <param name="model"></param>
        private void LogSuccessLogin(LoginViewModel model)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            AspNetSignInLog log = new AspNetSignInLog();
            log.Date = DateTime.Now;
            log.Email = model.Email;
            log.Success = true;
            db.AspNetSignInLog.Add(log);
            db.SaveChangesAsync();
        }

        /// <summary>
        /// Cambia el estado de conexión del usuario en la base de datos según
        /// esté conectado o desconectado
        /// </summary>
        private void SetStatusUserLogged(string email, bool state)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            ApplicationUser user = db.Users.Where(m => m.UserName == email).FirstOrDefault();
            if (user != null)
            {
                user.IsLogged = state;
                db.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Registra un logueo incorrecto de usuario en la base de datos
        /// </summary>
        /// <param name="model"></param>
        private void LogFailLogin(LoginViewModel model)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            AspNetSignInLog log = new AspNetSignInLog();
            log.Date = DateTime.Now;
            log.Email = model.Email;
            log.Success = false;
            log.Details = "Intento fallido de Login";
            db.AspNetSignInLog.Add(log);
            db.SaveChanges();
        }

        #region Pantalla de Registro

        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        /// <summary>
        /// Registra un usuario y le manda el mail de confirmación para dar por terminado el registro
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.UserName, Email = model.Email, LastPasswordDate = DateTime.Now };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    var code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    var callbackUrl = Url.Action("ConfirmEmailAndChangePassword", "Account", new { Area = "", userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    await UserManager.SendEmailAsync(user.Id, "Bienvenido a MVC!", "Por favor confirme la creación de su cuenta dirigiéndose a este link: <a href=\"" + callbackUrl + "\">Click Aquí</a>");
                    return View("DisplayEmail");
                }
                AddErrors(result);
            }


            return View(model);
        }

        #endregion

        /// <summary>
        /// Intenta la confirmación de Email del usuario.
        /// Esté método proviene de mail enviado al usuario al registrarse.
        /// </summary>
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            if (result.Succeeded)
            {
                return View("ConfirmEmail");
            }
            AddErrors(result);
            return View();
        }

        /// <summary>        
        /// Esté método proviene de mail enviado al usuario cuando un Administrador lo registra.        
        /// Redirecciona a la vista donde lo obliga a ingresar una password
        /// </summary>
        [AllowAnonymous]
        public ActionResult ConfirmEmailAndChangePassword(string userId, string code, bool changePassword = true)
        {
            ResetPasswordViewModel model = new ResetPasswordViewModel();
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var user = UserManager.FindById(userId);
            if (user == null) return View("Error");

            model.Code = code;
            model.Email = user.Email;
            model.Id = user.Id;
            model.HasToChangePassword = changePassword;

            return View("ConfirmEmailAndChangePassword", model);
        }

        /// <summary>
        /// Intenta la confirmación de Email del usuario y el ingreso de una password.
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmailAndChangePassword(ResetPasswordViewModel model)
        {
            var user = UserManager.FindById(model.Id);
            if (user == null) return View("Error");

            if (model.HasToChangePassword)
            {
                var passwordValidation = UserManager.PasswordValidator.ValidateAsync(model.Password);
                if (!passwordValidation.Result.Succeeded)
                {
                    AddErrors(passwordValidation.Result);
                    return View("ConfirmEmailAndChangePassword", model);
                }
            }
            var result = await UserManager.ConfirmEmailAsync(model.Id, model.Code);

            if (model.HasToChangePassword)
            {
                var pResult = await UserManager.AddPasswordAsync(model.Id, model.Password);
                if (!result.Succeeded)
                {
                    AddErrors(result);
                    return View("ConfirmEmailAndChangePassword", model);
                }

                if (pResult.Succeeded)
                {
                    UserManager.SavePassword(model.Id);
                    return View("ConfirmEmail");
                }

                UserManager.RemovePassword(model.Id);
                AddErrors(pResult);
                return View("ConfirmEmailAndChangePassword", model);
            }

            return View("ConfirmEmail");
        }

        /// <summary>
        /// Guarda la contraseña ingresada por el usuario luego de que éste haya sido obligado a cambiarla
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> PasswordChangeRequired(PasswordChangeRequiredViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByEmailAsync(model.Email);
            if (user == null) user = await UserManager.FindByNameAsync(model.Email);
            if (user != null)
            {
                var result = await UserManager.ChangePasswordAsync(user.Id, model.CurrentPassword, model.Password);
                if (result.Succeeded)
                {
                    UserManager.SavePassword(user.Id);
                    await SignInHelper.SignInAsync(user, false, false);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    AddErrors(result);
                    return View(model);
                }
            }
            return RedirectToAction("Index", "Home");
        }


        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        /// <summary>
        /// Envío de Mail al Usuario por olvido de contraseña.        
        /// </summary>                
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByEmailAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    ModelState.AddModelError("", "No existe un usuario con ese correo electrónico");
                    return View(model);
                }

                user.PasswordChangeExpiration = DateTime.Now.AddHours(72);
                await UserManager.RemovePasswordAsync(user.Id);
                await UserManager.UpdateAsync(user);

                // Envío de mail al usuario
                string token = UserManager.GeneratePasswordResetToken(user.Id);
                var callbackUrl = Url.Action("ResetPassword", "Account", new { Area = "", userId = user.Id, code = token }, protocol: Request.Url.Scheme);
                await UserManager.SendEmailAsync(user.Id, "MVC - Cambio de contraseña", "Para cambiar su contraseña,   <a href=\"" + callbackUrl + "\">Ingrese aquí</a></br>Este link será válido hasta " + user.PasswordChangeExpiration.ToString());

                return View("ForgotPasswordConfirmation");
            }

            return View(model);
        }

        /// <summary>
        /// Verifica si existe una cuenta con un correo electrónico determinado en la base de datos
        /// </summary>
        public JsonResult EmailExists(string Email)
        {
            var user = UserManager.FindByEmail(Email);
            if (user != null)
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            else
            {
                string errorMessage = "No existe un usuario con ese correo electrónico";
                return Json(errorMessage, JsonRequestBehavior.AllowGet);
            }
        }

        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        /// <summary>
        /// Método ingresado sólo mediante el mail con el que se le envía al usuario
        /// cuando un administrador le realizar un Reste de Password
        /// </summary>
        [AllowAnonymous]
        public ActionResult ResetPassword(string code, string userId = "")
        {
            ResetPasswordViewModel model;
            if (code == null)
                return View("Error");
            else
            {
                var user = UserManager.FindById(userId);
                if (SignInHelper.RequiresPasswordChange(user))
                {
                    model = new ResetPasswordViewModel();
                    model.Code = code;
                    model.Id = userId;
                    model.Email = user.Email;
                    return View(model);
                }
                else
                    return View("PasswordChangeNotAuthorized");
            }
        }

        /// <summary>
        /// Verifica si una password es válida utilizando el validador de password configurado
        /// </summary>
        public JsonResult IsPasswordvalid(string Password)
        {
            var passwordValidation = UserManager.PasswordValidator.ValidateAsync(Password);
            if (passwordValidation.Result.Succeeded)
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            else
            {
                string errorMessage = passwordValidation.Result.Errors.First();
                return Json(errorMessage, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Realiza el cambio de contraseña verificando que esto sea válido según las configuraciones
        /// de contraseñas repetidas y formato de password
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            var user = await UserManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "El usuario no existe");
                return View("ResetPassword", model);
            }
            var passwordValidation = UserManager.PasswordValidator.ValidateAsync(model.Password);
            if (!passwordValidation.Result.Succeeded)
            {
                AddErrors(passwordValidation.Result);
                return View("ResetPassword", model);
            }
            if (UserManager.PasswordIsRepeated(user.Id, model.Password))
            {
                ModelState.AddModelError("", "Password Repetida");
                return View("ResetPassword", model);
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                UserManager.SavePassword(user.Id);
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        /// <summary>
        /// Informa al usuario que su contraseña ha sido cambiada con éxito
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        /// <summary>
        /// Realiza el logoff del usuario no sin antes limpiar la variable de sesión correspondiente,
        /// y registrar el logoff en la base de datos
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut();
            SetStatusUserLogged(AuthenticationManager.User.Identity.Name, false);
            Session["user"] = null;

            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        // Cierra la conexión a la Base de Datos al terminar de usar el controlador
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }
            base.Dispose(disposing);
        }

        #region Common

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        /// <summary>
        /// Redirecciona a la url especificada o al Home en caso de que no exista
        /// </summary>
        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        #endregion
    }
}