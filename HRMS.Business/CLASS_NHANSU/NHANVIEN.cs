using Bu.DTO;
using DA;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bu
{
    public class NHANVIEN
    {
        public static Func<string, string> TranslateDelegate { get; set; }
        MyEntities db = new MyEntities();

        public TB_NHANVIEN getItem(int id)
        {
            return db.TB_NHANVIEN.FirstOrDefault(x => x.MANV == id);
        }
        public List<TB_NHANVIEN> getList()
        {
            return db.TB_NHANVIEN.ToList();
        }
        public List<NHANVIEN_DTO> getListFll_DTO(string langCode = "vi") 
        { 
            var rawList = (from nv in db.TB_NHANVIEN
                    where nv.MANV != 3207
                    join td in db.TB_TRINHDO on nv.IDTD equals td.IDTD into tdGroup
                    from td in tdGroup.DefaultIfEmpty()
                    join bp in db.TB_BOPHAN on nv.IDBP equals bp.IDBP into bpGroup
                    from bp in bpGroup.DefaultIfEmpty()
                    join pb in db.TB_PHONGBAN on nv.IDPB equals pb.IDPB into pbGroup
                    from pb in pbGroup.DefaultIfEmpty()
                    join dt in db.TB_DANTOC on nv.IDDT equals dt.IDDT into dtGroup
                    from dt in dtGroup.DefaultIfEmpty()
                    join cty in db.TB_CONGTY on nv.IDCTY equals cty.IDCTY into ctyGroup
                    from cty in ctyGroup.DefaultIfEmpty()
                    join cv in db.TB_CHUCVU on nv.IDCV equals cv.IDCV into cvGroup
                    from cv in cvGroup.DefaultIfEmpty()
                    join tg in db.TB_TONGIAO on nv.IDTG equals tg.IDTG into tgGroup
                    from tg in tgGroup.DefaultIfEmpty()
                    join gt in db.TB_GIOITINH on nv.IDGT equals gt.IDGT into gtGroup
                    from gt in gtGroup.DefaultIfEmpty()
                    join qt in db.TB_QUOCTICH on nv.IDQT equals qt.IDQT into qtGroup
                    from qt in qtGroup.DefaultIfEmpty()
                    select new NHANVIEN_DTO
                    {
                        MANV = nv.MANV,
                        HOTEN = nv.HOTEN,
                        IDGT = nv.IDGT,
                        CCCD = nv.CCCD,
                        NGAYSINH = nv.NGAYSINH,
                        DIENTHOAI = nv.DIENTHOAI,
                        DIACHI = nv.DIACHI,
                        HINHANH = nv.HINHANH,
                        DATHOIVIEC = nv.DATHOIVIEC,
                        IDTD = nv.IDTD,
                        TENTD = td != null ? td.TENTD : null,
                        IDBP = nv.IDBP,
                        TENBP = bp != null ? bp.TENBP : null,
                        IDPB = nv.IDPB,
                        TENPB = pb != null ? pb.TENPB : null,
                        IDDT = nv.IDDT,
                        TENDT = dt != null ? dt.TENDT : null,
                        IDCTY = nv.IDCTY,
                        TENCTY = cty != null ? cty.TENCTY : null,
                        IDCV = nv.IDCV,
                        TENCV = cv != null ? cv.TENCV : null,
                        IDTG = nv.IDTG,
                        TENTG = tg != null ? tg.TENTG : null,
                        TENGT = gt != null ? gt.TENGT : null,
                        IDQT = nv.IDQT,
                        TENQT = qt != null ? qt.TENQT : null,
                        CREATED_BY = nv.CREATED_BY,
                        CREATED_DATE = nv.CREATED_DATE,
                        UPDATED_BY = nv.UPDATED_BY,
                        UPDATED_DATE = nv.UPDATED_DATE,
                        DELETED_BY = nv.DELETED_BY,
                        DELETED_DATE = nv.DELETED_DATE,
                        LOAI_NV = nv.LOAI_NV
                    }).ToList();

            foreach (var item in rawList)
            {
                if (string.IsNullOrWhiteSpace(item.TENGT))
                {
                    item.TENGT = item.IDGT == 2 ? "Nữ" : (item.IDGT == 3 ? "Khác" : "Nam");
                }
            }

            if (string.IsNullOrEmpty(langCode) || langCode.ToLower() == "vi")
            {
                return rawList;
            }

            try
            {
                var translations = db.Database.SqlQuery<TB_TRANSLATION_RECORD>(@"
                    SELECT table_name, record_id, column_name, value 
                    FROM TB_TRANSLATIONS 
                    WHERE LOWER(language_code) = :p0", langCode.ToLower()).ToList();

                var transDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (var t in translations)
                {
                    string key = $"{t.TABLE_NAME}_{t.RECORD_ID}_{t.COLUMN_NAME}";
                    if (!transDict.ContainsKey(key))
                    {
                        transDict[key] = t.VALUE;
                    }
                }

                foreach (var item in rawList)
                {
                    if (item.IDPB.HasValue)
                    {
                        string k = $"TB_PHONGBAN_{item.IDPB.Value}_TENPB";
                        if (transDict.TryGetValue(k, out string v)) item.TENPB = v;
                        else if (TranslateDelegate != null) item.TENPB = TranslateDelegate(item.TENPB);
                    }
                    if (item.IDCV.HasValue)
                    {
                        string k = $"TB_CHUCVU_{item.IDCV.Value}_TENCV";
                        if (transDict.TryGetValue(k, out string v)) item.TENCV = v;
                        else if (TranslateDelegate != null) item.TENCV = TranslateDelegate(item.TENCV);
                    }
                    if (item.IDDT.HasValue)
                    {
                        string k = $"TB_DANTOC_{item.IDDT.Value}_TENDT";
                        if (transDict.TryGetValue(k, out string v)) item.TENDT = v;
                        else if (TranslateDelegate != null) item.TENDT = TranslateDelegate(item.TENDT);
                    }
                    if (item.IDTG.HasValue)
                    {
                        string k = $"TB_TONGIAO_{item.IDTG.Value}_TENTG";
                        if (transDict.TryGetValue(k, out string v)) item.TENTG = v;
                        else if (TranslateDelegate != null) item.TENTG = TranslateDelegate(item.TENTG);
                    }
                    if (item.IDTD.HasValue)
                    {
                        string k = $"TB_TRINHDO_{item.IDTD.Value}_TENTD";
                        if (transDict.TryGetValue(k, out string v)) item.TENTD = v;
                        else if (TranslateDelegate != null) item.TENTD = TranslateDelegate(item.TENTD);
                    }
                    if (item.IDBP.HasValue)
                    {
                        string k = $"TB_BOPHAN_{item.IDBP.Value}_TENBP";
                        if (transDict.TryGetValue(k, out string v)) item.TENBP = v;
                        else if (TranslateDelegate != null) item.TENBP = TranslateDelegate(item.TENBP);
                    }
                    if (item.IDQT.HasValue)
                    {
                        string k = $"TB_QUOCTICH_{item.IDQT.Value}_TENQT";
                        if (transDict.TryGetValue(k, out string v)) item.TENQT = v;
                        else if (TranslateDelegate != null) item.TENQT = TranslateDelegate(item.TENQT);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[NHANVIEN DTO Translation Error]: " + ex.Message);
            }

            return rawList;
        }

        private class TB_TRANSLATION_RECORD
        {
            public string TABLE_NAME { get; set; }
            public string RECORD_ID { get; set; }
            public string COLUMN_NAME { get; set; }
            public string VALUE { get; set; }
        }

        public TB_NHANVIEN Add(TB_NHANVIEN nv)
        {
            try
            {
                db.TB_NHANVIEN.Add(nv);
                db.SaveChanges();
                try
                {
                    Bu.Services.AI_Services.Vector.AiDataSyncHub.NotifyEmployeeChanged((int)nv.MANV);
                }
                catch { }
                return nv;
            }
            catch (DbEntityValidationException ex)
            {
                var errorMessages = ex.EntityValidationErrors
                    .SelectMany(x => x.ValidationErrors)
                    .Select(x => x.ErrorMessage);
                var fullErrorMessage = string.Join("; ", errorMessages);
                var exceptionMessage = string.Concat(ex.Message, " The validation errors are: ", fullErrorMessage);
                throw new Exception("Lỗi: " + exceptionMessage);
            }
            catch (Exception ex)
            {
                var sb = new StringBuilder();
                var cur = ex;
                while (cur != null)
                {
                    sb.Append(" --> " + cur.Message);
                    cur = cur.InnerException;
                }
                throw new Exception("Lỗi: " + sb.ToString());
            }
        }

        public TB_NHANVIEN Update(TB_NHANVIEN nv)
        {
            try
            {
                var _nv = db.TB_NHANVIEN.FirstOrDefault(x => x.MANV == nv.MANV);
                _nv.HOTEN = nv.HOTEN;
                _nv.IDGT = nv.IDGT;
                _nv.NGAYSINH = nv.NGAYSINH;
                _nv.DIENTHOAI = nv.DIENTHOAI;
                _nv.CCCD = nv.CCCD;
                _nv.DIACHI = nv.DIACHI;
                if (nv.HINHANH != null)
                {
                    _nv.HINHANH = nv.HINHANH.Length == 0 ? null : nv.HINHANH;
                }
                _nv.DATHOIVIEC = nv.DATHOIVIEC;
                _nv.IDPB = nv.IDPB;
                _nv.IDBP = nv.IDBP;
                _nv.IDCV = nv.IDCV;
                _nv.IDTD = nv.IDTD;
                _nv.IDDT = nv.IDDT;
                _nv.IDTG = nv.IDTG;
                _nv.IDCTY = nv.IDCTY;
                _nv.IDQT = nv.IDQT;
                _nv.UPDATED_BY = nv.UPDATED_BY;
                _nv.UPDATED_DATE = nv.UPDATED_DATE;
                _nv.LOAI_NV = nv.LOAI_NV;
                db.SaveChanges();
                try
                {
                    Bu.Services.AI_Services.Vector.AiDataSyncHub.NotifyEmployeeChanged((int)nv.MANV);
                }
                catch { }
                return nv;
            }
            catch (Exception ex)
            {

                throw new Exception("Lỗi" + ex.Message);
            }
        }

        public void Delete(int id, int iduser)
        {
            try
            {
                var _nv = db.TB_NHANVIEN.FirstOrDefault(x => x.MANV == id);
                if (_nv != null)
                {
                    // Chuyển trạng thái sang đã thôi việc thay vì xóa cứng khỏi CSDL
                    _nv.DATHOIVIEC = 1;
                    _nv.DELETED_BY = iduser;
                    _nv.DELETED_DATE = DateTime.Now;
                    _nv.UPDATED_BY = iduser;
                    _nv.UPDATED_DATE = DateTime.Now;
                    db.SaveChanges();
                    try
                    {
                        Bu.Services.AI_Services.Vector.AiDataSyncHub.NotifyEmployeeChanged(id);
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi: " + ex.Message);
            }
        }

        public void Restore(int id, int iduser)
        {
            try
            {
                var _nv = db.TB_NHANVIEN.FirstOrDefault(x => x.MANV == id);
                if (_nv != null)
                {
                    // Khôi phục trạng thái đi làm lại
                    _nv.DATHOIVIEC = 0;
                    _nv.DELETED_BY = null;
                    _nv.DELETED_DATE = null;
                    _nv.UPDATED_BY = iduser;
                    _nv.UPDATED_DATE = DateTime.Now;
                    db.SaveChanges();
                    try
                    {
                        Bu.Services.AI_Services.Vector.AiDataSyncHub.NotifyEmployeeChanged(id);
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi: " + ex.Message);
            }
        }

        public List<TB_NHANVIEN> getSinhNhat()
        {
            return db.TB_NHANVIEN.Where(x => x.NGAYSINH.Value.Month == DateTime.Now.Month).ToList();
        }

        public int GetTongNhanVien()
        {
            return db.TB_NHANVIEN.Count(x => x.MANV != 3207);
        }

        public List<DashboardPhongBanDTO> GetPhongBanStats()
        {
            var rawList = db.TB_NHANVIEN
                            .Where(nv => nv.MANV != 3207)
                            .Select(nv => new {
                                TenPB = nv.TB_PHONGBAN.TENPB
                            })
                            .ToList();

            return rawList
                    .GroupBy(x => x.TenPB ?? "Chưa xếp")
                    .Select(g => new DashboardPhongBanDTO
                    {
                        PhongBan = g.Key,
                        SoLuong = g.Count()
                    })
                    .ToList();
        }

        public List<DashboardLuongDTO> GetLuongStats()
        {
            var luongRaw = db.TB_BANGLUONG
                             .Select(bl => new { bl.MAKYCONG, bl.THUC_LINH })
                             .ToList()
                             .GroupBy(bl => bl.MAKYCONG)
                             .Select(g => new
                             {
                                 KyCong = g.Key.ToString(),
                                 TongLuong = (decimal)g.Sum(x => x.THUC_LINH ?? 0)
                             })
                             .OrderByDescending(x => x.KyCong)
                             .Take(6)
                             .ToList();

            return luongRaw.Select(x => new DashboardLuongDTO
            {
                KyCong = x.KyCong.Length >= 6 ? "T" + x.KyCong.Substring(4, 2) + "/" + x.KyCong.Substring(0, 4) : x.KyCong,
                TongLuong = x.TongLuong
            }).OrderBy(x => x.KyCong).ToList();
        }

        /// <summary>
        /// Kiểm tra tất cả nhân viên đối chiếu với kỳ công (tháng/năm).
        /// Nếu hợp đồng lao động đã hết hạn trước đầu kỳ công (NGAYKETTHUC < periodStart),
        /// tự động cập nhật DATHOIVIEC = 1 trong CSDL.
        /// Trả về danh sách nhân viên có hợp đồng còn thời hạn và chưa thôi việc.
        /// </summary>
        public List<TB_NHANVIEN> KiemTraVaCapNhatTrangThaiHopDong(int nam, int thang, int? macty = null)
        {
            DateTime periodStart = new DateTime(nam, thang, 1);
            DateTime periodEnd = new DateTime(nam, thang, DateTime.DaysInMonth(nam, thang));

            // Lấy danh sách hợp đồng mới nhất cho từng nhân viên
            var allHopDong = db.TB_HOPDONG
                .OrderByDescending(x => x.NGAYBATDAU)
                .ThenByDescending(x => x.SOHD)
                .ToList()
                .GroupBy(x => x.MANV)
                .ToDictionary(g => g.Key, g => g.FirstOrDefault());

            var allNhanVien = db.TB_NHANVIEN.ToList();
            bool hasChanges = false;
            List<TB_NHANVIEN> danhSachHopLe = new List<TB_NHANVIEN>();

            foreach (var nv in allNhanVien)
            {
                if (nv.MANV == 3207) continue; // Bỏ qua tài khoản test
                if (macty.HasValue && macty.Value > 0 && nv.IDCTY != macty.Value) continue;

                allHopDong.TryGetValue(nv.MANV, out var latestHd);

                // Trường hợp 1: Không có hợp đồng nào
                if (latestHd == null)
                {
                    continue;
                }

                // Trường hợp 2: Hợp đồng đã hết hạn trước thời điểm bắt đầu kỳ công
                if (latestHd.NGAYKETTHUC.HasValue && latestHd.NGAYKETTHUC.Value < periodStart)
                {
                    if (nv.DATHOIVIEC != 1)
                    {
                        nv.DATHOIVIEC = 1;
                        nv.UPDATED_DATE = DateTime.Now;
                        nv.DELETED_DATE = latestHd.NGAYKETTHUC.Value;
                        hasChanges = true;
                    }
                    continue; // Không đưa vào danh sách tính công/lương
                }

                // Trường hợp 3: Đã thôi việc
                if (nv.DATHOIVIEC == 1)
                {
                    continue;
                }

                // Trường hợp 4: Hợp đồng hợp lệ trong kỳ (Bắt đầu trước hoặc trong kỳ và chưa kết thúc trước kỳ)
                if (latestHd.NGAYBATDAU.HasValue && latestHd.NGAYBATDAU.Value <= periodEnd)
                {
                    danhSachHopLe.Add(nv);
                }
            }

            if (hasChanges)
            {
                db.SaveChanges();
            }

            return danhSachHopLe;
        }

        public List<TB_NHANVIEN> GetListNhanVienConHopDong(int nam, int thang, int? macty = null)
        {
            return KiemTraVaCapNhatTrangThaiHopDong(nam, thang, macty);
        }
    }
}
