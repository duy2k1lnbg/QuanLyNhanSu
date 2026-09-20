using Bu.DTO;
using DevExpress.XtraReports.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

namespace QLyNSu.Reports
{
    public partial class rptKhenThuong : DevExpress.XtraReports.UI.XtraReport
    {
        public rptKhenThuong()
        {
            InitializeComponent();
        }

        public rptKhenThuong(List<KHENTHUONG_KYLUAT_DTO> lstKT)
        {
            InitializeComponent();
            this._lstKT = lstKT;
            this.DataSource = lstKT;
            loadData();
        }
        public List<KHENTHUONG_KYLUAT_DTO> _lstKT;

        private void loadData()
        {
            lblSoQD.DataBindings.Add("Text", _lstKT, "SOQUYETDINH");
            lblNgay.DataBindings.Add("Text", _lstKT, "NGAY");
            lblTenCty.DataBindings.Add("Text", _lstKT, "TENCTY");
        }

    }
}
