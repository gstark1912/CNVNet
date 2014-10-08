/*
 * KATHIE WEB  (http://www.horizon.com.ar)
 * ========================================================================
 * Copyright 2014 Horizon.
 * ========================================================================
 * Servicio encargado del manejo de la pantalla de Acción Terapeutica.
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
    public class TherapeuticActionService
    {
        private readonly MedicineRepository _repositoryMedicine;
        private readonly UnitOfWork unitOfWork;

        public TherapeuticActionService()
        {
            _repositoryMedicine = new MedicineRepository();
            unitOfWork = new UnitOfWork();
        }

        public TherapeuticAction GetByID(int IDTherapeuticAction)
        {
            return (unitOfWork.therapeuticActionRepository.GetById(IDTherapeuticAction));
        }

        public PagedDataResult<TherapeuticAction> GetForGrid(string filter, PagedDataParameters parameters)
        {
            PagedDataResult<TherapeuticAction> result = unitOfWork.therapeuticActionRepository.GetForGrid(filter, parameters);
            return new PagedDataResult<TherapeuticAction>((result.Results), result.TotalCount);
        }

        public List<TherapeuticAction> GetForAutoComplete(string term)
        {
            return (unitOfWork.therapeuticActionRepository.GetForAutoComplete(term));
        }

        public void UpdateDefaultAndChange(TherapeuticAction dto)
        {
            TherapeuticAction newDefault = (dto);
            newDefault.Default = true;
            unitOfWork.therapeuticActionRepository.Update(newDefault);

            ChangeDefault();

            unitOfWork.Save();

        }

        public void InsertDefaultAndChange(TherapeuticAction dto)
        {
            TherapeuticAction newDefault = (dto);
            newDefault.Default = true;
            unitOfWork.therapeuticActionRepository.Insert(newDefault);

            ChangeDefault();
            unitOfWork.Save();
        }

        public void Insert(TherapeuticAction dto)
        {
            TherapeuticAction entity = (dto);
            unitOfWork.therapeuticActionRepository.Insert(entity);
            unitOfWork.Save();
        }

        public void Update(TherapeuticAction dto)
        {
            TherapeuticAction entity = unitOfWork.therapeuticActionRepository.GetById(dto.IDTherapeuticAction);
            entity.Description = dto.Description;
            entity.Default = dto.Default;
            unitOfWork.therapeuticActionRepository.Update(entity);
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
                TherapeuticAction defaultTherapeuticAction = unitOfWork.therapeuticActionRepository.GetDefault(id);
                if (defaultTherapeuticAction == null)
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
                TherapeuticAction defaultTherapeuticAction = unitOfWork.therapeuticActionRepository.GetDefault(id);
                foreach (Medicine m in medicines)
                {
                    m.TherapeuticAction = defaultTherapeuticAction;
                    _repositoryMedicine.Update(m);
                }
                unitOfWork.therapeuticActionRepository.Delete(id);
                unitOfWork.Save();
            }
            else
            {
                unitOfWork.therapeuticActionRepository.Delete(id);
                unitOfWork.Save();
            }
        }

        /// <summary>
        /// Verifica si existe un laboratorio
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public bool TherapeuticActionExists(TherapeuticAction dto)
        {
            return unitOfWork.therapeuticActionRepository.Exists((dto));
        }
        /// <summary>
        /// Obtiene todos los registros
        /// </summary>
        /// <returns></returns>
        public List<TherapeuticAction> GetAll()
        {
            return (unitOfWork.therapeuticActionRepository.GetAll().ToList());
        }
        /// <summary>
        /// Verifica si el ID es un registro default
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool IsDefault(int id)
        {
            return unitOfWork.therapeuticActionRepository.GetById(id).Default;
        }
        /// <summary>
        /// Verifica si existe un refistro default
        /// </summary>
        /// <returns></returns>
        public bool ExistsDefault()
        {
            return unitOfWork.therapeuticActionRepository.ExistsDefault();
        }
        /// <summary>
        /// Verifica si existe un refistro default
        /// </summary>
        /// <param name="idToExclude"></param>
        /// <returns></returns>
        public bool ExistsDefault(int? idToExclude)
        {
            return unitOfWork.therapeuticActionRepository.GetDefault(idToExclude) != null;
        }
        /// <summary>
        /// Verifica si el registro tiene dependencias
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool HasDependencies(int id)
        {
            return unitOfWork.therapeuticActionRepository.HasDependencies(id);
        }
        /// <summary>
        /// Obtiene las medicinas relacionadas
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private List<Medicine> GetReferenceMedicines(int id)
        {
            return unitOfWork.therapeuticActionRepository.GetReferenceMedicines(id);
        }
        /// <summary>
        /// Cambia el registro por default
        /// </summary>
        private void ChangeDefault()
        {
            TherapeuticAction defaultTherapeuticAction = unitOfWork.therapeuticActionRepository.GetDefault();

            if (defaultTherapeuticAction != null)
            {
                defaultTherapeuticAction.Default = false;
                unitOfWork.therapeuticActionRepository.Update(defaultTherapeuticAction);
            }
        }

        /// <summary>
        /// Verifica si un Laboratorio puede ser guardado, basándose en que no haya otro 
        /// Laboratorio por defecto en caso de que el Laboratorio a guardar deba ser así
        /// </summary>
        public bool CanBeSaved(TherapeuticAction dto)
        {
            if (dto.Default && ExistsDefault(dto.IDTherapeuticAction))
                return false;
            else
                return true;
        }

    }
}
