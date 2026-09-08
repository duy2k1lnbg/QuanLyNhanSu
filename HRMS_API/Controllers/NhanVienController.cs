using Bu;
using Bu.DTO;
using DA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Http;

namespace HRMS_API.Controllers
{
    [RoutePrefix("api/nhanvien")]
    public class NhanVienController : ApiController
    {
        private readonly NHANVIEN _nhanVienBus = new NHANVIEN();

        /// <summary>
        /// GET: api/nhanvien
        /// Lấy toàn bộ danh sách nhân viên từ Oracle Database kèm thông tin phòng ban, chức vụ
        /// </summary>
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetAll(string lang = "vi")
        {
            try
            {
                List<NHANVIEN_DTO> list = _nhanVienBus.getListFll_DTO(lang);
                return Ok(list);
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("Lỗi khi tải danh sách nhân viên: " + GetFullException(ex), ex));
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
                var nv = _nhanVienBus.getItem(id);
                if (nv == null)
                {
                    return NotFound();
                }
                return Ok(nv);
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("Lỗi khi lấy thông tin nhân viên #" + id + ": " + GetFullException(ex), ex));
            }
        }

        /// <summary>
        /// POST: api/nhanvien
        /// Thêm mới một nhân viên vào Oracle Database
        /// </summary>
        [HttpPost]
        [Route("")]
        public IHttpActionResult Create([FromBody] TB_NHANVIEN nv)
        {
            try
            {
                if (nv == null || string.IsNullOrWhiteSpace(nv.HOTEN))
                {
                    return BadRequest("Họ tên nhân viên không được để trống.");
                }

                // Tự động lấy ID hợp lệ đầu tiên từ Oracle Database nếu client không chỉ định
                using (var db = new MyEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    db.Configuration.ProxyCreationEnabled = false;

                    if (!nv.IDCTY.HasValue || nv.IDCTY <= 0) nv.IDCTY = db.TB_CONGTY.Select(x => x.IDCTY).FirstOrDefault();
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
                nv.CREATED_BY = 1;
                nv.CREATED_DATE = DateTime.Now;

                var created = _nhanVienBus.Add(nv);
                return Ok(created);
            }
            catch (Exception ex)
            {
                var fullErr = GetFullException(ex);
                return InternalServerError(new Exception("Lỗi khi thêm mới nhân viên: " + fullErr, ex));
            }
        }

        /// <summary>
        /// PUT: api/nhanvien/{id}
        /// Cập nhật thông tin nhân viên theo mã MANV
        /// </summary>
        [HttpPut]
        [Route("{id:int}")]
        public IHttpActionResult Update(int id, [FromBody] TB_NHANVIEN nv)
        {
            try
            {
                if (nv == null) return BadRequest("Dữ liệu cập nhật không hợp lệ.");

                nv.MANV = id;
                nv.UPDATED_BY = 1;
                nv.UPDATED_DATE = DateTime.Now;

                var updated = _nhanVienBus.Update(nv);
                return Ok(updated);
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("Lỗi khi cập nhật nhân viên #" + id + ": " + GetFullException(ex), ex));
            }
        }

        /// <summary>
        /// DELETE: api/nhanvien/{id}
        /// Thôi việc / Xóa nhân viên theo mã MANV
        /// </summary>
        [HttpDelete]
        [Route("{id:int}")]
        public IHttpActionResult Delete(int id, int iduser = 1)
        {
            try
            {
                _nhanVienBus.Delete(id, iduser);
                return Ok(new { success = true, message = $"Đã cập nhật trạng thái xóa/thôi việc cho nhân viên #{id} thành công." });
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("Lỗi khi xóa nhân viên #" + id + ": " + GetFullException(ex), ex));
            }
        }

        private static string GetFullException(Exception ex)
        {
            var sb = new StringBuilder();
            while (ex != null)
            {
                sb.AppendLine(ex.Message);
                ex = ex.InnerException;
            }
            return sb.ToString();
        }
    }
}
