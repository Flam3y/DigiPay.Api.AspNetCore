using DigiPay.Api.Requests.Abstractions;
using DigiPay.Api.Serialization;
using DigiPay.Api.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Text.Json;

namespace DigiPay.Api.AspNetCore;

public static class Extensions
{
    public static IServiceCollection ConfigurePlategaMvc(this IServiceCollection services)
        => services.Configure<MvcOptions>(options =>
        {
            options.InputFormatters.Insert(0, _inputFormatter);
            options.OutputFormatters.Insert(0, _outputFormatter);
        });

    private static readonly DigiPayInputFormatter _inputFormatter = new();
    private static readonly DigiPayOutputFormatter _outputFormatter = new();

    private sealed class DigiPayInputFormatter : TextInputFormatter
    {
        public DigiPayInputFormatter()
        {
            SupportedEncodings.Add(Encoding.UTF8);
            SupportedMediaTypes.Add("application/json");
        }

        protected override bool CanReadType(Type type) => type == typeof(WebhookRequest);

        public sealed override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context, Encoding encoding)
        {
            object? model = await JsonSerializer.DeserializeAsync(context.HttpContext.Request.Body, context.ModelType, JsonApi.Options, context.HttpContext.RequestAborted);
            return await InputFormatterResult.SuccessAsync(model);
        }
    }

    private sealed class DigiPayOutputFormatter : TextOutputFormatter
    {
        public DigiPayOutputFormatter()
        {
            SupportedEncodings.Add(Encoding.UTF8);
            SupportedMediaTypes.Add("application/json");
        }

        protected override bool CanWriteType(Type? type) => typeof(IRequest).IsAssignableFrom(type);

        public sealed override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            Stream stream = context.HttpContext.Response.Body;
            await JsonSerializer.SerializeAsync(stream, context.Object, JsonApi.Options, context.HttpContext.RequestAborted);
        }
    }
}
