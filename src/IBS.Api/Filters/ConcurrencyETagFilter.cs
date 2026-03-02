using IBS.BuildingBlocks.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace IBS.Api.Filters;

/// <summary>
/// Result filter that sets the ETag response header from DTOs implementing <see cref="IConcurrencyAware"/>.
/// </summary>
public sealed class ConcurrencyETagFilter : IAsyncResultFilter
{
    /// <inheritdoc />
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.Result is ObjectResult { Value: IConcurrencyAware concurrencyAware })
        {
            context.HttpContext.Response.Headers.ETag = $"\"{concurrencyAware.RowVersion}\"";
        }

        await next();
    }
}
