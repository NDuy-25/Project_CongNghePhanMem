using App12COFFEE.Models;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace App12COFFEE.Controllers
{
    public class TheoDoiAdminController : Controller
    {
        private Entities db = new Entities();

        public ActionResult DanhGiaVaChat()
        {
            ViewBag.DanhGias = db.DanhGias
                .Include(dg => dg.NguoiDung)
                .Include(dg => dg.SanPham)
                .Where(dg => dg.IsDeleted != true)
                .OrderByDescending(dg => dg.NgayDG)
                .Take(50)
                .ToList();

            ViewBag.Chats = db.AIChats
                .Include(c => c.NguoiDung)
                .Include(c => c.AITinNhans)
                .OrderByDescending(c => c.NgayTao)
                .Take(30)
                .ToList();

            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
