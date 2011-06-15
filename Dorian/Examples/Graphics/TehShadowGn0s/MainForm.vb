#Region "MIT"
' 
' Gorgon.
' Copyright (C) 2008 Michael Winsor
' 
' Permission is hereby granted, free of charge, to any person obtaining a copy
' of this software and associated documentation files (the "Software"), to deal
' in the Software without restriction, including without limitation the rights
' to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
' copies of the Software, and to permit persons to whom the Software is
' furnished to do so, subject to the following conditions:
' 
' The above copyright notice and this permission notice shall be included in
' all copies or substantial portions of the Software.
' 
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
' FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
' AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
' LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
' OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
' THE SOFTWARE.
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
    Private _blurShader As FXShader = Nothing               ' Blur shader.
    Private _samples As Single = 40                         ' Blur samples.
    Private _auto As Boolean = True                         ' Auto blur.
    Private _moveShip1 As Boolean = True                    ' Flag to move ship 1.
    Private _instructions As TextSprite = Nothing           ' Instructions.
    Private _font As Font = Nothing                         ' Font for instructions.
    Private _help As Boolean = True                         ' Flag to show help.
    Private _lastPos As Vector2D = Vector2D.Zero            ' Last position.
#End Region

#Region "Properties."
    ''' <summary>
    ''' Property to return the scale ratio of the screen to the current render target.
    ''' </summary>
    ''' <value>The target scale.</value>
    Private ReadOnly Property TargetScale() As Vector2D
        Get
            Return New Vector2D(CSng(Gorgon.CurrentRenderTarget.Width / Gorgon.Screen.Width), CSng(Gorgon.CurrentRenderTarget.Height / Gorgon.Screen.Height))
        End Get
    End Property
#End Region

#Region "Methods."
    ''' <summary>
    ''' Function to convert the given set of normalized coordinates into the coordinates of the render target.
    ''' </summary>
    ''' <param name="coordinate">The coordinate.</param>
    ''' <returns>Converted coordinates.</returns>
    Private Function ToTarget(ByVal coordinate As Vector2D) As Vector2D
        Dim output As Vector2D = Vector2D.Zero     ' Transformed vector.

        output.X = CInt(coordinate.X * Gorgon.CurrentRenderTarget.Width)
        output.Y = CInt(coordinate.Y * Gorgon.CurrentRenderTarget.Height)

        Return output
    End Function

    ''' <summary>
    ''' Function to convert the given set of absolute coordinates into normalized coordinates.
    ''' </summary>
    ''' <param name="coordinate">The coordinate.</param>
    ''' <returns>Converted coordinates.</returns>
    Private Function ToUnit(ByVal coordinate As Vector2D) As Vector2D
        Dim output As Vector2D = Vector2D.Zero     ' Transformed vector.

        output.X = CSng(coordinate.X / Gorgon.CurrentRenderTarget.Width)
        output.Y = CSng(coordinate.Y / Gorgon.CurrentRenderTarget.Height)

        Return output
    End Function

    ''' <summary>
    ''' Handles the Reset event of the Gorgon control.
    ''' </summary>
    ''' <param name="sender">The source of the event.</param>
    ''' <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
    Private Sub Gorgon_Reset(ByVal sender As Object, ByVal e As EventArgs)
        _blurShader.Techniques("Blur").Parameters("blurAmount").SetValue(_blurPass1.Width)
        _blurShader.Techniques("Blur").Parameters("blur1").SetValue(_blurPass1)
        _blurShader.Techniques("Blur").Parameters("blur2").SetValue(_blurPass2)
    End Sub

    ''' <summary>
    ''' Handles the Idle event of the Gorgon control.
    ''' </summary>
    ''' <param name="sender">The source of the event.</param>
    ''' <param name="e">The <see cref="GorgonLibrary.Graphics.FrameEventArgs"/> instance containing the event data.</param>
    Private Sub Gorgon_Idle(ByVal sender As Object, ByVal e As FrameEventArgs)

        If (_samples - 1 > 0) Then
            Gorgon.CurrentRenderTarget = _blurPass2
        End If

        _shadow2.Opacity = 255
        _shadow.Opacity = 255
        _bgSprite.Scale = TargetScale
        _sprite.Scale = Vector2D.Multiply(_bgSprite.Scale, 0.5D)
        _sprite2.Scale = Vector2D.Multiply(_bgSprite.Scale, 0.5D)
        _shadow2.Scale = Vector2D.Multiply(_bgSprite.Scale, 0.5D)
        _shadow.Scale = Vector2D.Multiply(_bgSprite.Scale, 0.5D)
        _screenSprite.UniformScale = 1
        _tileCount = New Drawing.Size(MathUtility.Round(Gorgon.CurrentRenderTarget.Width / _bgSprite.ScaledWidth, 0, MidpointRounding.AwayFromZero), MathUtility.Round(Gorgon.CurrentRenderTarget.Height / _bgSprite.ScaledHeight, 0, MidpointRounding.AwayFromZero))

        For x As Integer = 0 To _tileCount.Width
            For y As Integer = 0 To _tileCount.Height
                _bgSprite.SetPosition(x * _bgSprite.ScaledWidth, y * _bgSprite.ScaledHeight)
                _bgSprite.Draw()
            Next
        Next

        If Not (_moveShip1) Then
            _sprite.Position = ToTarget(_lastPos)
            _shadow.Position = Vector2D.Add(_sprite.Position, Vector2D.Divide(Vector2D.Subtract(_sprite.Position, New Vector2D(Gorgon.CurrentRenderTarget.Width / 2D, Gorgon.CurrentRenderTarget.Height / 2D)), 8D))
            _shadow.Draw()
            _sprite.Draw()
        Else
            _sprite2.Position = ToTarget(_lastPos)
            _shadow2.Position = Vector2D.Add(_sprite2.Position, Vector2D.Divide(Vector2D.Subtract(_sprite2.Position, New Vector2D(Gorgon.CurrentRenderTarget.Width / 2D, Gorgon.CurrentRenderTarget.Height / 2D)), 8D))
            _shadow2.Draw()
            _sprite2.Draw()
        End If

        If (_auto) Then
            If (_moveShip1) Then
                _sprite.Scale = _bgSprite.Scale
                _shadow.Scale = _bgSprite.Scale
                _sprite.Position = ToTarget(_mousePos)
                _shadow.Position = Vector2D.Add(_sprite.Position, Vector2D.Divide(Vector2D.Subtract(_sprite.Position, New Vector2D(Gorgon.CurrentRenderTarget.Width / 2D, Gorgon.CurrentRenderTarget.Height / 2D)), 2D))
                _shadow.Opacity = 192
                _shadow.Draw()
                _sprite.Draw()
            Else
                _sprite2.Scale = _bgSprite.Scale
                _shadow2.Scale = _bgSprite.Scale
                _sprite2.Position = ToTarget(_mousePos)
                _shadow2.Position = Vector2D.Add(_sprite2.Position, Vector2D.Divide(Vector2D.Subtract(_sprite2.Position, New Vector2D(Gorgon.CurrentRenderTarget.Width / 2D, Gorgon.CurrentRenderTarget.Height / 2D)), 2D))
                _shadow2.Opacity = 192
                _shadow2.Draw()
                _sprite2.Draw()
            End If

            Gorgon.CurrentRenderTarget.BeginDrawing()
            Gorgon.CurrentRenderTarget.FilledCircle(ToTarget(New Vector2D(0.5D, 0.5D)).X, ToTarget(New Vector2D(0.5D, 0.5D)).Y, 10, Drawing.Color.White)
            Gorgon.CurrentRenderTarget.EndDrawing()
        End If


        ' Only apply the shader if we have blurring.
        If (_samples - 1 > 0) Then
            For i As Integer = 0 To _samples - 1
                Gorgon.CurrentRenderTarget = _blurPass1
                Gorgon.CurrentShader = _blurShader.Techniques(0).Passes("hBlur")
                _screenSprite.Draw()

                Gorgon.CurrentRenderTarget = _blurPass2
                Gorgon.CurrentShader = _blurShader.Techniques(0).Passes("vBlur")
                _screenSprite.Draw()
            Next

            Gorgon.CurrentShader = Nothing

            _screenSprite.Image = _blurPass1.Image

            Gorgon.CurrentRenderTarget = Nothing

            _screenSprite.Scale = New Vector2D(1 / _bgSprite.Scale.X, 1 / _bgSprite.Scale.Y)
            _screenSprite.Draw()
        End If

        If (_auto) Then
            _samples -= e.FrameDeltaTime * 25
        Else
            If (_moveShip1) Then
                _sprite.Scale = TargetScale
                _shadow.Scale = TargetScale
                _sprite.Position = ToTarget(_mousePos)
                _shadow.Position = Vector2D.Add(_sprite.Position, Vector2D.Divide(Vector2D.Subtract(_sprite.Position, New Vector2D(Gorgon.CurrentRenderTarget.Width / 2D, Gorgon.CurrentRenderTarget.Height / 2D)), 2D))
                _shadow.Opacity = 192
                _shadow.Draw()
                _sprite.Draw()
            Else
                _sprite2.Scale = TargetScale
                _shadow2.Scale = TargetScale
                _sprite2.Position = ToTarget(_mousePos)
                _shadow2.Position = Vector2D.Add(_sprite2.Position, Vector2D.Divide(Vector2D.Subtract(_sprite2.Position, New Vector2D(Gorgon.CurrentRenderTarget.Width / 2D, Gorgon.CurrentRenderTarget.Height / 2D)), 2D))
                _shadow2.Opacity = 192
                _shadow2.Draw()
                _sprite2.Draw()
            End If
            Gorgon.CurrentRenderTarget.BeginDrawing()
            Gorgon.CurrentRenderTarget.FilledCircle(ToTarget(New Vector2D(0.5D, 0.5D)).X, ToTarget(New Vector2D(0.5D, 0.5D)).Y, 10, Drawing.Color.White)
            Gorgon.CurrentRenderTarget.EndDrawing()
        End If

        If (_samples < 0) Then
            _samples = 0
            _auto = False
            _blurShader.Techniques("Blur").Parameters("fadeFactor").SetValue(0.0F)
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
#If DEBUG Then
        _blurShader = FXShader.FromFile("..\..\..\..\Resources\Shaders\GaussBlur.fx", ShaderCompileOptions.Debug)
#Else
        _blurShader = FXShader.FromFile("..\..\..\..\Resources\Shaders\GaussBlur.fx", ShaderCompileOptions.OptimizationLevel3)
#End If

        _shadowGen = New ShadowSprite(_sprite)
        _shadow = _shadowGen.CreateShadow(16)

        _shadowGen.Sprite = _sprite2
        _shadow2 = _shadowGen.CreateShadow(16)

        _blurPass1 = New RenderImage("BlurOutput1", 256, 256, ImageBufferFormats.BufferRGB888X8)
        _blurPass2 = New RenderImage("BlurOutput2", _blurPass1.Width, _blurPass1.Height, ImageBufferFormats.BufferRGB888X8)
        _screenSprite = New Sprite("ScreenSprite", _blurPass2)
        _blurShader.Techniques("Blur").Parameters("blurAmount").SetValue(Convert.ToSingle(_blurPass1.Width))
        _blurShader.Techniques("Blur").Parameters("fadeFactor").SetValue(0.02D)
        _blurShader.Techniques("Blur").Parameters("blur1").SetValue(_blurPass1)
        _blurShader.Techniques("Blur").Parameters("blur2").SetValue(_blurPass2)

        _lastPos = New Vector2D(0.5D, 0.25D)

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

        _mousePos = ToUnit(e.Location)
    End Sub

    ''' <summary>
    ''' Raises the <see cref="E:System.Windows.Forms.Control.Click" /> event.
    ''' </summary>
    ''' <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
    Protected Overrides Sub OnClick(ByVal e As System.EventArgs)
        MyBase.OnClick(e)

        _lastPos = _mousePos
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
