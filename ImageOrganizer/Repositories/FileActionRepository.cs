using ImageOrganizer.Model;
using NLog;
using System;
using System.IO;
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
            if (dbContext.FileAction == null)
                return new List<string>();

            // Solo archivos directamente en la carpeta, no en subcarpetas ni en carpetas con el mismo prefijo
            return dbContext.FileAction
                .AsEnumerable() // Aquí empieza LINQ to Objects
                .Where(x => x.actionType == ActionType.Skip && IsFileInFolder(x.source, folder))
                .Select(x => x.source)
                .ToList();
        }

        private static bool IsFileInFolder(string file, string folder)
        {
            if (file == null || folder == null)
                return false;
            string fileFolder = Path.GetDirectoryName(file);
            return fileFolder != null && string.Equals(
                Path.TrimEndingDirectorySeparator(fileFolder),
                Path.TrimEndingDirectorySeparator(folder),
                StringComparison.OrdinalIgnoreCase);
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
                .AsEnumerable()
                .Where(x => x.actionType == ActionType.Skip && IsFileInFolder(x.source, folder))
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

        public void UpdateScannedFolder(string folder)
        {
            FileAction action = dbContext.FileAction
                .Where(x => x.actionType == ActionType.ScanFolder && x.source == folder)
                .FirstOrDefault();
            if (action == null)
                return;
            action.timestamp = DateTime.Now;
            dbContext.FileAction.Update(action);
            dbContext.SaveChanges();
        }
    }
}
