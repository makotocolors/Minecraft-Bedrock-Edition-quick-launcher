using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

class Program
{
    private const string appMutexName = "Global\\QuickLauncherForMinecraftBedrockEditionApp";
    private const string launchingMutexName = "Global\\QuickLauncherForMinecraftBedrockEditionlaunching";
    private const string windowMutexName = "Global\\QuickLauncherForMinecraftBedrockEditionWindow";

    private const int maxWaitTimeMs = 10000;
    private const int checkIntervalMs = 500;

    private static Mutex appMutex;
    private static Mutex launchingMutex;
    private static Mutex windowMutex;

    [STAThread]
    static void Main()
    {
        try
        {
            if (IsAcquiredMutex(ref launchingMutex))
            {
                return;
            }

            if (!AcquireMutex(ref appMutex, appMutexName))
            {
                ShowMainWindow("\"Quick Launcher for MBE\" is already running.", "Warning");
                return;
            }

            if (!IsGameRegistered())
            {
                ShowMainWindow("\"Minecraft: Bedrock Edition\" is not installed on this system.", "Error");
                return;
            }

            LaunchGame();
            WaitForGameToExit();
        }
        catch (Exception ex)
        {
            ShowMainWindow($"An error occurred: {ex.Message}", "Error");
            return;
        }
    }

    private static bool AcquireMutex(ref Mutex mutex, string name)
    {
        bool createdNew;
        mutex = new Mutex(true, name, out createdNew);
        if (!createdNew)
        {
            return false;
        }
        return true;
    }

    private static bool IsAcquiredMutex(ref Mutex mutex)
    {
        if (mutex != null)
        {
            return true;
        }
        return false;
    }

    private static void ReleaseMutex(ref Mutex mutex)
    {
        if (mutex != null)
        {
            mutex.ReleaseMutex();
            mutex.Dispose();
            mutex = null;
        }
    }

    private static void ShowMainWindow(string message, string caption)
    {
        if (AcquireMutex(ref windowMutex, windowMutexName))
        {
            var icon = caption == "Error" ? MessageBoxIcon.Error : MessageBoxIcon.Information;
            MessageBox.Show(message, caption, MessageBoxButtons.OK, icon);
        }
        else
        {
            BackToMainWindow();
        }
    }

    private static bool IsGameRegistered()
    {
        using RegistryKey key = Registry.ClassesRoot.OpenSubKey("minecraft");
        return key != null;
    }



    private static void LaunchGame()
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = "/c start minecraft:",
            WindowStyle = ProcessWindowStyle.Hidden,
            CreateNoWindow = true,
            UseShellExecute = true
        };

        Process.Start(startInfo);
    }

    private static void WaitForGameToExit()
    {
        int waitedTime = 0;
        Process gameProcess = null;
        AcquireMutex(ref launchingMutex, launchingMutexName);

        while (waitedTime < maxWaitTimeMs)
        {
            gameProcess = Process.GetProcessesByName("Minecraft.Windows").FirstOrDefault();

            if (gameProcess != null && !gameProcess.HasExited)
            {
                break;
            }

            Thread.Sleep(checkIntervalMs);
            waitedTime += checkIntervalMs;
        }

        ReleaseMutex(ref launchingMutex);

        if (gameProcess != null && !gameProcess.HasExited)
        {
            gameProcess.WaitForExit();
        }
    }

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")]
    private static extern bool IsIconic(IntPtr hWnd);
    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    private static void BackToMainWindow()
    {
        System.Media.SystemSounds.Asterisk.Play();
        Process current = Process.GetCurrentProcess();
        var otherProcesses = Process.GetProcessesByName(current.ProcessName)
            .Where(p => p.Id != current.Id)
            .OrderBy(p => p.StartTime)
            .ToList();

        Process targetProcess = otherProcesses.Count >= 2 ? otherProcesses[1] : otherProcesses.FirstOrDefault();

        if (targetProcess != null && targetProcess.MainWindowHandle != IntPtr.Zero)
        {
            if (IsIconic(targetProcess.MainWindowHandle))
            {
                ShowWindow(targetProcess.MainWindowHandle, 9);
            }

            SetForegroundWindow(targetProcess.MainWindowHandle);
        }
    }
}
