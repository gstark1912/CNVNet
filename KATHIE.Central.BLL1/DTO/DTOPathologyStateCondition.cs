
using KATHIE.Central.Domain;
namespace KATHIE.Central.BLL1
{
    public class DTOPathologyStateCondition
    {
        public int IDPathologyStateCondition { get; set; }
        public int IDPathologyState { get; set; }
        public int IDMedicalProperty { get; set; }
        public int IDCondition { get; set; }
        public decimal Value { get; set; }
        public int IDLogicOperator { get; set; }
        public bool IncludeNotApply { get; set; }
        public Condition Condition { get; set; }
        public LogicOperator LogicOperator { get; set; }
        public MedicalProperty MedicalProperty { get; set; }

        public int IDDefaultLogicOperatorY { get; set; }
        public int IDDefaultLogicOperatorEmpty { get; set; }
    }
}
