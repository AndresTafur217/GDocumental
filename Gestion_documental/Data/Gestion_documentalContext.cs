using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Gestion_documental.Models;

namespace Gestion_documental.Data
{
    public class Gestion_documentalContext : DbContext
    {
        public Gestion_documentalContext (DbContextOptions<Gestion_documentalContext> options)
            : base(options)
        {
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.LogTo(Console.WriteLine);
            optionsBuilder.EnableSensitiveDataLogging();
        }

        public DbSet<Categoria> Categoria { get; set; } = default!;
        public DbSet<Documento> Documento { get; set; } = default!;
        public DbSet<Usuario> Usuario { get; set; } = default!;
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Email)
                .IsUnique();
        }
    }
}
