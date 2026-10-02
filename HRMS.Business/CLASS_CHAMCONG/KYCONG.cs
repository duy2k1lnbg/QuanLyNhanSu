using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Bu.CLASS_SYSTEM;
using DA;

namespace Bu.CLASS_CHAMCONG
{
    public class KYCONG
    {
        private readonly Func<MyEntities> _contextFactory;

        public KYCONG()
        {
            _contextFactory = () => new MyEntities();
        }

        public KYCONG(Func<MyEntities> contextFactory)
        {
            _contextFactory = contextFactory ?? (() => new MyEntities());
        }

        private MyEntities CreateContext() => _contextFactory();

        public TB_KYCONG getItem(int id)
        {
            using (var db = CreateContext())
            {
                return db.TB_KYCONG.FirstOrDefault(x => x.MAKYCONG == id && x.DELETED_DATE == null);
            }
        }

        public TB_KYCONG getItemIncludingDeleted(int id)
        {
            using (var db = CreateContext())
            {
                return db.TB_KYCONG.FirstOrDefault(x => x.MAKYCONG == id);
            }
        }

        public List<TB_KYCONG> getList()
        {
            using (var db = CreateContext())
            {
                return db.TB_KYCONG.Where(x => x.DELETED_DATE == null).OrderByDescending(x => x.MAKYCONG).ToList();
            }
        }

        public int KiemTraMaKyCong(int makycong)
        {
            using (var db = CreateContext())
            {
                return db.TB_KYCONG.Any(x => x.MAKYCONG == makycong && x.DELETED_DATE == null) ? 1 : 0;
            }
        }

        public int KiemTraPhatSinhKyCong(int makycong)
        {
            using (var db = CreateContext())
            {
                var kc = db.TB_KYCONG.FirstOrDefault(x => x.MAKYCONG == makycong && x.DELETED_DATE == null);
                if (kc == null) return 0;
                return (kc.TRANGTHAI ?? 0) == 1 ? 1 : 0;
            }
        }

        public TB_KYCONG Add(TB_KYCONG kc)
        {
            if (kc == null)
                throw new BusinessException("VALIDATION_FAILED", "Thông tin kỳ công không được để trống.");

            int thang = (int)(kc.THANG ?? 0);
            int nam = (int)(kc.NAM ?? 0);
            if (thang < 1 || thang > 12)
                throw new BusinessException("VALIDATION_FAILED", "Tháng làm việc không hợp lệ (phải từ 1 đến 12).", "THANG");
            if (nam < 2000 || nam > 2100)
                throw new BusinessException("VALIDATION_FAILED", "Năm làm việc không hợp lệ (từ 2000 đến 2100).", "NAM");

            int expectedMakycong = nam * 100 + thang;
            if (kc.MAKYCONG <= 0)
            {
                kc.MAKYCONG = expectedMakycong;
            }

            using (var db = CreateContext())
            {
                var existing = db.TB_KYCONG.AsNoTracking().FirstOrDefault(x => x.MAKYCONG == kc.MAKYCONG);
                if (existing != null)
                {
                    if (existing.DELETED_DATE != null)
                    {
                        throw new BusinessException("PERIOD_IN_RECYCLE_BIN", 
                            $"Kỳ công tháng {thang}/{nam} (mã {kc.MAKYCONG}) đang trong Thùng rác. Vui lòng kiểm tra và khôi phục từ Thùng rác thay vì tạo mới.");
                    }
                    else
                    {
                        throw new BusinessException("PERIOD_ALREADY_EXISTS", 
                            $"Kỳ công tháng {thang}/{nam} (mã {kc.MAKYCONG}) đã tồn tại trong hệ thống. Vui lòng mở kỳ hiện có.");
                    }
                }
            }

            int userId = (int)(kc.CREATED_BY ?? UserSession.CurrentUser?.IDUSER ?? 0);
            if (userId <= 0 && UserSession.CurrentUser == null && !kc.CREATED_BY.HasValue)
            {
                throw new BusinessException("UNAUTHENTICATED", "Yêu cầu phiên đăng nhập hợp lệ để tạo kỳ công.");
            }

            if (UserSession.CurrentUser != null && !UserSession.IsAdmin && !UserSession.CanAdd("F_CC_BANGCONG"))
            {
                throw new BusinessException("PERMISSION_DENIED", "Bạn không có quyền thêm mới kỳ công. Vui lòng liên hệ quản trị viên.");
            }

            // Brand new period cannot be created in locked state
            kc.KHOA = 0;
            kc.TRANGTHAI = 0;

            using (var db = CreateContext())
            {

                if (!kc.CREATED_DATE.HasValue) kc.CREATED_DATE = DateTime.Now;
                if (!kc.NGAYTINHCONG.HasValue) kc.NGAYTINHCONG = DateTime.Now;
                if (!kc.CREATED_BY.HasValue && userId > 0) kc.CREATED_BY = userId;

                try
                {
                    db.TB_KYCONG.Add(kc);
                    db.SaveChanges();
                    return kc;
                }
                catch (Exception ex)
                {
                    string fullEx = ex.ToString();
                    if (fullEx.IndexOf("ORA-00001", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        throw new BusinessException("PERIOD_ALREADY_EXISTS", 
                            $"Kỳ công tháng {thang}/{nam} đã tồn tại do có phiên thao tác đồng thời. Vui lòng mở kỳ hiện có.", null, ex);
                    }
                    throw new BusinessException("DATABASE_ERROR", ErrorHelper.ResolveUserFriendlyMessage(ex, "thêm kỳ công"), null, ex);
                }
            }
        }

        public TB_KYCONG Update(TB_KYCONG kc)
        {
            if (kc == null)
                throw new BusinessException("VALIDATION_FAILED", "Thông tin kỳ công không được để trống.");

            int userId = (int)(kc.UPDATED_BY ?? UserSession.CurrentUser?.IDUSER ?? 0);
            if (userId <= 0 && UserSession.CurrentUser == null && !kc.UPDATED_BY.HasValue)
            {
                throw new BusinessException("UNAUTHENTICATED", "Yêu cầu phiên đăng nhập hợp lệ để cập nhật kỳ công.");
            }

            if (UserSession.CurrentUser != null && !UserSession.IsAdmin && !UserSession.CanEdit("F_CC_BANGCONG"))
            {
                throw new BusinessException("PERMISSION_DENIED", "Bạn không có quyền cập nhật kỳ công. Vui lòng liên hệ quản trị viên.");
            }

            int thang = (int)(kc.THANG ?? 0);
            int nam = (int)(kc.NAM ?? 0);
            if (thang < 1 || thang > 12)
                throw new BusinessException("VALIDATION_FAILED", "Tháng làm việc không hợp lệ (phải từ 1 đến 12).", "THANG");
            if (nam < 2000 || nam > 2100)
                throw new BusinessException("VALIDATION_FAILED", "Năm làm việc không hợp lệ (từ 2000 đến 2100).", "NAM");

            using (var db = CreateContext())
            using (var tx = db.Database.BeginTransaction())
            {
                try
                {
                    db.Database.SqlQuery<decimal>("SELECT NVL(KHOA,0) FROM TB_KYCONG WHERE MAKYCONG=:p0 FOR UPDATE",
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p0", kc.MAKYCONG)).SingleOrDefault();

                    var current = db.TB_KYCONG.FirstOrDefault(x => x.MAKYCONG == kc.MAKYCONG);
                    if (current == null)
                        throw new BusinessException("NOT_FOUND", $"Không tìm thấy kỳ công {kc.MAKYCONG}.");

                    // Locked period modification guard
                    if ((current.KHOA ?? 0) == 1)
                    {
                        throw new BusinessException("PERIOD_LOCKED", 
                            $"Kỳ công {kc.MAKYCONG} đã bị khóa sổ. Không thể sửa thông tin của kỳ đã khóa. Vui lòng mở khóa kỳ công trước khi chỉnh sửa.");
                    }

                    // Preserve lock and status - cannot be changed via standard Update
                    current.NAM = kc.NAM;
                    current.THANG = kc.THANG;
                    current.NGAYTINHCONG = kc.NGAYTINHCONG;
                    current.NGAYCONGTRONGTHANG = kc.NGAYCONGTRONGTHANG;
                    current.UPDATED_BY = userId > 0 ? userId : current.UPDATED_BY;
                    current.UPDATED_DATE = DateTime.Now;

                    db.SaveChanges();
                    tx.Commit();
                    return current;
                }
                catch (BusinessException)
                {
                    tx.Rollback();
                    throw;
                }
                catch (Exception ex)
                {
                    tx.Rollback();
                    throw new BusinessException("DATABASE_ERROR", ErrorHelper.ResolveUserFriendlyMessage(ex, "cập nhật kỳ công"), null, ex);
                }
            }
        }

        public void LockKyCong(int makycong, int iduser, string ghiChu = null)
        {
            if (iduser <= 0 && UserSession.CurrentUser == null)
                throw new BusinessException("UNAUTHENTICATED", "Yêu cầu phiên đăng nhập hợp lệ để khóa kỳ công.");

            if (UserSession.CurrentUser != null && !UserSession.IsAdmin && !UserSession.CanEdit("F_CC_BANGCONG"))
                throw new BusinessException("PERMISSION_DENIED", "Bạn không có quyền khóa sổ kỳ công. Vui lòng liên hệ quản trị viên.");

            using (var db = CreateContext())
            using (var tx = db.Database.BeginTransaction())
            {
                try
                {
                    db.Database.SqlQuery<decimal>("SELECT NVL(KHOA,0) FROM TB_KYCONG WHERE MAKYCONG=:p0 FOR UPDATE",
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p0", makycong)).SingleOrDefault();

                    var current = db.TB_KYCONG.FirstOrDefault(x => x.MAKYCONG == makycong);
                    if (current == null) throw new BusinessException("NOT_FOUND", $"Không tìm thấy kỳ công {makycong}.");

                    if ((current.KHOA ?? 0) == 1)
                        throw new BusinessException("PERIOD_LOCKED", $"Kỳ công {makycong} hiện đã ở trạng thái khóa.");

                    // Check readiness
                    var counts = db.Database.SqlQuery<LockReadiness>(@"
                        SELECT COUNT(*) TOTAL,
                          NVL(SUM(CASE WHEN b.LANTINH_ID_HIENHANH IS NULL OR NVL(b.DU_DIEUKIEN_CHOT,0)<>1
                            OR r.INPUT_REV<>k.CONG_INPUT_REV THEN 1 ELSE 0 END),0) BLOCKED
                        FROM TB_BANGCONG_CHITIET b JOIN TB_KYCONG k ON k.MAKYCONG=b.MAKYCONG
                        LEFT JOIN TB_CHAMCONG_LANTINH r ON r.IDLANTINH=b.LANTINH_ID_HIENHANH
                        WHERE b.MAKYCONG=:p0",
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p0", makycong)).SingleOrDefault() ?? new LockReadiness();

                    if (counts.TOTAL > 0 && counts.BLOCKED > 0)
                        throw new BusinessException("LOCK_NOT_READY", "Chưa thể khóa kỳ: còn dữ liệu công chưa tính toán đầy đủ hoặc có thay đổi đầu vào.");

                    int y = makycong / 100;
                    int m = makycong % 100;
                    var fromDate = new DateTime(y, m, 1);
                    var toDate = fromDate.AddMonths(1).AddDays(-1);

                    var blockingAnomalies = db.Database.SqlQuery<decimal>(@"
                        SELECT COUNT(*) FROM TB_CHAMCONG_BATTHUONG
                        WHERE NGAY >= :p_from AND NGAY <= :p_to
                          AND CHAN_CHOT = 1 AND TRANG_THAI = 'CHO_XU_LY'",
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p_from", fromDate),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p_to", toDate)).SingleOrDefault();

                    if (blockingAnomalies > 0)
                        throw new BusinessException("LOCK_NOT_READY", $"Chưa thể khóa kỳ: còn {blockingAnomalies} bất thường chấm công chặn chốt đang chờ xử lý.");

                    current.KHOA = 1;
                    current.TRANGTHAI = 1;
                    current.NGAYTINHCONG = DateTime.Now;
                    current.UPDATED_BY = iduser;
                    current.UPDATED_DATE = DateTime.Now;

                    db.SaveChanges();
                    tx.Commit();
                }
                catch (BusinessException)
                {
                    tx.Rollback();
                    throw;
                }
                catch (Exception ex)
                {
                    tx.Rollback();
                    throw new BusinessException("DATABASE_ERROR", ErrorHelper.ResolveUserFriendlyMessage(ex, "khóa kỳ công"), null, ex);
                }
            }
        }

        public void UnlockKyCong(int makycong, int iduser, string lyDo)
        {
            if (iduser <= 0 && UserSession.CurrentUser == null)
                throw new BusinessException("UNAUTHENTICATED", "Yêu cầu phiên đăng nhập hợp lệ để mở khóa kỳ công.");

            if (UserSession.CurrentUser != null && !UserSession.IsAdmin && !UserSession.CanEdit("F_CC_BANGCONG"))
                throw new BusinessException("PERMISSION_DENIED", "Bạn không có quyền mở khóa kỳ công. Vui lòng liên hệ quản trị viên.");

            if (string.IsNullOrWhiteSpace(lyDo))
                throw new BusinessException("MISSING_UNLOCK_REASON", "Vui lòng nhập lý do mở khóa kỳ công.", "LYDO");

            using (var db = CreateContext())
            using (var tx = db.Database.BeginTransaction())
            {
                try
                {
                    db.Database.SqlQuery<decimal>("SELECT NVL(KHOA,0) FROM TB_KYCONG WHERE MAKYCONG=:p0 FOR UPDATE",
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p0", makycong)).SingleOrDefault();

                    var current = db.TB_KYCONG.FirstOrDefault(x => x.MAKYCONG == makycong);
                    if (current == null) throw new BusinessException("NOT_FOUND", $"Không tìm thấy kỳ công {makycong}.");

                    if ((current.KHOA ?? 0) != 1)
                        throw new BusinessException("PERIOD_NOT_LOCKED", $"Kỳ công {makycong} hiện đang không ở trạng thái khóa.");

                    current.KHOA = 0;
                    current.UPDATED_BY = iduser;
                    current.UPDATED_DATE = DateTime.Now;

                    db.SaveChanges();
                    tx.Commit();
                }
                catch (BusinessException)
                {
                    tx.Rollback();
                    throw;
                }
                catch (Exception ex)
                {
                    tx.Rollback();
                    throw new BusinessException("DATABASE_ERROR", ErrorHelper.ResolveUserFriendlyMessage(ex, "mở khóa kỳ công"), null, ex);
                }
            }
        }

        private class LockReadiness
        {
            public decimal TOTAL { get; set; }
            public decimal BLOCKED { get; set; }
        }

        public void Delete(int makycong, int iduser)
        {
            if (iduser <= 0 && UserSession.CurrentUser == null)
                throw new BusinessException("UNAUTHENTICATED", "Yêu cầu phiên đăng nhập hợp lệ để xóa kỳ công.");

            if (UserSession.CurrentUser != null && !UserSession.IsAdmin && !UserSession.CanDelete("F_CC_BANGCONG"))
                throw new BusinessException("PERMISSION_DENIED", "Bạn không có quyền xóa kỳ công. Vui lòng liên hệ quản trị viên.");

            using (var db = CreateContext())
            {
                var kc = db.TB_KYCONG.FirstOrDefault(x => x.MAKYCONG == makycong);
                if (kc == null)
                    throw new BusinessException("NOT_FOUND", $"Không tìm thấy kỳ công {makycong}.");

                if (kc.DELETED_DATE != null)
                    throw new BusinessException("ALREADY_DELETED", $"Kỳ công {makycong} đã ở trong Thùng rác.");

                if ((kc.KHOA ?? 0) == 1)
                    throw new BusinessException("PERIOD_LOCKED", $"Kỳ công {makycong} đã bị khóa sổ. Không thể xóa kỳ công đang khóa.");

                kc.DELETED_DATE = DateTime.Now;
                kc.DELETED_BY = iduser;
                db.SaveChanges();
            }
        }
    }
}
