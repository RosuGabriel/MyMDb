namespace MyMDb.Helpers
{
    public class ProtectedStaticFilesMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _protectedPath;

        public ProtectedStaticFilesMiddleware(RequestDelegate next, string protectedPath)
        {
            _next = next;
            _protectedPath = protectedPath;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value;

            // Verify if the request is for a protected path
            if (path != null && path.StartsWith(_protectedPath, StringComparison.OrdinalIgnoreCase))
            {
                // Verify if the user is authenticated
                if (!context.User.Identity?.IsAuthenticated ?? true)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Unauthorized");
                    return;
                }
            }

            // Continue processing the request
            await _next(context);
        }
    }

}
