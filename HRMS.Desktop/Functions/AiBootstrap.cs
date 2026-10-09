using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace QLyNSu.Functions
{
    public static class AiBootstrap
    {
        /// <summary>
        /// Kiểm tra trạng thái máy chủ AI qua Backend API (không mở tiến trình nội bộ trên máy client)
        /// </summary>
        public static async Task EnsureOllama()
        {
            try
            {
                var status = await AiApiClient.Instance.GetStatusAsync();
                if (status.Connected)
                {
                    Debug.WriteLine($">>> AI SERVICE: Backend AI online. LLM Available: {status.LlmAvailable}");
                }
                else
                {
                    Debug.WriteLine($">>> AI SERVICE: Backend AI is offline or unreachable.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($">>> AI SERVICE PROBE ERROR: {ex.Message}");
            }
        }

        /// <summary>
        /// Bảo tồn dịch vụ Ollama dùng chung: Đóng Desktop không làm dừng dịch vụ Ollama dùng chung của toàn cơ quan.
        /// </summary>
        public static void StopOllama()
        {
            // No-op: Do not kill shared service processes from desktop client.
            Debug.WriteLine(">>> AI SERVICE: Preserving shared AI services on application exit.");
        }
    }
}
