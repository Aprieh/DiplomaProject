using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace DiplomaProject
{
    public class Heatsink
    {
        public int HeatsinkID { get; set; }
        public int? MaterialID { get; set; }
        public double? WallLength { get; set; }
        public double? Length { get; set; }
        public double? Depth { get; set; }
        public double? BaseThickness { get; set; }
        public string? FastenerType { get; set; }
        public double? FinHeight { get; set; }
        public double? FinThickness { get; set; }
        public int? NumberOfFins { get; set; }
        public double? TDP { get; set; }
        public double? Emissivity { get; set; }
        public double? TemperatureEnvironment { get; set; }
        public double? TemperatureLimit { get; set; }
        public double? TemperatureAchieved { get; set; }
        public Material Material { get; set; }
        public ICollection<Project> Projects { get; set; }
        public Heatsink()
        {
            MaterialID = null;
            Length = null;
            Depth = null;
            BaseThickness = null;
            FinHeight = null;
            FinThickness = null;
            NumberOfFins = null;
            TDP = null;
            Emissivity = null;
            TemperatureEnvironment = null;
            TemperatureLimit = null;
            TemperatureAchieved = null;
        }
    }

    public class Material
    {
        public int MaterialID { get; set; }
        public string Name { get; set; }
        public double ThermalConductivity { get; set; }
        public double Density { get; set; }
        public double Emissivity { get; set; }
        public ICollection<Heatsink> Heatsinks { get; set; }
    }
    public class Project
    {
        public int ProjectID { get; set; }
        public int? HeatsinkID { get; set; } 
        public string ProjectName { get; set; }
        public string Description { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public string? CADFilePath { get; set; }
        public Heatsink Heatsink { get; set; } // Navigation property to Heatsink
    }
    public class DatabaseContext : DbContext
    {
        public DbSet<Heatsink> Heatsinks { get; set; }
        public DbSet<Material> Materials { get; set; }
        public DbSet<Project> Projects { get; set; }

        private readonly string dataSource = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DataBase.db");
        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite("Data Source=" + dataSource);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Material>()
                .HasMany(m => m.Heatsinks)
                .WithOne(h => h.Material)
                .HasForeignKey(h => h.MaterialID);

            modelBuilder.Entity<Project>()
                .HasOne(p => p.Heatsink)
                .WithMany(h => h.Projects)
                .HasForeignKey(p => p.HeatsinkID);
        }
    }
}
