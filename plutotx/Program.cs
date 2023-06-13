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
using System.Threading.Tasks;

namespace plutotx
{
    class Program
    {
        private static Device tx;
        private static IOBuffer buf;
        private static Process process;

        static void Main(string[] args)
        {
            Start();

            Console.WriteLine("Hello World!");

            Console.CancelKeyPress += Console_CancelKeyPress;

            List<string> args2 = new List<string>();
            args2.Add("profile.txt");
            args2.AddRange(args);

            var mm = MemoryMappedFile.CreateFromFile(
                new FileStream("satgenmm", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite), null,
                1024 * 1024 * 100, MemoryMappedFileAccess.ReadWrite, HandleInheritability.None, true);
            //var mm = MemoryMappedFile.CreateOrOpen("satgen", 1024 * 1024 * 40);
            process = System.Diagnostics.Process.Start(new ProcessStartInfo("satgen2.exe", args2.Aggregate("", (a, b) => a + " " + b))
            { UseShellExecute = true });
            try
            {
                process.PriorityClass = ProcessPriorityClass.High;
                Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
            }
            catch
            {
            }

            //var pipeClient = new NamedPipeClientStream(".", "testpipe", PipeDirection.InOut, PipeOptions.None, TokenImpersonationLevel.Impersonation);

            var mmsrc = mm.CreateViewAccessor();

            Console.WriteLine("Connecting to server...\n");
            //pipeClient.Connect();


            Queue<byte[]> bufferlist = new Queue<byte[]>();

            Task.Run(() =>
            {
                while (true)
                {
                    while (bufferlist.Count == 0)
                        Thread.Sleep(1);

                    byte[] buffer;
                    lock (bufferlist)
                        buffer = bufferlist.Dequeue();

                    var numBytesToWrite = buffer.Length;

                    var samp = numBytesToWrite / 2 / 2;

                    Console.WriteLine(numBytesToWrite + " " + samp);
                    buf.fill(buffer);
                    buf.push((uint)samp);
                }
            });

            while (true)
            {
                //int numBytesToWrite = pipeClient.Read(buffer, 0, buffer.Length);
                var length = mmsrc.ReadInt32(0);
                if (length == 0)
                {
                    Thread.Sleep(1);
                    continue;
                }
                if (buf == null)
                {
                    buf = new IOBuffer(tx, (uint)(length / 2 / 2));
                    //buf.set_blocking_mode(false);
                }

                byte[] buffer = new byte[length];

                int numBytesToWrite = mmsrc.ReadArray(4, buffer, 0, length);
                mmsrc.Write(0, 0);
                if (numBytesToWrite == 0)
                {
                    continue;
                }

                lock (bufferlist)
                {
                    bufferlist.Enqueue(buffer.AsSpan().Slice((numBytesToWrite / 4) * 0, numBytesToWrite / 4).ToArray());
                    bufferlist.Enqueue(buffer.AsSpan().Slice((numBytesToWrite / 4) * 1, numBytesToWrite / 4).ToArray());
                    bufferlist.Enqueue(buffer.AsSpan().Slice((numBytesToWrite / 4) * 2, numBytesToWrite / 4).ToArray());
                    bufferlist.Enqueue(buffer.AsSpan().Slice((numBytesToWrite / 4) * 3, numBytesToWrite / 4).ToArray());
                }
            }



        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            process.Kill();
        }

        ~Program()
        {
            process.Kill();
        }

        public static void Start()
        {
            Context ctx = new Context("ip:192.168.1.10");
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
                var gainmode = phy.find_channel("voltage0", false).find_attribute("gain_control_mode");
                gainmode.write("slow_attack");

                //manual
                //gain.write(40);

                var rfbw = phy.find_channel("voltage0", false).find_attribute("rf_bandwidth");
                var freq = phy.find_channel("altvoltage0", true).find_attribute("frequency");
                var gain = phy.find_channel("voltage0", true).find_attribute("hardwaregain");

                rfbw.write(10500000);
                samplehz.write((long)10500000);
                gain.write(-10);


                var dev = ctx.get_device("cf-ad9361-lpc");
                var _rx0_i = dev.find_channel("voltage0", false);
                var _rx0_q = dev.find_channel("voltage1", false);

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


                //IOBuffer buf = new IOBuffer(dev, 2000000 / 20);

                float scale = 1.0f / 32768.0f;

                float[] lut = new float[0x10000];
                for (UInt16 i = 0x0000; i < 0xFFFF; i++)
                {
                    lut[i] = ((((i & 0xFFFF) + 32768) % 65536) - 32768) * scale;
                }

                //var sampleCount = buf.samples_count;


                //Console.WriteLine("Read " + chn.read(buf).Length + " bytes from hardware");
                //buf.Dispose();
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