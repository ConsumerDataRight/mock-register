using CDR.Register.API.Infrastructure.Filters;
using CDR.Register.Repository.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using System.IO;
using System.Threading.Tasks;

namespace CDR.Register.Admin.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly ILogger<AdminController> _logger;
        private readonly RegisterDatabaseContext _dbContext;

        public AdminController(ILogger<AdminController> logger, RegisterDatabaseContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        [HttpPost]
        [Route("Metadata")]
        [ServiceFilter(typeof(LogActionEntryAttribute))]
        public async Task LoadData()
        {
            using var reader = new StreamReader(Request.Body);
            string json = await reader.ReadToEndAsync();
            string respMsg = "";

            try
            {
                bool updated = await _dbContext.SeedDatabaseFromJson(json, _logger, true);
                if (updated)
                    Response.StatusCode = StatusCodes.Status200OK;
                else
                {
                    Response.StatusCode = StatusCodes.Status400BadRequest;
                    respMsg = "Database not updated";
                }
            }
            catch
            {
                // SeedDatabaseFromJson doesn't throw specific error exceptions, so lets just consider any exception a BadRequest
                Response.StatusCode = StatusCodes.Status400BadRequest;
                respMsg = "UnexpectedError, An error occurred loading the database.";
            }
            finally
            {
                Response.ContentType = "application/json";
                await Response.BodyWriter.WriteAsync(System.Text.Encoding.UTF8.GetBytes(respMsg));
            }
        }

        [HttpGet]
        [Route("Metadata")]
        [ServiceFilter(typeof(LogActionEntryAttribute))]
        public async Task GetData()
        {
            var metadata = await _dbContext.GetJsonFromDatabase();

            // Return the raw JSON response.
            Response.ContentType = "application/json";
            await Response.BodyWriter.WriteAsync(System.Text.UTF8Encoding.UTF8.GetBytes($"{{ \"LegalEntities\": {metadata} }}"));
        }
    }
}
