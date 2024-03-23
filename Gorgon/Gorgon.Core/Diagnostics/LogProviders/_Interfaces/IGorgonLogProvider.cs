
// 
// Gorgon
// Copyright (C) 2018 Michael Winsor
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
// Created: March 20, 2018 11:05:20 PM
// 

namespace Gorgon.Diagnostics.LogProviders;

/// <summary>
/// A provider that accesses a data store to store logging messages
/// </summary>
public interface IGorgonLogProvider
{
    /// <summary>
    /// Function to open the data store for writing.
    /// </summary>
    /// <param name="initialMessage">[Optional] The initial message to write.</param>
    void Open(string initialMessage = null);

    /// <summary>
    /// Function to write a message to the data store.
    /// </summary>
    /// <param name="message">The message to write.</param>
    void SendMessage(string message);

    /// <summary>
    /// Function to close the data store for writing.
    /// </summary>
    /// <param name="closingMessage">[Optional] The message to write when closing.</param>
    void Close(string closingMessage = null);
}
