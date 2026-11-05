using System.ComponentModel;
using bai_6._1.BUS;
using bai_6._1.DAL;
using bai_6._1.DTO;

namespace bai_6._1
{
    public partial class Form1 : Form
    {
        private readonly RetailBus _bus = new();
        private readonly List<CartItemDto> _cart = new();
        private DataGridView _productGrid = null!;
        private DataGridView _cartGrid = null!;
        private DataGridView _topProductsGrid = null!;
        private ComboBox _employeePicker = null!;
        private ComboBox _customerPicker = null!;
        private NumericUpDown _quantityInput = null!;
        private TextBox _saleSearch = null!;
        private Label _cartTotal = null!;
        private Label _todayRevenue = null!;
        private Label _monthRevenue = null!;

        public Form1()
        {
            InitializeComponent();
            Text = "Hệ thống quản lý bán hàng siêu thị mini";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(1000, 650);
            ClientSize = new Size(1280, 780);
            mainTabs.TabPages.Clear();
            mainTabs.TabPages.Add(BuildCatalogTab());
            mainTabs.TabPages.Add(BuildCheckoutTab());
            mainTabs.TabPages.Add(BuildReportsTab());
            Load += (_, _) => RefreshAll();
        }

        private TabPage BuildCatalogTab()
        {
            var page = new TabPage("Quản lý danh mục");
            var catalogTabs = new TabControl { Dock = DockStyle.Fill };
            catalogTabs.TabPages.Add(BuildProductsTab());
            catalogTabs.TabPages.Add(BuildCustomersTab());
            catalogTabs.TabPages.Add(BuildEmployeesTab());
            page.Controls.Add(catalogTabs);
            return page;
        }

        private TabPage BuildProductsTab()
        {
            var page = new TabPage("Sản phẩm");
            var search = new TextBox { Width = 260, PlaceholderText = "Nhập tên hoặc mã sản phẩm" };
            var grid = CreateGrid();
            var code = CreateField("Mã sản phẩm", 150);
            var name = CreateField("Tên sản phẩm", 230);
            var price = CreateField("Giá bán", 120);
            var stock = CreateField("Số lượng tồn", 100);
            var state = new CrudState();
            var body = CreateCrudBody(grid, new[] { code, name, price, stock });
            var searchBar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 48, Padding = new Padding(8), WrapContents = false };
            searchBar.Controls.Add(new Label { Text = "Tìm kiếm", AutoSize = true, Padding = new Padding(0, 8, 0, 0) });
            searchBar.Controls.Add(search);
            searchBar.Controls.Add(CreateButton("Tìm", (_, _) => RunSafe(() => LoadProducts(grid, search.Text))));
            searchBar.Controls.Add(CreateButton("Làm mới", (_, _) => RunSafe(() => { search.Clear(); LoadProducts(grid); })));
            page.Controls.Add(body);
            page.Controls.Add(searchBar);

            grid.SelectionChanged += (_, _) =>
            {
                if (grid.SelectedRows.Count == 0 || grid.CurrentRow?.DataBoundItem is not ProductDto item) return;
                state.IsNew = false;
                code.Text = item.MaSP;
                code.ReadOnly = true;
                name.Text = item.TenSP;
                price.Text = item.GiaBan.ToString("0.00");
                stock.Text = item.SoLuongTon.ToString();
            };
            AddCrudButtons(body, () =>
            {
                state.IsNew = true;
                grid.ClearSelection();
                code.Clear(); code.ReadOnly = false; name.Clear(); price.Clear(); stock.Clear();
                code.Focus();
            }, () => RunSafe(() =>
            {
                if (!decimal.TryParse(price.Text, out var parsedPrice) || !int.TryParse(stock.Text, out var parsedStock))
                    throw new ArgumentException("Vui lòng nhập giá bán và số lượng tồn hợp lệ.");
                _bus.SaveProduct(new ProductDto { MaSP = code.Text, TenSP = name.Text, GiaBan = parsedPrice, SoLuongTon = parsedStock }, state.IsNew);
                LoadProducts(grid, search.Text);
                ResetProductEditor();
            }), () => RunSafe(() =>
            {
                if (grid.CurrentRow?.DataBoundItem is not ProductDto item) throw new InvalidOperationException("Vui lòng chọn sản phẩm cần xóa.");
                _bus.DeleteProduct(item.MaSP);
                LoadProducts(grid, search.Text);
                ResetProductEditor();
            }));
            void ResetProductEditor() { state.IsNew = true; code.Clear(); code.ReadOnly = false; name.Clear(); price.Clear(); stock.Clear(); }
            return page;
        }

        private TabPage BuildCustomersTab()
        {
            var page = new TabPage("Khách hàng");
            var grid = CreateGrid();
            var name = CreateField("Tên khách hàng", 240);
            var phone = CreateField("Số điện thoại", 170);
            var points = CreateField("Điểm tích lũy", 130);
            var state = new CrudState();
            var body = CreateCrudBody(grid, new[] { name, phone, points });
            page.Controls.Add(body);
            grid.SelectionChanged += (_, _) =>
            {
                if (grid.SelectedRows.Count == 0 || grid.CurrentRow?.DataBoundItem is not CustomerDto item) return;
                state.IsNew = false;
                name.Text = item.TenKH; phone.Text = item.DienThoai ?? string.Empty; points.Text = item.DiemTichLuy.ToString();
            };
            AddCrudButtons(body, () =>
            {
                state.IsNew = true; grid.ClearSelection(); name.Clear(); phone.Clear(); points.Text = "0"; name.Focus();
            }, () => RunSafe(() =>
            {
                if (!int.TryParse(points.Text, out var parsedPoints)) throw new ArgumentException("Vui lòng nhập điểm tích lũy hợp lệ.");
                var item = new CustomerDto { TenKH = name.Text, DienThoai = phone.Text, DiemTichLuy = parsedPoints };
                if (!state.IsNew && grid.CurrentRow?.DataBoundItem is CustomerDto selected) item.MaKH = selected.MaKH;
                _bus.SaveCustomer(item, state.IsNew);
                LoadCustomers(grid); Reset(); RefreshPickers();
            }), () => RunSafe(() =>
            {
                if (grid.CurrentRow?.DataBoundItem is not CustomerDto item) throw new InvalidOperationException("Vui lòng chọn khách hàng cần xóa.");
                _bus.DeleteCustomer(item.MaKH); LoadCustomers(grid); Reset(); RefreshPickers();
            }));
            void Reset() { state.IsNew = true; name.Clear(); phone.Clear(); points.Text = "0"; }
            return page;
        }

        private TabPage BuildEmployeesTab()
        {
            var page = new TabPage("Nhân viên");
            var grid = CreateGrid();
            var name = CreateField("Tên nhân viên", 240);
            var phone = CreateField("Số điện thoại", 170);
            var role = CreateField("Chức vụ", 170);
            var state = new CrudState();
            var body = CreateCrudBody(grid, new[] { name, phone, role });
            page.Controls.Add(body);
            grid.SelectionChanged += (_, _) =>
            {
                if (grid.SelectedRows.Count == 0 || grid.CurrentRow?.DataBoundItem is not EmployeeDto item) return;
                state.IsNew = false;
                name.Text = item.TenNV; phone.Text = item.SoDienThoai ?? string.Empty; role.Text = item.ChucVu ?? string.Empty;
            };
            AddCrudButtons(body, () =>
            {
                state.IsNew = true; grid.ClearSelection(); name.Clear(); phone.Clear(); role.Clear(); name.Focus();
            }, () => RunSafe(() =>
            {
                var item = new EmployeeDto { TenNV = name.Text, SoDienThoai = phone.Text, ChucVu = role.Text };
                if (!state.IsNew && grid.CurrentRow?.DataBoundItem is EmployeeDto selected) item.MaNV = selected.MaNV;
                _bus.SaveEmployee(item, state.IsNew);
                LoadEmployees(grid); Reset(); RefreshPickers();
            }), () => RunSafe(() =>
            {
                if (grid.CurrentRow?.DataBoundItem is not EmployeeDto item) throw new InvalidOperationException("Vui lòng chọn nhân viên cần xóa.");
                _bus.DeleteEmployee(item.MaNV); LoadEmployees(grid); Reset(); RefreshPickers();
            }));
            void Reset() { state.IsNew = true; name.Clear(); phone.Clear(); role.Clear(); }
            return page;
        }

        private TabPage BuildCheckoutTab()
        {
            var page = new TabPage("Bán hàng");
            var selectors = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 48, Padding = new Padding(8), WrapContents = false };
            _employeePicker = new ComboBox { Width = 230, DropDownStyle = ComboBoxStyle.DropDownList };
            _customerPicker = new ComboBox { Width = 230, DropDownStyle = ComboBoxStyle.DropDownList };
            selectors.Controls.Add(new Label { Text = "Nhân viên", AutoSize = true, Padding = new Padding(0, 7, 0, 0) });
            selectors.Controls.Add(_employeePicker);
            selectors.Controls.Add(new Label { Text = "Khách hàng", AutoSize = true, Padding = new Padding(0, 7, 0, 0) });
            selectors.Controls.Add(_customerPicker);

            var addBar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 48, Padding = new Padding(8), WrapContents = false };
            _saleSearch = new TextBox { Width = 240, PlaceholderText = "Tìm theo tên hoặc mã sản phẩm" };
            _quantityInput = new NumericUpDown { Width = 80, Minimum = 1, Maximum = 100000, Value = 1 };
            addBar.Controls.Add(_saleSearch);
            addBar.Controls.Add(CreateButton("Tìm", (_, _) => RunSafe(() => LoadProducts(_productGrid, _saleSearch.Text))));
            addBar.Controls.Add(new Label { Text = "Số lượng", AutoSize = true, Padding = new Padding(0, 7, 0, 0) });
            addBar.Controls.Add(_quantityInput);
            addBar.Controls.Add(CreateButton("Thêm vào giỏ", (_, _) => AddSelectedProduct()));

            _productGrid = CreateGrid();
            _cartGrid = CreateGrid();
            var split = new SplitContainer { Dock = DockStyle.Fill, Size = new Size(1000, 600), Orientation = Orientation.Vertical, SplitterDistance = 650 };
            split.Panel1.Controls.Add(_productGrid);
            split.Panel2.Controls.Add(_cartGrid);
            var checkoutBar = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 55, Padding = new Padding(8), WrapContents = false };
            _cartTotal = new Label { Text = "Tổng tiền: 0", AutoSize = true, Font = new Font(Font, FontStyle.Bold), Padding = new Padding(0, 9, 20, 0) };
            checkoutBar.Controls.Add(_cartTotal);
            checkoutBar.Controls.Add(CreateButton("Xóa sản phẩm", (_, _) => RemoveSelectedCartItem()));
            checkoutBar.Controls.Add(CreateButton("Xóa giỏ hàng", (_, _) => { _cart.Clear(); RefreshCart(); }));
            checkoutBar.Controls.Add(CreateButton("Thanh toán", (_, _) => Checkout()));
            page.Controls.Add(split);
            page.Controls.Add(checkoutBar);
            page.Controls.Add(addBar);
            page.Controls.Add(selectors);
            RefreshCart();
            return page;
        }

        private TabPage BuildReportsTab()
        {
            var page = new TabPage("Báo cáo");
            var panel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = new Padding(12) };
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 55));
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 55));
            panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            var header = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
            _todayRevenue = new Label { Text = "Doanh thu hôm nay: —", AutoSize = true, Padding = new Padding(0, 10, 28, 0), Font = new Font(Font, FontStyle.Bold) };
            _monthRevenue = new Label { Text = "Doanh thu tháng này: —", AutoSize = true, Padding = new Padding(0, 10, 28, 0), Font = new Font(Font, FontStyle.Bold) };
            header.Controls.Add(_todayRevenue);
            header.Controls.Add(_monthRevenue);
            header.Controls.Add(CreateButton("Cập nhật báo cáo", (_, _) => RunSafe(LoadReports)));
            _topProductsGrid = CreateGrid();
            panel.Controls.Add(header, 0, 0);
            panel.Controls.Add(new Label { Text = "Top 5 sản phẩm bán chạy", AutoSize = true, Padding = new Padding(0, 14, 0, 0) }, 0, 1);
            panel.Controls.Add(_topProductsGrid, 0, 2);
            page.Controls.Add(panel);
            return page;
        }

        private void RefreshAll()
        {
            RunSafe(() =>
            {
                LoadProducts(_productGrid);
                var catalogProducts = FindCatalogGrid("Sản phẩm");
                if (catalogProducts is not null) LoadProducts(catalogProducts);
                LoadCustomersFromCatalog();
                LoadEmployeesFromCatalog();
                RefreshPickers();
                LoadReports();
            });
        }

        private void LoadProducts(DataGridView grid, string? search = null)
        {
            grid.DataSource = _bus.SearchProducts(search);
            SetColumnHeader(grid, nameof(ProductDto.MaSP), "Mã sản phẩm");
            SetColumnHeader(grid, nameof(ProductDto.TenSP), "Tên sản phẩm");
            SetColumnHeader(grid, nameof(ProductDto.GiaBan), "Giá bán");
            SetColumnHeader(grid, nameof(ProductDto.SoLuongTon), "Số lượng tồn");
            if (grid == _productGrid) ConfigureCheckoutProductsGrid();
        }

        private void LoadCustomers(DataGridView grid)
        {
            grid.DataSource = _bus.GetCustomers();
            SetColumnHeader(grid, nameof(CustomerDto.MaKH), "Mã khách hàng");
            SetColumnHeader(grid, nameof(CustomerDto.TenKH), "Tên khách hàng");
            SetColumnHeader(grid, nameof(CustomerDto.DienThoai), "Số điện thoại");
            SetColumnHeader(grid, nameof(CustomerDto.DiemTichLuy), "Điểm tích lũy");
        }

        private void LoadEmployees(DataGridView grid)
        {
            grid.DataSource = _bus.GetEmployees();
            SetColumnHeader(grid, nameof(EmployeeDto.MaNV), "Mã nhân viên");
            SetColumnHeader(grid, nameof(EmployeeDto.TenNV), "Tên nhân viên");
            SetColumnHeader(grid, nameof(EmployeeDto.SoDienThoai), "Số điện thoại");
            SetColumnHeader(grid, nameof(EmployeeDto.ChucVu), "Chức vụ");
        }

        private void LoadCustomersFromCatalog()
        {
            var grid = FindCatalogGrid("Khách hàng");
            if (grid is not null) LoadCustomers(grid);
        }

        private void LoadEmployeesFromCatalog()
        {
            var grid = FindCatalogGrid("Nhân viên");
            if (grid is not null) LoadEmployees(grid);
        }

        private DataGridView? FindCatalogGrid(string tabName)
        {
            var root = Controls.OfType<TabControl>().FirstOrDefault();
            var catalog = root?.TabPages.Cast<TabPage>().FirstOrDefault(page => page.Text == "Quản lý danh mục")?.Controls.OfType<TabControl>().FirstOrDefault();
            var tab = catalog?.TabPages.Cast<TabPage>().FirstOrDefault(page => page.Text == tabName);
            return tab?.Controls.OfType<SplitContainer>().SelectMany(split => split.Panel1.Controls.OfType<DataGridView>()).FirstOrDefault()
                ?? tab?.Controls.OfType<DataGridView>().FirstOrDefault();
        }

        private void RefreshPickers()
        {
            if (_employeePicker is null) return;
            var employeeChoices = _bus.GetEmployees().Select(item => new PickerChoice(item.TenNV, item.MaNV)).ToList();
            _employeePicker.DataSource = employeeChoices;
            _employeePicker.DisplayMember = nameof(PickerChoice.Name);
            _employeePicker.ValueMember = nameof(PickerChoice.Id);
            var customerChoices = new List<PickerChoice> { new("Khách lẻ", null) };
            customerChoices.AddRange(_bus.GetCustomers().Select(item => new PickerChoice(item.TenKH, item.MaKH)));
            _customerPicker.DataSource = customerChoices;
            _customerPicker.DisplayMember = nameof(PickerChoice.Name);
            _customerPicker.ValueMember = nameof(PickerChoice.Id);
        }

        private void LoadReports()
        {
            var summary = _bus.GetSalesSummary();
            _todayRevenue.Text = $"Doanh thu hôm nay: {summary.TodayRevenue:N2}";
            _monthRevenue.Text = $"Doanh thu tháng này: {summary.MonthRevenue:N2}";
            _topProductsGrid.DataSource = _bus.GetTopProducts();
            SetColumnHeader(_topProductsGrid, nameof(TopProductDto.MaSP), "Mã sản phẩm");
            SetColumnHeader(_topProductsGrid, nameof(TopProductDto.TenSP), "Tên sản phẩm");
            SetColumnHeader(_topProductsGrid, nameof(TopProductDto.QuantitySold), "Số lượng đã bán");
            SetColumnHeader(_topProductsGrid, nameof(TopProductDto.Revenue), "Doanh thu");
        }

        private void AddSelectedProduct()
        {
            if (_productGrid.SelectedRows.Count == 0 || _productGrid.CurrentRow?.DataBoundItem is not ProductDto product)
            {
                MessageBox.Show(this, "Vui lòng chọn sản phẩm.", "Bán hàng", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var quantity = (int)_quantityInput.Value;
            var existing = _cart.FirstOrDefault(item => item.Product.MaSP == product.MaSP);
            if (existing is null)
                _cart.Add(new CartItemDto { Product = product, Quantity = quantity });
            else
                existing.Quantity += quantity;
            RefreshCart();
        }

        private void RefreshCart()
        {
            if (_cartGrid is not null)
            {
                _cartGrid.DataSource = null;
                _cartGrid.DataSource = _cart.Select(item => new CartGridRow(item.Product.MaSP, item.Product.TenSP, item.Product.GiaBan, item.Quantity, item.LineTotal)).ToList();
                SetColumnHeader(_cartGrid, nameof(CartGridRow.ProductCode), "Mã sản phẩm");
                SetColumnHeader(_cartGrid, nameof(CartGridRow.ProductName), "Tên sản phẩm");
                SetColumnHeader(_cartGrid, nameof(CartGridRow.UnitPrice), "Đơn giá");
                SetColumnHeader(_cartGrid, nameof(CartGridRow.Quantity), "Số lượng");
                SetColumnHeader(_cartGrid, nameof(CartGridRow.LineTotal), "Thành tiền");
            }
            if (_cartTotal is not null)
                _cartTotal.Text = $"Tổng tiền: {_cart.Sum(item => item.LineTotal):N2}";
        }

        private void RemoveSelectedCartItem()
        {
            if (_cartGrid.CurrentRow?.DataBoundItem is not CartGridRow row) return;
            _cart.RemoveAll(item => item.Product.MaSP == row.ProductCode);
            RefreshCart();
        }

        private void Checkout()
        {
            RunSafe(() =>
            {
                var employeeId = _employeePicker.SelectedValue is int id ? id : 0;
                int? customerId = _customerPicker.SelectedValue is int selectedCustomer ? selectedCustomer : null;
                var invoiceId = _bus.Checkout(employeeId, customerId, _cart.ToList());
                MessageBox.Show(this, $"Tạo hóa đơn số {invoiceId} thành công.", "Bán hàng", MessageBoxButtons.OK, MessageBoxIcon.Information);
                _cart.Clear();
                RefreshCart();
                LoadProducts(_productGrid, _saleSearch.Text);
                RefreshPickers();
                LoadReports();
                LoadCustomersFromCatalog();
            });
        }

        private void ConfigureCheckoutProductsGrid()
        {
            if (_productGrid.Columns[nameof(ProductDto.GiaBan)] is not null)
                _productGrid.Columns[nameof(ProductDto.GiaBan)].DefaultCellStyle.Format = "N2";
        }

        private static void SetColumnHeader(DataGridView grid, string columnName, string headerText)
        {
            if (grid.Columns[columnName] is DataGridViewColumn column)
                column.HeaderText = headerText;
        }

        private static DataGridView CreateGrid() => new()
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            RowHeadersVisible = false
        };

        private static SplitContainer CreateCrudBody(DataGridView grid, IEnumerable<TextBox> fields)
        {
            var body = new SplitContainer { Dock = DockStyle.Fill, Size = new Size(1000, 600), Orientation = Orientation.Horizontal, SplitterDistance = 470, FixedPanel = FixedPanel.Panel2 };
            body.Panel1.Controls.Add(grid);
            var editor = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(8), WrapContents = true, AutoScroll = true };
            foreach (var field in fields)
            {
                var holder = (Control?)field.Tag!;
                editor.Controls.Add(holder);
            }
            body.Panel2.Controls.Add(editor);
            return body;
        }

        private static TextBox CreateField(string label, int width)
        {
            var input = new TextBox { Width = width, Tag = null };
            var holder = new FlowLayoutPanel { Width = width + 15, Height = 62, FlowDirection = FlowDirection.TopDown, WrapContents = false };
            holder.Controls.Add(new Label { Text = label, AutoSize = true });
            holder.Controls.Add(input);
            input.Tag = holder;
            return input;
        }

        private static void AddCrudButtons(SplitContainer body, Action add, Action save, Action delete)
        {
            var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 42, Padding = new Padding(8, 0, 0, 0), WrapContents = false };
            buttons.Controls.Add(CreateButton("Thêm mới", (_, _) => add()));
            buttons.Controls.Add(CreateButton("Lưu", (_, _) => save()));
            buttons.Controls.Add(CreateButton("Xóa", (_, _) => delete()));
            body.Panel2.Controls.OfType<FlowLayoutPanel>().First().Controls.Add(buttons);
        }

        private static Button CreateButton(string text, EventHandler click)
        {
            var button = new Button { Text = text, AutoSize = true, Height = 30, Margin = new Padding(5, 2, 5, 2) };
            button.Click += click;
            return button;
        }

        private static void RunSafe(Action action)
        {
            try
            {
                action();
            }
            catch (Exception exception)
            {
                MessageBox.Show(Database.GetUserFriendlyError(exception), "Hệ thống bán hàng", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private sealed class CrudState
        {
            public bool IsNew { get; set; } = true;
        }

        private sealed record PickerChoice(string Name, int? Id);
        private sealed record CartGridRow(string ProductCode, string ProductName, decimal UnitPrice, int Quantity, decimal LineTotal);
    }
}
