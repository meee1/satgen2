namespace XamlGeneratedNamespace
{
    using System;
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Windows.Markup;

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class GeneratedInternalTypeHelper : InternalTypeHelper
    {
        protected override object CreateInstance(Type type, CultureInfo culture)
        {
            return Activator.CreateInstance(type,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.CreateInstance,
                null, null, culture);
        }

        protected override object GetPropertyValue(PropertyInfo propertyInfo, object target, CultureInfo culture)
        {
            return propertyInfo.GetValue(target, BindingFlags.Default, null, null, culture);
        }

        protected override void SetPropertyValue(PropertyInfo propertyInfo, object target, object value,
            CultureInfo culture)
        {
            propertyInfo.SetValue(target, value, BindingFlags.Default, null, null, culture);
        }

        protected override Delegate CreateDelegate(Type delegateType, object target, string handler)
        {
            return (Delegate) target.GetType().InvokeMember("_CreateDelegate",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod, null, target, new object[2]
                {
                    delegateType,
                    handler
                }, null);
        }

        protected override void AddEventHandler(EventInfo eventInfo, object target, Delegate handler)
        {
            eventInfo.AddEventHandler(target, handler);
        }
    }
}