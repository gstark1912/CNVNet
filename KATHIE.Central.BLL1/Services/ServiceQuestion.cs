/*
 * HFX.Security v1.0.2 (http://www.horizon.com.ar)
 * ========================================================================
 * Copyright 2014 Horizon.
 * ========================================================================
 * Servicio para la logica de negocio de la entidad QUESTION
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
    public class ServiceQuestion
    {
        private readonly RepositoryQuestion _repositoryQuestion;
        private readonly RepositoryAnswer _repositoryAnswer;

        public ServiceQuestion()
        {
            _repositoryQuestion = new RepositoryQuestion();
            _repositoryAnswer = new RepositoryAnswer();
        }

        public Question GetWithoutAnswers(int IDQuestion)
        {
            Question dto = _repositoryQuestion.GetWithoutAnswers(IDQuestion);
            return dto;
        }

        public Question GetWithAnswers(int IDQuestion)
        {
            Question dto = _repositoryQuestion.GetWithAnswers(IDQuestion);
            return dto;
        }

        public bool ExistsOrder(int IDQuestionnaire, int IDQuestion, int order)
        {
            bool existsOrder = false;
            List<int> questions = _repositoryQuestion.GetSameOrder(IDQuestionnaire, IDQuestion, order);
            if (questions.Count > 0)
                existsOrder = true;
            return existsOrder;
        }

        public List<Question> GetTopOrderQuestions(int IDQuestionnaire, int order)
        {
            List<Question> list = _repositoryQuestion.GetTopOrderQuestions(IDQuestionnaire, order);
            return list;
        }

        public void Delete(int idQuestion)
        {
            List<int> questions = new List<int>();
            questions.Add(idQuestion);
            List<int> answers = _repositoryAnswer.GetIdsByQuestions(questions);
            List<Answer> refAnswers = _repositoryQuestion.GetReferenceAnswer(idQuestion);
            foreach (Answer ra in refAnswers)
            {
                ra.IDNextQuestion = null;
                ra.AnotherQuestion = false;
                _repositoryAnswer.Update(ra);
            }
            answers.ForEach(a => _repositoryAnswer.Delete(a));
            _repositoryQuestion.Delete(idQuestion);
        }

        public void SaveQuestion(Question dto)
        {
            if (dto.IDQuestion == 0)
            {
                _repositoryQuestion.Insert(dto);
            }
            else
            {
                _repositoryQuestion.Update(dto);
            }
        }

        public List<Question> GetByQuestionnaire(int IDQuestionnaire)
        {
            List<Question> list = _repositoryQuestion.GetByQuestionnaire(IDQuestionnaire);
            return list;
        }

        public List<Answer> GetReferenceAnswer(int IDQuestion)
        {
            List<Answer> list = _repositoryQuestion.GetReferenceAnswer(IDQuestion);
            return list;
        }
    }
}
