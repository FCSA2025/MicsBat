using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

/// <summary>
/// This is a .NET 'forms' application that changes the value of the registry key used
/// for SMTP client authentication.
/// </summary>
/// <remarks>
/// To launch this Windows GUI application executable in Windows Explorer, right click on it and choose "Run as Administrator": 
/// this will display the following window:
/// \image html "Usage - SetEmailPassword.PNG" ""
/// </remarks>
namespace SetEmailPassword
{
    static class SetEmailPassword
    {
        [STAThread]
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MyForm());
            }
            catch (Exception e)
            {
                string msg = "\n\nSetEmailPassword.Main(): exception caught: " + e.Message;
                msg += "\n\nSetEmailPassword.Main(): stack trace: \n\n" + e.StackTrace;
                MessageBox.Show(msg);
            }

            Environment.Exit(1);
        }







    }
}
