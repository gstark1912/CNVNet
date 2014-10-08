using MVC.Security.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC.Security.Models
{
    /// <summary>
    /// Modelo utilizado para mostrar las excepciones en la vista _ExceptionMessage
    /// </summary>
    public class HFXException
    {
        public bool Success { get; set; }
        public string UserMessage { get; set; }
        public string ExMessage { get; set; }
        public string StackTrace { get; set; }
    }


}
