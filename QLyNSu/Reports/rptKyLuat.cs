using Bu.DTO;
using DevExpress.XtraReports.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

namespace QLyNSu.Reports
{
    public partial class rptKyLuat : DevExpress.XtraReports.UI.XtraReport
    {
        public rptKyLuat()
        {
            InitializeComponent();
        }

        public rptKyLuat(List<KHENTHUONG_KYLUAT_DTO> lstKL)
        {
            InitializeComponent();
            this._lstKL = lstKL;
            this.DataSource = lstKL;
            loadData();
        }
        public List<KHENTHUONG_KYLUAT_DTO> _lstKL;

        private void loadData()
        {
            lblSoQD.DataBindings.Add("Text", _lstKL, "SOQUYETDINH");
            lblNgay.DataBindings.Add("Text", _lstKL, "NGAY");
            lblTenCty.DataBindings.Add("Text", _lstKL, "TENCTY");
        }

    }
}
