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
    }
}
