using Bu.DTO;
using DA;
using DevExpress.XtraReports.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using static DevExpress.Utils.HashCodeHelper;

namespace QLyNSu.Reports
{
    public partial class rptHopDongLaoDong : DevExpress.XtraReports.UI.XtraReport
    {
        public rptHopDongLaoDong()
        {
            InitializeComponent();
        }

        public rptHopDongLaoDong(List<HOPDONG_DTO> lstHD)
        {
            InitializeComponent();
            this._lstHD = lstHD;
            this.DataSource = lstHD;
            loadData();
        }
        public List<HOPDONG_DTO> _lstHD;

        private void loadData()
        {
            lblSOHD.DataBindings.Add("Text", _lstHD, "SOHD");
        }
    }
}
