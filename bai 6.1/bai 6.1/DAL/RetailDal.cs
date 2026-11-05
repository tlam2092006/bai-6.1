using System.Data;
using bai_6._1.DTO;
using Microsoft.Data.SqlClient;

namespace bai_6._1.DAL;

public sealed class RetailDal
{
    public List<ProductDto> GetProducts(string? search = null)
    {
        const string sql = "SELECT MaSP, TenSP, GiaBan, SoLuongTon FROM dbo.SanPham WHERE @Search IS NULL OR MaSP LIKE @Search OR TenSP LIKE @Search ORDER BY TenSP";
        using var connection = Database.CreateConnection();
        using var command = new SqlCommand(sql, connection);
        var term = string.IsNullOrWhiteSpace(search) ? null : $"%{search.Trim()}%";
        command.Parameters.Add("@Search", SqlDbType.NVarChar, 172).Value = (object?)term ?? DBNull.Value;
        connection.Open();
        using var reader = command.ExecuteReader();
        var results = new List<ProductDto>();
        while (reader.Read())
        {
            results.Add(new ProductDto
            {
                MaSP = reader.GetString(0),
                TenSP = reader.GetString(1),
                GiaBan = reader.GetDecimal(2),
                SoLuongTon = reader.GetInt32(3)
            });
        }
        return results;
    }

    public int SaveProduct(ProductDto product, bool isNew)
    {
        const string insertSql = "INSERT INTO dbo.SanPham (MaSP, TenSP, GiaBan, SoLuongTon) VALUES (@MaSP, @TenSP, @GiaBan, @SoLuongTon)";
        const string updateSql = "UPDATE dbo.SanPham SET TenSP = @TenSP, GiaBan = @GiaBan, SoLuongTon = @SoLuongTon WHERE MaSP = @MaSP";
        using var connection = Database.CreateConnection();
        using var command = new SqlCommand(isNew ? insertSql : updateSql, connection);
        AddProductParameters(command, product);
        connection.Open();
        return command.ExecuteNonQuery();
    }

    public int DeleteProduct(string productId)
    {
        using var connection = Database.CreateConnection();
        using var command = new SqlCommand("DELETE FROM dbo.SanPham WHERE MaSP = @MaSP", connection);
        command.Parameters.Add("@MaSP", SqlDbType.VarChar, 20).Value = productId;
        connection.Open();
        return command.ExecuteNonQuery();
    }

    public List<CustomerDto> GetCustomers()
    {
        using var connection = Database.CreateConnection();
        using var command = new SqlCommand("SELECT MaKH, TenKH, DienThoai, DiemTichLuy FROM dbo.KhachHang ORDER BY TenKH", connection);
        connection.Open();
        using var reader = command.ExecuteReader();
        var results = new List<CustomerDto>();
        while (reader.Read())
        {
            results.Add(new CustomerDto
            {
                MaKH = reader.GetInt32(0),
                TenKH = reader.GetString(1),
                DienThoai = reader.IsDBNull(2) ? null : reader.GetString(2),
                DiemTichLuy = reader.GetInt32(3)
            });
        }
        return results;
    }

    public int SaveCustomer(CustomerDto customer, bool isNew)
    {
        const string insertSql = "INSERT INTO dbo.KhachHang (TenKH, DienThoai, DiemTichLuy) VALUES (@TenKH, @DienThoai, @DiemTichLuy)";
        const string updateSql = "UPDATE dbo.KhachHang SET TenKH = @TenKH, DienThoai = @DienThoai, DiemTichLuy = @DiemTichLuy WHERE MaKH = @MaKH";
        using var connection = Database.CreateConnection();
        using var command = new SqlCommand(isNew ? insertSql : updateSql, connection);
        command.Parameters.Add("@TenKH", SqlDbType.NVarChar, 100).Value = customer.TenKH;
        command.Parameters.Add("@DienThoai", SqlDbType.VarChar, 15).Value = (object?)customer.DienThoai ?? DBNull.Value;
        command.Parameters.Add("@DiemTichLuy", SqlDbType.Int).Value = customer.DiemTichLuy;
        if (!isNew)
            command.Parameters.Add("@MaKH", SqlDbType.Int).Value = customer.MaKH;
        connection.Open();
        return command.ExecuteNonQuery();
    }

    public int DeleteCustomer(int customerId)
    {
        using var connection = Database.CreateConnection();
        using var command = new SqlCommand("DELETE FROM dbo.KhachHang WHERE MaKH = @MaKH", connection);
        command.Parameters.Add("@MaKH", SqlDbType.Int).Value = customerId;
        connection.Open();
        return command.ExecuteNonQuery();
    }

    public List<EmployeeDto> GetEmployees()
    {
        using var connection = Database.CreateConnection();
        using var command = new SqlCommand("SELECT MaNV, TenNV, SoDienThoai, ChucVu FROM dbo.NhanVien ORDER BY TenNV", connection);
        connection.Open();
        using var reader = command.ExecuteReader();
        var results = new List<EmployeeDto>();
        while (reader.Read())
        {
            results.Add(new EmployeeDto
            {
                MaNV = reader.GetInt32(0),
                TenNV = reader.GetString(1),
                SoDienThoai = reader.IsDBNull(2) ? null : reader.GetString(2),
                ChucVu = reader.IsDBNull(3) ? null : reader.GetString(3)
            });
        }
        return results;
    }

    public int SaveEmployee(EmployeeDto employee, bool isNew)
    {
        const string insertSql = "INSERT INTO dbo.NhanVien (TenNV, SoDienThoai, ChucVu) VALUES (@TenNV, @SoDienThoai, @ChucVu)";
        const string updateSql = "UPDATE dbo.NhanVien SET TenNV = @TenNV, SoDienThoai = @SoDienThoai, ChucVu = @ChucVu WHERE MaNV = @MaNV";
        using var connection = Database.CreateConnection();
        using var command = new SqlCommand(isNew ? insertSql : updateSql, connection);
        command.Parameters.Add("@TenNV", SqlDbType.NVarChar, 100).Value = employee.TenNV;
        command.Parameters.Add("@SoDienThoai", SqlDbType.VarChar, 15).Value = (object?)employee.SoDienThoai ?? DBNull.Value;
        command.Parameters.Add("@ChucVu", SqlDbType.NVarChar, 50).Value = (object?)employee.ChucVu ?? DBNull.Value;
        if (!isNew)
            command.Parameters.Add("@MaNV", SqlDbType.Int).Value = employee.MaNV;
        connection.Open();
        return command.ExecuteNonQuery();
    }

    public int DeleteEmployee(int employeeId)
    {
        using var connection = Database.CreateConnection();
        using var command = new SqlCommand("DELETE FROM dbo.NhanVien WHERE MaNV = @MaNV", connection);
        command.Parameters.Add("@MaNV", SqlDbType.Int).Value = employeeId;
        connection.Open();
        return command.ExecuteNonQuery();
    }

    public int CreateInvoice(int employeeId, int? customerId, IReadOnlyCollection<CartItemDto> items)
    {
        if (items.Count == 0)
            throw new InvalidOperationException("Giỏ hàng đang trống.");

        var total = items.Sum(item => item.LineTotal);
        using var connection = Database.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        try
        {
            using var invoiceCommand = new SqlCommand("INSERT INTO dbo.HoaDon (MaNV, MaKH, TongTien) OUTPUT INSERTED.MaHD VALUES (@MaNV, @MaKH, @TongTien)", connection, transaction);
            invoiceCommand.Parameters.Add("@MaNV", SqlDbType.Int).Value = employeeId;
            invoiceCommand.Parameters.Add("@MaKH", SqlDbType.Int).Value = (object?)customerId ?? DBNull.Value;
            var totalParameter = invoiceCommand.Parameters.Add("@TongTien", SqlDbType.Decimal);
            totalParameter.Precision = 18;
            totalParameter.Scale = 2;
            totalParameter.Value = total;
            var invoiceId = (int)invoiceCommand.ExecuteScalar()!;

            foreach (var item in items)
            {
                using var stockCommand = new SqlCommand("UPDATE dbo.SanPham SET SoLuongTon = SoLuongTon - @SoLuong WHERE MaSP = @MaSP AND SoLuongTon >= @SoLuong", connection, transaction);
                stockCommand.Parameters.Add("@SoLuong", SqlDbType.Int).Value = item.Quantity;
                stockCommand.Parameters.Add("@MaSP", SqlDbType.VarChar, 20).Value = item.Product.MaSP;
                if (stockCommand.ExecuteNonQuery() != 1)
                    throw new InvalidOperationException($"Sản phẩm {item.Product.MaSP} không tồn tại hoặc không đủ số lượng tồn.");

                using var detailCommand = new SqlCommand("INSERT INTO dbo.ChiTietHoaDon (MaHD, MaSP, SoLuong, DonGia) VALUES (@MaHD, @MaSP, @SoLuong, @DonGia)", connection, transaction);
                detailCommand.Parameters.Add("@MaHD", SqlDbType.Int).Value = invoiceId;
                detailCommand.Parameters.Add("@MaSP", SqlDbType.VarChar, 20).Value = item.Product.MaSP;
                detailCommand.Parameters.Add("@SoLuong", SqlDbType.Int).Value = item.Quantity;
                var priceParameter = detailCommand.Parameters.Add("@DonGia", SqlDbType.Decimal);
                priceParameter.Precision = 18;
                priceParameter.Scale = 2;
                priceParameter.Value = item.Product.GiaBan;
                detailCommand.ExecuteNonQuery();
            }

            if (customerId.HasValue)
            {
                using var pointsCommand = new SqlCommand("UPDATE dbo.KhachHang SET DiemTichLuy = DiemTichLuy + CONVERT(int, @TongTien / 100000) WHERE MaKH = @MaKH", connection, transaction);
                var pointsTotalParameter = pointsCommand.Parameters.Add("@TongTien", SqlDbType.Decimal);
                pointsTotalParameter.Precision = 18;
                pointsTotalParameter.Scale = 2;
                pointsTotalParameter.Value = total;
                pointsCommand.Parameters.Add("@MaKH", SqlDbType.Int).Value = customerId.Value;
                if (pointsCommand.ExecuteNonQuery() != 1)
                    throw new InvalidOperationException("Khách hàng đã chọn không còn tồn tại.");
            }

            transaction.Commit();
            return invoiceId;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public SalesSummaryDto GetSalesSummary()
    {
        const string todaySql = "SELECT COALESCE(SUM(TongTien), 0) FROM dbo.HoaDon WHERE NgayLap >= CONVERT(date, GETDATE()) AND NgayLap < DATEADD(day, 1, CONVERT(date, GETDATE()))";
        const string monthSql = "SELECT COALESCE(SUM(TongTien), 0) FROM dbo.HoaDon WHERE NgayLap >= DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1) AND NgayLap < DATEADD(month, 1, DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1))";
        using var connection = Database.CreateConnection();
        connection.Open();
        using var todayCommand = new SqlCommand(todaySql, connection);
        var today = Convert.ToDecimal(todayCommand.ExecuteScalar());
        using var monthCommand = new SqlCommand(monthSql, connection);
        var month = Convert.ToDecimal(monthCommand.ExecuteScalar());
        return new SalesSummaryDto { TodayRevenue = today, MonthRevenue = month };
    }

    public List<TopProductDto> GetTopProducts()
    {
        const string sql = "SELECT TOP (5) sp.MaSP, sp.TenSP, SUM(CONVERT(bigint, ct.SoLuong)) AS QuantitySold, SUM(ct.SoLuong * ct.DonGia) AS Revenue FROM dbo.ChiTietHoaDon AS ct INNER JOIN dbo.SanPham AS sp ON sp.MaSP = ct.MaSP GROUP BY sp.MaSP, sp.TenSP ORDER BY QuantitySold DESC, sp.TenSP";
        using var connection = Database.CreateConnection();
        using var command = new SqlCommand(sql, connection);
        connection.Open();
        using var reader = command.ExecuteReader();
        var results = new List<TopProductDto>();
        while (reader.Read())
        {
            results.Add(new TopProductDto
            {
                MaSP = reader.GetString(0),
                TenSP = reader.GetString(1),
                QuantitySold = reader.GetInt64(2),
                Revenue = reader.GetDecimal(3)
            });
        }
        return results;
    }

    private static void AddProductParameters(SqlCommand command, ProductDto product)
    {
        command.Parameters.Add("@MaSP", SqlDbType.VarChar, 20).Value = product.MaSP;
        command.Parameters.Add("@TenSP", SqlDbType.NVarChar, 150).Value = product.TenSP;
        var priceParameter = command.Parameters.Add("@GiaBan", SqlDbType.Decimal);
        priceParameter.Precision = 18;
        priceParameter.Scale = 2;
        priceParameter.Value = product.GiaBan;
        command.Parameters.Add("@SoLuongTon", SqlDbType.Int).Value = product.SoLuongTon;
    }
}
