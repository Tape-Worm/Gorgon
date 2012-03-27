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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using csogg;
using csvorbis;
using System.Diagnostics;

namespace GorgonLibrary.Sound
{
    /// <summary>
    /// A stream that can decode OGG Vorbis files
    /// </summary>
    public class OggDecodeStream : Stream
    {
        class DebugWriter : TextWriter
        {
            public override Encoding Encoding
            {
                get { return Encoding.UTF8; }
            }

            public override void WriteLine()
            {
                Debug.WriteLine(String.Empty);
            }

            public override void WriteLine(string s)
            {
                Debug.WriteLine(s);
            }
        }

        #region Variables

		/// <summary>
		/// Number of channels.
		/// </summary>
        public int Channels;
		/// <summary>
		/// Playback rate.
		/// </summary>
        public int Rate;

        private Stream decodedStream;
        private const int HEADER_SIZE = 36;

        #endregion

        #region Constructors
		/// <summary>
		/// Initializes a new instance of the <see cref="OggDecodeStream"/> class.
		/// </summary>
		/// <param name="input">The input stream.</param>
		/// <param name="skipWavHeader">TRUE to skip the header for the wav file, FALSE to include it.</param>
        public OggDecodeStream(Stream input, bool skipWavHeader)
        {
            if (input == null)
                throw new ArgumentNullException("input");
            decodedStream = DecodeStream(input, skipWavHeader);
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="OggDecodeStream"/> class.
		/// </summary>
		/// <param name="input">The input stream.</param>
        public OggDecodeStream(Stream input)
            : this(input, false)
        {
        }
        #endregion

        #region Methods

        Stream DecodeStream(Stream input, bool skipWavHeader)
        {
            int convsize = 4096 * 2;
            byte[] convbuffer = new byte[convsize]; // take 8k out of the data segment, not the stack

            TextWriter s_err = new DebugWriter();
            Stream output = new MemoryStream();

            if (!skipWavHeader)
                output.Seek(HEADER_SIZE, SeekOrigin.Begin); // reserve place for WAV header

            SyncState oy = new SyncState(); // sync and verify incoming physical bitstream
            StreamState os = new StreamState(); // take physical pages, weld into a logical stream of packets
            Page og = new Page(); // one Ogg bitstream page.  Vorbis packets are inside
            Packet op = new Packet(); // one raw packet of data for decode

            Info vi = new Info();  // struct that stores all the static vorbis bitstream settings
            Comment vc = new Comment(); // struct that stores all the bitstream user comments
            DspState vd = new DspState(); // central working state for the packet->PCM decoder
            Block vb = new Block(vd); // local working space for packet->PCM decode

            byte[] buffer;
            int bytes = 0;

            // Decode setup

            oy.init(); // Now we can read pages

            while (true)
            { // we repeat if the bitstream is chained
                int eos = 0;

                // grab some data at the head of the stream.  We want the first page
                // (which is guaranteed to be small and only contain the Vorbis
                // stream initial header) We need the first page to get the stream
                // serialno.

                // submit a 4k block to libvorbis' Ogg layer
                int index = oy.buffer(4096);
                buffer = oy.data;
                try
                {
                    bytes = input.Read(buffer, index, 4096);
                }
                catch (Exception e)
                {
                    s_err.WriteLine(e);
                }
                oy.wrote(bytes);

                // Get the first page.
                if (oy.pageout(og) != 1)
                {
                    // have we simply run out of data?  If so, we're done.
                    if (bytes < 4096) break;

                    // error case.  Must not be Vorbis data
                    s_err.WriteLine("Input does not appear to be an Ogg bitstream.");
                }

                // Get the serial number and set up the rest of decode.
                // serialno first; use it to set up a logical stream
                os.init(og.serialno());

                // extract the initial header from the first page and verify that the
                // Ogg bitstream is in fact Vorbis data

                // I handle the initial header first instead of just having the code
                // read all three Vorbis headers at once because reading the initial
                // header is an easy way to identify a Vorbis bitstream and it's
                // useful to see that functionality seperated out.

                vi.init();
                vc.init();
                if (os.pagein(og) < 0)
                {
                    // error; stream version mismatch perhaps
                    s_err.WriteLine("Error reading first page of Ogg bitstream data.");
                }

                if (os.packetout(op) != 1)
                {
                    // no page? must not be vorbis
                    s_err.WriteLine("Error reading initial header packet.");
                }

                if (vi.synthesis_headerin(vc, op) < 0)
                {
                    // error case; not a vorbis header
                    s_err.WriteLine("This Ogg bitstream does not contain Vorbis audio data.");
                }

                // At this point, we're sure we're Vorbis.  We've set up the logical
                // (Ogg) bitstream decoder.  Get the comment and codebook headers and
                // set up the Vorbis decoder

                // The next two packets in order are the comment and codebook headers.
                // They're likely large and may span multiple pages.  Thus we reead
                // and submit data until we get our two pacakets, watching that no
                // pages are missing.  If a page is missing, error out; losing a
                // header page is the only place where missing data is fatal. */

                int i = 0;

                while (i < 2)
                {
                    while (i < 2)
                    {

                        int result = oy.pageout(og);
                        if (result == 0) break; // Need more data
                        // Don't complain about missing or corrupt data yet.  We'll
                        // catch it at the packet output phase

                        if (result == 1)
                        {
                            os.pagein(og); // we can ignore any errors here
                            // as they'll also become apparent
                            // at packetout
                            while (i < 2)
                            {
                                result = os.packetout(op);
                                if (result == 0) break;
                                if (result == -1)
                                {
                                    // Uh oh; data at some point was corrupted or missing!
                                    // We can't tolerate that in a header.  Die.
                                    s_err.WriteLine("Corrupt secondary header.  Exiting.");
                                }
                                vi.synthesis_headerin(vc, op);
                                i++;
                            }
                        }
                    }
                    // no harm in not checking before adding more
                    index = oy.buffer(4096);
                    buffer = oy.data;
                    try
                    {
                        bytes = input.Read(buffer, index, 4096);
                    }
                    catch (Exception e)
                    {
                        s_err.WriteLine(e);
                    }
                    if (bytes == 0 && i < 2)
                    {
                        s_err.WriteLine("End of file before finding all Vorbis headers!");
                    }
                    oy.wrote(bytes);
                }

                // Throw the comments plus a few lines about the bitstream we're
                // decoding
                {
                    byte[][] ptr = vc.user_comments;
                    for (int j = 0; j < vc.user_comments.Length; j++)
                    {
                        if (ptr[j] == null) break;
                        s_err.WriteLine(vc.getComment(j));
                    }
                    s_err.WriteLine("\nBitstream is " + vi.channels + " channel, " + vi.rate + "Hz");
                    s_err.WriteLine("Encoded by: " + vc.getVendor() + "\n");
                }

                convsize = 4096 / vi.channels;

                // OK, got and parsed all three headers. Initialize the Vorbis
                //  packet->PCM decoder.
                vd.synthesis_init(vi); // central decode state
                vb.init(vd);           // local state for most of the decode

                // so multiple block decodes can
                // proceed in parallel.  We could init
                // multiple vorbis_block structures
                // for vd here

                float[][][] _pcm = new float[1][][];
                int[] _index = new int[vi.channels];
                // The rest is just a straight decode loop until end of stream
                while (eos == 0)
                {
                    while (eos == 0)
                    {

                        int result = oy.pageout(og);
                        if (result == 0) break; // need more data
                        if (result == -1)
                        { // missing or corrupt data at this page position
                            s_err.WriteLine("Corrupt or missing data in bitstream; continuing...");
                        }
                        else
                        {
                            os.pagein(og); // can safely ignore errors at
                            // this point
                            while (true)
                            {
                                result = os.packetout(op);

                                if (result == 0) break; // need more data
                                if (result == -1)
                                { // missing or corrupt data at this page position
                                    // no reason to complain; already complained above
                                }
                                else
                                {
                                    // we have a packet.  Decode it
                                    int samples;
                                    if (vb.synthesis(op) == 0)
                                    { // test for success!
                                        vd.synthesis_blockin(vb);
                                    }

                                    // **pcm is a multichannel float vector.  In stereo, for
                                    // example, pcm[0] is left, and pcm[1] is right.  samples is
                                    // the size of each channel.  Convert the float values
                                    // (-1.<=range<=1.) to whatever PCM format and write it out

                                    while ((samples = vd.synthesis_pcmout(_pcm, _index)) > 0)
                                    {
                                        float[][] pcm = _pcm[0];
                                        bool clipflag = false;
                                        int bout = (samples < convsize ? samples : convsize);

                                        // convert floats to 16 bit signed ints (host order) and
                                        // interleave
                                        for (i = 0; i < vi.channels; i++)
                                        {
                                            int ptr = i * 2;
                                            //int ptr=i;
                                            int mono = _index[i];
                                            for (int j = 0; j < bout; j++)
                                            {
                                                int val = (int)(pcm[i][mono + j] * 32767.0);
                                                //        short val=(short)(pcm[i][mono+j]*32767.);
                                                //        int val=(int)Math.round(pcm[i][mono+j]*32767.);
                                                // might as well guard against clipping
                                                if (val > 32767)
                                                {
                                                    val = 32767;
                                                    clipflag = true;
                                                }
                                                if (val < -32768)
                                                {
                                                    val = -32768;
                                                    clipflag = true;
                                                }
                                                if (val < 0) val = val | 0x8000;
                                                convbuffer[ptr] = (byte)(val);
                                                convbuffer[ptr + 1] = (byte)((uint)val >> 8);
                                                ptr += 2 * (vi.channels);
                                            }
                                        }

                                        if (clipflag)
                                        {
                                            //s_err.WriteLine("Clipping in frame "+vd.sequence);
                                        }

                                        output.Write(convbuffer, 0, 2 * vi.channels * bout);

                                        vd.synthesis_read(bout); // tell libvorbis how
                                        // many samples we
                                        // actually consumed
                                    }
                                }
                            }
                            if (og.eos() != 0) eos = 1;
                        }
                    }
                    if (eos == 0)
                    {
                        index = oy.buffer(4096);
                        buffer = oy.data;
                        try
                        {
                            bytes = input.Read(buffer, index, 4096);
                        }
                        catch (Exception e)
                        {
                            s_err.WriteLine(e);
                        }
                        oy.wrote(bytes);
                        if (bytes == 0) eos = 1;
                    }
                }

                // clean up this logical bitstream; before exit we see if we're
                // followed by another [chained]

                os.clear();

                // ogg_page and ogg_packet structs always point to storage in
                // libvorbis.  They're never freed or manipulated directly

                vb.clear();
                vd.clear();
                vi.clear();  // must be called last
            }

            // OK, clean up the framer
            oy.clear();
            s_err.WriteLine("Done.");

            byte[] Data = new byte[output.Length];
            output.Read(Data, 0, (int)output.Length);

            Channels = vi.channels;
            Rate = vi.rate;

            output.Seek(0, SeekOrigin.Begin);
            if (!skipWavHeader)
            {
                WriteHeader(output, (int)(output.Length - HEADER_SIZE), vi.rate, (ushort)16, (ushort)vi.channels);
                output.Seek(0, SeekOrigin.Begin);
            }
            return output;
        }


        void WriteHeader(Stream stream, int length, int audioSampleRate, ushort audioBitsPerSample, ushort audioChannels)
        {
            BinaryWriter bw = new BinaryWriter(stream);

            bw.Write(new char[4] { 'R', 'I', 'F', 'F' });
            int fileSize = HEADER_SIZE + length;
            bw.Write(fileSize);
            bw.Write(new char[8] { 'W', 'A', 'V', 'E', 'f', 'm', 't', ' ' });
            bw.Write((int)16);
            bw.Write((short)1);
            bw.Write((short)audioChannels);
            bw.Write(audioSampleRate);
            bw.Write((int)(audioSampleRate * ((audioBitsPerSample * audioChannels) / 8)));
            bw.Write((short)((audioBitsPerSample * audioChannels) / 8));
            bw.Write((short)audioBitsPerSample);

            bw.Write(new char[4] { 'd', 'a', 't', 'a' });
            bw.Write(length);
        }


		/// <summary>
		/// When overridden in a derived class, gets a value indicating whether the current stream supports reading.
		/// </summary>
		/// <returns>true if the stream supports reading; otherwise, false.
		///   </returns>
        public override bool CanRead
        {
            get { return true; }
        }

		/// <summary>
		/// When overridden in a derived class, gets a value indicating whether the current stream supports seeking.
		/// </summary>
		/// <returns>true if the stream supports seeking; otherwise, false.
		///   </returns>
        public override bool CanSeek
        {
            get { return true; }
        }

		/// <summary>
		/// When overridden in a derived class, gets a value indicating whether the current stream supports writing.
		/// </summary>
		/// <returns>true if the stream supports writing; otherwise, false.
		///   </returns>
        public override bool CanWrite
        {
            get { return false; }
        }

		/// <summary>
		/// When overridden in a derived class, clears all buffers for this stream and causes any buffered data to be written to the underlying device.
		/// </summary>
		/// <exception cref="T:System.IO.IOException">
		/// An I/O error occurs.
		///   </exception>
        public override void Flush()
        {
            throw new NotImplementedException();
        }

		/// <summary>
		/// When overridden in a derived class, gets the length in bytes of the stream.
		/// </summary>
		/// <returns>
		/// A long value representing the length of the stream in bytes.
		///   </returns>
		///   
		/// <exception cref="T:System.NotSupportedException">
		/// A class derived from Stream does not support seeking.
		///   </exception>
		///   
		/// <exception cref="T:System.ObjectDisposedException">
		/// Methods were called after the stream was closed.
		///   </exception>
        public override long Length
        {
            get { return decodedStream.Length; }
        }

		/// <summary>
		/// When overridden in a derived class, gets or sets the position within the current stream.
		/// </summary>
		/// <returns>
		/// The current position within the stream.
		///   </returns>
		///   
		/// <exception cref="T:System.IO.IOException">
		/// An I/O error occurs.
		///   </exception>
		///   
		/// <exception cref="T:System.NotSupportedException">
		/// The stream does not support seeking.
		///   </exception>
		///   
		/// <exception cref="T:System.ObjectDisposedException">
		/// Methods were called after the stream was closed.
		///   </exception>
        public override long Position
        {
            get
            {
                return decodedStream.Position;
            }
            set
            {
                decodedStream.Position = value;
            }
        }

		/// <summary>
		/// When overridden in a derived class, reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
		/// </summary>
		/// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between <paramref name="offset"/> and (<paramref name="offset"/> + <paramref name="count"/> - 1) replaced by the bytes read from the current source.</param>
		/// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which to begin storing the data read from the current stream.</param>
		/// <param name="count">The maximum number of bytes to be read from the current stream.</param>
		/// <returns>
		/// The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.
		/// </returns>
		/// <exception cref="T:System.ArgumentException">
		/// The sum of <paramref name="offset"/> and <paramref name="count"/> is larger than the buffer length.
		///   </exception>
		///   
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="buffer"/> is null.
		///   </exception>
		///   
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		///   <paramref name="offset"/> or <paramref name="count"/> is negative.
		///   </exception>
		///   
		/// <exception cref="T:System.IO.IOException">
		/// An I/O error occurs.
		///   </exception>
		///   
		/// <exception cref="T:System.NotSupportedException">
		/// The stream does not support reading.
		///   </exception>
		///   
		/// <exception cref="T:System.ObjectDisposedException">
		/// Methods were called after the stream was closed.
		///   </exception>
        public override int Read(byte[] buffer, int offset, int count)
        {
            return decodedStream.Read(buffer, offset, count);
        }

		/// <summary>
		/// When overridden in a derived class, sets the position within the current stream.
		/// </summary>
		/// <param name="offset">A byte offset relative to the <paramref name="origin"/> parameter.</param>
		/// <param name="origin">A value of type <see cref="T:System.IO.SeekOrigin"/> indicating the reference point used to obtain the new position.</param>
		/// <returns>
		/// The new position within the current stream.
		/// </returns>
		/// <exception cref="T:System.IO.IOException">
		/// An I/O error occurs.
		///   </exception>
		///   
		/// <exception cref="T:System.NotSupportedException">
		/// The stream does not support seeking, such as if the stream is constructed from a pipe or console output.
		///   </exception>
		///   
		/// <exception cref="T:System.ObjectDisposedException">
		/// Methods were called after the stream was closed.
		///   </exception>
        public override long Seek(long offset, SeekOrigin origin)
        {
            return decodedStream.Seek(offset, origin);
        }

		/// <summary>
		/// When overridden in a derived class, sets the length of the current stream.
		/// </summary>
		/// <param name="value">The desired length of the current stream in bytes.</param>
		/// <exception cref="T:System.IO.IOException">
		/// An I/O error occurs.
		///   </exception>
		///   
		/// <exception cref="T:System.NotSupportedException">
		/// The stream does not support both writing and seeking, such as if the stream is constructed from a pipe or console output.
		///   </exception>
		///   
		/// <exception cref="T:System.ObjectDisposedException">
		/// Methods were called after the stream was closed.
		///   </exception>
        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

		/// <summary>
		/// When overridden in a derived class, writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
		/// </summary>
		/// <param name="buffer">An array of bytes. This method copies <paramref name="count"/> bytes from <paramref name="buffer"/> to the current stream.</param>
		/// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which to begin copying bytes to the current stream.</param>
		/// <param name="count">The number of bytes to be written to the current stream.</param>
		/// <exception cref="T:System.ArgumentException">
		/// The sum of <paramref name="offset"/> and <paramref name="count"/> is greater than the buffer length.
		///   </exception>
		///   
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="buffer"/> is null.
		///   </exception>
		///   
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		///   <paramref name="offset"/> or <paramref name="count"/> is negative.
		///   </exception>
		///   
		/// <exception cref="T:System.IO.IOException">
		/// An I/O error occurs.
		///   </exception>
		///   
		/// <exception cref="T:System.NotSupportedException">
		/// The stream does not support writing.
		///   </exception>
		///   
		/// <exception cref="T:System.ObjectDisposedException">
		/// Methods were called after the stream was closed.
		///   </exception>
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
    

}
