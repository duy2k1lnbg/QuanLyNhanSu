using DA;
using HRMS_API.Filters;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;

namespace HRMS_API.Controllers
{
    /// <summary>
    /// API Nhật ký kiểm toán hệ thống (System Audit Trail) - Quy tắc 41
    /// Phục vụ tra cứu, giám sát và phân tích toàn bộ biến động dữ liệu từ CSDL Oracle
    /// </summary>
    [JwtAuthorize(RequireAdmin = true)]
    [RoutePrefix("api/audit")]
    public class AuditController : ApiController
    {
        /// <summary>
        /// GET: api/audit
        /// Truy vấn danh sách nhật ký kiểm toán có phân trang, lọc theo hành động, bảng, module và thời gian
        /// </summary>
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetAuditLogs(
            string search = null,
            string action = null,
            string module = null,
            string tableName = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int page = 1,
            int pageSize = 20)
        {
            try
            {
                using (var db = new MyEntities())
                {
                    string whereClause = " WHERE (1=1)";
                    var parameters = new List<OracleParameter>();

                    if (!string.IsNullOrWhiteSpace(action) && action.ToUpper() != "ALL")
                    {
                        whereClause += " AND UPPER(HANHDONG) = :pAction";
                        parameters.Add(new OracleParameter("pAction", action.Trim().ToUpper()));
                    }

                    if (!string.IsNullOrWhiteSpace(module) && module.ToUpper() != "ALL")
                    {
                        whereClause += " AND UPPER(MODULE_NAME) = :pModule";
                        parameters.Add(new OracleParameter("pModule", module.Trim().ToUpper()));
                    }

                    if (!string.IsNullOrWhiteSpace(tableName) && tableName.ToUpper() != "ALL")
                    {
                        whereClause += " AND UPPER(TEN_BANG) = :pTable";
                        parameters.Add(new OracleParameter("pTable", tableName.Trim().ToUpper()));
                    }

                    if (fromDate.HasValue)
                    {
                        whereClause += " AND THOIGIAN >= :pFromDate";
                        parameters.Add(new OracleParameter("pFromDate", fromDate.Value));
                    }

                    if (toDate.HasValue)
                    {
                        whereClause += " AND THOIGIAN <= :pToDate";
                        parameters.Add(new OracleParameter("pToDate", toDate.Value.AddDays(1)));
                    }

                    if (!string.IsNullOrWhiteSpace(search))
                    {
                        whereClause += " AND (LOWER(TEN_THUCHIEN) LIKE :pSearch OR LOWER(ID_BAN_GHI) LIKE :pSearch OR LOWER(IP_ADDRESS) LIKE :pSearch)";
                        parameters.Add(new OracleParameter("pSearch", $"%{search.Trim().ToLower()}%"));
                    }

                    string countSql = "SELECT COUNT(*) FROM HR.TB_SYS_LOG" + whereClause;
                    int total = db.Database.SqlQuery<int>(countSql, parameters.ToArray()).FirstOrDefault();

                    string selectSql = @"
                        SELECT ID_LOG, MANV_THUCHIEN, TEN_THUCHIEN, HANHDONG, TEN_BANG, ID_BAN_GHI,
                               IP_ADDRESS, MAC_ADDRESS, TEN_MAY_TINH, THOIGIAN, SESSION_ID,
                               MODULE_NAME, APP_VERSION, CHANGED_FIELDS
                        FROM HR.TB_SYS_LOG" + whereClause + " ORDER BY THOIGIAN DESC";

                    int skip = (page - 1) * pageSize;
                    var logs = db.Database.SqlQuery<AuditSummaryRow>(selectSql, parameters.Select(p => new OracleParameter(p.ParameterName, p.Value)).ToArray())
                        .Skip(skip)
                        .Take(pageSize)
                        .ToList();

                    var result = logs.Select(l => new
                    {
                        id = l.ID_LOG,
                        userId = l.MANV_THUCHIEN,
                        username = l.TEN_THUCHIEN ?? "System",
                        action = l.HANHDONG,
                        tableName = l.TEN_BANG,
                        recordId = l.ID_BAN_GHI,
                        module = l.MODULE_NAME ?? "System",
                        ipAddress = l.IP_ADDRESS ?? "127.0.0.1",
                        computerName = l.TEN_MAY_TINH,
                        timestamp = l.THOIGIAN.HasValue ? l.THOIGIAN.Value.ToString("dd/MM/yyyy HH:mm:ss") : "",
                        changedFields = l.CHANGED_FIELDS,
                        appVersion = l.APP_VERSION
                    }).ToList();

                    return Ok(new
                    {
                        success = true,
                        total = total,
                        page = page,
                        pageSize = pageSize,
                        data = result
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[GET /api/audit Error]: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi tải nhật ký kiểm toán." });
            }
        }

        /// <summary>
        /// GET: api/audit/{id}
        /// Lấy chi tiết toàn bộ dữ liệu cũ và mới của một bản ghi kiểm toán
        /// </summary>
        [HttpGet]
        [Route("{id:decimal}")]
        public IHttpActionResult GetAuditDetail(decimal id)
        {
            try
            {
                using (var db = new MyEntities())
                {
                    string sql = "SELECT * FROM HR.TB_SYS_LOG WHERE ID_LOG = :p0 AND ROWNUM = 1";
                    var log = db.Database.SqlQuery<AuditDetailRow>(sql, new OracleParameter("p0", id)).FirstOrDefault();

                    if (log == null)
                    {
                        return NotFound();
                    }

                    return Ok(new
                    {
                        success = true,
                        data = new
                        {
                            id = log.ID_LOG,
                            userId = log.MANV_THUCHIEN,
                            username = log.TEN_THUCHIEN,
                            action = log.HANHDONG,
                            tableName = log.TEN_BANG,
                            recordId = log.ID_BAN_GHI,
                            module = log.MODULE_NAME,
                            ipAddress = log.IP_ADDRESS,
                            macAddress = log.MAC_ADDRESS,
                            computerName = log.TEN_MAY_TINH,
                            timestamp = log.THOIGIAN.HasValue ? log.THOIGIAN.Value.ToString("dd/MM/yyyy HH:mm:ss") : "",
                            changedFields = log.CHANGED_FIELDS,
                            oldData = log.DU_LIEU_CU,
                            newData = log.DU_LIEU_MOI,
                            appVersion = log.APP_VERSION,
                            sessionId = log.SESSION_ID
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[GET /api/audit/{id} Error]: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Lỗi khi tải chi tiết nhật ký kiểm toán." });
            }
        }

        /// <summary>
        /// GET: api/audit/modules
        /// Lấy danh mục các module và bảng đã phát sinh nhật ký để đưa vào bộ lọc
        /// </summary>
        [HttpGet]
        [Route("modules")]
        public IHttpActionResult GetModules()
        {
            try
            {
                using (var db = new MyEntities())
                {
                    var modules = db.Database.SqlQuery<string>(
                        "SELECT DISTINCT MODULE_NAME FROM HR.TB_SYS_LOG WHERE MODULE_NAME IS NOT NULL ORDER BY MODULE_NAME"
                    ).ToList();

                    var tables = db.Database.SqlQuery<string>(
                        "SELECT DISTINCT TEN_BANG FROM HR.TB_SYS_LOG WHERE TEN_BANG IS NOT NULL ORDER BY TEN_BANG"
                    ).ToList();

                    var actions = db.Database.SqlQuery<string>(
                        "SELECT DISTINCT HANHDONG FROM HR.TB_SYS_LOG WHERE HANHDONG IS NOT NULL ORDER BY HANHDONG"
                    ).ToList();

                    return Ok(new
                    {
                        success = true,
                        modules = modules,
                        tables = tables,
                        actions = actions
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[GET /api/audit/modules Error]: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Lỗi khi tải danh mục lọc kiểm toán." });
            }
        }
    }

    public class AuditSummaryRow
    {
        public decimal ID_LOG { get; set; }
        public decimal? MANV_THUCHIEN { get; set; }
        public string TEN_THUCHIEN { get; set; }
        public string HANHDONG { get; set; }
        public string TEN_BANG { get; set; }
        public string ID_BAN_GHI { get; set; }
        public string IP_ADDRESS { get; set; }
        public string MAC_ADDRESS { get; set; }
        public string TEN_MAY_TINH { get; set; }
        public DateTime? THOIGIAN { get; set; }
        public string SESSION_ID { get; set; }
        public string MODULE_NAME { get; set; }
        public string APP_VERSION { get; set; }
        public string CHANGED_FIELDS { get; set; }
    }

    public class AuditDetailRow
    {
        public decimal ID_LOG { get; set; }
        public decimal? MANV_THUCHIEN { get; set; }
        public string TEN_THUCHIEN { get; set; }
        public string HANHDONG { get; set; }
        public string TEN_BANG { get; set; }
        public string ID_BAN_GHI { get; set; }
        public string DU_LIEU_CU { get; set; }
        public string DU_LIEU_MOI { get; set; }
        public string IP_ADDRESS { get; set; }
        public string MAC_ADDRESS { get; set; }
        public string TEN_MAY_TINH { get; set; }
        public DateTime? THOIGIAN { get; set; }
        public string SESSION_ID { get; set; }
        public string MODULE_NAME { get; set; }
        public string APP_VERSION { get; set; }
        public string CHANGED_FIELDS { get; set; }
    }
}
