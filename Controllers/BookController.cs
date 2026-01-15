using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;
using WebApplication1.VMs;

namespace WebApplication1.Controllers
{
    public class BookController : Controller
    {
        private readonly QuanLyThuVienContext _db;
        public BookController(QuanLyThuVienContext db) => _db = db;

        public async Task<IActionResult> Index(string? q, int? theLoaiId, int? nhaXuatBanId, int? tacGiaId)
        {
            // ====== Query DauSach + include để lấy tên thể loại / NXB ======
            var dsQuery = _db.DauSaches
                .Include(x => x.TheLoai)
                .Include(x => x.NhaXuatBan)
                .AsQueryable();

            // search
            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                dsQuery = dsQuery.Where(x => x.TieuDe.Contains(q)
                                          || (x.Isbn != null && x.Isbn.Contains(q)));
            }

            // filter
            if (theLoaiId.HasValue) dsQuery = dsQuery.Where(x => x.TheLoaiId == theLoaiId.Value);
            if (nhaXuatBanId.HasValue) dsQuery = dsQuery.Where(x => x.NhaXuatBanId == nhaXuatBanId.Value);

            // filter tác giả (N-N) qua bảng liên kết
            if (tacGiaId.HasValue)
            {
                dsQuery = dsQuery.Where(ds =>
                    _db.DauSachTacGia.Any(link => link.DauSachId == ds.DauSachId && link.TacGiaId == tacGiaId.Value));
            }

            // lấy list đầu sách (để lấy ids)
            var dauSachList = await dsQuery
                .OrderBy(x => x.TieuDe)
                .ToListAsync();

            var ids = dauSachList.Select(x => x.DauSachId).ToList();

            // ====== Đếm số cuốn từ bảng Sach ======
            var counts = await _db.Saches
                .Where(s => ids.Contains(s.DauSachId))
                .GroupBy(s => s.DauSachId)
                .Select(g => new
                {
                    DauSachId = g.Key,
                    Total = g.Count(),
                    Available = g.Count(x => x.TinhTrang == "Con") // đổi nếu bạn dùng giá trị khác
                })
                .ToListAsync();

            var countMap = counts.ToDictionary(x => x.DauSachId, x => x);

            // ====== Map tác giả text (N-N) ======
            var authorPairs = await _db.DauSachTacGia
                .Where(x => ids.Contains(x.DauSachId))
                .Join(_db.TacGia,
                      link => link.TacGiaId,
                      tg => tg.TacGiaId,
                      (link, tg) => new { link.DauSachId, tg.TenTacGia })
                .ToListAsync();

            var tacGiaTextMap = authorPairs
                .GroupBy(x => x.DauSachId)
                .ToDictionary(g => g.Key, g => string.Join(", ", g.Select(x => x.TenTacGia)));

            // ====== Build VM (đầu sách + counts + thể loại) ======
            var items = new List<DauSachListItemVM>();
            foreach (var ds in dauSachList)
            {
                countMap.TryGetValue(ds.DauSachId, out var c);

                items.Add(new DauSachListItemVM
                {
                    DauSachId = ds.DauSachId,
                    TieuDe = ds.TieuDe,
                    ISBN = ds.Isbn,
                    TenTheLoai = ds.TheLoai?.TenTheLoai,
                    TenNXB = ds.NhaXuatBan?.TenNxb,

                    TotalCount = c?.Total ?? 0,
                    AvailableCount = c?.Available ?? 0,

                    // nếu bạn muốn hiển thị tác giả trong view:
                    TacGiaText = tacGiaTextMap.ContainsKey(ds.DauSachId) ? tacGiaTextMap[ds.DauSachId] : "-"
                });
            }

            // Dropdown data
            ViewBag.TheLoais = await _db.TheLoais.OrderBy(x => x.TenTheLoai).ToListAsync();
            ViewBag.NhaXuatBans = await _db.NhaXuatBans.OrderBy(x => x.TenNxb).ToListAsync();
            ViewBag.TacGias = await _db.TacGia.OrderBy(x => x.TenTacGia).ToListAsync();

            // Current filters
            ViewBag.Q = q;
            ViewBag.TheLoaiId = theLoaiId;
            ViewBag.NhaXuatBanId = nhaXuatBanId;
            ViewBag.TacGiaId = tacGiaId;

            return View(items);
        }

        public async Task<IActionResult> Details(int id)
        {
            var ds = await _db.DauSaches
                .Include(x => x.TheLoai)
                .Include(x => x.NhaXuatBan)
                .FirstOrDefaultAsync(x => x.DauSachId == id);

            if (ds == null) return NotFound();

            // authors for details
            ViewBag.Authors = await _db.DauSachTacGia
                .Where(x => x.DauSachId == id)
                .Join(_db.TacGia, l => l.TacGiaId, tg => tg.TacGiaId, (l, tg) => tg.TenTacGia)
                .ToListAsync();

            return View(ds);
        }
    }
}
