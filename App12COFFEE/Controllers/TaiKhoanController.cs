using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Text;
using System.Web;
using System.Web.Mvc;
using App12COFFEE.Models;

namespace App12COFFEE.Controllers
{
    public class TaiKhoanController : Controller
    {
        private Entities db = new Entities();
        public static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password ?? ""));
                return Convert.ToBase64String(bytes);
            }
        }

        public static bool VerifyHashedPassword(string hashedPassword, string providedPassword)
        {
            return (hashedPassword ?? "") == HashPassword(providedPassword ?? "");
        }

        // ================== LẤY ID USER TỪ SESSION ==================
        private int? LayMaNguoiDung()
        {
            if (Session["MaND"] != null) return Convert.ToInt32(Session["MaND"]);

            if (Session["UserID"] != null)
            {
                if (int.TryParse(Session["UserID"].ToString(), out int id)) return id;
            }
            return null;
        }

        // GET: /TaiKhoan/ThongTin
        public ActionResult ThongTin()
        {
            var maND = LayMaNguoiDung();
            if (!maND.HasValue) return RedirectToAction("DangNhap", "NguoiDungs");

            var user = db.NguoiDungs.FirstOrDefault(x => x.MaND == maND.Value);
            if (user == null) return HttpNotFound();

            return View(user);
        }

        //  CHỈ cho cập nhật địa chỉ (không đổi họ tên/email/sđt/tên đăng nhập)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CapNhatDiaChi(string DiaChiGiaoHang, string Tinh, string Huyen, string Xa)
        {
            var maND = LayMaNguoiDung();
            if (!maND.HasValue) return RedirectToAction("DangNhap", "NguoiDungs");

            var user = db.NguoiDungs.FirstOrDefault(x => x.MaND == maND.Value);
            if (user == null) return HttpNotFound();

            user.DiaChiGiaoHang = (DiaChiGiaoHang ?? "").Trim();
            user.Tinh = (Tinh ?? "").Trim();
            user.Huyen = (Huyen ?? "").Trim();
            user.Xa = (Xa ?? "").Trim();

            db.SaveChanges();

            TempData["Message"] = "Cập nhật địa chỉ thành công!";
            return RedirectToAction("ThongTin");
        }

        // POST: /TaiKhoan/DoiMatKhau
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DoiMatKhau(string MatKhauHienTai, string MatKhauMoi, string XacNhanMatKhauMoi)
        {
            var maND = LayMaNguoiDung();
            if (!maND.HasValue) return RedirectToAction("DangNhap", "NguoiDungs");

            var user = db.NguoiDungs.FirstOrDefault(x => x.MaND == maND.Value);
            if (user == null) return HttpNotFound();

            MatKhauHienTai = (MatKhauHienTai ?? "").Trim();
            MatKhauMoi = (MatKhauMoi ?? "").Trim();
            XacNhanMatKhauMoi = (XacNhanMatKhauMoi ?? "").Trim();

            // 1) Validate input
            if (string.IsNullOrWhiteSpace(MatKhauHienTai) ||
                string.IsNullOrWhiteSpace(MatKhauMoi) ||
                string.IsNullOrWhiteSpace(XacNhanMatKhauMoi))
            {
                TempData["PassError"] = "Vui lòng nhập đầy đủ thông tin đổi mật khẩu.";
                return RedirectToAction("ThongTin");
            }

            if (MatKhauMoi != XacNhanMatKhauMoi)
            {
                TempData["PassError"] = "Xác nhận mật khẩu mới không khớp.";
                return RedirectToAction("ThongTin");
            }

            // 2) Rule mạnh: >= 8, có thường + hoa + số + ký tự đặc biệt
            var strongRegex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{8,}$");
            if (!strongRegex.IsMatch(MatKhauMoi))
            {
                TempData["PassError"] = "Mật khẩu mới phải ≥ 8 ký tự và có chữ thường, chữ hoa, số, ký tự đặc biệt.";
                return RedirectToAction("ThongTin");
            }

            // 3) Check mật khẩu hiện tại: hỗ trợ cả plain-text (role cũ) và hashed (customer)
            bool isOldPasswordCorrect = false;

            if ((user.VaiTro == "Admin" || user.VaiTro == "NhanVienDuyetDon" || user.VaiTro == "NhanVienGiaoHang") &&
                string.Equals((user.MatKhau ?? "").Trim(), MatKhauHienTai, StringComparison.Ordinal))
            {
                isOldPasswordCorrect = true;
            }
            else if (VerifyHashedPassword(user.MatKhau, MatKhauHienTai))
            {
                isOldPasswordCorrect = true;
            }

            if (!isOldPasswordCorrect)
            {
                TempData["PassError"] = "Mật khẩu hiện tại không đúng.";
                return RedirectToAction("ThongTin");
            }

            // 4) Không cho đặt mật khẩu mới trùng mật khẩu hiện tại
            // Nếu user đang hash thì so bằng hash; nếu user đang plain thì so plain
            bool isSameAsOld = false;

            if (user.VaiTro == "Admin" || user.VaiTro == "NhanVienDuyetDon" || user.VaiTro == "NhanVienGiaoHang")
            {
                // user có thể đang plain
                isSameAsOld = string.Equals((user.MatKhau ?? "").Trim(), MatKhauMoi, StringComparison.Ordinal)
                                             || VerifyHashedPassword(user.MatKhau, MatKhauMoi); // phòng khi admin đã được hash rồi
            }
            else
            {
                // customer: hash
                isSameAsOld = VerifyHashedPassword(user.MatKhau, MatKhauMoi);
            }

            if (isSameAsOld)
            {
                TempData["PassError"] = "Mật khẩu mới không được trùng mật khẩu hiện tại.";
                return RedirectToAction("ThongTin");
            }

            // 5) Update mật khẩu mới: luôn lưu dạng hash cho đồng bộ
            user.MatKhau = HashPassword(MatKhauMoi);
            db.SaveChanges();

            TempData["PassMessage"] = "Đổi mật khẩu thành công!";
            return RedirectToAction("ThongTin");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
