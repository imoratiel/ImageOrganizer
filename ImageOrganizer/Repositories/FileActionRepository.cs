using ImageOrganizer.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImageOrganizer.Repositories
{
    public class FileActionRepository
    {
        private FileActionDbContext dbContext;
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public FileActionRepository(FileActionDbContext dbcontext)
        {
            this.dbContext = dbcontext;
        }

        public void AddAction(ActionType actionType, string source, string destination)
        {
            dbContext.Add(new FileAction(actionType, source, destination));
            dbContext.SaveChanges();
#if DEBUG
            Print();
#endif
        }

        public FileAction GetLastItem()
        {
            return dbContext.FileAction!=null && dbContext.FileAction.Count() > 0 ? dbContext.FileAction.Where(x => x.actionType!=ActionType.Skip).OrderByDescending(x => x.timestamp).FirstOrDefault() : null;
        }
        public List<string> GetSkippedItems(string folder)
        {
            if(dbContext.FileAction != null && dbContext.FileAction.Count() > 0)
            {
                List<FileAction> listActions = dbContext.FileAction.Where(x => x.actionType == ActionType.Skip && x.source.StartsWith(folder)).ToList();
                return listActions!=null ? listActions.Select(x => x.source).ToList() : null;
            }
            return null;
        }

        public void RemoveLastItem()
        {
            if(dbContext.FileAction != null && dbContext.FileAction.Count() > 0)
            {
                FileAction lastItem = GetLastItem();
                dbContext.FileAction.Remove(lastItem);
                dbContext.SaveChanges();
            }
        }

        public List<FileAction> GetAll()
        {
            return dbContext.FileAction.ToList();
        }

        public void Print()
        {
            List<FileAction> items = this.GetAll();
            foreach (FileAction item in items)
            {
                logger.Debug(string.Format("Item {0} {1} {2} {3} {4}", item.Id, item.actionType, item.source, item.destination, item.timestamp));
            }
        }
    }
}
