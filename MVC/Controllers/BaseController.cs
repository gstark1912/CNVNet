/*
 * MVC.Security v1.0.2 (http://www.horizon.com.ar/Security)
 * ========================================================================
 * Copyright 2014 Horizon.
 * ========================================================================
 * Todos los controladores heredan de MVC.Security.Controllers.BaseController y 
 * este hereda de Controller, esto es así dado que en BaseController se implementan
 * todas las funcionalidades generales.
*/
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using MVC.Security.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System;
using System.IO;

namespace MVC.Security.Controllers
{
    public class BaseController : Controller
    {


        private ApplicationDbContext db = new ApplicationDbContext();

        /* 
         * Previo a una ejecucion de una acción se realizan validaciones de seguridad y operaciones generales
         * a todas las acciones. Respecto de la seguridad de ejecución se valida en primer instancia que el operador
         * esté auntenticado y en segunda instancia se valida que esté autorizado a la ejecución.
         * Adicionalmente, de acuerdo a la configuración del monitoreo de la ejecución se registrará la información correspondiente.
        */
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            /*
             * Se realiza un conjunto de operaciones para validar que el operador tenga privilegios suficientes.
             * Las operaciones consisten en:
             * 1 - Validar si la acción a ejecutar requiere autorización, esto lo realiza el propio Asp.Net. Si la acción
             * está marcada con el atributo [Authorize] el propio motor redirecciona al operador al login, tomando la ruta
             * definida como tal en el archivo Web.Config.
             * 2 - Se valida si el objeto "user" de la sesión está iniciado y tiene la información de acceso y ejecuciones.
             * 3 - Se verifica la configuración del módulo de seguridad propio de MD. El operador tiene asignado uno o más 
             * roles asignados y cada rol tiene definido que acciones puede ejecutar. Si dentro de los roles asociados 
             * está incluida esta acción es por que el operador la puede ejecutar. Caso contrario, se denegará la ejecución
             * y se le informará al operador.
             * 4 - Toda ejecución no permitida se registrará con fines control, para esto, seguirá la configuración del módulo
             * de auditoría y monitoreo.
            */

            // Id del usuario
            string userId = filterContext.HttpContext.User.Identity.GetUserId();

            // Nombre del usuario actual (login).
            string userName = (userId != String.Empty) && userId != null ? db.Users.Where(u => u.Id == userId).FirstOrDefault().UserName : "Anonymous";

            if (Session["user"] == null)
            {
                if (filterContext.HttpContext.User.Identity.IsAuthenticated)
                {
                    // Roles a los cuales el operador está asociado.                    
                    List<IdentityUserRole> r = db.Users.Where(u => u.Id == userId).FirstOrDefault().Roles.ToList();

                    // Instancia del usuario actual, se almacenará en la sesión.
                    ApplicationUser user = new ApplicationUser();
                    user.Actions = new List<string>();
                    user.Menus = new List<MenuItem>();

                    // Se cargan los roles que tiene asignados el usuario actual.
                    foreach (IdentityUserRole role in r)
                    {
                        IdentityUserRole userRole = new IdentityUserRole();
                        userRole.RoleId = role.RoleId;
                        userRole.UserId = db.Users.Where(u => u.Id == userId).FirstOrDefault().Id;
                        user.Roles.Add(userRole);

                        // Se agrega a la lista de acciones permitidas todas las que tiene el rol inspeccionado.
                        List<int> ActionsIds = db.AspNetRoleAction.Where(i => i.IdRole == role.RoleId).Select(a => a.IdAction).ToList();
                        foreach (int id in ActionsIds)
                        {
                            user.Actions.AddRange(db.AspNetActions.Where(h => h.Id == id).Select(a => a.Route).ToList());

                            // Se configura el menu para el usuario segun las acciones a las que puede acceder.
                            MenuItem item = new MenuItem();
                            AspNetMenu menu = db.AspNetMenu.Where(x => x.IdAction == id).FirstOrDefault();
                            AspNetActions action = db.AspNetActions.Where(x => x.Id == id).FirstOrDefault();

                            if (menu != null)
                            {
                                item.Id = menu.Id;
                                item.Name = menu.Name;
                                item.Order = menu.Order;
                                item.IdParent = menu.IdParent;
                                if (action != null)
                                {
                                    string[] array = action.Route.Split('/');
                                    string controller = array[0];
                                    string act = array[1];
                                    item.Action = act;
                                    item.Controller = controller;
                                }
                                user.Menus.Add(item);
                            }
                        }

                    }

                    // Se suman a la lista de acciones todas aquellas que permiten ejecución anónima.
                    user.Actions.AddRange(db.AspNetActions.Where(a => a.Anonymous == true).Select(a => a.Route).ToList());

                    Session.Add("user", user);
                }
                else
                {
                    // El usuario no está autenticado
                    return;
                }
            }

            // Con la ruta de navegación se determina que acción se intenta ejecutar y con eso, se valida contra la configuración
            // de seguridad. El operador debe tener asociado un rol que pueda ejecutarla.
            var area = ControllerContext.RouteData.DataTokens["area"];
            if (area != null) area = area.ToString() + "/"; else area = "";
            string route = area + filterContext.ActionDescriptor.ControllerDescriptor.ControllerName +
                       "/" + filterContext.ActionDescriptor.ActionName;

            //Si cierra la sesion no hay log 
            if (filterContext.ActionDescriptor.ActionName == "LogOff")
            {
                return;
            }

            // Se valida si la acción actual está dentro de la lista de acciones permitidas.
            if (!((ApplicationUser)Session["user"]).Actions.Contains(route))
            {
                // La acción no está permitida, se redirecciona la ejecución.
                filterContext.Result = new RedirectResult("~/Home/Index");

                //Se registra el intento de ejecución no permitida
                AspNetUsersNoAccess user = new AspNetUsersNoAccess();
                AspNetActions action = db.AspNetActions.Where(a => a.Route == route).FirstOrDefault();
                if (action != null)
                {
                    user.ActionId = action.Id;
                }
                user.Date = System.DateTime.Now;
                user.Route = route;
                user.UserId = userId;
                user.UserName = userName;
                user.Parameters = filterContext.ActionParameters.FirstOrDefault().Key + "=" + filterContext.ActionParameters.FirstOrDefault().Value;
                user.IpAddress = Request.UserHostAddress;
                db.AspNetUsersNoAccess.Add(user);
                db.SaveChangesAsync();
                return;
            }

            else
            {
                // La acción está permitida.Se registra la ejecución.
                AspNetUsersAccess user = new AspNetUsersAccess();
                user.ActionId = db.AspNetActions.Where(a => a.Route == route).FirstOrDefault().Id;
                user.Date = System.DateTime.Now;
                user.Parameters = filterContext.ActionParameters.FirstOrDefault().Key + "=" + filterContext.ActionParameters.FirstOrDefault().Value;
                user.Route = route;
                user.UserId = userId;
                user.UserName = userName;
                user.IpAddress = Request.UserHostAddress;
                db.AspNetUsersAccess.Add(user);
                db.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Sobrecarga del método OnException utilizado cuando ocurre una Exception no manejada.
        /// Lo que se hace es devolver un mensaje de error con los detalles correspondientes del mismo
        /// con el formato de envío correspondiente según el tipo de Request (POST o GET).
        /// Además guarda el detalle de la Exception en la base de datos
        /// </summary>
        protected override void OnException(ExceptionContext filterContext)
        {
            if (filterContext.ExceptionHandled)
                return;
            Exception ex = filterContext.Exception;

            ///Se guarda la excepción en la base de datos
            SaveException(ex);
            HFXException m = new HFXException()
            {
                Success = false,
                UserMessage = "Ha ocurrido un error",
                ExMessage = ex.Message,
                StackTrace = ex.StackTrace
            };

            if (Request.RequestType.Equals("POST"))
                filterContext.Result = ReturnErrorMessage(m);

            if (Request.RequestType.Equals("GET"))
                filterContext.Result = ReturnErrorView(m);

            filterContext.ExceptionHandled = true;
        }

        /// <summary>
        /// Se utiliza el siguiente método para retornar un mensaje al usuario en métodos POST.
        /// Para que se pueda ver en pantalla, debe ejecutarse la función ExecuteMessagesSuccess 
        /// en el evento success de la llamada ajax correspondiente.
        /// </summary>
        /// <param name="message">Mensaje a mostrar al usuario</param>
        /// <returns></returns>
        protected JsonResult ReturnMessage(string message)
        {
            System.Web.Mvc.JsonResult a = new JsonResult();
            object data = new
            {
                Success = false,
                UserMessage = message
            };
            a.Data = data;
            return a;
        }

        /// <summary>
        /// Se utiliza el siguiente método para retornar un mensaje con el error acontecido al 
        /// usuario en métodos de cargas de PartialViews..
        /// </summary>
        /// <param name="message">Mensaje a mostrar al usuario</param>
        /// <returns></returns>
        protected ActionResult ReturnException(Exception ex)
        {
            ///Se guarda la excepción en la base de datos
            SaveException(ex);
            HFXException m = new HFXException()
            {
                Success = false,
                UserMessage = "Ha ocurrido un error",
                ExMessage = ex.Message,
                StackTrace = ex.StackTrace
            };
            return PartialView("_ExceptionMessage", m);
        }

        /// <summary>
        /// Envío de Mensaje de Error para métodos POST
        /// </summary>
        private ActionResult ReturnErrorMessage(HFXException m)
        {
            System.Web.Mvc.JsonResult a = new JsonResult();
            object data = new
            {
                Success = false,
                Partial = this.RenderPartialViewToString("_ExceptionMessage", m)
            };
            a.Data = data;
            return a;
        }

        /// <summary>
        /// Envío de Mensaje de Error para métodos GET
        /// </summary>
        private ActionResult ReturnErrorView(HFXException m)
        {
            return PartialView("ErrorView", m);
        }

        /// <summary>
        /// Método utilizado para renderizar una Vista y enviarla en string.
        /// Se utiliza para el envío de Excepciones en métodos ajax.
        /// </summary>
        /// <param name="viewName">Nombre de la Vista</param>
        /// <param name="model">Modelo a enviar</param>
        /// <returns></returns>
        private string RenderPartialViewToString(string viewName, object model)
        {
            Controller controller = this;
            if (string.IsNullOrEmpty(viewName))
                viewName = controller.ControllerContext.RouteData.GetRequiredString("action");

            controller.ViewData.Model = model;

            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(controller.ControllerContext, viewName);
                var viewContext = new ViewContext(controller.ControllerContext, viewResult.View, controller.ViewData, controller.TempData, sw);
                viewResult.View.Render(viewContext, sw);

                return sw.GetStringBuilder().ToString();
            }
        }

        /// <summary>
        /// Guarda la Excepción en la base de datos con los datos de Controlador, Acción y Parametros utilizados
        /// </summary>
        /// <param name="ex">Excepción ocurrida</param>
        private void SaveException(Exception ex)
        {
            string parameters = "";
            int i = 0;
            while (Request.Params.Keys[i] != "__RequestVerificationToken")
            {
                parameters += Request.Params.Keys[i] + "=" + Request.Params[i] + " ";
                i++;
            }
            AspNetExceptions exception = new AspNetExceptions()
            {
                Action = ControllerContext.RouteData.Values["action"].ToString(),
                Controller = ControllerContext.RouteData.Values["controller"].ToString(),
                Date = DateTime.Now,
                ExMessage = ex.Message,
                StackTrace = ex.StackTrace,
                Parameters = parameters,
                UserId = HttpContext.User.Identity.GetUserId(),
                Username = HttpContext.User.Identity.GetUserName(),
                UserIpAddress = Request.UserHostAddress
            };
            db = new ApplicationDbContext();
            db.AspNetExceptions.Add(exception);
            db.SaveChangesAsync();
        }

    }
}