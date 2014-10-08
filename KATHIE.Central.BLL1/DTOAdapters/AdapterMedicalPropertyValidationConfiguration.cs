using KATHIE.Central.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KATHIE.Central.BLL1
{
    public static partial class AdapterMedicalPropertyValidation
    {
        public static MedicalPropertyValidation ToMedicalPropertyValidation(this MedicalPropertyValidation entity)
        {
            if (entity == null) return null;

            var dto = new MedicalPropertyValidation();

            dto.IDMedicalPropertyValidation = entity.IDMedicalPropertyValidation;
            dto.IDCondition = entity.IDCondition;
            dto.IDLogicOperator = entity.IDLogicOperator == null ? 0 : Convert.ToInt32(entity.IDLogicOperator);
            dto.IDMedicalPropertyConfigurationOwner = entity.IDMedicalPropertyConfigurationOwner;
            dto.IDMedicalProperty = entity.IDMedicalProperty;

            return dto;
        }

        public static List<MedicalPropertyValidation> ToDTOsCondition(this List<MedicalPropertyValidation> entities)
        {
            if (entities == null) return null;

            return entities.Select(e => e.ToMedicalPropertyValidation()).ToList();
        }

        public static MedicalPropertyValidation ToEntity(this MedicalPropertyValidation dto, int medicalPropertyId)
        {
            var entity = new MedicalPropertyValidation();

            entity.IDMedicalPropertyValidation = dto.IDMedicalPropertyValidation;
            entity.IDCondition = dto.IDCondition;
            if (dto.IDLogicOperator == 0)
                entity.IDLogicOperator = null;
            else
                entity.IDLogicOperator = dto.IDLogicOperator;
            entity.IDMedicalProperty = dto.IDMedicalProperty;
            entity.IDMedicalPropertyConfigurationOwner = medicalPropertyId;

            return entity;
        }

        public static List<MedicalPropertyValidation> ToEntities(this List<MedicalPropertyValidation> entities, int medicalPropertyId)
        {
            if (entities == null) return null;

            return entities.Select(e => e.ToEntity(medicalPropertyId)).ToList();
        }
    }
}
