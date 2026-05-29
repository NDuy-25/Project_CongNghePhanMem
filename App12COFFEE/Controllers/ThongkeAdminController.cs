using App12COFFEE.Models;
using Newtonsoft.Json;
using System.Linq;
using System.Web.Mvc;

namespace App12COFFEE.Controllers
{
    public class ThongkeAdminController : Controller
    {
        private Entities db = new Entities();

        public ActionResult DoanhThuThang()
        {
            var doanhThuTheoThang = db.DonHangs
                .Where(dh => dh.NgayDat.HasValue && dh.TrangThaiDon != "Đã hủy")
                .GroupBy(dh => new { Year = dh.NgayDat.Value.Year, Month = dh.NgayDat.Value.Month })
                .Select(g => new
                {
                    Nam = g.Key.Year,
                    Thang = g.Key.Month,
                    DoanhThu = g.Sum(dh => (dh.TongTien ?? 0m) + (dh.PhiVanChuyen ?? 0m))
                })
                .OrderBy(d => d.Nam)
                .ThenBy(d => d.Thang)
                .ToList();

            ViewBag.DoanhThuTheoThang = JsonConvert.SerializeObject(doanhThuTheoThang);
            return View();
        }

        public ActionResult DoanhThuNam()
        {
            var doanhThuTheoNam = db.DonHangs
                .Where(dh => dh.NgayDat.HasValue && dh.TrangThaiDon != "Đã hủy")
                .GroupBy(dh => dh.NgayDat.Value.Year)
                .Select(g => new
                {
                    Nam = g.Key,
                    DoanhThu = g.Sum(dh => (dh.TongTien ?? 0m) + (dh.PhiVanChuyen ?? 0m))
                })
                .OrderBy(d => d.Nam)
                .ToList();

            ViewBag.DoanhThuTheoNam = JsonConvert.SerializeObject(doanhThuTheoNam);
            return View();
        }

        public ActionResult DoanhThuMoiNhat()
        {
            var maxDate = db.DonHangs.Where(d => d.NgayDat.HasValue && d.TrangThaiDon != "Đã hủy").Max(d => d.NgayDat);
            decimal doanhThuMoiNhat = 0m;
            if (maxDate.HasValue)
            {
                doanhThuMoiNhat = db.DonHangs
                    .Where(dh => dh.NgayDat == maxDate)
                    .Select(dh => (dh.TongTien ?? 0m) + (dh.PhiVanChuyen ?? 0m))
                    .DefaultIfEmpty(0m)
                    .Sum();
            }

            ViewBag.DoanhThuMoiNhat = doanhThuMoiNhat;
            return View();
        }

        public ActionResult DoanhThuThangForChart()
        {
            var doanhThuTheoThang = db.DonHangs
                .Where(dh => dh.NgayDat.HasValue && dh.TrangThaiDon != "Đã hủy")
                .GroupBy(dh => new { Year = dh.NgayDat.Value.Year, Month = dh.NgayDat.Value.Month })
                .Select(g => new
                {
                    Nam = g.Key.Year,
                    Thang = g.Key.Month,
                    DoanhThu = g.Sum(dh => (dh.TongTien ?? 0m) + (dh.PhiVanChuyen ?? 0m))
                })
                .OrderBy(d => d.Nam)
                .ThenBy(d => d.Thang)
                .ToList();

            return Json(doanhThuTheoThang, JsonRequestBehavior.AllowGet);
        }

        public ActionResult XoaDoanhThuTheoThang(int id)
        {
            var donHang = db.DonHangs.FirstOrDefault(dh => dh.MaDH == id);
            if (donHang != null)
            {
                donHang.TrangThaiDon = "Đã hủy";
                db.SaveChanges();
                TempData["Message"] = "Đã loại đơn hàng khỏi thống kê bằng cách chuyển trạng thái sang Đã hủy.";
            }
            return RedirectToAction("DoanhThuThang");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}