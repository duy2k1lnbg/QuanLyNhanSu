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
            gvBangCongChiTiet.PopupMenuShowing += gvBangCongChiTiet_PopupMenuShowing; ;
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
            gcBangCongChiTiet.DataSource = _kcct.getList(_MAKYCONG);
            gvBangCongChiTiet.OptionsBehavior.Editable = false;
            CustomView(_thang, _nam);
            cboThang.Text = _thang.ToString();
            cboNam.Text = _nam.ToString();
 
        }

        public void loadBangCong()
        {
            _kcct = new KYCONGCHITIET();
            gcBangCongChiTiet.DataSource = _kcct.getList(int.Parse(cboNam.Text) * 100 + int.Parse(cboThang.Text));
            CustomView(int.Parse(cboThang.Text), int.Parse(cboNam.Text));
            gvBangCongChiTiet.OptionsBehavior.Editable = false;
        }
        private void btnPhatSinhKyCong_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            #region Check_Key
            SplashScreenManager.ShowForm(typeof(FrmWaiting), true, true);
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

            List<TB_NHANVIEN> lstNhanVien = _nhanvien.getList();
            _kcct.phatSinhKyCongChiTiet(_macty, int.Parse(cboThang.Text), int.Parse(cboNam.Text) ,1);

            foreach (var item in lstNhanVien)
            {
                for (int i = 1; i <= GetDayNumber(int.Parse(cboThang.Text), int.Parse(cboNam.Text)); i++)
                {
                    TB_BANGCONG_CHITIET bcct = new TB_BANGCONG_CHITIET();
                    bcct.MANV = item.MANV;
                    bcct.IDCTY = item.IDCTY;
                    bcct.HOTEN = item.HOTEN;
                    #region Cmt_Doi_May_Cham_Cong_Tu_Dong
                    //if (bcct.IDCALAM == "1") // ca ngày
                    //{
                    //    bcct.GIOVAO = "08:00";
                    //    bcct.GIORA = "17:00";
                    //}    
                    //else // ca đêm
                    //{
                    //    bcct.GIOVAO = "20:00";
                    //    bcct.GIORA = "06:00";
                    //}    
                    #endregion
                    bcct.GIOVAO = "08:00";
                    bcct.GIORA = "17:00";              
                    bcct.NGAY = DateTime.Parse(cboNam.Text+ "/" +cboThang.Text + "/" + i.ToString());
                    bcct.THU = ChamCong_Functions.layThuTrongTuan(int.Parse(cboNam.Text), int.Parse(cboThang.Text), i);
                    bcct.NGAYPHEP = 0;
                    bcct.CONGNGAYLE = 0;
                    bcct.CONGCHUNHAT = 0;
                    if (bcct.THU == "Chủ nhật")
                    {
                        bcct.KYHIEU = "CN";
                        bcct.NGAYCONG = 0;
                    }    
                    else
                    {
                        bcct.KYHIEU = "X";
                        bcct.NGAYCONG = 1;
                    }    
                    bcct.MAKYCONG = _MAKYCONG;
                    bcct.CREATED_BY = 1;
                    bcct.CREATED_DATE = DateTime.Now;
                    _bangcong_ct.Add(bcct);
                }    
            }    

            var kc =_kycong.getItem(int.Parse(cboNam.Text) * 100 + int.Parse(cboThang.Text));
            kc.TRANGTHAI = 1;
            _kycong.Update(kc);
            SplashScreenManager.CloseForm();
            loadBangCong();
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
            List<TB_KYCONGCHITIET> lst = _kcct.getList(_MAKYCONG);
            rptBangCongTongHop rpt = new rptBangCongTongHop(lst,_MAKYCONG.ToString());
            rpt.ShowRibbonPreviewDialog();
        }

        private void btnDong_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            this.Close();
        }

        

        private void CustomView(int thang, int nam)
        {
            gvBangCongChiTiet.RestoreLayoutFromXml(Application.StartupPath + @"\BangCong_Layout.xml");
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
            //FrmCapNhatNgayCong frm = new FrmCapNhatNgayCong();
            //frm._MAKYCONG = _MAKYCONG;
            //frm._manv = int.Parse(gvBangCongChiTiet.GetFocusedRowCellValue("MANV").ToString());
            //frm._hoten = gvBangCongChiTiet.GetFocusedRowCellValue("HOTEN").ToString();
            //frm._ngay = gvBangCongChiTiet.FocusedColumn.FieldName.ToString();
            //frm.ShowDialog();
            if (gvBangCongChiTiet.RowCount > 0) // Kiểm tra có hàng nào không
                {    
                    var focusedRowHandle = gvBangCongChiTiet.FocusedRowHandle;
                    if (focusedRowHandle >= 0) // Kiểm tra có hàng được chọn không
                    {
                        FrmCapNhatNgayCong frm = new FrmCapNhatNgayCong();
                        frm._MAKYCONG = _MAKYCONG;
                        frm._manv = int.Parse(gvBangCongChiTiet.GetFocusedRowCellValue("MANV").ToString());
                        frm._hoten = gvBangCongChiTiet.GetFocusedRowCellValue("HOTEN").ToString();
                        frm._ngay = gvBangCongChiTiet.FocusedColumn.FieldName.ToString();
                        frm.nam_f_bcct1 = int.Parse(cboNam.Text);
                        frm.thang_f1_bcct = int.Parse(cboThang.Text);
                        frm.ShowDialog();
                    }
                    else
                    {
                        MessageBox.Show($"Vui lòng chọn một hàng để cập nhật.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
               else
               {
                MessageBox.Show($"Không có dữ liệu ở trong bảng hiện tại: ", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
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
       

    }
}