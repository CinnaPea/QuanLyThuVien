using WebApplication1.Models;

namespace WebApplication1.Services.Interfaces
{
    public interface IReaderService
    {
        IEnumerable<DocGium> GetAll();
        DocGium Get(int id);
        void Create(DocGium docGia);
        void Update(DocGium docGia);
        void KhoaDocGia(int id);
        bool CoTheMuon(int docGiaId);
    }

}
