/*
===============================================================================
 BẢN CUỐI - SQL HOÀN CHỈNH THEO BÁO CÁO CNPM COFFEE 12
 File này dùng làm nguồn chính để viết PHẦN 3. THIẾT KẾ DỮ LIỆU.
 Có đủ bảng nghiệp vụ, khóa chính, khóa ngoại, ràng buộc và dữ liệu mẫu.
 Lưu ý: Bảng HinhAnhSanPham lưu tên file ảnh, không chứa file ảnh vật lý .jpg.
===============================================================================
*/

/*
================================================================================
 CSDL COFFEE 12 - Hệ thống quản lý bán hàng cửa hàng COFFEE 12
 Phiên bản: 1.0
 Hệ quản trị: Microsoft SQL Server
 Nội dung:
   1. Tạo database COFFEE12_DB
   2. Tạo đầy đủ bảng dữ liệu theo báo cáo CNPM:
      - VaiTro
      - NguoiDung
      - DiaChiGiaoHang
      - DanhMuc
      - SanPham
      - HinhAnhSanPham
      - GioHang
      - ChiTietGioHang
      - DonHang
      - ChiTietDonHang
      - ThanhToan
      - DanhGia
      - AIChat
      - AITinNhan
   3. Tạo khóa chính, khóa ngoại, ràng buộc CHECK, DEFAULT, UNIQUE
   4. Thêm dữ liệu mẫu cơ bản: vai trò, người dùng, danh mục, sản phẩm 55-89,
      hình ảnh sản phẩm 55-89, một giỏ hàng, một đơn hàng mẫu, thanh toán,
      đánh giá và dữ liệu Chat AI mẫu.

 Lưu ý:
   - Dữ liệu sản phẩm 55-89 được suy ra từ danh sách hình ảnh bạn đã cung cấp.
   - Giá bán và số lượng tồn là dữ liệu mẫu để demo, có thể thay lại theo menu thật.
   - Mật khẩu trong dữ liệu mẫu chỉ là chuỗi minh họa, khi cài đặt thật cần mã hóa.
================================================================================
*/

IF DB_ID(N'COFFEE12_DB') IS NULL
BEGIN
    CREATE DATABASE COFFEE12_DB;
END;
GO

USE COFFEE12_DB;
GO

/* Xóa bảng cũ theo đúng thứ tự phụ thuộc để script có thể chạy lại nhiều lần */
IF OBJECT_ID(N'dbo.AITinNhan', N'U') IS NOT NULL DROP TABLE dbo.AITinNhan;
IF OBJECT_ID(N'dbo.AIChat', N'U') IS NOT NULL DROP TABLE dbo.AIChat;
IF OBJECT_ID(N'dbo.DanhGia', N'U') IS NOT NULL DROP TABLE dbo.DanhGia;
IF OBJECT_ID(N'dbo.ThanhToan', N'U') IS NOT NULL DROP TABLE dbo.ThanhToan;
IF OBJECT_ID(N'dbo.ChiTietDonHang', N'U') IS NOT NULL DROP TABLE dbo.ChiTietDonHang;
IF OBJECT_ID(N'dbo.DonHang', N'U') IS NOT NULL DROP TABLE dbo.DonHang;
IF OBJECT_ID(N'dbo.ChiTietGioHang', N'U') IS NOT NULL DROP TABLE dbo.ChiTietGioHang;
IF OBJECT_ID(N'dbo.GioHang', N'U') IS NOT NULL DROP TABLE dbo.GioHang;
IF OBJECT_ID(N'dbo.HinhAnhSanPham', N'U') IS NOT NULL DROP TABLE dbo.HinhAnhSanPham;
IF OBJECT_ID(N'dbo.SanPham', N'U') IS NOT NULL DROP TABLE dbo.SanPham;
IF OBJECT_ID(N'dbo.DanhMuc', N'U') IS NOT NULL DROP TABLE dbo.DanhMuc;
IF OBJECT_ID(N'dbo.DiaChiGiaoHang', N'U') IS NOT NULL DROP TABLE dbo.DiaChiGiaoHang;
IF OBJECT_ID(N'dbo.NguoiDung', N'U') IS NOT NULL DROP TABLE dbo.NguoiDung;
IF OBJECT_ID(N'dbo.VaiTro', N'U') IS NOT NULL DROP TABLE dbo.VaiTro;
GO

/* ============================================================================
   1. NHÓM DỮ LIỆU NGƯỜI DÙNG
============================================================================ */

CREATE TABLE dbo.VaiTro (
    MaVaiTro        INT IDENTITY(1,1) NOT NULL,
    TenVaiTro       NVARCHAR(50)      NOT NULL,
    MoTa            NVARCHAR(255)     NULL,
    TrangThai       BIT               NOT NULL CONSTRAINT DF_VaiTro_TrangThai DEFAULT (1),
    CONSTRAINT PK_VaiTro PRIMARY KEY (MaVaiTro),
    CONSTRAINT UQ_VaiTro_TenVaiTro UNIQUE (TenVaiTro),
    CONSTRAINT CK_VaiTro_TenVaiTro CHECK (TenVaiTro IN (
        N'QuanTri',
        N'NhanVienDuyetDon',
        N'NhanVienGiaoHang',
        N'KhachHang'
    ))
);
GO

CREATE TABLE dbo.NguoiDung (
    MaND            INT IDENTITY(1,1) NOT NULL,
    MaVaiTro        INT               NOT NULL,
    TenDangNhap     NVARCHAR(50)      NOT NULL,
    MatKhau         NVARCHAR(255)     NOT NULL,
    HoTen           NVARCHAR(100)     NOT NULL,
    Email           NVARCHAR(100)     NULL,
    SoDienThoai     NVARCHAR(20)      NULL,
    TrangThai       BIT               NOT NULL CONSTRAINT DF_NguoiDung_TrangThai DEFAULT (1),
    NgayTao         DATETIME2(0)      NOT NULL CONSTRAINT DF_NguoiDung_NgayTao DEFAULT (SYSDATETIME()),
    CONSTRAINT PK_NguoiDung PRIMARY KEY (MaND),
    CONSTRAINT FK_NguoiDung_VaiTro FOREIGN KEY (MaVaiTro) REFERENCES dbo.VaiTro(MaVaiTro),
    CONSTRAINT UQ_NguoiDung_TenDangNhap UNIQUE (TenDangNhap)
);
GO

CREATE UNIQUE INDEX UX_NguoiDung_Email_NotNull
ON dbo.NguoiDung(Email)
WHERE Email IS NOT NULL;
GO

CREATE TABLE dbo.DiaChiGiaoHang (
    MaDiaChi         INT IDENTITY(1,1) NOT NULL,
    MaKH             INT               NOT NULL,
    HoTenNguoiNhan   NVARCHAR(100)     NOT NULL,
    SoDienThoaiNhan  NVARCHAR(20)      NOT NULL,
    DiaChiChiTiet    NVARCHAR(255)     NOT NULL,
    PhuongXa         NVARCHAR(100)     NULL,
    QuanHuyen        NVARCHAR(100)     NULL,
    TinhThanh        NVARCHAR(100)     NULL,
    MacDinh          BIT               NOT NULL CONSTRAINT DF_DiaChi_MacDinh DEFAULT (0),
    TrangThai        BIT               NOT NULL CONSTRAINT DF_DiaChi_TrangThai DEFAULT (1),
    NgayTao          DATETIME2(0)      NOT NULL CONSTRAINT DF_DiaChi_NgayTao DEFAULT (SYSDATETIME()),
    CONSTRAINT PK_DiaChiGiaoHang PRIMARY KEY (MaDiaChi),
    CONSTRAINT FK_DiaChi_NguoiDung FOREIGN KEY (MaKH) REFERENCES dbo.NguoiDung(MaND)
);
GO

/* ============================================================================
   2. NHÓM DỮ LIỆU MENU VÀ SẢN PHẨM
============================================================================ */

CREATE TABLE dbo.DanhMuc (
    MaDanhMuc    INT IDENTITY(1,1) NOT NULL,
    TenDanhMuc   NVARCHAR(100)     NOT NULL,
    MoTa         NVARCHAR(255)     NULL,
    ThuTu        INT               NOT NULL CONSTRAINT DF_DanhMuc_ThuTu DEFAULT (0),
    TrangThai    BIT               NOT NULL CONSTRAINT DF_DanhMuc_TrangThai DEFAULT (1),
    CONSTRAINT PK_DanhMuc PRIMARY KEY (MaDanhMuc),
    CONSTRAINT UQ_DanhMuc_TenDanhMuc UNIQUE (TenDanhMuc)
);
GO

CREATE TABLE dbo.SanPham (
    MaSP                 INT IDENTITY(1,1) NOT NULL,
    MaDanhMuc            INT               NOT NULL,
    TenSP                NVARCHAR(150)     NOT NULL,
    MoTa                 NVARCHAR(500)     NULL,
    GiaBan               DECIMAL(18,2)     NOT NULL,
    SoLuongTon           INT               NOT NULL CONSTRAINT DF_SanPham_SoLuongTon DEFAULT (0),
    TrangThaiKinhDoanh   BIT               NOT NULL CONSTRAINT DF_SanPham_TrangThaiKD DEFAULT (1),
    NgayTao              DATETIME2(0)      NOT NULL CONSTRAINT DF_SanPham_NgayTao DEFAULT (SYSDATETIME()),
    NgayCapNhat          DATETIME2(0)      NULL,
    CONSTRAINT PK_SanPham PRIMARY KEY (MaSP),
    CONSTRAINT FK_SanPham_DanhMuc FOREIGN KEY (MaDanhMuc) REFERENCES dbo.DanhMuc(MaDanhMuc),
    CONSTRAINT CK_SanPham_GiaBan CHECK (GiaBan >= 0),
    CONSTRAINT CK_SanPham_SoLuongTon CHECK (SoLuongTon >= 0)
);
GO

CREATE TABLE dbo.HinhAnhSanPham (
    MaHinhAnh     INT IDENTITY(1,1) NOT NULL,
    MaSP          INT               NOT NULL,
    TenFileAnh    NVARCHAR(255)     NOT NULL,
    MoTaAnh       NVARCHAR(255)     NULL,
    AnhChinh      BIT               NOT NULL CONSTRAINT DF_HinhAnh_AnhChinh DEFAULT (1),
    ThuTu         INT               NOT NULL CONSTRAINT DF_HinhAnh_ThuTu DEFAULT (1),
    NgayTao       DATETIME2(0)      NOT NULL CONSTRAINT DF_HinhAnh_NgayTao DEFAULT (SYSDATETIME()),
    CONSTRAINT PK_HinhAnhSanPham PRIMARY KEY (MaHinhAnh),
    CONSTRAINT FK_HinhAnh_SanPham FOREIGN KEY (MaSP) REFERENCES dbo.SanPham(MaSP) ON DELETE CASCADE
);
GO

/* ============================================================================
   3. NHÓM DỮ LIỆU GIỎ HÀNG
============================================================================ */

CREATE TABLE dbo.GioHang (
    MaGioHang     INT IDENTITY(1,1) NOT NULL,
    MaKH          INT               NOT NULL,
    TrangThai     NVARCHAR(30)      NOT NULL CONSTRAINT DF_GioHang_TrangThai DEFAULT (N'DangMo'),
    NgayTao       DATETIME2(0)      NOT NULL CONSTRAINT DF_GioHang_NgayTao DEFAULT (SYSDATETIME()),
    NgayCapNhat   DATETIME2(0)      NULL,
    CONSTRAINT PK_GioHang PRIMARY KEY (MaGioHang),
    CONSTRAINT FK_GioHang_NguoiDung FOREIGN KEY (MaKH) REFERENCES dbo.NguoiDung(MaND),
    CONSTRAINT CK_GioHang_TrangThai CHECK (TrangThai IN (N'DangMo', N'DaDat', N'DaHuy'))
);
GO

CREATE TABLE dbo.ChiTietGioHang (
    MaCTGH       INT IDENTITY(1,1) NOT NULL,
    MaGioHang    INT               NOT NULL,
    MaSP         INT               NOT NULL,
    SoLuong      INT               NOT NULL,
    DonGia       DECIMAL(18,2)     NOT NULL,
    ThanhTien    AS (CONVERT(DECIMAL(18,2), SoLuong * DonGia)) PERSISTED,
    NgayThem     DATETIME2(0)      NOT NULL CONSTRAINT DF_CTGH_NgayThem DEFAULT (SYSDATETIME()),
    CONSTRAINT PK_ChiTietGioHang PRIMARY KEY (MaCTGH),
    CONSTRAINT FK_CTGH_GioHang FOREIGN KEY (MaGioHang) REFERENCES dbo.GioHang(MaGioHang) ON DELETE CASCADE,
    CONSTRAINT FK_CTGH_SanPham FOREIGN KEY (MaSP) REFERENCES dbo.SanPham(MaSP),
    CONSTRAINT UQ_CTGH_GioHang_SanPham UNIQUE (MaGioHang, MaSP),
    CONSTRAINT CK_CTGH_SoLuong CHECK (SoLuong > 0),
    CONSTRAINT CK_CTGH_DonGia CHECK (DonGia >= 0)
);
GO

/* ============================================================================
   4. NHÓM DỮ LIỆU ĐƠN HÀNG
============================================================================ */

CREATE TABLE dbo.DonHang (
    MaDonHang             INT IDENTITY(1,1) NOT NULL,
    MaKH                  INT               NOT NULL,
    MaDiaChi              INT               NULL,
    MaNVDuyet             INT               NULL,
    MaNVGiaoHang          INT               NULL,
    HoTenNguoiNhan        NVARCHAR(100)     NOT NULL,
    SoDienThoaiNhan       NVARCHAR(20)      NOT NULL,
    DiaChiNhanHang        NVARCHAR(255)     NOT NULL,
    PhuongThucThanhToan   NVARCHAR(30)      NOT NULL,
    PhiVanChuyen          DECIMAL(18,2)     NOT NULL CONSTRAINT DF_DonHang_PhiVanChuyen DEFAULT (0),
    TongTienHang          DECIMAL(18,2)     NOT NULL CONSTRAINT DF_DonHang_TongTienHang DEFAULT (0),
    TongThanhToan         DECIMAL(18,2)     NOT NULL CONSTRAINT DF_DonHang_TongThanhToan DEFAULT (0),
    TrangThaiDon          NVARCHAR(30)      NOT NULL CONSTRAINT DF_DonHang_TrangThaiDon DEFAULT (N'ChoDuyet'),
    GhiChuKhachHang       NVARCHAR(500)     NULL,
    GhiChuXuLy            NVARCHAR(500)     NULL,
    LyDoTuChoi            NVARCHAR(500)     NULL,
    NgayDat               DATETIME2(0)      NOT NULL CONSTRAINT DF_DonHang_NgayDat DEFAULT (SYSDATETIME()),
    NgayDuyet             DATETIME2(0)      NULL,
    NgayGiaoHoanTat       DATETIME2(0)      NULL,
    CONSTRAINT PK_DonHang PRIMARY KEY (MaDonHang),
    CONSTRAINT FK_DonHang_KhachHang FOREIGN KEY (MaKH) REFERENCES dbo.NguoiDung(MaND),
    CONSTRAINT FK_DonHang_DiaChi FOREIGN KEY (MaDiaChi) REFERENCES dbo.DiaChiGiaoHang(MaDiaChi),
    CONSTRAINT FK_DonHang_NVDuyet FOREIGN KEY (MaNVDuyet) REFERENCES dbo.NguoiDung(MaND),
    CONSTRAINT FK_DonHang_NVGiaoHang FOREIGN KEY (MaNVGiaoHang) REFERENCES dbo.NguoiDung(MaND),
    CONSTRAINT CK_DonHang_PhuongThucTT CHECK (PhuongThucThanhToan IN (N'TienMat', N'ChuyenKhoan', N'ViDienTu')),
    CONSTRAINT CK_DonHang_TrangThai CHECK (TrangThaiDon IN (N'ChoDuyet', N'DaDuyet', N'DangGiao', N'HoanTat', N'TuChoi', N'Huy')),
    CONSTRAINT CK_DonHang_PhiVanChuyen CHECK (PhiVanChuyen >= 0),
    CONSTRAINT CK_DonHang_TongTienHang CHECK (TongTienHang >= 0),
    CONSTRAINT CK_DonHang_TongThanhToan CHECK (TongThanhToan >= 0)
);
GO

CREATE TABLE dbo.ChiTietDonHang (
    MaCTDH          INT IDENTITY(1,1) NOT NULL,
    MaDonHang       INT               NOT NULL,
    MaSP            INT               NOT NULL,
    TenSPSnapshot   NVARCHAR(150)     NOT NULL,
    SoLuong         INT               NOT NULL,
    DonGia          DECIMAL(18,2)     NOT NULL,
    ThanhTien       AS (CONVERT(DECIMAL(18,2), SoLuong * DonGia)) PERSISTED,
    CONSTRAINT PK_ChiTietDonHang PRIMARY KEY (MaCTDH),
    CONSTRAINT FK_CTDH_DonHang FOREIGN KEY (MaDonHang) REFERENCES dbo.DonHang(MaDonHang) ON DELETE CASCADE,
    CONSTRAINT FK_CTDH_SanPham FOREIGN KEY (MaSP) REFERENCES dbo.SanPham(MaSP),
    CONSTRAINT CK_CTDH_SoLuong CHECK (SoLuong > 0),
    CONSTRAINT CK_CTDH_DonGia CHECK (DonGia >= 0)
);
GO

/* ============================================================================
   5. NHÓM DỮ LIỆU THANH TOÁN, ĐÁNH GIÁ VÀ CHAT AI
============================================================================ */

CREATE TABLE dbo.ThanhToan (
    MaThanhToan          INT IDENTITY(1,1) NOT NULL,
    MaDonHang            INT               NOT NULL,
    PhuongThucThanhToan  NVARCHAR(30)      NOT NULL,
    SoTienThanhToan      DECIMAL(18,2)     NOT NULL,
    TrangThaiThanhToan   NVARCHAR(30)      NOT NULL CONSTRAINT DF_ThanhToan_TrangThai DEFAULT (N'ChuaThanhToan'),
    ThoiDiemThanhToan    DATETIME2(0)      NULL,
    GhiChu               NVARCHAR(500)     NULL,
    CONSTRAINT PK_ThanhToan PRIMARY KEY (MaThanhToan),
    CONSTRAINT UQ_ThanhToan_MaDonHang UNIQUE (MaDonHang),
    CONSTRAINT FK_ThanhToan_DonHang FOREIGN KEY (MaDonHang) REFERENCES dbo.DonHang(MaDonHang) ON DELETE CASCADE,
    CONSTRAINT CK_ThanhToan_PhuongThuc CHECK (PhuongThucThanhToan IN (N'TienMat', N'ChuyenKhoan', N'ViDienTu')),
    CONSTRAINT CK_ThanhToan_TrangThai CHECK (TrangThaiThanhToan IN (N'ChuaThanhToan', N'DaThanhToan', N'ThatBai', N'HoanTien')),
    CONSTRAINT CK_ThanhToan_SoTien CHECK (SoTienThanhToan >= 0)
);
GO

CREATE TABLE dbo.DanhGia (
    MaDanhGia     INT IDENTITY(1,1) NOT NULL,
    MaDonHang     INT               NOT NULL,
    MaSP          INT               NOT NULL,
    MaKH          INT               NOT NULL,
    SoSao         INT               NOT NULL,
    NoiDung       NVARCHAR(1000)    NULL,
    NgayDanhGia   DATETIME2(0)      NOT NULL CONSTRAINT DF_DanhGia_Ngay DEFAULT (SYSDATETIME()),
    TrangThai     BIT               NOT NULL CONSTRAINT DF_DanhGia_TrangThai DEFAULT (1),
    CONSTRAINT PK_DanhGia PRIMARY KEY (MaDanhGia),
    CONSTRAINT FK_DanhGia_DonHang FOREIGN KEY (MaDonHang) REFERENCES dbo.DonHang(MaDonHang),
    CONSTRAINT FK_DanhGia_SanPham FOREIGN KEY (MaSP) REFERENCES dbo.SanPham(MaSP),
    CONSTRAINT FK_DanhGia_NguoiDung FOREIGN KEY (MaKH) REFERENCES dbo.NguoiDung(MaND),
    CONSTRAINT UQ_DanhGia_DonHang_SanPham_KH UNIQUE (MaDonHang, MaSP, MaKH),
    CONSTRAINT CK_DanhGia_SoSao CHECK (SoSao BETWEEN 1 AND 5)
);
GO

CREATE TABLE dbo.AIChat (
    MaChat          INT IDENTITY(1,1) NOT NULL,
    MaKH            INT               NULL,
    TieuDe          NVARCHAR(200)     NULL,
    ThoiDiemBatDau  DATETIME2(0)      NOT NULL CONSTRAINT DF_AIChat_BatDau DEFAULT (SYSDATETIME()),
    ThoiDiemKetThuc DATETIME2(0)      NULL,
    TrangThai       NVARCHAR(30)      NOT NULL CONSTRAINT DF_AIChat_TrangThai DEFAULT (N'DangMo'),
    CONSTRAINT PK_AIChat PRIMARY KEY (MaChat),
    CONSTRAINT FK_AIChat_NguoiDung FOREIGN KEY (MaKH) REFERENCES dbo.NguoiDung(MaND),
    CONSTRAINT CK_AIChat_TrangThai CHECK (TrangThai IN (N'DangMo', N'DaDong'))
);
GO

CREATE TABLE dbo.AITinNhan (
    MaTinNhan     INT IDENTITY(1,1) NOT NULL,
    MaChat        INT               NOT NULL,
    NguoiGui      NVARCHAR(30)      NOT NULL,
    NoiDung       NVARCHAR(MAX)     NOT NULL,
    ThoiDiemGui   DATETIME2(0)      NOT NULL CONSTRAINT DF_AITinNhan_ThoiDiem DEFAULT (SYSDATETIME()),
    ThuTu         INT               NOT NULL CONSTRAINT DF_AITinNhan_ThuTu DEFAULT (1),
    CONSTRAINT PK_AITinNhan PRIMARY KEY (MaTinNhan),
    CONSTRAINT FK_AITinNhan_AIChat FOREIGN KEY (MaChat) REFERENCES dbo.AIChat(MaChat) ON DELETE CASCADE,
    CONSTRAINT CK_AITinNhan_NguoiGui CHECK (NguoiGui IN (N'KhachHang', N'AI', N'HeThong'))
);
GO

/* ============================================================================
   6. INDEX HỖ TRỢ TRA CỨU
============================================================================ */

CREATE INDEX IX_SanPham_MaDanhMuc ON dbo.SanPham(MaDanhMuc);
CREATE INDEX IX_SanPham_TenSP ON dbo.SanPham(TenSP);
CREATE INDEX IX_HinhAnhSanPham_MaSP ON dbo.HinhAnhSanPham(MaSP);
CREATE INDEX IX_GioHang_MaKH ON dbo.GioHang(MaKH);
CREATE INDEX IX_DonHang_MaKH ON dbo.DonHang(MaKH);
CREATE INDEX IX_DonHang_TrangThaiDon ON dbo.DonHang(TrangThaiDon);
CREATE INDEX IX_DonHang_MaNVDuyet ON dbo.DonHang(MaNVDuyet);
CREATE INDEX IX_DonHang_MaNVGiaoHang ON dbo.DonHang(MaNVGiaoHang);
CREATE INDEX IX_ChiTietDonHang_MaDonHang ON dbo.ChiTietDonHang(MaDonHang);
CREATE INDEX IX_DanhGia_MaSP ON dbo.DanhGia(MaSP);
CREATE INDEX IX_AIChat_MaKH ON dbo.AIChat(MaKH);
GO

/* ============================================================================
   7. DỮ LIỆU MẪU
============================================================================ */

INSERT INTO dbo.VaiTro (TenVaiTro, MoTa) VALUES
(N'QuanTri', N'Quản trị viên quản lý danh mục, sản phẩm, người dùng và giám sát đơn hàng'),
(N'NhanVienDuyetDon', N'Nhân viên tiếp nhận, kiểm tra, duyệt đơn và phân công giao hàng'),
(N'NhanVienGiaoHang', N'Nhân viên giao hàng, cập nhật trạng thái giao và thanh toán'),
(N'KhachHang', N'Khách hàng xem menu, quản lý giỏ hàng, đặt hàng, đánh giá và dùng Chat AI');
GO

INSERT INTO dbo.NguoiDung (MaVaiTro, TenDangNhap, MatKhau, HoTen, Email, SoDienThoai) VALUES
(1, N'admin', N'123456', N'Quản trị COFFEE 12', N'admin@coffee12.vn', N'0900000001'),
(2, N'nvduyet01', N'123456', N'Nhân viên duyệt đơn 01', N'nvduyet01@coffee12.vn', N'0900000002'),
(3, N'nvgiao01', N'123456', N'Nhân viên giao hàng 01', N'nvgiao01@coffee12.vn', N'0900000003'),
(4, N'khachhang01', N'123456', N'Nguyễn Văn Khách', N'khachhang01@gmail.com', N'0900000004');
GO

INSERT INTO dbo.DiaChiGiaoHang
(MaKH, HoTenNguoiNhan, SoDienThoaiNhan, DiaChiChiTiet, PhuongXa, QuanHuyen, TinhThanh, MacDinh)
VALUES
(4, N'Nguyễn Văn Khách', N'0900000004', N'12 Tô Hiến Thành, Vạn Giã', N'Vạn Giã', N'Vạn Ninh', N'Khánh Hòa', 1);
GO

SET IDENTITY_INSERT dbo.DanhMuc ON;
INSERT INTO dbo.DanhMuc (MaDanhMuc, TenDanhMuc, MoTa, ThuTu, TrangThai) VALUES
(1, N'Soda', N'Nhóm thức uống soda trái cây', 1, 1),
(2, N'Nước ép', N'Nhóm nước ép trái cây và rau củ', 2, 1),
(3, N'Hibi', N'Nhóm thức uống Hibi', 3, 1),
(4, N'Trà trái cây', N'Nhóm trà trái cây', 4, 1),
(5, N'Trà sữa và Ô Long', N'Nhóm trà sữa, sữa tươi và ô long', 5, 1),
(6, N'Yogurt', N'Nhóm yogurt', 6, 1);
SET IDENTITY_INSERT dbo.DanhMuc OFF;
GO

SET IDENTITY_INSERT dbo.SanPham ON;
INSERT INTO dbo.SanPham (MaSP, MaDanhMuc, TenSP, MoTa, GiaBan, SoLuongTon, TrangThaiKinhDoanh) VALUES
(55, 1, N'Soda Việt Quất', N'Soda vị việt quất', 30000, 50, 1),
(56, 1, N'Soda Chanh', N'Soda vị chanh', 28000, 50, 1),
(57, 1, N'Soda Táo Xanh', N'Soda vị táo xanh', 30000, 50, 1),
(58, 1, N'Soda Tắc 12 Coffee', N'Soda tắc theo phong cách 12 Coffee', 30000, 50, 1),
(59, 2, N'Nước Ép Táo', N'Nước ép táo', 30000, 50, 1),
(60, 2, N'Nước Ép Ổi', N'Nước ép ổi', 30000, 50, 1),
(61, 2, N'Nước Ép Thơm', N'Nước ép thơm', 30000, 50, 1),
(62, 2, N'Nước Ép Cà Rốt', N'Nước ép cà rốt', 30000, 50, 1),
(63, 2, N'Nước Ép Dưa Hấu', N'Nước ép dưa hấu', 30000, 50, 1),
(64, 2, N'Nước Ép Cà Chua', N'Nước ép cà chua', 30000, 50, 1),
(65, 2, N'Nước Ép Cam', N'Nước ép cam', 30000, 50, 1),
(66, 3, N'Hibi Vải', N'Thức uống Hibi vị vải', 32000, 50, 1),
(67, 3, N'Hibi Đào', N'Thức uống Hibi vị đào', 32000, 50, 1),
(68, 3, N'Hibi Xoài', N'Thức uống Hibi vị xoài', 32000, 50, 1),
(69, 3, N'Hibi Khế', N'Thức uống Hibi vị khế', 32000, 50, 1),
(70, 3, N'Hibi Me', N'Thức uống Hibi vị me', 32000, 50, 1),
(71, 4, N'Trà Xoài Muối Ớt', N'Trà xoài muối ớt', 35000, 50, 1),
(72, 4, N'Trà Khế Táo', N'Trà khế táo', 35000, 50, 1),
(73, 4, N'Trà Sen Vàng', N'Trà sen vàng', 35000, 50, 1),
(74, 4, N'Trà Đào', N'Trà đào', 35000, 50, 1),
(75, 4, N'Trà Vải', N'Trà vải', 35000, 50, 1),
(76, 4, N'Trà Nhiệt Đới', N'Trà nhiệt đới', 35000, 50, 1),
(77, 4, N'Trà Tắc Mơ', N'Trà tắc mơ', 35000, 50, 1),
(78, 4, N'Trà Xí Muội', N'Trà xí muội', 35000, 50, 1),
(79, 4, N'Trà Me', N'Trà me', 35000, 50, 1),
(80, 5, N'Sữa Tươi Trân Châu Đường Đen', N'Sữa tươi trân châu đường đen', 38000, 50, 1),
(81, 5, N'Ô Long Trân Châu Đường Đen', N'Ô long trân châu đường đen', 38000, 50, 1),
(82, 5, N'Ô Long Truyền Thống', N'Ô long truyền thống', 35000, 50, 1),
(83, 5, N'Ô Long Cốt Dừa', N'Ô long cốt dừa', 37000, 50, 1),
(84, 5, N'Ô Long Hạnh Nhân', N'Ô long hạnh nhân', 37000, 50, 1),
(85, 5, N'Trà Sữa Lài Đậu Biếc', N'Trà sữa lài đậu biếc', 37000, 50, 1),
(86, 5, N'Trà Sữa Lài Đậu Xanh', N'Trà sữa lài đậu xanh', 37000, 50, 1),
(87, 6, N'Yogurt Hạt Đác', N'Yogurt hạt đác', 35000, 50, 1),
(88, 6, N'Yogurt Việt Quất', N'Yogurt việt quất', 35000, 50, 1),
(89, 6, N'Yogurt', N'Yogurt truyền thống', 30000, 50, 1);
SET IDENTITY_INSERT dbo.SanPham OFF;
GO

INSERT INTO dbo.HinhAnhSanPham (MaSP, TenFileAnh, MoTaAnh, AnhChinh, ThuTu) VALUES
(55, 'soda-viet-quat.jpg', N'Ảnh Soda Việt Quất', 1, 1),
(56, 'soda-chanh.jpg', N'Ảnh Soda Chanh', 1, 1),
(57, 'soda-tao-xanh.jpg', N'Ảnh Soda Táo Xanh', 1, 1),
(58, 'soda-tac-12coffee.jpg', N'Ảnh Soda Tắc 12 Coffee', 1, 1),
(59, 'nuocep-tao.jpg', N'Ảnh Nước Ép Táo', 1, 1),
(60, 'nuocep-oi.jpg', N'Ảnh Nước Ép Ổi', 1, 1),
(61, 'nuocep-thom.jpg', N'Ảnh Nước Ép Thơm', 1, 1),
(62, 'nuocep-carot.jpg', N'Ảnh Nước Ép Cà Rốt', 1, 1),
(63, 'nuocep-duahau.jpg', N'Ảnh Nước Ép Dưa Hấu', 1, 1),
(64, 'nuocep-cachua.jpg', N'Ảnh Nước Ép Cà Chua', 1, 1),
(65, 'nuocep-cam.jpg', N'Ảnh Nước Ép Cam', 1, 1),
(66, 'hibi-vai-2.jpg', N'Ảnh Hibi Vải', 1, 1),
(67, 'hibi-dao-2.jpg', N'Ảnh Hibi Đào', 1, 1),
(68, 'hibi-xoai-2.jpg', N'Ảnh Hibi Xoài', 1, 1),
(69, 'hibi-khe-2.jpg', N'Ảnh Hibi Khế', 1, 1),
(70, 'hibi-me-2.jpg', N'Ảnh Hibi Me', 1, 1),
(71, 'tra-xoai-muoi-ot.jpg', N'Ảnh Trà Xoài Muối Ớt', 1, 1),
(72, 'tra-khe-tao.jpg', N'Ảnh Trà Khế Táo', 1, 1),
(73, 'tra-sen-vang.jpg', N'Ảnh Trà Sen Vàng', 1, 1),
(74, 'tra-dao.jpg', N'Ảnh Trà Đào', 1, 1),
(75, 'tra-vai.jpg', N'Ảnh Trà Vải', 1, 1),
(76, 'tra-nhiet-doi.jpg', N'Ảnh Trà Nhiệt Đới', 1, 1),
(77, 'tra-tac-mo.jpg', N'Ảnh Trà Tắc Mơ', 1, 1),
(78, 'tra-xi-muoi.jpg', N'Ảnh Trà Xí Muội', 1, 1),
(79, 'tra-me.jpg', N'Ảnh Trà Me', 1, 1),
(80, 'sua-tuoi-tc-duongden-2.jpg', N'Ảnh Sữa Tươi Trân Châu Đường Đen', 1, 1),
(81, 'olong-tc-duongden-2.jpg', N'Ảnh Ô Long Trân Châu Đường Đen', 1, 1),
(82, 'olong-truyen-thong.jpg', N'Ảnh Ô Long Truyền Thống', 1, 1),
(83, 'olong-cot-dua.jpg', N'Ảnh Ô Long Cốt Dừa', 1, 1),
(84, 'olong-hanh-nhan.jpg', N'Ảnh Ô Long Hạnh Nhân', 1, 1),
(85, 'trasua-lai-daubiec.jpg', N'Ảnh Trà Sữa Lài Đậu Biếc', 1, 1),
(86, 'trasua-lai-dauxanh.jpg', N'Ảnh Trà Sữa Lài Đậu Xanh', 1, 1),
(87, 'yogurt-hat-dac.jpg', N'Ảnh Yogurt Hạt Đác', 1, 1),
(88, 'yogurt-vietquat.jpg', N'Ảnh Yogurt Việt Quất', 1, 1),
(89, 'yogurt.jpg', N'Ảnh Yogurt', 1, 1);
GO

/* Giỏ hàng mẫu */
INSERT INTO dbo.GioHang (MaKH, TrangThai) VALUES
(4, N'DangMo');

INSERT INTO dbo.ChiTietGioHang (MaGioHang, MaSP, SoLuong, DonGia) VALUES
(1, 55, 2, 30000),
(1, 74, 1, 35000);
GO

/* Đơn hàng mẫu đã hoàn tất để có dữ liệu thanh toán và đánh giá */
INSERT INTO dbo.DonHang
(MaKH, MaDiaChi, MaNVDuyet, MaNVGiaoHang, HoTenNguoiNhan, SoDienThoaiNhan, DiaChiNhanHang,
 PhuongThucThanhToan, PhiVanChuyen, TongTienHang, TongThanhToan, TrangThaiDon,
 GhiChuKhachHang, GhiChuXuLy, NgayDuyet, NgayGiaoHoanTat)
VALUES
(4, 1, 2, 3, N'Nguyễn Văn Khách', N'0900000004', N'12 Tô Hiến Thành, Vạn Giã, Vạn Ninh, Khánh Hòa',
 N'TienMat', 5000, 95000, 100000, N'HoanTat',
 N'Giao trong giờ hành chính', N'Đơn đã duyệt và giao thành công', SYSDATETIME(), SYSDATETIME());

INSERT INTO dbo.ChiTietDonHang (MaDonHang, MaSP, TenSPSnapshot, SoLuong, DonGia) VALUES
(1, 55, N'Soda Việt Quất', 2, 30000),
(1, 74, N'Trà Đào', 1, 35000);

INSERT INTO dbo.ThanhToan
(MaDonHang, PhuongThucThanhToan, SoTienThanhToan, TrangThaiThanhToan, ThoiDiemThanhToan, GhiChu)
VALUES
(1, N'TienMat', 100000, N'DaThanhToan', SYSDATETIME(), N'Khách thanh toán khi nhận hàng');

INSERT INTO dbo.DanhGia (MaDonHang, MaSP, MaKH, SoSao, NoiDung)
VALUES
(1, 55, 4, 5, N'Soda ngon, giao hàng nhanh.');
GO

/* Dữ liệu Chat AI mẫu */
INSERT INTO dbo.AIChat (MaKH, TieuDe, TrangThai)
VALUES
(4, N'Tư vấn chọn thức uống', N'DaDong');

INSERT INTO dbo.AITinNhan (MaChat, NguoiGui, NoiDung, ThuTu) VALUES
(1, N'KhachHang', N'Hôm nay tôi muốn uống món gì mát và dễ uống?', 1),
(1, N'AI', N'Bạn có thể thử Trà Đào, Trà Vải hoặc Soda Chanh nếu muốn thức uống mát và dễ uống.', 2);
GO

/* ============================================================================
   8. TRUY VẤN KIỂM TRA NHANH
============================================================================ */

SELECT N'VaiTro' AS Bang, COUNT(*) AS SoDong FROM dbo.VaiTro
UNION ALL SELECT N'NguoiDung', COUNT(*) FROM dbo.NguoiDung
UNION ALL SELECT N'DanhMuc', COUNT(*) FROM dbo.DanhMuc
UNION ALL SELECT N'SanPham', COUNT(*) FROM dbo.SanPham
UNION ALL SELECT N'HinhAnhSanPham', COUNT(*) FROM dbo.HinhAnhSanPham
UNION ALL SELECT N'GioHang', COUNT(*) FROM dbo.GioHang
UNION ALL SELECT N'ChiTietGioHang', COUNT(*) FROM dbo.ChiTietGioHang
UNION ALL SELECT N'DonHang', COUNT(*) FROM dbo.DonHang
UNION ALL SELECT N'ChiTietDonHang', COUNT(*) FROM dbo.ChiTietDonHang
UNION ALL SELECT N'ThanhToan', COUNT(*) FROM dbo.ThanhToan
UNION ALL SELECT N'DanhGia', COUNT(*) FROM dbo.DanhGia
UNION ALL SELECT N'AIChat', COUNT(*) FROM dbo.AIChat
UNION ALL SELECT N'AITinNhan', COUNT(*) FROM dbo.AITinNhan;
GO

PRINT N'Đã tạo xong CSDL COFFEE12_DB đầy đủ theo báo cáo CNPM.';
GO
