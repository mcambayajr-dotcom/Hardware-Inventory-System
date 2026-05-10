using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ComputerHardwareStockMonitoringSystem
{
    public class MainForm : Form
    {
        private readonly ApiClient api = new ApiClient();

        private DataGridView grid;
        private TextBox txtSearch;
        private ComboBox cboFilterCategory;
        private ComboBox cboFilterStatus;
        private Label lblCount;
        private Label lblMode;
        private Label lblDetails;

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

        private DataGridView orderGrid;
        private TextBox txtOrderSearch;
        private ComboBox cboOrderFilter;
        private ComboBox cboOrderStatus;
        private TextBox txtAdminNote;
        private Label lblOrderCount;
        private Label lblOrderDetails;

        private int selectedId = 0;
        private int selectedOrderId = 0;
        private BindingList<HardwareItem> items = new BindingList<HardwareItem>();
        private BindingList<OrderRecord> orders = new BindingList<OrderRecord>();
        private List<HardwareItem> allItems = new List<HardwareItem>();
        private List<OrderRecord> allOrders = new List<OrderRecord>();

        private readonly string[] categories = { "Computer Unit", "Peripheral", "Network Device", "Storage Device", "Printer", "Power Device", "Other Hardware" };
        private readonly string[] statuses = { "Available", "In Use", "Low Stock", "For Repair", "Defective", "Disposed" };
        private readonly string[] filterCategories = { "All Categories", "Computer Unit", "Peripheral", "Network Device", "Storage Device", "Printer", "Power Device", "Other Hardware" };
        private readonly string[] filterStatuses = { "All Statuses", "Available", "In Use", "Low Stock", "For Repair", "Defective", "Disposed" };
        private readonly string[] orderStatuses = { "Pending", "Confirmed", "Completed", "Cancelled", "Rejected" };
        private readonly string[] orderFilterStatuses = { "All Statuses", "Pending", "Confirmed", "Completed", "Cancelled", "Rejected" };

        public MainForm()
        {
            Text = "Hardware Inventory Admin";
            Width = 1380;
            Height = 840;
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(1180, 740);
            Font = new Font("Segoe UI", 10F);
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoScroll = true;
            LoadBackground();
            BuildLoginUi();
        }

        private void LoadBackground()
        {
            string bg = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets", "hardware_background.png");
            if (File.Exists(bg))
            {
                BackgroundImage = Image.FromFile(bg);
                BackgroundImageLayout = ImageLayout.Stretch;
            }
            BackColor = Color.FromArgb(16, 31, 34);
        }

        private void BuildLoginUi()
        {
            Controls.Clear();
            selectedId = 0;
            selectedOrderId = 0;

            var title = new Label
            {
                Text = "Inventory Control Desk",
                Font = new Font("Segoe UI", 28, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(42, 42)
            };
            Controls.Add(title);

            var subtitle = new Label
            {
                Text = "Secure access for inventory records and customer orders.",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.FromArgb(220, 255, 246),
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(47, 92)
            };
            Controls.Add(subtitle);

            var loginPanel = new GlassPanel
            {
                Location = new Point(390, 165),
                Size = new Size(520, 365),
                Anchor = AnchorStyles.Top
            };
            Controls.Add(loginPanel);

            var heading = new Label
            {
                Text = "Admin Login",
                Location = new Point(36, 32),
                Size = new Size(390, 36),
                Font = new Font("Segoe UI", 19, FontStyle.Bold),
                ForeColor = Color.FromArgb(18, 54, 48),
                BackColor = Color.Transparent
            };
            loginPanel.Controls.Add(heading);

            AddLabel(loginPanel, "Username", 36, 92);
            var txtUsername = new TextBox { Location = new Point(36, 120), Size = new Size(448, 36), Font = new Font("Segoe UI", 11F) };
            loginPanel.Controls.Add(txtUsername);

            AddLabel(loginPanel, "Password", 36, 160);
            var txtPassword = new TextBox { Location = new Point(36, 194), Size = new Size(448, 36), Font = new Font("Segoe UI", 11F), UseSystemPasswordChar = true };
            loginPanel.Controls.Add(txtPassword);

            var btnLogin = MakeButton("Login", 36, 270, 136, Color.FromArgb(49, 183, 125));
            loginPanel.Controls.Add(btnLogin);

            var btnExit = MakeButton("Exit", 186, 270, 112, Color.FromArgb(102, 128, 123));
            btnExit.Click += (s, e) => Close();
            loginPanel.Controls.Add(btnExit);

            EventHandler loginAction = (s, e) =>
            {
                if (txtUsername.Text.Trim() == AdminSettings.AdminUsername && txtPassword.Text == AdminSettings.AdminPassword)
                {
                    BuildAdminUi();
                    LoadItems();
                    // EARLY STAGE TODO:
                    // Customer order loading will be enabled after the order API is added.
                    // LoadOrders();
                }
                else
                {
                    MessageBox.Show("Invalid username or password.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            };

            btnLogin.Click += loginAction;
            txtPassword.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) loginAction(s, e); };
            txtUsername.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) loginAction(s, e); };
            txtUsername.Focus();
        }

        private void BuildAdminUi()
        {
            Controls.Clear();

            var title = new Label
            {
                Text = "Hardware Inventory Admin",
                Font = new Font("Segoe UI", 23, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(30, 22)
            };
            Controls.Add(title);

            var subtitle = new Label
            {
                Text = "Manage stock records. Customer order management will be added in later commits.",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(220, 255, 246),
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(35, 62)
            };
            Controls.Add(subtitle);

            var btnLogout = MakeButton("Logout", 1215, 34, 115, Color.FromArgb(102, 128, 123));
            btnLogout.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnLogout.Click += (s, e) => BuildLoginUi();
            Controls.Add(btnLogout);

            var tabs = new TabControl
            {
                Location = new Point(30, 100),
                Size = new Size(1300, 655),
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold)
            };
            Controls.Add(tabs);

            var inventoryTab = new TabPage("Inventory CRUD") { BackColor = Color.FromArgb(222, 244, 238), AutoScroll = true };
            tabs.TabPages.Add(inventoryTab);

            BuildInventoryTab(inventoryTab);

            // EARLY STAGE TODO:
            // Add the Customer Orders tab again after completing the order database and API commits.
            // var orderTab = new TabPage("Customer Orders") { BackColor = Color.FromArgb(222, 244, 238), AutoScroll = true };
            // tabs.TabPages.Add(orderTab);
            // BuildOrdersTab(orderTab);
        }

        private void BuildInventoryTab(Control parent)
        {
            BuildEditorPanel(parent);
            BuildTablePanel(parent);
        }

        private void BuildEditorPanel(Control parent)
        {
            var panel = new GlassPanel
            {
                Location = new Point(18, 18),
                Size = new Size(395, 590),
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom
            };
            parent.Controls.Add(panel);

            AddSectionLabel(panel, "Item Details", 24, 20);
            lblMode = new Label
            {
                Text = "New Record",
                Location = new Point(235, 28),
                Size = new Size(95, 22),
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = Color.FromArgb(18, 54, 48),
                BackColor = Color.FromArgb(210, 228, 252),
                TextAlign = ContentAlignment.MiddleCenter
            };
            panel.Controls.Add(lblMode);

            AddLabel(panel, "Item Name", 24, 65);
            txtItemName = MakeTextBox(24, 91, 345);
            panel.Controls.Add(txtItemName);

            AddLabel(panel, "Category", 24, 124);
            cboCategory = MakeComboBox(24, 150, 345, categories);
            panel.Controls.Add(cboCategory);

            AddLabel(panel, "Brand", 24, 183);
            txtBrand = MakeTextBox(24, 209, 162);
            panel.Controls.Add(txtBrand);

            AddLabel(panel, "Model", 207, 183);
            txtModel = MakeTextBox(207, 209, 162);
            panel.Controls.Add(txtModel);

            AddLabel(panel, "Serial Number", 24, 242);
            txtSerial = MakeTextBox(24, 268, 162);
            panel.Controls.Add(txtSerial);

            AddLabel(panel, "Quantity", 207, 242);
            txtQuantity = MakeTextBox(207, 268, 162);
            panel.Controls.Add(txtQuantity);

            AddLabel(panel, "Status", 24, 301);
            cboStatus = MakeComboBox(24, 327, 162, statuses);
            panel.Controls.Add(cboStatus);

            AddLabel(panel, "Location", 207, 301);
            txtLocation = MakeTextBox(207, 327, 162);
            panel.Controls.Add(txtLocation);

            AddLabel(panel, "Remarks", 24, 360);
            txtRemarks = new TextBox
            {
                Location = new Point(24, 384),
                Size = new Size(345, 68),
                Font = new Font("Segoe UI", 10.5F),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };
            panel.Controls.Add(txtRemarks);

            var btnNew = MakeButton("New", 24, 474, 80, Color.FromArgb(88, 115, 112));
            btnNew.Click += (s, e) => ClearForm();
            panel.Controls.Add(btnNew);

            btnSave = MakeButton("Save", 116, 474, 105, Color.FromArgb(49, 183, 125));
            btnSave.Click += (s, e) => SaveItem();
            panel.Controls.Add(btnSave);

            btnDelete = MakeButton("Delete", 233, 474, 112, Color.FromArgb(214, 78, 78));
            btnDelete.Click += (s, e) => DeleteSelectedItem();
            panel.Controls.Add(btnDelete);

            var btnRefresh = MakeButton("Refresh", 24, 526, 321, Color.FromArgb(35, 112, 95));
            btnRefresh.Click += (s, e) => LoadItems();
            panel.Controls.Add(btnRefresh);
        }

        private void BuildTablePanel(Control parent)
        {
            var panel = new GlassPanel
            {
                Location = new Point(430, 18),
                Size = new Size(840, 590),
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom
            };
            parent.Controls.Add(panel);

            AddSectionLabel(panel, "Inventory Records", 24, 20);

            txtSearch = new TextBox { Location = new Point(24, 62), Size = new Size(290, 34), Font = new Font("Segoe UI", 10.5F) };
            txtSearch.TextChanged += (s, e) => ApplyFilters();
            panel.Controls.Add(txtSearch);

            cboFilterCategory = MakeComboBox(328, 62, 190, filterCategories);
            cboFilterCategory.SelectedIndexChanged += (s, e) => ApplyFilters();
            panel.Controls.Add(cboFilterCategory);

            cboFilterStatus = MakeComboBox(532, 62, 170, filterStatuses);
            cboFilterStatus.SelectedIndexChanged += (s, e) => ApplyFilters();
            panel.Controls.Add(cboFilterStatus);

            lblCount = new Label
            {
                Text = "0 record(s)",
                Location = new Point(718, 67),
                Size = new Size(160, 26),
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(18, 54, 48),
                BackColor = Color.Transparent
            };
            panel.Controls.Add(lblCount);

            grid = new DataGridView
            {
                Location = new Point(24, 112),
                Size = new Size(792, 380),
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom,
                AutoGenerateColumns = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                DataSource = items
            };
            StyleGrid(grid);
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "ID", DataPropertyName = "id", Width = 55 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Item", DataPropertyName = "item_name", Width = 185 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Category", DataPropertyName = "category", Width = 135 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Brand", DataPropertyName = "brand", Width = 105 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Model", DataPropertyName = "model", Width = 105 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Qty", DataPropertyName = "quantity", Width = 65 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Status", DataPropertyName = "status", Width = 115 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Location", DataPropertyName = "location", Width = 135 });
            grid.SelectionChanged += (s, e) => PopulateFromSelectedRow();
            panel.Controls.Add(grid);

            lblDetails = new Label
            {
                Text = "Select a record to view details.",
                Location = new Point(24, 508),
                Size = new Size(792, 56),
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(18, 54, 48),
                BackColor = Color.Transparent
            };
            panel.Controls.Add(lblDetails);
        }

        private void BuildOrdersTab(Control parent)
        {
            var panel = new GlassPanel
            {
                Location = new Point(18, 18),
                Size = new Size(1252, 590),
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom
            };
            parent.Controls.Add(panel);

            AddSectionLabel(panel, "Customer Orders", 24, 20);

            txtOrderSearch = new TextBox { Location = new Point(24, 62), Size = new Size(340, 34), Font = new Font("Segoe UI", 10.5F) };
            txtOrderSearch.TextChanged += (s, e) => ApplyOrderFilters();
            panel.Controls.Add(txtOrderSearch);

            cboOrderFilter = MakeComboBox(380, 62, 180, orderFilterStatuses);
            cboOrderFilter.SelectedIndexChanged += (s, e) => ApplyOrderFilters();
            panel.Controls.Add(cboOrderFilter);

            var btnRefreshOrders = MakeButton("Refresh", 575, 58, 110, Color.FromArgb(35, 112, 95));
            btnRefreshOrders.Click += (s, e) => LoadOrders();
            panel.Controls.Add(btnRefreshOrders);

            lblOrderCount = new Label
            {
                Text = "0 order(s)",
                Location = new Point(704, 67),
                Size = new Size(160, 26),
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(18, 54, 48),
                BackColor = Color.Transparent
            };
            panel.Controls.Add(lblOrderCount);

            orderGrid = new DataGridView
            {
                Location = new Point(24, 112),
                Size = new Size(790, 420),
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom,
                AutoGenerateColumns = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                DataSource = orders
            };
            StyleGrid(orderGrid);
            orderGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "ID", DataPropertyName = "id", Width = 55 });
            orderGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Customer", DataPropertyName = "customer_name", Width = 135 });
            orderGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Item", DataPropertyName = "item_name", Width = 160 });
            orderGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Qty", DataPropertyName = "quantity_ordered", Width = 55 });
            orderGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Status", DataPropertyName = "order_status", Width = 95 });
            orderGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Stock", DataPropertyName = "current_stock", Width = 65 });
            orderGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Date", DataPropertyName = "created_at", Width = 145 });
            orderGrid.SelectionChanged += (s, e) => PopulateFromSelectedOrder();
            panel.Controls.Add(orderGrid);

            var actionPanel = new GlassPanel
            {
                Location = new Point(840, 112),
                Size = new Size(370, 420),
                Anchor = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom,
                FillColor = Color.FromArgb(225, 255, 255, 255)
            };
            panel.Controls.Add(actionPanel);

            AddSectionLabel(actionPanel, "Order Review", 22, 18);

            lblOrderDetails = new Label
            {
                Text = "Select an order to review customer details.",
                Location = new Point(22, 62),
                Size = new Size(320, 118),
                Font = new Font("Segoe UI", 10.5F),
                ForeColor = Color.FromArgb(18, 54, 48),
                BackColor = Color.Transparent
            };
            actionPanel.Controls.Add(lblOrderDetails);

            AddLabel(actionPanel, "Set Status", 22, 178);
            cboOrderStatus = MakeComboBox(22, 206, 320, orderStatuses);
            actionPanel.Controls.Add(cboOrderStatus);

            AddLabel(actionPanel, "Admin Note", 22, 238);
            txtAdminNote = new TextBox
            {
                Location = new Point(22, 270),
                Size = new Size(320, 70),
                Font = new Font("Segoe UI", 10.5F),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };
            actionPanel.Controls.Add(txtAdminNote);

            var btnApply = MakeButton("Update Order", 22, 356, 160, Color.FromArgb(49, 183, 125));
            btnApply.Click += (s, e) => UpdateSelectedOrder();
            actionPanel.Controls.Add(btnApply);

            var btnConfirm = MakeButton("Confirm", 198, 356, 115, Color.FromArgb(35, 112, 95));
            btnConfirm.Click += (s, e) => QuickOrderStatus("Confirmed");
            actionPanel.Controls.Add(btnConfirm);
        }

        private void StyleGrid(DataGridView dgv)
        {
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(24, 91, 76);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 10.5F);
            dgv.ColumnHeadersHeight = 38;
            dgv.RowTemplate.Height = 34;
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(202, 241, 228);
            dgv.DefaultCellStyle.SelectionForeColor = Color.FromArgb(18, 54, 48);
            dgv.RowHeadersVisible = false;
        }

        private void LoadItems()
        {
            try
            {
                allItems = api.ListAdminItems();
                ApplyFilters();
                if (items.Count == 0)
                    ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to load inventory. Start the PHP server first.\n\n" + ex.Message, "Server Connection", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadOrders()
        {
            try
            {
                allOrders = api.ListOrders();
                ApplyOrderFilters();
                ClearOrderSelectionText();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to load orders. Start the PHP server first.\n\n" + ex.Message, "Server Connection", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApplyFilters()
        {
            if (items == null) return;

            string keyword = txtSearch == null ? string.Empty : txtSearch.Text.Trim().ToLowerInvariant();
            string category = cboFilterCategory == null || cboFilterCategory.SelectedIndex <= 0 ? string.Empty : cboFilterCategory.Text;
            string status = cboFilterStatus == null || cboFilterStatus.SelectedIndex <= 0 ? string.Empty : cboFilterStatus.Text;

            var filtered = allItems.Where(item =>
                (keyword == string.Empty || Contains(item.item_name, keyword) || Contains(item.category, keyword) || Contains(item.brand, keyword) || Contains(item.model, keyword) || Contains(item.serial_number, keyword) || Contains(item.location, keyword)) &&
                (category == string.Empty || item.category == category) &&
                (status == string.Empty || item.status == status)
            ).ToList();

            items.Clear();
            foreach (var item in filtered) items.Add(item);
            if (lblCount != null) lblCount.Text = items.Count + " record(s)";
        }

        private void ApplyOrderFilters()
        {
            if (orders == null) return;

            string keyword = txtOrderSearch == null ? string.Empty : txtOrderSearch.Text.Trim().ToLowerInvariant();
            string status = cboOrderFilter == null || cboOrderFilter.SelectedIndex <= 0 ? string.Empty : cboOrderFilter.Text;

            var filtered = allOrders.Where(order =>
                (keyword == string.Empty || Contains(order.customer_name, keyword) || Contains(order.customer_username, keyword) || Contains(order.customer_email, keyword) || Contains(order.item_name, keyword) || Contains(order.brand, keyword) || Contains(order.model, keyword)) &&
                (status == string.Empty || order.order_status == status)
            ).ToList();

            orders.Clear();
            foreach (var order in filtered) orders.Add(order);
            if (lblOrderCount != null) lblOrderCount.Text = orders.Count + " order(s)";
        }

        private bool Contains(string value, string keyword)
        {
            return (value ?? string.Empty).ToLowerInvariant().Contains(keyword);
        }

        private void PopulateFromSelectedRow()
        {
            if (grid == null || grid.SelectedRows.Count == 0 || grid.CurrentRow == null || grid.CurrentRow.DataBoundItem == null) return;
            var item = (HardwareItem)grid.CurrentRow.DataBoundItem;
            SetForm(item);
        }

        private void PopulateFromSelectedOrder()
        {
            if (orderGrid == null || orderGrid.SelectedRows.Count == 0 || orderGrid.CurrentRow == null || orderGrid.CurrentRow.DataBoundItem == null) return;
            var order = (OrderRecord)orderGrid.CurrentRow.DataBoundItem;
            selectedOrderId = order.id;
            SelectComboValue(cboOrderStatus, order.order_status, orderStatuses[0]);
            txtAdminNote.Text = order.admin_note ?? string.Empty;
            lblOrderDetails.Text =
                "Order #" + order.id + "\n" +
                "Customer: " + SafeText(order.customer_name) + " (" + SafeText(order.customer_username) + ")\n" +
                "Item: " + SafeText(order.item_name) + "\n" +
                "Quantity: " + order.quantity_ordered + " | Stock: " + order.current_stock + "\n" +
                "Customer Note: " + SafeText(order.customer_note);
        }

        private string SafeText(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "-" : value.Trim();
        }

        private void ClearOrderSelectionText()
        {
            selectedOrderId = 0;
            if (lblOrderDetails != null) lblOrderDetails.Text = "Select an order to review customer details.";
            if (txtAdminNote != null) txtAdminNote.Clear();
            if (cboOrderStatus != null && cboOrderStatus.Items.Count > 0) cboOrderStatus.SelectedIndex = 0;
            if (orderGrid != null) orderGrid.ClearSelection();
        }

        private void SetForm(HardwareItem item)
        {
            if (item == null) return;
            selectedId = item.id;
            txtItemName.Text = item.item_name ?? string.Empty;
            SelectComboValue(cboCategory, item.category, categories[0]);
            txtBrand.Text = item.brand ?? string.Empty;
            txtModel.Text = item.model ?? string.Empty;
            txtSerial.Text = item.serial_number ?? string.Empty;
            txtQuantity.Text = item.quantity.ToString();
            SelectComboValue(cboStatus, item.status, statuses[0]);
            txtLocation.Text = item.location ?? string.Empty;
            txtRemarks.Text = item.remarks ?? string.Empty;
            lblMode.Text = "Editing";
            btnSave.Text = "Update";
            lblDetails.Text = "Selected: " + item.item_name + " | " + item.category + " | " + item.status + " | Qty: " + item.quantity + " | Updated: " + item.updated_at;
        }

        private void ClearForm()
        {
            selectedId = 0;
            if (txtItemName != null) txtItemName.Clear();
            if (cboCategory != null) cboCategory.SelectedIndex = 0;
            if (txtBrand != null) txtBrand.Clear();
            if (txtModel != null) txtModel.Clear();
            if (txtSerial != null) txtSerial.Clear();
            if (txtQuantity != null) txtQuantity.Text = "0";
            if (cboStatus != null) cboStatus.SelectedIndex = 0;
            if (txtLocation != null) txtLocation.Clear();
            if (txtRemarks != null) txtRemarks.Clear();
            if (lblMode != null) lblMode.Text = "New Record";
            if (btnSave != null) btnSave.Text = "Save";
            if (lblDetails != null) lblDetails.Text = "Enter item details or select a record from the table.";
            if (grid != null) grid.ClearSelection();
            if (txtItemName != null) txtItemName.Focus();
        }

        private HardwareItem ReadForm()
        {
            int quantity;
            if (!int.TryParse(txtQuantity.Text.Trim(), out quantity) || quantity < 0)
            {
                throw new Exception("Quantity must be a valid non-negative number.");
            }

            string itemName = txtItemName.Text.Trim();
            if (itemName == string.Empty)
            {
                throw new Exception("Item name is required.");
            }

            return new HardwareItem
            {
                id = selectedId,
                item_name = itemName,
                category = cboCategory.Text,
                brand = txtBrand.Text.Trim(),
                model = txtModel.Text.Trim(),
                serial_number = txtSerial.Text.Trim(),
                quantity = quantity,
                status = cboStatus.Text,
                location = txtLocation.Text.Trim(),
                remarks = txtRemarks.Text.Trim()
            };
        }

        private void SaveItem()
        {
            try
            {
                HardwareItem item = ReadForm();
                string message = selectedId > 0 ? api.UpdateItem(item) : api.AddItem(item);
                MessageBox.Show(message, "Inventory", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadItems();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Unable to Save", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void DeleteSelectedItem()
        {
            if (selectedId <= 0)
            {
                MessageBox.Show("Select an item before deleting.", "Delete Item", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var confirm = MessageBox.Show("Delete the selected inventory record?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            try
            {
                string message = api.DeleteItem(selectedId);
                MessageBox.Show(message, "Inventory", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadItems();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Unable to Delete", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void QuickOrderStatus(string status)
        {
            if (cboOrderStatus != null) SelectComboValue(cboOrderStatus, status, orderStatuses[0]);
            UpdateSelectedOrder();
        }

        private void UpdateSelectedOrder()
        {
            if (selectedOrderId <= 0)
            {
                MessageBox.Show("Select an order first.", "Order", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                string message = api.UpdateOrderStatus(selectedOrderId, cboOrderStatus.Text, txtAdminNote.Text.Trim());
                MessageBox.Show(message, "Order", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadOrders();
                LoadItems();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Unable to Update Order", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void SelectComboValue(ComboBox combo, string value, string fallback)
        {
            if (combo == null) return;
            string target = string.IsNullOrWhiteSpace(value) ? fallback : value;
            if (combo.Items.Contains(target))
                combo.SelectedItem = target;
            else if (combo.Items.Count > 0)
                combo.SelectedIndex = 0;
        }

        private void AddLabel(Control parent, string text, int x, int y)
        {
            parent.Controls.Add(new Label
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(180, 24),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(18, 54, 48),
                BackColor = Color.Transparent
            });
        }

        private void AddSectionLabel(Control parent, string text, int x, int y)
        {
            parent.Controls.Add(new Label
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(300, 36),
                Font = new Font("Segoe UI", 17F, FontStyle.Bold),
                ForeColor = Color.FromArgb(18, 54, 48),
                BackColor = Color.Transparent
            });
        }

        private TextBox MakeTextBox(int x, int y, int w)
        {
            return new TextBox { Location = new Point(x, y), Size = new Size(w, 34), Font = new Font("Segoe UI", 10.5F) };
        }

        private ComboBox MakeComboBox(int x, int y, int w, string[] values)
        {
            var combo = new ComboBox { Location = new Point(x, y), Size = new Size(w, 36), DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10.5F), IntegralHeight = false, DropDownHeight = 180 };
            combo.Items.AddRange(values);
            if (combo.Items.Count > 0) combo.SelectedIndex = 0;
            return combo;
        }

        private Button MakeButton(string text, int x, int y, int w, Color color)
        {
            var btn = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(w, 40),
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }
    }
}
