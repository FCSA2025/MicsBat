# Documented File: WriteOnlyConsoleForm.cs
**Repository Path:** `SendLAMLreports\WriteOnlyConsoleForm.cs`
**Primary Layer:** `SendLAMLreports`
**Namespace:** `SendLAMLreports`

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

namespace SendLAMLreports
{
    public partial class WriteOnlyConsoleForm : Form
    {
        public WriteOnlyConsoleForm()
        {
            InitializeComponent();
        }

        private void bReturnToMainForm_Clicked(object sender, MouseEventArgs e)
        {
            Close();
        }
    }
}

```
