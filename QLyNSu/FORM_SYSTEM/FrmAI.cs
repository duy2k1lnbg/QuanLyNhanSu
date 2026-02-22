using Bu.CLASS_SYSTEM;
using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QLyNSu.FORM_SYSTEM
{
    public partial class FrmAI : DevExpress.XtraEditors.XtraForm
    {
        public FrmAI()
        {
            InitializeComponent();
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            AI_READONLY dal = new AI_READONLY();
            dgvData.DataSource = dal.GetAll();
        }
    }
}