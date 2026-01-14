namespace WebApplication1.Services.Interfaces
{
    public interface IBorrowService
    {
        int TaoPhieuMuon(int docGiaId, IEnumerable<int> sachIds, DateOnly hanTra);
        void TraSach(int phieuMuonId, IEnumerable<int> sachIds);
        bool SachDangDuocMuon(int sachId);
    }

}
