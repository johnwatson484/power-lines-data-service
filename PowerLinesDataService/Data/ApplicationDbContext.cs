using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using PowerLinesDataService.Models;
using Microsoft.Extensions.Configuration;

namespace PowerLinesDataService.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Result> Result { get; set; }
        public DbSet<Fixture> Fixture { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(Program.GetConfiguration().GetConnectionString("DefaultConnection"));
        }
    }
}
