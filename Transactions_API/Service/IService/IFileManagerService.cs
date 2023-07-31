using Transactions_DataAccess;

namespace Transactions_API.Service.IService
{
    public interface IFileManagerService
    {
        public List<Transaction>? ConvertExelFile(IFormFile file);
        public void ConvertDataToFile(List<Transaction> transactions);
    }
}
