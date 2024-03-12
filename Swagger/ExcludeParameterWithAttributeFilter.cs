using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace LinguacApi.Swagger
{
    public class ExcludeParameterWithAttributeFilter<TExclusionAttribute> : IOperationFilter where TExclusionAttribute : Attribute
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Parameters is null) return;

            // Get a list of parameters (from the action method) that have the specified attribute
            var excludedParameterNames = context.MethodInfo.GetParameters()
                .Where(parameterInfo => parameterInfo.GetCustomAttributes(typeof(TExclusionAttribute), false).Length != 0)
                .Select(parameterInfo => parameterInfo.Name)
                .ToList();

            // Remove these parameters from the Swagger operation
            operation.Parameters = operation.Parameters
                .Where(parameter => !excludedParameterNames.Contains(parameter.Name, StringComparer.OrdinalIgnoreCase))
                .ToList();
        }
    }
}
