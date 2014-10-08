using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Data.Entity.ModelConfiguration.Conventions;


namespace MVC.Security.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }
        public string LastName { get; set; }
        public DateTime? LastPasswordDate { get; set; }
        public bool IsLogged { get; set; }
        public DateTime? PasswordChangeExpiration { get; set; }
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
        public List<string> Actions { get; set; }
        public virtual List<AspNetUserPassword> AspNetUserPassword { get; set; }
        public virtual List<AspNetUsersAccess> AspNetUsersAccess { get; set; }
        public virtual List<AspNetUsersNoAccess> AspNetUsersNoAccess { get; set; }

        [NotMapped]
        public List<MenuItem> Menus { get; set; }
    }

    public class AspNetUserPassword
    {
        [Key]
        public int IdAspNetUserPassword { get; set; }
        public string PasswordHash { get; set; }
        public DateTime? PasswordDate { get; set; }

        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser ApplicationUser { get; set; }
    }
    public class AspNetRoleAction
    {
        public int Id { get; set; }
        public string IdRole { get; set; }
        public int IdAction { get; set; }

        [ForeignKey("IdAction")]
        public virtual AspNetActions Action { get; set; }

        [ForeignKey("IdRole")]
        public virtual IdentityRole Role { get; set; }
    }

    /// <summary>
    /// Configuración general de la aplicación
    /// </summary>
    public class AspNetGeneralConfiguration
    {
        public int Id { get; set; }
        public string CredentialUserName { get; set; }
        public string EmailPassword { get; set; }
        public string Client { get; set; }
        public bool EnableSSL { get; set; }
        public bool DefaultCredentials { get; set; }
        public int Port { get; set; }
        public int PasswordRequiredLength { get; set; }
        public bool PasswordRequireDigit { get; set; }
        public bool PasswordRequireLowercase { get; set; }
        public bool PasswordRequireUppercase { get; set; }
        public int MaxFailedAccessCount { get; set; }
        public int PasswordRepeat { get; set; }
        public int ExpirationTimeDays { get; set; }
    }

    /// <summary>
    /// Acciones de la aplicación
    /// </summary>
    public partial class AspNetActions
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Route { get; set; }
        public string Name { get; set; }
        public bool Anonymous { get; set; }

        public virtual ICollection<AspNetRoleAction> AspNetRoleAction { get; set; }
        public virtual ICollection<AspNetUsersNoAccess> AspNetUsersNoAccess { get; set; }
        public virtual ICollection<AspNetActionInMenu> AspNetActionInMenu { get; set; }
        public virtual ICollection<AspNetMenu> AspNetMenu { get; set; }
    }

    /// <summary>
    /// Log de intentos de acceder a acciones no
    /// permitidas
    /// </summary>
    public class AspNetUsersNoAccess
    {
        public int Id { get; set; }
        public string IpAddress { get; set; }
        public string UserId { get; set; }
        public int ActionId { get; set; }
        public string Route { get; set; }
        public System.DateTime Date { get; set; }
        public string UserName { get; set; }
        public string Parameters { get; set; }
    }

    /// <summary>
    /// Log de acciones permitidas para el usuario.
    /// </summary>
    public class AspNetUsersAccess
    {
        public int Id { get; set; }
        public string IpAddress { get; set; }
        public string UserId { get; set; }
        public int ActionId { get; set; }
        public string Route { get; set; }
        public System.DateTime Date { get; set; }
        public string UserName { get; set; }
        public string Parameters { get; set; }
    }

    /// <summary>
    /// Log de inicios de sesión.
    /// </summary>
    public class AspNetSignInLog
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public DateTime Date { get; set; }
        public bool Success { get; set; }
        public string Details { get; set; }
    }

    /// <summary>
    /// Log de inicios de sesión.
    /// </summary>
    public class AspNetSignOutLog
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public DateTime Date { get; set; }
    }

    /// <summary>
    /// As
    /// </summary>
    public class AspNetExceptions
    {
        public int Id { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public string ExMessage { get; set; }
        public string StackTrace { get; set; }
        public System.DateTime Date { get; set; }
        public string Parameters { get; set; }
        public string UserId { get; set; }
        public string Username { get; set; }
        public string UserIpAddress { get; set; }
    }

    #region Menu
    /// <summary>
    /// Tabla para la asignacion de menus dinamicos 
    /// </summary>
    public class AspNetMenu
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }


        public int IdAction { get; set; }
        public Nullable<int> IdParent { get; set; }
        public Nullable<int> Order { get; set; }

        [ForeignKey("IdAction")]
        public virtual AspNetActions AspNetActions { get; set; }

        public virtual ICollection<AspNetActionInMenu> AspNetActionInMenu { get; set; }

    }


    public class MenuItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public string Area { get; set; }
        public Nullable<int> Order { get; set; }
        public Nullable<int> IdParent { get; set; }
        public virtual ICollection<MenuItem> SubMenues { get; set; }
    }

    /// <summary>
    /// En esta tabla se especifica que acciones corresponden a cada menu
    /// </summary>
    public class AspNetActionInMenu
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int IdMenu { get; set; }
        public int IdAction { get; set; }

        [ForeignKey("IdAction")]
        public virtual AspNetActions AspNetActions { get; set; }

        [ForeignKey("IdMenu")]
        public virtual AspNetMenu AspNetMenu { get; set; }
    }
    #endregion


    /// <summary>
    /// Contexto de la aplicación para acceder a la base de datos.
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("SecurityDbs", throwIfV1Schema: false)
        //: base("DefaultConnection", throwIfV1Schema: false)
        // El string que se incluye dentro de base(), debe ser:
        // DefaultConnection para que la aplicación se encargue de crear dicha base en el entorno local que disponga
        // El nombre de un connectionstring declara en el WebConfig en caso de tener la base creada
        {
        }

        public System.Data.Entity.DbSet<AspNetGeneralConfiguration> AspNetGeneralConfiguration { get; set; }
        public System.Data.Entity.DbSet<AspNetUsersNoAccess> AspNetUsersNoAccess { get; set; }
        public System.Data.Entity.DbSet<AspNetUsersAccess> AspNetUsersAccess { get; set; }
        public System.Data.Entity.DbSet<AspNetActions> AspNetActions { get; set; }
        public System.Data.Entity.DbSet<AspNetRoleAction> AspNetRoleAction { get; set; }
        public System.Data.Entity.DbSet<AspNetSignInLog> AspNetSignInLog { get; set; }
        public System.Data.Entity.DbSet<AspNetSignOutLog> AspNetSignOutLog { get; set; }
        public System.Data.Entity.DbSet<AspNetMenu> AspNetMenu { get; set; }
        public System.Data.Entity.DbSet<AspNetUserPassword> AspNetUserPassword { get; set; }
        public System.Data.Entity.DbSet<AspNetActionInMenu> AspNetActionInMenu { get; set; }
        public System.Data.Entity.DbSet<AspNetExceptions> AspNetExceptions { get; set; }

        static ApplicationDbContext()
        {
            // Set the database intializer which is run once during application start
            // This seeds the database with admin user credentials and admin role
            Database.SetInitializer<ApplicationDbContext>(new ApplicationDbInitializer());
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            base.OnModelCreating(modelBuilder);
            /*
            modelBuilder.Entity<AspNetMenu>()
                .HasRequired(m => m.AspNetActions)
                .WithMany(m => m.AspNetMenu)
                .WillCascadeOnDelete(false);
             */
        }
    }

    public static class Extensions
    {
        /// <summary>
        /// Permite incluir entidades relacionadas a la entidad resultante que se consigue evitando usar un string como lo propone Microsoft
        /// </summary>
        public static IQueryable<T> IncludeMultiple<T>(this IQueryable<T> query, params Expression<Func<T, object>>[] includes) where T : class
        {
            if (includes != null)
            {
                query = includes.Aggregate(query,
                          (current, include) => current.Include(include));
            }

            return query;
        }

        /// <summary>
        /// A partir de un IQueryable aplicado a una entidad definida, se aplica la estrategia de Paginado
        /// y se devuelve la misma query con el agregado del paginado.
        /// </summary>        
        /// 

        public static IQueryable<T> PagedData<T>(this IQueryable<T> query, string sidx, string sord, int page, int rows) where T : class
        {
            return ApplyStrategy(query, sidx, sord, page, rows);
        }

        /// <summary>
        /// Genera la creación de la query a aplicar a partir de la ya existente enviada
        /// en el parámetro "source".
        /// Soporta la obtención de propiedades de una dependencia de la entidad,
        /// separadas por punto.
        /// Ej: Si se quisiera ordenar por la propiedad MainEntity.ChildEntity.PropertyName, debería ser
        /// enviado en el OrderFieldName de los parameters un string: "ChildEntity.PropertyName".
        /// </summary>      
        /// <param name="page">Página Actuali</param>
        /// <param name="rows">Cantidad de registros por página</param>        
        /// <param name="sidx">Nombre del campo por el que se ordena</param>
        /// <param name="sord">Sentido de orden. "asc" para Ascendente y "desc" para descendente</param>
        private static IQueryable<T> ApplyStrategy<T>(IQueryable<T> source, string sidx, string sord, int page, int rows)
        {
            var parameter = Expression.Parameter(typeof(T), "Entity");
            var propertyName = sidx;
            PropertyInfo property;
            Expression propertyAccess;

            /// Identifica si se refiere a una propiedad de la entidad, o
            /// a una propiedad de una entidad relacionada.
            if (propertyName.Contains('.'))
            {
                String[] childProperties = propertyName.Split('.');
                property = typeof(T).GetProperty(childProperties[0]);
                propertyAccess = Expression.MakeMemberAccess(parameter, property);
                for (int i = 1; i < childProperties.Length; i++)
                {
                    property = property.PropertyType.GetProperty(childProperties[i]);
                    propertyAccess = Expression.MakeMemberAccess(propertyAccess, property);
                }
            }
            else
            {
                property = typeof(T).GetProperty(propertyName);
                propertyAccess = Expression.MakeMemberAccess(parameter, property);
            }

            LambdaExpression expresion = Expression.Lambda(propertyAccess, parameter);

            // Define el orden de la query
            string orderby = sord == "desc" ? "OrderByDescending" : "OrderBy";

            /// Creación de la expresión a agregar a la query, ya con la obtención
            /// de las propiedades en una Expresión Lambda.
            MethodCallExpression resultExp = Expression.Call(typeof(Queryable), orderby,
                            new Type[] { typeof(T), property.PropertyType },
                            source.Expression, Expression.Quote(expresion));


            return (source.Provider.CreateQuery<T>(resultExp) as IQueryable<T>)
                        .Skip((page - 1) * rows) // Implementación de la página.
                        .Take(rows); // Implementación de la cantidad de filas por página.

        }
    }




}

