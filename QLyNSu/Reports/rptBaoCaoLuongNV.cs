using DevExpress.XtraReports.UI;
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;

namespace QLyNSu.Reports
{
    public partial class rptBaoCaoLuongNV : DevExpress.XtraReports.UI.XtraReport
    {
        public rptBaoCaoLuongNV()
        {
            InitializeComponent();
        }

        public XRTable CreateCustomTable(XtraReport report, string[] cellTexts, float[] cellWidths, System.Drawing.Color[] cellBackColors)
        {
            // Kiểm tra độ dài của các mảng
            if (cellTexts.Length != cellWidths.Length || cellTexts.Length != cellBackColors.Length)
            {
                throw new ArgumentException("Các mảng cellTexts, cellWidths, và cellBackColors phải có cùng độ dài.");
            }

            // Bước 1: Tạo một bảng
            XRTable table = new XRTable();
            table.Borders = DevExpress.XtraPrinting.BorderSide.All; // Thiết lập viền cho bảng
            table.WidthF = 300f; // Chiều rộng của bảng

            // Bước 2: Tạo một dòng cho bảng
            XRTableRow row = new XRTableRow();

            // Bước 3: Tạo các ô dựa trên tham số đầu vào
            for (int i = 0; i < cellTexts.Length; i++)
            {
                XRTableCell cell = new XRTableCell
                {
                    Text = cellTexts[i],
                    WidthF = cellWidths[i], // Chiều rộng ô
                    HeightF = 50f, // Chiều cao ô
                    BackColor = cellBackColors[i], // Màu nền ô
                    TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter // Căn giữa văn bản
                };
                row.Cells.Add(cell); // Thêm ô vào dòng
            }

            // Bước 4: Thêm dòng vào bảng
            table.Rows.Add(row);

            // Bước 5: Thêm bảng vào báo cáo
            report.Bands[BandKind.Detail].Controls.Add(table);

            return table; // Trả về bảng đã tạo
        }

    }
}
