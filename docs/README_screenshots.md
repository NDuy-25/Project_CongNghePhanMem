# Chụp ảnh giao diện Phần 5 - COFFEE 12

Script này dùng Playwright để tự động mở website COFFEE 12 và chụp 15 ảnh PNG phục vụ báo cáo Phần 5 - Thiết kế giao diện.

## Điều kiện trước khi chạy

1. Mở Visual Studio và chạy project App12COFFEE.
2. Website đang chạy tại `https://localhost:44300`.
3. SQL Server Express có database `12COFFEE`.

## Lệnh chạy

Mở PowerShell tại thư mục gốc project:

```powershell
cd "C:\Users\Admin\Downloads\Project_CongNghePhanMem-main\Project_CongNghePhanMem-main"
powershell -ExecutionPolicy Bypass -File .\docs\run_capture_screenshots.ps1
```

Nếu web chạy ở địa chỉ khác:

```powershell
powershell -ExecutionPolicy Bypass -File .\docs\run_capture_screenshots.ps1 -BaseUrl "https://localhost:44300"
```

## Kết quả

Ảnh PNG được lưu tại:

```text
docs/screenshots/
```

Script sẽ tự chuẩn bị dữ liệu mẫu cần cho báo cáo: tài khoản mẫu, giỏ hàng 2 sản phẩm, đơn chờ duyệt, đơn đang giao, đơn đã giao, thanh toán, đánh giá và lịch sử chat AI. Script không sửa code backend và không thay đổi nghiệp vụ chính.
