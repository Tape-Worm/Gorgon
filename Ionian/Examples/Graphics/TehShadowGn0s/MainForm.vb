Imports Drawing = System.Drawing
Imports Dialogs
Imports GorgonLibrary.Graphics

''' <summary>
''' Main application form.
''' </summary>
Public Class MainForm
#Region "Variables."
    Private _bgImage As Image = Nothing                     ' Background image.
    Private _sprite As Sprite = Nothing                     ' Sprite to draw.
    Private _shader As Shader = Nothing                     ' Shader that will draw shadowing.
#End Region

#Region "Methods."
    ''' <summary>
    ''' Handles the Idle event of the Gorgon control.
    ''' </summary>
    ''' <param name="sender">The source of the event.</param>
    ''' <param name="e">The <see cref="GorgonLibrary.Graphics.FrameEventArgs"/> instance containing the event data.</param>
    Private Sub Gorgon_Idle(ByVal sender As Object, ByVal e As FrameEventArgs)
        Gorgon.CurrentRenderTarget.Clear()


    End Sub

    ''' <summary>
    ''' Function to provide initialization for our example.
    ''' </summary>
    Private Sub Initialize()
        ' Set smoothing mode to all the sprites.
        Gorgon.GlobalStateSettings.GlobalSmoothing = Smoothing.Smooth
    End Sub

    ''' <summary>
    ''' Raises the <see cref="E:System.Windows.Forms.Control.KeyDown" /> event.
    ''' </summary>
    ''' <param name="e">A <see cref="T:System.Windows.Forms.KeyEventArgs" /> that contains the event data.</param>
    Protected Overrides Sub OnKeyDown(ByVal e As System.Windows.Forms.KeyEventArgs)
        MyBase.OnKeyDown(e)

        Select Case (e.KeyCode)
            Case Keys.Escape
                Close()
            Case Keys.S
                Gorgon.FrameStatsVisible = Not Gorgon.FrameStatsVisible
        End Select
    End Sub

    ''' <summary>
    ''' Raises the <see cref="E:System.Windows.Forms.Form.FormClosing" /> event.
    ''' </summary>
    ''' <param name="e">A <see cref="T:System.Windows.Forms.FormClosingEventArgs" /> that contains the event data.</param>
    Protected Overrides Sub OnFormClosing(ByVal e As System.Windows.Forms.FormClosingEventArgs)
        MyBase.OnFormClosing(e)

        RemoveHandler Gorgon.Idle, New FrameEventHandler(AddressOf Gorgon_Idle)
        Gorgon.Terminate()
    End Sub

    ''' <summary>
    ''' Raises the <see cref="E:System.Windows.Forms.Form.Load" /> event.
    ''' </summary>
    ''' <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
    Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
        MyBase.OnLoad(e)

        Try
            ' Initialize Gorgon
            ' Set it up so that we won't be rendering in the background, but allow the screensaver to activate.
            Gorgon.Initialize(False, True)

            ' Display the logo.
            Gorgon.LogoVisible = True
            Gorgon.FrameStatsVisible = False

            If (Gorgon.CurrentDriver.PixelShaderVersion < New Version(2, 0)) Then
                UI.ErrorBox(Me, "This example requires a shader model 2 capable video card.\nSee details for more information.", "Required pixel shader version: 2.0\n" + Gorgon.CurrentDriver.Description + " pixel shader version: " + Gorgon.CurrentDriver.PixelShaderVersion.ToString())
                Application.Exit()
                Return
            End If

            ' Set the video mode.
            ClientSize = New Drawing.Size(640, 480)
            Gorgon.SetMode(Me)

            ' Set an ugly background color.
            Gorgon.Screen.BackgroundColor = Drawing.Color.DarkGray

            ' Initialize.
            Initialize()

            ' Add events.
            AddHandler Gorgon.Idle, New FrameEventHandler(AddressOf Gorgon_Idle)

            ' Begin rendering.
            Gorgon.Go()
        Catch ex As Exception
            UI.ErrorBox(Me, "Unable to initialize the application.", ex)
        End Try
    End Sub
#End Region
End Class
