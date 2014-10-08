using KATHIE.Central.DAL.Repositories;
using KATHIE.Central.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KATHIE.Central.BLL1.Services
{
    public class PresentationService
    {
        private readonly PresentationRepository _repositoryPresentation;
        private readonly MedicineRepository _repositoryMedicine;

        public PresentationService()
        {
            _repositoryPresentation = new PresentationRepository();
            _repositoryMedicine = new MedicineRepository();
        }

        public PagedDataResult<Presentation> GetForGrid(string filter, PagedDataParameters parameters)
        {
            PagedDataResult<Presentation> result = _repositoryPresentation.GetForGrid(filter, parameters);
            return new PagedDataResult<Presentation>(result.Results, result.TotalCount);
        }

        public List<Presentation> GetForAutoComplete(string term)
        {
            return _repositoryPresentation.GetForAutoComplete(term);
        }

        public void UpdateDefaultAndChange(Presentation dto)
        {
            Presentation newDefault = (dto);
            newDefault.Default = true;
            _repositoryPresentation.Update(newDefault);

            ChangeDefault();

        }

        public void InsertDefaultAndChange(Presentation dto)
        {
            Presentation newDefault = (dto);
            newDefault.Default = true;
            _repositoryPresentation.Insert(newDefault);

            ChangeDefault();

        }

        public void Insert(Presentation entity)
        {
            _repositoryPresentation.Insert(entity);
        }

        public void Update(Presentation dto)
        {
            Presentation entity = _repositoryPresentation.GetById(dto.IDPresentation);
            entity.Description = dto.Description;
            entity.Default = dto.Default;
            _repositoryPresentation.Update(entity);
        }

        public bool CheckInUse(int id)
        {
            List<Medicine> medicines = GetReferenceMedicines(id);
            if (medicines.Count > 0) // tiene registros asociados
            {
                Presentation defaultPresentation = _repositoryPresentation.GetDefault(id);
                if (defaultPresentation == null)
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
                Presentation defaultPresentation = _repositoryPresentation.GetDefault(id);
                if (defaultPresentation != null)
                {
                    foreach (Medicine m in medicines)
                    {
                        m.Presentation = defaultPresentation;
                        m.DosePresentation = defaultPresentation;
                        _repositoryMedicine.Update(m);
                    }


                    _repositoryPresentation.Delete(id);
                    return true;
                }
                else
                {
                    message = "No se puede eliminar el registro ya que no existe Presentación configurada por defecto.";
                    return false;
                }
            }
            else
            {
                _repositoryPresentation.Delete(id);
                return true;
            }
        }

        public bool PresentationExists(Presentation dto)
        {
            return _repositoryPresentation.Exists((dto));
        }



        public List<Presentation> GetAll()
        {
            return _repositoryPresentation.GetAll();
        }

        public Presentation GetById(int id)
        {
            return _repositoryPresentation.GetById(id);
        }

        private void ChangeDefault()
        {
            Presentation defaultPresentation = _repositoryPresentation.GetDefault();

            if (defaultPresentation != null)
            {
                defaultPresentation.Default = false;
                _repositoryPresentation.Update(defaultPresentation);
            }
        }
        public bool IsDefault(int id)
        {
            return _repositoryPresentation.GetById(id).Default;
        }

        public bool ExistsDefault()
        {
            return _repositoryPresentation.ExistsDefault();
        }

        public bool ExistsDefault(int? idToExclude)
        {
            return _repositoryPresentation.GetDefault(idToExclude) != null;
        }

        public bool HasDependencies(int id)
        {
            return _repositoryPresentation.HasDependencies(id);
        }

        private List<Medicine> GetReferenceMedicines(int id)
        {
            return _repositoryPresentation.GetReferenceMedicines(id);
        }

    }
}
