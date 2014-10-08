using KATHIE.Central.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KATHIE.Central.BLL1
{
    public static partial class AdapterStageQuestionnaireForGrid
    {
        //public static DTOStageQuestionnaireForGrid toDTOStageQuestionnaireForGrid(this StageQuestionnaire entity)
        //{
        //    if (entity == null) return null;

        //    DTOStageQuestionnaireForGrid dto = new DTOStageQuestionnaireForGrid();
        //    dto.Questionnaire = entity.Questionnaire.Name;
        //    dto.IDStageConfiguration = entity.IDStageConfiguration;
        //    dto.IDStageQuestionnaire = entity.IDStageQuestionnaire;
        //    dto.Order = entity.Order;
        //    dto.CheckUpNumber = entity.CheckupNumber;
        //    if (entity.FeaturePX == null)
        //        dto.FeaturesToSet = "";
        //    else
        //        dto.FeaturesToSet = entity.FeaturePX.Name;
        //    return dto;
        //}

        //public static List<DTOStageQuestionnaireForGrid> toDTOsStageQuestionnaireForGrid(ICollection<StageQuestionnaire> entities)
        //{
        //    if (entities == null) return null;

        //    return entities.Select(e => e.toDTOStageQuestionnaireForGrid()).ToList();
        //}
    }
}
