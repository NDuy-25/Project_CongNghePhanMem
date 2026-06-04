using App12COFFEE.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace App12COFFEE.Controllers
{
    public class DonHangController : Controller
    {
        private Entities db = new Entities();

        private int? LayMaNguoiDung()
        {
            if (Session["MaND"] != null) return Convert.ToInt32(Session["MaND"]);
            if (Session["UserID"] != null) return Convert.ToInt32(Session["UserID"]);
            return null;
        }

        public ActionResult LichSu()
        {
            var maND = LayMaNguoiDung();
            if (!maND.HasValue) return RedirectToAction("DangNhap", "NguoiDungs");

            var donHangs = db.DonHangs
                .Include(d => d.ChiTietDonHangs.Select(ct => ct.SanPham))
                .Include(d => d.ThanhToans)
                .Where(d => d.MaND == maND.Value)
                .OrderByDescending(d => d.NgayDat)
                .ToList();

            return View(donHangs);
        }

        public ActionResult ChiTiet(int id)
        {
            var maND = LayMaNguoiDung();
            if (!maND.HasValue) return RedirectToAction("DangNhap", "NguoiDungs");

            var donHang = db.DonHangs
                .Include(d => d.DiaChiGiaoHang)
                .Include(d => d.ChiTietDonHangs.Select(ct => ct.SanPham.HinhAnhSanPhams))
                .Include(d => d.ThanhToans)
                .FirstOrDefault(d => d.MaDH == id && d.MaND == maND.Value);

            if (donHang == null) return HttpNotFound();
            return View(donHang);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}

