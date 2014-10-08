/*
 * HFX.Security v1.0.2 (http://www.horizon.com.ar)
 * ========================================================================
 * Copyright 2014 Horizon.
 * ========================================================================
 * Servicio para la logica de negocio de la entidad STAGEQUESTIONNAIRE
*/
using KATHIE.Central.DAL.Repositories;
using KATHIE.Central.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KATHIE.Central.BLL1.Services
{
    public class ServiceStageQuestionnaire
    {
        private readonly RepositoryStageQuestionnaire _rStageQuestionnaire;
        private readonly RepositoryFeaturePXRange _rFeaturePXRange;
        private readonly RepositoryQuestionnaireFeaturesPX _repositoryQuestionnaireFeaturesPX;

        public ServiceStageQuestionnaire()
        {
            _rStageQuestionnaire = new RepositoryStageQuestionnaire();
            _rFeaturePXRange = new RepositoryFeaturePXRange();
            _repositoryQuestionnaireFeaturesPX = new RepositoryQuestionnaireFeaturesPX();
        }

        public List<StageQuestionnaire> GetByStage(int id)
        {
            return _rStageQuestionnaire.GetByStage(id);
        }

        //public PagedDataResult<DTOStageQuestionnaireForGrid> GetForGridByStage(PagedDataParameters parameters, int idStage)
        //{
        //    PagedDataResult<StageQuestionnaire> result = _rStageQuestionnaire.GetPagedByStage(parameters, idStage);
        //    return new PagedDataResult<DTOStageQuestionnaireForGrid>(AdapterStageQuestionnaireForGrid.toDTOsStageQuestionnaireForGrid(result.Results), result.TotalCount);
        //}

        public List<int> GetStages(int idQuestionnaire)
        {
            return _rStageQuestionnaire.GetIdsStages(idQuestionnaire);
        }

        public List<int> GetQuestionnaires(int IDFeaturesPX)
        {
            return _rStageQuestionnaire.GetQuestionnaires(IDFeaturesPX);
        }

        public StageQuestionnaire GetStageQuestionnaireByStageAndQuestionnaire(int idStageQuestionnaire)
        {
            return _rStageQuestionnaire.GetStageQuestionnaireByID(idStageQuestionnaire);
        }

        public List<string> IsStageQuestionnaireValid(StageQuestionnaire sq)
        {
            StageQuestionnaire entity = sq;
            List<string> response = StageQuestionnaireHasUniqueOrderInVisit(entity);
            response = StageQuestionnaireFeaturesAreOk(sq, response);

            return response;
        }

        private List<string> StageQuestionnaireFeaturesAreOk(StageQuestionnaire sq, List<string> response)
        {
            List<int> rangeIds = sq.QuestionnaireFeaturePXes.Select(s => s.IDFeaturePXRange).ToList();
            List<int> repeatedRanges = new List<int>();

            foreach (int r in rangeIds)
            {
                if ((rangeIds.Where(l => r == l).Count() > 1) && (!repeatedRanges.Contains(r)))
                    repeatedRanges.Add(r);
            }
            List<FeaturePXRange> rangesForDesc = null;
            if (repeatedRanges.Count > 0)
                rangesForDesc = _rFeaturePXRange.GetByIds(repeatedRanges);
            foreach (int r in repeatedRanges)
            {
                string desc = rangesForDesc.Where(rang => rang.IDFeaturePXRange == r).FirstOrDefault().Name;
                response.Add("Se ha agregado al rango " + desc + " más de una vez.");
            }

            return response;
        }

        private List<string> StageQuestionnaireHasUniqueOrderInVisit(StageQuestionnaire entity)
        {
            List<string> response = new List<string>();
            List<int> visitsInCheckUp = GetVisitsListFromCheckupString(entity.CheckupNumber).OrderBy(v => v).ToList();

            // Verifica si en el/los rangos especificados se insertó la misma visita más de una vez
            List<int> duplicates = visitsInCheckUp.GroupBy(x => x).Where(g => g.Count() > 1).Select(s => s.Key).ToList();

            if (duplicates.Count > 0)
            {
                string msg = "Se ha insertado una o algunas visitas repetidas (";
                duplicates.ForEach(d => msg += " #" + d);
                msg += ")";
                response.Add(msg);
            }

            // Verifica el orden para el resto de las visitas
            List<StageQuestionnaire> result = _rStageQuestionnaire.GetStagesByOrder(entity);
            List<int> visitsInStageQuestionnaire;
            foreach (StageQuestionnaire sq in result.OrderBy(r => r.CheckupNumber).ThenBy(r => r.Order))
            {
                visitsInStageQuestionnaire = GetVisitsListFromCheckupString(sq.CheckupNumber).OrderBy(v => v).ToList();
                foreach (int v in visitsInStageQuestionnaire)
                {
                    if (visitsInCheckUp.Any(vic => vic == v))
                        response.Add("En la Visita #" + v + " ya existe un Cuestionario para el Orden #" + sq.Order);
                }
            }


            return response;
        }

        private List<int> GetVisitsListFromCheckupString(string checkUpNumber)
        {
            List<int> response = new List<int>();

            string[] visits = checkUpNumber.Split(new Char[] { ',' });

            foreach (string item in visits)
            {
                string[] range = item.Split(new Char[] { '-' });

                if (range.Length == 2)
                {
                    int fromVisit = Convert.ToInt32(range[0]);
                    int toVisit = Convert.ToInt32(range[1]);

                    if (fromVisit > toVisit)
                    {
                        int aux = fromVisit;
                        fromVisit = toVisit;
                        toVisit = aux;
                    }

                    for (int i = fromVisit; i <= toVisit; i++)
                        response.Add(i);
                }
                else
                {
                    response.Add(Convert.ToInt32(range[0]));
                }
            }
            return response;
        }

        public void InsertStageQuestionnaire(StageQuestionnaire sq)
        {
            StageQuestionnaire entity = sq;
            _rStageQuestionnaire.Insert(entity);
        }

        //public void UpdateStageQuestionnaire(StageQuestionnaire sq)
        //{
        //    StageQuestionnaire entity = sq;
        //    UpdateFeaturesPXQuestionnaire(entity, sq.FeaturesPXQuestionnaireModel);
        //    _rStageQuestionnaire.Update(entity);
        //}

        //private void UpdateFeaturesPXQuestionnaire(StageQuestionnaire entity, List<DTOFeaturesPXQuestionnaire> dtos)
        //{
        //    List<int> idsDto = dtos.Select(d => d.IDFeaturesPXQuestionnaire).ToList();
        //    List<int> idsEntityToDelete = entity.FeaturesPXQuestionnaire.Where(f => !idsDto.Contains(f.IDFeaturesPXQuestionnaire)).Select(fi => fi.IDFeaturesPXQuestionnaire).ToList();
        //    foreach (int item in idsEntityToDelete)
        //    {
        //        _rFeaturesPXQuestionnaire.Delete(item);
        //    }

        //    foreach (DTOFeaturesPXQuestionnaire item in dtos.Where(f => f.IDFeaturesPXQuestionnaire > 0))
        //    {
        //        FeaturesPXQuestionnaire e = entity.FeaturesPXQuestionnaire.Where(f => f.IDFeaturesPXQuestionnaire == item.IDFeaturesPXQuestionnaire).FirstOrDefault();
        //        e.IDRangeFeaturePX = item.IDRangeFeaturePX;
        //        _rFeaturesPXQuestionnaire.Update(e);
        //    }

        //    foreach (DTOFeaturesPXQuestionnaire item in dtos.Where(f => f.IDFeaturesPXQuestionnaire < 0))
        //    {
        //        FeaturesPXQuestionnaire e = new FeaturesPXQuestionnaire();
        //        e.IDRangeFeaturePX = item.IDRangeFeaturePX;
        //        e.IDStageQuestionnaire = entity.IDStageQuestionnaire;
        //        _rFeaturesPXQuestionnaire.Insert(e);
        //    }
        //}

        //public void DeleteStageQuestionnaire(int idStageQuestionnaire)
        //{
        //    StageQuestionnaire e = _rStageQuestionnaire.GetStageQuestionnaireByID(idStageQuestionnaire);
        //    e.FeaturesPXQuestionnaire.ToList().ForEach(f => _rFeaturesPXQuestionnaire.Delete(f.IDFeaturesPXQuestionnaire));
        //    _rStageQuestionnaire.Delete(idStageQuestionnaire);
        //    _rStageQuestionnaire.UnitOfWork.SaveChanges();
        //}
    }
}
