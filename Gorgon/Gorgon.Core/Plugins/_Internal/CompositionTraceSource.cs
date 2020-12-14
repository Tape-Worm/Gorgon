#if NETSTANDARD2_0
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Composition.Diagnostics
{
    internal static class CompositionTraceSource
    {
        private static readonly DebuggerTraceWriter _source = new DebuggerTraceWriter();

        public static bool CanWriteInformation => _source.CanWriteInformation;

        public static bool CanWriteWarning => _source.CanWriteWarning;

        public static bool CanWriteError => _source.CanWriteError;

        public static void WriteInformation(CompositionTraceId traceId, string format, params object[] arguments)
        {
            EnsureEnabled(CanWriteInformation);

            _source.WriteInformation(traceId, format, arguments);
        }

        public static void WriteWarning(CompositionTraceId traceId, string format, params object[] arguments)
        {
            EnsureEnabled(CanWriteWarning);

            _source.WriteWarning(traceId, format, arguments);
        }

        public static void WriteError(CompositionTraceId traceId, string format, params object[] arguments)
        {
            EnsureEnabled(CanWriteError);

            _source.WriteError(traceId, format, arguments);
        }

        private static void EnsureEnabled(bool condition)
        {
            if (!condition)
            {
                throw new Exception(string.Format(SR.Diagnostic_InternalExceptionMessage, SR.Diagnostic_TraceUnnecessaryWork));
            }
        }
    }
}
#endif