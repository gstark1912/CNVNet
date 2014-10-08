using KATHIE.Central.DAL.Repositories;
using KATHIE.Central.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KATHIE.Central.BLL1.Services
{
    public class DrugService
    {
        private readonly DrugRepository _repositoryDrug;

        public DrugService()
        {
            _repositoryDrug = new DrugRepository();
        }

        public PagedDataResult<Drug> GetForGrid(string filter, PagedDataParameters parameters)
        {
            PagedDataResult<Drug> result = _repositoryDrug.GetForGrid(filter, parameters);

            return new PagedDataResult<Drug>(result.Results, result.TotalCount);
        }

        public List<Drug> GetForAutoComplete(string term)
        {
            return _repositoryDrug.GetForAutoComplete(term);
        }

        public void Insert(Drug entity)
        {
            _repositoryDrug.Insert(entity);
        }

        public void Update(Drug entity)
        {
            _repositoryDrug.Update(entity);
        }

        public bool Delete(int id)
        {
            bool canDelete = true;
            List<int> medicines = _repositoryDrug.GetReferenceMedicines(id);
            if (medicines.Count > 0)
            {
                canDelete = false;
            }
            else
            {
                _repositoryDrug.Delete(id);
            }
            return canDelete;
        }

        public bool DrugExists(Drug dto)
        {
            return _repositoryDrug.Exists(dto);
        }



        public List<Drug> GetAll()
        {
            return _repositoryDrug.GetAll();
        }

        public Drug GetByID(int id)
        {
            return _repositoryDrug.GetById(id);
        }
    }
}
