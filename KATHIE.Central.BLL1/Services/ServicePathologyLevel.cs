using KATHIE.Central.BLL1;
/*
 * KATHIE Central v1.0.0 (http://www.horizon.com.ar)
 * ========================================================================
 * Copyright 2014 Horizon.
 * ========================================================================
 * Servicio para la logica de negocio de la entidad PATHOLOGYLEVEL.
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
    public class ServicePathologyLevel
    {
        private readonly RepositoryPathologyLevel _repositoryPathologyLevel;

        public ServicePathologyLevel()
        {
            _repositoryPathologyLevel = new RepositoryPathologyLevel();
        }

        public List<PathologyLevel> GetAll()
        {
            return _repositoryPathologyLevel.GetAll();
        }
    }
}
