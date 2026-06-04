using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Text;
using System.Web;
using System.Web.Mvc;
using App12COFFEE.Models;

namespace App12COFFEE.Controllers
{
    public class NguoiDungsController : Controller
    {
        private Entities db = new Entities();

        // ========== HÀM MÃ HÓA & KIỂM TRA ==========
        public static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }

        public static bool VerifyHashedPassword(string hashedPassword, string providedPassword)
        {
            return hashedPassword == HashPassword(providedPassword);
        }

        // ========== RULE MẬT KHẨU MẠNH ==========
        private static bool IsStrongPassword(string password)
        {
            // >= 8 ký tự, có thường + hoa + số + ký tự đặc biệt
            var strongRegex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{8,}$");
            return !string.IsNullOrWhiteSpace(password) && strongRegex.IsMatch(password);
        }

        // ========== HÀM SINH OTP ==========
        public static string GenerateOTP()
        {
            Random rand = new Random();
            return rand.Next(100000, 999999).ToString();
        }

        // ========== HÀM GỬI EMAIL (GMAIL + APP PASSWORD) ==========
        public static void SendEmail(string toEmail, string subject, string body)
        {
            var fromAddress = new MailAddress("duy.nd.65cntt@ntu.edu.vn", "12 COFFEE");
            var toAddress = new MailAddress(toEmail);
            string fromPassword = "hvxy kyul vknx jilu";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new System.Net.NetworkCredential(fromAddress.Address, fromPassword)
            };

            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            })
            {
                try
                {
                    smtp.Send(message);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Lỗi gửi mail: " + ex.Message);
                }
            }
        }

        // ========== ĐĂNG KÝ ==========
        [HttpGet]
        public ActionResult DangKy()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DangKy(NguoiDung model)
        {
            // ====== BẮT BUỘC NHẬP ĐỦ THÔNG TIN ======
            model.DienThoai = (Request.Form["DienThoai"] ?? "").Trim();
            model.DiaChiGiaoHang = (Request.Form["DiaChiGiaoHang"] ?? "").Trim();
            model.Tinh = (Request.Form["Tinh"] ?? "").Trim();
            model.Huyen = (Request.Form["Huyen"] ?? "").Trim();
            model.Xa = (Request.Form["Xa"] ?? "").Trim();

            //  mặc định Khánh Hòa
            if (string.IsNullOrWhiteSpace(model.Tinh))
                model.Tinh = "Khánh Hòa";

            // Validate bắt buộc
            if (string.IsNullOrWhiteSpace(model.TenDangNhap))
                ModelState.AddModelError("TenDangNhap", "Vui lòng nhập tên đăng nhập.");

            if (string.IsNullOrWhiteSpace(model.MatKhau))
                ModelState.AddModelError("MatKhau", "Vui lòng nhập mật khẩu.");
            else if (!IsStrongPassword(model.MatKhau))
                ModelState.AddModelError("MatKhau", "Mật khẩu phải >= 8 ký tự và có chữ thường, chữ hoa, số, ký tự đặc biệt.");

            if (string.IsNullOrWhiteSpace(model.HoTen))
                ModelState.AddModelError("HoTen", "Vui lòng nhập họ tên.");

            if (string.IsNullOrWhiteSpace(model.Email))
                ModelState.AddModelError("Email", "Vui lòng nhập email.");

            if (string.IsNullOrWhiteSpace(model.DienThoai))
                ModelState.AddModelError("DienThoai", "Vui lòng nhập số điện thoại.");

            if (string.IsNullOrWhiteSpace(model.DiaChiGiaoHang))
                ModelState.AddModelError("DiaChiGiaoHang", "Vui lòng nhập địa chỉ giao hàng.");

            if (string.IsNullOrWhiteSpace(model.Tinh))
                ModelState.AddModelError("Tinh", "Vui lòng nhập tỉnh/thành phố.");

            if (string.IsNullOrWhiteSpace(model.Huyen))
                ModelState.AddModelError("Huyen", "Vui lòng nhập quận/huyện.");

            if (string.IsNullOrWhiteSpace(model.Xa))
                ModelState.AddModelError("Xa", "Vui lòng nhập phường/xã.");

            if (!ModelState.IsValid)
                return View(model);

            // 1. Kiểm tra trùng
            var existingUser = db.NguoiDungs
                .FirstOrDefault(u => u.TenDangNhap == model.TenDangNhap || u.Email == model.Email);

            if (existingUser != null)
            {
                ModelState.AddModelError("", "Tên đăng nhập hoặc Email đã tồn tại.");
                return View(model);
            }

            // 2. Thiết lập thông tin mặc định
            model.MatKhau = HashPassword(model.MatKhau);
            model.VaiTro = "Customer";
            model.Quyen = "User";
            model.TrangThai = false;
            model.NgayTao = DateTime.Now;

            // 3. Lưu user TẠM vào Session, chờ OTP
            Session["PendingUser"] = model;

            // 4. Sinh OTP và lưu vào Session
            string otp = GenerateOTP();
            Session["OTP_Register"] = otp;

            // 5. Gửi email OTP
            SendEmail(model.Email, "Mã OTP xác thực tài khoản",
                "Chào bạn,\n\nMã OTP xác thực tài khoản của bạn là: " + otp +
                "\nVui lòng không chia sẻ mã này cho bất kỳ ai.\n\n12 COFFEE");

            TempData["Message"] = "Đăng ký thành công, hãy kiểm tra email để lấy mã OTP.";
            return RedirectToAction("XacThucOTP");
        }

        // ========== XÁC THỰC OTP ĐĂNG KÝ ==========
        [HttpGet]
        public ActionResult XacThucOTP()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult XacThucOTP(string otp)
        {
            if (Session["OTP_Register"] != null && Session["PendingUser"] != null)
            {
                if (Session["OTP_Register"].ToString() == otp)
                {
                    var user = (NguoiDung)Session["PendingUser"];
                    user.TrangThai = true;

                    db.NguoiDungs.Add(user);
                    db.SaveChanges();

                    Session.Remove("OTP_Register");
                    Session.Remove("PendingUser");

                    TempData["Message"] = "Xác thực thành công, hãy đăng nhập.";
                    return RedirectToAction("DangNhap");
                }
                else
                {
                    ViewBag.Error = "Mã OTP không đúng.";
                    return View();
                }
            }

            ViewBag.Error = "Phiên xác thực không hợp lệ hoặc đã hết hạn.";
            return View();
        }

        // ========== ĐĂNG NHẬP ==========
        [HttpGet]
        public ActionResult DangNhap()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DangNhap(string TenDangNhap, string MatKhau)
        {
            var user = db.NguoiDungs.FirstOrDefault(x => x.TenDangNhap == TenDangNhap);

            if (user != null)
            {
                bool isPasswordValid = false;

                if (string.Equals((user.MatKhau ?? "").Trim(), (MatKhau ?? "").Trim(), StringComparison.Ordinal))
                {
                    isPasswordValid = true;
                }
                else if (VerifyHashedPassword(user.MatKhau, MatKhau))
                {
                    isPasswordValid = true;
                }

                if (isPasswordValid)
                {
                    if (user.TrangThai)
                    {
                        Session["MaND"] = user.MaND;
                        Session["UserID"] = user.MaND;
                        Session["UserName"] = user.HoTen;
                        Session["UserRole"] = user.VaiTro;
                        Session["Email"] = user.Email;
                        Session["Phone"] = user.DienThoai;

                        switch (user.VaiTro)
                        {
                            case "Admin":
                                return RedirectToAction("Index", "Admin");
                            case "Customer":
                                return RedirectToAction("TrangChu", "Home");
                            case "NhanVienDuyetDon":
                                return RedirectToAction("Dashboard", "NhanVienDuyetDon");
                            case "NhanVienGiaoHang":
                                return RedirectToAction("Dashboard", "NhanVienGiaoHang");
                            default:
                                return RedirectToAction("TrangChu", "Home");
                        }
                    }
                    else
                    {
                        ViewBag.Error = "Tài khoản chưa được kích hoạt!";
                        return View();
                    }
                }
            }

            ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không đúng.";
            return View();
        }

        // ========== QUÊN MẬT KHẨU ==========
        [HttpGet]
        public ActionResult QuenMatKhau()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult QuenMatKhau(string Email)
        {
            var user = db.NguoiDungs.FirstOrDefault(u => u.Email == Email);
            if (user == null)
            {
                ViewBag.Error = "Email không tồn tại trong hệ thống.";
                return View();
            }

            string otp = GenerateOTP();
            Session["OTP_Reset"] = otp;
            Session["ResetEmail"] = Email;

            SendEmail(Email, "Mã OTP quên mật khẩu",
                "Chào bạn,\n\nMã OTP đặt lại mật khẩu của bạn là: " + otp +
                "\nNếu bạn không yêu cầu đặt lại mật khẩu, hãy bỏ qua email này.\n\n12 COFFEE");

            return RedirectToAction("XacThucOTPReset");
        }

        // ========== XÁC THỰC OTP QUÊN MẬT KHẨU ==========
        [HttpGet]
        public ActionResult XacThucOTPReset()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult XacThucOTPReset(string otp)
        {
            if (Session["OTP_Reset"] != null && Session["ResetEmail"] != null)
            {
                if (Session["OTP_Reset"].ToString() == otp)
                {
                    return RedirectToAction("DatLaiMatKhau");
                }
                else
                {
                    ViewBag.Error = "Mã OTP không đúng.";
                    return View();
                }
            }

            ViewBag.Error = "Phiên OTP không hợp lệ hoặc đã hết hạn.";
            return View();
        }

        // ========== ĐẶT LẠI MẬT KHẨU ==========
        [HttpGet]
        public ActionResult DatLaiMatKhau()
        {
            if (Session["ResetEmail"] == null)
            {
                TempData["Message"] = "Phiên đặt lại mật khẩu đã hết hạn.";
                return RedirectToAction("QuenMatKhau");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DatLaiMatKhau(string newPassword)
        {
            if (Session["ResetEmail"] == null)
            {
                TempData["Message"] = "Phiên đặt lại mật khẩu đã hết hạn.";
                return RedirectToAction("QuenMatKhau");
            }

            //  Validate mật khẩu mạnh
            if (!IsStrongPassword(newPassword))
            {
                ViewBag.Error = "Mật khẩu mới phải >= 8 ký tự và có chữ thường, chữ hoa, số, ký tự đặc biệt.";
                return View();
            }

            string email = Session["ResetEmail"].ToString();
            var user = db.NguoiDungs.FirstOrDefault(u => u.Email == email);
            if (user != null)
            {
                user.MatKhau = HashPassword(newPassword);
                db.SaveChanges();
            }

            Session.Remove("OTP_Reset");
            Session.Remove("ResetEmail");

            TempData["Message"] = "Đổi mật khẩu thành công!";
            return RedirectToAction("DangNhap");
        }

        // ========== ĐỔI MẬT KHẨU (USER ĐÃ ĐĂNG NHẬP) ==========
        [HttpGet]
        public ActionResult DoiMatKhau()
        {
            if (Session["UserID"] == null)
            {
                TempData["Error"] = "Bạn cần đăng nhập để đổi mật khẩu.";
                return RedirectToAction("DangNhap");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DoiMatKhau(string oldPassword, string newPassword, string confirmPassword)
        {
            if (Session["UserID"] == null)
            {
                TempData["Error"] = "Bạn cần đăng nhập để đổi mật khẩu.";
                return RedirectToAction("DangNhap");
            }

            int userId = (int)Session["UserID"];
            var user = db.NguoiDungs.FirstOrDefault(u => u.MaND == userId);
            if (user == null)
            {
                ViewBag.Error = "Không tìm thấy người dùng.";
                return View();
            }

            // 1. Kiểm tra mật khẩu cũ
            bool isOldPasswordCorrect = false;

            // Trường hợp tài khoản cũ chưa mã hóa mật khẩu
            if ((user.VaiTro == "Admin" || user.VaiTro == "NhanVienDuyetDon" || user.VaiTro == "NhanVienGiaoHang") &&
                string.Equals((user.MatKhau ?? "").Trim(), (oldPassword ?? "").Trim(), StringComparison.Ordinal))
            {
                isOldPasswordCorrect = true;
            }
            else if (VerifyHashedPassword(user.MatKhau, oldPassword))
            {
                isOldPasswordCorrect = true;
            }

            if (!isOldPasswordCorrect)
            {
                ViewBag.Error = "Mật khẩu hiện tại không đúng.";
                return View();
            }

            // 2. Kiểm tra xác nhận mật khẩu
            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "Xác nhận mật khẩu không khớp.";
                return View();
            }

            //  Validate mật khẩu mạnh
            if (!IsStrongPassword(newPassword))
            {
                ViewBag.Error = "Mật khẩu mới phải >= 8 ký tự và có chữ thường, chữ hoa, số, ký tự đặc biệt.";
                return View();
            }

            // Không cho đặt trùng mật khẩu cũ
            if (!string.IsNullOrEmpty(oldPassword) && oldPassword == newPassword)
            {
                ViewBag.Error = "Mật khẩu mới không được trùng mật khẩu hiện tại.";
                return View();
            }

            // 3. Cập nhật mật khẩu mới (mã hóa SHA256)
            user.MatKhau = HashPassword(newPassword);
            db.SaveChanges();

            ViewBag.Message = "Đổi mật khẩu thành công!";
            return View();
        }

        // ========== ĐĂNG XUẤT ==========
        public ActionResult DangXuat()
        {
            Session.Clear();
            return RedirectToAction("TrangChu", "Home");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}



