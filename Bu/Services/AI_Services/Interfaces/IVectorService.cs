using System.Collections.Generic;

namespace Bu.Services.AI_Services.Interfaces
{
    public interface IVectorService
    {
        void Add(string text, string tag = "GENERAL");
        List<string> Search(string query, string tag = null);
        void Clear();
        void RemoveByEmployeeId(int manv);
        void SyncEmployeeData(int manv);
    }
}
