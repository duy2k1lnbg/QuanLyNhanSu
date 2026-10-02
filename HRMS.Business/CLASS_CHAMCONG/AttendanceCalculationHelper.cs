using DA;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Bu.CLASS_CHAMCONG
{
    public class ShiftFrameInfo
    {
        public int Stt { get; set; }
        public int BatDauPhut { get; set; }
        public int KetThucPhut { get; set; }
        public string LoaiKhungGio { get; set; } // LAM_VIEC, NGHI_GIUA_CA, NGHI_KHONG_LUONG
        public bool BatBuocQuetThe { get; set; }

        public TimeSpan BatDauTimeSpan => TimeSpan.FromMinutes(BatDauPhut % 1440);
        public TimeSpan KetThucTimeSpan => TimeSpan.FromMinutes(KetThucPhut % 1440);
    }

    public class ShiftInfo
    {
        public long IdCaPhienBan { get; set; }
        public decimal IdLoaiCa { get; set; }
        public string TenPhienBan { get; set; }
        public string TenLoaiCa { get; set; }
        public long TongGiayChuan { get; set; }
        public decimal CongQuyDoi { get; set; } = 1.0m;
        public string TrangThai { get; set; } = "ACTIVE";
        public List<ShiftFrameInfo> Frames { get; set; } = new List<ShiftFrameInfo>();

        public bool IsNightShift =>
            IdLoaiCa == 2 ||
            (TenPhienBan != null && (TenPhienBan.IndexOf("đêm", StringComparison.OrdinalIgnoreCase) >= 0 || TenPhienBan.IndexOf("dem", StringComparison.OrdinalIgnoreCase) >= 0)) ||
            (Frames.Any(f => f.BatDauPhut > f.KetThucPhut || f.BatDauPhut >= 1260 || f.KetThucPhut <= 420));

        public List<ShiftFrameInfo> WorkFrames => Frames.Where(f => f.LoaiKhungGio == "LAM_VIEC").OrderBy(f => f.Stt).ToList();
        public List<ShiftFrameInfo> BreakFrames => Frames.Where(f => f.LoaiKhungGio != "LAM_VIEC").OrderBy(f => f.Stt).ToList();

        public string DisplayDescription
        {
            get
            {
                if (Frames.Count == 0)
                {
                    return IsNightShift ? "22:00 - 06:00 (Ca đêm 8h / 1.0 công)" : "08:00 - 12:00 | Nghỉ 12:00 - 13:00 | 13:00 - 17:00 (8h / 1.0 công)";
                }

                var parts = new List<string>();
                foreach (var f in Frames.OrderBy(x => x.Stt))
                {
                    string start = string.Format("{0:D2}:{1:D2}", (f.BatDauPhut / 60) % 24, f.BatDauPhut % 60);
                    string end = string.Format("{0:D2}:{1:D2}", (f.KetThucPhut / 60) % 24, f.KetThucPhut % 60);
                    if (f.LoaiKhungGio == "LAM_VIEC")
                    {
                        parts.Add($"{start} - {end}");
                    }
                    else
                    {
                        parts.Add($"Nghỉ {start} - {end}");
                    }
                }
                double hours = Math.Round(TongGiayChuan / 3600.0, 1);
                return $"{string.Join(" | ", parts)} ({hours}h / {CongQuyDoi:N1} công)";
            }
        }
    }

    public class AttendancePreviewResult
    {
        public decimal NgayCong { get; set; }
        public decimal NgayPhep { get; set; }
        public string KyHieu { get; set; }
        public string TrangThaiHienThi { get; set; }
        public int GioLamHopLePhut { get; set; }
        public string GioLamHopLeText { get; set; }
        public int DiMuonPhut { get; set; }
        public int VeSomPhut { get; set; }
        public string DiMuonVeSomText { get; set; }
        public bool HasWarning { get; set; }
        public string WarningMessage { get; set; }
        public bool DuDieuKienChot { get; set; }
        public bool HasConflict { get; set; }
        public string ConflictMessage { get; set; }
    }

    public static class AttendanceCalculationHelper
    {
        /// <summary>
        /// Trích xuất ConnectionString từ DbContext giữ nguyên mật khẩu cho kết nối trực tiếp ODP.NET Oracle
        /// </summary>
        public static string GetStoreConnectionString(MyEntities db = null)
        {
            bool disposeDb = false;
            if (db == null)
            {
                db = new MyEntities();
                disposeDb = true;
            }
            try
            {
                var objContext = ((System.Data.Entity.Infrastructure.IObjectContextAdapter)db).ObjectContext;
                var entityConn = objContext.Connection as System.Data.Entity.Core.EntityClient.EntityConnection;
                if (entityConn != null && entityConn.StoreConnection != null)
                {
                    return entityConn.StoreConnection.ConnectionString;
                }
                return db.Database.Connection.ConnectionString;
            }
            finally
            {
                if (disposeDb) db.Dispose();
            }
        }

        /// <summary>
        /// Tải danh sách tất cả các phiên bản ca đang hoạt động
        /// </summary>
        public static List<ShiftInfo> LoadAllShifts()
        {
            var result = new List<ShiftInfo>();
            try
            {
                string connStr = GetStoreConnectionString();
                using (var conn = new Oracle.ManagedDataAccess.Client.OracleConnection(connStr))
                {
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"
                            SELECT pb.IDCAPHIENBAN, pb.IDLOAICA, pb.TEN_PHIENBAN, 
                                   NVL(ca.TENLOAICA, ''), pb.TONG_GIAY_CHUAN, pb.CONG_QUY_DOI, pb.TRANG_THAI
                            FROM HR.TB_CA_PHIENBAN pb
                            LEFT JOIN HR.TB_LOAICA ca ON pb.IDLOAICA = ca.IDLOAICA
                            WHERE pb.TRANG_THAI = 'ACTIVE' OR pb.TRANG_THAI IS NULL
                            ORDER BY pb.IDLOAICA, pb.IDCAPHIENBAN";
                        using (var r = cmd.ExecuteReader())
                        {
                            while (r.Read())
                            {
                                result.Add(new ShiftInfo
                                {
                                    IdCaPhienBan = Convert.ToInt64(r[0]),
                                    IdLoaiCa = Convert.ToInt32(r[1]),
                                    TenPhienBan = r[2]?.ToString() ?? "",
                                    TenLoaiCa = r[3]?.ToString() ?? "",
                                    TongGiayChuan = r[4] != DBNull.Value ? Convert.ToInt32(r[4]) : 28800,
                                    CongQuyDoi = r[5] != DBNull.Value ? Convert.ToDecimal(r[5]) : 1.0m,
                                    TrangThai = r[6]?.ToString() ?? "ACTIVE",
                                    Frames = new List<ShiftFrameInfo>()
                                });
                            }
                        }

                        if (result.Count > 0)
                        {
                            cmd.CommandText = @"
                                SELECT IDCAPHIENBAN, STT, BATDAU_PHUT, KETTHUC_PHUT, LOAI_KHUNGGIO, BAT_BUOC_QUET_THE
                                FROM HR.TB_CA_KHUNGGIO
                                ORDER BY IDCAPHIENBAN, STT";
                            using (var r = cmd.ExecuteReader())
                            {
                                while (r.Read())
                                {
                                    long idCa = Convert.ToInt64(r[0]);
                                    var shift = result.FirstOrDefault(s => s.IdCaPhienBan == idCa);
                                    if (shift != null)
                                    {
                                        shift.Frames.Add(new ShiftFrameInfo
                                        {
                                            Stt = Convert.ToInt32(r[1]),
                                            BatDauPhut = Convert.ToInt32(r[2]),
                                            KetThucPhut = Convert.ToInt32(r[3]),
                                            LoaiKhungGio = r[4]?.ToString(),
                                            BatBuocQuetThe = r[5] != DBNull.Value && Convert.ToInt32(r[5]) == 1
                                        });
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                // Fallback nếu có lỗi kết nối
            }

            if (result.Count == 0)
            {
                result.AddRange(GetFallbackShifts());
            }

            return result;
        }

        public static List<ShiftInfo> GetFallbackShifts()
        {
            return new List<ShiftInfo>
            {
                new ShiftInfo
                {
                    IdCaPhienBan = 1,
                    IdLoaiCa = 1,
                    TenPhienBan = "Ca ngày (Hành chính)",
                    TenLoaiCa = "Ca ngày",
                    TongGiayChuan = 28800,
                    CongQuyDoi = 1.0m,
                    Frames = new List<ShiftFrameInfo>
                    {
                        new ShiftFrameInfo { Stt = 1, BatDauPhut = 480, KetThucPhut = 720, LoaiKhungGio = "LAM_VIEC", BatBuocQuetThe = true },
                        new ShiftFrameInfo { Stt = 2, BatDauPhut = 720, KetThucPhut = 780, LoaiKhungGio = "NGHI_GIUA_CA", BatBuocQuetThe = false },
                        new ShiftFrameInfo { Stt = 3, BatDauPhut = 780, KetThucPhut = 1020, LoaiKhungGio = "LAM_VIEC", BatBuocQuetThe = true }
                    }
                },
                new ShiftInfo
                {
                    IdCaPhienBan = 2,
                    IdLoaiCa = 1,
                    TenPhienBan = "Ca chiều",
                    TenLoaiCa = "Ca ngày",
                    TongGiayChuan = 28800,
                    CongQuyDoi = 1.0m,
                    Frames = new List<ShiftFrameInfo>
                    {
                        new ShiftFrameInfo { Stt = 1, BatDauPhut = 840, KetThucPhut = 1080, LoaiKhungGio = "LAM_VIEC", BatBuocQuetThe = true },
                        new ShiftFrameInfo { Stt = 2, BatDauPhut = 1080, KetThucPhut = 1140, LoaiKhungGio = "NGHI_GIUA_CA", BatBuocQuetThe = false },
                        new ShiftFrameInfo { Stt = 3, BatDauPhut = 1140, KetThucPhut = 1380, LoaiKhungGio = "LAM_VIEC", BatBuocQuetThe = true }
                    }
                },
                new ShiftInfo
                {
                    IdCaPhienBan = 3,
                    IdLoaiCa = 2,
                    TenPhienBan = "Ca đêm (22:00 - 06:00)",
                    TenLoaiCa = "Ca đêm",
                    TongGiayChuan = 28800,
                    CongQuyDoi = 1.0m,
                    Frames = new List<ShiftFrameInfo>
                    {
                        new ShiftFrameInfo { Stt = 1, BatDauPhut = 1320, KetThucPhut = 1560, LoaiKhungGio = "LAM_VIEC", BatBuocQuetThe = true }, // 22:00 - 02:00
                        new ShiftFrameInfo { Stt = 2, BatDauPhut = 1560, KetThucPhut = 1620, LoaiKhungGio = "NGHI_GIUA_CA", BatBuocQuetThe = false }, // 02:00 - 03:00
                        new ShiftFrameInfo { Stt = 3, BatDauPhut = 1620, KetThucPhut = 1800, LoaiKhungGio = "LAM_VIEC", BatBuocQuetThe = true }  // 03:00 - 06:00
                    }
                },
                new ShiftInfo
                {
                    IdCaPhienBan = 5,
                    IdLoaiCa = 1,
                    TenPhienBan = "Ca sáng nửa ngày",
                    TenLoaiCa = "Ca ngày",
                    TongGiayChuan = 14400,
                    CongQuyDoi = 0.5m,
                    Frames = new List<ShiftFrameInfo>
                    {
                        new ShiftFrameInfo { Stt = 1, BatDauPhut = 480, KetThucPhut = 720, LoaiKhungGio = "LAM_VIEC", BatBuocQuetThe = true }
                    }
                }
            };
        }

        /// <summary>
        /// Lấy ca được phân công cho nhân viên theo ngày từ TB_LICH_LAMVIEC
        /// </summary>
        public static ShiftInfo GetShiftForEmployee(List<ShiftInfo> allShifts, int manv, DateTime date)
        {
            if (allShifts == null || allShifts.Count == 0)
            {
                allShifts = LoadAllShifts();
            }

            try
            {
                string connStr = GetStoreConnectionString();
                using (var conn = new Oracle.ManagedDataAccess.Client.OracleConnection(connStr))
                {
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"
                            SELECT IDCAPHIENBAN 
                            FROM HR.TB_LICH_LAMVIEC 
                            WHERE MANV = :p_manv AND TRUNC(NGAY) = :p_ngay 
                            ORDER BY IDLICH DESC";
                        cmd.Parameters.Add("p_manv", (decimal)manv);
                        cmd.Parameters.Add("p_ngay", date.Date);

                        var val = cmd.ExecuteScalar();
                        if (val != null && val != DBNull.Value)
                        {
                            long caId = Convert.ToInt64(val);
                            var found = allShifts.FirstOrDefault(s => s.IdCaPhienBan == caId);
                            if (found != null) return found;
                        }
                    }
                }
            }
            catch { }

            return allShifts.FirstOrDefault(s => s.IdCaPhienBan == 1) ?? allShifts.FirstOrDefault() ?? GetFallbackShifts()[0];
        }

        /// <summary>
        /// Xác định khoảng giờ mặc định theo ca và phần ca cần làm
        /// </summary>
        public static (TimeSpan? GioVao, TimeSpan? GioRa) GetDefaultShiftHours(ShiftInfo shift, bool isDiLam, string phanNghi)
        {
            if (!isDiLam && phanNghi == "NN")
            {
                return (null, null); // Nghỉ nguyên ngày: không ghi nhận giờ làm
            }

            var workFrames = shift.WorkFrames;
            if (workFrames.Count == 0)
            {
                if (shift.IsNightShift)
                {
                    if (!isDiLam && phanNghi == "S") return (new TimeSpan(3, 0, 0), new TimeSpan(6, 0, 0));
                    if (!isDiLam && phanNghi == "C") return (new TimeSpan(22, 0, 0), new TimeSpan(2, 0, 0));
                    return (new TimeSpan(22, 0, 0), new TimeSpan(6, 0, 0));
                }
                else
                {
                    if (!isDiLam && phanNghi == "S") return (new TimeSpan(13, 0, 0), new TimeSpan(17, 0, 0));
                    if (!isDiLam && phanNghi == "C") return (new TimeSpan(8, 0, 0), new TimeSpan(12, 0, 0));
                    return (new TimeSpan(8, 0, 0), new TimeSpan(17, 0, 0));
                }
            }

            if (isDiLam || string.IsNullOrEmpty(phanNghi))
            {
                // Đi làm: toàn ca
                var first = workFrames.First();
                var last = workFrames.Last();
                TimeSpan gv = TimeSpan.FromMinutes(first.BatDauPhut % 1440);
                TimeSpan gr = TimeSpan.FromMinutes(last.KetThucPhut % 1440);
                return (gv, gr);
            }

            if (phanNghi == "S") // Nghỉ sáng / Nửa đầu ca -> Làm buổi chiều / Nửa cuối ca
            {
                if (workFrames.Count >= 2)
                {
                    var f = workFrames[1];
                    return (TimeSpan.FromMinutes(f.BatDauPhut % 1440), TimeSpan.FromMinutes(f.KetThucPhut % 1440));
                }
                else
                {
                    // Ca chỉ có 1 khung: lấy nửa sau
                    var f = workFrames[0];
                    int mid = (f.BatDauPhut + f.KetThucPhut) / 2;
                    return (TimeSpan.FromMinutes(mid % 1440), TimeSpan.FromMinutes(f.KetThucPhut % 1440));
                }
            }

            if (phanNghi == "C") // Nghỉ chiều / Nửa cuối ca -> Làm buổi sáng / Nửa đầu ca
            {
                var f = workFrames[0];
                if (workFrames.Count >= 2)
                {
                    return (TimeSpan.FromMinutes(f.BatDauPhut % 1440), TimeSpan.FromMinutes(f.KetThucPhut % 1440));
                }
                else
                {
                    int mid = (f.BatDauPhut + f.KetThucPhut) / 2;
                    return (TimeSpan.FromMinutes(f.BatDauPhut % 1440), TimeSpan.FromMinutes(mid % 1440));
                }
            }

            return (null, null);
        }

        /// <summary>
        /// Hàm tính công tập trung dùng chung cho Live Preview và Lưu dữ liệu
        /// </summary>
        public static AttendancePreviewResult Calculate(
            ShiftInfo shift,
            bool isDiLam,
            bool isCongTac,
            string phanNghi,
            string loaiNghi,
            DateTime? gioVao,
            DateTime? gioRa,
            bool coLogQuetTheGoc,
            DateTime date)
        {
            var res = new AttendancePreviewResult
            {
                DuDieuKienChot = true,
                HasWarning = false,
                HasConflict = false
            };

            // 1. Trường hợp Đi công tác
            if (isDiLam && isCongTac)
            {
                res.KyHieu = "CT";
                res.NgayCong = shift.CongQuyDoi;
                res.NgayPhep = 0;
                res.TrangThaiHienThi = "Đi công tác (Đủ công theo chế độ)";
                res.GioLamHopLePhut = (int)(shift.TongGiayChuan / 60);
                res.GioLamHopLeText = $"{res.GioLamHopLePhut / 60} giờ {res.GioLamHopLePhut % 60:D2} phút";
                res.DiMuonPhut = 0;
                res.VeSomPhut = 0;
                res.DiMuonVeSomText = "Không tính vi phạm (Công tác)";
                return res;
            }

            // 2. Trường hợp Nghỉ nguyên ngày
            if (!isDiLam && phanNghi == "NN")
            {
                if (loaiNghi == "P")
                {
                    res.KyHieu = "P";
                    res.NgayPhep = 1.0m;
                    res.NgayCong = 1.0m; // Phép hưởng nguyên lương theo quy định hiện có
                    res.TrangThaiHienThi = "Nghỉ phép cả ngày (Hưởng lương phép)";
                }
                else
                {
                    res.KyHieu = "V";
                    res.NgayPhep = 0m;
                    res.NgayCong = 0m;
                    res.TrangThaiHienThi = "Nghỉ không phép cả ngày (Không công)";
                }

                res.GioLamHopLePhut = 0;
                res.GioLamHopLeText = "0 giờ (Nghỉ nguyên ngày)";
                res.DiMuonPhut = 0;
                res.VeSomPhut = 0;
                res.DiMuonVeSomText = "Không áp dụng";

                // Kiểm tra xung đột: Có log quẹt thẻ nhưng lại khai báo nghỉ nguyên ngày
                if (coLogQuetTheGoc && gioVao.HasValue && gioRa.HasValue)
                {
                    res.HasConflict = true;
                    res.ConflictMessage = $"Xung đột dữ liệu: Đang chọn nghỉ nguyên ngày nhưng hệ thống phát hiện có dữ liệu quẹt thẻ ({gioVao.Value:HH:mm} - {gioRa.Value:HH:mm})!";
                    res.DuDieuKienChot = false;
                }
                return res;
            }

            // 3. Trường hợp Đi làm hoặc Nghỉ một phần (Sáng / Chiều)
            // Kiểm tra thiếu dữ liệu quẹt thẻ
            if (!gioVao.HasValue && !gioRa.HasValue)
            {
                res.HasWarning = true;
                res.WarningMessage = "Thiếu cả giờ vào và giờ ra — Không đủ căn cứ xác nhận công!";
                res.DuDieuKienChot = false;
                res.NgayCong = 0;
                res.GioLamHopLePhut = 0;
                res.GioLamHopLeText = "Chưa có giờ vào/ra";
                res.DiMuonVeSomText = "Chưa có dữ liệu";

                if (!isDiLam)
                {
                    ApplyLeaveCredit(res, loaiNghi, phanNghi);
                }
                else
                {
                    res.KyHieu = shift.IsNightShift ? "CD" : "X";
                    res.NgayPhep = 0;
                    res.TrangThaiHienThi = "Chưa xác nhận (Thiếu dữ liệu giờ vào/ra)";
                }
                return res;
            }
            if (!gioVao.HasValue)
            {
                res.HasWarning = true;
                res.WarningMessage = "Thiếu giờ vào — Cần bổ sung giờ vào trước khi chốt công!";
                res.DuDieuKienChot = false;
                res.NgayCong = 0;
                res.GioLamHopLePhut = 0;
                res.GioLamHopLeText = "Thiếu giờ vào";
                res.DiMuonVeSomText = "Chưa hoàn chỉnh";
                if (!isDiLam) ApplyLeaveCredit(res, loaiNghi, phanNghi);
                else { res.KyHieu = shift.IsNightShift ? "CD" : "X"; res.NgayPhep = 0; res.TrangThaiHienThi = "Chưa hoàn chỉnh (Thiếu giờ vào)"; }
                return res;
            }
            if (!gioRa.HasValue)
            {
                res.HasWarning = true;
                res.WarningMessage = "Thiếu giờ ra — Cần bổ sung giờ ra trước khi chốt công!";
                res.DuDieuKienChot = false;
                res.NgayCong = 0;
                res.GioLamHopLePhut = 0;
                res.GioLamHopLeText = "Thiếu giờ ra";
                res.DiMuonVeSomText = "Chưa hoàn chỉnh";
                if (!isDiLam) ApplyLeaveCredit(res, loaiNghi, phanNghi);
                else { res.KyHieu = shift.IsNightShift ? "CD" : "X"; res.NgayPhep = 0; res.TrangThaiHienThi = "Chưa hoàn chỉnh (Thiếu giờ ra)"; }
                return res;
            }

            // Cả 2 giờ đã có: Tính toán chi tiết thời gian hợp lệ
            DateTime dtVao = gioVao.Value;
            DateTime dtRa = gioRa.Value;

            // Xử lý ca qua đêm: khi giờ ra nhỏ hơn giờ vào hoặc ca là ca đêm qua đêm
            if (shift.IsNightShift)
            {
                if (dtRa <= dtVao || (dtRa.Date == dtVao.Date && dtRa.Hour < 14 && dtVao.Hour >= 18))
                {
                    dtRa = dtVao.Date.AddDays(1).Add(dtRa.TimeOfDay);
                }
            }
            else if (dtRa < dtVao)
            {
                dtRa = dtRa.AddDays(1);
            }

            // Xác định các khung ca cần làm dựa trên lựa chọn
            var workFrames = shift.WorkFrames;
            List<ShiftFrameInfo> activeFrames = new List<ShiftFrameInfo>();

            if (isDiLam)
            {
                activeFrames = workFrames.ToList();
                res.KyHieu = shift.IsNightShift ? "CD" : "X";
                res.NgayPhep = 0;
                res.TrangThaiHienThi = shift.IsNightShift ? "Đi làm ca đêm" : "Đi làm cả ngày";
            }
            else if (phanNghi == "S") // Nghỉ sáng -> Cần làm buổi chiều
            {
                activeFrames = workFrames.Count >= 2 ? new List<ShiftFrameInfo> { workFrames[1] } : workFrames.ToList();
                ApplyLeaveCredit(res, loaiNghi, phanNghi);
            }
            else if (phanNghi == "C") // Nghỉ chiều -> Cần làm buổi sáng
            {
                activeFrames = workFrames.Count >= 1 ? new List<ShiftFrameInfo> { workFrames[0] } : workFrames.ToList();
                ApplyLeaveCredit(res, loaiNghi, phanNghi);
            }

            if (activeFrames.Count == 0)
            {
                activeFrames = workFrames.ToList();
            }

            // Tính thời gian làm thực tế trong các khoảng hợp lệ (trừ giờ nghỉ giữa ca)
            int tongPhutHopLe = 0;
            DateTime baseDate = date.Date;

            foreach (var frame in activeFrames)
            {
                DateTime fStart = baseDate.AddMinutes(frame.BatDauPhut);
                DateTime fEnd = baseDate.AddMinutes(frame.KetThucPhut);
                if (fEnd <= fStart) fEnd = fEnd.AddDays(1);

                DateTime oStart = dtVao > fStart ? dtVao : fStart;
                DateTime oEnd = dtRa < fEnd ? dtRa : fEnd;

                if (oEnd > oStart)
                {
                    tongPhutHopLe += (int)(oEnd - oStart).TotalMinutes;
                }
            }

            res.GioLamHopLePhut = tongPhutHopLe;
            res.GioLamHopLeText = $"{tongPhutHopLe / 60} giờ {tongPhutHopLe % 60:D2} phút";

            // Xét Đi muộn / Về sớm
            var firstFrame = activeFrames.First();
            var lastFrame = activeFrames.Last();

            DateTime expStart = baseDate.AddMinutes(firstFrame.BatDauPhut);
            DateTime expEnd = baseDate.AddMinutes(lastFrame.KetThucPhut);
            if (expEnd <= expStart) expEnd = expEnd.AddDays(1);

            int diMuon = 0;
            if (dtVao > expStart)
            {
                diMuon = (int)(dtVao - expStart).TotalMinutes;
            }

            int veSom = 0;
            if (dtRa < expEnd)
            {
                veSom = (int)(expEnd - dtRa).TotalMinutes;
            }

            res.DiMuonPhut = diMuon;
            res.VeSomPhut = veSom;

            var viPhamParts = new List<string>();
            if (diMuon > 5) viPhamParts.Add($"Đi muộn {diMuon} phút");
            if (veSom > 5) viPhamParts.Add($"Về sớm {veSom} phút");
            res.DiMuonVeSomText = viPhamParts.Count > 0 ? string.Join(", ", viPhamParts) : "Đúng giờ (không vi phạm)";

            // Tính công làm thực tế
            if (isDiLam)
            {
                int chuanPhut = (int)(shift.TongGiayChuan / 60);
                if (tongPhutHopLe >= (chuanPhut - 15))
                {
                    res.NgayCong = shift.CongQuyDoi;
                }
                else if (tongPhutHopLe >= (chuanPhut / 2 - 15))
                {
                    res.NgayCong = Math.Round(shift.CongQuyDoi / 2.0m, 2);
                    res.HasWarning = true;
                    res.WarningMessage = "Không đủ giờ cả ca, chỉ ghi nhận 0.5 công thực tế!";
                }
                else if (tongPhutHopLe > 0)
                {
                    res.NgayCong = Math.Round((decimal)tongPhutHopLe / chuanPhut * shift.CongQuyDoi, 2);
                    res.HasWarning = true;
                    res.WarningMessage = $"Chưa đủ thời gian làm việc chuẩn ({res.NgayCong:N2} công)!";
                }
                else
                {
                    res.NgayCong = 0;
                    res.HasWarning = true;
                    res.WarningMessage = "Thời gian quẹt thẻ nằm ngoài ca làm việc!";
                }
            }
            else
            {
                // Nghỉ nửa ngày: Công buổi còn lại tối đa là 0.5 (hoặc nửa ca chuẩn)
                int nuaCaPhut = (int)(shift.TongGiayChuan / 120);
                if (tongPhutHopLe >= (nuaCaPhut - 15))
                {
                    res.NgayCong = 0.5m;
                }
                else if (tongPhutHopLe > 0)
                {
                    res.NgayCong = Math.Round((decimal)tongPhutHopLe / (nuaCaPhut * 2) * shift.CongQuyDoi, 2);
                    res.HasWarning = true;
                    res.WarningMessage = "Buổi làm còn lại không đủ thời lượng!";
                }
                else
                {
                    res.NgayCong = 0;
                    res.HasWarning = true;
                    res.WarningMessage = "Chưa làm việc trong buổi còn lại!";
                }
            }

            return res;
        }

        private static void ApplyLeaveCredit(AttendancePreviewResult res, string loaiNghi, string phanNghi)
        {
            string phanText = (phanNghi == "S") ? "sáng" : (phanNghi == "C") ? "chiều" : "nguyên ngày";
            if (loaiNghi == "P")
            {
                res.KyHieu = "P";
                res.NgayPhep = 0.5m;
                res.TrangThaiHienThi = $"Nghỉ phép buổi {phanText} (0.5 phép)";
            }
            else
            {
                res.KyHieu = "V";
                res.NgayPhep = 0m;
                res.TrangThaiHienThi = $"Nghỉ không phép buổi {phanText}";
            }
        }
    }
}
