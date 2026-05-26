using App12COFFEE.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace App12COFFEE.Controllers
{
    public class ThongkeAdminController : Controller
    {
        private Entities db = new Entities();
        // ================== THỐNG KÊ DOANH THU THEO THÁNG ==================
        public ActionResult DoanhThuThang()
        {
            var doanhThuTheoThang = db.DonHangs
                            .Where(dh => dh.NgayDat.HasValue) // tránh null
                            .GroupBy(dh => new { Year = dh.NgayDat.Value.Year, Month = dh.NgayDat.Value.Month })
                            .Select(g => new
                            {
                                Nam = g.Key.Year,
                                Thang = g.Key.Month,
                                DoanhThu = g.Sum(dh => dh.TongTien + dh.PhiVanChuyen)
                            })
                            .OrderBy(d => d.Nam)
                            .ThenBy(d => d.Thang)
                            .ToList();

            ViewBag.DoanhThuTheoThang = JsonConvert.SerializeObject(doanhThuTheoThang);
            return View();
        }

        // ================== THỐNG KÊ DOANH THU THEO NĂM ==================
        public ActionResult DoanhThuNam()
        {
            var doanhThuTheoNam = db.DonHangs
                            .Where(dh => dh.NgayDat.HasValue)
                            .GroupBy(dh => dh.NgayDat.Value.Year)
                            .Select(g => new
                            {
                                Nam = g.Key,
                                DoanhThu = g.Sum(dh => dh.TongTien + dh.PhiVanChuyen)
                            })
                            .OrderBy(d => d.Nam)
                            .ToList();

            ViewBag.DoanhThuTheoNam = JsonConvert.SerializeObject(doanhThuTheoNam);
            return View();
        }

        // ================== XEM DOANH THU MỚI NHẤT ==================
        public ActionResult DoanhThuMoiNhat()
        {
            var maxDate = db.DonHangs.Max(d => d.NgayDat);
            var doanhThuMoiNhat = db.DonHangs
                            .Where(dh => dh.NgayDat == maxDate)
                            .Sum(dh => dh.TongTien + dh.PhiVanChuyen);

            ViewBag.DoanhThuMoiNhat = doanhThuMoiNhat;
            return View();
        }

        // ================== DỮ LIỆU THEO THÁNG (DỄ DÙNG TRONG BIỂU ĐỒ) ==================
        public ActionResult DoanhThuThangForChart()
        {
            var doanhThuTheoThang = db.DonHangs
                            .Where(dh => dh.NgayDat.HasValue)
                            .GroupBy(dh => new { Year = dh.NgayDat.Value.Year, Month = dh.NgayDat.Value.Month })
                            .Select(g => new
                            {
                                Nam = g.Key.Year,
                                Thang = g.Key.Month,
                                DoanhThu = g.Sum(dh => dh.TongTien + dh.PhiVanChuyen)
                            })
                            .OrderBy(d => d.Nam)
                            .ThenBy(d => d.Thang)
                            .ToList();

            return Json(doanhThuTheoThang, JsonRequestBehavior.AllowGet);
        }

        // ================== XÓA DỮ LIỆU DOANH THU ==================
        public ActionResult XoaDoanhThuTheoThang(int id)
        {
            var donHang = db.DonHangs.FirstOrDefault(dh => dh.MaDH == id);
            if (donHang != null)
            {
                db.DonHangs.Remove(donHang);
                db.SaveChanges();
            }

            TempData["Message"] = "Dữ liệu doanh thu đã bị xóa!";
            return RedirectToAction("DoanhThuThang");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
