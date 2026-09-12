using Bu;
using Bu.DTO;
using DA;
using HRMS_API.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Http;

namespace HRMS_API.Controllers
{
    [JwtAuthorize]
    [RoutePrefix("api/nhanvien")]
    public class NhanVienController : ApiController
    {
        private readonly NHANVIEN _nhanVienBus = new NHANVIEN();

        /// <summary>
        /// GET: api/nhanvien
        /// Lấy danh sách nhân viên có áp dụng Data Scope theo công ty/đơn vị người dùng
        /// </summary>
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetAll(string lang = "vi")
        {
            try
            {
                var jwtUser = JwtAuthorizeAttribute.GetCurrentJwtUser(Request);
                List<NHANVIEN_DTO> list = _nhanVienBus.getListFll_DTO(lang);

                // Access Control theo Data Scope: User không phải Admin chỉ thấy dữ liệu công ty của họ
                if (jwtUser != null && !jwtUser.IsAdmin && !string.IsNullOrWhiteSpace(jwtUser.MaCty))
                {
                    if (int.TryParse(jwtUser.MaCty, out int userCtyId) && userCtyId > 0)
                    {
                        list = list.Where(x => x.IDCTY == userCtyId).ToList();
                    }
                }

                return Ok(list);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi khi tải danh sách nhân viên: " + ex.ToString());
                return Content(System.Net.HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi tải danh sách nhân viên." });
            }
        }

        /// <summary>
        /// GET: api/nhanvien/{id}
        /// Lấy thông tin chi tiết một nhân viên theo mã MANV
        /// </summary>
        [HttpGet]
        [Route("{id:int}")]
        public IHttpActionResult GetById(int id)
        {
            try
            {
                var jwtUser = JwtAuthorizeAttribute.GetCurrentJwtUser(Request);
                var nv = _nhanVienBus.getItem(id);
                if (nv == null)
                {
                    return NotFound();
                }

                // Data Scope Check
                if (jwtUser != null && !jwtUser.IsAdmin && !string.IsNullOrWhiteSpace(jwtUser.MaCty))
                {
                    if (int.TryParse(jwtUser.MaCty, out int userCtyId) && userCtyId > 0 && nv.IDCTY != userCtyId)
                    {
                        return Content(System.Net.HttpStatusCode.Forbidden, new { success = false, message = "Từ chối truy cập: Bản ghi nhân viên nằm ngoài phạm vi dữ liệu của bạn." });
                    }
                }

                return Ok(nv);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi khi lấy thông tin nhân viên #" + id + ": " + ex.ToString());
                return Content(System.Net.HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi lấy thông tin nhân viên." });
            }
        }

        /// <summary>
        /// GET: api/nhanvien/{id}/profile360
        /// Truy xuất hồ sơ 360 độ thực tế từ CSDL Oracle (Bảng lương, Hợp đồng, Chấm công, Sự kiện, Checklist)
        /// Tuyệt đối không sinh dữ liệu giả lập.
        /// </summary>
        [HttpGet]
        [Route("{id}/profile360")]
        public IHttpActionResult GetProfile360(string id)
        {
            try
            {
                if (!int.TryParse(id.Split('.')[0], out int manv))
                {
                    return BadRequest("Mã nhân viên không hợp lệ.");
                }

                var jwtUser = JwtAuthorizeAttribute.GetCurrentJwtUser(Request);
                var nv = _nhanVienBus.getItem(manv);
                if (nv == null)
                {
                    return NotFound();
                }

                if (jwtUser != null && !jwtUser.IsAdmin && !string.IsNullOrWhiteSpace(jwtUser.MaCty))
                {
                    if (int.TryParse(jwtUser.MaCty, out int userCtyId) && userCtyId > 0 && nv.IDCTY != userCtyId)
                    {
                        return Content(System.Net.HttpStatusCode.Forbidden, new { success = false, message = "Từ chối truy cập: Bản ghi nhân viên nằm ngoài phạm vi công ty của bạn." });
                    }
                }

                using (var db = new MyEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    db.Configuration.ProxyCreationEnabled = false;

                    var result = new Profile360ResultDTO
                    {
                        Summary = new Profile360SummaryDTO(),
                        LichSuLuong = new List<Profile360PayrollItemDTO>(),
                        Timeline = new List<Profile360TimelineItemDTO>(),
                        OnboardingChecklist = new List<Profile360ChecklistItemDTO>()
                    };

                    // 1. HỢP ĐỒNG LAO ĐỘNG
                    var contracts = db.TB_HOPDONG
                        .Where(x => x.MANV == manv && x.DEL_DATE == null)
                        .OrderBy(x => x.NGAYBATDAU ?? x.NGAYKY)
                        .ToList();

                    var earliestContract = contracts.FirstOrDefault();
                    var latestContract = contracts.LastOrDefault();

                    DateTime? joinDate = earliestContract?.NGAYBATDAU ?? earliestContract?.NGAYKY ?? nv.CREATED_DATE;
                    result.Summary.NgayVaoCongTy = joinDate.HasValue ? joinDate.Value.ToString("dd/MM/yyyy") : "Chưa ghi nhận";
                    result.Summary.SoHopDong = latestContract?.SOHD ?? "Chưa ký HĐ";
                    result.Summary.ThoiHanHD = latestContract?.THOIHAN ?? "Không xác định";
                    result.Summary.HeSoLuong = (latestContract?.LUONG_THOA_THUAN.HasValue == true && latestContract.LUONG_THOA_THUAN > 0) 
                        ? latestContract.LUONG_THOA_THUAN 
                        : latestContract?.HESOLUONG;

                    // 2. BẢNG LƯƠNG THỰC TẾ
                    var rawPayrolls = (from bl in db.TB_BANGLUONG
                                       where bl.MANV == manv
                                       join kc in db.TB_KYCONG on bl.MAKYCONG equals kc.MAKYCONG into kcGroup
                                       from kc in kcGroup.DefaultIfEmpty()
                                       orderby bl.MAKYCONG descending
                                       select new { bl, kc }).ToList();

                    foreach (var item in rawPayrolls)
                    {
                        var isPaid = item.kc != null && item.kc.KHOA == 1;
                        result.LichSuLuong.Add(new Profile360PayrollItemDTO
                        {
                            Key = item.bl.IDBL.ToString(),
                            Makycong = (int)item.bl.MAKYCONG,
                            KyLuong = $"Tháng {item.bl.THANG:00}/{item.bl.NAM}",
                            Gross = item.bl.LUONG_CONG_THUCTE ?? 0,
                            Ot = item.bl.TIEN_TANGCA ?? 0,
                            Net = item.bl.THUC_LINH ?? 0,
                            CongThucTe = item.bl.CONG_THUCTE ?? 0,
                            CongChuan = item.bl.CONG_CHUAN ?? 26,
                            TrangThai = isPaid ? "Đã chi trả" : "Chờ chi trả"
                        });
                    }

                    var latestBl = rawPayrolls.FirstOrDefault();
                    if (latestBl != null)
                    {
                        bool isLatestPaid = latestBl.kc != null && latestBl.kc.KHOA == 1;
                        result.Summary.ThuNhapNetKyGanNhat = latestBl.bl.THUC_LINH;
                        result.Summary.KyLuongNetLabel = isLatestPaid 
                            ? $"Đã chuyển khoản T{latestBl.bl.THANG:00}/{latestBl.bl.NAM}"
                            : $"Dự kiến chi trả T{latestBl.bl.THANG:00}/{latestBl.bl.NAM}";
                        result.Summary.TrangThaiChiTra = isLatestPaid ? "Đã chi trả" : "Chờ chi trả";
                    }
                    else
                    {
                        result.Summary.ThuNhapNetKyGanNhat = null;
                        result.Summary.KyLuongNetLabel = "Chưa phát sinh kỳ lương";
                        result.Summary.TrangThaiChiTra = "Chưa có";
                    }

                    // 3. CHUYÊN CẦN & NGHỈ PHÉP
                    var kcctList = db.TB_KYCONGCHITIET.Where(x => x.MANV == manv).ToList();
                    var latestKc = db.TB_KYCONG.OrderByDescending(x => x.MAKYCONG).FirstOrDefault();
                    TB_KYCONGCHITIET currentKcct = null;
                    if (latestKc != null)
                    {
                        currentKcct = kcctList.FirstOrDefault(x => x.MAKYCONG == latestKc.MAKYCONG);
                    }
                    if (currentKcct == null)
                    {
                        currentKcct = kcctList.OrderByDescending(x => x.MAKYCONG).FirstOrDefault();
                    }

                    if (currentKcct != null)
                    {
                        decimal tongCong = currentKcct.TONGNGAYCONG ?? 0;
                        decimal congChuan = (latestKc?.NGAYCONGTRONGTHANG.HasValue == true && latestKc.NGAYCONGTRONGTHANG > 0) 
                            ? (decimal)latestKc.NGAYCONGTRONGTHANG.Value 
                            : 26;

                        result.Summary.TongNgayCong = tongCong;
                        result.Summary.CongChuan = congChuan;
                        result.Summary.TyLeChuyenCan = congChuan > 0 ? Math.Round((double)(tongCong / congChuan) * 100, 1) : 0;
                        result.Summary.KyCongChuyenCanLabel = latestKc != null ? $"Kỳ T{latestKc.THANG:00}/{latestKc.NAM}" : "Kỳ gần nhất";
                    }
                    else
                    {
                        result.Summary.TongNgayCong = 0;
                        result.Summary.CongChuan = 26;
                        result.Summary.TyLeChuyenCan = 0;
                        result.Summary.KyCongChuyenCanLabel = "Chưa có dữ liệu";
                    }

                    decimal nghiPhep = kcctList.Sum(x => x.NGAYPHEP ?? 0);
                    result.Summary.SoNgayNghiPhep = nghiPhep;
                    result.Summary.TongQuyPhep = 12;
                    result.Summary.PhepConLai = Math.Max(0, 12 - nghiPhep);

                    // 4. TIMELINE SỰ NGHIỆP THỰC TẾ
                    var timelineList = new List<Profile360TimelineItemDTO>();

                    // Từ Hợp đồng
                    foreach (var hd in contracts)
                    {
                        var d = hd.NGAYKY ?? hd.NGAYBATDAU ?? DateTime.Now;
                        decimal sal = (hd.LUONG_THOA_THUAN.HasValue && hd.LUONG_THOA_THUAN > 0) ? hd.LUONG_THOA_THUAN.Value : (hd.HESOLUONG ?? 0);
                        timelineList.Add(new Profile360TimelineItemDTO
                        {
                            Ngay = d.ToString("dd/MM/yyyy"),
                            DateSort = d,
                            Loai = "HOPDONG",
                            TieuDe = $"Ký Hợp đồng Lao động (#{hd.SOHD})",
                            MoTa = $"Ký hợp đồng thời hạn: {hd.THOIHAN ?? "Xác định thời hạn"}. Lương thỏa thuận: {sal:N0} đ.",
                            TagColor = "purple",
                            IconType = "FileProtectOutlined"
                        });
                    }

                    // Từ Bảng lương
                    foreach (var b in rawPayrolls.Take(6))
                    {
                        var d = new DateTime(b.bl.NAM, b.bl.THANG, 28);
                        timelineList.Add(new Profile360TimelineItemDTO
                        {
                            Ngay = d.ToString("dd/MM/yyyy"),
                            DateSort = d,
                            Loai = "LUONG",
                            TieuDe = $"Chi trả lương kỳ Tháng {b.bl.THANG:00}/{b.bl.NAM}",
                            MoTa = $"Thực lĩnh: {(b.bl.THUC_LINH ?? 0):N0} đ (Công thực tế: {b.bl.CONG_THUCTE ?? 0}/{b.bl.CONG_CHUAN ?? 26} ngày công).",
                            TagColor = "green",
                            IconType = "DollarOutlined"
                        });
                    }

                    // Từ Nâng lương
                    var nlList = db.TB_NANGLUONG_NHANVIEN
                        .Where(x => x.MANV == manv && x.DELETED_DATE == null)
                        .ToList();
                    foreach (var nl in nlList)
                    {
                        var d = nl.NGAYLENLUONG ?? nl.NGAYKYNL ?? DateTime.Now;
                        timelineList.Add(new Profile360TimelineItemDTO
                        {
                            Ngay = d.ToString("dd/MM/yyyy"),
                            DateSort = d,
                            Loai = "NANGLUONG",
                            TieuDe = $"Quyết định Nâng lương (#{nl.SOQDNL})",
                            MoTa = $"Điều chỉnh mức lương / hệ số từ {nl.HESOLUONG_NOW} lên {nl.HESOLUONG_NEW}. {nl.GHICHUNL}",
                            TagColor = "blue",
                            IconType = "RocketOutlined"
                        });
                    }

                    // Từ Khen thưởng / Kỷ luật
                    var ktList = db.TB_KHENTHUONG_KYLUAT
                        .Where(x => x.MANV == manv && x.DELETED_DATE == null)
                        .ToList();
                    foreach (var kt in ktList)
                    {
                        var d = kt.NGAY ?? DateTime.Now;
                        bool isReward = (kt.LOAI ?? 1) == 1;
                        timelineList.Add(new Profile360TimelineItemDTO
                        {
                            Ngay = d.ToString("dd/MM/yyyy"),
                            DateSort = d,
                            Loai = isReward ? "KHENTHUONG" : "KYLUAT",
                            TieuDe = isReward ? $"Khen thưởng: {kt.NOIDUNG} (QĐ #{kt.SOQUYETDINH})" : $"Kỷ luật: {kt.NOIDUNG} (QĐ #{kt.SOQUYETDINH})",
                            MoTa = $"Lý do: {kt.LYDO}",
                            TagColor = isReward ? "gold" : "red",
                            IconType = isReward ? "TrophyOutlined" : "AlertOutlined"
                        });
                    }

                    // Từ Điều chuyển công tác
                    var dcList = db.TB_DIEUCHUYEN_NHANVIEN
                        .Where(x => x.MANV == manv && x.DELETED_DATE == null)
                        .ToList();
                    foreach (var dc in dcList)
                    {
                        var d = dc.NGAYDC ?? DateTime.Now;
                        timelineList.Add(new Profile360TimelineItemDTO
                        {
                            Ngay = d.ToString("dd/MM/yyyy"),
                            DateSort = d,
                            Loai = "DIEUCHUYEN",
                            TieuDe = $"Quyết định Điều chuyển công tác (#{dc.SOQDDIEUCHUYEN})",
                            MoTa = $"Lý do điều chuyển: {dc.LYDODC}. {dc.GHICHU}",
                            TagColor = "orange",
                            IconType = "CalendarOutlined"
                        });
                    }

                    if (timelineList.Count == 0 && joinDate.HasValue)
                    {
                        timelineList.Add(new Profile360TimelineItemDTO
                        {
                            Ngay = joinDate.Value.ToString("dd/MM/yyyy"),
                            DateSort = joinDate.Value,
                            Loai = "JOIN",
                            TieuDe = "Gia nhập công ty",
                            MoTa = "Khởi tạo hồ sơ nhân sự trên hệ thống HRMS.",
                            TagColor = "cyan",
                            IconType = "CheckCircleOutlined"
                        });
                    }

                    result.Timeline = timelineList.OrderByDescending(x => x.DateSort).ToList();

                    // 5. ONBOARDING CHECKLIST
                    bool hasCccd = !string.IsNullOrWhiteSpace(nv.CCCD);
                    bool hasPhone = !string.IsNullOrWhiteSpace(nv.DIENTHOAI);
                    bool hasAddress = !string.IsNullOrWhiteSpace(nv.DIACHI);
                    bool hasContract = contracts.Count > 0;
                    bool hasInsurance = db.TB_BAOHIEM.Any(x => x.MANV == manv);
                    bool hasDeptRole = nv.IDPB.HasValue && nv.IDPB > 0 && nv.IDCV.HasValue && nv.IDCV > 0;
                    bool hasAvatar = nv.HINHANH != null && nv.HINHANH.Length > 0;

                    result.OnboardingChecklist = new List<Profile360ChecklistItemDTO>
                    {
                        new Profile360ChecklistItemDTO { Title = "Tạo hồ sơ nhân sự trên hệ thống HRMS", Done = true, Detail = $"Mã NV #{manv}" },
                        new Profile360ChecklistItemDTO { Title = "Cập nhật số Căn cước công dân (CCCD)", Done = hasCccd, Detail = hasCccd ? nv.CCCD : "Chưa cập nhật" },
                        new Profile360ChecklistItemDTO { Title = "Cập nhật số điện thoại liên lạc", Done = hasPhone, Detail = hasPhone ? nv.DIENTHOAI : "Chưa cập nhật" },
                        new Profile360ChecklistItemDTO { Title = "Cập nhật địa chỉ cư trú", Done = hasAddress, Detail = hasAddress ? nv.DIACHI : "Chưa cập nhật" },
                        new Profile360ChecklistItemDTO { Title = "Phân bổ Phòng ban & Chức vụ công tác", Done = hasDeptRole, Detail = hasDeptRole ? "Đã phân bổ" : "Chưa phân bổ" },
                        new Profile360ChecklistItemDTO { Title = "Ký Hợp đồng lao động chính thức / thử việc", Done = hasContract, Detail = hasContract ? $"Số HĐ: {latestContract?.SOHD}" : "Chưa có hợp đồng" },
                        new Profile360ChecklistItemDTO { Title = "Tham gia Bảo hiểm xã hội (BHXH)", Done = hasInsurance, Detail = hasInsurance ? "Đã có thông tin BHXH" : "Chưa đăng ký" },
                        new Profile360ChecklistItemDTO { Title = "Tải lên ảnh chân dung nhân viên", Done = hasAvatar, Detail = hasAvatar ? "Đã có ảnh chân dung" : "Chưa có ảnh chân dung" },
                    };

                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi khi lấy Profile 360 nhân viên #" + id + ": " + ex.ToString());
                return Content(System.Net.HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi lấy hồ sơ 360 độ của nhân sự." });
            }
        }

        /// <summary>
        /// POST: api/nhanvien
        /// Thêm mới một nhân viên vào Oracle Database (kèm ảnh đại diện nếu có)
        /// </summary>
        [HttpPost]
        [Route("")]
        [JwtAuthorize(Right = "F_NHANSU_ADD")]
        public IHttpActionResult Create([FromBody] NhanVienInputModel model)
        {
            try
            {
                if (model == null || string.IsNullOrWhiteSpace(model.HOTEN))
                {
                    return BadRequest("Họ tên nhân viên không được để trống.");
                }

                var nv = model.ToEntity();
                var jwtUser = JwtAuthorizeAttribute.GetCurrentJwtUser(Request);
                int currentUserId = (jwtUser != null && int.TryParse(jwtUser.UserId, out int uid)) ? uid : 1;

                // Tự động lấy ID hợp lệ đầu tiên từ Oracle Database nếu client không chỉ định
                using (var db = new MyEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    db.Configuration.ProxyCreationEnabled = false;

                    if (!nv.IDCTY.HasValue || nv.IDCTY <= 0)
                    {
                        if (jwtUser != null && int.TryParse(jwtUser.MaCty, out int userCty) && userCty > 0)
                        {
                            nv.IDCTY = userCty;
                        }
                        else
                        {
                            nv.IDCTY = db.TB_CONGTY.Select(x => x.IDCTY).FirstOrDefault();
                        }
                    }
                    if (!nv.IDGT.HasValue || nv.IDGT <= 0) nv.IDGT = db.TB_GIOITINH.Select(x => x.IDGT).FirstOrDefault();
                    if (!nv.IDPB.HasValue || nv.IDPB <= 0) nv.IDPB = db.TB_PHONGBAN.Select(x => x.IDPB).FirstOrDefault();
                    if (!nv.IDBP.HasValue || nv.IDBP <= 0) nv.IDBP = db.TB_BOPHAN.Select(x => x.IDBP).FirstOrDefault();
                    if (!nv.IDCV.HasValue || nv.IDCV <= 0) nv.IDCV = db.TB_CHUCVU.Select(x => x.IDCV).FirstOrDefault();
                    if (!nv.IDTD.HasValue || nv.IDTD <= 0) nv.IDTD = db.TB_TRINHDO.Select(x => x.IDTD).FirstOrDefault();
                    if (!nv.IDDT.HasValue || nv.IDDT <= 0) nv.IDDT = db.TB_DANTOC.Select(x => x.IDDT).FirstOrDefault();
                    if (!nv.IDTG.HasValue || nv.IDTG <= 0) nv.IDTG = db.TB_TONGIAO.Select(x => x.IDTG).FirstOrDefault();
                    if (!nv.IDQT.HasValue || nv.IDQT <= 0) nv.IDQT = db.TB_QUOCTICH.Select(x => x.IDQT).FirstOrDefault();
                }

                if (!nv.LOAI_NV.HasValue || nv.LOAI_NV <= 0) nv.LOAI_NV = 1;
                if (!nv.NGAYSINH.HasValue) nv.NGAYSINH = new DateTime(1995, 1, 1);
                if (string.IsNullOrEmpty(nv.CCCD)) nv.CCCD = "001" + new Random().Next(100000000, 999999999);
                if (string.IsNullOrEmpty(nv.DIENTHOAI)) nv.DIENTHOAI = "0900000000";
                if (string.IsNullOrEmpty(nv.DIACHI)) nv.DIACHI = "Hà Nội";
                
                // Gán đúng danh tính người dùng thực hiện từ JWT
                nv.CREATED_BY = currentUserId;
                nv.CREATED_DATE = DateTime.Now;

                var created = _nhanVienBus.Add(nv);
                return Ok(created);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi khi thêm mới nhân viên: " + ex.ToString());
                return Content(System.Net.HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi thêm mới hồ sơ nhân viên." });
            }
        }

        /// <summary>
        /// PUT: api/nhanvien/{id}
        /// Cập nhật thông tin nhân viên theo mã MANV (hỗ trợ cập nhật ảnh chân dung)
        /// </summary>
        [HttpPut]
        [Route("{id:int}")]
        [JwtAuthorize(Right = "F_NHANSU_EDIT")]
        public IHttpActionResult Update(int id, [FromBody] NhanVienInputModel model)
        {
            try
            {
                if (model == null) return BadRequest("Dữ liệu cập nhật không hợp lệ.");

                var jwtUser = JwtAuthorizeAttribute.GetCurrentJwtUser(Request);
                int currentUserId = (jwtUser != null && int.TryParse(jwtUser.UserId, out int uid)) ? uid : 1;

                // Data Scope Check
                var existing = _nhanVienBus.getItem(id);
                if (existing == null) return NotFound();
                if (jwtUser != null && !jwtUser.IsAdmin && !string.IsNullOrWhiteSpace(jwtUser.MaCty))
                {
                    if (int.TryParse(jwtUser.MaCty, out int userCtyId) && userCtyId > 0 && existing.IDCTY != userCtyId)
                    {
                        return Content(System.Net.HttpStatusCode.Forbidden, new { success = false, message = "Từ chối truy cập: Bản ghi nhân viên nằm ngoài phạm vi công ty của bạn." });
                    }
                }

                var nv = model.ToEntity();
                nv.MANV = id;
                nv.UPDATED_BY = currentUserId;
                nv.UPDATED_DATE = DateTime.Now;

                var updated = _nhanVienBus.Update(nv);
                return Ok(updated);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi khi cập nhật nhân viên #" + id + ": " + ex.ToString());
                return Content(System.Net.HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi cập nhật hồ sơ nhân viên." });
            }
        }

        /// <summary>
        /// POST: api/nhanvien/{id}/avatar
        /// Tải lên ảnh chân dung nhân viên trực tiếp
        /// </summary>
        [HttpPost]
        [Route("{id}/avatar")]
        [JwtAuthorize(Right = "F_NHANSU_EDIT")]
        public IHttpActionResult UploadAvatar(string id, [FromBody] AvatarUploadModel model)
        {
            try
            {
                if (!int.TryParse(id.Split('.')[0], out int manv))
                {
                    return BadRequest("Mã nhân viên không hợp lệ.");
                }

                var existing = _nhanVienBus.getItem(manv);
                if (existing == null) return NotFound();

                byte[] bytes = model?.GetImageBytes();
                if (bytes == null || bytes.Length == 0)
                {
                    return BadRequest("Dữ liệu hình ảnh không hợp lệ.");
                }

                existing.HINHANH = bytes;
                existing.UPDATED_DATE = DateTime.Now;
                _nhanVienBus.Update(existing);

                return Ok(new { success = true, message = "Đã cập nhật ảnh đại diện thành công!" });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi khi cập nhật ảnh đại diện #" + id + ": " + ex.ToString());
                return Content(System.Net.HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi lưu ảnh đại diện." });
            }
        }

        /// <summary>
        /// GET: api/nhanvien/{id}/avatar
        /// Xem ảnh đại diện trực tiếp qua URL
        /// </summary>
        [HttpGet]
        [Route("{id}/avatar")]
        [AllowAnonymous]
        public System.Net.Http.HttpResponseMessage GetAvatar(string id)
        {
            if (!int.TryParse(id.Split('.')[0], out int manv))
            {
                return new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.NotFound);
            }

            var nv = _nhanVienBus.getItem(manv);
            if (nv?.HINHANH != null && nv.HINHANH.Length > 0)
            {
                var response = new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.OK);
                response.Content = new System.Net.Http.ByteArrayContent(nv.HINHANH);
                response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
                return response;
            }
            return new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.NotFound);
        }

        /// <summary>
        /// DELETE: api/nhanvien/{id}
        /// Thôi việc / Xóa nhân viên theo mã MANV
        /// </summary>
        [HttpDelete]
        [Route("{id:int}")]
        [JwtAuthorize(Right = "F_NHANSU_DELETE")]
        public IHttpActionResult Delete(int id)
        {
            try
            {
                var jwtUser = JwtAuthorizeAttribute.GetCurrentJwtUser(Request);
                int currentUserId = (jwtUser != null && int.TryParse(jwtUser.UserId, out int uid)) ? uid : 1;

                var existing = _nhanVienBus.getItem(id);
                if (existing == null) return NotFound();
                if (jwtUser != null && !jwtUser.IsAdmin && !string.IsNullOrWhiteSpace(jwtUser.MaCty))
                {
                    if (int.TryParse(jwtUser.MaCty, out int userCtyId) && userCtyId > 0 && existing.IDCTY != userCtyId)
                    {
                        return Content(System.Net.HttpStatusCode.Forbidden, new { success = false, message = "Từ chối truy cập: Bản ghi nhân viên nằm ngoài phạm vi công ty của bạn." });
                    }
                }

                _nhanVienBus.Delete(id, currentUserId);
                return Ok(new { success = true, message = $"Đã cập nhật trạng thái xóa/thôi việc cho nhân viên #{id} thành công." });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi khi xóa nhân viên #" + id + ": " + ex.ToString());
                return Content(System.Net.HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi xóa hồ sơ nhân viên." });
            }
        }
    }

    public class NhanVienInputModel
    {
        public decimal? MANV { get; set; }
        public string HOTEN { get; set; }
        public Nullable<decimal> IDGT { get; set; }
        public string GIOITINH { get; set; }
        public Nullable<System.DateTime> NGAYSINH { get; set; }
        public string DIENTHOAI { get; set; }
        public string CCCD { get; set; }
        public string DIACHI { get; set; }
        public string HINHANH { get; set; } // Base64 string hoặc data:image/...;base64,...
        public Nullable<decimal> IDPB { get; set; }
        public Nullable<decimal> IDBP { get; set; }
        public Nullable<decimal> IDCV { get; set; }
        public Nullable<decimal> IDTD { get; set; }
        public Nullable<decimal> IDDT { get; set; }
        public Nullable<decimal> IDTG { get; set; }
        public Nullable<decimal> IDCTY { get; set; }
        public Nullable<decimal> IDQT { get; set; }
        public Nullable<decimal> DATHOIVIEC { get; set; }
        public Nullable<int> LOAI_NV { get; set; }

        public byte[] GetImageBytes()
        {
            if (string.IsNullOrWhiteSpace(HINHANH)) return null;
            try
            {
                string clean = HINHANH.Trim();
                int commaIdx = clean.IndexOf(',');
                if (commaIdx >= 0 && clean.StartsWith("data:image", StringComparison.OrdinalIgnoreCase))
                {
                    clean = clean.Substring(commaIdx + 1);
                }
                return Convert.FromBase64String(clean);
            }
            catch
            {
                return null;
            }
        }

        public TB_NHANVIEN ToEntity()
        {
            decimal? idgt = this.IDGT;
            if (!string.IsNullOrWhiteSpace(this.GIOITINH))
            {
                string gt = this.GIOITINH.Trim().ToLower();
                if (gt == "nữ" || gt == "nu" || gt == "female" || gt == "2")
                {
                    idgt = 2;
                }
                else if (gt == "khác" || gt == "khac" || gt == "other" || gt == "3")
                {
                    idgt = 3;
                }
                else if (gt == "nam" || gt == "male" || gt == "1")
                {
                    idgt = 1;
                }
            }

            var entity = new TB_NHANVIEN
            {
                MANV = this.MANV ?? 0,
                HOTEN = this.HOTEN,
                IDGT = idgt,
                NGAYSINH = this.NGAYSINH,
                DIENTHOAI = this.DIENTHOAI,
                CCCD = this.CCCD,
                DIACHI = this.DIACHI,
                IDPB = this.IDPB,
                IDBP = this.IDBP,
                IDCV = this.IDCV,
                IDTD = this.IDTD,
                IDDT = this.IDDT,
                IDTG = this.IDTG,
                IDCTY = this.IDCTY,
                IDQT = this.IDQT,
                DATHOIVIEC = this.DATHOIVIEC,
                LOAI_NV = this.LOAI_NV,
            };
            if (!string.IsNullOrWhiteSpace(HINHANH))
            {
                if (HINHANH.Trim().Equals("CLEAR", StringComparison.OrdinalIgnoreCase) || HINHANH.Trim().Equals("REMOVE", StringComparison.OrdinalIgnoreCase))
                {
                    entity.HINHANH = new byte[0]; // Tín hiệu xóa ảnh đại diện
                }
                else
                {
                    var bytes = GetImageBytes();
                    if (bytes != null) entity.HINHANH = bytes;
                }
            }
            return entity;
        }
    }

    public class AvatarUploadModel
    {
        public string HINHANH { get; set; }
        public string ImageBase64 { get; set; }

        public byte[] GetImageBytes()
        {
            string raw = !string.IsNullOrWhiteSpace(HINHANH) ? HINHANH : ImageBase64;
            if (string.IsNullOrWhiteSpace(raw)) return null;
            try
            {
                string clean = raw.Trim();
                int commaIdx = clean.IndexOf(',');
                if (commaIdx >= 0 && clean.StartsWith("data:image", StringComparison.OrdinalIgnoreCase))
                {
                    clean = clean.Substring(commaIdx + 1);
                }
                return Convert.FromBase64String(clean);
            }
            catch
            {
                return null;
            }
        }
    }

    public class Profile360SummaryDTO
    {
        public string NgayVaoCongTy { get; set; }
        public string SoHopDong { get; set; }
        public string ThoiHanHD { get; set; }
        public decimal? HeSoLuong { get; set; }
        public double TyLeChuyenCan { get; set; }
        public decimal TongNgayCong { get; set; }
        public decimal CongChuan { get; set; }
        public string KyCongChuyenCanLabel { get; set; }
        public decimal SoNgayNghiPhep { get; set; }
        public int TongQuyPhep { get; set; }
        public decimal PhepConLai { get; set; }
        public decimal? ThuNhapNetKyGanNhat { get; set; }
        public string KyLuongNetLabel { get; set; }
        public string TrangThaiChiTra { get; set; }
    }

    public class Profile360PayrollItemDTO
    {
        public string Key { get; set; }
        public int Makycong { get; set; }
        public string KyLuong { get; set; }
        public decimal Gross { get; set; }
        public decimal Ot { get; set; }
        public decimal Net { get; set; }
        public decimal CongThucTe { get; set; }
        public decimal CongChuan { get; set; }
        public string TrangThai { get; set; }
    }

    public class Profile360TimelineItemDTO
    {
        public string Ngay { get; set; }
        public DateTime DateSort { get; set; }
        public string Loai { get; set; }
        public string TieuDe { get; set; }
        public string MoTa { get; set; }
        public string TagColor { get; set; }
        public string IconType { get; set; }
    }

    public class Profile360ChecklistItemDTO
    {
        public string Title { get; set; }
        public bool Done { get; set; }
        public string Detail { get; set; }
    }

    public class Profile360ResultDTO
    {
        public Profile360SummaryDTO Summary { get; set; }
        public List<Profile360PayrollItemDTO> LichSuLuong { get; set; }
        public List<Profile360TimelineItemDTO> Timeline { get; set; }
        public List<Profile360ChecklistItemDTO> OnboardingChecklist { get; set; }
    }
}
