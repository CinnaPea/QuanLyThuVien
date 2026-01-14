using WebApplication1.Models;
using WebApplication1.Services.Interfaces;

namespace WebApplication1.Services.Implementations
{
    public class ReaderService : IReaderService
    {
        private readonly QuanLyThuVienContext _context;

        public ReaderService(QuanLyThuVienContext context)
        {
            _context = context;
        }

        public IEnumerable<DocGium> GetAll()
            => _context.DocGia.ToList();

        public DocGium Get(int id)
            => _context.DocGia.Find(id);

        public void Create(DocGium docGia)
        {
            docGia.TrangThai = true;
            _context.DocGia.Add(docGia);
            _context.SaveChanges();
        }

        public void Update(DocGium docGia)
        {
            _context.DocGia.Update(docGia);
            _context.SaveChanges();
        }

        public void KhoaDocGia(int id)
        {
            var dg = _context.DocGia.Find(id);
            dg.TrangThai = false;
            _context.SaveChanges();
        }

        public bool CoTheMuon(int docGiaId)
        {
            var coPhatChuaTra =
                _context.Phats.Any(p =>
                    p.PhieuMuon.DocGiaId == docGiaId &&
                    p.SoTien >
                    _context.ThanhToanPhats
                        .Where(t => t.PhatId == p.PhatId)
                        .Sum(t => t.SoTien));

            return _context.DocGia.Any(d => d.DocGiaId == docGiaId && d.TrangThai)
                   && !coPhatChuaTra;
        }
    }

}
