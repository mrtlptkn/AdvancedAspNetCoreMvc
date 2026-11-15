using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ProductCatalog.Web.Filters;

// Her action'ın çalışma süresini loglar.
public class RequestTimingFilter : IActionFilter
{
    private const string TimerKey = "RequestTimer";
    private readonly ILogger<RequestTimingFilter> _logger;

    public RequestTimingFilter(ILogger<RequestTimingFilter> logger)
    {
        _logger = logger;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        context.HttpContext.Items[TimerKey] = Stopwatch.StartNew();
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.HttpContext.Items[TimerKey] is Stopwatch stopwatch)
        {
            stopwatch.Stop();
            _logger.LogInformation("{Action} {Ms} ms sürdü",
                context.ActionDescriptor.DisplayName, stopwatch.ElapsedMilliseconds);
        }
    }
}
