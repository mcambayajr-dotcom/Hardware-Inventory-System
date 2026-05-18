using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ComputerHardwareStockMonitoringSystem
{
    // Main administrator dashboard form for managing inventory and customer orders
    public class MainForm : Form
    {
        // API client used for server communication
        private readonly ApiClient api = new ApiClient();

        // Inventory table and filter controls
        private DataGridView grid;
        private TextBox txtSearch;
        private ComboBox cboFilterCategory;
        private ComboBox cboFilterStatus;
        private Label lblCount;
        private Label lblMode;
        private Label lblDetails;

        // Inventory input form controls
        private TextBox txtItemName;
        private TextBox txtBrand;
        private TextBox txtModel;
        private TextBox txtSerial;
        private TextBox txtQuantity;
        private TextBox txtLocation;
        private TextBox txtRemarks;
        private ComboBox cboCategory;
        private ComboBox cboStatus;
        private Button btnSave;
        private Button btnDelete;

        // Customer order management controls
        private DataGridView orderGrid;
        private TextBox txtOrderSearch;
        private ComboBox cboOrderFilter;
        private ComboBox cboOrderStatus;
        private TextBox txtAdminNote;
        private Label lblOrderCount;
        private Label lblOrderDetails;

        // Stores currently selected inventory and order IDs
        private int selectedId = 0;
        private int selectedOrderId = 0;

        // Data binding collections for UI updates
        private BindingList<HardwareItem> items = new BindingList<HardwareItem>();
        private BindingList<OrderRecord> orders = new BindingList<OrderRecord>();

        // Master lists used for filtering and searching
        private List<HardwareItem> allItems = new List<HardwareItem>();
        private List<OrderRecord> allOrders = new List<OrderRecord>();

        // Available inventory categories
        private readonly string[] categories =
        {
            "Computer Unit",
            "Peripheral",
            "Network Device",
            "Storage Device",
            "Printer",
            "Power Device",
            "Other Hardware"
        };

        // Available inventory statuses
        private readonly string[] statuses =
        {
            "Available",
            "In Use",
            "Low Stock",
            "For Repair",
            "Defective",
            "Disposed"
        };

        // Filter categories for inventory search
        private readonly string[] filterCategories =
        {
            "All Categories",
            "Computer Unit",
            "Peripheral",
            "Network Device",
            "Storage Device",
            "Printer",
            "Power Device",
            "Other Hardware"
        };

        // Filter statuses for inventory search
        private readonly string[] filterStatuses =
        {
            "All Statuses",
            "Available",
            "In Use",
            "Low Stock",
            "For Repair",
            "Defective",
            "Disposed"
        };

        // Order processing statuses
        private readonly string[] orderStatuses =
        {
            "Pending",
            "Confirmed",
            "Completed",
            "Cancelled",
            "Rejected"
        };

        // Order filter statuses
        private readonly string[] orderFilterStatuses =
        {
            "All Statuses",
            "Pending",
            "Confirmed",
            "Completed",
            "Cancelled",
            "Rejected"
        };

        // Main form constructor
        public MainForm()
        {
            // Set window properties
            Text = "Hardware Inventory Admin";
            Width = 1380;
            Height = 840;
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(1180, 740);

            // Set application font and scaling
            Font = new Font("Segoe UI", 10F);
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoScroll = true;

            // Load background image and login screen
            LoadBackground();
            BuildLoginUi();
        }

        // Loads application background image
        private void LoadBackground()
        {
            string bg = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "assets",
                "hardware_background.png"
            );

            // Check if background image exists
            if (File.Exists(bg))
            {
                BackgroundImage = Image.FromFile(bg);
                BackgroundImageLayout = ImageLayout.Stretch;
            }

            // Default fallback background color
            BackColor = Color.FromArgb(16, 31, 34);
        }

        // Builds the administrator login screen
        private void BuildLoginUi()
        {
            // Clear all existing controls
            Controls.Clear();

            // Reset selected records
            selectedId = 0;
            selectedOrderId = 0;

            // Additional login UI code here...
        }

        // Builds the main administrator dashboard
        private void BuildAdminUi()
        {
            // Clear login screen controls
            Controls.Clear();

            // Additional dashboard UI code here...
        }

        // Builds inventory management tab
        private void BuildInventoryTab(Control parent)
        {
            // Create item editor section
            BuildEditorPanel(parent);

            // Create inventory table section
            BuildTablePanel(parent);
        }

        // Builds inventory item editor form
        private void BuildEditorPanel(Control parent)
        {
            // Panel contains all item input fields and action buttons

            // Additional editor panel code here...
        }

        // Builds inventory records table section
        private void BuildTablePanel(Control parent)
        {
            // Panel contains inventory grid and search filters

            // Additional table panel code here...
        }

        // Builds customer orders management tab
        private void BuildOrdersTab(Control parent)
        {
            // Creates customer order list and review panel

            // Additional order tab code here...
        }

        // Applies consistent styling to all DataGridView controls
        private void StyleGrid(DataGridView dgv)
        {
            // Stop if grid is null
            if (dgv == null)
            {
                return;
            }

            // Disable default Windows header style
            dgv.EnableHeadersVisualStyles = false;

            // Header background color
            dgv.ColumnHeadersDefaultCellStyle.BackColor =
                Color.FromArgb(24, 91, 76);

            // Header text color
            dgv.ColumnHeadersDefaultCellStyle.ForeColor =
                Color.White;

            // Header font style
            dgv.ColumnHeadersDefaultCellStyle.Font =
                new Font(
                    "Segoe UI",
                    10.5F,
                    FontStyle.Bold
                );

            // Row font style
            dgv.DefaultCellStyle.Font =
                new Font("Segoe UI", 10.5F);

            // Set header and row height
            dgv.ColumnHeadersHeight = 38;
            dgv.RowTemplate.Height = 34;

            // Selected row background color
            dgv.DefaultCellStyle.SelectionBackColor =
                Color.FromArgb(202, 241, 228);

            // Selected row text color
            dgv.DefaultCellStyle.SelectionForeColor =
                Color.FromArgb(18, 54, 48);

            // Hide row headers
            dgv.RowHeadersVisible = false;
        }

        // Loads inventory items from API
        private void LoadItems()
        {
            try
            {
                // Retrieve inventory list from server
                allItems = api.ListAdminItems();

                // Apply current filters
                ApplyFilters();

                // Clear form if no items exist
                if (items.Count == 0)
                    ClearForm();
            }
            catch (Exception ex)
            {
                // Show connection or loading error
                MessageBox.Show(
                    "Unable to load inventory. Start the PHP server first.\n\n" + ex.Message,
                    "Server Connection",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        // Loads customer orders from API
        private void LoadOrders()
        {
            try
            {
                // Retrieve order list from server
                allOrders = api.ListOrders();

                // Apply order filters
                ApplyOrderFilters();

                // Reset order selection
                ClearOrderSelectionText();
            }
            catch (Exception ex)
            {
                // Show connection or loading error
                MessageBox.Show(
                    "Unable to load orders. Start the PHP server first.\n\n" + ex.Message,
                    "Server Connection",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        // Filters inventory records based on search and category
        private void ApplyFilters()
        {
            // Stop method if item collection is empty
            if (items == null)
            {
                return;
            }

            // Additional filtering logic here...
        }

        // Filters customer orders based on search and status
        private void ApplyOrderFilters()
        {
            // Stop method if order collection is empty
            if (orders == null)
            {
                return;
            }

            // Additional filtering logic here...
        }

        // Checks if text contains the search keyword
        private bool Contains(string value, string keyword)
        {
            string text = value ?? string.Empty;

            return text
                .ToLowerInvariant()
                .Contains(keyword);
        }

        // Loads selected inventory item into form fields
        private void PopulateFromSelectedRow()
        {
            // Prevent errors if no row is selected
            if (
                grid == null ||
                grid.SelectedRows.Count == 0 ||
                grid.CurrentRow == null ||
                grid.CurrentRow.DataBoundItem == null
            )
                return;

            // Get selected item
            var item =
                (HardwareItem)grid.CurrentRow.DataBoundItem;

            // Populate form fields
            SetForm(item);
        }

        // Loads selected customer order details
        // Loads selected customer order details
        private void PopulateFromSelectedOrder()
        {
            // Check if order table or selected row is invalid
            if (orderGrid == null)
                return;

            if (orderGrid.SelectedRows.Count == 0)
                return;

            if (orderGrid.CurrentRow == null)
                return;

            if (orderGrid.CurrentRow.DataBoundItem == null)
                return;

            // Get selected order
            OrderRecord order =
                (OrderRecord)orderGrid.CurrentRow.DataBoundItem;

            // Additional order selection logic here...
        }

        // Returns safe display text for null or empty values
        private string SafeText(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? "-"
                : value.Trim();
        }

        // Clears currently selected customer order
        private void ClearOrderSelectionText()
        {
            // Reset selected order ID
            selectedOrderId = 0;

            // Additional clearing logic here...
        }

        // Fills form controls with selected inventory item data
        private void SetForm(HardwareItem item)
        {
            // Prevent null reference errors
            if (item == null)
                return;

            // Additional form population logic here...
        }

        // Clears all inventory input fields
        private void ClearForm()
        {
            // Reset selected item ID
            selectedId = 0;

            // Additional clearing logic here...
        }

        // Reads and validates inventory form data
        private HardwareItem ReadForm()
        {
            // Validate quantity input
            int quantity;

            if (
                !int.TryParse(txtQuantity.Text.Trim(), out quantity)
                || quantity < 0
            )
            {
                throw new Exception(
                    "Quantity must be a valid non-negative number."
                );
            }

            // Validate required item name
            string itemName = txtItemName.Text.Trim();

            if (itemName == string.Empty)
            {
                throw new Exception(
                    "Item name is required."
                );
            }

            // Return validated hardware item object
            return new HardwareItem
            {
                id = selectedId,
                item_name = itemName
            };
        }

        // Saves or updates inventory item
        private void SaveItem()
        {
            try
            {
                // Read validated form data
                HardwareItem item = ReadForm();

                // Determine whether to create or update item
                string message =
                    selectedId > 0
                    ? api.UpdateItem(item)
                    : api.AddItem(item);

                // Show success message
                MessageBox.Show(
                    message,
                    "Inventory",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                // Reload inventory data
                LoadItems();

                // Reset form
                ClearForm();
            }
            catch (Exception ex)
            {
                // Show validation or API error
                MessageBox.Show(
                    ex.Message,
                    "Unable to Save",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }
        }

        // Deletes selected inventory item
        private void DeleteSelectedItem()
        {
            // Prevent deletion without selection
            if (selectedId <= 0)
            {
                MessageBox.Show(
                    "Select an item before deleting.",
                    "Delete Item",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                return;
            }

            // Additional delete logic here...
        }

        // Quickly changes order status
        private void QuickOrderStatus(string status)
        {
            // Set combo box value
            if (cboOrderStatus != null)
            {
                SelectComboValue(
                    cboOrderStatus,
                    status,
                    orderStatuses[0]
                );
            }

            // Save updated status
            UpdateSelectedOrder();
        }

        // Updates selected customer order
        private void UpdateSelectedOrder()
        {
            // Prevent update if no order selected
            if (selectedOrderId <= 0)
            {
                MessageBox.Show(
                    "Select an order first.",
                    "Order",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                return;
            }

            // Additional update logic here...
        }

        // Selects a combo box value safely
        private void SelectComboValue(
            ComboBox combo,
            string value,
            string fallback
        )
        {
            // Prevent null reference errors
            if (combo == null)
                return;

            // Additional combo selection logic here...
        }

        // Creates a reusable label control
        private void AddLabel(
            Control parent,
            string text,
            int x,
            int y
        )
        {
            // Add label to parent container
        }

        // Creates a reusable section title label
        private void AddSectionLabel(
            Control parent,
            string text,
            int x,
            int y
        )
        {
            // Add section label to parent container
        }

        // Creates reusable textbox control
        private TextBox MakeTextBox(
            int x,
            int y,
            int w
        )
        {
            return new TextBox
            {
                Location = new Point(x, y),
                Size = new Size(w, 34),
                Font = new Font("Segoe UI", 10.5F)
            };
        }

        // Creates reusable combo box control
        private ComboBox MakeComboBox(
            int x,
            int y,
            int w,
            string[] values
        )
        {
            var combo = new ComboBox
            {
                Location = new Point(x, y),
                Size = new Size(w, 36),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10.5F)
            };

            // Add dropdown values
            combo.Items.AddRange(values);

            // Select first item by default
            if (combo.Items.Count > 0)
                combo.SelectedIndex = 0;

            return combo;
        }

        // Creates reusable styled button
        private Button MakeButton(
            string text,
            int x,
            int y,
            int w,
            Color color
        )
        {
            Button btn = new Button();

            // Set button text
            btn.Text = text;

            // Set button position and size
            btn.Location = new Point(x, y);
            btn.Size = new Size(w, 40);

            // Set button appearance
            btn.BackColor = color;
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;

            // Set font style
            btn.Font = new Font(
                "Segoe UI",
                10F,
                FontStyle.Bold
            );

            // Show hand cursor on hover
            btn.Cursor = Cursors.Hand;

            // Remove border
            btn.FlatAppearance.BorderSize = 0;

            return btn;
        }
    }
}