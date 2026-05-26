using App12COFFEE.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using PagedList.Mvc;


namespace App12COFFEE.Controllers
{
    public class SanPhamAdminController : Controller
    {
        private Entities db = new Entities();
        // ================== DANH SÁCH SẢN PHẨM ==================
        public ActionResult Index(int? page)
        {
            var sanPhams = db.SanPhams.OrderByDescending(x => x.MaSP).ToList();
            int pageSize = 10;
            int pageNumber = page ?? 1;
            return View(sanPhams.ToPagedList(pageNumber, pageSize));
        }

        // ================== THÊM SẢN PHẨM ==================
        [HttpGet]
        public ActionResult ThemSanPham()
        {
            ViewBag.DanhMuc = db.DanhMucs.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThemSanPham(SanPham model, HttpPostedFileBase fileUpload)
        {
            ViewBag.DanhMuc = db.DanhMucs.ToList();

            if (!ModelState.IsValid)
                return View(model);

            db.SanPhams.Add(model);
            db.SaveChanges();

            // ================== UPLOAD ẢNH ==================
            if (fileUpload != null && fileUpload.ContentLength > 0)
            {
                string folderPath = Server.MapPath("~/Content/Images/");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string fileName = Path.GetFileName(fileUpload.FileName);
                string uniqueFileName = DateTime.Now.Ticks + "_" + fileName;
                string fullPath = Path.Combine(folderPath, uniqueFileName);

                fileUpload.SaveAs(fullPath);

                HinhAnhSanPham hinhAnh = new HinhAnhSanPham
                {
                    MaSP = model.MaSP,
                    DuongDan = uniqueFileName,
                    GhiChu = "Ảnh đại diện"
                };

                db.HinhAnhSanPhams.Add(hinhAnh);
                db.SaveChanges();
            }

            TempData["Message"] = "Thêm sản phẩm thành công!";
            return RedirectToAction("Index");
        }

        // ================== SỬA SẢN PHẨM ==================
        [HttpGet]
        public ActionResult SuaSanPham(int id)
        {
            var sanPham = db.SanPhams.FirstOrDefault(x => x.MaSP == id);
            if (sanPham == null) return HttpNotFound();

            ViewBag.DanhMuc = db.DanhMucs.ToList();
            return View(sanPham);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SuaSanPham(SanPham model, HttpPostedFileBase fileUpload)
        {
            ViewBag.DanhMuc = db.DanhMucs.ToList();
            if (!ModelState.IsValid)
                return View(model);

            var sanPham = db.SanPhams.FirstOrDefault(x => x.MaSP == model.MaSP);
            if (sanPham == null) return HttpNotFound();

            // Update thông tin
            sanPham.TenSP = model.TenSP;
            sanPham.MaDM = model.MaDM;
            sanPham.Gia = model.Gia;
            sanPham.MoTa = model.MoTa;
            sanPham.SoLuongTon = model.SoLuongTon;
            sanPham.TrangThai = model.TrangThai;

            // ================== ĐỔI ẢNH (NẾU CÓ) ==================
            if (fileUpload != null && fileUpload.ContentLength > 0)
            {
                string folderPath = Server.MapPath("~/Content/Images/");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                // Xóa ảnh cũ
                var oldImage = db.HinhAnhSanPhams.FirstOrDefault(x => x.MaSP == sanPham.MaSP);
                if (oldImage != null)
                {
                    string oldPath = Path.Combine(folderPath, oldImage.DuongDan);
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);

                    db.HinhAnhSanPhams.Remove(oldImage);
                    db.SaveChanges();
                }

                // Upload ảnh mới
                string fileName = Path.GetFileName(fileUpload.FileName);
                string uniqueFileName = DateTime.Now.Ticks + "_" + fileName;
                string fullPath = Path.Combine(folderPath, uniqueFileName);
                fileUpload.SaveAs(fullPath);

                HinhAnhSanPham newImage = new HinhAnhSanPham
                {
                    MaSP = sanPham.MaSP,
                    DuongDan = uniqueFileName,
                    GhiChu = "Ảnh đại diện"
                };

                db.HinhAnhSanPhams.Add(newImage);
            }

            db.SaveChanges();
            TempData["Message"] = "Cập nhật sản phẩm thành công!";
            return RedirectToAction("Index");
        }

        // ================== XÓA SẢN PHẨM ==================
        public ActionResult XoaSanPham(int id)
        {
            var sanPham = db.SanPhams.FirstOrDefault(x => x.MaSP == id);
            if (sanPham == null) return RedirectToAction("Index");

            string folderPath = Server.MapPath("~/Content/Images/");
            var images = db.HinhAnhSanPhams.Where(x => x.MaSP == id).ToList();

            foreach (var img in images)
            {
                string imgPath = Path.Combine(folderPath, img.DuongDan);
                if (System.IO.File.Exists(imgPath))
                    System.IO.File.Delete(imgPath);

                db.HinhAnhSanPhams.Remove(img);
            }

            db.SanPhams.Remove(sanPham);
            db.SaveChanges();

            TempData["Message"] = "Xóa sản phẩm thành công!";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }
    }
}
