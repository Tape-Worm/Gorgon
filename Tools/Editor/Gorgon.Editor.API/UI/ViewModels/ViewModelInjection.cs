
// 
// Gorgon
// Copyright (C) 2019 Michael Winsor
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: April 20, 2019 9:22:57 PM
// 

using Gorgon.Editor.PlugIns;

namespace Gorgon.Editor.UI.ViewModels;

/// <summary>
/// This object is used to inject common host application services into view models as parameters
/// </summary>
/// <typeparam name="T">The type of host services. Must implement <see cref="IHostServices"/>.</typeparam>
/// <remarks>
/// <para>
/// When creating view models, developers should pass custom data used for initialization by inheriting this type. For content, settings, etc... or other built in view model types, there are other 
/// base classes for the parameters that should be used
/// </para>
/// </remarks>
/// <seealso cref="IHostServices"/>
public class ViewModelInjection<T>
    : IViewModelInjection<T>
    where T : IHostServices
{
    /// <summary>Property to return the common services passed from host application.</summary>
    public T HostServices
    {
        get;
    }

    /// <summary>Initializes a new instance of the <see cref="ViewModelInjection{T}"/> class.</summary>
    /// <param name="copy">The injection parameter object to copy.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="copy"/> parameter is <b>null</b>.</exception>
    protected ViewModelInjection(IViewModelInjection<T> copy)
    {
        if (copy is null)
        {
            throw new ArgumentNullException(nameof(copy));
        }

        HostServices = copy.HostServices;
    }

    /// <summary>Initializes a new instance of the <see cref="ViewModelInjection{T}"/> class.</summary>
    /// <param name="hostServices">The services from the host application.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="hostServices"/> parameter is <b>null</b>.</exception>
    public ViewModelInjection(T hostServices)
    {
        if (hostServices == null)
        {
            throw new ArgumentNullException(nameof(hostServices));
        }

        HostServices = hostServices;
    }
}
