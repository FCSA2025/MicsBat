# Documented File: geosats.cs
**Repository Path:** `DBAccess\geosats.cs`
**Primary Layer:** `DBAccess`
**Namespace:** `DBAccess`

## Source Code Representation
```csharp
using System;
using System.IO;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Data.Odbc;																						
using System.Text;

namespace DBAccess
{
	/// <summary>
	/// This is the class for the geostationary satellite table.
	/// </summary>
	public class geosats
	{
		dbconnect geoConn = null;


		public geosats()
		{
			geoConn = new dbconnect();
		}

		public geosat[] getlist(double dStart, double dEnd)
		{
			string cSql;
			
			if (dStart <= dEnd){
				cSql = "SELECT satNoradId, satName, satOrbit, satcLong, satLong " 
				       + "FROM tsip.geosats "
							+ "WHERE satLong >= " + dStart.ToString() + " and satLong <= " + dEnd.ToString() 
					+ " ORDER BY satLong ";
			} else {
				cSql = "SELECT satNoradId, satName, satOrbit, satcLong, satLong " 
				       + "FROM tsip.geosats "
							+ "WHERE satLong >= " + dEnd.ToString() + " and satLong <= " + dStart.ToString() 
					+ " ORDER BY satLong ";
			}
			
			DataTable oDT = geoConn.retrieve(cSql);
			geosat[] aRet = new geosat[oDT.Rows.Count];
			int	nInd = 0;
			foreach (DataRow oDR in oDT.Rows){
				aRet[nInd] = new geosat(oDR["satNoradId"].ToString(),
				                        oDR["satName"].ToString(),
				                        oDR["satOrbit"].ToString(),
				                        oDR["satcLong"].ToString(),
				                        Convert.ToDouble(oDR["satLong"]));
				nInd++;
			}
			
			return aRet;
		}
	}

	public class geosat
	{
		string m_satNoradId;
		string m_satName;
		string m_satOrbit;
		string m_satcLong;
		double m_satLong;

		public geosat(string cNorad, string cName, string cOrbit, string cLong, double dLong)
		{
			m_satNoradId = cNorad;
			m_satName = cName;
			m_satOrbit = cOrbit;
			m_satcLong = cLong;
			m_satLong = dLong;
		}
		
		public string satNoradId { get {return m_satNoradId;}}
		public string satName { get {return m_satName;}}
		public string satOrbit { get {return m_satOrbit;}}
		public string satcLong { get {return m_satcLong;}}
		public double satLong { get {return m_satLong;}}
	}
}

```
