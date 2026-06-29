using ApiSgc.Services;
using ApiSgc.Services.Contracts;

namespace ApiSgc.Extensions;

static class ServiceExtension
{
    public static IServiceCollection AddCustomServices(this IServiceCollection services)
    {
        services.AddTransient<IAuthenticationService, AuthenticationService>();
        services.AddTransient<IUsuarioService, UsuarioService>();
        services.AddTransient<IProfessorService, ProfessorService>();
        services.AddTransient<ICursoService, CursoService>();
        services.AddTransient<ITurmaService, TurmaService>();
        services.AddTransient<IAulaService, AulaService>();
        services.AddTransient<IAvaliacaoService, AvaliacaoService>();
        services.AddTransient<IMatriculaService, MatriculaService>();
        services.AddTransient<INotaService, NotaService>();
        services.AddTransient<IFrequenciaService, FrequenciaService>();
        services.AddTransient<ICursoPrerequisitoService, CursoPrerequisitoService>();
        services.AddTransient<IEmailService, EmailService>();

        return services;
    }
}