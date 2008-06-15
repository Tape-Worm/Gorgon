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
    Private _blur As ShadowSprite = Nothing                 ' Blur sprite object.
    Private _shadow As Sprite = Nothing                     ' Soft shadow of sprite.
    Private _shadow2 As Sprite = Nothing                    ' Soft shadow of sprite.
#End Region

#Region "Methods."
    ''' <summary>
    ''' Handles the Reset event of the Gorgon control.
    ''' </summary>
    ''' <param name="sender">The source of the event.</param>
    ''' <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
    Private Sub Gorgon_Reset(ByVal sender As Object, ByVal e As EventArgs)
        _tileCount = New Drawing.Size(MathUtility.Round(Gorgon.Screen.Width / _bgSprite.Width, 0, MidpointRounding.AwayFromZero), MathUtility.Round(Gorgon.Screen.Height / _bgSprite.Height, 0, MidpointRounding.AwayFromZero))
    End Sub

    ''' <summary>
    ''' Handles the Idle event of the Gorgon control.
    ''' </summary>
    ''' <param name="sender">The source of the event.</param>
    ''' <param name="e">The <see cref="GorgonLibrary.Graphics.FrameEventArgs"/> instance containing the event data.</param>
    Private Sub Gorgon_Idle(ByVal sender As Object, ByVal e As FrameEventArgs)
        Gorgon.CurrentRenderTarget.Clear()

        For x As Integer = 0 To _tileCount.Width
            For y As Integer = 0 To _tileCount.Height
                _bgSprite.SetPosition(x * _bgSprite.Width, y * _bgSprite.Height)
                _bgSprite.Draw()
            Next
        Next

        _sprite2.SetPosition((Gorgon.Screen.Width / 2D), (Gorgon.Screen.Height / 2D) - 100)
        _shadow2.Position = _sprite2.Position + New Vector2D(5.0, 5.0)
        _shadow2.Draw()
        _sprite2.Draw()

        _shadow.Position = Vector2D.Add(_mousePos, New Vector2D(2.0, 2.0))
        _shadow.Draw()
        _sprite.Position = _mousePos
        _sprite.Draw()
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

        _tileCount = New Drawing.Size(MathUtility.Round(Gorgon.Screen.Width / _bgSprite.Width, 0, MidpointRounding.AwayFromZero), MathUtility.Round(Gorgon.Screen.Height / _bgSprite.Height, 0, MidpointRounding.AwayFromZero))

        _blur = New ShadowSprite(_sprite)
        _blur.BlurAmount = 2.25
        _shadow = _blur.CreateShadow(12)
        _shadow.Axis = _sprite.Axis

        _blur.BlurAmount = 1.0
        _blur.Sprite = _sprite2
        _blur.ShadowColor = Drawing.Color.FromArgb(128, 0, 0, 0)
        _shadow2 = _blur.CreateShadow(8)
        _shadow2.Axis = _sprite2.Axis
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

        If Not IsNothing(_blur) Then
            _blur.Dispose()
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
