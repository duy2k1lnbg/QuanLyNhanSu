using DA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace Bu.CLASS_SYSTEM
{
    public class SYS_FUNCTION
    {
        MyEntities db = new MyEntities();

        public List<TB_SYS_FUNCTION> getParent()
        {
            return db.TB_SYS_FUNCTION.Where(x => x.ISGROUP == 1 && x.MENU == 1).OrderBy(s=>s.SORT).ToList();
        }

        public List<TB_SYS_FUNCTION> getChild(string parent)
        {
            return db.TB_SYS_FUNCTION.Where(x => x.ISGROUP == 0 && x.MENU == 1 && x.PARENT == parent).OrderBy(s => s.SORT).ToList();
        }
    }
}
