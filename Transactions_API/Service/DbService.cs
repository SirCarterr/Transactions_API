using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Text;
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

        public async Task<IEnumerable<Transaction>> ExportTransactions(string? clientName, string? status, string[]? types)
        {
            StringBuilder rawSql = new("SELECT * FROM Transactions");

            bool hasWhereClause = false;

            if (string.IsNullOrEmpty(clientName) && string.IsNullOrEmpty(status) && types == null)
            {
                var transactions = await _db.Transactions.FromSqlRaw(rawSql.ToString()).ToListAsync();
                return transactions;
            }

            rawSql.Append(" WHERE ");

            if (!string.IsNullOrEmpty(clientName))
            {
                rawSql.Append($"ClientName = '{clientName}'");
                hasWhereClause = true;
            }

            if (!string.IsNullOrEmpty(status))
            {
                if (hasWhereClause) rawSql.Append(" AND ");

                rawSql.Append($"Status = '{status}'");

                hasWhereClause = true;
            }

            if (types?.Length > 0)
            {
                if (hasWhereClause) rawSql.Append(" AND ");

                string joinedTypes = string.Join(", ", types.Select(t => $"'{t}'"));

                rawSql.Append($"Type IN ({joinedTypes})");
            }

            var transactions_db = await _db.Transactions.FromSqlRaw(rawSql.ToString()).ToListAsync();
            return transactions_db;
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
