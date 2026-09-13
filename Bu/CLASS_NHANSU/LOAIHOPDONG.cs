using DA;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bu.CLASS_NHANSU
{
    public class LOAIHOPDONG
    {
        private MyEntities db = new MyEntities();

        public TB_LOAIHOPDONG getItem(int loaihd)
        {
            return db.TB_LOAIHOPDONG.FirstOrDefault(x => x.LOAIHD == loaihd);
        }

        public List<TB_LOAIHOPDONG> getList()
        {
            return db.TB_LOAIHOPDONG.Where(x => x.DELETED_BY == null).OrderBy(x => x.LOAIHD).ToList();
        }

        public List<TB_LOAIHOPDONG> getAll()
        {
            return db.TB_LOAIHOPDONG.OrderBy(x => x.LOAIHD).ToList();
        }

        public TB_LOAIHOPDONG Add(TB_LOAIHOPDONG lhd)
        {
            try
            {
                if (lhd.LOAIHD <= 0)
                {
                    decimal maxId = db.TB_LOAIHOPDONG.Any() ? db.TB_LOAIHOPDONG.Max(x => x.LOAIHD) : 0;
                    lhd.LOAIHD = maxId + 1;
                }

                lhd.CREATED_DATE = DateTime.Now;
                db.TB_LOAIHOPDONG.Add(lhd);
                db.SaveChanges();
                return lhd;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi thêm Loại hợp đồng: " + ex.Message, ex);
            }
        }

        public TB_LOAIHOPDONG Update(TB_LOAIHOPDONG lhd)
        {
            try
            {
                var item = db.TB_LOAIHOPDONG.FirstOrDefault(x => x.LOAIHD == lhd.LOAIHD);
                if (item == null)
                {
                    throw new Exception("Không tìm thấy loại hợp đồng với mã: " + lhd.LOAIHD);
                }

                item.TENLOAIHD = lhd.TENLOAIHD;
                item.UPDATED_BY = lhd.UPDATED_BY;
                item.UPDATED_DATE = DateTime.Now;
                db.SaveChanges();
                return item;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi cập nhật Loại hợp đồng: " + ex.Message, ex);
            }
        }

        public void Delete(int loaihd, int iduser)
        {
            try
            {
                var item = db.TB_LOAIHOPDONG.FirstOrDefault(x => x.LOAIHD == loaihd);
                if (item != null)
                {
                    item.DELETED_BY = iduser;
                    item.DELETED_DATE = DateTime.Now;
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi xóa Loại hợp đồng: " + ex.Message, ex);
            }
        }
    }
}
