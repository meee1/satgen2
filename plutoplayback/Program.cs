using iio;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;

namespace plutoplayback
{
    class Program
    {
        private static Device tx;
        private static IOBuffer buf;
        //private static Process process;
        //private static Context ctx;
        //private static Device phy;
        //private static Device devlpc;

        static void Main(string[] args)
        {
           
            Start();

            uint widthsamples = 2000000;
          
            byte[] buffer = new byte[widthsamples * 2 * 2];
           
            buf = new IOBuffer(tx, widthsamples);
           
            BufferedStream bufferedStream = new BufferedStream(File.OpenRead(@"..\..\..\satgen2\satgen2\bin\Debug\net6.0\output.IQ"), 1024 * 1024 * 100);
           
            while (bufferedStream.Position < bufferedStream.Length)
            {
                bufferedStream.Read(buffer, 0, (int)(widthsamples * 2 * 2));
                
                buf.fill(buffer);
                buf.push(widthsamples);
                Console.WriteLine("{0} < {1}", bufferedStream.Position , bufferedStream.Length);

                if (bufferedStream.Position == bufferedStream.Length)
                    bufferedStream.Position = 0;
            }
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
          
        }

        ~Program()
        {
           
        }

        public static void Start()
        {
            var ctx = new Context("ip:192.168.1.10");// 2.1");// 1.10");
            if (ctx == null)
            {
                Console.WriteLine("Unable to create IIO context");
                return;
            }

            {
                /*
// RX stream config
rxcfg.bw_hz = MHZ(2);   // 2 MHz rf bandwidth
rxcfg.fs_hz = MHZ(2.5);   // 2.5 MS/s rx sample rate
rxcfg.lo_hz = GHZ(2.5); // 2.5 GHz rf frequency
rxcfg.rfport = "A_BALANCED"; // port A (select for rf freq.)


                _rx_channel_names = ["voltage0", "voltage1"]
                _tx_channel_names = ["voltage0", "voltage1"]
                self._ctrl = self._ctx.find_device("ad9361-phy")
                self._rxadc = self._ctx.find_device("cf-ad9361-lpc")
                self._txdac = self._ctx.find_device("cf-ad9361-dds-core-lpc")
                    */

                var phy = ctx.get_device("ad9361-phy");

                // joint
                var samplehz = phy.find_channel("voltage0", false).find_attribute("sampling_frequency");

                //rx
                //var gainmode = phy.find_channel("voltage0", false).find_attribute("gain_control_mode");
                //gainmode.write("slow_attack");

                //manual
                //gain.write(40);

                var rfbw = phy.find_channel("voltage0", false).find_attribute("rf_bandwidth");
                //var freq = phy.find_channel("altvoltage0", true).find_attribute("frequency");
                var gain = phy.find_channel("voltage0", true).find_attribute("hardwaregain");

                rfbw.write(10500000);
                samplehz.write((long)10500000);
                gain.write(0);


                //devlpc = ctx.get_device("cf-ad9361-lpc");
                //var _rx0_i = devlpc.find_channel("voltage0", false);
                //var _rx0_q = devlpc.find_channel("voltage1", false);

                //_rx0_i.enable();
                //_rx0_q.enable();

                //tx

                var rfbwtx = phy.find_channel("voltage0", true).find_attribute("rf_bandwidth");
                var freqtx = phy.find_channel("altvoltage1", true).find_attribute("frequency");

                freqtx.write((long)Racelogic.Gnss.FrequencyBand.GpsL5);// 1575420000);

                rfbwtx.write(10500000);


                tx = ctx.get_device("cf-ad9361-dds-core-lpc");

                var _tx0_i = tx.find_channel("voltage0", true);
                var _tx0_q = tx.find_channel("voltage1", true);

                _tx0_i.enable();
                _tx0_q.enable();

            }


            Console.WriteLine("IIO context created: " + ctx.name);
            Console.WriteLine("IIO context description: " + ctx.description);
            Console.WriteLine("IIO context has " + ctx.devices.Count + " devices:");
            foreach (Device dev in ctx.devices)
            {
                Console.WriteLine("\t" + dev.id + ": " + dev.name);
                if (dev is Trigger)
                {
                    Console.WriteLine("Found trigger! Rate=" + ((Trigger)dev).get_rate());
                }
                Console.WriteLine("\t\t" + dev.channels.Count + " channels found:");
                foreach (var chn in dev.channels)
                {
                    string type = "input";
                    if (chn.output)
                    {
                        type = "output";
                    }
                    Console.WriteLine("\t\t\t" + chn.id + ": " + chn.name + " (" + type + ")");
                    if (chn.attrs.Count == 0)
                    {
                        continue;
                    }
                    Console.WriteLine("\t\t\t" + chn.attrs.Count + " channel-specific attributes found:");
                    foreach (Attr attr in chn.attrs)
                    {
                        Console.WriteLine("\t\t\t\t" + dev.name + " " + attr.name);
                        //if (attr.name.CompareTo("frequency") == 0)
                        try
                        {
                            Console.WriteLine("\t\t\t\t" + "Attribute content: " + attr.read());
                        }
                        catch { }
                    }

                }

                if (dev.attrs.Count == 0)
                {
                    continue;
                }
                Console.WriteLine("\t\t" + dev.attrs.Count + " device-specific attributes found:");
                foreach (Attr attr in dev.attrs)
                {
                    Console.WriteLine("\t\t\t" + attr.name);
                }
            }
        }



    }
}
