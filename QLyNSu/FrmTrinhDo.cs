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
    public partial class FrmTrinhDo : DevExpress.XtraEditors.XtraForm
    {
        TRINHDO _trinhdo;
        bool _them;
        int _IDTD;
        public FrmTrinhDo()
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

        void LoadData()
        {
            gcDsTD.DataSource = _trinhdo.getList();
            gvDsTD.OptionsBehavior.Editable = false;
        }

        void SaveData()
        {
            if (_them)
            {
                TB_TRINHDO tg = new TB_TRINHDO();
                tg.TENTD = txtTen.Text;
                _trinhdo.Add(tg);
            }
            else
            {
                var td = _trinhdo.getItem(_IDTD);
                td.TENTD = txtTen.Text;
                _trinhdo.Update(td);
            }
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
            if (MessageBox.Show("Mày có chắc là xoá nó đi không", "Thông báo", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                _trinhdo.Delete(_IDTD);
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

        private void btnDong_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            this.Close();
        }

        private void btnIn_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {

        }

        private void FrmTrinhDo_Load(object sender, EventArgs e)
        {
            _them = false;
            _trinhdo = new TRINHDO();
            showHide(true);
            LoadData();
        }

        private void gvDsTD_Click(object sender, EventArgs e)
        {
            _IDTD = int.Parse(gvDsTD.GetFocusedRowCellValue("IDTD").ToString());
            txtTen.Text = gvDsTD.GetFocusedRowCellValue("TENTD").ToString();
        }
    }
}