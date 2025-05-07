
// 
// Gorgon
// Copyright (C) 2025 Michael Winsor
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
// Created: Tuesday, September 18, 2012 8:03:43 PM
// 

using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Gorgon.Diagnostics;
using Gorgon.Timing;
using Gorgon.UI.WindowsForms;

namespace Gorgon.Examples;

/// <summary>
/// The application context
/// </summary>
/// <remarks>We'll use this to display our splash screen, and then our main form and still use the idle loop.</remarks>
internal class Context
    : ApplicationContext
{
    // A timer for the splash screen.
    private readonly GorgonTimer _timer;
    // A splash screen.
    private readonly Splash _splashScreen;
    // The log for the application.
    private readonly IGorgonLog _log;
    // The application loop. 
    private readonly GorgonApplicationLoop _loop;
    // The current idle loop.
    private Func<bool> _idle;

    /// <summary>
    /// Property to return the main form for the application.
    /// </summary>
    public Form? Form
    {
        get;
        private set;
    }

    /// <summary>
    /// Handles the KeyDown event of the MainForm control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="KeyEventArgs" /> instance containing the event data.</param>
    /// <exception cref="NotSupportedException"></exception>
    private void MainForm_KeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.KeyCode)
        {
            case Keys.Space:
                (MainForm as Form)?.Clear();

                string message;

                if (_idle == Program.Idle)
                {
                    message = "In new idle loop.";
                    _idle = Program.NewIdle;
                }
                else
                {
                    message = "In primary idle loop.";
                    _idle = Program.Idle;
                }

                _loop.Run(_idle, _loop.AllowBackgroundExecution);

                _log.Print(message, LoggingLevel.All);
                break;
            case Keys.E:
                _log.PrintError("This is an error.", LoggingLevel.Simple);
                break;
            case Keys.W:
                _log.PrintWarning("This is a warning.", LoggingLevel.Simple);
                break;
            case Keys.X:
                _log.PrintException(new Exception("This is an exception."));
                break;
        }
    }

    /// <inheritdoc/>
    protected override void OnMainFormClosed(object? sender, EventArgs e)
    {
        base.OnMainFormClosed(sender, e);

        Form = null;

        ExitThread();
    }

    /// <summary>
    /// Function to begin the execution of the application context.
    /// </summary>
    public void RunMe()
    {
        Debug.Assert(Form is not null, "Form is null.");

        string annoyUser = "I'm going to delay you for a bit.";
        int counter = 0;

        try
        {
            _timer.Reset();
            _splashScreen.Show();
            _splashScreen.Refresh();
            _splashScreen.UpdateText("This is the splash screen.");

            // Fade in the splash screen about 10% every 7 milliseconds.
            while (_splashScreen.Opacity < 1)
            {
                if (!(_timer.Milliseconds > 7))
                {
                    continue;
                }

                _timer.Reset();
                _splashScreen.Opacity += 0.01;
            }

            // Annoy the user.  They're asking for it.
            while (counter < 6)
            {
                while (_timer.Seconds > 1)
                {
                    if (annoyUser.Length < 50)
                    {
                        annoyUser += ".";
                    }
                    else
                    {
                        annoyUser = "I'm going to delay you for a bit.";
                    }

                    _splashScreen.UpdateText(annoyUser);
                    _timer.Reset();
                    counter++;
                }
            }

            // Fade it out.
            while (_splashScreen.Opacity > 0.02)
            {
                if (!(_timer.Milliseconds > 5))
                {
                    continue;
                }

                _timer.Reset();
                _splashScreen.Opacity -= 0.01;
            }

            Form.KeyDown += MainForm_KeyDown;
            Form.Deactivate += (sender, args) => _log.Print("Application is deactivated. Loops will pause.", LoggingLevel.All);
            Form.Activated += (sender, args) => _log.Print("Application is activated. Loops will run.", LoggingLevel.All);
            Form.ClientSize = new Size(1280, 800);
            Form.Show();

            // We'll start out with the default idle loop.            
            _loop.Run(_idle, true);
            _log.Print("In primary idle loop.", LoggingLevel.All);
        }
        catch (Exception ex)
        {
            // If we get an error, then leave the application.
            GorgonDialogs.Error(Form, ex);

            Form.Dispose();
            MainForm = null;
        }
        finally
        {
            // We don't need this any more.
            _splashScreen?.Dispose();
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Context" /> class.
    /// </summary>
    /// <param name="log">The log for the application.</param>
    public Context(IGorgonLog log)
    {
        _log = log;

        // Create our timer object.
        _timer = new GorgonTimer();

        // Create the splash screen and the main interface.
        _splashScreen = new Splash();
        Form = new Form()
        {
            Log = log
        };

        // Note that we're assign this to the inherited property 'MainForm'.
        // This how the application context knows which form controls the application.
        MainForm = Form;

        _loop = GorgonApplicationLoop.Create(log);
        _idle = Program.Idle;

        RunMe();
    }
}
