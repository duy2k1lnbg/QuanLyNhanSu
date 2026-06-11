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
        MyEntities db = new MyEntities();

        public TB_NHANVIEN getItem(int id)
        {
            return db.TB_NHANVIEN.FirstOrDefault(x => x.MANV == id);
        }
        public List<TB_NHANVIEN> getList()
        {
            return db.TB_NHANVIEN.ToList();
        }
        public List<NHANVIEN_DTO> getListFll_DTO() 
        { 
            return (from nv in db.TB_NHANVIEN
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
                var innerExceptionMessage = ex.InnerException?.Message ?? string.Empty;
                throw new Exception("Lỗi: " + ex.Message + " Inner Exception: " + innerExceptionMessage);
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
                _nv.HINHANH = nv.HINHANH;
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
                //db.TB_NHANVIEN.Remove(_nv);
                _nv.DELETED_BY = iduser;
                _nv.DELETED_DATE = DateTime.Now;
                db.SaveChanges();
                try
                {
                    Bu.Services.AI_Services.Vector.AiDataSyncHub.NotifyEmployeeDeleted(id);
                }
                catch { }
            }
            catch (Exception ex)
            {

                throw new Exception("Lỗi" + ex.Message);
            }
        }

        public List<TB_NHANVIEN> getSinhNhat()
        {
            return db.TB_NHANVIEN.Where(x => x.NGAYSINH.Value.Month == DateTime.Now.Month).ToList();
        }
    }
}
