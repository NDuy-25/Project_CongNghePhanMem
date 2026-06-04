using App12COFFEE.Models;
using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web.Mvc;

namespace App12COFFEE.Controllers
{
    public class DonHangAdminController : Controller
    {
        private Entities db = new Entities();

        private void GuiEmailXacNhan(string emailNguoiNhan, string tenNguoiNhan, int maDH)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(emailNguoiNhan)) return;

                var fromAddress = new MailAddress("duy.nd.65cntt@ntu.edu.vn", "12COFFEE");
                var toAddress = new MailAddress(emailNguoiNhan, tenNguoiNhan ?? "Khách hàng");
                const string fromPassword = "hvxy kyul vknx jilu";

                string subject = "Đơn hàng #" + maDH + " đã được xác nhận";
                string templatePath = Server.MapPath("~/EmailTemplates/XacNhanDonHang.html");
                string body = System.IO.File.Exists(templatePath)
                    ? System.IO.File.ReadAllText(templatePath)
                    : "Xin chào {TEN_KHACH_HANG}, đơn hàng #{MA_DON_HANG} của bạn đã được xác nhận.";

                body = body.Replace("{TEN_KHACH_HANG}", tenNguoiNhan ?? "");
                body = body.Replace("{MA_DON_HANG}", maDH.ToString());

                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                };

                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                })
                {
                    smtp.Send(message);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi gửi email: " + ex.Message);
            }
        }

        public ActionResult Index()
        {
            var donHangs = db.DonHangs
                .Include("NguoiDung")
                .Include("ChiTietDonHangs.SanPham")
                .Include("ThanhToans")
                .Where(d => d.TrangThaiDon == "Chờ xác nhận")
                .OrderByDescending(d => d.NgayDat)
                .ToList();

            return View(donHangs);
        }

        public ActionResult DonDaDuyet()
        {
            var donHangs = db.DonHangs
                .Include("NguoiDung")
                .Include("ChiTietDonHangs.SanPham")
                .Include("ThanhToans")
                .Where(d => d.TrangThaiDon == "Đã xác nhận" || d.TrangThaiDon == "Đang giao" || d.TrangThaiDon == "Đã giao")
                .OrderByDescending(d => d.NgayDat)
                .ToList();

            return View(donHangs);
        }

        public ActionResult Details(int id)
        {
            var donHang = db.DonHangs
                .Include("NguoiDung")
                .Include("DiaChiGiaoHang")
                .Include("ChiTietDonHangs.SanPham")
                .Include("ThanhToans")
                .FirstOrDefault(d => d.MaDH == id);

            if (donHang == null) return HttpNotFound();
            return View(donHang);
        }

        public ActionResult Edit(int id)
        {
            var donHang = db.DonHangs
                .Include("ChiTietDonHangs.SanPham")
                .Include("ThanhToans")
                .FirstOrDefault(d => d.MaDH == id);

            if (donHang == null) return HttpNotFound();

            ViewBag.SanPhamList = new SelectList(db.SanPhams.Where(sp => sp.TrangThai == true).OrderBy(sp => sp.TenSP).ToList(), "MaSP", "TenSP");
            return View(donHang);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThemMon(int maDH, int maSP, int soLuong)
        {
            if (soLuong <= 0)
            {
                TempData["Message"] = "Số lượng phải lớn hơn 0.";
                return RedirectToAction("Edit", new { id = maDH });
            }

            var donHang = db.DonHangs.Find(maDH);
            var sanPham = db.SanPhams.Find(maSP);
            if (donHang == null || sanPham == null)
            {
                TempData["Message"] = "Không tìm thấy đơn hàng hoặc sản phẩm.";
                return RedirectToAction("Edit", new { id = maDH });
            }

            var ct = db.ChiTietDonHangs.FirstOrDefault(c => c.MaDH == maDH && c.MaSP == maSP);
            if (ct == null)
            {
                db.ChiTietDonHangs.Add(new ChiTietDonHang
                {
                    MaDH = maDH,
                    MaSP = maSP,
                    SoLuong = soLuong,
                    DonGia = sanPham.Gia
                });
            }
            else
            {
                ct.SoLuong = (ct.SoLuong ?? 0) + soLuong;
                ct.DonGia = sanPham.Gia;
            }

            db.SaveChanges();
            CapNhatTongTien(maDH);
            TempData["Message"] = "Đã cập nhật món trong đơn hàng.";
            return RedirectToAction("Edit", new { id = maDH });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult XoaMon(int maDH, int maSP)
        {
            var ct = db.ChiTietDonHangs.FirstOrDefault(c => c.MaDH == maDH && c.MaSP == maSP);
            if (ct != null)
            {
                db.ChiTietDonHangs.Remove(ct);
                db.SaveChanges();
                CapNhatTongTien(maDH);
                TempData["Message"] = "Đã xóa món khỏi đơn hàng.";
            }

            return RedirectToAction("Edit", new { id = maDH });
        }

        public ActionResult Delete(int id)
        {
            var donHang = db.DonHangs
                .Include("ChiTietDonHangs")
                .Include("ThanhToans")
                .FirstOrDefault(d => d.MaDH == id);
            if (donHang == null) return HttpNotFound();
            return View(donHang);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var donHang = db.DonHangs.Find(id);
            if (donHang != null)
            {
                var chiTiet = db.ChiTietDonHangs.Where(c => c.MaDH == id).ToList();
                db.ChiTietDonHangs.RemoveRange(chiTiet);

                var thanhToan = db.ThanhToans.Where(t => t.MaDH == id).ToList();
                db.ThanhToans.RemoveRange(thanhToan);

                db.DonHangs.Remove(donHang);
                db.SaveChanges();
                TempData["Message"] = "Đã xóa đơn hàng.";
            }

            return RedirectToAction("Index");
        }

        public ActionResult DuyetDon(int maDH)
        {
            var donHang = db.DonHangs
                .Include("NguoiDung")
                .Include("ThanhToans")
                .FirstOrDefault(dh => dh.MaDH == maDH);

            if (donHang == null) return HttpNotFound();

            var tt = donHang.ThanhToans.FirstOrDefault();
            if (tt != null && tt.PhuongThuc != "COD" && tt.PhuongThuc != "TienMat" && tt.TrangThai != "Đã thanh toán")
            {
                TempData["Message"] = "Đơn này chưa thanh toán VietQR/PayOS nên chưa thể duyệt.";
                return RedirectToAction("Index");
            }

            donHang.TrangThaiDon = "Đã xác nhận";
            db.SaveChanges();

            if (donHang.NguoiDung != null && !string.IsNullOrWhiteSpace(donHang.NguoiDung.Email))
            {
                GuiEmailXacNhan(donHang.NguoiDung.Email, donHang.NguoiDung.HoTen, maDH);
            }

            TempData["Message"] = "Đơn hàng đã được xác nhận.";
            return RedirectToAction("Index");
        }

        private void CapNhatTongTien(int maDH)
        {
            var donHang = db.DonHangs.Find(maDH);
            if (donHang == null) return;

            var tongTien = db.ChiTietDonHangs
                .Where(c => c.MaDH == maDH)
                .Select(c => (decimal?)((c.SoLuong ?? 0) * (c.DonGia ?? 0m)))
                .DefaultIfEmpty(0m)
                .Sum() ?? 0m;

            donHang.TongTien = tongTien;
            db.SaveChanges();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}