using App12COFFEE.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace App12COFFEE.Controllers
{
    public class NhanVienGiaoHangController : Controller
    {
        private Entities db = new Entities();
        public ActionResult Dashboard()
        {
            var donHangs = db.DonHangs
                             .Include("NguoiDung")
                             .Where(dh => dh.TrangThaiDon == "Đã xác nhận")
                             .OrderByDescending(dh => dh.NgayDat)
                             .ToList();

            return View(donHangs);
        }

        // Nhân viên xác nhận nhận đơn
        public ActionResult XacNhanGiao(int maDH)
        {
            var donHang = db.DonHangs.FirstOrDefault(dh => dh.MaDH == maDH);
            if (donHang == null) return HttpNotFound();

            donHang.TrangThaiDon = "Đang giao hàng";
            db.SaveChanges();

            TempData["Message"] = "Bạn đã nhận đơn và bắt đầu giao hàng!";
            return RedirectToAction("Dashboard");
        }

        // Nhân viên xác nhận giao hàng thành công
        public ActionResult GiaoHangThanhCong(int maDH)
        {
            var donHang = db.DonHangs.FirstOrDefault(dh => dh.MaDH == maDH);
            if (donHang == null) return HttpNotFound();

            donHang.TrangThaiDon = "Giao hàng thành công";
            db.SaveChanges();

            TempData["Message"] = "Đơn hàng đã được giao thành công!";
            return RedirectToAction("Dashboard");
        }
    }
}