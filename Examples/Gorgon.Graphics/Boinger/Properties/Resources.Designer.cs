﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated
// </auto-generated>
//------------------------------------------------------------------------------

namespace Gorgon.Examples.Properties; 
using System;


/// <summary>
///   A strongly-typed resource class, for looking up localized strings, etc
/// </summary>
// This class was auto-generated by the StronglyTypedResourceBuilder
// class via a tool like ResGen or Visual Studio
// To add or remove a member, edit your .ResX file then rerun ResGen
// with the /str option, or rebuild your VS project.
[global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
[global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
internal class Resources {

    private static global::System.Resources.ResourceManager resourceMan;

    private static global::System.Globalization.CultureInfo resourceCulture;

    [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
    internal Resources() {
    }

    /// <summary>
    ///   Returns the cached ResourceManager instance used by this class.
    /// </summary>
    [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
    internal static global::System.Resources.ResourceManager ResourceManager {
        get {
            if (object.ReferenceEquals(resourceMan, null)) {
                global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Gorgon.Examples.Properties.Resources", typeof(Resources).Assembly);
                resourceMan = temp;
            }
            return resourceMan;
        }
    }

    /// <summary>
    ///   Overrides the current thread's CurrentUICulture property for all
    ///   resource lookups using this strongly typed resource class.
    /// </summary>
    [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
    internal static global::System.Globalization.CultureInfo Culture {
        get {
            return resourceCulture;
        }
        set {
            resourceCulture = value;
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to // Our default texture and sampler.
    ///Texture2D _texture : register(t0);
    ///SamplerState _sampler : register(s0);
    ///
    ///// The transformation matrices.
    ///cbuffer WorldViewProjection : register(b0)
    ///{
    ///	float4x4 WVP;
    ///}
    ///
    ///// The diffuse color.
    ///cbuffer Material : register(b0)
    ///{
    ///	float4 Diffuse;
    ///}
    ///
    ///// Our vertex.
    ///struct BoingerVertex
    ///{
    ///   float4 position : SV_POSITION;
    ///   float2 uv : TEXCOORD;
    ///};
    ///
    ///
    ///// Our default vertex shader.
    ///BoingerVertex BoingerVS(BoingerVertex vertex)
    ///{
    ///	BoingerVertex output =  [rest of string was truncated]&quot;;.
    /// </summary>
    internal static string Shader {
        get {
            return ResourceManager.GetString("Shader", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized resource of type System.Drawing.Bitmap.
    /// </summary>
    internal static System.Drawing.Bitmap Texture {
        get {
            object obj = ResourceManager.GetObject("Texture", resourceCulture);
            return ((System.Drawing.Bitmap)(obj));
        }
    }
}
