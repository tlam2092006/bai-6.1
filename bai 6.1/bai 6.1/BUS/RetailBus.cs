using bai_6._1.DAL;
using bai_6._1.DTO;

namespace bai_6._1.BUS;

public sealed class RetailBus
{
    private readonly RetailDal _dal = new();

    public List<ProductDto> SearchProducts(string? query = null) => _dal.GetProducts(query);

    public void SaveProduct(ProductDto product, bool isNew)
    {
        if (string.IsNullOrWhiteSpace(product.MaSP) || product.MaSP.Trim().Length > 20)
            throw new ArgumentException("Mã sản phẩm là bắt buộc và không được vượt quá 20 ký tự.");
        if (string.IsNullOrWhiteSpace(product.TenSP) || product.TenSP.Trim().Length > 150)
            throw new ArgumentException("Tên sản phẩm là bắt buộc và không được vượt quá 150 ký tự.");
        if (product.GiaBan <= 0 || product.GiaBan > 9999999999999999.99m)
            throw new ArgumentException("Giá bán phải lớn hơn 0.");
        if (product.SoLuongTon < 0)
            throw new ArgumentException("Số lượng tồn không được âm.");

        product.MaSP = product.MaSP.Trim();
        product.TenSP = product.TenSP.Trim();
        if (_dal.SaveProduct(product, isNew) != 1)
            throw new InvalidOperationException("Không tìm thấy sản phẩm hoặc không thể lưu thông tin.");
    }

    public void DeleteProduct(string productId)
    {
        if (_dal.DeleteProduct(productId) != 1)
            throw new InvalidOperationException("Không tìm thấy sản phẩm.");
    }

    public List<CustomerDto> GetCustomers() => _dal.GetCustomers();

    public void SaveCustomer(CustomerDto customer, bool isNew)
    {
        if (string.IsNullOrWhiteSpace(customer.TenKH) || customer.TenKH.Trim().Length > 100)
            throw new ArgumentException("Tên khách hàng là bắt buộc và không được vượt quá 100 ký tự.");
        ValidatePhone(customer.DienThoai);
        if (customer.DiemTichLuy < 0)
            throw new ArgumentException("Điểm tích lũy không được âm.");

        customer.TenKH = customer.TenKH.Trim();
        customer.DienThoai = NormalizeOptional(customer.DienThoai);
        if (_dal.SaveCustomer(customer, isNew) != 1)
            throw new InvalidOperationException("Không tìm thấy khách hàng hoặc không thể lưu thông tin.");
    }

    public void DeleteCustomer(int customerId)
    {
        if (_dal.DeleteCustomer(customerId) != 1)
            throw new InvalidOperationException("Không tìm thấy khách hàng.");
    }

    public List<EmployeeDto> GetEmployees() => _dal.GetEmployees();

    public void SaveEmployee(EmployeeDto employee, bool isNew)
    {
        if (string.IsNullOrWhiteSpace(employee.TenNV) || employee.TenNV.Trim().Length > 100)
            throw new ArgumentException("Tên nhân viên là bắt buộc và không được vượt quá 100 ký tự.");
        ValidatePhone(employee.SoDienThoai);
        if (employee.ChucVu?.Length > 50)
            throw new ArgumentException("Chức vụ không được vượt quá 50 ký tự.");

        employee.TenNV = employee.TenNV.Trim();
        employee.SoDienThoai = NormalizeOptional(employee.SoDienThoai);
        employee.ChucVu = NormalizeOptional(employee.ChucVu);
        if (_dal.SaveEmployee(employee, isNew) != 1)
            throw new InvalidOperationException("Không tìm thấy nhân viên hoặc không thể lưu thông tin.");
    }

    public void DeleteEmployee(int employeeId)
    {
        if (_dal.DeleteEmployee(employeeId) != 1)
            throw new InvalidOperationException("Không tìm thấy nhân viên.");
    }

    public int Checkout(int employeeId, int? customerId, IReadOnlyCollection<CartItemDto> items)
    {
        if (employeeId <= 0)
            throw new ArgumentException("Vui lòng chọn nhân viên trước khi thanh toán.");
        if (items.Count == 0)
            throw new ArgumentException("Vui lòng thêm ít nhất một sản phẩm vào giỏ hàng.");
        if (items.Any(item => item.Quantity <= 0 || string.IsNullOrWhiteSpace(item.Product.MaSP) || item.Product.GiaBan <= 0))
            throw new ArgumentException("Giỏ hàng có sản phẩm hoặc số lượng không hợp lệ.");
        if (items.Select(item => item.Product.MaSP).Distinct(StringComparer.OrdinalIgnoreCase).Count() != items.Count)
            throw new ArgumentException("Mỗi sản phẩm chỉ được xuất hiện một lần trong giỏ hàng.");

        return _dal.CreateInvoice(employeeId, customerId, items);
    }

    public SalesSummaryDto GetSalesSummary() => _dal.GetSalesSummary();

    public List<TopProductDto> GetTopProducts() => _dal.GetTopProducts();

    private static void ValidatePhone(string? phone)
    {
        if (phone?.Length > 15)
            throw new ArgumentException("Số điện thoại không được vượt quá 15 ký tự.");
    }

    private static string? NormalizeOptional(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }
}
