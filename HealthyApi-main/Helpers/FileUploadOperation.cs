using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace HealthyApi.Helpers
{
    public class FileUploadOperation : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // 找到使用 [FromForm] 的 DTO
            var formParams = context.MethodInfo
                .GetParameters()
                .Where(p => p.GetCustomAttribute<Microsoft.AspNetCore.Mvc.FromFormAttribute>() != null)
                .ToList();

            if (!formParams.Any()) return;

            // 强制生成 multipart/form-data 类型的表单结构
            operation.RequestBody = new OpenApiRequestBody
            {
                Content =
                {
                    ["multipart/form-data"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "object",
                            Properties = new Dictionary<string, OpenApiSchema>
                            {
                                ["image"] = new OpenApiSchema
                                {
                                    Type = "string",
                                    Format = "binary",
                                    //Description = "上传的食物图片文件"
                                },
                                ["user_id"] = new OpenApiSchema
                                {
                                    Type = "integer",
                                    Format = "int32",
                                    //Description = "用户ID"
                                },
                                ["meal_type"] = new OpenApiSchema
                                {
                                    Type = "string",
                                    //Description = "餐别（breakfast / lunch / dinner）"
                                }
                            },
                            Required = new HashSet<string> { "image", "user_id", "meal_type" }
                        }
                    }
                }
            };
        }
    }
}
