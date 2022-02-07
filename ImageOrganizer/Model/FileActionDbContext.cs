using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ImageOrganizer.Model
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
                
            };
        }
        #endregion

        //using (var db = new BloggingContext())
        //{
        //    // Note: This sample requires the database to be created before running.
        //    logger.Debug($"Database path: {db.DbPath}.");

        //    //// Create
        //    //logger.Debug("Inserting a new blog");
        //    //db.Add(new Blog { Url = "http://blogs.msdn.com/adonet" });
        //    //db.SaveChanges();

        //    //// Read
        //    //logger.Debug("Querying for a blog");
        //    //var blog = db.Blogs
        //    //    .OrderBy(b => b.BlogId)
        //    //    .First();

        //    //// Update
        //    //logger.Debug("Updating the blog and adding a post");
        //    //blog.Url = "https://devblogs.microsoft.com/dotnet";
        //    //blog.Posts.Add(
        //    //    new Post { Title = "Hello World", Content = "I wrote an app using EF Core!" });
        //    //db.SaveChanges();

        //    //// Delete
        //    //logger.Debug("Delete the blog");
        //    //db.Remove(blog);
        //    //db.SaveChanges();
        //}

    }
}
