using DA;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Bu.CLASS_CHAMCONG
{
    public class NGAYLE
    {
        private MyEntities db = new MyEntities();

        public TB_NGAYLE getItem(int idle)
        {
            return db.TB_NGAYLE.FirstOrDefault(x => x.IDLE == idle);
        }

        public List<TB_NGAYLE> getList()
        {
            return db.TB_NGAYLE.Where(x => x.DELETED_BY == null).OrderBy(x => x.NGAY).ToList();
        }

        public List<TB_NGAYLE> getAll()
        {
            return db.TB_NGAYLE.OrderBy(x => x.NGAY).ToList();
        }

        public List<TB_NGAYLE> getListByNam(int nam)
        {
            return db.TB_NGAYLE.Where(x => x.DELETED_BY == null && (x.NAM == nam || x.NGAY.Year == nam))
                               .OrderBy(x => x.NGAY)
                               .ToList();
        }

        /// <summary>
        /// Kiểm tra xem một ngày cụ thể có phải là ngày lễ hợp lệ (chưa bị xóa) hay không.
        /// </summary>
        public TB_NGAYLE KiemTraNgayLe(DateTime ngay)
        {
            DateTime d = ngay.Date;
            return db.TB_NGAYLE.FirstOrDefault(x => x.DELETED_BY == null 
                                                && DbFunctions.TruncateTime(x.NGAY) == d);
        }

        /// <summary>
        /// Kiểm tra trùng ngày lễ khi thêm mới hoặc sửa.
        /// </summary>
        public bool KiemTraTrungNgay(DateTime ngay, int? currentId = null)
        {
            DateTime d = ngay.Date;
            if (currentId.HasValue && currentId.Value > 0)
            {
                return db.TB_NGAYLE.Any(x => x.DELETED_BY == null 
                                          && x.IDLE != currentId.Value 
                                          && DbFunctions.TruncateTime(x.NGAY) == d);
            }
            return db.TB_NGAYLE.Any(x => x.DELETED_BY == null 
                                      && DbFunctions.TruncateTime(x.NGAY) == d);
        }

        /// <summary>
        /// Xác định Loại công (IDLOAICONG) cho một ngày theo đúng quy tắc nghiệp vụ:
        /// 1. Kiểm tra ngày đó có tồn tại trong NGAYLE và chưa bị xóa hay không -> Nếu có: Ngày lễ (3).
        /// 2. Nếu không phải ngày lễ -> kiểm tra có phải Chủ nhật hay không -> Nếu là Chủ nhật: Chủ nhật (2).
        /// 3. Nếu không -> Ngày thường (1).
        /// </summary>
        public int XacDinhLoaiCong(DateTime ngay)
        {
            // 1. Kiểm tra ngày lễ
            if (KiemTraNgayLe(ngay) != null)
            {
                return 3; // ID 3: Công ngày lễ
            }

            // 2. Kiểm tra Chủ nhật
            if (ngay.DayOfWeek == DayOfWeek.Sunday)
            {
                return 2; // ID 2: Công Chủ nhật
            }

            // 3. Mặc định: Công ngày thường
            return 1; // ID 1: Công ngày thường
        }

        public TB_NGAYLE Add(TB_NGAYLE nl)
        {
            try
            {
                if (KiemTraTrungNgay(nl.NGAY))
                {
                    throw new Exception($"Ngày {nl.NGAY:dd/MM/yyyy} đã được khai báo ngày lễ trong hệ thống.");
                }

                if (nl.IDLE <= 0)
                {
                    decimal maxId = db.TB_NGAYLE.Any() ? db.TB_NGAYLE.Max(x => x.IDLE) : 0;
                    nl.IDLE = maxId + 1;
                }

                if (nl.NAM == null || nl.NAM <= 0)
                {
                    nl.NAM = nl.NGAY.Year;
                }

                if (nl.HESO == null || nl.HESO <= 0)
                {
                    nl.HESO = 2.00m;
                }

                nl.CREATED_DATE = DateTime.Now;
                db.TB_NGAYLE.Add(nl);
                db.SaveChanges();
                return nl;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi thêm Ngày lễ: " + ex.Message, ex);
            }
        }

        public TB_NGAYLE Update(TB_NGAYLE nl)
        {
            try
            {
                if (KiemTraTrungNgay(nl.NGAY, (int)nl.IDLE))
                {
                    throw new Exception($"Ngày {nl.NGAY:dd/MM/yyyy} đã trùng với một ngày lễ khác trong hệ thống.");
                }

                var item = db.TB_NGAYLE.FirstOrDefault(x => x.IDLE == nl.IDLE);
                if (item == null)
                {
                    throw new Exception("Không tìm thấy ngày lễ với mã: " + nl.IDLE);
                }

                item.TENLE = nl.TENLE;
                item.NGAY = nl.NGAY;
                item.NAM = nl.NGAY.Year;
                item.HESO = nl.HESO;
                item.UPDATED_BY = nl.UPDATED_BY;
                item.UPDATED_DATE = DateTime.Now;
                db.SaveChanges();
                return item;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi cập nhật Ngày lễ: " + ex.Message, ex);
            }
        }

        public void Delete(int idle, int iduser)
        {
            try
            {
                var item = db.TB_NGAYLE.FirstOrDefault(x => x.IDLE == idle);
                if (item != null)
                {
                    item.DELETED_BY = iduser;
                    item.DELETED_DATE = DateTime.Now;
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi xóa Ngày lễ: " + ex.Message, ex);
            }
        }
    }
}
