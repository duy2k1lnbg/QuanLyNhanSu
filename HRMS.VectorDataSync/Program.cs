using System;
using System.Linq;
using Bu.Services.AI_Services.Core;
using Bu.Services.AI_Services.Vector;
using DA;
using Bu.Services.AI_Services.Memory;

namespace VectorDataSync
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("========================================");
            Console.WriteLine("    QDRANT SEEDER TOOL (AI_READONLY)   ");
            Console.WriteLine("========================================");
            
            try
            {
                var vectorService = Bu.Services.AI_Services.AiServiceLocator.GetService<Bu.Services.AI_Services.Interfaces.IVectorService>();

                Console.WriteLine("Clearing old data in Qdrant...");
                vectorService.Clear();
                Console.WriteLine("Qdrant collection cleared and recreated.");

                using (var db = new AiEntities())
                {
                    Console.WriteLine("Connecting to Oracle AI_READONLY...");
                    
                    var employees = db.V_AI_EMPLOYEE.ToList();
                    Console.WriteLine($"Found {employees.Count} employees.");
                    
                    int count = 0;
                    foreach (var emp in employees)
                    {
                        string text = $"Nhân viên {emp.HOTEN} (Mã NV: {emp.MANV}), sinh ngày {emp.NGAYSINH:dd/MM/yyyy}, " +
                                      $"thuộc phòng ban {emp.TEN_PHONGBAN}, chức vụ {emp.TEN_CHUCVU}, bộ phận {emp.TEN_BOPHAN}, " +
                                      $"số điện thoại {emp.DIENTHOAI}, địa chỉ {emp.DIACHI}.";
                        vectorService.Add(text, "EMPLOYEE");
                        count++;
                        Console.WriteLine($"[{count}/{employees.Count}] Seeded employee: {emp.HOTEN}");
                    }



                    Console.WriteLine("========================================");
                    Console.WriteLine($"SEEDING COMPLETE! Total vectors inserted: {count}");
                    Console.WriteLine("========================================");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}");
                var inner = ex.InnerException;
                while (inner != null)
                {
                    Console.WriteLine($"INNER: {inner.Message}");
                    inner = inner.InnerException;
                }
            }
        }
    }
}
