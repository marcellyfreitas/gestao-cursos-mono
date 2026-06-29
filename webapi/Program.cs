using ApiSgc.Database;
using ApiSgc.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>();
builder.Services.AddCustomAuthentication(builder.Configuration);
builder.Services.AddCustomControllers();
builder.Services.AddRazorPages();
builder.Services.AddMvc();
builder.Services.AddCustomServices();
builder.Services.AddCustomSwagger();
builder.Services.ConfigureApplicationCookie(options =>
{
    if (builder.Environment.IsProduction())
    {
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.Expiration = TimeSpan.FromHours(8);
    }
    else
    {
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.None;
    }
});

var app = builder.Build();

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Local"))
{
    app.UseCustomSwagger();
}

app.UseCors(policy => policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapCustomControllerRoutes();
app.UseHttpsRedirection();
app.MapControllers();
app.MapRazorPages();

await app.SeedDatabase();


app.Run();