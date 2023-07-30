using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Transactions_API.Service.IService;
using Transactions_DataAccess;

namespace Transactions_API.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    [Authorize]
    public class TransactionController : Controller
    {
        private readonly IFileManagerService _fileManagerService;
        private readonly IDbService _dbService;

        public TransactionController(IFileManagerService fileManagerService, IDbService dbService)
        {
            _fileManagerService = fileManagerService;
            _dbService = dbService;
        }

        /// <summary>
        /// Add/Update data from posted file in database
        /// </summary>
        /// <remarks>
        /// Accepts only ".xls" and ".xlsx" file formats! Also, make sure to have valid data format in "TransactionId" and "Amount" column in posted exel table.
        /// </remarks>
        /// <response code="201">Data successfully imported to database</response>
        /// <response code="406">Invalid file extension</response>
        /// <response code="400">Invalid data format in file</response>
        [HttpPost]
        [ActionName("import")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ImportTransactions([Required] IFormFile file)
        {
            if (Path.GetExtension(file.FileName) != ".xls" && Path.GetExtension(file.FileName) != ".xlsx")
                return StatusCode(406);

            List<Transaction>? transactions = await _fileManagerService.ConvertExelFile(file);

            if (transactions == null)
                return BadRequest();

            foreach (Transaction t in transactions)
            {
                await _dbService.ImportTransaction(t);
            }

            return StatusCode(201);
        }

        /// <summary>
        /// Updates transaction status by id
        /// </summary>
        /// <response code="201">Transaction's status updated</response>
        /// <response code="404">Transaction not found</response>
        /// <response code="406">Invalid transaction id</response>
        [HttpPut("{id:int}")]
        [ActionName("update")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
        public async Task<IActionResult> UpdateStatus(int id, [FromQuery(Name = "status"), Required] string status)
        {
            if (id == 0)
                return StatusCode(406);

            int result = await _dbService.UpdateStatus(id, status);

            return result > 0 ? StatusCode(201) : StatusCode(404);
        }

        /// <summary>
        /// Exports sorted by status and type transactions to csv file
        /// </summary>
        /// <response code="200">returns .csv file with transactions</response>
        [HttpGet]
        [ActionName("export-all")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ExportTransactions([FromQuery(Name = "status")] string? status, [FromQuery(Name = "types")] string? types)
        {
            IEnumerable<Transaction> transactions = await _dbService.ExportTransactions(status, types?.Split('-'));
            await _fileManagerService.ConvertDataToFile(transactions.ToList());
            FileStream fileStream = System.IO.File.OpenRead("Files/data.transactions.csv");
            return File(fileStream, "text/csv", "Transactions_data.csv");
        }

        /// <summary>
        /// Exports sorted by client name transactions to csv file
        /// </summary>
        /// <response code="200">returns .csv file with transactions</response>
        [HttpGet]
        [ActionName("export-by-client")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ExportTransactions([FromQuery(Name = "client-name"), Required] string clientName)
        {
            IEnumerable<Transaction> transactions = await _dbService.ExportTransactions(clientName);
            await _fileManagerService.ConvertDataToFile(transactions.ToList());
            FileStream fileStream = System.IO.File.OpenRead("Files/data.transactions.csv");
            return File(fileStream, "text/csv", "Transactions_data.csv");
        }
    }
}
