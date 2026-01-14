using WebApplication1.Models;

namespace WebApplication1.Services.Interfaces
{
    public interface IFineService
    {
        Phat LapPhatNeuQuaHan(int phieuMuonId);
        void ThanhToanPhat(int phatId, decimal soTien);
    }

}
