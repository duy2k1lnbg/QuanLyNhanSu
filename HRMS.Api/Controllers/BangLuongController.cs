using Bu.CLASS_CHAMCONG;
using Bu.DTO;
using DA;
using HRMS_API.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace HRMS_API.Controllers
{
    [JwtAuthorize]
    [RoutePrefix("api/bangluong")]
    public class BangLuongController : ApiController
    {
        private readonly BANGLUONG _bangLuongBus = new BANGLUONG();

        /// <summary>
        /// GET: api/bangluong?makycong={makycong}
        /// <summary>
        /// GET: api/bangluong?makycong={makycong}&dept={dept}&status={status}
        /// Lấy danh sách bảng lương chi tiết có hỗ trợ lọc theo phòng ban và trạng thái chi trả.
        /// </summary>
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetBangLuong(int makycong = 0, string dept = null, string status = null)
        {
            try
            {
                using (var db = new MyEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    db.Configuration.ProxyCreationEnabled = false;

                    if (makycong <= 0)
                    {
                        var latestKc = db.TB_KYCONG.OrderByDescending(x => x.MAKYCONG).FirstOrDefault();
                        if (latestKc != null)
                        {
                            makycong = (int)latestKc.MAKYCONG;
                        }
                    }

                    var currentKc = db.TB_KYCONG.FirstOrDefault(x => x.MAKYCONG == makycong);
                    bool isPeriodLocked = currentKc != null && (currentKc.KHOA == 1);
                    string defaultStatus = isPeriodLocked ? "Đã chi trả" : "Chờ chi trả";

                    var raw = (from bl in db.TB_BANGLUONG
                               where makycong <= 0 || bl.MAKYCONG == makycong
                               join nv in db.TB_NHANVIEN on bl.MANV equals nv.MANV into nvGroup
                               from nv in nvGroup.DefaultIfEmpty()
                               select new { bl, nv.HOTEN, nv.IDPB, nv.DATHOIVIEC }).ToList();

                    var pbMap = db.TB_PHONGBAN.ToDictionary(p => p.IDPB, p => p.TENPB);

                    var result = raw.Select(x => new BANGLUONG_DTO
                    {
                        IDBL = x.bl.IDBL,
                        MANV = x.bl.MANV,
                        HOTEN = x.HOTEN ?? "",
                        TENPB = (x.IDPB != null && pbMap.ContainsKey(x.IDPB.Value)) ? pbMap[x.IDPB.Value] : "",
                        IDPB = x.IDPB,
                        DATHOIVIEC = x.DATHOIVIEC,
                        KHOA = currentKc != null ? currentKc.KHOA : null,
                        TRANGTHAI_CHITRA = defaultStatus,
                        MAKYCONG = x.bl.MAKYCONG,
                        THANG = x.bl.THANG,
                        NAM = x.bl.NAM,
                        CONG_CHUAN = x.bl.CONG_CHUAN,
                        CONG_THUCTE = x.bl.CONG_THUCTE,
                        CONG_LAMNGAY = x.bl.CONG_LAMNGAY,
                        CONG_LAMDEM = x.bl.CONG_LAMDEM,
                        DAILY_RATE = x.bl.DAILY_RATE,
                        DAILY_ALLOWANCE = x.bl.DAILY_ALLOWANCE,
                        LUONG_CONG_THUCTE = x.bl.LUONG_CONG_THUCTE,
                        PHUCAP_CONG_THUCTE = x.bl.PHUCAP_CONG_THUCTE,
                        TIEN_TANGCA = x.bl.TIEN_TANGCA,
                        TIEN_CHUYENCAN = x.bl.TIEN_CHUYENCAN,
                        TIEN_AN_CA = x.bl.TIEN_AN_CA,
                        KHOAN_CONG_KHAC = x.bl.KHOAN_CONG_KHAC,
                        TONG_CONG = x.bl.TONG_CONG,
                        LUONG_BHXH = x.bl.LUONG_BHXH,
                        TIEN_BHXH = x.bl.TIEN_BHXH,
                        TIEN_BHYT = x.bl.TIEN_BHYT,
                        TIEN_BHTN = x.bl.TIEN_BHTN,
                        TIEN_BHXH_TRICH = x.bl.TIEN_BHXH_TRICH,
                        TIEN_CONG_DOAN = x.bl.TIEN_CONG_DOAN,
                        TIEN_TAMUNG = x.bl.TIEN_TAMUNG,
                        THUE_TNCN = x.bl.THUE_TNCN,
                        KHOAN_TRU_KHAC = x.bl.KHOAN_TRU_KHAC,
                        HOAN_THUE = x.bl.HOAN_THUE,
                        THUC_LINH = x.bl.THUC_LINH,
                        LUONG_CA_NGAY = (x.bl.DAILY_RATE != null && x.bl.CONG_LAMNGAY != null) ? (x.bl.DAILY_RATE * x.bl.CONG_LAMNGAY) : 0,
                        LUONG_CA_DEM = (x.bl.DAILY_RATE != null && x.bl.CONG_LAMDEM != null) ? (x.bl.DAILY_RATE * x.bl.CONG_LAMDEM * 1.30m) : 0
                    }).ToList();

                    // Modern 2026 Snapshot Enrichment
                    if (makycong >= 202601 && result.Count > 0)
                    {
                        try
                        {
                            var sql = @"SELECT IDBL, IS_LEGACY, TRANG_THAI, VUNG_LUONG, LUONG_TOI_THIEU_VUNG, MUC_THAM_CHIEU_BH,
                                               LUONG_DONG_BHXH, TIEN_BHXH_NSDLD, TIEN_BHYT_NSDLD, TIEN_BHTN_NSDLD, TIEN_TNLD_BNN_NSDLD,
                                               TIEN_DOAN_PHI_NLD, TIEN_KINH_PHI_CD_NSDLD, SO_NGUOI_PHU_THUOC, GIAM_TRU_BAN_THAN,
                                               GIAM_TRU_PHU_THUOC, GIAM_TRU_BAO_HIEM, TONG_THU_NHAP_CHIU_THUE, THU_NHAP_TINH_THUE,
                                               TONG_CHI_PHI_NSDLD
                                        FROM TB_BANGLUONG WHERE MAKYCONG = :p0";
                            var snaps = db.Database.SqlQuery<ModernPayrollSnapshotDto>(
                                sql,
                                new Oracle.ManagedDataAccess.Client.OracleParameter("p0", makycong)
                            ).ToDictionary(s => s.IDBL);

                            foreach (var item in result)
                            {
                                if (snaps.TryGetValue(item.IDBL, out var snap))
                                {
                                    item.IS_LEGACY = snap.IS_LEGACY;
                                    item.TRANG_THAI = snap.TRANG_THAI;
                                    item.VUNG_LUONG = snap.VUNG_LUONG;
                                    item.LUONG_TOI_THIEU_VUNG = snap.LUONG_TOI_THIEU_VUNG;
                                    item.MUC_THAM_CHIEU_BH = snap.MUC_THAM_CHIEU_BH;
                                    item.LUONG_DONG_BHXH = snap.LUONG_DONG_BHXH;
                                    item.TIEN_BHXH_NSDLD = snap.TIEN_BHXH_NSDLD;
                                    item.TIEN_BHYT_NSDLD = snap.TIEN_BHYT_NSDLD;
                                    item.TIEN_BHTN_NSDLD = snap.TIEN_BHTN_NSDLD;
                                    item.TIEN_TNLD_BNN_NSDLD = snap.TIEN_TNLD_BNN_NSDLD;
                                    item.TIEN_DOAN_PHI_NLD = snap.TIEN_DOAN_PHI_NLD;
                                    item.TIEN_KINH_PHI_CD_NSDLD = snap.TIEN_KINH_PHI_CD_NSDLD;
                                    item.SO_NGUOI_PHU_THUOC = snap.SO_NGUOI_PHU_THUOC;
                                    item.GIAM_TRU_BAN_THAN = snap.GIAM_TRU_BAN_THAN;
                                    item.GIAM_TRU_PHU_THUOC = snap.GIAM_TRU_PHU_THUOC;
                                    item.GIAM_TRU_BAO_HIEM = snap.GIAM_TRU_BAO_HIEM;
                                    item.TONG_THU_NHAP_CHIU_THUE = snap.TONG_THU_NHAP_CHIU_THUE;
                                    item.THU_NHAP_TINH_THUE = snap.THU_NHAP_TINH_THUE;
                                    item.TONG_CHI_PHI_NSDLD = snap.TONG_CHI_PHI_NSDLD;

                                    if (!string.IsNullOrEmpty(snap.TRANG_THAI))
                                    {
                                        if (snap.TRANG_THAI == "APPROVED" || snap.TRANG_THAI == "LEGACY_READONLY")
                                            item.TRANGTHAI_CHITRA = "Đã chi trả";
                                        else if (snap.TRANG_THAI == "DRAFT")
                                            item.TRANGTHAI_CHITRA = "Chờ chi trả";
                                    }
                                }
                                else
                                {
                                    item.IS_LEGACY = 1;
                                }
                            }
                        }
                        catch { }
                    }

                    // Lọc theo phòng ban nếu có
                    if (!string.IsNullOrWhiteSpace(dept) && dept != "all")
                    {
                        dept = System.Web.HttpUtility.UrlDecode(dept).Trim();
                        if (int.TryParse(dept, out int idpb))
                        {
                            result = result.Where(x => x.IDPB == idpb).ToList();
                        }
                        else
                        {
                            result = result.Where(x => !string.IsNullOrEmpty(x.TENPB) && (x.TENPB.Equals(dept, StringComparison.OrdinalIgnoreCase) || x.TENPB.IndexOf(dept, StringComparison.OrdinalIgnoreCase) >= 0)).ToList();
                        }
                    }

                    // Lọc theo trạng thái chi trả nếu có
                    if (!string.IsNullOrWhiteSpace(status) && status != "all")
                    {
                        if (status == "paid" || status == "da_chi_tra")
                        {
                            result = result.Where(x => x.TRANGTHAI_CHITRA == "Đã chi trả" || x.TRANG_THAI == "APPROVED" || x.TRANG_THAI == "PAID").ToList();
                        }
                        else if (status == "pending" || status == "cho_chi_tra")
                        {
                            result = result.Where(x => (x.TRANGTHAI_CHITRA == "Chờ chi trả" || x.TRANG_THAI == "DRAFT") && x.TRANG_THAI != "APPROVED").ToList();
                        }
                    }

                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi khi tải bảng lương: " + ex.ToString());
                return Content(System.Net.HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi tải bảng lương." });
            }
        }

        /// <summary>
        /// GET: api/bangluong/chitiet?makycong={makycong}&manv={manv}
        /// Chi tiết bảng lương của 1 nhân viên đầy đủ và chính xác theo CSDL Oracle XE
        /// </summary>
        [HttpGet]
        [Route("chitiet")]
        public IHttpActionResult GetChiTietLuongNV(int makycong, int manv)
        {
            try
            {
                using (var db = new MyEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    db.Configuration.ProxyCreationEnabled = false;

                    var bl = db.TB_BANGLUONG.FirstOrDefault(x => x.MAKYCONG == makycong && x.MANV == manv);
                    if (bl == null)
                    {
                        return NotFound();
                    }

                    var nv = db.TB_NHANVIEN.FirstOrDefault(x => x.MANV == manv);
                    string hoten = nv != null ? nv.HOTEN : "";
                    string tenpb = "";
                    string tencv = "";
                    if (nv != null && nv.IDPB != null)
                    {
                        var pb = db.TB_PHONGBAN.FirstOrDefault(x => x.IDPB == nv.IDPB);
                        if (pb != null) tenpb = pb.TENPB;
                    }
                    if (nv != null && nv.IDCV != null)
                    {
                        var cv = db.TB_CHUCVU.FirstOrDefault(x => x.IDCV == nv.IDCV);
                        if (cv != null) tencv = cv.TENCV;
                    }

                    int nam = (int)bl.NAM;
                    int thang = (int)bl.THANG;

                    // Lấy số giờ tăng ca
                    decimal otHours = 0;
                    var lstTangCa = db.TB_TANGCA.Where(x => x.MANV == manv && x.THANG == thang && x.NAM == nam).ToList();
                    if (lstTangCa.Count > 0)
                    {
                        otHours = (decimal)lstTangCa.Sum(x => x.SOGIO ?? 0);
                    }

                    // Phụ cấp chi tiết theo 13 loại chuẩn TB_PHUCAP
                    var phucaps = db.TB_NHANVIEN_PHUCAP.Where(x => x.MANV == manv).ToList();
                    var allPC = db.TB_PHUCAP.OrderBy(p => p.IDPC).ToList();
                    var allowancesList = allPC.Select(pc =>
                    {
                        var nvPc = phucaps.FirstOrDefault(x => x.IDPC == pc.IDPC);
                        return new
                        {
                            IDPC = pc.IDPC,
                            TENPC = pc.TENPC,
                            SOTIEN = nvPc?.SOTIEN ?? 0
                        };
                    }).ToList();

                    decimal pcNhaO = phucaps.FirstOrDefault(x => x.IDPC == 1)?.SOTIEN ?? 0;
                    decimal pcDiLai = phucaps.FirstOrDefault(x => x.IDPC == 2)?.SOTIEN ?? 0;
                    decimal pcGiaDinh = phucaps.FirstOrDefault(x => x.IDPC == 3)?.SOTIEN ?? 0;
                    decimal pcNguoiPhuThuoc = phucaps.FirstOrDefault(x => x.IDPC == 4)?.SOTIEN ?? 0;
                    decimal pcChucVu = phucaps.FirstOrDefault(x => x.IDPC == 5)?.SOTIEN ?? 0;
                    decimal pcChungChi = phucaps.FirstOrDefault(x => x.IDPC == 6)?.SOTIEN ?? 0;
                    decimal pcKyNang = phucaps.FirstOrDefault(x => x.IDPC == 7)?.SOTIEN ?? 0;
                    decimal pcKhuVuc = phucaps.FirstOrDefault(x => x.IDPC == 8)?.SOTIEN ?? 0;
                    decimal pcChuyenCan = phucaps.FirstOrDefault(x => x.IDPC == 9)?.SOTIEN ?? 0;
                    decimal pcThamNien = phucaps.FirstOrDefault(x => x.IDPC == 10)?.SOTIEN ?? 0;
                    decimal pcLamViecTaiNha = phucaps.FirstOrDefault(x => x.IDPC == 11)?.SOTIEN ?? 0;
                    decimal pcDacBiet = phucaps.FirstOrDefault(x => x.IDPC == 12)?.SOTIEN ?? 0;
                    decimal pcKhac = phucaps.FirstOrDefault(x => x.IDPC == 13)?.SOTIEN ?? 0;

                    decimal sumAllowances = phucaps.Sum(x => x.SOTIEN ?? 0);

                    decimal dailyRate = bl.DAILY_RATE ?? 0;
                    decimal congChuan = bl.CONG_CHUAN ?? 26;
                    decimal luongCoBan = dailyRate * congChuan;
                    decimal tongCoBanTroCap = luongCoBan + sumAllowances;

                    decimal luongCongThucTe = bl.LUONG_CONG_THUCTE ?? 0;
                    decimal phuCapCongThucTe = bl.PHUCAP_CONG_THUCTE ?? 0;
                    decimal tienChuyenCan = bl.TIEN_CHUYENCAN ?? 0;
                    decimal tienAnCa = bl.TIEN_AN_CA ?? 0;
                    decimal khoanCongKhac = bl.KHOAN_CONG_KHAC ?? 0;
                    decimal tongNgayCongThucTe = luongCongThucTe + phuCapCongThucTe + tienChuyenCan + tienAnCa + khoanCongKhac;

                    decimal tienTangCa = bl.TIEN_TANGCA ?? 0;
                    decimal totalGross = bl.TONG_CONG ?? (tongNgayCongThucTe + tienTangCa);

                    // Bảo hiểm chính xác từ CSDL
                    decimal insuranceBase = bl.LUONG_BHXH ?? (bl.TIEN_BHXH_TRICH != null && bl.TIEN_BHXH_TRICH > 0 ? Math.Round(bl.TIEN_BHXH_TRICH.Value / 0.105m, 0) : 0);
                    decimal bhxh = bl.TIEN_BHXH ?? Math.Round(insuranceBase * 0.08m, 0);
                    decimal bhyt = bl.TIEN_BHYT ?? Math.Round(insuranceBase * 0.015m, 0);
                    decimal bhtn = bl.TIEN_BHTN ?? Math.Round(insuranceBase * 0.01m, 0);
                    decimal tienBhxhTrich = bl.TIEN_BHXH_TRICH ?? (bhxh + bhyt + bhtn);

                    decimal phiCongDoan = bl.TIEN_CONG_DOAN ?? 0;
                    decimal thueTNCN = bl.THUE_TNCN ?? 0;
                    decimal tienTamUng = bl.TIEN_TAMUNG ?? 0;
                    decimal khoanTruKhac = bl.KHOAN_TRU_KHAC ?? 0;
                    decimal tongKhauTru = tienBhxhTrich + phiCongDoan + thueTNCN + tienTamUng + khoanTruKhac;
                    decimal thucLinh = bl.THUC_LINH ?? (totalGross - tongKhauTru);

                    // Query modern trace details if available
                    object insuranceTrace = null;
                    object unionTrace = null;
                    object otCompliance = null;
                    object modernSnapshot = null;
                    var taxTraces = new List<object>();
                    var itemizedDetails = new List<object>();

                    try
                    {
                        var idblParam = new Oracle.ManagedDataAccess.Client.OracleParameter("p0", bl.IDBL);
                        var snapSql = @"SELECT IS_LEGACY, TRANG_THAI, VUNG_LUONG, LUONG_TOI_THIEU_VUNG, MUC_THAM_CHIEU_BH,
                                               LUONG_DONG_BHXH, TIEN_BHXH_NSDLD, TIEN_BHYT_NSDLD, TIEN_BHTN_NSDLD, TIEN_TNLD_BNN_NSDLD,
                                               TIEN_DOAN_PHI_NLD, TIEN_KINH_PHI_CD_NSDLD, SO_NGUOI_PHU_THUOC, GIAM_TRU_BAN_THAN,
                                               GIAM_TRU_PHU_THUOC, GIAM_TRU_BAO_HIEM, TONG_THU_NHAP_CHIU_THUE, THU_NHAP_TINH_THUE,
                                               TONG_CHI_PHI_NSDLD
                                        FROM TB_BANGLUONG WHERE IDBL = :p0";
                        var snapRow = db.Database.SqlQuery<ModernPayrollSnapshotDto>(snapSql, idblParam).FirstOrDefault();
                        if (snapRow != null)
                        {
                            modernSnapshot = snapRow;
                            if (insuranceBase == 0 && snapRow.LUONG_DONG_BHXH.HasValue) insuranceBase = snapRow.LUONG_DONG_BHXH.Value;
                            if (phiCongDoan == 0 && snapRow.TIEN_DOAN_PHI_NLD.HasValue) phiCongDoan = snapRow.TIEN_DOAN_PHI_NLD.Value;
                        }

                        // Insurance Trace
                        var bhTraceRows = db.Database.SqlQuery<dynamic>(
                            "SELECT MUC_THAM_CHIEU, VUNG_LUONG, LUONG_TOI_THIEU_VUNG, LUONG_DONG_BHXH_AP_DUNG, TIEN_BHXH_NLD, TIEN_BHYT_NLD, TIEN_BHTN_NLD, TONG_BH_NLD, TIEN_BHXH_NSDLD, TIEN_BHYT_NSDLD, TIEN_BHTN_NSDLD, TIEN_TNLD_BNN_NSDLD, TONG_BH_NSDLD FROM TB_BANGLUONG_BAOHIEM WHERE IDBL = :p0",
                            new Oracle.ManagedDataAccess.Client.OracleParameter("p0", bl.IDBL)
                        ).ToList();
                        if (bhTraceRows.Count > 0) insuranceTrace = bhTraceRows[0];

                        // Union Trace
                        var cdTraceRows = db.Database.SqlQuery<dynamic>(
                            "SELECT LA_DOAN_VIEN, LUONG_CAN_CU_DONG, TY_LE_DOAN_PHI_NLD, MUC_TRAN_DOAN_PHI, TIEN_DOAN_PHI_NLD, TY_LE_KPCD_NSDLD, TIEN_KPCD_NSDLD FROM TB_BANGLUONG_CONG_DOAN WHERE IDBL = :p0",
                            new Oracle.ManagedDataAccess.Client.OracleParameter("p0", bl.IDBL)
                        ).ToList();
                        if (cdTraceRows.Count > 0) unionTrace = cdTraceRows[0];

                        // OT Compliance
                        var otTraceRows = db.Database.SqlQuery<dynamic>(
                            "SELECT ACTUAL_HOURS_MONTH, ACTUAL_HOURS_YTD, LEGAL_HOURS_MONTH, EXCESS_HOURS_MONTH, COMPLIANCE_STATUS, TOTAL_OT_PAYMENT, LEGAL_ALLOWED_PAYMENT, EXEMPT_OT_PAYMENT, TAXABLE_EXCESS_PAYMENT FROM TB_BANGLUONG_OT_COMPLIANCE WHERE IDBL = :p0",
                            new Oracle.ManagedDataAccess.Client.OracleParameter("p0", bl.IDBL)
                        ).ToList();
                        if (otTraceRows.Count > 0) otCompliance = otTraceRows[0];

                        // Tax Brackets Trace
                        var taxRows = db.Database.SqlQuery<dynamic>(
                            "SELECT BAC_THUE, CAN_DUOI, CAN_TREN, THU_NHAP_CHIU_THUE_BAC, THUE_SUAT, TIEN_THUE_BAC FROM TB_BANGLUONG_THUE_CT WHERE IDBL = :p0 ORDER BY BAC_THUE",
                            new Oracle.ManagedDataAccess.Client.OracleParameter("p0", bl.IDBL)
                        ).ToList();
                        taxTraces.AddRange(taxRows);

                        // Itemized Details
                        var ctRows = db.Database.SqlQuery<dynamic>(
                            "SELECT NHOM_KHOAN_MUC, MA_KHOAN_MUC, TEN_KHOAN_MUC, SO_LUONG, DON_GIA, HE_SO, THANH_TIEN, TINH_VAO_DONG_BHXH, TINH_THUE_TNCN, SO_TIEN_MIEN_THUE, SO_TIEN_CHIU_THUE, CONG_THUC_DIEN_GIAI FROM TB_BANGLUONG_CT WHERE IDBL = :p0 ORDER BY IDBLCT",
                            new Oracle.ManagedDataAccess.Client.OracleParameter("p0", bl.IDBL)
                        ).ToList();
                        itemizedDetails.AddRange(ctRows);
                    }
                    catch { }

                    var result = new
                    {
                        IDBL = bl.IDBL,
                        MANV = bl.MANV,
                        HOTEN = hoten,
                        TENPB = tenpb,
                        TENCV = tencv,
                        MAKYCONG = bl.MAKYCONG,
                        THANG = thang,
                        NAM = nam,
                        CONG_CHUAN = congChuan,
                        CONG_THUCTE = bl.CONG_THUCTE ?? 0,
                        CONG_LAMNGAY = bl.CONG_LAMNGAY ?? 0,
                        CONG_LAMDEM = bl.CONG_LAMDEM ?? 0,
                        DAILY_RATE = dailyRate,
                        DAILY_ALLOWANCE = bl.DAILY_ALLOWANCE ?? 0,
                        LUONG_CO_BAN = luongCoBan,

                        // Phụ cấp chi tiết
                        PC_TRACH_NHIEM = pcChucVu,
                        PC_CHUYEN_CAN = pcChuyenCan,
                        PC_NHA_O = pcNhaO,
                        PC_NGON_NGU = pcChungChi,
                        PC_THAM_NIEN = pcThamNien,
                        PC_DI_LAI = pcDiLai,
                        PC_KHAC = pcKhac,
                        PC_GIA_DINH = pcGiaDinh,
                        PC_NGUOI_PHU_THUOC = pcNguoiPhuThuoc,
                        PC_KY_NANG = pcKyNang,
                        PC_KHU_VUC = pcKhuVuc,
                        PC_LAM_VIEC_TAI_NHA = pcLamViecTaiNha,
                        PC_DAC_BIET = pcDacBiet,
                        SUM_ALLOWANCES = sumAllowances,
                        TONG_CO_BAN_TRO_CAP = tongCoBanTroCap,
                        ALLOWANCES_LIST = allowancesList,

                        // Chi tiết ngày công thực tế
                        LUONG_CONG_THUCTE = luongCongThucTe,
                        PHUCAP_CONG_THUCTE = phuCapCongThucTe,
                        TIEN_CHUYENCAN = tienChuyenCan,
                        TIEN_AN_CA = tienAnCa,
                        KHOAN_CONG_KHAC = khoanCongKhac,
                        TONG_NGAY_CONG_THUCTE = tongNgayCongThucTe,

                        // Tăng ca
                        OT_HOURS = otHours,
                        TIEN_TANGCA = tienTangCa,
                        TOTAL_GROSS = totalGross,

                        // Bảo hiểm & Khấu trừ
                        INSURANCE_BASE = insuranceBase,
                        BHXH_8 = bhxh,
                        BHYT_15 = bhyt,
                        BHTN_1 = bhtn,
                        TIEN_BHXH_TRICH = tienBhxhTrich,
                        PHI_CONG_DOAN = phiCongDoan,
                        THUE_TNCN = thueTNCN,
                        KHOAN_TRU_KHAC = khoanTruKhac,
                        TIEN_TAMUNG = tienTamUng,
                        TONG_KHAU_TRU = tongKhauTru,

                        // Thực lĩnh
                        THUC_LINH = thucLinh,

                        // Modern 2026 Details & Compliance Traces
                        MODERN_SNAPSHOT = modernSnapshot,
                        INSURANCE_TRACE = insuranceTrace,
                        UNION_TRACE = unionTrace,
                        OT_COMPLIANCE = otCompliance,
                        TAX_TRACES = taxTraces,
                        ITEMIZED_DETAILS = itemizedDetails
                    };

                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi khi tải chi tiết bảng lương nhân viên: " + ex.ToString());
                return Content(System.Net.HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi tải chi tiết bảng lương." });
            }
        }

        /// <summary>
        /// GET: api/bangluong/kycong
        /// Danh sách các kỳ công tính lương
        /// </summary>
        [HttpGet]
        [Route("kycong")]
        public IHttpActionResult GetKyCong()
        {
            try
            {
                using (var db = new MyEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    db.Configuration.ProxyCreationEnabled = false;

                    var list = db.TB_KYCONG
                        .OrderByDescending(x => x.MAKYCONG)
                        .Select(x => new
                        {
                            MAKYCONG = (int)x.MAKYCONG,
                            THANG = (int?)x.THANG,
                            NAM = (int?)x.NAM,
                            KHOA = (int?)x.KHOA,
                            NGAYCONGTRONGTHANG = (int?)x.NGAYCONGTRONGTHANG,
                            TRANGTHAI = (int?)x.TRANGTHAI,
                            NGAYTINHCONG = x.NGAYTINHCONG
                        })
                        .ToList();

                    return Ok(list);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi khi tải danh sách kỳ công: " + ex.ToString());
                return Content(System.Net.HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi tải danh sách kỳ công." });
            }
        }

        /// <summary>
        /// POST: api/bangluong/tinhluong
        /// Kích hoạt tính lương cho kỳ công (Yêu cầu quyền F_BANGLUONG_CALC)
        /// </summary>
        [HttpPost]
        [Route("tinhluong")]
        [JwtAuthorize(Right = "F_BANGLUONG_CALC")]
        public IHttpActionResult TinhLuong([FromBody] TinhLuongParam param)
        {
            try
            {
                if (param == null || param.Makycong <= 0)
                {
                    return BadRequest("Vui lòng cung cấp mã kỳ công hợp lệ.");
                }

                var jwtUser = JwtAuthorizeAttribute.GetCurrentJwtUser(Request);
                int currentUserId = (jwtUser != null && int.TryParse(jwtUser.UserId, out int uid)) ? uid : 1;

                _bangLuongBus.TinhLuongKyCong(param.Makycong, currentUserId);
                return Ok(new
                {
                    success = true,
                    message = $"Đã tính toán bảng lương thành công cho kỳ công {param.Makycong}."
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi khi tính lương kỳ công: " + ex.ToString());
                return Content(System.Net.HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi trong quá trình tính toán lương." });
            }
        }

        /// <summary>
        /// POST: api/bangluong/khoa
        /// Khóa hoặc mở khóa kỳ lương (chi trả / chờ chi trả)
        /// </summary>
        [HttpPost]
        [Route("khoa")]
        public IHttpActionResult ToggleKhoa([FromBody] KhoaKyCongParam param)
        {
            try
            {
                if (param == null || param.Makycong <= 0)
                {
                    return BadRequest("Vui lòng cung cấp mã kỳ công hợp lệ.");
                }

                using (var db = new MyEntities())
                {
                    var kc = db.TB_KYCONG.FirstOrDefault(x => x.MAKYCONG == param.Makycong);
                    if (kc == null)
                    {
                        return NotFound();
                    }

                    var jwtUser = JwtAuthorizeAttribute.GetCurrentJwtUser(Request);
                    int currentUserId = (jwtUser != null && int.TryParse(jwtUser.UserId, out int uid)) ? uid : 1;

                    kc.KHOA = param.Khoa ? 1 : 0;
                    kc.UPDATED_BY = currentUserId;
                    kc.UPDATED_DATE = DateTime.Now;

                    db.SaveChanges();

                    return Ok(new
                    {
                        success = true,
                        makycong = kc.MAKYCONG,
                        khoa = kc.KHOA == 1,
                        trangthai = kc.KHOA == 1 ? "Đã chi trả" : "Chờ chi trả",
                        message = param.Khoa ? $"Đã khóa sổ và xác nhận chi trả cho kỳ công {param.Makycong}." : $"Đã mở khóa kỳ công {param.Makycong} về trạng thái chờ chi trả."
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi khi khóa/mở khóa kỳ công: " + ex.ToString());
                return Content(System.Net.HttpStatusCode.InternalServerError, new { success = false, message = "Lỗi khi cập nhật trạng thái kỳ công: " + ex.Message });
            }
        }
    }

    public class TinhLuongParam
    {
        public int Makycong { get; set; }
        public int? IdUser { get; set; }
    }

    public class KhoaKyCongParam
    {
        public int Makycong { get; set; }
        public bool Khoa { get; set; }
    }
}
