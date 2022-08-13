using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace Racelogic.Libraries.Nmea;

public static class ExtensionMethods
{
	public static void Fire(this PropertyChangedEventHandler handler, object sender, string propertyName)
	{
		handler?.Invoke(sender, new PropertyChangedEventArgs(propertyName));
	}

	public static void Fire<TProperty>(this PropertyChangedEventHandler handler, object sender, Expression<Func<TProperty>> property)
	{
		handler.Fire(sender, property.GetMemberInfo().Name);
	}

	public static MemberInfo GetMemberInfo(this Expression expression)
	{
		LambdaExpression lambdaExpression = (LambdaExpression)expression;
		MemberExpression memberExpression = ((!(lambdaExpression.Body is UnaryExpression)) ? ((MemberExpression)lambdaExpression.Body) : ((MemberExpression)((UnaryExpression)lambdaExpression.Body).Operand));
		return memberExpression.Member;
	}
}
