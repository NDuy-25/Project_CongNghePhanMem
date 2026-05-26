using App12COFFEE.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace App12COFFEE.Controllers
{
    public class NhanVienDuyetDonController : Controller
    {
        private Entities db = new Entities();
        public ActionResult Dashboard()
        {
            ViewBag.TongNguoiDung = db.NguoiDungs.Count();
            ViewBag.TongSanPham = db.SanPhams.Count();
            ViewBag.DonHangChoXacNhan = db.DonHangs.Count(dh => dh.TrangThaiDon == "Chờ xác nhận");
            ViewBag.DonHangDaDuyet = db.DonHangs.Count(dh => dh.TrangThaiDon == "Đã xác nhận");

            return View();
        }

        public ActionResult DuyetDon(int maDH)
        {
            var donHang = db.DonHangs.FirstOrDefault(dh => dh.MaDH == maDH);
            if (donHang == null) return HttpNotFound();

            donHang.TrangThaiDon = "Đã xác nhận";
            db.SaveChanges();

            TempData["Message"] = "Đơn hàng đã được duyệt thành công!";
            return RedirectToAction("Dashboard");
        }
    }
}