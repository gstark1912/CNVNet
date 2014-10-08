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
using AutoMapper;
using MVC.Security.Controllers;
using MVC.Areas.Security.Models;


namespace MVC.Areas.Security.Controllers
{
    /// <summary>
    /// Este controlador se encarga de la actualización
    /// </summary>
    [Authorize]
    public class ConfigurationAdminController : BaseController
    {
        public ActionResult Index()
        {
            // Obtiene (si existe) el registro de configuración para su edición o lo deja en blanco
            // para poder insertar
            using (var dbContext = new ApplicationDbContext())
            {
                AspNetGeneralConfiguration entity = dbContext.AspNetGeneralConfiguration.FirstOrDefault();
                ConfigurationViewModel model = new ConfigurationViewModel();
                model.PasswordRequireDigit = true;
                model.PasswordRequireLowercase = true;
                model.PasswordRequireUppercase = true;
                if (entity != null)
                {
                    model.Id = entity.Id;
                    model.CredentialUserName = entity.CredentialUserName;
                    model.Port = entity.Port;
                    model.EmailPassword = entity.EmailPassword;
                    model.Client = entity.Client;
                    model.EnableSSL = entity.EnableSSL;
                    model.DefaultCredentials = entity.DefaultCredentials;
                    model.PasswordRequiredLength = entity.PasswordRequiredLength;
                    model.PasswordRequireDigit = entity.PasswordRequireDigit;
                    model.PasswordRequireLowercase = entity.PasswordRequireLowercase;
                    model.PasswordRequireUppercase = entity.PasswordRequireUppercase;
                    model.MaxFailedAccessCount = entity.MaxFailedAccessCount;
                    model.PasswordRepeat = entity.PasswordRepeat;
                    model.ExpirationTimeDays = entity.ExpirationTimeDays;
                }
                return View(model);
            }
        }

        /// <summary>
        /// Guarda la configuración
        /// </summary>
        /// <param name="model">Datos provenientes de la pantalla</param>        
        public ActionResult Save(ConfigurationViewModel model)
        {
            if (ModelState.IsValid)
            {
                bool result = SaveConfiguration(model);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            return Json(false, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// Guarda la configuración en la base de datos
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private bool SaveConfiguration(ConfigurationViewModel model)
        {
            using (var dbContext = new ApplicationDbContext())
            {
                AspNetGeneralConfiguration entity = dbContext.AspNetGeneralConfiguration.FirstOrDefault();
                bool edit;

                // Determina si la configuración ya existe y debe actualizarla, o si debe insertarla
                if (entity == null)
                {
                    edit = false;
                    entity = new AspNetGeneralConfiguration();
                }
                else
                    edit = true;

                entity.CredentialUserName = model.CredentialUserName;
                entity.Port = model.Port;
                entity.EmailPassword = model.EmailPassword;
                entity.Client = model.Client;
                entity.EnableSSL = model.EnableSSL;
                entity.DefaultCredentials = model.DefaultCredentials;
                entity.PasswordRequiredLength = model.PasswordRequiredLength;
                entity.PasswordRequireDigit = model.PasswordRequireDigit;
                entity.PasswordRequireLowercase = model.PasswordRequireLowercase;
                entity.PasswordRequireUppercase = model.PasswordRequireUppercase;
                entity.MaxFailedAccessCount = model.MaxFailedAccessCount;
                entity.PasswordRepeat = model.PasswordRepeat;
                entity.ExpirationTimeDays = model.ExpirationTimeDays;

                if (!edit)
                {
                    dbContext.Entry(entity).State = System.Data.Entity.EntityState.Added;
                    dbContext.AspNetGeneralConfiguration.Add(entity);
                }
                else
                    dbContext.Entry(entity).State = System.Data.Entity.EntityState.Modified;

                dbContext.SaveChanges();
                return true;
            }
        }
    }
}