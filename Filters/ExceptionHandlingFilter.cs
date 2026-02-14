namespace CBCMissionaryWallApi.Filters
{
    public class ExceptionHandlingFilter : IEndpointFilter
    {
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            try
            {
                return await next(context);
            }
            catch (Exception ex)
            {
                var env = context.HttpContext.RequestServices.GetRequiredService<IHostEnvironment>();

                //Log the exception (you can use any logging framework you prefer)
                Console.Error.WriteLine($"Exception caught in filter: {ex.Message}");
                
                // Return a generic error response
                return Results.Problem(
                    detail: env.IsDevelopment() ? ex.ToString() : null,
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "An unexpected error occurred.");
            }
        }
    }
}
