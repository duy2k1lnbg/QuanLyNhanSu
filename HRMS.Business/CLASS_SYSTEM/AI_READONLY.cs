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
        AiEntities dbc = new AiEntities();

        public List<V_AI_EMPLOYEE> GetAll()
        {
            return dbc.V_AI_EMPLOYEE.ToList();
        }
    }
}
