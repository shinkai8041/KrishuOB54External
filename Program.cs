using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Krishu_X_External
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // First show Login, then Krishuupdate (auto-updater), then Main
            Application.Run(new Login());

            // Note: Login form will open Krishuupdate, which will then open Main
        }
    }
}