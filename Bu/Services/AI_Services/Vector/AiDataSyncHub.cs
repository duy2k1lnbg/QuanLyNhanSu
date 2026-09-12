using System;

namespace Bu.Services.AI_Services.Vector
{
    public static class AiDataSyncHub
    {
        public static event Action<int> EmployeeChanged;
        public static event Action<int> EmployeeDeleted;

        public static void NotifyEmployeeChanged(int manv)
        {
            try
            {
                EmployeeChanged?.Invoke(manv);
                QdrantOutboxManager.Instance.EnqueueEmployeeSync(manv);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AiDataSyncHub NotifyEmployeeChanged Error]: {ex.Message}");
            }
        }

        public static void NotifyEmployeeDeleted(int manv)
        {
            try
            {
                EmployeeDeleted?.Invoke(manv);
                QdrantOutboxManager.Instance.EnqueueEmployeeRemove(manv);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AiDataSyncHub NotifyEmployeeDeleted Error]: {ex.Message}");
            }
        }
    }
}
