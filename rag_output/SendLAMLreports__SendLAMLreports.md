# Documented File: SendLAMLreports.cs
**Repository Path:** `SendLAMLreports\SendLAMLreports.cs`
**Primary Layer:** `SendLAMLreports`
**Namespace:** `SendLAMLreports`

## Source Code Representation
```csharp
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

/// <summary>
/// This Windows Forms application automates the sending of Licensed and Missing Link (LAML)
/// reports to FCSA members.
/// </summary>
namespace SendLAMLreports
{
    /// <summary>
    /// This class provides the Main() method for this application.
    /// </summary>
    public static class SendLAMLreports
    {
        public static MainForm thisForm;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try 
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(thisForm = new MainForm());
            }
            catch (Exception e)
            {
                MessageBox.Show(String.Format("SendLAMLreports.Main(): ERROR: exception: {0}\n{1}", e.Message, e.StackTrace), MainForm.MSGBOX_TITLE);
            }
        }





    }
}

```
