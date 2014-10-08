using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Filters
{
    public class MedicineSearch
    {
        public string term { get; set; }
        public bool byDescription { get; set; }
        public bool byDrug { get; set; }
        public bool byTherapeuticAction { get; set; }
        public bool byLaboratory { get; set; }
        public bool byPresentation { get; set; }
        public bool byCode { get; set; }
        public bool byName { get; set; }
        public bool active { get; set; }
    }
}
