using Microsoft.EntityFrameworkCore;
using ApiSgc.Models;

namespace ApiSgc.Database;

public class ApplicationDbContext : DbContext
{
    private readonly IConfiguration _configuration;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IConfiguration configuration) : base(options)
    {
        _configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            optionsBuilder.UseMySql(
                connectionString,
                ServerVersion.AutoDetect(connectionString)
            );
        }
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Professor> Professores => Set<Professor>();
    public DbSet<Curso> Cursos => Set<Curso>();
    public DbSet<CursoPrerequisito> CursoPrerequisitos => Set<CursoPrerequisito>();
    public DbSet<Turma> Turmas => Set<Turma>();
    public DbSet<Aula> Aulas => Set<Aula>();
    public DbSet<Matricula> Matriculas => Set<Matricula>();
    public DbSet<Frequencia> Frequencias => Set<Frequencia>();
    public DbSet<Avaliacao> Avaliacoes => Set<Avaliacao>();
    public DbSet<Nota> Notas => Set<Nota>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<CursoPrerequisito>()
            .HasOne(cp => cp.Curso)
            .WithMany(c => c.PrerequisitosDe)
            .HasForeignKey(cp => cp.CursoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CursoPrerequisito>()
            .HasOne(cp => cp.PrerequisitoCurso)
            .WithMany(c => c.PrerequisitosPara)
            .HasForeignKey(cp => cp.CursoPrerequisitoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Matricula>()
            .HasOne(m => m.Usuario)
            .WithMany(u => u.Matriculas)
            .HasForeignKey(m => m.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Matricula>()
            .HasIndex(m => new { m.UsuarioId, m.TurmaId })
            .IsUnique();

        modelBuilder.Entity<Frequencia>()
            .HasIndex(f => new { f.MatriculaId, f.AulaId })
            .IsUnique();

        modelBuilder.Entity<Nota>()
            .HasIndex(n => new { n.MatriculaId, n.AvaliacaoId })
            .IsUnique();

        modelBuilder.Entity<Aula>()
            .HasOne(a => a.Professor)
            .WithMany(p => p.Aulas)
            .HasForeignKey(a => a.ProfessorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Avaliacao>()
            .HasOne(a => a.Turma)
            .WithMany(t => t.Avaliacoes)
            .HasForeignKey(a => a.TurmaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Usuario>()
            .Property(u => u.Role)
            .HasConversion<string>();
    }
}