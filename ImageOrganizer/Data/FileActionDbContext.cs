using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ImageOrganizer.Data
{
    public class FileActionDbContext : DbContext
    {
        #region Contructor
        public FileActionDbContext(DbContextOptions<FileActionDbContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
        #endregion
        #region Public properties
        public DbSet<FileAction> FileAction { get; set; }
        #endregion
        #region Overridden method
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FileAction>().HasData(GetActions());
            base.OnModelCreating(modelBuilder);
        }
        #endregion
        #region Private method
        private List<FileAction> GetActions()
        {
            return new List<FileAction>
            {
                new FileAction { Id = 1, actionType = ActionType.Delete, source = "", destination = "", timestamp = DateTime.Now },
                new FileAction { Id = 2, actionType = ActionType.Move, source = "", destination = "", timestamp = DateTime.Now },
                new FileAction { Id = 3, actionType = ActionType.Skip, source = "", destination = "", timestamp = DateTime.Now }
            };
        }
        #endregion
    }
}
