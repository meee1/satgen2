using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Racelogic.Gnss.SatGen.BlackBox;

namespace UI
{
    public class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;
            AppDomain.CurrentDomain.ResourceResolve += CurrentDomain_ResourceResolve;
            AppDomain.CurrentDomain.TypeResolve += CurrentDomain_TypeResolve;

            string[] commandLineArgs = Environment.GetCommandLineArgs();
            if (commandLineArgs.Length != 2)
                Environment.Exit(-1);
            if (!File.Exists(commandLineArgs[1]))
                Environment.Exit(-1);
            //Application.Current.

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



                //harmony.Patch(original, new HarmonyMethod(prefix));

                install(original, typeof(Program).GetMethod("CheckFeature_orig", BindingFlags.Static | BindingFlags.NonPublic));

                // Thread.Sleep(5000);
            }

            var APP =new App();
            APP.Run();

            //Racelogic.Gnss.SatGen.BlackBox.App.Main();
        }

        private static bool CheckFeature_orig()
        {
            return true;
        }


        private static void install(MethodInfo methodToReplace, MethodInfo methodToInject)
        {
            RuntimeHelpers.PrepareMethod(methodToReplace.MethodHandle);
            RuntimeHelpers.PrepareMethod(methodToInject.MethodHandle);

            unsafe
            {
                if (IntPtr.Size == 4)
                {
                    int* inj = (int*)methodToInject.MethodHandle.Value.ToPointer() + 2;
                    int* tar = (int*)methodToReplace.MethodHandle.Value.ToPointer() + 2;

                    //Console.WriteLine("\nVersion x86 Release\n");
                    *tar = *inj;
                }
                else
                {
                    long* inj = (long*)methodToInject.MethodHandle.Value.ToPointer() + 1;
                    long* tar = (long*)methodToReplace.MethodHandle.Value.ToPointer() + 1;

                    //Console.WriteLine("\nVersion x64 Release\n");
                    *tar = *inj;
                }
            }
        }

        private static System.Reflection.Assembly CurrentDomain_TypeResolve(object sender, ResolveEventArgs args)
        {
            Console.WriteLine(args.Name);
            return null;
        }

        private static System.Reflection.Assembly CurrentDomain_ResourceResolve(object sender, ResolveEventArgs args)
        {
            return Assembly.GetAssembly(typeof(App));
            Console.WriteLine(args.Name);
            return null;
        }

        private static void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            Console.WriteLine(args.LoadedAssembly);
        }
    }

    public partial class App : Application
    {
        public App()
        {
            Startup += App_Startup;
        }

        void App_Startup(object sender, StartupEventArgs e)
        {
            try
            {
                string assemblyName = string.Format("{0}\\Racelogic.Gnss.SatGen.BlackBox.exe", new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName);
                Window wnd = LoadAssembly(assemblyName, "MainWindow");
                wnd.Show();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(string.Format("Failed to load window from{0} - {1}", "OtherWindow", ex.Message));
                throw new Exception(String.Format("Failed to load window from{0} - {1}", "OtherWindow", ex.Message), ex);

            }
        }

        private Window LoadAssembly(String assemblyName, String typeName)
        {
            try
            {
                Assembly assemblyInstance = Assembly.LoadFrom(assemblyName);
                foreach (Type t in assemblyInstance.GetTypes().Where(t => String.Equals(t.Name, typeName, StringComparison.OrdinalIgnoreCase)))
                {
                    var wnd = assemblyInstance.CreateInstance(t.FullName) as Window;
                    return wnd;
                }
                throw new Exception("Unable to load external window");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(string.Format("Failed to load window from{0}{1}", assemblyName, ex.Message));
                throw new Exception(string.Format("Failed to load external window{0}", assemblyName), ex);
            }
        }
    }
}
