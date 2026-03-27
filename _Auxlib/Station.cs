using _Configuration;
using _DataStructures;
using _NewLib;
using _Utillib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _Auxlib
{
    public class Station
    {
        /// <summary>
        /// This method populates an existing AxStation object with prescribed latitude 
        /// or longitude information.
        /// </summary>
        /// <param name="axStation"></param>
        /// <param name="cLatLong"></param>
        /// <param name="LLType"></param>
        /// <returns></returns>
        public static int LoadStn(ref AxStation axStation, string cLatLong, int LLType)
        {
            string cSense = "";
            int nCentiSecs;
            int nLen;

            // Manage the presence, or absence of, (lat, long) 'sense' characters.
            nLen = (int)cLatLong.Length;
            cSense = cLatLong.Substring(nLen - 1, 1);  /* Get the sense from end */
            if (Strings.IsAlphabetic(cSense[0]))
            {
                /*	A sense was present */
                cLatLong = Strings.DropLastChar(cLatLong);
            }
            else
            {
                cSense = (LLType == Constant.LATITUDE ? "N" : "W");
            }

            GenUtil.UtStrConvLongLat(LLType, cLatLong, cSense[0], out nCentiSecs);

            //...Log2.v("\nStation.LoadStn(): " + cLatLong + "    " + nCentiSecs);

            if (LLType == Constant.LATITUDE)
            {
                // It is a latitude.
                axStation.LL.latSeconds = (double)nCentiSecs / 100.0;
                axStation.LL.latDeg = nCentiSecs / 360000;
                nCentiSecs %= 360000;
                axStation.LL.latMin = nCentiSecs / 6000;
                nCentiSecs %= 6000;
                axStation.LL.latSec = nCentiSecs / 100;
                axStation.LL.latSens = cSense;

                //...Log2.v(String.Format("\nloadstn(): lat.  {0}  :  {1}  {2}  {3}  {4}  {5}", cLatLong, axStation.LL.latDeg, axStation.LL.latMin, axStation.LL.latSec, axStation.LL.latSeconds, nCentiSecs));
            }
            else
            {
                // It is a longitude.
                axStation.LL.longSeconds = (double)nCentiSecs / 100.0;
                axStation.LL.longDeg = nCentiSecs / 360000;
                nCentiSecs %= 360000;
                axStation.LL.longMin = nCentiSecs / 6000;
                nCentiSecs %= 6000;
                axStation.LL.longSec = nCentiSecs / 100;
                axStation.LL.longSens = cSense;

                //...Log2.v(String.Format("\nloadstn(): lat.  {0}  :  {1}  {2}  {3}  {4}  {5}", cLatLong, axStation.LL.longDeg, axStation.LL.longMin, axStation.LL.longSec, axStation.LL.longSeconds, nCentiSecs));
            }

            return (0);
        }








    }
}
