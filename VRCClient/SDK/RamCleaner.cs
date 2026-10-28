using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace VRClient.SDK
{
    internal class RamCleaner
    {
        [DllImport("kernel32.dll")]
        private static extern bool SetProcessWorkingSetSize(IntPtr process, IntPtr minimumWorkingSetSize, IntPtr maximumWorkingSetSize);
        public static System.Timers.Timer clearTimer;
        public static float clearInterval = 120f;
        public static bool useWorkingSetTrim = true;

        public static void StartMyCleaner()
        {
            clearTimer = new System.Timers.Timer(clearInterval * 3000f);
            clearTimer.Elapsed += ClearTimer_Elapsed;
            clearTimer.AutoReset = true;
            clearTimer.Start();
        }
        public static void forceclear()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            if (useWorkingSetTrim)
            {
                SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, new IntPtr(-1), new IntPtr(-1));
            }
        }
        public static void ClearTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                if (useWorkingSetTrim)
                {
                    SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, new IntPtr(-1), new IntPtr(-1));
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}
