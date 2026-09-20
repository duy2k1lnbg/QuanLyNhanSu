using DA;
using DevExpress.XtraReports.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

namespace QLyNSu.Reports
{
    public partial class rptBangCongCTNV : DevExpress.XtraReports.UI.XtraReport
    {
        public rptBangCongCTNV()
        {
            InitializeComponent();
        }
        public List<TB_BANGCONG_CHITIET> _bcct;
        public rptBangCongCTNV(List<TB_BANGCONG_CHITIET> bcct)
        {
            InitializeComponent();
            this._bcct = bcct;
            this.DataSource = _bcct;
            BindingData();
        }

        private void BindingData()
        {
            lblMAKYCONG.DataBindings.Add("Text", DataSource, "MAKYCONG");
            lblMANV.DataBindings.Add("Text", DataSource, "MANV");
            lblHOTEN.DataBindings.Add("Text", DataSource, "HOTEN");
            lblNGAY.DataBindings.Add("Text", DataSource, "NGAY");
            lblTHU.DataBindings.Add("Text", DataSource, "THU");
            lblGIOVAO.DataBindings.Add("Text", DataSource, "GIOVAO");
            lblGIORA.DataBindings.Add("Text", DataSource, "GIORA");
            lblNGAYPHEP.DataBindings.Add("Text", DataSource, "NGAYPHEP");
            lblCONGNGAYLE.DataBindings.Add("Text", DataSource, "CONGNGAYLE");
            lblCONGCHUNHAT.DataBindings.Add("Text", DataSource, "CONGCHUNHAT");
            lblNGAYCONG.DataBindings.Add("Text", DataSource, "NGAYCONG");
            lblKYHIEU.DataBindings.Add("Text", DataSource, "KYHIEU");
            lblGHICHU.DataBindings.Add("Text", DataSource, "GHICHU");
        }
    }
}
