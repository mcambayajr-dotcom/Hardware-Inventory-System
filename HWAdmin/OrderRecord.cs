namespace ComputerHardwareStockMonitoringSystem
{
    // Represents a customer order record in the system
    public class OrderRecord
    {
        // Unique order identifier
        public int id { get; set; }

        // ID of the customer who placed the order
        public int customer_id { get; set; }

        // Customer login username
        public string customer_username { get; set; }

        // Full name of the customer
        public string customer_name { get; set; }

        // Customer email address
        public string customer_email { get; set; }

        // ID of the ordered hardware item
        public int item_id { get; set; }

        // Name of the ordered item
        public string item_name { get; set; }

        // Hardware item category
        public string category { get; set; }

        // Brand name of the ordered item
        public string brand { get; set; }

        // Model information of the hardware item
        public string model { get; set; }

        // Current stock available in inventory
        public int current_stock { get; set; }

        // Quantity requested by the customer
        public int quantity_ordered { get; set; }

        // Current processing status of the order
        public string order_status { get; set; }

        // Customer message or request note
        public string customer_note { get; set; }

        // Admin remarks regarding the order
        public string admin_note { get; set; }

        // Indicates whether stock has already been deducted
        public int is_stock_deducted { get; set; }

        // Date and time when the order was created
        public string created_at { get; set; }

        // Date and time of the latest order update
        public string updated_at { get; set; }
    }
}