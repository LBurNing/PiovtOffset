using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

class DialogOperate
{
    public static string OpenFolder()
    {
        FolderBrowserDialog fb = new FolderBrowserDialog();
        fb.Description = "ѡ���ļ���";
        fb.RootFolder = Environment.SpecialFolder.MyComputer;
        fb.ShowNewFolderButton = false;

        if (fb.ShowDialog() == DialogResult.OK)
        {
            return fb.SelectedPath;
        }

        return null;
    }
}
