using System.Threading.Tasks;
using WebApi.Models.ReceiptsAccounts;

namespace WebApi.Interfaces
{
    public interface IReceiptsAccountsService
    {
        Task<int> Create(CreateRequest model);
        Task<int> CreateUnique(CreateSingle model);
        Task<int> Delete(string embarqueid, int accountid);
    }
}
