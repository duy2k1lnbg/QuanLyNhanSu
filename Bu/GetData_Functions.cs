using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bu
{
    public class GetData_Functions
    {
        public static int demSoNgayLamViecTrongThang(int thang, int nam)
        {
            int dem = 0;
            //DateTime f = new DateTime(nam, thang, 01);
            //int x = f.Month + 1;
            //while (f.Month < x)
            //{
            //    dem = dem + 1;
            //    if (f.DayOfWeek == DayOfWeek.Sunday)
            //    {
            //        dem = dem - 1;
            //    }
            //    f = f.AddDays(1);
            //}
            int soNgayTrongThang = DateTime.DaysInMonth(nam, thang);

            for (int i = 1; i <= soNgayTrongThang; i++)
            {
                DateTime currentDay = new DateTime(nam, thang, i);
                if (currentDay.DayOfWeek != DayOfWeek.Sunday) // Kiểm tra không phải Chủ Nhật
                {
                    dem++;
                }
            }
            return dem;
        }

        public static int laySoNgayCuaThang(int thang, int nam)
        {
            return DateTime.DaysInMonth(nam, thang);
        }

        public static string layThuTrongTuan(int nam, int thang, int ngay)
        {
            string thu = "";
            DateTime newDate = new DateTime(nam, thang, ngay);
            switch (newDate.DayOfWeek.ToString())
            {
                case "Monday":
                    thu = "Thứ hai";
                    break;
                case "Tuesday":
                    thu = "Thứ ba";
                    break;
                case "Wednesday":
                    thu = "Thứ tư";
                    break;
                case "Thursday":
                    thu = "Thứ năm";
                    break;
                case "Friday":
                    thu = "Thứ sáu";
                    break;
                case "Saturday":
                    thu = "Thứ bảy";
                    break;
                case "Sunday":
                    thu = "Chủ Nnật";
                    break;
            }
            return thu;
        }
    }
}
