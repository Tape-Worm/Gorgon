#region MIT.
// 
// Examples.
// Copyright (C) 2012 Michael Winsor
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
// Created: Tuesday, March 27, 2012 9:35:20 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GorgonLibrary;
using GorgonLibrary.Sound;

namespace GorgonLibrary.Example
{
    public partial class formMain : Form
    {
        GSContext Context;

        /// <summary>
        /// This basically stores the sound.
        /// </summary>
        SoundBuffer EngineSound;

        /// <summary>
        /// Another sound buffer for the background music
        /// </summary>
        SoundBuffer MusicSound;

        /// <summary>
        /// This is a player of a sound. You can have multiple Sound Sources to the same Sound Buffer
        /// </summary>
        SoundSource EngineSource;

        /// <summary>
        /// Plays background music
        /// </summary>
        SoundSource MusicSource;

        /// <summary>
        /// If true, then the pitch for the sound source is going up
        /// </summary>
        Boolean PitchGoingUp = true;

        public formMain()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //This is the setup for the graphics library
            Gorgon.Initialize();
            Gorgon.SetMode(this);
            Gorgon.LogoVisible = true;

            Gorgon.Idle += new GorgonLibrary.Graphics.FrameEventHandler(Gorgon_Idle);

            Gorgon.Go();

            //Now this is the setup for the sound
            Context = new GSContext();

            Context.SetListener(
                //Set the position of the listener to the center screen
                new Vector3D(Gorgon.Screen.Width / 2, Gorgon.Screen.Height / 2, 0f), 
                //Set the velocity of the listener to 0
                Vector3D.Zero, 
                //Set the direction of the listener to be looking in to the screen
                new Vector3D(0f, 0f, 1f));

            //EngineSound = new SoundBuffer(@"HT1Engine.wav", SoundType.Wav);
            EngineSound = new SoundBuffer(@"HT1Engine.wav", SoundType.Wav);

            //Create a source using the engine sound
            EngineSource = new SoundSource(EngineSound);

            //Set the sound to center screen
            EngineSource.Position = new Vector3D(Gorgon.Screen.Width / 2, Gorgon.Screen.Height / 2, 0f);
            //This will cause the sound to loop
            EngineSource.Loop = true;

            
            
            //Create the music buffer from an OGG File
            MusicSound = new SoundBuffer(@"DST-Xend.ogg", SoundType.OGG);

            MusicSource = new SoundSource(MusicSound);

            MusicSource.Loop = true;

            MusicSource.Play();

            //Tell the sound to play
            EngineSource.Play();
        }

        void Gorgon_Idle(object sender, GorgonLibrary.Graphics.FrameEventArgs e)
        {
            Gorgon.Screen.Clear(Color.Black);

            if (PitchGoingUp)
            {
                EngineSource.Pitch += 0.1f * e.FrameDeltaTime;
                if (EngineSource.Pitch > 1.5f)
                    PitchGoingUp = false;
            }
            else
            {
                EngineSource.Pitch -= 0.1f * e.FrameDeltaTime;
                if (EngineSource.Pitch < 0.25f)
                    PitchGoingUp = true;
            }
        }
    }
}
