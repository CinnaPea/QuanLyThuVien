using WebApplication1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class FineController : Controller
    {
        private readonly QuanLyThuVienContext _db;
        public FineController(QuanLyThuVienContext db) => _db = db;

        public async Task<IActionResult> MyFines()
        {
            var docGiumIdStr = User.FindFirstValue("DocGiumId");
            if (string.IsNullOrWhiteSpace(docGiumIdStr)) return Forbid();
            int docGiumId = int.Parse(docGiumIdStr);

            // lấy phạt theo phiếu mượn của độc giả
            var phieuIds = await _db.PhieuMuons.Where(p => p.DocGiaId == docGiumId).Select(p => p.PhieuMuonId).ToListAsync();

            var fines = await _db.Phats
                .Where(f => phieuIds.Contains(f.PhieuMuonId))
                .OrderByDescending(f => f.PhatId)
                .ToListAsync();

            return View(fines);
        }
    }
}
