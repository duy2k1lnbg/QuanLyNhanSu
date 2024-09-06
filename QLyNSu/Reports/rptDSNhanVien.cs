using DevExpress.XtraReports.UI;
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using Bu.DTO;
using System.Collections.Generic;

namespace QLyNSu.Reports
{
    public partial class rptDSNhanVien : DevExpress.XtraReports.UI.XtraReport
    {
        public rptDSNhanVien()
        {
            InitializeComponent();
        }

        List<NHANVIEN_DTO> _lstNV;

        public rptDSNhanVien(List<NHANVIEN_DTO> lstNV)
        {
            InitializeComponent();
            this._lstNV = lstNV;
            this.DataSource = lstNV;
            loadData();
        }

        void loadData()
        {
            lblMANV.DataBindings.Add("Text", _lstNV, "MANV");
            lblHOTEN.DataBindings.Add("Text", _lstNV, "HOTEN");
            lblGIOITINH.DataBindings.Add("Text", _lstNV, "TENGT");
            lblNGAYSINH.DataBindings.Add("Text", _lstNV, "NGAYSINH");
            lblCCCD.DataBindings.Add("Text", _lstNV, "CCCD");
            lblDIENTHOAI.DataBindings.Add("Text", _lstNV, "DIENTHOAI");
            lblPB.DataBindings.Add("Text", _lstNV, "TENPB");
            lblCV.DataBindings.Add("Text", _lstNV, "TENCV");
            lblTD.DataBindings.Add("Text", _lstNV, "TENTD");
            lblDT.DataBindings.Add("Text", _lstNV, "TENDT");
            lblTG.DataBindings.Add("Text", _lstNV, "TENTG");
            lblDC.DataBindings.Add("Text", _lstNV, "DIACHI");
            lblMANV.DataBindings.Add("Text", _lstNV, "MANV");
            lblMANV.DataBindings.Add("Text", _lstNV, "MANV");
            lblMANV.DataBindings.Add("Text", _lstNV, "MANV");
        }

    }
}
