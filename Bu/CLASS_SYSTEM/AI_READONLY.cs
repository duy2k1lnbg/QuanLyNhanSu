using DA;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bu.CLASS_SYSTEM
{
    public class AI_READONLY
    {
        AIEntities dbc = new AIEntities();

        public List<V_AI_NHANVIEN> GetAll()
        {
            return dbc.V_AI_NHANVIEN.ToList();
        }
    }
}
