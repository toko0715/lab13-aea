using Microsoft.EntityFrameworkCore;
using Taller.Entidades;

namespace Taller.Datos;

public partial class TallerContext
{
    public DbSet<Usuario> Usuarios { get; set; }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(u => u.UsuarioId);
            entity.HasIndex(u => u.Correo).IsUnique();
            entity.Property(u => u.Correo).HasMaxLength(120).IsRequired();
            entity.Property(u => u.ClaveHash).HasMaxLength(60).IsRequired();
            entity.Property(u => u.Rol).HasMaxLength(30).IsRequired();
        });
    }
}
