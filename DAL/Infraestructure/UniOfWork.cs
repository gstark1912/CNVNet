using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Infraestructure
{
    /// <summary>
    /// Esta clase es responsable de implementar el patrón "Unidad de Trabajo".
    /// Básicamente lo que se logra con este patrón, es mantener una única instancia de contexto, para que el ORM
    /// (el encargo de mantener el estado de las entidades) pueda tener control de todas las entidades a utilizar,
    /// sin que la ida y vuelta entre repositorios nos genere una desconexión.
    /// 
    /// Para que esto funcione, es necesario declarar en esta clase todos los repositorios que deben incluirse.  
    /// </summary>
    public class UnitOfWork : IDisposable
    {
        // Contexto de Entity Framework
        private KATHIECentralEntities context = new KATHIECentralEntities();

        // Atributos privados que contienen al repositorio, en la instancia correspondiente.
        private GenericRepository<Dia> _diaR { get; set; }

        // Atributos públicos:
        // Estos atributos son los que se usan para acceder a los repositorios desde los servicios a través de esta clase.
        // Antes de retornar un valor, la clase verificará que no el repositorio esté instanciado. En caso de no estarlo, lo instanciará
        // usando el contexto declarado más arriba.
        // Esto logra que cada repositorio que se utilice con esta unidad de trabajo, logre tener el mismo contexto y poder así mantener
        // el mismo estado de las entidades entre todos los repositorios.
        //
        // NOTA: Debido a la implementación del repositorio genérico podemos obtener el repositorio de una entidad sin crear ninguna clase, esto lo logramos
        // creando un atributo del tipo GenericRepository<TEntidad>, donde TEntidad es la entidad a utilizar.
        // En caso contrario, creamos una clase que herede de GenericRepository y modificamos/agregamos los métodos como se requiera.
        public GenericRepository<Dia> diaRepository { get { return GetRepository(_diaR); } set { this._diaR = value; } }
        /// <summary>
        /// Método utilizado para verificar si el repositorio a verificar está instanciado, para instanciarlo en caso de que no sea así
        /// o sólo retornarlo en caso de ya estarlo.
        /// </summary>
        private TEntidad GetRepository<TEntidad>(TEntidad repository) where TEntidad : class
        {
            if (repository == null)
                repository = (TEntidad)Activator.CreateInstance(typeof(TEntidad), context);
            return repository;
        }

        /// <summary>
        /// Métood utilizado para actualizar los datos de todas las entidades a actualizar.
        /// </summary>
        public void Save()
        {
            context.SaveChanges();
        }

        /// <summary>
        /// Los métodos y atributos a continuación se encargan de desechar la conexión a la base de datos cuando la clase deje de usarse,
        /// para evitar sobrecargar el procesador.
        /// </summary>
        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    context.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
