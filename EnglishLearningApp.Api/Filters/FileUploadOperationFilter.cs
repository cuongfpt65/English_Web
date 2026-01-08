using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EnglishLearningApp.Api.Filters;

public class FileUploadOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var formFileParams = context.ApiDescription.ParameterDescriptions
            .Where(p => p.Type == typeof(Microsoft.AspNetCore.Http.IFormFile))
            .ToList();

        if (formFileParams.Any())
        {
            operation.RequestBody = new OpenApiRequestBody
            {
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["multipart/form-data"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "object",
                            Properties = context.ApiDescription.ParameterDescriptions
                                .ToDictionary(
                                    p => p.Name,
                                    p => p.Type == typeof(Microsoft.AspNetCore.Http.IFormFile)
                                        ? new OpenApiSchema { Type = "string", Format = "binary" }
                                        : new OpenApiSchema { Type = "string" }
                                ),
                            Required = new HashSet<string>(formFileParams.Select(p => p.Name))
                        }
                    }
                }
            };
        }
    }
}
