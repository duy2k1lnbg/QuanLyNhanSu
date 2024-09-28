using DevExpress.XtraEditors;
using DevExpress.XtraReports.UI;
using QLyNSu.Reports;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QLyNSu.FORM_BAOCAO
{
    public partial class FrmBaoCaoChiTiet : DevExpress.XtraEditors.XtraForm
    {
        public FrmBaoCaoChiTiet()
        {
            InitializeComponent();
        }

        private void FrmBaoCaoChiTiet_Load(object sender, EventArgs e)
        {

        }

        private void btnIn_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            rptBaoCaoLuongNV rpt = new rptBaoCaoLuongNV();
            rpt.ShowRibbonPreviewDialog();
        }
    }
}