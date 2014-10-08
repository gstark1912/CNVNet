using MVC.Models;
using MVC.Security.Models;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;

namespace MVC.Security.Controllers
{
    public class HomeController : BaseController
    {
        [AllowAnonymous]
        public ActionResult Index()
        {
            if (Request.IsAuthenticated)
            {
                HomeViewModel model = new HomeViewModel();

                // Si el usuario pertence al rol admin, le permite ver el estado actual de la configuración
                // en caso de que falte configurar algo.
                if (HttpContext.User.IsInRole("admin"))
                {
                    using (var dbContext = new ApplicationDbContext())
                    {
                        AspNetGeneralConfiguration entity = dbContext.AspNetGeneralConfiguration.FirstOrDefault();
                        model.AdminNotes = GetConfigurationState(entity);
                    }
                }

                return View("", model);
            }
            else
                return RedirectToAction("Login", "Account");
        }

        /// <summary>
        /// Determina el estado de la configuración actual y da las advertencias correspondientes
        /// </summary>
        /// <returns></returns>
        private List<string> GetConfigurationState(AspNetGeneralConfiguration entity)
        {
            List<string> response = new List<string>();
            if (entity != null)
            {
                if (entity.CredentialUserName == null || entity.EmailPassword == null || entity.Client == null)
                    response.Add("La configuración de Mail no está completa.");
            }
            else
            {
                response.Add("La configuración de Mail no está completa.");
                response.Add("Logitud Mínima de Contraseña no configurada.");
                response.Add("Intentos Fallidos Permitidos no está configurado.");
                response.Add("Contraseñas anteriores no permitidas no configurada.");
                response.Add("Caducidad de Contraseña no configurada.");
            }
            return response;
        }
    }
}
