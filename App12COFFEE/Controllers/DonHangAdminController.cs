using App12COFFEE.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace App12COFFEE.Controllers
{
    public class DonHangAdminController : Controller
    {
        private Entities db = new Entities();
        // ================== HÀM GỬI EMAIL ==================
        private void GuiEmailXacNhan(string emailNguoiNhan, string tenNguoiNhan, int maDH)
        {
            try
            {
                var fromAddress = new MailAddress("duy.nd.65cntt@ntu.edu.vn", "12COFFEE");
                var toAddress = new MailAddress(emailNguoiNhan, tenNguoiNhan);
                const string fromPassword = "hvxy kyul vknx jilu"; // App Password Gmail

                string subject = $"Đơn hàng #{maDH} đã được xác nhận";

                // Đọc file HTML template
                string templatePath = Server.MapPath("~/EmailTemplates/XacNhanDonHang.html");
                string body = System.IO.File.ReadAllText(templatePath);

                // Thay dữ liệu động
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

        // ================== ĐƠN HÀNG CHỜ XÁC NHẬN ==================
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

        // ================== ĐƠN HÀNG ĐÃ DUYỆT ==================
        public ActionResult DonDaDuyet()
        {
            var donHangs = db.DonHangs
                             .Include("NguoiDung")
                             .Include("ChiTietDonHangs.SanPham")
                             .Include("ThanhToans")
                             .Where(d => d.TrangThaiDon == "Đã xác nhận")
                             .OrderByDescending(d => d.NgayDat)
                             .ToList();

            return View(donHangs);
        }

        // ================== CHI TIẾT ĐƠN HÀNG ==================
        public ActionResult Details(int id)
        {
            var donHang = db.DonHangs
                            .Include("NguoiDung")
                            .Include("ChiTietDonHangs.SanPham")
                            .Include("ThanhToans")
                            .FirstOrDefault(d => d.MaDH == id);

            if (donHang == null) return HttpNotFound();
            return View(donHang);
        }

        // ================== SỬA ĐƠN HÀNG ==================
        public ActionResult Edit(int id)
        {
            var donHang = db.DonHangs
                            .Include("ChiTietDonHangs.SanPham")
                            .Include("ThanhToans")
                            .FirstOrDefault(d => d.MaDH == id);

            if (donHang == null) return HttpNotFound();

            ViewBag.SanPhamList = new SelectList(db.SanPhams, "MaSP", "TenSP");
            return View(donHang);
        }

        // ================== THÊM MÓN ==================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThemMon(int maDH, int maSP, int soLuong)
        {
            var donHang = db.DonHangs.Include("ChiTietDonHangs").FirstOrDefault(d => d.MaDH == maDH);
            var sanPham = db.SanPhams.Find(maSP);

            if (donHang != null && sanPham != null)
            {
                var ct = new ChiTietDonHang
                {
                    MaDH = maDH,
                    MaSP = maSP,
                    SoLuong = soLuong,
                    DonGia = sanPham.Gia
                };

                db.ChiTietDonHangs.Add(ct);
                db.SaveChanges();

                donHang.TongTien = donHang.ChiTietDonHangs.Sum(c => c.SoLuong * c.DonGia);
                db.SaveChanges();
            }

            return RedirectToAction("Edit", new { id = maDH });
        }

        // ================== XÓA MÓN ==================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult XoaMon(int maDH, int maSP)
        {
            var donHang = db.DonHangs.Include("ChiTietDonHangs").FirstOrDefault(d => d.MaDH == maDH);
            var ct = db.ChiTietDonHangs.FirstOrDefault(c => c.MaDH == maDH && c.MaSP == maSP);

            if (ct != null)
            {
                db.ChiTietDonHangs.Remove(ct);
                db.SaveChanges();

                if (donHang != null)
                {
                    donHang.TongTien = donHang.ChiTietDonHangs.Sum(c => c.SoLuong * c.DonGia);
                    db.SaveChanges();
                }
            }

            return RedirectToAction("Edit", new { id = maDH });
        }

        // ================== XÓA ĐƠN HÀNG ==================
        public ActionResult Delete(int id)
        {
            var donHang = db.DonHangs.Find(id);
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
            }

            return RedirectToAction("Index");
        }

        // ================== DUYỆT ĐƠN HÀNG ==================
        public ActionResult DuyetDon(int maDH)
        {
            var donHang = db.DonHangs
                            .Include("NguoiDung")
                            .Include("ThanhToans")
                            .FirstOrDefault(dh => dh.MaDH == maDH);

            if (donHang == null) return HttpNotFound();

            var tt = donHang.ThanhToans.FirstOrDefault();

            // Nếu là VietQR/PayOS_QR mà chưa "Đã thanh toán" => không cho duyệt
            if (tt != null && tt.PhuongThuc != "COD" && tt.TrangThai != "Đã thanh toán")
            {
                TempData["Message"] = "Đơn này chưa thanh toán (VietQR/PayOS), không thể duyệt!";
                return RedirectToAction("Index");
            }

            // cập nhật trạng thái đơn
            donHang.TrangThaiDon = "Đã xác nhận";
            db.SaveChanges();

            // gửi email cho khách (nếu có email)
            if (donHang.NguoiDung != null && !string.IsNullOrWhiteSpace(donHang.NguoiDung.Email))
            {
                GuiEmailXacNhan(donHang.NguoiDung.Email, donHang.NguoiDung.HoTen, maDH);
            }

            TempData["Message"] = "Đơn hàng đã được xác nhận và email đã gửi cho khách!";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
