#if NETSTANDARD2_0

// 
// Gorgon
// Copyright (C) 2020 Michael Winsor
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
// Created: November 24, 2020 11:36:30 PM
// 



namespace System.Composition.Diagnostics
{
    /// <summary>
    /// Strings for the builder.
    /// </summary>
    internal class SR
    {
        /// <summary>
        /// Internal error.
        /// </summary>
        public const string Diagnostic_InternalExceptionMessage = "Internal error {0}";

        /// <summary>
        /// Constructor convention overridden.
        /// </summary>
        public const string Registration_ConstructorConventionOverridden = "Constructor convention overridden";

        /// <summary>
        /// Type export convention overridden.
        /// </summary>
        public const string Registration_TypeExportConventionOverridden = "Type export convention overridden";

        /// <summary>
        /// Member convention overidden.
        /// </summary>
        public const string Registration_MemberExportConventionOverridden = "Member export convention overidden";

        /// <summary>
        /// Member convention overridden.
        /// </summary>
        public const string Registration_MemberImportConventionOverridden = "Member import convetion overridden";

        /// <summary>
        /// Import statisfied overridden.
        /// </summary>
        public const string Registration_OnSatisfiedImportNotificationOverridden = "Import satisfied overidden";

        /// <summary>
        /// Part creation overidden.
        /// </summary>
        public const string Registration_PartCreationConventionOverridden = "Part creation overidden";

        /// <summary>
        /// Member import import convention matched twice.
        /// </summary>
        public const string Registration_MemberImportConventionMatchedTwice = "Member import convention matched twice";

        /// <summary>
        /// Part metadata convention overidden.
        /// </summary>
        public const string Registration_PartMetadataConventionOverridden = "Part metadata convention overidden";

        /// <summary>
        /// Parameter import convention overidden.
        /// </summary>
        public const string Registration_ParameterImportConventionOverridden = "Parameter import convention overidden";

        /// <summary>
        /// Unnecessary work.
        /// </summary>
        public const string Diagnostic_TraceUnnecessaryWork = "Unnecessary work";

        public const string Argument_ExpressionMustBePropertyMember = "Argument {0} must be a property.";
    }
}
#endif
