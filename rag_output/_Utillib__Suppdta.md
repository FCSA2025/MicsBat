# Documented File: Suppdta.cs
**Repository Path:** `_Utillib\Suppdta.cs`
**Primary Layer:** `_Utillib`
**Namespace:** `_Utillib`

## Source Code Representation
```csharp
using _Configuration;
using _NewLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Math;

namespace _Utillib
{
    public class Suppdta
    {

        /// <summary>
        /// </summary>
        /// <param name="n1"></param>
        /// <param name="fmin1"></param>
        /// <param name="fm1"></param>
        /// <param name="dsig1"></param>
        /// <param name="nf"></param>
        /// <param name="xif"></param>
        /// <param name="sif"></param>
        /// <param name="irf"></param>
        /// <param name="nFilter"></param>
        /// <param name="freq1in"></param>
        /// <param name="alpha1in"></param>
        /// <param name="ai70"></param>
        /// <param name="ai140"></param>
        /// <param name="nSpect"></param>
        /// <param name="yin"></param>
        /// <param name="zin"></param>
        /// <param name="abw1"></param>
        /// <param name="fs"></param>
        /// <param name="IsEs"></param>
        /// <param name="ctoi"></param>
        /// <param name="ilvl"></param>
        /// <returns></returns>
        public static int SuppDta(double n1,
                        double fmin1,
                        double fm1,
                        double dsig1,
                        double nf,
                        double xif,
                        double sif,
                        double irf,
                        int nFilter,
                        double[] freq1in,
                        double[] alpha1in,
                        double ai70,
                        double ai140,
                        int nSpect,
                        double[] yin,
                        double[] zin,
                        double abw1,
                        double fs,
                        bool IsEs,
                        out double ctoi,
                        out double ilvl)
        {
            // 'out' requirements.
            ctoi = 0.0;
            ilvl = 0.0;

            double[] f1 = new double[2 * Constant.MAX_FILTER];
            double[] f3 = new double[Constant.MAX_SPECTRUM];
            double[] f5 = new double[Constant.MAX_FILTER];
            double[] f3a = new double[2 * Constant.MAX_SPECTRUM];
            double[] d1 = new double[2 * Constant.MAX_FILTER];
            double[] d3 = new double[Constant.MAX_SPECTRUM];
            double[] p1hz = new double[Constant.MAX_SPECTRUM];
            double[] d3a = new double[2 * Constant.MAX_SPECTRUM];
            double[] d5 = new double[Constant.MAX_FILTER];
            double[] freq1 = new double[Constant.MAX_FILTER];
            double[] alpha1 = new double[Constant.MAX_FILTER];
            double[] y = new double[Constant.MAX_SPECTRUM];
            double[] z = new double[Constant.MAX_SPECTRUM];

            double nlr1, m1, temp_fs;

            int i;
            int nps1;
            int nps2;
            int np;

            double a70;
            double a140;
            double xn1;
            double sigma1;
            double fco;
            double xi;
            double df;
            double fl;
            double fh;
            double fm1hz;
            double pwr;
            double corr;

            int ilast;
            double inc;
            double mult;
            int nRet;
            double dMaxFS = (fs > Constant.MAX_FREQ_SEP_MHZ) ? fs : Constant.MAX_FREQ_SEP_MHZ;

            /*	Do some sanity checks.  GJS 2004.07.23 */
            if (nFilter >= Constant.MAX_FILTER)
            {
                nRet = -20;     /*	Filter array too long */
                ctoi = 0.0;
                ilvl = 0.0;
                return (nRet);
            }

            if (nSpect >= Constant.MAX_SPECTRUM)
            {
                nRet = -21;
                ctoi = 0.0;
                ilvl = 0.0;
                return (nRet);
            }

            /*	Initialize the variables */
            temp_fs = 0.0;
            for (i = 1; i < Constant.MAX_FILTER; i++)
            {
                f5[i] = 0.0;
                d5[i] = 0.0;
            }

            for (i = 1; i < Constant.MAX_SPECTRUM; i++)
            {
                p1hz[i] = 0.0;
                f3[i] = 0.0;
                d3[i] = 0.0;
            }

            for (i = 1; i < 2 * Constant.MAX_FILTER; i++)
            {
                f1[i] = 0.0;
                d1[i] = 0.0;
            }

            xn1 = n1;
            if (n1 < 240)
            { /* 200,210,210;*/
                nlr1 = 4.0 * Log10(xn1) - 1.0;
            }
            else
            {
                nlr1 = 10.0 * Log10(xn1) - 15.0;
            }

            sigma1 = dsig1 * Pow(10.0, (.05 * nlr1));
            m1 = sigma1 / fm1;
            if (nFilter == 0)
            {
                /*	We do not have a filter supplied, but we need one.  So if
				*		the arrays are null, then allocate them  and fill them in.
				*		This array will not be passed back. */
                fco = 1.5 * (3.76 * sigma1 + 2.0 * fm1);
                freq1[1] = 0.0;
                alpha1[1] = 0.0;
                freq1[2] = fco;
                alpha1[2] = 0.0;
                freq1[23] = fco * Pow(10.0, (Log10(2.0) * 60.0 / 25));
                alpha1[23] = 60.0;
                if (fs > Constant.MAX_FREQ_SEP_MHZ)
                {
                    freq1[24] = fs;
                }
                else
                {
                    freq1[24] = Constant.MAX_FREQ_SEP_MHZ;
                }
                alpha1[24] = 60.0;
                for (i = 3; i <= 22; i++)
                {
                    xi = i - 2;
                    freq1[i] = (xi * (freq1[23] - freq1[2]) / 21.0) + freq1[2];
                    df = freq1[i];
                    alpha1[i] = 25.0 * Log10(df / fco) / Log10(2.0);
                }
                nFilter = 24;
            }
            else
            {
                /*	The array has been passed in, copy it to the work array, zeroing
				*		the last part */
                for (i = 1; i < Constant.MAX_FILTER; i++)
                {
                    if (i <= nFilter)
                    {
                        freq1[i] = freq1in[i];
                        alpha1[i] = alpha1in[i];
                    }
                    else
                    {
                        freq1[i] =
                        alpha1[i] = 0.0;
                    }
                }
                //	We need to condition the input filter curve to extend out to the max frequency.
                if (freq1[nFilter] < dMaxFS)
                {
                    //	Add a point to the end of the curve, there should be room in the array.
                    freq1[nFilter + 1] = dMaxFS;
                    alpha1[nFilter + 1] = alpha1[nFilter];
                    nFilter++;
                }
            }

            /*	Now check that the frequency separation is less than the highest
			*		filter frequency. 1208 - GJS - 2006.03.16 */
            if (fs > freq1[nFilter])
            {
                ctoi = 0.0;
                ilvl = 0.0;
                return (-1);
            }
            /*
            *     calculate rf filter attenuation at if & 2if separation.  If the IF
            *			frequency is zero or less, then no IF calculations will take place.
            *			2005.04.25 - 1193 - GJS
            */
            if (xif > 0.0)
            {
                if (ai70 == 0.0)
                {
                    for (i = 1; i <= nFilter; i++)
                    {
                        if (freq1[i] > xif)
                        {
                            ai70 = (alpha1[i - 1] + (alpha1[i] - alpha1[i - 1]) * ((xif - freq1[i - 1]) /
                                         (freq1[i] - freq1[i - 1]))) / 2;
                            break;
                        }
                    }
                }

                if (ai140 == 0.0)
                {
                    for (i = 1; i <= nFilter; i++)
                    {
                        if (freq1[i] > 2 * xif)
                        {
                            ai140 = (alpha1[i - 1] + (alpha1[i] - alpha1[i - 1]) *
                                                ((2 * xif - freq1[i - 1]) / (freq1[i] - freq1[i - 1]))) / 2;
                            break;
                        }
                    }
                }
            }
            else
            {
                /*	This is just to avoid floating point errors.  The values will be
				*		ignored. 2005.04.25 - 1193 - GJS */
                ai70 =
                ai140 = 0.0;
            }
            /*
            c     calculate a70 and a140;
            */
            //L280: 
            if (sif != 0.0)
            {
                a70 = sif;
            }
            else
            {
                a70 = ai70 + 20;
            }

            if (irf != 0.0)
            {
                a140 = irf;
            }
            else
            {
                a140 = ai140;
            }

            if (nSpect == 0)
            {
                /*
                c     	generate default digital interferer spectrum;
                c       (revised 98 08 29 by l. abbott to include possible;
                c        level portion from y[3] to y[4] with 50 db attenuation);
                */
                fl = (55 - 10 * Log10(abw1)) * abw1 / 80;
                fh = (85 - 10 * Log10(abw1)) * abw1 / 80;
                y[1] = 0;
                z[1] = 10 * Log10(abw1 / 0.004);
                y[2] = abw1 / 2;
                z[2] = z[1];
                y[3] = abw1 / 2.0 + .000001;
                z[3] = 35 + 10 * Log10(abw1);
                if (z[3] <= 50)
                {
                    z[3] = 50;
                    y[4] = fl;
                    z[4] = 50;
                }
                else
                {
                    y[4] = y[3] + .000001;
                    z[4] = z[3];
                }
                /*	The following was changed to handle the request that
				*		the Pd be made 150 from 10.0 * BW + .1 out to 400
				*		Added as part of 1083 - GJS - 2003.12
				*		Further modified 2004.03.30 - GJS - 1170 when Telsat wanted the
				*		old algorithm used when the victim is ES.  */
                if (10.0 * abw1 < dMaxFS && !IsEs)
                {
                    /*	From fh out to 10.0 * abw1 value is 80 */
                    y[33] = fh;
                    z[33] = 80.0;
                    y[34] = 10.0 * abw1;
                    z[34] = 80.0;
                    /*	From 10.0 * abw1 out to max freq, value is 150 */
                    y[35] = 10.0 * abw1 + 0.1;
                    z[35] = 150.0;
                    y[36] = dMaxFS;
                    z[36] = 150.0;
                    ilast = 33;
                }
                else
                {
                    /*	Just use 80 all the way out to MAX_FREQ_SEP_MHZ */
                    y[35] = fh;
                    z[35] = 80;
                    y[36] = dMaxFS;
                    z[36] = 80;
                    ilast = 35;
                }
                /*	Now fill in the values in between using linear interp. */
                inc = 1.0 / (double)(ilast - 4);
                for (i = 5, mult = inc; i < ilast; i++, mult += inc)
                {
                    y[i] = (mult * (fh - y[4])) + y[4];
                    z[i] = (mult * (80 - z[4])) + z[4];
                }
                /*	Set the length of the array to max */
                nSpect = 36;
            }
            else
            {
                /*	The array has been passed in, copy it to the work array, zeroing
				*		the last part */
                for (i = 1; i < Constant.MAX_SPECTRUM; i++)
                {
                    if (i <= nSpect)
                    {
                        y[i] = yin[i];
                        z[i] = zin[i];
                    }
                    else if (i == nSpect + 1 && y[i] < dMaxFS)
                    {
                        //	Add the far end of the max spectrum if necessary.
                        y[i] = dMaxFS;
                        z[i] = zin[i - 1]; //	Extend the last value all the way out.
                    }
                    else
                    {
                        y[i] =
                        z[i] = 0.0;
                    }
                }
                nSpect++; //	Add the max frequency.
            }

            DtaSpect(f5, d5, m1, out nps1, fmin1, fm1);

            fm1hz = fm1 * 1e6;
            f1[nps1] = 0.0;
            d1[nps1] = Pow(10.0, (0.1 * d5[1])) / fm1hz;
            if (nps1 > 1)
            {  /*610,610,590;*/
                for (i = 2; i <= nps1; i++)
                {
                    f1[nps1 + i - 1] = f5[i] * fm1hz;
                    d1[nps1 + i - 1] = Pow(10.0, (0.1 * d5[i])) / fm1hz;
                    f1[nps1 - i + 1] = -f1[nps1 + i - 1];
                    d1[nps1 - i + 1] = d1[nps1 + i - 1];
                }
            }
            for (i = nSpect; i > 1; i--)
            {
                if (z[i] != 0)
                {
                    break;
                }
            }
            nps2 = i;

            for (i = nFilter; i > 1; i--)
            {
                if (freq1[i] != 0.0 || alpha1[i] != 0.0)
                {
                    break;
                }
            }
            np = i;
            /*
            c     create double sided interferer  spectrum;
            */
            for (i = 1; i <= nps2; i++)
            {
                f3[i] = y[i] * 1e6;
                z[i] = 10.0 * Log10(1.0 / 4000.0) - Abs(z[i]);
                p1hz[i] = Pow(10.0, (z[i] / 10.0));
                d3[i] = p1hz[i];
            }

            f3a[nps2] = f3[1];
            d3a[nps2] = d3[1];
            for (i = 2; i <= nps2; i++)
            {
                f3a[nps2 + i - 1] = f3[i];
                d3a[nps2 + i - 1] = d3[i];
                f3a[nps2 - i + 1] = -f3[i];
                d3a[nps2 - i + 1] = d3[i];
            }
            /*
            c     calculate correction factor for interferer spectrum for;
            c      total spectrum power equal to one.;
            */
            pwr = 0.0;
            for (i = 1; i <= 2 * nps2 - 2; i++)
            {
                pwr = pwr + 0.5 * (d3a[i] + d3a[i + 1]) * (f3a[i + 1] - f3a[i]);
            }
            if (pwr != 1)
            {  /*623,800,624; */
                if (pwr < 1)
                {
                    corr = Abs(10.0 * Log10(pwr));
                }
                else
                {
                    corr = -Abs(10.0 * Log10(pwr));
                }
                for (i = 1; i <= nps2; i++)
                {
                    d3[i] = Pow(10.0, ((10.0 * Log10(d3[i]) + corr) / 10.0));
                }
            }

            //...Log2.v(String.Format("\nn1 = " + n1));
            //...Log2.v(String.Format("\nfmin1 = " + fmin1));
            //...Log2.v(String.Format("\nfm1 = " + fm1));
            //...Log2.v(String.Format("\ndsig1 = " + dsig1));
            //...Log2.v(String.Format("\nnf = " + nf));
            //...Log2.v(String.Format("\nfs = " + fs));
            //...Log2.v(String.Format("\nctoi = " + ctoi));
            //...Log2.v(String.Format("\nilvl = " + ilvl));
            //...Log2.v(String.Format("\nxn1 = " + xn1));
            //...Log2.v(String.Format("\nnlr1 = " + nlr1));
            //...Log2.v(String.Format("\nsigma1 = " + sigma1));
            //...Log2.v(String.Format("\nm1 = " + m1));
            //...Log2.v(String.Format("\nf3[{0}] = {1}", j, f3[j]));
            //...Log2.v(String.Format("\nd3[{0}] = {1}", j, d3[j]));
            //...Log2.v(String.Format("\nnps1 = " + nps1));
            //...Log2.v(String.Format("\nfm1hz = " + fm1hz));
            //...Log2.v(String.Format("\nf1[{0}] = {1}", j, f1[j]));
            //...Log2.v(String.Format("\nd1[{0}] = {1}", j, d1[j]));
            //...Log2.v(String.Format("\nnps2 = " + nps2));
            //...Log2.v(String.Format("\na70 = " + a70));
            //...Log2.v(String.Format("\na140 = " + a140));
            //...Log2.v(String.Format("\nalpha1[{0}] = {1}", j, alpha1[j]));
            //...Log2.v(String.Format("\nfreq1[{0}] = {1}", j, freq1[j]));
            //...Log2.v(String.Format("\nnp = " + np));
            //...Log2.v(String.Format("\nxif = " + xif));
            //...Log2.v(String.Format("\ntemp_fs = " + temp_fs));

            Covri3(n1, fmin1, fm1, dsig1, nf, fs, ref ctoi, ref ilvl, xn1, nlr1,
                     sigma1, m1, f3, d3, nps1, fm1hz, f1, d1, nps2,
                     a70, a140, alpha1, freq1, np, xif, ref temp_fs);

            //...Log2.v("\nAfter_covri3(), ctoi = " + ctoi);

            return (0);
        }

        /// <summary>
        /// </summary>
        /// <param name="ff"></param>
        /// <param name="d"></param>
        /// <param name="m"></param>
        /// <param name="nps"></param>
        /// <param name="fmin"></param>
        /// <param name="fm"></param>
        public static void DtaSpect(double[] ff,
                                    double[] d,
                                    double m,
                                    out int nps,
                                    double fmin,
                                    double fm)
        {
            // 'out' requirement.
            nps = 0;

            double[] f1 = new double[9] { 0, 0.0, 0.1, 0.2, 0.3, 0.4, 0.6, 0.8, 1.0 };
            /*	The following is the array transposed */
            double[,] d1 = new double[9, 9]   {{0, 0, 0, 0, 0, 0, 0, 0, 0},
                                               {0, 7.0,  3.2, 1.0, -0.8, -2.1, -3.5, -4.8, -5.8},
                                               {0, 2.0, 0.5, 0.2, -1.0, -2.2, -3.6, -4.8, -5.8},
                                               {0, -2.5, -2.0, -0.7, -1.4, -2.4, -3.8, -4.8, -5.8},
                                               {0, -5.8, -3.9, -1.9, -1.9, -2.7, -4.0, -4.9, -5.8},
                                               {0, -8.0, -5.8, -3.2, -2.6, -3.1, -4.3, -5.0, -5.9},
                                               {0, -11.8, -9.3, -6.0, -4.5, -4.5, -5.0, -5.5, -6.1},
                                               {0, -14.5, -12.8, -9.0, -7.0, -6.2, -6.0, -6.0, -6.4},
                                               {0, -18.7, -16.0, -12.0, -9.8, -8.4, -7.4, -6.8, -6.7}
                                              };

            double[,] f2 = new double[7, 9] {{0, 0, 0, 0, 0, 0, 0, 0, 0},
                                             {0, 1.03, 1.23, 1.49, 1.68, 1.87, 2.27, 2.73, 3.84},
                                             {0, 1.35, 1.76, 2.05, 2.29, 2.51, 3.04, 3.67, 5.01},
                                             {0, 1.78, 2.22, 2.56, 2.85, 3.15, 3.80, 4.46, 5.96},
                                             {0, 2.20, 2.66, 3.02, 3.39, 3.75, 4.46, 5.16, 6.77},
                                             {0, 2.60, 3.06, 3.47, 3.90, 4.28, 5.10, 5.81, 7.50},
                                             {0, 2.98, 3.44, 3.90, 4.36, 4.81, 5.72, 6.43, 8.16}
                                            };
            double[] d2 = new double[7] { 0, -20.0, -30.0, -40.0, -50.0, -60.0, -70.0 };
            double[] m1 = new double[9] { 0, 0.2, 0.3, 0.4, 0.5, 0.6, 0.8, 1.0, 1.5 };

            double area;
            double e;
            double repatan;
            double phmod;
            double x1;
            double hpf;
            double fact;
            double x;
            double xi;
            double ff2;

            int i;
            int j;

            if (m >= 1.5)
            { /*130,100,100;*/
                for (i = 1; i <= 13; i++)
                {
                    ff[i] = i - 1;
                    d[i] = Exp(-ff[i] * ff[i] / (2.0 * m * m)) / (m * Sqrt(2.0 * 3.14159));
                    d[i] = 10.0 * Log10(d[i]);
                    if (d[i] <= -70.0)
                    { /*120,120,110; */
                        break;
                    }
                }

                nps = i;
                if (nps > 13) nps = 13;
                return;
            }
            ff[1] = 0.0;
            ff[2] = 0.5e-6 / fm;
            x1 = fmin / fm;
            repatan = Atan(0.6522 * x1 / (1.0 - 0.64 * x1 * x1));
            phmod = 0.319 * m * m / (1.0 - x1) * (1.25 / x1 + 3.5395 - 2.8123 * repatan + 0.6277
                  * Log((0.64 * x1 * x1 - 1.461 * x1 + 1.0) / (0.64 * x1 * x1 + 1.461 * x1 + 1.0)));
            e = Exp(-phmod);
            /*Console.Write("\ne = %10.4lf",e); */

            if (e > 0.0)
            {
                d[1] = 10.0 * Log10(fm * 1.0e6 * e);
            }
            else
            {
                d[1] = -100.0;
            }
            d[2] = d[1];
            ff[3] = 0.5000001e-6 / fm;
            if (m <= 0.2)
            {
                d[3] = -100.0;
                ff[4] = x1 - 0.0000001e-6 / fm;
                d[4] = -100.0;
                for (i = 1; i <= 20; i++)
                {
                    xi = i;
                    ff[i + 4] = x1 + (xi - 1.0) / 20.0 * (2.0 - x1);
                    x = ff[i + 4];
                    d[i + 4] = 0.0;
                    if (ff[i + 4] > 1.0) goto L150;
                    hpf = 1.0 + 5.25 / Pow((1.25 / x - x / 1.25), 2.0);
                    hpf = 3.15 / (1.0 + 6.90 / hpf);
                    d[i + 4] = 0.5 * m * m / ((1.0 - x1) * x * x);
                    d[i + 4] = d[i + 4] * hpf;
                    L150: ff2 = 0.0;
                    if (ff[i + 4] < 2.0 * x1 || ff[i + 4] > x1 + 1.0) goto L160;
                    ff2 = ff2 + x * (x - 2.0 * x1) / (x1 * (x - x1)) + 2.0 * Log((x - x1) / x1);
                    L160: if (ff[i + 4] < x1 + 1.0) goto L170;
                    ff2 = ff2 + x * (2.0 - x) / (x - 1.0) + 2.0 * Log(1.0 / (x - 1.0));
                    L170: ff2 = ff2 * 0.25 * Pow(m, 4.0) / (Pow((1.0 - x1), 2.0) * Pow(x, 3.0));
                    d[i + 4] = d[i + 4] + ff2;
                    d[i + 4] = 10.0 * Log10(d[i + 4] * e);
                    if (d[i + 4] <= -70.0)
                    {
                        break;
                    }
                }

                nps = i + 4;
                if (nps > 24) nps = 24;
                return;
            }


            for (i = 2; i <= 8; i++)
            {
                ff[i + 2] = f1[i];
            }

            for (i = 2; i <= 8; i++)
            {
                if (m <= m1[i]) goto L230;
                /* continue */
                ;
            }
            if (i > 8) i = 8;
            L230: fact = (m - m1[i - 1]) / (m1[i] - m1[i - 1]);

            j = i;
            for (i = 1; i <= 8; i++)
            {
                d[i + 2] = d1[i, j - 1] + (d1[i, j] - d1[i, j - 1]) * fact;
            }

            for (i = 1; i <= 6; i++)
            {
                d[i + 10] = d2[i];
                ff[i + 10] = f2[i, j - 1] + (f2[i, j] - f2[i, j - 1]) * fact;
            }

            nps = 16;
            /*
            c     code to be added to the spect subroutine to compensate
            c     for the error in power spectra.
            c     initialize area
            */
            area = 0.0;
            for (i = 2; i <= nps; i++)
            {
                area += (Pow(10.0, (.1 * d[i])) + Pow(10.0, (.1 * d[i - 1]))) *
                              (ff[i] - ff[i - 1]);
            }

            /* Console.Write("\nArea = {0,6:F3}", area); */

            for (i = 1; i <= nps; i++)
            {
                d[i] = d[i] - 10.0 * Log10(area);
            }
            /*
            c     ff(i) is the normalized frequency (f/fmax)
            c     d(i) is fmax times p(f) (spectral value in db)
            c     then, area between ff(i-1) and ff(i) is given by:
            c     area = ect.
            c     and the total power is the sum of all the areas multiplied by 2,
            c     since the spectra are stored as half-spectra (0 hz to infinity)
            */
            return;
        }

        /// <summary>
        /// </summary>
        /// <param name="n1"></param>
        /// <param name="fmin1"></param>
        /// <param name="fm1"></param>
        /// <param name="dsig1"></param>
        /// <param name="nf"></param>
        /// <param name="fs"></param>
        /// <param name="ctoi"></param>
        /// <param name="ilvl"></param>
        /// <param name="xn1"></param>
        /// <param name="nlr1"></param>
        /// <param name="sigma1"></param>
        /// <param name="m1"></param>
        /// <param name="f3"></param>
        /// <param name="d3"></param>
        /// <param name="nps1"></param>
        /// <param name="fm1hz"></param>
        /// <param name="f1"></param>
        /// <param name="d1"></param>
        /// <param name="nps2"></param>
        /// <param name="a70"></param>
        /// <param name="a140"></param>
        /// <param name="alpha1"></param>
        /// <param name="freq1"></param>
        /// <param name="np"></param>
        /// <param name="xif"></param>
        /// <param name="temp_fs"></param>
        public static void Covri3(double n1,         /* not used? */
                                    double fmin1,
                                    double fm1,
                                    double dsig1,
                                    double nf,
                                    double fs,
                                    ref double ctoi,
                                    ref double ilvl,
                                    double xn1,        /* not used? */
                                    double nlr1,
                                    double sigma1,
                                    double m1,
                                    double[] f3,
                                    double[] d3,
                                    int nps1,
                                    double fm1hz,
                                    double[] f1,
                                    double[] d1,
                                    int nps2,
                                    double a70,
                                    double a140,
                                    double[] alpha1,
                                    double[] freq1,
                                    int np,
                                    double xif,
                                    ref double temp_fs)
        {
            double[] r = new double[2 * Constant.MAX_SPECTRUM];
            double[] ra = new double[5 * Constant.MAX_FILTER];
            double[] f2 = new double[5 * Constant.MAX_FILTER];
            double[] f4 = new double[7 * Constant.MAX_FILTER];
            double[] r2 = new double[2 * Constant.MAX_SPECTRUM];
            double[] r2a = new double[5 * Constant.MAX_FILTER];
            double[] freq = new double[Constant.MAX_FILTER];
            double[] freq2 = new double[2 * Constant.MAX_FILTER];
            double[] s = new double[2 * Constant.MAX_SPECTRUM];
            double[] sa = new double[5 * Constant.MAX_FILTER];
            double[] d2 = new double[5 * Constant.MAX_FILTER];
            double[] d4 = new double[7 * Constant.MAX_FILTER];
            double[] s2 = new double[2 * Constant.MAX_SPECTRUM];
            double[] s2a = new double[5 * Constant.MAX_FILTER];
            double[] alpha2 = new double[2 * Constant.MAX_FILTER];

            double ifch, maxi, c70, c140, esele, ilvl3, ctoi3;
            double npr0 = 0.0;
            double ilvl1 = 0.0;
            int flag, flag1, flag2;

            int i;
            int j1;
            int j2;
            int k1;
            int k2;
            int k3;
            int nn1;
            int nn2;
            int ibeta;

            double p = 0.0;
            double q;
            double a;
            double b;
            double x;
            double fs1;
            double fs1hz;
            double fs2;
            double fs2hz;
            double fch;
            double ctoi1 = 0.0;
            double fchhz;
            double fshz;
            double beta1;
            double beta2;
            double beta;
            double area;
            double cval1 = 0.0;
            double cval2;
            double sele;
            double hpf;
            double fmin1h;
            double bwr1 = 0.0;

            int label;
            int nInd;
            /*
            c     initializes variables
            */

            /*******************************************************************/
            /*	Zero the internal arrays, for the dval entries */
            for (nInd = 0; nInd < 2 * Constant.MAX_FILTER; nInd++)
            {
                freq2[nInd] = 0.0;
                alpha2[nInd] = 0.0;
            }
            for (nInd = 0; nInd < 2 * Constant.MAX_SPECTRUM; nInd++)
            {
                r[nInd] = 0.0;
                s[nInd] = 0.0;
            }
            for (nInd = 0; nInd < 5 * Constant.MAX_FILTER; nInd++)
            {
                f2[nInd] = 0.0;
                d2[nInd] = 0.0;
            }

            flag = 0;
            flag1 = 0;
            flag2 = 0;
            temp_fs = fs;
            ctoi3 = -99;
            ilvl3 = +99;
            for (i = 1; i < Constant.MAX_FILTER; i++)
            {
                freq[i] = 0.0;
            }

            for (i = 1; i < 2 * Constant.MAX_FILTER; i++)
            {
                freq2[i] = 0.0;
                alpha2[i] = 0.0;
            }

            for (i = 1; i < 2 * Constant.MAX_SPECTRUM; i++)
            {
                r[i] = 0.0;
                s[i] = 0.0;
                r2[i] = 0.0;
                s2[i] = 0.0;
            }

            for (i = 1; i < 5 * Constant.MAX_FILTER; i++)
            {
                ra[i] = 0.0;
                sa[i] = 0.0;
                f2[i] = 0.0;
                d2[i] = 0.0;
                r2a[i] = 0.0;
                s2a[i] = 0.0;
            }

            for (i = 1; i < 7 * Constant.MAX_FILTER; i++)
            {
                f4[i] = 0.0;
                d4[i] = 0.0;
            }

            esele = 0.0;
            fs1 = fs;
            fs1hz = fs * 1e6;
            fs2hz = fs * 1e6;
            fs2 = fs;
            c70 = 0;
            c140 = 0;
            /*
            c     this code determines if freq. separation is in spurious
            c      response range, shifts offset by xif or 2xif and sets
            c      sensitivity of spurious responses.
            */
            if (xif <= 0 || fs < xif - xif / 2 || fs > xif + xif / 2) goto L520;
            flag = 1;
            if (fs > xif) flag1 = 1;
            c70 = a70;
            fs = Abs(xif - fs);
            temp_fs = Abs(xif - fs);
            if (flag1 == 1) temp_fs = Abs(xif + fs);
            if (temp_fs == xif) flag1 = 1;
            fs1 = fs;
            fs1hz = fs * 1e6;
            L520: if (xif <= 0 || fs < (2.0 * xif - xif / 2) || fs > (2.0 * xif + xif / 2)) goto L530;
            flag = 2;
            if (fs > (2 * xif)) flag2 = 1;
            c140 = a140;
            fs = Abs(2.0 * xif - fs);
            temp_fs = Abs((2.0 * xif) - fs);
            if (flag2 == 1) temp_fs = Abs((2 * xif) + fs);
            if (temp_fs == (2 * xif)) flag2 = 1;
            fs1 = fs;
            fs1hz = fs * 1e6;
            /*;
            c     part of code to determine worst channel in baseband;
            */
            ;
            L530: label = 1;
            fch = fmin1;
            label = 2;
            goto L180;

            L540: fch = fm1 / 8.0;
            ctoi1 = ctoi;
            ilvl1 = ilvl;
            label = 3;
            goto L180;

            L550: fch = fm1 / 4.0;
            if (ctoi > ctoi1) ctoi1 = ctoi;
            if (ilvl < ilvl1) ilvl1 = ilvl;
            label = 4;
            goto L180;

            L560: fch = 3.0 * fm1 / 8.0;
            if (ctoi > ctoi1) ctoi1 = ctoi;
            if (ilvl < ilvl1) ilvl1 = ilvl;
            label = 5;
            goto L180;

            L570: fch = fm1 / 2.0;
            if (ctoi > ctoi1) ctoi1 = ctoi;
            if (ilvl < ilvl1) ilvl1 = ilvl;
            label = 6;
            goto L180;

            L580: fch = 5.0 * fm1 / 8.0;
            if (ctoi > ctoi1) ctoi1 = ctoi;
            if (ilvl < ilvl1) ilvl1 = ilvl;
            label = 7;
            goto L180;

            L590: fch = 3.0 * fm1 / 4.0;
            if (ctoi > ctoi1) ctoi1 = ctoi;
            if (ilvl < ilvl1) ilvl1 = ilvl;
            label = 8;
            goto L180;

            L591: fch = 7.0 * fm1 / 8.0;
            if (ctoi > ctoi1) ctoi1 = ctoi;
            if (ilvl < ilvl1) ilvl1 = ilvl;
            label = 9;
            goto L180;

            L592: fch = fm1;
            if (ctoi > ctoi1) ctoi1 = ctoi;
            if (ilvl < ilvl1) ilvl1 = ilvl;
            label = 10;
            goto L180;

            L600: if (fs < fmin1 || fs > fm1) goto L700;
            fch = fs;
            if (ctoi > ctoi1) ctoi1 = ctoi;
            if (ilvl < ilvl1) ilvl1 = ilvl;
            label = 11;
            /*;
            c      setup parameters to calculate ifch and esele;
            */
            ;
            L180:
            //...Log2.v("\nL180_ctoi1 = " + ctoi1);

            fchhz = fch * 1e6;
            fshz = fs * 1e6;
            for (i = 1; i <= np; i++)
            {
                freq[i] = freq1[i] * 1e6;
            }

            beta1 = fchhz - fs1hz;
            if (Abs(beta1) <= 1550.0)
            { /*221,221,222; */
                beta1 = 0.0;
            }
            beta2 = -fchhz - fs1hz;
            if (Abs(beta2) <= 1550.0)
            { /* 223,223,224;*/
                beta2 = 0.0;
            }
            beta = beta1;
            ibeta = 1;
            r[nps2] = fs1hz;
            s[nps2] = d3[1];
            if (nps2 > 1)
            {
                for (i = 2; i <= nps2; i++)
                {
                    r[nps2 + i - 1] = fs1hz + f3[i];
                    s[nps2 + i - 1] = d3[i];
                    r[nps2 - i + 1] = fs1hz - f3[i];
                    s[nps2 - i + 1] = d3[i];
                }
            }

            freq2[np] = freq[1];
            alpha2[np] = Pow(10.0, (-alpha1[1] / 10.0));
            for (i = 2; i <= np; i++)
            {
                freq2[np + i - 1] = freq[i];
                alpha2[np + i - 1] = Pow(10.0, (-alpha1[i] / 10.0));
                freq2[np - i + 1] = -freq[i];
                alpha2[np - i + 1] = Pow(10.0, (-alpha1[i] / 10.0));
            }

            if (r[1] >= freq2[1]) p = r[1];
            if (r[1] < freq2[1]) p = freq2[1];
            if (r[2 * nps2 - 1] > freq2[2 * np - 1]) goto L850;
            q = r[2 * nps2 - 1];
            goto L855;

            L850: q = freq2[2 * np - 1];
            L855: k1 = 1;
            j1 = 1;
            j2 = 1;
            ra[1] = p;
            sa[1] = DtaDvaln(freq2, alpha2, p, freq2.Length) *
                          DtaDvaln(r, s, p, r.Length);
            x = p;
            L860: if (freq2[j1] < x) goto L885;
            if (r[j2] < x) goto L895;
            if (freq2[j1] < q || r[j2] < q) goto L865;
            goto L899;

            L865: k1 = k1 + 1;
            if (r[j2] < freq2[j1])
            { /* 870,875,880; */
                ra[k1] = r[j2];
                sa[k1] = s[j2] * DtaDvaln(freq2, alpha2, r[j2], freq2.Length);
                x = r[j2];
                goto L895;
            }
            else if (r[j2] == freq2[j1])
            {
                ra[k1] = r[j2];
                sa[k1] = s[j2] * alpha2[j1];
                x = r[j2];
                goto L890;
            }
            else
            {   /* r[j2] > freq2[j1] */
                goto L880;
            }

            L880: ra[k1] = freq2[j1];
            sa[k1] = alpha2[j1] * DtaDvaln(r, s, freq2[j1], r.Length);
            x = freq2[j1];
            L885: j1 = j1 + 1;
            goto L860;

            L890: j1 = j1 + 1;
            L895: j2 = j2 + 1;
            goto L860;

            L899: ra[k1 + 1] = q;
            sa[k1 + 1] = DtaDvaln(r, s, q, r.Length) *
                             DtaDvaln(freq2, alpha2, q, freq2.Length);
            k2 = k1;
            /*;
            c       calculate ifch;
            */
            ;
            L256:;
            for (i = 1; i <= k1 + 1; i++)
            {
                f2[k1 + 2 - i] = fs1hz - (ra[i] - fs1hz);
                d2[k1 + 2 - i] = sa[i];
            }

            for (i = 1; i <= k1 + 1; i++)
            {
                f2[i] = beta - fs1hz + f2[i];
            }

            if (f1[1] <= f2[1])
            { /* 270,270,280;*/
                a = f2[1];
            }
            else
            {
                a = f1[1];
            }

            nn1 = 2 * nps1 - 1;
            nn2 = k1 + 1;
            if (f1[nn1] <= f2[nn2])
            {  /* 300,300,310; */
                b = f1[nn1];
            }
            else
            {
                b = f2[nn2];
            }

            if (b > a)
            { /* 430,430,330; */
                k3 = 1;
                j1 = 1;
                j2 = 1;
                f4[1] = a;

#if EMULATE_LEGACY_BUGS
                d4[1] = DtaDvaln(f2, d2, a, f2.Length) *
                              DtaDvaln(f1, d1, a, 1);
#else
                d4[1] = DtaDvaln(f2, d2, a, f2.Length ) *
                              DtaDvaln(f1, d1, a, f1.Length );
#endif

                //...Log2.v(String.Format("\nA_d4[1] = {0:E}", d4[1]));
                x = a;
                L340: if (f1[j1] < x) goto L390;
                if (f2[j2] < x) goto L410;
                if (f1[j1] < b || f2[j2] < b) goto L350;
                goto L420;

                L350: k3 = k3 + 1;
                if (f2[j2] < f1[j1])
                { /* 360,370,380; */
                    f4[k3] = f2[j2];

                    //...Log2.v(String.Format("\nB_d2[j2] = " + d2[j2]));
                    //...Log2.v(String.Format("\nB_f2[j2] = " + f2[j2]));
                    //...Log2.v(String.Format("\nB_Length = " + f1.Length));

#if EMULATE_LEGACY_BUGS
                    d4[k3] = d2[j2] * DtaDvaln(f1, d1, f2[j2], 1);
#else
                    d4[k3] = d2[j2] * DtaDvaln(f1, d1, f2[j2], f1.Length );
#endif

                    //...Log2.v(String.Format("\nB_d4[k3] = {0:E}", d4[k3]));
                    x = f2[j2];
                    goto L410;
                }
                else if (f2[j2] == f1[j1])
                {
                    f4[k3] = f2[j2];
                    d4[k3] = d2[j2] * d1[j1];
                    //...Log2.v(String.Format("\nC_d4[k3] = {0:E}", d4[k3]));
                    x = f2[j2];
                    goto L400;
                }
                else
                {
                    goto L380;
                }

                L380: f4[k3] = f1[j1];
                d4[k3] = d1[j1] * DtaDvaln(f2, d2, f1[j1], f2.Length);
                //...Log2.v(String.Format("\nD_d4[k3] = {0:E}", d4[k3]));
                x = f1[j1];
                L390: j1 = j1 + 1;
                goto L340;

                L400: j1 = j1 + 1;
                L410: j2 = j2 + 1;
                goto L340;

                L420: f4[k3 + 1] = b;

#if EMULATE_LEGACY_BUGS
                DtaDvaln(f1, d1, b, f1.Length);
                d4[k3 + 1] = DtaDvaln(f2, d2, b, f2.Length) *
                                  DtaDvaln(f1, d1, b, 1);
#else
                DtaDvaln(f1, d1, b, f1.Length);
                d4[k3 + 1] = DtaDvaln(f2, d2, b, f2.Length) *
                                  DtaDvaln(f1, d1, b, f1.Length);
#endif

                //...Log2.v(String.Format("\nE_d4[k3 + 1] = {0:E}", d4[k3 + 1]));
                area = 0.0;
                for (i = 1; i <= k3; i++)
                {
                    area += (d4[i] + d4[i + 1]) * (f4[i + 1] - f4[i]) / 2.0;
                }
                goto L431;
            }

            area = 0.0;
            L431:;
            for (i = 1; i < 5 * Constant.MAX_FILTER; i++)
            {
                f2[i] = 0.0;
                d2[i] = 0.0;
            }

            for (i = 1; i < 7 * Constant.MAX_FILTER; i++)
            {
                f4[i] = 0.0;
                d4[i] = 0.0;
            }

            if (ibeta == 1)
            { /* 460,450,460; */
                ibeta = 2;
                beta = beta2;
                cval1 = area * 2.0;
                goto L256;
            }

            cval2 = area * 2.0;
            maxi = -174.0 + 10.0 * Log10(2.0 * (3.76 * sigma1 * 1e6 + 2.0 * fm1hz)) + nf - 5.87;
            ifch = 0.25 * (cval1 + cval2);

            sele = 0.0;
            for (i = 1; i <= k2 + 1; i++)
            {
                r2a[i] = ra[i];
                s2a[i] = sa[i];
            }

            k1 = k2;

            for (i = 1; i <= k1; i++)
            {
                sele = sele + (0.5 * (s2a[i] + s2a[i + 1]) * (r2a[i + 1] - r2a[i]));
            }

            esele = -10.0 * Log10(sele);
            if (ifch > 0.0)
            {
                /*	This code has been added by GJS upon encountering ifchs of 0.0.
				*		The logic is copied from suppata.c and is intended to cover
				*		the situation where the area under the curve is zero.
				*		1208 - GJS - 2006.03.15 */
                hpf = 1.0 + 5.25 / Pow((1.25 * fm1 / fch - fch / (1.25 * fm1)), 2.0);
                hpf = 5.0 - 10.0 * Log10(1.0 + 6.9 / hpf);
                fmin1h = fmin1 * 1e6;
                //npr0 = 10.0 * Log10(m1 * m1 * fm1hz /
                //                          ((1.0 - fmin1h / fm1hz) * ifch * fchhz * fchhz));
                //double term = m1 * m1 * fm1hz / ((1.0 - fmin1h / fm1hz) * ifch * fchhz * fchhz);

                double X = m1 * m1 * fm1hz;
                double Y = (1.0 - fmin1h / fm1hz);

                double Z = ifch * fchhz * fchhz;
                double term = X / (Y * Z);

                npr0 = 10.0 * Log10(term);

                npr0 = npr0 + hpf;
                bwr1 = 10.0 * Log10((fm1hz - fmin1h) / 3100.0);
                ctoi = 84.0 - npr0 - bwr1 + nlr1 - c70 - c140;

            }
            else
            {
                /*	Make it a default maximum */
                ctoi = 50.0 - esele - c70 - c140;
                //...Log2.v(String.Format("\nBAT_ctoi = {0}", ctoi));
            }
/*c471  if(alpha1[np] == 0.0)goto L490 */;
            if (ctoi >= (50.0 - esele - c70 - c140)) goto L490;
            ctoi = 50.0 - esele - c70 - c140;
            //...Log2.v(String.Format("\nCAT_ctoi = {0}", ctoi));

            L490: ilvl = npr0 - 139.1 + nf - 20.0 * Log10(dsig1 / fm1) + bwr1 - nlr1 - 4.01 - 5.87;
            ilvl = ilvl + c70 + c140;
            /*;
            c     determines if threshold degradation is controling;
            */
            /* c472  if(alpha1[np] == 0.0)goto L500; */
            if (ilvl <= (maxi + esele + c70 + c140)) goto L500;
            ilvl = maxi + esele + c70 + c140;
            /*
            c      loop for determining worst baseband channel;
            */
            L500: /* goto (720,540,550,560,570,580,590,591,592,600,700),label */;
            switch (label)
            {
                case 1:
                    goto L720;

                case 2:
                    goto L540;

                case 3:
                    goto L550;

                case 4:
                    goto L560;

                case 5:
                    goto L570;

                case 6:
                    goto L580;

                case 7:
                    goto L590;

                case 8:
                    goto L591;

                case 9:
                    goto L592;

                case 10:
                    goto L600;

                case 11:
                    goto L700;

                default:
                    break;
            }

            L700: if (ctoi >= ctoi1) goto L710;
            ctoi = ctoi1;
            //...Log2.v(String.Format("\nDOG_ctoi = {0}", ctoi));

            L710: if (ilvl <= ilvl1) goto L715;
            ilvl = ilvl1;
            /*;
            c     loop to determine if spurious responses are controlling in;
            c      critical ranges.;
            */
            ;
            L715: if (flag == 1 || flag == 2)
            {
                fs1 = fs2;
                fs1hz = fs2hz;
                c70 = 0;
                c140 = 0;
                ilvl3 = ilvl;
                ctoi3 = ctoi;
                flag = 0;
                goto L530;
            }

            if (ilvl <= ilvl3) goto L719;
            ilvl = ilvl3;
            L719: if (ctoi > ctoi3) goto L720;
            ctoi = ctoi3;
            //...Log2.v(String.Format("\nEMU_ctoi = {0}", ctoi));
            L720:  /* continue */;
            return;
        }

        /// <summary>
        /// This method is used to generate ctx curves for digital to analog 
        /// interference cases. options available are: default filter specific filter 
        /// no filter default digital spectrum specific digital spectrum if reponse 
        /// factor default if response factor image response factor default response 
        /// factor notes: abw1 - authorized bandwidth of digital interferer (mhz) 
        /// caution: in the calculated requirements for the first or second if 
        /// frequencies are more lax than those of the neighbouring frequencies, then 
        /// these values should not be used for the ctx curve. instead, interpolation 
        /// of the neighbouring values should be used. the more optimistic values are 
        /// likely results of optimistic estimations of the xif responses. converted 
        /// by: kourosh agharazi date: march 6, 1989 - from ibm mainframe to vax 
        /// 11/750. revised by: kourosh agharazi date: april 28, 1989 - added freq1(2) 
        /// to freq1(i) statement. - removed 'fsym' and changed calculations to be 
        /// based on 'abw'. - changed flow to accommodate specific xif and image 
        /// response factors. - changed the display of the output. revised by:l. abbott 
        /// date :1989 08 09 changes :(to fcsa june 21 version) - set limit at 300 
        /// -changed digital default spectrum per 'doratd' program -changed specific 
        /// if/image response variables to ans5 & ans6 to avoid conflict with existing 
        /// variables. - added calculation of ai70 & ai140 based on filter data. 
        /// -changed summary report format -changed order of ctoi & ilvl in output 
        /// statement -reduced number of data points -changed flow to calculate a70 & 
        /// a140 in main program -changed flow to allow input of specific if & image 
        /// response when ans3=1. -changed flow of covri3 to do a shift of xif & 2xif 
        /// and compare with unshifted results -increased width of if & image response 
        /// critical zones to +/-xif/2 -corrected default interferer spectrum and added 
        /// portion at 50 db attenuation from y(3) to y(4) -derived spurious response 
        /// attenuation for default from one half of default selectivity. (89 09 12) 
        /// -used esele to calculate threshold degradation part i. e. not used in the 
        /// 'sideband overlap' calculation. (89 09 12) -corrected bug in calculation of 
        /// ai70 & ai140 from selectivity information for default case. (89 11 16)lca 
        /// -corrected bug that truncated if response incorrectly for c/i curves only. 
        /// (89 11 27) lca Converted to C - Orthogonal Endeavours Ltd. , North 
        /// Vancouver FCSA Task 1083, 2003 07.  
        /// </summary>
        /// <param name="f"></param>
        /// <param name="d"></param>
        /// <param name="fval"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static double DtaDvaln(double[] f, double[] d, double fval, int n)
        {
            //...Log2.v(String.Format("\ndtaDvaln(): n = {0}", n));

            double dval2;
            int i;
            int m;
            int j;

            for (i = 1; i <= n; i++)
            {
                if (d[i] == 0)
                {
                    break;
                }
            }
            n = i - 1;

            if (fval < f[1] || fval > f[n]) goto L100;
            for (m = 1; m <= n; m++)
            {
                j = m;
                if (fval > f[m]) goto L110;
                if (m != 1) goto L120;
                goto L140;
                L110:  /* continue */;
            }
            L100: dval2 = 0.0;
            return dval2;
            L140: dval2 = d[1];
            return dval2;
            L120: dval2 = (d[j] - d[j - 1]) / (f[j] - f[j - 1]) * (fval - f[j - 1]) + d[j - 1];
            return dval2;
        }

    }
}

```
