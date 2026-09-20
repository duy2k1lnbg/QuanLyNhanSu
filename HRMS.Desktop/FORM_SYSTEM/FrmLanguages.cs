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
using Bu.CLASS_SYSTEM;
using Bu.DTO;
using QLyNSu.Functions;

namespace QLyNSu.FORM_SYSTEM
{
    public partial class FrmLanguages : DevExpress.XtraEditors.XtraForm
    {
        private SYS_LANGUAGE _sysLanguage;
        private bool _them;
        private string _selectedCode;

        public FrmLanguages()
        {
            InitializeComponent();
        }

        private void FrmLanguages_Load(object sender, EventArgs e)
        {
            _sysLanguage = new SYS_LANGUAGE();
            _them = false;
            showHide(true);
            LoadData();
            
            // Translate the Form controls using the translation manager
            TranslationManager.Translate(this);
        }

        private void showHide(bool kt)
        {
            btnThem.Enabled = kt;
            btnSua.Enabled = kt;
            btnXoa.Enabled = kt;
            btnLuu.Enabled = !kt;
            btnHuy.Enabled = !kt;
            btnDong.Enabled = kt;

            // Only enable code editing when inserting a new language
            txtCode.Enabled = !kt && _them;
            txtName.Enabled = !kt;
            chkActive.Enabled = !kt;
        }

        private void LoadData()
        {
            try
            {
                gcDs.DataSource = _sysLanguage.getList();
                FormManager_Functions.CustomView_Colums(gvDs);
            }
            catch (Exception ex)
            {
                MessageBox.Show(TranslationManager.Translate("Lỗi khi tải danh sách ngôn ngữ: ") + ex.Message, 
                    TranslationManager.Translate("Lỗi"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnThem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            _them = true;
            _selectedCode = string.Empty;
            txtCode.Text = string.Empty;
            txtName.Text = string.Empty;
            chkActive.Checked = true;
            showHide(false);
            txtCode.Focus();
        }

        private void btnSua_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedCode))
            {
                MessageBox.Show(TranslationManager.Translate("Vui lòng chọn một dòng để chỉnh sửa."), 
                    TranslationManager.Translate("Thông báo"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            _them = false;
            showHide(false);
            txtName.Focus();
        }

        private void btnXoa_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedCode))
            {
                MessageBox.Show(TranslationManager.Translate("Vui lòng chọn một dòng để xóa."), 
                    TranslationManager.Translate("Thông báo"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_selectedCode.Equals("vi", StringComparison.OrdinalIgnoreCase) || 
                _selectedCode.Equals("en", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(TranslationManager.Translate("Không thể xóa ngôn ngữ mặc định hệ thống (vi/en)."), 
                    TranslationManager.Translate("Cảnh báo"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string deleteConfirm = TranslationManager.Translate("Bạn có chắc chắn muốn xóa ngôn ngữ này không?");
            if (MessageBox.Show(deleteConfirm, TranslationManager.Translate("Xác nhận xóa"), 
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    _sysLanguage.Delete(_selectedCode);
                    _selectedCode = string.Empty;
                    txtCode.Text = string.Empty;
                    txtName.Text = string.Empty;
                    chkActive.Checked = true;
                    LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(TranslationManager.Translate("Lỗi khi xóa ngôn ngữ: ") + ex.Message, 
                        TranslationManager.Translate("Lỗi"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnLuu_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCode.Text))
            {
                MessageBox.Show(TranslationManager.Translate("Vui lòng điền mã ngôn ngữ."), 
                    TranslationManager.Translate("Cảnh báo"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCode.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show(TranslationManager.Translate("Vui lòng điền tên ngôn ngữ."), 
                    TranslationManager.Translate("Cảnh báo"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return;
            }

            try
            {
                var dto = new SysLanguageDTO
                {
                    CODE = txtCode.Text.Trim().ToLower(),
                    NAME = txtName.Text.Trim(),
                    IS_ACTIVE = chkActive.Checked
                };

                if (_them)
                {
                    // Check if duplicate code
                    if (_sysLanguage.getItem(dto.CODE) != null)
                    {
                        MessageBox.Show(TranslationManager.Translate("Mã ngôn ngữ này đã tồn tại trong hệ thống."), 
                            TranslationManager.Translate("Cảnh báo"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        txtCode.Focus();
                        return;
                    }
                    _sysLanguage.Add(dto);
                }
                else
                {
                    _sysLanguage.Update(dto);
                }

                _them = false;
                showHide(true);
                LoadData();
                MessageBox.Show(TranslationManager.Translate("Lưu thành công!"), 
                    TranslationManager.Translate("Thông báo"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(TranslationManager.Translate("Lỗi khi lưu dữ liệu: ") + ex.Message, 
                    TranslationManager.Translate("Lỗi"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnHuy_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            _them = false;
            showHide(true);
            gvDs_Click(sender, EventArgs.Empty);
        }

        private void btnDong_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            this.Close();
        }

        private void gvDs_Click(object sender, EventArgs e)
        {
            if (gvDs.GetFocusedRowCellValue("CODE") != null)
            {
                _selectedCode = gvDs.GetFocusedRowCellValue("CODE").ToString();
                txtCode.Text = _selectedCode;
                txtName.Text = gvDs.GetFocusedRowCellValue("NAME")?.ToString() ?? string.Empty;
                chkActive.Checked = Convert.ToBoolean(gvDs.GetFocusedRowCellValue("IS_ACTIVE"));
            }
        }
    }
}
