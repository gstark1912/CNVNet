using KATHIE.Central.DAL.Repositories;
using KATHIE.Central.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KATHIE.Central.BLL1.Services
{
    public class PackageTypeService
    {
        private readonly PackageTypeRepository _repositoryPackageType;
        private readonly MedicineRepository _repositoryMedicine;

        public PackageTypeService()
        {
            _repositoryPackageType = new PackageTypeRepository();
            _repositoryMedicine = new MedicineRepository();
        }


        public PagedDataResult<PackageType> GetForGrid(string filter, PagedDataParameters parameters)
        {
            PagedDataResult<PackageType> result = _repositoryPackageType.GetForGrid(filter, parameters);
            return new PagedDataResult<PackageType>((result.Results), result.TotalCount);
        }

        public List<PackageType> GetForAutoComplete(string term)
        {
            return (_repositoryPackageType.GetForAutoComplete(term));
        }

        public void UpdateDefaultAndChange(PackageType dto)
        {
            PackageType newDefault = (dto);
            newDefault.Default = true;
            _repositoryPackageType.Update(newDefault);

            ChangeDefault();

        }

        public void InsertDefaultAndChange(PackageType dto)
        {
            PackageType newDefault = (dto);
            newDefault.Default = true;
            _repositoryPackageType.Insert(newDefault);

            ChangeDefault();

        }

        public void Insert(PackageType dto)
        {
            PackageType entity = (dto);
            _repositoryPackageType.Insert(entity);
        }

        public void Update(PackageType dto)
        {
            PackageType entity = _repositoryPackageType.GetById(dto.IDPackageType);
            _repositoryPackageType.Update(entity);
        }

        public bool CheckInUse(int id)
        {
            List<Medicine> medicines = GetReferenceMedicines(id);
            if (medicines.Count > 0) // tiene registros asociados
            {
                PackageType defaultPackageType = _repositoryPackageType.GetDefault(id);
                if (defaultPackageType == null)
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

            //if (GetByID(id).Default)
            //{
            //    message = "No se puede eliminar el registro ya que está configurado por defecto.";
            //    return false;
            //}

            List<Medicine> medicines = GetReferenceMedicines(id);
            if (medicines.Count > 0)
            {
                PackageType defaultPackageType = _repositoryPackageType.GetDefault(id);
                if (defaultPackageType != null)
                {
                    foreach (Medicine m in medicines)
                    {
                        m.PackageType = defaultPackageType;
                        _repositoryMedicine.Update(m);
                    }


                    _repositoryPackageType.Delete(id);
                    return true;
                }
                else
                {
                    message = "No se puede eliminar el registro ya que no existe Tipo de Envase configurado por defecto.";
                    return false;
                }
            }
            else
            {
                _repositoryPackageType.Delete(id);
                return true;
            }
        }

        public bool PackageTypeExists(PackageType dto)
        {
            return _repositoryPackageType.Exists((dto));
        }

        public List<PackageType> GetAll()
        {
            return (_repositoryPackageType.GetAll());
        }

        public bool IsDefault(int id)
        {
            return _repositoryPackageType.GetById(id).Default;
        }

        public bool ExistsDefault()
        {
            return _repositoryPackageType.ExistsDefault();
        }

        public bool ExistsDefault(int? idToExclude)
        {
            return _repositoryPackageType.GetDefault(idToExclude) != null;
        }

        public bool HasDependencies(int id)
        {
            return _repositoryPackageType.HasDependencies(id);
        }

        private List<Medicine> GetReferenceMedicines(int id)
        {
            return _repositoryPackageType.GetReferenceMedicines(id);
        }

        private void ChangeDefault()
        {
            PackageType defaultPackageType = _repositoryPackageType.GetDefault();

            if (defaultPackageType != null)
            {
                defaultPackageType.Default = false;
                _repositoryPackageType.Update(defaultPackageType);
            }
        }



    }
}
