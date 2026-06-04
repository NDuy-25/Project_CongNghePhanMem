using App12COFFEE.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace App12COFFEE.Controllers
{
    public class DanhGiaController : Controller
    {
        private Entities db = new Entities();

        private int? LayMaNguoiDung()
        {
            if (Session["MaND"] != null) return Convert.ToInt32(Session["MaND"]);
            if (Session["UserID"] != null) return Convert.ToInt32(Session["UserID"]);
            return null;
        }

        public ActionResult Tao(int maSP)
        {
            var maND = LayMaNguoiDung();
            if (!maND.HasValue) return RedirectToAction("DangNhap", "NguoiDungs");

            bool daMua = db.ChiTietDonHangs.Any(ct =>
                ct.MaSP == maSP &&
                ct.DonHang.MaND == maND.Value &&
                ct.DonHang.TrangThaiDon == "Giao hàng thành công");

            if (!daMua)
            {
                TempData["Message"] = "Bạn chỉ có thể đánh giá sản phẩm trong đơn đã giao thành công.";
                return RedirectToAction("LichSu", "DonHang");
            }

            var sanPham = db.SanPhams.Find(maSP);
            if (sanPham == null) return HttpNotFound();
            ViewBag.SanPham = sanPham;
            return View(new DanhGia { MaSP = maSP, SoSao = 5 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Tao(DanhGia model)
        {
            var maND = LayMaNguoiDung();
            if (!maND.HasValue) return RedirectToAction("DangNhap", "NguoiDungs");

            if (!model.SoSao.HasValue || model.SoSao < 1 || model.SoSao > 5)
            {
                ModelState.AddModelError("", "Số sao phải từ 1 đến 5.");
            }

            bool daMua = db.ChiTietDonHangs.Any(ct =>
                ct.MaSP == model.MaSP &&
                ct.DonHang.MaND == maND.Value &&
                ct.DonHang.TrangThaiDon == "Giao hàng thành công");

            if (!daMua) ModelState.AddModelError("", "Sản phẩm chưa đủ điều kiện đánh giá.");

            if (!ModelState.IsValid)
            {
                ViewBag.SanPham = db.SanPhams.Find(model.MaSP);
                return View(model);
            }

            var danhGia = new DanhGia
            {
                MaSP = model.MaSP,
                MaND = maND.Value,
                SoSao = model.SoSao,
                BinhLuan = model.BinhLuan,
                NgayDG = DateTime.Now,
                TrangThai = true,
                IsDeleted = false
            };

            db.DanhGias.Add(danhGia);
            db.SaveChanges();

            TempData["Message"] = "Cảm ơn bạn đã gửi đánh giá.";
            return RedirectToAction("ChiTietSanPham", "Home", new { id = model.MaSP });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
