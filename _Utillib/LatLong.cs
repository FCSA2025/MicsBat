using _Configuration;
using _NewLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _Utillib
{
    public class LatLong
    {

        /// <summary>
        /// This method will convert integer centiseconds to decimal degrees.  
        /// </summary>
        /// <param name="centiSeconds"></param>
        /// <returns></returns>
        public static double FsecsToDeg(int centiSeconds)
        {
            /*  Convert integer seconds * 100 to double degrees */
            return (centiSeconds / 360000.0);
        }

        /// <summary>
        /// This method will convert decimal degrees to integer centiseconds.  
        /// </summary>
        /// <param name="dDeg"> - decimal degrees.</param>
        /// <returns></returns>
        public static int FDegToSecs(double dDeg)
        {
            /*  Convert degrees to integer seconds * 100  */
            return (int)(dDeg * 360000.0 + 0.5);
        }

        /// <summary>
        /// This method returns a latitude in centiseconds (1/100 s) for a prescribed
        /// latitude string of the form 00-00-00.00N; on return, cLat has its sense
        /// letter removed.
        /// </summary>
        /// <param name="cLat"> - prescribed latitude string.</param>
        /// <returns>Latitude in centiseconds</returns>
        public static int Get_Lat(ref string cLat)
        {
            //...Log2.v("\nLatLong.Get_Lat(): cLat = " + cLat);

            char cSense;
            int nOutLat;

            if (GetSense(ref cLat, out cSense, 'N') == Constant.SUCCESS)
            {
                if (GenUtil.UtStrConvLongLat(Constant.LATITUDE, cLat, cSense, out nOutLat) != Constant.SUCCESS)
                {
                    nOutLat = 0;
                }
            }
            else
            {
                nOutLat = 0;
            }

            //...Log2.v("\nLatLong.Get_Lat(): cLat, cSense = " + cLat + ", " + cSense);
            return (nOutLat);
        }


        /// <summary>
        /// This method returns a longitude in centiseconds (1/100 s) for a prescribed
        /// longitude string of the form 000-00-00.00W; on return, cLong has its sense
        /// letter removed.
        /// </summary>
        /// <param name="cLong"> - prescribed longitude string.</param>
        /// <returns>Longitude in centiseconds</returns>
        public static int Get_Long(ref string cLong)
        {
            //...Log2.v("\nLatLong.Get_Long(): cLong = " + cLong);

            char cSense;
            int nOutLong;

            if (GetSense(ref cLong, out cSense, 'W') == Constant.SUCCESS)
            {
                if (GenUtil.UtStrConvLongLat(Constant.LONGITUDE, cLong, cSense, out nOutLong) != Constant.SUCCESS)
                {
                    nOutLong = 0;
                }
            }
            else
            {
                nOutLong = 0;
            }

            //...Log2.v("\nLatLong.Get_Long(): cLong, cSense = " + cLong + ", " + cSense);
            return (nOutLong);
        }

        /// <summary>
        /// This method inputs a lat/long string of the form 000-00-00.00Z and outputs 
        /// its {N,S,E,W} sense letter and the lat/long string stripped of its sense letter; if the
        /// sense letter is absent from the input it is set to the prescribed default value.
        /// </summary>
        /// <param name="cCoord"> - on input, a string of the form 000-00-00.00Z; on output, the stripped string 000-00-00.00 </param>
        /// <param name="cSense"> - returns one of {N,S,E,W}</param>
        /// <param name="cDefault"> - prescribed default value for the sense.</param>
        /// <returns></returns>
        public static int GetSense(ref string cCoord, out char cSense, char cDefault)
        {
            //...Log2.v("\nLatLong.GetSense(): cCoord = " + cCoord);

            // 'out' requirement.
            cSense = '\0';

            /*	Scan the input cCoord string from the right. The first non-blank will
                    either be a sense (N, W, E, S) or a digit.  If a digit, then the
                    default is used.
            */
            short nLen = (short)cCoord.Length;
            int nRet = Constant.FAILURE;

            while (--nLen >= 0)
            {
                //...Log2.v("\nLatLong.GetSense(): nLen         = " + nLen);
                //...Log2.v("\nLatLong.GetSense(): cCoord[nLen] = " + cCoord[nLen]);

                if (Strings.IsDigit(cCoord[nLen]) || Strings.IsPunct(cCoord[nLen]))
                {
                    /*	We have a digit or punctuation.  We have gone too far and not
                            encountered a letter for the sense.  Use the default . */
                    cSense = cDefault;
                    nRet = Constant.SUCCESS;
                    break;
                }

                if (Strings.IsAlphabetic(cCoord[nLen]))
                {
                    /*	Got a letter, make sure it is valid */
                    if (Strings.StrChr("NnSsEeWw", cCoord[nLen]) == 0)
                    {
                        Log2.e("\nLatLong.GetSense(): ERROR: invalid sense letter.");
                        /*	Invalid sense */
                        cSense = cDefault;
                        nRet = Constant.FAILURE;
                        break;
                    }
                    else
                    {
                        /*	Found a valid sense, convert to u/c and return */
                        cSense = Char.ToUpper(cCoord[nLen]);
                        /*	Terminate cCoord at the sense. */
                        if (nLen == 0)
                        {
                            cCoord = "";
                        }
                        else
                        {
                            cCoord = cCoord.Substring(0, nLen);
                        }
                        nRet = Constant.SUCCESS;
                        break;
                    } // if (Strings.StrChr("NnSsEeWw", cCoord[nLen]) == 0)
                } // if (Strings.IsAlphabetic
            } // while (--nLen >= 0)

            //...Log2.v("\nLatLong.GetSense(): cCoord = " + cCoord);
            //...Log2.v("\nLatLong.GetSense(): cSense = " + cSense);
            //...Log2.v("\nLatLong.GetSense(): nRet   = " + nRet);
            return (nRet);
        }



    }
}
