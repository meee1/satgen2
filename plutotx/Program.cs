using iio;
using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;

namespace plutotx
{
    class Program
    {
        private static Device tx;
        private static IOBuffer buf;

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var process = System.Diagnostics.Process.Start(new ProcessStartInfo("satgen2.exe", "profile.txt")
                { UseShellExecute = true });

            var pipeClient =
                new NamedPipeClientStream(".", "testpipe",
                    PipeDirection.InOut, PipeOptions.None,
                    TokenImpersonationLevel.Impersonation);

            Console.WriteLine("Connecting to server...\n");
            pipeClient.Connect();

            Start();

            byte[] buffer = new byte[(3000000 * 2 * 2) / 10];

            var samples = 3000000 / 10;

            while (true)
            {
                int numBytesToWrite = pipeClient.Read(buffer, 0, buffer.Length);
                
                var samp = numBytesToWrite / 2 / 2;

                Console.WriteLine(numBytesToWrite + " " + samp);

                buf.fill(buffer);
                buf.push((uint)samp);
            }
        }

        public static void Start()
        {
            Context ctx = new Context("ip:192.168.2.1");
            if (ctx == null)
            {
                Console.WriteLine("Unable to create IIO context");
                return;
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
                /* If we find cf-ad9361-lpc, try to read a few bytes from the first channel */
                if (dev.name.CompareTo("cf-ad9361-lpc") == 0)
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

                    rfbw.write(3000000);
                    samplehz.write((long)3000000);



                    var _rx0_i = dev.find_channel("voltage0", false);
                    var _rx0_q = dev.find_channel("voltage1", false);

                    //_rx0_i.enable();
                    //_rx0_q.enable();

                    //tx

                    var rfbwtx = phy.find_channel("voltage0", true).find_attribute("rf_bandwidth");
                    var freqtx = phy.find_channel("altvoltage1", true).find_attribute("frequency");

                    freqtx.write(1575420000);

                    rfbwtx.write(3000000);


                    tx = ctx.get_device("cf-ad9361-dds-core-lpc");

                    var _tx0_i = tx.find_channel("voltage0", true);
                    var _tx0_q = tx.find_channel("voltage1", true);

                    _tx0_i.enable();
                    _tx0_q.enable();

                    buf = new IOBuffer(tx, 3000000 / 10);
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

    public unsafe sealed class UnsafeBuffer : IDisposable
    {
        private readonly GCHandle _handle;
        private void* _ptr;
        private int _length;
        private Array _buffer;

        private UnsafeBuffer(Array buffer, int realLength, bool aligned)
        {
            _buffer = buffer;
            _handle = GCHandle.Alloc(_buffer, GCHandleType.Pinned);
            _ptr = (void*)_handle.AddrOfPinnedObject();
            if (aligned)
            {
                _ptr = (void*)(((long)_ptr + 15) & ~15);
            }
            _length = realLength;
        }

        ~UnsafeBuffer()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (_handle.IsAllocated)
            {
                _handle.Free();
            }
            _buffer = null;
            _ptr = null;
            _length = 0;
            GC.SuppressFinalize(this);
        }

        public void* Address
        {
            get { return _ptr; }
        }

        public int Length
        {
            get { return _length; }
        }

        public static implicit operator void*(UnsafeBuffer unsafeBuffer)
        {
            return unsafeBuffer.Address;
        }

        public static UnsafeBuffer Create(int size)
        {
            return Create(1, size, true);
        }

        public static UnsafeBuffer Create(int length, int sizeOfElement)
        {
            return Create(length, sizeOfElement, true);
        }

        public static UnsafeBuffer Create(int length, int sizeOfElement, bool aligned)
        {
            var buffer = new byte[length * sizeOfElement + (aligned ? 16 : 0)];
            return new UnsafeBuffer(buffer, length, aligned);
        }

        public static UnsafeBuffer Create(Array buffer)
        {
            return new UnsafeBuffer(buffer, buffer.Length, false);
        }
    }

}
