using Transactions_DataAccess;

namespace Transactions_API.Service.IService
{
    public interface IFileManagerService
    {
        public Task<List<Transaction>?> ConvertExelFile(IFormFile file);
        public Task ConvertDataToFile(List<Transaction> transactions);
    }
}
