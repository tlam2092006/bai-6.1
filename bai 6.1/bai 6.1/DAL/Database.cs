using Microsoft.Data.SqlClient;

namespace bai_6._1.DAL;

internal static class Database
{
    private const string DefaultConnectionString = "Server=(localdb)\\MSSQLLocalDB;Database=SalesDB;Integrated Security=true;TrustServerCertificate=true";

    public static SqlConnection CreateConnection()
    {
        var connectionString = Environment.GetEnvironmentVariable("RETAIL_SALES_CONNECTION_STRING");
        return new SqlConnection(string.IsNullOrWhiteSpace(connectionString) ? DefaultConnectionString : connectionString);
    }

    public static string GetUserFriendlyError(Exception exception)
    {
        if (exception is not SqlException sqlException)
            return exception.Message;

        var connectionString = Environment.GetEnvironmentVariable("RETAIL_SALES_CONNECTION_STRING");
        if (string.IsNullOrWhiteSpace(connectionString))
            connectionString = DefaultConnectionString;

        var target = "SalesDB";
        try
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            target = $"cơ sở dữ liệu '{builder.InitialCatalog}' trên máy chủ '{builder.DataSource}'";
        }
        catch (ArgumentException)
        {
            // Keep the safe default label if the configured connection string is malformed.
        }

        return sqlException.Number switch
        {
            4060 => $"Không thể mở {target}. Cơ sở dữ liệu có thể chưa được tạo trên đúng máy chủ, đang ngoại tuyến hoặc tài khoản Windows hiện tại không có quyền truy cập. Hãy chạy tệp SalesDB.sql trên đúng máy chủ hoặc kiểm tra biến môi trường RETAIL_SALES_CONNECTION_STRING. (Mã lỗi SQL: {sqlException.Number})",
            18456 => $"Đăng nhập SQL Server bị từ chối khi kết nối tới {target}. Hãy kiểm tra thông tin đăng nhập hoặc quyền của tài khoản Windows hiện tại và cấu hình RETAIL_SALES_CONNECTION_STRING. (Mã lỗi SQL: {sqlException.Number})",
            _ => $"Đã xảy ra lỗi khi truy cập cơ sở dữ liệu. Hãy kiểm tra máy chủ, cơ sở dữ liệu và cấu hình kết nối. (Mã lỗi SQL: {sqlException.Number})"
        };
    }
}
