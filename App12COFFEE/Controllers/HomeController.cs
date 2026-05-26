using App12COFFEE.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace App12COFFEE.Controllers
{
    public class HomeController : Controller
    {
        private Entities db = new Entities();
        // ========== TRANG CHỦ ==========
        public ActionResult TrangChu()
        {
            string[] monNoiBat =
            {
                "Hibi Vải",
                "Ô Long TC Đường Đen",
                "Cà phê sữa",
                "Matcha Latte"
            };

            var sanPhamNoiBat = db.SanPhams
                .Include("HinhAnhSanPhams")
                .Where(sp => monNoiBat.Contains(sp.TenSP))
                .GroupBy(sp => sp.TenSP)
                .Select(g => g.FirstOrDefault())
                .ToList();

            return View(sanPhamNoiBat);
        }

        // ========== MENU ==========
        public ActionResult Menu(int? maDM, int page = 1, int pageSize = 12)
        {
            ViewBag.DanhMuc = db.DanhMucs.ToList();

            var query = db.SanPhams
                          .Include("DanhMuc")
                          .Include("HinhAnhSanPhams")
                          .Where(sp => sp.TrangThai == true);

            if (maDM.HasValue)
            {
                query = query.Where(sp => sp.MaDM == maDM.Value);
            }

            int totalItems = query.Count();
            var dsSanPham = query
                            .OrderBy(sp => sp.MaSP)
                            .Skip((page - 1) * pageSize)
                            .Take(pageSize)
                            .ToList();

            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;

            return View(dsSanPham);
        }


        // ========== LIÊN HỆ ==========
        [HttpGet]
        public ActionResult LienHe()
        {
            return View();
        }

        // ========== GIỚI THIỆU ==========
        public ActionResult GioiThieu()
        {
            return View();
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
