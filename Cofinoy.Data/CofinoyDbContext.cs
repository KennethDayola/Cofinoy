using Cofinoy.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cofinoy.Data
{
    public class CofinoyDbContext : DbContext
    {
        public CofinoyDbContext(DbContextOptions<CofinoyDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }

    }
}
