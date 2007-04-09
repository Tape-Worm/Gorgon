#Region "LGPL."
' 
' Gorgon.
' Copyright (C) 2006 Michael Winsor
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
' Created: Thursday, October 19, 2006 1:08:13 AM
' 
#End Region

Imports Forms = System.Windows.Forms
Imports SharpUtilities
Imports SharpUtilities.Utility
Imports GorgonLibrary
Imports GorgonLibrary.Graphics

''' <summary>
''' Main application form.
''' </summary>
Public Class MainForm
#Region "Methods."
    ''' <summary>
    ''' Handles the OnFrameBegin event of the Screen control.
    ''' </summary>
    ''' <param name="sender">The source of the event.</param>
    ''' <param name="e">The <see cref="GorgonLibrary.FrameEventArgs"/> instance containing the event data.</param>
    Private Sub Screen_OnFrameBegin(ByVal sender As Object, ByVal e As FrameEventArgs)
        ' For now, we do nothing here.
    End Sub

    ''' <summary>
    ''' Handles the Load event of the MainForm control.
    ''' </summary>
    ''' <param name="sender">The source of the event.</param>
    ''' <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
    Private Sub MainForm_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Try
            ' Initialize the library.
            Gorgon.Initialize()

            ' Display the logo and frame stats.
            Gorgon.LogoVisible = True
            Gorgon.FrameStatsVisible = False

            ' Set the video mode to match the form client area.
            Gorgon.SetMode(Me)

            ' Assign rendering event handler.
            AddHandler Gorgon.Screen.OnFrameBegin, New FrameEventHandler(AddressOf Screen_OnFrameBegin)

            ' Set the clear color to something ugly.
            Gorgon.Screen.BackgroundColor = Color.FromArgb(250, 245, 220)

            ' Begin execution.
            Gorgon.Go()
        Catch sEx As SharpException
            UI.ErrorBox(Me, "An unhandled error occured during execution, the program will now close.", sEx.ErrorLog)
            Application.Exit()
        Catch ex As Exception
            UI.ErrorBox(Me, "An unhandled error occured during execution, the program will now close.", ex.Message + "\n\n" + ex.StackTrace)
            Application.Exit()
        End Try
    End Sub

    ''' <summary>
    ''' Handles the FormClosing event of the MainForm control.
    ''' </summary>
    ''' <param name="sender">The source of the event.</param>
    ''' <param name="e">The <see cref="System.Windows.Forms.FormClosingEventArgs" /> instance containing the event data.</param>
    Private Sub MainForm_FormClosing(ByVal sender As System.Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles MyBase.FormClosing
        Gorgon.Terminate()
    End Sub

    ''' <summary>
    ''' Handles the KeyDown event of the MainForm control.
    ''' </summary>
    ''' <param name="sender">The source of the event.</param>
    ''' <param name="e">The <see cref="System.Windows.Forms.KeyEventArgs" /> instance containing the event data.</param>
    Private Sub MainForm_KeyDown(ByVal sender As System.Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles MyBase.KeyDown
        If (e.KeyCode = Keys.Escape) Then
            Close()
        End If
        If (e.KeyCode = Keys.S) Then
            Gorgon.FrameStatsVisible = Not Gorgon.FrameStatsVisible
        End If
    End Sub
#End Region
End Class
