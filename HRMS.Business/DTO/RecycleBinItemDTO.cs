using System;

namespace Bu.DTO
{
    public class RecycleBinItemDTO
    {
        public string ObjectType { get; set; }
        public string ObjectId { get; set; }
        public string DisplayName { get; set; }
        public decimal? EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public DateTime? DeletedDate { get; set; }
        public string DeletedBy { get; set; }
        public string Reason { get; set; }
        public bool CanRestore { get; set; } = true;
        public string BlockedReason { get; set; }
        public bool CanPurge { get; set; } = false;
    }
}
