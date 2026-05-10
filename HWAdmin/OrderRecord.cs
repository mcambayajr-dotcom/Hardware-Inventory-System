namespace ComputerHardwareStockMonitoringSystem
{
    public class OrderRecord
    {
        public int id { get; set; }
        public int customer_id { get; set; }
        public string customer_username { get; set; }
        public string customer_name { get; set; }
        public string customer_email { get; set; }
        public int item_id { get; set; }
        public string item_name { get; set; }
        public string category { get; set; }
        public string brand { get; set; }
        public string model { get; set; }
        public int current_stock { get; set; }
        public int quantity_ordered { get; set; }
        public string order_status { get; set; }
        public string customer_note { get; set; }
        public string admin_note { get; set; }
        public int is_stock_deducted { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
    }
}
