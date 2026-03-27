using _Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Math;
using static _NewLib.Maths;

namespace _Auxlib
{
    /// <summary>
    /// Provides methods that calculate a variety of refraction 
    /// parameters and effects.
    /// </summary>
    public class AxRefrac
    {
        /// <summary>
        /// Calculates the refractivity at a given altitude (height) for a 
        /// given refractivity (refractInd).  
        /// </summary>
        /// <param name="height"> - height at which refract is being calcd</param>
        /// <param name="refractInd"> - refractivity index</param>
        /// <param name="refractConst"> - refractivity constant</param>
        /// <param name="antHt1Aksl"> - height of stn1 antenna above sea level</param>
        /// <param name="refractHt"> - calculated refractivity</param>
        public static void AxRefalt(double height,        /* input  - height for refract calc */
                                    double refractInd,    /* input  - refractivity index */
                                    double refractConst,  /* input  - refractivity constant */
                                    double antHt1Aksl,    /* input  - height of stn1 ante */
                                out double refractHt)     /* output - calcd refractivity */
        {
            double ens;         /* real valued calculation variable */
            double deltan;      /* real valued calculation variable */
            double co;          /* real valued calculation variable */

            ens = refractInd * refractConst;

            deltan = -7.32 * Exp(0.005577 * ens);

            co = Log(ens / (ens + deltan));

            refractHt = 1.0 + ((ens * 0.000001) / Exp(co * (height - antHt1Aksl)));
        }

        /// <summary>
        /// Calculates the reference point AZIMUTH which represents the 
        /// intersection of the ROT (Refracted Orbit Trace) with the assumed radio 
        /// horizon using refractivity index refractInd. A Newton Raphson iteration is 
        /// used and 'so' is the starting point.  
        /// </summary>
        /// <param name="antHt1Aksl"> - height of stn1 antenna above sea level</param>
        /// <param name="antHt2Aksl"> - height of stn2 antenna above sea level</param>
        /// <param name="distKm"> - Great circle distance between stns, KM</param>
        /// <param name="so"> - isectn of elev w/ the ROT, no refract.</param>
        /// <param name="refractInd"> - refractivity index</param>
        /// <param name="refractConst"> - refractivity constant</param>
        /// <param name="k"> - constant based on radius of geo orbit, radius of the earth and height of the transmit ante</param>
        /// <param name="m"> - stn1 latitude</param>
        /// <param name="azimuth"> - azimuth ie calculated reference point</param>
        /// <returns></returns>
        public static int AxTsorit(double antHt1Aksl,      /* input  - stn1 antenna height */
                                    double antHt2Aksl,     /* input  - stn2 antenna height */
                                    double distKm,         /* input  - great circle dist between stns */
                                    double so,             /* input  - isectn of elev with the ROT */
                                    double refractInd,     /* input  - refractivity index */
                                    double refractConst,   /* input  - refractivity constant */
                                    double k,              /* input  - constant */
                                    double m,              /* input  - stn1 latitude */
                                out double azimuth)        /* output - azim calcd at refrence pt */
        {
            // 'out' requirement.
            azimuth = 0.0;

            int errorCode,  /* return code variable */
                count;      /* loop control variable */

            double angAssumeHor,    /* elev ang of assumed radio horizon  */
                rotElevAng, /* ROT elevation angle */
                rotElevAngDer;  /* derivative of ROT elevation angle */


            errorCode = AxAngElev(antHt1Aksl, antHt2Aksl, distKm, refractInd,
                                                        refractConst, out angAssumeHor);
            if (errorCode < Constant.SUCCESS)
            {
                return (errorCode);
            }

            azimuth = so;
            for (count = 1; count <= 10; count++)
            {
                AxTsoTau(azimuth, refractInd, refractConst, k, m, antHt1Aksl, out rotElevAng,
                                 out rotElevAngDer);
                if (Abs(rotElevAng - angAssumeHor) < Constant.TOLERANCE)
                {
                    return (Constant.SUCCESS);
                }
                else
                {
                    azimuth = azimuth - ((rotElevAng - angAssumeHor) / rotElevAngDer);
                }
            }
            return (Constant.TSORIT_CONVERG_FAIL);
        }

        /// <summary>
        /// This method calculated the antenna elevation of the assumed radio horizon. 
        ///  
        /// </summary>
        /// <param name="antHt1Aksl"> - height of stn1 antenna above sea level</param>
        /// <param name="antHt2Aksl"> - height of stn2 antenna above sea level</param>
        /// <param name="distKm"> - Great circle distance between stns, KM</param>
        /// <param name="refractInd"> - refractivity index</param>
        /// <param name="refractConst"> - refractivity constant</param>
        /// <param name="angAssumeHor"> - elevation angle of assumed radio horzn</param>
        /// <returns></returns>
        public static int AxAngElev(double antHt1Aksl,    /* input  - ht of stn1 antenna */
                                    double antHt2Aksl,    /* input  - ht of stn2 antenna */
                                    double distKm,        /* input  - great circle dist */
                                    double refractInd,    /* input  - refractivity index */
                                    double refractConst,  /* input  - refractivity constant */
                                out double angAssumeHor)  /* output - elev ang of assumed horzn */
        {
            // 'out' requirement.
            angAssumeHor = 0.0;

            double refractHt1;       /* stn1 ant elev of assumed radio hor*/
            double refractHt2;       /* stn2 ant elev of assumed radio hor*/
            double distGraze;        /* angular dist along graze surface  */
            double distAng;          /* angular dist between sites */
            double ko;               /* working var for refractInd */


            /* there are two possible equations to use 1) if the beam is*/
            /*        shooting down (horizon is less than height) and the */
            /*        receiver is in front of the graze, 2) otherwise */
            if (antHt1Aksl > antHt2Aksl)
            {
                AxRefalt(antHt1Aksl, refractInd, refractConst, antHt1Aksl,
                    out refractHt1);

                AxRefalt(antHt2Aksl, refractInd, refractConst, antHt1Aksl,
                    out refractHt2);

                angAssumeHor = AcosD((refractHt2 / refractHt1) *
                    ((Constant.ERTHRD + antHt2Aksl) / (Constant.ERTHRD + antHt1Aksl)));

                distGraze = 2.0 * AtanD((antHt1Aksl - antHt2Aksl) /
                    (TanD(angAssumeHor / 2.0) * (2.0 * Constant.ERTHRD +
                    antHt1Aksl + antHt2Aksl)));

                distAng = (distKm / Constant.ERTHRD) * Constant.RAD_TO_DEG;

                /* check to see if it is in front of the graze */
                if (distAng < distGraze)
                {
                    angAssumeHor = -1.0 * angAssumeHor;
                }
                else
                {
                    if (refractInd == 250.0)
                    {
                        ko = 1.25016;
                    }
                    else if (refractInd == 400.0)
                    {
                        ko = 1.90765;
                    }
                    else
                    {
                        return (Constant.ANGELEV_IRC_FAIL);
                    }
                    angAssumeHor = AtanD((antHt2Aksl - antHt1Aksl) /
                        ((2.0 * ko * Constant.ERTHRD + antHt2Aksl + antHt1Aksl) *
                         Tan(distKm / (2.0 * ko * Constant.ERTHRD)))) -
                        (distKm / (2.0 * ko * Constant.ERTHRD)) * Constant.RAD_TO_DEG;
                }
            }
            else
            {
                if (refractInd == 250.0)
                {
                    ko = 1.25016;
                }
                else if (refractInd == 400.0)
                {
                    ko = 1.90765;
                }
                else
                {
                    return (Constant.ANGELEV_IRC_FAIL);
                }

                angAssumeHor = AtanD((antHt2Aksl - antHt1Aksl) /
                    ((2.0 * ko * Constant.ERTHRD + antHt2Aksl + antHt1Aksl) *
                    Tan(distKm / (2.0 * ko * Constant.ERTHRD)))) -
                    (distKm / (2.0 * ko * Constant.ERTHRD)) * Constant.RAD_TO_DEG;
            }

            return (Constant.SUCCESS);
        }

        /// <summary>
        /// Calculates the elevation angle of the refracted orbit trace 
        /// (ROT) for a given refractivity (refractInd) at a given azimuth. The 
        /// derivative of the elevation is also calcd.  
        /// </summary>
        /// <param name="azimuth"> - azimuth</param>
        /// <param name="refractInd"> - refractivity index</param>
        /// <param name="refractConst"> - refractivity constant</param>
        /// <param name="k"> - constant based on radius of geo orbit, radius of earth, and height of the transmitting antenna</param>
        /// <param name="m"> - stn 1 (transmitter) latitude in deg</param>
        /// <param name="antHt1Aksl"> - height of stn1 antenna above sea level</param>
        /// <param name="rotElevAng"> - ROT elevation angle</param>
        /// <param name="rotElevAngDer"> - derivative of ROT elevation angle</param>
        public static void AxTsoTau(double azimuth,         /* input  - azimuth */
                                    double refractInd,      /* input  - refractivity index */
                                    double refractConst,    /* input  - refractivity constant */
                                    double k,               /* input  - constant */
                                    double m,               /* input  - stn1 latitude */
                                    double antHt1Aksl,      /* input  - ht of stn1 ante */
                                out double rotElevAng,      /* output - ROT elevation angle */
                                out double rotElevAngDer)   /* output - derivative of ROT elev */
        {

            double deps;   /* calculation variable */
            double b;      /* calculation variable */
            double afact;  /* calculation variable */
            double emin;   /* calculation variable */
            double eps1;   /* elev angle of the GOT for given lower azim*/
            double tau1;   /* refractive bending lower bounding value   */
            double dtau1;  /* derivative of tau1 */
            double eps2;   /* elev ang of the GOT for a given upper azim*/
            double tau2;   /* refractive bending upper bounding value   */
            double dtau2;  /* derivative of tau2 */
            double eps;    /* elevation angle of the GOT for given azim */
            double tau;    /* refractive bending */
            double dtau;   /* derivative of tau */


            b = AtanD(TanD(m) / CosD(azimuth));

            eps = AtanD(k / TanD(b / 2.0)) - (b / 2.0);

            deps = -SinD(2.0 * b) * TanD(azimuth) * 0.25 * (1.0 + (2.0 * k) /
                (1.0 + Pow(k, Constant.SQUARE) - CosD(b) + Pow(k, Constant.SQUARE) * CosD(b)));

            /* using EPS get preliminary values for ROT elev ang and its deriv */
            afact = (refractInd / 400.0) * ((1743.0 + refractInd) / 2143.0) /
                Exp((antHt1Aksl / 7.0) * Pow(refractInd / 176.0, 0.2));

            /* the approximation is in several segments */
            if (eps < 0.0)
            {
                /* calculate the boundary for the below 0 approx */
                eps1 = -2.5;
                tau1 = 2.069 * afact * Pow((400.0 / refractInd), 0.36);
                dtau1 = -0.95 * afact * Pow(1.198, (400.0 - refractInd) / 50.0);
                eps2 = 0.0;
                tau2 = 0.796 * afact;
                dtau2 = -0.275 * afact;

                /* calculate the EMIN below which the approx is linear */
                emin = -2.51 - ((refractInd - 41.837) *
                    (0.173469 + antHt1Aksl) / 1530.61);

                if (eps < emin)
                {

                    /* linear - calc EMIN value and slope and use that */
                    AxTsot1(eps1, tau1, dtau1, eps2, tau2, dtau2, Constant.EPSMIN,
                        out tau, out dtau);
                    dtau = dtau1;
                    tau = tau + dtau * (eps - emin);
                }
                else
                {
                    AxTsot1(eps1, tau1, dtau1, eps2, tau2, dtau2, eps,
                        out tau, out dtau);
                }
            }
            else if (eps <= 3.0)
            {

                /* use eqns 5.24 and 5.25 in engineering documentation */
                AxTsot1(0.0, 0.796 * afact, -0.275 * afact, 3.0, 0.352 * afact,
                    -0.08 * afact, eps, out tau, out dtau);
            }
            else if (eps <= 10.0)
            {

                /* use eqns 5.26 and 5.27 */
                AxTsot1(3.0, 0.352 * afact, -0.08 * afact, 10.0, 0.0003249405 *
                refractInd * refractConst, 0.00003316344 * refractInd *
                refractConst, eps, out tau, out dtau);
            }
            else
            {

                /* greater than 10 deg, use the cotangent approximation */
                tau = 0.00005729578 * refractInd * refractConst / TanD(eps);
                dtau = -0.000001 * refractInd * refractConst /
                Pow(SinD(eps), Constant.SQUARE);
            }

            /* calc final values for ROT elev ang and its derivative */
            rotElevAng = eps + tau;
            rotElevAngDer = (1.0 + dtau) * deps;
        }

        /// <summary>
        /// This method calculated the exponential approximation to the TAU function.  
        /// </summary>
        /// <param name="eps1"> - elev angle of the GOT for a given lower azim</param>
        /// <param name="tau1"> - refractive bending lower bounding value</param>
        /// <param name="dtau1"> - derivative of tau1</param>
        /// <param name="eps2"> - elev ang of the GOT for a given upper azim</param>
        /// <param name="tau2"> - refractive bending upper bounding value</param>
        /// <param name="dtau2"> - derivative of tau2</param>
        /// <param name="eps"> - elevation angle of the GOT for a given azim</param>
        /// <param name="tau"> - refractive bending</param>
        /// <param name="dtau"> - derivative of tau</param>
        public static void AxTsot1(double eps1,    /* input  - elev ang of GOT for upper azim */
                                    double tau1,   /* input  - refract. bending lower bound */
                                    double dtau1,  /* input  - derivative of tau1 */
                                    double eps2,   /* input  - elev ang of GOT for lower azim */
                                    double tau2,   /* input  - refract. bending upper bound */
                                    double dtau2,  /* input  - derivative of tau2 */
                                    double eps,    /* input  - elev ang of GOT for given azim */
                                out double tau,    /* output - refractive bending */
                                out double dtau)   /* output - derivative of tau */
        {
            double a, b, c, d;  /* real valued calculated coefficients */

            a = (1.0 / Pow(eps2 - eps1, 2)) * ((dtau1 / tau1) + (dtau2 / tau2) -
                    (2.0 / Pow((eps2 - eps1), 3)) * Log(tau2 / tau1));

            b = (1.0 / ((eps2 - eps1) * 2.0)) * ((dtau2 / tau2) - (dtau1 / tau1)) -
                  (1.5 * (eps1 + eps2) * a);

            c = (dtau1 / tau1) - (3.0 * a * Pow(eps1, 2)) - (2.0 * b * eps1);

            d = Log(tau1) - a * Pow(eps1, 3) - (b * Pow(eps1, 2)) -
                (c * eps1);

            tau = Exp(((a * eps + b) * eps + c) * eps + d);
            dtau = ((3.0 * a * eps + 2.0 * b) * eps + c) * tau;
        }

        /// <summary>
        /// Calculates critical ray parameters and maximum Effective
        /// Isotropic Radiated Power (EIRP) levels over three microwave bands: 
        /// 1-10 GHz, 10-15 GHz, and 15 GHz and above.
        /// </summary>
        /// <param name="so"> - azim of the unrefracted beam wrt ROT</param>
        /// <param name="minSep"> - min separation in deg</param>
        /// <param name="elevAng"> - antenna elevation angle</param>
        /// <param name="refractConst"> - refractivity constant</param>
        /// <param name="k"> - constant based on radius of geo orbit, radius of earth, and height of transmit ante</param>
        /// <param name="m"> - stn 1 (transmitter) latitude in deg</param>
        /// <param name="antHt1Aksl"> - height of stn1 antenna above sea level</param>
        /// <param name="sc1"> - working var for critical azim start</param>
        /// <param name="s"> - transmit azimuth from South</param>
        /// <param name="s1LongDeg"> - stn 1 longitude in deg</param>
        /// <param name="azim"> - azimuth</param>
        /// <param name="isectLong"> - intersection longitude</param>
        /// <param name="isectLongSen"> - intersection longitude sense, E or W</param>
        /// <param name="critLongSt"> - critical longitude start</param>
        /// <param name="critLongStSen"> - critical longitude start sens, E or W</param>
        /// <param name="sc2"> - working var for critical azim end</param>
        /// <param name="critLongEnd"> - critical longitude end</param>
        /// <param name="critLongEndSen"> - critical longitude end sens, E or W</param>
        /// <param name="minAngSep"> - minimum angular separation</param>
        /// <param name="eirp1_10"> - max EIRP for 1 to 10 GHz</param>
        /// <param name="eirp10_15"> - max EIRP for 10 to 15 GHz</param>
        /// <param name="eirpGt15"> - max EIRP for more than 15 GHz</param>
        /// <returns></returns>
        public static int AxEirpSet(double so,              /* input  - azim of unrefracted beam */
                                    double minSep,          /* input  - minimum separation */
                                    double elevAng,         /* input  - ante elev angle */
                                    double refractConst,    /* input  - refractivity const */
                                    double k,               /* input  - constant */
                                    double m,               /* input  - stn1 latitude */
                                    double antHt1Aksl,      /* input  - stn1 ante ht */
                                ref double sc1,             /* output - critical azim start */
                                ref double s,               /* output - transmit azim from South */
                                    double s1LongDeg,       /* input  - stn1 longitude */
                                    double azim,            /* input  - azimuth */
                                ref double isectLong,       /* output - intersection longitude */
                                ref string isectLongSen,    /* output - intersection long sense */
                                ref double critLongSt,      /* output - critical long start */
                                ref string critLongStSen,   /* output - crit. lont. start sense */
                                ref double sc2,             /* output - critical azim end */
                                ref double critLongEnd,     /* output - critical long end */
                                ref string critLongEndSen,  /* output - critical long end sense */
                                double minAngSep,           /* input  - minimum angular sep */
                                ref double eirp1_10,        /* output - max EIRP for 1 to 10 GHz */
                                ref double eirp10_15,       /* output - max EIRP for 10 to 15 GHz */
                                ref double eirpGt15)        /* output - max EIRP for above 15 GHz */
        {
            // 'out' requirements.
            //sc1 = 0.0;
            //s = 0.0;
            //s1LongDeg = 0.0;
            //isectLong = 0.0;
            //critLongSt = 0.0;
            //sc2 = 0.0;
            //critLongEnd = 0.0;
            //eirp1_10 = 0.0;
            //eirp10_15 = 0.0;
            //eirpGt15 = 0.0;
            //isectLongSen = "";
            //critLongStSen = "";
            //critLongEndSen = "";

            int errorCode;      /* return code variable */

            errorCode = AxTsoSc1(so, minSep, elevAng, refractConst, k, m, antHt1Aksl, out sc1);
            if (errorCode < Constant.SUCCESS)
            {
                return (errorCode);
            }

            AxTsoilg(s, m, s1LongDeg, azim, out isectLong, out isectLongSen, out s);
            AxTsoilg(sc1, m, s1LongDeg, azim, out critLongSt, out critLongStSen, out sc1);
            AxTsoilg(sc2, m, s1LongDeg, azim, out critLongEnd, out critLongEndSen, out sc2);

            if (minAngSep < 0.5)
            {
                eirp1_10 = 47.0;
                eirp10_15 = 45.0;
            }
            else if (minAngSep < 1.5)
            {
                eirp1_10 = 47.0 + ((minAngSep - 0.5) * 8.0);
                eirp10_15 = 45.0;
            }
            else
            {
                eirp1_10 = 55.0;
                eirp10_15 = 55.0;
            }
            eirpGt15 = 55.0;

            return (Constant.SUCCESS);
        }

        /// <summary>
        /// This method calculated the critical azimuth of a beam with angular 
        /// separation of MIN_SEP deg and below the GSO (geosync. orbit).  
        /// </summary>
        /// <param name="so"> - azim of the unrefracted beam wrt ROT</param>
        /// <param name="minSep"> - min separation in deg</param>
        /// <param name="elevAng"> - antenna elevation angle</param>
        /// <param name="refractConst"> - refractivity constant</param>
        /// <param name="k"> - constant based on radius of geo orbit, radius of earth, and height of the transmitting antenna</param>
        /// <param name="m"> - stn 1 (transmitter) latitude in deg</param>
        /// <param name="antHt1Aksl"> - height of stn1 antenna above sea level</param>
        /// <param name="azimuth"> - calculated critical azimuth</param>
        /// <returns></returns>
        private static int AxTsoSc1(double so,          /* input  - azim of unrefracted beam */
                                    double minSep,      /* input  - min separation */
                                    double elevAng,     /* input  - antenna elevation angle */
                                    double refractConst,/* input - refractivity constant */
                                    double k,           /* input  - constant */
                                    double m,           /* input  - stn1 latitude */
                                    double antHt1Aksl,  /* input  - height of stn1 antenna */
                                out double azimuth)     /* output - calculated critical azimuth */
        {
            int count;      /* loop control variable */

            double piovr2 = 90.0,   /* constant */
                            distOff,    /* distance off azimuth */
                            wg,     /* real calculation variable */
                            q1,     /* real calculation variable */
                            q2,     /* real calculation variable */
                            ev,     /* real calculation variable */
                            dev,        /* real calculation variable */
                            rotElevAngDer,  /* derivative of ROT elev angle */
                            rotElevAng, /* ROT elevation angle */
                            temp1,      /* real valued calculation variable */
                            temp2;      /* real valued calculation variable */


            distOff = so + minSep;
            for (count = 1; count <= 10; count++)
            {
                AxTsoTau(distOff, 250.0, refractConst, k, m, antHt1Aksl,
                                 out rotElevAng, out rotElevAngDer);

                temp1 = (piovr2 - rotElevAng - minSep) / 2.0;
                temp2 = (piovr2 - rotElevAng + minSep) / 2.0;

                wg = AtanD(rotElevAngDer / CosD(rotElevAng));
                q1 = AtanD(TanD(wg / 2.0) * SinD(temp1) / SinD(temp2));
                q2 = AtanD(TanD(wg / 2.0) * CosD(temp1) / CosD(temp2));
                ev = piovr2 - 2.0 * AtanD(CosD(q2) * TanD(temp2) / CosD(q1));
                dev = CosD(rotElevAng) * TanD(q1 + q2);

                azimuth = distOff + q2 - q1;
                if (Abs(ev - elevAng) <= Constant.TOLERANCE)
                {
                    return (Constant.SUCCESS);
                }
                azimuth = azimuth - ((ev - elevAng) / dev);
                distOff = azimuth - q2 + q1;
            }
            azimuth = 0.0;

            return (Constant.TSOSC1_CONVERG_FAIL);
        }

        /// <summary>
        /// Calculates the longitude of intersection on the GSO for a 
        /// given antenna azimuth.  
        /// </summary>
        /// <param name="azimuth"> - azimuth</param>
        /// <param name="m"> - stn1 (transmitter) latitude in deg</param>
        /// <param name="sDeg"> - stn1 (transmitter) longitude in deg</param>
        /// <param name="azim"> - input azimuth</param>
        /// <param name="intersect"> - intersection longitude</param>
        /// <param name="intersectSense"> - intersection long sense</param>
        /// <param name="azimuthTrue"> - true azimuth</param>
        private static void AxTsoilg(double azimuth,            /* input  - azimuth */
                                        double m,               /* input  - stn1 latitude */
                                        double sDeg,            /* input  - stn1 longitude */
                                        double azim,            /* input  - input azimuth */
                                    out double intersect,       /* output - intersection longitude */
                                    out string intersectSense,  /* output - intersection long sense */
                                    out double azimuthTrue)     /* output - true azimuth */
        {
            // 'out' requirements.
            intersect = 0.0;
            intersectSense = "";
            azimuthTrue = 0.0;

            double temp;            /* temporary working variable */

            /* the AZIMUTH used here is from south right to left */
            if (m > 0.0)
            {
                /* if northern hemishpere, point from 90 to 270 */
                /* if southern hemisphere, point from 270 to 370, 0 to 90 */
                azimuthTrue = (azim > 180.0) ? 180.0 - azimuth : 180.0 + azimuth;
            }
            else
            {
                azimuthTrue = (azim > 180.0) ? azimuth + 360.0 : -azimuth;
            }

            temp = AtanD(SinD(m) * TanD(azimuthTrue));
            intersect = (azim < 180.0) ? sDeg - Abs(temp) : sDeg + Abs(temp);

            /* make sure it is not over 180 */
            if (Abs(intersect) > 180.0)
            {
                intersect = (intersect > 0.0) ? -360.0 + intersect
                    : 360.0 + intersect;
            }

            if (intersect < 0.0)
            {
                intersectSense = Constant.EAST;
                intersect = -1.0 * intersect;
            }
            else
            {
                intersectSense = Constant.WEST;
            }

        }


    }
}
