using DA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bu.CLASS_SYSTEM
{
    public class SYS_USER
    {
        MyEntities db = new MyEntities();

        public TB_SYS_USER getItem(int iduser)
        {
            return db.TB_SYS_USER.FirstOrDefault(x => x.IDUSER == iduser);
        }
        public List<TB_SYS_USER> getALL() 
        { 
            return db.TB_SYS_USER.ToList();
        }

        //public List<TB_SYS_USER> getUser

        public bool checkUserExist(string username)
        {
            var us = db.TB_SYS_USER.FirstOrDefault(x => x.USERNAME == username);
            if (us != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public TB_SYS_USER Add(TB_SYS_USER user) 
        {
            try
            {
                db.TB_SYS_USER.Add(user);
                db.SaveChanges();
                return user;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi tạo Group. " +ex.Message);
            }
        }
    }
}
