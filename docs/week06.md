# Tuần 06 – API

API tuần 06 tiếp tục dùng database và project của tuần 05. Chạy API rồi mở Swagger tại `http://localhost:5080/swagger`.

## Customer

| Method | Đường dẫn | Kết quả |
|---|---|---|
| GET | `/api/customers` | Danh sách khách hàng |
| GET | `/api/customers/{id}` | Chi tiết; `404` nếu không có |
| POST | `/api/customers` | Tạo khách hàng, trả `201` |
| PUT | `/api/customers/{id}` | Cập nhật tên/trạng thái; `404` nếu không có |

Ví dụ tạo khách hàng:

```json
{"name":"Công ty Minh Phát","status":"Active"}
```

## Contract

| Method | Đường dẫn | Kết quả |
|---|---|---|
| GET | `/api/contracts` | Danh sách hợp đồng |
| GET | `/api/contracts/{id}` | Chi tiết; gồm Customer và nội dung |
| POST | `/api/contracts` | Tạo hợp đồng ở trạng thái `Draft`, trả `201` |
| PUT | `/api/contracts/{id}` | Cập nhật khi ở `Draft` hoặc `RevisionRequested` |
| POST | `/api/contracts/{id}/contents` | Thêm nội dung khi hợp đồng còn sửa được |
| POST | `/api/contracts/{id}/submit` | Gửi trình duyệt |

Submit chỉ nhận hợp đồng `Draft`, Customer phải đang `Active`, ngày bắt đầu/kết thúc phải có và hợp lệ, đồng thời hợp đồng phải có ít nhất một nội dung. Thành công chuyển sang `PendingApproval`; gửi lặp hoặc sửa ở trạng thái khác trả `409`. Khi thiếu điều kiện, API trả `400` và giữ nguyên trạng thái.

Ví dụ tạo hợp đồng:

```json
{
  "contractNo": "HD-2026-019",
  "customerId": 1,
  "startDate": "2026-11-06",
  "endDate": "2027-11-05"
}
```

Thêm nội dung rồi gửi:

```json
{"name":"Phạm vi dịch vụ"}
```

## Giả định CTR-02

Phase 1 chưa cung cấp danh sách tài liệu bắt buộc. Bản demo hiện coi một mục có tên trong `ContractContents` là nội dung bắt buộc. API chỉ lưu tên mục, chưa upload hay lưu file. Cần thay giả định này nếu Acceptance Criteria Phase 1 quy định khác.

## Giới hạn

Submit chỉ chuyển trạng thái sang `PendingApproval`; chưa có bước duyệt, từ chối, thanh toán hay upload file.