using CDR.Register.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Linq;
using System.Net;

namespace CDR.Register.API.Infrastructure.Middleware
{
    public static class ModelStateErrorMiddleware
    {
        public static IActionResult ExecuteResult(ActionContext context)
        {
            var modelStateEntries = context.ModelState.Where(e => e.Value?.Errors.Count > 0).ToArray();

            var responseErrorList = new ResponseErrorList();

            if (modelStateEntries.Length > 0)
            {
                foreach (var modelStateEntry in modelStateEntries)
                {
                    if (modelStateEntry.Value != null)
                    {
                        foreach (var errorMessage in modelStateEntry.Value.Errors.Select(x => x.ErrorMessage))
                        {
                            try
                            {
                                var deserError = JsonConvert.DeserializeObject<Error>(errorMessage);

                                if (deserError != null)
                                {
                                    responseErrorList.Errors.Add(new Error(deserError.Code, deserError.Title, string.Format(deserError.Detail, modelStateEntry.Key), deserError.Meta?.Urn));
                                }
                            }
                            catch
                            {
                                // This is for default and unhandled model errors.
                                var error = new Error(StatusCodes.Status400BadRequest.ToString(), HttpStatusCode.BadRequest.ToString(), $"{modelStateEntry.Key}: {errorMessage}");
                                responseErrorList.Errors.Add(error);
                            }
                        }
                    }

                }
            }

            return new BadRequestObjectResult(responseErrorList);
        }
    }
}
