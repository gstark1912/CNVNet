//------------------------------------------------------------------------------
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
    using System.Collections.Generic;
    
    public partial class EventoUsuario
    {
        public EventoUsuario()
        {
            this.EventoUsuarioHorarios = new HashSet<EventoUsuarioHorario>();
        }
    
        public int IDEventoUsuario { get; set; }
        public int IDEvento { get; set; }
        public int IDUsuario { get; set; }
    
        public virtual Evento Evento { get; set; }
        public virtual Usuario Usuario { get; set; }
        public virtual ICollection<EventoUsuarioHorario> EventoUsuarioHorarios { get; set; }
    }
}
