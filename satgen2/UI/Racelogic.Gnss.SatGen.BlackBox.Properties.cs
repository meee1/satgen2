// Racelogic.Gnss.SatGen.BlackBox.Properties
// Racelogic.Gnss.SatGen.BlackBox.Properties.Resources
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;
using Racelogic.Gnss.SatGen.BlackBox.Properties;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Racelogic.Gnss.SatGen.BlackBox.Properties;

namespace Racelogic.Gnss.SatGen.BlackBox.Properties
{
    [GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
    [DebuggerNonUserCode]
    [CompilerGenerated]
    internal class Resources
    {
        private static ResourceManager resourceMan;

        private static CultureInfo resourceCulture;

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static ResourceManager ResourceManager
        {
            get
            {
                if (resourceMan == null)
                {
                    resourceMan = new ResourceManager("Racelogic.Gnss.SatGen.BlackBox.Properties.Resources",
                        typeof(Resources).Assembly);
                }

                return resourceMan;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static CultureInfo Culture
        {
            get { return resourceCulture; }
            set { resourceCulture = value; }
        }

        internal Resources()
        {
        }
    }
// Racelogic.Gnss.SatGen.BlackBox.Properties.Settings


    [CompilerGenerated]
    [GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "15.9.0.0")]
    internal sealed class Settings : ApplicationSettingsBase
    {
        private static Settings defaultInstance = (Settings) SettingsBase.Synchronized(new Settings());

        public static Settings Default => defaultInstance;

        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("8")]
        public int LiveSatCountLimitMode
        {
            get { return (int) this["LiveSatCountLimitMode"]; }
            set { this["LiveSatCountLimitMode"] = value; }
        }

        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("8")]
        public int LiveGpsSatCountLimit
        {
            get { return (int) this["LiveGpsSatCountLimit"]; }
            set { this["LiveGpsSatCountLimit"] = value; }
        }

        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("8")]
        public int LiveGlonassSatCountLimit
        {
            get { return (int) this["LiveGlonassSatCountLimit"]; }
            set { this["LiveGlonassSatCountLimit"] = value; }
        }

        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("8")]
        public int LiveBeiDouSatCountLimit
        {
            get { return (int) this["LiveBeiDouSatCountLimit"]; }
            set { this["LiveBeiDouSatCountLimit"] = value; }
        }

        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("8")]
        public int LiveGalileoSatCountLimit
        {
            get { return (int) this["LiveGalileoSatCountLimit"]; }
            set { this["LiveGalileoSatCountLimit"] = value; }
        }

        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("99")]
        public int LiveAutomaticSatCountLimit
        {
            get { return (int) this["LiveAutomaticSatCountLimit"]; }
            set { this["LiveAutomaticSatCountLimit"] = value; }
        }

        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("False")]
        public bool LiveAttenuationsLinked
        {
            get { return (bool) this["LiveAttenuationsLinked"]; }
            set { this["LiveAttenuationsLinked"] = value; }
        }

        internal Settings()
        {
        }

        private void SettingChangingEventHandler(object sender, SettingChangingEventArgs e)
        {
        }

        private void SettingsSavingEventHandler(object sender, CancelEventArgs e)
        {
        }
    }
}