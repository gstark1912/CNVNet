using KATHIE.Central.BLL1;
/*
 * KATHIE Central v1.0.0 (http://www.horizon.com.ar)
 * ========================================================================
 * Copyright 2014 Horizon.
 * ========================================================================
 * Servicio para la logica de negocio de la entidad PATHOLOGYSTATECONDITION
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
    public class ServicePathologyStateCondition
    {
        private readonly RepositoryPathologyState _repositoryPathologyState;
        private readonly RepositoryPathologyStateCondition _repositoryPathologyStateCondition;

        public ServicePathologyStateCondition()
        {
            _repositoryPathologyStateCondition = new RepositoryPathologyStateCondition();
            _repositoryPathologyState = new RepositoryPathologyState();
        }



        public void SaveCondition(DTOPathologyStateCondition dto)
        {
            if (dto.IDPathologyStateCondition == 0)
            {
                PathologyStateCondition lastCondition = _repositoryPathologyState.GetLastCondition(dto.IDPathologyState);
                if (lastCondition != null)
                {
                    if (lastCondition.IDLogicOperator == dto.IDDefaultLogicOperatorEmpty)
                    {
                        lastCondition.IDLogicOperator = dto.IDDefaultLogicOperatorY;
                        _repositoryPathologyStateCondition.Update(lastCondition);
                    }
                }
                _repositoryPathologyStateCondition.Insert(AdapterPathologyStateCondition.ToEntity(dto));
            }
            else
            {
                _repositoryPathologyStateCondition.Update(AdapterPathologyStateCondition.ToEntity(dto));
            }

        }

        public void Delete(DTOPathologyStateCondition dto)
        {
            _repositoryPathologyStateCondition.Delete(dto.IDPathologyStateCondition);
            PathologyStateCondition lastCondition = _repositoryPathologyState.GetLastCondition(dto.IDPathologyState);
            if (lastCondition != null)
            {
                if (lastCondition.IDLogicOperator != dto.IDDefaultLogicOperatorEmpty)
                {
                    lastCondition.IDLogicOperator = dto.IDDefaultLogicOperatorEmpty;
                    _repositoryPathologyStateCondition.Update(lastCondition);
                }
            }
        }

        public DTOPathologyStateCondition GetById(int IDPathologyStateCondition)
        {
            return AdapterPathologyStateCondition.ToDTO(_repositoryPathologyStateCondition.GetById(IDPathologyStateCondition));
        }

        public List<DTOPathologyStateCondition> GetByPathologyState(int IDPathologyState)
        {
            return AdapterPathologyStateCondition.ToDTOs(_repositoryPathologyStateCondition.GetByPathologyState(IDPathologyState));
        }
    }
}

internal static class AdapterPathologyStateCondition
{
    internal static PathologyStateCondition ToEntity(DTOPathologyStateCondition dto)
    {
        if (dto == null) return null;
        PathologyStateCondition entity = new PathologyStateCondition();
        entity.IDPathologyStateCondition = dto.IDPathologyStateCondition;
        entity.IDPathologyState = dto.IDPathologyState;
        entity.IDMedicalProperty = dto.IDMedicalProperty;
        entity.IDCondition = dto.IDCondition;
        entity.Value = dto.Value;
        entity.IDLogicOperator = dto.IDLogicOperator;
        entity.IncludeNotApply = dto.IncludeNotApply;
        return entity;
    }

    internal static DTOPathologyStateCondition ToDTO(this PathologyStateCondition entity)
    {
        if (entity == null) return null;
        DTOPathologyStateCondition dto = new DTOPathologyStateCondition();
        dto.IDPathologyStateCondition = entity.IDPathologyStateCondition;
        dto.IDPathologyState = entity.IDPathologyState;
        dto.IDMedicalProperty = entity.IDMedicalProperty;
        dto.IDCondition = entity.IDCondition;
        dto.Value = entity.Value;
        dto.IDLogicOperator = entity.IDLogicOperator;
        dto.IncludeNotApply = entity.IncludeNotApply;
        dto.Condition = entity.Condition;
        dto.LogicOperator = entity.LogicOperator;
        dto.MedicalProperty = entity.MedicalProperty;
        return dto;
    }

    internal static List<DTOPathologyStateCondition> ToDTOs(this IEnumerable<PathologyStateCondition> entities)
    {
        if (entities == null) return null;
        return entities.Select(e => e.ToDTO()).ToList();
    }
}