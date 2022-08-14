using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows;
using Racelogic.Utilities;

namespace Racelogic.Gnss.SatGen.BlackBox;

public class App : Application
{
	protected override void OnStartup(StartupEventArgs e)
	{
		base.OnStartup(e);
		RLLogger.GetLogger().Initialize("SatGen3", useLocalAppDataFolder: true);
		RLLogger.GetLogger().LogUnhandledExceptions();
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.8.1.0")]
	public void InitializeComponent()
	{
		base.StartupUri = new Uri("MainWindow.xaml", UriKind.Relative);
	}

	[STAThread]
	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.8.1.0")]
	public static void Main()
	{
		App app = new App();
		app.InitializeComponent();
		app.Run();
	}
}
