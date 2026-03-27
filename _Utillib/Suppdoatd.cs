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
    public class Suppdoatd
    {
        /// <summary>
        /// </summary>
        /// <param name="abw"></param>
        /// <param name="nf"></param>
        /// <param name="xif"></param>
        /// <param name="ai70"></param>
        /// <param name="ai140"></param>
        /// <param name="thdcrit"></param>
        /// <param name="sif"></param>
        /// <param name="irf"></param>
        /// <param name="nSelect"></param>
        /// <param name="freq1in"></param>
        /// <param name="alpha1in"></param>
        /// <param name="dora"> - 1 - D into D, 0 - A into D</param>
        /// <param name="n2"></param>
        /// <param name="fmin2"></param>
        /// <param name="fm2"></param>
        /// <param name="dsig2"></param>
        /// <param name="nDefaultSpect"></param>
        /// <param name="y2in"></param>
        /// <param name="z2in"></param>
        /// <param name="abw1"></param>
        /// <param name="innth"></param>
        /// <param name="fs"></param>
        /// <param name="IsEs"></param>
        /// <param name="ilvl"></param>
        /// <returns></returns>
        public static int SuppDoatd(double abw,
              double nf,
                            double xif,
                            double ai70,
                            double ai140,
                            double thdcrit,
                            double sif,
                            double irf,
                            int nSelect,
                            double[] freq1in,
                            double[] alpha1in,
                            int dora,                   /* 1 - D into D, 0 - A into D */
                            double n2,
                            double fmin2,
                            double fm2,
                            double dsig2,
                            int nDefaultSpect,
                            double[] y2in,
                            double[] z2in,
                            double abw1,
                            double innth,
                            double fs,
                            bool IsEs,
                            out double ilvl
                            )
        {
            // 'out' requirement.
            ilvl = 0.0;

            double[] f2 = new double[Constant.MAX_SPECTRUM];
            double[] freq = new double[2 * Constant.MAX_FILTER];
            double[] r2 = new double[2 * Constant.MAX_SPECTRUM];
            double[] r = new double[2 * Constant.MAX_SPECTRUM];
            double[] ra = new double[3 * Constant.MAX_SPECTRUM];
            double[] freq2 = new double[2 * Constant.MAX_FILTER];
            double[] r2a = new double[3 * Constant.MAX_SPECTRUM];
            double[] f5 = new double[Constant.MAX_FILTER];
            double p = 0.0;
            double q;
            double x;
            double[] freq3 = new double[Constant.MAX_FILTER];
            double[] f2a = new double[2 * Constant.MAX_SPECTRUM];
            double[] d2 = new double[Constant.MAX_SPECTRUM];
            double[] alpha = new double[2 * Constant.MAX_FILTER];
            double[] s2 = new double[2 * Constant.MAX_SPECTRUM];
            double[] s = new double[2 * Constant.MAX_SPECTRUM];
            double[] sa = new double[3 * Constant.MAX_SPECTRUM];
            double[] alpha2 = new double[2 * Constant.MAX_FILTER];
            double[] s2a = new double[3 * Constant.MAX_SPECTRUM];
            double[] d5 = new double[Constant.MAX_FILTER];
            double[] alpha3 = new double[Constant.MAX_FILTER];
            double[] d2a = new double[2 * Constant.MAX_SPECTRUM];
            double iobj;
            double ilvl2;
            double m2;
            double minatt;
            double atti;
            double att2i;

            double[] freq1 = new double[Constant.MAX_FILTER];
            double[] alpha1 = new double[Constant.MAX_FILTER];
            double[] y2 = new double[Constant.MAX_SPECTRUM];
            double[] z2 = new double[Constant.MAX_SPECTRUM];

            //int ans1;
            //int ans2;
            int ans3;
            //int ans4;
            bool ans;

            int i;
            int np;
            int nps2 = 0;
            int k1;
            int k2;
            int j1;
            int j2;

            double ulh;
            double nlr2;
            double esele;
            double esele1;
            double sele;
            double sele1;
            double a70;
            double a140;
            double fs1;
            double fshz;
            double fs1hz;
            double corr = 0.0;
            double bwhz;
            double pwr;
            double bw1hz;
            double bw1;
            double bw2hz;
            double bw2;
            double bw;
            double fm2hz;
            double fl;
            double fh;
            double sigma2;
            double fm1;
            double span;
            double xi;
            double df;
            double xn2;
            double nth = 0.0;

            int nInd;
            double dMaxFS = (fs > Constant.MAX_FREQ_SEP_MHZ) ? fs : Constant.MAX_FREQ_SEP_MHZ;

            /*	Do some sanity checks */
            if (nSelect >= Constant.MAX_FILTER)
            {
                /*	Filter array too large */
                ilvl = 0.0;
                return (-22);
            }

            if (dora == 1 && nDefaultSpect >= Constant.MAX_SPECTRUM)
            {
                /*	We have digital interference and power spectrum too large */
                ilvl = 0.0;
                return (-23);
            }

            for (nInd = 0; nInd < 2 * Constant.MAX_SPECTRUM; nInd++)
            {
                r[nInd] = 0.0;
                s[nInd] = 0.0;
                r2[nInd] = 0.0;
                s2[nInd] = 0.0;
            }
            for (nInd = 0; nInd < 2 * Constant.MAX_FILTER; nInd++)
            {
                freq[nInd] = 0.0;
                alpha[nInd] = 0.0;
                freq2[nInd] = 0.0;
                alpha2[nInd] = 0.0;
            }


            //ans4 = 9;
            for (i = 1; i < Constant.MAX_FILTER; i++)
            {
                f5[i] = 0.0;
                d5[i] = 0.0;
            }
            for (i = 1; i < Constant.MAX_SPECTRUM; i++)
            {
                f2[i] = 0.0;
                d2[i] = 0.0;
            }

            fm1 = abw;
            span = xif / 2;

            if (nSelect == 0)
            {
                /**/
                /* *** calculate default receiver selectivity*/
                /**/
                freq1[1] = 0.0;
                alpha1[1] = 0.0;
                freq1[2] = .4 * abw;
                alpha1[2] = 0.0;
                freq1[3] = freq1[2] * Pow(10.0, Log10(2.0) * 20.0 / 60.0);
                alpha1[3] = 20.0;
                freq1[4] = .625 * abw;
                alpha1[4] = 20.0;
                freq1[23] = freq1[4] * Pow(10.0, Log10(2.0) * 90.0 / 30.0);

                alpha1[23] = Constant.DEFAULT_SELECTIVITY;
                freq1[24] = dMaxFS; /*	Out to the maximum */
                alpha1[24] = Constant.DEFAULT_SELECTIVITY;
                for (i = 5; i <= 22; i++)
                {
                    xi = i;
                    freq1[i] = (xi - 4) * (freq1[23] - freq1[4]) / 19.0 + freq1[4];
                    df = freq1[i] / freq1[4];
                    alpha1[i] = 20.0 + 30.0 * (Log10(df) / Log10(2.0));
                }
                nSelect = 24;
                ans3 = 0;       /*	This is the default selectivity switch */
            }
            else
            {
                /*	A filter has been supplied.  It is already 1 based on entry. */
                for (i = 1; i <= nSelect; i++)
                {
                    freq1[i] = freq1in[i];
                    alpha1[i] = alpha1in[i];
                }
                if (freq1[nSelect] < dMaxFS)
                {
                    //	Extend it out to the max frequency.
                    nSelect++;
                    freq1[nSelect] = dMaxFS;
                    alpha1[nSelect] = alpha1[nSelect - 1];
                }
                ans3 = 1;       /*	No default selectivity.  It has been entered. */
            }

            np = nSelect;
            //...Log2.v("\nnSelect = " + nSelect);
            /**/
            /*     normalize specific selectivity to zero*/
            /**/
            minatt = 150;

            for (i = 1; i <= np; i++)
            {
                if (alpha1[i] < minatt) minatt = alpha1[i];
            }

            for (i = 1; i <= np; i++)
            {
                alpha1[i] = alpha1[i] - minatt;
            }
            /**/
            /*     create double sided selectivity curve*/
            /*      (frequency in hertz and attenuation as a ratio)*/
            /**/

            freq[np] = freq1[1] * 1e6;
            alpha[np] = Pow(10.0, -alpha1[1] / 10.0);
            for (i = 2; i <= np; i++)
            {
                freq[np + i - 1] = freq1[i] * 1e6;
                alpha[np + i - 1] = Pow(10.0, -alpha1[i] / 10.0);
                freq[np - i + 1] = -freq1[i] * 1e6;
                alpha[np - i + 1] = Pow(10.0, -alpha1[i] / 10.0);
            }

            //...Log2.v("\nB_freq[1] = " + freq[1]);

            /**/
            /*      digital into digital or analogue into digital*/
            /*       option selection*/
            /**/
            if (dora == 0)
            {
                /**/
                /*     calculation analogue parameters*/
                /**/
                xn2 = n2;
                if (n2 < 240)
                {
                    nlr2 = 4.0 * Log10(xn2) - 1.0;
                }
                else
                {
                    nlr2 = 10.0 * Log10(xn2) - 15.0;
                }
                sigma2 = dsig2 * Pow(10.0, .05 * nlr2);
                m2 = sigma2 / fm2;
                /**/
                /*     calculate analogue spectrum (single sided)*/
                /**/
                DoatdSpect(f5, d5, m2, ref nps2, fmin2, fm2);

                fm2hz = fm2 * 1e6;
                for (i = 1; i <= nps2; i++)
                {
                    f2[i] = f5[i] * fm2hz;
                    d2[i] = Pow(10.0, 0.1 * d5[i]) / fm2hz;
                }
            }
            else
            {

                /**/
                /*     default or specific digital interferer spectrum option  */
                /**/
                if (nDefaultSpect == 0)
                {
                    int ilast;
                    double inc;
                    double mult;
                    /**/
                    /*     calculates default digital spectrum*/
                    /*      reads authorized bandwidth of interferer (abw1)*/
                    /**/
                    fl = (55 - 10 * Log10(abw1)) * abw1 / 80;
                    fh = (85 - 10 * Log10(abw1)) * abw1 / 80;
                    y2[1] = 0;
                    z2[1] = 10 * Log10(abw1 / 0.004);
                    y2[2] = abw1 / 2;
                    z2[2] = z2[1];
                    y2[3] = abw1 / 2.0 + .000001;
                    z2[3] = 35 + 10 * Log10(abw1);
                    if (z2[3] <= 50)
                    {
                        z2[3] = 50;
                        y2[4] = fl;
                        z2[4] = 50;
                    }
                    else
                    {
                        y2[4] = y2[3] + .000001;
                        z2[4] = z2[3];
                    }

                    /*	The following was changed to handle the request that
					*		the Pd be made 150 from 10.0 * BW + .1 out to 400
					*		Added as part of 1083 - GJS - 2003.12
					*		Further modified 2004.03.30 - GJS - 1170 so that the old
					*		algorithm will be used if the victim is ES */
                    if (10.0 * abw1 < dMaxFS && !IsEs)
                    {
                        /*	From fh out to 10.0 * abw1 value is 80 */
                        y2[33] = fh;
                        z2[33] = 80.0;
                        y2[34] = 10.0 * abw1;
                        z2[34] = 80.0;
                        /*	From 10.0 * abw1 out to max, value is 150 */
                        y2[35] = 10.0 * abw1 + 0.1;
                        z2[35] = 150.0;
                        y2[36] = dMaxFS;
                        z2[36] = 150.0;
                        ilast = 33;
                    }
                    else
                    {
                        /*	Just use 80 all the way out to max freq */
                        y2[35] = fh;
                        z2[35] = 80;
                        y2[36] = dMaxFS;
                        z2[36] = 80;
                        ilast = 35;
                    }
                    /*	Now fill in the values in between using linear interp. */
                    inc = 1.0 / (double)(ilast - 4);
                    for (i = 5, mult = inc; i < ilast; i++, mult += inc)
                    {
                        y2[i] = (mult * (fh - y2[4])) + y2[4];
                        z2[i] = (mult * (80 - z2[4])) + z2[4];
                    }
                    nDefaultSpect = 36;
                }
                else
                {
                    /*	A digital spectrum was supplied.  It is already 1 based on entry. */
                    for (i = 1; i < Constant.MAX_SPECTRUM; i++)
                    {
                        if (i <= nDefaultSpect)
                        {
                            y2[i] = y2in[i];
                            z2[i] = z2in[i];
                        }
                        else
                        {
                            y2[i] =
                            z2[i] = 0.0;
                        }
                    }
                    if (y2[nDefaultSpect] < dMaxFS)
                    {
                        //	Add the last point at the max separation.
                        nDefaultSpect++;
                        y2[nDefaultSpect] = dMaxFS;
                        z2[nDefaultSpect] = z2[nDefaultSpect - 1];
                    }
                }
            }

            /**/
            /*     specific thermal noise floor option (vic)*/
            /**/
            /*     Console.Write("\nDo you want a specific thermal noise value? (no=1/yes=0): ");
                  scanf("%d", &ans);  */
            ans = (innth == 0.0);

            if (ans3 < 1)
            {

                /**/
                /*     calculate 3 db bandwidth and brickwall filter bandwidth*/
                /*     compute thermal noise of victim*/
                /**/
                bw = 1.0 * abw;
                bwhz = bw * 1e6;
                nth = -174.0 + 10.0 * Log10(bwhz) + nf;
            }

            pwr = 0.0;
            for (i = 1; i <= 2 * np - 2; i++)
            {
                pwr = pwr + 0.5 * (alpha[i] + alpha[i + 1]) * (freq[i + 1] - freq[i]);
            }
            bw1hz = pwr;
            bw1 = bw1hz / 1e6;
            for (i = np + 1; i <= 2 * np - 1; i++)
            {
                if (alpha[i] == Pow(10.0, -0.3)) goto L594;
                if (alpha[i] < Pow(10.0, -0.3)) goto L596;
            }
            /* Console.Write("\n******  check your filter response data ******\n");  */
            return (-1);

            L594:
            bw2hz = freq[i] * 2;
            goto L598;

            L596:
            bw2hz = 2 * (freq[i - 1] + ((alpha[i - 1] - .5) / (alpha[i - 1] - alpha[i])) *
                   (freq[i] - freq[i - 1]));
            L598:
            bw2 = bw2hz / 1e6;
            if (bw2hz < bw1hz) bw1hz = bw2hz;
            if (ans3 == 0) goto L599;
            nth = -174.0 + 10.0 * Log10(bw1hz) + nf;
            L599:
            if (!ans)    // (ans != 1)
            {
                nth = innth;
            }
            if (dora >= 1.0)
            {
                /**/
                /*     create double sided digital interferer spectrum	*/
                /*     (freq in hertz and power in watts)	*/
                /**/
                nps2 = nDefaultSpect;
                for (i = 1; i <= nps2; i++)
                {
                    f2[i] = y2[i] * 1e6;
                    z2[i] = 10.0 * Log10(1.0 / 4000.0) - Abs(z2[i]);
                    d2[i] = Pow(10.0, z2[i] / 10.0);
                }

                f2a[nps2] = f2[1];
                d2a[nps2] = d2[1];
                for (i = 2; i <= nps2; i++)
                {
                    f2a[nps2 + i - 1] = f2[i];
                    d2a[nps2 + i - 1] = d2[i];
                    f2a[nps2 - i + 1] = -f2[i];
                    d2a[nps2 - i + 1] = d2[i];
                }
                /**/
                /* *** correction of digital to digital power spectra density*/
                /**/
                pwr = 0.0;
                for (i = 1; i <= 2 * nps2 - 2; i++)
                {
                    pwr = pwr + 0.5 * (d2a[i] + d2a[i + 1]) * (f2a[i + 1] - f2a[i]);
                }

                if (pwr < 1)
                {
                    corr = Abs(10.0 * Log10(pwr));
                }
                else if (pwr > 1)
                {
                    corr = -Abs(10.0 * Log10(pwr));
                }
                if (pwr != 1.0)
                {
                    for (i = 1; i <= nps2; i++)
                    {
                        d2[i] = Pow(10.0, ((10.0 * Log10(d2[i]) + corr) / 10.0));
                    }
                }
            }
            //L650: /* continue */ ;
            /**/
            /* *** this is the beginning of the do loop for computing*/
            /* *** a ctx curve*/
            /**/
            /**/
            /*     initialize parameters*/
            /**/
            for (i = 1; i < 2 * Constant.MAX_FILTER; i++)
            {
                freq2[i] = 0.0;
                alpha2[i] = 0.0;
            }
            for (i = 1; i < 2 * Constant.MAX_SPECTRUM; i++)
            {
                r2[i] = 0.0;
                s2[i] = 0.0;
                r[i] = 0.0;
                s[i] = 0.0;
            }
            for (i = 1; i < 3 * Constant.MAX_SPECTRUM; i++)
            {
                ra[i] = 0.0;
                sa[i] = 0.0;
                r2a[i] = 0.0;
                s2a[i] = 0.0;
            }
            esele = 0.0;
            sele = 0.0;
            a70 = 0.0;
            a140 = 0.0;
            fs = Abs(fs);
            fs1 = fs;
            fshz = fs * 1e6;
            fs1hz = fs * 1e6;
            /**/
            /*     	IF separation is in critical if or image response range*/
            /*      calculates attenuation of responses and shifts separation*/
            /*			IF calculations are ignored if xif is negative.*/
            /**/
            if (xif >= 0 && fs >= xif - span && fs <= xif + span)
            {
                atti = Constant.DEFAULT_SELECTIVITY;
                for (i = 1; i <= np; i++)
                {
                    if (freq1[i] > xif)
                    {
                        atti = alpha1[i - 1] + (alpha1[i] - alpha1[i - 1]) *
                                 ((xif - freq1[i - 1]) / (freq1[i] - freq1[i - 1]));
                        break;
                    }
                }
                /*	Jump out of last loop to here */
                if (sif != 0.0)
                {
                    a70 = sif;
                    //goto L690;
                }
                else
                {
                    if (ans3 == 1)
                    {
                        //ans1 = 1;
                        a70 = ai70 + 20.0;
                    }
                    else
                    {
                        a70 = atti / 2 + 20;
                    }
                }
                //L690:   
                fshz = Abs(xif - fs) * 1e6;
            }
            else
            {
                att2i = Constant.DEFAULT_SELECTIVITY;
                if (!(xif < 0 || fs < (2.0 * xif - span) || fs > (2.0 * xif + span)))
                {
                    for (i = 1; i <= np; i++)
                    {
                        if (freq1[i] > (2.0 * xif))
                        {
                            att2i = alpha1[i - 1] + (alpha1[i] - alpha1[i - 1]) *
                                            ((2.0 * xif - freq1[i - 1]) / (freq1[i] - freq1[i - 1]));
                            break;
                        }
                    }

                    if (irf != 0.0)
                    {
                        a140 = irf;
                    }
                    else
                    {
                        if (ans3 == 1)
                        {
                            //ans2 = 1;
                            a140 = ai140;
                        }
                        else
                        {
                            a140 = att2i / 2;
                        }
                    }
                    fshz = Abs((2.0 * xif) - fs) * 1e6;
                }
            }
            /**/
            /*     calculate interference spectrum at demodulator*/
            /*     based on selectivity and*/
            /*      interferer spectrum obtained and frequency separation.*/
            /*       frequency separation in the critical response range*/
            /*       is shifted by xif or 2xif.*/
            /**/
            r2[nps2] = fshz;
            s2[nps2] = d2[1];
            for (i = 2; i <= nps2; i++)
            {
                r2[nps2 + i - 1] = fshz + f2[i];
                s2[nps2 + i - 1] = d2[i];
                r2[nps2 - i + 1] = fshz - f2[i];
                s2[nps2 - i + 1] = d2[i];
            }

            //...Log2.v("\nr2[1] = " + r2[1]);
            //...Log2.v("\nfreq[1] = " + freq[1]);
            if (r2[1] >= freq[1]) p = r2[1];
            if (r2[1] < freq[1]) p = freq[1];

            if (r2[2 * nps2 - 1] <= freq[2 * np - 1])
            { //goto L800;
                q = r2[2 * nps2 - 1];
                //goto L810;
            }
            else
            {
                //L800:  	
                q = freq[2 * np - 1];
            }
            //L810:  	
            k1 = 1;
            j1 = 1;
            j2 = 1;

            //...Log2.v("\np = " + p);
            ra[1] = p;
            sa[1] = DoatdDvaln(freq, alpha, p, freq.Length) *
                              DoatdDvaln(r2, s2, p, r2.Length);
            x = p;
            L820:
            if (freq[j1] < x) goto L855;
            if (r2[j2] < x) goto L865;
            if (freq[j1] < q || r2[j2] < q) goto L830;
            goto L870;

            L830:
            k1 = k1 + 1;
            if (r2[j2] > freq[j1]) goto L850;  /* 840,845,850; */
            if (r2[j2] == freq[j1]) goto L845;
            ra[k1] = r2[j2];
            sa[k1] = s2[j2] * DoatdDvaln(freq, alpha, r2[j2], freq.Length);
            x = r2[j2];
            goto L865;

            L845:
            ra[k1] = r2[j2];
            sa[k1] = s2[j2] * alpha[j1];
            x = r2[j2];
            goto L860;

            L850:
            ra[k1] = freq[j1];
            sa[k1] = alpha[j1] * DoatdDvaln(r2, s2, freq[j1], r2.Length);
            x = freq[j1];

            L855:
            j1 = j1 + 1;
            goto L820;

            L860:
            j1 = j1 + 1;

            L865:
            j2 = j2 + 1;
            goto L820;

            L870:
            ra[k1 + 1] = q;
            sa[k1 + 1] = DoatdDvaln(r2, s2, q, r2.Length) *
                             DoatdDvaln(freq, alpha, q, freq.Length);
            k2 = k1;

            for (i = 1; i <= k2 + 1; i++)
            {
                r2a[i] = ra[i];
                s2a[i] = sa[i];
            }
            k1 = k2;
            /**/
            /*      calculates effective selectivity, interference objective*/
            /*      and ctx interference level based on derived interference*/
            /*      spectrum at the demodulator, threshold degradation*/
            /*      criteria and thermal noise floor.*/
            /**/

            for (i = 1; i <= k1; i++)
            {
                double dx1 = s2a[i] + s2a[i + 1];
                double dx2 = r2a[i + 1] - r2a[i];
                double term = 0.5 * dx1 * dx2;

                sele = sele + term;
                //sele = sele + 0.5*(s2a[i] + s2a[i + 1])*(r2a[i + 1] - r2a[i]);

                //...Log2.v(String.Format("\n{0}, {1:F6}, {2:F6}, {3:F6}, sele = {4:F6}", i, dx1, dx2, term, sele));
            }
            esele = -10.0 * Log10(sele);
            ulh = 10 * Log10(Pow(10.0, (thdcrit / 10)) - 1);
            iobj = nth + ulh;

            //...Log2.v(String.Format("\nesele = {0:F6}", esele));
            //...Log2.v(String.Format("\niobj = {0:F6}", iobj));
            //...Log2.v(String.Format("\na70 = {0:F6}", a70));
            //...Log2.v(String.Format("\na140 = {0:F6}", a140));

            ilvl = esele + iobj + a70 + a140;
            /**/
            /*     part of routine to determine if image or if responses*/
            /*      are controlling in the if and image response ranges.*/
            /**/
            if (fs1 < xif - span || fs1 > xif + span) goto L915;
            goto L710;

            L915:
            if (fs1 < (2.0 * xif) - span || fs1 > (2.0 * xif + span)) goto L910;
            /**/
            /*     calculates ctx interference level in the if and image*/
            /*     response ranges without spurious responses and compares*/
            /*     to the spurious responses and takes the worst case.*/
            /*     (frequency separation parameter is not shifted.)*/
            /**/
            L710:;
            for (i = 1; i < Constant.MAX_FILTER; i++)
            {
                freq3[i] = 0.0;
                alpha3[i] = 0.0;
            }
            for (i = 1; i < Constant.MAX_SPECTRUM; i++)
            {
                r[i] = 0.0;
                s[i] = 0.0;
            }
            for (i = 1; i < 2 * Constant.MAX_FILTER; i++)
            {
                freq2[i] = 0.0;
                alpha2[i] = 0.0;
            }
            for (i = 1; i < 3 * Constant.MAX_SPECTRUM; i++)
            {
                r2a[i] = 0.0;
                s2a[i] = 0.0;
            }
            for (i = 1; i <= np; i++)
            {
                freq3[i] = freq1[i];
                alpha3[i] = alpha1[i];
            }
            r[nps2] = fs1hz;
            s[nps2] = d2[1];
            for (i = 2; i <= nps2; i++)
            {
                r[nps2 + i - 1] = fs1hz + f2[i];
                s[nps2 + i - 1] = d2[i];
                r[nps2 - i + 1] = fs1hz - f2[i];
                s[nps2 - i + 1] = d2[i];
            }
            freq2[np] = freq3[1] * 1e6;
            alpha2[np] = Pow(10.0, -alpha3[1] / 10.0);
            for (i = 2; i <= np; i++)
            {
                freq2[np + i - 1] = freq3[i] * 1e6;
                alpha2[np + i - 1] = Pow(10.0, -alpha3[i] / 10.0);
                freq2[np - i + 1] = -freq3[i] * 1e6;
                alpha2[np - i + 1] = Pow(10.0, -alpha3[i] / 10.0);
            }
            if (r[1] >= freq2[1]) p = r[1];
            if (r[1] < freq2[1]) p = freq2[1];
            if (r[2 * nps2 - 1] > freq2[2 * np - 1]) goto L750;
            q = r[2 * nps2 - 1];
            goto L755;

            L750:
            q = freq2[2 * np - 1];

            L755:
            k1 = 1;
            j1 = 1;
            j2 = 1;
            r2a[1] = p;
            s2a[1] = DoatdDvaln(freq2, alpha2, p, freq2.Length) *
                DoatdDvaln(r, s, p, r.Length);
            x = p;
            L760:
            if (freq2[j1] < x)
            {
                j1 = j1 + 1;
                goto L760;
            }
            if (r[j2] < x) goto L795;
            if (freq2[j1] < q || r[j2] < q) goto L765;
            goto L799;

            L765:
            k1 = k1 + 1;
            if (r[j2] < freq2[j1])
            {
                r2a[k1] = r[j2];
                s2a[k1] = s[j2] * DoatdDvaln(freq2, alpha2, r[j2], freq2.Length);
                x = r[j2];
                goto L795;
            }
            if (r[j2] == freq2[j1])
            {
                r2a[k1] = r[j2];
                s2a[k1] = s[j2] * alpha2[j1];
                x = r[j2];
                goto L791;
            }
            if (r[j2] > freq2[j1])
            {
                r2a[k1] = freq2[j1];
                s2a[k1] = alpha2[j1] * DoatdDvaln(r, s, freq2[j1], r.Length);
                x = freq2[j1];
                //L785: this label is never referenced.
                j1 = j1 + 1;
                goto L760;
            }

            L791:
            j1 = j1 + 1;
            L795:
            j2 = j2 + 1;
            goto L760;

            L799:
            r2a[k1 + 1] = q;
            s2a[k1 + 1] = DoatdDvaln(r, s, q, r.Length) *
                DoatdDvaln(freq2, alpha2, q, freq2.Length);
            sele1 = 0.0;
            esele1 = 0.0;
            for (i = 1; i <= k1; i++)
            {
                sele1 = sele1 + 0.5 * (s2a[i] + s2a[i + 1]) * (r2a[i + 1] - r2a[i]);
            }
            esele1 = -10.0 * Log10(sele1);
            ilvl2 = esele1 + iobj;

            //...Log2.v("\nesele1 = " + esele1);
            //...Log2.v("\niobj = " + iobj);
            //...Log2.v("\nilvl2 = " + ilvl2);

            if (ilvl2 <= ilvl) ilvl = ilvl2;
            //...Log2.v("\nfinal_ilvl = " + ilvl);

            /***/
            /*     output data statements (file and screen)*/
            /**/
            L910:
            return 0;
        }

        /// <summary>
        /// </summary>
        /// <param name="ff"></param>
        /// <param name="d"></param>
        /// <param name="m"></param>
        /// <param name="nps"></param>
        /// <param name="fmin"></param>
        /// <param name="fm"></param>
        public static void DoatdSpect(double[] ff,
                                        double[] d,
                                        double m,
                                        ref int nps,
                                        double fmin,
                                        double fm)
        {
            double[] f1 = new double[9] { 0, 0.0, 0.1, 0.2, 0.3, 0.4, 0.6, 0.8, 1.0 };

            /*	The following is the array transposed */
            double[,] d1 = new double[9, 9]  {{0, 0, 0, 0, 0, 0, 0, 0, 0},
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
            {
                for (i = 1; i <= 13; i++)
                {
                    ff[i] = i - 1;
                    d[i] = Exp(-ff[i] * ff[i] / (2.0 * m * m)) /
                                 (m * 2.50662827463 /* Sqrt(2.0*3.14159) */);
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
            phmod = 0.319 * m * m / (1.0 - x1) *
                          (1.25 / x1 + 3.5395 - 2.8123 * repatan +
                                   0.6277 * Log((0.64 * x1 * x1 - 1.461 * x1 + 1.0) /
                                                (0.64 * x1 * x1 + 1.461 * x1 + 1.0)));
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
            c     code to be added to the spect subroutine to compensate;
            c     for the error in power spectra.;
            c     initialize area;
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
            c     ff(i) is the normalized frequency (f/fmax);
            c     d(i) is fmax times p(f) (spectral value in db);
            c     then, area between ff(i-1) and ff(i) is given by:;
            c     area = ect.;
            c     and the total power is the sum of all the areas multiplied by 2,;
            c     since the spectra are stored as half-spectra (0 hz to infinity);
            */
            return;
        }

        /// <summary>
        /// This method is used to generate ctx curves for digital or analog to 
        /// digital interference cases. 
        /// </summary>
        /// <remarks>
        /// Options available are: default victim filter 
        /// victim filter default digital spectrum digital spectrum default if response 
        /// factor if response factor default image response factor image response 
        /// factor caution : in the calculated requirements for the first or second, if 
        /// frequencies are more lax than those of the neighbouring frequencies, then 
        /// these values should not be used for the ctx curve. instead, linear 
        /// interpolation of the neighbouring requirements should be used. the more 
        /// optimistic values are likely results of the optimistic estimations of the 
        /// if responses. converted by: kourosh agharazi date: march 10, 1989 - from 
        /// ibm mainframe to vax 11/750. revised by: kourosh agharazi date: april 28, 
        /// 1989 - changed wlc(130) to wlc(139), and added 9 points between points 0 
        /// and 1. - removed 'fsym' and changed calculations to be based on 'abw'. - 
        /// changed fmcrit to thdcrit. - changed receive digital default selectivity 
        /// (dependence on abw). - changed program flow to permit 'specific xif 
        /// response factor' and 'specific image response factor'. - changed xif and 
        /// image response factor when default curve is used, and also it calculates 
        /// the response factors. - changed the display of the output. revised by: tom 
        /// woo date: july 24, 1989 revised by: les abbott date: july 27, 1989 
        /// changes:-changed answer variables for specific if and image response 
        /// factors to ans5 & ans6 to avoid confusion with variables ans1 & ans2 
        /// remaining from earlier versions of the program. -revised code to permit 
        /// calculation of a70 & a140 for the default filter based on actual filter 
        /// attenuation. -changed flow to permit 'summary report' to show 
        /// specific/default/if/image response factors,if & 2if attenuation and thermal 
        /// noise power for all cases. -edited the summary report format. -added code 
        /// to permit adding critical fs values. -corrected code to permit calculation 
        /// of if & image response factors for default victims. -fixed limit at 300 
        /// -changed fs data point from 35 to 35. 01 to avoid compile error. -changed 
        /// coding of line 596 to improve correlation between brickwall and 3 db 
        /// bandwidths. -corrected default interferer spectrum to agree with new code 
        /// used in d to a program (1989 08 31) -added code to normalize the specific 
        /// selectivity to 0 -corrected code to detect a nul data set revised by: eric 
        /// jerumanis date: september 27, 1990 changes: -corrected normalization of 
        /// specific selectivity (moved label 381 from line 261 to line 260, changed 
        /// minatt to 150 db).  
        /// </remarks>
        /// <param name="f"></param>
        /// <param name="d"></param>
        /// <param name="fval"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static double DoatdDvaln(double[] f, double[] d, double fval, int n)
        {
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
