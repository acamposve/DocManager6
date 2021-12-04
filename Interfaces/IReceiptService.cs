using System.Collections.Generic;
using System.Threading.Tasks;
using WebApi.Entities;
using WebApi.Models.Receipts;

namespace WebApi.Interfaces
{
    public interface IReceiptService
    {
        Task<IEnumerable<Receipt>> GetAll();
        Task<Receipt> GetById(int id);
        Task<int> Create(CreateRequest model);
        void Update(int id, UpdateRequest model);
        void Delete(int id);
        Task<IEnumerable<ReceiptsByAccount>> GetAllByAccount(int id);
        Task<IEnumerable<FilesByReceipt>> GetFilesByReceipt(int id);
    }
}
