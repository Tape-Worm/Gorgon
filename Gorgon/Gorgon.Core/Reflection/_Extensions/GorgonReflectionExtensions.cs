#region MIT
// 
// Gorgon.
// Copyright (C) 2015 Michael Winsor
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: Wednesday, May 27, 2015 8:49:48 PM
// 
#endregion

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Gorgon.Core.Properties;

namespace Gorgon.Reflection
{
	/// <summary>
	/// Delegate method used to create a new object instance with a generic type.
	/// </summary>
	/// <typeparam name="T">Type of object returned by the activator method.</typeparam>
	/// <param name="args">Arguments to pass to the activator.</param>
	/// <returns>The new object, strongly typed.</returns>
	public delegate T ObjectActivator<out T>(params object[] args);

	/// <summary>
	/// Delegate method used to host a property getter method.
	/// </summary>
	/// <typeparam name="T">The type of object that hosts the property.</typeparam>
	/// <typeparam name="TP">Type of value returned by the property getter.</typeparam>
	/// <param name="instance">The instance of the object that contains the property to retrieve.</param>
	/// <returns>The value stored in the property.</returns>
	public delegate TP PropertyGetter<in T, out TP>(T instance);

	/// <summary>
	/// Delegate method used to host a property setter method.
	/// </summary>
	/// <typeparam name="T">The type of object that hosts the property.</typeparam>
	/// <typeparam name="TP">Type of value set by the property setter.</typeparam>
	/// <param name="instance">The instance of the object that contains the property to set.</param>
	/// <param name="value">The value to assign to the property.</param>
	public delegate void PropertySetter<in T, in TP>(T instance, TP value);

	/// <summary>
	/// Utility extensions to be used with reflection types.
	/// </summary>
	/// <remarks>
	/// The extensions present in this type are meant to be used as alternatives to reflection functionality, which often has poor performance. 
	/// </remarks>
	public static class GorgonReflectionExtensions
	{
		/// <summary>
		/// Function to retrieve
		/// </summary>
		/// <param name="objectType">The type of object to evaluate.</param>
		/// <param name="paramTypes">The parameter types on the constructor to find.</param>
		/// <returns>The constructor info for the constructor that matches the parameter types, or <b>null</b> (<i>Nothing</i> in VB.Net) if no matching constructor is found.</returns>
		private static Tuple<ConstructorInfo, ParameterInfo[]> GetConstructor(Type objectType, Type[] paramTypes)
		{
			if (paramTypes == null)
			{
				paramTypes = new Type[0];
			}

			// Look for the constructor with the most parameters.
			var constructors = (from constructorInfo in objectType.GetConstructors()
			                   let parameters = constructorInfo.GetParameters()
			                   where (paramTypes.Length == parameters.Length)
			                   select new Tuple<ConstructorInfo, ParameterInfo[]>
			                          (
				                          constructorInfo,
				                          parameters
			                          )).ToArray();

			if (constructors.Length == 0)
			{
				return null;
			}

			if ((paramTypes.Length == 0) && (constructors.Length == 1))
			{
				return constructors[0];
			}

			return constructors.FirstOrDefault(item => paramTypes.SequenceEqual(item.Item2.Select(propItem => propItem.ParameterType)));
		}

		/// <summary>
		/// Function to create a property getter method.
		/// </summary>
		/// <typeparam name="T">Type of the object that contains the property. This type must be the same as the type that contains the <paramref name="propertyInfo"/> parameter.</typeparam>
		/// <typeparam name="TP">Type of the value to retrieve from the property. This type must be the same as the property type. (e.g. <c>string Property {get; set;} // With this property TP should be a string.</c></typeparam>
		/// <param name="propertyInfo">The property information used to generate the method to retrieve the property value.</param>
		/// <remarks>
		/// <para>
		/// This method will create a lambda method that will retrieve the value of a property for a given instance. The method generated by this method is meant to 
		/// replace the use of the <see cref="PropertyInfo.GetValue(object)"/> reflection method.
		/// </para>
		/// <para>
		/// When there is a need to dynamically get a property at runtime, most developers fall back to using reflection. However, reflection performance is not optimal. 
		/// Thus, this method will create a lambda method that will directly return the property on any instance at run time.  This avoids the overhead associated with
		/// reflection and is far more performant.
		/// </para>
		/// <para>
		/// Because this method uses an expression tree to compile the method, it is recommended that the application build a cache of these methods and call them from 
		/// the cache. Failure to do so may result in drastic performance drops.
		/// </para>
		/// <para>
		/// This code was derived from a blog post by Mariano Omar Rodriguez at <a href="http://weblogs.asp.net/marianor/using-expression-trees-to-get-property-getter-and-setters" target="_blank">http://weblogs.asp.net/marianor/using-expression-trees-to-get-property-getter-and-setters</a>
		/// </para>
		/// </remarks>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="propertyInfo"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="propertyInfo"/> has a <b>null</b> <see cref="MemberInfo.DeclaringType"/> property.</exception>
		/// <exception cref="InvalidCastException">Thrown when the declaring type of the property and type specified by <typeparamref name="T"/> are not the same, and the declaring type does not inherit from <typeparamref name="T"/>.
		/// <para>-or-</para>
		/// <para>Thrown when the type of the property does not match the type specified by <typeparamref name="TP"/>, or <see cref="object"/>.</para>
		/// </exception>
		/// <returns>The method that will retrieve a property value from an instance.</returns>
		public static PropertyGetter<T, TP> CreatePropertyGetter<T, TP>(this PropertyInfo propertyInfo)
		{
			if (propertyInfo == null)
			{
				throw new ArgumentNullException(nameof(propertyInfo));
			}

			if (propertyInfo.DeclaringType == null)
			{
				throw new ArgumentException(string.Format(Resources.GOR_ERR_PROPERTY_NO_DECLARING_TYPE, propertyInfo.Name));
			}

			if ((propertyInfo.DeclaringType != typeof(T)) && (!propertyInfo.DeclaringType.IsSubclassOf(typeof(T))))
			{
				throw new InvalidCastException(string.Format(Resources.GOR_ERR_PROPERTY_DECLARING_TYPE_MISMATCH,
				                                             propertyInfo.Name,
				                                             propertyInfo.DeclaringType.FullName,
				                                             typeof(T).FullName));
			}

			Type propertyType = typeof(TP);

			if ((propertyType != propertyInfo.PropertyType) && (propertyType != typeof(object)))
			{
				throw new InvalidCastException(string.Format(Resources.GOR_ERR_PROPERTY_TYPE_MISMATCH,
				                                             propertyInfo.Name,
				                                             propertyInfo.PropertyType.FullName,
				                                             propertyType.FullName));
			}
			 
			ParameterExpression instance;
			MemberExpression property;

			if (typeof(T) != propertyInfo.DeclaringType)
			{
				instance = Expression.Parameter(typeof(T), "parentInstance");
				UnaryExpression cast = Expression.TypeAs(instance, propertyInfo.DeclaringType);
				property = Expression.Property(cast, propertyInfo);
			}
			else
			{
				instance =  Expression.Parameter(propertyInfo.DeclaringType, "instance");
				property = Expression.Property(instance, propertyInfo);
			}

			if (propertyInfo.PropertyType == propertyType)
			{
				return Expression.Lambda<PropertyGetter<T, TP>>(property, instance).Compile();
			}

			UnaryExpression converter = Expression.TypeAs(property, typeof(object));

			return Expression.Lambda<PropertyGetter<T, TP>>(converter, instance).Compile();
		}

		/// <summary>
		/// Function to create a property setter method.
		/// </summary>
		/// <typeparam name="T">Type of the object that contains the property. This type must be the same as the type that contains the <paramref name="propertyInfo"/> parameter.</typeparam>
		/// <typeparam name="TP">Type of the value to retrieve from the property. This type must be the same as the property type. (e.g. <c>string Property {set; set;} // With this property TP should be a string.</c></typeparam>
		/// <param name="propertyInfo">The property information used to generate the method to set the property value.</param>
		/// <remarks>
		/// <para>
		/// This method will create a lambda method that will assign a value to a property for a given instance. The method generated by this method is meant to 
		/// replace the use of the <see cref="PropertyInfo.SetValue(object,object)"/> reflection method.
		/// </para>
		/// <para>
		/// When there is a need to dynamically set a property at runtime, most developers fall back to using reflection. However, reflection performance is not optimal. 
		/// Thus, this method will create a lambda method that will directly assign the property on any instance at run time.  This avoids the overhead associated with
		/// reflection and is far more performant.
		/// </para>
		/// <para>
		/// Because this method uses an expression tree to compile the method, it is recommended that the application build a cache of these methods and call them from 
		/// the cache. Failure to do so may result in drastic performance drops.
		/// </para>
		/// <para>
		/// This code was derived from a blog post by Mariano Omar Rodriguez at <a href="http://weblogs.asp.net/marianor/using-expression-trees-to-get-property-getter-and-setters" target="_blank">http://weblogs.asp.net/marianor/using-expression-trees-to-get-property-getter-and-setters</a>
		/// </para>
		/// </remarks>
		/// <returns>The method that will assign a value to a property on an instance.</returns>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="propertyInfo"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="propertyInfo"/> has a <b>null</b> <see cref="MemberInfo.DeclaringType"/> property.</exception>
		/// <exception cref="InvalidCastException">Thrown when the declaring type of the property and type specified by <typeparamref name="T"/> are not the same, and the declaring type does not inherit from <typeparamref name="T"/>.
		/// <para>-or-</para>
		/// <para>Thrown when the type of the property does not match the type specified by <typeparamref name="TP"/>, or <see cref="object"/>.</para>
		/// </exception>
		public static PropertySetter<T, TP> CreatePropertySetter<T, TP>(this PropertyInfo propertyInfo)
		{
			if (propertyInfo == null)
			{
				throw new ArgumentNullException(nameof(propertyInfo));
			}

			if (propertyInfo.DeclaringType == null)
			{
				throw new ArgumentException(string.Format(Resources.GOR_ERR_PROPERTY_NO_DECLARING_TYPE, propertyInfo.Name));
			}

			if ((propertyInfo.DeclaringType != typeof(T)) && (!propertyInfo.DeclaringType.IsSubclassOf(typeof(T))))
			{
				throw new InvalidCastException(string.Format(Resources.GOR_ERR_PROPERTY_DECLARING_TYPE_MISMATCH,
															 propertyInfo.Name,
															 propertyInfo.DeclaringType.FullName,
															 typeof(T).FullName));
			}

			Type propertyType = typeof(TP);

			if ((propertyType != propertyInfo.PropertyType) && (propertyType != typeof(object)))
			{
				throw new InvalidCastException(string.Format(Resources.GOR_ERR_PROPERTY_TYPE_MISMATCH,
															 propertyInfo.Name,
															 propertyInfo.PropertyType.FullName,
															 typeof(TP).FullName));
			}

			ParameterExpression instance;
			MemberExpression property;

			if (typeof(T) != propertyInfo.DeclaringType)
			{
				instance = Expression.Parameter(typeof(T), "instance");
				UnaryExpression cast = Expression.TypeAs(instance, propertyInfo.DeclaringType);
				property = Expression.Property(cast, propertyInfo);
			}
			else
			{
				instance = Expression.Parameter(propertyInfo.DeclaringType, "instance");
				property = Expression.Property(instance, propertyInfo);
			}

			ParameterExpression arg = Expression.Parameter(propertyType, "arg0");
			BinaryExpression assignment;

			if (propertyInfo.PropertyType != propertyType)
			{
				UnaryExpression converter = Expression.Convert(arg, propertyInfo.PropertyType);
				assignment = Expression.Assign(property, converter);
			}
			else
			{
				assignment = Expression.Assign(property, arg);
			}

			return Expression.Lambda<PropertySetter<T, TP>>(assignment, instance, arg).Compile();
		}

		/// <summary>
		/// Function to create a new activator delegate.
		/// </summary>
		/// <typeparam name="T">The type of object created by the activation method generated by this method. This type must be the same type as the <paramref name="type"/> parameter, or must be of type <see cref="object"/>.</typeparam>
		/// <param name="type">Type of object to construct.</param>
		/// <param name="paramTypes">The parameter types used to match the appropriate object constructor.</param>
		/// <returns>An activator object used to create an object.</returns>
		/// <remarks>
		/// <para>
		/// This method is used to generate a new object that will create, at runtime, an instance of an object. This is meant to replace calls to 
		/// <see cref="Activator.CreateInstance(System.Type)"/> since that method does not perform well.
		/// </para>
		/// <para>
		/// This method uses a lambda expression tree to build a method that calls the constructor for the type.  Developers who use this functionality 
		/// should cache their return values so that performance does not suffer from compiling multiple methods with various types.
		/// </para>
		/// <para>
		/// The activator delegate that is generated by this method uses the constructor of the type with the parameter types (in order) 
		/// specified by <paramref name="paramTypes"/>. If no constructor with the specified parameter types is found, then an exception is raised. If 
		/// no parameter types are passed to this method, then a parameterless constructor is assumed.
		/// </para>
		/// </remarks>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="type"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="TypeLoadException">Thrown when the type does not contain a constructor with the specified <paramref name="paramTypes"/>.</exception>
		/// <exception cref="InvalidCastException">Thrown when the type of the generic type parameter <typeparamref name="T"/> is not the same as the <paramref name="type"/> parameter.</exception>
		public static ObjectActivator<T> CreateActivator<T>(this Type type, params Type[] paramTypes)
		{
			if (type == null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			if (type.IsInterface)
			{
				throw new TypeLoadException(string.Format(Resources.GOR_ERR_ACTIVATOR_CANNOT_CREATE_INTERFACE_TYPE, type.FullName));
			}

			if (type.IsAbstract)
			{
				throw new TypeLoadException(string.Format(Resources.GOR_ERR_ACTIVATOR_CANNOT_CREATE_ABSTRACT, type.FullName));
			}

			Type typeT = typeof(T);

			bool isSubClass = type != typeT && type.IsSubclassOf(typeT);
			bool isInterfaceOf = type != typeT && typeT.IsInterface && typeT.IsAssignableFrom(type);

			if ((type != typeT) && (!isSubClass) && (!isInterfaceOf))
			{
				throw new InvalidCastException(string.Format(Resources.GOR_ERR_ACTIVATOR_TYPE_MISMATCH, type.FullName, typeT.FullName));
			}

			if (paramTypes == null)
			{
				paramTypes = new Type[0];
			}

			Tuple<ConstructorInfo, ParameterInfo[]> constructor = GetConstructor(type, paramTypes);

			if (constructor == null)
			{
				throw new TypeLoadException(string.Format(Resources.GOR_ERR_ACTIVATOR_CANNOT_FIND_CONSTRUCTOR, type.FullName));
			}

			ParameterExpression paramExpr = Expression.Parameter(typeof(object[]), "args");
			Expression[] argumentsExpr = new Expression[constructor.Item2.Length];

			//pick each arg from the params array 
			//and create a typed expression of them
			for (int i = 0; i < constructor.Item2.Length; ++i)
			{
				Expression index = Expression.Constant(i);
				Type paramType = constructor.Item2[i].ParameterType;

				Expression paramAccessorExp =
					Expression.ArrayIndex(paramExpr, index);

				Expression paramCastExp =
					Expression.Convert(paramAccessorExp, paramType);

				argumentsExpr[i] = paramCastExp;
			}

			//make a NewExpression that calls the
			//ctor with the args we just created
			NewExpression newExp = Expression.New(constructor.Item1, argumentsExpr);

			//create a lambda with the New
			//Expression as body and our param object[] as arg
			LambdaExpression lambda =
				Expression.Lambda(typeof(ObjectActivator<T>), newExp, paramExpr);

			//compile it
			ObjectActivator<T> compiled = (ObjectActivator<T>)lambda.Compile();
			return compiled;
		}
	}
}
