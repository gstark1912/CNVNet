using Domain;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Infraestructure
{
    /// <summary>
    /// Implementación de Repositorio Genérico de una Entidad
    /// Se entiende que todas las tablas contienen operaciones que funcionan de igual manera
    /// sin importar la entidad. Para esto se utiliza el patrón Repository que en nuestro caso
    /// se encargará de implementar de forma genérica los métodos Insert, Update, Delete, GetByID
    /// y GetAll.    
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class GenericRepository<TEntity> where TEntity : class
    {
        // Contexto a utilizar
        internal KATHIECentralEntities context;
        internal DbSet<TEntity> dbSet;

        /// <summary>
        /// El constructor recibe el contexto a utilizar y setea el DbSet que utilizará
        /// para las siguientes operaciones, basado en la Entidad a representar.
        /// </summary>        
        public GenericRepository(KATHIECentralEntities context)
        {
            this.context = context;
            this.dbSet = context.Set<TEntity>();
        }

        /// <summary>
        /// Retorna todas las filas de una entidad
        /// </summary>        
        public virtual IEnumerable<TEntity> GetAll()
        {
            IQueryable<TEntity> query = dbSet;
            return query.ToList();
        }

        /// <summary>
        /// Retorna la entidad que tenga como PK el ID pasado sin importar el tipo de dato que sea.
        /// Al utilizar al método Find el ORM se ocupa de no realizar una consulta a la base en caso de que dicha 
        /// entidad ya exista en el contexto, sino de traer dicha instancia.
        /// </summary>
        public virtual TEntity GetById(object id)
        {
            return dbSet.Find(id);
        }

        /// <summary>
        /// Inserta un nuevo registro en la entidad
        /// </summary>        
        public virtual void Insert(TEntity entity)
        {
            dbSet.Add(entity);
        }

        /// <summary>
        /// Elimina un registro existente en la entidad
        /// </summary>        
        public virtual void Delete(object id)
        {
            TEntity entityToDelete = dbSet.Find(id);
            Delete(entityToDelete);
        }

        /// <summary>
        /// Elimina un registro existente en la entidad
        /// </summary>        
        public virtual void Delete(TEntity entityToDelete)
        {
            if (context.Entry(entityToDelete).State == EntityState.Detached)
            {
                dbSet.Attach(entityToDelete);
            }
            dbSet.Remove(entityToDelete);
        }

        /// <summary>
        /// Actualiza los datos de un registro existente en la entidad
        /// </summary>        
        public virtual void Update(TEntity entityToUpdate)
        {
            dbSet.Attach(entityToUpdate);
            context.Entry(entityToUpdate).State = EntityState.Modified;
        }
    }
}
