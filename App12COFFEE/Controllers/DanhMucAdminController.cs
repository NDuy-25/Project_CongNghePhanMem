using App12COFFEE.Models;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace App12COFFEE.Controllers
{
    public class DanhMucAdminController : Controller
    {
        private Entities db = new Entities();

        public ActionResult Index()
        {
            var danhMucs = db.DanhMucs.Include(dm => dm.SanPhams).OrderBy(dm => dm.MaDM).ToList();
            return View(danhMucs);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Them(DanhMuc model)
        {
            if (string.IsNullOrWhiteSpace(model.TenDM))
            {
                TempData["Message"] = "Vui lòng nhập tên danh mục.";
                return RedirectToAction("Index");
            }

            model.TenDM = model.TenDM.Trim();
            db.DanhMucs.Add(model);
            db.SaveChanges();
            TempData["Message"] = "Đã thêm danh mục.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Sua(DanhMuc model)
        {
            var danhMuc = db.DanhMucs.Find(model.MaDM);
            if (danhMuc != null)
            {
                if (string.IsNullOrWhiteSpace(model.TenDM))
                {
                    TempData["Message"] = "Tên danh mục không được để trống.";
                    return RedirectToAction("Index");
                }

                danhMuc.TenDM = model.TenDM.Trim();
                danhMuc.MoTa = model.MoTa;
                danhMuc.HinhAnh = model.HinhAnh;
                db.SaveChanges();
                TempData["Message"] = "Đã cập nhật danh mục.";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Xoa(int id)
        {
            var danhMuc = db.DanhMucs.Include(dm => dm.SanPhams).FirstOrDefault(dm => dm.MaDM == id);
            if (danhMuc == null) return HttpNotFound();

            if (danhMuc.SanPhams.Any())
            {
                TempData["Message"] = "Không thể xóa danh mục đang có sản phẩm.";
                return RedirectToAction("Index");
            }

            db.DanhMucs.Remove(danhMuc);
            db.SaveChanges();
            TempData["Message"] = "Đã xóa danh mục.";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}