using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;

namespace FitSyncHub.Functions;

// https://github.com/Azure/azure-functions-dotnet-worker/issues/2372
public class HttpContextAccessorMiddleware(IHttpContextAccessor httpContextAccessor) : IFunctionsWorkerMiddleware
{
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        httpContextAccessor.HttpContext = context.GetHttpContext();
        await next(context);
    }
}
