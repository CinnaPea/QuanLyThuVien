using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Services.Interfaces;
using WebApplication1.VMs;

namespace WebApplication1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public AdminController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public IActionResult Index()
        {
            var vm = new DashboardVM
            {
                TongSach = _dashboardService.TongSoSach(),
                SachDangMuon = _dashboardService.SachDangMuon(),
                PhieuMuonQuaHan = _dashboardService.PhieuMuonQuaHan(),
                DocGiaDangHoatDong = _dashboardService.DocGiaDangHoatDong(),
                TongTienPhatChuaThanhToan = _dashboardService.TongTienPhatChuaThanhToan()
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home", new { area = "" });
        }
    }
}
