using DA;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bu.CLASS_CHAMCONG
{
    public class KYCONGCHITIET
    {
        MyEntities db = new MyEntities();

        public TB_KYCONGCHITIET getItem(int makycong, int manv)
        {
            return db.TB_KYCONGCHITIET.FirstOrDefault(x => x.MAKYCONG == makycong && x.MANV == manv);
        }

        public List<TB_KYCONGCHITIET> getList(int makycong)
        {
            return db.TB_KYCONGCHITIET.Where(x => x.MAKYCONG == makycong).ToList();
        }
        public void phatSinhKyCongChiTiet(int macty, int thang, int nam, int iduser)
        {
            phatSinhKyCongChiTiet(macty, thang, nam, iduser, null);
        }

        public void phatSinhKyCongChiTiet(int macty, int thang, int nam, int iduser, Action<int, int, string> progress)
        {
            try
            {
                progress?.Invoke(0, 100, "Đang kiểm tra thời hạn hợp đồng nhân sự...");
                NHANVIEN nhanvienBus = new NHANVIEN();
                var lstNV = nhanvienBus.GetEligibleEmployeesForPeriod(nam, thang, macty > 0 ? (int?)macty : null);
                if (lstNV == null || lstNV.Count == 0) return;

                // Lọc bỏ trùng lặp MANV nếu có để đảm bảo tính duy nhất của khóa chính (MAKYCONG, MANV)
                var distinctNV = lstNV.GroupBy(x => x.MANV).Select(g => g.First()).ToList();

                int makycong = nam * 100 + thang;

                // Chuẩn bị danh sách ký hiệu các ngày trong tháng (tính 1 lần dùng chung)
                List<string> listDay = new List<string>();
                int daysInMonth = GetDayNumber(thang, nam);
                NGAYLE ngayLeBus = new NGAYLE();
                for (int j = 1; j <= daysInMonth; j++)
                {
                    DateTime newDate = new DateTime(nam, thang, j);
                    int loaiCong = ngayLeBus.XacDinhLoaiCong(newDate);
                    if (loaiCong == 3)
                    {
                        listDay.Add("L"); // Ngày lễ
                    }
                    else if (loaiCong == 2)
                    {
                        listDay.Add("CN"); // Chủ nhật
                    }
                    else
                    {
                        listDay.Add("X"); // Ngày thường
                    }
                }
                while (listDay.Count < 31)
                {
                    listDay.Add("");
                }

                double soNgayLamViec = GetData_Functions.demSoNgayLamViecTrongThang(thang, nam);
                DateTime now = DateTime.Now;

                // Sử dụng một DbContext mới, độc lập để tránh xung đột với các thực thể cũ đang được track trong ObjectStateManager
                using (var cleanDb = new MyEntities())
                {
                    // 1. Kiểm tra kỳ công có bị khóa hay không
                    var kc = cleanDb.TB_KYCONG.FirstOrDefault(x => x.MAKYCONG == makycong);
                    if (kc != null && (kc.KHOA ?? 0) == 1)
                    {
                        throw new InvalidOperationException($"Kỳ công {makycong} đã bị khóa (KHOA = 1), không thể phát sinh lại.");
                    }

                    // Tải các bản ghi đã tồn tại để thực hiện non-destructive UPSERT thay vì DELETE
                    var existingList = cleanDb.TB_KYCONGCHITIET.Where(x => x.MAKYCONG == makycong).ToList();
                    var existingMap = existingList.ToDictionary(x => x.MANV);

                    int totalNV = distinctNV.Count;
                    int currentNV = 0;

                    foreach (var item in distinctNV)
                    {
                        if (existingMap.TryGetValue(item.MANV, out var existing))
                        {
                            existing.HOTEN = item.HOTEN;
                            existing.IDCTY = item.IDCTY;
                            existing.D1 = listDay[0];
                            existing.D2 = listDay[1];
                            existing.D3 = listDay[2];
                            existing.D4 = listDay[3];
                            existing.D5 = listDay[4];
                            existing.D6 = listDay[5];
                            existing.D7 = listDay[6];
                            existing.D8 = listDay[7];
                            existing.D9 = listDay[8];
                            existing.D10 = listDay[9];
                            existing.D11 = listDay[10];
                            existing.D12 = listDay[11];
                            existing.D13 = listDay[12];
                            existing.D14 = listDay[13];
                            existing.D15 = listDay[14];
                            existing.D16 = listDay[15];
                            existing.D17 = listDay[16];
                            existing.D18 = listDay[17];
                            existing.D19 = listDay[18];
                            existing.D20 = listDay[19];
                            existing.D21 = listDay[20];
                            existing.D22 = listDay[21];
                            existing.D23 = listDay[22];
                            existing.D24 = listDay[23];
                            existing.D25 = listDay[24];
                            existing.D26 = listDay[25];
                            existing.D27 = listDay[26];
                            existing.D28 = listDay[27];
                            existing.D29 = listDay[28];
                            existing.D30 = listDay[29];
                            existing.D31 = listDay[30];
                            existing.NGAYCONG = (decimal)soNgayLamViec;
                            existing.TONGNGAYCONG = (decimal)soNgayLamViec;
                            existing.UPDATED_BY = iduser;
                            existing.UPDATED_DATE = now;
                        }
                        else
                        {
                            TB_KYCONGCHITIET kycongchitiet = new TB_KYCONGCHITIET
                            {
                                MAKYCONG = makycong,
                                MANV = item.MANV,
                                HOTEN = item.HOTEN,
                                IDCTY = item.IDCTY,
                                D1 = listDay[0],
                                D2 = listDay[1],
                                D3 = listDay[2],
                                D4 = listDay[3],
                                D5 = listDay[4],
                                D6 = listDay[5],
                                D7 = listDay[6],
                                D8 = listDay[7],
                                D9 = listDay[8],
                                D10 = listDay[9],
                                D11 = listDay[10],
                                D12 = listDay[11],
                                D13 = listDay[12],
                                D14 = listDay[13],
                                D15 = listDay[14],
                                D16 = listDay[15],
                                D17 = listDay[16],
                                D18 = listDay[17],
                                D19 = listDay[18],
                                D20 = listDay[19],
                                D21 = listDay[20],
                                D22 = listDay[21],
                                D23 = listDay[22],
                                D24 = listDay[23],
                                D25 = listDay[24],
                                D26 = listDay[25],
                                D27 = listDay[26],
                                D28 = listDay[27],
                                D29 = listDay[28],
                                D30 = listDay[29],
                                D31 = listDay[30],
                                NGAYCONG = (decimal)soNgayLamViec,
                                TONGNGAYCONG = (decimal)soNgayLamViec,
                                CREATED_BY = iduser,
                                CREATED_DATE = now
                            };
                            cleanDb.TB_KYCONGCHITIET.Add(kycongchitiet);
                        }

                        currentNV++;

                        if (currentNV % 100 == 0 || currentNV == totalNV)
                        {
                            int percent = (int)((double)currentNV / totalNV * 40); // 0% - 40%
                            progress?.Invoke(percent, 100, $"Đang tạo kỳ công chi tiết: {currentNV}/{totalNV} nhân viên");
                        }
                    }

                    progress?.Invoke(40, 100, "Đang lưu dữ liệu kỳ công vào CSDL...");
                    cleanDb.SaveChanges();
                }

                // Làm mới lại db context của đối tượng hiện tại để các truy vấn sau luôn thấy dữ liệu mới nhất
                this.db = new MyEntities();
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi phát sinh kỳ công: " + ex.Message);
            }
        }

        /// <summary>
        /// Phát sinh toàn diện kỳ công & bảng công chi tiết theo chuẩn ACID (Transaction ReadCommitted).
        /// Đảm bảo tính nguyên tử tuyệt đối (All-or-Nothing): Nếu có bất kỳ lỗi nào, toàn bộ dữ liệu kỳ công cũ được giữ nguyên,
        /// không bao giờ xảy ra tình trạng dữ liệu dở dang hoặc lỗi trùng khóa chính.
        /// CHỈ DÀNH CHO BẢN DESKTOP NỘI BỘ (Không mở ra Web API public).
        /// </summary>
        public void PhatSinhToanBoKyCongVaBangCong(int macty, int thang, int nam, int iduser, Action<int, int, string> progress = null, bool tuPhatSinhBangCong = true)
        {
            using (var cleanDb = new MyEntities())
            {
                using (var trans = cleanDb.Database.BeginTransaction(System.Data.IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        progress?.Invoke(10, 100, "Đang kiểm tra thời hạn hợp đồng nhân sự...");
                        NHANVIEN nhanvienBus = new NHANVIEN();
                        var lstNV = nhanvienBus.GetEligibleEmployeesForPeriod(nam, thang, macty > 0 ? (int?)macty : null);
                        if (lstNV == null || lstNV.Count == 0)
                        {
                            trans.Rollback();
                            return;
                        }

                        // Lọc bỏ trùng lặp MANV nếu có để đảm bảo tính duy nhất của khóa chính (MAKYCONG, MANV)
                        var distinctNV = lstNV.GroupBy(x => x.MANV).Select(g => g.First()).ToList();
                        int makycong = nam * 100 + thang;

                        // Kiểm tra kỳ công có bị khóa hay không
                        var kcCheck = cleanDb.TB_KYCONG.FirstOrDefault(x => x.MAKYCONG == makycong);
                        if (kcCheck != null && (kcCheck.KHOA ?? 0) == 1)
                        {
                            trans.Rollback();
                            throw new InvalidOperationException($"Kỳ công {makycong} đã bị khóa (KHOA = 1), không thể phát sinh lại.");
                        }

                        // Tự động phát sinh bảng chấm công gốc TB_BANGCONG nếu chưa có máy chấm công
                        BANGCONG_NV_CHITIET bangcongBus = new BANGCONG_NV_CHITIET();
                        if (tuPhatSinhBangCong)
                        {
                            progress?.Invoke(25, 100, "Đang tự động phát sinh bảng chấm công gốc (TB_BANGCONG)...");
                            int rawCount = bangcongBus.PhatSinhBangCongRaw(nam, thang, iduser, cleanDb);
                            progress?.Invoke(35, 100, $"Đã phát sinh {rawCount} lượt chấm công chuẩn vào TB_BANGCONG.");
                        }

                        // Chuẩn bị danh sách ký hiệu các ngày trong tháng (tính 1 lần dùng chung)
                        List<string> listDay = new List<string>();
                        int daysInMonth = GetDayNumber(thang, nam);
                        NGAYLE ngayLeBus = new NGAYLE();
                        for (int j = 1; j <= daysInMonth; j++)
                        {
                            DateTime newDate = new DateTime(nam, thang, j);
                            int loaiCong = ngayLeBus.XacDinhLoaiCong(newDate);
                            if (loaiCong == 3)
                            {
                                listDay.Add("L"); // Ngày lễ
                            }
                            else if (loaiCong == 2)
                            {
                                listDay.Add("CN"); // Chủ nhật
                            }
                            else
                            {
                                listDay.Add("X"); // Ngày thường
                            }
                        }
                        while (listDay.Count < 31)
                        {
                            listDay.Add("");
                        }

                        double soNgayLamViec = GetData_Functions.demSoNgayLamViecTrongThang(thang, nam);
                        DateTime now = DateTime.Now;

                        cleanDb.Configuration.AutoDetectChangesEnabled = false;
                        cleanDb.Configuration.ValidateOnSaveEnabled = false;

                        // Non-destructive UPSERT vào TB_KYCONGCHITIET thay vì DELETE
                        var existingList = cleanDb.TB_KYCONGCHITIET.Where(x => x.MAKYCONG == makycong).ToList();
                        var existingMap = existingList.ToDictionary(x => x.MANV);

                        foreach (var item in distinctNV)
                        {
                            if (existingMap.TryGetValue(item.MANV, out var existing))
                            {
                                existing.HOTEN = item.HOTEN;
                                existing.IDCTY = item.IDCTY;
                                existing.D1 = listDay[0];
                                existing.D2 = listDay[1];
                                existing.D3 = listDay[2];
                                existing.D4 = listDay[3];
                                existing.D5 = listDay[4];
                                existing.D6 = listDay[5];
                                existing.D7 = listDay[6];
                                existing.D8 = listDay[7];
                                existing.D9 = listDay[8];
                                existing.D10 = listDay[9];
                                existing.D11 = listDay[10];
                                existing.D12 = listDay[11];
                                existing.D13 = listDay[12];
                                existing.D14 = listDay[13];
                                existing.D15 = listDay[14];
                                existing.D16 = listDay[15];
                                existing.D17 = listDay[16];
                                existing.D18 = listDay[17];
                                existing.D19 = listDay[18];
                                existing.D20 = listDay[19];
                                existing.D21 = listDay[20];
                                existing.D22 = listDay[21];
                                existing.D23 = listDay[22];
                                existing.D24 = listDay[23];
                                existing.D25 = listDay[24];
                                existing.D26 = listDay[25];
                                existing.D27 = listDay[26];
                                existing.D28 = listDay[27];
                                existing.D29 = listDay[28];
                                existing.D30 = listDay[29];
                                existing.D31 = listDay[30];
                                existing.NGAYCONG = (decimal)soNgayLamViec;
                                existing.TONGNGAYCONG = (decimal)soNgayLamViec;
                                existing.UPDATED_BY = iduser;
                                existing.UPDATED_DATE = now;
                            }
                            else
                            {
                                TB_KYCONGCHITIET kycongchitiet = new TB_KYCONGCHITIET
                                {
                                    MAKYCONG = makycong,
                                    MANV = item.MANV,
                                    HOTEN = item.HOTEN,
                                    IDCTY = item.IDCTY,
                                    D1 = listDay[0],
                                    D2 = listDay[1],
                                    D3 = listDay[2],
                                    D4 = listDay[3],
                                    D5 = listDay[4],
                                    D6 = listDay[5],
                                    D7 = listDay[6],
                                    D8 = listDay[7],
                                    D9 = listDay[8],
                                    D10 = listDay[9],
                                    D11 = listDay[10],
                                    D12 = listDay[11],
                                    D13 = listDay[12],
                                    D14 = listDay[13],
                                    D15 = listDay[14],
                                    D16 = listDay[15],
                                    D17 = listDay[16],
                                    D18 = listDay[17],
                                    D19 = listDay[18],
                                    D20 = listDay[19],
                                    D21 = listDay[20],
                                    D22 = listDay[21],
                                    D23 = listDay[22],
                                    D24 = listDay[23],
                                    D25 = listDay[24],
                                    D26 = listDay[25],
                                    D27 = listDay[26],
                                    D28 = listDay[27],
                                    D29 = listDay[28],
                                    D30 = listDay[29],
                                    D31 = listDay[30],
                                    NGAYCONG = (decimal)soNgayLamViec,
                                    TONGNGAYCONG = (decimal)soNgayLamViec,
                                    CREATED_BY = iduser,
                                    CREATED_DATE = now
                                };
                                cleanDb.TB_KYCONGCHITIET.Add(kycongchitiet);
                            }
                        }

                        progress?.Invoke(45, 100, "Đang lưu ma trận kỳ công vào CSDL...");
                        cleanDb.SaveChanges();

                        // 3. Phát sinh bảng công chi tiết từng ngày (trên cùng context, non-destructive MERGE)
                        progress?.Invoke(60, 100, "Đang tính toán chi tiết từng ngày công...");
                        bangcongBus.PhatSinhBangCongChiTiet(makycong, nam, thang, iduser, progress, cleanDb, false, false);

                        // 4. Không bật cờ TRANGTHAI = 1 tại đây vì chưa hoàn thành phân đoạn thời gian và công bố.
                        // Cờ TRANGTHAI chỉ được bật nguyên tử (ACID) khi BƯỚC B hoàn tất 100%.

                        // 5. TẤT CẢ THÀNH CÔNG -> COMMIT TRANSACTION (BƯỚC A: LƯU ĐẦU VÀO)
                        trans.Commit();

                        // 6. Sau khi commit thành công Bước A, chạy TimeSegmentationEngine và công bố bảng công chính thức V1.18 (BƯỚC B)
                        progress?.Invoke(95, 100, "Đang phân đoạn thời gian và công bố bảng công V1.18...");
                        var publishingService = new AttendancePublishingService();
                        var pubResult = publishingService.PublishAttendance(new AttendancePublishRequest
                        {
                            MaKyCong = makycong,
                            NguoiThucHien = iduser,
                            ManvList = distinctNV.Select(x => (long)x.MANV).ToList(),
                            GhiChu = "Phát sinh toàn bộ kỳ công và công bố V1.18",
                            ForceRecalculate = true
                        });

                        if (!pubResult.Success)
                        {
                            throw new InvalidOperationException("Đã lưu đầu vào kỳ công thành công, nhưng công bố kết quả chưa hoàn tất: " + pubResult.ErrorMessage + ". Dữ liệu đầu vào đã được lưu giữ; bạn có thể chạy lại bước công bố mà không cần phát sinh lại.");
                        }

                        // Đảm bảo cờ TRANGTHAI = 1 và NGAYTINHCONG được đồng bộ chắc chắn trên CSDL
                        using (var finalDb = new MyEntities())
                        {
                            finalDb.Database.ExecuteSqlCommand(
                                "UPDATE TB_KYCONG SET TRANGTHAI = 1, NGAYTINHCONG = SYSDATE, UPDATED_BY = :p_user, UPDATED_DATE = SYSDATE WHERE MAKYCONG = :p_mkc",
                                new Oracle.ManagedDataAccess.Client.OracleParameter("p_user", iduser),
                                new Oracle.ManagedDataAccess.Client.OracleParameter("p_mkc", makycong));
                        }

                        progress?.Invoke(100, 100, "Phát sinh kỳ công và bảng công chi tiết thành công!");
                    }
                    catch (Exception ex)
                    {
                        try { trans.Rollback(); } catch { }
                        throw new Exception(ex.Message, ex);
                    }
                }
            }

            // Làm mới lại db context của đối tượng hiện tại
            this.db = new MyEntities();
        }


        public TB_KYCONGCHITIET Update(TB_KYCONGCHITIET kcct, int iduser)
        {
            try
            {
                var kcLock = db.TB_KYCONG.FirstOrDefault(x => x.MAKYCONG == kcct.MAKYCONG);
                if (kcLock != null && (kcLock.KHOA ?? 0) == 1)
                {
                    throw new InvalidOperationException($"Kỳ công {kcct.MAKYCONG} đã bị khóa (KHOA = 1). Bảng công chi tiết chỉ có thể xem, không thể chỉnh sửa.");
                }

                var kycongchitiet = db.TB_KYCONGCHITIET.FirstOrDefault(x => x.MAKYCONG == kcct.MAKYCONG && x.MANV == kcct.MANV);
                kycongchitiet.D1 = kcct.D1;
                kycongchitiet.D2 = kcct.D2;
                kycongchitiet.D3 = kcct.D3;
                kycongchitiet.D4 = kcct.D4;
                kycongchitiet.D5 = kcct.D5;
                kycongchitiet.D6 = kcct.D6;
                kycongchitiet.D7 = kcct.D7;
                kycongchitiet.D8 = kcct.D8;
                kycongchitiet.D9 = kcct.D9;
                kycongchitiet.D10 = kcct.D10;
                kycongchitiet.D11 = kcct.D11;
                kycongchitiet.D12 = kcct.D12;
                kycongchitiet.D13 = kcct.D13;
                kycongchitiet.D14 = kcct.D14;
                kycongchitiet.D15 = kcct.D15;
                kycongchitiet.D16 = kcct.D16;
                kycongchitiet.D17 = kcct.D17;
                kycongchitiet.D18 = kcct.D18;
                kycongchitiet.D19 = kcct.D19;
                kycongchitiet.D20 = kcct.D20;
                kycongchitiet.D21 = kcct.D21;
                kycongchitiet.D22 = kcct.D22;
                kycongchitiet.D23 = kcct.D23;
                kycongchitiet.D24 = kcct.D24;
                kycongchitiet.D25 = kcct.D25;
                kycongchitiet.D26 = kcct.D26;
                kycongchitiet.D27 = kcct.D27;
                kycongchitiet.D28 = kcct.D28;
                kycongchitiet.D29 = kcct.D29;
                kycongchitiet.D30 = kcct.D30;
                kycongchitiet.D31 = kcct.D31;

                kycongchitiet.NGAYCONG = kcct.NGAYCONG;
                kycongchitiet.TONGNGAYCONG = kcct.TONGNGAYCONG;
                kycongchitiet.CONGCHUNHAT = kcct.CONGCHUNHAT;
                kycongchitiet.CONGNGAYLE = kcct.CONGNGAYLE;
                kycongchitiet.NGHIKHONGPHEP = kcct.NGHIKHONGPHEP;
                kycongchitiet.NGAYPHEP = kcct.NGAYPHEP;
                kycongchitiet.UPDATED_BY = iduser;
                kycongchitiet.UPDATED_DATE = DateTime.Now;
                db.SaveChanges();
                return kycongchitiet;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi update data" + ex.Message);
            }
        }

        public void UpdateChamCong(int _MAKYCONG, int _manv, int _cngay, string _valueChamCong)
        {
            var kcLock = db.TB_KYCONG.FirstOrDefault(x => x.MAKYCONG == _MAKYCONG);
            if (kcLock != null && (kcLock.KHOA ?? 0) == 1)
            {
                throw new InvalidOperationException($"Kỳ công {_MAKYCONG} đã bị khóa (KHOA = 1). Bảng công chi tiết chỉ có thể xem, không thể chỉnh sửa.");
            }

            // Tạo fieldName (D1, D2,...)
            string fieldName = "D" + _cngay.ToString();
            var kcct = getItem(_MAKYCONG, _manv);

            if (kcct != null)
            {
                // Sử dụng Reflection để tìm và cập nhật trường tương ứng
                var propertyInfo = kcct.GetType().GetProperty(fieldName);
                if (propertyInfo != null)
                {
                    propertyInfo.SetValue(kcct, _valueChamCong); // Cập nhật giá trị

                    using (var context = new MyEntities())
                    {
                        context.Entry(kcct).State = EntityState.Modified;
                        context.SaveChanges();
                    }
                }
                else
                {
                    MessageBox.Show($"Không tìm thấy thuộc tính '{fieldName}' trong bản ghi TB_KYCONGCHITIET.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Không tìm thấy bản ghi tương ứng", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private int GetDayNumber(int thang, int nam)
        {
            int dayNumber = 0;
            switch (thang)
            {
                case 2:
                    dayNumber = (nam % 4 == 0 && nam % 100 != 0) || nam % 400 == 0 ? 29 : 28;
                    break;

                case 4:
                case 6:
                case 9:
                case 11:
                    dayNumber = 30;
                    break;

                case 1:
                case 3:
                case 5:
                case 7:
                case 8:
                case 10:
                case 12:
                    dayNumber = 31;
                    break;
            }
            return dayNumber;
        }
    }
}
