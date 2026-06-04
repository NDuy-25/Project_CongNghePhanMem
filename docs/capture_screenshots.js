const fs = require('fs');
const path = require('path');
const os = require('os');
const { execFileSync } = require('child_process');
const { chromium } = require('playwright');

const BASE_URL = (process.env.BASE_URL || 'https://localhost:44300').replace(/\/$/, '');
const SQL_SERVER = process.env.SQL_SERVER || '.\\SQLEXPRESS';
const SQL_DATABASE = process.env.SQL_DATABASE || '12COFFEE';
const rootDir = path.resolve(__dirname, '..');
const screenshotDir = path.join(__dirname, 'screenshots');
fs.mkdirSync(screenshotDir, { recursive: true });

function runSql(sql) {
  const tmp = path.join(os.tmpdir(), `coffee12_seed_${Date.now()}_${Math.random().toString(16).slice(2)}.sql`);
  fs.writeFileSync(tmp, '\uFEFF' + sql, 'utf16le');
  try {
    return execFileSync('sqlcmd', ['-S', SQL_SERVER, '-d', SQL_DATABASE, '-E', '-b', '-i', tmp], {
      encoding: 'utf8',
      stdio: ['ignore', 'pipe', 'pipe']
    });
  } catch (error) {
    const stdout = error.stdout ? String(error.stdout) : '';
    const stderr = error.stderr ? String(error.stderr) : '';
    throw new Error(`SQL seed lỗi.\nSTDOUT:\n${stdout}\nSTDERR:\n${stderr}`);
  } finally {
    try { fs.unlinkSync(tmp); } catch (_) {}
  }
}

function seedData() {
  const sql = String.raw`
SET NOCOUNT ON;

DECLARE @kh INT, @admin INT, @nvDuyet INT, @nvGiao INT;

IF NOT EXISTS (SELECT 1 FROM NguoiDung WHERE TenDangNhap=N'admin')
BEGIN
  INSERT INTO NguoiDung(TenDangNhap, MatKhau, HoTen, Email, DienThoai, VaiTro, NgayTao, TrangThai, Quyen, DiaChiGiaoHang, Tinh, Huyen, Xa)
  VALUES(N'admin', N'123456', N'Quản trị viên', N'admin@coffee12.local', N'0900000001', N'Admin', GETDATE(), 1, N'Admin', N'12 Coffee', N'Khánh Hòa', N'Vạn Ninh', N'Vạn Giã');
END
ELSE
  UPDATE NguoiDung SET MatKhau=N'123456', HoTen=N'Quản trị viên', VaiTro=N'Admin', Quyen=N'Admin', TrangThai=1 WHERE TenDangNhap=N'admin';

IF NOT EXISTS (SELECT 1 FROM NguoiDung WHERE TenDangNhap=N'khachhang01')
BEGIN
  INSERT INTO NguoiDung(TenDangNhap, MatKhau, HoTen, Email, DienThoai, VaiTro, NgayTao, TrangThai, Quyen, DiaChiGiaoHang, Tinh, Huyen, Xa)
  VALUES(N'khachhang01', N'123456', N'Nguyễn Văn Hưng', N'khachhang01@coffee12.local', N'0123456789', N'KhachHang', GETDATE(), 1, N'KhachHang', N'123 Nguyễn Huệ, Tổ Hiến Thành', N'Khánh Hòa', N'Vạn Ninh', N'Vạn Giã');
END
ELSE
  UPDATE NguoiDung SET MatKhau=N'123456', HoTen=N'Nguyễn Văn Hưng', VaiTro=N'KhachHang', Quyen=N'KhachHang', TrangThai=1, DienThoai=N'0123456789', DiaChiGiaoHang=N'123 Nguyễn Huệ, Tổ Hiến Thành', Tinh=N'Khánh Hòa', Huyen=N'Vạn Ninh', Xa=N'Vạn Giã' WHERE TenDangNhap=N'khachhang01';

IF NOT EXISTS (SELECT 1 FROM NguoiDung WHERE TenDangNhap=N'nvduyet01')
BEGIN
  INSERT INTO NguoiDung(TenDangNhap, MatKhau, HoTen, Email, DienThoai, VaiTro, NgayTao, TrangThai, Quyen, DiaChiGiaoHang, Tinh, Huyen, Xa)
  VALUES(N'nvduyet01', N'123456', N'Nhân viên duyệt đơn', N'nvduyet01@coffee12.local', N'0900000002', N'NhanVienDuyetDon', GETDATE(), 1, N'NhanVienDuyetDon', N'12 Coffee', N'Khánh Hòa', N'Vạn Ninh', N'Vạn Giã');
END
ELSE
  UPDATE NguoiDung SET MatKhau=N'123456', HoTen=N'Nhân viên duyệt đơn', VaiTro=N'NhanVienDuyetDon', Quyen=N'NhanVienDuyetDon', TrangThai=1 WHERE TenDangNhap=N'nvduyet01';

IF NOT EXISTS (SELECT 1 FROM NguoiDung WHERE TenDangNhap=N'nvgiao01')
BEGIN
  INSERT INTO NguoiDung(TenDangNhap, MatKhau, HoTen, Email, DienThoai, VaiTro, NgayTao, TrangThai, Quyen, DiaChiGiaoHang, Tinh, Huyen, Xa)
  VALUES(N'nvgiao01', N'123456', N'Nhân viên giao hàng', N'nvgiao01@coffee12.local', N'0900000003', N'NhanVienGiaoHang', GETDATE(), 1, N'NhanVienGiaoHang', N'12 Coffee', N'Khánh Hòa', N'Vạn Ninh', N'Vạn Giã');
END
ELSE
  UPDATE NguoiDung SET MatKhau=N'123456', HoTen=N'Nhân viên giao hàng', VaiTro=N'NhanVienGiaoHang', Quyen=N'NhanVienGiaoHang', TrangThai=1 WHERE TenDangNhap=N'nvgiao01';

SELECT @kh=MaND FROM NguoiDung WHERE TenDangNhap=N'khachhang01';
SELECT @admin=MaND FROM NguoiDung WHERE TenDangNhap=N'admin';
SELECT @nvDuyet=MaND FROM NguoiDung WHERE TenDangNhap=N'nvduyet01';
SELECT @nvGiao=MaND FROM NguoiDung WHERE TenDangNhap=N'nvgiao01';

-- Dọn dữ liệu mẫu từng bị lỗi mã hóa khi chạy sqlcmd không đúng Unicode.
UPDATE DonHang SET TrangThaiDon=N'Chờ xác nhận' WHERE MaND=@kh AND (TrangThaiDon LIKE N'%Chá%' OR TrangThaiDon LIKE N'%xĂ%');
UPDATE DonHang SET TrangThaiDon=N'Đang giao hàng' WHERE MaND=@kh AND (TrangThaiDon LIKE N'%Ä%' OR TrangThaiDon LIKE N'%giao hĂ ng%' OR TrangThaiDon=N'Đang giao');
UPDATE DonHang SET TrangThaiDon=N'Giao hàng thành công' WHERE MaND=@kh AND (TrangThaiDon LIKE N'%thĂ nh cĂ´ng%' OR TrangThaiDon=N'Đã giao');
UPDATE tt SET PhuongThuc=N'COD' FROM ThanhToan tt JOIN DonHang dh ON dh.MaDH=tt.MaDH WHERE dh.MaND=@kh AND (tt.PhuongThuc LIKE N'%Tiá%' OR tt.PhuongThuc LIKE N'%Tiền mặt%');
UPDATE tt SET TrangThai=N'Chưa thanh toán' FROM ThanhToan tt JOIN DonHang dh ON dh.MaDH=tt.MaDH WHERE dh.MaND=@kh AND (tt.TrangThai LIKE N'%ChÆ%' OR tt.TrangThai LIKE N'%Chưa%');
UPDATE tt SET TrangThai=N'Đã thanh toán' FROM ThanhToan tt JOIN DonHang dh ON dh.MaDH=tt.MaDH WHERE dh.MaND=@kh AND (tt.TrangThai LIKE N'%Ä%' OR tt.TrangThai LIKE N'%thanh toĂ¡n%');

DECLARE @sp1 INT, @sp2 INT, @gia1 DECIMAL(18,2), @gia2 DECIMAL(18,2);
SELECT TOP 1 @sp1=MaSP, @gia1=ISNULL(Gia,0) FROM SanPham WHERE ISNULL(TrangThai,1)=1 ORDER BY MaSP DESC;
SELECT TOP 1 @sp2=MaSP, @gia2=ISNULL(Gia,0) FROM SanPham WHERE ISNULL(TrangThai,1)=1 AND MaSP<>@sp1 ORDER BY MaSP DESC;
IF @sp1 IS NULL RAISERROR(N'Không có sản phẩm để tạo dữ liệu chụp ảnh.', 16, 1);
IF @sp2 IS NULL BEGIN SET @sp2=@sp1; SET @gia2=@gia1; END;
UPDATE SanPham SET SoLuongTon = CASE WHEN ISNULL(SoLuongTon,0) < 20 THEN 50 ELSE SoLuongTon END, TrangThai=1 WHERE MaSP IN (@sp1, @sp2);

DECLARE @gh INT;
SELECT TOP 1 @gh=MaGH FROM GioHang WHERE MaND=@kh ORDER BY MaGH DESC;
IF @gh IS NULL
BEGIN
  INSERT INTO GioHang(MaND, NgayCapNhat) VALUES(@kh, GETDATE());
  SET @gh=SCOPE_IDENTITY();
END
ELSE
  UPDATE GioHang SET NgayCapNhat=GETDATE() WHERE MaGH=@gh;
DELETE FROM ChiTietGioHang WHERE MaGH=@gh;
INSERT INTO ChiTietGioHang(MaGH, MaSP, SoLuong) VALUES(@gh, @sp1, 1);
IF @sp2<>@sp1 INSERT INTO ChiTietGioHang(MaGH, MaSP, SoLuong) VALUES(@gh, @sp2, 1);

DECLARE @donCho INT, @donDangGiao INT, @donHoanTat INT;
DECLARE @tong DECIMAL(18,2)=@gia1+@gia2;

SELECT TOP 1 @donCho=MaDH FROM DonHang WHERE MaND=@kh AND TrangThaiDon=N'Chờ xác nhận' ORDER BY MaDH DESC;
IF @donCho IS NULL
BEGIN
  INSERT INTO DonHang(MaND, MaDC, MaNVGiaoHang, MaNVDuyet, NgayDat, TongTien, PhiVanChuyen, TrangThaiDon, MaNguoiDuyet)
  VALUES(@kh, NULL, @nvGiao, NULL, GETDATE(), @tong, 0, N'Chờ xác nhận', NULL);
  SET @donCho=SCOPE_IDENTITY();
END
ELSE
  UPDATE DonHang SET MaND=@kh, MaNVGiaoHang=@nvGiao, MaNVDuyet=NULL, NgayDat=GETDATE(), TongTien=@tong, PhiVanChuyen=0, TrangThaiDon=N'Chờ xác nhận', MaNguoiDuyet=NULL WHERE MaDH=@donCho;
DELETE FROM ChiTietDonHang WHERE MaDH=@donCho;
INSERT INTO ChiTietDonHang(MaDH, MaSP, SoLuong, DonGia) VALUES(@donCho, @sp1, 1, @gia1);
IF @sp2<>@sp1 INSERT INTO ChiTietDonHang(MaDH, MaSP, SoLuong, DonGia) VALUES(@donCho, @sp2, 1, @gia2);
DELETE FROM ThanhToan WHERE MaDH=@donCho;
INSERT INTO ThanhToan(MaDH, PhuongThuc, SoTien, NgayThanhToan, TrangThai) VALUES(@donCho, N'COD', @tong, GETDATE(), N'Chưa thanh toán');

SELECT TOP 1 @donDangGiao=MaDH FROM DonHang WHERE MaND=@kh AND TrangThaiDon=N'Đang giao hàng' ORDER BY MaDH DESC;
IF @donDangGiao IS NULL
BEGIN
  INSERT INTO DonHang(MaND, MaDC, MaNVGiaoHang, MaNVDuyet, NgayDat, TongTien, PhiVanChuyen, TrangThaiDon, MaNguoiDuyet)
  VALUES(@kh, NULL, @nvGiao, @nvDuyet, DATEADD(MINUTE,-30,GETDATE()), @tong, 0, N'Đang giao hàng', @nvDuyet);
  SET @donDangGiao=SCOPE_IDENTITY();
END
ELSE
  UPDATE DonHang SET MaND=@kh, MaNVGiaoHang=@nvGiao, MaNVDuyet=@nvDuyet, NgayDat=DATEADD(MINUTE,-30,GETDATE()), TongTien=@tong, PhiVanChuyen=0, TrangThaiDon=N'Đang giao hàng', MaNguoiDuyet=@nvDuyet WHERE MaDH=@donDangGiao;
DELETE FROM ChiTietDonHang WHERE MaDH=@donDangGiao;
INSERT INTO ChiTietDonHang(MaDH, MaSP, SoLuong, DonGia) VALUES(@donDangGiao, @sp1, 1, @gia1);
IF @sp2<>@sp1 INSERT INTO ChiTietDonHang(MaDH, MaSP, SoLuong, DonGia) VALUES(@donDangGiao, @sp2, 1, @gia2);
DELETE FROM ThanhToan WHERE MaDH=@donDangGiao;
INSERT INTO ThanhToan(MaDH, PhuongThuc, SoTien, NgayThanhToan, TrangThai) VALUES(@donDangGiao, N'COD', @tong, GETDATE(), N'Chưa thanh toán');

SELECT TOP 1 @donHoanTat=MaDH FROM DonHang WHERE MaND=@kh AND TrangThaiDon IN (N'Giao hàng thành công', N'Giao hàng thành công', N'Hoàn tất', N'HoanTat') ORDER BY MaDH DESC;
IF @donHoanTat IS NULL
BEGIN
  INSERT INTO DonHang(MaND, MaDC, MaNVGiaoHang, MaNVDuyet, NgayDat, TongTien, PhiVanChuyen, TrangThaiDon, MaNguoiDuyet)
  VALUES(@kh, NULL, @nvGiao, @nvDuyet, DATEADD(DAY,-1,GETDATE()), @tong, 0, N'Giao hàng thành công', @nvDuyet);
  SET @donHoanTat=SCOPE_IDENTITY();
END
ELSE
  UPDATE DonHang SET MaND=@kh, MaNVGiaoHang=@nvGiao, MaNVDuyet=@nvDuyet, NgayDat=DATEADD(DAY,-1,GETDATE()), TongTien=@tong, PhiVanChuyen=0, TrangThaiDon=N'Giao hàng thành công', MaNguoiDuyet=@nvDuyet WHERE MaDH=@donHoanTat;
DELETE FROM ChiTietDonHang WHERE MaDH=@donHoanTat;
INSERT INTO ChiTietDonHang(MaDH, MaSP, SoLuong, DonGia) VALUES(@donHoanTat, @sp1, 1, @gia1);
IF @sp2<>@sp1 INSERT INTO ChiTietDonHang(MaDH, MaSP, SoLuong, DonGia) VALUES(@donHoanTat, @sp2, 1, @gia2);
DELETE FROM ThanhToan WHERE MaDH=@donHoanTat;
INSERT INTO ThanhToan(MaDH, PhuongThuc, SoTien, NgayThanhToan, TrangThai) VALUES(@donHoanTat, N'COD', @tong, GETDATE(), N'Đã thanh toán');

IF NOT EXISTS (SELECT 1 FROM DanhGia WHERE MaND=@kh AND MaSP=@sp1)
  INSERT INTO DanhGia(MaSP, MaND, SoSao, BinhLuan, NgayDG, TrangThai, IsDeleted)
  VALUES(@sp1, @kh, 5, N'Đồ uống ngon, giao nhanh, đóng gói cẩn thận. Sẽ tiếp tục ủng hộ COFFEE 12.', GETDATE(), 1, 0);
ELSE
  UPDATE DanhGia SET SoSao=5, BinhLuan=N'Đồ uống ngon, giao nhanh, đóng gói cẩn thận. Sẽ tiếp tục ủng hộ COFFEE 12.', NgayDG=GETDATE(), TrangThai=1, IsDeleted=0 WHERE MaND=@kh AND MaSP=@sp1;

DECLARE @chat INT;
SELECT TOP 1 @chat=MaChat FROM AIChat WHERE MaND=@kh ORDER BY MaChat DESC;
IF @chat IS NULL
BEGIN
  INSERT INTO AIChat(MaND, TieuDe, MaSP, NgayTao) VALUES(@kh, N'Tư vấn chọn món theo khẩu vị', @sp1, GETDATE());
  SET @chat=SCOPE_IDENTITY();
END
ELSE
  UPDATE AIChat SET TieuDe=N'Tư vấn chọn món theo khẩu vị', MaSP=@sp1, NgayTao=GETDATE() WHERE MaChat=@chat;
DELETE FROM AITinNhan WHERE MaChat=@chat;
INSERT INTO AITinNhan(MaChat, LoaiNguoiGui, NoiDung, ThoiGian, DaDoc, Tokens)
VALUES
(@chat, N'KhachHang', N'Mình muốn uống món ít ngọt, có vị trái cây và không quá đậm cà phê.', DATEADD(MINUTE,-3,GETDATE()), 1, 18),
(@chat, N'AI', N'Bạn có thể chọn soda chanh, trà trái cây hoặc yogurt trái cây. Nếu thích vị nhẹ và tươi mát, soda chanh là lựa chọn phù hợp.', DATEADD(MINUTE,-2,GETDATE()), 1, 35),
(@chat, N'KhachHang', N'Món nào hợp để uống buổi chiều?', DATEADD(MINUTE,-1,GETDATE()), 1, 12),
(@chat, N'AI', N'Buổi chiều nên chọn trà trái cây hoặc soda để dễ uống. Nếu cần tỉnh táo nhẹ, bạn có thể chọn cà phê sữa ít ngọt.', GETDATE(), 1, 34);

PRINT N'SCREENSHOT_IDS:' + CAST(@sp1 AS NVARCHAR(20)) + N'|' + CAST(@donCho AS NVARCHAR(20)) + N'|' + CAST(@donDangGiao AS NVARCHAR(20)) + N'|' + CAST(@donHoanTat AS NVARCHAR(20));
`;
  const output = runSql(sql);
  const match = output.match(/SCREENSHOT_IDS:(\d+)\|(\d+)\|(\d+)\|(\d+)/);
  if (!match) {
    throw new Error(`Không đọc được ID dữ liệu mẫu từ SQL. Output:\n${output}`);
  }
  return {
    productId: match[1],
    pendingOrderId: match[2],
    shippingOrderId: match[3],
    completedOrderId: match[4]
  };
}

async function launchBrowser() {
  try {
    return await chromium.launch({ headless: true });
  } catch (error) {
    console.warn('Không mở được Chromium mặc định, thử dùng Microsoft Edge đã cài trên máy...');
    return chromium.launch({ channel: 'msedge', headless: true });
  }
}

async function gotoReady(page, url) {
  await page.goto(`${BASE_URL}${url}`, { waitUntil: 'networkidle', timeout: 45000 });
  await page.waitForTimeout(1000);
  const bodyText = await page.locator('body').innerText({ timeout: 5000 }).catch(() => '');
  if (/Server Error in '\/' Application|Compilation Error|Runtime Error/i.test(bodyText)) {
    throw new Error(`Trang ${url} đang lỗi server/compile. Hãy mở trực tiếp để xem stack trace.`);
  }
}

async function login(page, username, password) {
  await gotoReady(page, '/NguoiDungs/DangNhap');
  await page.locator('input[name="TenDangNhap"], input#TenDangNhap').first().fill(username);
  await page.locator('input[name="MatKhau"], input#MatKhau, input[type="password"]').first().fill(password);
  await Promise.all([
    page.waitForLoadState('networkidle').catch(() => {}),
    page.locator('button[type="submit"], input[type="submit"]').first().click()
  ]);
  await page.waitForTimeout(1000);
}

async function logout(page) {
  await page.goto(`${BASE_URL}/NguoiDungs/DangXuat`, { waitUntil: 'networkidle', timeout: 30000 }).catch(() => {});
  await page.waitForTimeout(500);
}

async function capture(page, fileName, url, options = {}) {
  await gotoReady(page, url);
  if (options.before) await options.before(page);
  const output = path.join(screenshotDir, fileName);
  await page.screenshot({ path: output, fullPage: false });
  console.log(`OK ${fileName}  <=  ${url}`);
}

async function main() {
  console.log('Đang chuẩn bị dữ liệu mẫu trong SQL Server...');
  const ids = seedData();
  console.log('Dữ liệu mẫu:', ids);

  const browser = await launchBrowser();
  const context = await browser.newContext({
    ignoreHTTPSErrors: true,
    viewport: { width: 1440, height: 900 }
  });
  const page = await context.newPage();
  page.setDefaultTimeout(20000);

  try {
    await login(page, 'khachhang01', '123456');
    await capture(page, 'Hinh_5_1_Trang_xem_menu_va_tim_kiem_san_pham.png', '/Home/Menu?page=1');
    await capture(page, 'Hinh_5_2_Trang_chi_tiet_san_pham.png', `/Home/ChiTietSanPham/${ids.productId}`);
    await capture(page, 'Hinh_5_3_Trang_gio_hang.png', '/GioHang');
    await capture(page, 'Hinh_5_4_Trang_xac_nhan_dat_hang_online.png', '/GioHang', {
      before: async (p) => {
        await p.locator('input[name="HoTenNguoiNhan"], input[name="HoTen"], input[placeholder*="người nhận" i]').first().fill('Nguyễn Văn Hưng').catch(() => {});
        await p.locator('input[name="SoDienThoai"], input[name="DienThoai"], input[placeholder*="điện thoại" i]').first().fill('0123456789').catch(() => {});
        await p.locator('textarea[name="DiaChiGiaoHang"], textarea[name="DiaChi"], textarea').first().fill('123 Nguyễn Huệ, Tổ Hiến Thành, Vạn Ninh, Khánh Hòa').catch(() => {});
        await p.waitForTimeout(300);
      }
    });
    await capture(page, 'Hinh_5_5_Thong_bao_dat_hang_thanh_cong.png', `/ThanhToan/ThanhToanThanhCong?maDH=${ids.completedOrderId}`);
    await capture(page, 'Hinh_5_6_Trang_theo_doi_trang_thai_don.png', '/DonHang/LichSu');
    await capture(page, 'Hinh_5_7_Trang_chat_ai_tu_van_khach_hang.png', '/AIChat');

    await logout(page);
    await login(page, 'nvduyet01', '123456');
    await capture(page, 'Hinh_5_8_Danh_sach_don_cho_duyet.png', '/NhanVienDuyetDon/Dashboard');
    await capture(page, 'Hinh_5_9_Duyet_don_va_phan_cong_giao_hang.png', '/NhanVienDuyetDon/Dashboard');

    await logout(page);
    await login(page, 'nvgiao01', '123456');
    await capture(page, 'Hinh_5_10_Cap_nhat_giao_hang_va_thanh_toan.png', '/NhanVienGiaoHang/Dashboard');
    await capture(page, 'Hinh_5_11_Thong_bao_giao_hang_thanh_toan_thanh_cong.png', `/NhanVienGiaoHang/GiaoHangThanhCong?maDH=${ids.shippingOrderId}`);

    await logout(page);
    await login(page, 'admin', '123456');
    await capture(page, 'Hinh_5_12_Dashboard_quan_tri.png', '/Admin/Index');
    await capture(page, 'Hinh_5_13_Quan_ly_san_pham_va_hinh_anh.png', '/SanPhamAdmin/Index');
    await capture(page, 'Hinh_5_14_Thong_ke_don_hang.png', '/ThongkeAdmin/DoanhThuThang');
    await capture(page, 'Hinh_5_15_Tra_cuu_danh_gia_va_lich_su_tu_van_AI.png', '/TheoDoiAdmin/DanhGiaVaChat');
  } finally {
    await browser.close();
  }

  console.log(`\nHoàn tất. Ảnh đã lưu tại: ${screenshotDir}`);
}

main().catch((error) => {
  console.error('\nCHỤP ẢNH THẤT BẠI:');
  console.error(error && error.stack ? error.stack : error);
  console.error('\nGợi ý kiểm tra: app đã chạy ở https://localhost:44300 chưa, SQL Server .\\SQLEXPRESS có database 12COFFEE chưa.');
  process.exit(1);
});







