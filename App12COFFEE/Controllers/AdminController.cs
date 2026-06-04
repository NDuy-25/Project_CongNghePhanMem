using App12COFFEE.Models;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Web.Helpers;
using System.Web.Mvc;

namespace App12COFFEE.Controllers
{
    public class AdminController : Controller
    {
        private Entities db = new Entities();

        public ActionResult Index()
        {
            ViewBag.TongNguoiDung = db.NguoiDungs.Count();
            ViewBag.TongSanPham = db.SanPhams.Count();
            ViewBag.DonHangChoXacNhan = db.DonHangs.Count(dh => dh.TrangThaiDon == "Chờ xác nhận");
            ViewBag.DonHangDaDuyet = db.DonHangs.Count(dh => dh.TrangThaiDon == "Đã xác nhận");
            return View();
        }

        public ActionResult IndexDonHang()
        {
            var donHangs = db.DonHangs.Where(dh => dh.TrangThaiDon == "Chờ xác nhận").ToList();
            return View(donHangs);
        }

        public ActionResult DuyetDon(int maDH)
        {
            var donHang = db.DonHangs.FirstOrDefault(dh => dh.MaDH == maDH);
            if (donHang == null) return HttpNotFound();

            donHang.TrangThaiDon = "Đã xác nhận";
            db.SaveChanges();
            TempData["Message"] = "Đơn hàng đã được xác nhận thành công!";
            return RedirectToAction("IndexDonHang");
        }

        public ActionResult ThongTinNhanVien()
        {
            var nhanViens = db.NguoiDungs
                .Where(nd => nd.VaiTro == "NhanVienGiaoHang" || nd.VaiTro == "NhanVienDuyetDon")
                .OrderBy(nd => nd.NgayTao)
                .ToList();
            return View(nhanViens);
        }

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
                model.VaiTro = VaiTro;
                model.Quyen = "User";
                model.TrangThai = true;
                model.NgayTao = DateTime.Now;
                model.MatKhau = Crypto.HashPassword(model.MatKhau);

                if (string.IsNullOrEmpty(model.DienThoai)) model.DienThoai = "000000000";
                if (string.IsNullOrEmpty(model.TenDangNhap)) model.TenDangNhap = model.Email;

                db.NguoiDungs.Add(model);
                db.SaveChanges();

                TempData["Message"] = "Tạo tài khoản nhân viên thành công!";
                return RedirectToAction("ThongTinNhanVien");
            }
            return View(model);
        }

        public ActionResult XoaNhanVien(int id)
        {
            var nv = db.NguoiDungs.FirstOrDefault(nd => nd.MaND == id);
            if (nv != null)
            {
                nv.TrangThai = false;
                db.SaveChanges();
                TempData["Message"] = "Đã khóa tài khoản nhân viên để giữ lịch sử xử lý đơn hàng.";
            }
            return RedirectToAction("ThongTinNhanVien");
        }

        public ActionResult DoanhThu()
        {
            var doanhThuTheoThang = db.DonHangs
                .Select(dh => new
                {
                    DonHang = dh,
                    ThanhToan = dh.ThanhToans.OrderByDescending(t => t.NgayThanhToan).FirstOrDefault()
                })
                .Where(x =>
                    x.ThanhToan != null &&
                    (((x.ThanhToan.PhuongThuc == "VietQR" || x.ThanhToan.PhuongThuc == "PayOS_QR") && x.ThanhToan.TrangThai == "Đã thanh toán") ||
                     ((x.ThanhToan.PhuongThuc == "COD" || x.ThanhToan.PhuongThuc == "TienMat") &&
                      (x.DonHang.TrangThaiDon == "Đã xác nhận" || x.DonHang.TrangThaiDon == "Đang giao hàng" || x.DonHang.TrangThaiDon == "Giao hàng thành công"))))
                .GroupBy(x => new
                {
                    Year = x.DonHang.NgayDat.HasValue ? x.DonHang.NgayDat.Value.Year : 0,
                    Month = x.DonHang.NgayDat.HasValue ? x.DonHang.NgayDat.Value.Month : 0
                })
                .Select(g => new
                {
                    Nam = g.Key.Year,
                    Thang = g.Key.Month,
                    DoanhThu = g.Sum(x => (x.DonHang.TongTien ?? 0) + (x.DonHang.PhiVanChuyen ?? 0))
                })
                .OrderBy(x => x.Nam)
                .ThenBy(x => x.Thang)
                .ToList();

            ViewBag.DoanhThuTheoThang = JsonConvert.SerializeObject(doanhThuTheoThang);
            return View();
        }

        public ActionResult QuanLyNguoiDung()
        {
            var nguoiDungs = db.NguoiDungs
                .Where(nd => nd.VaiTro == "Customer")
                .OrderBy(nd => nd.NgayTao)
                .ToList();
            return View(nguoiDungs);
        }

        public ActionResult ChiTietNguoiDung(int id)
        {
            var nd = db.NguoiDungs.FirstOrDefault(u => u.MaND == id);
            if (nd == null) return HttpNotFound();
            return View(nd);
        }

        public ActionResult XoaNguoiDung(int id)
        {
            var nd = db.NguoiDungs.Find(id);
            if (nd == null)
            {
                TempData["Error"] = "Không tìm thấy người dùng.";
                return RedirectToAction("QuanLyNguoiDung");
            }

            bool coLichSuDonHang = db.DonHangs.Any(x => x.MaND == nd.MaND);
            if (coLichSuDonHang)
            {
                nd.TrangThai = false;
                db.SaveChanges();
                TempData["Message"] = "Khách hàng đã có lịch sử đơn hàng nên hệ thống khóa tài khoản thay vì xóa cứng.";
                return RedirectToAction("QuanLyNguoiDung");
            }

            var danhGias = db.DanhGias.Where(x => x.MaND == nd.MaND).ToList();
            db.DanhGias.RemoveRange(danhGias);

            var chats = db.AIChats.Where(x => x.MaND == nd.MaND).ToList();
            foreach (var chat in chats)
            {
                var tinNhans = db.AITinNhans.Where(x => x.MaChat == chat.MaChat).ToList();
                db.AITinNhans.RemoveRange(tinNhans);
            }
            db.AIChats.RemoveRange(chats);

            var dc = db.DiaChiGiaoHangs.Where(x => x.MaND == nd.MaND).ToList();
            db.DiaChiGiaoHangs.RemoveRange(dc);

            var gio = db.GioHangs.Where(x => x.MaND == nd.MaND).ToList();
            foreach (var g in gio)
            {
                var ctgh = db.ChiTietGioHangs.Where(ct => ct.MaGH == g.MaGH).ToList();
                db.ChiTietGioHangs.RemoveRange(ctgh);
                db.GioHangs.Remove(g);
            }

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
