using App12COFFEE.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace App12COFFEE.Controllers
{
    public class GioHangController : Controller
    {
        private Entities db = new Entities();

        // ================== HÀM DÙNG CHUNG ==================
        private int? LayMaNguoiDung()
        {
            if (Session["MaND"] == null) return null;
            return Convert.ToInt32(Session["MaND"]);
        }

        private GioHang LayGioHangCuaUser()
        {
            var maND = LayMaNguoiDung();
            if (!maND.HasValue) return null;

            var gh = db.GioHangs.FirstOrDefault(x => x.MaND == maND.Value);
            if (gh == null)
            {
                gh = new GioHang
                {
                    MaND = maND.Value,
                    NgayCapNhat = DateTime.Now
                };
                db.GioHangs.Add(gh);
                db.SaveChanges();
            }
            return gh;
        }

        private void CapNhatSoLuongGio(GioHang gh)
        {
            if (gh == null)
            {
                Session["CartCount"] = 0;
                return;
            }

            var tongSL = db.ChiTietGioHangs
                           .Where(x => x.MaGH == gh.MaGH)
                           .Select(x => (int?)x.SoLuong)
                           .Sum() ?? 0;

            Session["CartCount"] = tongSL;
        }

        // ================== XEM GIỎ HÀNG ==================
        public ActionResult Index()
        {
            var maND = LayMaNguoiDung();
            if (!maND.HasValue) return RedirectToAction("DangNhap", "NguoiDungs");

            var gh = LayGioHangCuaUser();
            var user = db.NguoiDungs.FirstOrDefault(x => x.MaND == maND.Value);
            var dc = db.DiaChiGiaoHangs.Where(x => x.MaND == maND.Value)
                                       .OrderByDescending(x => x.MaDC)
                                       .FirstOrDefault();

            ViewBag.HoTen = user?.HoTen;
            ViewBag.DienThoai = user?.DienThoai;
            ViewBag.DiaChi = dc?.DiaChi;

            if (gh == null)
            {
                ViewBag.TongTien = 0;
                return View(Enumerable.Empty<ChiTietGioHang>());
            }

            var dsChiTiet = db.ChiTietGioHangs.Where(x => x.MaGH == gh.MaGH).ToList();

            decimal tongTien = 0;
            foreach (var item in dsChiTiet)
            {
                if (item.SanPham != null)
                {
                    tongTien += item.SoLuong.GetValueOrDefault() * (item.SanPham.Gia ?? 0m);
                }
            }
            ViewBag.TongTien = tongTien;

            if (Session["CartNotes"] == null)
            {
                Session["CartNotes"] = new Dictionary<int, string>();
            }

            return View(dsChiTiet);
        }

        // ================== THÊM VÀO GIỎ ==================
        public ActionResult Them(int maSP)
        {
            var maND = LayMaNguoiDung();
            if (!maND.HasValue) return RedirectToAction("DangNhap", "NguoiDungs");

            var gh = LayGioHangCuaUser();
            if (gh == null) return RedirectToAction("Index");

            var sp = db.SanPhams.Find(maSP);
            if (sp == null) return Redirect(Request.UrlReferrer?.ToString() ?? Url.Action("Menu", "Home"));

            var ct = db.ChiTietGioHangs.FirstOrDefault(x => x.MaGH == gh.MaGH && x.MaSP == maSP);
            if (ct == null)
            {
                ct = new ChiTietGioHang
                {
                    MaGH = gh.MaGH,
                    MaSP = maSP,
                    SoLuong = 1
                };
                db.ChiTietGioHangs.Add(ct);
            }
            else
            {
                ct.SoLuong = ct.SoLuong.GetValueOrDefault() + 1;
            }

            gh.NgayCapNhat = DateTime.Now;
            db.SaveChanges();
            CapNhatSoLuongGio(gh);

            return Redirect(Request.UrlReferrer?.ToString() ?? Url.Action("Menu", "Home"));
        }

        // ================== CẬP NHẬT GIỎ HÀNG ==================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CapNhatGioHang(FormCollection form)
        {
            var maND = LayMaNguoiDung();
            if (!maND.HasValue) return RedirectToAction("DangNhap", "NguoiDungs");

            var gh = LayGioHangCuaUser();
            if (gh == null) return RedirectToAction("Index");

            var dsChiTiet = db.ChiTietGioHangs.Where(x => x.MaGH == gh.MaGH).ToList();
            var notes = new Dictionary<int, string>();
            var danhSachXoa = new List<ChiTietGioHang>();

            foreach (var item in dsChiTiet)
            {
                string keySL = "SoLuong_" + item.MaSP;
                string keyNote = "GhiChu_" + item.MaSP;

                int soLuongMoi = item.SoLuong.GetValueOrDefault();
                if (int.TryParse(form[keySL], out int sl)) soLuongMoi = sl;

                if (soLuongMoi <= 0) danhSachXoa.Add(item);
                else item.SoLuong = soLuongMoi;

                string ghiChu = form[keyNote];
                if (!string.IsNullOrWhiteSpace(ghiChu)) notes[item.MaSP] = ghiChu.Trim();
            }

            if (danhSachXoa.Any()) db.ChiTietGioHangs.RemoveRange(danhSachXoa);

            gh.NgayCapNhat = DateTime.Now;
            db.SaveChanges();
            Session["CartNotes"] = notes;
            CapNhatSoLuongGio(gh);

            TempData["Message"] = "Đã cập nhật giỏ hàng.";
            return RedirectToAction("Index");
        }

        // ================== XÓA 1 MÓN ==================
        public ActionResult Xoa(int maSP)
        {
            var maND = LayMaNguoiDung();
            if (!maND.HasValue) return RedirectToAction("DangNhap", "NguoiDungs");

            var gh = LayGioHangCuaUser();
            if (gh == null) return RedirectToAction("Index");

            var ct = db.ChiTietGioHangs.FirstOrDefault(x => x.MaGH == gh.MaGH && x.MaSP == maSP);
            if (ct != null)
            {
                db.ChiTietGioHangs.Remove(ct);
                db.SaveChanges();
            }

            var notes = Session["CartNotes"] as Dictionary<int, string>;
            if (notes != null && notes.ContainsKey(maSP))
            {
                notes.Remove(maSP);
                Session["CartNotes"] = notes;
            }

            CapNhatSoLuongGio(gh);
            return RedirectToAction("Index");
        }

        // ================== THANH TOÁN ==================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThanhToan(string HoTen, string DienThoai, string DiaChi, string GhiChu, string PhuongThuc, FormCollection form)
        {
            var maND = LayMaNguoiDung();
            if (!maND.HasValue) return RedirectToAction("DangNhap", "NguoiDungs");

            var gh = LayGioHangCuaUser();
            if (gh == null) return RedirectToAction("Index");

            var dsChiTiet = db.ChiTietGioHangs.Where(x => x.MaGH == gh.MaGH).ToList();
            if (!dsChiTiet.Any())
            {
                TempData["Message"] = "Giỏ hàng trống, không thể thanh toán.";
                return RedirectToAction("Index");
            }

            var danhSachXoa = new List<ChiTietGioHang>();
            foreach (var item in dsChiTiet)
            {
                string keySL = "SoLuong_" + item.MaSP;
                if (int.TryParse(form[keySL], out int slMoi))
                {
                    if (slMoi <= 0) danhSachXoa.Add(item);
                    else item.SoLuong = slMoi;
                }
            }

            if (danhSachXoa.Any()) db.ChiTietGioHangs.RemoveRange(danhSachXoa);
            gh.NgayCapNhat = DateTime.Now;
            db.SaveChanges();

            dsChiTiet = db.ChiTietGioHangs.Where(x => x.MaGH == gh.MaGH).ToList();
            if (!dsChiTiet.Any())
            {
                TempData["Message"] = "Giỏ hàng trống sau khi cập nhật.";
                return RedirectToAction("Index");
            }

            // ✅ 2) Tính tổng tiền lại theo DB (đảm bảo đúng số lượng)
            decimal tongTien = 0;
            foreach (var item in dsChiTiet)
            {
                var sp = db.SanPhams.Find(item.MaSP);
                if (sp != null)
                {
                    // Nếu SoLuong hoặc Gia null thì lấy 0
                    tongTien += item.SoLuong.GetValueOrDefault() * (sp.Gia ?? 0m);
                }
            }

            // 3) Lưu địa chỉ giao hàng
            var diaChiGiao = new DiaChiGiaoHang
            {
                MaND = maND.Value,
                DiaChi = DiaChi,
                Tinh = "",
                Huyen = "",
                Xa = "",
                GhiChu = GhiChu,
                LaMacDinh = false
            };
            db.DiaChiGiaoHangs.Add(diaChiGiao);
            db.SaveChanges();

            // 4) Tạo đơn hàng
            var don = new DonHang
            {
                MaND = maND.Value,
                MaDC = diaChiGiao.MaDC,
                NgayDat = DateTime.Now,
                TongTien = tongTien,
                PhiVanChuyen = 0,
                TrangThaiDon = "Chờ xác nhận",
                MaNguoiDuyet = null
            };
            db.DonHangs.Add(don);
            db.SaveChanges();

            // 5) Lưu chi tiết đơn hàng
            foreach (var item in dsChiTiet)
            {
                var sp = db.SanPhams.Find(item.MaSP);
                if (sp == null) continue;
                var ctdh = new ChiTietDonHang
                {
                    MaDH = don.MaDH,
                    MaSP = item.MaSP,
                    SoLuong = item.SoLuong,     // ✅ đúng số lượng mới
                    DonGia = sp.Gia
                };
                db.ChiTietDonHangs.Add(ctdh);
            }

            // 6) Lưu thông tin thanh toán
            var thanhToan = new ThanhToan
            {
                MaDH = don.MaDH,
                PhuongThuc = PhuongThuc,
                SoTien = tongTien,
                NgayThanhToan = DateTime.Now,
                TrangThai = (PhuongThuc == "VietQR") ? "Chờ thanh toán" : "Chờ xử lý"
            };
            db.ThanhToans.Add(thanhToan);

            // 7) Xóa giỏ hàng
            db.ChiTietGioHangs.RemoveRange(dsChiTiet);
            db.SaveChanges();

            Session["CartNotes"] = null;
            CapNhatSoLuongGio(gh);

            // 8) Nếu VietQR -> chuyển sang PayOS QR
            if (PhuongThuc == "VietQR")
            {
                return RedirectToAction("ThanhToanVietQR", "ThanhToan", new { maDH = don.MaDH });
            }

            // 9) COD -> giữ luồng cũ
            ViewBag.HoTen = HoTen;
            ViewBag.DienThoai = DienThoai;
            ViewBag.DiaChi = DiaChi;
            ViewBag.GhiChu = GhiChu;
            ViewBag.PhuongThuc = PhuongThuc;
            ViewBag.TongTien = tongTien;
            ViewBag.MaDH = don.MaDH;
            ViewBag.ThoiGian = DateTime.Now;

            return View("ThanhToanThanhCong");
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

