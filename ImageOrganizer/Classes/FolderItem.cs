using System;
using System.Collections.Generic;
using System.Text;

namespace ImageOrganizer.Classes
{
    // Public: WPF bindings (DisplayMemberPath) don't resolve properties of internal types
    public class FolderItem
    {
        public string Path { get; set; }
        public string Name { get; set; }
        // Text shown in the combo; includes the parent folder when several folders share the same Name
        public string DisplayName { get; set; }
        public DateTime TimestampAdded { get; set; }
    }
}
