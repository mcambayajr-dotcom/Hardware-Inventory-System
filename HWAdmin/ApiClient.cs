using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;

namespace ComputerHardwareStockMonitoringSystem
{
    // Response model for hardware inventory API requests
    public class HardwareApiResponse
    {
        public bool success { get; set; }
        public string message { get; set; }
        public List<HardwareItem> data { get; set; }
    }

    // Response model for customer order API requests
    public class OrderApiResponse
    {
        public bool success { get; set; }
        public string message { get; set; }
        public List<OrderRecord> data { get; set; }
    }

    // Basic response model for general API operations
    public class BasicApiResponse
    {
        public bool success { get; set; }
        public string message { get; set; }
    }

    // Main API client class used for server communication
    public class ApiClient
    {
        // Base URL of the PHP API
        private const string BaseUrl = "http://localhost:8000/api.php";

        // JSON serializer for converting JSON responses into objects
        private readonly JavaScriptSerializer serializer = new JavaScriptSerializer();

        // Retrieve all inventory items for admin view
        public List<HardwareItem> ListAdminItems()
        {
            using (var wc = CreateWebClient(true))
            {
                string json = wc.DownloadString(BaseUrl + "?action=list_admin");

                HardwareApiResponse response =
                    serializer.Deserialize<HardwareApiResponse>(json);

                EnsureSuccess(response);

                return response.data ?? new List<HardwareItem>();
            }
        }

        // Add a new inventory item
        public string AddItem(HardwareItem item)
        {
            NameValueCollection values = ItemToPostValues(item);

            values["action"] = "add";

            return Post(values);
        }

        // Update an existing inventory item
        public string UpdateItem(HardwareItem item)
        {
            if (item == null || item.id <= 0)
                throw new ArgumentException("Select an item before updating.");

            NameValueCollection values = ItemToPostValues(item);

            values["action"] = "update";
            values["id"] = item.id.ToString();

            return Post(values);
        }

        // Delete an inventory item by ID
        public string DeleteItem(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Select an item before deleting.");

            var values = new NameValueCollection();

            values["action"] = "delete";
            values["id"] = id.ToString();

            return Post(values);
        }

        // Retrieve all customer orders from the API
        public List<OrderRecord> ListOrders()
        {
            using (WebClient client = CreateWebClient(true))
            {
                string requestUrl = BaseUrl + "?action=list_orders";

                string responseJson = client.DownloadString(requestUrl);

                OrderApiResponse apiResponse =
                    serializer.Deserialize<OrderApiResponse>(responseJson);

                EnsureSuccess(apiResponse);

                if (apiResponse.data == null)
                {
                    return new List<OrderRecord>();
                }

                return apiResponse.data;
            }
        }

        // Update the status of a customer order
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

        // Convert HardwareItem object into POST request values
        private NameValueCollection ItemToPostValues(HardwareItem item)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            var values = new NameValueCollection();

            values["item_name"] = Safe(item.item_name);
            values["category"] = Safe(item.category);
            values["brand"] = Safe(item.brand);
            values["model"] = Safe(item.model);
            values["serial_number"] = Safe(item.serial_number);

            // Prevent negative quantity values
            values["quantity"] = Math.Max(0, item.quantity).ToString();

            values["status"] = Safe(item.status);
            values["location"] = Safe(item.location);
            values["remarks"] = Safe(item.remarks);

            return values;
        }

        // Send POST request to the API
        private string Post(NameValueCollection values)
        {
            using (var wc = CreateWebClient(true))
            {
                try
                {
                    // Upload POST data to API
                    byte[] result = wc.UploadValues(BaseUrl, "POST", values);

                    // Convert byte response into string
                    string json = Encoding.UTF8.GetString(result);

                    // Deserialize response
                    BasicApiResponse response =
                        serializer.Deserialize<BasicApiResponse>(json);

                    EnsureSuccess(response);

                    return string.IsNullOrWhiteSpace(response.message)
                        ? "Operation completed."
                        : response.message;
                }
                catch (WebException ex)
                {
                    // Read server-side error message
                    string serverMessage = ReadServerError(ex);

                    throw new Exception(
                        string.IsNullOrWhiteSpace(serverMessage)
                            ? ex.Message
                            : serverMessage
                    );
                }
            }
        }

        // Read detailed error response from the server
        private string ReadServerError(WebException ex)
        {
            if (ex == null || ex.Response == null)
                return string.Empty;

            using (var stream = ex.Response.GetResponseStream())
            {
                if (stream == null)
                    return string.Empty;

                using (var reader = new System.IO.StreamReader(stream))
                {
                    string json = reader.ReadToEnd();

                    try
                    {
                        BasicApiResponse response =
                            serializer.Deserialize<BasicApiResponse>(json);

                        return response == null
                            ? string.Empty
                            : response.message;
                    }
                    catch
                    {
                        // Return raw response if JSON parsing fails
                        return json;
                    }
                }
            }
        }

        // Create configured WebClient instance
        private WebClient CreateWebClient(bool admin)
        {
            var wc = new WebClient();

            wc.Encoding = Encoding.UTF8;

            // Attach admin token header if admin access is required
            if (admin)
            {
                wc.Headers.Add(
                    "X-Admin-Token",
                    AdminSettings.AdminApiToken
                );
            }

            return wc;
        }

        // Safely trim null string values
        private string Safe(string value)
        {
            return value == null
                ? string.Empty
                : value.Trim();
        }

        // Validate basic API response
        private void EnsureSuccess(BasicApiResponse response)
        {
            if (response == null)
                throw new Exception("Invalid server response.");

            if (!response.success)
            {
                throw new Exception(
                    string.IsNullOrWhiteSpace(response.message)
                        ? "Request failed."
                        : response.message
                );
            }
        }

        // Validate hardware inventory response
        private void EnsureSuccess(HardwareApiResponse response)
        {
            if (response == null)
                throw new Exception("Invalid server response.");

            if (!response.success)
            {
                throw new Exception(
                    string.IsNullOrWhiteSpace(response.message)
                        ? "Request failed."
                        : response.message
                );
            }
        }

        // Validate customer order response
        private void EnsureSuccess(OrderApiResponse response)
        {
            if (response == null)
                throw new Exception("Invalid server response.");

            if (!response.success)
            {
                throw new Exception(
                    string.IsNullOrWhiteSpace(response.message)
                        ? "Request failed."
                        : response.message
                );
            }
        }
    }
}