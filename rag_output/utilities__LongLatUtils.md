# Documented File: LongLatUtils.cs
**Repository Path:** `utilities\LongLatUtils.cs`
**Primary Layer:** `utilities`
**Namespace:** `LongLatUtilities`

## Source Code Representation
```csharp
namespace LongLatUtilities
{
    /// <summary>
    /// Summary description for LongLatUtils.
    /// </summary>
    public class LongLatUtils
    {
        public static string tmplat;
        public static string tmplong;

        public static string latitDD;
        public static string latitMM;
        public static string latitSS;
        public static string latit00;
        public static string latitDir;
        public static string longitDD;
        public static string longitMM;
        public static string longitSS;
        public static string longit00;
        public static string longitDir;

        public static string intLong(string deg, string min, string sec, string dec)
        {
            //Changes latitude/longitude into an integer value

            string val = "";
            if (deg != "" && min != "" && sec != "" && dec != "")
            {
                //if inputted values are all there
                string dd;
                /*
				Check to see if -
					Latitude------ +(or nothing) = N
							------ -(negative) = S
					Longitude----- +(or nothing) = W
							------ -(negative) = E				
				*/
                if (deg.Substring(0, 1) == "-")
                {
                    //If the negative is present, then remove it to calculate Integer values
                    dd = deg.Substring(1);
                }
                else
                {
                    dd = deg;
                }
                int ideg = System.Convert.ToInt32(dd);
                int imin = System.Convert.ToInt32(min);
                int isec = System.Convert.ToInt32(sec);
                int idec = System.Convert.ToInt32(dec);
                //Calculate Integer Value to be stored
                int nval = (ideg * 360000) + (imin * 6000) + (isec * 100) + idec;
                val = nval.ToString();
                //If negative was present, put it back after the calculation for storage in the database
                if (deg.Substring(0, 1) == "-")
                {
                    val = "-" + val;
                }
            }
            else
            {
                val = "0";
            }
            return val;
        }
        public static string decdeg(string val)
        {
            // this function takes input in integer decmal seconds
            // (Number was stored as degrees*360000 + min*6000 + sec*100 + dec)
            // and converts it to decimal degrees
            string retval;
            if (val == "0" || val == "")
            {
                retval = "";
            }
            else
            {
                int nval = System.Convert.ToInt32(val);
                int msd = nval % 360000;      // get minutes/seconds/decimal
                int degrees = (nval - msd) / 360000;      // get degrees
                string dd = degrees.ToString();          // convert degrees to string

                double ddeg = msd / 360000.0;  // get minutes/secs/decimals as decimal deg
                double fdeg = degrees + ddeg;
                retval = fdeg.ToString();
            }
            return retval;
        }
        public static string relat(string val)
        {
            /*
				Conversion Function From Integer To String for Latitude and Longitudes in decimal seconds
				FORMULA:
				Number was stored as degrees*360000 + min*6000 + sec*100 + dec
			*/
            string retval;
            if (val == "0" || val == "")
            {
                retval = "";
            }
            else
            {
                int nval = System.Convert.ToInt32(val);
                int msd = nval % 360000;      // get minutes/seconds/decimal
                int degrees = (nval - msd) / 360000;      // get degrees
                string dd = degrees.ToString();          // convert degrees to string

                int sd = msd % 6000; // get seconds/decimals
                int minutes = (msd - sd) / 6000; // get minutes
                string mm = minutes.ToString();  // convert minutes to string

                int idec = sd % 100;
                int seconds = (sd - idec) / 100;
                string ss = seconds.ToString();

                string dec = idec.ToString();

                retval = dd + "/" + mm + "/" + ss + "/" + dec;
            }
            return retval;
        }

        private static string neg(string val)//Removes Negative Sign(-) for S/E
        {
            string retval;
            if (val.Substring(0, 1) == "-")
            {
                retval = val.Substring(1);
            }
            else
            {
                retval = val;
            }
            return retval;
        }
        private static string reneg(string val, string val2)//Puts Negative Sign Back For S/E
        {
            string retval = val2;
            if (val.Substring(0, 1) == "-")
            {
                retval = "-" + val2;
            }
            return retval;
        }
        public static void SplitLat(string latit)
        {
            string slat;
            latitDD = "";
            latitMM = "";
            latitSS = "";
            latit00 = "";
            latitDir = "N";

            if (latit != "")
            {
                //Remove Negative Value for S
                slat = neg(latit);
                //Perform Conversion for String Value
                slat = relat(slat);
                if (slat != "")
                {
                    //Put Negative Value Back For S
                    slat = reneg(latit, slat);
                    // split return string from relat into parts
                    char[] delimiter = "/".ToCharArray();
                    string[] latparts = slat.Split(delimiter);

                    latitDD = laloLength(latparts[0], 2);
                    latitMM = laloLength(latparts[1], 2);
                    latitSS = laloLength(latparts[2], 2);
                    latit00 = laloLength(latparts[3], 2);

                    if (latitDD.Substring(0, 1) == "-")
                    {
                        latitDir = "S";
                    }
                }
            }
        }
        public static void SplitLong(string longit)
        {
            string slong;
            longitDD = "";
            longitMM = "";
            longitSS = "";
            longit00 = "";
            longitDir = "W";

            if (longit != "")
            {
                //remove negative sign if E
                slong = neg(longit);
                //Calculate Longitude String Conversion
                slong = relat(slong);

                if (slong != "")
                {
                    //Put Negative Sign Back if E
                    slong = reneg(longit, slong);

                    // split return string from relat into parts
                    char[] delimiter = "/".ToCharArray();
                    string[] longparts = slong.Split(delimiter);

                    longitDD = laloLength(longparts[0], 3);
                    longitMM = laloLength(longparts[1], 2);
                    longitSS = laloLength(longparts[2], 2);
                    longit00 = laloLength(longparts[3], 2);

                    if (longitDD.Substring(0, 1) == "-")
                    {
                        longitDir = "E";
                    }
                }
            }
        }
        private static string laloLength(string val, int len)
        {
            if (val.Substring(0, 1) == "-" && val.Length <= len)
            {
                if (len == 2 && val.Length == 2)
                {
                    val = "-0" + val.Substring(1);
                }
                else if (len == 3 && val.Length == 2)
                {
                    val = "-00" + val.Substring(1);
                }
                else if (len == 3 && val.Length == 3)
                {
                    val = "-0" + val.Substring(1);
                }
            }
            else if (val.Length < len && val.Substring(0, 1) != "-")
            {
                if (val.Length == 1 && len == 3)
                {
                    val = "00" + val;
                }
                else
                {
                    val = "0" + val;
                }
            }
            return val;
        }
        public static string misslat(string latDD, string latMM, string latSS, string lat00)
        {
            if (latDD == "")
            {
                return "You must enter a latitude degrees value";
            }
            if (latMM == "")
            {
                return "You must enter a latitude minutes value";
            }
            if (latSS == "")
            {
                return "You must enter a latitude seconds value";
            }
            if (lat00 == "")
            {
                return "You must enter a latitude decimal seconds value";
            }
            return "f";
        }
        public static string misslong(string longDD, string longMM, string longSS, string long00)
        {
            if (longDD == "")
            {
                return "You must enter a longitude degrees value";
            }
            if (longMM == "")
            {
                return "You must enter a longitude minutes value";
            }
            if (longSS == "")
            {
                return "You must enter a longitude seconds value";
            }
            if (long00 == "")
            {
                return "You must enter a longitude decimal seconds value";
            }
            return "f";
        }
        public static string dashtointdecsecs(string incoord)
        {
            char[] delimiter = "-".ToCharArray();
            string[] coordparts = incoord.Split(delimiter);

            string coordDD = coordparts[0]; // degrees
            string coordMM = coordparts[1]; // minutes

            delimiter = ".".ToCharArray();  // split seconds to get decimal seconds
            string[] secparts = coordparts[2].Split(delimiter);
            string coordSS = secparts[0];
            string coord00 = secparts[1];

            return intLong(coordDD, coordMM, coordSS, coord00);
        }
    }
}

```
