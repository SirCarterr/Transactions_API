using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Transactions_API.Service.IService;
using Transactions_DataAccess;

namespace Transactions_API.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    [Authorize]
    public class TransactionController : ControllerBase
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
        /// <response code="202">Data successfully imported to database</response>
        /// <response code="406">Invalid file extension</response>
        /// <response code="400">Invalid data format in file</response>
        [HttpPost]
        [ActionName("import")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ImportTransactions([Required] IFormFile file)
        {
            if (Path.GetExtension(file.FileName) != ".xls" && Path.GetExtension(file.FileName) != ".xlsx")
                return StatusCode(406);

            List<Transaction>? transactions = _fileManagerService.ConvertExelFile(file);

            if (transactions == null)
                return BadRequest();

            foreach (Transaction t in transactions)
            {
                await _dbService.ImportTransaction(t);
            }

            return StatusCode(StatusCodes.Status202Accepted);
        }

        /// <summary>
        /// Updates transaction status by id
        /// </summary>
        /// <response code="204">Transaction's status updated</response>
        /// <response code="404">Transaction not found</response>
        /// <response code="406">Invalid transaction id</response>
        [HttpPut("{id:int}")]
        [ActionName("update")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
        public async Task<IActionResult> UpdateStatus(int id, [FromQuery, Required] string status)
        {
            if (id == 0)
                return StatusCode(406);

            int result = await _dbService.UpdateStatus(id, status);

            return result > 0 ? StatusCode(StatusCodes.Status204NoContent) : StatusCode(StatusCodes.Status404NotFound);
        }

        /// <summary>
        /// Exports sorted by status and type transactions to csv file
        /// </summary>
        /// <response code="200">returns .csv file with transactions</response>
        [HttpGet]
        [ActionName("export")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ExportTransactions([FromQuery] string? clientName, [FromQuery] string? status, [FromQuery] string? types)
        {
            IEnumerable<Transaction> transactions  = await _dbService.ExportTransactions(clientName, status, types?.Split('-'));

            _fileManagerService.ConvertDataToFile(transactions.ToList());
            FileStream fileStream = System.IO.File.OpenRead("Files/data.transactions.csv");

            return File(fileStream, "text/csv", "Transactions_data.csv");
        }
    }
}
