# Documented File: Subsidiary.cs
**Repository Path:** `DBAccess\Subsidiary.cs`
**Primary Layer:** `DBAccess`
**Namespace:** `DBAccess`

## Source Code Representation
```csharp
using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Text;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Data.Odbc;
using System.Xml;

namespace DBAccess
{

    /// <summary>
    /// This enumeration separates out the subsidiary database items from the 
    ///	subsidiary update file items.
    /// </summary>
    public enum eSubType { sd, su };
    public enum eRequestor { VAL, RADIO }

    
    ////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// This is the base sdf antenna information class.
    /// </summary>
    public class Antenna
    {

        protected eSubType m_type;

        protected string m_cmd;
        protected string m_recstat;
        protected string m_acode;
        public int? v_axtype;
        protected int m_axtype;
        protected string m_axref;
        public double? v_again;
        protected double m_again;
        public double? v_abw;
        protected double m_abw;
        public short? v_arms;
        protected short m_arms;
        protected string m_aband;
        protected string m_amanu;
        protected string m_apattern;
        protected string m_amodel;
        public short? v_anip;
        protected short m_anip;
        public double? v_ax0;
        protected double m_ax0;
        protected string m_adesc;
        protected string m_antype;
        public double? v_aftbr;
        protected double m_aftbr;
        public double? v_lofreq;
        protected double m_lofreq;
        public double? v_hifreq;
        protected double m_hifreq;
        protected string m_bandcodes;
        protected string m_mdate;
        protected string m_mtime;

        protected ArrayList m_apoints;

        private string m_file;

        public eSubType Type
        {
            get { return m_type; }
            set { m_type = value; }
        }

        public string cmd
        {
            get { return m_cmd; }
            set { m_cmd = value; }
        }

        public string recstat
        {
            get { return m_recstat; }
            set { m_recstat = value; }
        }

        public string acode
        {
            get { return m_acode; }
            set { m_acode = value; }
        }
        //public int? axtype
        //{
        //    get { return v_axtype; }
        //    set { v_axtype = value; }
        //}
        public int axtype
        {
            get { return m_axtype; }
            set { m_axtype = value; }
        }

        public string axref
        {
            get { return m_axref; }
            set { m_axref = value; }
        }

        //public double? again
        //{
        //    get { return v_again; }
        //    set { v_again = value; }
        //}
        public double again
        {
            get { return m_again; }
            set { m_again = value; }
        }

        public double abw
        {
            get { return m_abw; }
            set { m_abw = value; }
        }
        //public short? arms
        //{
        //    get { return v_arms; }
        //    set { v_arms = value; }
        //}
        public short arms
        {
            get { return m_arms; }
            set { m_arms = value; }
        }
        public string aband
        {
            get { return m_aband; }
            set { m_aband = value; }
        }

        public string amanu
        {
            get { return m_amanu; }
            set { m_amanu = value; }
        }

        public string apattern
        {
            get { return m_apattern; }
            set { m_apattern = value; }
        }

        public string amodel
        {
            get { return m_amodel; }
            set { m_amodel = value; }
        }

        public short anip
        {
            get { return m_anip; }
            set { m_anip = value; }
        }

        public double ax0
        {
            get { return m_ax0; }
            set { m_ax0 = value; }
        }

        public string adesc
        {
            get { return m_adesc; }
            set { m_adesc = value; }
        }

        public string antype
        {
            get { return m_antype; }
            set { m_antype = value; }
        }
  
        public double aftbr
        {
            get { return m_aftbr; }
            set { m_aftbr = value; }
        }

        public double lofreq
        {
            get { return m_lofreq; }
            set { m_lofreq = value; }
        }

        public double hifreq
        {
            get { return m_hifreq; }
            set { m_hifreq = value; }
        }

        public string bandcodes
        {
            get { return m_bandcodes; }
            set { m_bandcodes = value; }
        }

        public string mdate
        {
            get { return m_mdate; }
            set { m_mdate = value; }
        }

        public string mtime
        {
            get { return m_mtime; }
            set { m_mtime = value; }
        }

        public ArrayList apoints
        {
            get { return m_apoints; }
            set { m_apoints = value; }
        }

        public Antenna()
        {
            m_acode = "";
        }

        public Antenna(string sFile, string sacode)
        {
            string cSQL;
            if (sFile == null || sFile == "" || sFile == "main.sd_ante")
            {
                m_type = eSubType.sd;
                m_file = "main.sd_ante";
                cSQL = "select acode,axtype,axref,again,abw,arms,aband,amanu,apattern," +
                       "amodel,anip,ax0,adesc,antype,aftbr,lofreq,hifreq,bandcodes," +
                       "mdate,mtime " +
                       "from main.sd_ante " +
                       "where acode='" + sacode.Trim() + "'";
            }
            else
            {
                m_type = eSubType.su;
                m_file = sFile;
                cSQL = "select cmd,recstat,acode,axtype,axref,again,abw,arms,aband,amanu,apattern," +
                       "amodel,anip,ax0,adesc,antype,aftbr,lofreq,hifreq,bandcodes," +
                       "mdate,mtime " +
                       "from " + sFile +
                       " where acode='" + sacode.Trim() + "'";
            }

            dbconnect oCn = new dbconnect();

            DataTable oDT = oCn.retrieve(cSQL);
            if (oDT.Rows.Count > 0)
            {
                DataRow oDR = oDT.Rows[0];		//	Get the first row and use that.
                if (m_type == eSubType.sd)
                {
                    m_cmd = "";
                    m_recstat = "";
                }
                else
                {
                    m_cmd = Convert.ToString(oDR["cmd"]);
                    m_recstat = Convert.ToString(oDR["recstat"]);
                }
                m_acode = Convert.ToString(oDR["acode"]);
                v_axtype = FieldIO.getNullableInt32(oDR, "axtype");
                m_axref = Convert.ToString(oDR["axref"]);
                v_again = FieldIO.getNullableDouble(oDR, "again");
                v_abw = FieldIO.getNullableDouble(oDR, "abw");
                v_arms = FieldIO.getNullableInt16(oDR, "arms");
                m_aband = Convert.ToString(oDR["aband"]);
                m_amanu = Convert.ToString(oDR["amanu"]);
                m_apattern = Convert.ToString(oDR["apattern"]);
                m_amodel = Convert.ToString(oDR["amodel"]);
                v_anip = FieldIO.getNullableInt16(oDR, "anip");
                v_ax0 = FieldIO.getNullableDouble(oDR, "ax0");
                m_adesc = Convert.ToString(oDR["adesc"]);
                m_antype = Convert.ToString(oDR["antype"]);
                v_aftbr = FieldIO.getNullableDouble(oDR, "aftbr");
                v_lofreq = FieldIO.getNullableDouble(oDR, "lofreq");
                v_hifreq = FieldIO.getNullableDouble(oDR, "hifreq");
                m_bandcodes = Convert.ToString(oDR["bandcodes"]);
                m_mdate = Convert.ToString(oDR["mdate"]);
                m_mtime = Convert.ToString(oDR["mtime"]);

                if (!v_axtype.HasValue) { m_axtype = 0; } else { m_axtype = v_axtype.Value; }
                if (!v_again.HasValue) { m_again = 0; } else { m_again = v_again.Value; }
                if (!v_abw.HasValue) { m_abw = 0; } else { m_abw = v_abw.Value; }
                if (!v_arms.HasValue) { m_arms = 0; } else { m_arms = v_arms.Value; }
                if (!v_anip.HasValue) { m_anip = 0; } else { m_anip = v_anip.Value; }
                if (!v_ax0.HasValue) { m_ax0 = 0.0; } else { m_ax0 = v_ax0.Value; }
                if (!v_aftbr.HasValue) { m_aftbr = 0.0; } else { m_aftbr = v_aftbr.Value; }
                if (!v_lofreq.HasValue) { m_lofreq = 0.0; } else { m_lofreq = v_lofreq.Value; }
                if (!v_hifreq.HasValue) { m_hifreq = 0.0; } else { m_hifreq = v_hifreq.Value; }

                m_apoints = null;	//	Currently we don't read in the discrimination curve.
            }
            else
            {
                //throw new Exception("NOANTENNA: " + sacode + " in file " + m_file);
                m_acode = "";
            }

            oCn.dbdisconnect();
        }
        /// <summary>
        /// This method is user to obtain list of key values for the antenna records in the specified file
        /// </summary>
        /// <param name="anteTable"></param>
        /// <returns></returns>
        public static DataTable AntennaKeys(string anteTable)
        {
            dbconnect oCn = new dbconnect();
            string cSQL = "Select acode " +
                           " from " + anteTable +
                           " order by acode";

            DataTable oDT = null;
            try
            {
                oDT = oCn.retrieve(cSQL);
                //success = true;
            }
            finally
            {
                oCn.dbdisconnect();
            }
            return oDT;
        }
        public static string AnteUpdate(Antenna testAnte, string sFile)
        {
            string cSQL;
            string retval = "";

            cSQL = "UPDATE " + sFile + " SET " +
                    sFile + ".cmd='N'," +
                    sFile + ".recstat='U'," +
                    sFile + ".axtype=s.axtype," +
                    sFile + ".axref=s.axref," +
                    sFile + ".again=s.again," +
                    sFile + ".abw=s.abw," +
                    sFile + ".arms=s.arms," +
                    sFile + ".aband=s.aband," +
                    sFile + ".amanu=s.amanu," +
                    sFile + ".apattern=s.apattern," +
                    sFile + ".amodel=s.amodel," +
                    sFile + ".anip=s.anip," +
                    sFile + ".ax0=s.ax0," +
                    sFile + ".adesc=s.adesc," +
                    sFile + ".antype=s.antype," +
                    sFile + ".aftbr=s.aftbr," +
                    sFile + ".lofreq=s.lofreq," +
                    sFile + ".hifreq=s.hifreq," +
                    sFile + ".bandcodes=s.bandcodes," +
                    sFile + ".mdate=s.mdate," +
                    sFile + ".mtime=s.mtime " +
                    "FROM " + sFile + " t INNER JOIN main.sd_ante s " +
                    "ON t.acode = s.acode " +
                    "WHERE s.acode='" + testAnte.acode + "'";

            dbconnect oCn = new dbconnect();
            OdbcCommand update1 = new OdbcCommand(cSQL, oCn.Connection);
            try
            {

                try
                {
                    update1.ExecuteNonQuery();
                    retval = "OK";
                }
                catch (Exception e5)
                {
                    //cn.Close();
                    retval = "ERROR:" + cSQL + ":" + e5.Message;
                }
            }
            finally
            {
                oCn.dbdisconnect();
            }

            return retval;
        }
        public static string AntUpdateAnipNonD(string stacode, int intanip, string anteTable)
        {
            string cSQL;
            string retval;

            cSQL = "UPDATE " + anteTable + " SET " +
                    " anip = " + intanip +
                    " WHERE acode = '" + stacode + "'";

            dbconnect oCn = new dbconnect();
            OdbcCommand update1 = new OdbcCommand(cSQL, oCn.Connection);
            try
            {

                try
                {
                    update1.ExecuteNonQuery();
                    retval = "OK";
                }
                catch (Exception e5)
                {
                    //cn.Close();
                    retval = "ERROR:" + cSQL + ":" + e5.Message;
                }
            }
            finally
            {
                oCn.dbdisconnect();
            }

            return retval;
        }
        public static bool UnionAnteChk(string sacode, string anteTable)
        {
            bool found = false;

            dbconnect oCn = new dbconnect();
            string cSQL = "Select acode from main.sd_ante " +
                           " where acode='" + sacode.Trim() + "'" +
                           " UNION " +
                           "select acode from " + anteTable +
                           " where acode='" + sacode.Trim() + "'";
            try
            {
                DataTable oDT = oCn.retrieve(cSQL);
                if (oDT.Rows.Count > 0)
                {
                    found = true;  // antecode found
                }
            }
            catch (Exception)
            {
            }

            oCn.dbdisconnect();
            return found;
        }
        /// <summary>
        /// This is the internal recursive worker function that goes through
        ///	the cross-reference string to get a discrimination array.
        /// </summary>
        /// <param name="nLevel">The level of the search.  We stop at zero.</param>
        ///	<param name="oAnt">The current antenna to check for a curve.</param>
        /// <returns>The discrimination array.  zero length if there is none</returns>
        private AntennaPoint[] m_getxrefcurve(int nLevel, Antenna oAnt)
        {
            string cTrace = "";
            try
            {
                cTrace = "Args: nLevel=" + nLevel.ToString() + ", Antenna=" + oAnt.acode;
                if (nLevel <= 0)
                {
                    return new AntennaPoint[0];		//	Gone too far down the recursion list.
                }
                else if (oAnt.anip > 0)
                {
                    //	We have a curve.
                    cTrace += "\nNumber of Points=" + oAnt.anip.ToString();
                    AntennaPoint[] oAP = new AntennaPoint[oAnt.anip];
                    //	Now load it in.
                    string cSQL = "Select acode,antang,dcov,dxpv,dcoh,dxph,dtilt,interpstat,mdate,mtime " +
                                                    "from main.sd_antd " +
                                                "where acode='" + oAnt.acode + "' " +
                                            "order by antang";
                    dbconnect oCn = new dbconnect();
                    DataTable oDT = oCn.retrieve(cSQL);
                    cTrace += "\nNumber of rows=" + oDT.Rows.Count.ToString();
                    for (int nInd = 0; nInd < oAnt.anip; nInd++)
                    {
                        oAP[nInd] = new AntennaPoint();
                        try { oAP[nInd].acode = oDT.Rows[nInd]["acode"].ToString(); }
                        catch { oAP[nInd].acode = ""; }
                        try { oAP[nInd].antang = Convert.ToDouble(oDT.Rows[nInd]["antang"]); }
                        catch { oAP[nInd].antang = 0.0; }
                        try { oAP[nInd].dcov = Convert.ToDouble(oDT.Rows[nInd]["dcov"]); }
                        catch { oAP[nInd].dcov = 0.0; }
                        try { oAP[nInd].dxpv = Convert.ToDouble(oDT.Rows[nInd]["dxpv"]); }
                        catch { oAP[nInd].dxpv = 0.0; }
                        try { oAP[nInd].dcoh = Convert.ToDouble(oDT.Rows[nInd]["dcoh"]); }
                        catch { oAP[nInd].dcoh = 0.0; }
                        try { oAP[nInd].dxph = Convert.ToDouble(oDT.Rows[nInd]["dxph"]); }
                        catch { oAP[nInd].dxph = 0.0; }
                        try { oAP[nInd].dtilt = Convert.ToDouble(oDT.Rows[nInd]["dtilt"]); }
                        catch { oAP[nInd].dtilt = 0.0; }
                        try { oAP[nInd].interpstat = Convert.ToInt64(oDT.Rows[nInd]["interpstat"]); }
                        catch { oAP[nInd].interpstat = 0; }
                        try { oAP[nInd].mdate = oDT.Rows[nInd]["mdate"].ToString(); }
                        catch { oAP[nInd].mdate = ""; }
                        try { oAP[nInd].mtime = oDT.Rows[nInd]["mtime"].ToString(); }
                        catch { oAP[nInd].mtime = ""; }
                    }
                    oCn.dbdisconnect();

                    return oAP;

                }
                else if (oAnt.axref.Trim() != "")
                {
                    //	No points, but it does have an xref.
                    try
                    {
                        cTrace += "\nCrossreferencing:";
                        Antenna oNextAnt = new Antenna("", oAnt.axref);  //	Get the antenna.
                        return m_getxrefcurve(nLevel - 1, oNextAnt);
                    }
                    catch (Exception Ex)
                    {
                        throw new Exception("ANTENNA - Error crossreferencing:-\n" + Ex.Message);
                    }
                }
                else
                {
                    //	No cross reference.
                    return new AntennaPoint[0];
                }
            }
            catch (Exception Ex)
            {
                throw new Exception("ANTENNA - Getting xref curve:\n" + cTrace + "\n" + Ex.Message);
            }
        }


        /// <summary>
        /// Retrieve the discrimination curve for a given antenna, even if it means 
        ///	going through a string of xrefs.
        /// </summary>
        /// <returns></returns>
        public AntennaPoint[] getxrefcurve()
        {
            return m_getxrefcurve(5, this);
        }


        /// <summary>
        /// This routine is used to support the area coordination calculations where the
        ///	average of the antennas is used.
        /// </summary>
        /// <returns>The average gain of the antenna throughout its discrimination curve.
        ///	If no discrimination curve is found, then it will return the antenna gain.
        /// </returns>
        public double avgain()
        {
            double dGain = again;
            double dPrevGain = 0.0;
            AntennaPoint[] aAP = null;

            //	We need the average gain for a point to multipoint.  Calculate it here.
            try
            {
                aAP = getxrefcurve();
            }
            catch (Exception Ex)
            {
                throw new Exception("ANTENNA - Could not get discrimination curve:-\n" +
                                    Ex.Message);
            }
            double dArea = 0.0;
            //	We are calculating the average gain from the on-axis gain and the 
            //	minimum envelope of the discrimination curve.
            dPrevGain = dGain - OELSupport.MinOf(aAP[0].dcoh.Value, aAP[0].dcov.Value,
                                                     aAP[0].dxph.Value, aAP[0].dxpv.Value);
            //	Get the power values of the Gain.
            dPrevGain = Math.Pow(10.0, dPrevGain / 10.0);		//	Convert to W.

            double dCurrGain = 0.0;
            double dPrevAng = aAP[0].antang.Value;
            for (int nInd = 1; nInd < aAP.Length; nInd++)
            {
                // The discrimination is stored in db, we need mW to calculate the area
                dCurrGain = dGain - OELSupport.MinOf(aAP[nInd].dcoh.Value, aAP[nInd].dcov.Value,
                                                       aAP[nInd].dxph.Value, aAP[nInd].dxpv.Value);
                dCurrGain = Math.Pow(10.0, dCurrGain / 10.0);
                //	Trapezoidal Rule for integration.
                dArea += (dCurrGain + dPrevGain) * (aAP[nInd].antang.Value - dPrevAng) / 2.0;
                dPrevGain = dCurrGain;
                dPrevAng = aAP[nInd].antang.Value;
            }

            //	We have the Area under the curve in dArea and the last angle in dPrevAng
            if (dPrevAng > 0.0)
            {
                //	Calculate the average gain in db.
                dGain = 10.0 * Math.Log10(dArea / dPrevAng);
            }

            return dGain;
        }
    }
 
    ////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// This is the base sdf antenna discrimination information class.
    /// </summary>   
    public class AntennaPoint
    {
        public eSubType m_type;

        public string cmd;
        public string recstat;
        public string acode;
        public double? antang;
        public double? dcov;
        public double? dxpv;
        public double? dcoh;
        public double? dxph;
        public double? dtilt;
        // interpstat changed to long 2010/09/22 BA
        //public int? interpstat;
        public long? interpstat;
        public string mdate;
        public string mtime;

        public AntennaPoint()
        {
            cmd = "";
            acode = "";
            antang = 0.0;
            dcov =
            dxpv =
            dcoh =
            dxph = 0.0;
            dtilt = 0.0;
            interpstat = 0;
            mdate = "";
            mtime = "";
        }

        public AntennaPoint (string sFile, string sacode, double dantang)
        {
            string cSQL;
            if (sFile == null || sFile == "" || sFile == "main.sd_antd")
            {
                m_type = eSubType.sd;
                cSQL = "Select acode,antang,dcov,dxpv,dcoh,dxph,dtilt,interpstat,mdate,mtime " +
                       "from main.sd_antd " +
                       "where acode='" + sacode.Trim() + "' " +
                       "and antang = " + dantang;
            }
            else
            {
                m_type = eSubType.su;
                cSQL = "Select cmd,acode,antang,dcov,dxpv,dcoh,dxph,dtilt,interpstat,mdate,mtime " +
                       "from " + sFile +
                       " where acode='" + sacode.Trim() + "' " +
                       " and antang = " + dantang;
            }
            dbconnect oCn = new dbconnect();
            try
            {
                DataTable oDT = oCn.retrieve(cSQL);
                if (oDT.Rows.Count > 0)
                {
                    //	Take the first row and use that as the antd.
                    DataRow oDR = oDT.Rows[0];
                    if (m_type == eSubType.sd)
                    {
                        cmd = "";
                    }
                    else
                    {
                        cmd = Convert.ToString(oDR["cmd"]);
                    }
                    acode = Convert.ToString(oDR["acode"]);
                    antang = Convert.ToDouble(oDR["antang"]);
                    //catch { antang = 0.0; }
                    dcov = FieldIO.getNullableDouble(oDR, "dcov");
                    //catch { dcov = 0.0; }
                    dxpv = FieldIO.getNullableDouble(oDR, "dxpv");
                    //catch { dxpv = 0.0; }
                    dcoh = FieldIO.getNullableDouble(oDR, "dcoh");
                    //catch { dcoh = 0.0; }
                    dxph = FieldIO.getNullableDouble(oDR, "dxph");
                    //catch { dxph = 0.0; }
                    dtilt = FieldIO.getNullableDouble(oDR, "dtilt");
                    //catch { dtilt = 0.0; }
                    interpstat = FieldIO.getNullableInt64(oDR, "interpstat");
                    //catch { interpstat = 0; }
                    mdate = Convert.ToString(oDR["mdate"]);
                    mtime = Convert.ToString(oDR["mtime"]);
                }
                else
                {
                    acode = "";
                }

                oCn.dbdisconnect();
                
            }
            catch (Exception Ex)
            {
                throw new Exception("ERROR Getting antenna point from SDB:" + sacode + "-" + dantang.ToString() + ":" + Ex.Message);
            }
        }

        public AntennaPoint(string sacode, double dantang, string antdTable)
        {
            dbconnect oCn = new dbconnect();
            string cSQL = "Select acode,antang,dcov,dxpv,dcoh,dxph,dtilt,interpstat,mdate,mtime " +
                          "from " + antdTable +
                          " where acode='" + sacode.Trim() + "' " +
                          "and antang = " + dantang;
            try
            {
                DataTable oDT = oCn.retrieve(cSQL);
                if (oDT.Rows.Count > 0)
                {
                    //	Take the first row and use that as the antd.
                    DataRow oDR = oDT.Rows[0];
                    cmd = Convert.ToString(oDR["cmd"]);
                    acode = Convert.ToString(oDR["acode"]);
                    antang = Convert.ToDouble(oDR["antang"]);
                    //catch { antang = 0.0; }
                    dcov = Convert.ToDouble(oDR["dcov"]);
                    //catch { dcov = 0.0; }     // added BA 2010/3/29
                    dxpv = Convert.ToDouble(oDR["dxpv"]);
                    //catch { dxpv = 0.0; }     // added BA 2010/3/29
                    dcoh = Convert.ToDouble(oDR["dcoh"]);
                    //catch { dcoh = 0.0; }     // added BA 2010/3/29
                    dxph = Convert.ToDouble(oDR["dxph"]);
                    //catch { dxph = 0.0; }     // added BA 2010/3/29
                    dtilt = Convert.ToDouble(oDR["dtilt"]);
                    //catch { dtilt = 0.0; }     // added BA 2010/3/29
                    interpstat = Convert.ToInt64(oDR["interpstat"]);
                    //catch { interpstat = 0; }
                    mdate = Convert.ToString(oDR["mdate"]);
                    mtime = Convert.ToString(oDR["mtime"]);
                }
                else
                {
                    acode = "";
                }

                oCn.dbdisconnect();

            }
            catch (Exception Ex)
            {
                throw new Exception("Error getting antenna point from " + antdTable + ":" + sacode + "-" + dantang.ToString() + ":" + Ex.Message);
            }
        }

        public static DataTable AntennaPointKeys(string antdTable, string sacode)
        {
            dbconnect oCn = new dbconnect();
            string cSQL = "Select acode, antang " +
                           " from " + antdTable +
                           " where acode = '" + sacode.Trim() +
                           "' order by acode, antang";

            DataTable oDT = null;
            try
            {
                oDT = oCn.retrieve(cSQL);
                //success = true;
            }
            finally
            {
                oCn.dbdisconnect();
            }
            return oDT;
        }
        // added 2019/10/3 to count non deletion points
        // could also be done with select count(*) but this retains standard structure and tables are small
        public static DataTable AntennaPointKeysnonD(string antdTable, string sacode)
        {
            dbconnect oCn = new dbconnect();
            string cSQL = "Select acode, antang " +
                           " from " + antdTable +
                           " where acode = '" + sacode.Trim() +
                           "' AND cmd <> 'D'";

            DataTable oDT = null;
            try
            {
                oDT = oCn.retrieve(cSQL);
                //success = true;
            }
            finally
            {
                oCn.dbdisconnect();
            }
            return oDT;
        }
        
        public static string AntdUpdate(AntennaPoint testAntd, string sFile)
        {
            string cSQL;
            string retval = "";

            cSQL = "UPDATE " + sFile + " SET " +
                    sFile + ".cmd='N'," +
                    sFile + ".dcov=s.dcov," +
                    sFile + ".dxpv=s.dxpv," +
                    sFile + ".dcoh=s.dcoh," +
                    sFile + ".dxph=s.dxph," +
                    sFile + ".dtilt=s.dtilt," +
                    sFile + ".interpstat=s.interpstat," +
                    sFile + ".mdate=s.mdate," +
                    sFile + ".mtime=s.mtime " +
                    "FROM " + sFile + " t INNER JOIN main.sd_antd s " +
                    "ON t.acode = s.acode AND t.antang = s.antang " +
                    "WHERE s.acode='" + testAntd.acode + "' " +
                    "AND s.antang=" + testAntd.antang;

            dbconnect oCn = new dbconnect();
            OdbcCommand update1 = new OdbcCommand(cSQL, oCn.Connection);
            try
            {

                try
                {
                    update1.ExecuteNonQuery();
                    retval = "OK";
                }
                catch (Exception e5)
                {
                    //cn.Close();
                    retval = "ERROR:" + cSQL + ":" + e5.Message;
                }
            }
            finally
            {
                oCn.dbdisconnect();
            }

            return retval;
        }   
        
        public static bool UnionAntdChk(string sacode, double dantang, string antdTable)
        {
            bool found = false;

            dbconnect oCn = new dbconnect();
            string cSQL = "Select acode from main.sd_ante " +
                           " where acode='" + sacode.Trim() + "'" +
                           " and antang = " + dantang +
                           " UNION " +
                           "select acode from " + antdTable +
                           " where acode='" + sacode.Trim() + "'" +
                           " and antang = " + dantang;
           try
            {
                DataTable oDT = oCn.retrieve(cSQL);
                if (oDT.Rows.Count > 0)
                {
                    found = true;  // antecode found
                }
            }
            catch (Exception)
            {
            }

            oCn.dbdisconnect();
            return found;
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// This is the base sdf band information class.
    /// </summary> 
    public class Band
    {
        protected eSubType m_type;

        public string cmd;
        public string recstat;
        public string bndcde;
        public Int16? v_bandbitpos;
        public Int16 bandbitpos;
        public double? v_blo;
        public double blo;
        public double? v_bmidf;
        public double bmidf;
        public double? v_bhi;
        public double bhi;
        public string badj;
        public string mdate;
        public string mtime;

        public Band()
        {
            bndcde = "";			//	indicate uninitialized.
        }
         
        public Band(string sFile, string sbndcde)
        {
            string cSQL;
            if (sFile == null || sFile == "" || sFile == "main.sd_band")
            {
                m_type = eSubType.sd;
                cSQL = "Select bndcde, bandbitpos, blo, bmidf, bhi, badj, mdate, mtime " +
                       "from main.sd_band" +
                       " where bndcde='" + sbndcde.Trim() + "'";
            }
            else
            {
                m_type = eSubType.su;
                cSQL = "Select cmd, recstat, bndcde, bandbitpos, blo, bmidf, bhi, badj, mdate, mtime " +
                       "from " + sFile +
                       " where bndcde='" + sbndcde.Trim() + "'";
            }

            dbconnect oCn = new dbconnect();
            try
            {
                DataTable oDT = oCn.retrieve(cSQL);
                if (oDT.Rows.Count > 0)
                {
                    //	Take the first row and use that as the band.
                    DataRow oDR = oDT.Rows[0];
                    if (m_type == eSubType.sd)
                    {
                        cmd = "";
                        recstat = "";
                    }
                    else
                    {
                        cmd = Convert.ToString(oDR["cmd"]);
                        recstat = Convert.ToString(oDR["recstat"]);
                    }
                    bndcde = Convert.ToString(oDR["bndcde"]);
                    v_bandbitpos = FieldIO.getNullableInt16(oDR, "bandbitpos");
                    v_blo = FieldIO.getNullableDouble(oDR, "blo");
                    v_bmidf = FieldIO.getNullableDouble(oDR, "bmidf");
                    v_bhi = FieldIO.getNullableDouble(oDR, "bhi");
                    badj = Convert.ToString(oDR["badj"]);
                    mdate = Convert.ToString(oDR["mdate"]);
                    mtime = Convert.ToString(oDR["mtime"]);

                    // set any nulls to defaults
                    if (!v_bandbitpos.HasValue) { bandbitpos = 0; } else { bandbitpos = v_bandbitpos.Value; }
                    if (!v_blo.HasValue) { blo = 0.0; } else { blo = v_blo.Value; }
                    if (!v_bmidf.HasValue) { bmidf = 0.0; } else { bmidf = v_bmidf.Value; }
                    if (!v_bhi.HasValue) { bhi = 0.0; } else { bhi = v_bhi.Value; }
                }
                else
                {
                    //	Indicate that the bandcode was not found.
                    bndcde = "";
                }
            }
            finally
            {
                oCn.dbdisconnect();
            }

        }
 
        /// <summary>
        /// This method is used to obtain list of key values for the band records in the specified file
        /// </summary>
        /// <param name="anteTable"></param>
        /// <returns></returns>
        public static DataTable BandKeys(string bandTable)
        {
            dbconnect oCn = new dbconnect();
            string cSQL = "Select bndcde " +
                           " from " + bandTable +
                           " order by bndcde";

            DataTable oDT = null;
            try
            {
                oDT = oCn.retrieve(cSQL);
            }
            finally
            {
                oCn.dbdisconnect();
            }
            return oDT;
        }

        public static Band[] Bands()
        {
            dbconnect oCn = new dbconnect();
            int nCount = 0;
            int nInd = 0;

            nCount = (int)oCn.getscalar("select count(*) from main.sd_band");

            Band[] oBands = new Band[nCount];

            DataTable oDT = oCn.retrieve("select * from main.sd_band order by bndcde");
            foreach (DataRow oDR in oDT.Rows)
            {
                try
                {
                    oBands[nInd] = new Band();
                    oBands[nInd].bndcde = Convert.ToString(oDR["bndcde"]);
                    oBands[nInd].v_bandbitpos = FieldIO.getNullableInt16(oDR, "bandbitpos");
                    oBands[nInd].v_blo = FieldIO.getNullableDouble(oDR, "blo");
                    oBands[nInd].v_bmidf = FieldIO.getNullableDouble(oDR, "bmidf");
                    oBands[nInd].v_bhi = FieldIO.getNullableDouble(oDR, "bhi");
                    oBands[nInd].badj = Convert.ToString(oDR["badj"]);
                    oBands[nInd].mdate = Convert.ToString(oDR["mdate"]);
                    oBands[nInd].mtime = Convert.ToString(oDR["mtime"]);

                    // set defaults
                    if (!oBands[nInd].v_bandbitpos.HasValue) { oBands[nInd].bandbitpos = 0; }
                    if (!oBands[nInd].v_blo.HasValue) { oBands[nInd].blo = 0.0; }
                    if (!oBands[nInd].v_bmidf.HasValue) { oBands[nInd].bmidf = 0.0; }
                    if (!oBands[nInd].v_bhi.HasValue) { oBands[nInd].bhi = 0.0; }
                }
                catch (Exception Ex) { throw new Exception("Error loading Bands: " + Ex.Message); }
                nInd++;
            }

            return oBands;
        }
        public static string BandUpdate(Band testBand, string sFile)
        {
            // this routine is currently only called by sdfValidate
            string cSQL;
            string retval = "";

            cSQL = "UPDATE " + sFile + " SET " +
                sFile + ".cmd='N'," +
                sFile + ".recstat='U'," +
                sFile + ".bandbitpos = s.bandbitpos," +
                sFile + ".blo = s.blo," +
                sFile + ".bmidf = s.bmidf," +
                sFile + ".bhi = s.bhi," +
                sFile + ".badj = s.badj," +
                sFile + ".mdate = s.mdate," +
                sFile + ".mtime = s.mtime " +
                "FROM " + sFile + " t INNER JOIN main.sd_band s " +
                "ON t.bndcde = s.bndcde " +
                "WHERE s.bndcde = '" + testBand.bndcde + "'";
            
            dbconnect oCn = new dbconnect();
            OdbcCommand update1 = new OdbcCommand(cSQL, oCn.Connection);
            try
            {

                try
                {
                    update1.ExecuteNonQuery();
                    retval = "OK";
                }
                catch (Exception e5)
                {
                    //cn.Close();
                    retval = "ERROR:" + cSQL + ":" + e5.Message;
                }
            }
            finally
            {
                oCn.dbdisconnect();
            }

            return retval;
        }     
        public static bool UnionBandChk(string bandkey, string bandTable)
        {
            bool found = false;

            dbconnect oCn = new dbconnect();
            string cSQL = "Select bndcde from main.sd_band " +
                           " where bndcde='" + bandkey.Trim() + "'" +
                           " UNION " +
                           "select bndcde from " + bandTable +
                           " where bndcde='" + bandkey.Trim() + "'";
            try
            {
                DataTable oDT = oCn.retrieve(cSQL);
                if (oDT.Rows.Count > 0)
                {
                    found = true;  // bandcode found
                }
            }
            catch (Exception)
            {
            }

            oCn.dbdisconnect();
            return found;
        }

    }

    public class Ctx
    {
        protected eSubType m_type;

        public string cmd;
        public string recstat;
        public string tfcr;
        public string tfci;
        public string rxeqp;
        public float? v_rqco;
        public float? v_rqcull;
        public float? v_rqwrst;
        public short? v_ctxndp;
        public string ctxdesc;
        public string mdate;
        public string mtime;

        public Ctx()
        {
            tfcr = "";			//	indicate uninitialized.
        }

         public Ctx(string sFile, string stfcr, string stfci, string srxeqp)
        {
            string cSQL;
            if (sFile == null || sFile == "" || sFile == "main.sd_ctx")
            {
                m_type = eSubType.sd;
                cSQL = "Select tfcr, tfci, rxeqp, rqco, rqcull, " +
                       "rqwrst, ctxndp, ctxdesc, mdate, mtime " +
                       "from main.sd_ctx" +
                       " where tfcr='" + stfcr.Trim() +
                       "' and tfci='" + stfci.Trim() +
                       "' and rxeqp='" + srxeqp.Trim() + "'";
            }
            else
            {
                m_type = eSubType.su;
                cSQL = "Select cmd, recstat, tfcr, tfci, rxeqp, rqco, rqcull, " +
                       "rqwrst, ctxndp, ctxdesc, mdate, mtime " +
                       "from " + sFile +
                       " where tfcr='" + stfcr.Trim() +
                       "' and tfci='" + stfci.Trim() + 
                       "' and rxeqp='" + srxeqp.Trim() + "'";
            }

            dbconnect oCn = new dbconnect();
            try
            {
                DataTable oDT = oCn.retrieve(cSQL);
                if (oDT.Rows.Count > 0)
                {
                    //	Take the first row and use that as the ctx.
                    DataRow oDR = oDT.Rows[0];
                    if (m_type == eSubType.sd)
                    {
                        cmd = "";
                        recstat = "";
                    }
                    else
                    {
                        cmd = Convert.ToString(oDR["cmd"]);
                        recstat = Convert.ToString(oDR["recstat"]);
                    }
                    tfcr = Convert.ToString(oDR["tfcr"]);
                    tfci = Convert.ToString(oDR["tfci"]);
                    rxeqp = Convert.ToString(oDR["rxeqp"]);
                    v_rqco = FieldIO.getNullableSingle(oDR, "rqco");
                    v_rqcull = FieldIO.getNullableSingle(oDR, "rqcull");
                    v_rqwrst = FieldIO.getNullableSingle(oDR, "rqwrst");
                    v_ctxndp = FieldIO.getNullableInt16(oDR, "ctxndp");
                    if(!v_ctxndp.HasValue){v_ctxndp = 0;}
                    ctxdesc = Convert.ToString(oDR["ctxdesc"]);
                    mdate = Convert.ToString(oDR["mdate"]);
                    mtime = Convert.ToString(oDR["mtime"]);
                }
                else
                {
                    //	Indicate that the ctx was not found.
                    tfcr = "";
                }
            }
            catch
            {
            }

            oCn.dbdisconnect();
        }

        public static DataTable CtxKeys(string ctxTable)
        {
            dbconnect oCn = new dbconnect();
            string cSQL = "Select tfcr, tfci, rxeqp" +
                           " from " + ctxTable +
                           " order by tfcr, tfci, rxeqp";

            DataTable oDT = null;
            try
            {
                oDT = oCn.retrieve(cSQL);
            }
            finally
            {
                oCn.dbdisconnect();
            }
            return oDT;
        }
        public static string CtxUpdate(Ctx testCtx, string sFile)
        {
            string cSQL;
            string retval = "";

            cSQL = "UPDATE " + sFile + " SET " +
                    sFile + ".cmd='N'," +
                    sFile + ".recstat='U'," +
                    sFile + ".tfcr = s.tfcr," +
                    sFile + ".tfci = s.tfci," +
                    sFile + ".rxeqp = s.rxeqp," +
                    sFile + ".rqco = s.rqco," +
                    sFile + ".rqcull = s.rqcull," +
                    sFile + ".rqwrst = s.rqwrst," +
                    sFile + ".ctxndp = s.ctxndp," +
                    sFile + ".ctxdesc = s.ctxdesc," +
                    sFile + ".mdate=s.mdate," +
                    sFile + ".mtime=s.mtime " +
                    "FROM " + sFile + " t INNER JOIN main.sd_ctx s " +
                    "ON t.tfcr = s.tfcr AND t.tfci = s.tfci AND t.rxeqp = s.rxeqp " +
                    "WHERE s.tfcr='" + testCtx.tfcr + "' " +
                    "AND s.tfci='" + testCtx.tfci + "' " +
                    "AND s.rxeqp='" + testCtx.rxeqp + "'";

            dbconnect oCn = new dbconnect();
            OdbcCommand update1 = new OdbcCommand(cSQL, oCn.Connection);
            try
            {

                try
                {
                    update1.ExecuteNonQuery();
                    retval = "OK";
                }
                catch (Exception e5)
                {
                    //cn.Close();
                    retval = "ERROR:" + cSQL + ":" + e5.Message;
                }
            }
            finally
            {
                oCn.dbdisconnect();
            }

            return retval;
        }
        public static string CtxUpdateCtxndpNonD(string ctxTable, string stfcr, string stfci, string srxeqp, int intctxndp)
        {
            string cSQL;
            string retval;

            cSQL = "UPDATE " + ctxTable + " SET " +
                    " ctxndp = " + intctxndp +
                    " where tfcr ='" + stfcr.Trim() +
                    "' and tfci = '" + stfci.Trim() +
                    "' and rxeqp = '" + srxeqp.Trim() + "'";

            dbconnect oCn = new dbconnect();
            OdbcCommand update1 = new OdbcCommand(cSQL, oCn.Connection);
            try
            {

                try
                {
                    update1.ExecuteNonQuery();
                    retval = "OK";
                }
                catch (Exception e5)
                {
                    //cn.Close();
                    retval = "ERROR:" + cSQL + ":" + e5.Message;
                }
            }
            finally
            {
                oCn.dbdisconnect();
            }

            return retval;
        }

        public static bool UnionCtxChk(string strfcr, string strfci, string srxeqp, string ctxTable)
        {
            bool found = false;

            dbconnect oCn = new dbconnect();
            string cSQL = "Select trfrc, trfci, rxeqp from main.sd_ctx_ " +
                           " where trfcr='" + strfcr.Trim() + "'" +
                           " and trfci='" + strfci.Trim() + "'" +
                           " and rxeqp='" + srxeqp.Trim() + "'" +
                           " UNION " +
                           "Select trfrc, trfci, rxeqp from " + ctxTable +
                           " where trfcr='" + strfcr.Trim() + "'" +
                           " and trfci='" + strfci.Trim() + "'" +
                           " and rxeqp='" + srxeqp.Trim() + "'";
            try
            {
                DataTable oDT = oCn.retrieve(cSQL);
                if (oDT.Rows.Count > 0)
                {
                    found = true;  // bandcode found
                }
            }
            catch (Exception)
            {
            }

            oCn.dbdisconnect();
            return found;
        }

    }
    public class Ctxd
    {
        protected eSubType m_type;

        public string cmd;
        public string recstat;
        public string tfcr;
        public string tfci;
        public string rxeqp;
        public float? v_fsep;
        public float? v_rq;
        public string mdate;
        public string mtime;

        public Ctxd()
        {
            tfcr = "";			//	indicate uninitialized.
        }

        public Ctxd(string sFile, string stfcr, string stfci, string srxeqp, float dfsep)
        {
            //string logfile = "D:\\extractlogs\\Ctxd.txt";  //  log file
            // open logfile
            //StreamWriter swsapr = new StreamWriter(logfile, false);

            //DateTime curTime = DateTime.Now;
            //string log_time = curTime.ToString("yyyy/MM/dd HH:mm:ss");

            // write info to log file
            //swsapr.WriteLine(log_time);
            //swsapr.Flush();

            string cSQL;
            if (sFile == null || sFile == "" || sFile == "main.sd_ctxd")
            {
                m_type = eSubType.sd;
                cSQL = "Select  tfcr, tfci, rxeqp, fsep, rq, mdate, mtime " +
                               "from main.sd_ctxd " +
                               " where tfcr='" + stfcr.Trim() +
                               "' and tfci='" + stfci.Trim() +
                               "' and rxeqp='" + srxeqp.Trim() +
                               "' and fsep=" + dfsep.ToString();
            }
            else
            {
                m_type = eSubType.su;
                cSQL = "Select cmd, recstat, tfcr, tfci, rxeqp, fsep, rq, mdate, mtime " +
                               "from " + sFile +
                               " where tfcr='" + stfcr.Trim() +
                               "' and tfci='" + stfci.Trim() +
                               "' and rxeqp='" + srxeqp.Trim() +
                               "' and fsep=" + dfsep.ToString();
            }

            //swsapr.WriteLine(cSQL);
            //swsapr.Flush();

            dbconnect oCn = new dbconnect();
            try
            {
                DataTable oDT = oCn.retrieve(cSQL);
                if (oDT.Rows.Count > 0)
                {
                    //swsapr.WriteLine("Record found");
                    //swsapr.Flush();

                    //	Take the first row and use that as the ctxd.
                    DataRow oDR = oDT.Rows[0];
                    if (m_type == eSubType.sd)
                    {
                        cmd = "";
                        recstat = "";
                    }
                    else
                    {
                        cmd = Convert.ToString(oDR["cmd"]);
                        recstat = Convert.ToString(oDR["recstat"]);
                    }
                    tfcr = Convert.ToString(oDR["tfcr"]);
                    tfci = Convert.ToString(oDR["tfci"]);
                    rxeqp = Convert.ToString(oDR["rxeqp"]);
                    v_fsep = FieldIO.getNullableSingle(oDR, "fsep");
                    v_rq = FieldIO.getNullableSingle(oDR, "rq");
                    mdate = Convert.ToString(oDR["mdate"]);
                    mtime = Convert.ToString(oDR["mtime"]);
                }
                else
                {
                    //swsapr.WriteLine("Record not found");
                    //swsapr.Flush();

                    //	Indicate that the ctxd was not found.
                    tfcr = "";
                }
            }
            catch( Exception)
            {
                //swsapr.WriteLine("SQL ERROR" + cSQL + ":" + e5.Message);
                //swsapr.Flush();
            }
            //swsapr.Close();
            oCn.dbdisconnect();
        }
        
        public static DataTable CtxdKeys(string ctxdTable, string stfcr, string stfci, string srxeqp)
        {
            dbconnect oCn = new dbconnect();
            string cSQL = "Select tfcr, tfci, rxeqp, fsep " +
                           " from " + ctxdTable +
                           " where tfcr ='" + stfcr.Trim() +
                           "' and tfci = '" + stfci.Trim() +
                           "' and rxeqp = '" + srxeqp.Trim() +
                           "' order by tfcr, tfci, rxeqp, fsep";

            DataTable oDT = null;
            try
            {
                oDT = oCn.retrieve(cSQL);
            }
            finally
            {
                oCn.dbdisconnect();
            }
            return oDT;
        }
        // added 2019/10/3 to count non deletion points
        // could also be done with select count(*) but this retains standard structure and tables are small
        public static DataTable CtxdKeysNonD(string ctxdTable, string stfcr, string stfci, string srxeqp)
        {
            dbconnect oCn = new dbconnect();
            string cSQL = "Select tfcr, tfci, rxeqp, fsep " +
                           " from " + ctxdTable +
                           " where tfcr ='" + stfcr.Trim() +
                           "' and tfci = '" + stfci.Trim() +
                           "' and rxeqp = '" + srxeqp.Trim() +
                           "' and cmd <> 'D'";

            DataTable oDT = null;
            try
            {
                oDT = oCn.retrieve(cSQL);
            }
            finally
            {
                oCn.dbdisconnect();
            }
            return oDT;
        }
        
        public static string CtxdUpdate(Ctxd testCtxd, string sFile)
        {
            string cSQL;
            string retval = "";
            
            cSQL = "UPDATE " + sFile + " SET " +
                    sFile + ".cmd='N'," +
                    sFile + ".recstat='U'," +
                    sFile + ".tfcr = s.tfcr," +
                    sFile + ".tfci = s.tfci," +
                    sFile + ".rxeqp = s.rxeqp," +
                    sFile + ".fsep = s.fsep," +
                    sFile + ".rq = s.rq," +
                    sFile + ".mdate=s.mdate," +
                    sFile + ".mtime=s.mtime " +
                    "FROM " + sFile + " t INNER JOIN main.sd_ctxd s " +
                    "ON t.tfcr = s.tfcr AND t.tfci = s.tfci AND t.rxeqp = s.rxeqp AND t.fsep = s.fsep " + 
                    "WHERE s.tfcr='" + testCtxd.tfcr + "' " +
                    "AND s.tfci='" + testCtxd.tfci + "' " +
                    "AND s.rxeqp='" + testCtxd.rxeqp + "' " +
                    "AND s.fsep=" + testCtxd.v_fsep;

            dbconnect oCn = new dbconnect();
            OdbcCommand update1 = new OdbcCommand(cSQL, oCn.Connection);
            try
            {

                try
                {
                    update1.ExecuteNonQuery();
                    retval = "OK";
                }
                catch (Exception e5)
                {
                    //cn.Close();
                    retval = "ERROR:" + cSQL + ":" + e5.Message;
                }
            }
            finally
            {
                oCn.dbdisconnect();
            }

            return retval;
        }   
    }

    public class Equipment
    {
        protected eSubType m_type;

        public string cmd;
        public string recstat;
        public string ecode;
        public double? v_estab;
        public double estab;
        public string exref;
        public string emanu;
        public string emodel;
        public string edesc;
        public string etype;
        public string etraf;
        public string emission; 
        public double? v_e1stif;
        public double e1stif;
        public double? v_e2ndif;
        public double e2ndif;
        public double? v_thhold;
        public double thhold;
        public string ebndcde;
        public string mdate;
        public string mtime;

        public Equipment()
        {
            cmd = "";
            recstat = "";
            ecode = "";
            estab = 0.0;
            exref = "";
            emanu = "";
            emodel = "";
            edesc = "";
            etype = "";
            etraf = "";
            emission = "";
            e1stif = 0.0;
            e2ndif = 0.0;
            thhold = 0.0;
            ebndcde = "";
            mdate = "";
            mtime = "";
        }


        public Equipment(string EqptCode)
        {
            dbconnect oCn = new dbconnect();
            if (oCn.IsConnected())
            {
                string cSQL = "select ecode,estab,exref,emanu,emodel,edesc,etype,etraf,emission,e1stif,e2ndif," +
                              "thhold,ebndcde,mdate,mtime " +
                              "from main.sd_eqpt " +
                              "where ecode='" + EqptCode.Trim() + "'";

                DataTable oDT = null;
                try
                {
                    oDT = oCn.retrieve(cSQL);
                    if (oDT.Rows.Count > 0)
                    {
                        DataRow oDR = oDT.Rows[0];

                        ecode = Convert.ToString(oDR["ecode"]);
                        v_estab = FieldIO.getNullableDouble(oDR, "estab");
                        exref = Convert.ToString(oDR["exref"]);
                        emanu = Convert.ToString(oDR["emanu"]);
                        emodel = Convert.ToString(oDR["emodel"]);
                        edesc = Convert.ToString(oDR["edesc"]);
                        etype = Convert.ToString(oDR["etype"]);
                        etraf = Convert.ToString(oDR["etraf"]);
                        emission = Convert.ToString(oDR["emission"]);
                        v_e1stif = FieldIO.getNullableDouble(oDR, "e1stif");
                        v_e2ndif = FieldIO.getNullableDouble(oDR, "e2ndif");
                        v_thhold = FieldIO.getNullableDouble(oDR, "thhold");
                        ebndcde = Convert.ToString(oDR["ebndcde"]);
                        mdate = Convert.ToString(oDR["mdate"]);
                        mtime = Convert.ToString(oDR["mtime"]);

                        if (!v_estab.HasValue) { estab = 0.0; } else { estab = v_estab.Value; }
                        if (!v_e1stif.HasValue) { e1stif = 0.0; } else { e1stif = v_e1stif.Value; }
                        if (!v_e2ndif.HasValue) { e2ndif = 0.0; } else { e2ndif = v_e2ndif.Value; }
                        if (!v_thhold.HasValue) { thhold = 0.0; } else { thhold = v_thhold.Value; }
                        
                    }
                    else
                    {
                        ecode = "";
                        //throw new Exception("NOT FOUND: " + EqptCode);
                    }
                }
                catch (Exception Ex)
                {
                    ecode = "";
                    throw new Exception("IO ERROR: " + EqptCode + ":-\n" + Ex.Message + "\n" + cSQL);
                }
                finally
                {
                    oCn.dbdisconnect();
                }
            }
            else
            {
                ecode = "";
                throw new Exception("DB ERROR: Could not connect to database.");
            }
        }
        public Equipment(string sFile, string secode)
        {
            string cSQL;
            if (sFile == null || sFile == "" || sFile == "main.sd_note")
            {
                m_type = eSubType.sd;
                cSQL = "select ecode,estab,exref,emanu,emodel,edesc,etype,etraf,emission,e1stif,e2ndif," +
                       "thhold,ebndcde,mdate,mtime " +
                       "from main.sd_eqpt " +
                       "where ecode='" + secode.Trim() + "'";
            }
            else
            {
                m_type = eSubType.su;
                cSQL = "select cmd,recstat,ecode,estab,exref,emanu,emodel,edesc,etype,etraf,emission,e1stif,e2ndif," +
                       "thhold,ebndcde,mdate,mtime " +
                       "from " + sFile +
                       " where ecode='" + secode.Trim() + "'";
            }

            dbconnect oCn = new dbconnect();
            if (oCn.IsConnected())
            {

                DataTable oDT = null;
                try
                {
                    oDT = oCn.retrieve(cSQL);
                    if (oDT.Rows.Count > 0)
                    {
                        DataRow oDR = oDT.Rows[0];
                        if (m_type == eSubType.sd)
                        {
                            cmd = "";
                            recstat = "";
                        }
                        else
                        {
                            cmd = oDR["cmd"].ToString();
                            recstat = oDR["recstat"].ToString();
                        }
                        ecode = oDR["ecode"].ToString();
                        v_estab = FieldIO.getNullableDouble(oDR, "estab");
                        exref = oDR["exref"].ToString();
                        emanu = oDR["emanu"].ToString();
                        edesc = oDR["edesc"].ToString();
                        etype = oDR["etype"].ToString();
                        etraf = oDR["etraf"].ToString();
                        emodel = oDR["emodel"].ToString();
                        emission = oDR["emission"].ToString();
                        v_e1stif = FieldIO.getNullableDouble(oDR, "e1stif");
                        v_e2ndif = FieldIO.getNullableDouble(oDR, "e2ndif");
                        v_thhold = FieldIO.getNullableDouble(oDR, "thhold");
                        ebndcde = oDR["ebndcde"].ToString();
                        mdate = oDR["mdate"].ToString();
                        mtime = oDR["mtime"].ToString();

                        if (!v_estab.HasValue) { estab = 0.0; } else { estab = v_estab.Value; }
                        if (!v_e1stif.HasValue) { e1stif = 0.0; } else { e1stif = v_e1stif.Value; }
                        if (!v_e2ndif.HasValue) { e2ndif = 0.0; } else { e2ndif = v_e2ndif.Value; }
                        if (!v_thhold.HasValue) { thhold = 0.0; } else { thhold = v_thhold.Value; }

                    }
                    else
                    {
                        ecode = "";
                    }
                }
                catch (Exception Ex)
                {
                    ecode = "";
                    throw new Exception("IO ERROR: " + secode + ":-\n" + Ex.Message + "\n" + cSQL);
                }
                finally
                {
                    oCn.dbdisconnect();
                }
            }
            else
            {
                ecode = "";
                throw new Exception("DB ERROR: Could not connect to database.");
            }
        }


        public static DataTable EqptKeys(string eqptTable)
        {
            dbconnect oCn = new dbconnect();
            string cSQL = "Select ecode " +
                           " from " + eqptTable +
                           " order by ecode";

            DataTable oDT = null;
            try
            {
                oDT = oCn.retrieve(cSQL);
            }
            finally
            {
                oCn.dbdisconnect();
            }
            return oDT;
        }
        
        public static string EqptUpdate(Equipment testEqpt, string sFile)
        {
            string cSQL;
            string retval = "";

            cSQL = "UPDATE " + sFile + " SET " +
                    sFile + ".cmd='N'," +
                    sFile + ".recstat='U'," +
                    sFile + ".ecode = s.ecode," +
                    sFile + ".estab = s.estab," +
                    sFile + ".exref = s.exref," +
                    sFile + ".emanu = s.emanu," +
                    sFile + ".emodel = s.emodel," +
                    sFile + ".edesc = s.edesc," +
                    sFile + ".etype = s.etype," +
                    sFile + ".etraf = s.etraf," +
                    sFile + ".emission = s.emission," +
                    sFile + ".e1stif = s.e1stif," +
                    sFile + ".e2ndif = s.e2ndif," +
                    sFile + ".thhold = s.thhold," +
                    sFile + ".ebndcde = s.ebndcde," +
                    sFile + ".mdate=s.mdate," +
                    sFile + ".mtime=s.mtime " +
                    "FROM " + sFile + " t INNER JOIN main.sd_eqpt s " +
                    "ON t.ecode = s.ecode " + 
                    "WHERE s.ecode='" + testEqpt.ecode + "'";

            dbconnect oCn = new dbconnect();
            OdbcCommand update1 = new OdbcCommand(cSQL, oCn.Connection);
            try
            {

                try
                {
                    update1.ExecuteNonQuery();
                    retval = "OK";
                }
                catch (Exception e5)
                {
                    //cn.Close();
                    retval = "ERROR:" + cSQL + ":" + e5.Message;
                }
            }
            finally
            {
                oCn.dbdisconnect();
            }

            return retval;
        }   
    

        public static bool UnionEqptChk(string secode, string eqptTable)
        {
            bool found = false;

            dbconnect oCn = new dbconnect();
            string cSQL = "Select ecode from main.sd_eqpt " +
                           " where ecode='" + secode.Trim() + "'" +
                           " UNION " +
                           "Select ecode from " + eqptTable +
                           " where ecode='" + secode.Trim() + "'";
            try
            {
                DataTable oDT = oCn.retrieve(cSQL);
                if (oDT.Rows.Count > 0)
                {
                    found = true;  // bandcode found
                }
            }
            catch (Exception)
            {
            }

            oCn.dbdisconnect();
            return found;
        }

    }

    public class Note
    {
        protected eSubType m_type;

        public string cmd;
        public string recstat;
        public string oper;
        public string nonum;
        public string note;
        public string mdate;
        public string mtime;

        public Note()
        {
            oper = "";			//	indicate uninitialized.
        }

        public Note(string sFile, string soper, string snonum)
        {
            string cSQL;
            if (sFile == null || sFile == "" || sFile == "main.sd_note")
            {
                m_type = eSubType.sd;
                cSQL = "Select oper, nonum, note, mdate, mtime " +
                       "from main.sd_note " +
                       "where oper='" + soper.Trim() +
                       "' and nonum='" + snonum.Trim() + "'";
            }
            else
            {
                m_type = eSubType.su;
                cSQL = "Select cmd, recstat, oper, nonum, note, mdate, mtime " +
                       "from " + sFile +
                       " where oper='" + soper.Trim() +
                       "' and nonum='" + snonum.Trim() + "'";
            }

            dbconnect oCn = new dbconnect();

            try
            {
                DataTable oDT = oCn.retrieve(cSQL);
                if (oDT.Rows.Count > 0)
                {
                    //	Take the first row and use that as the note.
                    DataRow oDR = oDT.Rows[0];
                    if (m_type == eSubType.sd)
                    {
                        cmd = "";
                        recstat = "";
                    }
                    else
                    {
                        cmd = Convert.ToString(oDR["cmd"]);
                        recstat = Convert.ToString(oDR["recstat"]);
                    }
                    oper = Convert.ToString(oDR["oper"]);
                    nonum = Convert.ToString(oDR["nonum"]);
                    note = Convert.ToString(oDR["note"]);
                    mdate = Convert.ToString(oDR["mdate"]);
                    mtime = Convert.ToString(oDR["mtime"]);
                }
                else
                {
                    //	Indicate that the note was not found.
                    oper = "";
                }
            }
            catch
            {
            }

            oCn.dbdisconnect();
        }
        public static DataTable NoteKeys(string noteTable)
        {
            dbconnect oCn = new dbconnect();
            string cSQL = "Select oper, nonum " +
                           " from " + noteTable +
                           " order by oper, nonum";

            DataTable oDT = null;
            try
            {
                oDT = oCn.retrieve(cSQL);
            }
            finally
            {
                oCn.dbdisconnect();
            }
            return oDT;
        }
               
        public static string NoteUpdate(Note testNote, string sFile)
        {
            string cSQL;
            string retval = "";

            cSQL = "UPDATE " + sFile + " SET " +
                    sFile + ".cmd='N'," +
                    sFile + ".recstat='U'," +
                    sFile + ".oper = s.oper," +
                    sFile + ".nonum = s.nonum," +
                    sFile + ".note = s.note," +
                    sFile + ".mdate=s.mdate," +
                    sFile + ".mtime=s.mtime " +
                    "FROM " + sFile + " t INNER JOIN main.sd_note s " +
                    "ON t.oper = s.oper AND t.nonum = s.nonum " + 
                    "WHERE s.oper='" + testNote.oper + "' AND s.nonum = '" + testNote.nonum + "'";

            dbconnect oCn = new dbconnect();
            OdbcCommand update1 = new OdbcCommand(cSQL, oCn.Connection);
            try
            {

                try
                {
                    update1.ExecuteNonQuery();
                    retval = "OK";
                }
                catch (Exception e5)
                {
                    //cn.Close();
                    retval = "ERROR:" + cSQL + ":" + e5.Message;
                }
            }
            finally
            {
                oCn.dbdisconnect();
            }

            return retval;
        }   
   
        public static bool UnionNoteChk(string soper, string snonum, string noteTable)
        {
            bool found = false;

            dbconnect oCn = new dbconnect();
            string cSQL = "Select oper, nonum from main.sd_note " +
                           " where oper='" + soper.Trim() + "'" +
                           " and nonum='" + snonum.Trim() + "'" +
                           " UNION " +
                           "Select oper, nonum from " + noteTable +
                           " where oper='" + soper.Trim() + "'" +
                           " and nonum ='" + snonum.Trim() + "'";
            try
            {
                DataTable oDT = oCn.retrieve(cSQL);
                if (oDT.Rows.Count > 0)
                {
                    found = true;  // bandcode found
                }
            }
            catch (Exception)
            {
            }

            oCn.dbdisconnect();
            return found;
        }
    }

    public class Operator
    {
        protected eSubType m_type;

        public string cmd;
        public string recstat;
        public string oper;
        public string nameop;
        public string cooper;
        public string mdbm;
        public string addr;
        public string city;
        public string prstat;
        public string zippc;
        public string dept;
        public string namep;
        public string phonep;
        public string faxnum;
        public string telecom;
        public string opnote;
        public string admin;
        public string mdate;
        public string mtime;

        public Operator(string sFile, string soper)
        {
            string cSQL;
            if (sFile == null || sFile == "" || sFile == "main.sd_oper")
            {
                m_type = eSubType.sd;
                cSQL = "select oper, nameop,cooper,mdbm,addr,city,prstat,zippc,dept,namep," +
                       "phonep,faxnum,telecom,opnote,admin,mdate,mtime " +
                       "from main.sd_oper " +
                       "where oper='" + soper.Trim() + "'";
            }
            else
            {
                m_type = eSubType.su;
                cSQL = "select cmd,recstat,oper,nameop,cooper,mdbm,addr,city,prstat,zippc,dept,namep," +
                       "phonep,faxnum,telecom,opnote,admin,mdate,mtime " +
                       "from " + sFile +
                       " where oper='" + soper.Trim() + "'";
            }
            dbconnect oCn = new dbconnect();

            DataTable oDT = null;
            try
            {
                oDT = oCn.retrieve(cSQL);
                if (oDT.Rows.Count > 0)
                {
                    DataRow oDR = oDT.Rows[0];
                    if (m_type == eSubType.sd)
                    {
                        cmd = "";
                        recstat = "";
                    }
                    else
                    {
                        cmd = Convert.ToString(oDR["cmd"]);
                        recstat = Convert.ToString(oDR["recstat"]);
                    }
                    oper = Convert.ToString(oDR["oper"]).Trim();
                    nameop = Convert.ToString(oDR["nameop"]).Trim();
                    cooper = Convert.ToString(oDR["cooper"]).Trim();
                    mdbm = Convert.ToString(oDR["mdbm"]).Trim();
                    addr = Convert.ToString(oDR["addr"]).Trim();
                    city = Convert.ToString(oDR["city"]).Trim();
                    prstat = Convert.ToString(oDR["prstat"]).Trim();
                    zippc = Convert.ToString(oDR["zippc"]).Trim();
                    dept = Convert.ToString(oDR["dept"]).Trim();
                    namep = Convert.ToString(oDR["namep"]).Trim();
                    phonep = Convert.ToString(oDR["phonep"]).Trim();
                    faxnum = Convert.ToString(oDR["faxnum"]).Trim();
                    telecom = Convert.ToString(oDR["telecom"]).Trim();
                    opnote = Convert.ToString(oDR["opnote"]).Trim();
                    admin = Convert.ToString(oDR["admin"]).Trim();
                    mdate = Convert.ToString(oDR["mdate"]);
                    mtime = Convert.ToString(oDR["mtime"]);
                }
                else
                {
                    oper = "";
                }
            }
            catch (Exception Ex)
            {
                throw new Exception("IO ERROR: " + soper + ":-\n" + Ex.Message);
            }
            finally
            {
                oCn.dbdisconnect();
            }
        }
        public static DataTable OperKeys(string operTable)
        {
            dbconnect oCn = new dbconnect();
            string cSQL = "Select oper " +
                           " from " + operTable +
                           " order by oper";

            DataTable oDT = null;
            try
            {
                oDT = oCn.retrieve(cSQL);
            }
            finally
            {
                oCn.dbdisconnect();
            }
            return oDT;
        }
        public static string OperUpdate(Operator testOper, string sFile)
        {
            string cSQL;
            string retval = "";

            cSQL = "UPDATE " + sFile + " SET " +
                    sFile + ".cmd='N'," +
                    sFile + ".recstat='U'," +
                    sFile + ".oper = s.oper," +
                    sFile + ".nameop = s.nameop," +
                    sFile + ".cooper = s.cooper," +
                    sFile + ".mdbm = s.mdbm," +
                    sFile + ".addr = s.addr," +
                    sFile + ".city = s.city," +
                    sFile + ".prstat = s.prstat," +
                    sFile + ".zippc = s.zippc," +
                    sFile + ".dept = s.dept," +
                    sFile + ".email = s.email," +
                    sFile + ".namep = s.namep," +
                    sFile + ".phonep = s.phonep," +
                    sFile + ".faxnum = s.faxnum," +
                    sFile + ".telecom = s.telecom," +
                    sFile + ".opnote = s.opnote," +
                    sFile + ".admin = s.admin," +
                    sFile + ".mdate=s.mdate," +
                    sFile + ".mtime=s.mtime " +
                    "FROM " + sFile + " t INNER JOIN main.sd_oper s " +
                    "ON t.oper = s.oper " +
                    "WHERE s.oper='" + testOper.oper + "'";

            dbconnect oCn = new dbconnect();
            OdbcCommand update1 = new OdbcCommand(cSQL, oCn.Connection);
            try
            {

                try
                {
                    update1.ExecuteNonQuery();
                    retval = "OK";
                }
                catch (Exception e5)
                {
                    //cn.Close();
                    retval = "ERROR:" + cSQL + ":" + e5.Message;
                }
            }
            finally
            {
                oCn.dbdisconnect();
            }

            return retval;
        }   
   
        public static bool UnionOperChk(string soper, string operTable)
        {
            bool found = false;

            dbconnect oCn = new dbconnect();
            string cSQL = "Select oper from main.sd_oper " +
                           " where oper='" + soper.Trim() + "'" +
                           " UNION " +
                           "Select oper from " + operTable +
                           " where oper='" + soper.Trim() + "'";
            try
            {
                DataTable oDT = oCn.retrieve(cSQL);
                if (oDT.Rows.Count > 0)
                {
                    found = true;  // oper found
                }
            }
            catch (Exception)
            {
            }

            oCn.dbdisconnect();
            return found;
        }
        public static bool OperExists(string soper)
        {
            bool found = false;

            dbconnect oCn = new dbconnect();
            string cSQL = "Select code from main.sd_oper " +
                           " where opere='" + soper.Trim() + "'";
            try
            {
                DataTable oDT = oCn.retrieve(cSQL);
                if (oDT.Rows.Count > 0)
                {
                    found = true;  // bandcode found
                }
            }
            catch (Exception)
            {
            }

            oCn.dbdisconnect();
            return found;
        }
        public static bool ProvExists(string sprov)
        {
            bool found = false;

            dbconnect oCn = new dbconnect();
            string cSQL = "Select code from techdef.provinces " +
                           " where code='" + sprov.Trim() + "'";
            try
            {
                DataTable oDT = oCn.retrieve(cSQL);
                if (oDT.Rows.Count > 0)
                {
                    found = true;  // bandcode found
                }
            }
            catch (Exception)
            {
            }

            oCn.dbdisconnect();
            return found;
        }
        public static bool OpnoteExists(string sopnote)
        {
            bool found = false;

            dbconnect oCn = new dbconnect();
            string cSQL = "Select code from techdef.oper_class " +
                           " where code='" + sopnote.Trim() + "'";
            try
            {
                DataTable oDT = oCn.retrieve(cSQL);
                if (oDT.Rows.Count > 0)
                {
                    found = true;  // bandcode found
                }
            }
            catch (Exception)
            {
            }

            oCn.dbdisconnect();
            return found;
        }
    }

    public class Plan
    {
        protected eSubType m_type;

        public string cmd;
        public string recstat;
        public string sband;
        public string splan;
        public string srsp;
        public string srspiss;
        public string conform;
        public string uscan;
        public string mdate;
        public string mtime;

        public Plan()
        {
            sband = "";			//	indicate uninitialized.
        }

        public Plan(string sFile, string ssband, string ssplan)
        {
            string cSQL;
            if (sFile == null || sFile == "" || sFile == "main.sd_plan")
            {
                m_type = eSubType.sd;
                cSQL = "Select sband, splan, srsp, srspiss, conform, uscan, mdate, mtime " +
                       "from main.sd_plan " +
                       "where sband='" + ssband.Trim() +
                       "' and splan='" + ssplan.Trim() + "'";
            }
            else
            {
                m_type = eSubType.su;
                cSQL = "Select cmd, recstat, sband, splan, srsp, srspiss, conform, uscan, mdate, mtime " +
                       "from  " + sFile +
                       " where sband='" + ssband.Trim() +
                       "' and splan='" + ssplan.Trim() + "'";
            }
            dbconnect oCn = new dbconnect();

            try
            {
                DataTable oDT = oCn.retrieve(cSQL);
                if (oDT.Rows.Count > 0)
                {
                    //	Take the first row and use that as the plan.
                    DataRow oDR = oDT.Rows[0];
                    if (m_type == eSubType.sd)
                    {
                        cmd = "";
                        recstat = "";
                    }
                    else
                    {
                        cmd = Convert.ToString(oDR["cmd"]);
                        recstat = Convert.ToString(oDR["recstat"]);
                    }
                    sband = Convert.ToString(oDR["sband"]);
                    splan = Convert.ToString(oDR["splan"]);
                    srsp = Convert.ToString(oDR["srsp"]);
                    srspiss = Convert.ToString(oDR["srspiss"]);
                    conform = Convert.ToString(oDR["conform"]);
                    uscan = Convert.ToString(oDR["uscan"]);
                    mdate = Convert.ToString(oDR["mdate"]);
                    mtime = Convert.ToString(oDR["mtime"]);
                }
                else
                {
                    //	Indicate that the plan was not found.
                    sband = "";
                }
            }
            catch
            {
                sband = "";
            }

            oCn.dbdisconnect();
        }
        public static DataTable PlanKeys(string planTable)
        {
            dbconnect oCn = new dbconnect();
            string cSQL = "Select sband, splan " +
                           " from " + planTable +
                           " order by sband, splan";

            DataTable oDT = null;
            try
            {
                oDT = oCn.retrieve(cSQL);
            }
            finally
            {
                oCn.dbdisconnect();
            }
            return oDT;
        }
        public static string PlanUpdate(Plan testPlan, string sFile)
        {
            string cSQL;
            string retval = "";

            cSQL = "UPDATE " + sFile + " SET " +
                    sFile + ".cmd='N'," +
                    sFile + ".recstat='U'," +
                    sFile + ".sband = s.sband," +
                    sFile + ".splan = s.splan," +
                    sFile + ".srsp = s.srsp," +
                    sFile + ".srspiss = s.srspiss," +
                    sFile + ".conform = s.conform," +
                    sFile + ".uscan = s.uscan," +
                    sFile + ".mdate=s.mdate," +
                    sFile + ".mtime=s.mtime " +
                    "FROM " + sFile + " t INNER JOIN main.sd_plan s " +
                    "ON t.sband = s.sband AND t.splan = s.splan " +
                    "WHERE s.sband='" + testPlan.sband + "' AND s.splan = '" + testPlan.splan + "'";

            dbconnect oCn = new dbconnect();
            OdbcCommand update1 = new OdbcCommand(cSQL, oCn.Connection);
            try
            {

                try
                {
                    update1.ExecuteNonQuery();
                    retval = "OK";
                }
                catch (Exception e5)
                {
                    //cn.Close();
                    retval = "ERROR:" + cSQL + ":" + e5.Message;
                }
            }
            finally
            {
                oCn.dbdisconnect();
            }

            return retval;
        }   
   
        public static bool UnionPlanChk(string ssband, string ssplan, string planTable)
        {
            bool found = false;

            dbconnect oCn = new dbconnect();
            string cSQL = "Select sband, splan from main.sd_plan " +
                           " where sband='" + ssband.Trim() + "'" +
                           " and splan='" + ssplan.Trim() + "'" +
                           " UNION " +
                           "Select sband, splan from " + planTable +
                           " where sband='" + ssband.Trim() + "'" +
                           " and splan ='" + ssplan.Trim() + "'";
            try
            {
                DataTable oDT = oCn.retrieve(cSQL);
                if (oDT.Rows.Count > 0)
                {
                    found = true;  // bandcode found
                }
            }
            catch (Exception)
            {
            }

            oCn.dbdisconnect();
            return found;
        }
    }
    public class Plnd
    {
        protected eSubType m_type;

        public string cmd;
        public string recstat;
        public string sband;
        public string splan;
        public short? v_spno;
        public short spno;
        public double? v_set1;
        public double set1;
        public string s1chid;
        public double? v_set2;
        public double set2;
        public string s2chid;
        public double? v_set3;
        public double set3;
        public string s3chid;
        public double? v_set4;
        public double set4;
        public string s4chid;
        public string mdate;
        public string mtime;

        public Plnd()
        {
            sband = "";			//	indicate uninitialized.
        }

        public Plnd(string sFile, string ssband, string ssplan, short sspno)
        {
            string cSQL;
            if (sFile == null || sFile == "" || sFile == "main.sd_plnd")
            {
                m_type = eSubType.sd;
                cSQL = "Select sband, splan, spno, set1, s1chid, set2, s2chid, set3, s3chid, set4, s4chid, mdate, mtime " +
                       "from main.sd_plnd " +
                       "where sband='" + ssband.Trim() +
                       "' and splan='" + ssplan.Trim() +
                       "' and spno=" + sspno.ToString();
            }
            else
            {
                m_type = eSubType.su;
                cSQL = "Select cmd, recstat, sband, splan, spno, set1, s1chid, set2, s2chid, set3, s3chid, set4, s4chid, mdate, mtime " +
                       "from " + sFile +
                       " where sband='" + ssband.Trim() +
                       "' and splan='" + ssplan.Trim() +
                       "' and spno=" + sspno.ToString();
            }
            dbconnect oCn = new dbconnect();

            try
            {
                DataTable oDT = oCn.retrieve(cSQL);
                if (oDT.Rows.Count > 0)
                {
                    //	Take the first row and use that as the plnd.
                    DataRow oDR = oDT.Rows[0];
                    if (m_type == eSubType.sd)
                    {
                        cmd = "";
                        recstat = "";
                    }
                    else
                    {
                        cmd = Convert.ToString(oDR["cmd"]);
                        recstat = Convert.ToString(oDR["recstat"]);
                    }
                    sband = Convert.ToString(oDR["sband"]);
                    splan = Convert.ToString(oDR["splan"]);
                    v_spno = FieldIO.getNullableInt16(oDR, "spno");
                    v_set1 = FieldIO.getNullableDouble(oDR, "set1");
                    //catch { set1 = 0.00; }
                    s1chid = Convert.ToString(oDR["s1chid"]);
                    v_set2 = FieldIO.getNullableDouble(oDR, "set2");
                    //catch { set2 = 0.00; }
                    s2chid = Convert.ToString(oDR["s2chid"]);
                    v_set3 = FieldIO.getNullableDouble(oDR, "set3");
                    //catch { set3 = 0.00; }
                    s3chid = Convert.ToString(oDR["s3chid"]);
                    v_set4 = FieldIO.getNullableDouble(oDR, "set4");
                    //catch { set4 = 0.00; }
                    s4chid = Convert.ToString(oDR["s4chid"]);
                    mdate = Convert.ToString(oDR["mdate"]);
                    mtime = Convert.ToString(oDR["mtime"]);
                }
                else
                {
                    //	Indicate that the ctx was not found.
                    sband = "";
                }
            }
            catch
            {
            }

            oCn.dbdisconnect();
        }
        public static DataTable PlndKeys(string plndTable, string ssband, string ssplan)
        {
            dbconnect oCn = new dbconnect();
            string cSQL = "Select sband, splan, spno " +
                           " from " + plndTable +
                           " where sband ='" + ssband.Trim() +
                           "' and splan = '" + ssplan.Trim() +
                           "' order by sband, splan, spno";

            DataTable oDT = null;
            try
            {
                oDT = oCn.retrieve(cSQL);
            }
            finally
            {
                oCn.dbdisconnect();
            }
            return oDT;
        }
       
        public static string PlndUpdate(Plnd testPlnd, string sFile)
        {
            string cSQL;
            string retval = "";

            cSQL = "UPDATE " + sFile + " SET " +
                    sFile + ".cmd='N'," +
                    sFile + ".recstat='U'," +
                    sFile + ".sband = s.sband," +
                    sFile + ".splan = s.splan," +
                    sFile + ".spno = s.spno," +
                    sFile + ".set1 = s.set1," +
                    sFile + ".s1chid = s.s1chid," +
                    sFile + ".set2 = s.set2," +
                    sFile + ".s2chid = s.s2chid," +
                    sFile + ".set3 = s.set3," +
                    sFile + ".s3chid = s.s3chid," +
                    sFile + ".set4 = s.set4," +
                    sFile + ".s4chid = s.s4chid," +
                    sFile + ".mdate=s.mdate," +
                    sFile + ".mtime=s.mtime " +
                    "FROM " + sFile + " t INNER JOIN main.sd_plnd s " +
                    "ON t.sband = s.sband AND t.splan = s.splan AND t.spno = s.spno " +
                    "WHERE s.sband='" + testPlnd.sband + "' AND s.splan = '" + testPlnd.splan + "' AND s.spno = " + testPlnd.v_spno.Value;

            dbconnect oCn = new dbconnect();
            OdbcCommand update1 = new OdbcCommand(cSQL, oCn.Connection);
            try
            {

                try
                {
                    update1.ExecuteNonQuery();
                    retval = "OK";
                }
                catch (Exception e5)
                {
                    //cn.Close();
                    retval = "ERROR:" + cSQL + ":" + e5.Message;
                }
            }
            finally
            {
                oCn.dbdisconnect();
            }

            return retval;
        }   
   
        public static bool UnionPlndChk(string ssband, string ssplan, Int16 sspno, string planTable)
        {
            bool found = false;

            dbconnect oCn = new dbconnect();
            string cSQL = "Select sband, splan, spno from main.sd_plan " +
                           " where sband='" + ssband.Trim() + "'" +
                           " and splan='" + ssplan.Trim() + "'" +
                           " and spno =" + sspno + 
                           " UNION " +
                           "Select sband, splan from " + planTable +
                           " where sband='" + ssband.Trim() + "'" +
                           " and splan ='" + ssplan.Trim() + "'" +
                           " and spno =" + sspno;
            try
            {
                DataTable oDT = oCn.retrieve(cSQL);
                if (oDT.Rows.Count > 0)
                {
                    found = true;  // bandcode found
                }
            }
            catch (Exception)
            {
            }

            oCn.dbdisconnect();
            return found;
        }
    }

    public class Route
    {
        protected eSubType m_type;

        public string cmd;
        public string recstat;
        public string rcomp;
        public string routnumb;
        public string rtprov;
        public string rtcall;
        public string rtname;
        public string mdate;
        public string mtime;

        public Route()
        {
            rcomp = "";			//	indicate uninitialized.
        }

        public Route(string sFile, string srcomp, string iroutnumb)
        {
            string cSQL;
            if (sFile == null || sFile == "" || sFile == "main.sd_rout")
            {
                m_type = eSubType.sd;
                cSQL = "Select rcomp, routnumb, rtprov, rtcall, rtname, mdate, mtime " +
                       "from main.sd_rout " +
                       "where rcomp='" + srcomp.Trim() +
                       "' and routnumb='" + iroutnumb + "'";
            }
            else
            {
                m_type = eSubType.su;
                cSQL = "Select cmd, recstat, rcomp, routnumb, rtprov, rtcall, rtname, mdate, mtime " +
                       "from " + sFile +
                       " where rcomp='" + srcomp.Trim() +
                       "' and routnumb='" + iroutnumb + "'";
            }
            dbconnect oCn = new dbconnect();

            try
            {
                DataTable oDT = oCn.retrieve(cSQL);
                if (oDT.Rows.Count > 0)
                {
                    //	Take the first row and use that as the route.
                    DataRow oDR = oDT.Rows[0];
                    if (m_type == eSubType.sd)
                    {
                        cmd = "";
                        recstat = "";
                    }
                    else
                    {
                        cmd = Convert.ToString(oDR["cmd"]);
                        recstat = Convert.ToString(oDR["recstat"]);
                    }
                    rcomp = Convert.ToString(oDR["rcomp"]);
                    routnumb = Convert.ToString(oDR["routnumb"]);
                    rtprov = Convert.ToString(oDR["rtprov"]);
                    rtcall = Convert.ToString(oDR["rtcall"]);
                    rtname = Convert.ToString(oDR["rtname"]);
                    mdate = Convert.ToString(oDR["mdate"]);
                    mtime = Convert.ToString(oDR["mtime"]);
                }
                else
                {
                    //	Indicate that the route was not found.
                    rcomp = "";
                }
            }
            catch
            {
            }

            oCn.dbdisconnect();
        }
        public static DataTable RoutKeys(string routTable)
        {
            //string logfile = "D:\\extractlogs\\RoutKeys.txt";  //  log file
            // open logfile
            //StreamWriter swsapr = new StreamWriter(logfile, false);

            //DateTime curTime = DateTime.Now;
            //string log_time = curTime.ToString("yyyy/MM/dd HH:mm:ss");

            // write info to log file
            //swsapr.WriteLine(log_time);
            //swsapr.Flush();

            dbconnect oCn = new dbconnect();
            string cSQL = "Select rcomp, routnumb " +
                           " from " + routTable +
                           " order by rcomp, routnumb";
            //swsapr.WriteLine(cSQL);
            //swsapr.Flush();

            DataTable oDT = null;
            try
            {
                oDT = oCn.retrieve(cSQL);
                //swsapr.WriteLine("DataTable loaded");
                //swsapr.Close();
            }
            catch (Exception Ex)
            {
                //swsapr.WriteLine("DataTable load faileded: " + Ex.Message);
                //swsapr.Close();
                throw new Exception("IO ERROR: " + Ex.Message + "\n" + cSQL);
            }
            finally
            {
                oCn.dbdisconnect();
            }
            return oDT;
        }
        public static string RoutUpdate(Route testRout, string sFile)
        {
            string cSQL;
            string retval = "";

            cSQL = "UPDATE " + sFile + " SET " +
                    sFile + ".cmd='N'," +
                    sFile + ".recstat='U'," +
                    sFile + ".rcomp = s.rcomp," +
                    sFile + ".routnumb = s.routnumb," +
                    sFile + ".rtprov = s.rtprov," +
                    sFile + ".rtcall = s.rtcall," +
                    sFile + ".rtname = s.rtname," +
                    sFile + ".mdate=s.mdate," +
                    sFile + ".mtime=s.mtime " +
                    "FROM " + sFile + " t INNER JOIN main.sd_rout s " +
                    "ON t.rcomp = s.rcomp AND t.routnumb = s.routnumb " +
                    "WHERE s.rcomp='" + testRout.rcomp + "' AND s.routnumb= '" + testRout.routnumb + "'";

            dbconnect oCn = new dbconnect();
            OdbcCommand update1 = new OdbcCommand(cSQL, oCn.Connection);
            try
            {

                try
                {
                    update1.ExecuteNonQuery();
                    retval = "OK";
                }
                catch (Exception e5)
                {
                    //cn.Close();
                    retval = "ERROR:" + cSQL + ":" + e5.Message;
                }
            }
            finally
            {
                oCn.dbdisconnect();
            }

            return retval;
        }   
   
        public static bool UnionRoutChk(string srcomp, int iroutnumb, string routTable)
        {
            bool found = false;

            dbconnect oCn = new dbconnect();
            string cSQL = "Select rcomp, routnumb from main.sd_rout " +
                           " where rcomp='" + srcomp.Trim() + "'" +
                           " and sroutnumb='" + iroutnumb + "'" + 
                           " UNION " +
                           "Select rcomp, routnumb from " + routTable +
                           " where rcomp='" + srcomp.Trim() + "'" +
                           " and routnumb ='" + iroutnumb + "'";
            try
            {
                DataTable oDT = oCn.retrieve(cSQL);
                if (oDT.Rows.Count > 0)
                {
                    found = true;  // bandcode found
                }
            }
            catch (Exception)
            {
            }

            oCn.dbdisconnect();
            return found;
        }
    }

    public class TowerNote
    {
        public eSubType m_type;

        public string cmd;
        public string recstat;
        public string call1;
        public string oper;
        public string twcode;
        public double? v_twht;
        public double twht;
        public short atwrno;
        public string twli;
        public string twpa;
        public string nott;
        public string tpoint;
        public string adate;
        public string sdate;
        public string mdate;
        public string mtime;

         /*public string call1
         {
             get { return call1; }
             set { call1 = value; }
         }

         public string oper
         {
             get { return oper; }
             set { oper = value; }
         }

         public string twcode
         {
             get { return twcode; }
             set { twcode = value; }
         }

         public double twht
         {
             get { return twht; }
             set { twht = value; }
         }

         public short atwrno
         {
             get { return atwrno; }
             set { atwrno = value; }
         }

         public string twli
         {
             get { return twli; }
             set { twli = value; }
         }

         public string twpa
         {
             get { return twpa; }
             set { twpa = value; }
         }

         public string nott
         {
             get { return nott; }
             set { nott = value; }
         }

         public string tpoint
         {
             get { return tpoint; }
             set { tpoint = value; }
         }

         public string adate
         {
             get { return adate; }
             set { adate = value; }
         }

         public string sdate
         {
             get { return sdate; }
             set { sdate = value; }
         }

         public string mdate
         {
             get { return mdate; }
             set { mdate = value; }
         }

         public string mtime
         {
             get { return mtime; }
             set { mtime = value; }
         }

 */


        /// <summary>
        /// Constructor to create an empty TowerNote.
        /// </summary>
        public TowerNote()
        {
            m_type = eSubType.sd;
            call1 = "";
        }


        /// <summary>
        /// Constructor for a tower note, given the call sign and tower number
        ///	This routine will throw a "Not Found" exception if it is not there.
        /// </summary>
        /// <param name="cCall">The call sign of the site</param>
        /// <param name="nTowerNo">The tower number referred to</param>
        public TowerNote(string sFile, string scall1, int nTowerNo)
        {
            string cSQL;
            if (sFile == null || sFile == "" || sFile == "main.sd_town")
            {
                m_type = eSubType.sd;
                cSQL = "SELECT * " +
                       "FROM main.sd_town " +
                       "WHERE call1='" + scall1 + "' and atwrno=" + nTowerNo;
            }
            else
            {
                m_type = eSubType.su;
                cSQL = "SELECT * " +
                       "FROM " + sFile +
                       " WHERE call1='" + scall1 + "' and atwrno=" + nTowerNo;
            }
            dbconnect oCn = new dbconnect();

            DataTable oDT = oCn.retrieve(cSQL);
            if (oDT.Rows.Count > 0)
            {
                //	Only concern ourselves with the first row.
                DataRow oDR = oDT.Rows[0];
                if (m_type == eSubType.sd)
                {
                    cmd = "";
                    recstat = "";
                }
                else
                {
                    cmd = Convert.ToString(oDR["cmd"]);
                    recstat = Convert.ToString(oDR["recstat"]);
                }
                call1 = Convert.ToString(oDR["call1"]);
                oper = Convert.ToString(oDR["oper"]);
                twcode = Convert.ToString(oDR["twcode"]);
                v_twht = FieldIO.getNullableDouble(oDR, "twht");
                //catch { twht = 0.0f; }
                atwrno = Convert.ToInt16(oDR["atwrno"]);
                //catch { atwrno = 0; }
                twli = Convert.ToString(oDR["twli"]);
                twpa = Convert.ToString(oDR["twpa"]);
                nott = Convert.ToString(oDR["nott"]);
                tpoint = Convert.ToString(oDR["tpoint"]);
                adate = Convert.ToString(oDR["adate"]);
                sdate = Convert.ToString(oDR["sdate"]);
                mdate = Convert.ToString(oDR["mdate"]);
                mtime = Convert.ToString(oDR["mtime"]);

                if (!v_twht.HasValue) { twht = 0; } else { twht = v_twht.Value; }

            }
            else
            {
                call1 = "";
            }

            oCn.dbdisconnect();
        }

        public static DataTable TownKeys(string townTable)
        {
            dbconnect oCn = new dbconnect();
            string cSQL = "Select call1, atwrno " +
                           " from " + townTable +
                           " order by call1, atwrno";

            DataTable oDT = null;
            try
            {
                oDT = oCn.retrieve(cSQL);
            }
            finally
            {
                oCn.dbdisconnect();
            }
            return oDT;
        }
        public static string TownUpdate(TowerNote testTown, string sFile)
        {
            string cSQL;
            string retval = "";

            cSQL = "UPDATE " + sFile + " SET " +
                    sFile + ".cmd='N'," +
                    sFile + ".recstat='U'," +
                    sFile + ".call1 = s.call1," +
                    sFile + ".oper = s.oper," +
                    sFile + ".twcode = s.twcode," +
                    sFile + ".twht = s.twht," +
                    sFile + ".atwrno = s.atwrno," +
                    sFile + ".twli = s.twli," +
                    sFile + ".twpa = s.twpa," +
                    sFile + ".nott = s.nott," +
                    sFile + ".tpoint = s.tpoint," +
                    sFile + ".adate = s.adate," +
                    sFile + ".sdate = s.sdate," +
                    sFile + ".mdate=s.mdate," +
                    sFile + ".mtime=s.mtime " +
                    "FROM " + sFile + " t INNER JOIN main.sd_town s " +
                    "ON t.call1 = s.call1 AND t.atwrno = s.atwrno " +
                    "WHERE s.call1='" + testTown.call1 + "' AND s.atwrno = " + testTown.atwrno;

            dbconnect oCn = new dbconnect();
            OdbcCommand update1 = new OdbcCommand(cSQL, oCn.Connection);
            try
            {

                try
                {
                    update1.ExecuteNonQuery();
                    retval = "OK";
                }
                catch (Exception e5)
                {
                    //cn.Close();
                    retval = "ERROR:" + cSQL + ":" + e5.Message;
                }
            }
            finally
            {
                oCn.dbdisconnect();
            }

            return retval;
        }

        public static bool OperExists(string soper)
        {
            bool found = false;

            dbconnect oCn = new dbconnect();
            string cSQL = "Select oper from main.sd_oper " +
                           " where oper='" + soper.Trim() + "'";
            try
            {
                DataTable oDT = oCn.retrieve(cSQL);
                if (oDT.Rows.Count > 0)
                {
                    found = true;  // operator found
                }
            }
            catch (Exception)
            {
            }

            oCn.dbdisconnect();
            return found;
        }
        public static bool UnionTownChk(string scall1, Int16 iatwrno, string townTable)
        {
            bool found = false;

            dbconnect oCn = new dbconnect();
            string cSQL = "Select call1, atwrno from main.sd_town " +
                           " where call1='" + scall1.Trim() + "'" +
                           " and atwrno=" + iatwrno + 
                           " UNION " +
                           "Select call1, atwrno from " + townTable +
                           " where call1='" + scall1.Trim() + "'" +
                           " and atwrno =" + iatwrno;
            try
            {
                DataTable oDT = oCn.retrieve(cSQL);
                if (oDT.Rows.Count > 0)
                {
                    found = true;  // bandcode found
                }
            }
            catch (Exception)
            {
            }

            oCn.dbdisconnect();
            return found;
        }
        public XmlElement TowerNoteXml(DocFile oFile)
        {
            XmlElement tTownEl = oFile.CreateElement("towernote");

            tTownEl.AppendChild(oFile.XmlEl("call1", call1));
            tTownEl.AppendChild(oFile.XmlEl("oper", oper));
            tTownEl.AppendChild(oFile.XmlEl("twcode", twcode));
            tTownEl.AppendChild(oFile.XmlEl("twht", twht.ToString()));
            tTownEl.AppendChild(oFile.XmlEl("atwrno", atwrno.ToString()));
            tTownEl.AppendChild(oFile.XmlEl("twli", twli));
            tTownEl.AppendChild(oFile.XmlEl("twpa", twpa));
            tTownEl.AppendChild(oFile.XmlEl("nott", nott));
            tTownEl.AppendChild(oFile.XmlEl("tpoint", tpoint));
            tTownEl.AppendChild(oFile.XmlEl("adate", adate));
            tTownEl.AppendChild(oFile.XmlEl("sdate", sdate));
            tTownEl.AppendChild(oFile.XmlEl("mdate", mdate));
            tTownEl.AppendChild(oFile.XmlEl("mtime", mtime));

            return tTownEl;
        }
    }

    public class Tower
    {
        protected eSubType m_type;

        public string cmd;
        public string recstat;
        public string twcode;
        public string twdesc;
        public string mdate;
        public string mtime;

        public Tower()
        {
            twcode = "";
        }

        public Tower(string sFile, string stwcode)
        {
            string cSQL;
            if (sFile == null || sFile == "" || sFile == "main.sd_towr")
            {
                m_type = eSubType.sd;
                cSQL = "select twcode, twdesc, mdate, mtime " +
                       "from main.sd_towr " +
                       "where twcode='" + stwcode.Trim() + "'";
            }
            else
            {
                m_type = eSubType.su;
                cSQL = "select cmd, recstat, twcode, twdesc, mdate, mtime " +
                       "from " + sFile +
                       " where twcode='" + stwcode.Trim() + "'";
            }
            dbconnect oCn = new dbconnect();

            DataTable oDT = null;
            try
            {
                oDT = oCn.retrieve(cSQL);
                if (oDT.Rows.Count > 0)
                {
                    DataRow oDR = oDT.Rows[0];
                    if (m_type == eSubType.sd)
                    {
                        cmd = "";
                        recstat = "";
                    }
                    else
                    {
                        cmd = Convert.ToString(oDR["cmd"]);
                        recstat = Convert.ToString(oDR["recstat"]);
                    }
                    twcode = Convert.ToString(oDR["twcode"]);
                    twdesc = Convert.ToString(oDR["twdesc"]);
                    mdate = Convert.ToString(oDR["mdate"]);
                    mtime = Convert.ToString(oDR["mtime"]);
                }
                else
                {
                    twcode = "";
                }
            }
            //catch (Exception Ex)
            //{
            //    throw new Exception("IO ERROR: " + twcode + ":-\n" + Ex.Message);
            //}
            finally
            {
                oCn.dbdisconnect();
            }
        }
        public static DataTable TowrKeys(string towrTable)
        {
            dbconnect oCn = new dbconnect();
            string cSQL = "Select twcode " +
                           " from " + towrTable +
                           " order by twcode";

            DataTable oDT = null;
            try
            {
                oDT = oCn.retrieve(cSQL);
            }
            finally
            {
                oCn.dbdisconnect();
            }
            return oDT;
        }
        public static string TowrUpdate(Tower testTowr, string sFile)
        {
            string cSQL;
            string retval = "";

            cSQL = "UPDATE " + sFile + " SET " +
                    sFile + ".cmd='N'," +
                    sFile + ".recstat='U'," +
                    sFile + ".twcode = s.twcode," +
                    sFile + ".twdesc = s.twdesc," +
                    sFile + ".mdate=s.mdate," +
                    sFile + ".mtime=s.mtime " +
                    "FROM " + sFile + " t INNER JOIN main.sd_towr s " +
                    "ON t.twcode = s.twcode " +
                    "WHERE s.twcode='" + testTowr.twcode + "'";

            dbconnect oCn = new dbconnect();
            OdbcCommand update1 = new OdbcCommand(cSQL, oCn.Connection);
            try
            {

                try
                {
                    update1.ExecuteNonQuery();
                    retval = "OK";
                }
                catch (Exception e5)
                {
                    //cn.Close();
                    retval = "ERROR:" + cSQL + ":" + e5.Message;
                }
            }
            finally
            {
                oCn.dbdisconnect();
            }

            return retval;
        }   
   
        public static bool UnionTowrChk(string stwcode, string towrTable)
        {
            bool found = false;

            dbconnect oCn = new dbconnect();
            string cSQL = "Select twcode from main.sd_towr " +
                           " where twcode='" + stwcode.Trim() + "'" +
                           " UNION " +
                           "Select twcode from " + towrTable +
                           " where twcode='" + stwcode.Trim() + "'";
            try
            {
                DataTable oDT = oCn.retrieve(cSQL);
                if (oDT.Rows.Count > 0)
                {
                    found = true;  // bandcode found
                }
            }
            catch (Exception)
            {
            }

            oCn.dbdisconnect();
            return found;
        }
    }

    public class Traffic
    {
        public eSubType m_type;

        public string cmd;
        public string recstat;
        public string trafcode;
        public string ecode;
        public string xreftrcde;
        public string xrefeqcde;
        public string trdesc;
        public string mdate;
        public string mtime;

        public Traffic()
        {
            trafcode = "";
        }

        public Traffic(string sFile, string strafcode, string secode)
        {
            string cSQL;
            if (sFile == null || sFile == "" || sFile == "main.sd_traf")
            {
                m_type = eSubType.sd;
                cSQL = "select trafcode, ecode, xreftrcde, xrefeqcde, trdesc, mdate, mtime " +
                       "from main.sd_traf " +
                       "where trafcode='" + strafcode.Trim() +
                       "' and ecode='" + secode.Trim() + "'";
            }
            else
            {
                m_type = eSubType.su;
                cSQL = "select cmd, recstat, trafcode, ecode, xreftrcde, xrefeqcde, trdesc, mdate, mtime " +
                       "from " + sFile +
                       " where trafcode='" + strafcode.Trim() +
                       "' and ecode='" + secode.Trim() + "'";
            }
            dbconnect oCn = new dbconnect();

            DataTable oDT = null;
            try
            {
                oDT = oCn.retrieve(cSQL);
                if (oDT.Rows.Count > 0)
                {
                    DataRow oDR = oDT.Rows[0];
                    if (m_type == eSubType.sd)
                    {
                        cmd = "";
                        recstat = "";
                    }
                    else
                    {
                        cmd = Convert.ToString(oDR["cmd"]);
                        recstat = Convert.ToString(oDR["recstat"]);
                    }
                    trafcode = Convert.ToString(oDR["trafcode"]);
                    ecode = Convert.ToString(oDR["ecode"]);
                    xreftrcde = Convert.ToString(oDR["xreftrcde"]);
                    xrefeqcde = Convert.ToString(oDR["xrefeqcde"]);
                    trdesc = Convert.ToString(oDR["trdesc"]);
                    mdate = Convert.ToString(oDR["mdate"]);
                    mtime = Convert.ToString(oDR["mtime"]);
                }
                else
                {
                    trafcode = "";
                }
            }
            //catch (Exception Ex)
            //{
            //    throw new Exception("IO ERROR: " + trafcode + "-" + ecode + ":-\n" + Ex.Message);
            //}
            finally
            {
                oCn.dbdisconnect();
            }
        }
        public static DataTable TrafKeys(string trafTable)
        {
            dbconnect oCn = new dbconnect();
            string cSQL = "Select trafcode, ecode " +
                           " from " + trafTable +
                           " order by trafcode, ecode";

            DataTable oDT = null;
            try
            {
                oDT = oCn.retrieve(cSQL);
            }

            finally
            {
                oCn.dbdisconnect();
            }
            return oDT;
        }
        public static string TrafUpdate(Traffic testTraf, string sFile)
        {
            string cSQL;
            string retval = "";

            cSQL = "UPDATE " + sFile + " SET " +
                    sFile + ".cmd='N'," +
                    sFile + ".recstat='U'," +
                    sFile + ".trafcode = s.trafcode," +
                    sFile + ".ecode = s.ecode," +
                    sFile + ".xreftrcde = s.xreftrcde," +
                    sFile + ".xrefeqcde = s.xrefeqcde," +
                    sFile + ".trdesc = s.trdesc," +
                    sFile + ".mdate=s.mdate," +
                    sFile + ".mtime=s.mtime " +
                    "FROM " + sFile + " t INNER JOIN main.sd_traf s " +
                    "ON t.trafcode = s.trafcode AND t.ecode = s.ecode " +
                    "WHERE s.trafcode='" + testTraf.trafcode + "' AND s.ecode = '" + testTraf.ecode + "'";

            dbconnect oCn = new dbconnect();
            OdbcCommand update1 = new OdbcCommand(cSQL, oCn.Connection);
            try
            {

                try
                {
                    update1.ExecuteNonQuery();
                    retval = "OK";
                }
                catch (Exception e5)
                {
                    //cn.Close();
                    retval = "ERROR:" + cSQL + ":" + e5.Message;
                }
            }
            finally
            {
                oCn.dbdisconnect();
            }

            return retval;
        }   
   
        public static bool UnionTrafChk(string strafcode, string secode, string trafTable)
        {
            bool found = false;

            dbconnect oCn = new dbconnect();
            string cSQL = "Select trafcode, ecode from main.sd_traf " +
                           " where trafcode='" + strafcode.Trim() + "'" +
                           " and ecode='" + secode.Trim() + "'" +
                           " UNION " +
                           "Select trafcode, ecode from " + trafTable +
                           " where trafcode='" + strafcode.Trim() + "'" +
                           " and ecode ='" + secode.Trim() + "'";
            try
            {
                DataTable oDT = oCn.retrieve(cSQL);
                if (oDT.Rows.Count > 0)
                {
                    found = true;  // trafcode-ecode found
                }
            }
            catch (Exception)
            {
            }

            oCn.dbdisconnect();
            return found;
        }
        public static bool UnionTrafCodeChk(string strafcode, string trafTable, int ihits)
        {
            // this routine checks number if records in SDF and SDB with the specified trafcode
            // it is used by validate to ensure trafcode is unique
            // for cmd A, ihits is 1 (exists only once in SDF file)
            // for cmd U, ihits is 2 (exists once in SDF and once in SDB)
            // for cmd D, ihits is 2 (exists once in SDF and once in SDB)

            int numhits = 0;

            dbconnect oCn = new dbconnect();
            string cSQL = "SELECT trafcode FROM main.sd_traf " +
                           " WHERE trafcode='" + strafcode.Trim() + "'" +
                           " UNION ALL " +
                           "SELECT trafcode FROM " + trafTable +
                           " WHERE trafcode='" + strafcode.Trim() + "'";


            DataTable oDT = null;
            try
            {
                oDT = oCn.retrieve(cSQL);
                numhits = oDT.Rows.Count;
            }
            catch (Exception)
            {
            }

            oCn.dbdisconnect();

            if (numhits == ihits)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    /// <summary>
    /// This is just a set of routines to handle the in-memory caching of the fee-codes.
    ///	GJS - 1207 - 2006.05.08!
    /// </summary>
    public class FeeCode
    {
        public string code;
        public double fee;
        private static string[] aCodes = null;
        private static double[] aFees = null;


        /// <summary>
        /// Preload in the fee code table.  We really don't use feecode objects, just
        ///	lookups into the table.
        /// </summary>
        /// <returns></returns>
        public static int GetFees()
        {
            string logfile = "D:\\extractlogs\\GetFees.txt";  //  log file
            // open logfile
            StreamWriter swsapr = new StreamWriter(logfile, false);

            DateTime curTime = DateTime.Now;
            string log_time = curTime.ToString("yyyy/MM/dd HH:mm:ss");

            // write info to log file
            swsapr.WriteLine(log_time);
            swsapr.WriteLine(HttpContext.Current.Session["s_cnString"].ToString());
            swsapr.Close();

            int nInd;

            dbconnect oCn = new dbconnect();

            //	Get the size of the table and allocate it in memory.
            int nLen = Convert.ToInt32(oCn.getscalar("Select count(*) from techdef.fee_codes"));
            aCodes = new string[nLen];
            aFees = new double[nLen];

            //	Now load it in.
            DataTable oDT = oCn.retrieve("Select code, fee from techdef.fee_codes");
            nInd = 0;
            foreach (DataRow oDR in oDT.Rows)
            {
                aCodes[nInd] = oDR["code"].ToString();
                try
                {
                    aFees[nInd] = Convert.ToDouble(oDR["fee"]);
                }
                catch { aFees[nInd] = 0.0; }
                nInd++;
            }

            oCn.dbdisconnect();

            return 0;
        }


        /// <summary>
        /// Given a fee code, return the fee.
        /// </summary>
        /// <param name="cCode">One of the fee codes loaded in.</param>
        /// <returns>The fee for the given code.  If it is not there, it will return -1.0</returns>
        public static double FeeVal(string cCode)
        {
            int nLen = aCodes.GetLength(0);
            double Val = -1.0;

            if (cCode == null || cCode == "" || cCode == " ")
            {
                //	The null case returns 0, only error cases return -1
                Val = 0.0;
            }
            else
            {
                for (int nInd = 0; nInd < nLen; nInd++)
                {
                    if (cCode == aCodes[nInd])
                    {
                        Val = aFees[nInd];
                        break;
                    }
                }
            }

            return Val;
        }
    }

    public class FieldIO
    {
        public static Nullable<Int16> getNullableInt16(DataRow oDR, string sfield)
        {
            if (oDR.IsNull(sfield))
            {
                return null;
            }
            else
            {
                return Convert.ToInt16(oDR[sfield]);
            }
        }
        public static Nullable<Int32> getNullableInt32(DataRow oDR, string sfield)
        {
            if (oDR.IsNull(sfield))
            {
                return null;
            }
            else
            {
                return Convert.ToInt32(oDR[sfield]);
            }
        }
        public static Nullable<Int64> getNullableInt64(DataRow oDR, string sfield)
        {
            if (oDR.IsNull(sfield))
            {
                return null;
            }
            else
            {
                return Convert.ToInt64(oDR[sfield]);
            }
        }
        public static Nullable<double> getNullableDouble(DataRow oDR, string sfield)
        {
            if (oDR.IsNull(sfield))
            {
                return null;
            }
            else
            {
                return Convert.ToDouble(oDR[sfield]);
            }
        }
        public static Nullable<float> getNullableSingle(DataRow oDR, string sfield)
        {
            if (oDR.IsNull(sfield))
            {
                return null;
            }
            else
            {
                return Convert.ToSingle(oDR[sfield]);
            }
        }  
    }

}

```
