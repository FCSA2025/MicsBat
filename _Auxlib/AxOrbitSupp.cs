using _Configuration;
using _DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Math;
using static _NewLib.Maths;
using System.IO;
using _Utillib;
using _NewLib;

namespace _Auxlib
{
    /// <summary>
    /// Provides methods that support orbit calculations.
    /// </summary>
    public class AxOrbitSupp
    {

        /// <summary>
        /// Calculates the angle of closest approach between the main lobe 
        /// of a terrestrial radio relay antenna and the refracted geostationary 
        /// satellite orbit.  
        /// </summary>
        /// <param name="distUnit"> - distance unit, M or F</param>
        /// <param name="s1"> - structure of information for station 1</param>
        /// <param name="s2"> - structure of information for station 2 Each structure contains the location of the station in degrees, minutes, seconds, and sense (N, S, E, or W), as well as the station name, elevation above sea level, and antenna height.</param>
        /// <param name="trueAzim"> - true Azimuth</param>
        /// <param name="trueElev"> - true Elevation</param>
        /// <param name="trueVals"> - flag if true Azim & Elev</param>
        /// <param name="distKm"> - stn1 to stn2 distance in KM</param>
        /// <param name="azimuth"> - satellite azimuth in deg</param>
        /// <param name="elevAng"> - elevation angle from stn 1 to stn 2</param>
        /// <param name="isectLong"> - intersection longitude</param>
        /// <param name="isectLongSen"> - intersection longitude sense, E or W</param>
        /// <param name="minAngSep"> - minimum angular separation in deg</param>
        /// <param name="eirp1_10"> - maximum EIRP in dBW for 1 to 10 GHz</param>
        /// <param name="eirp10_15"> - maximum EIRP in dBW for 10 to 15 GHz</param>
        /// <param name="eirpGt15"> - maximum EIRP in dBW for above 15 GHz</param>
        /// <param name="critAzimSt"> - critical azimuth start in deg</param>
        /// <param name="critAzimEnd"> - critical azimuth end in deg</param>
        /// <param name="critLongSt"> - critical longitude start</param>
        /// <param name="critLongStSen"> - critical longitude start, E or W</param>
        /// <param name="critLongEnd"> - critical longitude end</param>
        /// <param name="critLongEndSen"> - critical longitude end sense, E or W</param>
        /// <param name="trans"> - flag indicating if the stations were transpsed</param>
        /// <returns></returns>
        public static int AxOrbit(string distUnit,		/* input  - distance unit */

            ref AxStation s1,               /* input - station 1 data */
            ref AxStation s2,               /* input - station 2 data */
            double trueAzim,			/* input - true Azimuth */
            double trueElev,			/* input - true Elevation */
            int trueVals,				/* input - flag if true Azim & Elev */

            ref double distKm,			/* output - stn1 to stn2 distance */
            ref double azimuth,			/* output - satellite azimuth */
            ref double elevAng,			/* output - stn1 to stn1 elev angle */
            ref double isectLong,		/* output - intersection longitude */
            ref string isectLongSen,	/* output - intersection longitude sense */
            ref double minAngSep,		/* output - minimum angular separation */
            ref double eirp1_10,		/* output - max EIRP for 1 to 10 GHz */
            ref double eirp10_15,		/* output - max EIRP for 10 to 15 GHz */
            ref double eirpGt15,		/* output - max EIRP for above 15 GHz */
            ref double critAzimSt,		/* output - critical azimuth start */
            ref double critAzimEnd,		/* output - critical azimuth end */
            ref double critLongSt,		/* output - critical longitude start */
            ref string critLongStSen,	/* output - critical longitude start sense */
            ref double critLongEnd,		/* output - critical longitude end */
            ref string critLongEndSen,	/* output - critical longitude end sense */
            ref int trans)              /* output - flag for transposed stations */
        {
            // 'out' requirements.
            //distKm = 0.0;
            //azimuth = 0.0;
            //elevAng = 0.0;
            //isectLong = 0.0;
            //isectLongSen = "";
            //minAngSep = 0.0;
            //eirp1_10 = 0.0;
            //eirp10_15 = 0.0;
            //eirpGt15 = 0.0;
            //critAzimSt = 0.0;
            //critAzimEnd = 0.0;
            //critLongSt = 0.0;
            //critLongStSen = "";
            //critLongEnd = 0.0;
            //critLongEndSen = "";
            //trans = 0;

            int errorCode = Constant.ORBIT_FAIL;
            int count;
            int exitFlag;

            double antHt1Aksl;      /* height of antenna at stn1 in KM above sea  */
            double angAssumeHor;    /* elevation angle of assumed radio horizon   */
            double azimDist;        /* min angular dist to the antenna elevation  */
            double b;               /* calculation variable                       */
            double bearing21;       /* true bearing from stn 2 to stn 1 in deg    */
            double elevAngMax;      /* calculated maximum elevation angle         */
            double elevAngMin;      /* calculated minimum elevation angle         */
            double elevAng21;       /* elevation angle from stn 2 to stn 1 in deg */
            double k;               /* constant based on radius of geosync orbit; */
                                    /*      radius of earth; and height of the    */
                                    /*      transmitting antenna                  */
            double ko;              /* working variable                           */
            double m;               /* stn 1 latitude in deg & fractions of deg   */
            double refractConst;    /* refractivity const for transmitting antenna*/
            double refractHt;       /* calculated refractivity                    */
            double ri250;           /* calcd intersection of the elevation angle  */
                                    /*      between stn1 and the ROT with ref ind */
                                    /*      250 (lower reference point)           */
            double ri400;           /* calcd intersection of the elevation angle  */
                                    /*      between stn1 and the ROT with ref ind */
                                    /*      400 (upper reference point)           */
            double rotElevAng;      /* elevation angle of the ROT                 */
            double rotElevAngDer;   /* derivative of the elevaiton angle of ROT   */
            double s;               /* transmit azimuth from south                */
            double sc1;             /* working var for critical azimuth start     */
            double sc2;             /* working var for crit azimuth end           */
            double so;              /* calculated intersection of the elevation   */
                                    /*      angle between stn1 and the ROT with no*/
                                    /*      refraction (ROT=Refracted Orbit Trace)*/
            double s2eleva;         /* elevation above sea level of stn 2         */
            double temp;            /* temporary working variable                 */
            double testpt;          /* calculation variable                       */
            double wg;              /* calculation variable                       */
            double X1;              /* calculation variable                       */
            double X2;              /* calculation variable                       */
            double Y1;              /* calculation variable                       */
            double Y2;              /* calculation variable                       */

            AxStation tmp;

            int nRet;

            sc1 = 0.0;
            sc2 = 0.0;

            /*--- call INVPOS - to get azimuth and elevation angle ---*/
            AxInvPos.AxInvpos(distUnit, s1, s2, out distKm, out azimuth, out bearing21, out elevAng, out elevAng21);

            /* set most polar site to station 1 and set transposed flag */
            if (Abs(s1.LL.latSeconds) < Abs(s2.LL.latSeconds))
            {
                trans = Constant.TRUE;

                // For this 'swap' to have persistence after this method returns both
                // s1 and s2 need to be declared as 'ref' in the call argument list.
                tmp = s1;
                s1 = s2;
                s2 = tmp;

                azimuth = bearing21;
                elevAng = elevAng21;
            }
            else
            {

                trans = Constant.FALSE;
            }

            if (trueVals == Constant.TRUE)
            {

                azimuth = trueAzim;

                elevAng = trueElev;
            }

            /* calculate antenna heights in KM above sea level */
            antHt1Aksl = (s1.elevM + s1.antHtM) / 1000.0;
            s2eleva = (s2.elevM) / 1000.0;

            k = (Constant.GSORAD - Constant.ERTHRD1 - antHt1Aksl) / (Constant.GSORAD + Constant.ERTHRD1 + antHt1Aksl);
            m = s1.LL.latSeconds / 3600.0;

            if (m > 0)
            {
                s = -Abs(azimuth - 180.0);
            }
            else
            {
                s = (azimuth > 180.0) ? azimuth - 360.0 : -1.0 * azimuth;
            }
            refractConst = Exp(-antHt1Aksl / 7.0);
            ko = Constant.GSORAD / (antHt1Aksl + Constant.ERTHRD1);


            /* check if station is too near the equator - 5 degrees */
            if (Abs(m) < Constant.SAFE_SEP)
            {
                errorCode = Constant.ORBIT_TOO_CLOSE_FAIL;
                return (errorCode);
            }

            /* check if elev angle is above the entire orbit */
            elevAngMax = AtanD(k / TanD(m / 2.0)) - (m / 2.0);
            if (elevAng > (elevAngMax + Constant.MIN_SEP))
            {
                return (Constant.ORBIT_EA_ABOVE_ORB_FAIL);
            }

            /* check if elev angle is below sea level */
            AxRefrac.AxRefalt(0.0, 400.0, refractConst, antHt1Aksl, out temp);

            AxRefrac.AxRefalt(antHt1Aksl, 400.0, refractConst, antHt1Aksl, out refractHt);

            elevAngMin = AcosD((temp / refractHt) * Constant.ERTHRD1 / (Constant.ERTHRD1 + antHt1Aksl));

            if (elevAng < (elevAngMin - Constant.MIN_SEP))
            {
                return (Constant.ORBIT_EA_BELOW_SL_FAIL);
            }

            /*	If the elevation angle is greater than 10 degrees or the latitude is
            *		greater than 60.0, then the model used below fails.  In this case we
            *		use the ITU calculations to return the angular separation from the
            *		refracted orbit. */
            if (elevAng > 10.0 || Abs(m) > 60.0)
            {
                double dAngSepMin = 2.0;        /*	This is the minimum acceptable angular
																*		separation.  Set to 2 for this case. */
                string cType;
                int nType;

                nRet = ITUangSep(m, dAngSepMin, s, elevAng, antHt1Aksl, 0.0,
                                                 out critAzimSt, out cType, out nType);
                if (nRet == 0)
                {
                    /*	We have a solution and the angular separation is in *critAzimSt. */
                    if (elevAng > 10)
                    {
                        errorCode = Constant.ORBIT_ELEV_ABOVE_10;
                    }
                    else
                    {
                        errorCode = Constant.ORBIT_LATITUDE_GT_60;
                    }
                    /*	Convert single critical Azimuth to a range.
                    *critAzimEnd = *critAzimSt + MIN_SEP;
                    *critAzimSt -= MIN_SEP;
        */
                }
                else
                {
                    /*	We could not complete the calculation.  */
                    errorCode = Constant.ORBIT_ELEV_ABOVE_10_ERR;
                }

                critAzimEnd = 0.0;

                critLongSt = 0.0;

                critLongEnd = 0.0;
            }
            else
            {

                /* calculate the reference points to see where the beam lies with */
                /*      respect to the ROT (refracted orbit trace) First calculate*/
                /*      so (unrefracted) and then ri400 and ri250 as necessary    */

                temp = AcosD(CosD(elevAng) / ko) - elevAng;
                so = -AcosD(TanD(m) / TanD(temp));

                /* calculate upper reference point - ri400 */
                errorCode = AxRefrac.AxTsorit(antHt1Aksl, s2eleva, distKm, so, 400.0,
                                                         refractConst, k, m, out ri400);
                if (errorCode < Constant.SUCCESS)
                {
                    return (errorCode);
                }

                if (s < ri400)
                {
                    /* the beam is above the ROT */
                    if (s < (ri400 - Constant.SAFE_SEP))
                    {
                        /* safe separation - no intersection */
                        errorCode = Constant.ORBIT_ABOVE_MAS5;
                    }
                    else
                    {
                        /* get the critical azimuth to check for isectn - sc2 */
                        errorCode = AxRefrac.AxAngElev(antHt1Aksl, s2eleva, distKm,
                                                                    400.0, refractConst, out angAssumeHor);

                        if (errorCode < Constant.SUCCESS)
                        {
                            return (errorCode);
                        }

                        /* calculate the critical azimuth sc2 for a desired */
                        /*      angular separation of MIN_SEP deg above ROT */
                        sc2 = ri400 - Sqrt(Pow(Constant.MIN_SEP, Constant.SQUARE) -

                                                             Pow(angAssumeHor - elevAng, Constant.SQUARE));

                        /* get mim angular separation of a beam at azimuth  */
                        /*      when the beam is above ROT. ri400 is the    */
                        /*      reference point                             */
                        minAngSep = Abs(AcosD(CosD(elevAng) * CosD(angAssumeHor) *
                                                                        CosD(ri400 - s) + SinD(elevAng) *
                                                                        SinD(angAssumeHor)));
                        if (s >= sc2)
                        {
                            /* it is an intersecion */
                            errorCode = AxRefrac.AxEirpSet(so, Constant.MIN_SEP, elevAng, refractConst, k, m,
                                                               antHt1Aksl, ref sc1, ref s, s1.LL.longSeconds / 3600.0,
                                                               azimuth, ref isectLong, ref isectLongSen, ref critLongSt,
                                                               ref critLongStSen, ref sc2, ref critLongEnd, ref critLongEndSen,
                                                               minAngSep, ref eirp1_10, ref eirp10_15, ref eirpGt15);

                            if (errorCode < Constant.SUCCESS)
                            {
                                return (errorCode);
                            }

                            /* intersects refr geo orbit (N=400). Min   */
                            /*      ang sep is minAngSep deg or less if */
                            /*      the radio horizon is below the      */
                            /*      assumed radio horizon               */
                            errorCode = Constant.ORBIT_INTERSECT_SET_MAS;
                        }
                        else
                        {
                            /* if > 3 there is no possibility of isect  */
                            /*      below the straight line horizon -   */
                            /*      points above teh geo orbit (N=400)  */
                            /* else they could intersect - points above */
                            /*      the refr geo orbit (N=400)          */
                            errorCode = (minAngSep > 3.0) ? Constant.ORBIT_ABOVE_GO : Constant.ORBIT_ABOVE_RGO;
                        }
                    }
                }
                else
                {

                    /* the azimuth is greater than the mim reference point ri400 */
                    /*      calculate the upper reference point ri250 to see     */
                    /*      if the beam is a direct hit or below the ROT         */
                    errorCode = AxRefrac.AxTsorit(antHt1Aksl, s2eleva, distKm, so, 250.0,
                                                             refractConst, k, m, out ri250);
                    if (errorCode < Constant.SUCCESS)
                    {
                        return (errorCode);
                    }
                    if (s <= ri250)
                    {

                        /* direct hit - get both crit. azimuths, calulate the*/
                        /*      crit azim sc2 for desired ang sep of MIN_SEP */
                        /*      degrees above the ROT                        */
                        errorCode = AxRefrac.AxAngElev(antHt1Aksl, s2eleva, distKm,
                                                                    250.0, refractConst, out angAssumeHor);

                        if (errorCode < Constant.SUCCESS)
                        {
                            return (errorCode);
                        }
                        sc2 = ri400 - Sqrt(Pow(Constant.MIN_SEP, Constant.SQUARE) -

                                                             Pow(angAssumeHor - elevAng, Constant.SQUARE));

                        minAngSep = 0.0;
                        errorCode = AxRefrac.AxEirpSet(so, Constant.MIN_SEP, elevAng, refractConst,
                                                                    k, m, antHt1Aksl, ref sc1, ref s,
                                                                    s1.LL.longSeconds / 3600.0, azimuth, ref isectLong,
                                                                    ref isectLongSen, ref critLongSt, ref critLongStSen, ref sc2,
                                                                    ref critLongEnd, ref critLongEndSen, minAngSep,
                                                                    ref eirp1_10, ref eirp10_15, ref eirpGt15);
                        if (errorCode < Constant.SUCCESS)
                        {
                            return (errorCode);
                        }

                        /* intersection of beam - Note ang sep for isectn   */
                        /*      is set at 1.5 deg for 1-15 GHz and 0 deg    */
                        /*      above 15 GHz                                */
                        errorCode = Constant.ORBIT_INTERSECT_RGO;
                    }
                    else
                    {
                        /* calculate the ang sep between the antenna beam   */
                        /*      and the refracted orbit trace when the beam */
                        /*      at azimuth is below the ROT                 */
                        AxRefrac.AxTsoTau(s, 250.0, refractConst, k, m, antHt1Aksl,
                                         out rotElevAng, out rotElevAngDer);

                        /* get the min ang dist to the antenna elevation by  */
                        /* calculating the perpendicular dist minAngSep from */
                        /*      the point rotElevAng on the ROT to the antena*/
                        /*      elevation angle elevAng                      */
                        wg = AtanD(rotElevAngDer / CosD(rotElevAng));
                        b = AsinD(SinD(wg) * CosD(rotElevAng) / CosD(elevAng));
                        azimDist = 2.0 * AtanD(SinD((rotElevAng - elevAng) /
                                                                             2.0) * TanD((wg + b) / 2.0) /

                                                                    CosD((rotElevAng + elevAng) / 2.0));

                        minAngSep = Abs(2.0 * AtanD(CosD((b - wg) / 2.0) *
                                                                                 TanD((rotElevAng - elevAng) / 2.0) /
                                                                                 CosD((b + wg) / 2.0)));
                        X2 = s;
                        Y2 = 0.0;
                        testpt = s - (azimDist / 3.0);
                        count = 0;
                        exitFlag = Constant.FALSE;
                        while (++count <= 10 && exitFlag == Constant.FALSE)
                        {

                            AxRefrac.AxTsoTau(testpt, 250.0, refractConst, k, m, antHt1Aksl, out rotElevAng, out rotElevAngDer);

                            /* get the min ang dist to the antenna elev */
                            /*      by finding the perpendicular dist   */
                            /*      minAngSep from rotElevAng on the ROT*/
                            /*      to the antena elev angle -  elevAng */
                            wg = AtanD(rotElevAngDer / CosD(rotElevAng));
                            b = AsinD(SinD(wg) * CosD(rotElevAng) / CosD(elevAng));
                            azimDist = 2.0 * AtanD(SinD((rotElevAng - elevAng) / 2.0) *
                                                                        TanD((wg + b) / 2.0) / CosD((rotElevAng +
                                                                                                                                elevAng) / 2.0));

                            minAngSep = Abs(2.0 * AtanD(CosD((b - wg) / 2.0) *
                                                                                     TanD((rotElevAng - elevAng) / 2.0) /
                                                                                     CosD((b + wg) / 2.0)));

                            if (Abs(testpt + azimDist - s) >= Constant.TOLERANCE)
                            {
                                X1 = X2;
                                Y1 = Y2;
                                X2 = testpt;
                                Y2 = testpt + azimDist - s;
                                if (Abs(X1 - X2) >= Constant.TOLERANCE)
                                {
                                    testpt = (X2 * Y1 - X1 * Y2) / (Y1 - Y2);
                                }
                                else
                                {
                                    exitFlag = Constant.TRUE;
                                }
                            }
                            else
                            {
                                exitFlag = Constant.TRUE;
                            }
                        } /* enddo */
                        if (exitFlag == Constant.FALSE)
                        {
                            /* no convergence during angular sep calc*/
                            minAngSep = 0.0;
                            return (Constant.ORBIT_AS_CONVERG_FAIL);
                        }

                        if (minAngSep <= Constant.SAFE_SEP)
                        {
                            /* below the orbit but a potential isectn */
                            if (minAngSep <= Constant.MIN_SEP)
                            {
                                /* intersection, calc the crit azims */
                                /*    calc the crit azim ac2 for a   */
                                /*    desired ang sep of MIN_SEP deg */
                                /*    above the ROT                  */
                                errorCode = AxRefrac.AxAngElev(antHt1Aksl, s2eleva, distKm, 400.0,
                                                                            refractConst, out angAssumeHor);

                                if (errorCode < Constant.SUCCESS)
                                {
                                    return (errorCode);
                                }
                                sc2 = ri400 - Sqrt(Pow(Constant.MIN_SEP, 2) - Pow(angAssumeHor - elevAng, 2));

                                errorCode = AxRefrac.AxEirpSet(so, Constant.MIN_SEP, elevAng, refractConst, k, m,
                                                                   antHt1Aksl, ref sc1, ref s,
                                                                   s1.LL.longSeconds / 3600.0,
                                                                   azimuth, ref isectLong, ref isectLongSen,
                                                                   ref critLongSt, ref critLongStSen, ref sc2, ref critLongEnd,
                                                                   ref critLongEndSen, minAngSep, ref eirp1_10,
                                                                   ref eirp10_15, ref eirpGt15);

                                if (errorCode < Constant.SUCCESS)
                                {
                                    return (errorCode);
                                }

                                /* intersection of the beam */
                                errorCode = Constant.ORBIT_BEAM_INTERSECT;
                            }
                            else
                            {

                                /* ang sep is greater than min sep */
                                /*    No intersection. Pts below   */
                                /*    the ref geo orbit (N=250)    */
                                errorCode = Constant.ORBIT_BELOW_SET_MAS;
                                sc1 = 0.0;
                                sc2 = 0.0;
                            }
                        }
                        else
                        {

                            /* ang sep is high and safe. Pts below the */
                            /*    refr geo orbit (N=250). Min ang sep  */
                            /*    is more than 5 deg                   */
                            errorCode = Constant.ORBIT_BELOW_MAS5;
                            sc1 = 0.0;
                            sc2 = 0.0;
                        }
                    }
                }

                critAzimSt = sc1;

                critAzimEnd = sc2;
            }

            return (errorCode);
        }

        /// <summary>
        /// This is a shim to call the ITU separation angle routine. It requires its 
        /// arguments in radians, we use degrees.  
        /// </summary>
        /// <param name="l"> - Latitude in degrees</param>
        /// <param name="b"> - Separation to be avoided</param>
        /// <param name="a0"> - Azimuth of antenna main beam</param>
        /// <param name="e0"> - Elevation of the antenna main beam</param>
        /// <param name="h0"> - Altitude of the station in km.</param>
        /// <param name="h1"> - Altitude of horizon in km.</param>
        /// <param name="psa"> - Output separation angle</param>
        /// <param name="pstrC"> - Output Type of separation</param>
        /// <param name="pkf"> - Output decision flag</param>
        /// <returns></returns>
        public static int ITUangSep(double l,       /*	Latitude in degrees */
                                    double b,       /*	Separation to be avoided */
                                    double a0,      /*	Azimuth of antenna main beam */
                                    double e0,      /*	Elevation of the antenna main beam */
                                    double h0,      /*	Altitude of the station in km.		 */
                                    double h1,      /*	Altitude of horizon in km.         */
                                out double psa,    /*	Output separation angle */
                                out string pstrC,   /*	Output Type of separation */
                                out int pkf)       /*	Output decision flag */
        {
            int nRet;

            nRet = Sangle.CalculationSub(l * Constant.DEG_TO_RAD,
                                  b * Constant.DEG_TO_RAD,
                                  Abs(a0 * Constant.DEG_TO_RAD),
                                  e0 * Constant.DEG_TO_RAD,
                                  h0,
                                  h1,
                              out psa,
                              out pstrC,
                              out pkf,
                                  0);

            psa *= Constant.RAD_TO_DEG; // /= Constant.DEG_TO_RAD;

            return nRet;
        }

        /// <summary>
        /// Writes the orbit report to the specified output file.  
        /// </summary>
        /// <param name="tw"></param>
        /// <param name="distKm"> - stn1 to stn2 distance in KM</param>
        /// <param name="azim"> - satellite azimuth in deg</param>
        /// <param name="elevAng"> - elevation angle from stn 1 to stn 2</param>
        /// <param name="insLong"> - intersection longitude</param>
        /// <param name="insLongSen"> - intersection longitude sense, E or W</param>
        /// <param name="minAngSep"> - minimum angular separation in deg</param>
        /// <param name="eirp1_10"> - maximum EIRP in dBW for 1 to 10 GHz</param>
        /// <param name="eirp10_15"> - maximum EIRP in dBW for 10 to 15 GHz</param>
        /// <param name="eirpGt15"> - maximum EIRP in dBW for above 15 GHz</param>
        /// <param name="critAzimSt"> - critical azimuth start in deg</param>
        /// <param name="critAzimEnd"> - critical azimuth end in deg</param>
        /// <param name="critLongSt"> - critical longitude start</param>
        /// <param name="critLongStSen"> - critical longitude start, E or W</param>
        /// <param name="critLongEnd"> - critical longitude end</param>
        /// <param name="critLongEndSen"> - critical longitude end sense, E or W</param>
        /// <param name="trans"> - flag indicating if the stations were transpsed</param>
        /// <param name="rptCode"> - orbit return code for type of result</param>
        /// <param name="trueVals"> - flag if true azim and elev entered</param>
        /// <param name="s1"> - structure of information for station 1</param>
        /// <param name="s2"> - structure of information for station 2 Each structure contains the location of the station in degrees, minutes, seconds, and sense (N, S, E, or W), as well as the station name, elevation above sea level, and antenna height</param>
        public static void AxRptOrb(TextWriter tw,          /* input - pointer to output file */
                                    double distKm,          /* input - stn1 to stn2 distance */
                                    double azim,            /* input - satellite azimuth */
                                    double elevAng,         /* input - stn1 to stn1 elev angle */
                                    double insLong,         /* input - intersection longitude */
                                    string insLongSen,      /* input - intersection longitude sense */
                                    double minAngSep,       /* input - minimum angular separation */
                                    double eirp1_10,        /* input - max EIRP for 1 to 10 GHz */
                                    double eirp10_15,       /* input - max EIRP for 10 to 15 GHz */
                                    double eirpGt15,        /* input - max EIRP for above 15 GHz */
                                    double critAzimSt,      /* input - critical azimuth start */
                                    double critAzimEnd,     /* input - critical azimuth end */
                                    double critLongSt,      /* input - critical longitude start */
                                    string critLongStSen,   /* input - critical longitude start sense */
                                    double critLongEnd,     /* input - critical longitude end */
                                    string critLongEndSen,  /* input - critical longitude end sense */
                                    int trans,              /* input - flag for transposed stations */
                                    int rptCode,            /* input - return code for type of result */
                                    int trueVals, 	        /* input - flag if true azim and elev entered*/

                                    AxStation s1,   /* input - station 1 data */
                                    AxStation s2)   /* input - station 2 data */
        {
            //...Log2.v("\nAxOrbitSupp.AxRptOrb(): Entry");

            tw.Write("\r\n\r\n\r\n                                ORBIT Program\r\n\r\n");
            tw.Write(" Name:                   {0}                    {1}\r\n",
                s1.name, s2.name);
            tw.Write(" Lat. (DMS):           {0:D2}  {1:D2}  {2:D2}  {3}               {4:D2}  {5:D2}  {6:D2} {7}\r\n",
                s1.LL.latDeg, s1.LL.latMin, s1.LL.latSec, s1.LL.latSens,
                s2.LL.latDeg, s2.LL.latMin, s2.LL.latSec, s2.LL.latSens);

            tw.Write(" Long. (DMS):         {0,3:D2}  {1:D2}  {2:D2}  {3}              {4,3:D2}  {5:D2}  {6:D2} {7}\r\n",
                s1.LL.longDeg, s1.LL.longMin, s1.LL.longSec,
                s1.LL.longSens, s2.LL.longDeg, s2.LL.longMin,
                s2.LL.longSec, s2.LL.longSens);

            tw.Write(" Grnd. Elev. (AMSL): {0,7:F2} M,  {1,7:F2} F\t {2,7:F2} M, {3,7:F2} F\r\n",
                s1.elevM, s1.elevM * Constant.METERS_TO_FT, s2.elevM,
                s2.elevM * Constant.METERS_TO_FT);

            tw.Write(" Antenna Ht. (AGL):  {0,7:F2} M,  {1,7:F2} F\t {2,7:F2} M, {3,7:F2} F\r\n\r\n",
                s1.antHtM, s1.antHtM * Constant.METERS_TO_FT, s2.antHtM,
                s2.antHtM * Constant.METERS_TO_FT);

            tw.Write(" Distance:           {0,7:F2} KM, {1,7:F2} MI\r\n", distKm,
                distKm * Constant.KM_TO_MILES);

            tw.Write(" Azimuth:            {0,7:F2} DEG\r\n", azim);

            tw.Write(" Elevation Angle:    {0,7:F2} DEG\r\n\r\n", elevAng);

            /* display case sepecific data depending on report code from ORBIT */
            switch (rptCode)
            {
                case Constant.ORBIT_TOO_CLOSE_FAIL:
                    tw.Write(" Notes:\r\n");
                    tw.Write("     Site too close to equator, not processed\r\n");
                    if (trans == Constant.TRUE)
                    {
                        tw.Write("     Stations transposed\r\n");
                    }
                    if (trueVals == Constant.TRUE)
                    {
                        tw.Write("     True Azimuth and Elevation used\r\n");
                    }
                    break;

                case Constant.ORBIT_EA_ABOVE_ORB_FAIL:
                    /* NO INTERSECTION:(1D) */
                    tw.Write(" Notes:\r\n");
                    tw.Write("     Elevation angle points above the ");
                    tw.Write("entire orbit\r\n");
                    tw.Write("     Check link elevation angle\r\n");
                    if (trans == Constant.TRUE)
                    {
                        tw.Write("     Stations transposed\r\n");
                    }
                    if (trueVals == Constant.TRUE)
                    {
                        tw.Write("     True Azimuth and Elevation used\r\n");
                    }
                    break;

                case Constant.ORBIT_EA_BELOW_SL_FAIL:
                    /* NO INTERSECTION:(1C) */
                    tw.Write(" Notes:\r\n");
                    tw.Write("     Elevation angle points below sea level ");
                    tw.Write("horizon\r\n");
                    tw.Write("     Check link elevation angle\r\n");
                    if (trans == Constant.TRUE)
                    {
                        tw.Write("     Stations transposed\r\n");
                    }
                    if (trueVals == Constant.TRUE)
                    {
                        tw.Write("     True Azimuth and Elevation used\r\n");
                    }
                    break;

                case Constant.ORBIT_AS_CONVERG_FAIL:
                    tw.Write(" Notes:\r\n");
                    tw.Write("     No convergence during angular ");
                    tw.Write("separation calculation\r\n");
                    if (trans == Constant.TRUE)
                    {
                        tw.Write("     Stations transposed\r\n");
                    }
                    if (trueVals == Constant.TRUE)
                    {
                        tw.Write("     True Azimuth and Elevation used\r\n");
                    }
                    break;

                case Constant.TSORIT_CONVERG_FAIL:
                    tw.Write(" Notes:\r\n");
                    tw.Write("     No convergence to reference point ");
                    tw.Write("for given refractivity\r\n");
                    if (trans == Constant.TRUE)
                    {
                        tw.Write("     Stations transposed\r\n");
                    }
                    if (trueVals == Constant.TRUE)
                    {
                        tw.Write("     True Azimuth and Elevation used\r\n");
                    }
                    break;

                case Constant.ANGELEV_IRC_FAIL:
                    tw.Write(" Notes:\r\n");
                    tw.Write("     Invalid refractivity constant\r\n");
                    if (trans == Constant.TRUE)
                    {
                        tw.Write("     Stations transposed\r\n");
                    }
                    if (trueVals == Constant.TRUE)
                    {
                        tw.Write("     True Azimuth and Elevation used\r\n");
                    }
                    break;

                case Constant.TSOSC1_CONVERG_FAIL:
                    tw.Write(" Notes:\r\n");
                    tw.Write("     No convergence while calculating ");
                    tw.Write("critical azimuth\r\n");
                    if (trans == Constant.TRUE)
                    {
                        tw.Write("     Stations transposed\r\n");
                    }
                    if (trueVals == Constant.TRUE)
                    {
                        tw.Write("     True Azimuth and Elevation used\r\n");
                    }
                    break;

                case Constant.ORBIT_ABOVE_MAS5:
                    /* O Constant.INTERSECTION:(1A) */
                    tw.Write(" Notes:\r\n");
                    tw.Write("     Points above the refracted ");
                    tw.Write("geostationary orbit (N=400)\r\n");
                    tw.Write("     Min angular separation is greater ");
                    tw.Write("than 5 deg\r\n");
                    if (trans == Constant.TRUE)
                    {
                        tw.Write("     Stations transposed\r\n");
                    }
                    if (trueVals == Constant.TRUE)
                    {
                        tw.Write("     True Azimuth and Elevation used\r\n");
                    }
                    break;

                case Constant.ORBIT_INTERSECT_SET_MAS:
                    tw.Write(" Intersection Long.:  {0,7:F2} {1}\r\n", insLong, insLongSen);

                    tw.Write(" Min Angular Sep:     {0,7:F2} DEG\r\n", minAngSep);

                    tw.Write(" Max EIRP (1-10 GHz): {0,7:F2} DBW\r\n", eirp1_10);

                    tw.Write("         (10-15 GHz): {0,7:F2} DBW\r\n", eirp10_15);

                    tw.Write("       (over 15 GHz): {0,7:F2} DBW\r\n", eirpGt15);

                    tw.Write(" Critical Azimuths:   {0,7:F2}   TO {1,7:F2} DEG\r\n", critAzimSt, critAzimEnd);

                    tw.Write(" Critical Long.'s:    {0,7:F2} {1} TO {2,7:F2} {3}\r\n\r\n", critLongSt, critLongStSen, critLongEnd, critLongEndSen);

                    /* INTERSECTION:(3A) */
                    tw.Write(" Notes:\r\n");
                    tw.Write("     Intersects the refracted geostationary ");
                    tw.Write("orbit (N=400)\r\n");
                    tw.Write("     Min Angular Sep could be less than ");
                    tw.Write("{0,7:F2} DEG if the\r\n\t actual radio horizon is ", minAngSep);
                    tw.Write("below the assumed radio horizon.\r\n");
                    tw.Write("     Angular separation for intersection is ");
                    tw.Write("set at 1.5 degrees \r\n\t for 1-15 GHZ ");
                    tw.Write("and zero degrees for above 15 GHZ.\r\n");

                    if (trans == Constant.TRUE)
                    {
                        tw.Write("     Stations transposed\r\n");
                    }
                    if (trueVals == Constant.TRUE)
                    {
                        tw.Write("     True Azimuth and Elevation used\r\n");
                    }

                    /************************************************************************\
                    *
                    *		Tell the user where to go to find the satellites that are within the
                    *		critical longitude area. 1205 - GJS - 2007.08.03
                    *		Changed to the database of satellites - GJS - 2007.10.17
                    *
                    \***********************************************************************
                    tw.Write("Go to http://www.lyngsat.com/ for a list of satellites between {0,5:F1}{0,1} and {0,5:F1}{0}\r\n",
                                    critLongSt, critLongStSen, critLongEnd, critLongEndSen);*/
                    Ssutil.ShowSats(tw,
                                     Ssutil.GetLong(critLongSt, critLongStSen),

                                     Ssutil.GetLong(critLongEnd, critLongEndSen));
                    break;

                case Constant.ORBIT_ABOVE_GO:
                    tw.Write(" Min Angular Sep:     {0,7:F2} DEG\r\n\r\n",
                        minAngSep);
                    /* NO INTERSECION:(2A1) */
                    tw.Write(" Notes:\r\n");
                    tw.Write("     Points above the geostationary orbit ");
                    tw.Write("(N=400)\r\n");
                    tw.Write("     Min Angular Sep could be less than ");
                    tw.Write("{0,7:F2} DEG if the\r\n\t actual radio horizon is ", minAngSep);
                    tw.Write("below the assumed radio horizon.\r\n");
                    if (trans == Constant.TRUE)
                    {
                        tw.Write("     Stations transposed\r\n");
                    }
                    if (trueVals == Constant.TRUE)
                    {
                        tw.Write("     True Azimuth and Elevation used\r\n");
                    }
                    break;

                case Constant.ORBIT_ABOVE_RGO:
                    tw.Write(" Min Angular Sep:     {0,7:F2} DEG\r\n\r\n",
                        minAngSep);
                    /* NO INTERSECION:(2A2) */
                    tw.Write(" Notes:\r\n");
                    tw.Write("     Points above the refracted geostationary orbit ");
                    tw.Write("(N=400)\r\n");
                    tw.Write("     Min Angular Sep could be less than ");
                    tw.Write("{0,7:F2} DEG if the\r\n\t actual radio horizon is ", minAngSep);
                    tw.Write("below the assumed radio horizon.\r\n");
                    if (trans == Constant.TRUE)
                    {
                        tw.Write("     Stations transposed\r\n");
                    }
                    if (trueVals == Constant.TRUE)
                    {
                        tw.Write("     True Azimuth and Elevation used\r\n");
                    }
                    break;

                case Constant.ORBIT_INTERSECT_RGO:
                    tw.Write(" Intersection Long.:  {0,7:F2} {1}\r\n", insLong, insLongSen);

                    tw.Write(" Min Angular Sep:     {0,7:F2} DEG\r\n", minAngSep);

                    tw.Write(" Max EIRP (1-10 GHz): {0,7:F2} DBW\r\n", eirp1_10);

                    tw.Write("         (10-15 GHz): {0,7:F2} DBW\r\n", eirp10_15);

                    tw.Write("       (over 15 GHz): {0,7:F2} DBW\r\n", eirpGt15);

                    tw.Write(" Critical Azimuths:   {0,7:F2}   TO {1,7:F2} DEG\r\n", critAzimSt, critAzimEnd);

                    tw.Write(" Critical Long.'s:    {0,7:F2} {1} TO {2,7:F2} {3}\r\n\r\n", critLongSt, critLongStSen, critLongEnd, critLongEndSen);

                    /* NTERSECTION:(3C)\r\n"); */
                    tw.Write(" Notes:\r\n");
                    tw.Write("     Intersects the refracted geostationary ");
                    tw.Write("orbit\r\n");
                    tw.Write("     Angular separation for intersection is ");
                    tw.Write("set at 1.5 degrees \r\n       for 1-15 GHZ ");
                    tw.Write("and zero degrees for above 15 GHZ.\r\n");

                    if (trans == Constant.TRUE)
                    {
                        tw.Write("     Stations transposed\r\n");
                    }
                    if (trueVals == Constant.TRUE)
                    {
                        tw.Write("     True Azimuth and Elevation used\r\n");
                    }
                    /************************************************************************\
                    *
                    *		Tell the user where to go to find the satellites that are withing the
                    *		critical longitude area. 1205 - GJS - 2007.08.03
                    *		Changed to the database of satellites - GJS - 2007.10.17
                    *
                    \***********************************************************************
                    tw.Write("Go to http://www.lyngsat.com/ for a list of satellites between {0,5:F1}{0,1} and {0,5:F1}{0}\r\n",
                                    critLongSt, critLongStSen, critLongEnd, critLongEndSen);*/
                    Ssutil.ShowSats(tw,
                                     Ssutil.GetLong(critLongSt, critLongStSen),
                                     Ssutil.GetLong(critLongEnd, critLongEndSen));
                    break;

                case Constant.ORBIT_BEAM_INTERSECT:
                    tw.Write(" Intersection Long.:  {0,7:F2} {1}\r\n",
                        insLong, insLongSen);
                    tw.Write(" Min Angular Sep:     {0,7:F2} DEG\r\n",
                        minAngSep);
                    tw.Write(" Max EIRP (1-10 GHz): {0,7:F2} DBW\r\n",
                        eirp1_10);
                    tw.Write("         (10-15 GHz): {0,7:F2} DBW\r\n",
                        eirp10_15);
                    tw.Write("       (over 15 GHz): {0,7:F2} DBW\r\n",
                        eirpGt15);
                    tw.Write(" Critical Azimuths:   {0,7:F2}   TO {1,7:F2} DEG\r\n", critAzimSt,
                        critAzimEnd);
                    tw.Write(" Critical Long.'s:    {0,7:F2} {1} TO {2,7:F2} {3}\r\n\r\n", critLongSt,
                        critLongStSen, critLongEnd, critLongEndSen);

                    /* NTERSECTION:(3B)\r\n"); */
                    tw.Write(" Notes:\r\n");
                    tw.Write("     Intersects the refracted geostationary ");
                    tw.Write("orbit (N=250)\r\n");
                    tw.Write("     Angular separation for intersection is ");
                    tw.Write("set at 1.5 degrees \r\n\t for 1-15 GHZ ");
                    tw.Write("and zero degrees for above 15 GHZ.\r\n");

                    if (trans == Constant.TRUE)
                    {
                        tw.Write("     Stations transposed\r\n");
                    }
                    if (trueVals == Constant.TRUE)
                    {
                        tw.Write("     True Azimuth and Elevation used\r\n");
                    }
                    /************************************************************************\
                    *
                    *		Tell the user where to go to find the satellites that are withing the
                    *		critical longitude area. 1205 - GJS - 2007.08.03
                    *		Changed to the database of satellites - GJS - 2007.10.17
                    *
                    \***********************************************************************
                    tw.Write("Go to http://www.lyngsat.com/ for a list of satellites between {0,5:F1}{0,1} and {0,5:F1}{0}\r\n",
                                    critLongSt, critLongStSen, critLongEnd, critLongEndSen);*/
                    Ssutil.ShowSats(tw,
                                     Ssutil.GetLong(critLongSt, critLongStSen),

                                     Ssutil.GetLong(critLongEnd, critLongEndSen));
                    break;

                case Constant.ORBIT_BELOW_SET_MAS:
                    tw.Write(" Min Angular Sep:    {0,7:F2} DEG\r\n\r\n",
                        minAngSep);
                    /* NO INTERSECTION:(2B) */
                    tw.Write(" Notes:\r\n");
                    tw.Write("     Points below the refracted geostationary ");
                    tw.Write("orbit (N=250)\r\n");
                    if (trans == Constant.TRUE)
                    {
                        tw.Write("     Stations transposed\r\n");
                    }
                    if (trueVals == Constant.TRUE)
                    {
                        tw.Write("     True Azimuth and Elevation used\r\n");
                    }
                    break;

                case Constant.ORBIT_BELOW_MAS5:
                    /* NO INTERSECTION:(1B) */
                    tw.Write(" Notes:\r\n");
                    tw.Write("     Points below the refracted ");
                    tw.Write("geostationary orbit (N=250)\r\n");
                    tw.Write("     Min angular separation is greater ");
                    tw.Write("than 5 deg\r\n");
                    if (trans == Constant.TRUE)
                    {
                        tw.Write("     Stations transposed\r\n");
                    }
                    if (trueVals == Constant.TRUE)
                    {
                        tw.Write("     True Azimuth and Elevation used\r\n");
                    }
                    break;

                case Constant.ORBIT_ELEV_ABOVE_10:
                case Constant.ORBIT_LATITUDE_GT_60:
                    /*	Out of range for our algorithm.  ITU algortithm used. */
                    if (rptCode == Constant.ORBIT_ELEV_ABOVE_10)
                    {
                        tw.Write(" Antenna elevation angle is greater than 10 degrees.\r\n");
                    }
                    else
                    {
                        tw.Write(" Latitude is greater than 60 degrees.\r\n");
                    }
                    tw.Write(" ITU algorithm used.\r\n\r\n");
                    tw.Write(" Minimum Angle of Separation between antenna beam and the ROT is: {0,5:F2} degrees.\r\n", critAzimSt);
                    if (trans == Constant.TRUE)
                    {
                        tw.Write("     Stations transposed\r\n");
                    }
                    if (trueVals == Constant.TRUE)
                    {
                        tw.Write("     True Azimuth and Elevation used\r\n");
                    }
                    break;

                case Constant.ORBIT_ELEV_ABOVE_10_ERR:
                    /*	Specific error in the ITU program  */
                    tw.Write(" Error in the ITU Angular Separation Program.\r\n");
                    break;

                default:
                    tw.Write(" Error number {0} occurred\r\n", rptCode);
                    break;
            }

            //...Log2.v("\nAxOrbitSupp.AxRptOrb(): Exit, final.");
        }








    }
}
