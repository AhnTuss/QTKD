# Tuần 07 – Kiểm thử

Chạy toàn bộ test từ thư mục gốc repo:

```powershell
dotnet test QTKD.sln
```

## Bảng TC01–TC08

| Test | Kiểm tra | Loại | Kết quả |
|---|---|---|---|
| TC01 | Tạo Customer hợp lệ, nhận `201` và đọc lại từ DB | Integration | Pass |
| TC02 | Tạo Customer thiếu tên, nhận `400`, DB không có bản ghi mới | Integration | Pass |
| TC03 | Sửa Contract Draft, nhận `200` và DB được cập nhật | Integration | Pass |
| TC04 | Sửa Contract PendingApproval, nhận `409`, dữ liệu giữ nguyên | Integration | Pass |
| TC05 | Submit thiếu nội dung, nhận `400`, Contract vẫn Draft | Integration | Pass |
| TC06 | Submit hợp lệ, response và DB cùng là PendingApproval | Integration | Pass |
| TC07 | Gửi `status: Approved` trực tiếp, nhận `400`, Contract vẫn Draft | Integration | Pass |
| TC08 | Submit lần hai nhận `409`, không tạo thêm Contract | Integration | Pass |

Integration test chạy request qua ASP.NET Core TestServer và SQLite in-memory riêng cho từng test. Database bị hủy khi test kết thúc; PostgreSQL `qtkd` không được truy cập.

## Kiểm tra Pricing bổ sung

Ngoài TC01–TC08, integration test tạo PriceList, thêm và đọc PriceItem; đồng thời xác nhận giá bằng 0 bị từ chối, ServiceCode trùng trả `409`, và DB chỉ lưu một item hợp lệ.

## Unit test CTR

`ContractRuleTests` có 7 unit tests: sửa được Draft nhưng không được sửa PendingApproval (CTR-01); Submit thành công khi đủ điều kiện và bị từ chối nếu thiếu nội dung, Customer không Active hoặc khoảng ngày sai (CTR-02); Submit chỉ chuyển Draft sang PendingApproval, không sang Approved (CTR-03).

## Lỗi tìm thấy và sửa

TC07 ban đầu nhận `200` khi request có trường JSON `status: Approved`: model binding mặc định bỏ qua trường không khai báo, dù database vẫn giữ Draft. `UpdateContractRequest` hiện từ chối trường JSON không được định nghĩa, API trả `400` và dữ liệu không đổi.

## Giới hạn

CTR-02 đang dùng giả định demo được ghi trong `docs/week06.md`: một nội dung có tên được xem là mục bắt buộc. API chưa lưu file thật. Integration test dùng SQLite để độc lập với PostgreSQL; cần chạy migration và demo tuần 06 riêng trên PostgreSQL trước khi nộp.