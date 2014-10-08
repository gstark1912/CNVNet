/*
 * HFX.Security v1.0.2 (http://www.horizon.com.ar)
 * ========================================================================
 * Copyright 2014 Horizon.
 * ========================================================================
 * Servicio para la logica de negocio de la entidad QUESTIONNAIREFEATURESPX
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
    public class ServiceQuestionnaireFeaturesPX
    {
        private readonly RepositoryQuestionnaireFeaturesPX _repositoryQuestionnaireFeaturesPX;

        public ServiceQuestionnaireFeaturesPX()
        {
            _repositoryQuestionnaireFeaturesPX = new RepositoryQuestionnaireFeaturesPX();
        }

        public List<int> GetIdsFeatures(List<int> idsStages)
        {
            return _repositoryQuestionnaireFeaturesPX.GetIdsFeatures(idsStages);
        }
    }
}
