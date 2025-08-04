using System;
using System.Diagnostics;
using Microsoft.Win32;
using System.Windows.Forms;

class Program
{
    [STAThread]
    static void Main()
    {
        bool isInstalled = Registry.ClassesRoot.OpenSubKey("minecraft") != null;

        if (!isInstalled)
        {
            MessageBox.Show("Minecraft Bedrock is not installed on this system.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        ProcessStartInfo psi = new ProcessStartInfo();
        psi.FileName = "cmd.exe";
        psi.Arguments = "/c start minecraft:";
        psi.WindowStyle = ProcessWindowStyle.Hidden;
        psi.CreateNoWindow = true;
        psi.UseShellExecute = true;

        Process.Start(psi);
    }
}
