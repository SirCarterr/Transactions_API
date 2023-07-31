using IronXL;
using System.Text.RegularExpressions;
using Transactions_API.Service.IService;
using Transactions_DataAccess;

namespace Transactions_API.Service
{
    public class FIleManagerService : IFileManagerService
    {
        /*
         * Creates new workbook with worksheet 
         * Write data and save it in "File/data.transactions.cvs"
         */
        public void ConvertDataToFile(List<Transaction> transactions)
        {
            WorkBook workBook = WorkBook.Create(ExcelFileFormat.XLSX);
            WorkSheet workSheet = workBook.CreateWorkSheet("transactions");

            workSheet["A1"].StringValue = "TransactionId";
            workSheet["B1"].StringValue = "Status";
            workSheet["C1"].StringValue = "Type";
            workSheet["D1"].StringValue = "ClientName";
            workSheet["E1"].StringValue = "Amount";

            for (int i = 0; i < transactions.Count; i++)
            {
                workSheet["A" + (i + 2)].IntValue = transactions[i].Id;
                workSheet["B" + (i + 2)].StringValue = transactions[i].Status;
                workSheet["C" + (i + 2)].StringValue = transactions[i].Type;
                workSheet["D" + (i + 2)].StringValue = transactions[i].ClientName;
                workSheet["E" + (i + 2)].StringValue = "$" + transactions[i].Amount.ToString();
            }

            workBook.SaveAsCsv("Files/data.csv");
        }

        /*
         * Gets posted file and converts it to stream. 
         * Then loads the data in workbook and copy it to the list of transactions
         */
        public List<Transaction>? ConvertExelFile(IFormFile file)
        {
            using var ms = new MemoryStream();
            file.CopyTo(ms);
            WorkBook workBook = WorkBook.LoadExcel(ms);
            WorkSheet workSheet = workBook.DefaultWorkSheet;

            List<Transaction> transactions = new List<Transaction>();
            int i = 2;
            try
            {
                while (true)
                {
                    var cells = workSheet[$"A{i}:E{i}"].ToList();

                    if (!Regex.IsMatch(cells[0].StringValue, @"\d+") && !string.IsNullOrEmpty(cells[0].StringValue))
                        return null;

                    if (cells[0].IntValue == 0)
                        break;

                    string value = cells[4].StringValue.Split('$')[1].Replace('.', ',');
                    decimal amount = decimal.Parse(value);
                    transactions.Add(new Transaction
                    {
                        Id = cells[0].IntValue,
                        Status = cells[1].StringValue,
                        Type = cells[2].StringValue,
                        ClientName = cells[3].StringValue,
                        Amount = amount
                    });

                    i++;
                }

                return transactions;
            }
            catch
            {
                return null;
            } 
            finally 
            {
                workBook.Close();
            }
        }
    }
}
