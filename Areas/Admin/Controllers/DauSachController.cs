using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;
using WebApplication1.VMs;

namespace WebApplication1.Areas.Admin.Controllers
{
    [Area("Admin")]
 
    public class DauSachController : Controller
    {
        private readonly QuanLyThuVienContext _db;
        public DauSachController(QuanLyThuVienContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            var data = await _db.DauSaches
                .Select(d => new DauSachListItemVM
                {
                    DauSachId = d.DauSachId,
                    TieuDe = d.TieuDe,
                    ISBN = d.Isbn,
                    TenTheLoai = d.TheLoai != null ? d.TheLoai.TenTheLoai : null,
                    TenNXB = d.NhaXuatBan != null ? d.NhaXuatBan.TenNxb : null,
                    TacGiaText = null,

                    TotalCount = _db.Saches.Count(s => s.DauSachId == d.DauSachId),
                    AvailableCount = _db.Saches.Count(s => s.DauSachId == d.DauSachId)
                })
                .ToListAsync();

            var ids = data.Select(x => x.DauSachId).ToList();

            var authorRows = await _db.DauSachTacGia
                .Where(j => ids.Contains(j.DauSachId))
                .Select(j => new
                {
                    j.DauSachId,
                    j.TacGiaId,
                    TenTacGia = j.TacGia.TenTacGia
                })
                .ToListAsync();

            var authorMap = authorRows
                .GroupBy(r => r.DauSachId)
                .ToDictionary(
                    g => g.Key,
                    g => string.Join(", ", g.Select(r => $"{r.TenTacGia}"))
                );

            foreach (var item in data)
                item.TacGiaText = authorMap.TryGetValue(item.DauSachId, out var txt) ? txt : null;

            return View(data);
        }

        private void LoadDropDowns(DauSach? item = null)
        {
            ViewBag.TheLoaiId = new SelectList(
                _db.TheLoais.ToList(),
                "TheLoaiId",
                "TenTheLoai",
                item?.TheLoaiId
            );

            ViewBag.NhaXuatBanId = new SelectList(
                _db.NhaXuatBans.ToList(),
                "NhaXuatBanId",
                "TenNxb",          // ✅ FIX THIS (match your model property exactly)
                item?.NhaXuatBanId
            );
        }
        private void PopulateDropdowns(IEnumerable<int>? selectedTacGiaIds = null)
        {
            ViewBag.TheLoaiId = new SelectList(_db.TheLoais, "TheLoaiId", "TenTheLoai");
            ViewBag.NhaXuatBanId = new SelectList(_db.NhaXuatBans, "NhaXuatBanId", "TenNxb");

            // Multi-select list for authors
            ViewBag.TacGiaIds = new MultiSelectList(
                _db.TacGia,          // DbSet<TacGium>
                "TacGiaId",
                "TenTacGia",
                selectedTacGiaIds
            );
        }

        public IActionResult Create()
        {
            PopulateDropdowns();
            return View(new DauSachFormVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DauSachFormVM vm)
        {
            if (!ModelState.IsValid)
            {
                PopulateDropdowns(vm.TacGiaIds);
                return View(vm);
            }

            var entity = new DauSach
            {
                TieuDe = vm.TieuDe!,
                Isbn = vm.Isbn,
                NamXuatBan = vm.NamXuatBan,
                NgonNgu = vm.NgonNgu,
                MoTa = vm.MoTa,
                TheLoaiId = vm.TheLoaiId,
                NhaXuatBanId = vm.NhaXuatBanId
            };

            _db.DauSaches.Add(entity);
            await _db.SaveChangesAsync();

            // Insert join rows
            if (vm.TacGiaIds != null && vm.TacGiaIds.Count > 0)
            {
                foreach (var tgId in vm.TacGiaIds.Distinct())
                {
                    _db.DauSachTacGia.Add(new DauSachTacGium
                    {
                        DauSachId = entity.DauSachId,
                        TacGiaId = tgId
                    });
                }
                await _db.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var dauSach = await _db.DauSaches
                .Include(d => d.DauSachTacGia) // join rows
                .FirstOrDefaultAsync(d => d.DauSachId == id);

            if (dauSach == null) return NotFound();

            var vm = new DauSachFormVM
            {
                DauSachId = dauSach.DauSachId,
                TieuDe = dauSach.TieuDe,
                Isbn = dauSach.Isbn,
                NamXuatBan = dauSach.NamXuatBan,
                NgonNgu = dauSach.NgonNgu,
                MoTa = dauSach.MoTa,
                TheLoaiId = dauSach.TheLoaiId,
                NhaXuatBanId = dauSach.NhaXuatBanId,
                TacGiaIds = dauSach.DauSachTacGia.Select(x => x.TacGiaId).ToList()
            };

            PopulateDropdowns(vm.TacGiaIds);
            return View(vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DauSachFormVM vm)
        {
            if (id != vm.DauSachId) return BadRequest();

            if (!ModelState.IsValid)
            {
                PopulateDropdowns(vm.TacGiaIds);
                return View(vm);
            }

            var dauSach = await _db.DauSaches
                .Include(d => d.DauSachTacGia)
                .FirstOrDefaultAsync(d => d.DauSachId == id);

            if (dauSach == null) return NotFound();

            // Update scalar fields
            dauSach.TieuDe = vm.TieuDe;
            dauSach.Isbn = vm.Isbn;
            dauSach.NamXuatBan = vm.NamXuatBan;
            dauSach.NgonNgu = vm.NgonNgu;
            dauSach.MoTa = vm.MoTa;
            dauSach.TheLoaiId = vm.TheLoaiId;
            dauSach.NhaXuatBanId = vm.NhaXuatBanId;

            // Sync many-to-many authors (replace strategy)
            var newIds = (vm.TacGiaIds ?? new List<int>()).Distinct().ToHashSet();
            var oldIds = dauSach.DauSachTacGia.Select(x => x.TacGiaId).ToHashSet();

            // remove missing
            var toRemove = dauSach.DauSachTacGia.Where(x => !newIds.Contains(x.TacGiaId)).ToList();
            _db.DauSachTacGia.RemoveRange(toRemove);

            // add new
            var toAdd = newIds.Except(oldIds);
            foreach (var tgId in toAdd)
            {
                dauSach.DauSachTacGia.Add(new DauSachTacGium
                {
                    DauSachId = dauSach.DauSachId,
                    TacGiaId = tgId
                });
            }

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _db.DauSaches.FindAsync(id);
            if (item == null) return NotFound();
            _db.DauSaches.Remove(item);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
