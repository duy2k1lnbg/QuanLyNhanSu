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

namespace QLyNSu.FORM_CHAMCONG
{
    public partial class FrmCapNhatNgayCong : DevExpress.XtraEditors.XtraForm
    {
        public FrmCapNhatNgayCong()
        {
            InitializeComponent();
        }

        public int _manv;
        public string _hoten;
        public int _MAKYCONG;
        public string _ngay;
        private void FrmCapNhatNgayCong_Load(object sender, EventArgs e)
        {

        }

        private void btnCapNhat_Click(object sender, EventArgs e)
        {
            MessageBox.Show(_manv.ToString() + " -  " + _MAKYCONG.ToString() + " -  " + _ngay);
        }

        private void btnDong_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}