using Bu.CLASS_SYSTEM;
using Bu.DTO;
using DA;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bu
{
    public class RestoreResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    public class PurgeResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    public class RecycleBinService
    {
        private readonly MyEntities _db;

        public RecycleBinService()
        {
            _db = new MyEntities();
        }

        public RecycleBinService(MyEntities db)
        {
            _db = db;
        }

        public List<RecycleBinItemDTO> GetDeletedItems(string objectType = null)
        {
            var results = new List<RecycleBinItemDTO>();

            // 1. Contracts
            if (string.IsNullOrEmpty(objectType) || objectType == "HOPDONG")
            {
                try
                {
                    var contracts = (from hd in _db.TB_HOPDONG
                                     where hd.DEL_DATE != null
                                     join nv in _db.TB_NHANVIEN on hd.MANV equals nv.MANV into nvGroup
                                     from nv in nvGroup.DefaultIfEmpty()
                                     select new
                                     {
                                         hd.SOHD,
                                         hd.MANV,
                                         hd.LUONG_THOA_THUAN,
                                         hd.DEL_DATE,
                                         hd.DEL_BY,
                                         Hoten = nv.HOTEN
                                     }).ToList();

                    foreach (var c in contracts)
                    {
                        bool hasReferences = false;
                        if (c.MANV.HasValue)
                        {
                            decimal manv = c.MANV.Value;
                            string sohd = c.SOHD ?? string.Empty;
                            try
                            {
                                int payrollCount = _db.Database.SqlQuery<int>(
                                    "SELECT COUNT(*) FROM TB_BANGLUONG WHERE MANV = :p0",
                                    new OracleParameter("p0", manv)).FirstOrDefault();

                                int salaryEffectiveCount = _db.Database.SqlQuery<int>(
                                    "SELECT COUNT(*) FROM TB_LUONG_HIEULUC WHERE SOHD = :p0",
                                    new OracleParameter("p0", sohd)).FirstOrDefault();

                                hasReferences = payrollCount > 0 || salaryEffectiveCount > 0;
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"[RecycleBin] Check reference failed for contract {c.SOHD}: {ex.Message}");
                                hasReferences = true; // Safe fallback: do not allow purge if check errors
                            }
                        }

                        results.Add(new RecycleBinItemDTO
                        {
                            ObjectType = "HOPDONG",
                            ObjectId = c.SOHD,
                            DisplayName = "HĐ " + c.SOHD + " (Lương: " + (c.LUONG_THOA_THUAN ?? 0) + " đ)",
                            EmployeeId = c.MANV,
                            EmployeeName = c.Hoten,
                            DeletedDate = c.DEL_DATE,
                            DeletedBy = c.DEL_BY != null ? c.DEL_BY.ToString() : "Hệ thống",
                            Reason = "Đã chuyển vào thùng rác",
                            CanRestore = true,
                            CanPurge = !hasReferences
                        });
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[RecycleBin] Error loading deleted contracts: {ex.Message}");
                }
            }

            // 2. Allowances
            if (string.IsNullOrEmpty(objectType) || objectType == "PHUCAP")
            {
                try
                {
                    var allowances = (from np in _db.TB_NHANVIEN_PHUCAP
                                      where np.DELETED_DATE != null
                                      join nv in _db.TB_NHANVIEN on np.MANV equals nv.MANV into nvGroup
                                      from nv in nvGroup.DefaultIfEmpty()
                                      join pc in _db.TB_PHUCAP on np.IDPC equals pc.IDPC into pcGroup
                                      from pc in pcGroup.DefaultIfEmpty()
                                      select new RecycleBinItemDTO
                                      {
                                          ObjectType = "PHUCAP",
                                          ObjectId = np.MANV + "_" + np.IDPC,
                                          DisplayName = (pc != null ? pc.TENPC : "Phụ cấp " + np.IDPC) + ": " + (np.SOTIEN ?? 0) + " đ",
                                          EmployeeId = np.MANV,
                                          EmployeeName = nv.HOTEN,
                                          DeletedDate = np.DELETED_DATE,
                                          DeletedBy = np.DELETED_BY != null ? np.DELETED_BY.ToString() : "Hệ thống",
                                          Reason = "Phụ cấp đã xóa",
                                          CanRestore = true,
                                          CanPurge = false
                                      }).ToList();
                    results.AddRange(allowances);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[RecycleBin] Error loading deleted allowances: {ex.Message}");
                }
            }

            // 3. Period attendance / KyCong
            if (string.IsNullOrEmpty(objectType) || objectType == "KYCONG")
            {
                try
                {
                    var kycongs = (from kc in _db.TB_KYCONG
                                   where kc.DELETED_DATE != null
                                   select new RecycleBinItemDTO
                                   {
                                       ObjectType = "KYCONG",
                                       ObjectId = kc.MAKYCONG.ToString(),
                                       DisplayName = "Kỳ công " + kc.MAKYCONG + " (Tháng " + kc.THANG + "/" + kc.NAM + ")",
                                       DeletedDate = kc.DELETED_DATE,
                                       DeletedBy = kc.DELETED_BY != null ? kc.DELETED_BY.ToString() : "Hệ thống",
                                       Reason = "Kỳ công đã xóa mềm",
                                       CanRestore = true,
                                       CanPurge = false
                                   }).ToList();
                    results.AddRange(kycongs);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[RecycleBin] Error loading deleted kycongs: {ex.Message}");
                }
            }

            // 4. Advances (TB_UNGLUONG)
            if (string.IsNullOrEmpty(objectType) || objectType == "UNGLUONG")
            {
                try
                {
                    var advances = (from ul in _db.TB_UNGLUONG
                                    where ul.DELETED_DATE != null
                                    join nv in _db.TB_NHANVIEN on ul.MANV equals nv.MANV into nvGroup
                                    from nv in nvGroup.DefaultIfEmpty()
                                    select new RecycleBinItemDTO
                                    {
                                        ObjectType = "UNGLUONG",
                                        ObjectId = ul.IDUL.ToString(),
                                        DisplayName = "Tạm ứng: " + (ul.SOTIENUNG ?? 0) + " đ (Tháng " + ul.THANG + "/" + ul.NAM + ")",
                                        EmployeeId = ul.MANV,
                                        EmployeeName = nv.HOTEN,
                                        DeletedDate = ul.DELETED_DATE,
                                        DeletedBy = ul.DELETED_BY != null ? ul.DELETED_BY.ToString() : "Hệ thống",
                                        Reason = "Phiếu tạm ứng đã xóa",
                                        CanRestore = true,
                                        CanPurge = false
                                    }).ToList();
                    results.AddRange(advances);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[RecycleBin] Error loading deleted advances: {ex.Message}");
                }
            }

            // 5. Period occurrences (TB_KHENTHUONG_KYLUAT)
            if (string.IsNullOrEmpty(objectType) || objectType == "PHATSINH")
            {
                try
                {
                    var occurrences = (from kt in _db.TB_KHENTHUONG_KYLUAT
                                       where kt.DELETED_DATE != null
                                       join nv in _db.TB_NHANVIEN on kt.MANV equals nv.MANV into nvGroup
                                       from nv in nvGroup.DefaultIfEmpty()
                                       select new RecycleBinItemDTO
                                       {
                                           ObjectType = "PHATSINH",
                                           ObjectId = kt.SOQUYETDINH,
                                           DisplayName = (kt.LOAI == 1 ? "Thưởng: " : "Trừ: ") + (kt.NOIDUNG ?? kt.SOQUYETDINH) + " (" + (kt.SOTIEN ?? 0) + " đ)",
                                           EmployeeId = kt.MANV,
                                           EmployeeName = nv.HOTEN,
                                           DeletedDate = kt.DELETED_DATE,
                                           DeletedBy = kt.DELETED_BY != null ? kt.DELETED_BY.ToString() : "Hệ thống",
                                           Reason = "Khoản phát sinh lương đã xóa mềm",
                                           CanRestore = true,
                                           CanPurge = false
                                       }).ToList();
                    results.AddRange(occurrences);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[RecycleBin] Error loading deleted occurrences: {ex.Message}");
                }
            }

            return results.OrderByDescending(x => x.DeletedDate).ToList();
        }

        public RestoreResult Restore(string objectType, string objectId, int userId)
        {
            if (string.IsNullOrEmpty(objectType) || string.IsNullOrEmpty(objectId))
                return new RestoreResult { Success = false, Message = "Dữ liệu đối tượng không hợp lệ." };

            if (userId <= 0)
                return new RestoreResult { Success = false, Message = "Yêu cầu đăng nhập trước khi thực hiện khôi phục." };

            try
            {
                if (objectType == "HOPDONG")
                {
                    var hd = _db.TB_HOPDONG.FirstOrDefault(x => x.SOHD == objectId);
                    if (hd == null)
                        return new RestoreResult { Success = false, Message = "Không tìm thấy hợp đồng: " + objectId };

                    // Kiểm tra chồng lấn với hợp đồng đang hoạt động
                    if (hd.MANV.HasValue)
                    {
                        var activeHd = _db.TB_HOPDONG.FirstOrDefault(x => x.MANV == hd.MANV.Value 
                            && x.SOHD != hd.SOHD 
                            && x.DEL_DATE == null
                            && x.NGAYBATDAU <= (hd.NGAYKETTHUC ?? DateTime.MaxValue)
                            && (x.NGAYKETTHUC == null || x.NGAYKETTHUC >= (hd.NGAYBATDAU ?? DateTime.MinValue)));

                        if (activeHd != null)
                        {
                            return new RestoreResult
                            {
                                Success = false,
                                Message = $"Không thể khôi phục vì nhân viên đã có hợp đồng hoạt động ({activeHd.SOHD}) chồng lấn thời gian!"
                            };
                        }
                    }

                    hd.DEL_DATE = null;
                    hd.DEL_BY = null;
                    hd.UPDATE_BY = userId;
                    hd.UPDATE_DATE = DateTime.Now;
                    _db.SaveChanges();

                    RecordPersistentAudit(userId, "KHOI_PHUC", "TB_HOPDONG", objectId, "Khôi phục hợp đồng từ Thùng rác");
                    return new RestoreResult { Success = true, Message = "Khôi phục hợp đồng " + objectId + " thành công!" };
                }
                else if (objectType == "PHUCAP")
                {
                    var parts = objectId.Split('_');
                    if (parts.Length != 2 || !decimal.TryParse(parts[0], out decimal manv) || !decimal.TryParse(parts[1], out decimal idpc))
                        return new RestoreResult { Success = false, Message = "Mã phụ cấp không hợp lệ." };

                    var pc = _db.TB_NHANVIEN_PHUCAP.FirstOrDefault(x => x.MANV == manv && x.IDPC == idpc);
                    if (pc == null)
                        return new RestoreResult { Success = false, Message = "Không tìm thấy bản ghi phụ cấp." };

                    pc.DELETED_DATE = null;
                    pc.DELETED_BY = null;
                    pc.UPDATED_BY = userId;
                    pc.UPDATED_DATE = DateTime.Now;
                    _db.SaveChanges();

                    RecordPersistentAudit(userId, "KHOI_PHUC", "TB_NHANVIEN_PHUCAP", objectId, "Khôi phục phụ cấp từ Thùng rác");
                    return new RestoreResult { Success = true, Message = "Khôi phục phụ cấp thành công!" };
                }
                else if (objectType == "KYCONG")
                {
                    if (!decimal.TryParse(objectId, out decimal makycong))
                        return new RestoreResult { Success = false, Message = "Mã kỳ công không hợp lệ." };

                    var kc = _db.TB_KYCONG.FirstOrDefault(x => x.MAKYCONG == makycong);
                    if (kc == null)
                        return new RestoreResult { Success = false, Message = "Không tìm thấy kỳ công." };

                    kc.DELETED_DATE = null;
                    kc.DELETED_BY = null;
                    kc.UPDATED_BY = userId;
                    kc.UPDATED_DATE = DateTime.Now;
                    _db.SaveChanges();

                    RecordPersistentAudit(userId, "KHOI_PHUC", "TB_KYCONG", objectId, "Khôi phục kỳ công từ Thùng rác");
                    return new RestoreResult { Success = true, Message = "Khôi phục kỳ công " + makycong + " thành công!" };
                }
                else if (objectType == "UNGLUONG")
                {
                    if (!int.TryParse(objectId, out int idul))
                        return new RestoreResult { Success = false, Message = "Mã phiếu tạm ứng không hợp lệ." };

                    var ul = _db.TB_UNGLUONG.FirstOrDefault(x => x.IDUL == idul);
                    if (ul == null)
                        return new RestoreResult { Success = false, Message = "Không tìm thấy phiếu tạm ứng." };

                    ul.DELETED_DATE = null;
                    ul.DELETED_BY = null;
                    ul.UPDATED_BY = userId;
                    ul.UPDATED_DATE = DateTime.Now;
                    _db.SaveChanges();

                    RecordPersistentAudit(userId, "KHOI_PHUC", "TB_UNGLUONG", objectId, "Khôi phục phiếu tạm ứng từ Thùng rác");
                    return new RestoreResult { Success = true, Message = "Khôi phục phiếu tạm ứng " + idul + " thành công!" };
                }
                else if (objectType == "PHATSINH")
                {
                    var kt = _db.TB_KHENTHUONG_KYLUAT.FirstOrDefault(x => x.SOQUYETDINH == objectId);
                    if (kt == null)
                        return new RestoreResult { Success = false, Message = "Không tìm thấy khoản phát sinh mã: " + objectId };

                    kt.DELETED_DATE = null;
                    kt.DELETED_BY = null;
                    kt.UPDATED_BY = null; // Revert to draft
                    kt.UPDATED_DATE = null; // Revert to draft
                    _db.SaveChanges();

                    RecordPersistentAudit(userId, "KHOI_PHUC", "TB_KHENTHUONG_KYLUAT", objectId, "Khôi phục khoản phát sinh từ Thùng rác về trạng thái Bản nháp");
                    return new RestoreResult { Success = true, Message = "Khôi phục khoản phát sinh " + objectId + " thành công (ở trạng thái Bản nháp)." };
                }

                return new RestoreResult { Success = false, Message = "Loại đối tượng chưa được hỗ trợ khôi phục: " + objectType };
            }
            catch (Exception ex)
            {
                return new RestoreResult { Success = false, Message = "Lỗi khi khôi phục: " + ex.Message };
            }
        }

        public PurgeResult Purge(string objectType, string objectId, int userId)
        {
            if (string.IsNullOrEmpty(objectId))
                return new PurgeResult { Success = false, Message = "Mã đối tượng không hợp lệ." };

            if (objectType != "HOPDONG")
                return new PurgeResult { Success = false, Message = "Chỉ cho phép xóa vĩnh viễn hợp đồng nháp chưa sử dụng theo chính sách bảo toàn dữ liệu." };

            // Xác minh quyền thực tế theo cơ chế phân quyền của dự án
            if (userId <= 0 || (!UserSession.IsAdmin && !UserSession.CanDelete("F_SYSTEM_PURGE")))
            {
                return new PurgeResult
                {
                    Success = false,
                    Message = "Từ chối truy cập: Bạn không có quyền xóa vĩnh viễn dữ liệu (yêu cầu quyền quản trị viên hoặc quyền F_SYSTEM_PURGE)."
                };
            }

            using (var tx = _db.Database.BeginTransaction())
            {
                try
                {
                    // 1. Lock and inspect contract inside transaction
                    var hd = _db.TB_HOPDONG.SqlQuery(
                        "SELECT * FROM TB_HOPDONG WHERE SOHD = :p0 FOR UPDATE",
                        new OracleParameter("p0", objectId)
                    ).FirstOrDefault();

                    if (hd == null)
                        return new PurgeResult { Success = false, Message = "Không tìm thấy hợp đồng: " + objectId };

                    if (hd.DEL_DATE == null)
                        return new PurgeResult { Success = false, Message = "Đối tượng phải ở trong Thùng rác mới được phép xóa vĩnh viễn." };

                    // Kiểm tra hợp đồng đã ký chính thức chưa
                    if (hd.NGAYKY.HasValue)
                    {
                        return new PurgeResult
                        {
                            Success = false,
                            Message = "Hợp đồng đã có ngày ký chính thức, không được phép xóa vĩnh viễn (chỉ được lưu trữ lịch sử trong Thùng rác)!"
                        };
                    }

                    // 2. Kiểm tra dữ liệu tính lương phát sinh (TB_BANGLUONG)
                    if (hd.MANV.HasValue)
                    {
                        var hasPayroll = _db.Database.SqlQuery<int>(@"
                            SELECT COUNT(*) FROM TB_BANGLUONG
                            WHERE MANV = :p0",
                            new OracleParameter("p0", hd.MANV.Value)
                        ).FirstOrDefault();

                        if (hasPayroll > 0)
                        {
                            return new PurgeResult
                            {
                                Success = false,
                                Message = "Hợp đồng đã phát sinh dữ liệu tính lương (TB_BANGLUONG), bị chặn xóa vĩnh viễn theo quy định bảo toàn dữ liệu!"
                            };
                        }

                        // 3. Kiểm tra dữ liệu mức lương hiệu lực (TB_LUONG_HIEULUC)
                        var hasEffectiveSalary = _db.Database.SqlQuery<int>(@"
                            SELECT COUNT(*) FROM TB_LUONG_HIEULUC
                            WHERE SOHD = :p0",
                            new OracleParameter("p0", objectId)
                        ).FirstOrDefault();

                        if (hasEffectiveSalary > 0)
                        {
                            return new PurgeResult
                            {
                                Success = false,
                                Message = "Hợp đồng đã có lịch sử mức lương hiệu lực (TB_LUONG_HIEULUC), bị chặn xóa vĩnh viễn."
                            };
                        }

                        // 4. Kiểm tra chấm công chi tiết (TB_BANGCONG_CHITIET)
                        var hasAttendance = _db.Database.SqlQuery<int>(@"
                            SELECT COUNT(*) FROM TB_BANGCONG_CHITIET
                            WHERE MANV = :p0",
                            new OracleParameter("p0", hd.MANV.Value)
                        ).FirstOrDefault();

                        if (hasAttendance > 0)
                        {
                            return new PurgeResult
                            {
                                Success = false,
                                Message = "Nhân viên của hợp đồng đã có dữ liệu chấm công chi tiết, bị chặn xóa vĩnh viễn."
                            };
                        }
                    }

                    // 5. Xóa hợp đồng nháp không sử dụng DELETE CASCADE
                    _db.Database.ExecuteSqlCommand("DELETE FROM TB_HOPDONG WHERE SOHD = :p0", new OracleParameter("p0", objectId));

                    // 6. Ghi persistent audit log vào TB_SYS_LOG
                    string auditUser = UserSession.CurrentUser?.FULLNAME ?? ("User " + userId);
                    _db.Database.ExecuteSqlCommand(@"
                        INSERT INTO TB_SYS_LOG (
                            MANV_THUCHIEN, TEN_THUCHIEN, HANHDONG, TEN_BANG, ID_BAN_GHI,
                            DU_LIEU_CU, THOIGIAN, MODULE_NAME, CHANGED_FIELDS
                        ) VALUES (
                            :p0, :p1, :p2, :p3, :p4,
                            :p5, CURRENT_TIMESTAMP, :p6, :p7
                        )",
                        new OracleParameter("p0", userId),
                        new OracleParameter("p1", auditUser),
                        new OracleParameter("p2", "PURGE_VINH_VIEN"),
                        new OracleParameter("p3", "TB_HOPDONG"),
                        new OracleParameter("p4", objectId),
                        new OracleParameter("p5", $"PURGED_CONTRACT: SOHD={objectId}, MANV={hd.MANV}, LUONG={hd.LUONG_THOA_THUAN}"),
                        new OracleParameter("p6", "THUNGRAC"),
                        new OracleParameter("p7", "STATUS=PURGED_PERMANENT")
                    );

                    tx.Commit();
                    return new PurgeResult { Success = true, Message = "Đã xóa vĩnh viễn hợp đồng nháp " + objectId + " khỏi hệ thống." };
                }
                catch (Exception ex)
                {
                    tx.Rollback();
                    System.Diagnostics.Debug.WriteLine($"[RecycleBin] Purge failed: {ex.Message}");
                    return new PurgeResult { Success = false, Message = "Lỗi khi xóa vĩnh viễn: " + ex.Message };
                }
            }
        }

        private void RecordPersistentAudit(int userId, string action, string tableName, string recordId, string description)
        {
            try
            {
                string auditUser = UserSession.CurrentUser?.FULLNAME ?? ("User " + userId);
                _db.Database.ExecuteSqlCommand(@"
                    INSERT INTO TB_SYS_LOG (
                        MANV_THUCHIEN, TEN_THUCHIEN, HANHDONG, TEN_BANG, ID_BAN_GHI,
                        DU_LIEU_MOI, THOIGIAN, MODULE_NAME, CHANGED_FIELDS
                    ) VALUES (
                        :p0, :p1, :p2, :p3, :p4,
                        :p5, CURRENT_TIMESTAMP, :p6, :p7
                    )",
                    new OracleParameter("p0", userId),
                    new OracleParameter("p1", auditUser),
                    new OracleParameter("p2", action),
                    new OracleParameter("p3", tableName),
                    new OracleParameter("p4", recordId),
                    new OracleParameter("p5", description),
                    new OracleParameter("p6", "THUNGRAC"),
                    new OracleParameter("p7", description)
                );
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[RecycleBin] Audit log error: {ex.Message}");
            }
        }
    }
}
