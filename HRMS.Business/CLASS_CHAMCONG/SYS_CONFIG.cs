using DA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bu.CLASS_CHAMCONG
{
    public class SYS_CONFIG
    {
        MyEntities db = new MyEntities();

        public TB_CONFIG getItem(string name)
        {
            return db.TB_CONFIG.FirstOrDefault(x => x.NAME == name);
        }

        public string getValue(string name, string defaultValue = "")
        {
            var item = db.TB_CONFIG.FirstOrDefault(x => x.NAME == name);
            return item != null ? item.VALUE : defaultValue;
        }

        public void setItem(string name, string value)
        {
            var item = db.TB_CONFIG.FirstOrDefault(x => x.NAME == name);
            if (item != null)
            {
                item.VALUE = value;
            }
            else
            {
                decimal newId = db.TB_CONFIG.Any() ? db.TB_CONFIG.Max(x => x.ID_CF) + 1 : 1;
                db.TB_CONFIG.Add(new TB_CONFIG { ID_CF = newId, NAME = name, VALUE = value });
            }
            db.SaveChanges();
        }
    }
}
