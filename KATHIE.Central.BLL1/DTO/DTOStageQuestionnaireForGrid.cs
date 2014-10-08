using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KATHIE.Central.BLL1
{
    class DTOStageQuestionnaireForGrid
    {
        public int IDStageQuestionnaire { get; set; }
        public int IDStageConfiguration { get; set; }
        public string Questionnaire { get; set; }
        public int Order { get; set; }
        public string CheckUpNumber { get; set; }
        public string FeaturesToSet { get; set; }
    }
}
