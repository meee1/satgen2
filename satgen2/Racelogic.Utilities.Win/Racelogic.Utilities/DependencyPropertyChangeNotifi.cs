using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;

namespace Racelogic.Utilities;

public sealed class DependencyPropertyChangeNotifier : DependencyObject, IDisposable
{
	private WeakReference propertySource;

	public static readonly DependencyProperty PropertyValueProperty = DependencyProperty.Register("PropertyValue", typeof(object), typeof(DependencyPropertyChangeNotifier), new FrameworkPropertyMetadata(null, OnPropertyChanged));

	public DependencyObject PropertySource
	{
		get
		{
			try
			{
				return propertySource.IsAlive ? (propertySource.Target as DependencyObject) : null;
			}
			catch
			{
				return null;
			}
		}
		private set
		{
			propertySource = new WeakReference(value);
		}
	}

	[Description("The value of the watched property")]
	[Category("Behavior")]
	[Bindable(true)]
	[Browsable(false)]
	public object PropertyValue
	{
		get
		{
			return GetValue(PropertyValueProperty);
		}
		set
		{
			SetValue(PropertyValueProperty, value);
		}
	}

	public event EventHandler PropertyValueChanged;

	public DependencyPropertyChangeNotifier(DependencyObject propertySource, string propertyName, EventHandler propertyChangedEventHandler = null)
		: this(propertySource, new PropertyPath(propertyName), propertyChangedEventHandler)
	{
	}

	public DependencyPropertyChangeNotifier(DependencyObject propertySource, DependencyProperty property, EventHandler propertyChangedEventHandler = null)
		: this(propertySource, new PropertyPath(property), propertyChangedEventHandler)
	{
	}

	public DependencyPropertyChangeNotifier(DependencyObject propertySource, PropertyPath propertyPath, EventHandler propertyChangedEventHandler = null)
	{
		if (propertyChangedEventHandler != null)
		{
			PropertyValueChanged -= propertyChangedEventHandler;
			PropertyValueChanged += propertyChangedEventHandler;
		}
		PropertySource = propertySource ?? throw new ArgumentNullException("propertySource");
		Binding binding = new Binding
		{
			Path = (propertyPath ?? throw new ArgumentNullException("propertyPath")),
			Mode = BindingMode.OneWay,
			Source = PropertySource
		};
		BindingOperations.SetBinding(this, PropertyValueProperty, binding);
	}

	private static void OnPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
	{
		if (dependencyObject is DependencyPropertyChangeNotifier dependencyPropertyChangeNotifier && dependencyPropertyChangeNotifier.PropertyValueChanged != null)
		{
			dependencyPropertyChangeNotifier.PropertyValueChanged(dependencyPropertyChangeNotifier.PropertySource, EventArgs.Empty);
		}
	}

	public void Dispose()
	{
		BindingOperations.ClearBinding(this, PropertyValueProperty);
	}
}
