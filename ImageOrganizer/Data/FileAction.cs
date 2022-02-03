using System;
using System.Collections.Generic;
using System.Text;

namespace ImageOrganizer.Data
{
    public enum ActionType
    {
        Delete,
        Move,
        RotateLeft,
        RoteteRight,
        Skip
    }
    public class FileAction
    {
        public int Id { get; set; }
        public ActionType actionType { get; set; }
        public string source { get; set; }
        public string destination { get; set; }
        public DateTime timestamp { get; set; }

        public FileAction()
        {
            this.timestamp = DateTime.Now;
        }
        public FileAction(ActionType actionType, string source, string destination)
        {
            this.actionType = actionType;
            this.source = source;
            this.destination = destination;
            this.timestamp = DateTime.Now;
        }
    }
}
