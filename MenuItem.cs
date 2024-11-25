namespace BMSOverlay.Menu
{
    public class MenuItem
    {
        public string? Label { get; set; }
        public string? Key { get; set; } // e.g., "A"
        public string? ExitKey { get; set; } // e.g., "B"
        public List<string>? Keys { get; set; } // For multiple keys
        public List<MenuItem> Submenu { get; set; }

        public bool CloseMenuAfterAction { get; set; } = true; // Default to true

        public MenuItem()
        {
            Submenu = new List<MenuItem>();
        }
    }
}
