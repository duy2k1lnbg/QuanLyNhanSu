using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DA;
using Newtonsoft.Json.Linq;

namespace Bu.Services.AI_Services.Core
{
    public class AiConfigurationSnapshot
    {
        public string OllamaHost { get; set; } = "http://127.0.0.1:11434";
        public string AiModel { get; set; } = "qwen2.5:latest";
        public string QdrantUrl { get; set; } = "http://127.0.0.1:6333";
        public double AiTemp { get; set; } = 0.4;
        public int AiMaxTokens { get; set; } = 1000;
        public int AiCtx { get; set; } = 3072;
        public int AiTopK { get; set; } = 30;
        public double AiTopP { get; set; } = 0.8;
        public double AiRepeat { get; set; } = 1.15;
        public long Version { get; set; } = 1;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public AiConfigurationSnapshot Clone()
        {
            return new AiConfigurationSnapshot
            {
                OllamaHost = this.OllamaHost,
                AiModel = this.AiModel,
                QdrantUrl = this.QdrantUrl,
                AiTemp = this.AiTemp,
                AiMaxTokens = this.AiMaxTokens,
                AiCtx = this.AiCtx,
                AiTopK = this.AiTopK,
                AiTopP = this.AiTopP,
                AiRepeat = this.AiRepeat,
                Version = this.Version,
                UpdatedAt = this.UpdatedAt
            };
        }
    }

    public class AiConfigUpdateDto
    {
        public string OllamaHost { get; set; }
        public string AiModel { get; set; }
        public string QdrantUrl { get; set; }
        public double? AiTemp { get; set; }
        public int? AiMaxTokens { get; set; }
        public int? AiCtx { get; set; }
        public int? AiTopK { get; set; }
        public double? AiTopP { get; set; }
        public double? AiRepeat { get; set; }
        public long? ExpectedVersion { get; set; }
    }

    public class AiConfigTestResult
    {
        public bool Success { get; set; }
        public string TargetKind { get; set; } // OLLAMA or QDRANT
        public string Message { get; set; }
        public int StatusCode { get; set; }
        public long LatencyMs { get; set; }
    }

    /// <summary>
    /// Bộ điều phối cấu hình AI dùng chung giữa Desktop và Website.
    /// Nguồn dữ liệu hợp nhất từ TB_CONFIG. Cập nhật có versioning, transaction an toàn, SSRF guard và cache snapshot.
    /// </summary>
    public class AiConfigurationCoordinator
    {
        private static readonly Lazy<AiConfigurationCoordinator> _instance =
            new Lazy<AiConfigurationCoordinator>(() => new AiConfigurationCoordinator());

        public static AiConfigurationCoordinator Instance => _instance.Value;

        private readonly object _lock = new object();
        private readonly object _refreshLock = new object();
        private volatile AiConfigurationSnapshot _currentSnapshot;
        private readonly Func<MyEntities> _dbFactory;
        private DateTime _lastDbCheckTime = DateTime.MinValue;
        private bool _isDatabaseAvailable = true;

        public AiConfigurationCoordinator(Func<MyEntities> dbFactory = null)
        {
            _dbFactory = dbFactory ?? (() => new MyEntities());
            _currentSnapshot = LoadSnapshotFromDatabaseOrDefaults();
        }

        public bool IsDatabaseAvailable => _isDatabaseAvailable;

        /// <summary>
        /// Trả về bản sao snapshot phòng thủ (defensive copy) để ngăn chặn caller làm thay đổi trạng thái singleton dùng chung.
        /// Tự động thăm dò version mới từ DB mỗi 5 giây để đồng bộ liên tiến trình (cross-process sync).
        /// </summary>
        public AiConfigurationSnapshot CurrentSnapshot
        {
            get
            {
                CheckForRemoteUpdatesLightweight();
                return _currentSnapshot.Clone();
            }
        }

        private void CheckForRemoteUpdatesLightweight()
        {
            if ((DateTime.UtcNow - _lastDbCheckTime).TotalSeconds < 5) return;
            lock (_refreshLock)
            {
                if ((DateTime.UtcNow - _lastDbCheckTime).TotalSeconds < 5) return;
                _lastDbCheckTime = DateTime.UtcNow;
                try
                {
                    using (var db = _dbFactory())
                    {
                        var verRow = db.TB_CONFIG.AsNoTracking()
                            .FirstOrDefault(c => c.NAME == "AiConfigVersion");
                        if (verRow != null && long.TryParse(verRow.VALUE, out long dbVer))
                        {
                            if (dbVer > _currentSnapshot.Version)
                            {
                                Reload();
                            }
                        }
                    }
                }
                catch
                {
                    // Chế độ suy giảm: giữ bản snapshot hợp lệ gần nhất trong bộ nhớ
                }
            }
        }

        public static string NormalizeBaseUrl(string rawUrl, string defaultScheme = "http", int defaultPort = 80)
        {
            if (string.IsNullOrWhiteSpace(rawUrl))
            {
                throw new ArgumentException("Địa chỉ URL không được để trống.");
            }
            string url = rawUrl.Trim();

            // Xóa /api/generate hoặc /api/tags hoặc /collections nếu người dùng nhập nhầm đường dẫn đầy đủ
            url = Regex.Replace(url, @"/api/generate/?$", "", RegexOptions.IgnoreCase);
            url = Regex.Replace(url, @"/api/tags/?$", "", RegexOptions.IgnoreCase);
            url = Regex.Replace(url, @"/collections/?$", "", RegexOptions.IgnoreCase);
            url = url.TrimEnd('/');

            // Kiểm tra nếu đã có scheme nhưng không phải http/https (ví dụ: ftp://, file://)
            if (url.Contains("://"))
            {
                if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                    !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentException("Chỉ chấp nhận giao thức HTTP hoặc HTTPS.");
                }
            }
            else
            {
                url = defaultScheme + "://" + url;
            }

            if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                // Chỉ chấp nhận HTTP/HTTPS, không chấp nhận userinfo
                if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
                {
                    throw new ArgumentException("Chỉ chấp nhận giao thức HTTP hoặc HTTPS.");
                }
                if (!string.IsNullOrEmpty(uri.UserInfo))
                {
                    throw new ArgumentException("Địa chỉ URL không được chứa thông tin xác thực (userinfo).");
                }
                return uri.GetLeftPart(UriPartial.Authority);
            }

            throw new ArgumentException("Địa chỉ URL không hợp lệ.");
        }

        public static bool ValidateEndpointTargetSecurity(string rawUrl, out string normalizedUrl, out string errorMessage)
        {
            normalizedUrl = null;
            errorMessage = null;

            if (string.IsNullOrWhiteSpace(rawUrl))
            {
                errorMessage = "Địa chỉ URL không được để trống.";
                return false;
            }

            try
            {
                normalizedUrl = NormalizeBaseUrl(rawUrl);
            }
            catch (Exception ex)
            {
                errorMessage = "URL không hợp lệ: " + ex.Message;
                return false;
            }

            if (!Uri.TryCreate(normalizedUrl, UriKind.Absolute, out var uri))
            {
                errorMessage = "Địa chỉ URL không hợp lệ.";
                return false;
            }

            // Kiểm tra cổng mạng: chỉ cho phép các cổng chuẩn và cổng dịch vụ AI
            int port = uri.Port;
            if (port != 80 && port != 443 && port != 11434 && port != 6333 && port != 8080 && port != 8443)
            {
                errorMessage = $"Cổng mạng [{port}] không nằm trong danh mục cổng được phép sử dụng cho dịch vụ AI.";
                return false;
            }

            string host = uri.Host;
            // Chặn địa chỉ metadata của các nhà cung cấp đám mây (Cloud Metadata SSRF)
            if (host.Equals("169.254.169.254", StringComparison.OrdinalIgnoreCase) ||
                host.StartsWith("169.254.", StringComparison.OrdinalIgnoreCase))
            {
                errorMessage = "Từ chối truy cập địa chỉ IP metadata của hệ thống đám mây (169.254.x.x).";
                return false;
            }

            try
            {
                IPAddress[] addresses;
                if (IPAddress.TryParse(host, out var parsedIp))
                {
                    addresses = new[] { parsedIp };
                }
                else
                {
                    addresses = Dns.GetHostAddresses(host);
                }

                if (addresses == null || addresses.Length == 0)
                {
                    errorMessage = $"Không thể phân giải tên miền máy chủ [{host}].";
                    return false;
                }

                foreach (var ip in addresses)
                {
                    if (ip.IsIPv6LinkLocal || ip.IsIPv6Multicast || ip.IsIPv6SiteLocal)
                    {
                        errorMessage = "Từ chối truy cập địa chỉ IPv6 link-local hoặc multicast.";
                        return false;
                    }
                    if (IPAddress.IsLoopback(ip))
                    {
                        // Cho phép loopback cho môi trường AI chạy cục bộ
                        continue;
                    }
                    byte[] bytes = ip.GetAddressBytes();
                    if (bytes.Length == 4)
                    {
                        // 0.0.0.0 hoặc broadcast 255.255.255.255
                        if (bytes[0] == 0 || (bytes[0] == 255 && bytes[1] == 255 && bytes[2] == 255 && bytes[3] == 255))
                        {
                            errorMessage = "Địa chỉ IP không hợp lệ.";
                            return false;
                        }
                        // 169.254.x.x link-local
                        if (bytes[0] == 169 && bytes[1] == 254)
                        {
                            errorMessage = "Từ chối truy cập địa chỉ link-local IP.";
                            return false;
                        }
                        // Multicast 224.0.0.0 - 239.255.255.255
                        if (bytes[0] >= 224 && bytes[0] <= 239)
                        {
                            errorMessage = "Từ chối truy cập địa chỉ multicast.";
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = "Không thể kiểm tra an toàn địa chỉ mạng: " + ex.Message;
                return false;
            }

            return true;
        }

        public static bool ParseInvariantDouble(string raw, double fallback, out double result, double min = 0.0, double max = 2.0)
        {
            result = fallback;
            if (string.IsNullOrWhiteSpace(raw)) return false;
            string clean = raw.Trim().Replace(',', '.');
            if (double.TryParse(clean, NumberStyles.Float, CultureInfo.InvariantCulture, out double val))
            {
                if (!double.IsNaN(val) && !double.IsInfinity(val) && val >= min && val <= max)
                {
                    result = val;
                    return true;
                }
            }
            return false;
        }

        public static bool ParseInvariantInt(string raw, int fallback, out int result, int min = 1, int max = 128000)
        {
            result = fallback;
            if (string.IsNullOrWhiteSpace(raw)) return false;
            if (int.TryParse(raw.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int val))
            {
                if (val >= min && val <= max)
                {
                    result = val;
                    return true;
                }
            }
            return false;
        }

        public AiConfigurationSnapshot Reload()
        {
            lock (_lock)
            {
                _currentSnapshot = LoadSnapshotFromDatabaseOrDefaults();
                return _currentSnapshot.Clone();
            }
        }

        public void SetSnapshotDirect(AiConfigurationSnapshot snapshot)
        {
            if (snapshot == null) return;
            lock (_lock)
            {
                _currentSnapshot = snapshot.Clone();
            }
        }

        private AiConfigurationSnapshot LoadSnapshotFromDatabaseOrDefaults()
        {
            try
            {
                using (var db = _dbFactory())
                {
                    var configs = db.TB_CONFIG.AsNoTracking().ToList();
                    string GetVal(string name, string fallback) =>
                        configs.FirstOrDefault(c => string.Equals(c.NAME, name, StringComparison.OrdinalIgnoreCase))?.VALUE ?? fallback;

                    string host = GetVal("OllamaHost", null) ?? GetVal("OllamaUrl", null) ?? ConfigurationManager.AppSettings["OllamaHost"] ?? ConfigurationManager.AppSettings["OllamaUrl"] ?? "http://127.0.0.1:11434";
                    string qdrant = GetVal("QdrantUrl", ConfigurationManager.AppSettings["QdrantUrl"] ?? "http://127.0.0.1:6333");

                    var snap = new AiConfigurationSnapshot
                    {
                        OllamaHost = NormalizeBaseUrl(host, "http", 11434),
                        AiModel = GetVal("AiModel", ConfigurationManager.AppSettings["AiModel"] ?? "qwen2.5:latest"),
                        QdrantUrl = NormalizeBaseUrl(qdrant, "http", 6333)
                    };

                    if (ParseInvariantDouble(GetVal("AiTemp", "0.4"), 0.4, out double temp)) snap.AiTemp = temp;
                    if (ParseInvariantInt(GetVal("AiMaxTokens", "1000"), 1000, out int maxTokens, 1, 16384)) snap.AiMaxTokens = maxTokens;
                    if (ParseInvariantInt(GetVal("AiCtx", "3072"), 3072, out int ctx, 1024, 128000)) snap.AiCtx = ctx;
                    if (ParseInvariantInt(GetVal("AiTopK", "30"), 30, out int topk, 1, 100)) snap.AiTopK = topk;
                    if (ParseInvariantDouble(GetVal("AiTopP", "0.8"), 0.8, out double topp, 0.0, 1.0)) snap.AiTopP = topp;
                    if (ParseInvariantDouble(GetVal("AiRepeat", "1.15"), 1.15, out double rep, 0.01, 2.0)) snap.AiRepeat = rep;

                    string verStr = GetVal("AiConfigVersion", "1");
                    snap.Version = long.TryParse(verStr, out long v) ? v : 1;
                    snap.UpdatedAt = DateTime.UtcNow;
                    _isDatabaseAvailable = true;
                    return snap;
                }
            }
            catch (Exception ex)
            {
                _isDatabaseAvailable = false;
                System.Diagnostics.Trace.TraceWarning($"[AiConfigurationCoordinator] Database connection failed: {ex.Message}");
                if (_currentSnapshot != null)
                {
                    // Giữ bản snapshot hợp lệ trước đó khi CSDL tạm lỗi (chế độ suy giảm)
                    return _currentSnapshot;
                }
                var degraded = new AiConfigurationSnapshot
                {
                    OllamaHost = NormalizeBaseUrl(ConfigurationManager.AppSettings["OllamaHost"] ?? ConfigurationManager.AppSettings["OllamaUrl"] ?? "http://127.0.0.1:11434"),
                    AiModel = ConfigurationManager.AppSettings["AiModel"] ?? "qwen2.5:latest",
                    QdrantUrl = NormalizeBaseUrl(ConfigurationManager.AppSettings["QdrantUrl"] ?? "http://127.0.0.1:6333"),
                    Version = 0
                };
                return degraded;
            }
        }

        public async Task<(bool Success, string Message, AiConfigurationSnapshot Snapshot)> UpdateConfigurationAsync(
            AiConfigUpdateDto update,
            decimal actorUserId,
            string correlationId = null)
        {
            if (update == null)
            {
                return (false, "Dữ liệu cấu hình không hợp lệ.", _currentSnapshot.Clone());
            }

            // 1. Kiểm soát xung đột: Bắt buộc ExpectedVersion
            if (!update.ExpectedVersion.HasValue)
            {
                return (false, "Bắt buộc cung cấp phiên bản cấu hình dự kiến (ExpectedVersion) để chống xung đột ghi đè.", _currentSnapshot.Clone());
            }

            // 2. Kiểm tra toàn bộ dữ liệu hợp lệ TRƯỚC KHI mở giao dịch CSDL (không áp dụng một phần)
            string validatedOllamaHost = null;
            if (!string.IsNullOrWhiteSpace(update.OllamaHost))
            {
                if (!ValidateEndpointTargetSecurity(update.OllamaHost, out validatedOllamaHost, out string err))
                {
                    return (false, $"Địa chỉ Ollama không hợp lệ: {err}", _currentSnapshot.Clone());
                }
            }

            string validatedQdrantUrl = null;
            if (!string.IsNullOrWhiteSpace(update.QdrantUrl))
            {
                if (!ValidateEndpointTargetSecurity(update.QdrantUrl, out validatedQdrantUrl, out string err))
                {
                    return (false, $"Địa chỉ Qdrant không hợp lệ: {err}", _currentSnapshot.Clone());
                }
            }

            if (update.AiTemp.HasValue)
            {
                if (double.IsNaN(update.AiTemp.Value) || double.IsInfinity(update.AiTemp.Value) || update.AiTemp.Value < 0.0 || update.AiTemp.Value > 2.0)
                {
                    return (false, "Chat Temperature phải là số từ 0.0 đến 2.0.", _currentSnapshot.Clone());
                }
            }

            if (update.AiMaxTokens.HasValue && (update.AiMaxTokens.Value < 1 || update.AiMaxTokens.Value > 16384))
            {
                return (false, "Max Tokens phải là số nguyên từ 1 đến 16384.", _currentSnapshot.Clone());
            }

            if (update.AiCtx.HasValue && (update.AiCtx.Value < 1024 || update.AiCtx.Value > 128000))
            {
                return (false, "Context Window phải là số nguyên từ 1024 đến 128000.", _currentSnapshot.Clone());
            }

            if (update.AiTopK.HasValue && (update.AiTopK.Value < 1 || update.AiTopK.Value > 100))
            {
                return (false, "Top K phải là số nguyên từ 1 đến 100.", _currentSnapshot.Clone());
            }

            if (update.AiTopP.HasValue)
            {
                if (double.IsNaN(update.AiTopP.Value) || double.IsInfinity(update.AiTopP.Value) || update.AiTopP.Value < 0.0 || update.AiTopP.Value > 1.0)
                {
                    return (false, "Top P phải là số từ 0.0 đến 1.0.", _currentSnapshot.Clone());
                }
            }

            if (update.AiRepeat.HasValue)
            {
                if (double.IsNaN(update.AiRepeat.Value) || double.IsInfinity(update.AiRepeat.Value) || update.AiRepeat.Value <= 0.0 || update.AiRepeat.Value > 2.0)
                {
                    return (false, "Repeat Penalty phải là số lớn hơn 0.0 và nhỏ hơn hoặc bằng 2.0.", _currentSnapshot.Clone());
                }
            }

            lock (_lock)
            {
                using (var db = _dbFactory())
                using (var tx = db.Database.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        var configs = db.TB_CONFIG.ToList();

                        // Kiểm tra xung đột với bản ghi phiên bản thực tế trong DB
                        var dbVerItem = configs.FirstOrDefault(c => string.Equals(c.NAME, "AiConfigVersion", StringComparison.OrdinalIgnoreCase));
                        long currentDbVersion = (dbVerItem != null && long.TryParse(dbVerItem.VALUE, out long v)) ? v : 1;

                        if (update.ExpectedVersion.Value != currentDbVersion)
                        {
                            tx.Rollback();
                            return (false, $"Xung đột phiên bản: Cấu hình hiện tại là phiên bản {currentDbVersion}, khác với phiên bản {update.ExpectedVersion.Value} bạn đang xem. Vui lòng tải lại trước khi lưu.", _currentSnapshot.Clone());
                        }

                        void Upsert(string name, string val)
                        {
                            var existing = configs.FirstOrDefault(c => string.Equals(c.NAME, name, StringComparison.OrdinalIgnoreCase));
                            if (existing != null)
                            {
                                existing.VALUE = val;
                            }
                            else
                            {
                                // EDMX đã cấu hình StoreGeneratedPattern=Identity cho ID_CF: Không gán MAX(ID_CF)+1 thủ công
                                var item = new TB_CONFIG { NAME = name, VALUE = val };
                                db.TB_CONFIG.Add(item);
                                configs.Add(item);
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(validatedOllamaHost))
                        {
                            Upsert("OllamaHost", validatedOllamaHost);
                            Upsert("OllamaUrl", validatedOllamaHost);
                        }

                        if (!string.IsNullOrWhiteSpace(update.AiModel))
                        {
                            Upsert("AiModel", update.AiModel.Trim());
                        }

                        if (!string.IsNullOrWhiteSpace(validatedQdrantUrl))
                        {
                            Upsert("QdrantUrl", validatedQdrantUrl);
                        }

                        if (update.AiTemp.HasValue)
                        {
                            Upsert("AiTemp", update.AiTemp.Value.ToString("0.0", CultureInfo.InvariantCulture));
                        }

                        if (update.AiMaxTokens.HasValue)
                        {
                            Upsert("AiMaxTokens", update.AiMaxTokens.Value.ToString(CultureInfo.InvariantCulture));
                        }

                        if (update.AiCtx.HasValue)
                        {
                            Upsert("AiCtx", update.AiCtx.Value.ToString(CultureInfo.InvariantCulture));
                        }

                        if (update.AiTopK.HasValue)
                        {
                            Upsert("AiTopK", update.AiTopK.Value.ToString(CultureInfo.InvariantCulture));
                        }

                        if (update.AiTopP.HasValue)
                        {
                            Upsert("AiTopP", update.AiTopP.Value.ToString("0.00", CultureInfo.InvariantCulture));
                        }

                        if (update.AiRepeat.HasValue)
                        {
                            Upsert("AiRepeat", update.AiRepeat.Value.ToString("0.00", CultureInfo.InvariantCulture));
                        }

                        long nextVersion = currentDbVersion + 1;
                        Upsert("AiConfigVersion", nextVersion.ToString(CultureInfo.InvariantCulture));

                        db.SaveChanges();
                        tx.Commit();

                        // Cập nhật snapshot trong bộ nhớ bằng đúng các giá trị đã kiểm tra hợp lệ
                        var newSnap = new AiConfigurationSnapshot
                        {
                            OllamaHost = validatedOllamaHost ?? _currentSnapshot.OllamaHost,
                            AiModel = !string.IsNullOrWhiteSpace(update.AiModel) ? update.AiModel.Trim() : _currentSnapshot.AiModel,
                            QdrantUrl = validatedQdrantUrl ?? _currentSnapshot.QdrantUrl,
                            AiTemp = update.AiTemp ?? _currentSnapshot.AiTemp,
                            AiMaxTokens = update.AiMaxTokens ?? _currentSnapshot.AiMaxTokens,
                            AiCtx = update.AiCtx ?? _currentSnapshot.AiCtx,
                            AiTopK = update.AiTopK ?? _currentSnapshot.AiTopK,
                            AiTopP = update.AiTopP ?? _currentSnapshot.AiTopP,
                            AiRepeat = update.AiRepeat ?? _currentSnapshot.AiRepeat,
                            Version = nextVersion,
                            UpdatedAt = DateTime.UtcNow
                        };
                        _currentSnapshot = newSnap;
                        _lastDbCheckTime = DateTime.UtcNow;

                        string corr = correlationId ?? Guid.NewGuid().ToString("N");
                        System.Diagnostics.Trace.TraceInformation($"[AI_CONFIG_AUDIT][{corr}] User {actorUserId} updated AI config to version {nextVersion}.");

                        return (true, "Đã lưu và áp dụng cấu hình AI thành công cho toàn hệ thống.", newSnap.Clone());
                    }
                    catch (Exception ex)
                    {
                        tx.Rollback();
                        string corr = correlationId ?? Guid.NewGuid().ToString("N");
                        System.Diagnostics.Trace.TraceError($"[AI_CONFIG_ERROR][{corr}] Lỗi khi lưu cấu hình: {ex}");
                        return (false, "Đã xảy ra lỗi khi lưu cấu hình vào cơ sở dữ liệu. Vui lòng thử lại sau.", _currentSnapshot.Clone());
                    }
                }
            }
        }

        public async Task<AiConfigTestResult> TestTargetEndpointAsync(string targetKind, string rawUrl, string modelName = null)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            string kind = (targetKind ?? "").ToUpperInvariant();

            try
            {
                if (!ValidateEndpointTargetSecurity(rawUrl, out string baseUrl, out string secError))
                {
                    return new AiConfigTestResult
                    {
                        Success = false,
                        TargetKind = kind,
                        Message = secError,
                        StatusCode = 400
                    };
                }

                // Không tự follow redirects, timeout 5 giây, bảo vệ phản hồi tối đa 512KB
                using (var handler = new HttpClientHandler { AllowAutoRedirect = false })
                using (var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(5) })
                {
                    string probeUrl;
                    if (kind == "OLLAMA")
                    {
                        probeUrl = baseUrl + "/api/tags";
                    }
                    else if (kind == "QDRANT")
                    {
                        probeUrl = baseUrl + "/collections";
                    }
                    else
                    {
                        return new AiConfigTestResult { Success = false, TargetKind = kind, Message = "Loại máy chủ không được hỗ trợ (chỉ hỗ trợ OLLAMA hoặc QDRANT).", StatusCode = 400 };
                    }

                    using (var resp = await client.GetAsync(probeUrl, HttpCompletionOption.ResponseHeadersRead))
                    {
                        sw.Stop();

                        if (!resp.IsSuccessStatusCode)
                        {
                            return new AiConfigTestResult
                            {
                                Success = false,
                                TargetKind = kind,
                                Message = $"Máy chủ phản hồi mã trạng thái HTTP {(int)resp.StatusCode} {resp.ReasonPhrase}.",
                                StatusCode = (int)resp.StatusCode,
                                LatencyMs = sw.ElapsedMilliseconds
                            };
                        }

                        // Giới hạn kích thước đọc luồng tối đa 512KB để chống tấn công DoS/OOM
                        string body;
                        using (var stream = await resp.Content.ReadAsStreamAsync())
                        using (var ms = new MemoryStream())
                        {
                            byte[] buf = new byte[8192];
                            int totalRead = 0;
                            int read;
                            while ((read = await stream.ReadAsync(buf, 0, buf.Length)) > 0)
                            {
                                totalRead += read;
                                if (totalRead > 512 * 1024)
                                {
                                    return new AiConfigTestResult
                                    {
                                        Success = false,
                                        TargetKind = kind,
                                        Message = "Phản hồi từ máy chủ vượt quá giới hạn an toàn 512KB.",
                                        StatusCode = 413,
                                        LatencyMs = sw.ElapsedMilliseconds
                                    };
                                }
                                ms.Write(buf, 0, read);
                            }
                            body = Encoding.UTF8.GetString(ms.ToArray());
                        }

                        if (kind == "OLLAMA")
                        {
                            if (!string.IsNullOrWhiteSpace(modelName))
                            {
                                // Phân tích JSON tags để so sánh model chính xác thay vì tìm chuỗi con mơ hồ
                                bool modelFound = false;
                                try
                                {
                                    var jobj = JObject.Parse(body);
                                    var models = jobj["models"] as JArray;
                                    if (models != null)
                                    {
                                        string targetModel = modelName.Trim();
                                        foreach (var m in models)
                                        {
                                            string mName = m["name"]?.ToString() ?? m["model"]?.ToString();
                                            if (string.Equals(mName, targetModel, StringComparison.OrdinalIgnoreCase) ||
                                                (mName != null && targetModel.Contains(":") && string.Equals(mName, targetModel, StringComparison.OrdinalIgnoreCase)) ||
                                                (mName != null && !targetModel.Contains(":") && string.Equals(mName.Split(':')[0], targetModel, StringComparison.OrdinalIgnoreCase)))
                                            {
                                                modelFound = true;
                                                break;
                                            }
                                        }
                                    }
                                }
                                catch
                                {
                                    modelFound = body.IndexOf(modelName.Trim(), StringComparison.OrdinalIgnoreCase) >= 0;
                                }

                                if (!modelFound)
                                {
                                    return new AiConfigTestResult
                                    {
                                        Success = false,
                                        TargetKind = kind,
                                        Message = $"Kết nối Ollama thành công, nhưng model [{modelName}] chưa được cài đặt trên máy chủ.",
                                        StatusCode = (int)resp.StatusCode,
                                        LatencyMs = sw.ElapsedMilliseconds
                                    };
                                }
                            }
                        }
                        else if (kind == "QDRANT")
                        {
                            // Chỉ đọc danh sách collections, tuyệt đối không tạo collection hay upsert khi kiểm tra
                            try
                            {
                                var jobj = JObject.Parse(body);
                                string status = jobj["status"]?.ToString();
                                if (!string.Equals(status, "ok", StringComparison.OrdinalIgnoreCase) && jobj["result"] == null)
                                {
                                    return new AiConfigTestResult
                                    {
                                        Success = false,
                                        TargetKind = kind,
                                        Message = "Máy chủ phản hồi nhưng cấu trúc dữ liệu Qdrant không hợp lệ.",
                                        StatusCode = (int)resp.StatusCode,
                                        LatencyMs = sw.ElapsedMilliseconds
                                    };
                                }
                            }
                            catch
                            {
                                // Giữ kết quả HTTP thành công nếu không parse được JSON
                            }
                        }

                        return new AiConfigTestResult
                        {
                            Success = true,
                            TargetKind = kind,
                            Message = $"Kết nối máy chủ {kind} thành công từ máy API ({sw.ElapsedMilliseconds}ms).",
                            StatusCode = (int)resp.StatusCode,
                            LatencyMs = sw.ElapsedMilliseconds
                        };
                    }
                }
            }
            catch (OperationCanceledException)
            {
                sw.Stop();
                return new AiConfigTestResult
                {
                    Success = false,
                    TargetKind = kind,
                    Message = "Quá thời gian kết nối (timeout 5 giây). Vui lòng kiểm tra địa chỉ và cổng dịch vụ.",
                    StatusCode = 408,
                    LatencyMs = sw.ElapsedMilliseconds
                };
            }
            catch (Exception ex)
            {
                sw.Stop();
                return new AiConfigTestResult
                {
                    Success = false,
                    TargetKind = kind,
                    Message = "Không thể kết nối đến máy chủ: " + ex.Message,
                    StatusCode = 503,
                    LatencyMs = sw.ElapsedMilliseconds
                };
            }
        }
    }
}
