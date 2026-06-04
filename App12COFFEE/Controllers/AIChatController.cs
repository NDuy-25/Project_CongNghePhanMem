using App12COFFEE.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace App12COFFEE.Controllers
{
    public class AIChatController : Controller
    {
        private Entities db = new Entities();

        private class GoiYSanPham
        {
            public SanPham SanPham { get; set; }
            public int Diem { get; set; }
        }

        private int? LayMaNguoiDung()
        {
            if (Session["MaND"] != null) return Convert.ToInt32(Session["MaND"]);
            if (Session["UserID"] != null) return Convert.ToInt32(Session["UserID"]);
            return null;
        }

        private static string ChuanHoaTuKhoa(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;

            string normalized = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder();

            foreach (char c in normalized)
            {
                var category = CharUnicodeInfo.GetUnicodeCategory(c);
                if (category != UnicodeCategory.NonSpacingMark)
                {
                    builder.Append(c);
                }
            }

            return builder.ToString()
                .Normalize(NormalizationForm.FormC)
                .Replace('đ', 'd')
                .Replace("  ", " ");
        }

        private static bool CoTu(string text, params string[] keys)
        {
            return keys.Any(k => text.Contains(k));
        }

        private static List<string> TachTuKhoa(string noiDung)
        {
            string text = ChuanHoaTuKhoa(noiDung);
            var tuKhoa = text.Split(new[] { ' ', ',', '.', ';', ':', '-', '_', '/', '\\', '(', ')' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(t => t.Length >= 2)
                .Distinct()
                .ToList();

            if (CoTu(text, "ca phe", "cafe", "coffee", "bac xiu", "espresso", "latte"))
                tuKhoa.AddRange(new[] { "ca", "phe", "cafe", "coffee", "bac", "xiu" });
            if (CoTu(text, "tra", "dao", "vai", "sen", "olong", "o long"))
                tuKhoa.AddRange(new[] { "tra", "dao", "vai", "sen", "olong" });
            if (CoTu(text, "matcha", "thai xanh", "tra xanh"))
                tuKhoa.AddRange(new[] { "matcha", "xanh" });
            if (CoTu(text, "soda", "co gas", "giai khat"))
                tuKhoa.AddRange(new[] { "soda", "chanh", "tac" });
            if (CoTu(text, "nuoc ep", "ep", "trai cay", "vitamin"))
                tuKhoa.AddRange(new[] { "ep", "trai", "cay" });
            if (CoTu(text, "da xay", "xay", "frappe"))
                tuKhoa.AddRange(new[] { "xay", "da" });
            if (CoTu(text, "yogurt", "sua chua", "chua"))
                tuKhoa.AddRange(new[] { "yogurt", "chua" });
            if (CoTu(text, "ngot", "beo", "sua", "kem"))
                tuKhoa.AddRange(new[] { "sua", "kem", "beo" });
            if (CoTu(text, "chua", "thanh", "mat", "tuoi", "nong", "giai nhiet"))
                tuKhoa.AddRange(new[] { "chanh", "tac", "dao", "vai", "ep", "soda" });

            return tuKhoa.Distinct().ToList();
        }

        private AIChat LayHoacTaoChat(int maND)
        {
            var chat = db.AIChats
                .Include(c => c.AITinNhans)
                .Where(c => c.MaND == maND)
                .OrderByDescending(c => c.NgayTao)
                .FirstOrDefault();

            if (chat != null) return chat;

            chat = new AIChat
            {
                MaND = maND,
                TieuDe = "Tư vấn sản phẩm",
                NgayTao = DateTime.Now
            };
            db.AIChats.Add(chat);
            db.SaveChanges();

            ThemTinNhanAI(chat.MaChat,
                "Xin chào, mình có thể tư vấn món theo khẩu vị, giá tiền, thời tiết, tâm trạng hoặc nhu cầu như ít ngọt, ít đá, đồ mát, cà phê tỉnh táo, trà trái cây, soda, yogurt. Bạn muốn uống kiểu nào?");
            db.Entry(chat).Collection(c => c.AITinNhans).Load();
            return chat;
        }

        private void ThemTinNhanAI(int maChat, string noiDung)
        {
            db.AITinNhans.Add(new AITinNhan
            {
                MaChat = maChat,
                LoaiNguoiGui = "AI",
                NoiDung = noiDung,
                ThoiGian = DateTime.Now,
                DaDoc = false,
                Tokens = noiDung.Length
            });
            db.SaveChanges();
        }

        private static List<GoiYSanPham> ChamDiemSanPham(IEnumerable<SanPham> sanPhams, string noiDung)
        {
            string text = ChuanHoaTuKhoa(noiDung);
            var tuKhoa = TachTuKhoa(noiDung);
            decimal? giaToiDa = null;

            if (CoTu(text, "re", "duoi 30000", "duoi 30", "30k", "sinh vien")) giaToiDa = 30000;
            if (CoTu(text, "duoi 40000", "duoi 40", "40k")) giaToiDa = 40000;

            var ketQua = new List<GoiYSanPham>();
            foreach (var sp in sanPhams)
            {
                string noiDungTimKiem = ChuanHoaTuKhoa((sp.TenSP ?? "") + " " + (sp.MoTa ?? "") + " " + (sp.MoTaChiTiet ?? "") + " " + (sp.DanhMuc != null ? sp.DanhMuc.TenDM : ""));
                int diem = 0;

                foreach (var tu in tuKhoa)
                {
                    if (noiDungTimKiem.Contains(tu)) diem += tu.Length >= 4 ? 3 : 1;
                }

                if (giaToiDa.HasValue && (sp.Gia ?? 0) <= giaToiDa.Value) diem += 4;
                if (CoTu(text, "ban chay", "pho bien", "nhieu nguoi", "hot", "best seller")) diem += sp.LuotBan.GetValueOrDefault() > 0 ? 5 : 1;
                if (CoTu(text, "khong ca phe", "khong cafe") && CoTu(noiDungTimKiem, "ca phe", "cafe")) diem -= 8;
                if (CoTu(text, "khong sua", "it beo") && CoTu(noiDungTimKiem, "sua", "kem", "yogurt")) diem -= 4;

                if (diem > 0)
                {
                    ketQua.Add(new GoiYSanPham { SanPham = sp, Diem = diem });
                }
            }

            return ketQua
                .OrderByDescending(x => x.Diem)
                .ThenByDescending(x => x.SanPham.LuotBan ?? 0)
                .ThenBy(x => x.SanPham.Gia ?? 0)
                .Take(5)
                .ToList();
        }

        private string TaoTraLoi(string noiDung, List<SanPham> sanPhams)
        {
            string text = ChuanHoaTuKhoa(noiDung);

            if (CoTu(text, "dat hang", "mua hang", "them gio", "thanh toan", "giao hang", "ship"))
            {
                return "Bạn có thể chọn món ở Menu, bấm Đặt hàng hoặc Thêm vào giỏ, vào Giỏ hàng nhập địa chỉ giao hàng rồi chọn Tiền mặt khi nhận hoặc VietQR. Sau khi đặt, bạn theo dõi trạng thái ở mục Đơn hàng.";
            }

            if (CoTu(text, "dia chi", "lien he", "o dau", "quan o dau"))
            {
                return "12 COFFEE hiện hiển thị địa chỉ Tô Hiến Thành, Vạn Ninh, Khánh Hòa. Bạn có thể xem thêm ở mục Liên hệ trên thanh menu.";
            }

            if (CoTu(text, "goi y", "hoi duoc gi", "ban lam duoc gi", "huong dan", "help"))
            {
                return "Bạn có thể hỏi mình như: 'món nào mát ít ngọt?', 'cà phê nào tỉnh táo?', 'đồ uống dưới 30k?', 'không uống cà phê thì chọn gì?', 'món nào hợp trời nóng?', 'gợi ý trà trái cây', hoặc 'cách đặt hàng và thanh toán'.";
            }

            var goiY = ChamDiemSanPham(sanPhams, noiDung).Select(x => x.SanPham).ToList();
            if (!goiY.Any())
            {
                if (CoTu(text, "re", "duoi", "sinh vien"))
                    goiY = sanPhams.OrderBy(sp => sp.Gia ?? 0).Take(4).ToList();
                else if (CoTu(text, "ban chay", "hot", "pho bien"))
                    goiY = sanPhams.OrderByDescending(sp => sp.LuotBan ?? 0).ThenBy(sp => sp.Gia ?? 0).Take(4).ToList();
                else
                    goiY = sanPhams.OrderBy(sp => Guid.NewGuid()).Take(4).ToList();
            }

            if (!goiY.Any())
                return "Hiện SQL chưa có sản phẩm đang bán để tư vấn. Bạn kiểm tra lại dữ liệu sản phẩm trong phần quản trị nhé.";

            string moDau = "Mình gợi ý cho bạn: ";
            if (CoTu(text, "nong", "giai nhiet", "mat", "khát", "khat")) moDau = "Nếu muốn uống mát và dễ chịu, mình gợi ý: ";
            if (CoTu(text, "tinh tao", "buon ngu", "hoc bai", "lam viec")) moDau = "Nếu cần tỉnh táo để học/làm việc, bạn có thể chọn: ";
            if (CoTu(text, "it ngot", "khong ngot", "giam ngot")) moDau = "Nếu muốn ít ngọt, bạn có thể chọn món này và ghi chú giảm ngọt khi thanh toán: ";

            string danhSach = string.Join("; ", goiY.Select(sp => sp.TenSP + " - " + string.Format("{0:N0}", sp.Gia ?? 0) + "đ"));
            return moDau + danhSach + ". Bạn có thể bấm Mở Menu để xem ảnh, hoặc nhập đúng tên món để mình tư vấn tiếp.";
        }

        public ActionResult Index()
        {
            var maND = LayMaNguoiDung();
            if (!maND.HasValue) return RedirectToAction("DangNhap", "NguoiDungs");

            var chat = LayHoacTaoChat(maND.Value);
            ViewBag.GoiY = new[]
            {
                "Món nào mát, ít ngọt?",
                "Gợi ý đồ uống dưới 30k",
                "Không uống cà phê thì chọn gì?",
                "Cà phê nào giúp tỉnh táo?",
                "Trà trái cây nào dễ uống?",
                "Món nào hợp trời nóng?",
                "Cách đặt hàng và thanh toán",
                "Gợi ý món cho học bài"
            };
            return View(chat);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GuiTinNhan(string noiDung)
        {
            var maND = LayMaNguoiDung();
            if (!maND.HasValue) return RedirectToAction("DangNhap", "NguoiDungs");
            if (string.IsNullOrWhiteSpace(noiDung)) return RedirectToAction("Index");

            var chat = LayHoacTaoChat(maND.Value);
            string cauHoi = noiDung.Trim();

            db.AITinNhans.Add(new AITinNhan
            {
                MaChat = chat.MaChat,
                LoaiNguoiGui = "KhachHang",
                NoiDung = cauHoi,
                ThoiGian = DateTime.Now,
                DaDoc = true,
                Tokens = cauHoi.Length
            });

            var sanPhams = db.SanPhams
                .Include(sp => sp.DanhMuc)
                .Where(sp => sp.TrangThai == true)
                .ToList();

            string traLoi = TaoTraLoi(cauHoi, sanPhams);
            db.AITinNhans.Add(new AITinNhan
            {
                MaChat = chat.MaChat,
                LoaiNguoiGui = "AI",
                NoiDung = traLoi,
                ThoiGian = DateTime.Now,
                DaDoc = false,
                Tokens = traLoi.Length
            });

            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
