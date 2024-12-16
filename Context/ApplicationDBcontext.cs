// ApplicationDBcontext.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using API_Cursos_Online.Models;

namespace API_Cursos_Online.Context
{
    public class BibliotecaContext : IdentityDbContext<IdentityUser>
    {
        public BibliotecaContext(DbContextOptions<BibliotecaContext> options) : base(options) {}

        public DbSet<Curso> Cursos { get; set; }
        public DbSet<EstudianteCurso> EstudiantesCursos { get; set; } // Tabla intermedia

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de la relación muchos a muchos entre Curso y Estudiante
            modelBuilder.Entity<EstudianteCurso>()
                .HasKey(ec => new { ec.CursoId, ec.EstudianteId }); // Clave compuesta

            modelBuilder.Entity<EstudianteCurso>()
                .HasOne(ec => ec.Curso)
                .WithMany(c => c.EstudiantesCursos)
                .HasForeignKey(ec => ec.CursoId);

            modelBuilder.Entity<EstudianteCurso>()
                .HasOne(ec => ec.Estudiante)
                .WithMany()
                .HasForeignKey(ec => ec.EstudianteId);
        }
    }
}

