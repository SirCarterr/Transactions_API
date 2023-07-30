using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Transactions_API.Service.IService;
using Transactions_DataAccess;
using Transactions_DataAccess.Data;

namespace Transactions_API.Service
{
    public class DbService : IDbService
    {
        private readonly AppDbContext _db;

        public DbService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Transaction>> ExportTransactions(string? status, string[]? types)
        {
            if (!string.IsNullOrEmpty(status) && types?.Length > 0)
            {
                List<SqlParameter> parameters = new List<SqlParameter>();
                for (int j = 0; j < types.Length; j++)
                {
                    parameters.Add(new SqlParameter($"@p{j}", types[j]));
                }
                string rawSql = string.Format("SELECT * FROM Transactions WHERE Status = '{0}' AND Type IN ({1})", status, string.Join(", ", parameters));
                List<Transaction> transactions_db = await _db.Transactions.FromSqlRaw(rawSql, parameters.ToArray()).ToListAsync();
                return transactions_db;
            }
            else if (!string.IsNullOrEmpty(status))
            {
                List<Transaction> transactions_db = await _db.Transactions.FromSqlRaw("SELECT * FROM Transactions WHERE Status = {0}", status).ToListAsync();
                return transactions_db;
            }
            else if (types?.Length > 0) {
                List<SqlParameter> parameters = new List<SqlParameter>();
                for (int j = 0; j < types.Length; j++)
                {
                    parameters.Add(new SqlParameter($"@p{j}", types[j]));
                }
                string rawSql = string.Format("SELECT * FROM Transactions WHERE Type IN ({0})", string.Join(", ", parameters));
                List<Transaction> transactions_db = await _db.Transactions.FromSqlRaw(rawSql, parameters.ToArray()).ToListAsync();
                return transactions_db;
            }
            else
            {
                List<Transaction> transactions_db = await _db.Transactions.FromSqlRaw("SELECT * FROM Transactions").ToListAsync();
                return transactions_db;
            }
        }

        public async Task<IEnumerable<Transaction>> ExportTransactions(string clientName)
        {
            return await _db.Transactions.FromSqlRaw("SELECT * FROM Transactions WHERE ClientName = {0}", clientName).ToListAsync();
        }

        public async Task ImportTransaction(Transaction transaction)
        {
            var db_transaction = _db.Transactions.FirstOrDefault(t => t.Id == transaction.Id);
            if (db_transaction != null)
            {
                db_transaction.Status = transaction.Status;
                _db.Transactions.Update(db_transaction);
                await _db.SaveChangesAsync();
            }
            else
            {
                _db.Database.ExecuteSqlRaw("INSERT INTO Transactions VALUES ({0}, {1}, {2}, {3}, {4})", 
                    transaction.Id, 
                    transaction.Status, 
                    transaction.Type, 
                    transaction.ClientName, 
                    transaction.Amount);
            }
        }

        public async Task<int> UpdateStatus(int id, string status)
        {
            return await _db.Database.ExecuteSqlRawAsync("UPDATE Transactions SET Status = {0} WHERE Id = {1}", status, id);
        }
    }
}
