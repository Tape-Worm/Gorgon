#pragma region LGPL.
// 
// Gorgon.
// Copyright (C) 2006 Michael Winsor
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// 
// Created: Thursday, October 19, 2006 1:18:59 AM
// 
#pragma endregion

#include"stdafx.h"
#include"MainForm.h"

/* Note:
 *  The function implementations are stored in this seperate file just for ease of reading.
 *  Having everything in the header file looks too cluttered.
 */
namespace Example1_Initializing
{
	#pragma region Methods.
	/// <summary>
	/// Handles the OnFrameBegin event of the Screen control.
	/// </summary>
	/// <param name="sender">The source of the event.</param>
	/// <param name="e">The <see cref="GorgonLibrary.FrameEventArgs"/> instance containing the event data.</param>
	System::Void MainForm::Screen_OnFrameBegin(Object^ sender, FrameEventArgs^ e)
	{
		// For now, we do nothing here.
	}

	/// <summary>
	/// Handles the Load event of the MainForm control.
	/// </summary>
	/// <param name="sender">The source of the event.</param>
	/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
	System::Void MainForm::MainForm_Load(System::Object^  sender, System::EventArgs^  e) 
	{
		try
		{
			// Initialize the library.
			Gorgon::Initialize();

			// Display the logo and frame stats.
			Gorgon::LogoVisible = true;
			Gorgon::FrameStatsVisible = false;

			// Set the video mode to match the form client area.
			Gorgon::SetMode(this);

			// Assign rendering event handler.
			Gorgon::Screen->OnFrameBegin += gcnew FrameEventHandler(this, &MainForm::Screen_OnFrameBegin);

			// Set the clear color to something ugly.
			Gorgon::Screen->BackgroundColor = Color::FromArgb(250, 245, 220);				
			
			// Begin execution.
			Gorgon::Go();
		}
		catch(SharpException^ sEx)
		{
			UI::ErrorBox(this, "An unhandled error occured during execution, the program will now close.", sEx->ErrorLog);
			System::Windows::Forms::Application::Exit();
		}
		catch(Exception^ ex)
		{
			UI::ErrorBox(this, "An unhandled error occured during execution, the program will now close.", ex->Message + "\n\n" + ex->StackTrace);
			System::Windows::Forms::Application::Exit();
		}
	}

	/// <summary>
	/// Handles the FormClosing event of the MainForm control.
	/// </summary>
	/// <param name="sender">The source of the event.</param>
	/// <param name="e">The <see cref="System.Windows.Forms.FormClosingEventArgs"/> instance containing the event data.</param>
	System::Void MainForm::MainForm_FormClosing(System::Object^  sender, System::Windows::Forms::FormClosingEventArgs^  e) 
	{
		Gorgon::Terminate();
	}

	/// <summary>
	/// Handles the KeyDown event of the MainForm control.
	/// </summary>
	/// <param name="sender">The source of the event.</param>
	/// <param name="e">The <see cref="System.Windows.Forms.KeyEventArgs"/> instance containing the event data.</param>
	System::Void MainForm::MainForm_KeyDown(System::Object^  sender, System::Windows::Forms::KeyEventArgs^  e) 
	{
		if (e->KeyCode == Keys::Escape)
			Close();
		if (e->KeyCode == Keys::S)
			Gorgon::FrameStatsVisible = !Gorgon::FrameStatsVisible;
	}
	#pragma endregion
}