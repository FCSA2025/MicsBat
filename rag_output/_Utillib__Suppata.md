# Documented File: Suppata.cs
**Repository Path:** `_Utillib\Suppata.cs`
**Primary Layer:** `_Utillib`
**Namespace:** `_Utillib`

## Source Code Representation
```csharp
using _Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Math;

namespace _Utillib
{
    public class Suppata
    {
        private static double[] f1 = new double[48];
        private static double[] d1 = new double[48];
        private static double[] f3 = new double[25];
        private static double[] d3 = new double[25];
        private static double nlr1, nlr2, m1, m2, sigma1, sigma2;
        private static double xn1, xn2;
        private static int nps1;
        private static int nps2;
        private static double fm1hz, fm2hz;
        private static double fm2 = 0.0;
        private static int i;

        private static double[] f1_ = new double[9] { 0, 0.0, 0.1, 0.2, 0.3, 0.4, 0.6, 0.8, 1.0 };

        /*	The following is the array transposed */
        private static double[,] d1_ = new double[9, 9] { { 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                                               {0, 7.0,  3.2, 1.0, -0.8, -2.1, -3.5, -4.8, -5.8},
                                               {0, 2.0, 0.5, 0.2, -1.0, -2.2, -3.6, -4.8, -5.8},
                                               {0, -2.5, -2.0, -0.7, -1.4, -2.4, -3.8, -4.8, -5.8},
                                               {0, -5.8, -3.9, -1.9, -1.9, -2.7, -4.0, -4.9, -5.8},
                                               {0, -8.0, -5.8, -3.2, -2.6, -3.1, -4.3, -5.0, -5.9},
                                               {0, -11.8, -9.3, -6.0, -4.5, -4.5, -5.0, -5.5, -6.1},
                                               {0, -14.5, -12.8, -9.0, -7.0, -6.2, -6.0, -6.0, -6.4},
                                               {0, -18.7, -16.0, -12.0, -9.8, -8.4, -7.4, -6.8, -6.7}
                                             };

        private static double[,] f2 = new double[7, 9] {{0, 0, 0, 0, 0, 0, 0, 0, 0},
                                            {0, 1.03, 1.23, 1.49, 1.68, 1.87, 2.27, 2.73, 3.84},
                                            {0, 1.35, 1.76, 2.05, 2.29, 2.51, 3.04, 3.67, 5.01},
                                            {0, 1.78, 2.22, 2.56, 2.85, 3.15, 3.80, 4.46, 5.96},
                                            {0, 2.20, 2.66, 3.02, 3.39, 3.75, 4.46, 5.16, 6.77},
                                            {0, 2.60, 3.06, 3.47, 3.90, 4.28, 5.10, 5.81, 7.50},
                                            {0, 2.98, 3.44, 3.90, 4.36, 4.81, 5.72, 6.43, 8.16}
                                           };

        private static double[] d2 = new double[7] { 0, -20.0, -30.0, -40.0, -50.0, -60.0, -70.0 };

        private static double[] m1_ = new double[9] { 0, 0.2, 0.3, 0.4, 0.5, 0.6, 0.8, 1.0, 1.5 };

        private static double[] f5 = new double[49];
        private static double[] d5 = new double[49];

        private static double[] f4 = new double[96];
        private static double[] d4 = new double[96];

        /// <summary>
        /// Calculates the analog into analog interference requirement using the AGT 
        /// routines.  
        /// </summary>
        /// <param name="n1_"></param>
        /// <param name="fmin1_"></param>
        /// <param name="fm1_"></param>
        /// <param name="dsig1_"></param>
        /// <param name="nf_"></param>
        /// <param name="n2_"></param>
        /// <param name="fmin2_"></param>
        /// <param name="fm2_"></param>
        /// <param name="dsig2_"></param>
        /// <param name="fs_"></param>
        /// <param name="ctoi_"></param>
        /// <param name="ilvl_"></param>
        /// <returns></returns>
        public static int SuppAta(int n1_,
                        double fmin1_,
                        double fm1_,
                        double dsig1_,
                        int nf_,
                        int n2_,
                        double fmin2_,
                        double fm2_,
                        double dsig2_,
                        double fs_,
                        out double ctoi_,
                        out double ilvl_)
        {
            // 'out' requirements.
            //ctoi = 0.0;
            ilvl_ = 0.0;

            xn1 = n1_;
            xn2 = n2_;
            if (n1_ < 240)
            {
                nlr1 = 4.0 * Log10(xn1) - 1.0;
            }
            else
            {
                nlr1 = 10.0 * Log10(xn1) - 15.0;
            }
            sigma1 = dsig1_ * Pow(10.0, (.05 * nlr1));
            m1 = sigma1 / fm1_;
            if (n2_ < 240)/* 130,140,140 */
            {
                nlr2 = 4.0 * Log10(xn2) - 1.0;
            }
            else
            {
                nlr2 = 10.0 * Log10(xn2) - 15.0;
            }
            sigma2 = dsig2_ * Pow(10.0, (.05 * nlr2));
            m2 = sigma2 / fm2_;

            AtaSpect(ref f3, ref d3, m1, ref nps1, fmin1_, fm1_);

            fm1hz = fm1_ * 1.0e6;
            f1[nps1] = 0.0;
            d1[nps1] = Pow(10.0, (0.1 * d3[1])) / fm1hz;
            if (nps1 > 1)
            { /* 210,210,190;	*/
                for (i = 2; i <= nps1; i++)
                {
                    f1[nps1 + i - 1] = f3[i] * fm1hz;
                    d1[nps1 + i - 1] = Pow(10.0, (0.1 * d3[i])) / fm1hz;
                    f1[nps1 - i + 1] = -f1[nps1 + i - 1];
                    d1[nps1 - i + 1] = d1[nps1 + i - 1];
                }
            }

            AtaSpect(ref f3, ref d3, m2, ref nps2, fmin2_, fm2_);

            fm2hz = fm2 * 1.0e6;
            for (i = 1; i <= nps2; i++)
            {
                f3[i] = f3[i] * fm2hz;
                d3[i] = Pow(10.0, (0.1 * d3[i])) / fm2hz;
            }

            Covri1(fmin1_, fm1_, dsig1_, nf_, fs_, out ctoi_, out ilvl_,
                   nlr1, sigma1, m1, f3, d3, nps1, fm1hz, f1, d1, nps2);

            return Constant.SUCCESS;
        }

        /// <summary>
        /// </summary>
        /// <param name="ff"></param>
        /// <param name="d"></param>
        /// <param name="m"></param>
        /// <param name="nps"></param>
        /// <param name="fmin"></param>
        /// <param name="fm"></param>
        public static void AtaSpect(ref double[] ff,
                                    ref double[] d,
                                    double m,
                                    ref int nps,
                                    double fmin,
                                    double fm)
        {

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

            int i_;
            int j;

            if (m >= 1.5)
            { /*130,100,100;*/
                for (i_ = 1; i_ <= 13; i_++)
                {
                    ff[i_] = i_ - 1;
                    d[i_] = Exp(-ff[i_] * ff[i_] / (2.0 * m * m)) / (m * Sqrt(2.0 * 3.14159));
                    d[i_] = 10.0 * Log10(d[i_]);
                    if (d[i_] <= -70.0)
                    { /*120,120,110; */
                        break;
                    }
                }

                nps = i_;
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
                for (i_ = 1; i_ <= 20; i_++)
                {
                    xi = i_;
                    ff[i_ + 4] = x1 + (xi - 1.0) / 20.0 * (2.0 - x1);
                    x = ff[i_ + 4];
                    d[i_ + 4] = 0.0;
                    if (ff[i_ + 4] > 1.0) goto L150;
                    hpf = 1.0 + 5.25 / Pow((1.25 / x - x / 1.25), 2.0);
                    hpf = 3.15 / (1.0 + 6.90 / hpf);
                    d[i_ + 4] = 0.5 * m * m / ((1.0 - x1) * x * x);
                    d[i_ + 4] = d[i_ + 4] * hpf;
                    L150: ff2 = 0.0;
                    if (ff[i_ + 4] < 2.0 * x1 || ff[i_ + 4] > x1 + 1.0) goto L160;
                    ff2 = ff2 + x * (x - 2.0 * x1) / (x1 * (x - x1)) + 2.0 * Log((x - x1) / x1);
                    L160: if (ff[i_ + 4] < x1 + 1.0) goto L170;
                    ff2 = ff2 + x * (2.0 - x) / (x - 1.0) + 2.0 * Log(1.0 / (x - 1.0));
                    L170: ff2 = ff2 * 0.25 * Pow(m, 4.0) / (Pow((1.0 - x1), 2.0) * Pow(x, 3.0));
                    d[i_ + 4] = d[i_ + 4] + ff2;
                    d[i_ + 4] = 10.0 * Log10(d[i_ + 4] * e);
                    if (d[i_ + 4] <= -70.0)
                    {
                        break;
                    }
                }

                nps = i_ + 4;
                if (nps > 24) nps = 24;
                return;
            }


            for (i_ = 2; i_ <= 8; i_++)
            {
                ff[i_ + 2] = f1_[i_];
            }

            for (i_ = 2; i_ <= 8; i_++)
            {
                if (m <= m1_[i_]) goto L230;
                /* continue */
                ;
            }
            if (i_ > 8) i_ = 8;
            L230: fact = (m - m1_[i_ - 1]) / (m1_[i_] - m1_[i_ - 1]);

            j = i_;
            for (i_ = 1; i_ <= 8; i_++)
            {
                d[i_ + 2] = d1_[i_, j - 1] + (d1_[i_, j] - d1_[i_, j - 1]) * fact;
            }

            for (i_ = 1; i_ <= 6; i_++)
            {
                d[i_ + 10] = d2[i_];
                ff[i_ + 10] = f2[i_, j - 1] + (f2[i_, j] - f2[i_, j - 1]) * fact;
            }


            nps = 16;
            /*
            c     code to be added to the ataSpect subroutine to compensate;
            c     for the error in power spectra.;
            c     initialize area;
            */
            area = 0.0;
            for (i_ = 2; i_ <= nps; i_++)
            {
                area += (Pow(10.0, (.1 * d[i_])) + Pow(10.0, (.1 * d[i_ - 1]))) *
                              (ff[i_] - ff[i_ - 1]);
            }

            /*Console.Write("\nArea = {0,6:F3}", area); */

            for (i_ = 1; i_ <= nps; i_++)
            {
                d[i_] = d[i_] - 10.0 * Log10(area);
            }
            /*
            c     ff(i_) is the normalized frequency (f/fmax);
            c     d(i_) is fmax times p(f) (spectral value in db);
            c     then, area between ff(i_-1) and ff(i_) is given by:;
            c     area = ect.;
            c     and the total power is the sum of all the areas multiplied by 2,;
            c     since the spectra are stored as half-spectra (0 hz to infinity);
            */

            return;
        }

        // The provenance of this method is obviously derived from some Stone Age FORTRAN
        // code. It is riddled with 'goto' statements and incomprehensible in its present form.
        /// <summary>
        /// </summary>
        /// <param name="fmin1_"></param>
        /// <param name="fm1_"></param>
        /// <param name="dsig1_"></param>
        /// <param name="nf_"></param>
        /// <param name="fs_"></param>
        /// <param name="ctoi_"></param>
        /// <param name="ilvl_"></param>
        /// <param name="nlr1_"></param>
        /// <param name="sigma1_"></param>
        /// <param name="m1_"></param>
        /// <param name="f3_"></param>
        /// <param name="d3_"></param>
        /// <param name="nps1_"></param>
        /// <param name="fm1hz_"></param>
        /// <param name="f1_"></param>
        /// <param name="d1_"></param>
        /// <param name="nps2_"></param>
        public static void Covri1(double fmin1_, double fm1_, double dsig1_, int nf_,
                                    double fs_,
                                    out double ctoi_, out double ilvl_,
                                    double nlr1_, double sigma1_, double m1_,
                                    double[] f3_, double[] d3_, int nps1_,
                                    double fm1hz_, double[] f1_, double[] d1_, int nps2_
                                 )
        {
            double fco, fch, fmin1h, a, b, x, alpha, beta, maxi;
            double ilvl1 = 0.0;
            double beta1, beta2, ibeta, spike, cval2;
            double cval1 = 0.0;
            double ctoi1 = 0.0;
            double ifch, bwr1, npr0, hpf, a70, fchhz, fshz, area;
            int label;

            int nn1;
            int nn2;
            int k1;
            int j1;
            int j2;

            int nInd;

            /*******************************************************************/
            /*	Zero the internal arrays */
            for (nInd = 0; nInd <= 48; nInd++)
            {
                f5[nInd] = 0.0;
                d5[nInd] = 0.0;
            }

            for (nInd = 0; nInd <= 95; nInd++)
            {
                f4[nInd] = 0.0;
                d4[nInd] = 0.0;
            }

            alpha = 0.0;
            a70 = 0.0;
            fco = 1.5 * (3.76 * sigma1_ + 2.0 * fm1_);

            if (fs_ > fco)
            {
                alpha = 25.0 * Log10(fs_ / fco) / Log10(2.0);
                if (alpha > 60.0)
                {
                    alpha = 60.0;
                }
            }

            if (fs_ >= 70.0 - fm1_ && fs_ <= 70.0 + fm1_)
            {
                a70 = 20.0;
                fs_ = Abs(70.0 - fs_);
            }

            if (fs_ >= 140.0 - fm1_ && fs_ <= 140.0 + fm1_)
            {
                fs_ = Abs(140.0 - fs_);
            }

            label = 1;
            if (fs_ < fmin1_)
            {
                fch = fm1_ / 2.0;
                label = 5;
                goto L180;

                fch = fm1_ / 4.0;
                label = 4;
                ctoi1 = ctoi_;
                ilvl1 = ilvl_;
                goto L180;
            };

            if (fs_ < fm1_ / 2.0)
            { /*560,580,580;*/
                fch = fs_;
                label = 2;
                goto L180;

                // L570:  this label is never referenced.
                fch = fm1_ / 2.0;
                label = 6;
                ctoi1 = ctoi_;
                ilvl1 = ilvl_;
                goto L180;

                fch = fm1_ / 4.0;
                label = 4;
                if (ctoi1 >= ctoi_) goto L180;
                ctoi1 = ctoi_;
                ilvl1 = ilvl_;
                goto L180;
            }


            if (fs_ <= fm1_)
            { /*590,590,600; */
                fch = fs_;
                goto L180;
            }

            if (fs_ < fm1_ + fmin1_)
            { /*610,630,630;*/
                fch = fs_ - fmin1_;
                label = 3;
                goto L180;

                fch = fm1_;
                label = 4;
                ctoi1 = ctoi_;
                ilvl1 = ilvl_;
                goto L180;
            }


            fch = fm1_;

            /*	Start of a subroutine loop */
            L180: fchhz = fch * 1.0e6;
            fshz = fs_ * 1.0e6;
            beta1 = fchhz - fshz;
            if (Abs(beta1) <= 1550.0)
            { /*221,221,222;*/
                beta1 = 0.0;
            }

            beta2 = -fchhz - fshz;
            if (Abs(beta2) <= 1550.0)
            { /*223,223,224; */
                beta2 = 0.0;
            }

            beta = beta1;
            ibeta = 1;
            L230: f5[nps2_] = beta;
            d5[nps2_] = d3_[1];
            if (nps2_ > 1)
            { /*260,260,240; */
                for (i = 2; i <= nps2_; i++)
                {
                    f5[nps2_ + i - 1] = beta + f3_[i];
                    d5[nps2_ + i - 1] = d3_[i];
                    f5[nps2_ - i + 1] = beta - f3_[i];
                    d5[nps2_ - i + 1] = d3_[i];
                }
            }

            if (f1_[1] <= f5[1])
            { /* 270,270,280; */
                a = f5[1];
            }
            else
            {
                a = f1_[1];
            }

            nn1 = 2 * nps1_ - 1;
            nn2 = 2 * nps2_ - 1;
            if (f1_[nn1] <= f5[nn2])
            { /*300,300,310;*/
                b = f1_[nn1];
            }
            else
            {
                b = f5[nn2];
            }

            if (b > a)
            { /*430,430,330; */
                k1 = 1;
                j1 = 1;
                j2 = 1;
                f4[1] = a;
                d4[1] = Dval(f5, d5, a) * Dval(f1_, d1_, a);
                x = a;

                L340: if (f1_[j1] <= x) goto L390;
                if (f5[j2] <= x) goto L410;
                if (f1_[j1] < b || f5[j2] < b) goto L350;
                goto L420;

                L350: k1 = k1 + 1;
                if (Abs(f5[j2] - f1_[j1]) <= .00000005) goto L370;
                if (f5[j2] < f1_[j1]) goto L360;
                if (f5[j2] > f1_[j1]) goto L380;

                L360: f4[k1] = f5[j2];
                d4[k1] = d5[j2] * Dval(f1_, d1_, f5[j2]);
                x = f5[j2];
                goto L410;

                L370: f4[k1] = f5[j2];
                d4[k1] = d5[j2] * d1_[j1];
                x = f5[j2];
                goto L400;

                L380: f4[k1] = f1_[j1];
                d4[k1] = d1_[j1] * Dval(f5, d5, f1_[j1]);
                x = f1_[j1];
                L390: j1 = j1 + 1;
                goto L340;

                L400: j1 = j1 + 1;
                L410: j2 = j2 + 1;
                goto L340;

                L420: f4[k1 + 1] = b;
                d4[k1 + 1] = Dval(f5, d5, b) * Dval(f1_, d1_, b);
                area = 0.0;

                for (i = 1; i <= k1; i++)
                {
                    area += (d4[i] + d4[i + 1]) * (f4[i + 1] - f4[i]) / 2.0;
                }

                if (beta != 0)
                { /*440,426,440;*/
                    goto L440;
                }

                if (f1_[nps1_ + 1] < 0.51 && f5[nps2_ + 1] < 0.51) goto L427;

                goto L440;

                L427: spike = (d1_[nps1_] - d1_[nps1_ + 2]) * (d5[nps2_] - d5[nps2_ + 2]);
                area += -spike + spike / 3100.0 - 9.0 * d1_[nps1_] * d5[nps2_] / 31000.0;
                goto L440;
            }

            area = 0.0;
            L440: if (ibeta == 1)
            { /*460,450,460;*/
                ibeta = 2;
                beta = beta2;
                cval1 = area * 2.0;
                goto L230;
            }

            cval2 = area * 2.0;
            maxi = -174.0 + 10.0 * Log10(2.0 * (3.76 * sigma1_ * 1.0e6 + 2.0 * fm1hz_)) + nf_ - 5.87;
            ifch = 0.25 * (cval1 + cval2);
            if (ifch > 0)
            { /*480,480,470;*/
                hpf = 1.0 + 5.25 / Pow((1.25 * fm1_ / fch - fch / (1.25 * fm1_)), 2.0);
                hpf = 5.0 - 10.0 * Log10(1.0 + 6.9 / hpf);
                fmin1h = fmin1_ * 1.0e6;
                npr0 = 10.0 * Log10(m1_ * m1_ * fm1hz_ / ((1.0 - fmin1h / fm1hz_) * ifch * fchhz * fchhz));
                npr0 = npr0 + hpf;
                bwr1 = 10.0 * Log10((fm1hz_ - fmin1h) / 3100.0);
                ctoi_ = 84.0 - npr0 - bwr1 + nlr1_ - alpha - a70;

                if (ctoi_ >= 50.0 - alpha)
                { /*481,490,490;*/
                    goto L490;
                }
                else
                {
                    goto L481;
                }
            }

            ctoi_ = 50.0 - alpha;
            ilvl_ = maxi + alpha;
            goto L510;

            L481: ctoi_ = 50.0 - alpha;
            L490: ilvl_ = npr0 - 139.1 + nf_ - 20.0 * Log10(dsig1_ / fm1_) + bwr1 - nlr1_ - 4.01 - 5.87;
            ilvl_ = ilvl_ + alpha + a70;
            if (ilvl_ > maxi + alpha)
            {  /*510,510,500;*/
                ilvl_ = maxi + alpha;
            }

            L510:
            switch (label)
            {
                case 1:
                    goto L710;
                case 2:
                    fch = fm1_ / 2.0;
                    label = 6;
                    ctoi1 = ctoi_;
                    ilvl1 = ilvl_;
                    goto L180;
                case 3:
                    fch = fm1_;
                    label = 4;
                    ctoi1 = ctoi_;
                    ilvl1 = ilvl_;
                    goto L180;
                case 4:
                    goto L700;
                case 5:
                    fch = fm1_ / 4.0;
                    label = 4;
                    ctoi1 = ctoi_;
                    ilvl1 = ilvl_;
                    goto L180;
                case 6:
                    fch = fm1_ / 4.0;
                    label = 4;
                    if (ctoi1 >= ctoi_) goto L180;
                    ctoi1 = ctoi_;
                    ilvl1 = ilvl_;
                    goto L180;
                default:
                    break;
            }

            L700:
            if (ctoi_ >= ctoi1) goto L710;
            ctoi_ = ctoi1;
            ilvl_ = ilvl1;
            L710: return;
        }

        /// <summary>
        /// </summary>
        /// <param name="f"></param>
        /// <param name="d"></param>
        /// <param name="fval_"></param>
        /// <returns></returns>
        public static double Dval(double[] f, double[] d, double fval_)
        {
            return (AtaDvaln(f, d, fval_, 47));
        }

        /// <summary>
        /// </summary>
        /// <param name="f"></param>
        /// <param name="d"></param>
        /// <param name="fval_"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static double AtaDvaln(double[] f, double[] d, double fval_, int n)
        {
            double dval2;
            int i_;
            int m;
            int j;

            for (i_ = 1; i_ <= n; i_++)
            {
                if (d[i_] == 0)
                {
                    break;
                }
            }
            n = i_ - 1;

            if (fval_ < f[1] || fval_ > f[n]) goto L100;
            for (m = 1; m <= n; m++)
            {
                j = m;
                if (fval_ > f[m]) goto L110;
                if (m != 1) goto L120;
                goto L140;
                L110:  /* continue */;
            }
            L100: dval2 = 0.0;
            return dval2;
            L140: dval2 = d[1];
            return dval2;
            L120: dval2 = (d[j] - d[j - 1]) / (f[j] - f[j - 1]) * (fval_ - f[j - 1]) + d[j - 1];
            return dval2;
        }




    }
}

```
