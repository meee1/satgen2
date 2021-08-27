using System;
using System.Diagnostics;
using System.Linq.Expressions;

namespace Racelogic.DataTypes
{
	public static class GenericOperator<T, U, V>
	{
		private static readonly T zero = ((typeof(T).IsValueType && typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(Nullable<>)) ? ((T)Activator.CreateInstance(typeof(T).GetGenericArguments()[0])) : default(T));

		private static readonly Func<T, T> increment = MakeFunction<T>(Expression.Increment);

		private static readonly Func<T, T> decrement = MakeFunction<T>(Expression.Decrement);

		private static readonly Func<T, U, V> add = MakeFunction<T, U, V>(Expression.Add);

		private static readonly Func<T, U, V> subtract = MakeFunction<T, U, V>(Expression.Subtract);

		private static readonly Func<T, U, V> multiply = MakeFunction<T, U, V>(Expression.Multiply);

		private static readonly Func<T, U, V> divide = MakeFunction<T, U, V>(Expression.Divide);

		public static T Zero
		{
			[DebuggerStepThrough]
			get
			{
				return zero;
			}
		}

		public static Func<T, U, V> Add
		{
			[DebuggerStepThrough]
			get
			{
				return add;
			}
		}

		public static Func<T, U, V> Subtract
		{
			[DebuggerStepThrough]
			get
			{
				return subtract;
			}
		}

		public static Func<T, U, V> Multiply
		{
			[DebuggerStepThrough]
			get
			{
				return multiply;
			}
		}

		public static Func<T, U, V> Divide
		{
			[DebuggerStepThrough]
			get
			{
				return divide;
			}
		}

		public static Func<T, T> Increment
		{
			[DebuggerStepThrough]
			get
			{
				return GenericOperator<T, T, T>.increment;
			}
		}

		public static Func<T, T> Decrement
		{
			[DebuggerStepThrough]
			get
			{
				return GenericOperator<T, T, T>.decrement;
			}
		}

		private static Func<X, Y, Z> MakeFunction<X, Y, Z>(Func<Expression, Expression, BinaryExpression> operation)
		{
			ParameterExpression parameterExpression = Expression.Parameter(typeof(X), "arg1");
			ParameterExpression parameterExpression2 = Expression.Parameter(typeof(Y), "arg2");
			try
			{
				return Expression.Lambda<Func<X, Y, Z>>(operation(parameterExpression, parameterExpression2), new ParameterExpression[2] { parameterExpression, parameterExpression2 }).Compile();
			}
			catch (Exception ex2)
			{
				Exception ex = ex2;
				return delegate
				{
					throw new InvalidOperationException(ex.Message, ex);
				};
			}
		}

		private static Func<X, X> MakeFunction<X>(Func<Expression, UnaryExpression> operation)
		{
			ParameterExpression parameterExpression = Expression.Parameter(typeof(X), "arg");
			try
			{
				return Expression.Lambda<Func<X, X>>(operation(parameterExpression), new ParameterExpression[1] { parameterExpression }).Compile();
			}
			catch (Exception ex2)
			{
				Exception ex = ex2;
				return delegate
				{
					throw new InvalidOperationException(ex.Message, ex);
				};
			}
		}
	}
}
