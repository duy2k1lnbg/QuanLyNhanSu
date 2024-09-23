using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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

        //=============================================================== DBA Oracle =======================================

        static OracleConnection con = new OracleConnection();

        public static void taoKetNoi()
        {
            con.ConnectionString = "Data Source=localhost:1521/orcl; User Id=hr; Password=hr;";
            try
            {
                con.Open();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static void dongKetNoi()
        {
            con.Close();
        }

        public static DataTable getData(string query)
        {
            taoKetNoi();
            DataTable tb = new DataTable();
            OracleDataAdapter da = new OracleDataAdapter(query, con);
            da.Fill(tb);
            dongKetNoi();
            return tb;
        }

        public static DataSet getDataSet(string query)
        {
            taoKetNoi();
            OracleDataAdapter da = new OracleDataAdapter(query, con);
            OracleCommandBuilder cmdBuilder = new OracleCommandBuilder(da);
            DataSet ds = new DataSet();
            da.Fill(ds);
            dongKetNoi();
            return ds;
        }

        public static void execQuery(string qr)
        {
            //taoKetNoi();
            //OracleCommand cmd = new OracleCommand(qr, con);
            //cmd.CommandType = CommandType.Text;
            //cmd.ExecuteNonQuery(); // Thực thi câu lệnh
            //dongKetNoi();
            OracleCommand cmd = null;
            try
            {
                taoKetNoi();
                cmd = new OracleCommand(qr, con);
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery(); // Thực thi câu lệnh
            }
            catch (OracleException ex)
            {
                // Hiển thị thông báo lỗi mà không dừng chương trình
                MessageBox.Show("Lỗi khi thực thi câu lệnh SQL: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                // Hiển thị thông báo lỗi chung
                MessageBox.Show("Có lỗi xảy ra: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                dongKetNoi(); // Đảm bảo luôn đóng kết nối
            }
        }
    }
}
