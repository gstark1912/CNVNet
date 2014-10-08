using KATHIE.Central.DAL.Repositories;
using KATHIE.Central.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KATHIE.Central.BLL1.Services
{
    public class UnitTypeService
    {
        private readonly UnitTypeRepository _repositoryUnitType;
        private readonly MedicineRepository _repositoryMedicine;

        public UnitTypeService()
        {
            _repositoryUnitType = new UnitTypeRepository();
            _repositoryMedicine = new MedicineRepository();
        }

        public UnitType GetByID(int IDUnitType)
        {
            return _repositoryUnitType.GetById(IDUnitType);
        }


        public void UpdateDefaultAndChange(UnitType dto)
        {
            UnitType newDefault = (dto);
            newDefault.Default = true;
            _repositoryUnitType.Update(newDefault);

            ChangeDefault(dto.Type);


        }

        public void InsertDefaultAndChange(UnitType dto)
        {
            UnitType newDefault = dto;
            newDefault.Default = true;
            _repositoryUnitType.Insert(newDefault);

            ChangeDefault(dto.Type);


        }

        public PagedDataResult<UnitType> GetForGrid(string filter, PagedDataParameters parameters)
        {
            PagedDataResult<UnitType> result = _repositoryUnitType.GetForGrid(filter, parameters);

            return new PagedDataResult<UnitType>(result.Results, result.TotalCount);
        }

        public List<UnitType> GetForAutoComplete(string term)
        {
            return _repositoryUnitType.GetForAutoComplete(term);
        }

        public void Insert(UnitType dto)
        {
            UnitType entity = (dto);
            _repositoryUnitType.Insert(entity);
        }

        public void Update(UnitType dto)
        {
            UnitType entity = _repositoryUnitType.GetById(dto.IDUnitType);
            _repositoryUnitType.Update(entity);
        }

        public bool CheckInUse(int id)
        {
            UnitType dto = GetByID(id);
            List<Medicine> medicines = GetReferenceMedicines(id);
            if (medicines.Count > 0) // tiene registros asociados
            {
                UnitType defaultUnitType = _repositoryUnitType.GetDefault(dto.Type, dto.IDUnitType);
                if (defaultUnitType == null)
                {
                    return true; // No hay default, no se puede borrar
                }
                else
                {
                    return false;// hay default, se puede borrar
                }
            }
            return false; // no tiene registros asociados, se puede borrar
        }

        public bool Delete(int id, ref string message)
        {
            UnitType dto = GetByID(id);
            //if (dto.Default)
            //{
            //    message = "No se puede eliminar el tipo de unidad ya que está configurado por defecto.";
            //    return false;
            //}

            List<Medicine> medicines = GetReferenceMedicines(id);
            if (medicines.Count > 0)
            {
                UnitType defaultUnitType = _repositoryUnitType.GetDefault(dto.Type, dto.IDUnitType);
                if (defaultUnitType != null)
                {
                    foreach (Medicine m in medicines)
                    {
                        //m.PresentationUnitType = defaultUnitType;
                        AssignDefaultUnit(id, defaultUnitType, m);
                        _repositoryMedicine.Update(m);
                    }

                    _repositoryUnitType.Delete(id);
                    return true;
                }
                else
                {
                    message = "No se puede eliminar el registro ya que no existe Unidades configurado por defecto.";
                    return false;
                }
            }
            else
            {
                _repositoryUnitType.Delete(id);
                return true;
            }
        }

        public bool IsDefault(int id)
        {
            return _repositoryUnitType.GetById(id).Default;
        }

        public bool ExistsDefault(int type)
        {
            return _repositoryUnitType.ExistsDefault(type);
        }

        public bool ExistsDefault(int type, int? idToExclude)
        {
            return _repositoryUnitType.GetDefault(type, idToExclude) != null;
        }

        public bool UnitTypeExists(UnitType dto)
        {
            return _repositoryUnitType.Exists(dto);
        }



        public List<UnitType> GetAll()
        {
            return _repositoryUnitType.GetAll();
        }

        public UnitType GetUnitTypeById(int id)
        {
            return _repositoryUnitType.GetById(id);
        }


        private void AssignDefaultUnit(int originID, UnitType defUnitType, Medicine medicine)
        {
            if (medicine.IDPresentationUnitType == originID)
                medicine.PresentationUnitType = defUnitType;
            if (medicine.IDFrequencyUnit == originID)
                medicine.FrequencyUnit = defUnitType;
            if (medicine.IDMaximumTimeUnit == originID)
                medicine.MaximumTimeUnit = defUnitType;
            if (medicine.IDMaximumUnit == originID)
                medicine.MaximumUnit = defUnitType;
            if (medicine.IDTreatmentUnit == originID)
                medicine.TreatmentUnit = defUnitType;
        }

        private List<Medicine> GetReferenceMedicines(int id)
        {
            return _repositoryUnitType.GetReferenceMedicines(id);
        }

        private void ChangeDefault(int type)
        {
            UnitType defaultUnitType = _repositoryUnitType.GetDefault(type);

            if (defaultUnitType != null)
            {
                defaultUnitType.Default = false;
                _repositoryUnitType.Update(defaultUnitType);
            }
        }


    }
}
