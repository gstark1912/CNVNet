/*
 * KATHIE WEB  (http://www.horizon.com.ar)
 * ========================================================================
 * Copyright 2014 Horizon.
 * ========================================================================
 * Servicio encargado del manejo de la pantalla de Laboratorio.
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
    public class LaboratoryService
    {
        private readonly MedicineRepository _repositoryMedicine;
        private readonly UnitOfWork unitOfWork;

        public LaboratoryService()
        {
            _repositoryMedicine = new MedicineRepository();
            unitOfWork = new UnitOfWork();
        }

        /// <summary>
        /// Devuelve un laboratorio segun su ID
        /// </summary>
        /// <param name="IDLaboratory"></param>
        /// <returns></returns>
        public Laboratory GetByID(int IDLaboratory)
        {
            return (unitOfWork.laboratoryRepository.GetById(IDLaboratory));
        }

        /// <summary>
        /// Devuelve un listado paginado para cargar la grilla 
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public PagedDataResult<Laboratory> GetForGrid(string filter, PagedDataParameters parameters)
        {
            PagedDataResult<Laboratory> result = unitOfWork.laboratoryRepository.GetForGrid(filter, parameters);
            return new PagedDataResult<Laboratory>((result.Results), result.TotalCount);
        }

        /// <summary>
        /// Obtiene lista para autocomplete
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>
        public List<Laboratory> GetForAutoComplete(string term)
        {
            return (unitOfWork.laboratoryRepository.GetForAutoComplete(term));
        }

        /// <summary>
        /// Actualiza el registro por defecto y cambia el anterior
        /// </summary>
        /// <param name="dto"></param>
        public void UpdateDefaultAndChange(Laboratory dto)
        {
            Laboratory newDefault = (dto);
            newDefault.Default = true;
            unitOfWork.laboratoryRepository.Update(newDefault);

            ChangeDefault();

            unitOfWork.Save();

        }
        /// <summary>
        /// Inserta un nuevo default y cambia el default anterior
        /// </summary>
        /// <param name="dto"></param>
        public void InsertDefaultAndChange(Laboratory dto)
        {
            Laboratory newDefault = (dto);
            newDefault.Default = true;
            unitOfWork.laboratoryRepository.Insert(newDefault);

            ChangeDefault();
            unitOfWork.Save();

        }
        /// <summary>
        /// Inserta un registro nuevo
        /// </summary>
        /// <param name="dto"></param>
        public void Insert(Laboratory dto)
        {
            Laboratory entity = (dto);
            unitOfWork.laboratoryRepository.Insert(entity);
            unitOfWork.Save();
        }
        /// <summary>
        /// Actualiza un registro
        /// </summary>
        /// <param name="dto"></param>
        public void Update(Laboratory dto)
        {
            Laboratory entity = unitOfWork.laboratoryRepository.GetById(dto.IDLaboratory);
            entity.Description = dto.Description;
            entity.Default = dto.Default;
            unitOfWork.laboratoryRepository.Update(entity);
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
                Laboratory defaultLaboratory = unitOfWork.laboratoryRepository.GetDefault(id);
                if (defaultLaboratory == null)
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
                Laboratory defaultLaboratory = unitOfWork.laboratoryRepository.GetDefault(id);
                foreach (Medicine m in medicines)
                {
                    m.Laboratory = defaultLaboratory;
                    _repositoryMedicine.Update(m);
                }
                unitOfWork.laboratoryRepository.Delete(id);
                unitOfWork.Save();
            }
            else
            {
                unitOfWork.laboratoryRepository.Delete(id);
                unitOfWork.Save();
            }
        }

        /// <summary>
        /// Verifica si existe un laboratorio
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public bool LaboratoryExists(Laboratory dto)
        {
            return unitOfWork.laboratoryRepository.Exists((dto));
        }
        /// <summary>
        /// Obtiene todos los registros
        /// </summary>
        /// <returns></returns>
        public List<Laboratory> GetAll()
        {
            return (unitOfWork.laboratoryRepository.GetAll().ToList());
        }
        /// <summary>
        /// Verifica si el ID es un registro default
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool IsDefault(int id)
        {
            return unitOfWork.laboratoryRepository.GetById(id).Default;
        }
        /// <summary>
        /// Verifica si existe un refistro default
        /// </summary>
        /// <returns></returns>
        public bool ExistsDefault()
        {
            return unitOfWork.laboratoryRepository.ExistsDefault();
        }
        /// <summary>
        /// Verifica si existe un refistro default
        /// </summary>
        /// <param name="idToExclude"></param>
        /// <returns></returns>
        public bool ExistsDefault(int? idToExclude)
        {
            return unitOfWork.laboratoryRepository.GetDefault(idToExclude) != null;
        }
        /// <summary>
        /// Verifica si el registro tiene dependencias
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool HasDependencies(int id)
        {
            return unitOfWork.laboratoryRepository.HasDependencies(id);
        }
        /// <summary>
        /// Obtiene las medicinas relacionadas
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private List<Medicine> GetReferenceMedicines(int id)
        {
            return unitOfWork.laboratoryRepository.GetReferenceMedicines(id);
        }
        /// <summary>
        /// Cambia el registro por default
        /// </summary>
        private void ChangeDefault()
        {
            Laboratory defaultLaboratory = unitOfWork.laboratoryRepository.GetDefault();

            if (defaultLaboratory != null)
            {
                defaultLaboratory.Default = false;
                unitOfWork.laboratoryRepository.Update(defaultLaboratory);
            }
        }

        /// <summary>
        /// Verifica si un Laboratorio puede ser guardado, basándose en que no haya otro 
        /// Laboratorio por defecto en caso de que el Laboratorio a guardar deba ser así
        /// </summary>
        public bool CanBeSaved(Laboratory dto)
        {
            if (dto.Default && ExistsDefault(dto.IDLaboratory))
                return false;
            else
                return true;
        }
    }
}
