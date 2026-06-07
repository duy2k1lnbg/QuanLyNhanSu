using DevExpress.XtraReports.UI;
using System;
using System.Data;
using System.Drawing;
using DevExpress.XtraPrinting;

namespace QLyNSu.Reports
{
    public class rptDSTangCa : XtraReport
    {
        public rptDSTangCa()
        {
            // Default constructor
        }

        public rptDSTangCa(DataTable dt, int makycong)
        {
            this.DataSource = dt;
            InitializeReportLayout(makycong);
        }

        private void InitializeReportLayout(int makycong)
        {
            // Report settings
            this.PaperKind = DevExpress.Drawing.Printing.DXPaperKind.A4;
            this.Margins = new DevExpress.Drawing.DXMargins(50, 50, 50, 50);
            this.PageWidth = 827;
            this.PageHeight = 1169;

            // Create Bands
            var detail = new DetailBand { HeightF = 25F };
            var topMargin = new TopMarginBand { HeightF = 125F };
            var bottomMargin = new BottomMarginBand { HeightF = 50F };
            var reportFooter = new ReportFooterBand { HeightF = 80F };

            this.Bands.AddRange(new Band[] { topMargin, detail, bottomMargin, reportFooter });

            // Title Label
            var lblTitle = new XRLabel
            {
                Text = "BÁO CÁO TỔNG HỢP GIỜ TĂNG CA",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                LocationF = new PointF(0, 10),
                SizeF = new SizeF(727, 35),
                TextAlignment = TextAlignment.MiddleCenter
            };
            topMargin.Controls.Add(lblTitle);

            // Subtitle
            int nam = makycong / 100;
            int thang = makycong % 100;
            var lblSub = new XRLabel
            {
                Text = $"Kỳ công: {thang}/{nam} - Ngày lập: {DateTime.Now:dd/MM/yyyy}",
                Font = new Font("Segoe UI", 10.5F, FontStyle.Italic),
                LocationF = new PointF(0, 48),
                SizeF = new SizeF(727, 22),
                TextAlignment = TextAlignment.MiddleCenter
            };
            topMargin.Controls.Add(lblSub);

            // Table Headers
            var tableHeader = new XRTable { LocationF = new PointF(0, 90), SizeF = new SizeF(727, 35) };
            var rowHeader = new XRTableRow();

            rowHeader.Cells.Add(CreateHeaderCell("Mã NV", 70));
            rowHeader.Cells.Add(CreateHeaderCell("Họ Tên", 150));
            rowHeader.Cells.Add(CreateHeaderCell("Ngày", 60));
            rowHeader.Cells.Add(CreateHeaderCell("Số Giờ", 75));
            rowHeader.Cells.Add(CreateHeaderCell("Loại Ca", 90));
            rowHeader.Cells.Add(CreateHeaderCell("Hệ Số", 60));
            rowHeader.Cells.Add(CreateHeaderCell("Tiền Tăng Ca", 110));
            rowHeader.Cells.Add(CreateHeaderCell("Ghi Chú", 112));

            tableHeader.Rows.Add(rowHeader);
            topMargin.Controls.Add(tableHeader);

            // Detail Table (Data Rows)
            var tableDetail = new XRTable { LocationF = new PointF(0, 0), SizeF = new SizeF(727, 25) };
            var rowDetail = new XRTableRow();

            rowDetail.Cells.Add(CreateDataCell("MANV", 70, TextAlignment.MiddleCenter));
            rowDetail.Cells.Add(CreateDataCell("HOTEN", 150, TextAlignment.MiddleLeft));
            rowDetail.Cells.Add(CreateDataCell("NGAY", 60, TextAlignment.MiddleCenter));
            rowDetail.Cells.Add(CreateDataCell("SOGIO", 75, TextAlignment.MiddleRight, "{0:N1}"));
            rowDetail.Cells.Add(CreateDataCell("TENLOAICA", 90, TextAlignment.MiddleCenter));
            rowDetail.Cells.Add(CreateDataCell("HESOLOAICA", 60, TextAlignment.MiddleCenter, "{0:N1}"));
            rowDetail.Cells.Add(CreateDataCell("SOTIENTC", 110, TextAlignment.MiddleRight, "{0:N0}"));
            rowDetail.Cells.Add(CreateDataCell("GHICHU", 112, TextAlignment.MiddleLeft));

            tableDetail.Rows.Add(rowDetail);
            detail.Controls.Add(tableDetail);

            // Footer Label
            var lblFooter = new XRLabel
            {
                Text = "Người lập biểu\n(Ký và ghi rõ họ tên)",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                LocationF = new PointF(477, 10),
                SizeF = new SizeF(250, 60),
                TextAlignment = TextAlignment.TopCenter
            };
            reportFooter.Controls.Add(lblFooter);
        }

        private XRTableCell CreateHeaderCell(string text, float width)
        {
            var cell = new XRTableCell
            {
                Text = text,
                WidthF = width,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                BackColor = Color.LightGray,
                ForeColor = Color.Black,
                TextAlignment = TextAlignment.MiddleCenter,
                Borders = BorderSide.All,
                BorderColor = Color.DarkGray
            };
            return cell;
        }

        private XRTableCell CreateDataCell(string fieldName, float width, TextAlignment alignment, string format = "")
        {
            var cell = new XRTableCell
            {
                WidthF = width,
                Font = new Font("Segoe UI", 9.5F),
                TextAlignment = alignment,
                Borders = BorderSide.All,
                BorderColor = Color.LightGray
            };
            cell.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", $"[{fieldName}]"));
            if (!string.IsNullOrEmpty(format))
            {
                cell.TextFormatString = format;
            }
            return cell;
        }
    }
}