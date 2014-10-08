/*
 * HFX.Security v1.0.2 (http://www.horizon.com.ar)
 * ========================================================================
 * Copyright 2014 Horizon.
 * ========================================================================
 * Servicio para la logica de negocio de la entidad QUESTIONNAIRE
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
    public class ServiceQuestionnaire
    {
        private readonly RepositoryQuestionnaire _repositoryQuestionnaire;
        private readonly RepositoryStageQuestionnaire _repositoryStageQuestionnaire;
        private readonly RepositoryQuestionnaireFeaturesPX _repositoryQuestionnaireFeaturesPX;
        private readonly RepositoryQuestion _repositoryQuestion;
        private readonly RepositoryAnswer _repositoryAnswer;

        public ServiceQuestionnaire()
        {
            _repositoryQuestionnaire = new RepositoryQuestionnaire();
            _repositoryQuestion = new RepositoryQuestion();
            _repositoryAnswer = new RepositoryAnswer();
            _repositoryStageQuestionnaire = new RepositoryStageQuestionnaire();
            _repositoryQuestionnaireFeaturesPX = new RepositoryQuestionnaireFeaturesPX();
        }



        public Questionnaire GetQuestionnaireByID(int id)
        {
            Questionnaire dto = _repositoryQuestionnaire.GetQuestionnaireById(id);
            return dto;
        }

        public void Delete(int idQuestionnaire)
        {
            List<int> stages = _repositoryStageQuestionnaire.GetIdsStages(idQuestionnaire);
            List<int> features = _repositoryQuestionnaireFeaturesPX.GetIdsFeatures(stages);
            List<int> questions = _repositoryQuestion.GetIdsByQuestionnaire(idQuestionnaire);
            List<int> answers = _repositoryAnswer.GetIdsByQuestions(questions);
            features.ForEach(f => _repositoryQuestionnaireFeaturesPX.Delete(f));
            stages.ForEach(s => _repositoryStageQuestionnaire.Delete(s));
            answers.ForEach(a => _repositoryAnswer.Delete(a));
            questions.ForEach(q => _repositoryQuestion.Delete(q));
            _repositoryQuestionnaire.Delete(idQuestionnaire);
        }

        public List<Questionnaire> GetAll()
        {
            List<Questionnaire> dtos = _repositoryQuestionnaire.GetAll();
            return dtos;
        }

        public PagedDataResult<Questionnaire> GetAll(PagedDataParameters parameters)
        {
            PagedDataResult<Questionnaire> result = _repositoryQuestionnaire.GetAll(parameters);
            return new PagedDataResult<Questionnaire>(result.Results, result.TotalCount);
        }

        public void Save(Questionnaire entity)
        {
            if (entity.IDQuestionnaire == 0)
            {
                _repositoryQuestionnaire.Save(entity);
            }
            else
            {
                _repositoryQuestionnaire.Update(entity);
            }
        }
    }
}
