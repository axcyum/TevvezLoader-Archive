using VRClient.Modules;
using static VRClient.Modules.UiModule;

namespace VRClient.SDK
{
    public static class Logging
    {
        public enum LType : byte
        {
            Info,
            Warning,
            Error,
            Success,
            Debug,
            Join,
            Leave,
            World,
            Crash,
            JavaScript,
            UdonDump,
            CEAddresse,
            Avatar,
            Udon,
            Heap,
            Login,
            Photon,
            Assets,
            Event
        }

        public static readonly bool _logAvailable;

        static Logging()
        {
            _logAvailable = EntryPoint._Log != null;
        }

        public static void Log(string msg, LType type = LType.Info, bool systemlog = false)
        {
            if (_logAvailable)
            {
                EntryPoint._Log.LogMessage($"[{type}] {msg}");

                if (systemlog)
                    UiModule.AddLogEntry(msg, type);
            }
        }
    }
}