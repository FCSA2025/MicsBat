using _Configuration;
using _DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _Auxlib
{
    public class AxSub4
    {
        /// <summary>
        /// This method populates the degrees/minutes/seconds fields of an AxStation
        /// object with prescribed integer latitude and longitude values whose decimal
        /// digits are encode as latitude:ddmmss and longitude:dddmmss.
        /// </summary>
        /// <param name="latitude"> - latitude ddmmss</param>
        /// <param name="longitude"> - longitude dddmmss</param>
        /// <param name="axStation"> - AxStation object</param>
        /// <param name="message"> - error message</param>
        /// <returns></returns>
        public static int AxLoadStr(int latitude,              /* input  - latitude ddmmss */
                                    int longitude,		       /* input  - longitude dddmmss */
                                    ref AxStation axStation,   /* output - station data structure */
                                    out string message)        /* output -  - station data structure */
        {
            // 'out' requirement.
            message = "";

            int deg;            /* temp storage for degrees */
            int xmin;           /* temp storage for minutes */
            int sec;            /* temp storage for seconds */

            if (latitude < 0)
            {

                message = "Latitude must have a positive value";
                return (Constant.BAD_LATIT);
            }

            /* parse latitude into degrees, minutes and seconds from ddmmss form */
            deg = (latitude / 10000);
            latitude = latitude - deg * 10000;
            xmin = (latitude / 100);
            sec = latitude - xmin * 100;

            /* verify if data is valid and if so store in output structure */
            if (deg > 90)
            {
                /* come here if latitudeDegrees are too high */
                message = "Invalid Latitude:  Reenter with degrees <= 90";
                return (Constant.BAD_LATIT);
            }
            else if (xmin >= 60)
            {
                /* come here if latitudeMinutes are too high */
                message = "Invalid Latitude:  Reenter with minutes < 60";
                return (Constant.BAD_LATIT);
            }
            else if (sec >= 60)
            {
                /* come here if latitudeSeconds are too high */
                message = "Invalid Latitude:  Reenter with seconds < 60";
                return (Constant.BAD_LATIT);
            }
            else
            {
                /* valid input so store in axStation structure */
                axStation.LL.latDeg = (int)deg;
                axStation.LL.latMin = (int)xmin;
                axStation.LL.latSec = (int)sec;
            }


            if (longitude < 0)
            {

                message = "Longitude must have a positive value";
                return (Constant.BAD_LONGIT);
            }

            /* parse longitude into degrees, minutes & seconds from dddmmss form */
            deg = (longitude / 10000);
            longitude = longitude - deg * 10000;
            xmin = (longitude / 100);
            sec = longitude - xmin * 100;

            /* verify if data is valid and if so store in output structure */
            if (deg > 180)
            {
                /* come here if longitudeDegrees are too high */
                message = "Invalid Longitude:  Reenter with degrees <= 180";
                return (Constant.BAD_LONGIT);
            }
            else if (xmin >= 60)
            {
                /* come here if longitudeMinutes are too high */
                message = "Invalid Longitude:  minutes < 60";
                return (Constant.BAD_LONGIT);
            }
            else if (sec >= 60)
            {
                /* come here if longitudeSeconds are too high */
                message = "Invalid Longitude:  seconds < 60";
                return (Constant.BAD_LONGIT);
            }
            else
            {
                /* valid input so store in axStation structure */
                axStation.LL.longDeg = (int)deg;
                axStation.LL.longMin = (int)xmin;
                axStation.LL.longSec = (int)sec;
            }

            /* now compute the degrees in seconds ie deg*3600 + min*60 + sec and
             * store that in the structure also
             */
            AxSub1.AxDsconv(ref axStation.LL);

            return (Constant.SUCCESS);
        }





    }
}
