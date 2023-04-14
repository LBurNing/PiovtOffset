using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

public static class CmdUtil
{
    public static string ProcessCommand(string command, string argument = "", bool shell = false)
    {
        ProcessStartInfo start = new ProcessStartInfo(command);
        start.Arguments = argument;
        start.CreateNoWindow = false;
        start.ErrorDialog = true;
        start.UseShellExecute = shell;

        if (shell)
        {
            start.RedirectStandardOutput = false;
            start.RedirectStandardError = false;
            start.RedirectStandardInput = false;
        }
        else
        {
            start.RedirectStandardOutput = true;
            start.RedirectStandardError = true;
            start.RedirectStandardInput = true;
            start.StandardOutputEncoding = System.Text.UTF8Encoding.UTF8;
            start.StandardErrorEncoding = System.Text.UTF8Encoding.UTF8;
        }

        Process p = Process.Start(start);
        string result = p.StandardOutput.ReadToEnd();
        string info = p.StandardOutput.ReadLine();
        System.Console.WriteLine(info);

        p.WaitForExit();
        p.Close();
        return result;
    }
}
