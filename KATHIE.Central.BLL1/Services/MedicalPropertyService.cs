/*
 * HFX.Security v1.0.2 (http://www.horizon.com.ar)
 * ========================================================================
 * Copyright 2014 Horizon.
 * ========================================================================
 * Servicio para la logica de negocio de la entidad MEdicalProperty
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
    public class MedicalPropertyService
    {
        #region Edit Validations
        public List<string> IsMedicalPropertyValidForUpdate(MedicalProperty newEntity, List<MedicalPropertyValueType> valueTypes)
        {
            List<string> response = new List<string>();

            IsValueNotApplyOk(newEntity, response);
            AreValuesOk(newEntity, response, valueTypes);
            IsRelationshipMaleFemaleStandardOk(newEntity, response);

            return response;
        }
        private void IsRelationshipMaleFemaleStandardOk(MedicalProperty newEntity, List<string> response)
        {
            MedicalPropertyValue standardValues = newEntity.MedicalPropertyValues.Where(m => m.IDMedicalPropertyValueType == 1).FirstOrDefault();
            MedicalPropertyValue femaleValues = newEntity.MedicalPropertyValues.Where(m => m.IDMedicalPropertyValueType == 2).FirstOrDefault();
            MedicalPropertyValue maleValues = newEntity.MedicalPropertyValues.Where(m => m.IDMedicalPropertyValueType == 3).FirstOrDefault();

            if (IsValueEmpty(standardValues))
            {
                if (IsValueEmpty(maleValues) || IsValueEmpty(femaleValues))
                    response.Add("Deben declararse los Valores para Hombre y Mujer en caso de no declarar valores Estándar");
            }
            else
                if (!IsValueEmpty(maleValues) || !IsValueEmpty(femaleValues))
                    response.Add("Si se declaran los valores Estándar, deben quedar vacíos los valores para Hombre y Mujer");
        }
        private void AreValuesOk(MedicalProperty newEntity, List<string> response, List<MedicalPropertyValueType> valueTypes)
        {
            string valType;

            foreach (MedicalPropertyValue item in newEntity.MedicalPropertyValues)
            {
                valType = valueTypes.Where(v => v.IDMedicalPropertyValueType == item.IDMedicalPropertyValueType).FirstOrDefault().Description;

                if (ValueNotCompleted(item))
                    response.Add("Los Valores de " + valType + " no están completos");
                else
                {
                    if (item.StandardMaxValue > newEntity.MaxValue)
                        response.Add("El Valor Máximo " + valType + " no puede ser mayor al Valor Máximo de la Propiedad");

                    if (item.StandardMinValue > newEntity.MaxValue)
                        response.Add("El Valor Mínimo " + valType + " no puede ser mayor al Valor Mínimo de la Propiedad");

                    if ((item.StandardValue < item.StandardMinValue) || (item.StandardValue > item.StandardMaxValue))
                        response.Add("El Valor Estándar " + valType + " debe estar entre el Máximo y el Mínimo de " + valType);
                }
            }
        }
        public bool IsValueEmpty(MedicalPropertyValue item)
        {
            bool response;

            if ((item.StandardValue == null) && (item.StandardMaxValue == null) && (item.StandardMinValue == null))
                response = true;
            else
                response = false;

            return response;
        }
        private bool ValueNotCompleted(MedicalPropertyValue item)
        {
            bool response;

            if ((item.StandardValue == null) && (item.StandardMaxValue == null) && (item.StandardMinValue == null))
                response = false;
            else
                if ((item.StandardValue == null) || (item.StandardMaxValue == null) || (item.StandardMinValue == null))
                    response = true;
                else
                    response = false;

            return response;
        }
        private void IsValueNotApplyOk(MedicalProperty newEntity, List<string> response)
        {
            if (newEntity.AllowNotApply && newEntity.ValueNotApply == null)
                response.Add("Si Permite No aplica, debe ingresar un Valor para el mismo");
        }
        #endregion
    }
}
