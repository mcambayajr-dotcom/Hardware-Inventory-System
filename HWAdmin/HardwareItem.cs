namespace ComputerHardwareStockMonitoringSystem
{
    public class HardwareItem
    {
        public int id { get; set; }
        public string item_name { get; set; }
        public string category { get; set; }
        public string brand { get; set; }
        public string model { get; set; }
        public string serial_number { get; set; }
        public int quantity { get; set; }
        public string status { get; set; }
        public string location { get; set; }
        public string remarks { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
    }
}
