using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

public static class CmdUtil
{
    public static void ProcessCommand(string command, string argument = "")
    {
        ProcessStartInfo start = new ProcessStartInfo(command);
        start.Arguments = argument;
        start.CreateNoWindow = false;
        start.ErrorDialog = true;
        start.UseShellExecute = true;
        Process p = Process.Start(start);

        p.WaitForExit();
        p.Close();
        return;
    }
}
