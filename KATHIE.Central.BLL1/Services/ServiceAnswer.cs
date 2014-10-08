/*
 * HFX.Security v1.0.2 (http://www.horizon.com.ar)
 * ========================================================================
 * Copyright 2014 Horizon.
 * ========================================================================
 * Servicio para la logica de negocio de la entidad ANSWER
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
    public class ServiceAnswer
    {
        private readonly RepositoryAnswer _repositoryAnswer;

        public ServiceAnswer()
        {
            _repositoryAnswer = new RepositoryAnswer();
        }

        public void SaveAnswer(Answer dto)
        {
            if (dto.IDAnswer == 0)
            {
                _repositoryAnswer.Insert(dto);
            }
            else
            {
                _repositoryAnswer.Update(dto);
            }
        }

        public Answer GetAnswerByID(int idAnswer)
        {
            Answer dto = _repositoryAnswer.GetById(idAnswer);
            return dto;
        }

        public void Delete(int idAnswer)
        {
            _repositoryAnswer.Delete(idAnswer);
        }

        public List<Answer> GetByQuestion(int IDQuestion)
        {
            List<Answer> answers = _repositoryAnswer.GetByQuestion(IDQuestion);
            return answers;
        }

        public bool ExistsOrder(int IDQuestion, int IDAnswer, int order)
        {
            bool existsOrder = false;
            List<int> answers = _repositoryAnswer.GetSameOrder(IDQuestion, IDAnswer, order);
            if (answers.Count > 0)
                existsOrder = true;
            return existsOrder;
        }
    }
}
