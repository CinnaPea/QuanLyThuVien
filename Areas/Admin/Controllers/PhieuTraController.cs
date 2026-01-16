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

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> ReturnAll(
        //    int phieuMuonId,
        //    List<int> sachIds,
        //    List<string> tinhTrangLucTra,
        //    List<int>? returnIds)
        //{
        //    if (sachIds == null || sachIds.Count == 0)
        //    {
        //        TempData["err"] = "Không có bản sao để trả.";
        //        return RedirectToAction(nameof(ReturnForm), new { id = phieuMuonId });
        //    }

        //    if (tinhTrangLucTra == null || tinhTrangLucTra.Count != sachIds.Count)
        //    {
        //        TempData["err"] = "Dữ liệu tình trạng trả không hợp lệ.";
        //        return RedirectToAction(nameof(ReturnForm), new { id = phieuMuonId });
        //    }

        //    // If nothing checked => treat as return all
        //    var returnSet = (returnIds == null || returnIds.Count == 0)
        //        ? sachIds.Distinct().ToHashSet()
        //        : returnIds.Distinct().ToHashSet();

        //    await using var tx = await _db.Database.BeginTransactionAsync();

        //    var phieu = await _db.PhieuMuons.FirstOrDefaultAsync(x => x.PhieuMuonId == phieuMuonId);
        //    if (phieu == null) return NotFound();

        //    if (phieu.TrangThai != "DangMuon")
        //    {
        //        TempData["err"] = "Chỉ lập phiếu trả cho phiếu đang mượn.";
        //        return RedirectToAction(nameof(Index));
        //    }

        //    var ctMuon = await _db.CtPhieuMuons
        //        .Where(x => x.PhieuMuonId == phieuMuonId)
        //        .ToListAsync();

        //    var validIds = ctMuon.Select(x => x.SachId).ToHashSet();
        //    if (returnSet.Any(id => !validIds.Contains(id)))
        //    {
        //        TempData["err"] = "Có bản sao không thuộc phiếu mượn này.";
        //        return RedirectToAction(nameof(ReturnForm), new { id = phieuMuonId });
        //    }

        //    var saches = await _db.Saches.Where(x => returnSet.Contains(x.SachId)).ToListAsync();

        //    // Create Phiếu trả
        //    var phieuTra = new PhieuTra
        //    {
        //        PhieuMuonId = phieuMuonId,
        //        NgayTra = DateTime.Now,
        //        GhiChu = null
        //    };
        //    _db.PhieuTras.Add(phieuTra);
        //    await _db.SaveChangesAsync();

        //    var today = DateOnly.FromDateTime(DateTime.Now);
        //    var isLate = today > phieu.HanTra;
        //    var soNgayTre = isLate ? (today.DayNumber - phieu.HanTra.DayNumber) : 0;

        //    const decimal PHAT_TRE_MOI_NGAY = 5000m;
        //    const decimal PHAT_HONG = 20000m;   // chỉnh theo rule của bạn
        //    const decimal PHAT_MAT = 100000m;   // chỉnh theo rule của bạn

        //    // Map status by posted order (sachIds[i] -> tinhTrangLucTra[i])
        //    var statusMap = new Dictionary<int, string>();
        //    for (int i = 0; i < sachIds.Count; i++)
        //    {
        //        var sid = sachIds[i];
        //        var tt = (tinhTrangLucTra[i] ?? "Con").Trim();
        //        statusMap[sid] = tt;
        //    }

        //    // Update per returned item
        //    foreach (var sid in returnSet)
        //    {
        //        var ct = ctMuon.First(x => x.SachId == sid);

        //        // prevent double-return
        //        if (ct.NgayTraThucTe != null) continue;

        //        var ttTra = statusMap.TryGetValue(sid, out var tt) ? tt : "Con";
        //        if (ttTra != "Con" && ttTra != "Hong" && ttTra != "Mat")
        //            ttTra = "Con";

        //        ct.NgayTraThucTe = today;
        //        ct.TinhTrangLucTra = ttTra;

        //        _db.CtPhieuTras.Add(new CtPhieuTra
        //        {
        //            PhieuTraId = phieuTra.PhieuTraId,
        //            SachId = sid,
        //            TinhTrangLucTra = ttTra
        //        });

        //        var sach = saches.First(s => s.SachId == sid);
        //        sach.TinhTrang = ttTra; // Con/Hong/Mat

        //        // Fine per book (late + condition)
        //        decimal tienPhat = 0m;
        //        string lyDo = "";

        //        if (isLate)
        //        {
        //            tienPhat += soNgayTre * PHAT_TRE_MOI_NGAY;
        //            lyDo += $"Trễ hạn {soNgayTre} ngày; ";
        //        }

        //        if (ttTra == "Hong")
        //        {
        //            tienPhat += PHAT_HONG;
        //            lyDo += "Hỏng; ";
        //        }
        //        else if (ttTra == "Mat")
        //        {
        //            tienPhat += PHAT_MAT;
        //            lyDo += "Mất; ";
        //        }

        //        if (tienPhat > 0)
        //        {
        //            _db.Phats.Add(new Phat
        //            {
        //                PhieuMuonId = phieuMuonId,
        //                SachId = sid,
        //                LyDo = lyDo.Trim(),
        //                SoTien = tienPhat,
        //                DaThanhToan = false,
        //                NgayLap = DateTime.Now,
        //                GhiChu = null
        //            });
        //        }
        //    }

        //    // If all books in loan are returned => DaTra else keep DangMuon
        //    var allReturned = ctMuon.All(x => x.NgayTraThucTe != null);
        //    phieu.TrangThai = allReturned ? "DaTra" : "DangMuon";

        //    await _db.SaveChangesAsync();
        //    await tx.CommitAsync();

        //    TempData["ok"] = "Đã lập phiếu trả.";
        //    return RedirectToAction(nameof(Index));
        //}

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
