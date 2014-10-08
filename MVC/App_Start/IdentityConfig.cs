using System.Linq;
using System.Security.Claims;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Web;

namespace MVC.Security.Models
{
    // Configure the application user manager used in this application. UserManager is defined in ASP.NET Identity and is used by the application.

    public class ApplicationUserManager : UserManager<ApplicationUser>
    {
        public ApplicationUserManager(IUserStore<ApplicationUser> store)
            : base(store)
        {
        }

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context)
        {
            var manager = new ApplicationUserManager(new UserStore<ApplicationUser>(context.Get<ApplicationDbContext>()));
            AspNetGeneralConfiguration entity;
            try
            {
                using (var dbContext = new ApplicationDbContext())
                {
                    entity = dbContext.AspNetGeneralConfiguration.FirstOrDefault();
                }
            }
            catch (Exception)
            {
                entity = null;
            }
            // Configuración de Validación de Contraseña
            if (entity != null)
            {
                manager.PasswordValidator = new PasswordValidator
                {
                    RequiredLength = entity.PasswordRequiredLength, //Longitud de Contraseña
                    RequireDigit = entity.PasswordRequireDigit, //Requiere Número
                    RequireLowercase = entity.PasswordRequireLowercase, //Requiere Minúscula
                    RequireUppercase = entity.PasswordRequireUppercase, //Requiere Mayúscula
                };

                // Configuración de Bloqueo de Cuenta por Intentos fallido de Login

                manager.UserLockoutEnabledByDefault = entity.MaxFailedAccessCount > 0 ? true : false; //Habilitado si la cantidad de intentos permitidos es mayor a 0
                manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromDays(364635); //Simluación de Bloqueo definitivo
                manager.MaxFailedAccessAttemptsBeforeLockout = entity.MaxFailedAccessCount; //Cantidad máxima de intentos permitidos
            }
            else
            {
                //Configuración por Defecto propuesta por MVC necesaria para que la aplciación corra
                manager.PasswordValidator = new PasswordValidator
                {
                    RequiredLength = 6,
                    RequireNonLetterOrDigit = false,
                    RequireDigit = false,
                    RequireLowercase = false,
                    RequireUppercase = false,
                };

                manager.UserLockoutEnabledByDefault = true;
                manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
                manager.MaxFailedAccessAttemptsBeforeLockout = 5;
            }

            // Configuración de validación por nombre de usuarios
            manager.UserValidator = new UserValidator<ApplicationUser>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };

            //Validación de Email
            manager.RegisterTwoFactorProvider("EmailCode", new EmailTokenProvider<ApplicationUser>
            {
                Subject = "SecurityCode",
                BodyFormat = "Your security code is {0}"
            });
            manager.EmailService = new EmailService();

            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider = new DataProtectorTokenProvider<ApplicationUser>(dataProtectionProvider.Create("ASP.NET Identity"));
            }
            return manager;
        }

        /// <summary>
        /// Método para agregar un Usuario a múltiples Roles
        /// </summary>
        /// <param name="userId">Id de Usuario</param>
        /// <param name="roles">Lista de nombres de roles</param>        
        public virtual async Task<IdentityResult> AddUserToRolesAsync(string userId, IList<string> roles)
        {
            var userRoleStore = (IUserRoleStore<ApplicationUser, string>)Store;

            var user = await FindByIdAsync(userId).ConfigureAwait(false);
            if (user == null)
            {
                throw new InvalidOperationException("Invalid user Id");
            }

            var userRoles = await userRoleStore.GetRolesAsync(user).ConfigureAwait(false);
            // Agregar el usuario a cada rol utilizando UserRolesStore
            foreach (var role in roles.Where(role => !userRoles.Contains(role)))
            {
                await userRoleStore.AddToRoleAsync(user, role).ConfigureAwait(false);
            }

            // Hace Update del usuario con los nuevos roles
            return await UpdateAsync(user).ConfigureAwait(false);
        }

        /// <summary>
        /// Elimina al usuario de múltiples roles
        /// </summary>
        /// <param name="userId">Id de usuario</param>
        /// <param name="roles">Lista de nombres de roles</param>
        /// <returns></returns>
        public virtual async Task<IdentityResult> RemoveUserFromRolesAsync(string userId, IList<string> roles)
        {
            var userRoleStore = (IUserRoleStore<ApplicationUser, string>)Store;

            var user = await FindByIdAsync(userId).ConfigureAwait(false);
            if (user == null)
            {
                throw new InvalidOperationException("Invalid user Id");
            }

            var userRoles = await userRoleStore.GetRolesAsync(user).ConfigureAwait(false);
            // Elimina al usuario de cada Rol utilizando UserRoleStore
            foreach (var role in roles.Where(userRoles.Contains))
            {
                await userRoleStore.RemoveFromRoleAsync(user, role).ConfigureAwait(false);
            }

            // Hace Update del Usuario con los nuevos roles
            return await UpdateAsync(user).ConfigureAwait(false);
        }

        /// <summary>
        /// Verifica basado en el número permitido de contraseñas anteriores configurado,
        /// si la nueva contraseña ingresada es válida.
        /// </summary>
        /// <param name="id">Id de usuario</param>
        /// <param name="password">Contraseña</param>
        /// <returns></returns>
        public bool PasswordIsRepeated(string id, string password)
        {
            using (var dbContext = new ApplicationDbContext())
            {
                ApplicationUser user = dbContext
                    .Users
                    .Where(u => u.Id == id)
                    .FirstOrDefault();
                AspNetGeneralConfiguration conf = dbContext.AspNetGeneralConfiguration.FirstOrDefault();
                if (conf != null)
                {
                    int allowedRep = conf.PasswordRepeat;
                    if (allowedRep > 0)
                    {
                        int aux = 1;
                        foreach (AspNetUserPassword item in user.AspNetUserPassword.OrderByDescending(a => a.PasswordDate))
                        {
                            if (PasswordHasher.VerifyHashedPassword(item.PasswordHash, password) == PasswordVerificationResult.Success && aux <= allowedRep)
                                return true;
                            aux++;
                        }
                    }

                }
            }
            return false;
        }

        /// <summary>
        /// Guarda la nueva contraseña en la base de datos
        /// </summary>
        /// <param name="id">Id de usuario</param>
        /// <param name="password">Contraseña</param>
        public void SavePassword(string id)
        {
            using (var dbContext = new ApplicationDbContext())
            {
                ApplicationUser user = dbContext.Users.Where(u => u.Id == id).FirstOrDefault();
                AspNetUserPassword p = new AspNetUserPassword { PasswordDate = DateTime.Now, PasswordHash = user.PasswordHash, UserId = id };

                user.AspNetUserPassword.Add(p);
                user.PasswordChangeExpiration = null;
                user.LastPasswordDate = DateTime.Now;
                dbContext.Entry(user).State = System.Data.Entity.EntityState.Modified;
                dbContext.Entry(p).State = System.Data.Entity.EntityState.Added;
                dbContext.SaveChanges();
            }
        }
    }

    // Configuración del RoleManager
    public class ApplicationRoleManager : RoleManager<IdentityRole>
    {
        public ApplicationRoleManager(IRoleStore<IdentityRole, string> roleStore)
            : base(roleStore)
        {
        }

        public static ApplicationRoleManager Create(IdentityFactoryOptions<ApplicationRoleManager> options, IOwinContext context)
        {
            var manager = new ApplicationRoleManager(new RoleStore<IdentityRole>(context.Get<ApplicationDbContext>()));

            return manager;
        }
    }

    // Configuración del Servicio de Mails
    public class EmailService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            AspNetGeneralConfiguration entity;
            string credentialUserName;
            string sentFrom;
            string pwd;
            string messageSubject;
            string messageBody;
            System.Net.Mail.SmtpClient client;
            try
            {
                using (var dbContext = new ApplicationDbContext())
                {
                    entity = dbContext.AspNetGeneralConfiguration.FirstOrDefault();
                }
            }
            catch (Exception)
            {
                entity = null;
            }

            //Configuración del proveedor del servicio de email. 

            if (entity == null)
            {
                //Credenciales 
                credentialUserName = "example@example.com";
                sentFrom = "example@example.com";
                pwd = "pass1234";

                //Configuración del cliente
                client = new System.Net.Mail.SmtpClient("mail.example.com");
                client.EnableSsl = false;
            }
            else
            {
                //Credenciales 
                credentialUserName = entity.CredentialUserName;
                sentFrom = entity.CredentialUserName;
                pwd = entity.EmailPassword;
                int port = entity.Port;

                //Configuración del cliente
                client = new System.Net.Mail.SmtpClient(entity.Client, port);
                client.EnableSsl = entity.EnableSSL;
                client.UseDefaultCredentials = entity.DefaultCredentials;
            }

            messageSubject = message.Subject;
            messageBody = message.Body;

            client.Port = 25;
            client.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;

            //Crea las credenciales
            System.Net.NetworkCredential credentials = new System.Net.NetworkCredential(credentialUserName, pwd);


            client.Credentials = credentials;

            //Crear el mensaje
            var mail = new System.Net.Mail.MailMessage(sentFrom, message.Destination);
            mail.Subject = message.Subject;
            mail.Body = message.Body;

            //Enviar
            return client.SendMailAsync(mail);
        }
    }

    // Esta clase es utilizada al crear la Base de Datos para crear los datos necesarios para 
    // correr la aplicación.
    // Además, evita recrear el modelo la Base de Datos cada vez que corre la aplicación al 
    // heredar de la clase DropCreateDatabaseIfModelChanges. Esta clase sólo recrea las tablas
    // definidas en ApplicationDbContext sólo si dichas tablas (clases) cambian.
    public class ApplicationDbInitializer : DropCreateDatabaseIfModelChanges<ApplicationDbContext>
    {
        protected override void Seed(ApplicationDbContext context)
        {
            InitializeIdentityForEF(context);
            base.Seed(context);
        }

        // Inserción de datos necesarios para comenzar a utilizar la aplicación
        // Estos son:
        //          Definición de Menús, Acciones y Acciones en Menús necesarias para el módulo de seguridad
        //          Usuario Admin con contraseña "admin1234"
        //          Rol Admin con permiso a todas las acciones definidas
        //          Asignación del usuario Admin al rol Admin
        public static void InitializeIdentityForEF(ApplicationDbContext db)
        {
            var roleStore = new RoleStore<IdentityRole>(db);
            var roleManager = new RoleManager<IdentityRole>(roleStore);
            var userStore = new UserStore<ApplicationUser>(db);
            var userManager = new UserManager<ApplicationUser>(userStore);

            //Creación del usuario Admin
            var user = new ApplicationUser { UserName = "admin", Email = "admin@admin.com", EmailConfirmed = true };
            userManager.Create(user, "admin1234");

            //Creación del Rol admin
            roleManager.Create(new IdentityRole { Name = "admin" });

            //Asignación de Usuario Admin al Rol admin
            userManager.AddToRole(user.Id, "admin");

            string roleId = roleManager.FindByName("admin").Id;

            // Creación de Menús y Acciones del módulo de Seguridad 
            // Para crear un menú con acciones dentro se realizan los siguientes pasos:
            // 1) Se instancia un AspNetAction que contiene la ruta a la que responderá ese menú.
            // 2) Se instancia un AspNetMenu con la accion creada en el paso anterior.
            // 3) Se instancia un AspNetActionInMenu para relacionar el menú con la acción que definimos al principio. Luego se repite este paso para cada
            // acción no creada en el paso anterior que deba incluirse en el menú.
            // NOTA: en cada acción que se agrega, la idea propuesta es de realizar la asignación de dicha acción al rol "admin". Es por eso que al crear una acción
            // se la instancia definiendo su propiedad AspNetRoleAction de la siguiente forma: "AspNetRoleAction = GetRoleActionForRole(roleId)", siendo roleId el id del rol admin           

            // Menú Inicio y Acciones
            AspNetActions mInicioAction = new AspNetActions { Name = "Index", Route = "Home/Index", Anonymous = true, AspNetRoleAction = GetRoleActionForRole(roleId) };
            AspNetMenu mInicio = new AspNetMenu { Name = "Inicio", IdParent = 0, Order = 1, AspNetActions = mInicioAction };
            db.AspNetActionInMenu.Add(new AspNetActionInMenu { AspNetMenu = mInicio, AspNetActions = mInicioAction });
            db.AspNetActionInMenu.Add(new AspNetActionInMenu { AspNetMenu = mInicio, AspNetActions = new AspNetActions { Name = "Cierre de Sesión", Route = "Account/LogOff", Anonymous = false, AspNetRoleAction = GetRoleActionForRole(roleId) } });
            db.AspNetActionInMenu.Add(new AspNetActionInMenu { AspNetMenu = mInicio, AspNetActions = new AspNetActions { Name = "Cambiar datos personales", Route = "Security/Users/ChangePersonalData", Anonymous = false, AspNetRoleAction = GetRoleActionForRole(roleId) } });
            db.AspNetActionInMenu.Add(new AspNetActionInMenu { AspNetMenu = mInicio, AspNetActions = new AspNetActions { Name = "Guardar datos personales", Route = "Security/Users/SavePersonalData", Anonymous = false, AspNetRoleAction = GetRoleActionForRole(roleId) } });
            db.AspNetActionInMenu.Add(new AspNetActionInMenu { AspNetMenu = mInicio, AspNetActions = new AspNetActions { Name = "Cambiar contraseña", Route = "Security/Users/ChangePassword", Anonymous = false, AspNetRoleAction = GetRoleActionForRole(roleId) } });

            // Menú Roles y Acciones
            AspNetActions mRolesAction = new AspNetActions { Name = "Roles", Route = "Security/RolesAdmin/Index", Anonymous = false, AspNetRoleAction = GetRoleActionForRole(roleId) };
            AspNetMenu mRoles = new AspNetMenu { Name = "Roles", IdParent = 0, Order = 2, AspNetActions = mRolesAction };
            db.AspNetActionInMenu.Add(new AspNetActionInMenu { AspNetMenu = mRoles, AspNetActions = mRolesAction });
            db.AspNetActionInMenu.Add(new AspNetActionInMenu { AspNetMenu = mRoles, AspNetActions = new AspNetActions { Name = "Crear Rol", Route = "Security/RolesAdmin/InsertRole", Anonymous = false, AspNetRoleAction = GetRoleActionForRole(roleId) } });
            db.AspNetActionInMenu.Add(new AspNetActionInMenu { AspNetMenu = mRoles, AspNetActions = new AspNetActions { Name = "Editar Rol", Route = "Security/RolesAdmin/UpdateRole", Anonymous = false, AspNetRoleAction = GetRoleActionForRole(roleId) } });
            db.AspNetActionInMenu.Add(new AspNetActionInMenu { AspNetMenu = mRoles, AspNetActions = new AspNetActions { Name = "Eliminar Rol", Route = "Security/RolesAdmin/DeleteRole", Anonymous = false, AspNetRoleAction = GetRoleActionForRole(roleId) } });
            db.AspNetActionInMenu.Add(new AspNetActionInMenu { AspNetMenu = mRoles, AspNetActions = new AspNetActions { Name = "Verificación de eliminación de rol", Route = "Security/RolesAdmin/CanDeleteRole", Anonymous = false, AspNetRoleAction = GetRoleActionForRole(roleId) } });
            db.AspNetActionInMenu.Add(new AspNetActionInMenu { AspNetMenu = mRoles, AspNetActions = new AspNetActions { Name = "PartialView Rol", Route = "Security/RolesAdmin/_RoleEdit", Anonymous = false, AspNetRoleAction = GetRoleActionForRole(roleId) } });
            db.AspNetActionInMenu.Add(new AspNetActionInMenu { AspNetMenu = mRoles, AspNetActions = new AspNetActions { Name = "Roles", Route = "Security/RolesAdmin/Index", Anonymous = false, AspNetRoleAction = GetRoleActionForRole(roleId) } });
            db.AspNetActionInMenu.Add(new AspNetActionInMenu { AspNetMenu = mRoles, AspNetActions = new AspNetActions { Name = "Buscar Roles", Route = "Security/RolesAdmin/SearchRoles", Anonymous = false, AspNetRoleAction = GetRoleActionForRole(roleId) } });
            db.AspNetActionInMenu.Add(new AspNetActionInMenu { AspNetMenu = mRoles, AspNetActions = new AspNetActions { Name = "Pantalla de Asignación de Acciones a Rol", Route = "Security/RolesAdmin/AssignActions", Anonymous = false, AspNetRoleAction = GetRoleActionForRole(roleId) } });
            db.AspNetActionInMenu.Add(new AspNetActionInMenu { AspNetMenu = mRoles, AspNetActions = new AspNetActions { Name = "Asignación de Acciones a Rol", Route = "Security/RolesAdmin/SaveAssignActions", Anonymous = false, AspNetRoleAction = GetRoleActionForRole(roleId) } });
            db.AspNetActionInMenu.Add(new AspNetActionInMenu { AspNetMenu = mRoles, AspNetActions = new AspNetActions { Name = "Verificación de disponibilidad del nombre rol", Route = "Security/RolesAdmin/IsRoleAvailable", Anonymous = false, AspNetRoleAction = GetRoleActionForRole(roleId) } });

            // Menú Usuario y Acciones
            AspNetActions mUsuarioAction = new AspNetActions { Name = "Alta Usuarios", Route = "Security/UsersAdmin/Index", Anonymous = false, AspNetRoleAction = GetRoleActionForRole(roleId) };
            AspNetMenu mUsuario = new AspNetMenu { Name = "Usuarios", IdParent = 0, Order = 3, AspNetActions = mUsuarioAction };
            db.AspNetActionInMenu.Add(new AspNetActionInMenu { AspNetMenu = mUsuario, AspNetActions = mUsuarioAction });
            db.AspNetActionInMenu.Add(new AspNetActionInMenu { AspNetMenu = mUsuario, AspNetActions = new AspNetActions { Name = "Alta Usuarios", Route = "Security/UsersAdmin/Index", Anonymous = false, AspNetRoleAction = GetRoleActionForRole(roleId) } });
            db.AspNetActionInMenu.Add(new AspNetActionInMenu { AspNetMenu = mUsuario, AspNetActions = new AspNetActions { Name = "Crear Usuario", Route = "Security/UsersAdmin/InsertUser", Anonymous = false, AspNetRoleAction = GetRoleActionForRole(roleId) } });
            db.AspNetActionInMenu.Add(new AspNetActionInMenu { AspNetMenu = mUsuario, AspNetActions = new AspNetActions { Name = "Eliminar Usuario", Route = "Security/UsersAdmin/DeleteUser", Anonymous = false, AspNetRoleAction = GetRoleActionForRole(roleId) } });
            db.AspNetActionInMenu.Add(new AspNetActionInMenu { AspNetMenu = mUsuario, AspNetActions = new AspNetActions { Name = "Editar Usuario", Route = "Security/UsersAdmin/UpdateUser", Anonymous = false, AspNetRoleAction = GetRoleActionForRole(roleId) } });
            db.AspNetActionInMenu.Add(new AspNetActionInMenu { AspNetMenu = mUsuario, AspNetActions = new AspNetActions { Name = "Vista de Edición de Usuario", Route = "Security/UsersAdmin/_UserEdit", Anonymous = false, AspNetRoleAction = GetRoleActionForRole(roleId) } });
            db.AspNetActionInMenu.Add(new AspNetActionInMenu { AspNetMenu = mUsuario, AspNetActions = new AspNetActions { Name = "Reset de Contraseña por Administracion", Route = "Security/UsersAdmin/ResetPassword", Anonymous = false, AspNetRoleAction = GetRoleActionForRole(roleId) } });
            db.AspNetActionInMenu.Add(new AspNetActionInMenu { AspNetMenu = mUsuario, AspNetActions = new AspNetActions { Name = "Vista de Asignación de Roles", Route = "Security/UsersAdmin/_AssignRoles", Anonymous = false, AspNetRoleAction = GetRoleActionForRole(roleId) } });
            db.AspNetActionInMenu.Add(new AspNetActionInMenu { AspNetMenu = mUsuario, AspNetActions = new AspNetActions { Name = "Desbloquear Usuario", Route = "Security/UsersAdmin/UnlockUser", Anonymous = false, AspNetRoleAction = GetRoleActionForRole(roleId) } });
            db.AspNetActionInMenu.Add(new AspNetActionInMenu { AspNetMenu = mUsuario, AspNetActions = new AspNetActions { Name = "Bloquear Usuario", Route = "Security/UsersAdmin/LockUser", Anonymous = false, AspNetRoleAction = GetRoleActionForRole(roleId) } });
            db.AspNetActionInMenu.Add(new AspNetActionInMenu { AspNetMenu = mUsuario, AspNetActions = new AspNetActions { Name = "Búsqueda de Usuario", Route = "Security/UsersAdmin/SearchUsers", Anonymous = false, AspNetRoleAction = GetRoleActionForRole(roleId) } });
            db.AspNetActionInMenu.Add(new AspNetActionInMenu { AspNetMenu = mUsuario, AspNetActions = new AspNetActions { Name = "Asignación de Roles a Usuario", Route = "Security/UsersAdmin/AssignRoles", Anonymous = false, AspNetRoleAction = GetRoleActionForRole(roleId) } });

            // Menú Configuración General y Acciones
            AspNetActions mConfAction = new AspNetActions { Name = "Configuración General", Route = "Security/ConfigurationAdmin/Index", Anonymous = false, AspNetRoleAction = GetRoleActionForRole(roleId) };
            AspNetMenu mConf = new AspNetMenu { Name = "Configuración General", IdParent = 0, Order = 4, AspNetActions = mConfAction };
            db.AspNetActionInMenu.Add(new AspNetActionInMenu { AspNetMenu = mConf, AspNetActions = mConfAction });
            db.AspNetActionInMenu.Add(new AspNetActionInMenu { AspNetMenu = mConf, AspNetActions = new AspNetActions { Name = "Guardar Configuración General", Route = "Security/ConfigurationAdmin/Save", Anonymous = false, AspNetRoleAction = GetRoleActionForRole(roleId) } });

            // Menú Administración de Menú y Acciones
            AspNetActions mMenuAction = new AspNetActions { Name = "Configuración del menu", Route = "Security/Menu/Index", Anonymous = false, AspNetRoleAction = GetRoleActionForRole(roleId) };
            AspNetMenu mMenu = new AspNetMenu { Name = "Menu", IdParent = 0, Order = 5, AspNetActions = mMenuAction };
            db.AspNetActionInMenu.Add(new AspNetActionInMenu { AspNetMenu = mMenu, AspNetActions = mMenuAction });
            db.AspNetActionInMenu.Add(new AspNetActionInMenu { AspNetMenu = mMenu, AspNetActions = new AspNetActions { Name = "Pop up para edición de Menu", Route = "Security/Menu/_MenuEdit", Anonymous = false, AspNetRoleAction = GetRoleActionForRole(roleId) } });
            db.AspNetActionInMenu.Add(new AspNetActionInMenu { AspNetMenu = mMenu, AspNetActions = new AspNetActions { Name = "Guardarorden y jerarquía de Menús", Route = "Security/Menu/UpdateMenusHierarchy", Anonymous = false, AspNetRoleAction = GetRoleActionForRole(roleId) } });
            db.AspNetActionInMenu.Add(new AspNetActionInMenu { AspNetMenu = mMenu, AspNetActions = new AspNetActions { Name = "Eliminación de Menú", Route = "Security/Menu/DeleteMenu", Anonymous = false, AspNetRoleAction = GetRoleActionForRole(roleId) } });
            db.AspNetActionInMenu.Add(new AspNetActionInMenu { AspNetMenu = mMenu, AspNetActions = new AspNetActions { Name = "Validación previa a Eliminación de Menú", Route = "Security/Menu/CanDeleteMenu", Anonymous = false, AspNetRoleAction = GetRoleActionForRole(roleId) } });
            db.AspNetActionInMenu.Add(new AspNetActionInMenu { AspNetMenu = mMenu, AspNetActions = new AspNetActions { Name = "Edicion de Menú", Route = "Security/Menu/UpdateMenu", Anonymous = false, AspNetRoleAction = GetRoleActionForRole(roleId) } });
            db.AspNetActionInMenu.Add(new AspNetActionInMenu { AspNetMenu = mMenu, AspNetActions = new AspNetActions { Name = "Nuevo Menú", Route = "Security/Menu/InsertMenu", Anonymous = false, AspNetRoleAction = GetRoleActionForRole(roleId) } });
            db.AspNetActionInMenu.Add(new AspNetActionInMenu { AspNetMenu = mMenu, AspNetActions = new AspNetActions { Name = "Refrescar Menús", Route = "Security/Menu/RefreshTreeData", Anonymous = false, AspNetRoleAction = GetRoleActionForRole(roleId) } });

            // Menú Logs y Acciones
            AspNetActions mLogsAction = new AspNetActions { Name = "Home logs", Route = "AccessLogs/Index", Anonymous = false, AspNetRoleAction = GetRoleActionForRole(roleId) };
            AspNetMenu mLogs = new AspNetMenu { Name = "Logs", IdParent = 0, Order = 6, AspNetActions = mLogsAction };
            db.AspNetActionInMenu.Add(new AspNetActionInMenu { AspNetMenu = mLogs, AspNetActions = mLogsAction });
            db.AspNetActionInMenu.Add(new AspNetActionInMenu { AspNetMenu = mLogs, AspNetActions = new AspNetActions { Name = "Accesos de Usuario", Route = "AccessLogs/ViewUsersAccess", Anonymous = false, AspNetRoleAction = GetRoleActionForRole(roleId) } });
            db.AspNetActionInMenu.Add(new AspNetActionInMenu { AspNetMenu = mLogs, AspNetActions = new AspNetActions { Name = "Accesos rechazados de Usuario", Route = "AccessLogs/ViewUsersNoAccess", Anonymous = false, AspNetRoleAction = GetRoleActionForRole(roleId) } });

            // Acciones que permiten su uso de forma anónima
            db.AspNetActions.Add(new AspNetActions { Name = "Navegación de Menú", Route = "Security/Menu/NavigationMenu", Anonymous = true });
            db.AspNetActions.Add(new AspNetActions { Name = "Login", Route = "Account/Login", Anonymous = true });
            db.AspNetActions.Add(new AspNetActions { Name = "Validación de Password", Route = "Account/IsPasswordvalid", Anonymous = true });
            db.AspNetActions.Add(new AspNetActions { Name = "Validación de existencia de correo electrónico", Route = "Account/EmailExists", Anonymous = true });
            db.AspNetActions.Add(new AspNetActions { Name = "Validación de Email", Route = "Security/UsersAdmin/IsEmailAvailable", Anonymous = true });
            db.AspNetActions.Add(new AspNetActions { Name = "Validación de Usuario", Route = "Security/UsersAdmin/IsUserNameAvailable", Anonymous = true });
        }

        /// <summary>
        /// Devuelve la estructura necesaria para asginar una acción a un rol a partir de su Id
        /// </summary>        
        /// <param name="roleId">Id del rol</param>
        private static ICollection<AspNetRoleAction> GetRoleActionForRole(string roleId)
        {
            List<AspNetRoleAction> roleAction = new List<AspNetRoleAction>();
            roleAction.Add(new AspNetRoleAction { IdRole = roleId });
            return roleAction;
        }

    }

    //Enumerado para el resultado del intento de Login
    public enum SignInStatus
    {
        Success,
        LockedOut,
        LockedOutForAttempts,
        Failure,
        RequiresPasswordChange,
        RequiresEmailConfirmation
    }

    // Clase utilizada para poder ejecutar el Login
    public class SignInHelper
    {
        public SignInHelper(ApplicationUserManager userManager, IAuthenticationManager authManager)
        {
            UserManager = userManager;
            AuthenticationManager = authManager;
        }

        public ApplicationUserManager UserManager { get; private set; }
        public IAuthenticationManager AuthenticationManager { get; private set; }


        /// <summary>
        /// Realiza un Login del Usuario sin la necesidad de insertar el password
        /// </summary>
        /// <param name="user">Usuario</param>
        /// <param name="isPersistent">Determina si la expiración de la cookie no sucederá por un tiempo largo</param>
        /// <param name="rememberBrowser">Determina si se debe guardar la cookie para el usuario</param>
        /// <returns></returns>
        public async Task SignInAsync(ApplicationUser user, bool isPersistent, bool rememberBrowser)
        {
            // Clear any partial cookies from external or two factor partial sign ins
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie, DefaultAuthenticationTypes.TwoFactorCookie);
            var userIdentity = await user.GenerateUserIdentityAsync(UserManager);
            if (rememberBrowser)
            {
                var rememberBrowserIdentity = AuthenticationManager.CreateTwoFactorRememberBrowserIdentity(user.Id);
                AuthenticationManager.SignIn(new AuthenticationProperties { IsPersistent = isPersistent }, userIdentity, rememberBrowserIdentity);
            }
            else
            {
                AuthenticationManager.SignIn(new AuthenticationProperties { IsPersistent = isPersistent }, userIdentity);
            }
        }

        /// <summary>
        /// Realiza el Login de Usuario con su respectiva contraseña
        /// </summary>
        /// <param name="userName">Nombre de Usuario</param>
        /// <param name="password">Contraseña</param>
        /// <param name="rememberBrowser">Recordar Usuario</param>
        public async Task<SignInStatus> PasswordSignIn(string userName, string password, bool rememberBrowser)
        {
            var user = await UserManager.FindByNameAsync(userName);
            if (user == null) user = await UserManager.FindByEmailAsync(userName);

            //Si no existe el usuario
            if (user == null)
            {
                return SignInStatus.Failure;
            }

            //Si el usuario tiene su cuenta bloqueada
            if (await UserManager.IsLockedOutAsync(user.Id))
            {
                return SignInStatus.LockedOut;
            }

            //Si el Usuario todavía no confirmó su Correo Electrónico
            if (!await UserManager.IsEmailConfirmedAsync(user.Id))
            {
                return SignInStatus.RequiresEmailConfirmation;
            }

            //Verifica si la contraseña concuerda con la ingresada
            if (await UserManager.CheckPasswordAsync(user, password))
            {
                //Verifica si el usuario debe cambiar su contraseña sea porque la misma
                //expiró, o porque el Administrador la ha reseteado.                
                if (PasswordHasExpire(user) || RequiresPasswordChange(user))
                    return SignInStatus.RequiresPasswordChange;
                else
                {
                    //Sino, realiza el Login del usuario.
                    await SignInAsync(user, true, rememberBrowser);
                    return SignInStatus.Success;
                }
            }

            //Al no concordar la contraseña, registra (si está habilitado) el intento fallido de login
            if (UserManager.UserLockoutEnabledByDefault)
            {
                //Aumenta en 1 el intento fallido
                await UserManager.AccessFailedAsync(user.Id);

                //Verifica si luego del último intento fallido la cuenta se encuentra bloqueada
                if (await UserManager.IsLockedOutAsync(user.Id))
                {
                    return SignInStatus.LockedOutForAttempts;
                }
            }

            return SignInStatus.Failure;
        }

        /// <summary>
        /// Verifica si el usuario debe cambiar su contraseña por reset de parte del Administrador
        /// </summary>
        /// <param name="user">Usuario</param>        
        public bool RequiresPasswordChange(ApplicationUser user)
        {
            if (UserManager.HasPassword(user.Id))
                return false;

            DateTime? date = user.PasswordChangeExpiration;

            if (date == null)
                return true;
            else
            {
                if (date.Value < DateTime.Now)
                {
                    //Expiró la fecha hasta la cual el usuario podía cambiar la password
                    user.PasswordChangeExpiration = null;
                    UserManager.UpdateAsync(user);
                    return false;
                }
                else
                    return true;
            }
        }

        /// <summary>
        /// Determina si (según la configuración) la contraseña del usuario ha expirado
        /// </summary>
        /// <param name="user">Usuario</param>        
        private bool PasswordHasExpire(ApplicationUser user)
        {
            using (var dbContext = new ApplicationDbContext())
            {
                AspNetGeneralConfiguration conf = dbContext.AspNetGeneralConfiguration.FirstOrDefault();
                if (conf == null || user.LastPasswordDate == null)
                    return false;
                TimeSpan timeSpan = DateTime.Today - user.LastPasswordDate.Value;
                if ((conf.ExpirationTimeDays > 0) && (timeSpan.Days >= conf.ExpirationTimeDays))
                    return true;
                return false;
            }
        }
    }

}