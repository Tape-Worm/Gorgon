// 
// Gorgon.
// Copyright (C) 2025 Michael Winsor
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
// Created: June 7, 2018 3:55:47 PM
// 

using Gorgon.Memory;

namespace Gorgon.Patterns;

/// <summary>
/// An interface that defines a standard set of functionality for a builder pattern object.
/// </summary>
/// <typeparam name="TB">The type of builder. Used to return a fluent interface for the builder.</typeparam>
/// <typeparam name="TBo">The type of object produced by the builder.</typeparam>
/// <remarks>
/// <para>
/// This interface is used to define a pattern for a fluent builder used to create objects.  
/// </para>
/// </remarks>
[Obsolete("This is for the old version of Gorgon. Do not use it.")]
public interface IGorgonFluentBuilderDoNotUse<out TB, TBo>
    where TB : class
    where TBo : class
{
    /// <summary>
    /// Function to return the object that is being built.
    /// </summary>
    /// <returns>The object being built by this fluent builder.</returns>
    /// <remarks>
    /// <para>
    /// Use this method to create a brand new instance of the object being built up by this builder.
    /// </para>
    /// </remarks>
    TBo Build();

    /// <summary>
    /// Function to reset the builder to the specified state provided by the object passed in.
    /// </summary>
    /// <param name="builderObject">The specified object containing the state to copy.</param>
    /// <returns>The fluent builder interface.</returns>
    /// <remarks>
    /// <para>
    /// Implementations can use this to make a copy of the settings for a previous object in the builder instead of manually setting all the state.
    /// </para>
    /// <para>
    /// Unless stated otherwise by an implementation, if the <paramref name="builderObject"/> is <b>null</b>, then this will be the same as calling the <see cref="Clear"/> method.
    /// </para>
    /// </remarks>
    /// <seealso cref="Clear"/>
    TB ResetTo(TBo? builderObject);

    /// <summary>
    /// Function to clear the builder to a default state.
    /// </summary>
    /// <returns>The fluent builder interface.</returns>
    TB Clear();
}

/// <summary>
/// An interface that defines a standard set of functionality for a builder pattern object along with a custom allocator for creating objects.
/// </summary>
/// <typeparam name="TB">The type of builder. Used to return a fluent interface for the builder.</typeparam>
/// <typeparam name="TBo">The type of object produced by the builder.</typeparam>
/// <typeparam name="TBa">The type of optional allocator to use for building objects. Must derive from <see cref="IGorgonAllocator{TBo}"/>.</typeparam>
/// <remarks>
/// <para>
/// This interface is used to define a fluent builder pattern for creating objects. Please note that this interface is separate from <see cref="IGorgonFluentBuilderDoNotUse{TB, TBo}"/> and cannot be used 
/// interchangeably.
/// </para>
/// <para>
/// Unlike the <see cref="IGorgonFluentBuilderDoNotUse{TB, TBo}"/> interface, this one defines an allocator type <typeparamref name="TBa"/>. This allows the builder to use a custom allocator to create objects. 
/// </para>
/// </remarks>
/// <seealso cref="IGorgonAllocator{TBo}"/>
[Obsolete("This is for the old version of Gorgon. Do not use it.")]
public interface IGorgonFluentBuilderWithBuildDoNotUse<out TB, TBo, in TBa>
    where TB : class
    where TBo : class
    where TBa : class, IGorgonAllocator<TBo>
{
    /// <summary>
    /// Function to return the object.
    /// </summary>
    /// <param name="allocator">[Optional] The allocator used to create an instance of the object</param>
    /// <returns>The object created or updated by this builder.</returns>
    /// <remarks>
    /// <para>
    /// Using an <paramref name="allocator"/> can provide different strategies when building objects.  If omitted, the object will be created using the standard <see langword="new"/> keyword.
    /// </para>
    /// <para>
    /// A custom allocator can be beneficial because it allows us to use a pool for allocating the objects, and thus allows for recycling of objects. This keeps the garbage collector happy by keeping objects
    /// around for as long as we need them, instead of creating objects that can potentially end up in the large object heap or in Gen 2.
    /// </para>
    /// </remarks>
    TBo Build(TBa? allocator = null);

    /// <summary>
    /// Function to reset the builder to the specified state provided by the object passed in.
    /// </summary>
    /// <param name="builderObject">The specified object containing the state to copy.</param>
    /// <returns>The fluent builder interface.</returns>
    /// <remarks>
    /// <para>
    /// Implementations can use this to make a copy of the settings for a previous object in the builder instead of manually setting all the state.
    /// </para>
    /// </remarks>
    TB ResetTo(TBo builderObject);

    /// <summary>
    /// Function to clear the builder to a default state.
    /// </summary>
    /// <returns>The fluent builder interface.</returns>
    TB Clear();
}

/// <summary>
/// An interface that defines a standard set of functionality for a builder pattern object along with a custom allocator for creating objects.
/// </summary>
/// <typeparam name="TB">The type of builder. Used to return a fluent interface for the builder.</typeparam>
/// <typeparam name="TBo">The type of object produced by the builder.</typeparam>
/// <typeparam name="TBa">The type of optional allocator to use for building objects. Must derive from <see cref="IGorgonAllocator{TBo}"/>.</typeparam>
/// <seealso cref="IGorgonAllocator{TBo}"/>
public interface IGorgonFluentBuilder<out TB, TBo, in TBa>
    where TB : class
    where TBo : class
    where TBa : class, IGorgonAllocator<TBo>
{
    /// <summary>
    /// Function to return the object.
    /// </summary>
    /// <param name="allocator">[Optional] The allocator used to create an instance of the object.</param>
    /// <returns>The object created or updated by this builder.</returns>
    /// <remarks>
    /// <para>
    /// Using an <paramref name="allocator"/> can provide different strategies when building objects.  If omitted, the object will be created using the standard <see langword="new"/> keyword.
    /// </para>
    /// <para>
    /// A custom allocator can be beneficial because it allows us to use a pool for allocating the objects, and thus allows for recycling of objects. This keeps the garbage collector happy by keeping objects
    /// around for as long as we need them, instead of creating objects that can potentially end up in the large object heap or in Gen 2.
    /// </para>
    /// </remarks>
    TBo Build(TBa? allocator = null);        

    /// <summary>
    /// Function to reset the builder to the specified state provided by the object passed in.
    /// </summary>
    /// <param name="builderObject">The specified object containing the state to copy.</param>
    /// <returns>The fluent builder interface.</returns>
    /// <remarks>
    /// <para>
    /// Implementations can use this to make a copy of the settings for a previous object in the builder instead of manually setting all the state.
    /// </para>
    /// </remarks>
    TB ResetTo(TBo builderObject);

    /// <summary>
    /// Function to clear the builder to a default state.
    /// </summary>
    /// <returns>The fluent builder interface.</returns>
    TB Clear();
}

/// <inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa}"/>
/// <typeparam name="TB"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa}" path="/typeparam[@name='TB']"/></typeparam>
/// <typeparam name="TBo"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa}" path="/typeparam[@name='TBo']"/></typeparam>
/// <typeparam name="TBa"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa}" path="/typeparam[@name='TBa']"/></typeparam>
/// <typeparam name="TP1">The first custom parameter for the build method.</typeparam>
public interface IGorgonFluentBuilder<out TB, TBo, in TBa, in TP1>
    where TB : class
    where TBo : class
    where TBa : class, IGorgonAllocator<TBo>
{
    /// <inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa}.Build" path="/summary"/>
    /// <param name="p1">The first parameter used to build the object.</param>
    /// <param name="allocator"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa}.Build" path="/param[@name='allocator']"/></param>
    /// <inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa}.Build" path="/returns"/>
    /// <inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa}.Build" path="/remarks"/>
    TBo Build(TP1 p1, TBa? allocator = null);

    /// <summary>
    /// Function to reset the builder to the specified state provided by the object passed in.
    /// </summary>
    /// <param name="builderObject">The specified object containing the state to copy.</param>
    /// <returns>The fluent builder interface.</returns>
    /// <remarks>
    /// <para>
    /// Implementations can use this to make a copy of the settings for a previous object in the builder instead of manually setting all the state.
    /// </para>
    /// </remarks>
    TB ResetTo(TBo builderObject);

    /// <summary>
    /// Function to clear the builder to a default state.
    /// </summary>
    /// <returns>The fluent builder interface.</returns>
    TB Clear();
}

/// <inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa}"/>
/// <typeparam name="TB"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa}" path="/typeparam[@name='TB']"/></typeparam>
/// <typeparam name="TBo"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa}" path="/typeparam[@name='TBo']"/></typeparam>
/// <typeparam name="TBa"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa}" path="/typeparam[@name='TBa']"/></typeparam>
/// <typeparam name="TP1"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa, TP1}" path="/typeparam[@name='TP1']"/></typeparam>
/// <typeparam name="TP2">The second custom parameter for the build method.</typeparam>
public interface IGorgonFluentBuilder<out TB, TBo, in TBa, in TP1, in TP2>
    where TB : class
    where TBo : class
    where TBa : class, IGorgonAllocator<TBo>
{
    /// <inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa}.Build" path="/summary"/>
    /// <param name="p1"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa, TP1}.Build" path="/param[@name='p1']"/></param>
    /// <param name="p2">The second parameter used to build the object.</param>
    /// <param name="allocator"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa}.Build" path="/param[@name='allocator']"/></param>
    /// <inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa}.Build" path="/returns"/>
    /// <inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa}.Build" path="/remarks"/>
    TBo Build(TP1 p1, TP2 p2, TBa? allocator = null);

    /// <summary>
    /// Function to reset the builder to the specified state provided by the object passed in.
    /// </summary>
    /// <param name="builderObject">The specified object containing the state to copy.</param>
    /// <returns>The fluent builder interface.</returns>
    /// <remarks>
    /// <para>
    /// Implementations can use this to make a copy of the settings for a previous object in the builder instead of manually setting all the state.
    /// </para>
    /// </remarks>
    TB ResetTo(TBo builderObject);

    /// <summary>
    /// Function to clear the builder to a default state.
    /// </summary>
    /// <returns>The fluent builder interface.</returns>
    TB Clear();
}

/// <inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa}"/>
/// <typeparam name="TB"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa}" path="/typeparam[@name='TB']"/></typeparam>
/// <typeparam name="TBo"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa}" path="/typeparam[@name='TBo']"/></typeparam>
/// <typeparam name="TBa"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa}" path="/typeparam[@name='TBa']"/></typeparam>
/// <typeparam name="TP1"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa, TP1}" path="/typeparam[@name='TP1']"/></typeparam>
/// <typeparam name="TP2"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa, TP1, TP2}" path="/typeparam[@name='TP2']"/></typeparam>
/// <typeparam name="TP3">The third custom parameter for the build method.</typeparam>
public interface IGorgonFluentBuilder<out TB, TBo, in TBa, in TP1, in TP2, in TP3>
    where TB : class
    where TBo : class
    where TBa : class, IGorgonAllocator<TBo>
{
    /// <inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa}.Build" path="/summary"/>
    /// <param name="p1"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa, TP1}.Build" path="/param[@name='p1']"/></param>
    /// <param name="p2"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa, TP1, TP2}.Build" path="/param[@name='p2']"/></param>
    /// <param name="p3">The third parameter used to build the object.</param>
    /// <param name="allocator"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa}.Build" path="/param[@name='allocator']"/></param>
    /// <inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa}.Build" path="/returns"/>
    /// <inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa}.Build" path="/remarks"/>
    TBo Build(TP1 p1, TP2 p2, TP3 p3, TBa? allocator = null);

    /// <summary>
    /// Function to reset the builder to the specified state provided by the object passed in.
    /// </summary>
    /// <param name="builderObject">The specified object containing the state to copy.</param>
    /// <returns>The fluent builder interface.</returns>
    /// <remarks>
    /// <para>
    /// Implementations can use this to make a copy of the settings for a previous object in the builder instead of manually setting all the state.
    /// </para>
    /// </remarks>
    TB ResetTo(TBo builderObject);

    /// <summary>
    /// Function to clear the builder to a default state.
    /// </summary>
    /// <returns>The fluent builder interface.</returns>
    TB Clear();
}

/// <inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa}"/>
/// <typeparam name="TB"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa}" path="/typeparam[@name='TB']"/></typeparam>
/// <typeparam name="TBo"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa}" path="/typeparam[@name='TBo']"/></typeparam>
/// <typeparam name="TBa"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa}" path="/typeparam[@name='TBa']"/></typeparam>
/// <typeparam name="TP1"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa, TP1}" path="/typeparam[@name='TP1']"/></typeparam>
/// <typeparam name="TP2"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa, TP1, TP2}" path="/typeparam[@name='TP2']"/></typeparam>
/// <typeparam name="TP3"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa, TP1, TP2, TP3}" path="/typeparam[@name='TP3']"/></typeparam>
/// <typeparam name="TP4">The fourth custom parameter for the build method.</typeparam>
public interface IGorgonFluentBuilder<out TB, TBo, in TBa, in TP1, in TP2, in TP3, in TP4>
    where TB : class
    where TBo : class
    where TBa : class, IGorgonAllocator<TBo>
{
    /// <inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa}.Build" path="/summary"/>
    /// <param name="p1"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa, TP1}.Build" path="/param[@name='p1']"/></param>
    /// <param name="p2"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa, TP1, TP2}.Build" path="/param[@name='p2']"/></param>
    /// <param name="p3"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa, TP1, TP2, TP3}.Build" path="/param[@name='p3']"/></param>
    /// <param name="p4">The third parameter used to build the object.</param>
    /// <param name="allocator"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa}.Build" path="/param[@name='allocator']"/></param>
    /// <inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa}.Build" path="/returns"/>
    /// <inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa}.Build" path="/remarks"/>
    TBo Build(TP1 p1, TP2 p2, TP3 p3, TP4 p4, TBa? allocator = null);

    /// <summary>
    /// Function to reset the builder to the specified state provided by the object passed in.
    /// </summary>
    /// <param name="builderObject">The specified object containing the state to copy.</param>
    /// <returns>The fluent builder interface.</returns>
    /// <remarks>
    /// <para>
    /// Implementations can use this to make a copy of the settings for a previous object in the builder instead of manually setting all the state.
    /// </para>
    /// </remarks>
    TB ResetTo(TBo builderObject);

    /// <summary>
    /// Function to clear the builder to a default state.
    /// </summary>
    /// <returns>The fluent builder interface.</returns>
    TB Clear();
}

/// <inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa}"/>
/// <typeparam name="TB"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa}" path="/typeparam[@name='TB']"/></typeparam>
/// <typeparam name="TBo"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa}" path="/typeparam[@name='TBo']"/></typeparam>
/// <typeparam name="TBa"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa}" path="/typeparam[@name='TBa']"/></typeparam>
/// <typeparam name="TP1"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa, TP1}" path="/typeparam[@name='TP1']"/></typeparam>
/// <typeparam name="TP2"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa, TP1, TP2}" path="/typeparam[@name='TP2']"/></typeparam>
/// <typeparam name="TP3"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa, TP1, TP2, TP3}" path="/typeparam[@name='TP3']"/></typeparam>
/// <typeparam name="TP4"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa, TP1, TP2, TP3, TP4}" path="/typeparam[@name='TP4']"/></typeparam>
/// <typeparam name="TP5">The fifth custom parameter for the build method.</typeparam>
public interface IGorgonFluentBuilder<out TB, TBo, in TBa, in TP1, in TP2, in TP3, in TP4, in TP5>
    where TB : class
    where TBo : class
    where TBa : class, IGorgonAllocator<TBo>
{
    /// <inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa}.Build" path="/summary"/>
    /// <param name="p1"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa, TP1}.Build" path="/param[@name='p1']"/></param>
    /// <param name="p2"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa, TP1, TP2}.Build" path="/param[@name='p2']"/></param>
    /// <param name="p3"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa, TP1, TP2, TP3}.Build" path="/param[@name='p3']"/></param>
    /// <param name="p4"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa, TP1, TP2, TP3, TP4}.Build" path="/param[@name='p4']"/></param>
    /// <param name="p5">The fifth parameter used to build the object.</param>
    /// <param name="allocator"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa}.Build" path="/param[@name='allocator']"/></param>
    /// <inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa}.Build" path="/returns"/>
    /// <inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa}.Build" path="/remarks"/>
    TBo Build(TP1 p1, TP2 p2, TP3 p3, TP4 p4, TP5 p5, TBa? allocator = null);

    /// <summary>
    /// Function to reset the builder to the specified state provided by the object passed in.
    /// </summary>
    /// <param name="builderObject">The specified object containing the state to copy.</param>
    /// <returns>The fluent builder interface.</returns>
    /// <remarks>
    /// <para>
    /// Implementations can use this to make a copy of the settings for a previous object in the builder instead of manually setting all the state.
    /// </para>
    /// </remarks>
    TB ResetTo(TBo builderObject);

    /// <summary>
    /// Function to clear the builder to a default state.
    /// </summary>
    /// <returns>The fluent builder interface.</returns>
    TB Clear();
}

/// <inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa}"/>
/// <typeparam name="TB"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa}" path="/typeparam[@name='TB']"/></typeparam>
/// <typeparam name="TBo"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa}" path="/typeparam[@name='TBo']"/></typeparam>
/// <typeparam name="TBa"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa}" path="/typeparam[@name='TBa']"/></typeparam>
/// <typeparam name="TP1"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa, TP1}" path="/typeparam[@name='TP1']"/></typeparam>
/// <typeparam name="TP2"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa, TP1, TP2}" path="/typeparam[@name='TP2']"/></typeparam>
/// <typeparam name="TP3"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa, TP1, TP2, TP3}" path="/typeparam[@name='TP3']"/></typeparam>
/// <typeparam name="TP4"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa, TP1, TP2, TP3, TP4}" path="/typeparam[@name='TP4']"/></typeparam>
/// <typeparam name="TP5"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa, TP1, TP2, TP3, TP4, TP5}" path="/typeparam[@name='TP5']"/></typeparam>
/// <typeparam name="TP6">The sixth custom parameter for the build method.</typeparam>
public interface IGorgonFluentBuilder<out TB, TBo, in TBa, in TP1, in TP2, in TP3, in TP4, in TP5, in TP6>
    where TB : class
    where TBo : class
    where TBa : class, IGorgonAllocator<TBo>
{
    /// <inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa}.Build" path="/summary"/>
    /// <param name="p1"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa, TP1}.Build" path="/param[@name='p1']"/></param>
    /// <param name="p2"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa, TP1, TP2}.Build" path="/param[@name='p2']"/></param>
    /// <param name="p3"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa, TP1, TP2, TP3}.Build" path="/param[@name='p3']"/></param>
    /// <param name="p4"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa, TP1, TP2, TP3, TP4}.Build" path="/param[@name='p4']"/></param>
    /// <param name="p5"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa, TP1, TP2, TP3, TP4, TP5}.Build" path="/param[@name='p5']"/></param>
    /// <param name="p6">The sixth parameter used to build the object.</param>
    /// <param name="allocator"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa}.Build" path="/param[@name='allocator']"/></param>
    /// <inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa}.Build" path="/returns"/>
    /// <inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa}.Build" path="/remarks"/>
    TBo Build(TP1 p1, TP2 p2, TP3 p3, TP4 p4, TP5 p5, TP6 p6, TBa? allocator = null);

    /// <summary>
    /// Function to reset the builder to the specified state provided by the object passed in.
    /// </summary>
    /// <param name="builderObject">The specified object containing the state to copy.</param>
    /// <returns>The fluent builder interface.</returns>
    /// <remarks>
    /// <para>
    /// Implementations can use this to make a copy of the settings for a previous object in the builder instead of manually setting all the state.
    /// </para>
    /// </remarks>
    TB ResetTo(TBo builderObject);

    /// <summary>
    /// Function to clear the builder to a default state.
    /// </summary>
    /// <returns>The fluent builder interface.</returns>
    TB Clear();
}

/// <inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa}"/>
/// <typeparam name="TB"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa}" path="/typeparam[@name='TB']"/></typeparam>
/// <typeparam name="TBo"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa}" path="/typeparam[@name='TBo']"/></typeparam>
/// <typeparam name="TBa"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa}" path="/typeparam[@name='TBa']"/></typeparam>
/// <typeparam name="TP1"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa, TP1}" path="/typeparam[@name='TP1']"/></typeparam>
/// <typeparam name="TP2"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa, TP1, TP2}" path="/typeparam[@name='TP2']"/></typeparam>
/// <typeparam name="TP3"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa, TP1, TP2, TP3}" path="/typeparam[@name='TP3']"/></typeparam>
/// <typeparam name="TP4"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa, TP1, TP2, TP3, TP4}" path="/typeparam[@name='TP4']"/></typeparam>
/// <typeparam name="TP5"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa, TP1, TP2, TP3, TP4, TP5}" path="/typeparam[@name='TP5']"/></typeparam>
/// <typeparam name="TP6"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa, TP1, TP2, TP3, TP4, TP5, TP6}" path="/typeparam[@name='TP6']"/></typeparam>
/// <typeparam name="TP7">The seventh custom parameter for the build method.</typeparam>
public interface IGorgonFluentBuilder<out TB, TBo, in TBa, in TP1, in TP2, in TP3, in TP4, in TP5, in TP6, in TP7>
    where TB : class
    where TBo : class
    where TBa : class, IGorgonAllocator<TBo>
{
    /// <inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa}.Build" path="/summary"/>
    /// <param name="p1"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa, TP1}.Build" path="/param[@name='p1']"/></param>
    /// <param name="p2"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa, TP1, TP2}.Build" path="/param[@name='p2']"/></param>
    /// <param name="p3"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa, TP1, TP2, TP3}.Build" path="/param[@name='p3']"/></param>
    /// <param name="p4"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa, TP1, TP2, TP3, TP4}.Build" path="/param[@name='p4']"/></param>
    /// <param name="p5"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa, TP1, TP2, TP3, TP4, TP5}.Build" path="/param[@name='p5']"/></param>
    /// <param name="p6"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa, TP1, TP2, TP3, TP4, TP5, TP6}.Build" path="/param[@name='p6']"/></param>
    /// <param name="p7">The seventh parameter used to build the object.</param>
    /// <param name="allocator"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa}.Build" path="/param[@name='allocator']"/></param>
    /// <inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa}.Build" path="/returns"/>
    /// <inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa}.Build" path="/remarks"/>
    TBo Build(TP1 p1, TP2 p2, TP3 p3, TP4 p4, TP5 p5, TP6 p6, TP7 p7, TBa? allocator = null);

    /// <summary>
    /// Function to reset the builder to the specified state provided by the object passed in.
    /// </summary>
    /// <param name="builderObject">The specified object containing the state to copy.</param>
    /// <returns>The fluent builder interface.</returns>
    /// <remarks>
    /// <para>
    /// Implementations can use this to make a copy of the settings for a previous object in the builder instead of manually setting all the state.
    /// </para>
    /// </remarks>
    TB ResetTo(TBo builderObject);

    /// <summary>
    /// Function to clear the builder to a default state.
    /// </summary>
    /// <returns>The fluent builder interface.</returns>
    TB Clear();
}

/// <inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa}"/>
/// <typeparam name="TB"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa}" path="/typeparam[@name='TB']"/></typeparam>
/// <typeparam name="TBo"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa}" path="/typeparam[@name='TBo']"/></typeparam>
/// <typeparam name="TBa"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa}" path="/typeparam[@name='TBa']"/></typeparam>
/// <typeparam name="TP1"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa, TP1}" path="/typeparam[@name='TP1']"/></typeparam>
/// <typeparam name="TP2"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa, TP1, TP2}" path="/typeparam[@name='TP2']"/></typeparam>
/// <typeparam name="TP3"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa, TP1, TP2, TP3}" path="/typeparam[@name='TP3']"/></typeparam>
/// <typeparam name="TP4"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa, TP1, TP2, TP3, TP4}" path="/typeparam[@name='TP4']"/></typeparam>
/// <typeparam name="TP5"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa, TP1, TP2, TP3, TP4, TP5}" path="/typeparam[@name='TP5']"/></typeparam>
/// <typeparam name="TP6"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa, TP1, TP2, TP3, TP4, TP5, TP6}" path="/typeparam[@name='TP6']"/></typeparam>
/// <typeparam name="TP7"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa, TP1, TP2, TP3, TP4, TP5, TP6, TP7}" path="/typeparam[@name='TP6']"/></typeparam>
/// <typeparam name="TP8">The eighth custom parameter for the build method.</typeparam>
public interface IGorgonFluentBuilder<out TB, TBo, in TBa, in TP1, in TP2, in TP3, in TP4, in TP5, in TP6, in TP7, in TP8>
    where TB : class
    where TBo : class
    where TBa : class, IGorgonAllocator<TBo>
{
    /// <inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa}.Build" path="/summary"/>
    /// <param name="p1"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa, TP1}.Build" path="/param[@name='p1']"/></param>
    /// <param name="p2"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa, TP1, TP2}.Build" path="/param[@name='p2']"/></param>
    /// <param name="p3"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa, TP1, TP2, TP3}.Build" path="/param[@name='p3']"/></param>
    /// <param name="p4"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa, TP1, TP2, TP3, TP4}.Build" path="/param[@name='p4']"/></param>
    /// <param name="p5"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa, TP1, TP2, TP3, TP4, TP5}.Build" path="/param[@name='p5']"/></param>
    /// <param name="p6"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa, TP1, TP2, TP3, TP4, TP5, TP6}.Build" path="/param[@name='p6']"/></param>
    /// <param name="p7"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa, TP1, TP2, TP3, TP4, TP5, TP6, TP7}.Build" path="/param[@name='p7']"/></param>
    /// <param name="p8">The eighth parameter used to build the object.</param>
    /// <param name="allocator"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa}.Build" path="/param[@name='allocator']"/></param>
    /// <inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa}.Build" path="/returns"/>
    /// <inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa}.Build" path="/remarks"/>
    TBo Build(TP1 p1, TP2 p2, TP3 p3, TP4 p4, TP5 p5, TP6 p6, TP7 p7, TP8 p8, TBa? allocator = null);

    /// <summary>
    /// Function to reset the builder to the specified state provided by the object passed in.
    /// </summary>
    /// <param name="builderObject">The specified object containing the state to copy.</param>
    /// <returns>The fluent builder interface.</returns>
    /// <remarks>
    /// <para>
    /// Implementations can use this to make a copy of the settings for a previous object in the builder instead of manually setting all the state.
    /// </para>
    /// </remarks>
    TB ResetTo(TBo builderObject);

    /// <summary>
    /// Function to clear the builder to a default state.
    /// </summary>
    /// <returns>The fluent builder interface.</returns>
    TB Clear();
}
