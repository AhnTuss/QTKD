# QTKD

Bài thực hành Phase 2, hiện đã làm tiếp các chức năng tuần 06. Project dùng ASP.NET Core Web API, Entity Framework Core và PostgreSQL.

API hiện có Customer, Contract, Submit hợp đồng và quản lý PriceList/PriceItem. Các route và giả định CTR-02 được ghi trong [`docs/week06.md`](docs/week06.md). Dữ liệu mẫu được thêm khi database trống.

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

Danh sách API tuần 06 và ví dụ request nằm trong [`docs/week06.md`](docs/week06.md).

## Chạy test

```powershell
dotnet test QTKD.sln
```

Test API bằng `WebApplicationFactory` và SQLite in-memory riêng cho từng test. Không cần bật PostgreSQL và không dùng database `qtkd` của ứng dụng. Danh sách tình huống cùng kết quả nằm trong [`docs/week07.md`](docs/week07.md).

Migration và cấu hình database nằm trong `src/QTKD.Api`; sơ đồ tuần 05 ở `docs/week05.md`.