using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;

namespace ComputerHardwareStockMonitoringSystem
{
    public class HardwareApiResponse
    {
        public bool success { get; set; }
        public string message { get; set; }
        public List<HardwareItem> data { get; set; }
    }

    public class OrderApiResponse
    {
        public bool success { get; set; }
        public string message { get; set; }
        public List<OrderRecord> data { get; set; }
    }

    public class BasicApiResponse
    {
        public bool success { get; set; }
        public string message { get; set; }
    }

    public class ApiClient
    {
        private const string BaseUrl = "http://localhost:8000/api.php";
        private readonly JavaScriptSerializer serializer = new JavaScriptSerializer();

        public List<HardwareItem> ListAdminItems()
        {
            using (var wc = CreateWebClient(true))
            {
                string json = wc.DownloadString(BaseUrl + "?action=list_admin");
                HardwareApiResponse response = serializer.Deserialize<HardwareApiResponse>(json);
                EnsureSuccess(response);
                return response.data ?? new List<HardwareItem>();
            }
        }

        public string AddItem(HardwareItem item)
        {
            NameValueCollection values = ItemToPostValues(item);
            values["action"] = "add";
            return Post(values);
        }

        public string UpdateItem(HardwareItem item)
        {
            if (item == null || item.id <= 0)
                throw new ArgumentException("Select an item before updating.");

            NameValueCollection values = ItemToPostValues(item);
            values["action"] = "update";
            values["id"] = item.id.ToString();
            return Post(values);
        }

        public string DeleteItem(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Select an item before deleting.");

            var values = new NameValueCollection();
            values["action"] = "delete";
            values["id"] = id.ToString();
            return Post(values);
        }

        public List<OrderRecord> ListOrders()
        {
            using (var wc = CreateWebClient(true))
            {
                string json = wc.DownloadString(BaseUrl + "?action=list_orders");
                OrderApiResponse response = serializer.Deserialize<OrderApiResponse>(json);
                EnsureSuccess(response);
                return response.data ?? new List<OrderRecord>();
            }
        }

        public string UpdateOrderStatus(int orderId, string orderStatus, string adminNote)
        {
            if (orderId <= 0)
                throw new ArgumentException("Select an order first.");

            var values = new NameValueCollection();
            values["action"] = "update_order_status";
            values["id"] = orderId.ToString();
            values["order_status"] = Safe(orderStatus);
            values["admin_note"] = Safe(adminNote);
            return Post(values);
        }

        private NameValueCollection ItemToPostValues(HardwareItem item)
        {
            if (item == null) throw new ArgumentNullException("item");

            var values = new NameValueCollection();
            values["item_name"] = Safe(item.item_name);
            values["category"] = Safe(item.category);
            values["brand"] = Safe(item.brand);
            values["model"] = Safe(item.model);
            values["serial_number"] = Safe(item.serial_number);
            values["quantity"] = Math.Max(0, item.quantity).ToString();
            values["status"] = Safe(item.status);
            values["location"] = Safe(item.location);
            values["remarks"] = Safe(item.remarks);
            return values;
        }

        private string Post(NameValueCollection values)
        {
            using (var wc = CreateWebClient(true))
            {
                try
                {
                    byte[] result = wc.UploadValues(BaseUrl, "POST", values);
                    string json = Encoding.UTF8.GetString(result);
                    BasicApiResponse response = serializer.Deserialize<BasicApiResponse>(json);
                    EnsureSuccess(response);
                    return string.IsNullOrWhiteSpace(response.message) ? "Operation completed." : response.message;
                }
                catch (WebException ex)
                {
                    string serverMessage = ReadServerError(ex);
                    throw new Exception(string.IsNullOrWhiteSpace(serverMessage) ? ex.Message : serverMessage);
                }
            }
        }

        private string ReadServerError(WebException ex)
        {
            if (ex == null || ex.Response == null) return string.Empty;
            using (var stream = ex.Response.GetResponseStream())
            {
                if (stream == null) return string.Empty;
                using (var reader = new System.IO.StreamReader(stream))
                {
                    string json = reader.ReadToEnd();
                    try
                    {
                        BasicApiResponse response = serializer.Deserialize<BasicApiResponse>(json);
                        return response == null ? string.Empty : response.message;
                    }
                    catch
                    {
                        return json;
                    }
                }
            }
        }

        private WebClient CreateWebClient(bool admin)
        {
            var wc = new WebClient();
            wc.Encoding = Encoding.UTF8;
            if (admin)
            {
                wc.Headers.Add("X-Admin-Token", AdminSettings.AdminApiToken);
            }
            return wc;
        }

        private string Safe(string value)
        {
            return value == null ? string.Empty : value.Trim();
        }

        private void EnsureSuccess(BasicApiResponse response)
        {
            if (response == null)
                throw new Exception("Invalid server response.");
            if (!response.success)
                throw new Exception(string.IsNullOrWhiteSpace(response.message) ? "Request failed." : response.message);
        }

        private void EnsureSuccess(HardwareApiResponse response)
        {
            if (response == null)
                throw new Exception("Invalid server response.");
            if (!response.success)
                throw new Exception(string.IsNullOrWhiteSpace(response.message) ? "Request failed." : response.message);
        }

        private void EnsureSuccess(OrderApiResponse response)
        {
            if (response == null)
                throw new Exception("Invalid server response.");
            if (!response.success)
                throw new Exception(string.IsNullOrWhiteSpace(response.message) ? "Request failed." : response.message);
        }
    }
}
