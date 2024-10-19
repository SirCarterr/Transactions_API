using Transactions_DataAccess;

namespace Transactions_API.Service.IService
{
    public interface IDbService
    {
        public Task ImportTransaction(Transaction transaction);
        public Task<int> UpdateStatus(int id, string status);
        public Task<IEnumerable<Transaction>> ExportTransactions(string? clientName, string? status, string[]? types);
    }
}
