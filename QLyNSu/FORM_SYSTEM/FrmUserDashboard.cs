using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using Bu.CLASS_SYSTEM;

namespace QLyNSu.FORM_SYSTEM
{
    public partial class FrmUserDashboard : DevExpress.XtraEditors.XtraForm
    {
        SYS_USER _sysUser;

        public FrmUserDashboard()
        {
            InitializeComponent();
        }

        private void FrmUserDashboard_Load(object sender, EventArgs e)
        {
            _sysUser = new SYS_USER();
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                var data = _sysUser.GetDashboardUserLoginInfo();
                gridControl1.DataSource = data;
                gridView1.BestFitColumns();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Lỗi khi tải dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnRefresh_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            LoadData();
        }
    }
}
