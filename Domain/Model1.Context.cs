﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Este código se generó a partir de una plantilla.
//
//     Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//     Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Domain
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class KATHIECentralEntities : DbContext
    {
        public KATHIECentralEntities()
            : base("name=KATHIECentralEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Dia> Dias { get; set; }
        public virtual DbSet<Disponibilidad> Disponibilidads { get; set; }
        public virtual DbSet<Estado> Estadoes { get; set; }
        public virtual DbSet<Evento> Eventoes { get; set; }
        public virtual DbSet<EventoUsuario> EventoUsuarios { get; set; }
        public virtual DbSet<EventoUsuarioHorario> EventoUsuarioHorarios { get; set; }
        public virtual DbSet<Usuario> Usuarios { get; set; }
        public virtual DbSet<UsuarioHorario> UsuarioHorarios { get; set; }
    }
}
