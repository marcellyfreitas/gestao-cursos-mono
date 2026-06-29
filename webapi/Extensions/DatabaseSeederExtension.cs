using ApiSgc.Database;
using Microsoft.EntityFrameworkCore;
using ApiSgc.Database.Seeders;

namespace ApiSgc.Extensions;

public static class DatabaseSeederExtension
{
    public static async Task SeedDatabase(this IHost app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;

        var logger = services.GetRequiredService<ILogger<IHost>>();
        var context = services.GetRequiredService<ApplicationDbContext>();

        logger.LogInformation("Aplicando migrações do banco de dados...");
        await context.Database.MigrateAsync();
        logger.LogInformation("Migrações aplicadas com sucesso");

        // await LimparBanco(context, logger);

        var seeders = new List<ISeeder>
        {
            new UsuarioSeeder(context),
            // new CursoSeeder(context),
            // new ProfessorSeeder(context),
            // new CursoPrerequisitoSeeder(context),
            // new TurmaSeeder(context),
            // new AulaSeeder(context),
            // new AvaliacaoSeeder(context),
            // new MatriculaSeeder(context),
            // new FrequenciaSeeder(context),
            // new NotaSeeder(context)
        };

        foreach (var seeder in seeders)
        {
            try
            {
                logger.LogInformation("Iniciando seed de {SeederName}", seeder.GetType().Name);
                await seeder.Seed();
                logger.LogInformation("Seed de {SeederName} concluído com sucesso", seeder.GetType().Name);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao executar seed de {SeederName}", seeder.GetType().Name);
            }
        }
    }

    private static async Task LimparBanco(ApplicationDbContext context, ILogger<IHost> logger)
    {
        logger.LogInformation("Limpando dados existentes...");

        await context.Database.ExecuteSqlRawAsync("DELETE FROM frequencia");
        await context.Database.ExecuteSqlRawAsync("DELETE FROM nota");
        await context.Database.ExecuteSqlRawAsync("DELETE FROM matricula");
        await context.Database.ExecuteSqlRawAsync("DELETE FROM aula");
        await context.Database.ExecuteSqlRawAsync("DELETE FROM avaliacao");
        await context.Database.ExecuteSqlRawAsync("DELETE FROM curso_prerequisito");
        await context.Database.ExecuteSqlRawAsync("DELETE FROM turma");
        await context.Database.ExecuteSqlRawAsync("DELETE FROM curso");
        await context.Database.ExecuteSqlRawAsync("DELETE FROM professor");
        await context.Database.ExecuteSqlRawAsync("DELETE FROM usuario");

        logger.LogInformation("Dados existentes limpos com sucesso");
    }
}
