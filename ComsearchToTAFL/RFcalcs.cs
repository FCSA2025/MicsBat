using _NewLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComsearchToTAFL
{
    public class RFcalcs
    {
        /// <summary>
        /// This method returns the unfaded received signal level (dBW) calculated from the prescribed set of parameters.
        /// </summary>
        /// <param name="txPower_dBm"></param>
        /// <param name="txGain"></param>
        /// <param name="txLosses"></param>
        /// <param name="frequencyMHz"></param>
        /// <param name="distanceKm"></param>
        /// <param name="rxGain"></param>
        /// <param name="rxLosses"></param>
        /// <returns></returns>
        public static double RxSignalLevelUnfaded_dBW(double txPower_dBm,
                                                        double txGain,
                                                        double txLosses,
                                                        double frequencyMHz,
                                                        double distanceKm,
                                                        double rxGain,
                                                        double rxLosses
                                                        )
        {
            // See Powerpoint file: "Intro to Microwave Communications.pptx"
            //
            // Effective Isotropically Radiated Power (EIRP) = TxPowerdBm + TxAntennaGain - TxLineLossesdB.
            // Free Space Loss (FSL)                         = 32.4 + 20 log(f in MHz) + 20 log(D in km).
            // Isotropic Receive Level (IRL)                 = EIRP - FSL.
            // Received Signal Level (unfaded)               = IRL + RxAntennaGain - RxLineLossesdB.

            double EIRPdBm = txPower_dBm + txGain - txLosses;

            double FSLdB = 32.4 + 20 * Math.Log10(frequencyMHz) + 20 * Math.Log10(distanceKm);

            double IRLdBm = EIRPdBm - FSLdB;

            double RSLdBm = IRLdBm + rxGain - rxLosses;

            double RSLdBW = RSLdBm - 30.0;

            return RSLdBW;
        }



        /// <summary>
        /// This method returns the receiver's threshold signal level in dBW that is 
        /// required to deliver a prescribed Bit Error Rate (BER).
        /// </summary>
        /// <param name="modulationScheme"></param>
        /// <param name="bandwidth_Hz"></param>
        /// <param name="bitRate_Hz"></param>
        /// <param name="enbw"></param>
        /// <param name="prescribedBER"></param>
        /// <returns></returns>
        public static double RxThresholdSignalLevelForBER_dBW(string modulationScheme, double bandwidth_Hz, double bitRate_Hz, double enbw, double prescribedBER)
        {
            // See document: "Rx Sensitivity - Equivalent Noise Bandwidth - BER.docx"
            //
            // RxSensitivity = 10*log10(kTB) + NF + C⁄N
            //
            //                 K = Boltzmann's Constant.
            //                 T = temperature in Kelvin. (We assume 290 K).
            //                 B = bandwidth of the channel selective filtering in the receiver. (Hz)
            //
            // Noise Factor:             NF = noise power added by the receiver electronics receiver relative to
            //                                the thermal noise power from the input of the receiver. (dB)
            //                                NF is equipment-dependent but a typical value is 10 dB.
            //
            // Carrier-to-Noise ratio:   C/N = the received signal power required to give a prescribed bit-error-rate (BER).
            //
            //                               = 10*Log10(Eb/N0) + 10*Log10(Fb/B)
            //              where:
            //                         Eb = the Energy per bit (Eb).
            //                         N0 = the noise spectral density (the noise power present in a 1 Hz band).
            //                         Fb = bit rate.
            //                         B  = receiver equivalent noise bandwidth (ENBW) defined as the bandwidth of
            //                              a perfect rectangular filter that passes the same amount of power as
            //                              the cumulative bandwidth of the channel selective filters in the receiver. (Hz)
            // 
            // For a specific digital modulation scheme the prescribed BER determines the required value of Eb/N0.
            // The relation between BER and Eb/N0 is different for different modulation schemes and the mathematics
            // is complicated.

            double rxThresholdSignalLevelForBER_dBW = double.MaxValue;

            const double K = 1.380649E-23;
            const double T = 290.0;
            double thermalNoise_dBW = 10 * Math.Log10(K * T * bandwidth_Hz);

            const double NF = 10.0;

            double EbOverN0 = double.MaxValue;
            double CoverN;
            double N = 0.0;
            int M = 0;

            // Identify the digital modulation scheme.
            modulationScheme = modulationScheme.Trim().ToUpper();
            modulationScheme = modulationScheme.Replace(" ", "");

            if (modulationScheme.Equals(""))
            {
                // This corresponds to the modulation field being absent from the original Comsearch 
                // record due to it being a one-way link (i.e. has no 'Centre Frequency Site 2'.
                // Just set EbOverN0 to a default value.
                EbOverN0 = double.MaxValue;
            }
            else if (modulationScheme.Equals("QPSK") || modulationScheme.Equals("4PSK"))
            {
                // For QPSK we have BER = 0.5 * erfc(sqrt(EbOverN0))    =>    EbOverN0 = {Inverfc(2 * BER)}**2.
                EbOverN0 = Math.Pow(Maths.InvErfc(2 * prescribedBER), 2);
            }
            else if (modulationScheme.Equals("BFSK") || modulationScheme.Equals("ASK") || modulationScheme.Equals("OOK"))
            {
                // For BFSK we have BER = 0.5 * erfc(sqrt(0.5 * EbOverN0))    =>    EbOverN0 = 2 * {Inverfc(2 * BER)}**2.
                EbOverN0 = 2 * Math.Pow(Maths.InvErfc(2 * prescribedBER), 2);
            }
            else if (modulationScheme.Equals("BPSK"))
            {
                // For BPSK we have BER = 0.5 * erfc(sqrt(EbOverN0))    =>    EbOverN0 = {Inverfc(2 * BER)}**2.
                // BER for BPSK is the same as for QPSK.
                EbOverN0 = Math.Pow(Maths.InvErfc(2 * prescribedBER), 2);
            }
            else if (modulationScheme.Equals("DBPSK"))
            {
                // BER for DBPSK is twice that of BPSK.
                EbOverN0 = 2 * Math.Pow(Maths.InvErfc(2 * prescribedBER), 2);
            }
            else if (modulationScheme.Equals("OFDM"))
            {
                // For OFDM we have BER = 0.5 * erfc(sqrt(beta * EbOverN0)).
                // The parameter beta depends on the specific details of the 
                // OFDM modulation being used.
                // We will just use a 'nominal' of beta = 0.75 which is the value
                // characteristic of IEEE 802.11a.
                double beta = 0.75;
                EbOverN0 = (1.0 / beta) * Math.Pow(Maths.InvErfc(2 * prescribedBER), 2);
            }
            else if (modulationScheme.EndsWith("QAM"))
            {
                // Get the value of M, the constellation size.
                string mStr = modulationScheme.Substring(0, modulationScheme.Length - 3);

                try
                {
                    M = Convert.ToInt32(mStr);
                }
                catch
                {
                    Console.Error.Write("\nRFcalcs.RxThresholdSignalLevelForBER_dBW(): ERROR: QAM: failed to convert '{0}' to an int.");
                }

                // Write N = Log2(M);
                N = Math.Log10(M) * 3.3219280948873623478703194294894;

                // For M-QAM we have BER = (4 / N) * Q(sqrt(3 * EbOverN0 * N / (M - 1))
                //        =>    EbOverN0 = ((M - 1) / (3 * N) ) * {InvQ(0.25 * N * BER)}**2.
                EbOverN0 = ((M - 1) / (3 * N)) * Math.Pow(Maths.InvQ(0.25 * N * prescribedBER), 2.0);
            }
            else if (modulationScheme.EndsWith("PSK"))
            {
                // Get the value of M, the constellation size.
                string mStr = modulationScheme.Substring(0, modulationScheme.Length - 3);

                try
                {
                    M = Convert.ToInt32(mStr);
                }
                catch
                {
                    Console.Error.Write("\nRFcalcs.RxThresholdSignalLevelForBER_dBW(): ERROR: m-ary PSK: failed to convert '{0}' to an int.");
                }

                // Write N = Log2(M);
                N = Math.Log10(M) * 3.3219280948873623478703194294894;

                // For M-PSK we have BER ~ (2 / N) * Q(Math.Sin(PI/M) * sqrt(2 * EbOverN0))
                //        =>    EbOverN0 = (1.0 / Math.Sin(PI/M) ) * {InvQ(N * BER)}**2.
                EbOverN0 = (1.0 / Math.Sin(Math.PI / M)) * Math.Pow(Maths.InvQ(N * prescribedBER), 2.0);
            }
            else
            {
                // Modulation scheme is not recognized.
                // To provide a result in the right ball-park use the QPSK calculation for EbOverN0.
                EbOverN0 = Math.Pow(Maths.InvErfc(2 * prescribedBER), 2);
                Console.Error.Write("\nRFcalcs.RxThresholdSignalLevelForBER_dBW(): ERROR: unknown modulation scheme: '{0}'", modulationScheme);
            }

            double term1 = 10 * Math.Log10(EbOverN0);
            double term2 = 10 * Math.Log10(bitRate_Hz / enbw);
            CoverN = term1 + term2;

            // Assembly everything.
            rxThresholdSignalLevelForBER_dBW = thermalNoise_dBW + NF + CoverN;
#if true
            Console.Write("\nmodulationScheme = {0}", modulationScheme);
            Console.Write("\nM = {0}, N = {1}", M, N);
            Console.Write("\nthermalNoise_dBW = {0}", thermalNoise_dBW);
            Console.Write("\nEbOverNF         = {0}", NF);
            Console.Write("\nEbOverN0         = {0}", EbOverN0);
            Console.Write("\nbitRate          = {0}", bitRate_Hz);
            Console.Write("\nenbw             = {0}", enbw);
            Console.Write("\n10*Log10(Eb/N0)  = {0}", term1);
            Console.Write("\n10*Log10(Fb/B)   = {0}", term2);
            Console.Write("\nrxThresholdSignalLevelForBER_dBW = {0}", rxThresholdSignalLevelForBER_dBW);
#endif
            return rxThresholdSignalLevelForBER_dBW;
        }









    }
}

