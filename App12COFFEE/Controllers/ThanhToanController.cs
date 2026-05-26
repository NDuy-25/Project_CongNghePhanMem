using App12COFFEE.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using App12COFFEE.Services;


namespace App12COFFEE.Controllers
{
    public class ThanhToanController : Controller
    {
        private Entities db = new Entities();
        private readonly string checksumKey = "4d746f39a6e477ced7f764e9566c424961a3055719d3c90f135019382b774bfc";

        // ===================== TẠO QR THANH TOÁN (NHẢY THẲNG PAYOS) ==========================
        public ActionResult ThanhToanVietQR(int maDH)
        {
            var dh = db.DonHangs.Find(maDH);
            if (dh == null) return HttpNotFound();

            // Tính lại tổng tiền từ chi tiết đơn hàng
            var dsChiTiet = db.ChiTietDonHangs.Where(x => x.MaDH == maDH).ToList();
            decimal tongTien = 0;
            foreach (var item in dsChiTiet)
            {
                tongTien += item.SoLuong.GetValueOrDefault() * (item.DonGia ?? 0m);
            }
            dh.TongTien = tongTien;
            db.SaveChanges();

            int amount = (int)Math.Round(tongTien);

            // cập nhật bản ghi ThanhToan
            var tt = db.ThanhToans.FirstOrDefault(x => x.MaDH == maDH);
            if (tt == null)
            {
                tt = new ThanhToan
                {
                    MaDH = maDH,
                    PhuongThuc = "PayOS_QR",
                    SoTien = amount,
                    NgayThanhToan = DateTime.Now,
                    TrangThai = "Chờ thanh toán"
                };
                db.ThanhToans.Add(tt);
            }
            else
            {
                tt.SoTien = amount;
                tt.PhuongThuc = "PayOS_QR";
                if (string.IsNullOrWhiteSpace(tt.TrangThai))
                    tt.TrangThai = "Chờ thanh toán";
            }
            db.SaveChanges();

            PayOSService pay = new PayOSService();
            string url = pay.CreatePayment(maDH, amount);

            return Redirect(url);
        }


        // ========================= RETURN URL =========================
        public ActionResult ThanhToanThanhCong(int maDH)
        {
            var dh = db.DonHangs.Find(maDH);
            if (dh == null) return HttpNotFound();

            ViewBag.MaDH = maDH;
            ViewBag.TongTien = dh.TongTien;
            return View();
        }

        public ActionResult ThanhToanThatBai(int maDH)
        {
            var dh = db.DonHangs.Find(maDH);
            if (dh == null) return HttpNotFound();

            ViewBag.MaDH = maDH;
            ViewBag.TongTien = dh.TongTien;
            return View();
        }

        // ========================= VIEW POLL TRẠNG THÁI (NẾU BẠN DÙNG) =========================
        [HttpGet]
        public JsonResult KiemTraTrangThai(int maDH)
        {
            var dh = db.DonHangs.Find(maDH);
            return Json(new { trangThai = dh?.TrangThaiDon ?? "" }, JsonRequestBehavior.AllowGet);
        }
        // ========================= CALLBACK WEBHOOK =========================
        [HttpPost]
        [AllowAnonymous]
        public ActionResult PayOSCallback()
        {
            Request.InputStream.Position = 0;
            string body = new StreamReader(Request.InputStream).ReadToEnd();

            LogPayOS("\n================ WEBHOOK =================");
            LogPayOS(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            LogPayOS("RAW => " + body);

            JObject json;
            try
            {
                json = JObject.Parse(body);
            }
            catch
            {
                return new HttpStatusCodeResult(400, "Invalid JSON");
            }

            var data = json["data"] as JObject;
            string signature = json["signature"]?.ToString();

            if (data == null || string.IsNullOrWhiteSpace(signature))
                return new HttpStatusCodeResult(400, "Missing data or signature");

            string raw = BuildRawSignature(data);
            string mySign = HashWithSHA256(raw, checksumKey);

            LogPayOS("RAW_TO_SIGN => " + raw);
            LogPayOS("SIGN_GEN    => " + mySign);
            LogPayOS("SIGN_PAYOS   => " + signature);

            if (!string.Equals(mySign, signature, StringComparison.OrdinalIgnoreCase))
                return new HttpStatusCodeResult(401, "Signature invalid");

            int maDH;
            if (!int.TryParse(data["orderCode"]?.ToString(), out maDH))
                return new HttpStatusCodeResult(400, "Invalid orderCode");

            var dh = db.DonHangs.Find(maDH);
            if (dh == null) return HttpNotFound();

            if (data["code"]?.ToString() == "00")
            {
                var tt = db.ThanhToans.FirstOrDefault(x => x.MaDH == maDH);
                if (tt != null)
                {
                    tt.TrangThai = "Đã thanh toán";
                    tt.NgayThanhToan = DateTime.Now;
                }

                db.SaveChanges();
            }
            else
            {
                LogPayOS($"NOT PAID: MaDH={maDH}, code={data["code"]}");
            }

            return Content("OK");
        }

        private string BuildRawSignature(JObject data)
        {
            var props = data.Properties()
                            .OrderBy(p => p.Name, StringComparer.Ordinal)
                            .ToList();

            var parts = new List<string>();
            foreach (var p in props)
            {
                string value = (p.Value == null || p.Value.Type == JTokenType.Null)
                    ? ""
: p.Value.ToString();

                parts.Add($"{p.Name}={value}");
            }

            return string.Join("&", parts);
        }

        private string HashWithSHA256(string data, string key)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(key)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }

        private void LogPayOS(string text)
        {
            try
            {
                using (var writer = new StreamWriter(Server.MapPath("~/payos_log.txt"), true))
                {
                    writer.WriteLine(text);
                }
            }
            catch { }
        }
    }
}