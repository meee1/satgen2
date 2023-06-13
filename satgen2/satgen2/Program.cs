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
using Racelogic.Core.Filters;
using Racelogic.DataTypes;
using Racelogic.Geodetics;
using Racelogic.Gnss;
using Racelogic.Gnss.SatGen;
using Racelogic.Gnss.SatGen.Gps;
using Racelogic.Libraries.Nmea;
using Racelogic.Maths;
using Racelogic.Utilities;
using Channel = Racelogic.Gnss.SatGen.Channel;


namespace satgen2
{
    public class Program
    {
        //https://cddis.nasa.gov/archive/gnss/data/daily/2022/225/22n/brdc2250.22n.gz
        //https://gist.github.com/patapovich/c69a41fcc7f673bac6d8d431f97907af

        //ftp://cddis.gsfc.nasa.gov/gnss/data/daily/$current_year/brdc/
        //wget -4 "ftp://cddis.gsfc.nasa.gov/gnss/data/daily/$current_year/brdc/$latest_version"


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

            Trace.Listeners.Add(new TextWriterTraceListener("log.log", "tracelog"));

            RLLogger.GetLogger().LogMessage("start");
            string[] commandLineArgs = Environment.GetCommandLineArgs();
            ConfigFile config = ConfigFile.Read(commandLineArgs[1]);

            Console.WriteLine(config.ToJSON());

            //Environment.NewLine = "\r\n";

            //(NmeaFile nmeaFile = new NmeaFile(config.NmeaFile));
   


            string text = config.OutputFile.ToLower();
            string a = Path.GetExtension(text).ToLowerInvariant();
            Quantization bitsPerSample = (Quantization) config.BitsPerSample;
            //var output = new LabSat3wOutput(config.OutputFile, config.SignalTypes, bitsPerSample);

            //var output = new BladeRFFileOutput(config.OutputFile, config.SignalTypes, (int)MHZ(10.5));

            //var output = new EightBitOutput(config.OutputFile, config.SignalTypes, (int)MHZ(12));

            var output = new PipeOutput(config.OutputFile, config.SignalTypes, (int)MHZ(10.5));

            Console.WriteLine(output.ChannelPlan.ToJSON());

            GnssTime startTime = GnssTime.FromUtc(config.Date);

            Console.WriteLine(startTime.ToJSON());

            //Trajectory trajectory = new NmeaFileTrajectory(in startTime, config.NmeaFile, config.GravitationalModel);

            //new LiveNmeaTrajectory(DateTime.Now, "df", 115200);
            var trajectory = new FakeLiveNmeaTrajectory(GnssTime.Now, 1);

            var lat = 0.0;
            var lng = 0.0;
            var alt = 10000.0;

            if (args.Count() > 3)
            {
                lat = double.Parse(args[1]);
                lng = double.Parse(args[2]);
                alt = double.Parse(args[3]);
            }

            //((FakeLiveNmeaTrajectory)trajectory).ecef = Geodetic.FromDegrees(lat ,  lng, alt).ToEcef(Datum.WGS84, Geoid.Egm96);

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
                /*
                foreach (var almanacBaselineSatellite in item.Almanac.BaselineSatellites)
                {
                    Satellite eph = almanacBaselineSatellite as Satellite;
                    var data = new byte[0];

                    ///https://github.com/tomojitakasu/RTKLIB/blob/master/src/rinex.c#L1028
                    eph.Af0 = data[0];
                    eph.Af1 = data[1];
                    eph.Af2 = data[2];

                    eph.SqrtA = SQR(data[10]); eph.Eccentricity = data[8]; eph.i0 = data[15]; eph.OMG0 = data[13];
                    eph.omg = data[17]; eph.M0 = data[6]; eph.deln = data[5]; eph.OMGd = data[18];
                    eph.idot = data[19]; eph.Crc = data[16]; eph.Crs = data[4]; eph.Cuc = data[7];
                    eph.Cus = data[9]; eph.Cic = data[12]; eph.Cis = data[14];

                    eph.iode = (int)data[3];      // IODE 
                    eph.iodc = (int)data[26];      // IODC 
                    eph.toes = data[11];      // toe (s) in gps week 
                    eph.week = (int)data[21];      // gps week 
                    eph.toe = adjweek(gpst2time(eph.week, data[11]), toc);
                    eph.ttr = adjweek(gpst2time(eph.week, data[27]), toc);

                    eph.code = (int)data[20];      // GPS: codes on L2 ch 
                    eph.svh = (int)data[24];      // sv health 
                    eph.sva = uraindex(data[23]);  // ura (m.index) 
                    eph.flag = (int)data[22];      // GPS: L2 P data flag 

                    eph.tgd[0] = data[25];      // TGD 
                    eph.fit = data[28];      // fit interval 


                    
                     	return new Satellite
			{
				ArgumentOfPerigee = satellite.ArgumentOfPerigee,
				Eccentricity = satellite.Eccentricity,
				Health = satellite.Health,
				Id = satellite.Id,
				Inclination = satellite.Inclination,
				IsEnabled = satellite.IsEnabled,
				IsHealthy = satellite.IsHealthy,
				MeanMotionCorrection = satellite.MeanMotionCorrection,
				OrbitType = satellite.OrbitType,
				RateOfInclination = satellite.RateOfInclination,
				RateOfLongitudeOfAscendingNode = satellite.RateOfLongitudeOfAscendingNode,
				SqrtA = satellite.SqrtA,
				IssueOfDataClock = issueOfDataClock,
				LongitudeOfAscendingNode = radians,
				MeanAnomaly = meanAnomaly,
				TimeOfApplicability = gpsSecondOfWeek,
				TransmissionInterval = transmissionInterval,
				Week = gpsWeek
			};
                     
                     
                }*/

            }

            Console.WriteLine(
                (config.SignalTypes, trajectory, interval, output,
                    /*readOnlyList*/ 0, config.Mask, config.Attenuation).ToJSON());

            var simulation = Simulation.Create(new SimulationParams(config.SignalTypes, trajectory, in interval, output,
                readOnlyList, config.Mask, config.Attenuation));

            //checkiio();

            //DoPatch();

            simulation.Start();
            var progress = 0.0;
            simulation.ProgressChanged += (o, e) => { progress = e.Progress; elapesed = e.ElapsedTime; simtime = e.SimulatedTime; };


            while (simulation.SimulationState != SimulationState.Finished)
            {
                Thread.Sleep(1000);
                Console.WriteLine("{0}  {1}  {2}  {3} {4} {5}", progress * 100.0, simulation.SimulationState, simulation.IsAlive, simulation.BufferUnderrunCount, elapesed, simtime);
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

        static byte[] buffer = new byte[0];

        private static bool WriteBlocks(ref int __result, IntPtr bufferPointer, int numBytesToWrite)
        {
            __result = numBytesToWrite;
            if (buffer.Length < numBytesToWrite)
                Array.Resize(ref buffer, numBytesToWrite);
            Marshal.Copy(bufferPointer, buffer, 0, numBytesToWrite);
            //stream.Write(buffer, 0, numBytesToWrite);
            //tx_buffer.fill(buffer);


            //if (pipeServer.IsConnected)
            {
                Console.WriteLine(".");
                   // pipeServer.Write(buffer, 0, numBytesToWrite);
            }

            return false;
        }

        private static string filename = "";
        private static FileStream stream;
        private static IOBuffer tx_buffer;


        //private static NamedPipeServerStream pipeServer = new NamedPipeServerStream("testpipe", PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.None, 1000, 3000000 * 2 * 2 * 2);
        private static TimeSpan elapesed;
        private static GnssTime simtime;

        private static void OpenForWriting(object __instance, string fileName)
        {
            filename = fileName;
            stream = new FileStream(fileName, FileMode.Create);

            //pipeServer.WaitForConnection();
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