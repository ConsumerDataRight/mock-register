using System.Linq;
using System.Net;
using CDR.Register.API.Infrastructure.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace CDR.Register.API.Infrastructure.Middleware
{
    public static class ModelStateErrorMiddleware
    {
        public static IActionResult ExecuteResult(ActionContext context)
        {
            var modelStateEntries = context.ModelState.Where(e => e.Value.Errors.Count > 0).ToArray();

            var responseErrorList = new ResponseErrorList();

            if (modelStateEntries.Any())
            {
                foreach (var modelStateEntry in modelStateEntries)
                {
                    foreach (var modelStateError in modelStateEntry.Value.Errors)
                    {
                        try
                        {
                            var error = JsonConvert.DeserializeObject<Error>(modelStateError.ErrorMessage);
                            error.Detail = string.Format(error.Detail, modelStateEntry.Key);
                            responseErrorList.Errors.Add(error);
                        }
                        catch
                        {
                            // This is for default and unhandled model errors.
                            var error = new Error
                            {
                                Code = StatusCodes.Status400BadRequest.ToString(),
                                Title = HttpStatusCode.BadRequest.ToString(),
                                Detail = $"{modelStateEntry.Key}: {modelStateError.ErrorMessage}"
                            };
                            responseErrorList.Errors.Add(error);
                        }
                    }
                }
            }

            return new BadRequestObjectResult(responseErrorList);
        }
    }
}
