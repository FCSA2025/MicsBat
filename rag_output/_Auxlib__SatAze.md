# Documented File: SatAze.cs
**Repository Path:** `_Auxlib\SatAze.cs`
**Primary Layer:** `_Auxlib`
**Namespace:** `_Auxlib`

## Source Code Representation
```csharp
using _DataStructures;
using System;
using static System.Math;
using static _NewLib.Maths;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _Configuration;

namespace _Auxlib
{
    /// <summary>
    /// Provides methods that calculate azimuth and elevation between an
    /// Earth station and a geostationary satellite, taking refraction 
    /// into account.
    /// </summary>
    public class SatAze
    {
        private const double _dAccuracy = 0.01;
        /// <summary>
        /// Calculates the azimuth and elevation angle from a given earth 
        /// station to a given geostationary satellite. Atmospheric refraction is 
        /// considered. RETURNS:- 0 All okay. 1 Hits the earth <0 Other error.  
        /// </summary>
        /// <param name="es"> - earth station data</param>
        /// <param name="ste"> - satellite data</param>
        /// <param name="azim"> - azimuth</param>
        /// <param name="elevAng"> - elevation angle</param>
        /// <param name="rtElevAng"> - refracted elev angle</param>
        /// <returns></returns>
        public static int Sataze(AxStation es,  /* 	input  - earth station data */
                 AxStation ste, /* 	input  - satellite data */
                 out double azim,					/* output - azimuth */
                  out double elevAng,				/* output - elevation angle */
             out double rtElevAng)         /* output - refracted elev angle */
        {
            // 'out' requirements;
            azim = 0.0;
            elevAng = 0.0;
            rtElevAng = 0.0;

            int err;                /* return code variable */

            double esLtd;           /* ES latitude in decimal */
            double esLgd;          /* ES longitude in decimal */
            double steLgd;         /* satellite longitude in decimal */
            double longDiff;       /* long difference between antenna and satel */
            double ae;                 /* angle wrt equator in degrees */
            double antHt1Aksl; /* antenna height for ES in KM above sea level*/
            double angleB;         /* angle B in degrees */
            double k;                  /* constant */
                                       //diff = 1.0;	/* diff between geo. and refractive angles    */
            double dHorizonElev = 0.0;  /*	Horizon Elevation		*/
            bool IsLow = false;
            int nRet = 0;

            /* convert lat and long to decimal numbers */
            ste.LL.latDeg = ste.LL.latMin = ste.LL.latSec = 0;
            ste.LL.latSens = " ";

            AxSub1.AxDsconv(ref es.LL);

            es.LL.latSeconds = Abs(es.LL.latSeconds);
            esLtd = es.LL.latSeconds / 3600.0;
            esLgd = es.LL.longSeconds / 3600.0;
            steLgd = ste.LL.longSeconds / 3600.0;

            //&&Console.Error.Write("\nsataze.sataze(): zebra: esLtd = {0}", esLtd);
            //&&Console.Error.Write("\nsataze.sataze(): zebra: esLgd = {0}", esLgd);
            //&&Console.Error.Write("\nsataze.sataze(): zebra: steLgd = {0}", steLgd);

            /* calc longitude difference */
            longDiff = esLgd - steLgd;

            /* calculate angle wrt equator */
            ae = AtanD(TanD(longDiff) / SinD(esLtd));

            //&&Console.Error.Write("\nsataze.sataze(): lizard: ae = {0}", ae);
            /* calculate azimuth */
            if (es.LL.latSens.Equals(Constant.SOUTH))
            {
                azim = (ae < 0.0) ? ae + 360.0 : ae;
            }
            else
            {
                azim = 180.0 - ae;
            }
            antHt1Aksl = (es.antHtM + es.elevM) / 1000.0;

            /* calulate angle B */
            angleB = AtanD(TanD(esLtd) / CosD(ae));

            /* calculate constant K */
            k = (Constant.GSORAD - Constant.ERTHRD - antHt1Aksl) / (Constant.GSORAD + Constant.ERTHRD + antHt1Aksl);

            /* calc geometric elevation angle */
            elevAng = AtanD(k / TanD(angleB / 2.0)) - (angleB / 2.0);

            /*	If the geometric elevation angle is less than -2.5, then assume it hits
            *		the earth with any refraction.
            *		** CHANGE ** We are getting unsolvable equations at -2.5, so we move it
            *		to 0.0 --- This needs to be looked at mathematically.  */
            if (elevAng < 0.0)
            {
                rtElevAng = elevAng;
                nRet = 1;
            }
            else if (elevAng < dHorizonElev)
            {
                /*	If the geometric elevation angle is below the horizon.  First try
                *		to calculate the actual antenna elevation angle (the refracted
                *		elevation angle) with N=250.0  This could get the closest beam to the
                *		Horizon. */
                if ((err = SolveElev(250.0, antHt1Aksl, elevAng, out rtElevAng)) < 0)
                {
                    return (err);
                }
                IsLow = true;       /*	Indicate that the lower refraction cooefficient used*/
            }
            else
            {
                /*	Try to get the most refracted beam */
                if ((err = SolveElev(400.0, antHt1Aksl, elevAng, out rtElevAng)) < 0)
                {
                    /*	Problem, return error */
                    return (err);
                }
                IsLow = false;
            }

            if (rtElevAng < dHorizonElev && IsLow)
            {
                /*	The beam was below the horizon and is still.  Try once more with
                *		maximum refraction (N=400) */
                if ((err = SolveElev(400.0, antHt1Aksl, elevAng, out rtElevAng)) < 0)
                {
                    /*	Problem, return error */
                    return (err);
                }
            }

            /*	At this point, the refracted elevation angle is a close above the
            *		horizon as possible.  Check it.*/
            if (rtElevAng < dHorizonElev)
            {
                nRet = 1;
            }
            else
            {
                nRet = 0;
            }

            return (nRet);
        }

        /// <summary>
        /// Given the refraction coefficient, the antenna height, the geometric 
        /// elevation angle to the satellite, this routine will return the antenna 
        /// elevation angle. It uses the Regula Falsi algorithm. RETURNS - 0 All went 
        /// okay. <0 Error. Check calcula->sResult.  
        /// </summary>
        /// <param name="dN"> - Refraction Cooefficient</param>
        /// <param name="dHeightKm"> - Height of antenna in km.</param>
        /// <param name="dEps"> - Geometric elevation angle</param>
        /// <param name="dRoot"> - Output Antenna Elevation angle</param>
        /// <returns></returns>
        public static int SolveElev(double dN,				 	/*	Refraction Cooefficient 					*/
              double dHeightKm,		/*	Height of antenna in km.					*/
              double dEps,				/*	Geometric elevation angle					*/
              out double dRoot)        /*	Output Antenna Elevation angle		*/
        {
            // 'out' requirement.
            dRoot = 0.0;

            ExZoneCalc calcula = new ExZoneCalc();  /*  the work area                 			*/
            int nRet;
            double dTau;

            /*	Search from dEps + 3 to dEps for solution = dEps */
            if ((nRet = AxTau(dN, dHeightKm, dEps + 3.0, out dTau)) != 0)
            {
                //                       123456789012345678901234567890123456    
                calcula.sResult = "Could not calculate Tau for eps + 3.";
                return (-5);
            }
            calcula.dLow = dEps + 3.0;
            calcula.dLowVal = 3.0 - dTau;
            calcula.dHigh = dEps;
            if ((nRet = AxTau(dN, dHeightKm, dEps, out dTau)) != 0)
            {
                calcula.sResult = "Could not calculate Tau for eps.";
                return (-4);
            }
            calcula.dHighVal = -dTau;

            calcula.nIter = 0;
            while (true)
            {
                /* Ensure that there is a solution */
                if (calcula.dLowVal * calcula.dHighVal > 0)
                {
                    /*  If the product of the end-points is positive then they are both
                        on the same side of the x-axis. */
                    calcula.sResult = "End Points same sign.";
                    return (-1);

                }
                else
                {
                    dRoot = calcula.dLow +
                             (Abs(calcula.dLowVal) / (Abs(calcula.dLowVal) +
                                                       Abs(calcula.dHighVal))) *
                             (calcula.dHigh - calcula.dLow);

                    /*	This is the call to the function ***************************/
                    if ((nRet = AxTau(dN, dHeightKm, dRoot, out dTau)) != 0)
                    {
                        calcula.sResult = "Could not calculate Tau for Root.";
                        return (-4);
                    }
                    calcula.dNextVal = dRoot - dTau - dEps;
                    /*	************************************************************/

                    /*  See if we have an end condition */
                    if (Abs(calcula.dNextVal) < _dAccuracy)
                    {
                        /*  Exit condition.  We have solved the equation */
                        calcula.sResult = "";
                        return (0);
                    }
                    else
                    {
                        /*  Check for too many iterations */
                        if (++(calcula.nIter) > 100)
                        {
                            calcula.sResult = "Too many Iterations.";
                            return (-2);
                        }
                        else
                        {
                            /*  Calculate the next end point set */
                            if (calcula.dNextVal * calcula.dLowVal > 0.0)
                            {
                                /* Next val is the same sign as the low val */
                                calcula.dLow = dRoot;
                                calcula.dLowVal = calcula.dNextVal;
                            }
                            else
                            {
                                calcula.dHigh = dRoot;
                                calcula.dHighVal = calcula.dNextVal;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Calculates the refractive bending of a beam as it goes 
        /// throught the atomosphere. It is calulated two ways, depending on whether or 
        /// not the initial angle is positive or negative.  
        /// </summary>
        /// <param name="refInd"> - refractive index</param>
        /// <param name="antHt1Aksl"> - ht of antenna in KM above sea level</param>
        /// <param name="theta0"> - antenna elevation angle</param>
        /// <param name="tauout"> - refractive bending</param>
        /// <returns></returns>
        public static int AxTau(double refInd,        /* input  - refractive index */
                                double antHt1Aksl,  /* input  - ht of antenna in KM above sea */
                                double theta0,      /* input  - antenna elevation angle */
                                out double tauout)     /* output - refractive bending */
        {
            return AtBend.Refrac(theta0, (refInd < 325.0 ? 2 : 1), antHt1Aksl, out tauout);
        }

        /// <summary>
        /// This method calculates the azimuth and elevation angle from a given earth station 
        /// to a given geostationary satellite; atmospheric refraction is considered.
        /// </summary>
        /// <param name="du">- distance unit, F or M.</param>
        /// <param name="es">- earth station location and ant information.</param>
        /// <param name="refInd">- refract index.</param>
        /// <param name="ste">- satellite location and ant information.</param>
        /// <param name="azim">azim	- ES to satellite azimuth.</param>
        /// <param name="elevAng">- ES to satellite geometric elevation angle.</param>
        /// <param name="rtElevAng">- refracted elevation angle in deg.</param>
        /// <returns></returns>
        public static int AxSataze(string du,						/* input  - distance unit */
                                    ref AxStation es,               /* input  - earth station data */
                                    double refInd,					/* input  - refractive index */
                                    ref AxStation ste,              /* input  - satellite data */
                                    out double azim,			    /* output - azimuth */
                                    out double elevAng,				/* output - elevation angle */
                                    out double rtElevAng)           /* output - refracted elev angle */
        {
            // 'out' requirements.
            azim = 0.0;
            elevAng = 0.0;
            rtElevAng = 0.0;

            int err;        /* return code variable */
            int count = 0;  /* loop control variable */

            double esLtd;       /* ES latitude in decimal */
            double esLgd;       /* ES longitude in decimal */
            double steLgd;      /* satellite longitude in decimal */
            double longDiff;    /* long difference between antenna and satel */
            double ae;          /* angle wrt equator in degrees */
            double antHt1Aksl;  /* antenna height for ES in KM above sea level*/
            double angleB;      /* angle B in degrees */
            double k;           /* constant */
            double e2;          /* calulation var to determine refract angle  */
            double diff = 1.0;  /* diff between geo. and refractive angles    */
            double tauout;      /* calc variable for TAU (refractive bending  */


            /* if distance units in feet, convert to meters for calculations */
            if (du.Equals(Constant.FEET))
            {
                es.elevM *= Constant.FT_TO_METERS;
                es.antHtM *= Constant.FT_TO_METERS;
            }

            /* convert lat and long to decimal numbers */
            ste.LL.latDeg = ste.LL.latMin = ste.LL.latSec = 0;
            ste.LL.latSens = " ";

            AxSub1.AxDsconv(ref es.LL);

            es.LL.latSeconds = Abs(es.LL.latSeconds);
            esLtd = es.LL.latSeconds / 3600.0;
            esLgd = es.LL.longSeconds / 3600.0;
            steLgd = ste.LL.longSeconds / 3600.0;

            if (ste.LL.longSens.Equals("E") ||
                ste.LL.longSens.Equals("e"))
            {
                /*	The satellite is at longitude East */
                steLgd = -steLgd;
            }

            /* calc longitude difference */
            longDiff = esLgd - steLgd;

            /* calculate angle wrt equator */
            ae = AtanD(TanD(longDiff) / SinD(esLtd));

            /* calculate azimuth */
            if (es.LL.latSens.Equals(Constant.SOUTH))
            {

                azim = (ae < 0.0) ? ae + 360.0 : ae;
            }
            else
            {
                azim = 180.0 - ae;
            }

            antHt1Aksl = (es.antHtM + es.elevM) / 1000.0;

            /* calulate angle B */
            angleB = AtanD(TanD(esLtd) / CosD(ae));

            /* calculate constant K */
            k = (Constant.GSORAD - Constant.ERTHRD - antHt1Aksl) / (Constant.GSORAD + Constant.ERTHRD + antHt1Aksl);

            /* calc geometric elevation angle */
            elevAng = AtanD(k / TanD(angleB / 2.0)) - (angleB / 2.0);

            /* zero in refracted beam */
            if ((err = AxTau(refInd, antHt1Aksl, elevAng, out tauout)) < Constant.SUCCESS)
            {
                return (err);
            }

            while ((Abs(diff) > 0.01) && (count++ < 10))
            {

                rtElevAng = elevAng + tauout;

                if ((err = AxTau(refInd, antHt1Aksl, rtElevAng, out tauout)) < Constant.SUCCESS)
                {
                    return (err);
                }
                else
                {
                    e2 = rtElevAng - tauout;
                    diff = e2 - elevAng;
                }
            }
            if (Abs(diff) > 0.01)
            {
                /* In SATAZE: No convergence to TAU */
                return (Constant.SAT_CONVERG_FAIL);
            }
            /* In SATAZE: Convergence to TAU is successful */
            return (Constant.SUCCESS);
        }





    }
}

```
