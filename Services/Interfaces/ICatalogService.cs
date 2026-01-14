using WebApplication1.Models;

namespace WebApplication1.Services.Interfaces
{
    public interface ICatalogService
    {
        // DauSach
        IEnumerable<DauSach> GetAllDauSach();
        DauSach GetDauSach(int id);
        void CreateDauSach(DauSach dauSach, IEnumerable<int> tacGiaIds);
        void UpdateDauSach(DauSach dauSach, IEnumerable<int> tacGiaIds);
        void DeleteDauSach(int id);

        // Sach
        IEnumerable<Sach> GetSachByDauSach(int dauSachId);
        void CreateSach(Sach sach);
        void UpdateSach(Sach sach);

        // Master data
        IEnumerable<TacGium> GetTacGia();
        IEnumerable<TheLoai> GetTheLoai();
        IEnumerable<NhaXuatBan> GetNhaXuatBan();
    }
}
