﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SKNIBot.Core.Database.Logger;
using SKNIBot.Core.Database.Models;

namespace SKNIBot.Core.Database
{
    public class DynamicDBContext : DbContext
    {
        public virtual DbSet<OnlineStats> OnlineStats { get; set; }
        public virtual DbSet<JavaThing> JavaThings { get; set; }
        public virtual DbSet<Server> Servers { get; set; }
        public virtual DbSet<Emoji> Emojis { get; set; }
        public virtual DbSet<AssignRole> AssignRoles { get; set; }

        public DynamicDBContext() : base(GetOptions("Data Source=DynamicDatabase.sqlite"))
        {

        }

        private static DbContextOptions GetOptions(string connectionString)
        {
            return SqliteDbContextOptionsBuilderExtensions.UseSqlite(new DbContextOptionsBuilder(), connectionString).Options;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
#if DEBUG
            optionsBuilder.UseLoggerFactory(new DbLoggerFactory());
#endif
            base.OnConfiguring(optionsBuilder);
        }
    }
}
