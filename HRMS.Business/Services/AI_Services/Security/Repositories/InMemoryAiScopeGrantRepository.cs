using System;
using System.Collections.Generic;
using System.Linq;

namespace Bu.Services.AI_Services.Security
{
    /// <summary>
    /// Triển khai In-Memory phục vụ Unit Tests độc lập:
    /// Cho phép mô phỏng chính xác hành vi của Oracle (Schema not ready, Concurrency conflict, Transaction failure, Audit diff)
    /// mà không cần kết nối database thật.
    /// </summary>
    public class InMemoryAiScopeGrantRepository : IAiScopeGrantRepository
    {
        public class MockSubject
        {
            public string SubjectType { get; set; }
            public int SubjectId { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public int? Manv { get; set; }
            public bool IsDisabled { get; set; }
        }

        public class MockAuditRecord
        {
            public int ActorUserId { get; set; }
            public string ActorUsername { get; set; }
            public string SubjectType { get; set; }
            public int SubjectId { get; set; }
            public string EventType { get; set; }
            public string Result { get; set; }
            public string Reason { get; set; }
            public string ClientType { get; set; }
            public DateTime OccurredAt { get; set; }
            public string MetadataJson { get; set; }
        }

        public bool SimulateSchemaNotReady { get; set; } = false;
        public string SimulatedNotReadyMessage { get; set; } = "Hệ thống chính sách AI chưa sẵn sàng: Bảng TB_AI_SCOPE_GRANT chưa được khởi tạo.";
        public bool SimulateConcurrencyConflict { get; set; } = false;
        public bool SimulateTransactionFailure { get; set; } = false;

        public long Revision { get; set; } = 1;

        public Dictionary<string, MockSubject> Subjects { get; set; } = new Dictionary<string, MockSubject>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<int, List<int>> UserGroups { get; set; } = new Dictionary<int, List<int>>();
        public Dictionary<string, HashSet<string>> FunctionRights { get; set; } = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, List<AiScopeGrantRecord>> GrantsStore { get; set; } = new Dictionary<string, List<AiScopeGrantRecord>>(StringComparer.OrdinalIgnoreCase);

        public List<KeyValuePair<string, string>> Departments { get; set; } = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("2", "Phòng IT"),
            new KeyValuePair<string, string>("5", "Phòng Kế toán"),
            new KeyValuePair<string, string>("10", "Phòng Nhân sự")
        };

        public List<KeyValuePair<string, string>> Companies { get; set; } = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("1", "Công ty Cổ phần HRMS"),
            new KeyValuePair<string, string>("2", "Chi nhánh Miền Nam")
        };

        public List<MockAuditRecord> Audits { get; set; } = new List<MockAuditRecord>();

        public void AddUser(int userId, string username, string fullname, int? manv = null, bool isDisabled = false, IEnumerable<string> rights = null)
        {
            string key = $"USER:{userId}";
            Subjects[key] = new MockSubject
            {
                SubjectType = "USER",
                SubjectId = userId,
                Code = username,
                Name = fullname,
                Manv = manv,
                IsDisabled = isDisabled
            };

            if (rights != null)
            {
                FunctionRights[key] = new HashSet<string>(rights, StringComparer.OrdinalIgnoreCase);
            }
        }

        public void AddGroup(int groupId, string groupCode, string groupName, bool isDisabled = false, IEnumerable<string> rights = null)
        {
            string key = $"GROUP:{groupId}";
            Subjects[key] = new MockSubject
            {
                SubjectType = "GROUP",
                SubjectId = groupId,
                Code = groupCode,
                Name = groupName,
                IsDisabled = isDisabled
            };

            if (rights != null)
            {
                FunctionRights[key] = new HashSet<string>(rights, StringComparer.OrdinalIgnoreCase);
            }
        }

        public void AddMembership(int userId, int groupId)
        {
            if (!UserGroups.TryGetValue(userId, out var list))
            {
                list = new List<int>();
                UserGroups[userId] = list;
            }
            if (!list.Contains(groupId)) list.Add(groupId);
        }

        public bool IsSchemaReady(out string notReadyReason)
        {
            if (SimulateSchemaNotReady)
            {
                notReadyReason = SimulatedNotReadyMessage;
                return false;
            }
            notReadyReason = null;
            return true;
        }

        public bool SubjectExists(string subjectType, int subjectId, out string code, out string name, out int? manv, out bool isDisabled)
        {
            string key = $"{subjectType}:{subjectId}";
            if (Subjects.TryGetValue(key, out var s))
            {
                code = s.Code;
                name = s.Name;
                manv = s.Manv;
                isDisabled = s.IsDisabled;
                return true;
            }
            code = "";
            name = "";
            manv = null;
            isDisabled = false;
            return false;
        }

        public List<int> GetUserGroupIds(int userId)
        {
            return UserGroups.TryGetValue(userId, out var list) ? new List<int>(list) : new List<int>();
        }

        public string GetGroupName(int groupId)
        {
            string key = $"GROUP:{groupId}";
            return Subjects.TryGetValue(key, out var s) ? s.Name : $"Nhóm #{groupId}";
        }

        public HashSet<string> GetUserFunctionRights(int userId, IEnumerable<int> groupIds)
        {
            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            string userKey = $"USER:{userId}";
            if (FunctionRights.TryGetValue(userKey, out var ur))
            {
                foreach (var r in ur) result.Add(r);
            }

            if (groupIds != null)
            {
                foreach (var gid in groupIds)
                {
                    string groupKey = $"GROUP:{gid}";
                    if (FunctionRights.TryGetValue(groupKey, out var gr))
                    {
                        foreach (var r in gr) result.Add(r);
                    }
                }
            }
            return result;
        }

        public HashSet<string> GetGroupFunctionRights(int groupId)
        {
            string groupKey = $"GROUP:{groupId}";
            return FunctionRights.TryGetValue(groupKey, out var gr)
                ? new HashSet<string>(gr, StringComparer.OrdinalIgnoreCase)
                : new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        public List<AiScopeGrantRecord> GetGrants(string subjectType, int subjectId)
        {
            string key = $"{subjectType}:{subjectId}";
            if (GrantsStore.TryGetValue(key, out var list))
            {
                return list.Select(CloneGrant).ToList();
            }
            return new List<AiScopeGrantRecord>();
        }

        public List<KeyValuePair<string, string>> GetDepartmentOptions()
        {
            return new List<KeyValuePair<string, string>>(Departments);
        }

        public List<KeyValuePair<string, string>> GetCompanyOptions()
        {
            return new List<KeyValuePair<string, string>>(Companies);
        }

        public long GetCurrentPolicyRevision()
        {
            return Revision;
        }

        public bool SaveGrantsTransactional(
            string subjectType,
            int subjectId,
            List<AiScopeGrantRecord> grantsToSave,
            long expectedRevision,
            int actorUserId,
            string actorUsername,
            string clientType,
            out long newRevision,
            out int affectedCount,
            out string errorMessage)
        {
            newRevision = Revision;
            affectedCount = 0;
            errorMessage = null;

            if (SimulateSchemaNotReady)
            {
                errorMessage = SimulatedNotReadyMessage;
                return false;
            }

            if (SimulateConcurrencyConflict || (expectedRevision > 0 && Revision != expectedRevision))
            {
                errorMessage = $"Xung đột phiên bản chính sách: Dữ liệu đã bị thay đổi bởi quản trị viên khác (Revision hiện tại: {Revision}, Revision của bạn: {expectedRevision}). Vui lòng tải lại trang trước khi lưu.";
                return false;
            }

            if (SimulateTransactionFailure)
            {
                errorMessage = "Lỗi trong tiến trình giao dịch: Mô phỏng lỗi commit CSDL.";
                return false;
            }

            string key = $"{subjectType}:{subjectId}";
            var existingGrants = GrantsStore.TryGetValue(key, out var exList) ? exList : new List<AiScopeGrantRecord>();

            bool AreGrantsIdentical(List<AiScopeGrantRecord> a, List<AiScopeGrantRecord> b)
            {
                if (a.Count != b.Count) return false;
                string GrantKey(AiScopeGrantRecord g) => $"{g.CapabilityCode}|{g.Effect}|{g.ScopeType}|{g.ScopeKey ?? ""}|{g.ValidFrom:yyyyMMddHHmmss}|{g.ValidTo:yyyyMMddHHmmss}";
                var setA = new HashSet<string>(a.Select(GrantKey));
                return b.All(g => setA.Contains(GrantKey(g)));
            }

            if (AreGrantsIdentical(existingGrants, grantsToSave ?? new List<AiScopeGrantRecord>()))
            {
                newRevision = Revision;
                affectedCount = 0;
                errorMessage = "NO_CHANGES";
                return true;
            }

            // Ghi đè grants
            var clonedNew = (grantsToSave ?? new List<AiScopeGrantRecord>()).Select(CloneGrant).ToList();
            long nextId = 1000;
            foreach (var g in clonedNew)
            {
                if (g.GrantId <= 0) g.GrantId = nextId++;
            }
            GrantsStore[key] = clonedNew;

            // Tăng revision
            Revision++;
            newRevision = Revision;
            affectedCount = clonedNew.Count;

            // Ghi audit
            Audits.Add(new MockAuditRecord
            {
                ActorUserId = actorUserId,
                ActorUsername = actorUsername,
                SubjectType = subjectType,
                SubjectId = subjectId,
                EventType = "AI_SCOPE_GRANT_UPDATE",
                Result = "SUCCESS",
                Reason = $"Cập nhật phân quyền tra cứu AI ({affectedCount} bản ghi) cho {subjectType} #{subjectId}. Revision mới: {newRevision}",
                ClientType = clientType ?? "TEST",
                OccurredAt = DateTime.UtcNow
            });

            return true;
        }

        private static AiScopeGrantRecord CloneGrant(AiScopeGrantRecord g)
        {
            return new AiScopeGrantRecord
            {
                GrantId = g.GrantId,
                SubjectType = g.SubjectType,
                SubjectId = g.SubjectId,
                CapabilityCode = g.CapabilityCode,
                ScopeType = g.ScopeType,
                ScopeKey = g.ScopeKey,
                Effect = g.Effect,
                IsEnabled = g.IsEnabled,
                ValidFrom = g.ValidFrom,
                ValidTo = g.ValidTo,
                CreatedAt = g.CreatedAt
            };
        }
    }
}
