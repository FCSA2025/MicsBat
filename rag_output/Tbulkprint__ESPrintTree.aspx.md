# Documented File: ESPrintTree.aspx.cs
**Repository Path:** `Tbulkprint\ESPrintTree.aspx.cs`
**Primary Layer:** `Tbulkprint`
**Namespace:** `Tbulkprint`

## Source Code Representation
```csharp
using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

namespace Tbulkprint
{
	/// <summary>
	/// Summary description for ESPrintTree.
	/// </summary>
	public partial class ESPrintTree : System.Web.UI.Page
	{

		protected void Page_Load(object sender, System.EventArgs e)
		{
			try
			{
				sesSiteName.Value = Session["SiteName"].ToString();
			}
			catch (Exception e1)
			{
				Response.Redirect("../relogin.aspx");
			}
			txtLevel.Value = Server.HtmlDecode(Request.QueryString["key"]).ToString();
			if(txtLevel.Value == "")
			{
				txtPdfName.Value = "";
			}
			else
			{
				// split key into parts 
				char [] delimiter = ".".ToCharArray();
				string [] keyparts = txtLevel.Value.Split(delimiter);
				if(keyparts.Length > 1)
				{
					txtPdfName.Value = keyparts[1];
				}
				else
				{
					txtPdfName.Value = ":" + txtLevel.Value + ":";
				}
			}
		}

		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);
		}
		
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{    
		}
		#endregion
	}
}

```
