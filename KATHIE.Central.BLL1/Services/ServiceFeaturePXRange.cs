/*
 * HFX.Security v1.0.2 (http://www.horizon.com.ar)
 * ========================================================================
 * Copyright 2014 Horizon.
 * ========================================================================
 * Servicio para la logica de negocio de la entidad FEATUREPXRANGE
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
    public class ServiceFeaturePXRange
    {
        private readonly RepositoryFeaturePXRange _repositoryFeaturePXRange;

        public ServiceFeaturePXRange()
        {
            _repositoryFeaturePXRange = new RepositoryFeaturePXRange();
        }


        public FeaturePXRange GetRangeFeaturePXByID(int idRangeFeaturePX)
        {
            FeaturePXRange dto = _repositoryFeaturePXRange.GetByID(idRangeFeaturePX);
            return dto;
        }

        public void Delete(int idRangeFeaturePX)
        {
            _repositoryFeaturePXRange.Delete(idRangeFeaturePX);
        }

        public void SaveRangeFeaturePX(FeaturePXRange dto)
        {
            if (dto.IDFeaturePXRange == 0)
            {
                _repositoryFeaturePXRange.Insert(dto);
            }
            else
            {
                _repositoryFeaturePXRange.Update(dto);
            }
        }

        public List<FeaturePXRange> GetAll()
        {
            return _repositoryFeaturePXRange.GetAll();
        }

        public List<FeaturePXRange> GetByIDFeaturePX(int IDFeaturePX)
        {
            List<FeaturePXRange> list = _repositoryFeaturePXRange.GetByFeaturePX(IDFeaturePX);
            return list;
        }
        public PagedDataResult<FeaturePXRange> GetForGrid(PagedDataParameters parameters, int IDFeaturePX)
        {
            PagedDataResult<FeaturePXRange> result = _repositoryFeaturePXRange.GetPagedByStage(parameters, IDFeaturePX);
            return new PagedDataResult<FeaturePXRange>(result.Results, result.TotalCount);
        }
    }
}
