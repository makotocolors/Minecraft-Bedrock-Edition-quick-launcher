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
                    MessageBox.Show("Minecraft Bedrock is not installed on this system.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

            Thread.Sleep(5000);

            Process minecraftProcess = null;

            while (minecraftProcess == null)
            {
                minecraftProcess = Process.GetProcessesByName("Minecraft.Windows").FirstOrDefault();
                Thread.Sleep(1000);
            }

            if (!minecraftProcess.HasExited)
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
