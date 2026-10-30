# QTKD

Bài thực hành Phase 2, tuần 05. Project dùng ASP.NET Core Web API, Entity Framework Core và PostgreSQL.

Hiện tại API đọc được danh sách khách hàng và chi tiết hợp đồng. Dữ liệu mẫu gồm 2 khách hàng, 1 hợp đồng Draft và 1 bảng giá có 2 dịch vụ. Chức năng tạo/sửa dữ liệu và gửi hợp đồng chưa làm trong tuần này.

## Chạy project

Cần cài .NET SDK 10 và PostgreSQL. Từ thư mục gốc repo, tạo database và user nếu chưa có:

```sql
CREATE USER qtkd WITH PASSWORD '<mat_khau>' CREATEDB;
CREATE DATABASE qtkd OWNER qtkd;
```

Đặt chuỗi kết nối bằng User Secrets để không lưu mật khẩu trong Git:

```powershell
dotnet user-secrets set "ConnectionStrings:QtkdDb" "Host=localhost;Port=5432;Database=qtkd;Username=qtkd;Password=<mat_khau>" --project src/QTKD.Api
```

Migration đầu tiên đã có trong source. Cập nhật database rồi chạy API:

```powershell
dotnet restore
dotnet build
dotnet ef database update --project src/QTKD.Api
dotnet run --project src/QTKD.Api
```

Nếu máy chưa có công cụ EF, cài một lần bằng lệnh `dotnet tool install --global dotnet-ef`.

Khi chạy ở Development, API mở Swagger tại <http://localhost:5080/swagger>. Dữ liệu mẫu được thêm nếu database đã migrate và bảng Customers chưa có dữ liệu.

## API hiện có

| Method | Đường dẫn | Kết quả |
|---|---|---|
| GET | `/api/customers` | Danh sách khách hàng |
| GET | `/api/contracts/{id}` | Thông tin hợp đồng; trả 404 nếu không tìm thấy |

Thử nhanh:

```powershell
curl.exe http://localhost:5080/api/customers
curl.exe http://localhost:5080/api/contracts/1
curl.exe -i http://localhost:5080/api/contracts/999
```

Migration và cấu hình database nằm trong `src/QTKD.Api`. Tài liệu sơ đồ tuần 05 ở `docs/week05.md`.