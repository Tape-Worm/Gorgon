
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: Tuesday, September 8, 2015 11:48:36 PM
// 


namespace Gorgon.Input;

/// <summary>
/// Receives Gorgon raw input data and allows processing of that data on the device object
/// </summary>
/// <typeparam name="T">The type of data to process.</typeparam>
/// <remarks>
/// This interface is not meant to be used within an application and should only be used internally by Gorgon
/// </remarks>
public interface IGorgonRawInputDeviceData<T>
    where T : struct
{
    /// <summary>
    /// Function to process the Gorgon raw input data into device state data and appropriate events.
    /// </summary>
    /// <param name="rawInputData">The data to process.</param>
    void ProcessData(in T rawInputData);
}
