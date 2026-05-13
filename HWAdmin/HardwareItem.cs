namespace ComputerHardwareStockMonitoringSystem
{
    // Represents a single hardware inventory item
    public class HardwareItem
    {
        // Unique item identifier
        public int id { get; set; }

        // Name of the hardware item
        public string item_name { get; set; }

        // Item category (Laptop, Monitor, Keyboard, etc.)
        public string category { get; set; }

        // Brand name of the hardware item
        public string brand { get; set; }

        // Model information of the item
        public string model { get; set; }

        // Unique serial number for tracking
        public string serial_number { get; set; }

        // Available stock quantity
        public int quantity { get; set; }

        // Current inventory status
        public string status { get; set; }

        // Physical storage location of the item
        public string location { get; set; }

        // Additional notes or remarks
        public string remarks { get; set; }

        // Record creation date and time
        public string created_at { get; set; }

        // Last update date and time
        public string updated_at { get; set; }
    }
}