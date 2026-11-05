namespace bai_6._1.DTO;

public sealed class ProductDto
{
    public string MaSP { get; set; } = string.Empty;
    public string TenSP { get; set; } = string.Empty;
    public decimal GiaBan { get; set; }
    public int SoLuongTon { get; set; }
}

public sealed class CustomerDto
{
    public int MaKH { get; set; }
    public string TenKH { get; set; } = string.Empty;
    public string? DienThoai { get; set; }
    public int DiemTichLuy { get; set; }
}

public sealed class EmployeeDto
{
    public int MaNV { get; set; }
    public string TenNV { get; set; } = string.Empty;
    public string? SoDienThoai { get; set; }
    public string? ChucVu { get; set; }
}

public sealed class CartItemDto
{
    public ProductDto Product { get; set; } = new();
    public int Quantity { get; set; }
    public decimal LineTotal => Product.GiaBan * Quantity;
}

public sealed class TopProductDto
{
    public string MaSP { get; set; } = string.Empty;
    public string TenSP { get; set; } = string.Empty;
    public long QuantitySold { get; set; }
    public decimal Revenue { get; set; }
}

public sealed class SalesSummaryDto
{
    public decimal TodayRevenue { get; set; }
    public decimal MonthRevenue { get; set; }
}
