using System;

namespace AllLive.Core.Helper
{
    public static class CoreDebug
    {
        public static Action<string> Logger { get; set; }

        public static void Log(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }
            try
            {
                Logger?.Invoke(message);
            }
            catch
            {
            }
        }
    }
}
