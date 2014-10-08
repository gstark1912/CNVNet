using KATHIE.Central.DAL.Repositories;
using KATHIE.Central.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KATHIE.Central.BLL1.Services
{
    public class AdminViaService
    {
        private readonly AdminViaRepository _repositoryAdminVia;
        private readonly MedicineRepository _repositoryMedicine;

        public AdminViaService()
        {
            _repositoryAdminVia = new AdminViaRepository();
            _repositoryMedicine = new MedicineRepository();
        }

        public AdminVia GetByID(int ID)
        {
            return _repositoryAdminVia.GetById(ID);
        }

        public PagedDataResult<AdminVia> GetForGrid(string filter, PagedDataParameters parameters)
        {
            PagedDataResult<AdminVia> result = _repositoryAdminVia.GetForGrid(filter, parameters);
            return new PagedDataResult<AdminVia>(result.Results, result.TotalCount);
        }

        public List<AdminVia> GetForAutoComplete(string term)
        {
            return _repositoryAdminVia.GetForAutoComplete(term);
        }
        public void UpdateDefaultAndChange(AdminVia dto)
        {
            AdminVia newDefault = (dto);
            newDefault.Default = true;
            _repositoryAdminVia.Update(newDefault);

            ChangeDefault();

        }

        public void InsertDefaultAndChange(AdminVia dto)
        {
            AdminVia newDefault = (dto);
            newDefault.Default = true;
            _repositoryAdminVia.Insert(newDefault);

            ChangeDefault();

        }
        public void Insert(AdminVia entity)
        {
            _repositoryAdminVia.Insert(entity);
        }

        public void Update(AdminVia dto)
        {
            AdminVia entity = _repositoryAdminVia.GetById(dto.IDAdminVia);
            _repositoryAdminVia.Update(entity);
        }

        public bool CheckInUse(int id)
        {
            List<Medicine> medicines = GetReferenceMedicines(id);
            if (medicines.Count > 0) // tiene registros asociados
            {
                AdminVia defaultAdminVia = _repositoryAdminVia.GetDefault(id);
                if (defaultAdminVia == null)
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
                AdminVia defaultAdminVia = _repositoryAdminVia.GetDefault(id);
                if (defaultAdminVia != null)
                {
                    foreach (Medicine m in medicines)
                    {
                        m.AdminVia = defaultAdminVia;
                        _repositoryMedicine.Update(m);
                    }


                    _repositoryAdminVia.Delete(id);
                    return true;
                }
                else
                {
                    message = "No se puede eliminar el registro ya que no existe Vía de Administración configurada por defecto.";
                    return false;
                }
            }
            else
            {
                _repositoryAdminVia.Delete(id);
                return true;
            }
        }


        public bool AdminViaExists(AdminVia dto)
        {
            return _repositoryAdminVia.Exists((dto));
        }

        public List<AdminVia> GetAll()
        {
            return (_repositoryAdminVia.GetAll());
        }

        public AdminVia GetById(int id)
        {
            return (_repositoryAdminVia.GetById(id));
        }



        private void ChangeDefault()
        {
            AdminVia defaultAdminVia = _repositoryAdminVia.GetDefault();

            if (defaultAdminVia != null)
            {
                defaultAdminVia.Default = false;
                _repositoryAdminVia.Update(defaultAdminVia);
            }
        }
        public bool IsDefault(int id)
        {
            return _repositoryAdminVia.GetById(id).Default;
        }

        public bool ExistsDefault()
        {
            return _repositoryAdminVia.ExistsDefault();
        }

        public bool ExistsDefault(int? idToExclude)
        {
            return _repositoryAdminVia.GetDefault(idToExclude) != null;
        }

        public bool HasDependencies(int id)
        {
            return _repositoryAdminVia.HasDependencies(id);
        }

        private List<Medicine> GetReferenceMedicines(int id)
        {
            return _repositoryAdminVia.GetReferenceMedicines(id);
        }



    }
}
