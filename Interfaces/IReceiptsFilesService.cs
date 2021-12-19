using System.Threading.Tasks;
using WebApi.Models.ReceiptsFiles;

namespace WebApi.Interfaces
{
    public interface IReceiptsFilesService
    {
        Task<int> Create(CreateRequest model);
        void Delete(int id);
    }
}
