using Bu;
using Bu.CLASS_CHAMCONG;
using DA;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Mask;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraReports.UI;
using DevExpress.XtraSplashScreen;
using QLyNSu.Functions;
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

namespace QLyNSu.FORM_CHAMCONG
{
    public partial class FrmBangCong_ChiTiet : DevExpress.XtraEditors.XtraForm
    {
        private ContextMenuStrip contextMenu;
        public FrmBangCong_ChiTiet()
        {
            InitializeComponent();
            InitContextMenuStrip();
        }
        private void InitContextMenuStrip()
        {
            contextMenu = new ContextMenuStrip();
            ToolStripMenuItem mnCapNhatNgayCong = new ToolStripMenuItem("Cập nhật ngày công");
            mnCapNhatNgayCong.Click += new EventHandler(this.mnCapNhatNgayCong_Click);
            contextMenu.Items.Add(mnCapNhatNgayCong);
            gcBangCongChiTiet.ContextMenuStrip = contextMenu;
            gvBangCongChiTiet.PopupMenuShowing += gvBangCongChiTiet_PopupMenuShowing;
            gvBangCongChiTiet.CellValueChanged += gvBangCongChiTiet_CellValueChanged;
        }

        private KYCONGCHITIET _kcct;
        private KYCONG _kycong;
        private NHANVIEN _nhanvien;
        private BANGCONG_NV_CHITIET _bangcong_ct;
        public int _macty;
        public int _thang;
        public int _nam;
        public int _MAKYCONG;
        

        private void FrmBangCong_ChiTiet_Load(object sender, EventArgs e)
        {
            chkTrangThai.Enabled = false;
            cboNam.Enabled = false;
            cboThang.Enabled = false;
            _kcct = new KYCONGCHITIET();
            _kycong = new KYCONG();
            _nhanvien = new NHANVIEN();
            _bangcong_ct = new BANGCONG_NV_CHITIET();

            if (_nam > 0 && _thang > 0)
            {
                cboThang.Text = _thang.ToString();
                cboNam.Text = _nam.ToString();
                if (_MAKYCONG <= 0)
                {
                    _MAKYCONG = _nam * 100 + _thang;
                }
            }
            else if (int.TryParse(cboNam.Text, out int n) && int.TryParse(cboThang.Text, out int t))
            {
                _nam = n;
                _thang = t;
                if (_MAKYCONG <= 0)
                {
                    _MAKYCONG = _nam * 100 + _thang;
                }
            }
            else
            {
                _nam = DateTime.Now.Year;
                _thang = DateTime.Now.Month;
                _MAKYCONG = _nam * 100 + _thang;
                cboNam.Text = _nam.ToString();
                cboThang.Text = _thang.ToString();
            }

            gcBangCongChiTiet.DataSource = _kcct.getList(_MAKYCONG);
            gvBangCongChiTiet.OptionsBehavior.Editable = true;
            CustomView(_thang, _nam);
            LockInfoColumns();
        }

        public void loadBangCong()
        {
            _kcct = new KYCONGCHITIET();
            int nam = _nam > 0 ? _nam : DateTime.Now.Year;
            int thang = _thang > 0 ? _thang : DateTime.Now.Month;
            if (int.TryParse(cboNam.Text, out int n)) nam = n;
            if (int.TryParse(cboThang.Text, out int t)) thang = t;

            _nam = nam;
            _thang = thang;
            _MAKYCONG = _nam * 100 + _thang;

            gcBangCongChiTiet.DataSource = _kcct.getList(_MAKYCONG);
            CustomView(_thang, _nam);
            gvBangCongChiTiet.OptionsBehavior.Editable = true;
            LockInfoColumns();
        }
        private async void btnPhatSinhKyCong_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            #region Check_Key
            SplashScreenManager.ShowForm(this, typeof(FrmWaiting), true, true, ParentFormState.Locked);
            if (_kycong.KiemTraMaKyCong(int.Parse(cboNam.Text) * 100 + int.Parse(cboThang.Text)) == 0)
            {
                MessageBox.Show("Kỳ công này chưa được tạo vui lòng vào bảng công và thêm.", "Thông báo");
                SplashScreenManager.CloseForm();
                return;
            }
            if (_kycong.KiemTraPhatSinhKyCong(int.Parse(cboNam.Text) * 100 + int.Parse(cboThang.Text)) == 1)
            {
                MessageBox.Show("Kỳ công đã tồn tại.", "Thông báo");
                SplashScreenManager.CloseForm();
                return;
            }
            #endregion

            int thang = int.Parse(cboThang.Text);
            int nam = int.Parse(cboNam.Text);
            int macty = _macty;
            int makycong = nam * 100 + thang;
            _MAKYCONG = makycong;

            try
            {
                SplashScreenManager.Default.SetWaitFormCaption("Phát Sinh Kỳ Công");
                SplashScreenManager.Default.SetWaitFormDescription("Đang chuẩn bị dữ liệu... (0%)");
                SplashScreenManager.Default.SendCommand(FrmWaiting.WaitFormCommand.SetProgress, 0);

                Action<int, int, string> onProgress = (current, total, msg) =>
                {
                    int percent = total > 0 ? (int)((double)current / total * 100) : current;
                    if (percent < 0) percent = 0;
                    if (percent > 100) percent = 100;
                    try
                    {
                        SplashScreenManager.Default.SetWaitFormDescription($"{msg} ({percent}%)");
                        SplashScreenManager.Default.SendCommand(FrmWaiting.WaitFormCommand.SetProgress, percent);
                    }
                    catch { }
                };

                await Task.Run(() =>
                {
                    // 1. Kiểm tra hợp đồng, tự động cập nhật thôi việc & phát sinh kỳ công chi tiết
                    _kcct.phatSinhKyCongChiTiet(macty, thang, nam, 1, onProgress);

                    // 2. Phát sinh bảng công chi tiết từng ngày (tối ưu tốc độ cao qua Oracle SQL)
                    _bangcong_ct.PhatSinhBangCongChiTiet(makycong, nam, thang, 1, onProgress);

                    // 3. Cập nhật trạng thái kỳ công
                    var kc = _kycong.getItem(nam * 100 + thang);
                    if (kc != null)
                    {
                        kc.TRANGTHAI = 1;
                        _kycong.Update(kc);
                    }
                    onProgress(100, 100, "Hoàn tất phát sinh kỳ công!");
                });

                SplashScreenManager.CloseForm();
                loadBangCong();
                XtraMessageBox.Show("Phát sinh kỳ công thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                SplashScreenManager.CloseForm();
                XtraMessageBox.Show("Lỗi khi phát sinh kỳ công: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnRefresh_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            loadBangCong();
        }

        private void btnXemBC_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            loadBangCong();
        }

        private void btnIn_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (_MAKYCONG <= 0 && int.TryParse(cboNam.Text, out int n) && int.TryParse(cboThang.Text, out int t))
            {
                _MAKYCONG = n * 100 + t;
            }
            List<TB_KYCONGCHITIET> lst = _kcct.getList(_MAKYCONG);
            rptBangCongTongHop rpt = new rptBangCongTongHop(lst, _MAKYCONG.ToString());
            rpt.ShowRibbonPreviewDialog();
        }

        private void btnDong_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            this.Close();
        }

        

        private void CustomView(int thang, int nam)
        {
            string layoutPath = Application.StartupPath + @"\BangCong_Layout.xml";
            if (System.IO.File.Exists(layoutPath))
            {
                gvBangCongChiTiet.RestoreLayoutFromXml(layoutPath);
            }
            int i;
            foreach (GridColumn gridColumn in gvBangCongChiTiet.Columns)
            {
                if (gridColumn.FieldName == "HOTEN") continue;

                RepositoryItemTextEdit textEdit = new RepositoryItemTextEdit();
                textEdit.Mask.MaskType = MaskType.RegEx;
                textEdit.Mask.EditMask = @"\p{Lu}+";
                gridColumn.ColumnEdit = textEdit;
            }

            for (i = 1; i <= GetDayNumber(thang, nam); i++)
            {
                #region Column_Casting
                DateTime newDate = new DateTime(nam, thang, i);

                GridColumn column = new GridColumn();
                column.AppearanceHeader.Font = new Font("Tahoma", 8, FontStyle.Regular);
                string fieldName = "D" + i;
                switch (newDate.DayOfWeek.ToString())
                {
                    case "Monday":
                        column = gvBangCongChiTiet.Columns[fieldName];
                        column.Caption = "T.Hai " + Environment.NewLine + i;
                        column.OptionsColumn.AllowEdit = true;
                        column.AppearanceHeader.ForeColor = Color.Blue;
                        column.AppearanceHeader.BackColor = Color.Transparent;
                        column.AppearanceHeader.BackColor2 = Color.Transparent;
                        column.AppearanceCell.ForeColor = Color.Black;
                        column.AppearanceCell.BackColor = Color.Transparent;
                        column.OptionsColumn.AllowFocus = true;
                        //column.Width = 30;
                        //column.AppearanceHeader.Font = new Font("Tahoma", 8, FontStyle.Regular);
                        break;

                    case "Tuesday":
                        column = gvBangCongChiTiet.Columns[fieldName];
                        column.Caption = "T.Ba " + Environment.NewLine + i;
                        column.OptionsColumn.AllowEdit = true;
                        column.AppearanceHeader.ForeColor = Color.Blue;
                        column.AppearanceHeader.BackColor = Color.Transparent;
                        column.AppearanceHeader.BackColor2 = Color.Transparent;
                        column.AppearanceCell.ForeColor = Color.Black;
                        column.AppearanceCell.BackColor = Color.Transparent;
                        column.OptionsColumn.AllowFocus = true;
                        //column.AppearanceHeader.Font = new Font("Tahoma", 8, FontStyle.Regular);
                        //column.Width = 30;
                        break;

                    case "Wednesday":
                        column = gvBangCongChiTiet.Columns[fieldName];
                        column.Caption = "T.Tư " + Environment.NewLine + i;
                        column.OptionsColumn.AllowEdit = true;
                        column.AppearanceHeader.ForeColor = Color.Blue;
                        column.AppearanceHeader.BackColor = Color.Transparent;
                        column.AppearanceHeader.BackColor2 = Color.Transparent;
                        column.AppearanceCell.ForeColor = Color.Black;
                        column.AppearanceCell.BackColor = Color.Transparent;
                        column.OptionsColumn.AllowFocus = true;
                        //column.AppearanceHeader.Font = new Font("Tahoma", 8, FontStyle.Regular);
                        //column.Width = 30;
                        break;
                    case "Thursday":
                        column = gvBangCongChiTiet.Columns[fieldName];
                        column.Caption = "T.Năm " + Environment.NewLine + i;
                        column.OptionsColumn.AllowEdit = true;
                        column.AppearanceHeader.ForeColor = Color.Blue;
                        column.AppearanceHeader.BackColor = Color.Transparent;
                        column.AppearanceHeader.BackColor2 = Color.Transparent;
                        column.AppearanceCell.ForeColor = Color.Black;
                        column.AppearanceCell.BackColor = Color.Transparent;
                        column.OptionsColumn.AllowFocus = true;
                        //column.AppearanceHeader.Font = new Font("Tahoma", 8, FontStyle.Regular);
                        //column.Width = 30;
                        break;
                    case "Friday":
                        column = gvBangCongChiTiet.Columns[fieldName];
                        column.Caption = "T.Sáu " + Environment.NewLine + i;
                        column.OptionsColumn.AllowEdit = true;
                        column.AppearanceHeader.ForeColor = Color.Blue;
                        column.AppearanceHeader.BackColor = Color.Transparent;
                        column.AppearanceHeader.BackColor2 = Color.Transparent;
                        column.AppearanceCell.ForeColor = Color.Black;
                        column.AppearanceCell.BackColor = Color.Transparent;
                        column.OptionsColumn.AllowFocus = true;
                        //column.AppearanceHeader.Font = new Font("Tahoma", 8, FontStyle.Regular);
                        //column.Width = 30;
                        break;
                    case "Saturday":
                        column = gvBangCongChiTiet.Columns[fieldName];
                        column.Caption = "T.Bảy " + Environment.NewLine + i;
                        column.OptionsColumn.AllowEdit = true;
                        column.AppearanceHeader.ForeColor = Color.Blue;
                        column.AppearanceHeader.BackColor = Color.Transparent;
                        column.AppearanceHeader.BackColor2 = Color.Transparent;
                        column.AppearanceCell.ForeColor = Color.Black;
                        column.AppearanceCell.BackColor = Color.Transparent;
                        column.OptionsColumn.AllowFocus = true;
                        //column.AppearanceHeader.Font = new Font("Tahoma", 8, FontStyle.Regular);
                        //column.Width = 30;
                        break;
                    case "Sunday":
                        column = gvBangCongChiTiet.Columns[fieldName];
                        column.Caption = "CN " + Environment.NewLine + i;
                        column.OptionsColumn.AllowEdit = false;
                        column.AppearanceHeader.ForeColor = Color.Red;
                        column.AppearanceHeader.BackColor = Color.GreenYellow;
                        column.AppearanceHeader.BackColor2 = Color.GreenYellow;
                        column.AppearanceCell.ForeColor = Color.Black;
                        column.AppearanceCell.BackColor = Color.Orange;
                        //column.AppearanceHeader.Font = new Font("Tahoma", 8, FontStyle.Regular);
                        //column.Width = 30;
                        //column.OptionsColumn.AllowFocus = false;
                        break;

                }
                #endregion
            }

            while (i <= 31)
            {
                gvBangCongChiTiet.Columns[i + 1].Visible = false;
                i++;
            }

        }

        private int GetDayNumber(int thang, int nam)
        {
            int dayNumber = 0;
            switch (thang)
            {
                case 2:
                    dayNumber = (nam % 4 == 0 && nam % 100 != 0) || nam % 400 == 0 ? 29 : 28;
                    break;

                case 4:
                case 6:
                case 9:
                case 11:
                    dayNumber = 30;
                    break;

                case 1:
                case 3:
                case 5:
                case 7:
                case 8:
                case 10:
                case 12:
                    dayNumber = 31;
                    break;
            }

            return dayNumber;
        }

        private void mnCapNhatNgayCong_Click(object sender, EventArgs e)
        {
            if (gvBangCongChiTiet.RowCount <= 0)
            {
                MessageBox.Show("Không có dữ liệu trong bảng hiện tại.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var focusedRowHandle = gvBangCongChiTiet.FocusedRowHandle;
            if (focusedRowHandle < 0)
            {
                MessageBox.Show("Vui lòng chọn một nhân viên để cập nhật.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (gvBangCongChiTiet.FocusedColumn == null)
            {
                MessageBox.Show("Vui lòng chọn một ô ngày công (cột ngày từ 1 đến 31) để cập nhật.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string fieldName = gvBangCongChiTiet.FocusedColumn.FieldName;
            if (string.IsNullOrEmpty(fieldName) || !fieldName.StartsWith("D", StringComparison.OrdinalIgnoreCase) || !int.TryParse(fieldName.Substring(1), out int dayNum) || dayNum < 1 || dayNum > 31)
            {
                MessageBox.Show("Vui lòng chọn đúng ô ngày công (cột ngày từ 1 đến 31) để cập nhật.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            object manvVal = gvBangCongChiTiet.GetFocusedRowCellValue("MANV");
            object hotenVal = gvBangCongChiTiet.GetFocusedRowCellValue("HOTEN");
            if (manvVal == null || manvVal == DBNull.Value || !int.TryParse(manvVal.ToString(), out int manv))
            {
                MessageBox.Show("Không tìm thấy thông tin mã nhân viên của dòng được chọn.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_MAKYCONG <= 0 && int.TryParse(cboNam.Text, out int namVal) && int.TryParse(cboThang.Text, out int thangVal))
            {
                _MAKYCONG = namVal * 100 + thangVal;
                _nam = namVal;
                _thang = thangVal;
            }

            FrmCapNhatNgayCong frm = new FrmCapNhatNgayCong();
            frm._MAKYCONG = _MAKYCONG;
            frm._manv = manv;
            frm._hoten = hotenVal != null ? hotenVal.ToString() : "";
            frm._ngay = fieldName;
            frm.nam_f_bcct1 = int.TryParse(cboNam.Text, out int n) ? n : (_MAKYCONG / 100);
            frm.thang_f1_bcct = int.TryParse(cboThang.Text, out int t) ? t : (_MAKYCONG % 100);
            frm.ShowDialog();
        }

        private void gvBangCongChiTiet_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
        {
            if (e.CellValue == null)
            {

            }
            else 
            {
                if (e.CellValue.ToString() == "CT")
                {
                    e.Appearance.BackColor = Color.DeepSkyBlue;
                    e.Appearance.ForeColor = Color.White;
                }

                if (e.CellValue.ToString() == "VR")
                {
                    e.Appearance.BackColor = Color.DarkGreen;
                    e.Appearance.ForeColor = Color.White;
                }

                if (e.CellValue.ToString() == "P")
                {
                    e.Appearance.BackColor = Color.LightBlue;
                    e.Appearance.ForeColor = Color.White;
                }

                if (e.CellValue.ToString() == "V")
                {
                    e.Appearance.BackColor = Color.IndianRed;
                    e.Appearance.ForeColor = Color.White;
                }

                //if (e.CellValue.ToString() == "X")
                //{
                //    e.Appearance.BackColor = Color.White;
                //    e.Appearance.ForeColor = Color.Black;
                //}

            }
        }

        private void gvBangCongChiTiet_PopupMenuShowing(object sender, DevExpress.XtraGrid.Views.Grid.PopupMenuShowingEventArgs e)
        {
            // Kiểm tra nếu menu là loại context menu cho hàng
            if (e.MenuType == DevExpress.XtraGrid.Views.Grid.GridMenuType.Row)
            {
                var column = e.HitInfo.Column;

                if (column != null)
                {
                    string columnName = column.FieldName;

                    // Hiển thị tên cột
                    //MessageBox.Show($"Cột đang được chọn: {columnName}");

                    // Kiểm tra nếu cột là "D1", không cho hiển thị menu
                    if (columnName == "D1")
                    {
                        e.Allow = false; // Ngăn hiển thị menu
                    }
                }
            }
        }

        private void LockInfoColumns()
        {
            colMaNV.OptionsColumn.AllowEdit = false;
            colHoTen.OptionsColumn.AllowEdit = false;
            NGAYCONG.OptionsColumn.AllowEdit = false;
            NGHIKHONGPHEP.OptionsColumn.AllowEdit = false;
            NGAYPHEP.OptionsColumn.AllowEdit = false;
            CONGNGAYLE.OptionsColumn.AllowEdit = false;
            CONGCHUNHAT.OptionsColumn.AllowEdit = false;
            TONGNGAYCONG.OptionsColumn.AllowEdit = false;
        }

        private void gvBangCongChiTiet_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            try
            {
                string fieldName = e.Column.FieldName;
                if (!fieldName.StartsWith("D", StringComparison.OrdinalIgnoreCase) || !int.TryParse(fieldName.Substring(1), out int dayNum) || dayNum < 1 || dayNum > 31)
                {
                    return;
                }

                object manvObj = gvBangCongChiTiet.GetRowCellValue(e.RowHandle, "MANV");
                if (manvObj == null || !int.TryParse(manvObj.ToString(), out int manv))
                    return;

                string hoten = gvBangCongChiTiet.GetRowCellValue(e.RowHandle, "HOTEN")?.ToString() ?? "";
                string newValue = e.Value?.ToString()?.Trim()?.ToUpper() ?? "";

                // Đảm bảo _MAKYCONG hợp lệ
                if (_MAKYCONG <= 0 && int.TryParse(cboNam.Text, out int namVal) && int.TryParse(cboThang.Text, out int thangVal))
                {
                    _MAKYCONG = namVal * 100 + thangVal;
                    _nam = namVal;
                    _thang = thangVal;
                }

                // 1. Cập nhật TB_KYCONGCHITIET (bảng tổng hợp D1..D31)
                _kcct.UpdateChamCong(_MAKYCONG, manv, dayNum, newValue);

                // 2. Cập nhật hoặc thêm mới bản ghi chi tiết từng ngày TB_BANGCONG_CHITIET
                TB_BANGCONG_CHITIET bcctnv = _bangcong_ct.getItem(_MAKYCONG, manv, dayNum);
                if (bcctnv == null)
                {
                    int year = _MAKYCONG / 100;
                    int month = _MAKYCONG % 100;
                    DateTime dateVal = new DateTime(year, month, dayNum);
                    bcctnv = new TB_BANGCONG_CHITIET
                    {
                        MAKYCONG = _MAKYCONG,
                        MANV = manv,
                        HOTEN = hoten,
                        IDCTY = 1,
                        NGAY = dateVal,
                        THU = dateVal.DayOfWeek == DayOfWeek.Sunday ? "Chủ nhật" : ("Thứ " + ((int)dateVal.DayOfWeek + 1)),
                        GIOVAO = "08:00",
                        GIORA = "17:00",
                        NGAYPHEP = 0,
                        CONGNGAYLE = 0,
                        CONGCHUNHAT = dateVal.DayOfWeek == DayOfWeek.Sunday ? 1 : 0,
                        CREATED_BY = 1,
                        CREATED_DATE = DateTime.Now
                    };
                    _bangcong_ct.Add(bcctnv);
                }

                bcctnv.KYHIEU = newValue;
                switch (newValue)
                {
                    case "X":
                        bcctnv.NGAYCONG = 1;
                        bcctnv.NGAYPHEP = 0;
                        bcctnv.GIOVAO = "08:00";
                        bcctnv.GIORA = "17:00";
                        break;
                    case "CD":
                        bcctnv.NGAYCONG = 1;
                        bcctnv.NGAYPHEP = 0;
                        bcctnv.GIOVAO = "22:00";
                        bcctnv.GIORA = "06:00";
                        break;
                    case "P":
                        bcctnv.NGAYPHEP = 1;
                        bcctnv.NGAYCONG = 1;
                        break;
                    case "CT":
                        bcctnv.NGAYCONG = 1;
                        bcctnv.NGAYPHEP = 0;
                        break;
                    case "V":
                        bcctnv.NGAYCONG = 0;
                        bcctnv.NGAYPHEP = 0;
                        break;
                    case "VR":
                        bcctnv.NGAYCONG = 0;
                        bcctnv.NGAYPHEP = 1;
                        break;
                    case "L":
                        bcctnv.NGAYCONG = 1;
                        bcctnv.CONGNGAYLE = 1;
                        break;
                    case "CN":
                        bcctnv.NGAYCONG = 0;
                        bcctnv.CONGCHUNHAT = 1;
                        break;
                    default:
                        break;
                }
                _bangcong_ct.Update(bcctnv);

                // 3. Tính toán lại tổng ngày công và ngày phép cho nhân viên
                decimal tongngaycong = _bangcong_ct.tongNgayCong(_MAKYCONG, manv);
                decimal tongngayphep = _bangcong_ct.tongNgayPhep(_MAKYCONG, manv);

                var kcct = _kcct.getItem(_MAKYCONG, manv);
                if (kcct != null)
                {
                    kcct.NGAYPHEP = tongngayphep;
                    kcct.TONGNGAYCONG = tongngaycong;
                    _kcct.Update(kcct, 1);
                }

                // Cập nhật lại hiển thị tổng ngày công trên dòng đang sửa
                gvBangCongChiTiet.SetRowCellValue(e.RowHandle, "TONGNGAYCONG", tongngaycong);
                gvBangCongChiTiet.SetRowCellValue(e.RowHandle, "NGAYPHEP", tongngayphep);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi đồng bộ dữ liệu sửa nhanh: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}