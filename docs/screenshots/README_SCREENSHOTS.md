# README SCREENSHOTS - QA COFFEE 12

Thời điểm kiểm tra: 29/05/2026.

Phạm vi thực hiện: chỉ quét project, chạy thử màn hình và chụp ảnh kết quả. Không sửa source code, không sửa file Word/docx, không viết lại nội dung báo cáo.

## Kết quả nhanh

- Bộ ảnh mới theo yêu cầu: 17/17 ảnh.
- Kích thước ảnh: 1440x900.
- Định dạng: PNG.
- Không phát hiện ảnh trắng, lỗi 404, lỗi chưa đăng nhập hoặc Server Error trong bộ ảnh mới.
- 16 ảnh đạt yêu cầu nội dung.
- 1 ảnh đạt một phần: quản lý sản phẩm có dữ liệu sản phẩm, giá, tồn kho, trạng thái và thao tác, nhưng chưa hiển thị trực tiếp cột danh mục và hình ảnh trong bảng.
- Lưu ý: thư mục `docs/screenshots` vẫn còn một số ảnh cũ từ lần chụp trước. Bộ ảnh mới là 17 file đúng tên trong bảng dưới đây.

| STT | Tên file ảnh | URL | Vai trò | Loại màn hình | Có dữ liệu chưa | Ghi chú kiểm tra |
|---:|---|---|---|---|---|---|
| 1 | Hinh_5_1_Menu_va_tim_kiem_san_pham.png | `/Home/Menu?page=1` | Khách hàng | Tra cứu/xem danh sách | Có | Đạt. Có sản phẩm, danh mục, giá, nút Chi tiết và Đặt hàng. |
| 2 | Hinh_5_2_Chi_tiet_san_pham.png | `/Home/ChiTietSanPham/89` | Khách hàng | Thông tin chi tiết | Có | Đạt. Có ảnh sản phẩm, giá, tồn kho, nút Thêm vào giỏ và Hỏi AI tư vấn. |
| 3 | Hinh_5_3_Gio_hang.png | `/GioHang` | Khách hàng | Giỏ hàng | Có | Đạt. Có ít nhất 2 sản phẩm, số lượng, đơn giá, thành tiền và tổng thanh toán. |
| 4 | Hinh_5_4_Xac_nhan_dat_hang_online.png | `/GioHang` | Khách hàng | Nhập liệu/xác nhận đặt hàng | Có | Đạt. Có người nhận, số điện thoại, địa chỉ giao hàng, ghi chú, phương thức thanh toán và tổng tiền. |
| 5 | Hinh_5_5_Thong_bao_dat_hang_thanh_cong.png | `/ThanhToan/ThanhToanThanhCong?maDH=10` | Khách hàng | Thông báo | Có | Đạt. Có thông báo thành công, mã đơn, tổng tiền và trạng thái xử lý. |
| 6 | Hinh_5_6_Theo_doi_trang_thai_don_hang.png | `/DonHang/LichSu` | Khách hàng | Theo dõi/tra cứu | Có | Đạt. Có mã đơn, ngày đặt, sản phẩm, thanh toán, tổng tiền, trạng thái và nút Chi tiết. |
| 7 | Hinh_5_7_Danh_gia_san_pham.png | `/DanhGia/Tao?maSP=88` | Khách hàng | Nhập liệu đánh giá | Có | Đạt. Có sản phẩm đã mua, chọn số sao, ô nhập nội dung và nút Gửi đánh giá. |
| 8 | Hinh_5_8_Chat_AI_tu_van_khach_hang.png | `/AIChat` | Khách hàng | Tương tác/tư vấn | Có | Đạt. Có câu hỏi của khách hàng, câu trả lời AI và gợi ý nhanh. |
| 9 | Hinh_5_9_Danh_sach_don_cho_duyet.png | `/NhanVienDuyetDon/Dashboard` | Nhân viên duyệt đơn | Danh sách xử lý | Có | Đạt. Có đơn chờ xác nhận, thông tin khách hàng, sản phẩm, tổng tiền và trạng thái. |
| 10 | Hinh_5_10_Duyet_don_va_phan_cong_giao_hang.png | `/NhanVienDuyetDon/Dashboard` | Nhân viên duyệt đơn | Thao tác xử lý | Có | Đạt. Có chọn nhân viên giao hàng, nút Duyệt và Từ chối. |
| 11 | Hinh_5_11_Cap_nhat_giao_hang_va_thanh_toan.png | `/NhanVienGiaoHang/Dashboard` | Nhân viên giao hàng | Cập nhật trạng thái | Có | Đạt. Có đơn đang giao, phương thức COD, số tiền, trạng thái và nút Giao thành công/Giao thất bại. |
| 12 | Hinh_5_12_Thong_bao_giao_hang_thanh_toan_thanh_cong.png | `/NhanVienGiaoHang/GiaoHangThanhCong?maDH=4` | Nhân viên giao hàng | Thông báo kết quả | Có | Đạt. Có thông báo giao hàng thành công và cập nhật thanh toán nếu là COD. |
| 13 | Hinh_5_13_Dashboard_quan_tri.png | `/Admin/Index` | Quản trị | Dashboard | Có | Đạt. Có các chỉ số tổng quan về người dùng, sản phẩm và đơn hàng. |
| 14 | Hinh_5_14_Quan_ly_danh_muc.png | `/DanhMucAdmin/Index` | Quản trị | Quản lý dữ liệu | Có | Đạt. Có danh sách danh mục, số sản phẩm, ô thêm/sửa và nút xóa. |
| 15 | Hinh_5_15_Quan_ly_san_pham_va_hinh_anh.png | `/SanPhamAdmin/Index` | Quản trị | Quản lý dữ liệu | Có một phần | Có sản phẩm, giá, tồn kho, trạng thái, sửa/xóa. Chưa hiển thị trực tiếp cột danh mục và hình ảnh trong bảng; nếu yêu cầu ảnh báo cáo phải đúng tuyệt đối thì nên bổ sung cột này sau khi được xác nhận. |
| 16 | Hinh_5_16_Thong_ke_don_hang_doanh_thu.png | `/ThongkeAdmin/DoanhThuThang` | Quản trị | Báo biểu/thống kê | Có | Đạt. Có biểu đồ/số liệu doanh thu đơn hàng. |
| 17 | Hinh_5_17_Tra_cuu_danh_gia_va_lich_su_tu_van_AI.png | `/TheoDoiAdmin/DanhGiaVaChat` | Quản trị | Tra cứu/giám sát | Có | Đạt. Có dữ liệu đánh giá sản phẩm và lịch sử Chat AI. |

## Kết luận QA

1. Đã chụp được 17/17 ảnh theo danh sách mới.
2. Ảnh đạt: 16 ảnh.
3. Ảnh đạt một phần: `Hinh_5_15_Quan_ly_san_pham_va_hinh_anh.png` vì chưa hiển thị trực tiếp cột hình ảnh và danh mục trong bảng quản lý sản phẩm.
4. Không phát hiện màn hình chưa tồn tại trong danh sách yêu cầu.
5. Có cần sửa code không: không bắt buộc nếu chỉ cần ảnh minh họa hiện trạng project. Nếu muốn khớp tuyệt đối yêu cầu “quản lý sản phẩm và hình ảnh”, nên đề xuất sửa `SanPhamAdmin/Index` để thêm cột ảnh đại diện và danh mục, nhưng chưa sửa trong lần QA này.
