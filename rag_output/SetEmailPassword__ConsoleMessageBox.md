# Documented File: ConsoleMessageBox.cs
**Repository Path:** `SetEmailPassword\ConsoleMessageBox.cs`
**Primary Layer:** `SetEmailPassword`
**Namespace:** `SetEmailPassword`

## Source Code Representation
```csharp
﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SetEmailPassword
{
    /// <summary>
    /// Implements a ConsoleMessageBox class as an extension of a standard .NET
    /// Form object.
    /// </summary>
    public partial class ConsoleMessageBox : Form
    {
        /// <summary>
        /// Constructor method for this class.
        /// </summary>
        public ConsoleMessageBox()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Event handler for mouse clicks on the 'OK' dialog button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OK_MouseClick(object sender, MouseEventArgs e)
        {
            this.Dispose();
        }

        /// <summary>
        /// Event handler for clicks on the 'OK' dialog button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OK_Click(object sender, EventArgs e)
        {

        }
    }
}

```
