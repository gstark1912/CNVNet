using KATHIE.Central.Domain;
using System;
namespace KATHIE.Central.BLL1
{
    public class DTOMedicalPropertyForGrid
    {

        public int IDMedicalPropertyConfiguration { get; set; }
        public string Description { get; set; }
        public string MeasurementUnit { get; set; }
        public string IsManual { get; set; }
        public string IsVisible { get; set; }
        public string AllowNotApply { get; set; }
        public decimal MinValue { get; set; }
        public decimal MaxValue { get; set; }
        public Nullable<decimal> ValueNotApply { get; set; }
    }
}
