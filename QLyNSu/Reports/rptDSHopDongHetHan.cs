using DevExpress.XtraReports.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using Bu.DTO;
using DevExpress.XtraPrinting;

namespace QLyNSu.Reports
{
    public class rptDSHopDongHetHan : XtraReport
    {
        private List<HOPDONG_DTO> _lstHD;

        public rptDSHopDongHetHan()
        {
            // Default constructor
        }

        public rptDSHopDongHetHan(List<HOPDONG_DTO> lstHD)
        {
            this._lstHD = lstHD;
            this.DataSource = lstHD;
            InitializeReportLayout();
        }

        private void InitializeReportLayout()
        {
            // Report settings
            this.PaperKind = DevExpress.Drawing.Printing.DXPaperKind.A4;
            this.Margins = new DevExpress.Drawing.DXMargins(50, 50, 50, 50);
            this.PageWidth = 827; // A4 portrait width in hundredths of an inch is 827
            this.PageHeight = 1169;

            // Create Bands
            var detail = new DetailBand { HeightF = 25F };
            var topMargin = new TopMarginBand { HeightF = 120F };
            var bottomMargin = new BottomMarginBand { HeightF = 50F };
            var reportFooter = new ReportFooterBand { HeightF = 80F };

            this.Bands.AddRange(new Band[] { topMargin, detail, bottomMargin, reportFooter });

            // Title Label
            var lblTitle = new XRLabel
            {
                Text = "DANH SÁCH HỢP ĐỒNG SẮP HẾT HẠN",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                LocationF = new PointF(0, 10),
                SizeF = new SizeF(727, 40),
                TextAlignment = TextAlignment.MiddleCenter
            };
            topMargin.Controls.Add(lblTitle);

            // Subtitle / Date
            var lblSub = new XRLabel
            {
                Text = $"Ngày xuất báo cáo: {DateTime.Now:dd/MM/yyyy}",
                Font = new Font("Segoe UI", 10, FontStyle.Italic),
                LocationF = new PointF(0, 50),
                SizeF = new SizeF(727, 20),
                TextAlignment = TextAlignment.MiddleCenter
            };
            topMargin.Controls.Add(lblSub);

            // Table Headers
            var tableHeader = new XRTable { LocationF = new PointF(0, 85), SizeF = new SizeF(727, 30) };
            var rowHeader = new XRTableRow();
            
            rowHeader.Cells.Add(CreateHeaderCell("Số HĐ", 120));
            rowHeader.Cells.Add(CreateHeaderCell("Nhân Viên", 180));
            rowHeader.Cells.Add(CreateHeaderCell("Ngày Ký", 100));
            rowHeader.Cells.Add(CreateHeaderCell("Ngày Hết Hạn", 100));
            rowHeader.Cells.Add(CreateHeaderCell("Lần Ký", 60));
            rowHeader.Cells.Add(CreateHeaderCell("Hệ Số", 60));
            rowHeader.Cells.Add(CreateHeaderCell("Lương Thỏa Thuận", 107));

            tableHeader.Rows.Add(rowHeader);
            topMargin.Controls.Add(tableHeader);

            // Detail Table (Data Rows)
            var tableDetail = new XRTable { LocationF = new PointF(0, 0), SizeF = new SizeF(727, 25) };
            var rowDetail = new XRTableRow();

            rowDetail.Cells.Add(CreateDataCell("SOHD", 120, TextAlignment.MiddleLeft));
            rowDetail.Cells.Add(CreateDataCell("HOTEN", 180, TextAlignment.MiddleLeft));
            rowDetail.Cells.Add(CreateDataCell("NGAYKY", 100, TextAlignment.MiddleCenter, "{0:dd/MM/yyyy}"));
            rowDetail.Cells.Add(CreateDataCell("NGAYKETTHUC", 100, TextAlignment.MiddleCenter, "{0:dd/MM/yyyy}"));
            rowDetail.Cells.Add(CreateDataCell("LANKY", 60, TextAlignment.MiddleCenter));
            rowDetail.Cells.Add(CreateDataCell("HESOLUONG", 60, TextAlignment.MiddleRight, "{0:N2}"));
            rowDetail.Cells.Add(CreateDataCell("LUONG_THOA_THUAN", 107, TextAlignment.MiddleRight, "{0:N0}"));

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