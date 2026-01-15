using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class PhieuMuonController : Controller
    {
        private readonly QuanLyThuVienContext _db;
        public PhieuMuonController(QuanLyThuVienContext db) => _db = db;

        // Danh sách
        public async Task<IActionResult> Index()
        {
            var list = await _db.PhieuMuons
                .Include(x => x.DocGia)
                .OrderByDescending(x => x.PhieuMuonId)
                .ToListAsync();

            return View(list);
        }

        public IActionResult Create()
        {
            ViewBag.DocGiaId = _db.DocGia.ToList();
            ViewBag.Sachs = _db.Saches
                .Where(x => x.TinhTrang == "Con")
                .ToList();

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int docGiaId, List<int> sachIds)
        {
            if (sachIds == null || sachIds.Count == 0)
            {
                TempData["err"] = "Chọn ít nhất 1 bản sao.";
                return RedirectToAction(nameof(Create));
            }

            sachIds = sachIds.Distinct().ToList();

            // Optional: check sách còn "Con" tại thời điểm request (không lock)
            var saches = await _db.Saches.Where(s => sachIds.Contains(s.SachId)).ToListAsync();
            if (saches.Count != sachIds.Count || saches.Any(s => s.TinhTrang != "Con")) // chuẩn hóa 1 trạng thái
            {
                TempData["err"] = "Có bản sao không khả dụng. Vui lòng chọn lại.";
                return RedirectToAction(nameof(Create));
            }

            await using var tx = await _db.Database.BeginTransactionAsync();

            var phieu = new PhieuMuon
            {
                DocGiaId = docGiaId,
                NgayMuon = DateTime.Now, 
                TrangThai = "ChoDuyet",
                HanTra = DateOnly.FromDateTime(DateTime.Today),                 
                GhiChu = null
            };

            _db.PhieuMuons.Add(phieu);
            await _db.SaveChangesAsync();

            foreach (var sid in sachIds)
            {
                _db.CtPhieuMuons.Add(new CtPhieuMuon
                {
                    PhieuMuonId = phieu.PhieuMuonId,
                    SachId = sid,
                    // chưa giao sách => chưa cần set tình trạng lúc mượn
                    TinhTrangLucMuon = null
                });
            }

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            TempData["ok"] = "Đã gửi yêu cầu mượn. Chờ admin duyệt.";
            return RedirectToAction(nameof(Details), new { id = phieu.PhieuMuonId });
        }


        public async Task<IActionResult> ChoDuyet()
        {
            var list = await _db.PhieuMuons
                .Include(x => x.DocGia)
                .Where(x => x.TrangThai == "ChoDuyet")
                .OrderBy(x => x.PhieuMuonId)
                .ToListAsync();
            return View(list);
        }

        public async Task<IActionResult> Details(int id)
        {
            var phieu = await _db.PhieuMuons
                .Include(x => x.DocGia)
                .FirstOrDefaultAsync(x => x.PhieuMuonId == id);
            if (phieu == null) return NotFound();

            var ct = await _db.CtPhieuMuons
                .Include(x => x.Sach).ThenInclude(s => s.DauSach)
                .Where(x => x.PhieuMuonId == id)
                .ToListAsync();

            ViewBag.CT = ct;
            return View(phieu);
        }
        [HttpPost]
        public async Task<IActionResult> Approve(int id, int soNgayMuon = 7)
        {
            await using var tx = await _db.Database.BeginTransactionAsync();

            var phieu = await _db.PhieuMuons
                .Include(p => p.CtPhieuMuons)
                .FirstOrDefaultAsync(p => p.PhieuMuonId == id);

            if (phieu == null) return NotFound();
            if (phieu.TrangThai != "ChoDuyet")
            {
                TempData["err"] = "Phiếu không ở trạng thái chờ duyệt.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var sachIds = phieu.CtPhieuMuons.Select(x => x.SachId).Distinct().ToList();
            var saches = await _db.Saches.Where(s => sachIds.Contains(s.SachId)).ToListAsync();

            if (saches.Any(s => s.TinhTrang != "Con"))
            {
                TempData["err"] = "Có bản sao không còn khả dụng. Không thể duyệt.";
                return RedirectToAction(nameof(Details), new { id });
            }

            // set hạn trả + trạng thái phiếu
            phieu.TrangThai = "DangMuon";
            phieu.HanTra = DateOnly.FromDateTime(DateTime.Now.AddDays(soNgayMuon));

            // giao sách: đổi trạng thái sách, và snapshot tình trạng lúc mượn
            foreach (var ct in phieu.CtPhieuMuons)
            {
                var sach = saches.First(s => s.SachId == ct.SachId);
                ct.TinhTrangLucMuon = sach.TinhTrang; // trước khi đổi (thường là "Con") :contentReference[oaicite:5]{index=5}
                sach.TinhTrang = "DangMuon";           // :contentReference[oaicite:6]{index=6}
            }

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            TempData["ok"] = "Đã duyệt và giao sách.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        public async Task<IActionResult> Reject(int id, string? ghiChu = null)
        {
            var phieu = await _db.PhieuMuons.FindAsync(id);
            if (phieu == null) return NotFound();

            if (phieu.TrangThai != "ChoDuyet")
            {
                TempData["err"] = "Phiếu không ở trạng thái chờ duyệt.";
                return RedirectToAction(nameof(Details), new { id });
            }

            phieu.TrangThai = "TuChoi";
            phieu.GhiChu = ghiChu;
            await _db.SaveChangesAsync();

            TempData["ok"] = "Đã từ chối yêu cầu.";
            return RedirectToAction(nameof(Details), new { id });
        }

    }
}
