using System;
using System.Windows.Input;

namespace Racelogic.Core;

public class RacelogicCommand : ICommand
{
	public delegate void ICommandOnExecute(object parameter);

	public delegate bool ICommandOnCanExecute(object parameter);

	private ICommandOnExecute _execute;

	private ICommandOnCanExecute _canExecute;

	private IAddRemoveEventHandler _addRemoveEventHandler;

	public event EventHandler CanExecuteChanged
	{
		add
		{
			_addRemoveEventHandler?.AddHandler(value);
		}
		remove
		{
			_addRemoveEventHandler?.RemoveHandler(value);
		}
	}

	public RacelogicCommand(ICommandOnExecute onExecuteMethod, ICommandOnCanExecute onCanExecuteMethod, IAddRemoveEventHandler addRemoveEventHandler)
	{
		_execute = onExecuteMethod;
		_canExecute = onCanExecuteMethod;
		_addRemoveEventHandler = addRemoveEventHandler;
	}

	public bool CanExecute(object parameter)
	{
		return _canExecute(parameter);
	}

	public void Execute(object parameter)
	{
		_execute(parameter);
	}
}
