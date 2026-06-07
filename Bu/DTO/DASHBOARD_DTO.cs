using System;

namespace Bu.DTO
{
    public class DashboardPhongBanDTO
    {
        public string PhongBan { get; set; }
        public double SoLuong { get; set; }
    }

    public class DashboardGioiTinhDTO
    {
        public string GioiTinh { get; set; }
        public double SoLuong { get; set; }
    }

    public class DashboardTrinhDoDTO
    {
        public string TrinhDo { get; set; }
        public double SoLuong { get; set; }
    }

    public class DashboardTuoiDTO
    {
        public string Range { get; set; }
        public double SoLuong { get; set; }
    }

    public class DashboardLuongDTO
    {
        public string KyCong { get; set; }
        public double TongLuong { get; set; }
    }

    public class DashboardLuongBinhQuanDTO
    {
        public string PhongBan { get; set; }
        public double LuongBinhQuan { get; set; }
    }

    public class DashboardTangCaDTO
    {
        public string KyCong { get; set; }
        public double TongGio { get; set; }
    }
}
