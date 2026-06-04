using App12COFFEE.Models;
using PagedList;
using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace App12COFFEE.Controllers
{
    public class SanPhamAdminController : Controller
    {
        private Entities db = new Entities();

        public ActionResult Index(int? page)
        {
            var sanPhams = db.SanPhams.OrderByDescending(x => x.MaSP).ToList();
            int pageSize = 10;
            int pageNumber = page ?? 1;
            return View(sanPhams.ToPagedList(pageNumber, pageSize));
        }

        [HttpGet]
        public ActionResult ThemSanPham()
        {
            ViewBag.DanhMuc = db.DanhMucs.OrderBy(x => x.TenDM).ToList();
            return View(new SanPham { TrangThai = true, SoLuongTon = 0, Gia = 0 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThemSanPham(SanPham model, HttpPostedFileBase fileUpload)
        {
            ViewBag.DanhMuc = db.DanhMucs.OrderBy(x => x.TenDM).ToList();

            if (string.IsNullOrWhiteSpace(model.TenSP)) ModelState.AddModelError("TenSP", "Vui lòng nhập tên sản phẩm.");
            if (!model.MaDM.HasValue) ModelState.AddModelError("MaDM", "Vui lòng chọn danh mục.");
            if (!model.Gia.HasValue || model.Gia < 0) ModelState.AddModelError("Gia", "Giá phải lớn hơn hoặc bằng 0.");
            if (!model.SoLuongTon.HasValue || model.SoLuongTon < 0) ModelState.AddModelError("SoLuongTon", "Số lượng tồn phải lớn hơn hoặc bằng 0.");

            if (!ModelState.IsValid) return View(model);

            model.TrangThai = model.TrangThai ?? true;
            model.LuotBan = model.LuotBan ?? 0;
            db.SanPhams.Add(model);
            db.SaveChanges();

            LuuAnhSanPham(model.MaSP, fileUpload, true);

            TempData["Message"] = "Thêm sản phẩm thành công.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult SuaSanPham(int id)
        {
            var sanPham = db.SanPhams.FirstOrDefault(x => x.MaSP == id);
            if (sanPham == null) return HttpNotFound();

            ViewBag.DanhMuc = db.DanhMucs.OrderBy(x => x.TenDM).ToList();
            return View(sanPham);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SuaSanPham(SanPham model, HttpPostedFileBase fileUpload)
        {
            ViewBag.DanhMuc = db.DanhMucs.OrderBy(x => x.TenDM).ToList();

            if (string.IsNullOrWhiteSpace(model.TenSP)) ModelState.AddModelError("TenSP", "Vui lòng nhập tên sản phẩm.");
            if (!model.MaDM.HasValue) ModelState.AddModelError("MaDM", "Vui lòng chọn danh mục.");
            if (!model.Gia.HasValue || model.Gia < 0) ModelState.AddModelError("Gia", "Giá phải lớn hơn hoặc bằng 0.");
            if (!model.SoLuongTon.HasValue || model.SoLuongTon < 0) ModelState.AddModelError("SoLuongTon", "Số lượng tồn phải lớn hơn hoặc bằng 0.");

            if (!ModelState.IsValid) return View(model);

            var sanPham = db.SanPhams.FirstOrDefault(x => x.MaSP == model.MaSP);
            if (sanPham == null) return HttpNotFound();

            sanPham.TenSP = model.TenSP;
            sanPham.MaDM = model.MaDM;
            sanPham.Gia = model.Gia;
            sanPham.MoTa = model.MoTa;
            sanPham.MoTaChiTiet = model.MoTaChiTiet;
            sanPham.SoLuongTon = model.SoLuongTon;
            sanPham.TrangThai = model.TrangThai ?? true;

            LuuAnhSanPham(sanPham.MaSP, fileUpload, false);
            db.SaveChanges();

            TempData["Message"] = "Cập nhật sản phẩm thành công.";
            return RedirectToAction("Index");
        }

        public ActionResult XoaSanPham(int id)
        {
            var sanPham = db.SanPhams.FirstOrDefault(x => x.MaSP == id);
            if (sanPham == null) return RedirectToAction("Index");

            bool daPhatSinhDuLieu = db.ChiTietDonHangs.Any(x => x.MaSP == id) || db.ChiTietGioHangs.Any(x => x.MaSP == id) || db.DanhGias.Any(x => x.MaSP == id);
            if (daPhatSinhDuLieu)
            {
                sanPham.TrangThai = false;
                db.SaveChanges();
                TempData["Message"] = "Sản phẩm đã phát sinh dữ liệu nên hệ thống chuyển sang ngừng bán thay vì xóa cứng.";
                return RedirectToAction("Index");
            }

            XoaTatCaAnh(id);
            db.SanPhams.Remove(sanPham);
            db.SaveChanges();

            TempData["Message"] = "Xóa sản phẩm thành công.";
            return RedirectToAction("Index");
        }

        private void LuuAnhSanPham(int maSP, HttpPostedFileBase fileUpload, bool themMoi)
        {
            if (fileUpload == null || fileUpload.ContentLength <= 0) return;

            string ext = Path.GetExtension(fileUpload.FileName).ToLowerInvariant();
            string[] allowed = { ".jpg", ".jpeg", ".png", ".webp" };
            if (!allowed.Contains(ext))
            {
                TempData["Error"] = "Ảnh chỉ hỗ trợ JPG, PNG hoặc WEBP.";
                return;
            }

            string folderPath = Server.MapPath("~/Content/Images/");
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            if (!themMoi) XoaTatCaAnh(maSP);

            string uniqueFileName = "sp_" + maSP + "_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ext;
            string fullPath = Path.Combine(folderPath, uniqueFileName);
            fileUpload.SaveAs(fullPath);

            db.HinhAnhSanPhams.Add(new HinhAnhSanPham
            {
                MaSP = maSP,
                DuongDan = uniqueFileName,
                GhiChu = "Ảnh đại diện"
            });
        }

        private void XoaTatCaAnh(int maSP)
        {
            string folderPath = Server.MapPath("~/Content/Images/");
            var images = db.HinhAnhSanPhams.Where(x => x.MaSP == maSP).ToList();
            foreach (var img in images)
            {
                string imgPath = Path.Combine(folderPath, img.DuongDan ?? "");
                if (System.IO.File.Exists(imgPath)) System.IO.File.Delete(imgPath);
                db.HinhAnhSanPhams.Remove(img);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
