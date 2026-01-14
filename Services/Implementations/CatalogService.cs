using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;
using WebApplication1.Services.Interfaces;

namespace WebApplication1.Services.Implementations
{
    public class CatalogService : ICatalogService
    {
        private readonly QuanLyThuVienContext _context;

        public CatalogService(QuanLyThuVienContext context)
        {
            _context = context;
        }

        public IEnumerable<DauSach> GetAllDauSach()
            => _context.DauSaches
                .Include(d => d.DauSachTacGia)
                    .ThenInclude(x => x.TacGia)
                .Include(d => d.TheLoai)
                .Include(d => d.NhaXuatBan)
                .ToList();

        public DauSach GetDauSach(int id)
            => _context.DauSaches
                .Include(d => d.DauSachTacGia)
                .FirstOrDefault(d => d.DauSachId == id);

        public void CreateDauSach(DauSach dauSach, IEnumerable<int> tacGiaIds)
        {
            _context.DauSaches.Add(dauSach);
            _context.SaveChanges();

            foreach (var tgId in tacGiaIds)
            {
                _context.DauSachTacGia.Add(new DauSachTacGium
                {
                    DauSachId = dauSach.DauSachId,
                    TacGiaId = tgId
                });
            }

            _context.SaveChanges();
        }

        public void UpdateDauSach(DauSach dauSach, IEnumerable<int> tacGiaIds)
        {
            _context.DauSaches.Update(dauSach);

            var oldLinks = _context.DauSachTacGia
                .Where(x => x.DauSachId == dauSach.DauSachId);

            _context.DauSachTacGia.RemoveRange(oldLinks);

            foreach (var tgId in tacGiaIds)
            {
                _context.DauSachTacGia.Add(new DauSachTacGium
                {
                    DauSachId = dauSach.DauSachId,
                    TacGiaId = tgId
                });
            }

            _context.SaveChanges();
        }

        public IEnumerable<Sach> GetSachByDauSach(int dauSachId)
            => _context.Saches.Where(s => s.DauSachId == dauSachId).ToList();

        public void CreateSach(Sach sach)
        {
            _context.Saches.Add(sach);
            _context.SaveChanges();
        }

        public void UpdateSach(Sach sach)
        {
            _context.Saches.Update(sach);
            _context.SaveChanges();
        }
        public void DeleteDauSach(Sach sach)
        {
            _context.Saches.Update(sach);
            _context.SaveChanges();
        }

        public IEnumerable<TacGium> GetTacGia() => _context.TacGia.ToList();
        public IEnumerable<TheLoai> GetTheLoai() => _context.TheLoais.ToList();
        public IEnumerable<NhaXuatBan> GetNhaXuatBan() => _context.NhaXuatBans.ToList();

        public void DeleteDauSach(int id)
        {
            throw new NotImplementedException();
        }
    }

}
