using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace AtronSuite
{
    // --- HELPER WINGET ---
    public static class WingetHelper
    {
        public static List<(string Name, string Id, string Ver)> GetUpdates(bool filterDrivers)
        {
            var l = new List<(string Name, string Id, string Ver)>();
            var psi = new ProcessStartInfo { FileName = "winget", Arguments = "upgrade --include-unknown", RedirectStandardOutput = true, UseShellExecute = false, CreateNoWindow = true, StandardOutputEncoding = Encoding.UTF8 };
            var driverKeywords = new[] { "driver", "realtek", "nvidia", "amd", "intel", "geforce", "radeon", "bluetooth", "wireless", "ethernet", "audio", "display", "chipset" };
            try
            {
                using (var p = Process.Start(psi))
                {
                    string outStr = p.StandardOutput.ReadToEnd(); p.WaitForExit();
                    foreach (var line in outStr.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (line.Trim().StartsWith("Name") || line.StartsWith("-")) continue;
                        var parts = Regex.Split(line.Trim(), @"\s{2,}");
                        if (parts.Length >= 3)
                        {
                            bool isDriver = driverKeywords.Any(k => parts[0].ToLower().Contains(k) || parts[1].ToLower().Contains(k));
                            if (filterDrivers == isDriver) l.Add((parts[0], parts[1], parts.Length > 3 ? parts[3] : parts[2]));
                        }
                    }
                }
            }
            catch { }
            return l;
        }
        public static Task RunUpgrade(string id) => Task.Run(() => {
            var psi = new ProcessStartInfo { FileName = "winget", Arguments = $"upgrade --id \"{id}\" --accept-package-agreements --accept-source-agreements", RedirectStandardOutput = true, UseShellExecute = false, CreateNoWindow = true };
            using (var p = Process.Start(psi)) { p.WaitForExit(); }
        });
    }
}