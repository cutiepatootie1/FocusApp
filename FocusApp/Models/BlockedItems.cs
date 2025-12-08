using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FocusApp.Models
{
    public class BlockedItems
    {
        public string Name { get; set; }
        public ItemType ItemType { get; set; } // stores whether the item is a website url or an app
        public bool isActive { get; set; } = true;
        public string Platform { get; set; }

    }

    public enum ItemType { Website, App}
}
