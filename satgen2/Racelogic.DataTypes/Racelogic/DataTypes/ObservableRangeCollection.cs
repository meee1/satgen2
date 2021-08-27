using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;

namespace Racelogic.DataTypes
{
	public class ObservableRangeCollection<T> : ObservableCollection<T>
	{
		private bool useResetAction;

		public bool UseResetAction
		{
			[DebuggerStepThrough]
			get
			{
				return useResetAction;
			}
			[DebuggerStepThrough]
			set
			{
				useResetAction = value;
			}
		}

		public ObservableRangeCollection()
		{
		}

		public ObservableRangeCollection(IEnumerable<T> collection)
			: base(collection)
		{
		}

		public ObservableRangeCollection(List<T> collection)
			: base(collection)
		{
		}

		public void AddRange(IEnumerable<T> newItems)
		{
			List<T> list = new List<T>(newItems);
			foreach (T item in list)
			{
				base.Items.Add(item);
			}
			OnPropertyChanged(new PropertyChangedEventArgs("Count"));
			OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
			NotifyCollectionChangedEventArgs e = ((!useResetAction) ? new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, list) : new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
			OnCollectionChanged(e);
		}
	}
}
