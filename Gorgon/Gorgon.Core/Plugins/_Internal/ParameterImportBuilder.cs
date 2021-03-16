#if NETSTANDARD2_0
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.ComponentModel.Composition.Registration
{
    // This class exists to enable configuration of PartBuilder<T>
    internal class ParameterImportBuilder
    {
        public T Import<T>() => default;


        [Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        public T Import<T>(Action<ImportBuilder> configure) => default;
    }
}
#endif