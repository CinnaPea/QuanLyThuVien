using WebApplication1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApplication1;
namespace LibraryManagement.Controllers
{
    [Authorize]
    public class BorrowController : Controller
    {
        private readonly QuanLyThuVienContext _db;
        public BorrowController(QuanLyThuVienContext db) => _db = db;

        private const string CART_KEY = "BORROW_CART";

        [HttpPost]
        public async Task<IActionResult> AddToCart(int sachId)
        {
            var sach = await _db.Saches.FirstOrDefaultAsync(x => x.SachId == sachId);
            if (sach == null) return NotFound();

            if (!(sach.TinhTrang == "Con" || sach.TinhTrang == "Available"))
            {
                TempData["err"] = "Sách không còn khả dụng.";
                return RedirectToAction("Details", "Books", new { id = sach.DauSachId });
            }

            var cart = HttpContext.Session.GetObject<List<int>>(CART_KEY) ?? new List<int>();
            if (!cart.Contains(sachId)) cart.Add(sachId);
            HttpContext.Session.SetObject(CART_KEY, cart);

            TempData["ok"] = "Đã thêm vào giỏ mượn.";
            return RedirectToAction("Cart");
        }

        public async Task<IActionResult> Cart()
        {
            var cart = HttpContext.Session.GetObject<List<int>>(CART_KEY) ?? new List<int>();

            var items = await _db.Saches
                .Include(x => x.DauSach)
                .Where(x => cart.Contains(x.SachId))
                .ToListAsync();

            return View(items);
        }

        [HttpPost]
        public IActionResult Remove(int sachId)
        {
            var cart = HttpContext.Session.GetObject<List<int>>(CART_KEY) ?? new List<int>();
            cart.Remove(sachId);
            HttpContext.Session.SetObject(CART_KEY, cart);
            return RedirectToAction("Cart");
        }

        [HttpPost]
        public async Task<IActionResult> CreatePhieuMuonOnline(string? ghiChu)
        {
            var cart = HttpContext.Session.GetObject<List<int>>(CART_KEY) ?? new List<int>();
            if (!cart.Any())
            {
                TempData["err"] = "Giỏ mượn trống.";
                return RedirectToAction("Cart");
            }

            // Lấy DocGiumId từ claim (được set khi đăng nhập)
            var docGiumIdStr = User.FindFirstValue("DocGiumId");
            if (string.IsNullOrWhiteSpace(docGiumIdStr))
            {
                TempData["err"] = "Tài khoản chưa gắn DocGia.";
                return RedirectToAction("Cart");
            }

            int docGiumId = int.Parse(docGiumIdStr);

            var saches = await _db.Saches.Where(x => cart.Contains(x.SachId)).ToListAsync();
            if (saches.Any(s => !(s.TinhTrang == "Con" || s.TinhTrang == "Available")))
            {
                TempData["err"] = "Có sách trong giỏ không còn khả dụng. Hãy kiểm tra lại.";
                return RedirectToAction("Cart");
            }

            var phieu = new PhieuMuon
            {
                DocGiaId = docGiumId,
                NgayMuon = DateTime.Now,
                HanTra = DateOnly.FromDateTime(DateTime.Now.AddDays(45)) // hoặc 0 ngày tùy bạn
 ,              // admin duyệt thì mới set
                TrangThai = "ChoDuyet",
                GhiChu = ghiChu
            };

            _db.PhieuMuons.Add(phieu);
            await _db.SaveChangesAsync();

            foreach (var s in saches)
            {
                _db.CtPhieuMuons.Add(new CtPhieuMuon
                {
                    PhieuMuonId = phieu.PhieuMuonId,
                    SachId = s.SachId,
                    NgayTraThucTe = null,
                    TinhTrangLucMuon = s.TinhTrang,
                    TinhTrangLucTra = null
                });
            }

            await _db.SaveChangesAsync();

            HttpContext.Session.Remove(CART_KEY);
            TempData["ok"] = $"Tạo phiếu mượn online thành công (Mã: {phieu.PhieuMuonId}).";
            return RedirectToAction("MyBorrows");
        }

        public async Task<IActionResult> MyBorrows()
        {
            var docGiumIdStr = User.FindFirstValue("DocGiumId");
            if (string.IsNullOrWhiteSpace(docGiumIdStr)) return Forbid();
            int docGiumId = int.Parse(docGiumIdStr);

            var list = await _db.PhieuMuons
                .Where(x => x.DocGiaId == docGiumId)
                .OrderByDescending(x => x.PhieuMuonId)
                .ToListAsync();

            return View(list);
        }
    }
}
