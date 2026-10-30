# QTKD – Hệ thống Quản trị kinh doanh (Phase 2)

Dự án thực hành *Phát triển ứng dụng HTTT hiện đại*, Phase 2 (tuần 05–07).
Công nghệ: C# · ASP.NET Core Web API · Entity Framework Core · PostgreSQL · xUnit.

**Trạng thái hiện tại: Tuần 05** – Web API kết nối PostgreSQL, có `Customer`, `Contract`,
`PriceList`, `PriceItem` và hai endpoint đọc dữ liệu:

| Method | Endpoint | Kết quả |
|---|---|---|
| GET | `/api/customers` | 200 + danh sách khách hàng |
| GET | `/api/contracts/{id}` | 200 + chi tiết hợp đồng, hoặc 404 nếu không có |

## Yêu cầu

- .NET SDK 10 (`dotnet --version`)
- PostgreSQL 14 trở lên (đã dùng 16)
- Công cụ EF Core: `dotnet tool install --global dotnet-ef`

## Cấu trúc thư mục

```
QTKD/
├── QTKD.sln
├── README.md
├── src/QTKD.Api/
│   ├── Controllers/      # nhận HTTP request, trả HTTP response
│   ├── DTOs/             # dữ liệu vào/ra của API
│   ├── Entities/         # Customer, Contract, PriceList, PriceItem
│   ├── Services/         # xử lý nghiệp vụ
│   ├── Data/             # QtkdDbContext, DbSeeder
│   ├── Migrations/       # sinh ra bởi dotnet ef (commit vào Git)
│   └── Program.cs
├── tests/                # xUnit (tuần 07)
└── docs/                 # sơ đồ, tài liệu API, minh chứng
```

## 1. Tạo database

Dùng `psql` (quyền `postgres`), thay `<mat_khau>` bằng mật khẩu của bạn:

```sql
CREATE USER qtkd WITH PASSWORD '<mat_khau>' CREATEDB;
CREATE DATABASE qtkd OWNER qtkd;
```

## 2. Cấu hình chuỗi kết nối (không đưa mật khẩu lên Git)

`appsettings.json` chỉ chứa chuỗi kết nối **không có mật khẩu**. Đặt mật khẩu bằng user-secrets:

```bash
dotnet user-secrets set "ConnectionStrings:QtkdDb" "Host=localhost;Port=5432;Database=qtkd;Username=qtkd;Password=<mat_khau>" --project src/QTKD.Api
```

Hoặc dùng biến môi trường (Linux/macOS: `export`, PowerShell: `$env:`):

```bash
export ConnectionStrings__QtkdDb="Host=localhost;Port=5432;Database=qtkd;Username=qtkd;Password=<mat_khau>"
```

## 3. Restore, build và tạo database bằng migration

Chạy ở thư mục gốc của repository:

```bash
dotnet restore
dotnet build
dotnet ef database update --project src/QTKD.Api
```

- Migration `InitialCreate` đã được commit trong `src/QTKD.Api/Migrations`; clone mới chỉ cần
  chạy `database update`. Chỉ tạo migration mới khi thay đổi schema.
- Sau đó kiểm tra bảng, khóa chính, khóa ngoại trong PostgreSQL:

```bash
psql -h localhost -U qtkd -d qtkd -c "\d \"Contracts\""
psql -h localhost -U qtkd -d qtkd -c "\d \"PriceItems\""
```

## 4. Chạy API

```bash
dotnet run --project src/QTKD.Api
```

- Chạy ở môi trường Development (xem `Properties/launchSettings.json`): bật Swagger và
  **tự seed dữ liệu mẫu** nếu database còn trống (2 Customer, 1 Contract `Draft`,
  1 PriceList có 2 PriceItem).
- Swagger UI: <http://localhost:5080/swagger>

## 5. Thử hai endpoint

```bash
curl http://localhost:5080/api/customers
curl http://localhost:5080/api/contracts/1
curl -i http://localhost:5080/api/contracts/999   # 404
```

Kết quả mong đợi (id có thể khác nếu bạn đã xóa/seed lại):

```json
[{"id":1,"name":"Công ty Minh Phát","status":"Active"},
 {"id":2,"name":"Công ty An Khang","status":"Active"}]
```

```json
{"id":1,"contractNo":"HD-2026-018","customerId":1,"customerName":"Công ty Minh Phát",
 "startDate":null,"endDate":null,"status":"Draft"}
```

Đối chiếu với database:

```bash
psql -h localhost -U qtkd -d qtkd -c 'SELECT * FROM "Customers";'
psql -h localhost -U qtkd -d qtkd -c 'SELECT * FROM "Contracts";'
psql -h localhost -U qtkd -d qtkd -c 'SELECT * FROM "PriceItems";'
```

## Quyết định thiết kế (cần tự giải thích khi demo)

- **Service gọi thẳng `QtkdDbContext`, chưa tách Repository.** Phạm vi còn nhỏ nên giảm số lớp;
  đổi lại, khi test nghiệp vụ ở tuần 07 phải dùng database thật hoặc EF InMemory. Có thể tách
  Repository sau nếu cần.
- **Trạng thái lưu dạng chuỗi** (`"Draft"`, `"Active"`) để đọc trực tiếp trong PostgreSQL và
  không bị lệch khi thêm giá trị enum.
- **`StartDate`, `EndDate` của Contract cho phép null**: hợp đồng `Draft` được phép chưa đủ
  thông tin; quy tắc CTR-02 (tuần 06) sẽ chặn việc gửi trình duyệt khi còn thiếu.
- **`PriceItem` dùng khóa chính ghép (`PriceListId`, `ServiceCode`)**: mỗi bảng giá chỉ có một
  đơn giá cho mỗi mã dịch vụ.
- **Xóa `Customer` bị chặn** nếu còn hợp đồng (`DeleteBehavior.Restrict`).
- **Seed ở `DbSeeder`**, chỉ chạy ở Development và chỉ khi database trống.

## Giả định cần đối chiếu với Phase 1

Các trường và trạng thái dưới đây lấy theo gợi ý của đề thực hành, chưa phải đặc tả đã xác nhận.
Hãy cập nhật cho khớp Phase 1 và ghi lý do thay đổi:

- Trường của `Customer`, `Contract`, `PriceList`, `PriceItem`.
- Danh sách trạng thái hợp đồng (`Draft`, `PendingApproval`, `RevisionRequested`, `Approved`).
- Mã dịch vụ và đơn giá mẫu (`SVC-CONSULT`, `SVC-SUPPORT`) chỉ là dữ liệu minh họa.
- Danh sách nội dung/tệp bắt buộc theo CTR-02: chưa có, sẽ xác định ở tuần 06.

## Lỗi thường gặp

| Triệu chứng | Cách xử lý |
|---|---|
| `Password authentication failed` | Kiểm tra lại user-secrets/biến môi trường và mật khẩu của role `qtkd`. |
| `dotnet ef` không tìm thấy | `dotnet tool install --global dotnet-ef`, mở lại terminal. |
| `relation "Customers" does not exist` | Chưa chạy `dotnet ef database update`. |
| Không thấy Swagger | Đang chạy môi trường Production; chạy bằng `dotnet run --project src/QTKD.Api` hoặc đặt `ASPNETCORE_ENVIRONMENT=Development`. |
| Không seed | Database đã có Customer, hoặc chưa migrate (xem log cảnh báo khi khởi động). |

## Kế hoạch tiếp theo

- **Tuần 06:** CRUD Customer/Contract/PriceList, `POST /api/contracts/{id}/submit`, quy tắc CTR-01..03.
- **Tuần 07:** project xUnit (unit + integration test), bảng TC01–TC08, sửa lỗi, hoàn thiện README.
