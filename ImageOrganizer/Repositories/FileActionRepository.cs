using ImageOrganizer.Model;
using NLog;
using System;
using System.Linq;
using System.Collections.Generic;

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
            if (dbContext.FileAction == null || !dbContext.FileAction.Any())
                return null;

            // Forzamos el cast a IEnumerable explícito para eliminar la ambigüedad
            return dbContext.FileAction
                .AsEnumerable() // Aquí empieza LINQ to Objects
                .Where(x => x.actionType != ActionType.ScanFolder && x.actionType != ActionType.GetDuplicated)
                .OrderByDescending(x => x.timestamp)
                .FirstOrDefault();
            //return dbContext.FileAction!=null && dbContext.FileAction.Count() > 0 ? dbContext.FileAction.AsEnumerable().Where(x => x.actionType != ActionType.ScanFolder && x.actionType != ActionType.GetDuplicated).OrderByDescending(x => x.timestamp).FirstOrDefault() : null;
        }
        public List<string> GetSkippedItems(string folder)
        {
            if (dbContext.FileAction != null && dbContext.FileAction.Count() > 0)
            {
                // Usar expresión lambda como Expression<Func<T, bool>> para evitar ambigüedad
                List<FileAction> listActions = dbContext.FileAction
                .AsEnumerable() // Aquí empieza LINQ to Objects
                .Where(x => x.actionType == ActionType.Skip && x.source.StartsWith(folder))
                .ToList();
                return listActions != null ? listActions.Select(x => x.source).ToList() : null;
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

        public System.Collections.Generic.List<FileAction> GetAll()
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

        public void ResetSkippedFiles(string folder)
        {
            List<FileAction> items = dbContext.FileAction
                .Where(x => x.source.StartsWith(folder) && x.actionType == ActionType.Skip)
                .ToList();
            dbContext.FileAction.RemoveRange(items);
            dbContext.SaveChanges();
        }

        public List<FileAction> GetScannedFolders()
        {
            return dbContext.FileAction
                .Where(x => x.actionType == ActionType.ScanFolder)
                .OrderBy(x => x.timestamp)
                .ToList();
        }

        public FileAction GetScannedFolderByName(string name)
        {
            List<FileAction> candidates = dbContext.FileAction
                .Where(x => x.actionType == ActionType.ScanFolder && x.source.Length >= name.Length && x.source.Contains(name))
                .OrderBy(x => x.timestamp)
                .ToList();
            if(candidates.Count == 1)
            {
                return candidates.First();
            }
            else
            {
                return candidates.Where(x => x.source.Substring(x.source.Length - name.Length) == name).OrderBy(x => x.timestamp).FirstOrDefault();
            }               
        }

        public void UpdateScannedFolder(string folder)
        {
            FileAction action = dbContext.FileAction
                .Where(x => x.actionType == ActionType.ScanFolder && x.source == folder)
                .FirstOrDefault();
            action.timestamp = DateTime.Now;
            dbContext.FileAction.Update(action);
        }
    }
}
