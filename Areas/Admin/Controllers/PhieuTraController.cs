using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;
using WebApplication1.VMs;

namespace WebApplication1.Areas.Admin.Controllers
{
    [Area("Admin")]
   
    public class PhieuTraController : Controller
    {
        private readonly QuanLyThuVienContext _db;
        public PhieuTraController(QuanLyThuVienContext db) => _db = db;

        // danh sách phiếu mượn đang mượn để trả
        public async Task<IActionResult> Index()
        {
            var vm = new PhieuTraIndexVM
            {
                // Phiếu còn đang mượn
                DangMuon = await _db.PhieuMuons
                    .Include(x => x.DocGia)
                    .Where(x => x.TrangThai == "DangMuon")
                    .OrderBy(x => x.PhieuMuonId)
                    .ToListAsync(),

                // ===== LỊCH SỬ PHIẾU TRẢ =====
                LichSuTra = await _db.PhieuTras
                    .Include(x => x.PhieuMuon)
                        .ThenInclude(pm => pm.DocGia)
                    .OrderByDescending(x => x.NgayTra)
                    .ToListAsync()
            };

            return View(vm);
        }

        public async Task<IActionResult> Details(int id)
        {
            var phieuTra = await _db.PhieuTras
                .Include(x => x.PhieuMuon)
                    .ThenInclude(pm => pm.DocGia)
                .FirstOrDefaultAsync(x => x.PhieuTraId == id);

            if (phieuTra == null)
                return NotFound();

            var ct = await _db.CtPhieuTras
                .Include(x => x.Sach)
                    .ThenInclude(s => s.DauSach)
                .Where(x => x.PhieuTraId == id)
                .ToListAsync();

            ViewBag.CT = ct;
            return View(phieuTra);
        }


        public async Task<IActionResult> ReturnForm(int id) // id = PhieuMuonId
        {
            var phieu = await _db.PhieuMuons
                .Include(x => x.DocGia)
                .FirstOrDefaultAsync(x => x.PhieuMuonId == id);
            if (phieu == null) return NotFound();

            var ct = await _db.CtPhieuMuons
    .Include(x => x.Sach)
        .ThenInclude(s => s.DauSach)
    .Where(x => x.PhieuMuonId == id && x.NgayTraThucTe == null)
    .ToListAsync();

            ViewBag.CT = ct;
            return View(phieu);
        }

        [HttpPost]
        public async Task<IActionResult> ReturnAll(int phieuMuonId)
        {
            var phieu = await _db.PhieuMuons.FindAsync(phieuMuonId);
            if (phieu == null) return NotFound();
            var ctMuon = await _db.CtPhieuMuons
                .Where(x => x.PhieuMuonId == phieuMuonId && x.NgayTraThucTe == null)
                .ToListAsync();

            var sachIds = ctMuon.Select(x => x.SachId).ToList();
            var saches = await _db.Saches.Where(x => sachIds.Contains(x.SachId)).ToListAsync();

            // tạo Phiếu trả
            var phieuTra = new PhieuTra
            {
                PhieuMuonId = phieuMuonId,
                NgayTra = DateTime.Now
            };
            _db.PhieuTras.Add(phieuTra);
            await _db.SaveChangesAsync();

            // tạo CT_PhieuTra + cập nhật CT_PhieuMuon
            foreach (var ct in ctMuon)
            {
                ct.NgayTraThucTe = DateOnly.FromDateTime(DateTime.Now);

                ct.TinhTrangLucTra = "Con";

                _db.CtPhieuTras.Add(new CtPhieuTra
                {
                    PhieuTraId = phieuTra.PhieuTraId,
                    SachId = ct.SachId,
                    TinhTrangLucTra = "Con"
                });
            }

            // trả sách => còn
            foreach (var s in saches)
                s.TinhTrang = "Con";

            // update trạng thái phiếu mượn
            phieu.TrangThai = "DaTra";

            await _db.SaveChangesAsync();

            // phạt trễ hạn (nếu có HanTra)
            var today = DateOnly.FromDateTime(DateTime.Now);

            if (today > phieu.HanTra)
            {
                int soNgayTre = today.DayNumber - phieu.HanTra.DayNumber;

                decimal tienPhat = soNgayTre * 5000m;

                foreach (var sid in sachIds)
                {
                    _db.Phats.Add(new Phat
                    {
                        PhieuMuonId = phieuMuonId,
                        SachId = sid,
                        LyDo = $"Trễ hạn {soNgayTre} ngày",
                        SoTien = tienPhat,
                        DaThanhToan = false,
                        NgayLap = DateTime.Now,
                        GhiChu = null
                    });
                }
            await _db.SaveChangesAsync();
            }

            TempData["ok"] = "Đã lập phiếu trả.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ReturnSelected(int phieuMuonId, int[] sachIds)
        {
            var phieu = await _db.PhieuMuons.FindAsync(phieuMuonId);
            if (phieu == null) return NotFound();

            var phieuTra = new PhieuTra
            {
                PhieuMuonId = phieuMuonId,
                NgayTra = DateTime.Now
            };
            _db.PhieuTras.Add(phieuTra);
            await _db.SaveChangesAsync();

            var today = DateOnly.FromDateTime(DateTime.Now);

            foreach (var sachId in sachIds)
            {
                var ctMuon = await _db.CtPhieuMuons.FirstAsync(x =>
                    x.PhieuMuonId == phieuMuonId &&
                    x.SachId == sachId &&
                    x.NgayTraThucTe == null);

                var tinhTrang = Request.Form[$"tinhTrang_{sachId}"].ToString();

                ctMuon.NgayTraThucTe = today;
                ctMuon.TinhTrangLucTra = tinhTrang;

                _db.CtPhieuTras.Add(new CtPhieuTra
                {
                    PhieuTraId = phieuTra.PhieuTraId,
                    SachId = sachId,
                    TinhTrangLucTra = tinhTrang
                });

                var sach = await _db.Saches.FindAsync(sachId);

                if (tinhTrang == "Con")
                    sach.TinhTrang = "Con";
                else if (tinhTrang == "HuHong")
                {
                    sach.TinhTrang = "HuHong";
                    _db.Phats.Add(new Phat
                    {
                        PhieuMuonId = phieuMuonId,
                        SachId = sachId,
                        LyDo = "Hư hỏng sách",
                        SoTien = 80000,
                        DaThanhToan = false,
                        NgayLap = DateTime.Now
                    });
                }
                else if (tinhTrang == "Mat")
                {
                    sach.TinhTrang = "Mat";
                    _db.Phats.Add(new Phat
                    {
                        PhieuMuonId = phieuMuonId,
                        SachId = sachId,
                        LyDo = "Mất sách",
                        SoTien = 100000,
                        DaThanhToan = false,
                        NgayLap = DateTime.Now
                    });
                }

                if (today > phieu.HanTra)
                {
                    int soNgayTre = today.DayNumber - phieu.HanTra.DayNumber;
                    _db.Phats.Add(new Phat
                    {
                        PhieuMuonId = phieuMuonId,
                        SachId = sachId,
                        LyDo = $"Trễ hạn {soNgayTre} ngày",
                        SoTien = soNgayTre * 5000,
                        DaThanhToan = false,
                        NgayLap = DateTime.Now
                    });
                }
            }

            // 🔴 SAVE TRƯỚC
            await _db.SaveChangesAsync();

            // 🔴 KIỂM TRA SAU
            bool conChuaXuLy = await _db.CtPhieuMuons.AnyAsync(x =>
                x.PhieuMuonId == phieuMuonId &&
                x.NgayTraThucTe == null
            );

            if (!conChuaXuLy)
            {
                phieu.TrangThai = "DaTra";
                await _db.SaveChangesAsync();
            }

            TempData["ok"] = "Đã lập phiếu trả.";
            return RedirectToAction(nameof(Index));
        }






    }
}
