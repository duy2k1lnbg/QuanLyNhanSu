using DevExpress.XtraReports.UI;
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Collections.Generic;

namespace QLyNSu.Reports
{
    public partial class rptBaoCaoLuongNV : DevExpress.XtraReports.UI.XtraReport
    {
        public rptBaoCaoLuongNV()
        {
            InitializeComponent();
        }

        public void BindData(DA.TB_BANGLUONG bl, string hoten, string tenpb, decimal otHours, List<DA.TB_NHANVIEN_PHUCAP> phucaps)
        {
            xrTableCell1.Text = "BẢNG LƯƠNG CHI TIẾT - THÁNG " + bl.THANG + " NĂM " + bl.NAM;
            xrTableCell3.Text = bl.MANV.ToString();
            
            xrTableCell4.Text = hoten;
            xrTableCell5.Text = tenpb;
            xrTableCell6.Text = bl.MANV.ToString();
            
            xrTableCell11.Text = string.Format("{0:N0}", bl.CONG_CHUAN ?? 0);
            
            // Lương cơ bản
            decimal dailyRate = bl.DAILY_RATE ?? 0;
            decimal congChuan = bl.CONG_CHUAN ?? 0;
            decimal luongCoBan = dailyRate * congChuan;
            xrTableCell14.Text = string.Format("{0:N0}", luongCoBan);
            
            // Phụ cấp chi tiết
            decimal pcTrachNhiem = phucaps.FirstOrDefault(x => x.IDPC == 1)?.SOTIEN ?? 0;
            decimal pcChuyenCan = phucaps.FirstOrDefault(x => x.IDPC == 2)?.SOTIEN ?? 0;
            decimal pcNhaO = phucaps.FirstOrDefault(x => x.IDPC == 3)?.SOTIEN ?? 0;
            decimal pcNgonNgu = phucaps.FirstOrDefault(x => x.IDPC == 4)?.SOTIEN ?? 0;
            decimal pcThamNien = phucaps.FirstOrDefault(x => x.IDPC == 5)?.SOTIEN ?? 0;
            decimal pcDiLai = phucaps.FirstOrDefault(x => x.IDPC == 6)?.SOTIEN ?? 0;
            decimal pcKhac = phucaps.FirstOrDefault(x => x.IDPC == 7)?.SOTIEN ?? 0;
            
            xrTableCell17.Text = string.Format("{0:N0}", pcTrachNhiem);
            xrTableCell20.Text = string.Format("{0:N0}", pcNgonNgu);
            xrTableCell23.Text = string.Format("{0:N0}", pcThamNien);
            xrTableCell26.Text = string.Format("{0:N0}", pcChuyenCan);
            xrTableCell29.Text = string.Format("{0:N0}", pcNhaO);
            xrTableCell32.Text = string.Format("{0:N0}", pcKhac);
            xrTableCell35.Text = string.Format("{0:N0}", pcDiLai);
            
            decimal sumAllowances = pcTrachNhiem + pcChuyenCan + pcNhaO + pcNgonNgu + pcThamNien + pcDiLai + pcKhac;
            xrTableCell38.Text = string.Format("{0:N0}", luongCoBan + sumAllowances);
            
            // Chi tiết công
            xrTableCell41.Text = string.Format("{0:N1}", bl.CONG_THUCTE ?? 0);
            xrTableCell42.Text = string.Format("{0:N0}", bl.LUONG_CONG_THUCTE ?? 0);
            
            xrTableCell53.Text = string.Format("{0:N1}", bl.CONG_THUCTE ?? 0);
            xrTableCell54.Text = string.Format("{0:N0}", bl.PHUCAP_CONG_THUCTE ?? 0);
            
            xrTableCell65.Text = string.Format("{0:N0}", bl.TIEN_CHUYENCAN ?? 0);
            xrTableCell68.Text = string.Format("{0:N0}", bl.TIEN_AN_CA ?? 0);
            xrTableCell71.Text = string.Format("{0:N0}", bl.KHOAN_CONG_KHAC ?? 0);
            
            decimal tongNgayCongThucTe = (bl.LUONG_CONG_THUCTE ?? 0) + (bl.PHUCAP_CONG_THUCTE ?? 0) + (bl.TIEN_CHUYENCAN ?? 0) + (bl.TIEN_AN_CA ?? 0) + (bl.KHOAN_CONG_KHAC ?? 0);
            xrTableCell77.Text = string.Format("{0:N0}", tongNgayCongThucTe);
            
            // Tăng ca
            xrTableCell83.Text = string.Format("{0:N1}", otHours);
            xrTableCell84.Text = string.Format("{0:N0}", bl.TIEN_TANGCA ?? 0);
            
            // Tổng thu nhập
            decimal totalGross = tongNgayCongThucTe + (bl.TIEN_TANGCA ?? 0);
            xrTableCell167.Text = string.Format("{0:N0}", totalGross);
            
            // Các khoản trích trừ bảo hiểm
            decimal insuranceBase = 0;
            decimal bhxh = 0;
            decimal bhyt = 0;
            decimal bhtn = 0;
            if (bl.TIEN_BHXH_TRICH != null && bl.TIEN_BHXH_TRICH > 0)
            {
                insuranceBase = bl.TIEN_BHXH_TRICH.Value / 0.105m;
                bhxh = insuranceBase * 0.08m;
                bhyt = insuranceBase * 0.015m;
                bhtn = insuranceBase * 0.01m;
            }
            
            xrTableCell171.Text = string.Format("{0:N0}", insuranceBase);
            xrTableCell174.Text = string.Format("{0:N0}", bhxh);
            xrTableCell178.Text = string.Format("{0:N0}", bhyt);
            xrTableCell182.Text = string.Format("{0:N0}", bhtn);
            xrTableCell186.Text = "0"; // Phí công đoàn mặc định 0
            
            xrTableCell190.Text = string.Format("{0:N0}", bl.KHOAN_TRU_KHAC ?? 0); // Thuế TNCN
            xrTableCell198.Text = string.Format("{0:N0}", bl.TIEN_TAMUNG ?? 0);     // Tạm ứng
            
            xrTableCell202.Text = string.Format("{0:N0}", bl.THUC_LINH ?? 0);       // Thực lĩnh
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
