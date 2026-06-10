# Documented File: EsRadio.cs
**Repository Path:** `DBAccess\EsRadio.cs`
**Primary Layer:** `DBAccess`
**Namespace:** `DBAccess`

## Source Code Representation
```csharp
using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Web;																											
using System.Web.SessionState;
using System.Web.UI;
using System.Data.Odbc;
using System.Text;
using System.Xml;


namespace DBAccess {
	public enum eEsType {fe, me};

	/////////////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// This is the top level xml storage for the radios returned.  It creates an
	/// xml document.
	/// </summary>
	public class EsFile: DocFile 
	{
		/// <summary>
		/// Create an XmlDocument that also represents a file of radios.
		///	The radios themselves can be added manually to the DocumentElement of
		///	this document.  The document element is called es_radios.
		/// </summary>
		/// <param name="inName">The name of the file to be created.</param>
		public EsFile(string inName):
		       base()
		{
			m_name = inName;
			this.AppendChild(this.CreateElement("es_radios"));
		}


		/// <summary>
		/// After creating or obtaining a radio element; this method is used to
		///	add it to the XML document of all the radios.
		/// </summary>
		/// <param name="EsRadio">An Es radio</param>
		public void EsAddRadio(EsRadio oRad)
		{
			XmlNode XmlSite;
			
			if (oRad.Type == eEsType.me){
				XmlSite = this.DocumentElement.AppendChild((oRad as meRadio).meRadioXml(this));
			} else {
				XmlSite = this.DocumentElement.AppendChild((oRad as feRadio).feRadioXml(this));
			}
/*
			xmlSite = this.DocumentElement.AppendChild(oRad.EsRadioXml(this));
			//	Add the antennas node:-
			XmlNode xmlAnts = xmlSite.AppendChild(CreateElement("es_antennas"));
			//	Add the antennas
			foreach (EsAntenna antenna in oRad.Antennas){
				xmlAnts.AppendChild(antenna.EsAntennaXml(oRad.Type, this));
			}
			// Add the channel node:-
			XmlNode xmlChan = xmlSite.AppendChild(CreateElement("es_channels"));
			//	And now add the channels to this node
			foreach(EsChannel channel in oRad.Channels){
				xmlChan.AppendChild(channel.EsChannelXml(oRad.Type, this));
			}
			// Add the Azimuths node:-
			XmlNode xmlAz = xmlSite.AppendChild(CreateElement("es_azimuths"));
			//	And now add the channels to this node
			foreach(EsAzimuth az in oRad.Azimuths){
				xmlAz.AppendChild(az.EsAzimuthXml(oRad.Type, this));
			}
*/
		}
		

		/// <summary>
		/// This method returns an ArrayList of EsRadios that satisfy an SQL 
		///	where clause.  The site fields are referred to by s., the antennas
		///	by a. and the channels by c..  There is no reference to azimuths as
		///	they will be pulled in for each antenna.
		/// </summary>
		/// <param name="etypein">me or fe; probably me is most used</param>
		/// <param name="sWhere">an SQL where clause that details what is to be 
		///		brought in</param>
		/// <param name="sTable">The public name of the table or either blank or
		/// null for the mdb.</param>
		/// <param name="nDepth">1 - locations only, 2 - antennas as well, 3 - channels too.  
		///	Azimuths are always brought in for 2 and 3.</param>
		/// <returns>An ArrayList of EsRadio objects.</returns>
		public ArrayList EsRadiosFromSql(DBAccess.eEsType	etypein, 
																		 string						sWhere, 
																		 string						sTable,
																		 int							nDepth) 
		{
			string		sLocTable;
			string		sAntTable;
			string		sChnTable;
			string		sAzTable;

			if (etypein == eEsType.me){
				sLocTable = "main.me_site";
				sAntTable = "main.me_ante";
				sChnTable = "main.me_chan";
				sAzTable	= "main.me_azim";
			} else {
				sLocTable = "fe_" + sTable.Trim() + "_site";
				sAntTable = "fe_" + sTable.Trim() + "_ante";
				sChnTable = "fe_" + sTable.Trim() + "_chan";
				sAzTable  = "fe_" + sTable.Trim() + "_azim";
			}	
			dbconnect oConnection = new dbconnect();
			
			//  Construct the SQL statement depending on the depth.  This will determine both the
			//	fields that are pulled in and the tables that are referred to.
			string	sSQL = "SELECT s.*, o.nameop";
			string	sFrom = " FROM " + sLocTable + " s left outer join main.sd_oper o on s.oper=o.oper";
			string  sOrder = " ORDER BY s.location";
			if (nDepth > 1){
				//	Add the antenna fields and tables 
				sSQL += ", a.location, a.call1, a.txband, a.rxband, a.acodetx, a.acoderx, a.g_t, " +
				        "a.lnat, a.aht, a.afslt, a.afslr, a.txhgmax, a.rxhgmax, a.satlongit, a.satlong, " +
				        "a.satlongs, a.az, a.el, a.sarc1, a.sarc2, a.rxpre, a.txpre, a.rxtro, a.txtro, " +
				        "a.licence, a.satname, a.stata, a.nota, a.op2, a.antref, a.orbit, " +
				        "a.mdate as amdate, a.mtime as amtime, a.userid as auserid ";
				sFrom += " left outer join " + sAntTable + " a on s.location = a.location";
				sOrder += ", a.call1";
				
				//	Add the channel fields and tables if necessary
				if (nDepth > 2){
					sSQL += ", c.location, c.call1, c.chid, c.freqtx, c.poltx, c.maxtxpower, c.pwrtx, " +
					        "c.p4khz, c.eqpttx, c.traftx, c.stattx, c.feetx, c.	freqrx, c.polrx, c.pwrrx, " +
					        "c.eqptrx, c.trafrx, c.statrx, c.i20, c.it01, c.ip01, c.feerx, c.notc, " +
					        "c.srvctx, c.srvcrx, c.mdate as cmdate, c.mtime cmtime, c.userid as cuserid";
					sFrom += " left outer join " + sChnTable + 
					         " c on (a.location = c.location and a.call1 = c.call1)";
					sOrder += ", c.chid";
				} 
			} else {
			}

			OdbcDataAdapter oAdapter = null;																 
			sSQL += sFrom + " WHERE " + sWhere + sOrder;
			try{
				oAdapter = new OdbcDataAdapter(sSQL, oConnection.Connection);
			} catch (Exception Ex){
				throw (new Exception( "ODBC Error:-\nSQL: " + sSQL + "\n>>" + Ex.Message));
			}
			
			DataSet oDS;
			DataTable oDT;
			try{
				oDS = new DataSet();
			} catch (Exception Ex){
				throw (new Exception("New Dataset Error:\n" + Ex.Message + "\nSQL: " + sSQL));
			}
			try{
				oAdapter.Fill(oDS);
				// We assume that we have retrieved only one record (or zero)
				oDT = oDS.Tables[0];
			} catch (Exception Ex){
				throw (new Exception("Fill Error:\n" + Ex.Message + "\nSQL: " + sSQL));
			}

			DataRow oDR = null;
			ArrayList alSites = new ArrayList();
			EsRadio eRadio = null;
			EsAntenna eAnte = null;
			EsChannel eChan = null;
			if (oDT.Rows.Count > 0){
				//	Got something. 
				try{ 
				for (int nRow = 0; nRow < oDT.Rows.Count; nRow++){
					oDR = oDT.Rows[nRow];
					if (eRadio == null || oDR["location"].ToString() != eRadio.location){
						if (eRadio != null){
							if (nDepth > 1){
								try{
									if (etypein == eEsType.me){
										(eRadio as meRadio).meAddAnt((meAntenna) eAnte);
									} else {
										(eRadio as feRadio).feAddAnt((feAntenna) eAnte);
									}
									if (nDepth > 3){
										//	When we add an antenna, add its azimuths
										eRadio.EsAddAzimuth(eAnte.call1, sTable);
									}
								} catch (Exception ex){
									throw new Exception("Could not add antenna: " + ex.Message);
								}
							}
							if (etypein == eEsType.me){
								alSites.Add(eRadio as meRadio);
							} else {
								alSites.Add(eRadio as feRadio);
							}
						}
						if (etypein == eEsType.me){
							eRadio = new meRadio(oDR);
						} else {
							eRadio = new feRadio(oDR);
						}
						eAnte = null;
					}
					
					// If new antenna, add the antenna
					if (nDepth > 1 &&
					    (eAnte == null || 
					     (oDR["location"].ToString() != eAnte.location ||
					      oDR["call1"].ToString() != eAnte.call1))){
						if (eAnte != null){
							if (etypein == eEsType.me){
								(eRadio as meRadio).meAddAnt((meAntenna) eAnte);
							} else {
								(eRadio as feRadio).feAddAnt((feAntenna) eAnte);
							}
							//	When we add an antenna, add its azimuths
							if (nDepth > 3){
								eRadio.EsAddAzimuth(eAnte.call1, sTable);
							}
						}
						if (etypein == eEsType.me){
							eAnte = new meAntenna(oDR);
						} else {
							eAnte = new feAntenna(oDR);
						}
					}
					
					if (nDepth > 2){
						//	Now add the channels
						if (etypein == eEsType.me){
							eChan = new meChannel(oDR);
						} else {
							eChan = new feChannel(oDR);
						}
						if (etypein == eEsType.me){
							(eRadio as meRadio).meAddChn((meChannel) eChan);
						} else {
							(eRadio as feRadio).feAddChn((feChannel) eChan);
						}
					}
				}
		
				if (nDepth > 1){
					if (etypein == eEsType.me){
						(eRadio as meRadio).meAddAnt((meAntenna) eAnte);
					} else {
						(eRadio as feRadio).feAddAnt((feAntenna) eAnte);
					}
					if (nDepth > 3){
						//	When we add an antenna, add its azimuths
						eRadio.EsAddAzimuth(eAnte.call1, sTable);
					}
				}
				
				if (etypein == eEsType.me){
					alSites.Add(eRadio as meRadio);
				} else {
					alSites.Add(eRadio as feRadio);
				}
				
				} catch (Exception Ex){
					throw (new Exception("\n>>" + Ex.Message + "\n>>" + Ex.StackTrace));
				}
			}
			
			oConnection.dbdisconnect();
			
			return (alSites);
		}
				               

		/// <summary>
		/// This routine returns an ArrayList of es sites that have their locations stored in
		/// the ptr table and which also satisfy the sql query requirements.
		/// </summary>
		/// <param name="etypein">The fe or me type</param>
		/// <param name="sMarker">This is the marker value in the column of the ptr table</param>
		/// <param name="sWhere">The additional sql where clause</param>
		/// <param name="sTable">The table name if this is an fe table.</param>
		/// <param name="nDepth">Depth is 1, 2, 3, or 4</param>
		/// <returns>An ArrayList of the found ES sites.</returns>
		public ArrayList EsRadiosFromPtr(DBAccess.eEsType	etypein, 
		                                 string						sMarker,
																		 string						sWhere, 
																		 string						sTable,
																		 int							nDepth) 
		{
			string		sLocTable;
			string		sAntTable;
			string		sChnTable;
			string		sAzTable;

			if (etypein == eEsType.me){
				sLocTable = "main.me_site";
				sAntTable = "main.me_ante";
				sChnTable = "main.me_chan";
				sAzTable	= "main.me_azim";
			} else {
				sLocTable = "fe_" + sTable.Trim() + "_site";
				sAntTable = "fe_" + sTable.Trim() + "_ante";
				sChnTable = "fe_" + sTable.Trim() + "_chan";
				sAzTable  = "fe_" + sTable.Trim() + "_azim";
			}	
			dbconnect oConnection = new dbconnect();
                    			
			//  Construct the SQL statement depending on the depth.  This will determine both the
			//	fields that are pulled in and the tables that are referred to.
			string	sSQL = "SELECT s.*, o.nameop";
			string	sFrom = "(ptrtable p left outer join " + sLocTable + 
			                " s on p.ptr=s.location) left outer join main.sd_oper o on s.oper=o.oper";
			string  sOrder = " ORDER BY s.location";
			if (nDepth > 1){
				//	Add the antenna fields and tables 
				sSQL += ",a.location,a.call1,a.txband,a.rxband,a.acodetx,a.acoderx,a.g_t," +
				        "a.lnat,a.aht,a.afslt,a.afslr,a.txhgmax,a.rxhgmax,a.satlongit,a.satlong," +
				        "a.satlongs,a.az,a.el,a.sarc1,a.sarc2,a.rxpre,a.txpre,a.rxtro,a.txtro," +
				        "a.licence,a.satname,a.stata,a.nota,a.op2,a.antref,a.orbit," +
				        "a.mdate as amdate,a.mtime as amtime,a.userid as auserid ";
				sFrom = "(" + sFrom + ")";
				sFrom += " left outer join " + sAntTable + " a on s.location = a.location";
				sOrder += ",a.call1";
				
				//	Add the channel fields and tables if necessary
				if (nDepth > 2){
					sSQL += ",c.location,c.call1,c.chid,c.freqtx,c.poltx,c.maxtxpower,c.pwrtx," +
					        "c.p4khz,c.eqpttx,c.traftx,c.stattx,c.feetx,c.freqrx,c.polrx,c.pwrrx," +
					        "c.eqptrx,c.trafrx,c.statrx,c.i20,c.it01,c.ip01,c.feerx,c.notc," +
					        "c.srvctx,c.srvcrx,c.mdate as cmdate,c.mtime as cmtime,c.userid as cuserid";
					sFrom = "(" + sFrom + ")";
					sFrom += " left outer join " + sChnTable + 
					         " c on (a.location = c.location and a.call1 = c.call1)";
					sOrder += ",c.chid";
				} 
			} else {
			}

			OdbcDataAdapter oAdapter = null;
			
			if (sWhere == null || sWhere == ""){
				//	This is to handle an ingres anomaly with complex joins (I think).
				sWhere = " s.location!='' ";
			}			
			sWhere = " and " + sWhere;

			sSQL += " FROM " + sFrom + " WHERE p.Marker='" + sMarker + "'" + sWhere + sOrder;
//throw new Exception("\nDepth: " + nDepth.ToString() + "\nsWhere[" + sWhere + "]=null: " + 
//                    (sWhere == null).ToString() + 
//                    "\nSQL: [" + sSQL + "]");
			try{
				oAdapter = new OdbcDataAdapter(sSQL, oConnection.Connection);
			} catch (Exception Ex){
				throw (new Exception("ODBC Error:-\nSQL: " + sSQL + "\n>>" + Ex.Message));
			}
			
			DataSet oDS;
			DataTable oDT;
			try{
				oDS = new DataSet();
			} catch (Exception Ex){
				throw (new Exception("New Dataset Error:\n" + Ex.Message + "\nSQL: " + sSQL));
			}
			try{
				oAdapter.Fill(oDS);
				// We assume that we have retrieved only one record (or zero)
				oDT = oDS.Tables[0];
			} catch (Exception Ex){
				throw (new Exception("Fill Error:\n" + Ex.Message + "\nSQL: " + sSQL));
			}

			DataRow oDR = null;
			ArrayList alSites = new ArrayList();
			EsRadio eRadio = null;
			EsAntenna eAnte = null;
			EsChannel eChan = null;
			if (oDT.Rows.Count > 0){
				//	Got something. 
				try{ 
					for (int nRow = 0; nRow < oDT.Rows.Count; nRow++){
						oDR = oDT.Rows[nRow];
						if (eRadio == null || oDR["location"].ToString() != eRadio.location){
							if (eRadio != null){
								if (nDepth > 1){
									try{
										if (etypein == eEsType.me){
											(eRadio as meRadio).meAddAnt((meAntenna) eAnte);
										} else {
											(eRadio as feRadio).feAddAnt((feAntenna) eAnte);
										}
										if (nDepth > 3){
											//	When we add an antenna, add its azimuths
											eRadio.EsAddAzimuth(eAnte.call1, sTable);
										}
									} catch (Exception ex){
										throw new Exception("Could not add antenna: " + ex.Message);
									}
								}
								if (etypein == eEsType.me){
									alSites.Add(eRadio as meRadio);
								} else {
									alSites.Add(eRadio as feRadio);
								}
							}
							if (etypein == eEsType.me){
								eRadio = new meRadio(oDR);
							} else {
								eRadio = new feRadio(oDR);
							}
							eAnte = null;
						}
						
						// If new antenna, add the antenna
						if (nDepth > 1 &&
								(eAnte == null || 
								(oDR["location"].ToString() != eAnte.location ||
									oDR["call1"].ToString() != eAnte.call1))){
							if (eAnte != null){
								if (etypein == eEsType.me){
									(eRadio as meRadio).meAddAnt((meAntenna) eAnte);
								} else {
									(eRadio as feRadio).feAddAnt((feAntenna) eAnte);
								}
								//	When we add an antenna, add its azimuths
								if (nDepth > 3){
									eRadio.EsAddAzimuth(eAnte.call1, sTable);
								}
							}
							if (etypein == eEsType.me){
								eAnte = new meAntenna(oDR);
							} else {
								eAnte = new feAntenna(oDR);
							}
						}
						
						if (nDepth > 2){
							//	Now add the channels
							if (etypein == eEsType.me){
								eChan = new meChannel(oDR);
							} else {
								eChan = new feChannel(oDR);
							}
							if (etypein == eEsType.me){
								(eRadio as meRadio).meAddChn((meChannel) eChan);
							} else {
								(eRadio as feRadio).feAddChn((feChannel) eChan);
							}
						}
					}
			
					if (nDepth > 1){
						if (etypein == eEsType.me){
							(eRadio as meRadio).meAddAnt((meAntenna) eAnte);
						} else {
							(eRadio as feRadio).feAddAnt((feAntenna) eAnte);
						}
						if (nDepth > 3){
							//	When we add an antenna, add its azimuths
							eRadio.EsAddAzimuth(eAnte.call1, sTable);
						}
					}
					
					if (etypein == eEsType.me){
						alSites.Add(eRadio as meRadio);
					} else {
						alSites.Add(eRadio as feRadio);
					}
					
				} catch (Exception Ex){
					throw (new Exception("\n>>" + Ex.Message + "\n>>" + Ex.StackTrace));
				}
			}
			
			oConnection.dbdisconnect();
			
			return (alSites);
		}
				               


		/// <summary>
		/// This routine will return the name of the appropriate site table 
		///	given the type of table and the fill antenna or channel.
		/// </summary>
		/// <param name="etypein">The eEsType of the table.</param>
		/// <param name="sTable">The full table name</param>
		/// <returns>The correct site table name.</returns>
		public static string sitetable(DBAccess.eEsType etypein, string sTable)
		{
			string sTableOut;
			
			if (etypein == eEsType.me){
				sTableOut		= "main.me_site";
			} else {
				int nBegin	= sTable.IndexOf("_") + 1;
				int nLen		= sTable.LastIndexOf("_") - nBegin;
				sTableOut		= "fe_" + sTable.Substring(nBegin, nLen) + "_site";
			}
			
			return sTableOut;
		}
		
		
		public override string ToString()	{
			return "\n" + m_name + ":-\n" + this.DocumentElement.InnerXml + "\n";
		}
			
	}


	////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// This is the base Es antenna information class.
	/// </summary>
	public class EsAntenna {
		protected eEsType				m_eType;
		
		protected string				m_cmd;
		protected string				m_recstat;

	  protected	string				m_location;
	  protected string				m_call1;
	  protected string				m_txband;
	  protected string				m_rxband;
	  protected string				m_acodetx;
	  protected string				m_acoderx;
		protected double				m_g_t;
		protected double				m_lnat;
		protected double				m_aht;
		protected double				m_afslt;
		protected double				m_afslr;
		protected double				m_txhgmax;
		protected double				m_rxhgmax;
		protected int						m_satlongit;
		protected double				m_satlong;
	  protected string				m_satlongs;
		protected double				m_az;
		protected double				m_el;
		protected double				m_sarc1;
		protected double				m_sarc2;
		protected double				m_rxpre;
		protected double				m_txpre;
		protected double				m_rxtro;
		protected double				m_txtro;
	  protected string				m_licence;
	  protected string				m_satname;
	  protected string				m_stata;
	  protected string				m_nota;
	  protected string				m_op2;
		protected int						m_antref;
	  protected string				m_orbit;
	  protected string				m_mdate;
	  protected string				m_mtime;
	  
	  protected string				m_userid;
		
		
		public DBAccess.eEsType Type{
			get {return m_eType;}
		}

		//	Publish the keys
		public string location{
			get{return m_location;}
			set{m_location = value;}
		}
		
		public string call1{
			get{return m_call1;}
			set{m_call1 = value;}
		}
		

		//	***********************************************************************
		//	Render the Insert Command to add this antenna record to the database 
		//	***********************************************************************
		protected string InsertCommand(eEsType eType, string sTable)
		{
			StringBuilder sbColumns = new StringBuilder(320);
			StringBuilder sbValues  = new StringBuilder(320);
			string tableName;
	
			
			if (eType == eEsType.fe){
				tableName = "fe_" + sTable.Trim() + "_ante";
				
				sbColumns.Append("cmd,");
				sbValues.Append("'" + m_cmd + "',");
				
				sbColumns.Append("recstat,");
				sbValues.Append("'" + m_recstat + "',");
			} else {
				tableName = "main.me_ante";
			}
			
			sbColumns.Append("location,");
			sbValues.Append("'" + m_location + "',");
			
			sbColumns.Append("call1,");
			sbValues.Append("'" + m_call1 + "',");
			
			sbColumns.Append("txband,");
			sbValues.Append("'" + m_txband + "',");
			
			sbColumns.Append("rxband,");
			sbValues.Append("'" + m_rxband + "',");
			
			sbColumns.Append("acodetx,");
			sbValues.Append("'" + m_acodetx + "',");
			
			sbColumns.Append("acoderx,");
			sbValues.Append("'" + m_acoderx + "',");
			
			sbColumns.Append("g_t,");
			sbValues.Append("'" + m_g_t.ToString() + "',");
			
			sbColumns.Append("lnat,");
			sbValues.Append("'" + m_lnat.ToString() + "',");
			
			sbColumns.Append("aht,");
			sbValues.Append("'" + m_aht.ToString() + "',");
			
			sbColumns.Append("afslt,");
			sbValues.Append("'" + m_afslt.ToString() + "',");
			
			sbColumns.Append("afslr,");
			sbValues.Append("'" + m_afslr.ToString() + "',");
			
			sbColumns.Append("txhgmax,");
			sbValues.Append("'" + m_txhgmax.ToString() + "',");
			
			sbColumns.Append("rxhgmax,");
			sbValues.Append("'" + m_rxhgmax.ToString() + "',");
			
			sbColumns.Append("rxhgmax,");
			sbValues.Append("'" + m_satlongit.ToString() + "',");
			
			sbColumns.Append("satlong,");
			sbValues.Append("'" + m_satlong.ToString() + "',");
			
			sbColumns.Append("satlongs,");
			sbValues.Append("'" + m_satlongs + "',");
			
			sbColumns.Append("az,");
			sbValues.Append("'" + m_az.ToString() + "',");
			
			sbColumns.Append("el,");
			sbValues.Append("'" + m_el.ToString() + "',");
			
			sbColumns.Append("sarc1,");
			sbValues.Append("'" + m_sarc1.ToString() + "',");
			
			sbColumns.Append("sarc2,");
			sbValues.Append("'" + m_sarc2.ToString() + "',");
			
			sbColumns.Append("rxpre,");
			sbValues.Append("'" + m_rxpre.ToString() + "',");
			
			sbColumns.Append("txpre,");
			sbValues.Append("'" + m_txpre.ToString() + "',");
			
			sbColumns.Append("rxtro,");
			sbValues.Append("'" + m_rxtro.ToString() + "',");
			
			sbColumns.Append("txtro,");
			sbValues.Append("'" + m_txtro.ToString() + "',");
			
			sbColumns.Append("licence,");
			sbValues.Append("'" + m_licence + "',");
			
			sbColumns.Append("satname,");
			sbValues.Append("'" + m_satname + "',");
			
			sbColumns.Append("stata,");
			sbValues.Append("'" + m_stata + "',");
			
			sbColumns.Append("nota,");
			sbValues.Append("'" + m_nota + "',");
			
			sbColumns.Append("op2,");
			sbValues.Append("'" + m_op2 + "',");
			
			sbColumns.Append("antref,");
			sbValues.Append("'" + m_antref.ToString() + "',");
			
			sbColumns.Append("orbit,");
			sbValues.Append("'" + m_orbit + "',");
			
			sbColumns.Append("mdate,");
			sbValues.Append("'" + m_mdate + "',");
			
			sbColumns.Append("mtime");
			sbValues.Append("'" + m_mtime + "'");
			
			if (eType == eEsType.me){
				sbColumns.Append(",userid");
				sbValues.Append(",'" + m_userid + "'");
			}
		
			
			return ("INSERT into " + tableName + " (" + sbColumns + ") values (" +
			        sbValues + ")");
		}
		
		
		//	***********************************************************************
		//	Just construct an uninitialized antenna record
		//	***********************************************************************
		public EsAntenna()
		{
			m_location = "-";			//	Uninitialized.
		}
		
		//	***********************************************************************
		//	Just construct an uninitialized antenna record of a specified type.
		//	***********************************************************************
		public EsAntenna(eEsType		eType)
		{
			m_eType = eType;
			m_location = "-";
		}

		//	***********************************************************************
		//	This constructor will create an antenna from a datarow.  It makes the 
		//	assumption that the datarow is from an antenna table (either me or fe),
		//	and it just reads the row into the new antenna object.
		//	***********************************************************************
		public EsAntenna(eEsType eType, DataRow	oDR)
		{
			m_eType = eType;
			
			try {
				if (m_eType == eEsType.fe){
					m_cmd			= oDR["cmd"].ToString();
					m_recstat	= oDR["recstat"].ToString();
				}

				m_location		= oDR["location"].ToString();
				m_call1				= oDR["call1"].ToString();
				m_txband			= oDR["txband"].ToString();
				m_rxband			= oDR["rxband"].ToString();
				m_acodetx			= oDR["acodetx"].ToString();
				m_acoderx			= oDR["acoderx"].ToString();
				try{m_g_t			= Convert.ToDouble(oDR["g_t"]);} catch {m_g_t = 0.0f;};
				try{m_lnat		= Convert.ToDouble(oDR["lnat"]);} catch {m_lnat = 0.0f;};
				try{m_aht			= Convert.ToDouble(oDR["aht"]);} catch {m_aht = 0.0f;};
				try{m_afslt		= Convert.ToDouble(oDR["afslt"]);} catch {m_afslt = 0.0f;};
				try{m_afslr		= Convert.ToDouble(oDR["afslr"]);} catch {m_afslr = 0.0f;};
				try{m_txhgmax = Convert.ToDouble(oDR["txhgmax"]);} catch {m_txhgmax = 0.0f;};
				try{m_rxhgmax = Convert.ToDouble(oDR["rxhgmax"]);} catch {m_rxhgmax = 0.0f;};
				try{m_satlongit = Convert.ToInt32(oDR["satlongit"]);} catch {m_satlongit = 0;};
				try{m_satlong = Convert.ToDouble(oDR["satlong"]);} catch {m_satlong = 0.0f;};
				m_satlongs		= oDR["satlongs"].ToString();
				try{m_az			= Convert.ToDouble(oDR["az"]);} catch {m_az = 0.0f;};
				try{m_el			= Convert.ToDouble(oDR["el"]);} catch {m_el = 0.0f;};
				try{m_sarc1		= Convert.ToDouble(oDR["sarc1"]);} catch {m_sarc1 = 0.0f;};
				try{m_sarc2		= Convert.ToDouble(oDR["sarc2"]);} catch {m_sarc2 = 0.0f;};
				try{m_rxpre		= Convert.ToDouble(oDR["rxpre"]);} catch {m_rxpre = 0.0f;};
				try{m_txpre		= Convert.ToDouble(oDR["txpre"]);} catch {m_txpre = 0.0f;};
				try{m_rxtro		= Convert.ToDouble(oDR["rxtro"]);} catch {m_rxtro = 0.0f;};
				try{m_txtro		= Convert.ToDouble(oDR["txtro"]);} catch {m_txtro = 0.0f;};
				m_licence			= oDR["licence"].ToString();
				m_satname			= oDR["satname"].ToString();
				m_stata				= oDR["stata"].ToString();
				m_nota				= oDR["nota"].ToString();
				m_op2					= oDR["op2"].ToString();
				try{m_antref	= Convert.ToInt32(oDR["antref"]);} catch {m_antref = 0;};
				m_orbit				= oDR["orbit"].ToString();
				try{
					m_mdate			= oDR["amdate"].ToString();
					m_mdate			= DateTime.Parse(m_mdate).ToString("dd-MMM-yyyy");
					m_mtime			= oDR["amtime"].ToString();
					if (m_eType == eEsType.me){
						m_userid	= oDR["auserid"].ToString();
					}
				}	catch {
					try {
						m_mdate		= oDR["mdate"].ToString();
						m_mdate		= DateTime.Parse(m_mdate).ToString("dd-MMM-yyyy");
						m_mtime		= oDR["mtime"].ToString();
						if (m_eType == eEsType.me){
							m_userid	= oDR["userid"].ToString();
						}
					} catch {
						m_mdate = "";
						m_mtime = "";
						m_userid = "";
					}
				}
				
			} catch (Exception e){
				throw new Exception("Problem creating antenna:\n" + e.Message);
			}
		}
		
		//	************************************************************************
		//	This constructor will read in an antenna record whose key is completely
		//	known, from a table whose name is known (either main.me_ante or an fe table).
		//	************************************************************************
		public EsAntenna(eEsType	eType,
					           string		cLocation, 
		                 string		cCall1, 
		                 string		cTable)
		{
			//	Connect and get the antenna
			dbconnect oConnection = new dbconnect();
			
			string	cSite = EsFile.sitetable(eType, cTable);
			
			string	antCommand = "SELECT * " +
                             "FROM " + cTable +	" a" +
				                   " WHERE location='" + cLocation.ToUpper() + "' " +
				                      "and call1='" + cCall1.ToUpper();
			
			m_eType = eType;
			m_cmd = "-";
			m_recstat = "-";
			m_userid = "-";

			OdbcDataAdapter oAdapter = new OdbcDataAdapter(antCommand, 
				                                             oConnection.Connection);
			DataSet oDS = new DataSet();
			oAdapter.Fill(oDS);

			// We assume that we have retrieved only one record (or zero)
			DataTable oDT = oDS.Tables[0];
			if (oDT.Rows.Count > 0){
				//	Got something.  We are just interested in the first row.
				DataRow oDR = oDT.Rows[0];

				if (m_eType == eEsType.fe){
					m_cmd			= oDR["cmd"].ToString();
					m_recstat	= oDR["recstat"].ToString();
				}

				m_location		= oDR["location"].ToString();
				m_call1				= oDR["call1"].ToString();
				m_txband			= oDR["txband"].ToString();
				m_rxband			= oDR["rxband"].ToString();
				m_acodetx			= oDR["acodetx"].ToString();
				m_acoderx			= oDR["acoderx"].ToString();
				try{m_g_t			= Convert.ToDouble(oDR["g_t"]);} catch {m_g_t = 0.0f;};
				try{m_lnat		= Convert.ToDouble(oDR["lnat"]);} catch {m_lnat = 0.0f;};
				try{m_aht			= Convert.ToDouble(oDR["aht"]);} catch {m_aht = 0.0f;};
				try{m_afslt		= Convert.ToDouble(oDR["afslt"]);} catch {m_afslt = 0.0f;};
				try{m_afslr		= Convert.ToDouble(oDR["afslr"]);} catch {m_afslr = 0.0f;};
				try{m_txhgmax = Convert.ToDouble(oDR["txhgmax"]);} catch {m_txhgmax = 0.0f;};
				try{m_rxhgmax = Convert.ToDouble(oDR["rxhgmax"]);} catch {m_rxhgmax = 0.0f;};
				try{m_satlongit = Convert.ToInt32(oDR["satlongit"]);} catch {m_satlongit = 0;};
				try{m_satlong = Convert.ToDouble(oDR["satlong"]);} catch {m_satlong = 0.0f;};
				m_satlongs		= oDR["satlongs"].ToString();
				try{m_az			= Convert.ToDouble(oDR["az"]);} catch {m_az = 0.0f;};
				try{m_el			= Convert.ToDouble(oDR["el"]);} catch {m_el = 0.0f;};
				try{m_sarc1		= Convert.ToDouble(oDR["sarc1"]);} catch {m_sarc1 = 0.0f;};
				try{m_sarc2		= Convert.ToDouble(oDR["sarc2"]);} catch {m_sarc2 = 0.0f;};
				try{m_rxpre		= Convert.ToDouble(oDR["rxpre"]);} catch {m_rxpre = 0.0f;};
				try{m_txpre		= Convert.ToDouble(oDR["txpre"]);} catch {m_txpre = 0.0f;};
				try{m_rxtro		= Convert.ToDouble(oDR["rxtro"]);} catch {m_rxtro = 0.0f;};
				try{m_txtro		= Convert.ToDouble(oDR["txtro"]);} catch {m_txtro = 0.0f;};
				m_licence			= oDR["licence"].ToString();
				m_satname			= oDR["satname"].ToString();
				m_stata				= oDR["stata"].ToString();
				m_nota				= oDR["nota"].ToString();
				m_op2					= oDR["op2"].ToString();
				try{m_antref	= Convert.ToInt32(oDR["antref"]);} catch {m_antref = 0;};
				m_orbit				= oDR["orbit"].ToString();
				try{
					m_mdate			= oDR["amdate"].ToString();
					m_mdate			= DateTime.Parse(m_mdate).ToString("dd-MMM-yyyy");
				}	catch {m_mdate = "";}
				m_mtime				= oDR["amtime"].ToString();

				if (m_eType == eEsType.me){
					m_userid	= oDR["auserid"].ToString();
				}
			}
			
			oConnection.dbdisconnect();
		}


		//	*********************************************************************
		//	Copy an antenna record into a newly created antenna.
		//	*********************************************************************
		protected void EsCopAnt(eEsType eType, EsAntenna tAnt)
		{
			if (eType == eEsType.fe){
				m_cmd = tAnt.m_cmd;
				m_recstat	=	tAnt.m_recstat;
			}

			m_location	=	tAnt.m_location;
			m_call1			=	tAnt.m_call1;
			m_txband		=	tAnt.m_txband;
			m_rxband		=	tAnt.m_rxband;
			m_acodetx		=	tAnt.m_acodetx;
			m_acoderx		=	tAnt.m_acoderx;
			m_g_t				=	tAnt.m_g_t;
			m_lnat			=	tAnt.m_lnat;
			m_aht				=	tAnt.m_aht;
			m_afslt			=	tAnt.m_afslt;
			m_afslr			=	tAnt.m_afslr;
			m_txhgmax		=	tAnt.m_txhgmax;
			m_rxhgmax		=	tAnt.m_rxhgmax;
			m_satlongit	=	tAnt.m_satlongit;
			m_satlong		=	tAnt.m_satlong;
			m_satlongs	=	tAnt.m_satlongs;
			m_az				=	tAnt.m_az;
			m_el				=	tAnt.m_el;
			m_sarc1			=	tAnt.m_sarc1;
			m_sarc2			=	tAnt.m_sarc2;
			m_rxpre			=	tAnt.m_rxpre;
			m_txpre			=	tAnt.m_txpre;
			m_rxtro			=	tAnt.m_rxtro;
			m_txtro			=	tAnt.m_txtro;
			m_licence		=	tAnt.m_licence;
			m_satname		=	tAnt.m_satname;
			m_stata			=	tAnt.m_stata;
			m_nota			=	tAnt.m_nota;
			m_op2				=	tAnt.m_op2;
			m_antref		=	tAnt.m_antref;
			m_orbit			=	tAnt.m_orbit;
			m_mdate			=	tAnt.m_mdate;
			m_mtime			=	tAnt.m_mtime;
			
			if (eType == eEsType.me){
				m_userid =	tAnt.m_userid;
			}
			
			return;
		}
		
		
		public XmlElement EsAntennaXml(eEsType			eType, 
		                               EsFile				oFile)
		{
			XmlElement AntennaEl = oFile.CreateElement("es_antenna");
			
			if (eType == eEsType.fe){
				AntennaEl.AppendChild(oFile.XmlEl("cmd", m_cmd));
				AntennaEl.AppendChild(oFile.XmlEl("recstat", m_recstat));
			}
			AntennaEl.AppendChild(oFile.XmlEl("location", m_location));
			AntennaEl.AppendChild(oFile.XmlEl("call1", m_call1));
			AntennaEl.AppendChild(oFile.XmlEl("txband", m_txband));
			AntennaEl.AppendChild(oFile.XmlEl("rxband", m_rxband));
			AntennaEl.AppendChild(oFile.XmlEl("acodetx", m_acodetx));
			AntennaEl.AppendChild(oFile.XmlEl("acoderx", m_acoderx));
			AntennaEl.AppendChild(oFile.XmlEl("g_t", m_g_t.ToString()));
			AntennaEl.AppendChild(oFile.XmlEl("lnat", m_lnat.ToString()));
			AntennaEl.AppendChild(oFile.XmlEl("aht", m_aht.ToString()));
			AntennaEl.AppendChild(oFile.XmlEl("afslt", m_afslt.ToString()));
			AntennaEl.AppendChild(oFile.XmlEl("afslr", m_afslr.ToString()));
			AntennaEl.AppendChild(oFile.XmlEl("txhgmax", m_txhgmax.ToString()));
			AntennaEl.AppendChild(oFile.XmlEl("rxhgmax", m_rxhgmax.ToString()));
			AntennaEl.AppendChild(oFile.XmlEl("satlongit", m_satlongit.ToString()));
			AntennaEl.AppendChild(oFile.XmlEl("satlong", m_satlong.ToString()));
			AntennaEl.AppendChild(oFile.XmlEl("satlongs", m_satlongs));
			AntennaEl.AppendChild(oFile.XmlEl("az", m_az.ToString()));
			AntennaEl.AppendChild(oFile.XmlEl("el", m_el.ToString()));
			AntennaEl.AppendChild(oFile.XmlEl("sarc1", m_sarc1.ToString()));
			AntennaEl.AppendChild(oFile.XmlEl("sarc2", m_sarc2.ToString()));
			AntennaEl.AppendChild(oFile.XmlEl("rxpre", m_rxpre.ToString()));
			AntennaEl.AppendChild(oFile.XmlEl("txpre", m_txpre.ToString()));
			AntennaEl.AppendChild(oFile.XmlEl("rxtro", m_rxtro.ToString()));
			AntennaEl.AppendChild(oFile.XmlEl("txtro", m_txtro.ToString()));
			AntennaEl.AppendChild(oFile.XmlEl("licence", m_licence));
			AntennaEl.AppendChild(oFile.XmlEl("satname", m_satname));
			AntennaEl.AppendChild(oFile.XmlEl("stata", m_stata));
			AntennaEl.AppendChild(oFile.XmlEl("nota", m_nota));
			AntennaEl.AppendChild(oFile.XmlEl("op2", m_op2));
			AntennaEl.AppendChild(oFile.XmlEl("antref", m_antref.ToString()));
			AntennaEl.AppendChild(oFile.XmlEl("orbit", m_orbit));
			AntennaEl.AppendChild(oFile.XmlEl("mdate", m_mdate));
			AntennaEl.AppendChild(oFile.XmlEl("mtime", m_mtime));
			
			if (eType == eEsType.me){
				AntennaEl.AppendChild(oFile.XmlEl("userid", m_userid));
			}
			
			return AntennaEl;
		}		                                           
	}


	//********************************************************************
	/// <summary>
	/// This is the channel information for both mdb and fe channels.
	/// </summary>
	//********************************************************************
	public class EsChannel {
	
		protected eEsType				m_eType;

		protected string				m_cmd;
		protected string				m_recstat;
		
		protected string				m_location;
		protected string				m_call1;
		protected string				m_chid;
		protected double				m_freqtx;
		protected string				m_poltx;
		protected double				m_maxtxpower;
		protected double				m_pwrtx;
		protected double				m_p4khz;
		protected string				m_eqpttx;
		protected string				m_traftx;
		protected string				m_stattx;
		protected string				m_feetx;
		protected double				m_freqrx;
		protected string				m_polrx;
		protected double				m_pwrrx;
		protected string				m_eqptrx;
		protected string				m_trafrx;
		protected string				m_statrx;
		protected double				m_i20;
		protected double				m_it01;
		protected double				m_ip01;
		protected string				m_feerx;
		protected string				m_notc;
		protected string				m_srvctx;
		protected string				m_srvcrx;
		protected string				m_mdate;
		protected string				m_mtime;
		
		protected string				m_userid;
		
		public DBAccess.eEsType Type{
			get {return m_eType;}
		}

		//	Publish the keys
		public string location{
			get{return m_location;}
			set{m_location = value;}
		}
		
		public string call1{
			get{return m_call1;}
			set{m_call1 = value;}
		}
		
		public string chid{
			get{return m_chid;}
			set{m_chid = value;}
		}
		
		//	***********************************************************************
		//	Render the Insert Command to add this channel record to the database 
		//	***********************************************************************
		protected string InsertCommand(eEsType eType, string sTable) 
		{
			StringBuilder sbColumns = new StringBuilder(320);
			StringBuilder sbValues  = new StringBuilder(320);
			string tableName;
	
			
			if (eType == eEsType.fe){
				tableName = "fe_" + sTable.Trim() + "_chan";
				
				sbColumns.Append("cmd,");
				sbValues.Append("'" + m_cmd + "',");
				
				sbColumns.Append("recstat,");
				sbValues.Append("'" + m_recstat + "',");
			} else {
				tableName = "main.me_chan";
			}

			sbColumns.Append("location,");
			sbValues.Append("'" + m_location.ToString() + "',");

			sbColumns.Append("call1,");
			sbValues.Append("'" + m_call1.ToString() + "',");

			sbColumns.Append("chid,");
			sbValues.Append("'" + m_chid.ToString() + "',");

			sbColumns.Append("freqtx,");
			sbValues.Append("'" + m_freqtx.ToString() + "',");

			sbColumns.Append("poltx,");
			sbValues.Append("'" + m_poltx + "',");

			sbColumns.Append("maxtxpower,");
			sbValues.Append("'" + m_maxtxpower.ToString() + "',");

			sbColumns.Append("pwrtx,");
			sbValues.Append("'" + m_pwrtx.ToString() + "',");

			sbColumns.Append("p4khz,");
			sbValues.Append("'" + m_p4khz.ToString() + "',");

			sbColumns.Append("eqpttx,");
			sbValues.Append("'" + m_eqpttx.ToString() + "',");

			sbColumns.Append("traftx,");
			sbValues.Append("'" + m_traftx.ToString() + "',");

			sbColumns.Append("stattx,");
			sbValues.Append("'" + m_stattx.ToString() + "',");

			sbColumns.Append("feetx,");
			sbValues.Append("'" + m_feetx.ToString() + "',");

			sbColumns.Append("freqrx,");
			sbValues.Append("'" + m_freqrx.ToString() + "',");

			sbColumns.Append("polrx,");
			sbValues.Append("'" + m_polrx.ToString() + "',");

			sbColumns.Append("pwrrx,");
			sbValues.Append("'" + m_pwrrx.ToString() + "',");

			sbColumns.Append("eqptrx,");
			sbValues.Append("'" + m_eqptrx.ToString() + "',");

			sbColumns.Append("trafrx,");
			sbValues.Append("'" + m_trafrx.ToString() + "',");

			sbColumns.Append("statrx,");
			sbValues.Append("'" + m_statrx.ToString() + "',");

			sbColumns.Append("i20,");
			sbValues.Append("'" + m_i20.ToString() + "',");

			sbColumns.Append("it01,");
			sbValues.Append("'" + m_it01.ToString() + "',");

			sbColumns.Append("ip01,");
			sbValues.Append("'" + m_ip01.ToString() + "',");

			sbColumns.Append("feerx,");
			sbValues.Append("'" + m_feerx.ToString() + "',");

			sbColumns.Append("notc,");
			sbValues.Append("'" + m_notc.ToString() + "',");

			sbColumns.Append("srvctx,");
			sbValues.Append("'" + m_srvctx.ToString() + "',");

			sbColumns.Append("srvcrx,");
			sbValues.Append("'" + m_srvcrx.ToString() + "',");

			sbColumns.Append("mdate,");
			sbValues.Append("'" + m_mdate.ToString() + "',");

			sbColumns.Append("mtime,");
			sbValues.Append("'" + m_mtime.ToString() + "',");

			
			if (eType == eEsType.me){
				sbColumns.Append(",userid");
				sbValues.Append(",'" + m_userid + "'");
			}
			
			return ("INSERT into " + tableName + " (" + sbColumns + ") values (" +
				sbValues + ")");
		}
		
		
		//	***********************************************************************
		//	Constructor for a blank Channel
		//	***********************************************************************
		
		public EsChannel()
		{
		}
		
		
		//	***********************************************************************
		//	Constructor for a channel of a designated type.
		//	***********************************************************************
		public EsChannel(eEsType eTypein)
		{
			m_eType = eTypein;
		}
		
		//	***********************************************************************
		//	Constructor for a channel of a designated type from a datarow.
		//	***********************************************************************
		public EsChannel(eEsType eType, DataRow	oDR)
		{
			m_eType	= eType;
			try{
				if (eType == eEsType.fe){
					m_cmd			=	oDR["cmd"].ToString();
					m_recstat	=	oDR["recstat"].ToString();
				}
			
				m_location				= oDR["location"].ToString();
				m_call1						= oDR["call1"].ToString();
				m_chid						= oDR["chid"].ToString();
				try{m_freqtx			= Convert.ToDouble(oDR["freqtx"]);} catch {m_freqtx = 0.0f;};
				m_poltx						= oDR["poltx"].ToString();
				try{m_maxtxpower	= Convert.ToDouble(oDR["maxtxpower"]);} catch {m_maxtxpower = 0.0f;};
				try{m_pwrtx				= Convert.ToDouble(oDR["pwrtx"]);} catch {m_pwrtx = 0.0f;};
				try{m_p4khz				= Convert.ToDouble(oDR["p4khz"]);} catch {m_p4khz = 0.0f;};
				m_eqpttx					= oDR["eqpttx"].ToString();
				m_traftx					= oDR["traftx"].ToString();
				m_stattx					= oDR["stattx"].ToString();
				m_feetx						= oDR["feetx"].ToString();
				try{m_freqrx			= Convert.ToDouble(oDR["freqrx"]);} catch {m_freqrx = 0.0f;};
				m_polrx						= oDR["polrx"].ToString();
				try{m_pwrrx				= Convert.ToDouble(oDR["pwrrx"]);} catch {m_pwrrx = 0.0f;};
				m_eqptrx					= oDR["eqptrx"].ToString();
				m_trafrx					= oDR["trafrx"].ToString();
				m_statrx					= oDR["statrx"].ToString();
				try{m_i20					= Convert.ToDouble(oDR["i20"]);} catch {m_i20 = 0.0f;};
				try{m_it01				= Convert.ToDouble(oDR["it01"]);} catch {m_it01 = 0.0f;};
				try{m_ip01				= Convert.ToDouble(oDR["ip01"]);} catch {m_ip01 = 0.0f;};
				m_feerx						= oDR["feerx"].ToString();
				m_notc						= oDR["notc"].ToString();
				m_srvctx					= oDR["srvctx"].ToString();
				m_srvcrx					= oDR["srvcrx"].ToString();
				try {
					m_mdate					= oDR["cmdate"].ToString();
					m_mtime					= oDR["cmtime"].ToString();
					m_mdate					= DateTime.Parse(m_mdate).ToString("dd-MMM-yyyy");
					if (eType == eEsType.me){
						m_userid			= oDR["cuserid"].ToString();
					}
				} catch {
					try {
						m_mdate				= oDR["mdate"].ToString();
						m_mtime				= oDR["mtime"].ToString();
						m_mdate				= DateTime.Parse(m_mdate).ToString("dd-MMM-yyyy");
						if (eType == eEsType.me){
							m_userid		= oDR["userid"].ToString();
						}
					} catch {
						m_mdate		= "";
						m_mtime		= "";
						m_userid	= "";
					}					
				}
			} catch (Exception Ex){
				throw(new Exception("EsChannel Constructor Error:-\n" + Ex.Message));
			}
		}
		
		
		//	*********************************************************************
		//	Copy a channel record into a newly created channel.
		//	*********************************************************************
		protected void EsCopChn(eEsType eType, EsChannel tChn) {
			if (eType == eEsType.fe){
				m_cmd = tChn.m_cmd;
				m_recstat	=	tChn.m_recstat;
			}
			m_location	=	tChn.m_location;
			m_call1	=	tChn.m_call1;
			m_chid	=	tChn.m_chid;
			m_freqtx	=	tChn.m_freqtx;
			m_poltx	=	tChn.m_poltx;
			m_maxtxpower	=	tChn.m_maxtxpower;
			m_pwrtx	=	tChn.m_pwrtx;
			m_p4khz	=	tChn.m_p4khz;
			m_eqpttx	=	tChn.m_eqpttx;
			m_traftx	=	tChn.m_traftx;
			m_stattx	=	tChn.m_stattx;
			m_feetx	=	tChn.m_feetx;
			m_freqrx	=	tChn.m_freqrx;
			m_polrx	=	tChn.m_polrx;
			m_pwrrx	=	tChn.m_pwrrx;
			m_eqptrx	=	tChn.m_eqptrx;
			m_trafrx	=	tChn.m_trafrx;
			m_statrx	=	tChn.m_statrx;
			m_i20	=	tChn.m_i20;
			m_it01	=	tChn.m_it01;
			m_ip01	=	tChn.m_ip01;
			m_feerx	=	tChn.m_feerx;
			m_notc	=	tChn.m_notc;
			m_srvctx	=	tChn.m_srvctx;
			m_srvcrx	=	tChn.m_srvcrx;
			m_mdate	=	tChn.m_mdate;
			m_mtime	=	tChn.m_mtime;

			if (eType == eEsType.me){
				m_userid	=	tChn.m_userid;
			}
			
			return;
		}
	
		public XmlElement EsChannelXml(eEsType			eType, 
			                             EsFile				oFile) 
		{
			// XmlElement tChn = oFile.CreateElement("channel");
			XmlElement tChannelEl = oFile.CreateElement("es_channel");
			
			if (eType == eEsType.fe){
				tChannelEl.AppendChild(oFile.XmlEl("cmd", m_cmd));
				tChannelEl.AppendChild(oFile.XmlEl("recstat", m_recstat));
			}
			tChannelEl.AppendChild(oFile.XmlEl("location", m_location.ToString()));
			tChannelEl.AppendChild(oFile.XmlEl("call1", m_call1.ToString()));
			tChannelEl.AppendChild(oFile.XmlEl("chid", m_chid.ToString()));
			tChannelEl.AppendChild(oFile.XmlEl("freqtx", m_freqtx.ToString()));
			tChannelEl.AppendChild(oFile.XmlEl("poltx", m_poltx.ToString()));
			tChannelEl.AppendChild(oFile.XmlEl("maxtxpower", m_maxtxpower.ToString()));
			tChannelEl.AppendChild(oFile.XmlEl("pwrtx", m_pwrtx.ToString()));
			tChannelEl.AppendChild(oFile.XmlEl("p4khz", m_p4khz.ToString()));
			tChannelEl.AppendChild(oFile.XmlEl("eqpttx", m_eqpttx.ToString()));
			tChannelEl.AppendChild(oFile.XmlEl("traftx", m_traftx.ToString()));
			tChannelEl.AppendChild(oFile.XmlEl("stattx", m_stattx.ToString()));
			tChannelEl.AppendChild(oFile.XmlEl("feetx", m_feetx.ToString()));
			tChannelEl.AppendChild(oFile.XmlEl("freqrx", m_freqrx.ToString()));
			tChannelEl.AppendChild(oFile.XmlEl("polrx", m_polrx.ToString()));
			tChannelEl.AppendChild(oFile.XmlEl("pwrrx", m_pwrrx.ToString()));
			tChannelEl.AppendChild(oFile.XmlEl("eqptrx", m_eqptrx.ToString()));
			tChannelEl.AppendChild(oFile.XmlEl("trafrx", m_trafrx.ToString()));
			tChannelEl.AppendChild(oFile.XmlEl("statrx", m_statrx.ToString()));
			tChannelEl.AppendChild(oFile.XmlEl("i20", m_i20.ToString()));
			tChannelEl.AppendChild(oFile.XmlEl("it01", m_it01.ToString()));
			tChannelEl.AppendChild(oFile.XmlEl("ip01", m_ip01.ToString()));
			tChannelEl.AppendChild(oFile.XmlEl("feerx", m_feerx.ToString()));
			tChannelEl.AppendChild(oFile.XmlEl("notc", m_notc.ToString()));
			tChannelEl.AppendChild(oFile.XmlEl("srvctx", m_srvctx.ToString()));
			tChannelEl.AppendChild(oFile.XmlEl("srvcrx", m_srvcrx.ToString()));
			tChannelEl.AppendChild(oFile.XmlEl("mdate", m_mdate.ToString()));
			tChannelEl.AppendChild(oFile.XmlEl("mtime", m_mtime.ToString()));

			if (eType == eEsType.me){
				tChannelEl.AppendChild(oFile.XmlEl("userid", m_userid));
			}
			
			return tChannelEl;
		}		                                           
	}

	/// <summary>
	/// ***********************************************************************************
	///	This is the class for a row in the ES azimuth table.
	///	***********************************************************************************
	/// </summary>
	public class EsAzimuth {
	
		protected eEsType				m_eType;

		protected string				m_cmd;							// fe
		protected string				m_recstat;					// fe
		protected string				m_deleteall;				// fe
		protected string				m_location;
		protected string				m_call1;
		protected double				m_azim;
		protected double				m_elev;
		protected double				m_dist;
		protected double				m_loss;
		protected string				m_mdate;
		protected string				m_mtime;
		protected string				m_userid;						// me
		

		public DBAccess.eEsType Type{
			get {return m_eType;}
		}

		//	***********************************************************************
		//	Render the Insert Command to add this channel record to the database 
		//	***********************************************************************
		protected string InsertCommand(eEsType eType, string sTable) 
		{
			StringBuilder sbColumns = new StringBuilder(320);
			StringBuilder sbValues  = new StringBuilder(320);
			string tableName;
	
			
			if (eType == eEsType.fe){
				tableName = "fe_" + sTable.Trim() + "_chan";
				
				sbColumns.Append("cmd,");
				sbValues.Append("'" + m_cmd + "',");
				
				sbColumns.Append("recstat,");
				sbValues.Append("'" + m_recstat + "',");
				
				sbColumns.Append("deleteall,");
				sbValues.Append("'" + m_deleteall + "',");
			} else {
				tableName = "main.me_chan";
			}
			
			sbColumns.Append("location,");
			sbValues.Append("'" + m_location + "',");

			sbColumns.Append("call1,");
			sbValues.Append("'" + m_call1 + "',");

			sbColumns.Append("azim,");
			sbValues.Append("'" + m_azim.ToString() + "',");

			sbColumns.Append("elev,");
			sbValues.Append("'" + m_elev.ToString() + "',");

			sbColumns.Append("dist,");
			sbValues.Append("'" + m_dist.ToString() + "',");

			sbColumns.Append("loss,");
			sbValues.Append("'" + m_loss.ToString() + "',");

			sbColumns.Append("mdate,");
			sbValues.Append("'" + m_mdate + "',");

			sbColumns.Append("mtime,");
			sbValues.Append("'" + m_mtime + "',");

			if (eType == eEsType.me){
				sbColumns.Append(",userid");
				sbValues.Append(",'" + m_userid + "'");
			}
			
			return ("INSERT into " + tableName + " (" + sbColumns + ") values (" +
				sbValues + ")");
		}
		
		
		//	***********************************************************************
		//	Constructor for a blank Channel
		//	***********************************************************************
		
		public EsAzimuth()
		{
		}
		
		
		//	***********************************************************************
		//	Constructor for an Azimuth of a designated type.
		//	***********************************************************************
		public EsAzimuth(eEsType eTypein)
		{
			m_eType = eTypein;
		}
		
		//	***********************************************************************
		//	Constructor for an Azimuth of a designated type from a datarow.
		//	***********************************************************************
		public EsAzimuth(eEsType eType, DataRow	oDR)
		{
			m_eType	= eType;

			try {			
				if (eType == eEsType.fe){
					m_cmd				=	oDR["cmd"].ToString();
					m_recstat		=	oDR["recstat"].ToString();
					m_deleteall = oDR["deleteall"].ToString();				// fe
				}
				
				m_location	= oDR["location"].ToString();
				m_call1			= oDR["call1"].ToString();
				try{m_azim	= Convert.ToDouble(oDR["azim"]);} catch {m_azim = 0.0f;};
				try{m_elev	= Convert.ToDouble(oDR["elev"]);} catch {m_elev = 0.0f;};
				try{m_dist	= Convert.ToDouble(oDR["dist"]);} catch {m_dist = 0.0f;};
				try{m_loss	= Convert.ToDouble(oDR["loss"]);} catch {m_loss = 0.0f;};
				m_mdate			= oDR["mdate"].ToString();
				try{
					m_mdate		= DateTime.Parse(m_mdate).ToString("dd-MMM-yyyy");
				} catch {}			
				m_mtime			= oDR["mtime"].ToString();
			
				if (eType == eEsType.me){
					m_userid	= oDR["userid"].ToString();
				}
			} catch (Exception Ex){
				throw (new Exception("EsAzimuth Constructor Error:-\n" + Ex.Message));
			}
		}
		
		
		//	*********************************************************************
		//	Copy an azimuth record into a newly created azimuth.
		//	*********************************************************************
		protected void EsCopAz(eEsType eType, EsAzimuth tAz) {
			if (eType == eEsType.fe){
				m_cmd				= tAz.m_cmd;
				m_recstat		=	tAz.m_recstat;
				m_deleteall = tAz.m_deleteall;
			}

			m_location	= tAz.m_location;
			m_call1			= tAz.m_call1;
			m_azim			= tAz.m_azim;
			m_elev			= tAz.m_elev;
			m_dist			= tAz.m_dist;
			m_loss			= tAz.m_loss;
			m_mdate			= tAz.m_mdate;
			m_mtime			= tAz.m_mtime;
				
			if (eType == eEsType.me){
				m_userid	=	tAz.m_userid;
			}
			
			return;
		}
	
		/// <summary>
		/// Create the azimuth element for this azimuth instance.
		/// </summary>
		/// <param name="eType">The type (fe or me)</param>
		/// <param name="oFile">The output file used for XML reference.</param>
		/// <returns>An XmlElement representing one record in the Azimuth database.</returns>
		public XmlElement EsAzimuthXml(eEsType			eType, 
			                             EsFile				oFile) 
		{
			XmlElement tAzimuthEl = oFile.CreateElement("es_azimuth");
			
			if (eType == eEsType.fe){
				tAzimuthEl.AppendChild(oFile.XmlEl("cmd", m_cmd));
				tAzimuthEl.AppendChild(oFile.XmlEl("recstat", m_recstat));
				tAzimuthEl.AppendChild(oFile.XmlEl("deleteall", m_deleteall));
				
			}

			tAzimuthEl.AppendChild(oFile.XmlEl("location", m_location));
			tAzimuthEl.AppendChild(oFile.XmlEl("call1", m_call1));
			tAzimuthEl.AppendChild(oFile.XmlEl("azim", m_azim.ToString()));
			tAzimuthEl.AppendChild(oFile.XmlEl("elev", m_elev.ToString()));
			tAzimuthEl.AppendChild(oFile.XmlEl("dist", m_dist.ToString()));
			tAzimuthEl.AppendChild(oFile.XmlEl("loss", m_loss.ToString()));
			tAzimuthEl.AppendChild(oFile.XmlEl("mdate", m_mdate));
			tAzimuthEl.AppendChild(oFile.XmlEl("mtime", m_mtime));
			
			if (eType == eEsType.me){
				tAzimuthEl.AppendChild(oFile.XmlEl("userid", m_userid));
			}
			
			return tAzimuthEl;
		}		                                           
	}


	//	**************************************************************************
	/// <summary>
	/// General Class to Access both MDB and PDF Es Radios.  The site information is
	/// stored in the radio class itself.
	/// </summary>
	public class EsRadio {
		protected eEsType			m_etype;
		protected int					m_nDepth;

		protected string			m_cmd;
		protected string			m_recstat;
	  protected string			m_location;
	  protected string			m_name;
	  protected string			m_prov;
	  protected string			m_oper;
		protected int					m_latit;
	  protected string			m_strlatit;
	  protected string			m_strlatits;
		protected int					m_longit;
	  protected string			m_strlongit;
	  protected string			m_strlongits;
		protected double			m_grnd;
	  protected string			m_radio;
		protected int					m_rain;
	  protected string			m_sdate;
	  protected string			m_stats;
	  protected string			m_nots;
	  protected string			m_oprtyp;
	  protected string			m_reg;
	  protected string			m_mdate;
	  protected string			m_mtime;
	  protected string			m_userid;
	  
		//	Optional elements
		protected string			m_nameop;

		//	Arrays and counters
		protected int					m_numants = 0;
		protected ArrayList		m_Antennas = null;
		protected int					m_numchan = 0;
		protected ArrayList		m_Channels = null;
		protected int					m_numazim = 0;
		protected ArrayList		m_Azimuths = null;
		
		public int numants{
			get {return m_numants;}
		}

		public ArrayList Antennas{
			get {return m_Antennas;}
		}

		public int numchan{
			get {return m_numchan;}
		}

		public ArrayList Channels{
			get {return m_Channels;}
		}

		public int numazim{
			get {return m_numazim;}
		}

		public ArrayList Azimuths{
			get {return m_Azimuths;}
		}

		public DBAccess.eEsType Type{
			get{return m_etype;}
		}

		//	Location is the key in both fe and me
		public string location{
			get{return m_location;}
			set{m_location = value;}
		}

		public EsRadio(){}
		
		
		public EsRadio(eEsType etypein, DataRow oDR)
		{
			m_etype = etypein;
			
			try{
			if (etypein == eEsType.fe){
				m_cmd				= oDR["cmd"].ToString();
				m_recstat		= oDR["recstat"].ToString();
			}
			m_location	= oDR["location"].ToString();
			m_name			= oDR["name"].ToString();
			m_prov			= oDR["prov"].ToString();
			m_oper			= oDR["oper"].ToString();
			m_latit			= Convert.ToInt32(oDR["latit"]);
			m_longit		= Convert.ToInt32(oDR["longit"]);
			if (etypein == eEsType.me){
				m_strlatit		= oDR["strlatit"].ToString();
				m_strlatits		= oDR["strlatits"].ToString();
				m_strlongit		= oDR["strlongit"].ToString();
				m_strlongits	=	oDR["strlongits"].ToString();
				m_userid			= oDR["userid"].ToString();
			}
			try {m_grnd	= Convert.ToDouble(oDR["grnd"]);} catch {m_grnd = 0.0f;}
			m_radio			= oDR["radio"].ToString();
			try {m_rain	= Convert.ToInt32(oDR["rain"]);} catch {m_rain = 0;}
			m_sdate			= oDR["sdate"].ToString();
			try{
				m_sdate			= DateTime.Parse(m_sdate).ToString("dd-MMM-yyyy");
			} catch {}
			m_stats			= oDR["stats"].ToString();
			m_nots			= oDR["nots"].ToString();
			m_oprtyp		= oDR["oprtyp"].ToString();
			m_reg				= oDR["reg"].ToString();
			try{
				m_mdate		= oDR["mdate"].ToString();
				m_mdate		= DateTime.Parse(m_mdate).ToString("dd-MMM-yyyy");
			}	catch {m_mdate = "";}
			m_mtime			= oDR["mtime"].ToString();
			
			//	The operator name may or may not come from the operator table 
			try{m_nameop	= oDR["nameop"].ToString();} catch{m_nameop = "";}

			m_nDepth = 1;
			} catch (Exception Ex){
				throw (new Exception("EsRadio Constructor Error:-\n" + Ex.Message));
			}
			
			m_Antennas = new ArrayList();
			m_numants = 0;
			m_Channels = new ArrayList();
			m_numchan = 0;
			m_Azimuths = new ArrayList();
			m_numazim = 0;
			
			return;
		}

		
		public EsRadio(DBAccess.eEsType etypein, 
		               string						sLocation, 
		               string						sTable,
		               int							nDepth) 
		{
			// 
			// Retrieve a full radio given its callsign and depth.
			//	Start by reading in the site information, and then call the 
			//	constructors for the antennas and channels if necessary.
			//
			string		sTableName;
			if (etypein == eEsType.me){
				sTableName = "main.me_site";
			} else {
				sTableName = "fe_" + sTable.Trim() + "_site";
			}	
			dbconnect oConnection = new dbconnect();
			
			const string  sFieldNames_me =	"s.location, s.name, s.prov, s.oper, s.latit,	" +
			                                "s.strlatit,	s.strlatits, s.longit, s.strlongit,	" +
			                                "s.strlongits,	s.grnd,	s.radio, s.rain, s.sdate,	" +
			                                "s.stats, s.nots, s.oprtyp, s.reg,	s.mdate, s.mtime,	" +
			                                "s.userid, o.nameop";	 
			const string	sFieldNames_fe =  "s.cmd,	s.recstat,	s.location,	s.name,	s.prov,	" +
			                                "s.oper,	s.latit,	s.longit,	s.grnd,	s.radio,	" +
			                                "s.rain,	s.sdate,	s.stats,	s.nots,	s.oprtyp,	" +
			                                "s.reg,	s.mdate,	s.mtime,	o.nameop";

			string	sFieldNames;
			switch ((int) etypein){
				case (int) eEsType.me:
					sFieldNames = sFieldNames_me;
					break;
				case (int) eEsType.fe:
					sFieldNames = sFieldNames_fe;
					break;
				default:
					throw new Exception("Invalid Es type entered, must be me or fe.");
			}
																 
			string	meCommand = "SELECT " + sFieldNames +
                           " FROM " + sTableName + " s left outer join main.sd_oper o " +
                                      " on (s.oper = o.oper) " +
				                  " WHERE location='" + sLocation.ToUpper() + "'";
			m_etype = etypein;
			m_cmd = "-";
			m_recstat = "-";
			m_strlatit = "";
			m_strlatits = "";
			m_strlongit	= "";
			m_strlongits = "";
			m_userid = "";
			
			m_numants = 0;
			m_Antennas = new ArrayList();
			m_numchan = 0;
			m_Channels = new ArrayList();
			m_numazim = 0;
			m_Azimuths = new ArrayList();
			m_nDepth = 0;	//	No info so far

			OdbcDataAdapter oAdapter = new OdbcDataAdapter(meCommand, 
				                                             oConnection.Connection);
			DataSet oDS = new DataSet();
			oAdapter.Fill(oDS);

			// We assume that we have retrieved only one record (or zero)
			DataTable oDT = oDS.Tables[0];
			if (oDT.Rows.Count > 0){
				//	Got something.  We are just interested in the first row.
				DataRow oDR = oDT.Rows[0];
				
				if (m_etype == eEsType.fe){
					m_cmd				= oDR["cmd"].ToString();
					m_recstat		= oDR["recstat"].ToString();
				}
				m_location	= oDR["location"].ToString();
				m_name			= oDR["name"].ToString();
				m_prov			= oDR["prov"].ToString();
				m_oper			= oDR["oper"].ToString();
				m_latit			= Convert.ToInt32(oDR["latit"]);
				m_longit		= Convert.ToInt32(oDR["longit"]);
				if (m_etype == eEsType.me){
					m_strlatit		= oDR["strlatit"].ToString();
					m_strlatits		= oDR["strlatits"].ToString();
					m_strlongit		= oDR["strlongit"].ToString();
					m_strlongits	=	oDR["strlongit"].ToString();
					m_userid			= oDR["userid"].ToString();
				}
				try {m_grnd	= Convert.ToDouble(oDR["grnd"]);} catch {m_grnd = 0.0f;}
				m_radio			= oDR["radio"].ToString();
				try {m_rain	= Convert.ToInt32(oDR["rain"]);} catch {m_rain = 0;}
				m_sdate			= oDR["sdate"].ToString();
				try {
					m_sdate			= DateTime.Parse(m_sdate).ToString("dd-MMM-yyyy");
				} catch {}
				m_stats			= oDR["stats"].ToString();
				m_nots			= oDR["nots"].ToString();
				m_oprtyp		= oDR["oprtyp"].ToString();
				m_reg				= oDR["reg"].ToString();
				m_mdate			= oDR["mdate"].ToString();
				try{
					m_mdate		= DateTime.Parse(m_mdate).ToString("dd-MMM-yyyy");
				}	catch {}
				m_mtime			= oDR["mtime"].ToString();
				
				//	The operator name may or may not come from the operator table 
				try{m_nameop	= oDR["nameop"].ToString();} catch{m_nameop = "";}

				m_nDepth = 1;		// We at least got the site info.
				
				//	Now get the Antennas, if the depth requests it
				if (nDepth > 1){
					string cErrString = "Es Radio-site";
					m_numants = 0;
					m_Antennas.Clear();		//	Clear out the antenna list
					//	The user is asking for at least the antennas as well
					try{
						if (etypein == eEsType.me){
							sTableName = "main.me_ante";
						} else {
							sTableName = "fe_" + sTable.Trim() + "_ante";
						}	
						string antCommand = "SELECT * " +
																	"from " + sTableName + " a" +
																" where location='" + sLocation.ToUpper() +
														"' order by location, call1";
						                     
						OdbcDataAdapter oAntAdapter = new OdbcDataAdapter(antCommand, 
																											oConnection.Connection);
						DataSet oADS = new DataSet();
						oAntAdapter.Fill(oADS);

						DataTable oADT = oADS.Tables[0];  // One table retrieved
						cErrString += ",Count=" + oADT.Rows.Count.ToString();
						if (oADT.Rows.Count > 0){
							foreach (DataRow oADR in oADT.Rows){
								cErrString = "tsa: ";
								if (etypein == eEsType.me){
									cErrString += "me";
									m_Antennas.Add(new meAntenna(oADR));
								} else {
									//	Handle the fe here
									cErrString += "fe";
									m_Antennas.Add(new feAntenna(oADR));
								}
								m_numants++;
							}
						}
						m_nDepth = 2;
					} catch (Exception e1) {
						Exception ex = new Exception(cErrString + "\n<br>" + e1.Message);
						throw ex;
					}
					
					// Now go for the channels if they are requested.
					if (nDepth > 2){
						if (etypein == eEsType.me){
							sTableName = "main.me_chan";
						} else {
							sTableName = "fe_" + sTable + "_chan";
						}	
						string chnCommand = "SELECT * " +
																	"from " + sTableName + " a" +
																" where location='" + sLocation.ToUpper() +
														"' order by location, call1, chid";
						try{                   
							OdbcDataAdapter oChnAdapter = new OdbcDataAdapter(chnCommand, 
								                                  oConnection.Connection);
							DataSet oCDS = new DataSet();
							oChnAdapter.Fill(oCDS);

							m_numchan = 0;
							m_Channels.Clear(); // Clear the channels
							DataTable oCDT = oCDS.Tables[0];  // One table retrieved
							if (oCDT.Rows.Count > 0){
								foreach (DataRow oCDR in oCDT.Rows){
									if (etypein == eEsType.me){
										m_Channels.Add(new meChannel(oCDR));
									} else {
										m_Channels.Add(new feChannel(oCDR));
									}
									m_numchan++;
								}
							}
							m_nDepth = 3;
						} catch (Exception ex){
							Exception ex1 = new Exception("EsRadio Channels<br />\n" +
								                            ex.Message + "(" + ex.Source +
								                            ")");
							throw ex1;
						}
					}

					// Now go for the Azimuths if they are requested.
					if (nDepth > 3){
						if (etypein == eEsType.me){
							sTableName = "main.me_azim";
						} else {
							sTableName = "fe_" + sTable + "_azim";
						}	
						string chnCommand = "SELECT * " +
																	"from " + sTableName + " z" +
																" where location='" + sLocation.ToUpper() +
														"' order by location, call1, azim";
						try{                   
							OdbcDataAdapter oChnAdapter = new OdbcDataAdapter(chnCommand, 
								                                   oConnection.Connection);
							DataSet oCDS = new DataSet();
							oChnAdapter.Fill(oCDS);

							m_numazim = 0;
							m_Azimuths.Clear();
							DataTable oCDT = oCDS.Tables[0];  // One table retrieved
							if (oCDT.Rows.Count > 0){
								foreach (DataRow oCDR in oCDT.Rows){
									if (etypein == eEsType.me){
										m_Azimuths.Add(new meAzimuth(oCDR));
									} else {
										m_Azimuths.Add(new feAzimuth(oCDR));
									}
									m_numazim++;
								}
							}
							m_nDepth = 4;
						} catch (Exception ex){
							Exception ex1 = new Exception("EsRadio Azimuths<br />\n" +
								                            ex.Message + "(" + ex.Source +
								                            ")");
							throw ex1;
						}
					}

				}	
			} else {
				//	Call sign not found.
				throw new System.Exception("100 - Location (" + sLocation + 
					                         ") not found in " + sTable);
			}
			
			oConnection.dbdisconnect();
		}
		
		
		/// <summary>
		/// This routine will add the azimuth of the specified antenna to the site.  
		///	We do it this way because the azimuths are in the site not the antenna.
		/// </summary>
		/// <param name="sCall1">The antennas call sign</param>
		public void EsAddAzimuth(string sCall1, string sTable)
		{
			string sTableFull;
			if (m_etype == eEsType.me){
				sTableFull = "main.me_azim";
			} else {
				sTableFull = "fe_" + sTable + "_azim";
			}
			string sSQL = "SELECT * FROM " + sTableFull + 
			              " WHERE location='" + m_location.Trim() + "' and call1='" +
			                      sCall1 + "'";
			try{
				dbconnect oConnection = new dbconnect();
				OdbcDataAdapter oAdapter = new OdbcDataAdapter(sSQL, oConnection.Connection);
				DataSet oDS = new DataSet();
				oAdapter.Fill(oDS);

				// We assume that we have retrieved only one table
				DataTable oDT = oDS.Tables[0];
				DataRow oDR = null;
				
				if (oDT.Rows.Count > 0){
					for (int nRow = 0; nRow < oDT.Rows.Count; nRow++){
						oDR = oDT.Rows[nRow];
						if (m_etype == eEsType.me){
							meAzimuth	tAz = new meAzimuth(oDR);
							m_Azimuths.Add(tAz);
						} else {
							feAzimuth tAz = new feAzimuth(oDR);
							m_Azimuths.Add(tAz);
						}
						m_numazim++;
					}
				}
				
				oConnection.dbdisconnect();
						
			} catch (Exception Ex){
				throw (new Exception("Add Azimuth Error:-\n" + Ex.Message + "\nSQL:-\n" + sSQL));
			}
		}
		
		
		//	***********************************************************************
		//	Set the depth value in the site.  If this is greater than the current
		//	depth it is set, if it is less than or equal it is ignored.
		//	***********************************************************************
		protected void setdepth(int nDepth) {
			if (nDepth > m_nDepth){
				m_nDepth = nDepth;
			}
			return;
		}	
		
	
		//	***********************************************************************
		//	Render the Insert Command to add this site record to the database 
		//	***********************************************************************
		protected string InsertCommand(eEsType eType, string sTable) 
		{
			StringBuilder sbColumns = new StringBuilder(320);
			StringBuilder sbValues  = new StringBuilder(320);
			string tableName;
	
			
			if (eType == eEsType.fe){
				tableName = "fe_" + sTable.Trim() + "_site";
				
				sbColumns.Append("cmd,");
				sbValues.Append("'" + m_cmd + "',");
				
				sbColumns.Append("recstat,");
				sbValues.Append("'" + m_recstat + "',");
			} else {
				tableName = "main.me_site";
			}
			sbColumns.Append("location,");
			sbValues.Append("'" + m_location + "',");

			sbColumns.Append("name,");
			sbValues.Append("'" + m_name + "',");

			sbColumns.Append("prov,");
			sbValues.Append("'" + m_prov + "',");

			sbColumns.Append("oper,");
			sbValues.Append("'" + m_oper + "',");

			sbColumns.Append("latit,");
			sbValues.Append("'" + m_latit.ToString() + "',");

			sbColumns.Append("strlatit,");
			sbValues.Append("'" + m_strlatit + "',");

			sbColumns.Append("strlatits,");
			sbValues.Append("'" + m_strlatits + "',");

			sbColumns.Append("longit,");
			sbValues.Append("'" + m_longit.ToString() + "',");

			sbColumns.Append("strlongit,");
			sbValues.Append("'" + m_strlongit + "',");

			sbColumns.Append("strlongits,");
			sbValues.Append("'" + m_strlongits + "',");

			sbColumns.Append("grnd,");
			sbValues.Append("'" + m_grnd.ToString() + "',");

			sbColumns.Append("radio,");
			sbValues.Append("'" + m_radio + "',");

			sbColumns.Append("rain,");
			sbValues.Append("'" + m_rain.ToString() + "',");

			sbColumns.Append("sdate,");
			sbValues.Append("'" + m_sdate + "',");

			sbColumns.Append("stats,");
			sbValues.Append("'" + m_stats + "',");

			sbColumns.Append("nots,");
			sbValues.Append("'" + m_nots + "',");

			sbColumns.Append("oprtyp,");
			sbValues.Append("'" + m_oprtyp + "',");

			sbColumns.Append("reg,");
			sbValues.Append("'" + m_reg + "',");

			sbColumns.Append("mdate,");
			sbValues.Append("'" + m_mdate + "',");

			sbColumns.Append("mtime,");
			sbValues.Append("'" + m_mtime + "',");

			if (eType == eEsType.me){
				sbColumns.Append("userid,");
				sbValues.Append("'" + m_userid + "',");
			}
				  
					//	Optional elements
			sbColumns.Append("nameop,");
			sbValues.Append("'" + m_nameop + "',");
		
			
			return ("INSERT into " + tableName + " (" + sbColumns + ") values (" +
							sbValues + ")");
		}


		//	*********************************************************************
		//	Copy a site record into a newly created Radio.
		//	*********************************************************************
		protected void EsCopRad(eEsType eType, EsRadio tRad) 
		{
			if (eType == eEsType.fe){
				m_cmd = tRad.m_cmd;
				m_recstat	=	tRad.m_recstat;
			}
			m_location		=	tRad.m_location;
			m_name				=	tRad.m_name;
			m_prov				=	tRad.m_prov;
			m_oper				=	tRad.m_oper;
			m_latit				=	tRad.m_latit;
			m_strlatit		=	tRad.m_strlatit;
			m_strlatits		=	tRad.m_strlatits;
			m_longit			=	tRad.m_longit;
			m_strlongit		=	tRad.m_strlongit;
			m_strlongits	=	tRad.m_strlongits;
			m_grnd				=	tRad.m_grnd;
			m_radio				=	tRad.m_radio;
			m_rain				=	tRad.m_rain;
			m_sdate				=	tRad.m_sdate;
			m_stats				=	tRad.m_stats;
			m_nots				=	tRad.m_nots;
			m_oprtyp			=	tRad.m_oprtyp;
			m_reg					=	tRad.m_reg;
			m_mdate				=	tRad.m_mdate;
			m_mtime				=	tRad.m_mtime;
			if (eType == eEsType.me){
				m_userid		=	tRad.m_userid;
			}
	  
			//	Optional elements
			m_nameop	=	tRad.m_nameop;

			return;
		}


		//************************************************************************
		/// <summary>
		/// This routine will generate the XML object from the radio.  It calls
		///	The corresponding routines for the channels and antennas.
		///	This routine returns the radio Element, with the site node below it,
		///	the antennas and the channels can be appended to the returned radio
		///	node.   This will generate a structure like the following:-
		///		radio - site - [site properties...]
		///		        antennas - antenna - [antenna properties...]
		///                      antenna - [antenna properties...]
		///		                   ...
		///		        channels - channel - [channel properties...]
		///	                     channel - [channel properties...]
		///						azimuths - azimuth - [azimuth properties...]
		///	                     ...
		/// </summary>
		///	<param name="eType">The type of the radio me or fe.</param>
		/// <param name="oFile">The XML file object that this radio will be added to
		///	</param>
		/// <returns>XMLDocumentFragment corresponding to a single TS Radio without
		///          any antennas or channels.</returns>
		public XmlElement EsRadioXml(EsFile oFile)
		{
			eEsType eType = Type;

			XmlElement	tRadNode = oFile.CreateElement("es_radio");
			
			XmlElement EsiteNode = oFile.CreateElement("es_site");
			if (eType == eEsType.fe){
				EsiteNode.AppendChild(oFile.XmlEl("cmd", m_cmd));
				EsiteNode.AppendChild(oFile.XmlEl("recstat", m_recstat));
			}
			EsiteNode.AppendChild(oFile.XmlEl("location", m_location));
			EsiteNode.AppendChild(oFile.XmlEl("name", m_name));
			EsiteNode.AppendChild(oFile.XmlEl("prov", m_prov));
			EsiteNode.AppendChild(oFile.XmlEl("oper", m_oper));
			EsiteNode.AppendChild(oFile.XmlEl("latit", m_latit.ToString()));
			EsiteNode.AppendChild(oFile.XmlEl("strlatit", m_strlatit));
			EsiteNode.AppendChild(oFile.XmlEl("strlatits", m_strlatits));
			EsiteNode.AppendChild(oFile.XmlEl("longit", m_longit.ToString()));
			EsiteNode.AppendChild(oFile.XmlEl("strlongit", m_strlongit));
			EsiteNode.AppendChild(oFile.XmlEl("strlongits", m_strlongits));
			EsiteNode.AppendChild(oFile.XmlEl("grnd", m_grnd.ToString("#0.0")));
			EsiteNode.AppendChild(oFile.XmlEl("radio", m_radio));
			EsiteNode.AppendChild(oFile.XmlEl("rain", m_rain.ToString()));
			EsiteNode.AppendChild(oFile.XmlEl("sdate", m_sdate));
			EsiteNode.AppendChild(oFile.XmlEl("stats", m_stats));
			EsiteNode.AppendChild(oFile.XmlEl("nots", m_nots));
			EsiteNode.AppendChild(oFile.XmlEl("oprtyp", m_oprtyp));
			EsiteNode.AppendChild(oFile.XmlEl("reg", m_reg));
			EsiteNode.AppendChild(oFile.XmlEl("mdate", m_mdate));
			EsiteNode.AppendChild(oFile.XmlEl("mtime", m_mtime));
			if (eType == eEsType.me){
				EsiteNode.AppendChild(oFile.XmlEl("userid", m_userid));
			}
	  
			//	Optional elements
			EsiteNode.AppendChild(oFile.XmlEl("nameop", m_nameop));

			tRadNode.AppendChild(EsiteNode);	//	Add the site to the radio.
			return tRadNode;

		}
		
		/// <summary>
		/// Add an antenna to the radio.
		/// </summary>
		/// <param name="etypein">Type: me or fe</param>
		/// <param name="tAnt">an ES antenna object</param>
		public EsAntenna EsAddAnt(eEsType etypein, EsAntenna tAnt)
		{
			Antennas.Add(tAnt);
			m_numants++;
			return tAnt;
		}


		/// <summary>
		/// Add a channel to the radio.
		/// </summary>
		/// <param name="etypein">Type: me or fe</param>
		/// <param name="tAnt">an ES channel object</param>
		public EsChannel EsAddChn(eEsType etypein, EsChannel tChn)
		{
			try{
			Channels.Add(tChn);
			m_numchan++;
			return tChn;
			} catch (Exception ex){
				string Errm = "Error in EsAddChn, Chan type: " + tChn.GetType().FullName;
				if (tChn != null){
					Errm += "\nchannel: " + tChn.location + "," + tChn.call1 + "," + tChn.chid;
				}
				throw new Exception(Errm + "\n" + ex.Message + "\n" + ex.StackTrace);
			}
		}

	}
	

	/// <summary>
	/// This is the me Azimuth class
	/// </summary>
	public class meAzimuth:EsAzimuth{

		public string			deleteall{
			get{return m_deleteall;}
			set{m_deleteall = value;}
		}

		public string			location{
			get{return m_location;}
			set{m_location = value;}
		}

		public string			call1{
			get{return m_call1;}
			set{m_call1 = value;}
		}
		public double			azim{
			get{return m_azim;}
			set{m_azim = value;}
		}

		public double			elev{
			get{return m_elev;}
			set{m_elev = value;}
		}

		public double			dist{
			get{return m_dist;}
			set{m_dist = value;}
		}

		public double			loss{
			get{return m_loss;}
			set{m_loss = value;}
		}

		public string			mdate{
			get{return m_mdate;}
			set{m_mdate = value;}
		}

		public string			mtime{
			get{return m_mtime;}
			set{m_mtime = value;}
		}
		
		public string			userid{
			get {return m_userid;}
			set {m_userid = value;}
		}

		/// <summary>
		/// Some miscellaneous constructors for the azimuth.
		/// </summary>
		public meAzimuth():base (eEsType.me){
		}
		
		public meAzimuth(DataRow oDR):base (eEsType.me, oDR){
		}
		
		/// <summary>
		/// Return the insert command.
		/// </summary>
		/// <param name="sTable">The base name of the table </param>
		/// <returns>The full "INSERT INTO xxx..." SQL command to insert this azimuth into the table
		/// </returns>
		public string InsertCommand(string sTable) {
			return(base.InsertCommand(eEsType.me, sTable));
		}

		/// <summary>
		/// Copy an azimuth into the current azimuth.
		/// </summary>
		/// <param name="tAz">The azimuth to be copied</param>
		public void meCopAz(meAzimuth tAz){
			base.EsCopAz(eEsType.me, tAz);
			return;
		}
		
		/// <summary>
		/// Produce the XML representing this azimuth.
		/// </summary>
		/// <param name="oFile">The XML output file it will go to</param>
		/// <returns>an XmlElement with this azimuth in it.</returns>
		public XmlElement meAzimuthXml(EsFile oFile){
			return(base.EsAzimuthXml(eEsType.me, oFile));
		}
	}
	
	/// <summary>
	/// This is the MDB Es antenna information class.  It inherits from EsAntenna
	///
	/// </summary>
	public class meAntenna: EsAntenna 
	{
	/*	-- The key is accessed through the base class
		public string			location{
			get{return m_location;}
			set{m_location = value;}
		}
		
		public string			call1{
			get{return m_call1;}
			set{m_call1 = value;}
		}
	*/
		
		public string			txband{
			get{return m_txband;}
			set{m_txband = value;}
		}
		
		public string			rxband{
			get{return m_rxband;}
			set{m_rxband = value;}
		}
		
		public string			acodetx{
			get{return m_acodetx;}
			set{m_acodetx = value;}
		}
		
		public string			acoderx{
			get{return m_acoderx;}
			set{m_acoderx = value;}
		}
		
		public double			g_t{
			get{return m_g_t;}
			set{m_g_t = value;}
		}
		
		public double			lnat{
			get{return m_lnat;}
			set{m_lnat = value;}
		}
		
		public double			aht{
			get{return m_aht;}
			set{m_aht = value;}
		}
		
		public double			afslt{
			get{return m_afslt;}
			set{m_afslt = value;}
		}
		
		public double			afslr{
			get{return m_afslr;}
			set{m_afslr = value;}
		}
		
		public double			txhgmax{
			get{return m_txhgmax;}
			set{m_txhgmax = value;}
		}
		
		public double			rxhgmax{
			get{return m_rxhgmax;}
			set{m_rxhgmax = value;}
		}
		
		public int			satlongit{
			get{return m_satlongit;}
			set{m_satlongit = value;}
		}

		public double			satlong{
			get{return m_satlong;}
			set{m_satlong = value;}
		}

		public string			satlongs{
			get{return m_satlongs;}
			set{m_satlongs = value;}
		}
		
		public double			az{
			get{return m_az;}
			set{m_az = value;}
		}
		
		public double			el{
			get{return m_el;}
			set{m_el = value;}
		}
		
		public double			sarc1{
			get{return m_sarc1;}
			set{m_sarc1 = value;}
		}
		
		public double			sarc2{
			get{return m_sarc2;}
			set{m_sarc2 = value;}
		}
		
		public double			rxpre{
			get{return m_rxpre;}
			set{m_rxpre = value;}
		}
		
		public double			txpre{
			get{return m_txpre;}
			set{m_txpre = value;}
		}
		
		public double			rxtro{
			get{return m_rxtro;}
			set{m_rxtro = value;}
		}
		
		public double			txtro{
			get{return m_txtro;}
			set{m_txtro = value;}
		}

		public string			licence{
			get{return m_licence;}
			set{m_licence = value;}
		}
		
		public string			satname{
			get{return m_satname;}
			set{m_satname = value;}
		}
		
		public string			stata{
			get{return m_stata;}
			set{m_stata = value;}
		}
		
		public string			nota{
			get{return m_nota;}
			set{m_nota = value;}
		}
		
		public string			op2{
			get{return m_op2;}
			set{m_op2 = value;}
		}
		
		public int			antref{
			get{return m_antref;}
			set{m_antref = value;}
		}

		public string			orbit{
			get{return m_orbit;}
			set{m_orbit = value;}
		}
		
		public string			mdate{
			get{return m_mdate;}
			set{m_mdate = value;}
		}
		
		public string			mtime{
			get{return m_mtime;}
			set{m_mtime = value;}
		}
		
		public string			userid{
			get{return m_userid;}
			set{m_userid = value;}
		}


		
		/// <summary>
		/// This is the default constructor.  It will create an empty meAntenna
		///	object.
		/// </summary>
		//	The default constructor
		public meAntenna(): base (eEsType.me)
		{
		}
		
		/// <summary>
		/// This constructor will create an meAntenna from a row in an main.me_ante
		///	table.  The row must be passed in as the parameter.
		/// </summary>
		/// <param name="oDR">The Datarow that contains the meAntenna information
		///	that the user wishes to read into the current object.</param>
		public meAntenna(DataRow oDR): base (eEsType.me, oDR)
		{
		}
		

		public XmlElement meAntennaXml(EsFile oFile)
		{
			return(base.EsAntennaXml(eEsType.me, oFile));
		}
	}


	/// <summary>
	/// This is the channel information for me channels.
	/// </summary>
	public class meChannel: EsChannel 
	{
/*		public string			location{
			get{return m_location;}
			set{m_location = value;}
		}
		public string			call1{
			get{return m_call1;}
			set{m_call1 = value;}
		}
		public string			chid{
			get{return m_chid;}
			set{m_chid = value;}
		}
*/
		public double			freqtx{
			get{return m_freqtx;}
			set{m_freqtx = value;}
		}
		public string			poltx{
			get{return m_poltx;}
			set{m_poltx = value;}
		}
		public double			maxtxpower{
			get{return m_maxtxpower;}
			set{m_maxtxpower = value;}
		}
		public double			pwrtx{
			get{return m_pwrtx;}
			set{m_pwrtx = value;}
		}
		public double			p4khz{
			get{return m_p4khz;}
			set{m_p4khz = value;}
		}

		public string			eqpttx{
			get{return m_eqpttx;}
			set{m_eqpttx = value;}
		}
		public string			traftx{
			get{return m_traftx;}
			set{m_traftx = value;}
		}
		public string			stattx{
			get{return m_stattx;}
			set{m_stattx = value;}
		}
		public string			feetx{
			get{return m_feetx;}
			set{m_feetx = value;}
		}
		public double			freqrx{
			get{return m_freqrx;}
			set{m_freqrx = value;}
		}

		public string			polrx{
			get{return m_polrx;}
			set{m_polrx = value;}
		}
		public double			pwrrx{
			get{return m_pwrrx;}
			set{m_pwrrx = value;}
		}

		public string			eqptrx{
			get{return m_eqptrx;}
			set{m_eqptrx = value;}
		}
		public string			trafrx{
			get{return m_trafrx;}
			set{m_trafrx = value;}
		}
		public string			statrx{
			get{return m_statrx;}
			set{m_statrx = value;}
		}
		public double			i20{
			get{return m_i20;}
			set{m_i20 = value;}
		}
		public double			it01{
			get{return m_it01;}
			set{m_it01 = value;}
		}
		public double			ip01{
			get{return m_ip01;}
			set{m_ip01 = value;}
		}

		public string			feerx{
			get{return m_feerx;}
			set{m_feerx = value;}
		}
		public string			notc{
			get{return m_notc;}
			set{m_notc = value;}
		}
		public string			srvctx{
			get{return m_srvctx;}
			set{m_srvctx = value;}
		}
		public string			srvcrx{
			get{return m_srvcrx;}
			set{m_srvcrx = value;}
		}
		public string			mdate{
			get{return m_mdate;}
			set{m_mdate = value;}
		}
		public string			mtime{
			get{return m_mtime;}
			set{m_mtime = value;}
		}
		public string			userid{
			get{return m_userid;}
			set{m_userid = value;}
		}

		
		/// <summary>
		/// This creates an empty me Channel object.
		/// </summary>
		public meChannel(): base(eEsType.me)
		{
		}
		
		
		/// <summary>
		/// This creates a new me Channel object and fills it in from the current
		///	row that is passed in.
		/// </summary>
		/// <param name="oDR">a DataRow object containing the channel information
		///	</param>
		public meChannel(DataRow oDR): base(eEsType.me, oDR)
		{
		}
		

		public XmlElement meChannelXml(EsFile oFile) {
			return(base.EsChannelXml(eEsType.me, oFile));
		}
	}


	/// <summary>
	/// meRadio is the Es radio structure that reflecEs the status of 
	/// radios in the MDB.  There is no insert or update capability as yet,
	/// as this is still done from the Alpha.
	/// </summary>
	public class meRadio: EsRadio {
		public int depth{
			get{return (int) m_nDepth;} // nDepth is set when created.
		}
/*	
		public string			location{
			get{return m_location;}
			set{m_location = value;}
		}
*/
		public string			name{
			get{return m_name;}
			set{m_name = value;}
		}
		public string			prov{
			get{return m_prov;}
			set{m_prov = value;}
		}
		public string			oper{
			get{return m_oper;}
			set{m_oper = value;}
		}
		public int			latit{
			get{return m_latit;}
			set{m_latit = value;}
		}

		public string			strlatit{
			get{return m_strlatit;}
			set{m_strlatit = value;}
		}
		public string			strlatits{
			get{return m_strlatits;}
			set{m_strlatits = value;}
		}
		public int			longit{
			get{return m_longit;}
			set{m_longit = value;}
		}

		public string			strlongit{
			get{return m_strlongit;}
			set{m_strlongit = value;}
		}
		public string			strlongits{
			get{return m_strlongits;}
			set{m_strlongits = value;}
		}
		public double			grnd{
			get{return m_grnd;}
			set{m_grnd = value;}
		}

		public string			radio{
			get{return m_radio;}
			set{m_radio = value;}
		}
		public int			rain{
			get{return m_rain;}
			set{m_rain = value;}
		}

		public string			sdate{
			get{return m_sdate;}
			set{m_sdate = value;}
		}
		public string			stats{
			get{return m_stats;}
			set{m_stats = value;}
		}
		public string			nots{
			get{return m_nots;}
			set{m_nots = value;}
		}
		public string			oprtyp{
			get{return m_oprtyp;}
			set{m_oprtyp = value;}
		}
		public string			reg{
			get{return m_reg;}
			set{m_reg = value;}
		}
		public string			mdate{
			get{return m_mdate;}
			set{m_mdate = value;}
		}
		public string			mtime{
			get{return m_mtime;}
			set{m_mtime = value;}
		}
		public string			userid{
			get{return m_userid;}
			set{m_userid = value;}
		}

	  
		//	Optional elements
		public string			nameop{
			get{return m_nameop;}
			set{m_nameop = value;}
		}


		/// <summary>
		/// This constructs a basic empty meRadio structure.
		/// </summary>
		public meRadio() 
		{
			m_etype = eEsType.me;
			m_nDepth = 0;	// Indicate that nothing has been set.
			m_numants = 0;
			m_numchan = 0;
			m_numazim = 0;
		}

		/// <summary>
		/// Constructor from a datarow.
		/// </summary>
		/// <param name="oDR">Datarow in a table with all the fields</param>
		public meRadio(DataRow oDR): base(eEsType.me, oDR)
		{
		}


		/// <summary>
		/// This will construct an meRadio object for the specified callsign.
		///	It will read the whole thing into the object
		/// </summary>
		/// <param name="inCall">The callsign of the radio we seek</param>
		/// <param name="nInDepth">Depth is 1 for Site information only,
		///	2 for Site and Antenna data, and 3 for all, Site, Antenna, and Channels
		///	</param>
		public meRadio(string inCall, int nInDepth):
					 	  base(eEsType.me, inCall, "", nInDepth)
		{
		}

		
		
		public void meAddAnt(meAntenna tAnt)
		{            
			base.EsAddAnt(eEsType.me, tAnt);
		}
		

		public void meAddChn(meChannel tChn)
		{ 
			base.EsAddChn(eEsType.me, tChn);
		}
		

		public XmlElement meRadioXml(EsFile oFile) 
		{
			XmlElement tRadNode = base.EsRadioXml(oFile);
			// Now start on the Antennas
			XmlElement tAntennas = oFile.CreateElement("es_antennas");
			for (int nInd = 0; nInd < numants; nInd++){
				tAntennas.AppendChild(((meAntenna) Antennas[nInd]).meAntennaXml(oFile));
			}
			tRadNode.AppendChild(tAntennas);
			
			//	And now the channels
			XmlElement tChannels = oFile.CreateElement("es_channels");
			for (int nInd = 0; nInd < numchan; nInd++){
				tChannels.AppendChild(((meChannel) Channels[nInd]).meChannelXml(oFile));
			}
			tRadNode.AppendChild(tChannels);
			
			//	And now the azimuths
			XmlElement tAzimuths = oFile.CreateElement("es_azimuths");
			for (int nInd = 0; nInd < numazim; nInd++){
				tAzimuths.AppendChild(((meAzimuth) Azimuths[nInd]).meAzimuthXml(oFile));
			}
			tRadNode.AppendChild(tAzimuths);
			
			return tRadNode;
		}
		
	}
	
	/// <summary>
	/// This is the fe Azimuth class
	/// </summary>
	public class feAzimuth:EsAzimuth{
		public string			cmd{
			get{return m_cmd;}
			set{m_cmd = value;}
		}

		public string			recstat{
			get{return m_recstat;}
			set{m_recstat = value;}
		}

		public string			deleteall{
			get{return m_deleteall;}
			set{m_deleteall = value;}
		}

		public string			location{
			get{return m_location;}
			set{m_location = value;}
		}

		public string			call1{
			get{return m_call1;}
			set{m_call1 = value;}
		}
		public double			azim{
			get{return m_azim;}
			set{m_azim = value;}
		}

		public double			elev{
			get{return m_elev;}
			set{m_elev = value;}
		}

		public double			dist{
			get{return m_dist;}
			set{m_dist = value;}
		}

		public double			loss{
			get{return m_loss;}
			set{m_loss = value;}
		}

		public string			mdate{
			get{return m_mdate;}
			set{m_mdate = value;}
		}

		public string			mtime{
			get{return m_mtime;}
			set{m_mtime = value;}
		}

		/// <summary>
		/// Some miscellaneous constructors for the azimuth.
		/// </summary>
		public feAzimuth():base (eEsType.fe){
		}
		
		public feAzimuth(DataRow oDR):base (eEsType.fe, oDR){
		}
		
		/// <summary>
		/// Return the insert command.
		/// </summary>
		/// <param name="sTable">The base name of the table </param>
		/// <returns>The full "INSERT INTO xxx..." SQL command to insert this azimuth into the table
		/// </returns>
		public string InsertCommand(string sTable) {
			return(base.InsertCommand(eEsType.fe, sTable));
		}

		/// <summary>
		/// Copy an azimuth into the current azimuth.
		/// </summary>
		/// <param name="tAz">The azimuth to be copied</param>
		public void feCopAz(feAzimuth tAz){
			base.EsCopAz(eEsType.fe, tAz);
			return;
		}
		
		/// <summary>
		/// Produce the XML representing this azimuth.
		/// </summary>
		/// <param name="oFile">The XML output file it will go to</param>
		/// <returns>an XmlElement with this azimuth in it.</returns>
		public XmlElement feAzimuthXml(EsFile oFile){
			return(base.EsAzimuthXml(eEsType.fe, oFile));
		}
	}
	

	/// <summary>
	/// This is the fe antenna information class.
	/// </summary>
	public class feAntenna: EsAntenna {
		public string			cmd{
			get{return m_cmd;}
			set{m_cmd = value;}
		}

		public string			recstat{
			get{return m_recstat;}
			set{m_recstat = value;}
		}
	/* -- The key is accessed through the base class 
		public string			location{
			get{return m_location;}
			set{m_location = value;}
		}

		public string			call1{
			get{return m_call1;}
			set{m_call1 = value;}
		}
	*/

		public string			txband{
			get{return m_txband;}
			set{m_txband = value;}
		}

		public string			rxband{
			get{return m_rxband;}
			set{m_rxband = value;}
		}

		public string			acodetx{
			get{return m_acodetx;}
			set{m_acodetx = value;}
		}

		public string			acoderx{
			get{return m_acoderx;}
			set{m_acoderx = value;}
		}
		
		public double			g_t{
			get{return m_g_t;}
			set{m_g_t = value;}
		}

		public double			lnat{
			get{return m_lnat;}
			set{m_lnat = value;}
		}

		public double			aht{
			get{return m_aht;}
			set{m_aht = value;}
		}

		public double			afslt{
			get{return m_afslt;}
			set{m_afslt = value;}
		}

		public double			afslr{
			get{return m_afslr;}
			set{m_afslr = value;}
		}

		public double			txhgmax{
			get{return m_txhgmax;}
			set{m_txhgmax = value;}
		}

		public double			rxhgmax{
			get{return m_rxhgmax;}
			set{m_rxhgmax = value;}
		}

		public int			satlongit{
			get{return m_satlongit;}
			set{m_satlongit = value;}
		}

		public double			satlong{
			get{return m_satlong;}
			set{m_satlong = value;}
		}

		public string			satlongs{
			get{return m_satlongs;}
			set{m_satlongs = value;}
		}
		public double			az{
			get{return m_az;}
			set{m_az = value;}
		}

		public double			el{
			get{return m_el;}
			set{m_el = value;}
		}

		public double			sarc1{
			get{return m_sarc1;}
			set{m_sarc1 = value;}
		}

		public double			sarc2{
			get{return m_sarc2;}
			set{m_sarc2 = value;}
		}

		public double			rxpre{
			get{return m_rxpre;}
			set{m_rxpre = value;}
		}

		public double			txpre{
			get{return m_txpre;}
			set{m_txpre = value;}
		}

		public double			rxtro{
			get{return m_rxtro;}
			set{m_rxtro = value;}
		}

		public double			txtro{
			get{return m_txtro;}
			set{m_txtro = value;}
		}

		public string			licence{
			get{return m_licence;}
			set{m_licence = value;}
		}

		public string			satname{
			get{return m_satname;}
			set{m_satname = value;}
		}

		public string			stata{
			get{return m_stata;}
			set{m_stata = value;}
		}

		public string			nota{
			get{return m_nota;}
			set{m_nota = value;}
		}

		public string			op2{
			get{return m_op2;}
			set{m_op2 = value;}
		}
		public int			antref{
			get{return m_antref;}
			set{m_antref = value;}
		}

		public string			orbit{
			get{return m_orbit;}
			set{m_orbit = value;}
		}

		public string			mdate{
			get{return m_mdate;}
			set{m_mdate = value;}
		}

		public string			mtime{
			get{return m_mtime;}
			set{m_mtime = value;}
		}


		
		/// <summary>
		/// Constructor for the empty fe Antenna structure.
		/// </summary>
		public feAntenna(): base (eEsType.fe) {
		}
		
		/// <summary>
		/// Constructor for an fe antenna structure from a datarow in the antenna
		///	table.
		/// </summary>
		/// <param name="oDR">The DataRow object to have the antenna constructed 
		///	from.</param>
		public feAntenna(DataRow oDR): base (eEsType.fe, oDR) {
		}


		/// <summary>
		/// Create the SQL INSERT command to insert this antenna into the specified 
		///	table.
		/// </summary>
		/// <param name="sTable">The base name of the antenna table.</param>
		/// <returns>A string containing the INSERT Command.</returns>
		public string InsertCommand(string sTable) {			
			return(base.InsertCommand(eEsType.fe, sTable));
		}
		
						
		/// <summary>
		/// Copy an Antenna to the current antenna.  Both must be of the same 
		///	type.
		/// </summary>
		/// <param name="tAnt">The antenna structure to be copied.</param>
		public void feCopAnt(feAntenna	tAnt) {		//	Antenna struct source.
			base.EsCopAnt(eEsType.fe, tAnt);
			return;
		}		
	
	
		public XmlElement feAntennaXml(EsFile oFile) {
			return(base.EsAntennaXml(eEsType.fe, oFile));
		}
	}


	/// <summary>
	/// This is the channel information for fe channels.
	/// </summary>
	public class feChannel: EsChannel {

		public string			cmd{
			get{return m_cmd;}
			set{m_cmd = value;}
		}

		public string			recstat{
			get{return m_recstat;}
			set{m_recstat = value;}
		}
 /*
		public string			location{
			get{return m_location;}
			set{m_location = value;}
		}

		public string			call1{
			get{return m_call1;}
			set{m_call1 = value;}
		}

		public string			chid{
			get{return m_chid;}
			set{m_chid = value;}
		}
*/
		public double			freqtx{
			get{return m_freqtx;}
			set{m_freqtx = value;}
		}

		public string			poltx{
			get{return m_poltx;}
			set{m_poltx = value;}
		}

		public double			maxtxpower{
			get{return m_maxtxpower;}
			set{m_maxtxpower = value;}
		}

		public double			pwrtx{
			get{return m_pwrtx;}
			set{m_pwrtx = value;}
		}

		public double			p4khz{
			get{return m_p4khz;}
			set{m_p4khz = value;}
		}

		public string			eqpttx{
			get{return m_eqpttx;}
			set{m_eqpttx = value;}
		}

		public string			traftx{
			get{return m_traftx;}
			set{m_traftx = value;}
		}

		public string			stattx{
			get{return m_stattx;}
			set{m_stattx = value;}
		}

		public string			feetx{
			get{return m_feetx;}
			set{m_feetx = value;}
		}
		public double			freqrx{
			get{return m_freqrx;}
			set{m_freqrx = value;}
		}


		public string			polrx{
			get{return m_polrx;}
			set{m_polrx = value;}
		}
		public double			pwrrx{
			get{return m_pwrrx;}
			set{m_pwrrx = value;}
		}


		public string			eqptrx{
			get{return m_eqptrx;}
			set{m_eqptrx = value;}
		}

		public string			trafrx{
			get{return m_trafrx;}
			set{m_trafrx = value;}
		}

		public string			statrx{
			get{return m_statrx;}
			set{m_statrx = value;}
		}

		public double			i20{
			get{return m_i20;}
			set{m_i20 = value;}
		}

		public double			it01{
			get{return m_it01;}
			set{m_it01 = value;}
		}

		public double			ip01{
			get{return m_ip01;}
			set{m_ip01 = value;}
		}

		public string			feerx{
			get{return m_feerx;}
			set{m_feerx = value;}
		}

		public string			notc{
			get{return m_notc;}
			set{m_notc = value;}
		}

		public string			srvctx{
			get{return m_srvctx;}
			set{m_srvctx = value;}
		}

		public string			srvcrx{
			get{return m_srvcrx;}
			set{m_srvcrx = value;}
		}

		public string			mdate{
			get{return m_mdate;}
			set{m_mdate = value;}
		}

		public string			mtime{
			get{return m_mtime;}
			set{m_mtime = value;}
		}


		
		/// <summary>
		/// The basic constructor for an empty fe Channel.
		/// </summary>
		public feChannel(): base(eEsType.fe) {
		}
		
		/// <summary>
		/// This constructor will create an fe Channel from a row in a channel 
		/// table.
		/// </summary>
		/// <param name="oDR">A DataRow object containing the channel information 
		///	we want.</param>
		public feChannel(DataRow oDR): base(eEsType.fe, oDR) {
		}
		

		/// <summary>
		/// This will create the INSERT command for the current channel.  This 
		/// insert command can be used to add the channel to the table.
		/// </summary>
		/// <param name="sTable">The base name of the table.</param>
		/// <returns>A String containing the SQL INSERT command for the channel.
		/// </returns>
		public string InsertCommand(string sTable) {			
			return(base.InsertCommand(eEsType.fe, sTable));
		}				
	

		/// <summary>
		/// Copy the argument into the current channel.
		/// </summary>
		/// <param name="tChn">Channel object to be copied.</param>
		public void feCopChn(feChannel	tChn) {		//	Channel struct source.
			base.EsCopChn(eEsType.fe, tChn);
			return;
		}		
	

		public XmlElement feChannelXml(EsFile oFile) {
			return(base.EsChannelXml(eEsType.fe, oFile));
		}

	}


	/// <summary>
	/// feRadio is the Es radio structure that reflecEs the status of 
	/// radios in a pdf.  The Radio also contains the arrays of Antennas and
	///	channels (feAntenna(s) and feChannel(s)).
	/// </summary>
	public class feRadio: EsRadio {
	
		public int depth{
			get{return (int) m_nDepth;} // nDepth is set when created.
		}

		public string			cmd{
			get{return m_cmd;}
			set{m_cmd = value;}
		}

		public string			recstat{
			get{return m_recstat;}
			set{m_recstat = value;}
		}
 /*
		public string			location{
			get{return m_location;}
			set{m_location = value;}
		}
*/
		public string			name{
			get{return m_name;}
			set{m_name = value;}
		}

		public string			prov{
			get{return m_prov;}
			set{m_prov = value;}
		}

		public string			oper{
			get{return m_oper;}
			set{m_oper = value;}
		}
		
		public int			latit{
			get{return m_latit;}
			set{m_latit = value;}
		}

		public string			strlatit{
			get{return m_strlatit;}
			set{m_strlatit = value;}
		}

		public string			strlatits{
			get{return m_strlatits;}
			set{m_strlatits = value;}
		}
		
		public int			longit{
			get{return m_longit;}
			set{m_longit = value;}
		}

		public string			strlongit{
			get{return m_strlongit;}
			set{m_strlongit = value;}
		}

		public string			strlongits{
			get{return m_strlongits;}
			set{m_strlongits = value;}
		}
		
		public double			grnd{
			get{return m_grnd;}
			set{m_grnd = value;}
		}

		public string			radio{
			get{return m_radio;}
			set{m_radio = value;}
		}
		
		public int			rain{
			get{return m_rain;}
			set{m_rain = value;}
		}

		public string			sdate{
			get{return m_sdate;}
			set{m_sdate = value;}
		}

		public string			stats{
			get{return m_stats;}
			set{m_stats = value;}
		}

		public string			nots{
			get{return m_nots;}
			set{m_nots = value;}
		}

		public string			oprtyp{
			get{return m_oprtyp;}
			set{m_oprtyp = value;}
		}

		public string			reg{
			get{return m_reg;}
			set{m_reg = value;}
		}

		public string			mdate{
			get{return m_mdate;}
			set{m_mdate = value;}
		}

		public string			mtime{
			get{return m_mtime;}
			set{m_mtime = value;}
		}
	  
		//	Optional elements
		public string			nameop{
			get{return m_nameop;}
			set{m_nameop = value;}
		}



		/// <summary>
		/// Constructor for an empty Site.
		/// </summary>
		public feRadio() {
			m_etype = eEsType.me;
			m_nDepth = 0;	// Indicate that nothing has been set.
			m_numants = 0;
			m_numchan = 0;
			m_numazim = 0;
		}

		/// <summary>
		/// Constructor for a Site to be read in.  The callsign and the file
		///	are given and the whole site (including channels and antennas) is 
		///	read in and initialized.
		/// </summary>
		/// <param name="inCall">Call sign of the site.</param>
		/// <param name="cFile">Base file name of the fe file to read.</param>
		/// <param name="nInDepth">Depth is 1 for Site only, 2 for Site and 
		///	antenna information, and 3 for Site, antenna and Channel information
		///	</param>
		public feRadio(string inCall, string cFile, int nInDepth):
			base(eEsType.fe, inCall, cFile, nInDepth) 
		{
		}
		

		/// <summary>
		/// Constructor from a datarow.
		/// </summary>
		/// <param name="oDR">Datarow in a table with all the fields</param>
		public feRadio(DataRow oDR): base(eEsType.fe, oDR)
		{
		}


		/// <summary>
		/// Shallow copy constructor for an fe Site.  Antenna and Channel arrays are
		///	set to empty in the current object.
		/// </summary>
		/// <param name="tfeRadio">An feRadio to be copied from.</param>
		public feRadio(feRadio tFeRadio)
		{
			m_etype = eEsType.fe;
			m_nDepth = 1;				//	Start at depth of 1.

			this.EsCopRad(eEsType.fe, tFeRadio);

			m_numants = 0;
			m_Antennas = null;
			m_numchan = 0;
			m_Channels = null;
			m_numazim = 0;
			m_Azimuths = null;
		}
		

		public void feAddAnt(feAntenna tAnt)
		{            
			base.EsAddAnt(eEsType.fe, tAnt);
		}
		

		public void feAddChn(feChannel tChn)
		{            
			base.EsAddChn(eEsType.fe, tChn);
		}
		

		//	**********************************************************************
		/// <summary>
		/// Returns the SQL INSERT command to insert this site into the site table
		/// </summary>
		/// <param name="sTable">The base name of the table.</param>
		/// <returns>String containing the INSERT statement.</returns>
		public string InsertCommand(string sTable){			
			return(base.InsertCommand(eEsType.fe, sTable));
		}
				
		
		//	**********************************************************************
		/// <summary>
		/// Find an antenna specified by call1 in a site.
		/// </summary>
		/// <param name="cCall1">Call sign for this link.</param>
		/// <returns>This will return the index of the antenna if found, and 
		///	-1 if no antenna is found matching this description.</returns>
		public int feFindAnt(string cCall1){
			for (int i = 0; i < m_numants && i < Antennas.Count; i++){
				if (cCall1 == ((feAntenna)Antennas[i]).call1){
				  return i;	//	Found.
				}
			}
			return -1;		//	Not found
		}
		
		//**********************************************************************
		/// <summary>
		/// Adds a new empty antenna to the antenna array and returns its index
		/// </summary>
		/// <returns>The index of the new antenna in the antenna array</returns>
		public int feAddAntLoc(){
			Antennas[m_numants] = new feAntenna();
			base.setdepth(2);		//	Make sure that we know about antennas.
			return m_numants++;
		}
		
		
		//	*******************************************************************
		/// <summary>
		/// Given an existing antenna, this routine will either replace or 
		///	add it to the antenna array, copying it in.
		/// </summary>
		/// <param name="tAnt">The antenna to be copied in</param>
		/// <param name="IsOverwrite">True if it is to be overwritten if it is 
		///	already there, false, if it is not.</param>
		/// <returns>0</returns>
		public int feAddAnt(feAntenna tAnt, bool IsOverwrite) {
			int nAntNum = feFindAnt(tAnt.call1);
			
			if (nAntNum < 0){
				//	Not found
				nAntNum = feAddAntLoc();
				IsOverwrite = true;
			}
			
			if (IsOverwrite){
				((feAntenna) Antennas[nAntNum]).feCopAnt(tAnt);
			}
			return 0;
		}
		
		
		//	********************************************************************
		/// <summary>
		/// Get the index of a specified channel in the channel array.
		/// </summary>
		/// <param name="cCall1">Callsign at this end of the link</param>
		/// <param name="cChid">Channel id.</param>
		/// <returns>Index of the channel or -1 if it was not found</returns>
		public int feFindChn(string cLocation, string cChid) 
		{
			for (int i = 0; i < numchan && i < Channels.Count; i++){
				if (cLocation	==	((feChannel) Channels[i]).location &&
						cChid		==	((feChannel) Channels[i]).chid){
					return i;	//	Found.
				}
			}
			return -1;		//	Not found
		}
		
		
		//	********************************************************************
		/// <summary>
		/// Adds a new empty channel to the end of the channel array.
		/// </summary>
		/// <returns>The index of the new channel.</returns>
		public int feAddChnLoc() {
			m_Channels[m_numchan] = new feChannel();
			setdepth(3);
			return m_numchan++;
		}
		
		
		//	*******************************************************************
		/// <summary>
		/// Adds a channel to the Channel array of the site.  If the channel
		///	already exists in the site, the user has the option of overwriting 
		///	it.
		/// </summary>
		/// <param name="tChn">The channel to be added.  It will be copied.</param>
		/// <param name="IsOverwrite">True if the channel is to be overwritten
		///	if present.  False if it is not.</param>
		/// <returns>0</returns>
		public int feAddChn(feChannel tChn, bool IsOverwrite) {
			int nChnNum = feFindChn(tChn.call1, tChn.chid);
			
			if (nChnNum < 0){
				//	Not found
				nChnNum = feAddChnLoc();
				IsOverwrite = true;			//	Make sure that it is copied.
			} 
						
			if (IsOverwrite){
				((feChannel) Channels[nChnNum]).feCopChn(tChn);
			}
			return 0;
		}
		
		
		//	********************************************************************
		/// <summary>
		/// Adds a new empty Azimuth to the end of the channel array.
		/// </summary>
		/// <returns>The index of the new Azimuth.</returns>
		public int feAddAzLoc() {
			m_Azimuths[m_numazim] = new feAzimuth();
			setdepth(4);
			return m_numazim++;
		}
		
		
		//	*******************************************************************
		/// <summary>
		/// Adds an Azimuth to the azimuth array of the site.
		/// </summary>
		/// <param name="tAz">The Azimuth to be added.  It will be copied.</param>
		/// <returns>0</returns>
		public int feAddAzim(feAzimuth tAz) {
			int nAzNum = feAddAzLoc();
			((feAzimuth) Azimuths[nAzNum]).feCopAz(tAz);
			return 0;
		}
		
		
		//	********************************************************************
		/// <summary>
		/// Delete the site from the specified table.  The site record and all
		///	the antennas and channels will be deleted.
		/// </summary>
		/// <param name="sTable">The base name of the table.</param>
		public void DeleteSite(string sTable)
		{
			dbconnect oConnect = new dbconnect();
			OdbcConnection oConn = oConnect.Connection;
			OdbcCommand oDelete = new OdbcCommand(
			                          "DELETE from fe_" + sTable.Trim() +
																						 "_site " +
																			"where location='" + this.location + "'",
																oConn);
			oDelete.ExecuteNonQuery();
			oDelete.Dispose();
			
			oDelete = new OdbcCommand(
									  "DELETE from fe_" + sTable.Trim() + "_ante " +
									        "where location='" + this.location + "'", oConn);
			oDelete.ExecuteNonQuery();
			oDelete.Dispose();
			
			oDelete = new OdbcCommand(
			              "DELETE from fe_" + sTable.Trim() + "_chan " +
			                    "where location='" + this.location + "'", oConn);
			oDelete.ExecuteNonQuery();
			oDelete.Dispose();
			
			oDelete = new OdbcCommand(
			              "DELETE from fe_" + sTable.Trim() + "_azim " +
			                    "where location='" + this.location + "'", oConn);
			oDelete.ExecuteNonQuery();
			oDelete.Dispose();
			
			oConnect.dbdisconnect();	
		}
		
		
		//	********************************************************************
		/// <summary>
		/// Writes out a site to a specified table.  All the antennas and channels
		/// are written as well.
		/// </summary>
		/// <param name="sTable">The base name of the table.</param>
		/// <param name="nDepth">The depth of the write: 1 - Site only, 2 - Site and
		///	antenna, 3 - Site, antenna and channels.  This depth may not be
		///	deeper than the depth the site contains.</param>
		public void WriteSite(string sTable, int nDepth)
		{
			dbconnect oConn = new dbconnect();
			string strCommand;
			
			strCommand = InsertCommand(sTable);
			//	We want to use a transaction so this insertion goes all the 
			//	way or not at all.
			OdbcTransaction oTrans = oConn.Connection.BeginTransaction();
			OdbcCommand oCommand = new OdbcCommand(strCommand, oConn.Connection);
			try {
				if (depth >= 1 && nDepth >= 1){
					oCommand.ExecuteNonQuery();
					
					if (depth >= 2 && nDepth >= 2){
						for (int nInd = 0; nInd < numants; nInd++){
							oCommand.CommandText = ((feAntenna) Antennas[nInd]).InsertCommand(sTable);
							oCommand.ExecuteNonQuery();
						}
						
						if (depth >= 3 && nDepth >= 3){
							for (int nInd = 0; nInd < numchan; nInd++){
								oCommand.CommandText = ((feChannel) Channels[nInd]).InsertCommand(sTable);
								oCommand.ExecuteNonQuery();
							}
						}

						if (depth >= 4 && nDepth >= 4){
							for (int nInd = 0; nInd < numazim; nInd++){
								oCommand.CommandText = ((feAzimuth) Azimuths[nInd]).InsertCommand(sTable);
								oCommand.ExecuteNonQuery();
							}
						}
					}	
					oTrans.Commit();
				}
			} catch (Exception e) {
				oTrans.Rollback();
				throw e;
			}
			
			oConn.dbdisconnect();
		}
		
		
		//	********************************************************************
		/// <summary>
		///	This method will add the site to the table.  If the site is present
		///	already and the IsReplace argument is true it will be deleted first, 
		///	in either case the site will be added.
		///
		///	It will throw an exception if something goes wrong.
		/// </summary>
		public void Add(string sTable, bool IsReplace)
		{
			bool		IsPresent;
			feRadio tRadio = null;
			
			if (IsReplace){
				DeleteSite(sTable);
			}
			
			try {
				tRadio = new feRadio(this.location, sTable, 3);
				IsPresent = true;
			} catch {
				IsPresent = false;
			}
			
			if (IsPresent){
				//	Merge the radio found in the db with the current one.
				//	This only involves adding antennas and channels that are
				//	not present.  The site of the current radio takes precedence.
				for (int nInd = 0; nInd < tRadio.numants; nInd++){
					feAddAnt((feAntenna) tRadio.Antennas[nInd], false); //	Don't overwrite.
				}				
				//	Now the channels
				for (int nInd = 0; nInd < tRadio.numchan; nInd++){
					feAddChn((feChannel) tRadio.Channels[nInd], false); //	Don't overwrite.
				}				
				//	Now the azimuths
				for (int nInd = 0; nInd < tRadio.numazim; nInd++){
					feAddAzim((feAzimuth) tRadio.Azimuths[nInd]); //	Don't overwrite.
				}				
			}
			
			WriteSite(sTable, 3);			
		}
		
		
		public XmlElement feRadioXml(EsFile oFile)
		{
			XmlElement tRadNode = base.EsRadioXml(oFile);
			// Now start on the Antennas
			XmlElement tAntennas = oFile.CreateElement("es_antennas");
			
			for (int nInd = 0; nInd < numants; nInd++){
				tAntennas.AppendChild(((feAntenna) Antennas[nInd]).feAntennaXml(oFile));
			}
			tRadNode.AppendChild(tAntennas);
			
			//	And now the channels
			XmlElement tChannels = oFile.CreateElement("es_channels");
			for (int nInd = 0; nInd < numchan; nInd++){
				tChannels.AppendChild(((feChannel) Channels[nInd]).feChannelXml(oFile));
			}
			tRadNode.AppendChild(tChannels);
			
			//	And now the channels
			XmlElement tAzimuths = oFile.CreateElement("es_azimuths");
			for (int nInd = 0; nInd < numazim; nInd++){
				tAzimuths.AppendChild(((feAzimuth) Azimuths[nInd]).feAzimuthXml(oFile));
			}
			tRadNode.AppendChild(tAzimuths);
			
			return tRadNode;
		}
		
	}
	
}

```
