using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVC.Models
{
    public class HomeViewModel
    {
        public HomeViewModel()
        {
            AdminNotes = new List<string>();
        }
        public List<String> AdminNotes { get; set; }
    }
}