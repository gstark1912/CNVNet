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
    public class ServicePathology
    {
        private readonly RepositoryPathology _repositoryPathology;

        public ServicePathology()
        {
            _repositoryPathology = new RepositoryPathology();
        }

        public PagedDataResult<Pathology> GetAll(PagedDataParameters parameters)
        {
            PagedDataResult<Pathology> result = _repositoryPathology.GetAll(parameters);
            return new PagedDataResult<Pathology>(result.Results, result.TotalCount);
        }
    }
}
