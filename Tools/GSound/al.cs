#region MIT.
// 
// GSound (Gorgon Sound)
// Copyright (C) 2012 Devin Argent
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
#endregion

/*
 * C# OpenGL Header Version 0.12 by Frank Herrlich (Magellan86@web.de)
*/
#define USE_ALUT

using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace GorgonLibrary.Sound
{
    /// <summary>
    /// This code was created by Magellan http://trinidad.delphigl.com/. Comments were translated/created by ShadowDust702
    /// </summary>
    internal static class al : object
    {
        #region Constant values

        public const string DLLName = "OpenAL32.dll";

        public const int INVALID = -1;
        public const int NONE = 0;
        public const int FALSE = 0;
        public const int TRUE = 1;
        public const int NO_ERROR = FALSE;

        public const int SOURCE_TYPE = 0x0200;
        public const int SOURCE_ABSOLUTE = 0x0201;
        public const int SOURCE_RELATIVE = 0x0202;

        public const int CONE_INNER_ANGLE = 0x1001;
        public const int CONE_OUTER_ANGLE = 0x1002;
        public const int PITCH = 0x1003;
        public const int POSITION = 0x1004;
        public const int DIRECTION = 0x1005;
        public const int VELOCITY = 0x1006;
        public const int LOOPING = 0x1007;
        public const int BUFFER = 0x1009;
        public const int GAIN = 0x100A;
        public const int MIN_GAIN = 0x100D;
        public const int MAX_GAIN = 0x100E;
        public const int ORIENTATION = 0x100F;

        public const int SOURCE_STATE = 0x1010;
        public const int INITIAL = 0x1011;
        public const int PLAYING = 0x1012;
        public const int PAUSED = 0x1013;
        public const int STOPPED = 0x1014;

        public const int BUFFERS_QUEUED = 0x1015;
        public const int BUFFERS_PROCESSED = 0x1016;

        public const int FORMAT_MONO8 = 0x1100;
        public const int FORMAT_MONO16 = 0x1101;
        public const int FORMAT_STEREO8 = 0x1102;
        public const int FORMAT_STEREO16 = 0x1103;

        public const int REFERENCE_DISTANCE = 0x1020;
        public const int ROLLOFF_FACTOR = 0x1021;
        public const int CONE_OUTER_GAIN = 0x1022;
        public const int MAX_DISTANCE = 0x1023;

        public const int FREQUENCY = 0x2001;
        public const int BITS = 0x2002;
        public const int CHANNELS = 0x2003;
        public const int SIZE = 0x2004;
        public const int DATA = 0x2005;

        public const int UNUSED = 0x2010;
        public const int PENDING = 0x2011;
        public const int PROCESSED = 0x2012;

        public const int CHANNEL_MASK = 0x3000;

        public const int INVALID_NAME = 0xA001;
        public const int ILLEGAL_ENUM = 0xA002;
        public const int INVALID_ENUM = 0xA002;
        public const int INVALID_VALUE = 0xA003;
        public const int ILLEGAL_COMMAND = 0xA004;
        public const int INVALID_OPERATION = 0xA004;
        public const int OUT_OF_MEMORY = 0xA005;

        public const int VENDOR = 0xB001;
        public const int VERSION = 0xB002;
        public const int RENDERER = 0xB003;
        public const int EXTENSIONS = 0xB004;

        public const int DOPPLER_FACTOR = 0xC000;
        public const int DOPPLER_VELOCITY = 0xC001;

        public const int DISTANCE_MODEL = 0xD000;
        public const int INVERSE_DISTANCE = 0xD001;
        public const int INVERSE_DISTANCE_CLAMPED = 0xD002;

        #endregion

        #region DLLImports

        [DllImport(DLLName, EntryPoint = "alEnable", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
        public static extern void Enable(int capability);

        [DllImport(DLLName, EntryPoint = "alDisable", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
        public static extern void Disable(int capability);

        [DllImport(DLLName, EntryPoint = "alIsEnabled", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
        public static extern bool IsEnabled(int capability);

        [DllImport(DLLName, EntryPoint = "alHint", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
        public static extern void Hint(int target, int mode);

        [DllImport(DLLName, EntryPoint = "alGetString", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.LPStr)]
        public static extern string GetString(int param);

        [DllImport(DLLName, EntryPoint = "alGetBoolean", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
        public static extern bool GetBoolean(int param);

        [DllImport(DLLName, EntryPoint = "alGetBooleanv", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
        public static extern void GetBooleanv(int param, bool[] data);

        [DllImport(DLLName, EntryPoint = "alGetInteger", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
        public static extern int GetInteger(int param);

        [DllImport(DLLName, EntryPoint = "alGetIntegerv", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
        public static extern void GetIntegerv(int param, int[] data);

        [DllImport(DLLName, EntryPoint = "alGetFloat", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
        public static extern float GetFloat(int param);

        [DllImport(DLLName, EntryPoint = "alGetFloatv", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
        public static extern void GetFloatv(int param, float[] data);

        [DllImport(DLLName, EntryPoint = "alGetDouble", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
        public static extern double GetDouble(int param);

        [DllImport(DLLName, EntryPoint = "alGetDoublev", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
        public static extern void GetDoublev(int param, double[] data);

        [DllImport(DLLName, EntryPoint = "alGetError", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
        public static extern int GetError();

        [DllImport(DLLName, EntryPoint = "alIsExtensionPresent", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
        public static extern bool IsExtensionPresent([MarshalAs(UnmanagedType.LPStr)] string fname);

        [DllImport(DLLName, EntryPoint = "alGetEnumValue", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
        public static extern int GetEnumValue(byte[] ename);

        [DllImport(DLLName, EntryPoint = "alListeneri", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
        public static extern void Listeneri(int param, int value);

        [DllImport(DLLName, EntryPoint = "alListenerf", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
        public static extern void Listenerf(int param, float value);

        [DllImport(DLLName, EntryPoint = "alListener3f", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
        public static extern void Listener3f(int param, float f1, float f2, float f3);

        [DllImport(DLLName, EntryPoint = "alListenerfv", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
        public static extern void Listenerfv(int param, float[] values);

        [DllImport(DLLName, EntryPoint = "alGetListeneriv", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
        public static extern void GetListeneriv(int param, int[] values);

        [DllImport(DLLName, EntryPoint = "alGetListenerfv", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
        public static extern void GetListenerfv(int param, float[] values);

        [DllImport(DLLName, EntryPoint = "alGenSources", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
        public static extern void GenSources(int n, int[] sources);

        [DllImport(DLLName, EntryPoint = "alDeleteSources", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
        public static extern void DeleteSources(int n, int[] sources);

        [DllImport(DLLName, EntryPoint = "alIsSource", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
        public static extern bool IsSource(int id);

        [DllImport(DLLName, EntryPoint = "alSourcei", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
        public static extern void Sourcei(int source, int param, int value);

        [DllImport(DLLName, EntryPoint = "alSourcef", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
        public static extern void Sourcef(int source, int param, float value);

        [DllImport(DLLName, EntryPoint = "alSource3f", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
        public static extern void Source3f(int source, int param, float v1, float v2, float v3);

        [DllImport(DLLName, EntryPoint = "alSourcefv", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
        public static extern void Sourcefv(int source, int param, float[] values);

        [DllImport(DLLName, EntryPoint = "alGetSourcei", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
        public static extern void GetSourcei(int source, int param, int[] value);

        [DllImport(DLLName, EntryPoint = "alGetSourcef", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
        public static extern void GetSourcef(int source, int param, float[] value);

        [DllImport(DLLName, EntryPoint = "alGetSource3f", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
        public static extern void GetSource3f(int source, int param, float[] v1, float[] v2, float[] v3);

        [DllImport(DLLName, EntryPoint = "alGetSourcefv", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
        public static extern void GetSourcefv(int source, int param, float[] values);

        [DllImport(DLLName, EntryPoint = "alSourcePlay", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
        public static extern void SourcePlay(int source);

        [DllImport(DLLName, EntryPoint = "alSourcePause", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
        public static extern void SourcePause(int source);

        [DllImport(DLLName, EntryPoint = "alSourceStop", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
        public static extern void SourceStop(int source);

        [DllImport(DLLName, EntryPoint = "alSourceRewind", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
        public static extern void SourceRewind(int source);

        [DllImport(DLLName, EntryPoint = "alSourcePlayv", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
        public static extern void SourcePlayv(int n, int[] sources);

        [DllImport(DLLName, EntryPoint = "alSourceStopv", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
        public static extern void SourceStopv(int n, int[] sources);

        [DllImport(DLLName, EntryPoint = "alSourceRewindv", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
        public static extern void SourceRewindv(int n, int[] sources);

        [DllImport(DLLName, EntryPoint = "alSourcePausev", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
        public static extern void SourcePausev(int n, int[] sources);

        [DllImport(DLLName, EntryPoint = "alGenBuffers", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
        public static extern void GenBuffers(int n, int[] buffers);

        [DllImport(DLLName, EntryPoint = "alDeleteBuffers", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
        public static extern void DeleteBuffers(int n, int[] buffers);

        [DllImport(DLLName, EntryPoint = "alIsBuffer", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
        public static extern bool IsBuffer(int buffer);

        [DllImport(DLLName, EntryPoint = "alBufferData", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
        public static extern void BufferData(int buffer, int format, byte[] data, int size, int freq);

        [DllImport(DLLName, EntryPoint = "alGetBufferi", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
        public static extern void GetBufferi(int buffer, int param, int[] value);

        [DllImport(DLLName, EntryPoint = "alGetBufferf", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
        public static extern void GetBufferf(int buffer, int param, float[] value);

        [DllImport(DLLName, EntryPoint = "alSourceQueueBuffers", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
        public static extern void SourceQueueBuffers(int source, int n, int[] buffers);

        [DllImport(DLLName, EntryPoint = "alSourceUnqueueBuffers", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
        public static extern void SourceUnqueueBuffers(int source, int n, int[] buffers);

        [DllImport(DLLName, EntryPoint = "alDistanceModel", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
        public static extern void DistanceModel(int value);

        [DllImport(DLLName, EntryPoint = "alDopplerFactor", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
        public static extern void DopplerFactor(float value);

        [DllImport(DLLName, EntryPoint = "alDopplerVelocity", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
        public static extern void DopplerVelocity(float value);

        #endregion

        #region auxiliary functions

        /// <summary>
        /// Deletes a buffer
        /// </summary>
        public static void DeleteBuffer(int Buffer)
        {
            int[] B = { Buffer };
            al.DeleteBuffers(1, B);
        }

        /// <summary>
        /// Deletes a source
        /// </summary>
        public static void DeleteSource(int Source)
        {
            int[] S = { Source };
            al.DeleteSources(1, S);
        }

        /// <summary>
        /// Generates a buffer
        /// </summary>
        /// <returns>returns the buffer ID needed to do anything with it</returns>
        public static int GenBuffer()
        {
            int[] Buffer = new int[1];
            al.GenBuffers(1, Buffer);
            return Buffer[0];
        }

        /// <summary>
        /// Generates a source
        /// </summary>
        /// <returns>returns the source ID needed to do anything with it</returns>
        public static int GenSource()
        {
            int[] Source = new int[1];
            al.GenSources(1, Source);
            return Source[0];
        }

        #endregion
    }

    /// <summary>
    /// Provides all OpenAL (alc) capabilities.
    /// </summary>
    internal static class alc : object
    {
        #region Variables

        public const string DLLName = "OpenAL32.dll";

        public const int INVALID = 0;
        public const int FALSE = 0;
        public const int TRUE = 1;
        public const int NO_ERROR = FALSE;

        public const int FREQUENCY = 0x1007;
        public const int REFRESH = 0x1008;
        public const int SYNC = 0x1009;

        public const int INVALID_DEVICE = 0xA001;
        public const int INVALID_CONTEXT = 0xA002;
        public const int INVALID_ENUM = 0xA003;
        public const int INVALID_VALUE = 0xA004;
        public const int OUT_OF_MEMORY = 0xA005;

        public const int DEFAULT_DEVICE_SPECIFIER = 0x1004;
        public const int DEVICE_SPECIFIER = 0x1005;
        public const int EXTENSIONS = 0x1006;

        public const int MAJOR_VERSION = 0x1000;
        public const int MINOR_VERSION = 0x1001;

        public const int ATTRIBUTES_SIZE = 0x1002;
        public const int ALL_ATTRIBUTES = 0x1003;

        #endregion

        #region DLLImports

        [DllImport(DLLName, EntryPoint = "alcCreateContext", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
        public static extern IntPtr CreateContext(IntPtr device, int[] attrlist);

        [DllImport(DLLName, EntryPoint = "alcMakeContextCurrent", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
        public static extern int MakeContextCurrent(IntPtr context);

        [DllImport(DLLName, EntryPoint = "alcProcessContext", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
        public static extern void ProcessContext(IntPtr context);

        [DllImport(DLLName, EntryPoint = "alcSuspendContext", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
        public static extern void SuspendContext(IntPtr context);

        [DllImport(DLLName, EntryPoint = "alcDestroyContext", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
        public static extern void DestroyContext(IntPtr context);

        [DllImport(DLLName, EntryPoint = "alcGetError", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
        public static extern int GetError(IntPtr device);

        [DllImport(DLLName, EntryPoint = "alcGetCurrentContext", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
        public static extern IntPtr GetCurrentContext();

        [DllImport(DLLName, EntryPoint = "alcOpenDevice", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
        public static extern IntPtr OpenDevice([MarshalAs(UnmanagedType.LPStr)]string deviceName);

        [DllImport(DLLName, EntryPoint = "alcCloseDevice", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
        public static extern void CloseDevice(IntPtr device);

        [DllImport(DLLName, EntryPoint = "alcIsExtensionPresent", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
        public static extern bool IsExtensionPresent(IntPtr device, string extName);

        [DllImport(DLLName, EntryPoint = "alcGetProcAddress", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
        public static extern IntPtr GetProcAddress(IntPtr device, byte[] funcName);

        [DllImport(DLLName, EntryPoint = "alcGetEnumValue", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
        public static extern int GetEnumValue(IntPtr device, byte[] enumName);

        [DllImport(DLLName, EntryPoint = "alcGetContextDevice", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
        public static extern IntPtr GetContextsDevice(IntPtr context);

        [DllImport(DLLName, EntryPoint = "alcGetIntegerv", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
        public static extern void GetIntegerv(IntPtr device, int param, int intsize, out int data);

        [DllImport(DLLName, EntryPoint = "alcGetString", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
        public static extern IntPtr GetString(IntPtr device, int param);

        #endregion

        #region auxiliary functions

        public static int GetIntegerv(IntPtr Device, int Param)
        {
            int Result;
            alc.GetIntegerv(Device, Param, sizeof(int), out Result);

            return Result;
        }

        public static string GetString(IntPtr Device, int Param, string Seperator)
        {
            int Offset = -1;

            IntPtr P = GetString(Device, Param);

            StringBuilder SB = new StringBuilder();

            while (true)
            {
                Offset++;
                char C = (char)Marshal.ReadByte(P, Offset);
                if (C == 0)
                {
                    //Die Liste endet mit zwei Nullzeichen
                    if ((char)Marshal.ReadByte(P, Offset + 1) == 0)
                        return SB.ToString();
                    SB.Append(Seperator);
                    continue;
                }
                SB = SB.Append(C);
            }
        }

        #endregion
    }

#if USE_ALUT
    internal static class alut : object
    {
        public const string DLLName = "alut.dll";

        [DllImport(DLLName, EntryPoint = "alutInit", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
        public static extern void Init(IntPtr argc, IntPtr argv);

        [DllImport(DLLName, EntryPoint = "alutExit", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
        public static extern void Exit();

        [DllImport(DLLName, EntryPoint = "alutLoadWAVFile", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
        public static extern void LoadWAVFile(string fname, out int format, out IntPtr data, out int size, out int frequ, out int loop);

        [DllImport(DLLName, EntryPoint = "alutUnloadWAV", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
        public static extern void UnloadWAV(int format, IntPtr data, int size, int frequ);
    }
#endif

    
}
