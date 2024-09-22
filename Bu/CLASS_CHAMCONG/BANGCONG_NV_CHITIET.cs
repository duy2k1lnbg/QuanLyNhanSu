using DA;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bu.CLASS_CHAMCONG
{
    public class BANGCONG_NV_CHITIET
    {
        MyEntities db = new MyEntities();

        public TB_BANGCONG_CHITIET Add(TB_BANGCONG_CHITIET bcct)
        {
            try
            {
                db.TB_BANGCONG_CHITIET.Add(bcct);
                db.SaveChanges();
                return bcct;
            }
            catch (Exception ex)
            {

                throw new Exception("Lỗi Add data " + ex.Message);
            }
            
        }
    }
}
