using KATHIE.Central.BLL1;
/*
 * KATHIE Central v1.0.0 (http://www.horizon.com.ar)
 * ========================================================================
 * Copyright 2014 Horizon.
 * ========================================================================
 * Servicio para la logica de negocio de la entidad MEDICALPROPERTY
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
    public class ServiceMedicalProperties
    {
        private readonly RepositoryMedicalProperty _repositoryMedicalProperty;
        private readonly RepositoryMedicalPropertyValue _repositoryMedicalPropertyValue;
        private readonly RepositoryMedicalPropertyValueType _repositoryMedicalPropertyValueType;
        private readonly MedicalPropertyService _medicalPropertyService;
        private readonly RepositoryCondition _repositoryCondition;
        private readonly RepositoryLogicOperator _repositoryLogicOperator;
        private readonly RepositoryMedicalPropertyValidation _repositoryMedicalPropertyValidation;

        public ServiceMedicalProperties()
        {
            _repositoryMedicalProperty = new RepositoryMedicalProperty();
            _repositoryMedicalPropertyValueType = new RepositoryMedicalPropertyValueType();
            _medicalPropertyService = new MedicalPropertyService();
            _repositoryMedicalPropertyValue = new RepositoryMedicalPropertyValue();
            _repositoryCondition = new RepositoryCondition();
            _repositoryLogicOperator = new RepositoryLogicOperator();
            _repositoryMedicalPropertyValidation = new RepositoryMedicalPropertyValidation();
        }

        public PagedDataResult<DTOMedicalPropertyForGrid> GetMedicalPropertiesByDescription(string desc, PagedDataParameters parameters)
        {
            PagedDataResult<MedicalProperty> result = _repositoryMedicalProperty.GetByDescription(desc, parameters);
            return new PagedDataResult<DTOMedicalPropertyForGrid>(AdapterMedicalPropertyForGrid.ToDTOsMedicalPropertyForGrid(result.Results), result.TotalCount);
        }

        public MedicalProperty GetMedicalPropertyById(int id)
        {
            return _repositoryMedicalProperty.GetById(id);
        }

        public List<MedicalPropertyValueType> GetAllValueTypes()
        {
            return _repositoryMedicalPropertyValueType.GetAll();
        }

        public List<string> IsMedicalPropertyOk(MedicalProperty dto)
        {
            return _medicalPropertyService.IsMedicalPropertyValidForUpdate(dto, _repositoryMedicalPropertyValueType.GetAll());
        }

        public void Edit(MedicalProperty dto)
        {
            List<MedicalPropertyValue> newValues = dto.MedicalPropertyValues.ToList();

            InsertValuesInMedicalProperty(dto, newValues);

            _repositoryMedicalProperty.Update(dto);
        }
        private void InsertValuesInMedicalProperty(MedicalProperty entity, List<MedicalPropertyValue> newValues)
        {
            MedicalPropertyValue valueInEntity;

            foreach (MedicalPropertyValue item in newValues)
            {
                if (!_medicalPropertyService.IsValueEmpty(item))
                {
                    valueInEntity = entity.MedicalPropertyValues.Where(m => m.IDMedicalPropertyValueType == item.IDMedicalPropertyValueType).FirstOrDefault();

                    if (valueInEntity != null)
                    {
                        valueInEntity.StandardMaxValue = item.StandardMaxValue;
                        valueInEntity.StandardMinValue = item.StandardMinValue;
                        valueInEntity.StandardValue = item.StandardValue;

                        _repositoryMedicalPropertyValue.Update(valueInEntity);
                    }
                    else
                    {
                        item.IDMedicalProperty = entity.IDMedicalProperty;
                        _repositoryMedicalPropertyValue.Insert(item);
                    }
                }
                else
                {
                    valueInEntity = entity.MedicalPropertyValues.Where(m => m.IDMedicalPropertyValueType == item.IDMedicalPropertyValueType).FirstOrDefault();
                    if (valueInEntity != null)
                    {
                        _repositoryMedicalPropertyValue.Delete(valueInEntity.IDMedicalPropertyValue);
                    }
                }
            }
        }

        public List<MedicalProperty> GetAllForCombo()
        {
            return _repositoryMedicalProperty.GetAll();
        }

        public List<Condition> GetAllConditions()
        {
            return _repositoryCondition.GetAll();
        }

        public List<LogicOperator> GetAllLogicOperators()
        {
            return _repositoryLogicOperator.GetAll();
        }

        public List<MedicalPropertyValidation> GetMedicalPropertyValidations(int id)
        {
            return _repositoryMedicalPropertyValidation.GetByMedicalProperty(id);
        }

        public List<MedicalProperty> GetAllMedicalProperties()
        {
            return _repositoryMedicalProperty.GetAll();
        }

        public List<MedicalPropertyValidation> GetMedicalPropertyValidationsById(int medicalPropertyId)
        {
            return _repositoryMedicalPropertyValidation.GetByMedicalProperty(medicalPropertyId);
        }

        public List<string> AreValidationsOk(List<MedicalPropertyValidation> validations, int medicalPropertyId)
        {
            List<string> response = new List<string>();
            return response;
        }

        public void UpdateValidations(List<MedicalPropertyValidation> validations, int medicalPropertyId)
        {
            List<MedicalPropertyValidation> newEntities = AdapterMedicalPropertyValidation.ToEntities(validations, medicalPropertyId);
            List<MedicalPropertyValidation> oldEntities = _repositoryMedicalPropertyValidation.GetByMedicalProperty(medicalPropertyId);

            List<int> newEntitiesId = newEntities.Select(n => n.IDMedicalPropertyValidation).ToList();
            List<int> oldEntitiesId = oldEntities.Select(n => n.IDMedicalPropertyValidation).ToList();

            AddValidations(newEntities.Where(n => !oldEntitiesId.Contains(n.IDMedicalPropertyValidation)));
            UpdateValidations(newEntities.Where(n => oldEntitiesId.Contains(n.IDMedicalPropertyValidation)), oldEntities);
            DeleteValidations(oldEntities.Where(n => !newEntitiesId.Contains(n.IDMedicalPropertyValidation)));

        }

        private void DeleteValidations(IEnumerable<MedicalPropertyValidation> oldEntities)
        {
            foreach (MedicalPropertyValidation item in oldEntities)
            {
                _repositoryMedicalPropertyValidation.Delete(item.IDMedicalPropertyValidation);
            }
        }

        private void UpdateValidations(IEnumerable<MedicalPropertyValidation> newEntities, List<MedicalPropertyValidation> oldEntities)
        {
            foreach (MedicalPropertyValidation item in newEntities)
            {
                MedicalPropertyValidation realEntity = oldEntities.Where(o => o.IDMedicalPropertyValidation == item.IDMedicalPropertyValidation).FirstOrDefault();

                realEntity.IDLogicOperator = item.IDLogicOperator;
                realEntity.IDCondition = item.IDCondition;
                realEntity.IDMedicalProperty = item.IDMedicalProperty;

                _repositoryMedicalPropertyValidation.Update(realEntity);
            }
        }

        private void AddValidations(IEnumerable<MedicalPropertyValidation> newEntities)
        {
            foreach (MedicalPropertyValidation item in newEntities)
            {
                _repositoryMedicalPropertyValidation.Insert(item);
            }
        }
    }
}

public static partial class AdapterMedicalPropertyForGrid
{
    public static DTOMedicalPropertyForGrid ToDTOMedicalPropertyForGrid(this MedicalProperty entity)
    {
        if (entity == null) return null;

        var dto = new DTOMedicalPropertyForGrid();

        dto.IDMedicalPropertyConfiguration = entity.IDMedicalProperty;
        dto.Description = entity.Description;
        dto.AllowNotApply = entity.AllowNotApply == true ? "Sí" : "No";
        dto.IsManual = entity.Manual == true ? "Sí" : "No";
        dto.IsVisible = entity.Visible == true ? "Sí" : "No";
        dto.MeasurementUnit = entity.MeasurementUnit.Description;
        dto.MinValue = entity.MinValue;
        dto.MaxValue = entity.MaxValue;
        if (dto.AllowNotApply != "No")
            dto.ValueNotApply = entity.ValueNotApply;
        //else
        //    dto.ValueNotApply = 0;
        return dto;
    }

    public static List<DTOMedicalPropertyForGrid> ToDTOsMedicalPropertyForGrid(this List<MedicalProperty> entities)
    {
        if (entities == null) return null;

        return entities.Select(e => e.ToDTOMedicalPropertyForGrid()).ToList();
    }

}