/*
 * KATHIE WEB  (http://www.horizon.com.ar)
 * ========================================================================
 * Copyright 2014 Horizon.
 * ========================================================================
 * Servicio encargado del manejo de la pantalla de Grupo Terapeutico.
*/
using KATHIE.Central.DAL.Infraestructure;
using KATHIE.Central.DAL.Repositories;
using KATHIE.Central.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KATHIE.Central.BLL1.Services
{
    public class TherapeuticGroupService
    {
        private readonly MedicineRepository _repositoryMedicine;
        private readonly UnitOfWork unitOfWork;

        public TherapeuticGroupService()
        {
            _repositoryMedicine = new MedicineRepository();
            unitOfWork = new UnitOfWork();
        }

        public TherapeuticGroup GetByID(int IDTherapeuticGroup)
        {
            return (unitOfWork.therapeuticGroupRepository.GetById(IDTherapeuticGroup));
        }

        public PagedDataResult<TherapeuticGroup> GetForGrid(string filter, PagedDataParameters parameters)
        {
            PagedDataResult<TherapeuticGroup> result = unitOfWork.therapeuticGroupRepository.GetForGrid(filter, parameters);
            return new PagedDataResult<TherapeuticGroup>((result.Results), result.TotalCount);
        }

        public List<TherapeuticGroup> GetForAutoComplete(string term)
        {
            return (unitOfWork.therapeuticGroupRepository.GetForAutoComplete(term));
        }

        public void UpdateDefaultAndChange(TherapeuticGroup dto)
        {
            TherapeuticGroup newDefault = (dto);
            newDefault.Default = true;
            unitOfWork.therapeuticGroupRepository.Update(newDefault);

            ChangeDefault();

            unitOfWork.Save();

        }

        public void InsertDefaultAndChange(TherapeuticGroup dto)
        {
            TherapeuticGroup newDefault = (dto);
            newDefault.Default = true;
            unitOfWork.therapeuticGroupRepository.Insert(newDefault);

            ChangeDefault();
            unitOfWork.Save();
        }

        public void Insert(TherapeuticGroup dto)
        {
            TherapeuticGroup entity = (dto);
            unitOfWork.therapeuticGroupRepository.Insert(entity);
            unitOfWork.Save();
        }

        public void Update(TherapeuticGroup dto)
        {
            TherapeuticGroup entity = unitOfWork.therapeuticGroupRepository.GetById(dto.IDTherapeuticGroup);
            entity.Description = dto.Description;
            entity.Default = dto.Default;
            unitOfWork.therapeuticGroupRepository.Update(entity);
            unitOfWork.Save();
        }

        /// <summary>
        /// Verifica si el laboratoria esta en uso por algún medicamento
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool CanBeDeleted(int id)
        {
            List<Medicine> medicines = GetReferenceMedicines(id);
            if (medicines.Count > 0) // tiene registros asociados
            {
                TherapeuticGroup defaultTherapeuticGroup = unitOfWork.therapeuticGroupRepository.GetDefault(id);
                if (defaultTherapeuticGroup == null)
                {
                    return false; // No hay default, no se puede borrar
                }
                else
                {
                    return true;// hay default, se puede borrar
                }
            }
            return true; // no tiene registros asociados, se puede borrar
        }

        /// <summary>
        /// Elimina un registro
        /// </summary>
        /// <param name="id"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public void Delete(int id)
        {
            List<Medicine> medicines = GetReferenceMedicines(id);
            if (medicines.Count > 0)
            {
                TherapeuticGroup defaultTherapeuticGroup = unitOfWork.therapeuticGroupRepository.GetDefault(id);
                foreach (Medicine m in medicines)
                {
                    m.TherapeuticGroup = defaultTherapeuticGroup;
                    _repositoryMedicine.Update(m);
                }
                unitOfWork.therapeuticGroupRepository.Delete(id);
                unitOfWork.Save();
            }
            else
            {
                unitOfWork.therapeuticGroupRepository.Delete(id);
                unitOfWork.Save();
            }
        }

        /// <summary>
        /// Verifica si existe un laboratorio
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public bool TherapeuticGroupExists(TherapeuticGroup dto)
        {
            return unitOfWork.therapeuticGroupRepository.Exists((dto));
        }
        /// <summary>
        /// Obtiene todos los registros
        /// </summary>
        /// <returns></returns>
        public List<TherapeuticGroup> GetAll()
        {
            return (unitOfWork.therapeuticGroupRepository.GetAll().ToList());
        }
        /// <summary>
        /// Verifica si el ID es un registro default
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool IsDefault(int id)
        {
            return unitOfWork.therapeuticGroupRepository.GetById(id).Default;
        }
        /// <summary>
        /// Verifica si existe un refistro default
        /// </summary>
        /// <returns></returns>
        public bool ExistsDefault()
        {
            return unitOfWork.therapeuticGroupRepository.ExistsDefault();
        }
        /// <summary>
        /// Verifica si existe un refistro default
        /// </summary>
        /// <param name="idToExclude"></param>
        /// <returns></returns>
        public bool ExistsDefault(int? idToExclude)
        {
            return unitOfWork.therapeuticGroupRepository.GetDefault(idToExclude) != null;
        }
        /// <summary>
        /// Verifica si el registro tiene dependencias
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool HasDependencies(int id)
        {
            return unitOfWork.therapeuticGroupRepository.HasDependencies(id);
        }
        /// <summary>
        /// Obtiene las medicinas relacionadas
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private List<Medicine> GetReferenceMedicines(int id)
        {
            return unitOfWork.therapeuticGroupRepository.GetReferenceMedicines(id);
        }
        /// <summary>
        /// Cambia el registro por default
        /// </summary>
        private void ChangeDefault()
        {
            TherapeuticGroup defaultTherapeuticGroup = unitOfWork.therapeuticGroupRepository.GetDefault();

            if (defaultTherapeuticGroup != null)
            {
                defaultTherapeuticGroup.Default = false;
                unitOfWork.therapeuticGroupRepository.Update(defaultTherapeuticGroup);
            }
        }

        /// <summary>
        /// Verifica si un Laboratorio puede ser guardado, basándose en que no haya otro 
        /// Laboratorio por defecto en caso de que el Laboratorio a guardar deba ser así
        /// </summary>
        public bool CanBeSaved(TherapeuticGroup dto)
        {
            if (dto.Default && ExistsDefault(dto.IDTherapeuticGroup))
                return false;
            else
                return true;
        }

    }
}