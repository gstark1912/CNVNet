/*
 * HFX.Security v1.0.2 (http://www.horizon.com.ar)
 * ========================================================================
 * Copyright 2014 Horizon.
 * ========================================================================
 * Servicio para la logica de negocio de la entidad PATHOLOGYSTATE
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
    public class ServicePathologyState
    {
        private readonly RepositoryPathologyState _repositoryPathologyState;

        public ServicePathologyState()
        {
            _repositoryPathologyState = new RepositoryPathologyState();
        }


        public void Update(PathologyState dto)
        {
            _repositoryPathologyState.Update(dto);
        }

        public PathologyState GetById(int IDPathologyState)
        {
            return _repositoryPathologyState.GetById(IDPathologyState);
        }

        public List<PathologyState> GetByPathology(int IDPathology)
        {
            return _repositoryPathologyState.GetByPathology(IDPathology);
        }

        public bool OnlyOneNotApplyAlternative(int IDPathology, int IDPathologyState)
        {
            return _repositoryPathologyState.OnlyOneNotApplyAlternative(IDPathology, IDPathologyState);
        }

        public PathologyStateCondition GetLastCondition(int IDPathologyState)
        {
            return _repositoryPathologyState.GetLastCondition(IDPathologyState);
        }
    }
}
