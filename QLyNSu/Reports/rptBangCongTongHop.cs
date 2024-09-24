using DA;
using DevExpress.Office.Utils;
using DevExpress.XtraReports.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using Bu;

namespace QLyNSu.Reports
{
    public partial class rptBangCongTongHop : DevExpress.XtraReports.UI.XtraReport
    {
        public rptBangCongTongHop()
        {
            InitializeComponent();
        }

        public rptBangCongTongHop(List<TB_KYCONGCHITIET> lstKCCT, string makycong)
        {
            InitializeComponent();
            this._lstKCCT = lstKCCT;
            this._title = makycong;
            this.DataSource = _lstKCCT;
            BinData();
            RemoveInvalidDaysInMonth(makycong);
        }

        private string _title = "";

        public List<TB_KYCONGCHITIET> _lstKCCT;
        private void BinData()
        {
            lblTitle.Text = "BẢNG CÔNG TỔNG HỢP THÁNG " + _title.Substring(4) + " NĂM " + _title.Substring(0, 4);
            MANV.DataBindings.Add("Text", DataSource, "MANV");
            HOTEN.DataBindings.Add("Text", DataSource, "HOTEN");

            D1.DataBindings.Add("Text", DataSource, "D1");
            D2.DataBindings.Add("Text", DataSource, "D2");
            D3.DataBindings.Add("Text", DataSource, "D3");
            D4.DataBindings.Add("Text", DataSource, "D4");
            D5.DataBindings.Add("Text", DataSource, "D5");
            D6.DataBindings.Add("Text", DataSource, "D6");
            D7.DataBindings.Add("Text", DataSource, "D7");
            D8.DataBindings.Add("Text", DataSource, "D8");
            D9.DataBindings.Add("Text", DataSource, "D9");
            D10.DataBindings.Add("Text", DataSource, "D10");
            D11.DataBindings.Add("Text", DataSource, "D11");
            D12.DataBindings.Add("Text", DataSource, "D12");
            D13.DataBindings.Add("Text", DataSource, "D13");
            D14.DataBindings.Add("Text", DataSource, "D14");
            D15.DataBindings.Add("Text", DataSource, "D15");
            D16.DataBindings.Add("Text", DataSource, "D16");
            D17.DataBindings.Add("Text", DataSource, "D17");
            D18.DataBindings.Add("Text", DataSource, "D18");
            D19.DataBindings.Add("Text", DataSource, "D19");
            D20.DataBindings.Add("Text", DataSource, "D20");
            D21.DataBindings.Add("Text", DataSource, "D21");
            D22.DataBindings.Add("Text", DataSource, "D22");
            D23.DataBindings.Add("Text", DataSource, "D23");
            D24.DataBindings.Add("Text", DataSource, "D24");
            D25.DataBindings.Add("Text", DataSource, "D25");
            D26.DataBindings.Add("Text", DataSource, "D26");
            D27.DataBindings.Add("Text", DataSource, "D27");
            D28.DataBindings.Add("Text", DataSource, "D28");
            D29.DataBindings.Add("Text", DataSource, "D29");
            D30.DataBindings.Add("Text", DataSource, "D30");
            D31.DataBindings.Add("Text", DataSource, "D31");

            NGAYCONG.DataBindings.Add("Text", DataSource, "NGAYCONG");
            NGAYPHEP.DataBindings.Add("Text", DataSource, "NGAYPHEP");
            CONGNGAYLE.DataBindings.Add("Text", DataSource, "CONGNGAYLE");
            CONGCHUNHAT.DataBindings.Add("Text", DataSource, "CONGCHUNHAT");
            NGHIKHONGPHEP.DataBindings.Add("Text", DataSource, "NGHIKHONGPHEP");
            TONGNGAYCONG.DataBindings.Add("Text", DataSource, "TONGNGAYCONG");

        }
        #region An_Columns
        //public void HideInvalidDaysInMonth(string _MAKYCONG)
        //{
        //    // Parse _MAKYCONG to get the year and month
        //    string yearString = _MAKYCONG.Substring(0, 4);
        //    string monthString = _MAKYCONG.Substring(4);

        //    int year = int.Parse(yearString);
        //    int month = int.Parse(monthString);

        //    // Determine the number of days in the month
        //    int daysInMonth;
        //    if (month == 2) // February
        //    {
        //        daysInMonth = DateTime.IsLeapYear(year) ? 29 : 28;
        //    }
        //    else if (month == 4 || month == 6 || month == 9 || month == 11) // April, June, September, November
        //    {
        //        daysInMonth = 30;
        //    }
        //    else // January, March, May, July, August, October, December
        //    {
        //        daysInMonth = 31;
        //    }

        //    // Hide days that exceed the number of days in the month
        //    for (int day = daysInMonth + 1; day <= 31; day++)
        //    {
        //        // Hide corresponding xrTableCell in xrTable1 and xrTable2
        //        xrTable1.Rows[0].Cells[day + 1].Visible = false;
        //        xrTable2.Rows[0].Cells[day + 1].Visible = false;
        //    }

        //    // Auto adjust size to shrink the table
        //    //AutoAdjustTableWidth(xrTable1);
        //    //AutoAdjustTableWidth(xrTable2);
        //}
        #endregion

        private void RemoveInvalidDaysInMonth(string _MAKYCONG)
        {
            // Parse _MAKYCONG to get the year and month
            string yearString = _MAKYCONG.Substring(0, 4);
            string monthString = _MAKYCONG.Substring(4);

            int year = int.Parse(yearString);
            int month = int.Parse(monthString);

            // Determine the number of days in the month
            int daysInMonth;
            if (month == 2) // February
            {
                daysInMonth = DateTime.IsLeapYear(year) ? 29 : 28;
            }
            else if (month == 4 || month == 6 || month == 9 || month == 11) // April, June, September, November
            {
                daysInMonth = 30;
            }
            else // January, March, May, July, August, October, December
            {
                daysInMonth = 31;
            }

            // Remove days that exceed the number of days in the month
            for (int day = 31; day > daysInMonth; day--)
            {
                // Xóa các ô tương ứng trong xrTable1 và xrTable2
                xrTable1.Rows[0].Cells.RemoveAt(day + 1);
                xrTable2.Rows[0].Cells.RemoveAt(day + 1);
            }

            // Nếu cần, tự động điều chỉnh kích thước bảng
            AutoAdjustTableWidth(xrTable1);
            AutoAdjustTableWidth(xrTable2);
        }


        private void AutoAdjustTableWidth(XRTable table)
        {
            float totalVisibleWidth = 0;
            int visibleColumnCount = 0;
            foreach (XRTableCell cell in table.Rows[0].Cells)
            {
                if (cell.Visible)
                {
                    totalVisibleWidth += cell.Width;
                    visibleColumnCount++;
                }
            }
            if (visibleColumnCount == 0)
                return;
            float newTableWidth = totalVisibleWidth; 
            table.Width = (int)newTableWidth;
        }
    }
}
