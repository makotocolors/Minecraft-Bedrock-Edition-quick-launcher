using System;
using System.Diagnostics;
using Microsoft.Win32;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

class Program
{
    [STAThread]
    static void Main()
    {
        try
        {
            using (RegistryKey key = Registry.ClassesRoot.OpenSubKey("minecraft"))
            {
                if (key == null)
                {
                    MessageBox.Show("\"Minecraft: Bedrock Edition\" is not installed on this system.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            var psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/c start minecraft:",
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                UseShellExecute = true
            };

            Process.Start(psi);

            const int maxWaitTimeMs = 10000;
            const int checkIntervalMs = 500;
            int waitedTime = 0;
            Process minecraftProcess = null;

            while (waitedTime < maxWaitTimeMs)
            {
                minecraftProcess = Process.GetProcessesByName("Minecraft.Windows").FirstOrDefault();

                if (minecraftProcess != null && !minecraftProcess.HasExited)
                {
                    break;
                }

                Thread.Sleep(checkIntervalMs);
                waitedTime += checkIntervalMs;
            }

            if (minecraftProcess != null && !minecraftProcess.HasExited)
            {
                minecraftProcess.WaitForExit();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
