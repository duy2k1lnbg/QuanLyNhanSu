using Bu.CLASS_SYSTEM;
using DA;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bu.CLASS_PAYROLL
{
    public class PayrollOccurrenceDto
    {
        public string SourceType { get; set; }
        public string SourceId { get; set; }
        public int MaNV { get; set; }
        public string HoTen { get; set; }
        public string Nhom { get; set; }
        public string TenKhoan { get; set; }
        public DateTime NgayPhatSinh { get; set; }
        public decimal SoLuong { get; set; }
        public string DonVi { get; set; }
        public decimal DonGia { get; set; }
        public decimal SoTien { get; set; }
        public string LyDo { get; set; }
        public string SoChungTu { get; set; }
        public string TrangThai { get; set; }
        public bool CanApprove { get; set; }
        public bool CanRevoke { get; set; }
        public bool CanDelete { get; set; }
        public decimal? DataVersion { get; set; }
    }

    public class PayrollOccurrenceInput
    {
        public int MaNV { get; set; }
        public int MaKyCong { get; set; }
        public int Loai { get; set; } // 1: Thu nhập, 2: Khấu trừ
        public string TenKhoan { get; set; }
        public decimal SoTien { get; set; }
        public string LyDo { get; set; }
        public string SoChungTu { get; set; }
        public DateTime NgayPhatSinh { get; set; }
    }

    public class PayrollOccurrenceService
    {
        private class OccurrenceStatusRow
        {
            public string ID_BAN_GHI { get; set; }
            public string HANHDONG { get; set; }
            public string TEN_THUCHIEN { get; set; }
            public DateTime? THOIGIAN { get; set; }
            public string DU_LIEU_MOI { get; set; }
        }

        private class OccurrenceDbRow
        {
            public string SOQUYETDINH { get; set; }
            public string TRANG_THAI { get; set; }
            public decimal? APPROVED_BY { get; set; }
            public DateTime? APPROVED_DATE { get; set; }
            public decimal? REVOKED_BY { get; set; }
            public DateTime? REVOKED_DATE { get; set; }
            public string REVOKED_REASON { get; set; }
            public decimal? DATA_VERSION { get; set; }
        }

        internal static string GetOccurrenceStatus(MyEntities db, string soQuyetDinh)
        {
            if (PayrollEngine.CheckTrangThaiColumnInKtkl(db))
            {
                var status = db.Database.SqlQuery<string>(
                    "SELECT TRANG_THAI FROM TB_KHENTHUONG_KYLUAT WHERE SOQUYETDINH = :p0 AND DELETED_DATE IS NULL",
                    new OracleParameter("p0", soQuyetDinh)
                ).FirstOrDefault();
                if (!string.IsNullOrEmpty(status)) return status.Trim().ToUpperInvariant();
            }

            var latestAction = db.Database.SqlQuery<string>(@"
                SELECT HANHDONG FROM (
                    SELECT HANHDONG, ROW_NUMBER() OVER (ORDER BY THOIGIAN DESC, ID_LOG DESC) as rn
                    FROM TB_SYS_LOG
                    WHERE TEN_BANG = 'TB_KHENTHUONG_KYLUAT'
                      AND ID_BAN_GHI = :p0
                      AND HANHDONG IN ('DUYET_PHATSINH', 'THU_HOI_PHATSINH', 'THEM_PHATSINH_NHAP', 'SUA_PHATSINH_NHAP')
                ) WHERE rn = 1",
                new OracleParameter("p0", soQuyetDinh)
            ).FirstOrDefault();

            if (latestAction == "DUYET_PHATSINH") return "APPROVED";
            if (latestAction == "THU_HOI_PHATSINH") return "REVOKED";
            return "DRAFT";
        }

        public List<PayrollOccurrenceDto> GetOccurrences(int makycong, string filterType = "Tất cả")
        {
            var results = new List<PayrollOccurrenceDto>();
            int nam = makycong / 100;
            int thang = makycong % 100;
            DateTime startOfMonth = new DateTime(nam, thang, 1);
            DateTime endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            using (var db = new MyEntities())
            {
                // 1. Genuine period occurrences from TB_KHENTHUONG_KYLUAT
                var ktklQuery = from kt in db.TB_KHENTHUONG_KYLUAT
                                where kt.DELETED_DATE == null
                                   && ((kt.NAM_APDUNG == nam && kt.THANG_APDUNG == thang)
                                       || (kt.NAM_APDUNG == null && kt.THANG_APDUNG == null && kt.NGAY >= startOfMonth && kt.NGAY <= endOfMonth))
                                join nv in db.TB_NHANVIEN on kt.MANV equals nv.MANV into nvGroup
                                from nv in nvGroup.DefaultIfEmpty()
                                select new
                                {
                                    kt.SOQUYETDINH,
                                    kt.MANV,
                                    kt.LOAI,
                                    kt.SOTIEN,
                                    kt.NOIDUNG,
                                    kt.LYDO,
                                    kt.NGAY,
                                    kt.UPDATED_DATE,
                                    kt.UPDATED_BY,
                                    HOTEN = nv.HOTEN
                                };

                var ktklList = ktklQuery.ToList();

                // Lấy trạng thái phê duyệt từ DB column nếu đã có schema V1_21
                bool hasTrangThaiCol = PayrollEngine.CheckTrangThaiColumnInKtkl(db);
                var dbStatuses = new Dictionary<string, OccurrenceDbRow>(StringComparer.OrdinalIgnoreCase);
                if (hasTrangThaiCol)
                {
                    try
                    {
                        var rows = db.Database.SqlQuery<OccurrenceDbRow>(@"
                            SELECT SOQUYETDINH, TRANG_THAI, APPROVED_BY, APPROVED_DATE, REVOKED_BY, REVOKED_DATE, REVOKED_REASON, DATA_VERSION
                            FROM TB_KHENTHUONG_KYLUAT
                            WHERE DELETED_DATE IS NULL
                        ").ToList();
                        foreach (var r in rows)
                        {
                            if (!string.IsNullOrEmpty(r.SOQUYETDINH))
                                dbStatuses[r.SOQUYETDINH] = r;
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.TraceWarning("[PayrollOccurrenceService] Query db statuses: " + ex.Message);
                    }
                }

                // Lấy trạng thái phê duyệt từ TB_SYS_LOG phục vụ fallback đối soát
                var statusLogs = new Dictionary<string, OccurrenceStatusRow>(StringComparer.OrdinalIgnoreCase);
                try
                {
                    var logs = db.Database.SqlQuery<OccurrenceStatusRow>(@"
                        SELECT ID_BAN_GHI, HANHDONG, TEN_THUCHIEN, THOIGIAN, DU_LIEU_MOI
                        FROM (
                            SELECT ID_BAN_GHI, HANHDONG, TEN_THUCHIEN, THOIGIAN, DU_LIEU_MOI,
                                   ROW_NUMBER() OVER (PARTITION BY ID_BAN_GHI ORDER BY THOIGIAN DESC, ID_LOG DESC) as rn
                            FROM TB_SYS_LOG
                            WHERE TEN_BANG = 'TB_KHENTHUONG_KYLUAT'
                              AND HANHDONG IN ('DUYET_PHATSINH', 'THU_HOI_PHATSINH', 'THEM_PHATSINH_NHAP', 'SUA_PHATSINH_NHAP')
                        )
                        WHERE rn = 1
                    ").ToList();

                    foreach (var l in logs)
                    {
                        if (!string.IsNullOrEmpty(l.ID_BAN_GHI))
                            statusLogs[l.ID_BAN_GHI] = l;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceWarning("[PayrollOccurrenceService] Query status logs fallback: " + ex.Message);
                }

                foreach (var k in ktklList)
                {
                    bool isIncome = k.LOAI == 1;
                    string nhom = isIncome ? "Thu nhập phát sinh" : "Khấu trừ phát sinh";

                    if (filterType != "Tất cả" && filterType != nhom)
                        continue;

                    string trangThai = "Bản nháp";
                    bool isApproved = false;

                    if (dbStatuses.TryGetValue(k.SOQUYETDINH, out var dbRow) && !string.IsNullOrEmpty(dbRow.TRANG_THAI))
                    {
                        string upperStatus = dbRow.TRANG_THAI.Trim().ToUpperInvariant();
                        if (upperStatus == "APPROVED")
                        {
                            isApproved = true;
                            trangThai = "Đã duyệt";
                        }
                        else if (upperStatus == "REVOKED")
                        {
                            trangThai = "Đã thu hồi / Hủy";
                        }
                        else if (upperStatus == "PENDING_AUDIT")
                        {
                            trangThai = "Chờ đối soát";
                        }
                        else
                        {
                            trangThai = "Bản nháp";
                        }
                    }
                    else if (statusLogs.TryGetValue(k.SOQUYETDINH, out var log))
                    {
                        if (log.HANHDONG == "DUYET_PHATSINH")
                        {
                            isApproved = true;
                            trangThai = "Đã duyệt";
                        }
                        else if (log.HANHDONG == "THU_HOI_PHATSINH")
                        {
                            trangThai = "Đã thu hồi / Hủy";
                        }
                        else
                        {
                            trangThai = "Bản nháp";
                        }
                    }
                    else
                    {
                        trangThai = "Bản nháp (Chưa duyệt)";
                    }

                    results.Add(new PayrollOccurrenceDto
                    {
                        SourceType = "PHATSINH",
                        SourceId = k.SOQUYETDINH,
                        MaNV = (int)(k.MANV ?? 0),
                        HoTen = k.HOTEN ?? "",
                        Nhom = nhom,
                        TenKhoan = k.NOIDUNG ?? (isIncome ? "Khen thưởng / Thưởng phát sinh" : "Khấu trừ phát sinh khác"),
                        NgayPhatSinh = k.NGAY ?? startOfMonth,
                        SoLuong = 1,
                        DonVi = "khoản",
                        DonGia = (decimal)(k.SOTIEN ?? 0),
                        SoTien = (decimal)(k.SOTIEN ?? 0),
                        LyDo = k.LYDO ?? "",
                        SoChungTu = k.SOQUYETDINH ?? "",
                        TrangThai = trangThai,
                        CanApprove = !isApproved && trangThai != "Đã thu hồi / Hủy",
                        CanRevoke = isApproved,
                        CanDelete = !isApproved,
                        DataVersion = dbRow?.DATA_VERSION ?? 1m
                    });
                }

                // 2. Reference data: Allowances from TB_NHANVIEN_PHUCAP (Read-only reference)
                if (filterType == "Tất cả" || filterType == "Thu nhập phát sinh")
                {
                    var allowances = (from np in db.TB_NHANVIEN_PHUCAP
                                      where np.DELETED_DATE == null
                                        && (np.TU_NGAY == null || np.TU_NGAY <= endOfMonth)
                                        && (np.DEN_NGAY == null || np.DEN_NGAY >= startOfMonth)
                                      join nv in db.TB_NHANVIEN on np.MANV equals nv.MANV into nvGroup
                                      from nv in nvGroup.DefaultIfEmpty()
                                      join pc in db.TB_PHUCAP on np.IDPC equals pc.IDPC into pcGroup
                                      from pc in pcGroup.DefaultIfEmpty()
                                      select new
                                      {
                                          np.MANV,
                                          np.IDPC,
                                          HOTEN = nv.HOTEN,
                                          TENPC = pc.TENPC,
                                          np.SOTIEN,
                                          np.GHICHU,
                                          np.CREATED_DATE
                                      }).Take(100).ToList();

                    foreach (var a in allowances)
                    {
                        results.Add(new PayrollOccurrenceDto
                        {
                            SourceType = "PHUCAP",
                            SourceId = $"{a.MANV}_{a.IDPC}",
                            MaNV = (int)a.MANV,
                            HoTen = a.HOTEN ?? "",
                            Nhom = "Thu nhập phát sinh",
                            TenKhoan = a.TENPC ?? "Phụ cấp",
                            NgayPhatSinh = a.CREATED_DATE ?? startOfMonth,
                            SoLuong = 1,
                            DonVi = "khoản",
                            DonGia = (decimal)(a.SOTIEN ?? 0),
                            SoTien = (decimal)(a.SOTIEN ?? 0),
                            LyDo = a.GHICHU ?? "Phụ cấp theo hợp đồng lao động",
                            SoChungTu = "HĐ-PC-" + a.MANV,
                            TrangThai = "Theo hợp đồng (Tham chiếu)",
                            CanApprove = false,
                            CanRevoke = false,
                            CanDelete = false
                        });
                    }
                }

                // 3. Reference data: Advances from TB_UNGLUONG (Read-only reference)
                if (filterType == "Tất cả" || filterType == "Khấu trừ phát sinh")
                {
                    var advances = (from ul in db.TB_UNGLUONG
                                    where ul.THANG == thang && ul.NAM == nam && ul.DELETED_DATE == null
                                    join nv in db.TB_NHANVIEN on ul.MANV equals nv.MANV into nvGroup
                                    from nv in nvGroup.DefaultIfEmpty()
                                    select new
                                    {
                                        ul.IDUL,
                                        ul.MANV,
                                        HOTEN = nv.HOTEN,
                                        ul.NGAY,
                                        ul.SOTIENUNG,
                                        ul.GHICHU
                                    }).ToList();

                    foreach (var u in advances)
                    {
                        DateTime ngayUng;
                        try
                        {
                            int day = (int)(u.NGAY ?? 1);
                            if (day < 1 || day > DateTime.DaysInMonth(nam, thang)) day = 1;
                            ngayUng = new DateTime(nam, thang, day);
                        }
                        catch
                        {
                            ngayUng = startOfMonth;
                        }

                        results.Add(new PayrollOccurrenceDto
                        {
                            SourceType = "UNGLUONG",
                            SourceId = u.IDUL.ToString(),
                            MaNV = (int)(u.MANV ?? 0),
                            HoTen = u.HOTEN ?? "",
                            Nhom = "Khấu trừ phát sinh",
                            TenKhoan = "Tạm ứng lương",
                            NgayPhatSinh = ngayUng,
                            SoLuong = 1,
                            DonVi = "lần",
                            DonGia = (decimal)(u.SOTIENUNG ?? 0),
                            SoTien = (decimal)(u.SOTIENUNG ?? 0),
                            LyDo = u.GHICHU ?? "Tạm ứng tiền lương trong kỳ",
                            SoChungTu = "UL-" + u.IDUL,
                            TrangThai = "Phiếu tạm ứng (Tham chiếu)",
                            CanApprove = false,
                            CanRevoke = false,
                            CanDelete = false
                        });
                    }
                }
            }

            return results.OrderBy(x => x.MaNV).ThenBy(x => x.Nhom).ToList();
        }

        public string SaveDraftOccurrence(PayrollOccurrenceInput input, int userId)
        {
            if (UserSession.CurrentUser == null || UserSession.CurrentUser.IDUSER <= 0)
                throw new BusinessException("UNAUTHENTICATED", "Người dùng chưa đăng nhập hoặc phiên làm việc đã hết hạn.");

            int effectiveUserId = (int)UserSession.CurrentUser.IDUSER;

            if (!UserSession.IsAdmin && !UserSession.CanAdd("F_CC_BANGLUONG"))
                throw new BusinessException("PERMISSION_DENIED", "Bạn không có quyền thêm mới khoản phát sinh lương. Vui lòng liên hệ quản trị viên.");

            if (input.MaNV <= 0)
                throw new BusinessException("VALIDATION_ERROR", "Mã nhân viên không hợp lệ.", "MaNV");

            if (input.SoTien <= 0)
                throw new BusinessException("VALIDATION_ERROR", "Số tiền phát sinh phải lớn hơn 0.", "SoTien");

            if (string.IsNullOrWhiteSpace(input.LyDo))
                throw new BusinessException("VALIDATION_ERROR", "Vui lòng nhập lý do phát sinh bắt buộc.", "LyDo");

            int nam = input.MaKyCong / 100;
            int thang = input.MaKyCong % 100;

            using (var db = new MyEntities())
            using (var tx = db.Database.BeginTransaction())
            {
                try
                {
                    // Kiểm tra kỳ công có bị khóa không
                    var kc = db.TB_KYCONG.FirstOrDefault(x => x.MAKYCONG == (decimal)input.MaKyCong);
                    if (kc != null && (kc.KHOA ?? 0) == 1)
                    {
                        throw new BusinessException("PERIOD_LOCKED", $"Kỳ công {input.MaKyCong} đã bị khóa sổ. Không thể thêm phát sinh lương mới.");
                    }

                    // Kiểm tra nhân viên tồn tại
                    var nv = db.TB_NHANVIEN.FirstOrDefault(x => x.MANV == input.MaNV);
                    if (nv == null)
                        throw new BusinessException("NOT_FOUND", $"Không tìm thấy nhân viên mã {input.MaNV}.");

                    string soqd = string.IsNullOrWhiteSpace(input.SoChungTu)
                        ? $"PS-{nam}{thang:D2}-{input.MaNV}-{Guid.NewGuid().ToString("N").Substring(0, 4).ToUpper()}"
                        : input.SoChungTu.Trim();

                    // Kiểm tra trùng số quyết định / chứng từ
                    var dup = db.TB_KHENTHUONG_KYLUAT.FirstOrDefault(x => x.SOQUYETDINH == soqd);
                    if (dup != null)
                        throw new BusinessException("DUPLICATE_KEY", $"Số quyết định/chứng từ {soqd} đã tồn tại trong hệ thống.");

                    var item = new TB_KHENTHUONG_KYLUAT
                    {
                        SOQUYETDINH = soqd,
                        MANV = input.MaNV,
                        LOAI = input.Loai,
                        SOTIEN = input.SoTien,
                        NOIDUNG = string.IsNullOrWhiteSpace(input.TenKhoan) ? (input.Loai == 1 ? "Thưởng phát sinh" : "Khấu trừ phát sinh") : input.TenKhoan.Trim(),
                        LYDO = input.LyDo.Trim(),
                        NGAY = input.NgayPhatSinh != DateTime.MinValue ? input.NgayPhatSinh : DateTime.Today,
                        THANG_APDUNG = thang,
                        NAM_APDUNG = nam,
                        CREATED_BY = effectiveUserId,
                        CREATED_DATE = DateTime.Now,
                        UPDATED_BY = null,  // Bản nháp chưa duyệt
                        UPDATED_DATE = null // Bản nháp chưa duyệt
                    };

                    db.TB_KHENTHUONG_KYLUAT.Add(item);
                    db.SaveChanges();

                    if (PayrollEngine.CheckTrangThaiColumnInKtkl(db))
                    {
                        db.Database.ExecuteSqlCommand(
                            "UPDATE TB_KHENTHUONG_KYLUAT SET TRANG_THAI = 'DRAFT', DATA_VERSION = 1 WHERE SOQUYETDINH = :p0",
                            new OracleParameter("p0", soqd)
                        );
                    }

                    // Ghi persistent audit log
                    string auditUser = UserSession.CurrentUser.FULLNAME ?? ("User " + effectiveUserId);
                    db.Database.ExecuteSqlCommand(@"
                        INSERT INTO TB_SYS_LOG (
                            MANV_THUCHIEN, TEN_THUCHIEN, HANHDONG, TEN_BANG, ID_BAN_GHI,
                            DU_LIEU_MOI, THOIGIAN, MODULE_NAME, CHANGED_FIELDS
                        ) VALUES (
                            :p0, :p1, :p2, :p3, :p4,
                            :p5, CURRENT_TIMESTAMP, :p6, :p7
                        )",
                        new OracleParameter("p0", effectiveUserId),
                        new OracleParameter("p1", auditUser),
                        new OracleParameter("p2", "THEM_PHATSINH_NHAP"),
                        new OracleParameter("p3", "TB_KHENTHUONG_KYLUAT"),
                        new OracleParameter("p4", soqd),
                        new OracleParameter("p5", $"MANV={input.MaNV}, SoTien={input.SoTien}, Loai={input.Loai}, LyDo={input.LyDo}"),
                        new OracleParameter("p6", "TIENLUONG"),
                        new OracleParameter("p7", "STATUS=DRAFT")
                    );

                    tx.Commit();
                    return soqd;
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }

        public void UpdateDraftOccurrence(PayrollOccurrenceInput input, int userId, decimal? expectedVersion = null)
        {
            if (UserSession.CurrentUser == null || UserSession.CurrentUser.IDUSER <= 0)
                throw new BusinessException("UNAUTHENTICATED", "Người dùng chưa đăng nhập hoặc phiên làm việc đã hết hạn.");

            int effectiveUserId = (int)UserSession.CurrentUser.IDUSER;

            if (!UserSession.IsAdmin && !UserSession.CanEdit("F_CC_BANGLUONG"))
                throw new BusinessException("PERMISSION_DENIED", "Bạn không có quyền cập nhật khoản phát sinh lương. Vui lòng liên hệ quản trị viên.");

            if (string.IsNullOrWhiteSpace(input.SoChungTu))
                throw new BusinessException("INVALID_ID", "Mã quyết định/chứng từ không hợp lệ.");

            if (input.SoTien <= 0)
                throw new BusinessException("VALIDATION_ERROR", "Số tiền phát sinh phải lớn hơn 0.", "SoTien");

            if (string.IsNullOrWhiteSpace(input.LyDo))
                throw new BusinessException("VALIDATION_ERROR", "Vui lòng nhập lý do phát sinh bắt buộc.", "LyDo");

            using (var db = new MyEntities())
            using (var tx = db.Database.BeginTransaction())
            {
                try
                {
                    var item = db.TB_KHENTHUONG_KYLUAT.FirstOrDefault(x => x.SOQUYETDINH == input.SoChungTu && x.DELETED_DATE == null);
                    if (item == null)
                        throw new BusinessException("NOT_FOUND", $"Không tìm thấy khoản phát sinh mã [{input.SoChungTu}].");

                    if (item.NAM_APDUNG.HasValue && item.THANG_APDUNG.HasValue)
                    {
                        decimal makycong = item.NAM_APDUNG.Value * 100 + item.THANG_APDUNG.Value;
                        var kc = db.TB_KYCONG.FirstOrDefault(x => x.MAKYCONG == makycong);
                        if (kc != null && (kc.KHOA ?? 0) == 1)
                        {
                            throw new BusinessException("PERIOD_LOCKED", $"Kỳ công {makycong} đã bị khóa sổ. Không thể sửa phát sinh của kỳ đã khóa.");
                        }
                    }

                    // Kiểm tra trạng thái hiện tại
                    string currentStatus = GetOccurrenceStatus(db, input.SoChungTu);
                    if (currentStatus == "APPROVED")
                    {
                        throw new BusinessException("CANNOT_EDIT_APPROVED", $"Khoản phát sinh [{input.SoChungTu}] đã duyệt không thể sửa trực tiếp. Vui lòng thu hồi phê duyệt trước khi chỉnh sửa.");
                    }
                    if (currentStatus == "REVOKED")
                    {
                        throw new BusinessException("INVALID_STATE", $"Khoản phát sinh [{input.SoChungTu}] đã bị thu hồi/hủy không thể sửa trực tiếp.");
                    }

                    item.MANV = input.MaNV;
                    item.LOAI = input.Loai;
                    item.SOTIEN = input.SoTien;
                    item.NOIDUNG = string.IsNullOrWhiteSpace(input.TenKhoan) ? (input.Loai == 1 ? "Thưởng phát sinh" : "Khấu trừ phát sinh") : input.TenKhoan.Trim();
                    item.LYDO = input.LyDo.Trim();
                    if (input.NgayPhatSinh != DateTime.MinValue) item.NGAY = input.NgayPhatSinh;

                    // Sửa bản nháp vẫn giữ là bản nháp
                    item.UPDATED_BY = effectiveUserId;
                    item.UPDATED_DATE = null; // Vẫn là nháp

                    db.SaveChanges();

                    if (PayrollEngine.CheckTrangThaiColumnInKtkl(db))
                    {
                        if (expectedVersion.HasValue)
                        {
                            int rows = db.Database.ExecuteSqlCommand(@"
                                UPDATE TB_KHENTHUONG_KYLUAT 
                                SET TRANG_THAI = 'DRAFT', DATA_VERSION = NVL(DATA_VERSION, 1) + 1 
                                WHERE SOQUYETDINH = :p0 AND (DATA_VERSION = :p1 OR (DATA_VERSION IS NULL AND :p1 = 1))",
                                new OracleParameter("p0", input.SoChungTu),
                                new OracleParameter("p1", expectedVersion.Value)
                            );
                            if (rows == 0)
                            {
                                throw new BusinessException("CONCURRENCY_CONFLICT", $"Khoản phát sinh [{input.SoChungTu}] đã bị người khác thay đổi trước đó (phiên bản dữ liệu không khớp). Vui lòng tải lại trang.");
                            }
                        }
                        else
                        {
                            db.Database.ExecuteSqlCommand(
                                "UPDATE TB_KHENTHUONG_KYLUAT SET TRANG_THAI = 'DRAFT', DATA_VERSION = NVL(DATA_VERSION, 1) + 1 WHERE SOQUYETDINH = :p0",
                                new OracleParameter("p0", input.SoChungTu)
                            );
                        }
                    }

                    string auditUser = UserSession.CurrentUser.FULLNAME ?? ("User " + effectiveUserId);
                    db.Database.ExecuteSqlCommand(@"
                        INSERT INTO TB_SYS_LOG (
                            MANV_THUCHIEN, TEN_THUCHIEN, HANHDONG, TEN_BANG, ID_BAN_GHI,
                            DU_LIEU_MOI, THOIGIAN, MODULE_NAME, CHANGED_FIELDS
                        ) VALUES (
                            :p0, :p1, :p2, :p3, :p4,
                            :p5, CURRENT_TIMESTAMP, :p6, :p7
                        )",
                        new OracleParameter("p0", effectiveUserId),
                        new OracleParameter("p1", auditUser),
                        new OracleParameter("p2", "SUA_PHATSINH_NHAP"),
                        new OracleParameter("p3", "TB_KHENTHUONG_KYLUAT"),
                        new OracleParameter("p4", input.SoChungTu),
                        new OracleParameter("p5", $"MANV={input.MaNV}, SoTien={input.SoTien}, Loai={input.Loai}, LyDo={input.LyDo}"),
                        new OracleParameter("p6", "TIENLUONG"),
                        new OracleParameter("p7", "STATUS=DRAFT")
                    );

                    tx.Commit();
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }

        public void ApproveOccurrence(string soQuyetDinh, int userId, decimal? expectedVersion = null)
        {
            if (UserSession.CurrentUser == null || UserSession.CurrentUser.IDUSER <= 0)
                throw new BusinessException("UNAUTHENTICATED", "Người dùng chưa đăng nhập hoặc phiên làm việc đã hết hạn.");

            int effectiveUserId = (int)UserSession.CurrentUser.IDUSER;

            if (string.IsNullOrWhiteSpace(soQuyetDinh))
                throw new BusinessException("INVALID_ID", "Mã quyết định/chứng từ không hợp lệ.");

            if (!UserSession.IsAdmin && !UserSession.CanEdit("F_CC_BANGLUONG"))
                throw new BusinessException("PERMISSION_DENIED", "Bạn không có quyền phê duyệt phát sinh lương. Vui lòng liên hệ quản trị viên.");

            using (var db = new MyEntities())
            using (var tx = db.Database.BeginTransaction())
            {
                try
                {
                    var item = db.TB_KHENTHUONG_KYLUAT.FirstOrDefault(x => x.SOQUYETDINH == soQuyetDinh && x.DELETED_DATE == null);
                    if (item == null)
                    {
                        throw new BusinessException("NOT_FOUND", $"Không tìm thấy khoản phát sinh mã [{soQuyetDinh}] hoặc khoản này đã bị xóa.");
                    }

                    // Kiểm tra kỳ công có bị khóa không
                    if (item.NAM_APDUNG.HasValue && item.THANG_APDUNG.HasValue)
                    {
                        decimal makycong = item.NAM_APDUNG.Value * 100 + item.THANG_APDUNG.Value;
                        var kc = db.TB_KYCONG.FirstOrDefault(x => x.MAKYCONG == makycong);
                        if (kc != null && (kc.KHOA ?? 0) == 1)
                        {
                            throw new BusinessException("PERIOD_LOCKED", $"Kỳ công {makycong} đã bị khóa sổ. Không thể duyệt phát sinh cho kỳ đã khóa.");
                        }
                    }

                    // Kiểm tra trạng thái hiện tại
                    string currentStatus = GetOccurrenceStatus(db, soQuyetDinh);
                    if (currentStatus == "APPROVED")
                    {
                        throw new BusinessException("ALREADY_APPROVED", $"Khoản phát sinh [{soQuyetDinh}] đã ở trạng thái được duyệt.");
                    }
                    if (currentStatus == "REVOKED")
                    {
                        throw new BusinessException("INVALID_STATE", $"Khoản phát sinh [{soQuyetDinh}] đã bị thu hồi/hủy. Vui lòng tạo bản ghi mới thay vì duyệt lại.");
                    }

                    item.UPDATED_BY = effectiveUserId;
                    item.UPDATED_DATE = DateTime.Now;

                    int rows = db.SaveChanges();
                    if (rows == 0)
                    {
                        throw new BusinessException("CONCURRENCY_ERROR", "Dữ liệu đã bị thay đổi hoặc không có dòng nào được cập nhật.");
                    }

                    if (PayrollEngine.CheckTrangThaiColumnInKtkl(db))
                    {
                        int updatedRows;
                        if (expectedVersion.HasValue)
                        {
                            updatedRows = db.Database.ExecuteSqlCommand(@"
                                UPDATE TB_KHENTHUONG_KYLUAT 
                                SET TRANG_THAI = 'APPROVED', 
                                    APPROVED_BY = :p0, 
                                    APPROVED_DATE = SYSTIMESTAMP, 
                                    DATA_VERSION = NVL(DATA_VERSION, 1) + 1 
                                WHERE SOQUYETDINH = :p1 AND (DATA_VERSION = :p2 OR (DATA_VERSION IS NULL AND :p2 = 1))",
                                new OracleParameter("p0", effectiveUserId),
                                new OracleParameter("p1", soQuyetDinh),
                                new OracleParameter("p2", expectedVersion.Value)
                            );
                        }
                        else
                        {
                            updatedRows = db.Database.ExecuteSqlCommand(@"
                                UPDATE TB_KHENTHUONG_KYLUAT 
                                SET TRANG_THAI = 'APPROVED', 
                                    APPROVED_BY = :p0, 
                                    APPROVED_DATE = SYSTIMESTAMP, 
                                    DATA_VERSION = NVL(DATA_VERSION, 1) + 1 
                                WHERE SOQUYETDINH = :p1",
                                new OracleParameter("p0", effectiveUserId),
                                new OracleParameter("p1", soQuyetDinh)
                            );
                        }

                        if (expectedVersion.HasValue && updatedRows == 0)
                        {
                            throw new BusinessException("CONCURRENCY_CONFLICT", $"Khoản phát sinh [{soQuyetDinh}] đã bị người dùng khác thay đổi trước đó (phiên bản không khớp). Vui lòng tải lại dữ liệu trước khi phê duyệt.");
                        }
                    }

                    // Ghi audit DUYET_PHATSINH
                    string auditUser = UserSession.CurrentUser.FULLNAME ?? ("User " + effectiveUserId);
                    db.Database.ExecuteSqlCommand(@"
                        INSERT INTO TB_SYS_LOG (
                            MANV_THUCHIEN, TEN_THUCHIEN, HANHDONG, TEN_BANG, ID_BAN_GHI,
                            DU_LIEU_MOI, THOIGIAN, MODULE_NAME, CHANGED_FIELDS
                        ) VALUES (
                            :p0, :p1, :p2, :p3, :p4,
                            :p5, CURRENT_TIMESTAMP, :p6, :p7
                        )",
                        new OracleParameter("p0", effectiveUserId),
                        new OracleParameter("p1", auditUser),
                        new OracleParameter("p2", "DUYET_PHATSINH"),
                        new OracleParameter("p3", "TB_KHENTHUONG_KYLUAT"),
                        new OracleParameter("p4", soQuyetDinh),
                        new OracleParameter("p5", $"UPDATED_BY={effectiveUserId}, UPDATED_DATE={DateTime.Now:yyyy-MM-dd HH:mm:ss}"),
                        new OracleParameter("p6", "TIENLUONG"),
                        new OracleParameter("p7", "STATUS=APPROVED")
                    );

                    tx.Commit();
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }

        public void RevokeOccurrence(string soQuyetDinh, int userId, string lyDo = null, decimal? expectedVersion = null)
        {
            if (UserSession.CurrentUser == null || UserSession.CurrentUser.IDUSER <= 0)
                throw new BusinessException("UNAUTHENTICATED", "Người dùng chưa đăng nhập hoặc phiên làm việc đã hết hạn.");

            int effectiveUserId = (int)UserSession.CurrentUser.IDUSER;

            if (!UserSession.IsAdmin && !UserSession.CanEdit("F_CC_BANGLUONG"))
                throw new BusinessException("PERMISSION_DENIED", "Bạn không có quyền thu hồi phê duyệt phát sinh lương. Vui lòng liên hệ quản trị viên.");

            if (string.IsNullOrWhiteSpace(lyDo))
                throw new BusinessException("VALIDATION_ERROR", "Vui lòng nhập lý do thu hồi phê duyệt phát sinh lương bắt buộc.", "LyDo");

            using (var db = new MyEntities())
            using (var tx = db.Database.BeginTransaction())
            {
                try
                {
                    var item = db.TB_KHENTHUONG_KYLUAT.FirstOrDefault(x => x.SOQUYETDINH == soQuyetDinh && x.DELETED_DATE == null);
                    if (item == null)
                        throw new BusinessException("NOT_FOUND", $"Không tìm thấy khoản phát sinh mã [{soQuyetDinh}].");

                    if (item.NAM_APDUNG.HasValue && item.THANG_APDUNG.HasValue)
                    {
                        decimal makycong = item.NAM_APDUNG.Value * 100 + item.THANG_APDUNG.Value;
                        var kc = db.TB_KYCONG.FirstOrDefault(x => x.MAKYCONG == makycong);
                        if (kc != null && (kc.KHOA ?? 0) == 1)
                        {
                            throw new BusinessException("PERIOD_LOCKED", $"Kỳ công {makycong} đã bị khóa sổ. Không thể thu hồi phát sinh của kỳ đã khóa.");
                        }
                    }

                    // Kiểm tra trạng thái hiện tại
                    string currentStatus = GetOccurrenceStatus(db, soQuyetDinh);
                    if (currentStatus != "APPROVED")
                    {
                        throw new BusinessException("INVALID_STATE", $"Chỉ có thể thu hồi các khoản phát sinh đang ở trạng thái 'Đã duyệt'. Khoản này hiện là '{currentStatus}'.");
                    }

                    item.UPDATED_BY = effectiveUserId;
                    item.UPDATED_DATE = DateTime.Now;
                    db.SaveChanges();

                    if (PayrollEngine.CheckTrangThaiColumnInKtkl(db))
                    {
                        int updatedRows;
                        if (expectedVersion.HasValue)
                        {
                            updatedRows = db.Database.ExecuteSqlCommand(@"
                                UPDATE TB_KHENTHUONG_KYLUAT 
                                SET TRANG_THAI = 'REVOKED', 
                                    REVOKED_BY = :p0, 
                                    REVOKED_DATE = SYSTIMESTAMP, 
                                    REVOKED_REASON = :p1, 
                                    DATA_VERSION = NVL(DATA_VERSION, 1) + 1 
                                WHERE SOQUYETDINH = :p2 AND (DATA_VERSION = :p3 OR (DATA_VERSION IS NULL AND :p3 = 1))",
                                new OracleParameter("p0", effectiveUserId),
                                new OracleParameter("p1", (object)lyDo.Trim() ?? DBNull.Value),
                                new OracleParameter("p2", soQuyetDinh),
                                new OracleParameter("p3", expectedVersion.Value)
                            );
                        }
                        else
                        {
                            updatedRows = db.Database.ExecuteSqlCommand(@"
                                UPDATE TB_KHENTHUONG_KYLUAT 
                                SET TRANG_THAI = 'REVOKED', 
                                    REVOKED_BY = :p0, 
                                    REVOKED_DATE = SYSTIMESTAMP, 
                                    REVOKED_REASON = :p1, 
                                    DATA_VERSION = NVL(DATA_VERSION, 1) + 1 
                                WHERE SOQUYETDINH = :p2",
                                new OracleParameter("p0", effectiveUserId),
                                new OracleParameter("p1", (object)lyDo.Trim() ?? DBNull.Value),
                                new OracleParameter("p2", soQuyetDinh)
                            );
                        }

                        if (expectedVersion.HasValue && updatedRows == 0)
                        {
                            throw new BusinessException("CONCURRENCY_CONFLICT", $"Khoản phát sinh [{soQuyetDinh}] đã bị người dùng khác thay đổi trước đó (phiên bản không khớp). Vui lòng tải lại dữ liệu trước khi thu hồi.");
                        }
                    }

                    string auditUser = UserSession.CurrentUser.FULLNAME ?? ("User " + effectiveUserId);
                    db.Database.ExecuteSqlCommand(@"
                        INSERT INTO TB_SYS_LOG (
                            MANV_THUCHIEN, TEN_THUCHIEN, HANHDONG, TEN_BANG, ID_BAN_GHI,
                            DU_LIEU_MOI, THOIGIAN, MODULE_NAME, CHANGED_FIELDS
                        ) VALUES (
                            :p0, :p1, :p2, :p3, :p4,
                            :p5, CURRENT_TIMESTAMP, :p6, :p7
                        )",
                        new OracleParameter("p0", effectiveUserId),
                        new OracleParameter("p1", auditUser),
                        new OracleParameter("p2", "THU_HOI_PHATSINH"),
                        new OracleParameter("p3", "TB_KHENTHUONG_KYLUAT"),
                        new OracleParameter("p4", soQuyetDinh),
                        new OracleParameter("p5", $"REVERT_TO_DRAFT;LYDO={lyDo.Trim()}"),
                        new OracleParameter("p6", "TIENLUONG"),
                        new OracleParameter("p7", "STATUS=REVOKED")
                    );

                    tx.Commit();
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }

        public void DeleteOccurrence(string soQuyetDinh, int userId, decimal? expectedVersion = null)
        {
            if (UserSession.CurrentUser == null || UserSession.CurrentUser.IDUSER <= 0)
                throw new BusinessException("UNAUTHENTICATED", "Người dùng chưa đăng nhập hoặc phiên làm việc đã hết hạn.");

            int effectiveUserId = (int)UserSession.CurrentUser.IDUSER;

            if (!UserSession.IsAdmin && !UserSession.CanDelete("F_CC_BANGLUONG"))
                throw new BusinessException("PERMISSION_DENIED", "Bạn không có quyền xóa khoản phát sinh lương. Vui lòng liên hệ quản trị viên.");

            using (var db = new MyEntities())
            using (var tx = db.Database.BeginTransaction())
            {
                try
                {
                    var item = db.TB_KHENTHUONG_KYLUAT.FirstOrDefault(x => x.SOQUYETDINH == soQuyetDinh && x.DELETED_DATE == null);
                    if (item == null)
                        throw new BusinessException("NOT_FOUND", $"Không tìm thấy khoản phát sinh mã [{soQuyetDinh}].");

                    if (item.NAM_APDUNG.HasValue && item.THANG_APDUNG.HasValue)
                    {
                        decimal makycong = item.NAM_APDUNG.Value * 100 + item.THANG_APDUNG.Value;
                        var kc = db.TB_KYCONG.FirstOrDefault(x => x.MAKYCONG == makycong);
                        if (kc != null && (kc.KHOA ?? 0) == 1)
                        {
                            throw new BusinessException("PERIOD_LOCKED", $"Kỳ công {makycong} đã bị khóa sổ. Không thể xóa khoản phát sinh của kỳ đã khóa.");
                        }
                    }

                    // Không cho phép xóa khoản đang ở trạng thái đã duyệt
                    string currentStatus = GetOccurrenceStatus(db, soQuyetDinh);
                    if (currentStatus == "APPROVED")
                    {
                        throw new BusinessException("CANNOT_DELETE_APPROVED", $"Khoản phát sinh [{soQuyetDinh}] đã được duyệt. Vui lòng thu hồi phê duyệt trước khi xóa.");
                    }

                    item.DELETED_DATE = DateTime.Now;
                    item.DELETED_BY = effectiveUserId;
                    db.SaveChanges();

                    if (PayrollEngine.CheckTrangThaiColumnInKtkl(db))
                    {
                        int updatedRows;
                        if (expectedVersion.HasValue)
                        {
                            updatedRows = db.Database.ExecuteSqlCommand(@"
                                UPDATE TB_KHENTHUONG_KYLUAT 
                                SET DATA_VERSION = NVL(DATA_VERSION, 1) + 1 
                                WHERE SOQUYETDINH = :p0 AND (DATA_VERSION = :p1 OR (DATA_VERSION IS NULL AND :p1 = 1))",
                                new OracleParameter("p0", soQuyetDinh),
                                new OracleParameter("p1", expectedVersion.Value)
                            );
                        }
                        else
                        {
                            updatedRows = db.Database.ExecuteSqlCommand(@"
                                UPDATE TB_KHENTHUONG_KYLUAT 
                                SET DATA_VERSION = NVL(DATA_VERSION, 1) + 1 
                                WHERE SOQUYETDINH = :p0",
                                new OracleParameter("p0", soQuyetDinh)
                            );
                        }

                        if (expectedVersion.HasValue && updatedRows == 0)
                        {
                            throw new BusinessException("CONCURRENCY_CONFLICT", $"Khoản phát sinh [{soQuyetDinh}] đã bị người dùng khác thay đổi trước đó (phiên bản không khớp). Vui lòng tải lại dữ liệu trước khi xóa.");
                        }
                    }

                    string auditUser = UserSession.CurrentUser.FULLNAME ?? ("User " + effectiveUserId);
                    db.Database.ExecuteSqlCommand(@"
                        INSERT INTO TB_SYS_LOG (
                            MANV_THUCHIEN, TEN_THUCHIEN, HANHDONG, TEN_BANG, ID_BAN_GHI,
                            DU_LIEU_MOI, THOIGIAN, MODULE_NAME, CHANGED_FIELDS
                        ) VALUES (
                            :p0, :p1, :p2, :p3, :p4,
                            :p5, CURRENT_TIMESTAMP, :p6, :p7
                        )",
                        new OracleParameter("p0", effectiveUserId),
                        new OracleParameter("p1", auditUser),
                        new OracleParameter("p2", "XOA_MEM_PHATSINH"),
                        new OracleParameter("p3", "TB_KHENTHUONG_KYLUAT"),
                        new OracleParameter("p4", soQuyetDinh),
                        new OracleParameter("p5", $"DELETED_BY={effectiveUserId}, DELETED_DATE={DateTime.Now:yyyy-MM-dd HH:mm:ss}"),
                        new OracleParameter("p6", "TIENLUONG"),
                        new OracleParameter("p7", "STATUS=TRASH")
                    );

                    tx.Commit();
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }
    }
}
