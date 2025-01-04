using Bu.CLASS_SYSTEM;
using DA;
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
    public partial class frmGroup : DevExpress.XtraEditors.XtraForm
    {
        public frmGroup()
        {
            InitializeComponent();
        }

        private SYS_USER _sysUser;
        private TB_SYS_USER _user;

        private bool _them;


        private void frmGroup_Load(object sender, EventArgs e)
        {
            _sysUser = new SYS_USER();
        }

        private void btnLuu_Click(object sender, EventArgs e)
        {
            if (txtTenNhom.Text.Trim() =="")
            {
                MessageBox.Show("Chưa nhập tên nhóm. Vui lòng viết liền không dấu", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtTenNhom.SelectAll();
                txtTenNhom.Focus();
                return;
            }
        }

        private void btnDong_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void saveData()
        {
            if (_them)
            {
                bool checkedUser = _sysUser.checkUserExist(txtTenNhom.Text.Trim());
                if (checkedUser)
                {
                    MessageBox.Show("Nhóm đã tồn tại. Vui lòng kiểm tra lại", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    txtTenNhom.SelectAll();
                    return;
                }
                _user = new TB_SYS_USER();
                _user.USERNAME = txtTenNhom.Text.Trim();
                _user.FULLNAME = txtMoTa.Text;
                _user.ISGROUP = 1;
                _user.DISABLED = 0;
                _user.MACTY = "1";
                _user.MADVI = "1";
                _sysUser.Add(_user);
            }
        }
    }
}