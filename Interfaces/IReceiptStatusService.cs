using System.Collections.Generic;
using System.Threading.Tasks;
using WebApi.Entities;
using WebApi.Models.ReceiptsStatus;

namespace WebApi.Interfaces
{
    public interface IReceiptStatusService
    {
        Task<IEnumerable<ReceiptStatus>> GetAll();
        Task<ReceiptStatus> GetById(int id);
        void Create(CreateRequest model);
        void Update(int id, UpdateRequest model);
        void Delete(int id);
    }
}
