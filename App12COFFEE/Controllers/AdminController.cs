using App12COFFEE.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace App12COFFEE.Controllers
{
    public class AdminController : Controller
    {
        private Entities db = new Entities();
        // ================== TRANG CHÀO MỪNG / DASHBOARD ==================
        public ActionResult Index()
        {
            ViewBag.TongNguoiDung = db.NguoiDungs.Count();
            ViewBag.TongSanPham = db.SanPhams.Count();
            ViewBag.DonHangChoXacNhan = db.DonHangs.Count(dh => dh.TrangThaiDon == "Chờ xác nhận");
            ViewBag.DonHangDaDuyet = db.DonHangs.Count(dh => dh.TrangThaiDon == "Đã xác nhận");

            return View();
        }

        // ================== XEM DANH SÁCH ĐƠN HÀNG CHỜ ==================
        public ActionResult IndexDonHang()
        {
            var donHangs = db.DonHangs
                             .Where(dh => dh.TrangThaiDon == "Chờ xác nhận")
                             .ToList();
            return View(donHangs);
        }

        // ================== DUYỆT ĐƠN HÀNG ==================
        public ActionResult DuyetDon(int maDH)
        {
            var donHang = db.DonHangs.FirstOrDefault(dh => dh.MaDH == maDH);
            if (donHang == null) return HttpNotFound();

            donHang.TrangThaiDon = "Đã xác nhận";
            db.SaveChanges();
            TempData["Message"] = "Đơn hàng đã được xác nhận thành công!";
            return RedirectToAction("IndexDonHang");
        }

        // ================== THÔNG TIN NHÂN VIÊN ==================
        public ActionResult ThongTinNhanVien()
        {
            var nhanViens = db.NguoiDungs
                              .Where(nd => nd.VaiTro == "NhanVienGiaoHang" || nd.VaiTro == "NhanVienDuyetDon")
                              .OrderBy(nd => nd.NgayTao)
                              .ToList();
            return View(nhanViens);
        }

        // ================== THÊM NHÂN VIÊN ==================
        [HttpGet]
        public ActionResult ThemNhanVien()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThemNhanVien(NguoiDung model, string VaiTro)
        {
            if (ModelState.IsValid)
            {
                model.VaiTro = VaiTro; // "NhanVienGiaoHang" hoặc "NhanVienDuyetDon"
                model.Quyen = "User";
                model.TrangThai = true;
                model.NgayTao = DateTime.Now;

                // Mã hóa mật khẩu
                model.MatKhau = Crypto.HashPassword(model.MatKhau);

                if (string.IsNullOrEmpty(model.DienThoai))
                    model.DienThoai = "000000000";

                if (string.IsNullOrEmpty(model.TenDangNhap))
                    model.TenDangNhap = model.Email;
                db.NguoiDungs.Add(model);
                db.SaveChanges();

                TempData["Message"] = "Tạo tài khoản nhân viên thành công!";
                return RedirectToAction("ThongTinNhanVien");
            }
            return View(model);
        }

        // ================== XÓA NHÂN VIÊN ==================
        public ActionResult XoaNhanVien(int id)
        {
            var nv = db.NguoiDungs.FirstOrDefault(nd => nd.MaND == id);
            if (nv != null)
            {
                db.NguoiDungs.Remove(nv);
                db.SaveChanges();
                TempData["Message"] = "Xóa nhân viên thành công!";
            }
            return RedirectToAction("ThongTinNhanVien");
        }

        // ================== XEM DOANH THU ==================
        public ActionResult DoanhThu()
        {
            var doanhThuTheoThang = db.DonHangs
                .Select(dh => new
                {
                    DonHang = dh,
                    // Lấy record thanh toán mới nhất của đơn (phòng trường hợp có nhiều record)
                    ThanhToan = dh.ThanhToans
                        .OrderByDescending(t => t.NgayThanhToan)
                        .FirstOrDefault()
                })
                .Where(x =>
                    x.ThanhToan != null &&
                    (
                        // 1) QR/PayOS: chỉ tính khi ĐÃ THANH TOÁN
                        (
                            (x.ThanhToan.PhuongThuc == "VietQR" || x.ThanhToan.PhuongThuc == "PayOS_QR")
                            && x.ThanhToan.TrangThai == "Đã thanh toán"
                        )
                        ||
                        // 2) COD/Tiền mặt: chỉ tính khi admin ĐÃ DUYỆT (Đã xác nhận)
                        (
                            (x.ThanhToan.PhuongThuc == "COD" || x.ThanhToan.PhuongThuc == "TienMat")
                            && x.DonHang.TrangThaiDon == "Đã xác nhận"
                        )
                    )
                )
                .GroupBy(x => new {
                    Year = x.DonHang.NgayDat.HasValue ? x.DonHang.NgayDat.Value.Year : 0,
                    Month = x.DonHang.NgayDat.HasValue ? x.DonHang.NgayDat.Value.Month : 0
                })
                .Select(g => new
                {
                    Nam = g.Key.Year,
                    Thang = g.Key.Month,
                    DoanhThu = g.Sum(x => x.DonHang.TongTien + x.DonHang.PhiVanChuyen)
                })
                .OrderBy(x => x.Nam)
                .ThenBy(x => x.Thang)
                .ToList();

            ViewBag.DoanhThuTheoThang = JsonConvert.SerializeObject(doanhThuTheoThang);
            return View();
        }

        // ================== QUẢN LÝ NGƯỜI DÙNG ==================
        public ActionResult QuanLyNguoiDung()
        {
            var nguoiDungs = db.NguoiDungs
                               .Where(nd => nd.VaiTro == "Customer")
                               .OrderBy(nd => nd.NgayTao)
                               .ToList();
            return View(nguoiDungs);
        }

        // ================== XEM THÔNG TIN NGƯỜI DÙNG ==================
        public ActionResult ChiTietNguoiDung(int id)
        {
            var nd = db.NguoiDungs.FirstOrDefault(u => u.MaND == id);
            if (nd == null) return HttpNotFound();
            return View(nd);
        }

        // ================== XÓA NGƯỜI DÙNG ==================
        public ActionResult XoaNguoiDung(int id)
        {
            var nd = db.NguoiDungs.Find(id);
            if (nd == null)
            {
                TempData["Error"] = "Không tìm thấy người dùng.";
                return RedirectToAction("QuanLyNguoiDung");
            }

            // Lấy tất cả đơn hàng của người dùng
            var donHangs = db.DonHangs.Where(x => x.MaND == nd.MaND).ToList();

            // 1. Xóa thanh toán
            foreach (var dh in donHangs)
            {
                var thanhToans = db.ThanhToans.Where(t => t.MaDH == dh.MaDH).ToList();
                db.ThanhToans.RemoveRange(thanhToans);
            }

            // 2. Xóa chi tiết đơn hàng
            foreach (var dh in donHangs)
            {
                var ctdh = db.ChiTietDonHangs.Where(x => x.MaDH == dh.MaDH).ToList();
                db.ChiTietDonHangs.RemoveRange(ctdh);
            }

            // 3. Xóa đơn hàng
            db.DonHangs.RemoveRange(donHangs);

            // 4. Xóa địa chỉ giao hàng
            var dc = db.DiaChiGiaoHangs.Where(x => x.MaND == nd.MaND).ToList();
            db.DiaChiGiaoHangs.RemoveRange(dc);

            // 5. Xóa giỏ hàng và chi tiết giỏ hàng
            var gio = db.GioHangs.Where(x => x.MaND == nd.MaND).ToList();
            foreach (var g in gio)
            {
                var ctgh = db.ChiTietGioHangs.Where(ct => ct.MaGH == g.MaGH).ToList();
                db.ChiTietGioHangs.RemoveRange(ctgh);
                db.GioHangs.Remove(g);
            }

            // 6. Cuối cùng xóa người dùng
            db.NguoiDungs.Remove(nd);
            db.SaveChanges();

            TempData["Message"] = "Xóa người dùng thành công!";
            return RedirectToAction("QuanLyNguoiDung");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
