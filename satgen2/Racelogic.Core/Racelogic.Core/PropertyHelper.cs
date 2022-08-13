using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace Racelogic.Core;

[Obsolete("PropertyHelper is deprecacted and will soon be removed from Racelgic.Core.  Please use 'nameof' keyword instead.")]
public static class PropertyHelper
{
	private static readonly bool fullPropertyCheck = Debugger.IsAttached;

	public static string GetPropertyName<T>(Expression<Func<T>> propertyExpression)
	{
		MemberExpression memberExpression;
		return GetPropertyName(propertyExpression, out memberExpression);
	}

	public static string GetPropertyName<T>(Expression<Func<T, object>> propertyExpression)
	{
		if (propertyExpression == null)
		{
			throw new ArgumentNullException("'propertyExpression' should not be null");
		}
		MemberExpression memberExpression = propertyExpression.Body as MemberExpression;
		if (memberExpression == null)
		{
			memberExpression = ((propertyExpression.Body as UnaryExpression) ?? throw new ArgumentException("The expression should be a member or unary expression", "propertyExpression")).Operand as MemberExpression;
		}
		if (fullPropertyCheck)
		{
			PropertyInfo obj = memberExpression.Member as PropertyInfo;
			if (obj == null)
			{
				throw new ArgumentException("The expression does not access a property.", "propertyExpresssion");
			}
			if (obj.GetGetMethod(nonPublic: true)!.IsStatic)
			{
				throw new ArgumentException("The referenced property is a static property.", "propertyExpresssion");
			}
		}
		return memberExpression.Member.Name;
	}

	public static string GetPropertyName<T>(T propertyOwner, Expression<Func<T, object>> propertyExpression)
	{
		if (propertyExpression == null)
		{
			throw new ArgumentNullException("'propertyExpression' should not be null");
		}
		MemberExpression memberExpression = propertyExpression.Body as MemberExpression;
		if (memberExpression == null)
		{
			memberExpression = ((propertyExpression.Body as UnaryExpression) ?? throw new ArgumentException("The expression should be a member or unary expression", "propertyExpression")).Operand as MemberExpression;
		}
		if (fullPropertyCheck)
		{
			PropertyInfo obj = memberExpression.Member as PropertyInfo;
			if (obj == null)
			{
				throw new ArgumentException("The expression does not access a property.", "propertyExpresssion");
			}
			if (obj.GetGetMethod(nonPublic: true)!.IsStatic)
			{
				throw new ArgumentException("The referenced property is a static property.", "propertyExpresssion");
			}
		}
		return memberExpression.Member.Name;
	}

	internal static string GetPropertyName<T>(Expression<Func<T>> propertyExpression, out object callingObject)
	{
		MemberExpression memberExpression;
		string propertyName = GetPropertyName(propertyExpression, out memberExpression);
		Delegate @delegate = Expression.Lambda(memberExpression.Expression).Compile();
		callingObject = @delegate.DynamicInvoke();
		return propertyName;
	}

	internal static string GetPropertyName<T>(Expression<Func<T>> propertyExpression, out MemberExpression memberExpression)
	{
		if (propertyExpression == null)
		{
			throw new ArgumentNullException("'propertyExpression' should not be null");
		}
		memberExpression = propertyExpression.Body as MemberExpression;
		if (memberExpression == null)
		{
			throw new ArgumentException("The expression should be a member expression", "propertyExpression");
		}
		if (fullPropertyCheck)
		{
			if (!(memberExpression.Expression is ConstantExpression))
			{
				throw new ArgumentException("The expression body should be a constant expression", "propertyExpression");
			}
			PropertyInfo obj = memberExpression.Member as PropertyInfo;
			if (obj == null)
			{
				throw new ArgumentException("The expression does not access a property.", "propertyExpresssion");
			}
			if (obj.GetGetMethod(nonPublic: true)!.IsStatic)
			{
				throw new ArgumentException("The referenced property is a static property.", "propertyExpresssion");
			}
		}
		return memberExpression.Member.Name;
	}
}
