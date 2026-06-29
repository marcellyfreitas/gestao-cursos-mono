namespace ApiSgc.Extensions;

public static class RouteExtension
{
    public static WebApplication MapCustomControllerRoutes(this WebApplication app)
    {
        app.MapControllerRoute(
            name: "api",
            pattern: "api/v1/{controller=Home}/{action=Index}/{id?}"
        );

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}"
        );

        app.MapGet("/", () => Results.Redirect("/swagger"));

        return app;
    }
}