/*
 * KATHIE WEB  (http://www.horizon.com.ar)
 * ========================================================================
 * Copyright 2014 Horizon.
 * ========================================================================
 * Servicio encargado del manejo de la pantalla de Medicamentos.
*/

using KATHIE.Central.DAL.Infraestructure;
using KATHIE.Central.DAL.Repositories;
using KATHIE.Central.Domain;
using KATHIE.Central.Domain.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KATHIE.Central.BLL1.Services
{
    public class MedicinesService
    {
        private readonly MedicineRepository medicineRepository;
        private readonly MedicineDrugRepository medicineDrugRepository;

        private readonly UnitOfWork unitOfWork;

        public MedicinesService()
        {
            medicineRepository = new MedicineRepository();
            medicineDrugRepository = new MedicineDrugRepository();
            unitOfWork = new UnitOfWork();
        }
        /// <summary>
        /// Obtiene Medicamento para AutoComplete
        /// </summary>
        /// <param name="IDDescription"></param>
        /// <param name="term"></param>
        /// <returns></returns>
        public List<Medicine> GetForAutoComplete(int IDDescription, string term)
        {
            return medicineRepository.GetForAutoComplete(IDDescription, term);
        }
        /// <summary>
        /// Busqueda para la grilla según filtro y parámetros
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public PagedDataResult<Medicine> SearchMedicines(MedicineSearch filter, PagedDataParameters parameters)
        {
            PagedDataResult<Medicine> result = medicineRepository.Search(filter, parameters);
            return new PagedDataResult<Medicine>(result.Results, result.TotalCount);
        }
        /// <summary>
        /// Inserta Medicamento
        /// </summary>
        /// <param name="entity"></param>
        public void Insert(Medicine entity)
        {
            medicineRepository.Insert(entity);
        }
        /// <summary>
        /// verifica existencia de código
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public bool CodeExists(string code)
        {
            return medicineRepository.CodeExists(code);
        }
        /// <summary>
        /// Verifica existencia de código para edición
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public bool CodeExistsForEdition(string code, int IDMedicine)
        {
            return medicineRepository.CodeExistsForEdition(code, IDMedicine);
        }
        /// <summary>
        /// Verifica existencia del nombre comercial del medicamento
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public bool MedicineExists(string Name, string ComercialName)
        {
            return medicineRepository.MedicineExists(Name, ComercialName);
        }
        /// <summary>
        /// Verifica existencia del nombre comercial del medicamento para edición
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public bool MedicineExistsForEdition(string Name, string ComercialName, int IDMedicine)
        {
            return medicineRepository.MedicineExistsForEdition(Name, ComercialName, IDMedicine);
        }
        /// <summary>
        /// Verifica si el id esta en el segmento
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool IsInSegment(int id)
        {
            bool isInSegment = false;
            List<int> segments = medicineRepository.GetSegments(id);
            if (segments.Count > 0)
                isInSegment = true;
            return isInSegment;
        }
        /// <summary>
        /// Obtiene medicina para edición
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Medicine GetForEdition(int id)
        {
            return medicineRepository.GetById(id);
        }

        /// <summary>
        /// Actualiza un medicamento
        /// </summary>
        public void Update(Medicine dto)
        {
            Medicine entity = medicineRepository.GetById(dto.IDMedicine);
            // Ejemplo de UnitOfWork, eliminar luego
            //Medicine entity = unitOfWork.medicineRepository.GetById(dto.IDMedicine);
            foreach (MedicineDrug medicine in entity.MedicineDrugs)
            {
                medicineDrugRepository.Delete(medicine.IDMedicineDrug);
                // Ejemplo de UnitOfWork, eliminar luego
                //unitOfWork.medicineDrugRepository.Delete(medicine.IDMedicineDrug);
            }
            entity.MedicineDrugs = dto.MedicineDrugs;

            medicineRepository.Update(entity);

        }


        /// <summary>
        /// Obtiene Lista paginada de medicamentos
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public PagedDataResult<Medicine> GetAllMedicineByDescription(string filter, PagedDataParameters parameters)
        {
            PagedDataResult<Medicine> result = medicineRepository.GetByDescription(filter, parameters);
            return new PagedDataResult<Medicine>(result.Results, result.TotalCount);

        }
        /// <summary>
        /// Borrar medicamento y drogas asociadas
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool Delete(int id)
        {
            Medicine med = medicineRepository.GetByIDWithMedicineDrugs(id);

            if (med.Segments.Count() > 0)
                return false;

            med.MedicineDrugs.ToList().ForEach(m => medicineDrugRepository.Delete(m.IDMedicineDrug));
            medicineRepository.Delete(id);

            return true;
        }
    }
}
