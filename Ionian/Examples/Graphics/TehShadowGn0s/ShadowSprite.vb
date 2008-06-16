﻿#Region "LGPL."
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
' Created: Sunday, June 15, 2008 3:46:16 PM
' 
#End Region

Imports GorgonLibrary.Graphics
Imports Dialogs

''' <summary>
''' Object to create a soft shadow from a sprite.
''' </summary>
Public Class ShadowSprite
    Implements IDisposable

#Region "Variables."
    Private _disposed As Boolean = False                                        ' Flag to indicate that the blur object is disposed.
    Private _shader As Shader = Nothing                                         ' Blur shader.
    Private _blurHTarget As RenderImage = Nothing                               ' Blur horizontal target.
    Private _blurVTarget As RenderImage = Nothing                               ' Blur vertical target.
    Private _output As Image = Nothing                                          ' Final output.
    Private _sprite As Sprite = Nothing                                         ' Sprite to blur.
    Private _blurAmount As Single = 2.5                                         ' Blur amount.
    Private _color As Drawing.Color = Drawing.Color.FromArgb(255, 0, 0, 0)      ' Shadow color.
    Private _bufferRoom As Single = 12.0                                        ' Area around the sprite to leave empty.
#End Region

#Region "Properties."
    ''' <summary>
    ''' Gets or sets the buffer room.
    ''' </summary>
    ''' <value>The buffer room.</value>
    Public Property BufferRoom() As Single
        Get
            Return _bufferRoom
        End Get
        Set(ByVal value As Single)
            _bufferRoom = value

            If Not IsNothing(_blurHTarget) Then
                _blurHTarget.Dispose()
                _blurHTarget = Nothing
            End If
            If Not IsNothing(_blurVTarget) Then
                _blurVTarget.Dispose()
                _blurVTarget = Nothing
            End If

            _blurHTarget = New RenderImage(_sprite.Name + ".BlurH", _sprite.Width + _bufferRoom * 2.0, _sprite.Height + _bufferRoom * 2.0, ImageBufferFormats.BufferRGB888A8)
            _blurVTarget = New RenderImage(_sprite.Name + ".BlurV", _sprite.Width + _bufferRoom * 2.0, _sprite.Height + _bufferRoom * 2.0, ImageBufferFormats.BufferRGB888A8)
            _blurHTarget.Clear(Drawing.Color.Transparent)
            _blurVTarget.Clear(Drawing.Color.Transparent)
            _shader.Techniques("Blur").Parameters("sourceImage").SetValue(_sprite.Image)
            _shader.Techniques("Blur").Parameters("blur1").SetValue(_blurHTarget)
            _shader.Techniques("Blur").Parameters("blur2").SetValue(_blurVTarget)
        End Set
    End Property

    ''' <summary>
    ''' Property to set or return the blur amount.
    ''' </summary>
    Public Property BlurAmount() As Integer
        Get
            Return _blurAmount
        End Get
        Set(ByVal value As Integer)
            If (value < 0.001) Then
                value = 0.001
            End If
            _blurAmount = value
        End Set
    End Property

    ''' <summary>
    ''' Property to set or return the color of the shadow.
    ''' </summary>
    Public Property ShadowColor() As Drawing.Color
        Get
            Return _color
        End Get
        Set(ByVal value As Drawing.Color)
            _color = value
        End Set
    End Property

    ''' <summary>
    ''' Property to set or return the sprite to create a shadow from.
    ''' </summary>
    Public Property Sprite() As Sprite
        Get
            Return _sprite
        End Get
        Set(ByVal value As Sprite)
            If IsNothing(value) Then
                Throw New ArgumentNullException("value")
            End If
            _sprite = TryCast(value.Clone(), Sprite)
            _sprite.Axis = Vector2D.Zero
            _sprite.Position = Vector2D.Zero

            If Not IsNothing(_blurHTarget) Then
                _blurHTarget.Dispose()
                _blurHTarget = Nothing
            End If
            If Not IsNothing(_blurVTarget) Then
                _blurVTarget.Dispose()
                _blurVTarget = Nothing
            End If

            _blurHTarget = New RenderImage(_sprite.Name + ".BlurH", _sprite.Width + _bufferRoom * 2.0, _sprite.Height + _bufferRoom * 2.0, ImageBufferFormats.BufferRGB888A8)
            _blurVTarget = New RenderImage(_sprite.Name + ".BlurV", _sprite.Width + _bufferRoom * 2.0, _sprite.Height + _bufferRoom * 2.0, ImageBufferFormats.BufferRGB888A8)
            _blurHTarget.Clear(Drawing.Color.Transparent)
            _blurVTarget.Clear(Drawing.Color.Transparent)
            _shader.Techniques("Blur").Parameters("sourceImage").SetValue(_sprite.Image)
            _shader.Techniques("Blur").Parameters("blur1").SetValue(_blurHTarget)
            _shader.Techniques("Blur").Parameters("blur2").SetValue(_blurVTarget)
        End Set
    End Property
#End Region

#Region "Methods."
    ''' <summary>
    ''' Function to perform create a shadow.
    ''' </summary>
    ''' <param name="samples">The number of times to blur the shadow for softer edges.</param>
    Public Function CreateShadow(ByVal samples As Integer) As Sprite
        Dim result As Sprite = Nothing                      ' Resulting sprite.
        Dim previousTarget As RenderTarget = Nothing        ' Previous render target.

        If (samples < 1) Then
            samples = 1
        End If

        previousTarget = Gorgon.CurrentRenderTarget
        Try
            _shader.Techniques("Blur").Parameters("blurAmount").SetValue(CSng(_blurHTarget.Width) / _blurAmount)

            ' Draw the initial image.
            Gorgon.CurrentRenderTarget = _blurVTarget

            _sprite.Shader = _shader
            _sprite.Position = New Vector2D(_bufferRoom, _bufferRoom)
            _sprite.Draw()

            _sprite.Image = _blurVTarget.Image
            _sprite.ImageOffset = Vector2D.Zero
            _sprite.Width = _blurVTarget.Width
            _sprite.Height = _blurVTarget.Height
            _sprite.Position = Vector2D.Zero

            For i As Integer = 0 To samples - 1
                Gorgon.CurrentRenderTarget = _blurHTarget
                _sprite.ShaderPass = 1
                _sprite.Draw()
                Gorgon.CurrentRenderTarget = _blurVTarget
                _sprite.ShaderPass = 2
                _sprite.Draw()
            Next

            ' Create a new image with the blurred data.
            _output = New Image(_sprite.Name + ".BlurImage", _blurVTarget.Width, _blurVTarget.Height, _blurVTarget.Format)
            _blurVTarget.CopyToImage(_output)
            result = New Sprite(_sprite.Name + ".Blurred", _output)
            result.Color = _color
            result.Axis = New Vector2D(_bufferRoom, _bufferRoom)
        Finally
            Gorgon.CurrentRenderTarget = previousTarget
        End Try

        Return result
    End Function
#End Region

#Region "Constructor/Destructor."
    ''' <summary>
    ''' Initializes a new instance of the <see cref="BlurSprite" /> class.
    ''' </summary>
    ''' <param name="sprite">The sprite to blur.</param>
    Public Sub New(ByVal sprite As Sprite)

        If IsNothing(sprite) Then
            Throw New ArgumentNullException("sprite")
        End If

        _shader = Shader.FromFile("..\..\..\..\Resources\Shaders\SpriteShadow.fx")

        Me.Sprite = TryCast(sprite.Clone(), Sprite)
        _sprite.Axis = Vector2D.Zero
        _sprite.Position = Vector2D.Zero
    End Sub
#End Region


#Region " IDisposable Support "
    ''' <summary>
    ''' Releases unmanaged and - optionally - managed resources
    ''' </summary>
    ''' <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    Protected Overridable Sub Dispose(ByVal disposing As Boolean)
        If Not _disposed Then
            If disposing Then
                If Not IsNothing(_shader) Then
                    _shader.Dispose()
                End If

                If Not IsNothing(_blurHTarget) Then
                    _blurHTarget.Dispose()
                End If

                If Not IsNothing(_blurVTarget) Then
                    _blurVTarget.Dispose()
                End If
            End If
        End If
        _disposed = True
    End Sub

    ''' <summary>
    ''' Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    ''' </summary>
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
#End Region

End Class
