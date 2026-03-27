using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Math;
using static _NewLib.Maths;
using _Configuration;

namespace _Auxlib
{
    /// <summary>
    /// Provides methods that calculate the separation angles between 
    /// radio-relay antenna beams and the geostationary-satellite orbit.   
    /// </summary>
    public class Sangle
    {
        private const double K = 6.63;          /* orbit radius / earth radius */
        private static double H0;               /* height of station in km */
        private static double H1;               /* height of horizon in km */
        private static double SINL, COSL, TANL;

        /// <summary>
        /// Calculates the separation angle, judged zone, and decision code.
        /// /// </summary>
        /// <remarks>
        /// Output decision codes:
        /// <list type="bullet">
        /// <item>+3 = extreme northern or southern latitude;</item> 
        /// <item>+2 = SA is at least A;</item> 
        /// <item>+1 = SA is at least B;</item>
        /// <item> 0 = SA is 0;</item>
        /// <item>-1 = SA is less than B;</item>
        /// <item>-2:If horizon is flat then SA is less than B.</item> 
        /// </list>
        /// </remarks>
        /// <param name="l"> - latitude of the station.</param>
        /// <param name="b"> - separation angle to avoided.</param>
        /// <param name="a0"> - azimuth of the antenna main beam.</param>
        /// <param name="e0"> - elevation of the antenna main beam.</param>
        /// <param name="dh0"> - height of station in km.</param>
        /// <param name="dh1"> - height of horizon in km.</param>
        /// <param name="psa"> - separation angle.</param>
        /// <param name="pstrC"> - judged zone.</param>
        /// <param name="pkf"> - decision code; see the remarks for details.</param>
        /// <param name="nFlg"> - if flg == 0 then skip "preliminary elimination".</param>
        /// <returns> - 1 if there is no solution, or the orbit is 
        /// below the horizon; 0 if there is a solution in *psa.</returns>
        public static int CalculationSub(double l,
                                             double b,
                                             double a0,
                                             double e0,
                                             double dh0,
                                             double dh1,
                                         out double psa,
                                         out string pstrC,
                                         out int pkf,
                                             int nFlg)
        {
            double r;       /* earth radius */
            double t6;  /*  */
            double n0;  /* refractivity at sea level in N unit */

            double dn;  /* refractivity difference at 1km above sea level in N unit */
            double em1; /* the local horizon at maximum atmospheric bending */
            double em2; /* the local horizon at minimum atmospheric bending */
            double al;  /* equation */

            double as_;  // Legacy 'C/C++' code used variable name 'as' but this is a reserved
                         // token in C#; consequently, use 'as_' instead.
            double am1, am2, dem, dam, be, emax, smax, emin, smin, e1, a1, ee1, s1, es, e3,
                    a3, a31, s31, ee3, s3, a5, s5, e51, ee51, a51, s51, de, ees, sa0,
                    es1, sa1, eem1, eem2, wkes, wkEs_0, wkAs_0, wkSA_0, wkEs_1, wkAs_1,
                    wkSA_1; /* for SAF */

            /*	The azimuth must be positive. 1130 - GJS - 2007.08.03 */
            a0 = Abs(a0);

            /* constants init*/
            r = 6370.0;
            t6 = .000001;
            n0 = 400.0;
            dn = -68;
            H0 = dh0;
            H1 = dh1;
            em1 = Fn_Em(r, t6, dn, n0);
            n0 = 250.0;
            dn = -30;
            em2 = Fn_Em(r, t6, dn, n0);
            SINL = Sin(l);
            COSL = Cos(l);
            TANL = Tan(l);
            al = SINL / Sqrt(Pow((1 - 1 / (K * K)), 2) + Pow((SINL / K), 2));
            /* preliminary calculation */
            eem1 = em1 - Fn_TMAX(em1);
            eem2 = em2 - Fn_TMIN(em2);
            if (al > 1.0)
            {
                pstrC = String.Format("PRELIM");
                pkf = 3;
                psa = 0.0;         /* Impossible solution.  Possibly below horizon.  */
                return 1;
            }
            else
            {
                be = Sqrt(1 - al * al);
            }
            am1 = Fn_C(eem1);
            am2 = Fn_C(eem2);
            dem = em1 - em2;
            dam = am1 - am2;
            /* preliminary elimination */
            if (nFlg == 1)
            {
                if (a0 >= am1 + b)
                {
                    psa = a0 - am1;
                    pstrC = String.Format("PRELIM");
                    pkf = 2;
                    return 1;
                }
                if (e0 <= em2 - b)
                {
                    psa = em2 - e0;
                    pstrC = String.Format("PRELIM");
                    pkf = 2;
                    return 1;
                }
            }
            /* classification of main beam direction  */
            if ((am1 <= a0 && em1 <= e0) ||
                 (am2 <= a0 && a0 < am1 && dem * (a0 - am1) <= (e0 - em1) * dam) ||
                (a0 < am2 && em2 <= e0)
                )
            {
                /* main beam is on or above the horizon */
                emax = e0 - Fn_TMAX(e0);
                smax = Fn_S(a0, emax);
                emin = e0 - Fn_TMIN(e0);
                smin = Fn_S(a0, emin);
                if (smin < 0) goto PROC_ZONE_1;
                if (smax <= 0) goto PROC_ZONE_2;
                if (al * (a0 - am1) < be * (e0 - em1)) goto PROC_ZONE_3;
                else goto PROC_ZONE_4;
            }
            else
            {
                /* main beam is below the horizon */
                if (al * (a0 - am2) < be * (e0 - em2)) goto PROC_ZONE_5;
                if (dem * (e0 - em2) + dam * (a0 - am2) < 0) goto PROC_ZONE_6;
                if (dem * (e0 - em1) + dam * (a0 - am1) < 0) goto PROC_ZONE_7;
                else goto PROC_ZONE_8;
            }

            /* preliminary determination */
            PROC_ZONE_1:
            if (e0 < .3 * Fn_ET(l))
            {
                e1 = e0 + al * b;
                a1 = a0 + be * b;
                ee1 = e1 - Fn_TMIN(e1);
                s1 = Fn_S(a1, ee1);
                psa = b * smin / (smin - s1);
                if (Abs(smin) > 20.0 * Constant.DEG_TO_RAD)
                {
                    psa = Abs(smin);
                }
                es = e0 + al * (psa);
            }
            else
            {
                wkes = Fn_ET(l);
                wkEs_0 = wkes - Fn_TMIN(wkes);
                es = wkes;
                wkAs_0 = Fn_C(wkEs_0);
                wkSA_0 = Fn_SAF(wkAs_0, wkes, a0, e0);
                do
                {
                    wkes = wkes - 1.0 * Constant.DEG_TO_RAD;
                    wkEs_1 = wkes - Fn_TMIN(wkes);
                    wkAs_1 = Fn_C(wkEs_1);
                    wkSA_1 = Fn_SAF(wkAs_1, wkes, a0, e0);
                    if (wkSA_1 < wkSA_0)
                    {
                        wkSA_0 = wkSA_1;
                        es = wkes;
                    }
                } while (wkSA_1 <= wkSA_0);
                psa = wkSA_0;
            }
            pstrC = String.Format("ZONE 1");
            goto DETAIL_CALC_ZONE156;

            PROC_ZONE_2:
            psa = 0;
            pstrC = String.Format("ZONE 2");
            pkf = 0;
            return 0;

            PROC_ZONE_3:
            if (e0 < .3 * Fn_ET(l))
            {
                e3 = e0 - al * b; a3 = a0 - be * b;
                pstrC = String.Format("ZONE 3");
                if (e3 < em1)
                {
                    a31 = a0 - (e0 - em1) * be / al;
                    s31 = Fn_S(a31, eem1);
                    if (Abs(smax - s31) <= .001 * Constant.DEG_TO_RAD) psa = smax;
                    else psa = (e0 - em1) / al * smax / (smax - s31);
                    es = e0 - al * (psa);
                }
                else
                {
                    ee3 = e3 - Fn_TMAX(e3);
                    s3 = Fn_S(a3, ee3);
                    psa = b * smax / (smax - s3);
                    es = e0 - al * (psa);
                    if (es < em1) es = em1;
                }
                if (smax > 20.0 * Constant.DEG_TO_RAD) psa = smax;
            }
            else
            {
                pstrC = String.Format("ZONE 3");
                wkes = Fn_ET(l);
                wkEs_0 = wkes - Fn_TMAX(wkes);
                es = wkes;
                wkAs_0 = Fn_C(wkEs_0);
                wkSA_0 = Fn_SAF(wkAs_0, wkes, a0, e0);
                /* search */
                do
                {
                    wkes = wkes - 1.0 * Constant.DEG_TO_RAD;
                    wkEs_1 = wkes - Fn_TMAX(wkes);
                    wkAs_1 = Fn_C(wkEs_1);
                    wkSA_1 = Fn_SAF(wkAs_1, wkes, a0, e0);
                    if (wkSA_1 < wkSA_0)
                    {
                        wkSA_0 = wkSA_1;
                        es = wkes;
                    }
                } while (wkSA_1 <= wkSA_0);
                psa = wkSA_0;
            }
            goto DETAIL_CALC_ZONE3;

            PROC_ZONE_4:
            psa = Fn_SAF(am1, em1, a0, e0);
            pstrC = String.Format("ZONE 4");
            goto JUDGEMENT;

            PROC_ZONE_5:
            a5 = a0 + (em2 - e0) * be / al;
            s5 = Fn_S(a5, eem2);
            e51 = em2 + al * b;
            a51 = a5 + be * b;
            ee51 = e51 - Fn_TMIN(e51);
            s51 = Fn_S(a51, ee51);
            psa = (em2 - e0) / al + b * s5 / (s5 - s51);
            if (psa > 1) psa = (em2 - e0) / al - s5;
            es = e0 + al * (psa);
            pstrC = String.Format("ZONE 5");
            goto DETAIL_CALC_ZONE156;

            PROC_ZONE_6:
            psa = Fn_SAF(am2, em2, a0, e0); es = em2;
            pstrC = String.Format("ZONE 6");
            goto DETAIL_CALC_ZONE156;

            PROC_ZONE_7:
            psa = (dem * (a0 - am1) - (e0 - em1) * dam) / Sqrt(dem * dem + dam * dam);
            pstrC = String.Format("ZONE 7");
            goto JUDGEMENT;

            PROC_ZONE_8:
            psa = Fn_SAF(am1, em1, a0, e0);
            pstrC = String.Format("ZONE 8"); goto JUDGEMENT;

            /* detailed calculation for zone 1,5,6 */
            DETAIL_CALC_ZONE156:
            /* step 1 */
            if (psa >= 2.0 * b) goto JUDGEMENT;
            else de = be * b / 200.0;

            ZONE156_STEP1_LOOP:
            ees = es - Fn_TMIN(es);
            if (Fn_F(ees) - ees < l)
            {
                es = es - de;
                goto ZONE156_STEP1_LOOP;
            }

            as_ = Fn_C(ees);
            sa0 = Fn_SAF(as_, es, a0, e0);
            es1 = es + de;
            ees = es1 - Fn_TMIN(es1);
            if (Fn_F(ees) - ees < l)
            {
                es1 = es;
                psa = sa0;
                goto ZONE156_STEP3;
            }

            as_ = Fn_C(ees);
            psa = Fn_SAF(as_, es1, a0, e0);
            if (psa > sa0)
            {
                es1 = es;
                psa = sa0;
                goto ZONE156_STEP3;
            }


            /* step 2 */
            ZONE156_STEP2:
            es1 = es1 + de;
            ees = es1 - Fn_TMIN(es1);
            if (Fn_F(ees) - ees < l) goto JUDGEMENT;

            as_ = Fn_C(ees);
            sa1 = Fn_SAF(as_, es1, a0, e0);
            if (sa1 < psa)
            {
                psa = sa1;
                goto ZONE156_STEP2;
            }
            else
            {
                goto JUDGEMENT;
            }


            /* step 3 */
            ZONE156_STEP3:
            if (es1 <= em2) goto JUDGEMENT;
            es1 = es1 - de;
            if (es1 < em2) es1 = em2;
            ees = es1 - Fn_TMIN(es1);

            as_ = Fn_C(ees);
            sa1 = Fn_SAF(as_, es1, a0, e0);
            if (sa1 < psa)
            {
                psa = sa1;
                goto ZONE156_STEP3;
            }
            else
            {
                goto JUDGEMENT;
            }


            /* detailed calculation for zone 3 */
            DETAIL_CALC_ZONE3:
            /* step 1 */
            if (psa >= 2.0 * b) goto JUDGEMENT;
            else de = be * b / 200.0;


            ZONE3_STEP1:
            ees = es - Fn_TMAX(es);
            if (Fn_F(ees) - ees < l)
            {
                es = es - de;
                goto ZONE3_STEP1;
            }

            as_ = Fn_C(ees);
            sa0 = Fn_SAF(as_, es, a0, e0);
            es1 = es + de;
            ees = es1 - Fn_TMAX(es1);
            if (Fn_F(ees) - ees < l)
            {
                es1 = es;
                psa = sa0;
                goto ZONE3_STEP3;
            }

            as_ = Fn_C(ees);
            psa = Fn_SAF(as_, es1, a0, e0);
            if (psa > sa0)
            {
                es1 = es;
                psa = sa0;
                goto ZONE3_STEP3;
            }


            /* step 2 */
            ZONE3_STEP2:
            es1 = es1 + de;
            ees = es1 - Fn_TMAX(es1);
            if (Fn_F(ees) - ees < l) goto JUDGEMENT;

            as_ = Fn_C(ees);
            sa1 = Fn_SAF(as_, es1, a0, e0);
            if (sa1 < psa)
            {
                psa = sa1;
                goto ZONE3_STEP2;
            }
            else
            {
                goto JUDGEMENT;
            }

            /* step 3 */
            ZONE3_STEP3:
            if (es1 <= em1) goto JUDGEMENT;
            es1 = es1 - de;
            if (es1 < em1) es1 = em1;
            ees = es1 - Fn_TMAX(es1); as_ = Fn_C(ees);
            sa1 = Fn_SAF(as_, es1, a0, e0);
            if (sa1 < psa)
            {
                psa = sa1;
                goto ZONE3_STEP3;
            }

            JUDGEMENT:
            if (psa >= b) pkf = 1;
            else pkf = -2;
            return 0;
        }

        /// <summary>
        /// Calculates elevation angle toward the local horizon.  
        /// </summary>
        /// <param name="r"></param>
        /// <param name="t6"></param>
        /// <param name="dn"></param>
        /// <param name="n0"></param>
        /// <returns></returns>
        private static double Fn_Em(double r, double t6, double dn, double n0)
        {
            double emh;
            emh = (r + H1) / (r + H0) * (1 + n0 * t6 * Pow((1 + dn / n0), H1))
                        / (1 + n0 * t6 * Pow((1 + dn / n0), H0));

            if (emh == 1)
            {
                return 0.0;
            }
            else
            {
                return (-1 * Acos(emh));
            }
        }

        /// <summary>
        /// Calculates the maximum atmospheric bending in radians.  
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private static double Fn_TMAX(double e)
        {
            return (double)(Constant.DEG_TO_RAD / (.7885809 + .175963 * H0 + .025162 * H0 * H0
                + e * Constant.RAD_TO_DEG * (.549056 + .0744484 * H0 + .010165 * H0 * H0
                + e * Constant.RAD_TO_DEG * (.0187029 + .0143814 * H0))));
        }


        /// <summary>
        /// Calculates the minimum atmospheric bending in radians.  
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private static double Fn_TMIN(double e)
        {
            return (Constant.DEG_TO_RAD / (1.755698 + .313461 * H0 + e * Constant.RAD_TO_DEG * (.815022
                + .109154 * H0 + e * Constant.RAD_TO_DEG * (.0295668 + .0185682 * H0))));

        }

        /// <summary>
        /// Calculates azimuth of the orbit.  
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private static double Fn_C(double e)
        {
            return (double)(Acos(TANL / Tan(Fn_F(e) - e)));
        }

        /// <summary>
        /// Calculates auxiliary elevation angle.  
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private static double Fn_F(double e)
        {
            return (Acos(Cos(e) / K));
        }

        /// <summary>
        /// Calculates angle between the beam and the orbit.  
        /// </summary>
        /// <param name="a"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private static double Fn_S(double a, double e)
        {
            return (Asin(SINL * Cos(Fn_F(e) - e) - COSL * Sin(Fn_F(e) - e) * Cos(a)));
        }

        /// <summary>
        /// Calculates highest elevation angle of the geostationary-satellite orbit.  
        /// </summary>
        /// <param name="l"></param>
        /// <returns></returns>
        private static double Fn_ET(double l)

        {
            double et = PI / 2 - l;

            for (int i = 0; i < 5; i++)
            {
                et = et - (K * Cos(et + l) - Cos(et))
                    / (-1 * K * Sin(et + l) + Sin(et));
            }
            return et;
        }

        /// <summary>
        /// Calculates angle between the beam and the orbit.  
        /// </summary>
        /// <param name="a"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        /// <summary>
        /// Calculates separation angle.  
        /// </summary>
        /// <param name="a"></param>
        /// <param name="e"></param>
        /// <param name="a0"></param>
        /// <param name="e0"></param>
        /// <returns></returns>
        private static double Fn_SAF(double a, double e, double a0, double e0)
        {
            return (Acos(Cos(e0) * Cos(e) * Cos(a - a0) + Sin(e0) * Sin(e)));
        }


    }
}
