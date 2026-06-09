using System;

namespace Bu.Services.AI_Services.Vector
{
    public static class AiDataSyncHub
    {
        public static event Action<int> EmployeeChanged;
        public static event Action<int> EmployeeDeleted;

        public static void NotifyEmployeeChanged(int manv)
        {
            EmployeeChanged?.Invoke(manv);
        }

        public static void NotifyEmployeeDeleted(int manv)
        {
            EmployeeDeleted?.Invoke(manv);
        }
    }
}
