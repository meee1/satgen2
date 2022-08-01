using System;
using System.Buffers;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Aladdin.HASP;
using HarmonyLib;
using iio;
using Mono.Cecil;
using Racelogic.DataTypes;
using Racelogic.Geodetics;
using Racelogic.Gnss;
using Racelogic.Gnss.SatGen;
using Racelogic.Libraries.Nmea;
using Racelogic.Maths;
using Racelogic.Utilities;
using Channel = Racelogic.Gnss.SatGen.Channel;


namespace satgen2
{
    /*
       [Flags]
       public enum SignalType : ulong
       {
           None = 0UL,
           GpsL1CA = 1UL,
           GpsL1C = 2UL,
           GpsL1P = 4UL,
           GpsL1M = 8UL,
           GpsL2C = 16UL,
           GpsL2P = 32UL,
           GpsL2M = 64UL,
           GpsL5I = 128UL,
           GpsL5Q = 256UL,
           GpsL5 = GpsL5Q | GpsL5I,
           GlonassL1OF = 512UL,
           GlonassL2OF = 1024UL,
           BeiDouB1I = 2048UL,
           BeiDouB2I = 8192UL,
           BeiDouB3I = 16384UL,
           GalileoE1BC = 32768UL,
           GalileoE5aI = 65536UL,
           GalileoE5aQ = 131072UL,
           GalileoE5a = GalileoE5aQ | GalileoE5aI,
           GalileoE5bI = 262144UL,
           GalileoE5bQ = 524288UL,
           GalileoE5b = GalileoE5bQ | GalileoE5bI,
           GalileoE5AltBocI = 1048576UL,
           GalileoE5AltBocQ = 2097152UL,
           GalileoE5AltBoc = GalileoE5AltBocQ | GalileoE5AltBocI,
           GalileoE6BC = 4194304UL,
           NavicL5SPS = 8388608UL,
           NavicSSPS = 16777216UL,
       }
    */
    public class Program
    {
        // Racelogic.Gnss.ExtensionMethods

        Racelogic.Gnss.SatGen.BlackBox.ConnectionStatus item;

        public static string ToCodeName(SignalType signalType)
        {
            switch (signalType)
            {
                case SignalType.None:
                case SignalType.GpsL1CA:
                case SignalType.GpsL1C:
                case SignalType.GpsL1CA | SignalType.GpsL1C:
                case SignalType.GpsL1P:
                case SignalType.GpsL1CA | SignalType.GpsL1P:
                case SignalType.GpsL1C | SignalType.GpsL1P:
                case SignalType.GpsL1CA | SignalType.GpsL1C | SignalType.GpsL1P:
                case SignalType.GpsL1M:
                case SignalType.GpsL1CA | SignalType.GpsL1M:
                case SignalType.GpsL1C | SignalType.GpsL1M:
                case SignalType.GpsL1CA | SignalType.GpsL1C | SignalType.GpsL1M:
                case SignalType.GpsL1P | SignalType.GpsL1M:
                case SignalType.GpsL1CA | SignalType.GpsL1P | SignalType.GpsL1M:
                case SignalType.GpsL1C | SignalType.GpsL1P | SignalType.GpsL1M:
                case SignalType.GpsL1CA | SignalType.GpsL1C | SignalType.GpsL1P | SignalType.GpsL1M:
                case SignalType.GpsL2C:
                {
                    SignalType num = signalType - 1;
                    if (num <= (SignalType.GpsL1CA | SignalType.GpsL1C))
                    {
                        switch (num)
                        {
                            case SignalType.None:
                                return "GPS_L1CA";
                            case SignalType.GpsL1CA:
                                return "GPS_L1C";
                            case SignalType.GpsL1CA | SignalType.GpsL1C:
                                return "GPS_L1P";
                            case SignalType.GpsL1C:
                                goto end_IL_0007;
                        }
                    }

                    switch (signalType)
                    {
                        case SignalType.GpsL1M:
                            return "GPS_L1M";
                        case SignalType.GpsL2C:
                            return "GPS_L2C";
                    }

                    break;
                }
                case SignalType.GpsL2P:
                    return "GPS_L2P";
                case SignalType.GpsL2M:
                    return "GPS_L2M";
                case SignalType.GpsL5I:
                    return "GPS_L5I";
                case SignalType.GpsL5Q:
                    return "GPS_L5Q";
                case SignalType.GpsL5:
                    return "GPS_L5";
                case SignalType.GlonassL1OF:
                    return "GLO_L1OF";
                case SignalType.GlonassL2OF:
                    return "GLO_L2OF";
                case SignalType.BeiDouB1I:
                    return "BDS_B1I";
                case SignalType.BeiDouB2I:
                    return "BDS_B2I";
                case SignalType.BeiDouB3I:
                    return "BDS_B3I";
                case SignalType.GalileoE1BC:
                    return "GAL_E1BC";
                case SignalType.GalileoE5aI:
                    return "GAL_E5AI";
                case SignalType.GalileoE5aQ:
                    return "GAL_E5AQ";
                case SignalType.GalileoE5a:
                    return "GAL_E5A";
                case SignalType.GalileoE5bI:
                    return "GAL_E5BI";
                case SignalType.GalileoE5bQ:
                    return "GAL_E5BQ";
                case SignalType.GalileoE5b:
                    return "GAL_E5B";
                case SignalType.GalileoE5AltBoc:
                    return "GAL_E5ALTBOC";
                case SignalType.GalileoE6BC:
                    return "GAL_E6BC";
                case SignalType.NavicL5SPS:
                    return "NAV_L5SPS";
                case SignalType.NavicSSPS:
                {
                    return "NAV_SSPS";
                }
                    end_IL_0007:
                    break;
            }

            return "???";
        }


        [STAThread]
        static void Main(string[] args)
        {
            

            runoutside(args);
            return;
            /*
            {
                //private void CheckFeatures(in bool allowRetest = true)

                var original = typeof(Racelogic.Gnss.SatGen.Simulation).GetMethod("CheckFeatures",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                Console.WriteLine(original);

                var postfix =
                    typeof(Program).GetMethod("CheckFeature_pre2", BindingFlags.Static | BindingFlags.NonPublic);
                Console.WriteLine(postfix);

                harmony.Patch(original, new HarmonyMethod(postfix));
            }
            {
                //private void CheckFeaturesPeriodically(in double secondsFromStart)
                
                var original = typeof(Racelogic.Gnss.SatGen.Simulation).GetMethod("CheckFeaturesPeriodically",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                Console.WriteLine(original);

                var postfix =
                    typeof(Program).GetMethod("CheckFeature_pre2", BindingFlags.Static | BindingFlags.NonPublic);
                Console.WriteLine(postfix);

                harmony.Patch(original, new HarmonyMethod(postfix));
            }
            {
                //private void <CheckFeaturesPeriodically>b__74_0()

                var original = typeof(Racelogic.Gnss.SatGen.Simulation).GetMethod("<CheckFeaturesPeriodically>b__74_0",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                Console.WriteLine(original);

                var postfix =
                    typeof(Program).GetMethod("CheckFeature_pre2", BindingFlags.Static | BindingFlags.NonPublic);
                Console.WriteLine(postfix);

                harmony.Patch(original, new HarmonyMethod(postfix));
            }
            */

            //var app = new App();

            //app.MainWindow = new real.Racelogic.Gnss.SatGen.BlackBox.MainWindow();
            //app.Startup += App_Startup;
            //app.Run();
        }

        private static void checkiio()
        {
            Context ctx = new Context("ip:192.168.2.1");

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
                foreach (iio.Channel chn in dev.channels)
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
                        Console.WriteLine("\t\t\t\t" + attr.name);
                        if (attr.name.CompareTo("frequency") == 0)
                        {
                            Console.WriteLine("Attribute content: " + attr.read());
                        }
                    }

                }
                /* If we find cf-ad9361-lpc, try to read a few bytes from the first channel */
                if (dev.name.CompareTo("cf-ad9361-lpc") == 0)
                {
                    iio.Channel chn = dev.channels[0];
                    chn.enable();
                    IOBuffer buf = new IOBuffer(dev, 0x8000);
                    buf.refill();

                    Console.WriteLine("Read " + chn.read(buf).Length + " bytes from hardware");
                    buf.Dispose();
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

            var tx = ctx.find_device("cf-ad9361-dds-core-lpc");

            tx.set_kernel_buffers_count(8);

            var phydev = ctx.find_device("ad9361-phy");
            var phy_chn = phydev.find_channel("voltage0", true);
            phy_chn.find_attribute("rf_port_select").write("A");
            phy_chn.find_attribute("rf_bandwidth").write(MHZ(3.0));
            phy_chn.find_attribute("sampling_frequency").write(MHZ(2.6));
            phy_chn.find_attribute("hardwaregain").write(-20);

            phydev.find_channel("altvoltage0", true).find_attribute("powerdown").write(true);

            phydev.find_channel("altvoltage1", true).find_attribute("frequency").write(GHZ(1.575420));

            var tx0_i = tx.find_channel("voltage0", true);
            var tx0_q = tx.find_channel("voltage1", true);

            tx0_i.enable();
            tx0_q.enable();

            

            // samples * iq * short
            tx_buffer = new IOBuffer(tx, (uint) MHZ(2.6));

            phydev.find_channel("altvoltage1", true).find_attribute("powerdown").write(false);

            var inp = File.OpenRead(@"C:\Users\mich1\Desktop\Hex\gps-sdr-sim\gpssim.bin");
            
            inp = File.OpenRead(@"C:\Users\mich1\Desktop\Hex\satgen2\satgen2\satgen2\bin\Debug\netcoreapp3.1\output.LS3W");

            var buf2 = new byte[(uint)MHZ(2.6) * 2 * 2];
            //tx_buffer.fill();
            while (true)
            {
                inp.Read(buf2);
                tx_buffer.fill(buf2);
                tx_buffer.push();
                Console.WriteLine(inp.Position / (float)inp.Length);
            }

        }

        private static long GHZ(double x)
        {
            return (long)(x * 1000000000.0 + .5);
        }

        private static long MHZ(double x)
        {
            return (long)(x * 1000000.0 + .5);
        }

        private static void DoPatch()
        {
            Harmony.DEBUG = true;
            var harmony = new Harmony("com.company.project.product");

            //Task.Run(() =>

            {
                //while(true)
                {
                    var original = typeof(Racelogic.Gnss.SatGen.Simulation).GetMethod("CheckFeature",
                        BindingFlags.NonPublic | BindingFlags.Static);
                    Console.WriteLine(original);
                    var prefix =
                        typeof(Program).GetMethod("CheckFeature_pre", BindingFlags.Static | BindingFlags.NonPublic);
                    Console.WriteLine(prefix);
                    var postfix =
                        typeof(Program).GetMethod("CheckFeature_post", BindingFlags.Static | BindingFlags.NonPublic);
                    Console.WriteLine(postfix);

                    //RuntimeHelpers.PrepareMethod(original.MethodHandle);



                    harmony.Patch(original, new HarmonyMethod(prefix));

                    //install(original, typeof(Program).GetMethod("CheckFeature_orig", BindingFlags.Static | BindingFlags.NonPublic));

                    // Thread.Sleep(5000);
                }
            } //);
            //if (false)
            {
                //Environment.NewLine

                var original = typeof(System.Environment).GetMethod("get_NewLine",
                    BindingFlags.Public | BindingFlags.Static);
                Console.WriteLine(original);

                var postfix = 
                    typeof(Program).GetMethod("newline", BindingFlags.Static | BindingFlags.NonPublic);
                Console.WriteLine(postfix);

                harmony.Patch(original, new HarmonyMethod(postfix));
            }
            //if (false)
            {
                //Racelogic.Utilities.WinFileIO
                //public void OpenForWriting(string fileName)

                var original = typeof(Racelogic.Utilities.WinFileIO).GetMethod("OpenForWriting",
                    BindingFlags.Public | BindingFlags.Instance);
                Console.WriteLine(original);

                var prefix =
                    typeof(Program).GetMethod("OpenForWriting", BindingFlags.Static | BindingFlags.NonPublic);
                Console.WriteLine(prefix);

                //RuntimeHelpers.PrepareMethod(prefix.MethodHandle);

                harmony.Patch(original, new HarmonyMethod(prefix));

                //install(original, prefix);
            }
            if (false)
            {
                //public bool Close()

                var original = typeof(Racelogic.Utilities.WinFileIO).GetMethod("Close",
                    BindingFlags.Public | BindingFlags.Instance);
                Console.WriteLine(original);

                var prefix =
                    typeof(Program).GetMethod("Close", BindingFlags.Static | BindingFlags.NonPublic);
                Console.WriteLine(prefix);

                RuntimeHelpers.PrepareMethod(original.MethodHandle);

                harmony.Patch(original, new HarmonyMethod(prefix));
            }

            //if(false)
            {
                //public int WriteBlocks(IntPtr bufferPointer, int numBytesToWrite)

                var original = typeof(Racelogic.Utilities.WinFileIO).GetMethods().Where(a => a.Name == "WriteBlocks")
                    .Last();
                Console.WriteLine(original);

                var prefix =
                    typeof(Program).GetMethod("WriteBlocks", BindingFlags.Static | BindingFlags.NonPublic);
                Console.WriteLine(prefix);

                RuntimeHelpers.PrepareMethod(original.MethodHandle);
                RuntimeHelpers.PrepareMethod(prefix.MethodHandle);
                

                harmony.Patch(original, new HarmonyMethod(prefix));
            }

            Console.WriteLine();

            harmony.PatchAll();

            var methods = harmony.GetPatchedMethods();
            foreach (var method in methods)
            {
                //...
                Console.WriteLine("Patched {0}", method.ToString());
                MethodBody mb = method.GetMethodBody();
                Console.WriteLine("\r\nMethod: {0}", method);

                var il = mb.GetILAsByteArray();

                var ans = VRCheat.IL.ILParser.Parse(method);

                try
                {
                    foreach (var item in ans)
                    {
                        Console.WriteLine(item);
                    }
                }
                catch
                {

                }

                //var rd = new ReflectionDisassembler(new PlainTextOutput(), CancellationToken.None);

                //rd.DisassembleMethod(new PEFile(,), method.MethodHandle);

            }
        }



        public static void runoutside(string[] args)
        {
            Console.WriteLine("blah.exe profile.txt");
            string[] commandLineArgs = Environment.GetCommandLineArgs();
            ConfigFile config = ConfigFile.Read(commandLineArgs[1]);

            Console.WriteLine(config.ToJSON());

            //Environment.NewLine = "\r\n";

            using (NmeaFile nmeaFile = new NmeaFile(config.NmeaFile))
            {
                //Console.WriteLine(nmeaFile.ToJSON());
            }


            string text = config.OutputFile.ToLower();
            string a = Path.GetExtension(text).ToLowerInvariant();
            Quantization bitsPerSample = (Quantization) config.BitsPerSample;
            //var output = new LabSat3wOutput(config.OutputFile, config.SignalTypes, bitsPerSample);

            var output = new BladeRFFileOutput(config.OutputFile, config.SignalTypes, (int)MHZ(3));

            Console.WriteLine(output.ChannelPlan.ToJSON());

            GnssTime startTime = GnssTime.FromUtc(config.Date);

            Console.WriteLine(startTime.ToJSON());

            Trajectory trajectory = new NmeaFileTrajectory(in startTime, config.NmeaFile, config.GravitationalModel);

            //new LiveNmeaTrajectory(DateTime.Now, "df", 115200);
            trajectory = new FakeLiveNmeaTrajectory(GnssTime.Now, 1);

            var lat = 0.0;
            var lng = 0.0;
            var alt = 10000.0;

            if (args.Count() > 3)
            {
                lat = double.Parse(args[1]);
                lng = double.Parse(args[2]);
                alt = double.Parse(args[3]);
            }

            ((FakeLiveNmeaTrajectory)trajectory).ecef = Geodetic.FromDegrees(lat ,  lng, alt).ToEcef(Datum.WGS84, Geoid.Egm96);

            Range<GnssTime, GnssTimeSpan> interval = trajectory.Interval;
            Console.WriteLine(interval.ToJSON());

           

            IReadOnlyList<ConstellationBase> readOnlyList = ConstellationBase.Create(config.SignalTypes);
            foreach (ConstellationBase item in readOnlyList)
            {
                string almanacPath = GetAlmanacPath(item.ConstellationType, config);
                if (!item.LoadAlmanac(almanacPath, in startTime))
                {
                    string text2 = "Invalid " + item.ConstellationType.ToLongName() + " almanac file \"" +
                                   Path.GetFileName(almanacPath) + "\"";
                    RLLogger.GetLogger().LogMessage(text2);
                    Console.WriteLine(text2);
                    //MessageBox.Show(Application.Current.MainWindow, text2, "SatGen error", MessageBoxButton.OK, MessageBoxImage.Hand);
                    return;
                }

                AlmanacBase almanac = item.Almanac;
                GnssTime simulationTime = interval.Start;
                almanac.UpdateAlmanacForTime(in simulationTime);
            }

            Console.WriteLine(
                (config.SignalTypes, trajectory, interval, output,
                    /*readOnlyList*/ 0, config.Mask, config.Attenuation).ToJSON());

            var simulation = Simulation.Create(new SimulationParams(config.SignalTypes, trajectory, in interval, output,
                readOnlyList, config.Mask, config.Attenuation));

            //checkiio();

            DoPatch();

            simulation.Start();
            var progress = 0.0;
            simulation.ProgressChanged += (o, e) => { progress = e.Progress; };

            while (simulation.SimulationState != SimulationState.Finished)
            {
                Thread.Sleep(1000);
                Console.WriteLine("{0}  {1}  ", progress * 100.0, simulation.SimulationState);
            }
        }

        private static string GetAlmanacPath(ConstellationType constellationType, ConfigFile config)
        {
            switch (constellationType)
            {
                case ConstellationType.Glonass:
                    return config.GlonassAlmanacFile;
                case ConstellationType.BeiDou:
                    return config.BeiDouAlmanacFile;
                case ConstellationType.Galileo:
                    return config.GalileoAlmanacFile;
                case ConstellationType.Navic:
                    return config.NavicAlmanacFile;
                default:
                    return config.GpsAlmanacFile;
            }
        }

        private static void install(MethodInfo methodToReplace, MethodInfo methodToInject)
        {
            RuntimeHelpers.PrepareMethod(methodToReplace.MethodHandle);
            RuntimeHelpers.PrepareMethod(methodToInject.MethodHandle);

            unsafe
            {
                if (IntPtr.Size == 4)
                {
                    int* inj = (int*) methodToInject.MethodHandle.Value.ToPointer() + 2;
                    int* tar = (int*) methodToReplace.MethodHandle.Value.ToPointer() + 2;

                    //Console.WriteLine("\nVersion x86 Release\n");
                    *tar = *inj;
                }
                else
                {
                    long* inj = (long*) methodToInject.MethodHandle.Value.ToPointer() + 1;
                    long* tar = (long*) methodToReplace.MethodHandle.Value.ToPointer() + 1;

                    //Console.WriteLine("\nVersion x64 Release\n");
                    *tar = *inj;
                }
            }
        }

        public static void HijackMethod(Type sourceType, string sourceMethod, Type targetType, string targetMethod)
        {
            // Get methods using reflection
            var source = sourceType.GetMethod(sourceMethod);
            var target = targetType.GetMethod(targetMethod);

            // Prepare methods to get machine code (not needed in this example, though)
            RuntimeHelpers.PrepareMethod(source.MethodHandle);
            RuntimeHelpers.PrepareMethod(target.MethodHandle);

            var sourceMethodDescriptorAddress = source.MethodHandle.Value;
            var targetMethodMachineCodeAddress = target.MethodHandle.GetFunctionPointer();

            // Pointer is two pointers from the beginning of the method descriptor
            Marshal.WriteIntPtr(sourceMethodDescriptorAddress, 2 * IntPtr.Size, targetMethodMachineCodeAddress);
        }

        public static void HijackMethod(MethodBase source, MethodBase target)
        {
            RuntimeHelpers.PrepareMethod(source.MethodHandle);
            RuntimeHelpers.PrepareMethod(target.MethodHandle);


            var offset = 2 * IntPtr.Size;
            IntPtr sourceAddress = Marshal.ReadIntPtr(source.MethodHandle.Value, offset);
            IntPtr targetAddress = Marshal.ReadIntPtr(target.MethodHandle.Value, offset);

            var is32Bit = IntPtr.Size == 4;
            byte[] instruction;

            if (is32Bit)
            {
                instruction = new byte[] {
                    0x68, // push <value>
                }
                 .Concat(BitConverter.GetBytes((int)targetAddress))
                 .Concat(new byte[] {
                    0xC3 //ret
                 }).ToArray();
            }
            else
            {
                instruction = new byte[] {
                    0x48, 0xB8 // mov rax <value>
                }
                .Concat(BitConverter.GetBytes((long)targetAddress))
                .Concat(new byte[] {
                    0x50, // push rax
                    0xC3  // ret
                }).ToArray();
            }

            Marshal.Copy(instruction, 0, sourceAddress, instruction.Length);
        }

        /*
        private static void App_Startup(object sender, StartupEventArgs e)
        {
            ((Application) sender).MainWindow.Show();
        }
        */
        private static void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {

        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (args.Name.Contains(".resou"))
            {
                Assembly assemTBC = Assembly.Load("Racelogic.Gnss.SatGen.BlackBox");
                //return assemTBC;
            }

            return null;
        }

        private static bool CheckFeature_pre(ref bool __result)
        {
            Console.WriteLine("CheckFeature_pre Run");
            __result = true;
            return false;
        }

        private static bool CheckFeature_orig()
        {
            return true;
        }

        private static void CheckFeature_post()
        {
        }

        private static bool newline(ref string __result)
        {
            __result = "\r\n";
            return false;
        }

        private static bool WriteBlocks(ref int __result, IntPtr bufferPointer, int numBytesToWrite)
        {
            __result = numBytesToWrite;
            byte[] buffer = new byte[numBytesToWrite];
            Marshal.Copy(bufferPointer, buffer, 0, numBytesToWrite);
            //stream.Write(buffer, 0, numBytesToWrite);
            //tx_buffer.fill(buffer);


            //if (pipeServer.IsConnected)
            {
                Console.WriteLine(".");
                pipeServer.Write(buffer, 0, numBytesToWrite);
            }

            return false;
        }

        private static string filename = "";
        private static FileStream stream;
        private static IOBuffer tx_buffer;


        private static NamedPipeServerStream pipeServer =
            new NamedPipeServerStream("testpipe", PipeDirection.InOut, 1, PipeTransmissionMode.Byte,
                PipeOptions.None, 1000, 3000000 * 2 * 2 * 2);

        private static void OpenForWriting(object __instance, string fileName)
        {
            filename = fileName;
            stream = new FileStream(fileName, FileMode.Create);

            pipeServer.WaitForConnection();
        }

        private static bool Close(ref bool __result)
        {
            stream?.Close();
            return false;
        }

   
    }


    /*
    public class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            RLLogger.GetLogger().Initialize("SatGen3", useLocalAppDataFolder: true);
            RLLogger.GetLogger().LogUnhandledExceptions();
        }
    }
    */
}