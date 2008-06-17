#Region "LGPL."
' 
' Gorgon.
' Copyright (C) 2008 Michael Winsor
' 
' This library is free software; you can redistribute it and/or
' modify it under the terms of the GNU Lesser General Public
' License as published by the Free Software Foundation; either
' version 2.1 of the License, or (at your option) any later version.
' 
' This library is distributed in the hope that it will be useful,
' but WITHOUT ANY WARRANTY; without even the implied warranty of
' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
' Lesser General Public License for more details.
' 
' You should have received a copy of the GNU Lesser General Public
' License along with this library; if not, write to the Free Software
' Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
' 
' Created: Sunday, June 15, 2008 3:45:39 PM
' 
#End Region

Imports Drawing = System.Drawing
Imports Dialogs
Imports GorgonLibrary.Graphics

''' <summary>
''' Main application form.
''' </summary>
Public Class MainForm
#Region "Variables."
    Private _bgImage As Image = Nothing                     ' Background image.
    Private _bgSprite As Sprite = Nothing                   ' Background sprite.
    Private _sprite As Sprite = Nothing                     ' Sprite to draw.
    Private _sprite2 As Sprite = Nothing                    ' Sprite to draw.
    Private _tileCount As Drawing.Size = Drawing.Size.Empty ' Number of tiles.
    Private _mousePos As Vector2D = Vector2D.Zero           ' Mouse position.
    Private _shadowGen As ShadowSprite = Nothing            ' Soft shadow generator.
    Private _shadow As Sprite = Nothing                     ' Soft shadow of sprite.
    Private _shadow2 As Sprite = Nothing                    ' Soft shadow of sprite.
    Private _blurPass1 As RenderImage = Nothing             ' Blur pass image.
    Private _blurPass2 As RenderImage = Nothing             ' Final output image.
    Private _screenSprite As Sprite = Nothing               ' Sprite used to draw the output.
    Private _blurShader As Shader = Nothing                 ' Blur shader.
    Private _samples As Single = 40                         ' Blur samples.
    Private _auto As Boolean = True                         ' Auto blur.
    Private _moveShip1 As Boolean = True                    ' Flag to move ship 1.
    Private _instructions As TextSprite = Nothing           ' Instructions.
    Private _font As Font = Nothing                         ' Font for instructions.
    Private _help As Boolean = True                         ' Flag to show help.
#End Region

#Region "Methods."
    ''' <summary>
    ''' Handles the Reset event of the Gorgon control.
    ''' </summary>
    ''' <param name="sender">The source of the event.</param>
    ''' <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
    Private Sub Gorgon_Reset(ByVal sender As Object, ByVal e As EventArgs)
        _tileCount = New Drawing.Size(MathUtility.Round(Gorgon.Screen.Width / _bgSprite.Width, 0, MidpointRounding.AwayFromZero), MathUtility.Round(Gorgon.Screen.Height / _bgSprite.Height, 0, MidpointRounding.AwayFromZero))
        _blurPass2.SetDimensions(Gorgon.Screen.Width, Gorgon.Screen.Height)
        _blurPass1.SetDimensions(Gorgon.Screen.Width, Gorgon.Screen.Height)
        _blurShader.Techniques("Blur").Parameters("blurAmount").SetValue(Gorgon.Screen.Width)
        _blurShader.Techniques("Blur").Parameters("blur1").SetValue(_blurPass1)
        _blurShader.Techniques("Blur").Parameters("blur2").SetValue(_blurPass2)
        _screenSprite.Width = _blurPass1.Width
        _screenSprite.Height = _blurPass1.Height
    End Sub

    ''' <summary>
    ''' Handles the Idle event of the Gorgon control.
    ''' </summary>
    ''' <param name="sender">The source of the event.</param>
    ''' <param name="e">The <see cref="GorgonLibrary.Graphics.FrameEventArgs"/> instance containing the event data.</param>
    Private Sub Gorgon_Idle(ByVal sender As Object, ByVal e As FrameEventArgs)
        _screenSprite.Shader = Nothing

        Gorgon.CurrentRenderTarget = _blurPass2

        For x As Integer = 0 To _tileCount.Width
            For y As Integer = 0 To _tileCount.Height
                _bgSprite.SetPosition(x * _bgSprite.Width, y * _bgSprite.Height)
                _bgSprite.Draw()
            Next
        Next

        If Not (_moveShip1) Then
            _shadow.Draw()
            _sprite.Draw()
        Else
            _shadow2.Draw()
            _sprite2.Draw()
        End If

        If (_auto) Then
            If (_moveShip1) Then
                _sprite.Position = _mousePos
                _shadow.Position = _mousePos
                _shadow.Draw()
                _sprite.Draw()
            Else
                _sprite2.Position = _mousePos
                _shadow2.Position = _mousePos
                _shadow2.Draw()
                _sprite2.Draw()
            End If
        End If

        _screenSprite.Image = _blurPass2.Image

        For i As Integer = 0 To _samples - 1
            _screenSprite.Shader = _blurShader
            Gorgon.CurrentRenderTarget = _blurPass1
            _screenSprite.ShaderPass = 0
            _screenSprite.Draw()

            Gorgon.CurrentRenderTarget = _blurPass2
            _screenSprite.ShaderPass = 1
            _screenSprite.Draw()
        Next

        _screenSprite.Shader = Nothing
        _screenSprite.Image = _blurPass2.Image

        Gorgon.CurrentRenderTarget = Nothing
        _screenSprite.Draw()

        If (_auto) Then
            _samples -= e.FrameDeltaTime * 25
        Else
            If (_moveShip1) Then
                _shadow.Position = _mousePos
                _sprite.Position = _mousePos
                _shadow.Draw()
                _sprite.Draw()
            Else
                _shadow2.Position = _mousePos
                _sprite2.Position = _mousePos
                _shadow2.Draw()
                _sprite2.Draw()
            End If
        End If

        If (_samples < 0) Then
            _samples = 0
            _auto = False
            _blurShader.Techniques("Blur").Parameters("fadeFactor").SetValue(0)
        End If

        If (_help) Then
            _instructions.Draw()
        End If
    End Sub

    ''' <summary>
    ''' Function to provide initialization for our example.
    ''' </summary>
    Private Sub Initialize()
        ' Set smoothing mode to all the sprites.
        Gorgon.GlobalStateSettings.GlobalSmoothing = Smoothing.Smooth

        _bgImage = Image.FromFile("..\..\..\..\Resources\Images\VBback.jpg")
        _bgSprite = New Sprite("BgSprite", _bgImage)

        Image.FromFile("..\..\..\..\Resources\Images\0_HardVacuum.png")
        _sprite = Sprite.FromFile("..\..\..\..\Resources\Sprites\TehShadowGn0s\WeirdShip.gorSprite")
        _sprite2 = Sprite.FromFile("..\..\..\..\Resources\Sprites\TehShadowGn0s\WeirdShip2.gorSprite")
        _blurShader = Shader.FromFile("..\..\..\..\Resources\Shaders\GaussBlur.fx")

        _tileCount = New Drawing.Size(MathUtility.Round(Gorgon.Screen.Width / _bgSprite.Width, 0, MidpointRounding.AwayFromZero), MathUtility.Round(Gorgon.Screen.Height / _bgSprite.Height, 0, MidpointRounding.AwayFromZero))

        _shadowGen = New ShadowSprite(_sprite)
        _shadow = _shadowGen.CreateShadow(12)
        _shadow.Axis = Vector2D.Add(Vector2D.Add(_sprite.Axis, New Vector2D(-24.0, -24.0)), _shadow.Axis)

        _shadowGen.Sprite = _sprite2
        _shadow2 = _shadowGen.CreateShadow(8)
        _shadow2.Axis = Vector2D.Add(Vector2D.Add(_sprite2.Axis, New Vector2D(-12.0, -12.0)), _shadow2.Axis)

        _blurPass2 = New RenderImage("Output", Gorgon.Screen.Width, Gorgon.Screen.Height, ImageBufferFormats.BufferRGB888X8)
        _blurPass1 = New RenderImage("BlurOutput", Gorgon.Screen.Width, Gorgon.Screen.Height, ImageBufferFormats.BufferRGB888X8)
        _screenSprite = New Sprite("ScreenSprite", _blurPass2)
        _screenSprite.Shader = _blurShader
        _blurShader.Techniques("Blur").Parameters("blurAmount").SetValue(Gorgon.Screen.Width)
        _blurShader.Techniques("Blur").Parameters("fadeFactor").SetValue(0.02D)
        _blurShader.Techniques("Blur").Parameters("blur1").SetValue(_blurPass1)
        _blurShader.Techniques("Blur").Parameters("blur2").SetValue(_blurPass2)

        _sprite2.SetPosition((Gorgon.CurrentRenderTarget.Width / 2D), (Gorgon.CurrentRenderTarget.Height / 2D) - 100)
        _shadow2.Position = _sprite2.Position

        _font = New Font("Arial", "Arial", 9.0, True, True)
        _instructions = New TextSprite("Instructions", "Help:" & ControlChars.CrLf & "F1 - Show/hide this text." & ControlChars.CrLf & "S - Show frame stats." & ControlChars.CrLf & _
                "Click - move other ship into foreground." & ControlChars.CrLf & "Mousewheel - blur/sharpen background." & ControlChars.CrLf & "ESC - Quit.", _
                _font, Vector2D.Zero, Drawing.Color.Yellow)
        _instructions.Shadowed = True
    End Sub

    ''' <summary>
    ''' Raises the <see cref="E:System.Windows.Forms.Control.MouseMove" /> event.
    ''' </summary>
    ''' <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs" /> that contains the event data.</param>
    Protected Overrides Sub OnMouseMove(ByVal e As System.Windows.Forms.MouseEventArgs)
        MyBase.OnMouseMove(e)

        _mousePos = e.Location
    End Sub

    ''' <summary>
    ''' Raises the <see cref="E:System.Windows.Forms.Control.Click" /> event.
    ''' </summary>
    ''' <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
    Protected Overrides Sub OnClick(ByVal e As System.EventArgs)
        MyBase.OnClick(e)

        _moveShip1 = Not _moveShip1
    End Sub

    ''' <summary>
    ''' Raises the <see cref="E:System.Windows.Forms.Control.MouseWheel" /> event.
    ''' </summary>
    ''' <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs" /> that contains the event data.</param>
    Protected Overrides Sub OnMouseWheel(ByVal e As System.Windows.Forms.MouseEventArgs)
        MyBase.OnMouseWheel(e)

        If (e.Delta < 0) Then
            _samples -= 0.25
        End If

        If (e.Delta > 0) Then
            _samples += 0.25
        End If

        If (_samples < 0) Then
            _samples = 0
        End If

        If (_samples > 100) Then
            _samples = 100
        End If
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
            Case Keys.F1
                _help = Not _help
        End Select
    End Sub

    ''' <summary>
    ''' Raises the <see cref="E:System.Windows.Forms.Form.FormClosing" /> event.
    ''' </summary>
    ''' <param name="e">A <see cref="T:System.Windows.Forms.FormClosingEventArgs" /> that contains the event data.</param>
    Protected Overrides Sub OnFormClosing(ByVal e As System.Windows.Forms.FormClosingEventArgs)
        MyBase.OnFormClosing(e)

        If Not IsNothing(_shadowGen) Then
            _shadowGen.Dispose()
        End If

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
            AddHandler Gorgon.DeviceReset, New EventHandler(AddressOf Gorgon_Reset)

            ' Begin rendering.
            Gorgon.Go()
        Catch ex As Exception
            UI.ErrorBox(Me, "Unable to initialize the application.", ex)
        End Try
    End Sub
#End Region
End Class
