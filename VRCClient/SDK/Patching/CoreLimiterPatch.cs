using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace VRClient.SDK.Patching
{
    internal class CoreLimiterPatch
    {
        public static void Start()
        {
            Logging.Log($"Have {Environment.ProcessorCount} processor cores.");
            ApplyAffinity();
        }

        public static void ApplyAffinity()
        {
            int processorCount = Environment.ProcessorCount;

            int coresToReserve = 2;
            int coresToUse = Math.Max(1, processorCount - coresToReserve);

            Logging.Log($"Using {coresToUse} out of {processorCount} cores (reserving {coresToReserve} cores).");

            long mask = 0;
            for (int i = 0; i < coresToUse; i++)
            {
                int coreIndex = processorCount - 1 - i; // beginne beim höchsten Kern
                if (coreIndex >= 0)
                    mask |= 1L << coreIndex;
            }

            IntPtr processHandle = Process.GetCurrentProcess().Handle;
            Logging.Log($"Assigning affinity mask: 0x{mask:X}.");
            SetProcessAffinityMask(processHandle, new IntPtr(mask));
        }

        [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern bool SetProcessAffinityMask(IntPtr hProcess, IntPtr dwProcessAffinityMask);
    }
}