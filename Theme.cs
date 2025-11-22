namespace AtronSuite
{
    // --- CORES E ESTILOS GLOBAIS ---
    public static class Theme
    {
        public static readonly Color BackDark = Color.FromArgb(32, 33, 36);
        public static readonly Color BackLight = Color.FromArgb(45, 48, 53);
        public static readonly Color TextMain = Color.FromArgb(232, 234, 237);
        public static readonly Color TextMuted = Color.Gray;

        // Cores dos Módulos
        public static readonly Color CleanerColor = Color.FromArgb(220, 53, 69);   // Red
        public static readonly Color UpdaterColor = Color.FromArgb(40, 167, 69);   // Green
        public static readonly Color DriverColor = Color.FromArgb(138, 43, 226);   // Purple
        public static readonly Color SentinelColor = Color.FromArgb(255, 140, 0);  // Dark Orange

        // Cores de Estado
        public static readonly Color Success = Color.FromArgb(40, 167, 69);
        public static readonly Color Accent = Color.FromArgb(100, 149, 237); // Blue
        public static readonly Color MonitorActive = Color.FromArgb(0, 191, 255); // Cyan

        public static Font FontTitle = new Font("Segoe UI", 16, FontStyle.Bold);
        public static Font FontSub = new Font("Segoe UI", 10, FontStyle.Regular);
        public static Font FontBold = new Font("Segoe UI", 10, FontStyle.Bold);
    }
}