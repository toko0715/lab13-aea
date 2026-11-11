using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Taller.Entidades;

namespace Taller.Datos;

public partial class TallerContext : DbContext
{
    public TallerContext(DbContextOptions<TallerContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Cliente> Clientes { get; set; }

    public virtual DbSet<Vehiculo> Vehiculos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.ClienteId).HasName("PK__Clientes__71ABD087A8C7645A");

            entity.HasIndex(e => e.Dni, "UQ_Clientes_DNI").IsUnique();

            entity.Property(e => e.Dni)
                .HasMaxLength(8)
                .HasColumnName("DNI");
            entity.Property(e => e.Email).HasMaxLength(120);
            entity.Property(e => e.Nombre).HasMaxLength(100);
            entity.Property(e => e.Telefono).HasMaxLength(20);
        });

        modelBuilder.Entity<Vehiculo>(entity =>
        {
            entity.HasKey(e => e.VehiculoId).HasName("PK__Vehiculo__AA088600D833CDA7");

            entity.HasIndex(e => e.ClienteId, "IX_Vehiculos_ClienteId");

            entity.HasIndex(e => e.Placa, "UQ_Vehiculos_Placa").IsUnique();

            entity.Property(e => e.Marca).HasMaxLength(40);
            entity.Property(e => e.Modelo).HasMaxLength(60);
            entity.Property(e => e.Placa).HasMaxLength(7);

            entity.HasOne(d => d.Cliente).WithMany(p => p.Vehiculos)
                .HasForeignKey(d => d.ClienteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Vehiculos_Clientes");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
