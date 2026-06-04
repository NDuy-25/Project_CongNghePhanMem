using App12COFFEE.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace App12COFFEE.Controllers
{
    public class NhanVienDuyetDonController : Controller
    {
        private Entities db = new Entities();

        private int? LayMaNhanVien()
        {
            if (Session["MaND"] != null) return Convert.ToInt32(Session["MaND"]);
            if (Session["UserID"] != null) return Convert.ToInt32(Session["UserID"]);
            return null;
        }

        public ActionResult Dashboard()
        {
            ViewBag.TongNguoiDung = db.NguoiDungs.Count();
            ViewBag.TongSanPham = db.SanPhams.Count();
            ViewBag.DonHangChoXacNhan = db.DonHangs.Count(dh => dh.TrangThaiDon == "Chờ xác nhận");
            ViewBag.DonHangDaDuyet = db.DonHangs.Count(dh => dh.TrangThaiDon == "Đã xác nhận");
            ViewBag.NhanVienGiaoHang = db.NguoiDungs
                .Where(nd => nd.VaiTro == "NhanVienGiaoHang" || nd.Quyen == "NhanVienGiaoHang")
                .OrderBy(nd => nd.HoTen)
                .ToList();

            var donHangs = db.DonHangs
                .Include(dh => dh.NguoiDung)
                .Include(dh => dh.DiaChiGiaoHang)
                .Include(dh => dh.ChiTietDonHangs.Select(ct => ct.SanPham))
                .Include(dh => dh.ThanhToans)
                .Where(dh => dh.TrangThaiDon == "Chờ xác nhận" || dh.TrangThaiDon == "Đã xác nhận")
                .OrderByDescending(dh => dh.NgayDat)
                .ToList();

            return View(donHangs);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DuyetDon(int maDH, int? maNVGiaoHang)
        {
            var donHang = db.DonHangs.Include(dh => dh.ThanhToans).FirstOrDefault(dh => dh.MaDH == maDH);
            if (donHang == null) return HttpNotFound();

            var thanhToan = donHang.ThanhToans.FirstOrDefault();
            if (thanhToan != null && thanhToan.PhuongThuc != "COD" && thanhToan.PhuongThuc != "TienMat" && thanhToan.TrangThai != "Đã thanh toán")
            {
                TempData["Message"] = "Đơn thanh toán online chưa hoàn tất, chưa thể duyệt.";
                return RedirectToAction("Dashboard");
            }

            donHang.TrangThaiDon = "Đã xác nhận";
            donHang.MaNVDuyet = LayMaNhanVien();
            donHang.MaNguoiDuyet = donHang.MaNVDuyet;
            donHang.MaNVGiaoHang = maNVGiaoHang;
            db.SaveChanges();

            TempData["Message"] = "Đã duyệt đơn và phân công giao hàng.";
            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TuChoiDon(int maDH, string lyDo)
        {
            var donHang = db.DonHangs.Find(maDH);
            if (donHang == null) return HttpNotFound();

            donHang.TrangThaiDon = "Từ chối";
            donHang.MaNVDuyet = LayMaNhanVien();
            donHang.MaNguoiDuyet = donHang.MaNVDuyet;
            db.SaveChanges();

            TempData["Message"] = string.IsNullOrWhiteSpace(lyDo) ? "Đã từ chối đơn hàng." : "Đã từ chối đơn hàng: " + lyDo;
            return RedirectToAction("Dashboard");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}


