using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace QLyNSu.Functions
{
    public static class AiBootstrap
    {
        private const string OLLAMA_URL = "http://localhost:11434/api/tags";
        private static readonly HttpClient _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };

        /// <summary>
        /// Đảm bảo Ollama đang chạy. Nếu chưa, sẽ tự động khởi động ngầm.
        /// </summary>
        public static async Task EnsureOllama()
        {
            try
            {
                // 1. Kiểm tra xem service đã online chưa
                if (await IsOllamaRunning())
                {
                    Debug.WriteLine(">>> AI SERVICE: Ollama is already active.");
                    return;
                }

                // 2. Nếu chưa, thử lệnh khởi động
                Debug.WriteLine(">>> AI SERVICE: Attempting to start Ollama serve...");
                bool started = StartOllamaProcess();

                if (started)
                {
                    // Đợi tối đa 10s (5 lần thử, mỗi lần 2s) để service sẵn sàng
                    for (int i = 0; i < 5; i++)
                    {
                        await Task.Delay(2000);
                        if (await IsOllamaRunning())
                        {
                            Debug.WriteLine(">>> AI SERVICE: Ollama started successfully.");
                            return;
                        }
                    }
                    Debug.WriteLine(">>> AI SERVICE: Ollama is taking too long to start.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($">>> AI SERVICE ERROR: {ex.Message}");
            }
        }

        /// <summary>
        /// Kiểm tra phản hồi từ API của Ollama
        /// </summary>
        private static async Task<bool> IsOllamaRunning()
        {
            try
            {
                var response = await _httpClient.GetAsync(OLLAMA_URL);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Chạy tiến trình ollama serve ở chế độ ẩn
        /// </summary>
        private static bool StartOllamaProcess()
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "ollama",
                    Arguments = "serve",
                    CreateNoWindow = true,      // Không hiện cửa sổ CMD
                    UseShellExecute = false,    // Cần thiết để ẩn cửa sổ
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                Process.Start(startInfo);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($">>> AI SERVICE: Cannot find 'ollama' command. {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Dừng triệt để tất cả tiến trình Ollama khi thoát ứng dụng
        /// </summary>
        public static void StopOllama()
        {
            try
            {
                // Tìm cả tiến trình 'ollama' (CLI) và 'ollama app' (nếu có)
                var processes = Process.GetProcessesByName("ollama");
                foreach (var p in processes)
                {
                    Debug.WriteLine($">>> AI SERVICE: Killing process {p.Id}...");
                    p.Kill();
                    p.WaitForExit(2000); // Đợi giải phóng tài nguyên
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($">>> AI SERVICE: Error during shutdown. {ex.Message}");
            }
        }
    }
}
