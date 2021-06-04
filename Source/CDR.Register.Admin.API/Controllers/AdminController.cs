using System.IO;
using System.Threading.Tasks;
using CDR.Register.Repository.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

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
        public async Task<IActionResult> LoadData()
        {
            using var reader = new StreamReader(Request.Body);
            string json = await reader.ReadToEndAsync();

            try
            {
                await _dbContext.SeedDatabaseFromJson(json, _logger, true);
            }
            catch
            {
                // SeedDatabaseFromJson doesn't throw specific error exceptions, so lets just consider any exception a BadRequest
                return new BadRequestObjectResult(new CDR.Register.API.Infrastructure.Models.ResponseErrorList("UnexpectedError", "An error occurred loading the database.", null));
            }

            return Ok();
        }

        [HttpGet]
        [Route("Metadata")]
        public async Task GetData()
        {
            var metadata = await _dbContext.GetJsonFromDatabase(_logger);

            // Return the raw JSON response.
            Response.ContentType = "application/json";
            await Response.BodyWriter.WriteAsync(System.Text.UTF8Encoding.UTF8.GetBytes($"{{ \"LegalEntities\": {metadata} }}"));
        }
    }
}
