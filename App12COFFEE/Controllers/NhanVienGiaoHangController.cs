using App12COFFEE.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace App12COFFEE.Controllers
{
    public class NhanVienGiaoHangController : Controller
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
            var maNV = LayMaNhanVien();
            var donHangs = db.DonHangs
                .Include(dh => dh.NguoiDung)
                .Include(dh => dh.DiaChiGiaoHang)
                .Include(dh => dh.ChiTietDonHangs.Select(ct => ct.SanPham))
                .Include(dh => dh.ThanhToans)
                .Where(dh =>
                    dh.TrangThaiDon == "Đã xác nhận" ||
                    dh.TrangThaiDon == "Đang giao hàng" ||
                    dh.TrangThaiDon == "Giao hàng thất bại" ||
                    dh.TrangThaiDon == "Giao hàng thành công")
                .Where(dh => !maNV.HasValue || dh.MaNVGiaoHang == null || dh.MaNVGiaoHang == maNV.Value)
                .OrderByDescending(dh => dh.NgayDat)
                .ToList();

            return View(donHangs);
        }

        public ActionResult XacNhanGiao(int maDH)
        {
            var donHang = db.DonHangs.FirstOrDefault(dh => dh.MaDH == maDH);
            if (donHang == null) return HttpNotFound();

            donHang.TrangThaiDon = "Đang giao hàng";
            if (donHang.MaNVGiaoHang == null) donHang.MaNVGiaoHang = LayMaNhanVien();
            db.SaveChanges();

            TempData["Message"] = "Bạn đã nhận đơn và bắt đầu giao hàng.";
            return RedirectToAction("Dashboard");
        }

        public ActionResult GiaoHangThanhCong(int maDH)
        {
            var donHang = db.DonHangs.Include(dh => dh.ThanhToans).FirstOrDefault(dh => dh.MaDH == maDH);
            if (donHang == null) return HttpNotFound();

            donHang.TrangThaiDon = "Giao hàng thành công";
            if (donHang.MaNVGiaoHang == null) donHang.MaNVGiaoHang = LayMaNhanVien();

            var thanhToan = donHang.ThanhToans.FirstOrDefault();
            if (thanhToan != null && (thanhToan.PhuongThuc == "COD" || thanhToan.PhuongThuc == "TienMat"))
            {
                thanhToan.TrangThai = "Đã thanh toán";
                thanhToan.NgayThanhToan = DateTime.Now;
            }

            db.SaveChanges();
            TempData["Message"] = "Đơn hàng đã giao thành công và cập nhật thanh toán nếu là COD.";
            return RedirectToAction("Dashboard");
        }

        public ActionResult GiaoHangThatBai(int maDH)
        {
            var donHang = db.DonHangs.FirstOrDefault(dh => dh.MaDH == maDH);
            if (donHang == null) return HttpNotFound();

            donHang.TrangThaiDon = "Giao hàng thất bại";
            if (donHang.MaNVGiaoHang == null) donHang.MaNVGiaoHang = LayMaNhanVien();
            db.SaveChanges();

            TempData["Message"] = "Đã cập nhật trạng thái giao hàng thất bại.";
            return RedirectToAction("Dashboard");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}

