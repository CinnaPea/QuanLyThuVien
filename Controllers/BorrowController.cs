using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Helpers;
using WebApplication1.Models;
using WebApplication1.VMs;

namespace WebApplication1.Controllers
{
    public class BorrowController : Controller
    {
        private const string CART_KEY = "BORROW_CART_V1";
        private readonly QuanLyThuVienContext _db;

        public BorrowController(QuanLyThuVienContext db) => _db = db;

        // ===== session helpers =====
        private List<BorrowCartItemDto> GetCart()
            => SessionJson.Get<List<BorrowCartItemDto>>(HttpContext.Session, CART_KEY) ?? new List<BorrowCartItemDto>();

        private void SaveCart(List<BorrowCartItemDto> cart)
            => SessionJson.Set(HttpContext.Session, CART_KEY, cart);

        // ===== add đầu sách vào giỏ =====
        [HttpPost]
        public async Task<IActionResult> Add(int dauSachId, string? returnUrl = null)
        {
            var ds = await _db.DauSaches.FirstOrDefaultAsync(x => x.DauSachId == dauSachId);
            if (ds == null) return NotFound();

            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.DauSachId == dauSachId);

            var available = await _db.Saches.CountAsync(s => s.DauSachId == dauSachId && s.TinhTrang == "Con");

            if (available <= 0)
            {
                TempData["CartMessage"] = "❌ Đầu sách này hiện không còn cuốn nào có thể mượn.";
                TempData["CartMessageType"] = "error";
                return Redirect(returnUrl ?? Url.Action("Index", "Book")!);
            }

            if (item == null)
                cart.Add(new BorrowCartItemDto { DauSachId = dauSachId, Qty = 1, Selected = true });
            else if (item.Qty < available)
                item.Qty += 1;

            SaveCart(cart);

            TempData["CartMessage"] = "✅ Đã thêm vào giỏ mượn.";
            TempData["CartMessageType"] = "success";

            return Redirect(returnUrl ?? Url.Action("Index", "Book")!);
        }


        // ===== xem giỏ =====
        [HttpGet]
        public async Task<IActionResult> Cart()
        {
            var cart = GetCart();
            var vm = new BorrowCartVM();

            if (cart.Count == 0) return View(vm);

            var ids = cart.Select(x => x.DauSachId).ToList();

            var dauSachs = await _db.DauSaches
                .Include(x => x.TheLoai)
                .Include(x => x.NhaXuatBan)
                .Where(x => ids.Contains(x.DauSachId))
                .ToListAsync();

            // available theo đầu sách
            var availDict = await _db.Saches
                .Where(s => ids.Contains(s.DauSachId))
                .GroupBy(s => s.DauSachId)
                .Select(g => new
                {
                    DauSachId = g.Key,
                    Available = g.Count(x => x.TinhTrang == "Con")
                })
                .ToDictionaryAsync(x => x.DauSachId, x => x.Available);

            foreach (var ds in dauSachs.OrderBy(x => x.TieuDe))
            {
                var dto = cart.First(x => x.DauSachId == ds.DauSachId);
                availDict.TryGetValue(ds.DauSachId, out var av);

                // clamp qty theo available (đề phòng kho thay đổi)
                var qty = dto.Qty;
                if (av <= 0) qty = 0;
                else if (qty > av) qty = av;

                vm.Items.Add(new BorrowCartItemVM
                {
                    DauSachId = ds.DauSachId,
                    TieuDe = ds.TieuDe,
                    TheLoai = ds.TheLoai?.TenTheLoai,
                    NXB = ds.NhaXuatBan?.TenNxb,
                    Available = av,
                    Qty = qty,
                    Selected = dto.Selected
                });
            }

            // cập nhật lại giỏ (nếu qty bị clamp)
            foreach (var row in vm.Items)
            {
                var dto = cart.First(x => x.DauSachId == row.DauSachId);
                dto.Qty = row.Qty;
            }
            SaveCart(cart);

            return View(vm);
        }

        // ===== tăng số lượng (+1) =====
        [HttpPost]
        public async Task<IActionResult> Plus(int dauSachId)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.DauSachId == dauSachId);
            if (item == null) return RedirectToAction("Cart");

            var available = await _db.Saches.CountAsync(s => s.DauSachId == dauSachId && s.TinhTrang == "Con");
            if (item.Qty < available) item.Qty += 1;

            SaveCart(cart);
            return RedirectToAction("Cart");
        }

        // ===== giảm số lượng (-1) =====
        [HttpPost]
        public IActionResult Minus(int dauSachId)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.DauSachId == dauSachId);
            if (item == null) return RedirectToAction("Cart");

            item.Qty -= 1;
            if (item.Qty <= 0)
                cart.Remove(item);

            SaveCart(cart);
            return RedirectToAction("Cart");
        }

        // ===== toggle checkbox =====
        [HttpPost]
        public IActionResult ToggleSelect(int dauSachId, bool selected)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.DauSachId == dauSachId);
            if (item != null)
            {
                item.Selected = selected;
                SaveCart(cart);
            }
            return RedirectToAction("Cart");
        }

        // ===== xóa dòng =====
        [HttpPost]
        public IActionResult Remove(int dauSachId)
        {
            var cart = GetCart();
            cart.RemoveAll(x => x.DauSachId == dauSachId);
            SaveCart(cart);
            return RedirectToAction("Cart");
        }


        // ===== xóa hết =====
        [HttpPost]
        public IActionResult Clear()
        {
            HttpContext.Session.Remove(CART_KEY);
            return RedirectToAction("Cart");
        }

        // ===== Lập phiếu mượn cho những dòng được chọn =====
        // NOTE: bạn có thể lấy docGiaId từ login/session, ở đây demo nhập docGiaId
        [HttpPost]
        public async Task<IActionResult> CreateBorrowSlip(int soNgayMuon = 7)
        {
            var role = HttpContext.Session.GetString("ROLE");
            if (role != "User")
            {
                TempData["Error"] = "Chỉ độc giả mới được lập phiếu mượn.";
                return RedirectToAction("Cart");
            }

            var docGiaId = HttpContext.Session.GetInt32("DOCGIA_ID");
            if (docGiaId == null)
            {
                TempData["Error"] = "Không xác định được độc giả.";
                return RedirectToAction("Cart");
            }

            var cart = GetCart();
            var selectedItems = cart.Where(x => x.Selected && x.Qty > 0).ToList();
            if (!selectedItems.Any())
            {
                TempData["Error"] = "Bạn chưa chọn đầu sách nào.";
                return RedirectToAction("Cart");
            }

            if (soNgayMuon < 1) soNgayMuon = 7;

            var phieu = new PhieuMuon
            {
                DocGiaId = docGiaId.Value,
                NgayMuon = DateTime.Now,
                HanTra = DateOnly.FromDateTime(DateTime.Now.AddDays(soNgayMuon)),
                TrangThai = "ChoDuyet"
            };

            _db.PhieuMuons.Add(phieu);
            await _db.SaveChangesAsync();

            // ===== CHỐT SÁCH + GIỮ SÁCH =====
            foreach (var it in selectedItems)
            {
                var books = await _db.Saches
                    .Where(s => s.DauSachId == it.DauSachId && s.TinhTrang == "Con")
                    .OrderBy(s => s.SachId)
                    .Take(it.Qty)
                    .ToListAsync();

                if (books.Count < it.Qty)
                {
                    TempData["Error"] = "Số lượng sách không đủ.";
                    return RedirectToAction("Cart");
                }

                foreach (var sach in books)
                {
                    _db.CtPhieuMuons.Add(new CtPhieuMuon
                    {
                        PhieuMuonId = phieu.PhieuMuonId,
                        SachId = sach.SachId,
                        TinhTrangLucMuon = sach.TinhTrang
                    });

                    // ⭐ GIỮ SÁCH
                    sach.TinhTrang = "DatTruoc";
                }
            }

            await _db.SaveChangesAsync();

            // ===== XÓA ITEM ĐÃ TẠO PHIẾU =====
            cart = cart.Where(x => !x.Selected).ToList();
            SaveCart(cart);

            TempData["Success"] = $"✅ Đã lập phiếu mượn #{phieu.PhieuMuonId}.";
            return RedirectToAction("Cart");
        }

    }
}
