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
using DA;
using Bu;

namespace QLyNSu
{
    public partial class FrmDanToc : DevExpress.XtraEditors.XtraForm
    {
        DANTOC _dantoc;
        bool _them;
        int _IDDT;
        public FrmDanToc()
        {
            InitializeComponent();
        }

        void showHide(bool kt)
        {
            btnLuu.Enabled = !kt;
            btnHuy.Enabled = !kt;
            btnThem.Enabled = kt;
            btnXoa.Enabled = kt;
            btnSua.Enabled = kt;
            btnIn.Enabled = kt;
            btnDong.Enabled = kt;
            txtTen.Enabled = !kt;
        }
        private void labelControl1_Click(object sender, EventArgs e)
        {

        }

        private void btnDong_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            this.Close();
        }

        private void btnThem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            _them = true;
            showHide(false);
            txtTen.Text = string.Empty;
        }

        private void btnSua_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            _them = false;
            showHide(false);
        }

        private void btnXoa_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if(MessageBox.Show("Mày có chắc là xoá nó đi không","Thông báo",MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                _dantoc.Delete(_IDDT);
                LoadData();
            }    
        }

        private void btnLuu_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            SaveData();
            LoadData();
            _them = false;
            showHide(true);
        }

        private void btnHuy_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            _them = false;
            showHide(true);
        }

        private void btnIn_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {

        }

        private void FrmDanToc_Load(object sender, EventArgs e)
        {
            _them= false;
            _dantoc = new DANTOC();
            showHide(true);
            LoadData();
        }

        void LoadData()
        {
            gcDsDT.DataSource = _dantoc.getList();
            gvDsDT.OptionsBehavior.Editable = false;
        }

        void SaveData()
        {
            if (_them)
            {
                TB_DANTOC dt = new TB_DANTOC();
                dt.TENDT = txtTen.Text;
                _dantoc.Add(dt);
            }
            else
            {
                var dt = _dantoc.getItem(_IDDT);
                dt.TENDT = txtTen.Text;
                _dantoc.Update(dt);
            }
        }

        private void gvDsDT_Click(object sender, EventArgs e)
        {
            _IDDT = int.Parse(gvDsDT.GetFocusedRowCellValue("IDDT").ToString());
            txtTen.Text = gvDsDT.GetFocusedRowCellValue("TENDT").ToString();
        }
    }
}