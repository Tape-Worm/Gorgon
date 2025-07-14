// Gorgon.
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: July 12, 2025 10:08:32 PM
//

using Windows.Win32;

namespace Gorgon.UI.Win32;

/// <summary>
/// Several common windows message values.
/// </summary>
public static class GorgonWindowMessages
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    /// <summary>A Dynamic Data Exchange (DDE) client application sends a WM\_DDE\_INITIATE message to initiate a conversation with a server application responding to the specified application and topic names.</summary>
    /// <remarks>If the low-order word of *lParam* is **NULL**, any server application can respond. If the high-order word of *lParam* is **NULL**, any topic is valid. Upon receiving a **WM\_DDE\_INITIATE** request with the high-order word of the *lParam* parameter set to **NULL**, a server must send a [**WM\_DDE\_ACK**](wm-dde-ack.md) message for each of the topics it supports.</remarks>
    public const int WM_DDE_INITIATE = 992;

    /// <summary>A Dynamic Data Exchange (DDE) application (client or server) posts a WM\_DDE\_TERMINATE message to terminate a conversation. To post this message, call the PostMessage function with the following parameters.</summary>
    /// <remarks></remarks>
    public const int WM_DDE_TERMINATE = 993;

    /// <summary>A Dynamic Data Exchange (DDE) client application posts the WM\_DDE\_ADVISE message to a DDE server application to request the server to supply an update for a data item whenever the item changes.</summary>
    /// <remarks>If a client application supports more than one clipboard format for a single topic and item, it can post multiple **WM\_DDE\_ADVISE** messages for the topic and item, specifying a different clipboard format with each message. Note that a server can support multiple formats only for hot data links, not warm data links.</remarks>
    public const int WM_DDE_ADVISE = 994;

    /// <summary>A Dynamic Data Exchange (DDE) client application posts a WM\_DDE\_UNADVISE message to inform a DDE server application that the specified item or a particular clipboard format for the item should no longer be updated.</summary>
    /// <remarks>
    /// <para>The client application allocates the high-order word of *lParam* by calling the [**GlobalAddAtom**](/windows/desktop/api/Winbase/nf-winbase-globaladdatoma) function. The server application posts the [**WM\_DDE\_ACK**](wm-dde-ack.md) message to respond positively or negatively. When posting **WM\_DDE\_ACK**, the server can either reuse the atom, or it can delete the atom and create a new one.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/dataxchg/wm-dde-unadvise#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_DDE_UNADVISE = 995;

    /// <summary>The WM\_DDE\_ACK message notifies a Dynamic Data Exchange (DDE) application of the receipt and processing of the following messages WM\_DDE\_POKE, WM\_DDE\_EXECUTE, WM\_DDE\_DATA, WM\_DDE\_ADVISE, WM\_DDE\_UNADVISE, WM\_DDE\_INITIATE, or WM\_DDE\_REQUEST (in some cases). To post this message, call the PostMessage function with the following parameters.</summary>
    /// <remarks></remarks>
    public const int WM_DDE_ACK = 996;

    /// <summary>A Dynamic Data Exchange (DDE) server application posts a WM\_DDE\_DATA message to a DDE client application to pass a data item to the client or to notify the client of the availability of a data item.</summary>
    /// <remarks></remarks>
    public const int WM_DDE_DATA = 997;

    /// <summary>A Dynamic Data Exchange (DDE) client application posts a WM\_DDE\_REQUEST message to a DDE server application to request the value of a data item. To post this message, call the PostMessage function with the following parameters.</summary>
    /// <remarks></remarks>
    public const int WM_DDE_REQUEST = 998;

    /// <summary>A Dynamic Data Exchange (DDE) client application posts a WM\_DDE\_POKE message to a DDE server application.</summary>
    /// <remarks></remarks>
    public const int WM_DDE_POKE = 999;

    /// <summary>A Dynamic Data Exchange (DDE) client application posts a WM\_DDE\_EXECUTE message to a DDE server application to send a string to the server to be processed as a series of commands.</summary>
    /// <remarks>
    /// <para>The command string is a null-terminated string consisting of one or more opcode strings enclosed in single brackets (\[ \]). Each opcode string has the following syntax, where the *parameters* list is optional: *opcode parameters* The *opcode* is any application-defined single token. It cannot include spaces, commas, parentheses, brackets, or quotation marks. The *parameters* list can contain any application-defined value or values. Multiple parameters are separated by commas, and the entire parameter list is enclosed in parentheses. Parameters cannot include commas or parentheses except inside a quoted string. If a bracket or parenthesis character is to appear in a quoted string, it need not be doubled, as was the case under the old rules. The following are valid command strings:</para>
    /// <para></para>
    /// <para>This doc was truncated.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/dataxchg/wm-dde-execute#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_DDE_EXECUTE = 1000;

    public const int WM_IME_REPORT = 640;

    public const int WM_WNT_CONVERTREQUESTEX = 265;

    public const int WM_CONVERTREQUEST = 266;

    public const int WM_CONVERTRESULT = 267;

    public const int WM_INTERIM = 268;

    public const int WM_IMEKEYDOWN = 656;

    public const int WM_IMEKEYUP = 657;

    /// <summary>The WM\_CTLCOLOR message is used in 16-bit versions of Windows to change the color scheme of list boxes, the list boxes of combo boxes, message boxes, button controls, edit controls, static controls, and dialog boxes.Note  For information related to this message and 32-bit versions of Windows, see Remarks.</summary>
    /// <returns>If an application processes this message, it returns a handle to a brush. The system uses the brush to paint the background of the control.</returns>
    /// <remarks>
    /// <para>The **WM\_CTLCOLOR** message from 16-bit Windows has been replaced by more specific notifications. These replacements include the following: -   [**WM\_CTLCOLORBTN**](../controls/wm-ctlcolorbtn.md) -   [**WM\_CTLCOLOREDIT**](../controls/wm-ctlcoloredit.md) -   [**WM\_CTLCOLORDLG**](../dlgbox/wm-ctlcolordlg.md) -   [**WM\_CTLCOLORLISTBOX**](../controls/wm-ctlcolorlistbox.md) -   [**WM\_CTLCOLORSCROLLBAR**](../controls/wm-ctlcolorscrollbar.md) -   [**WM\_CTLCOLORSTATIC**](../controls/wm-ctlcolorstatic.md)</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/DevNotes/wm-ctlcolor-#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_CTLCOLOR = 25;

    /// <summary>Posted to a window when the cursor hovers over the client area of the window for the period of time specified in a prior call to TrackMouseEvent.</summary>
    /// <returns>If an application processes this message, it should return zero.</returns>
    /// <remarks>
    /// <para>Hover tracking stops when **WM\_MOUSEHOVER** is generated. The application must call [**TrackMouseEvent**](/windows/win32/api/winuser/nf-winuser-trackmouseevent) again if it requires further tracking of mouse hover behavior. Use the following code to obtain the horizontal and vertical position:</para>
    /// <para></para>
    /// <para>This doc was truncated.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/inputdev/wm-mousehover#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_MOUSEHOVER = 673;

    /// <summary>Posted to a window when the cursor leaves the client area of the window specified in a prior call to TrackMouseEvent.</summary>
    /// <returns>If an application processes this message, it should return zero.</returns>
    /// <remarks>All tracking requested by [**TrackMouseEvent**](/windows/win32/api/winuser/nf-winuser-trackmouseevent) is canceled when this message is generated. The application must call **TrackMouseEvent** when the mouse reenters its window if it requires further tracking of mouse hover behavior.</remarks>
    public const int WM_MOUSELEAVE = 675;

    /// <summary>An application sends the WM\_CHOOSEFONT\_GETLOGFONT message to a Font dialog box to retrieve information about the user's current font selections.</summary>
    /// <returns>This message does not return a value.</returns>
    /// <remarks>
    /// <para>The [**ChooseFont**](/windows/win32/api/commdlg/ns-commdlg-choosefonta) function creates a **Font** dialog box. When the user closes the **Font** dialog box, the **ChooseFont** function returns information about the user's font selections in the [**CHOOSEFONT**](/windows/win32/api/commdlg/ns-commdlg-choosefonta) structure. The **lpLogFont** member of the **CHOOSEFONT** structure is a pointer to a [**LOGFONT**](/windows/win32/api/wingdi/ns-wingdi-logfonta) structure. Use the **WM\_CHOOSEFONT\_GETLOGFONT** message to get information about the user's current font selections while the **Font** dialog box is open. For example, if you enable the **Apply** button in the **Font** dialog box, send the message to get the font information to apply to the current text selection. Typically, you enable a [*CFHookProc*](/windows/win32/api/commdlg/nc-commdlg-lpcfhookproc) hook procedure to process [**WM\_COMMAND**](/windows/desktop/menurc/wm-command) messages for the **Apply** button. When the user clicks the **Apply** button, the hook procedure sends the **WM\_CHOOSEFONT\_GETLOGFONT** message to the dialog box.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/dlgbox/wm-choosefont-getlogfont#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_CHOOSEFONT_GETLOGFONT = 1025;

    /// <summary>An application sends the WM\_CHOOSEFONT\_SETLOGFONT message to a Font dialog box to set the current logical font information.</summary>
    /// <returns>This message has no return value.</returns>
    /// <remarks>
    /// <para>When you call the [**ChooseFont**](/windows/win32/api/commdlg/ns-commdlg-choosefonta) function to create a **Font** dialog box, you can use the **lpLogFont** member of the [**CHOOSEFONT**](/windows/win32/api/commdlg/ns-commdlg-choosefonta) structure to specify a [**LOGFONT**](/windows/win32/api/wingdi/ns-wingdi-logfonta) structure containing initial values for the dialog box. Use the **WM\_CHOOSEFONT\_SETLOGFONT** message to specify a **LOGFONT** structure with different values while the **Font** dialog box is open. Typically, you would send the **WM\_CHOOSEFONT\_SETLOGFONT** message from a [**CFHookProc**](/windows/win32/api/commdlg/nc-commdlg-lpcfhookproc) hook procedure. The hook procedure can also send the [**WM\_CHOOSEFONT\_GETLOGFONT**](wm-choosefont-getlogfont.md) and [**WM\_CHOOSEFONT\_SETFLAGS**](wm-choosefont-setflags.md) messages.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/dlgbox/wm-choosefont-setlogfont#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_CHOOSEFONT_SETLOGFONT = 1125;

    /// <summary>An application sends the WM\_CHOOSEFONT\_SETFLAGS message to a Font dialog box to set the display options for the dialog box.</summary>
    /// <returns>No return value.</returns>
    /// <remarks>
    /// <para>The [**ChooseFont**](/windows/win32/api/commdlg/ns-commdlg-choosefonta) function creates a **Font** dialog box and uses a [**CHOOSEFONT**](/windows/win32/api/commdlg/ns-commdlg-choosefonta) structure to specify the initial values for the **Flags** member. Use the **WM\_CHOOSEFONT\_SETFLAGS** message to specify different values for the **Flags** member while the **Font** dialog box is open. Typically, you should send the **WM\_CHOOSEFONT\_SETFLAGS** message from a [**CFHookProc**](/windows/win32/api/commdlg/nc-commdlg-lpcfhookproc) hook procedure.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/dlgbox/wm-choosefont-setflags#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_CHOOSEFONT_SETFLAGS = 1126;

    /// <summary>Notifies a PagePaintHook hook procedure of the coordinates of the sample page rectangle in the Page Setup dialog box. The dialog box sends this message when it is about to draw the contents of the sample page.</summary>
    /// <returns>
    /// <para>If the hook procedure returns **TRUE**, the dialog box sends no more messages and does not draw in the sample page until the next time the system needs to redraw the sample page. If the hook procedure returns **FALSE**, the dialog box sends the remaining messages of the drawing sequence.</para>
    /// </returns>
    /// <remarks>The **Page Setup** dialog box includes an image of a sample page that shows how the user's selections affect the appearance of the printed output. When you call the [**PageSetupDlg**](/previous-versions/windows/desktop/legacy/ms646937(v=vs.85)) function, you can provide a [*PagePaintHook*](/windows/win32/api/commdlg/nc-commdlg-lppagepainthook) hook procedure to customize the appearance of the sample page. Whenever the dialog box is about to draw the contents of the sample page, the dialog box sends a sequence of messages to the hook procedure.</remarks>
    public const int WM_PSD_FULLPAGERECT = 1025;

    /// <summary>Notifies a PagePaintHook hook procedure of the coordinates of the margin rectangle in the sample page. A Page Setup dialog box sends this message when it is about to draw the contents of the sample page.</summary>
    /// <returns>
    /// <para>If the hook procedure returns **TRUE**, the dialog box sends no more messages and does not draw in the sample page until the next time the system needs to redraw the sample page. If the hook procedure returns **FALSE**, the dialog box sends the remaining messages of the drawing sequence.</para>
    /// </returns>
    /// <remarks>The **Page Setup** dialog box includes an image of a sample page that shows how the user's selections affect the appearance of the printed output. When you call the [**PageSetupDlg**](/previous-versions/windows/desktop/legacy/ms646937(v=vs.85)) function, you can provide a [*PagePaintHook*](/windows/win32/api/commdlg/nc-commdlg-lppagepainthook) hook procedure to customize the appearance of the sample page. Whenever the dialog box is about to draw the contents of the sample page, the dialog box sends a sequence of messages to the hook procedure.</remarks>
    public const int WM_PSD_MINMARGINRECT = 1026;

    /// <summary>Notifies the hook procedure of a Page Setup dialog box, PagePaintHook, that the dialog box is about to draw the margin rectangle of the sample page.</summary>
    /// <returns>
    /// <para>If the hook procedure returns **TRUE**, the dialog box does not draw the margin rectangle in the sample page. If the hook procedure returns **FALSE**, the dialog box draws the margin rectangle in the sample page.</para>
    /// </returns>
    /// <remarks>The **Page Setup** dialog box includes an image of a sample page that shows how the user's selections affect the appearance of the printed output. When you call the [**PageSetupDlg**](/previous-versions/windows/desktop/legacy/ms646937(v=vs.85)) function, you can provide a [*PagePaintHook*](/windows/win32/api/commdlg/nc-commdlg-lppagepainthook) hook procedure to customize the appearance of the sample page. Whenever the dialog box is about to draw the contents of the sample page, the dialog box sends a sequence of messages to the hook procedure.</remarks>
    public const int WM_PSD_MARGINRECT = 1027;

    /// <summary>Notifies the hook procedure of a Page Setup dialog box, PagePaintHook, that the dialog box is about to draw Greek text inside the margin rectangle of the sample page.</summary>
    /// <returns>
    /// <para>If the hook procedure returns **TRUE**, the dialog box does not draw the Greek text portion of the sample page. If the hook procedure returns **FALSE**, the dialog box draws the Greek text portion of the sample page.</para>
    /// </returns>
    /// <remarks>The **Page Setup** dialog box includes an image of a sample page that shows how the user's selections affect the appearance of the printed output. When you call the [**PageSetupDlg**](/previous-versions/windows/desktop/legacy/ms646937(v=vs.85)) function, you can provide a [*PagePaintHook*](/windows/win32/api/commdlg/nc-commdlg-lppagepainthook) hook procedure to customize the appearance of the sample page. Whenever the dialog box is about to draw the contents of the sample page, the dialog box sends a sequence of messages to the hook procedure.</remarks>
    public const int WM_PSD_GREEKTEXTRECT = 1028;

    /// <summary>Notifies the hook procedure of a Page Setup dialog box, PagePaintHook, that the dialog box is about to draw the envelope-stamp rectangle of the sample page.</summary>
    /// <returns>
    /// <para>If the hook procedure returns **TRUE**, the dialog box does not draw the envelope-stamp portion of the sample page. If the hook procedure returns **FALSE**, the dialog box draws the envelope-stamp portion of the sample page.</para>
    /// </returns>
    /// <remarks>
    /// <para>The **Page Setup** dialog box includes an image of a sample page that shows how the user's selections affect the appearance of the printed output. When you call the [**PageSetupDlg**](/previous-versions/windows/desktop/legacy/ms646937(v=vs.85)) function, you can provide a [*PagePaintHook*](/windows/win32/api/commdlg/nc-commdlg-lppagepainthook) hook procedure to customize the appearance of the sample page. Whenever the dialog box is about to draw the contents of the sample page, the dialog box sends a sequence of messages to the hook procedure. A hook procedure receives this message only if the selected paper type is an envelope.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/dlgbox/wm-psd-envstamprect#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_PSD_ENVSTAMPRECT = 1029;

    /// <summary>Notifies the hook procedure of a Page Setup dialog box, PagePaintHook, that the dialog box is about to draw the return address portion of an envelope sample page.</summary>
    /// <returns>
    /// <para>If the hook procedure returns **TRUE**, the dialog box does not draw the return address portion of an envelope sample page. If the hook procedure returns **FALSE**, the dialog box draws the return address portion of an envelope sample page. If the paper type is not an envelope, the return value has no effect.</para>
    /// </returns>
    /// <remarks>The **Page Setup** dialog box includes an image of a sample page that shows how the user's selections affect the appearance of the printed output. When you call the [**PageSetupDlg**](/previous-versions/windows/desktop/legacy/ms646937(v=vs.85)) function, you can provide a [*PagePaintHook*](/windows/win32/api/commdlg/nc-commdlg-lppagepainthook) hook procedure to customize the appearance of the sample page. Whenever the dialog box is about to draw the contents of the sample page, the dialog box sends a sequence of messages to the hook procedure.</remarks>
    public const int WM_PSD_YAFULLPAGERECT = 1030;

    /// <summary>WM_CPL_LAUNCH message - This message is not supported.</summary>
    /// <remarks>
    /// <para><see href="https://learn.microsoft.com/windows/win32/shell/wm-cpl-launch">Learn more about this API from docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_CPL_LAUNCH = 2024;

    /// <summary>WM_CPL_LAUNCHED message - This message is not supported.</summary>
    /// <remarks>
    /// <para><see href="https://learn.microsoft.com/windows/win32/shell/wm-cpl-launched">Learn more about this API from docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_CPL_LAUNCHED = 2025;

    public const int WM_TABLET_DEFBASE = 704;

    public const int WM_TABLET_MAXOFFSET = 32;

    /// <summary>The WM\_TABLET\_ADDED message is posted when a tablet device is added to Windows.</summary>
    /// <remarks>
    /// <para>This message is sent to all top-level windows in the system, including disabled or invisible unowned windows, overlapped windows, and pop-up windows; but the message is not sent to child windows. The indexes passed in *wParam* are related to the index used by the [**ITabletManager::GetTablet**](/previous-versions/windows/desktop/legacy/aa373683(v=vs.85)) method.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/tablet/wm-tablet-added#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_TABLET_ADDED = 712;

    /// <summary>The WM\_TABLET\_DELETED message is posted when a tablet device is removed from Windows.</summary>
    /// <remarks>
    /// <para>This message is sent to all top-level windows in the system, including disabled or invisible unowned windows, overlapped windows, and pop-up windows; but the message is not sent to child windows. The indexes passed in *wParam* are related to the index used by the [**ITabletManager::GetTablet**](/previous-versions/windows/desktop/legacy/aa373683(v=vs.85)) method.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/tablet/wm-tablet-deleted#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_TABLET_DELETED = 713;

    /// <summary>Sent when a user performs a pen flick. A window receives this message through its WindowProc function.</summary>
    /// <remarks>
    /// <para>A pen flick is a unidirectional pen gesture that requires the user to contact the digitizer in a quick, straight flicking motion. A flick is characterized by high speed and a high degree of straightness. A flick is identified by its direction. Flicks can be made in eight directions corresponding to the cardinal and secondary compass directions. When a pen flick occurs, Windows first notifies an application by sending a **WM\_TABLET\_FLICK** message, which a window receives through its [*WindowProc*](/previous-versions/windows/desktop/legacy/ms633573(v=vs.85)) function. Return the **FLICK\_WM\_HANDLED\_MASK** constant, described in [Flicks Constants](flicks-constants.md), to indicate that the application responded to the **WM\_TABLET\_FLICK** message. If the application does not return **FLICK\_WM\_HANDLED\_MASK**, Windows performs the default action specified in the flicks control panel by sending a follow-up notification, such as [**WM\_APPCOMMAND**](../inputdev/wm-appcommand.md), [**WM\_VSCROLL**](../controls/wm-vscroll.md), or [**WM\_KEYDOWN**](../inputdev/wm-keydown.md), depending on which action is associated with the pen flick. Use caution when handling the **WM\_TABLET\_FLICK** message. **WM\_TABLET\_FLICK** is passed via the [**SendMessageTimeout**](/windows/win32/api/winuser/nf-winuser-sendmessagetimeouta) function. If you call methods on a COM interface, that object must be within the same process. If not, COM throws an exception.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/tablet/wm-tablet-flick-message#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_TABLET_FLICK = 715;

    /// <summary>Sent when the system asks a window which system gestures it would like to receive.</summary>
    /// <remarks>
    /// <para>By handling this message, you can dynamically disable flicks for regions of a window. > [!Note] > The *lParam* can be converted to x-coordinates and y-coordinates by using the `GET_X_LPARAM` and `GET_Y_LPARAM` macros.</para>
    /// <para>By default, your window will receive all system gesture events. You can choose which events you would like your window to receive and which events you would like disabled by responding to the **WM\_TABLET\_QUERYSYSTEMGESTURESTATUS** message in your **WndProc**. The **WM\_TABLET\_QUERYSYSTEMGESTURESTATUS** message is defined in tpcshrd.h. The values to enable and disable system tablet system gestures are also defined in tpcshrd.h as follows: </para>
    /// <para>This doc was truncated.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/tablet/wm-tablet-querysystemgesturestatus-message#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_TABLET_QUERYSYSTEMGESTURESTATUS = 716;

    /// <summary>Notifies a window that the user clicked the right mouse button (right-clicked) in the window.</summary>
    /// <returns>No return value.</returns>
    /// <remarks>
    /// <para>A window can process this message by displaying a shortcut menu using the [**TrackPopupMenu**](/windows/desktop/api/Winuser/nf-winuser-trackpopupmenu) or [**TrackPopupMenuEx**](/windows/desktop/api/Winuser/nf-winuser-trackpopupmenuex) functions. To obtain the horizontal and vertical positions, use the following code.</para>
    /// <para></para>
    /// <para>This doc was truncated.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/menurc/wm-contextmenu#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_CONTEXTMENU = 123;

    /// <summary>The WM\_UNICHAR message can be used by an application to post input to other windows.</summary>
    /// <returns>An application should return zero if it processes this message.</returns>
    /// <remarks>
    /// <para>The **WM\_UNICHAR** message is similar to [**WM\_CHAR**](wm-char.md), but it uses Unicode Transformation Format (UTF)-32, whereas **WM\_CHAR** uses UTF-16. This message is designed to send or post Unicode characters to ANSI windows and can handle Unicode Supplementary Plane characters. Because there is not necessarily a one-to-one correspondence between keys pressed and character messages generated, the information in the high-order word of the *lParam* parameter is generally not useful to applications. The information in the high-order word applies only to the most recent [**WM\_KEYDOWN**](wm-keydown.md) message that precedes the posting of the **WM\_UNICHAR** message. For enhanced 101- and 102-key keyboards, extended keys are the right ALT and the right CTRL keys on the main section of the keyboard; the INS, DEL, HOME, END, PAGE UP, PAGE DOWN and arrow keys in the clusters to the left of the numeric keypad; and the divide (/) and ENTER keys in the numeric keypad. Some other keyboards may support the extended-key bit in the *lParam* parameter.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/inputdev/wm-unichar#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_UNICHAR = 265;

    /// <summary>The WM\_PRINTCLIENT message is sent to a window to request that it draw its client area in the specified device context, most commonly in a printer device context.</summary>
    /// <remarks>
    /// <para>A window can process this message in much the same manner as [**WM\_PAINT**](./wm-paint.md), except that [**BeginPaint**](/windows/desktop/api/Winuser/nf-winuser-beginpaint) and [**EndPaint**](/windows/desktop/api/Winuser/nf-winuser-endpaint) need not be called (a device context is provided), and the window should draw its entire client area rather than just the invalid region. Windows that can be used anywhere in the system, such as controls, should process this message. It is probably worthwhile for other windows to process this message as well because it is relatively easy to implement. The [AnimateWindow](/windows/desktop/api/winuser/nf-winuser-animatewindow) function requires that the window being animated implements the **WM\_PRINTCLIENT** message.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/gdi/wm-printclient#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_PRINTCLIENT = 792;

    /// <summary>Sent by a common control to its parent window when an event has occurred or the control requires some information.</summary>
    /// <returns>The return value is ignored except for notification messages that specify otherwise.</returns>
    /// <remarks>
    /// <para>The destination of the message must be the **HWND** of the parent of the control. This value can be obtained by using [**GetParent**](/windows/desktop/api/winuser/nf-winuser-getparent), as shown in the following example, where *m\_controlHwnd* is the **HWND** of the control itself.</para>
    /// <para></para>
    /// <para>This doc was truncated.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/Controls/wm-notify#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_NOTIFY = 78;

    /// <summary>Notifies an application of a change to the hardware configuration of a device or the computer.</summary>
    /// <returns>
    /// <para>Return **TRUE** to grant the request. Return **BROADCAST\_QUERY\_DENY** to deny the request.</para>
    /// </returns>
    /// <remarks>For devices that offer software-controllable features, such as ejection and locking, the system typically sends a [DBT\_DEVICEREMOVEPENDING](dbt-deviceremovepending.md) message to let applications and device drivers end their use of the device gracefully. If the system forcibly removes a device, it may not send a [DBT\_DEVICEQUERYREMOVE](dbt-devicequeryremove.md) message before doing so.</remarks>
    public const int WM_DEVICECHANGE = 537;

    /// <summary>Performs no operation. An application sends the WM\_NULL message if it wants to post a message that the recipient window will ignore.</summary>
    /// <returns>
    /// <para>Type: **LRESULT** An application returns zero if it processes this message.</para>
    /// </returns>
    /// <remarks>
    /// <para>For example, if an application has installed a **WH\_GETMESSAGE** hook and wants to prevent a message from being processed, the [**GetMsgProc**](/previous-versions/windows/desktop/legacy/ms644981(v=vs.85)) callback function can change the message number to **WM\_NULL** so the recipient will ignore it. As another example, an application can check if a window is responding to messages by sending the **WM\_NULL** message with the [**SendMessageTimeout**](/windows/win32/api/winuser/nf-winuser-sendmessagetimeouta) function.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/winmsg/wm-null#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_NULL = 0;

    /// <summary>Sent when an application requests that a window be created by calling the CreateWindowEx or CreateWindow function.</summary>
    /// <returns>
    /// <para>Type: **LRESULT** If an application processes this message, it should return zero to continue creation of the window. If the application returns –1, the window is destroyed and the [**CreateWindowEx**](/windows/win32/api/winuser/nf-winuser-createwindowexa) or [**CreateWindow**](/windows/win32/api/winuser/nf-winuser-createwindowa) function returns a **NULL** handle.</para>
    /// </returns>
    /// <remarks>
    /// <para><see href="https://learn.microsoft.com/windows/win32/winmsg/wm-create">Learn more about this API from docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_CREATE = 1;

    /// <summary>Sent when a window is being destroyed. It is sent to the window procedure of the window being destroyed after the window is removed from the screen.</summary>
    /// <returns>
    /// <para>Type: **LRESULT** If an application processes this message, it should return zero.</para>
    /// </returns>
    /// <remarks>If the window being destroyed is part of the clipboard viewer chain (set by calling the [**SetClipboardViewer**](/windows/win32/api/winuser/nf-winuser-setclipboardviewer) function), the window must remove itself from the chain by processing the [**ChangeClipboardChain**](/windows/win32/api/winuser/nf-winuser-changeclipboardchain) function before returning from the **WM\_DESTROY** message.</remarks>
    public const int WM_DESTROY = 2;

    /// <summary>Sent after a window has been moved.</summary>
    /// <returns>
    /// <para>Type: **LRESULT** If an application processes this message, it should return zero.</para>
    /// </returns>
    /// <remarks>
    /// <para>The parameters are given in screen coordinates for overlapped and pop-up windows and in parent-client coordinates for child windows. The following example demonstrates how to obtain the position from the *lParam* parameter.</para>
    /// <para></para>
    /// <para>This doc was truncated.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/winmsg/wm-move#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_MOVE = 3;

    /// <summary>Sent to a window after its size has changed.</summary>
    /// <returns>
    /// <para>Type: **LRESULT** If an application processes this message, it should return zero.</para>
    /// </returns>
    /// <remarks>
    /// <para>If the [**SetScrollPos**](https://msdn.microsoft.com/library/Cc411085(v=MSDN.10).aspx) or [**MoveWindow**](/windows/win32/api/winuser/nf-winuser-movewindow) function is called for a child window as a result of the **WM\_SIZE** message, the *bRedraw* or *bRepaint* parameter should be nonzero to cause the window to be repainted. Although the width and height of a window are 32-bit values, the *lParam* parameter contains only the low-order 16 bits of each. The [**DefWindowProc**](/windows/desktop/api/winuser/nf-winuser-defwindowproca) function sends the **WM\_SIZE** and **WM\_MOVE** messages when it processes the [**WM\_WINDOWPOSCHANGED**](wm-windowposchanged.md) message. The **WM\_SIZE** and **WM\_MOVE** messages are not sent if an application handles the **WM\_WINDOWPOSCHANGED** message without calling **DefWindowProc**.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/winmsg/wm-size#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_SIZE = 5;

    /// <summary>Sent to both the window being activated and the window being deactivated.</summary>
    /// <returns>If an application processes this message, it should return zero.</returns>
    /// <remarks>If the window is being activated and is not minimized, the [**DefWindowProc**](/windows/desktop/api/winuser/nf-winuser-defwindowproca) function sets the keyboard focus to the window. If the window is activated by a mouse click, it also receives a [**WM\_MOUSEACTIVATE**](wm-mouseactivate.md) message.</remarks>
    public const int WM_ACTIVATE = 6;

    /// <summary>Sent to a window after it has gained the keyboard focus.</summary>
    /// <returns>An application should return zero if it processes this message.</returns>
    /// <remarks>To display a caret, an application should call the appropriate caret functions when it receives the **WM\_SETFOCUS** message.</remarks>
    public const int WM_SETFOCUS = 7;

    /// <summary>Sent to a window immediately before it loses the keyboard focus.</summary>
    /// <returns>An application should return zero if it processes this message.</returns>
    /// <remarks>
    /// <para>If an application is displaying a caret, the caret should be destroyed at this point. While processing this message, do not make any function calls that display or activate a window. This causes the thread to yield control and can cause the application to stop responding to messages. For more information, see [Message Deadlocks](/windows/desktop/winmsg/about-messages-and-message-queues).</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/inputdev/wm-killfocus#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_KILLFOCUS = 8;

    /// <summary>Sent when an application changes the enabled state of a window.</summary>
    /// <returns>
    /// <para>Type: **LRESULT** If an application processes this message, it should return zero.</para>
    /// </returns>
    /// <remarks>
    /// <para><see href="https://learn.microsoft.com/windows/win32/winmsg/wm-enable">Learn more about this API from docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_ENABLE = 10;

    /// <summary>You send the **WM_SETREDRAW** message to a window to allow changes in that window to be redrawn, or to prevent changes in that window from being redrawn.</summary>
    /// <returns>Your application should return 0 if it processes this message.</returns>
    /// <remarks>
    /// <para>This message can be useful if your application must add several items to a list box. Your application can call this message with *wParam* set to **FALSE**, add the items, and then call the message again with *wParam* set to **TRUE**. Finally, your application can call [**RedrawWindow**](/windows/win32/api/Winuser/nf-winuser-redrawwindow)(*hWnd*, **NULL**, **NULL**, RDW\_ERASE \| RDW\_FRAME \| RDW\_INVALIDATE \| RDW\_ALLCHILDREN) to cause the list box to be repainted. > [!NOTE] > You should use [**RedrawWindow**](/windows/win32/api/Winuser/nf-winuser-redrawwindow) with the specified flags, instead of [**InvalidateRect**](/windows/win32/api/Winuser/nf-winuser-invalidaterect), because the former is necessary for some controls that have nonclient area of their own, or have window styles that cause them to be given a nonclient area (such as **WS_THICKFRAME**, **WS_BORDER**, or **WS_EX_CLIENTEDGE**). If the control does not have a nonclient area, then **RedrawWindow** with these flags will do only as much invalidation as **InvalidateRect** would. Passing a **WM_SETREDRAW** message to the **DefWindowProc** function removes the **WS_VISIBLE** style from the window when *wParam* is set to **FALSE**. Although the window content remains visible on screen, the [**IsWindowVisible**](/windows/win32/api/winuser/nf-winuser-iswindowvisible) function returns **FALSE** when called on a window in this state. Passing a **WM_SETREDRAW** message to the **DefWindowProc** function adds the **WS_VISIBLE** style to the window, if not set, when *wParam* is set to **TRUE**. If your application sends the **WM_SETREDRAW** message with *wParam* set to **TRUE** to a hidden window, then the window becomes visible. **Windows 10 and later; Windows Server 2016 and later**. The system sets a property named *SysSetRedraw* on a window whose window procedure passes **WM_SETREDRAW** messages to **DefWindowProc**. You can use the [**GetProp**](/windows/win32/api/Winuser/nf-winuser-getpropa) function to get the property value when it's available. **GetProp** returns a non-zero value when redraw is disabled. **GetProp** will return zero when redraw is enabled, or when the window property doesn't exist.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/gdi/wm-setredraw#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_SETREDRAW = 11;

    /// <summary>Sets the text of a window.</summary>
    /// <returns>
    /// <para>Type: **LRESULT** The return value is **TRUE** if the text is set. It is **FALSE** (for an edit control), **LB\_ERRSPACE** (for a list box), or **CB\_ERRSPACE** (for a combo box) if insufficient space is available to set the text in the edit control. It is **CB\_ERR** if this message is sent to a combo box without an edit control.</para>
    /// </returns>
    /// <remarks>
    /// <para>The [**DefWindowProc**](/windows/desktop/api/winuser/nf-winuser-defwindowproca) function sets and displays the window text. For an edit control, the text is the contents of the edit control. For a combo box, the text is the contents of the edit-control portion of the combo box. For a button, the text is the button name. For other windows, the text is the window title. This message does not change the current selection in the list box of a combo box. An application should use the [**CB\_SELECTSTRING**](../controls/cb-selectstring.md) message to select the item in a list box that matches the text in the edit control.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/winmsg/wm-settext#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_SETTEXT = 12;

    /// <summary>Copies the text that corresponds to a window into a buffer provided by the caller.</summary>
    /// <returns>
    /// <para>Type: **LRESULT** The return value is the number of characters copied, not including the terminating null character.</para>
    /// </returns>
    /// <remarks>
    /// <para>The [**DefWindowProc**](/windows/desktop/api/winuser/nf-winuser-defwindowproca) function copies the text associated with the window into the specified buffer and returns the number of characters copied. Note, for non-text static controls this gives you the text with which the control was originally created, that is, the ID number. However, it gives you the ID of the non-text static control as originally created. That is, if you subsequently used a **STM\_SETIMAGE** to change it the original ID would still be returned. For an edit control, the text to be copied is the content of the edit control. For a combo box, the text is the content of the edit control (or static-text) portion of the combo box. For a button, the text is the button name. For other windows, the text is the window title. To copy the text of an item in a list box, an application can use the [**LB\_GETTEXT**](../controls/lb-gettext.md) message. When the **WM\_GETTEXT** message is sent to a static control with the **SS\_ICON** style, a handle to the icon will be returned in the first four bytes of the buffer pointed to by *lParam*. This is true only if the [**WM\_SETTEXT**](wm-settext.md) message has been used to set the icon. **Rich Edit:** If the text to be copied exceeds 64K, use either the [**EM\_STREAMOUT**](../controls/em-streamout.md) or [**EM\_GETSELTEXT**](../controls/em-getseltext.md) message. Sending a **WM\_GETTEXT** message to a non-text static control, such as a static bitmap or static icon control, does not return a string value. Instead, it returns zero. In addition, in early versions of Windows, applications could send a **WM\_GETTEXT** message to a non-text static control to retrieve the control's ID. To retrieve a control's ID, applications can use [**GetWindowLong**](/windows/win32/api/winuser/nf-winuser-getwindowlonga) passing **GWL\_ID** as the index value or [**GetWindowLongPtr**](/windows/win32/api/winuser/nf-winuser-getwindowlongptra) using **GWLP\_ID**.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/winmsg/wm-gettext#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_GETTEXT = 13;

    /// <summary>Determines the length, in characters, of the text associated with a window.</summary>
    /// <returns>
    /// <para>Type: **LRESULT** The return value is the length of the text in characters, not including the terminating null character.</para>
    /// </returns>
    /// <remarks>
    /// <para>For an edit control, the text to be copied is the content of the edit control. For a combo box, the text is the content of the edit control (or static-text) portion of the combo box. For a button, the text is the button name. For other windows, the text is the window title. To determine the length of an item in a list box, an application can use the [**LB\_GETTEXTLEN**](../controls/lb-gettextlen.md) message. When the **WM\_GETTEXTLENGTH** message is sent, the [**DefWindowProc**](/windows/desktop/api/winuser/nf-winuser-defwindowproca) function returns the length, in characters, of the text. Under certain conditions, the **DefWindowProc** function returns a value that is larger than the actual length of the text. This occurs with certain mixtures of ANSI and Unicode, and is due to the system allowing for the possible existence of double-byte character set (DBCS) characters within the text. The return value, however, will always be at least as large as the actual length of the text; you can thus always use it to guide buffer allocation. This behavior can occur when an application uses both ANSI functions and common dialogs, which use Unicode. To obtain the exact length of the text, use the [**WM\_GETTEXT**](wm-gettext.md), [**LB\_GETTEXT**](../controls/lb-gettext.md), or [**CB\_GETLBTEXT**](../controls/cb-getlbtext.md) messages, or the [**GetWindowText**](/windows/win32/api/winuser/nf-winuser-getwindowtexta) function. Sending a **WM\_GETTEXTLENGTH** message to a non-text static control, such as a static bitmap or static icon controlc, does not return a string value. Instead, it returns zero.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/winmsg/wm-gettextlength#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_GETTEXTLENGTH = 14;

    /// <summary>The WM\_PAINT message is sent when the system or another application makes a request to paint a portion of an application's window.</summary>
    /// <returns>An application returns zero if it processes this message.</returns>
    /// <remarks>
    /// <para>The **WM\_PAINT** message is generated by the system and should not be sent by an application. To force a window to draw into a specific device context, use the [**WM\_PRINT**](wm-print.md) or [**WM\_PRINTCLIENT**](wm-printclient.md) message. Note that this requires the target window to support the **WM\_PRINTCLIENT** message. Most common controls support the **WM\_PRINTCLIENT** message. The [**DefWindowProc**](/windows/desktop/api/winuser/nf-winuser-defwindowproca)  function validates the update region. The function may also send the [**WM\_NCPAINT**](wm-ncpaint.md) message to the window procedure if the window frame must be painted and send the [**WM\_ERASEBKGND**](../winmsg/wm-erasebkgnd.md) message if the window background must be erased. The system sends this message when there are no other messages in the application's message queue. [**DispatchMessage**](/windows/win32/api/winuser/nf-winuser-dispatchmessage) determines where to send the message; [**GetMessage**](/windows/win32/api/winuser/nf-winuser-getmessage) determines which message to dispatch. **GetMessage** returns the **WM\_PAINT** message when there are no other messages in the application's message queue, and **DispatchMessage** sends the message to the appropriate window procedure. A window may receive internal paint messages as a result of calling [**RedrawWindow**](/windows/desktop/api/Winuser/nf-winuser-redrawwindow) with the RDW\_INTERNALPAINT flag set. In this case, the window may not have an update region. An application may call the [**GetUpdateRect**](/windows/desktop/api/Winuser/nf-winuser-getupdaterect) function to determine whether the window has an update region. If **GetUpdateRect** returns zero, the application need not call the [**BeginPaint**](/windows/desktop/api/Winuser/nf-winuser-beginpaint) and [**EndPaint**](/windows/desktop/api/Winuser/nf-winuser-endpaint) functions. An application must check for any necessary internal painting by looking at its internal data structures for each **WM\_PAINT** message, because a **WM\_PAINT** message may have been caused by both a non-NULL update region and a call to [**RedrawWindow**](/windows/desktop/api/Winuser/nf-winuser-redrawwindow) with the RDW\_INTERNALPAINT flag set. The system sends an internal **WM\_PAINT** message only once. After an internal **WM\_PAINT** message is returned from [**GetMessage**](/windows/win32/api/winuser/nf-winuser-getmessage) or [**PeekMessage**](/windows/win32/api/winuser/nf-winuser-peekmessagea) or is sent to a window by [**UpdateWindow**](/windows/desktop/api/Winuser/nf-winuser-updatewindow), the system does not post or send further **WM\_PAINT** messages until the window is invalidated or until [**RedrawWindow**](/windows/desktop/api/Winuser/nf-winuser-redrawwindow) is called again with the RDW\_INTERNALPAINT flag set. For some common controls, the default **WM\_PAINT** message processing checks the *wParam* parameter. If *wParam* is non-NULL, the control assumes that the value is an HDC and paints using that device context.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/gdi/wm-paint#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_PAINT = 15;

    /// <summary>Sent as a signal that a window or an application should terminate.</summary>
    /// <returns>
    /// <para>Type: **LRESULT** If an application processes this message, it should return zero.</para>
    /// </returns>
    /// <remarks>
    /// <para>An application can prompt the user for confirmation, prior to destroying a window, by processing the **WM\_CLOSE** message and calling the [**DestroyWindow**](/windows/win32/api/winuser/nf-winuser-destroywindow) function only if the user confirms the choice. By default, the [**DefWindowProc**](/windows/desktop/api/winuser/nf-winuser-defwindowproca) function calls the [**DestroyWindow**](/windows/win32/api/winuser/nf-winuser-destroywindow) function to destroy the window.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/winmsg/wm-close#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_CLOSE = 16;

    /// <summary>The WM\_QUERYENDSESSION message is sent when the user chooses to end the session or when an application calls one of the system shutdown functions.</summary>
    /// <returns>
    /// <para>Applications should respect the user's intentions and return **TRUE**. By default, the [**DefWindowProc**](/windows/desktop/api/winuser/nf-winuser-defwindowproca) function returns **TRUE** for this message. If shutting down would corrupt the system or media that is being burned, the application can return **FALSE**. However, it is good practice to respect the user's actions.</para>
    /// </returns>
    /// <remarks>
    /// <para>When an application returns **TRUE** for this message, it receives the [**WM\_ENDSESSION**](wm-endsession.md) message, regardless of how the other applications respond to the **WM\_QUERYENDSESSION** message. Each application should return **TRUE** or **FALSE** immediately upon receiving this message, and defer any cleanup operations until it receives the **WM\_ENDSESSION** message. Applications can display a user interface prompting the user for information at shutdown, however it is not recommended. After five seconds, the system displays information about the applications that are preventing shutdown and allows the user to terminate them. For example, Windows XP displays a dialog box, while Windows Vista displays a full screen with additional information about the applications blocking shutdown. If your application must block or postpone system shutdown, use the [**ShutdownBlockReasonCreate**](/windows/desktop/api/Winuser/nf-winuser-shutdownblockreasoncreate) function. For more information, see [Shutdown Changes for Windows Vista](shutdown-changes-for-windows-vista.md). Console applications can use the [**SetConsoleCtrlHandler**](/windows/console/setconsolectrlhandler) function to receive shutdown notification. Service applications can use the [**RegisterServiceCtrlHandlerEx**](/windows/win32/api/winsvc/nf-winsvc-registerservicectrlhandlerexa) function to receive shutdown notifications in a handler routine.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/Shutdown/wm-queryendsession#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_QUERYENDSESSION = 17;

    /// <summary>Sent to an icon when the user requests that the window be restored to its previous size and position.</summary>
    /// <returns>
    /// <para>Type: **LRESULT** If the icon can be opened, an application that processes this message should return **TRUE**; otherwise, it should return **FALSE** to prevent the icon from being opened.</para>
    /// </returns>
    /// <remarks>
    /// <para>By default, the [**DefWindowProc**](/windows/desktop/api/winuser/nf-winuser-defwindowproca) function returns **TRUE**. While processing this message, the application should not perform any action that would cause an activation or focus change (for example, creating a dialog box).</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/winmsg/wm-queryopen#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_QUERYOPEN = 19;

    /// <summary>The WM\_ENDSESSION message is sent to an application after the system processes the results of the WM\_QUERYENDSESSION message. The WM\_ENDSESSION message informs the application whether the session is ending.</summary>
    /// <returns>If an application processes this message, it should return zero.</returns>
    /// <remarks>
    /// <para>Applications that have unsaved data could save the data to a temporary location and restore it the next time the application starts. It is recommended that applications save their data and state frequently; for example, automatically save data between save operations initiated by the user to reduce the amount of data to be saved at shutdown. The application need not call the [**DestroyWindow**](/windows/win32/api/winuser/nf-winuser-destroywindow) or [**PostQuitMessage**](/windows/win32/api/winuser/nf-winuser-postquitmessage) function when the session is ending.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/Shutdown/wm-endsession#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_ENDSESSION = 22;

    /// <summary>Indicates a request to terminate an application, and is generated when the application calls the PostQuitMessage function. This message causes the GetMessage function to return zero.</summary>
    /// <returns>
    /// <para>Type: **LRESULT** This message does not have a return value because it causes the message loop to terminate before the message is sent to the application's window procedure.</para>
    /// </returns>
    /// <remarks>
    /// <para>The **WM\_QUIT** message is not associated with a window and therefore will never be received through a window's window procedure. It is retrieved only by the [**GetMessage**](/windows/win32/api/winuser/nf-winuser-getmessage) or [**PeekMessage**](/windows/win32/api/winuser/nf-winuser-peekmessagea) functions. Do not post the **WM\_QUIT** message using the [**PostMessage**](/windows/win32/api/winuser/nf-winuser-postmessagea) function; use [**PostQuitMessage**](/windows/win32/api/winuser/nf-winuser-postquitmessage).</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/winmsg/wm-quit#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_QUIT = 18;

    /// <summary>Sent when the window background must be erased (for example, when a window is resized). The message is sent to prepare an invalidated portion of a window for painting.</summary>
    /// <returns>
    /// <para>Type: **LRESULT** An application should return nonzero if it erases the background; otherwise, it should return zero.</para>
    /// </returns>
    /// <remarks>
    /// <para>The [**DefWindowProc**](/windows/desktop/api/winuser/nf-winuser-defwindowproca) function erases the background by using the class background brush specified by the **hbrBackground** member of the [**WNDCLASS**](/windows/win32/api/winuser/ns-winuser-wndclassa) structure. If **hbrBackground** is **NULL**, the application should process the **WM\_ERASEBKGND** message and erase the background. An application should return nonzero in response to **WM\_ERASEBKGND** if it processes the message and erases the background; this indicates that no further erasing is required. If the application returns zero, the window will remain marked for erasing. (Typically, this indicates that the **fErase** member of the [**PAINTSTRUCT**](/windows/win32/api/winuser/ns-winuser-paintstruct) structure will be **TRUE**.)</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/winmsg/wm-erasebkgnd#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_ERASEBKGND = 20;

    /// <summary>The WM\_SYSCOLORCHANGE message is sent to all top-level windows when a change is made to a system color setting.</summary>
    /// <remarks>
    /// <para>The system sends a [**WM\_PAINT**](wm-paint.md) message to any window that is affected by a system color change. Applications that have brushes using the existing system colors should delete those brushes and re-create them using the new system colors. Top level windows that use common controls must forward the **WM\_SYSCOLORCHANGE** message to the controls; otherwise, the controls will not be notified of the color change. This ensures that the colors used by your common controls are consistent with those used by other user interface objects. For example, a toolbar control uses the "3D Objects" color to draw its buttons. If the user changes the 3D Objects color but the **WM\_SYSCOLORCHANGE** message is not forwarded to the toolbar, the toolbar buttons will remain in their original color while the color of other buttons in the system changes.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/gdi/wm-syscolorchange#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_SYSCOLORCHANGE = 21;

    /// <summary>Sent to a window when the window is about to be hidden or shown.</summary>
    /// <returns>
    /// <para>Type: **LRESULT** If an application processes this message, it should return zero.</para>
    /// </returns>
    /// <remarks>
    /// <para>The [**DefWindowProc**](/windows/desktop/api/winuser/nf-winuser-defwindowproca) function hides or shows the window, as specified by the message. If a window has the [**WS\_VISIBLE**](window-styles.md) style when it is created, the window receives this message after it is created, but before it is displayed. A window also receives this message when its visibility state is changed by the [**ShowWindow**](/windows/win32/api/winuser/nf-winuser-showwindow) or [**ShowOwnedPopups**](/windows/win32/api/winuser/nf-winuser-showownedpopups) function. The **WM\_SHOWWINDOW** message is not sent under the following circumstances: -   When a top-level, overlapped window is created with the [**WS\_MAXIMIZE**](window-styles.md) or **WS\_MINIMIZE** style. -   When the **SW\_SHOWNORMAL** flag is specified in the call to the [**ShowWindow**](/windows/win32/api/winuser/nf-winuser-showwindow) function.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/winmsg/wm-showwindow#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_SHOWWINDOW = 24;

    /// <summary>An application sends the WM\_WININICHANGE message to all top-level windows after making a change to the WIN.INI file. The SystemParametersInfo function sends this message after an application uses the function to change a setting in WIN.INI.</summary>
    /// <returns>
    /// <para>Type: **LRESULT** If you process this message, return zero.</para>
    /// </returns>
    /// <remarks>
    /// <para>To send the **WM\_WININICHANGE** message to all top-level windows, use the [**SendMessage**](/windows/win32/api/winuser/nf-winuser-sendmessage) function with the *hWnd* parameter set to **HWND\_BROADCAST**. Calls to functions that change WIN.INI may be mapped to the registry instead. This mapping occurs when WIN.INI and the section being changed are specified in the registry under the following key: **HKEY\_LOCAL\_MACHINE\\Software\\Microsoft\\Windows NT\\CurrentVersion\\IniFileMapping** The change in the storage location has no effect on the behavior of this message.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/winmsg/wm-wininichange#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_WININICHANGE = 26;

    /// <summary>A message that is sent to all top-level windows when the SystemParametersInfo function changes a system-wide setting or when policy settings have changed.</summary>
    /// <returns>
    /// <para>Type: **LRESULT** If you process this message, return zero.</para>
    /// </returns>
    /// <remarks>The *lParam* parameter indicates which system metric has changed, for example, "ConvertibleSlateMode" if the CONVERTIBLESLATEMODE indicator was toggled or "SystemDockMode" if the DOCKED indicator was toggled.</remarks>
    public const int WM_SETTINGCHANGE = 26;

    /// <summary>The WM\_DEVMODECHANGE message is sent to all top-level windows whenever the user changes device-mode settings.</summary>
    /// <returns>An application should return zero if it processes this message.</returns>
    /// <remarks>This message cannot be sent directly to a window. To send the **WM\_DEVMODECHANGE** message to all top-level windows, use the [**SendMessageTimeout**](/windows/win32/api/winuser/nf-winuser-sendmessagetimeouta) function with the *hWnd* parameter set to HWND\_BROADCAST.</remarks>
    public const int WM_DEVMODECHANGE = 27;

    /// <summary>Sent when a window belonging to a different application than the active window is about to be activated. The message is sent to the application whose window is being activated and to the application whose window is being deactivated.</summary>
    /// <returns>
    /// <para>Type: **LRESULT** If an application processes this message, it should return zero.</para>
    /// </returns>
    /// <remarks>
    /// <para><see href="https://learn.microsoft.com/windows/win32/winmsg/wm-activateapp">Learn more about this API from docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_ACTIVATEAPP = 28;

    /// <summary>An application sends the WM\_FONTCHANGE message to all top-level windows in the system after changing the pool of font resources.</summary>
    /// <remarks>
    /// <para>An application that adds or removes fonts from the system (for example, by using the [**AddFontResource**](/windows/desktop/api/Wingdi/nf-wingdi-addfontresourcea) or [**RemoveFontResource**](/windows/desktop/api/Wingdi/nf-wingdi-removefontresourcea) function) should send this message to all top-level windows. To send the **WM\_FONTCHANGE** message to all top-level windows, an application can call the **SendMessage** function with the *hwnd* parameter set to HWND\_BROADCAST.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/gdi/wm-fontchange#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_FONTCHANGE = 29;

    /// <summary>A message that is sent whenever there is a change in the system time.</summary>
    /// <returns>An application should return zero if it processes this message.</returns>
    /// <remarks>An application should not broadcast this message, because the system will broadcast this message when the application changes the system time.</remarks>
    public const int WM_TIMECHANGE = 30;

    /// <summary>Sent to cancel certain modes, such as mouse capture.</summary>
    /// <returns>
    /// <para>Type: **LRESULT** If an application processes this message, it should return zero.</para>
    /// </returns>
    /// <remarks>When the **WM\_CANCELMODE** message is sent, the [**DefWindowProc**](/windows/desktop/api/winuser/nf-winuser-defwindowproca) function cancels internal processing of standard scroll bar input, cancels internal menu processing, and releases the mouse capture.</remarks>
    public const int WM_CANCELMODE = 31;

    /// <summary>Sent to a window if the mouse causes the cursor to move within a window and mouse input is not captured.</summary>
    /// <returns>If an application processes this message, it should return **TRUE** to halt further processing or **FALSE** to continue.</returns>
    /// <remarks>The [**DefWindowProc**](/windows/desktop/api/winuser/nf-winuser-defwindowprocw) function passes the **WM\_SETCURSOR** message to a parent window before processing. If the parent window returns **TRUE**, further processing is halted. Passing the message to a window's parent window gives the parent window control over the cursor's setting in a child window. The **DefWindowProc** function also uses this message to set the cursor to an arrow if it is not in the client area, or to the registered class cursor if it is in the client area. If the low-order word of the *lParam* parameter is **HTERROR** and the high-order word of *lParam* specifies that one of the mouse buttons is pressed, **DefWindowProc** calls the [**MessageBeep**](/windows/desktop/api/winuser/nf-winuser-messagebeep) function.</remarks>
    public const int WM_SETCURSOR = 32;

    /// <summary>Sent when the cursor is in an inactive window and the user presses a mouse button. The parent window receives this message only if the child window passes it to the DefWindowProc function.</summary>
    /// <returns>
    /// <para>The return value specifies whether the window should be activated and whether the identifier of the mouse message should be discarded. It must be one of the following values.</para>
    /// <para>| Return code/value                                                                                                                                          | Description                                                                      | |------------------------------------------------------------------------------------------------------------------------------------------------------------|----------------------------------------------------------------------------------| | <dl> <dt>**MA\_ACTIVATE**</dt> <dt>1</dt> </dl>         | Activates the window, and does not discard the mouse message.<br/>         | | <dl> <dt>**MA\_ACTIVATEANDEAT**</dt> <dt>2</dt> </dl>   | Activates the window, and discards the mouse message.<br/>                 | | <dl> <dt>**MA\_NOACTIVATE**</dt> <dt>3</dt> </dl>       | Does not activate the window, and does not discard the mouse message.<br/> | | <dl> <dt>**MA\_NOACTIVATEANDEAT**</dt> <dt>4</dt> </dl> | Does not activate the window, but discards the mouse message.<br/>         |</para>
    /// </returns>
    /// <remarks>The [**DefWindowProc**](/windows/desktop/api/winuser/nf-winuser-defwindowproca) function passes the message to a child window's parent window before any processing occurs. The parent window determines whether to activate the child window. If it activates the child window, the parent window should return **MA\_NOACTIVATE** or **MA\_NOACTIVATEANDEAT** to prevent the system from processing the message further.</remarks>
    public const int WM_MOUSEACTIVATE = 33;

    /// <summary>Sent to a child window when the user clicks the window's title bar or when the window is activated, moved, or sized.</summary>
    /// <returns>
    /// <para>Type: **LRESULT** If an application processes this message, it should return zero.</para>
    /// </returns>
    /// <remarks>
    /// <para><see href="https://learn.microsoft.com/windows/win32/winmsg/wm-childactivate">Learn more about this API from docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_CHILDACTIVATE = 34;

    /// <summary>Sent by a computer-based training (CBT) application to separate user-input messages from other messages sent through the WH\_JOURNALPLAYBACK procedure.</summary>
    /// <returns>
    /// <para>Type: **void** A CBT application should return zero if it processes this message.</para>
    /// </returns>
    /// <remarks>
    /// <para>Whenever a CBT application uses the [**WH\_JOURNALPLAYBACK**](about-hooks.md) procedure, the first and last messages are **WM\_QUEUESYNC**. This allows the CBT application to intercept and examine user-initiated messages without doing so for events that it sends. If an application specifies a **NULL** window handle, the message is posted to the message queue of the active window.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/winmsg/wm-queuesync#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_QUEUESYNC = 35;

    /// <summary>Sent to a window when the size or position of the window is about to change. An application can use this message to override the window's default maximized size and position, or its default minimum or maximum tracking size.</summary>
    /// <returns>
    /// <para>Type: **LRESULT** If an application processes this message, it should return zero.</para>
    /// </returns>
    /// <remarks>The maximum tracking size is the largest window size that can be produced by using the borders to size the window. The minimum tracking size is the smallest window size that can be produced by using the borders to size the window.</remarks>
    public const int WM_GETMINMAXINFO = 36;

    public const int WM_PAINTICON = 38;

    public const int WM_ICONERASEBKGND = 39;

    /// <summary>Sent to a dialog box procedure to set the keyboard focus to a different control in the dialog box.</summary>
    /// <returns>An application should return zero if it processes this message.</returns>
    /// <remarks>
    /// <para>This message performs additional dialog box management operations beyond those performed by the [**SetFocus**](/windows/desktop/api/winuser/nf-winuser-setfocus) function **WM\_NEXTDLGCTL** updates the default pushbutton border, sets the default control identifier, and automatically selects the text of an edit control (if the target window is an edit control). Do not use the [**SendMessage**](/windows/desktop/api/winuser/nf-winuser-sendmessage) function to send a **WM\_NEXTDLGCTL** message if your application will concurrently process other messages that set the focus. Use the [**PostMessage**](/windows/desktop/api/winuser/nf-winuser-postmessagea) function instead.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/dlgbox/wm-nextdlgctl#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_NEXTDLGCTL = 40;

    /// <summary>The WM\_SPOOLERSTATUS message is sent from Print Manager whenever a job is added to or removed from the Print Manager queue.</summary>
    /// <returns>An application should return zero if it processes this message.</returns>
    /// <remarks>
    /// <para>This message is for informational purposes only. This message is advisory and does not have guaranteed delivery semantics. Applications should not assume that they will receive a WM\_SPOOLERSTATUS message for every change in spooler status. The WM\_SPOOLERSTATUS message is not supported after Windows XP. To be notified of changes to the print queue status, you can use [**FindFirstPrinterChangeNotification**](findfirstprinterchangenotification.md) and [**FindNextPrinterChangeNotification**](findnextprinterchangenotification.md).</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/printdocs/wm-spoolerstatus#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_SPOOLERSTATUS = 42;

    /// <summary>Sent to the parent window of an owner-drawn button, combo box, list box, or menu when a visual aspect of the button, combo box, list box, or menu has changed.</summary>
    /// <returns>If an application processes this message, it should return **TRUE**.</returns>
    /// <remarks>
    /// <para>By default, the [**DefWindowProc**](/windows/desktop/api/winuser/nf-winuser-defwindowproca) function draws the focus rectangle for an owner-drawn list box item. The *itemAction* member of the [**DRAWITEMSTRUCT**](/windows/win32/api/winuser/ns-winuser-drawitemstruct) structure specifies the drawing operation that an application should perform. Before returning from processing this message, an application should ensure that the device context identified by the *hDC* member of the [**DRAWITEMSTRUCT**](/windows/win32/api/winuser/ns-winuser-drawitemstruct) structure is in the default state.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/Controls/wm-drawitem#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_DRAWITEM = 43;

    /// <summary>Sent to the owner window of a combo box, list box, list-view control, or menu item when the control or menu is created.</summary>
    /// <returns>If an application processes this message, it should return **TRUE**.</returns>
    /// <remarks>
    /// <para>When the owner window receives the **WM\_MEASUREITEM** message, the owner fills in the [**MEASUREITEMSTRUCT**](/windows/win32/api/winuser/ns-winuser-measureitemstruct) structure pointed to by the *lParam* parameter of the message and returns; this informs the system of the dimensions of the control. If a list box or combo box is created with the [**LBS\_OWNERDRAWVARIABLE**](list-box-styles.md) or [**CBS\_OWNERDRAWVARIABLE**](combo-box-styles.md) style, this message is sent to the owner for each item in the control; otherwise, this message is sent once. The system sends the **WM\_MEASUREITEM** message to the owner window of combo boxes and list boxes created with the OWNERDRAWFIXED style before sending the [**WM\_INITDIALOG**](/windows/desktop/dlgbox/wm-initdialog) message. As a result, when the owner receives this message, the system has not yet determined the height and width of the font used in the control; function calls and calculations requiring these values should occur in the main function of the application or library.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/Controls/wm-measureitem#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_MEASUREITEM = 44;

    /// <summary>Sent to the owner of a list box or combo box when the list box or combo box is destroyed or when items are removed by the LB\_DELETESTRING, LB\_RESETCONTENT, CB\_DELETESTRING, or CB\_RESETCONTENT message.</summary>
    /// <returns>An application should return **TRUE** if it processes this message.</returns>
    /// <remarks>
    /// <para>Microsoft Windows NT and later: Windows sends a **WM\_DELETEITEM** message only for items deleted from an owner-drawn list box (with the [**LBS\_OWNERDRAWFIXED**](list-box-styles.md) or [**LBS\_OWNERDRAWVARIABLE**](list-box-styles.md) style) or owner-drawn combo box (with the [**CBS\_OWNERDRAWFIXED**](combo-box-styles.md) or [**CBS\_OWNERDRAWVARIABLE**](combo-box-styles.md) style). Windows 95: Windows sends the **WM\_DELETEITEM** message for any deleted list box or combo box item with nonzero item data.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/Controls/wm-deleteitem#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_DELETEITEM = 45;

    /// <summary>Sent by a list box with the LBS\_WANTKEYBOARDINPUT style to its owner in response to a WM\_KEYDOWN message.</summary>
    /// <returns>The return value specifies the action that the application performed in response to the message. A return value of -2 indicates that the application handled all aspects of selecting the item and requires no further action by the list box. (See Remarks.) A return value of -1 indicates that the list box should perform the default action in response to the keystroke. A return value of 0 or greater specifies the index of an item in the list box and indicates that the list box should perform the default action for the keystroke on the specified item.</returns>
    /// <remarks>
    /// <para>A return value of -2 is valid only for keys that are not translated into characters by the list box control. If the [**WM\_KEYDOWN**](/windows/desktop/inputdev/wm-keydown) message translates to a [**WM\_CHAR**](/windows/desktop/inputdev/wm-char) message and the application processes the **WM\_VKEYTOITEM** message generated as a result of the key press, the list box ignores the return value and does the default processing for that character). **WM\_KEYDOWN** messages generated by keys such as VK\_UP, VK\_DOWN, VK\_NEXT, and VK\_PREVIOUS are not translated to **WM\_CHAR** messages. In such cases, trapping the **WM\_VKEYTOITEM** message and returning -2 prevents the list box from doing the default processing for that key. To trap keys that generate a char message and do special processing, the application must subclass the list box, trap both the [**WM\_KEYDOWN**](/windows/desktop/inputdev/wm-keydown) and [**WM\_CHAR**](/windows/desktop/inputdev/wm-char) messages, and process the messages appropriately in the subclass procedure. The preceding remarks apply to regular list boxes that are created with the [**LBS\_WANTKEYBOARDINPUT**](list-box-styles.md) style. If the list box is owner-drawn, the application must process the [**WM\_CHARTOITEM**](wm-chartoitem.md) message. The [**DefWindowProc**](/windows/desktop/api/winuser/nf-winuser-defwindowproca) function returns -1. If a dialog box procedure handles this message, it should cast the desired return value to a **BOOL** and return the value directly. The DWL\_MSGRESULT value set by the [**SetWindowLong**](/windows/desktop/api/winuser/nf-winuser-setwindowlonga) function is ignored.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/Controls/wm-vkeytoitem#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_VKEYTOITEM = 46;

    /// <summary>Sent by a list box with the LBS\_WANTKEYBOARDINPUT style to its owner in response to a WM\_CHAR message.</summary>
    /// <returns>The return value specifies the action that the application performed in response to the message. A return value of -1 or -2 indicates that the application handled all aspects of selecting the item and requires no further action by the list box. A return value of 0 or greater specifies the zero-based index of an item in the list box and indicates that the list box should perform the default action for the keystroke on the specified item.</returns>
    /// <remarks>
    /// <para>The [**DefWindowProc**](/windows/desktop/api/winuser/nf-winuser-defwindowproca) function returns -1. Only owner-drawn list boxes that do not have the [**LBS\_HASSTRINGS**](list-box-styles.md) style can receive this message. If a dialog box procedure handles this message, it should cast the desired return value to a **BOOL** and return the value directly. The *DWL\_MSGRESULT* value set by the [**SetWindowLong**](/windows/desktop/api/winuser/nf-winuser-setwindowlonga) function is ignored.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/Controls/wm-chartoitem#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_CHARTOITEM = 47;

    /// <summary>Sets the font that a control is to use when drawing text.</summary>
    /// <returns>
    /// <para>Type: **LRESULT** This message does not return a value.</para>
    /// </returns>
    /// <remarks>
    /// <para>The **WM\_SETFONT** message applies to all controls, not just those in dialog boxes. The best time for the owner of a dialog box control to set the font of the control is when it receives the [**WM\_INITDIALOG**](../dlgbox/wm-initdialog.md) message. The application should call the [**DeleteObject**](/windows/win32/api/wingdi/nf-wingdi-deleteobject) function to delete the font when it is no longer needed; for example, after it destroys the control. The size of the control does not change as a result of receiving this message. To avoid clipping text that does not fit within the boundaries of the control, the application should correct the size of the control window before it sets the font. When a dialog box uses the [DS\_SETFONT](../dlgbox/about-dialog-boxes.md) style to set the text in its controls, the system sends the **WM\_SETFONT** message to the dialog box procedure before it creates the controls. An application can create a dialog box that contains the DS\_SETFONT style by calling any of the following functions: -   [**CreateDialogIndirect**](/windows/win32/api/winuser/nf-winuser-createdialogindirecta) -   [**CreateDialogIndirectParam**](/windows/win32/api/winuser/nf-winuser-createdialogindirectparama) -   [**DialogBoxIndirect**](/windows/win32/api/winuser/nf-winuser-dialogboxindirecta) -   [**DialogBoxIndirectParam**](/windows/win32/api/winuser/nf-winuser-dialogboxindirectparama)</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/winmsg/wm-setfont#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_SETFONT = 48;

    /// <summary>Retrieves the font with which the control is currently drawing its text.</summary>
    /// <returns>
    /// <para>Type: **HFONT** The return value is a handle to the font used by the control, or **NULL** if the control is using the system font.</para>
    /// </returns>
    /// <remarks>
    /// <para><see href="https://learn.microsoft.com/windows/win32/winmsg/wm-getfont">Learn more about this API from docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_GETFONT = 49;

    /// <summary>Sent to a window to associate a hot key with the window. When the user presses the hot key, the system activates the window.</summary>
    /// <returns>
    /// <para>The return value is one of the following.</para>
    /// <para>| Return value                                                                  | Description                                                                             | |-------------------------------------------------------------------------------|-----------------------------------------------------------------------------------------| | <dl> <dt>-1</dt> </dl> | The function is unsuccessful; the hot key is invalid.<br/>                        | | <dl> <dt>0</dt> </dl>  | The function is unsuccessful; the window is invalid.<br/>                         | | <dl> <dt>1</dt> </dl>  | The function is successful, and no other window has the same hot key.<br/>        | | <dl> <dt>2</dt> </dl>  | The function is successful, but another window already has the same hot key.<br/> |</para>
    /// </returns>
    /// <remarks>
    /// <para>A hot key cannot be associated with a child window. **VK\_ESCAPE**, **VK\_SPACE**, and **VK\_TAB** are invalid hot keys. When the user presses the hot key, the system generates a [**WM\_SYSCOMMAND**](/windows/desktop/menurc/wm-syscommand) message with *wParam* equal to **SC\_HOTKEY** and *lParam* equal to the window's handle. If this message is passed on to [**DefWindowProc**](/windows/desktop/api/winuser/nf-winuser-defwindowproca), the system will bring the window's last active popup (if it exists) or the window itself (if there is no popup window) to the foreground. A window can only have one hot key. If the window already has a hot key associated with it, the new hot key replaces the old one. If more than one window has the same hot key, the window that is activated by the hot key is random. These hot keys are unrelated to the hot keys set by [**RegisterHotKey**](/windows/win32/api/winuser/nf-winuser-registerhotkey).</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/inputdev/wm-sethotkey#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_SETHOTKEY = 50;

    /// <summary>Sent to determine the hot key associated with a window.</summary>
    /// <returns>
    /// <para>The return value is the virtual-key code and modifiers for the hot key, or **NULL** if no hot key is associated with the window. The virtual-key code is in the low byte of the return value and the modifiers are in the high byte. The modifiers can be a combination of the following flags from CommCtrl.h.</para>
    /// <para>| Return code/value                                                                                                                                         | Description             | |-----------------------------------------------------------------------------------------------------------------------------------------------------------|-------------------------| | <dl> <dt>**HOTKEYF\_ALT**</dt> <dt>0x04</dt> </dl>     | ALT key<br/>      | | <dl> <dt>**HOTKEYF\_CONTROL**</dt> <dt>0x02</dt> </dl> | CTRL key<br/>     | | <dl> <dt>**HOTKEYF\_EXT**</dt> <dt>0x08</dt> </dl>     | Extended key<br/> | | <dl> <dt>**HOTKEYF\_SHIFT**</dt> <dt>0x01</dt> </dl>   | SHIFT key<br/>    |</para>
    /// </returns>
    /// <remarks>These hot keys are unrelated to the hot keys set by the [**RegisterHotKey**](/windows/win32/api/winuser/nf-winuser-registerhotkey) function.</remarks>
    public const int WM_GETHOTKEY = 51;

    /// <summary>Sent to a minimized (iconic) window.</summary>
    /// <returns>
    /// <para>Type: **LRESULT** An application should return a handle to a cursor or icon that the system is to display while the user drags the icon. The cursor or icon must be compatible with the display driver's resolution. If the application returns **NULL**, the system displays the default cursor.</para>
    /// </returns>
    /// <remarks>
    /// <para>When the user drags the icon of a window without a class icon, the system replaces the icon with a default cursor. If the application requires a different cursor to be displayed during dragging, it must return a handle to the cursor or icon compatible with the display driver's resolution. If an application returns a handle to a color cursor or icon, the system converts the cursor or icon to black and white. The application can call the [**LoadCursor**](/windows/win32/api/winuser/nf-winuser-loadcursora) or [**LoadIcon**](/windows/win32/api/winuser/nf-winuser-loadicona) function to load a cursor or icon from the resources in its executable (.exe) file and to retrieve this handle. If a dialog box procedure handles this message, it should cast the desired return value to a **BOOL** and return the value directly. The **DWL\_MSGRESULT** value set by the [**SetWindowLong**](/windows/win32/api/winuser/nf-winuser-setwindowlonga) function is ignored.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/winmsg/wm-querydragicon#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_QUERYDRAGICON = 55;

    /// <summary>Sent to determine the relative position of a new item in the sorted list of an owner-drawn combo box or list box.</summary>
    /// <returns>
    /// <para>The return value indicates the relative position of the two items. It may be any of the values shown in the following table.</para>
    /// <para>| Return code                                                                          | Description                                                  | |--------------------------------------------------------------------------------------|--------------------------------------------------------------| | <dl> <dt>**Value**</dt> </dl> | Meaning<br/>                                           | | <dl> <dt>**-1**</dt> </dl>    | Item 1 precedes item 2 in the sorted order.<br/>       | | <dl> <dt>**0**</dt> </dl>     | Items 1 and 2 are equivalent in the sorted order.<br/> | | <dl> <dt>**1**</dt> </dl>     | Item 1 follows item 2 in the sorted order.<br/>        |</para>
    /// </returns>
    /// <remarks>
    /// <para>When the owner of an owner-drawn combo box or list box receives this message, the owner returns a value indicating which of the items specified by the [**COMPAREITEMSTRUCT**](/windows/win32/api/winuser/ns-winuser-compareitemstruct) structure will appear before the other. Typically, the system sends this message several times until it determines the exact position for the new item. If a dialog box procedure handles this message, it should cast the desired return value to a **BOOL** and return the value directly. The DWL\_MSGRESULT value set by the [**SetWindowLong**](/windows/desktop/api/winuser/nf-winuser-setwindowlonga) function is ignored.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/Controls/wm-compareitem#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_COMPAREITEM = 57;

    /// <summary>Sent by both Microsoft Active Accessibility and Microsoft UI Automation to obtain information about an accessible object contained in a server application.</summary>
    /// <returns>
    /// <para>If the window or control does not need to respond to this message, it should pass the message to the [**DefWindowProc**](/windows/desktop/api/winuser/nf-winuser-defwindowproca) function; otherwise, the window or control should return a value that corresponds to the request specified by *dwObjId*: -   If the window or control implements UI Automation, the window or control should return the value obtained by a call to the [**UiaReturnRawElementProvider**](/windows/desktop/api/UIAutomationCoreApi/nf-uiautomationcoreapi-uiareturnrawelementprovider) function. -   If *dwObjId* is [**OBJID\_NATIVEOM**](object-identifiers.md) and the window exposes a native Object Model, the windows should return the value obtained by a call to the [**LresultFromObject**](/windows/desktop/api/Oleacc/nf-oleacc-lresultfromobject) function. -   If *dwObjId* is [**OBJID\_CLIENT**](object-identifiers.md) and the window implements [**IAccessible**](/windows/desktop/api/oleacc/nn-oleacc-iaccessible), the window should return the value obtained by a call to the [**LresultFromObject**](/windows/desktop/api/Oleacc/nf-oleacc-lresultfromobject) function.</para>
    /// </returns>
    /// <remarks>
    /// <para>When a client calls [**AccessibleObjectFromWindow**](/windows/desktop/api/Oleacc/nf-oleacc-accessibleobjectfromwindow) or any of the other **AccessibleObjectFrom***X* functions that retrieve an interface to an object, Microsoft Active Accessibility sends the **WM\_GETOBJECT** message to the appropriate window procedure within the appropriate server application. While processing **WM\_GETOBJECT**, server applications call [**LresultFromObject**](/windows/desktop/api/Oleacc/nf-oleacc-lresultfromobject) and use the return value of this function as the return value for the message. Microsoft Active Accessibility, in conjunction with the COM library, performs the appropriate marshaling and passes the interface pointer from the server back to the client. Servers do not respond to **WM\_GETOBJECT** before the object is fully initialized or after it begins to close down. When an application creates a new window, the system sends [**EVENT\_OBJECT\_CREATE**](event-constants.md) to notify clients before it sends the [WM\_CREATE](../winmsg/wm-create.md) message to the application's window procedure. Because many applications use WM\_CREATE to start their initialization process, servers do not respond to the **WM\_GETOBJECT** message until finished processing the **WM\_CREATE** message. A server uses **WM\_GETOBJECT** to perform the following tasks: -   [Create New Accessible Objects](create-new-accessible-objects.md) -   [Reuse Existing Pointers to Objects](reuse-existing-pointers-to-objects.md) -   [Create New Interfaces to the Same Object](create-new-interfaces-to-the-same-object.md) For clients, this means that they might receive distinct interface pointers for the same user interface element, depending on the server's action. To determine if two interface pointers point to the same user interface element, clients compare [**IAccessible**](/windows/desktop/api/oleacc/nn-oleacc-iaccessible) properties of the object. Comparing pointers does not work.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/WinAuto/wm-getobject#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_GETOBJECT = 61;

    /// <summary>Sent to all top-level windows when the system detects more than 12.5 percent of system time over a 30- to 60-second interval is being spent compacting memory. This indicates that system memory is low.</summary>
    /// <returns>
    /// <para>Type: **LRESULT** If an application processes this message, it should return zero.</para>
    /// </returns>
    /// <remarks>When an application receives this message, it should free as much memory as possible, taking into account the current level of activity of the application and the total number of applications running on the system.</remarks>
    public const int WM_COMPACTING = 65;

    public const int WM_COMMNOTIFY = 68;

    /// <summary>Sent to a window whose size, position, or place in the Z order is about to change as a result of a call to the SetWindowPos function or another window-management function.</summary>
    /// <returns>
    /// <para>Type: **LRESULT** If an application processes this message, it should return zero.</para>
    /// </returns>
    /// <remarks>
    /// <para>For a window with the [**WS\_OVERLAPPED**](window-styles.md) or **WS\_THICKFRAME** style, the [**DefWindowProc**](/windows/desktop/api/winuser/nf-winuser-defwindowproca) function sends the [**WM\_GETMINMAXINFO**](wm-getminmaxinfo.md) message to the window. This is done to validate the new size and position of the window and to enforce the [CS\_BYTEALIGNCLIENT](about-window-classes.md) and CS\_BYTEALIGNWINDOW client styles. By not passing the **WM\_WINDOWPOSCHANGING** message to the **DefWindowProc** function, an application can override these defaults. While this message is being processed, modifying any of the values in [**WINDOWPOS**](/windows/win32/api/winuser/ns-winuser-windowpos) affects the window's new size, position, or place in the Z order. An application can prevent changes to the window by setting or clearing the appropriate bits in the **flags** member of **WINDOWPOS**.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/winmsg/wm-windowposchanging#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_WINDOWPOSCHANGING = 70;

    /// <summary>Sent to a window whose size, position, or place in the Z order has changed as a result of a call to the SetWindowPos function or another window-management function.</summary>
    /// <returns>
    /// <para>Type: **LRESULT** If an application processes this message, it should return zero.</para>
    /// </returns>
    /// <remarks>By default, the [**DefWindowProc**](/windows/desktop/api/winuser/nf-winuser-defwindowproca) function sends the [**WM\_SIZE**](wm-size.md) and [**WM\_MOVE**](wm-move.md) messages to the window. The **WM\_SIZE** and **WM\_MOVE** messages are not sent if an application handles the **WM\_WINDOWPOSCHANGED** message without calling **DefWindowProc**. It is more efficient to perform any move or size change processing during the **WM\_WINDOWPOSCHANGED** message without calling **DefWindowProc**.</remarks>
    public const int WM_WINDOWPOSCHANGED = 71;

    /// <summary>Notifies applications that the system, typically a battery-powered personal computer, is about to enter a suspended mode.</summary>
    /// <returns>The value an application returns depends on the value of the *wParam* parameter. If *wParam* is **PWR\_SUSPENDREQUEST**, the return value is **PWR\_FAIL** to prevent the system from entering the suspended state; otherwise, it is **PWR\_OK**. If *wParam* is **PWR\_SUSPENDRESUME** or **PWR\_CRITICALRESUME**, the return value is zero.</returns>
    /// <remarks>
    /// <para>This message is broadcast only to an application that is running on a system that conforms to the Advanced Power Management (APM) basic input/output system (BIOS) specification. The message is broadcast by the power-management driver to each window returned by the **EnumWindows** function. The suspended mode is the state in which the greatest amount of power savings occurs, but all operational data and parameters are preserved. Random-access memory (RAM) contents are preserved, but many devices are likely to be turned off.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/Power/wm-power#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_POWER = 72;

    /// <summary>An application sends the WM\_COPYDATA message to pass data to another application.</summary>
    /// <returns>If the receiving application processes this message, it should return **TRUE**; otherwise, it should return **FALSE**.</returns>
    /// <remarks>
    /// <para>The data being passed must not contain pointers or other references to objects not accessible to the application receiving the data. While this message is being sent, the referenced data must not be changed by another thread of the sending process. The receiving application should consider the data read-only. The *lParam* parameter is valid only during the processing of the message. The receiving application should not free the memory referenced by *lParam*. If the receiving application must access the data after [**SendMessage**](/windows/desktop/api/winuser/nf-winuser-sendmessage) returns, it must copy the data into a local buffer.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/dataxchg/wm-copydata#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_COPYDATA = 74;

    /// <summary>Posted to an application when a user cancels the application's journaling activities. The message is posted with a NULL window handle.</summary>
    /// <returns>
    /// <para>Type: **void** This message does not return a value. It is meant to be processed from within an application's main loop or a [**GetMessage**](/windows/win32/api/winuser/nf-winuser-getmessage) hook procedure, not from a window procedure.</para>
    /// </returns>
    /// <remarks>
    /// <para>Journal record and playback modes are modes imposed on the system that let an application sequentially record or play back user input. The system enters these modes when an application installs a [*JournalRecordProc*](/previous-versions/windows/desktop/legacy/ms644983(v=vs.85)) or [*JournalPlaybackProc*](/previous-versions/windows/desktop/legacy/ms644982(v=vs.85)) hook procedure. When the system is in either of these journaling modes, applications must take turns reading input from the input queue. If any one application stops reading input while the system is in a journaling mode, other applications are forced to wait. To ensure a robust system, one that cannot be made unresponsive by any one application, the system automatically cancels any journaling activities when a user presses CTRL+ESC or CTRL+ALT+DEL. The system then unhooks any journaling hook procedures, and posts a **WM\_CANCELJOURNAL** message, with a **NULL** window handle, to the application that set the journaling hook. The **WM\_CANCELJOURNAL** message has a **NULL** window handle, therefore it cannot be dispatched to a window procedure. There are two ways for an application to see a **WM\_CANCELJOURNAL** message: If the application is running in its own main loop, it must catch the message between its call to [**GetMessage**](/windows/win32/api/winuser/nf-winuser-getmessage) or [**PeekMessage**](/windows/win32/api/winuser/nf-winuser-peekmessagea) and its call to [**DispatchMessage**](/windows/win32/api/winuser/nf-winuser-dispatchmessage). If the application is not running in its own main loop, it must set a [*GetMsgProc*](/previous-versions/windows/desktop/legacy/ms644981(v=vs.85)) hook procedure (through a call to [**SetWindowsHookEx**](/windows/win32/api/winuser/nf-winuser-setwindowshookexa) specifying the **WH\_GETMESSAGE** hook type) that watches for the message. When an application sees a **WM\_CANCELJOURNAL** message, it can assume two things: the user has intentionally canceled the journal record or playback mode, and the system has already unhooked any journal record or playback hook procedures. Note that the key combinations mentioned above (CTRL+ESC or CTRL+ALT+DEL) cause the system to cancel journaling. If any one application is made unresponsive, they give the user a means of recovery. The [**VK\_CANCEL**](../inputdev/virtual-key-codes.md) virtual key code (usually implemented as the CTRL+BREAK key combination) is what an application that is in journal record mode should watch for as a signal that the user wishes to cancel the journaling activity. The difference is that watching for **VK\_CANCEL** is a suggested behavior for journaling applications, whereas CTRL+ESC or CTRL+ALT+DEL cause the system to cancel journaling regardless of a journaling application's behavior.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/winmsg/wm-canceljournal#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_CANCELJOURNAL = 75;

    /// <summary>Posted to the window with the focus when the user chooses a new input language, either with the hotkey (specified in the Keyboard control panel application) or from the indicator on the system taskbar.</summary>
    /// <returns>
    /// <para>Type: **LRESULT** This message is posted, not sent, to the application, so the return value is ignored. To accept the change, the application should pass the message to [**DefWindowProc**](/windows/desktop/api/winuser/nf-winuser-defwindowproca). To reject the change, the application should return zero without calling **DefWindowProc**.</para>
    /// </returns>
    /// <remarks>
    /// <para>When the [**DefWindowProc**](/windows/desktop/api/winuser/nf-winuser-defwindowproca) function receives the **WM\_INPUTLANGCHANGEREQUEST** message, it activates the new input locale and notifies the application of the change by sending the [**WM\_INPUTLANGCHANGE**](wm-inputlangchange.md) message. The language indicator is present on the taskbar only if you have installed more than one keyboard layout and if you have enabled the indicator using the Keyboard control panel application.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/winmsg/wm-inputlangchangerequest#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_INPUTLANGCHANGEREQUEST = 80;

    /// <summary>Sent to the topmost affected window after an application's input language has been changed. You should make any application-specific settings and pass the message to the DefWindowProc function, which passes the message to all first-level child windows.</summary>
    /// <returns>
    /// <para>Type: **LRESULT** An application should return nonzero if it processes this message.</para>
    /// </returns>
    /// <remarks>
    /// <para>You can retrieve the [BCP 47](https://www.rfc-editor.org/info/bcp47) [locale name](../Intl/locale-names.md) from the language identifier by calling the [LCIDToLocaleName](/windows/win32/api/winnls/nf-winnls-lcidtolocalename) function. Once you have the locale name, you can then use [modern locale functions](/windows/win32/intl/calling-the--locale-name--functions) to extract additional locale information. </para>
    /// <para>This doc was truncated.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/winmsg/wm-inputlangchange#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_INPUTLANGCHANGE = 81;

    /// <summary>Sent to an application that has initiated a training card with Windows Help.</summary>
    /// <returns>The return value is ignored; use zero.</returns>
    /// <remarks>
    /// <para><see href="https://learn.microsoft.com/windows/win32/shell/wm-tcard">Learn more about this API from docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_TCARD = 82;

    /// <summary>Indicates that the user pressed the F1 key.</summary>
    /// <returns>Returns **TRUE**.</returns>
    /// <remarks>The [**DefWindowProc**](/windows/desktop/api/winuser/nf-winuser-defwindowproca) function passes **WM\_HELP** to the parent window of a child window or to the owner of a top-level window.</remarks>
    public const int WM_HELP = 83;

    /// <summary>Sent to all windows after the user has logged on or off. When the user logs on or off, the system updates the user-specific settings. The system sends this message immediately after updating the settings.</summary>
    /// <returns>
    /// <para>Type: **LRESULT** An application should return zero if it processes this message.</para>
    /// </returns>
    /// <remarks>
    /// <para><see href="https://learn.microsoft.com/windows/win32/winmsg/wm-userchanged">Learn more about this API from docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_USERCHANGED = 84;

    /// <summary>Determines if a window accepts ANSI or Unicode structures in the WM\_NOTIFY notification message. WM\_NOTIFYFORMAT messages are sent from a common control to its parent window and from the parent window to the common control.</summary>
    /// <returns>
    /// <para>Returns one of the following values.</para>
    /// <para>| Return code                                                                                 | Description                                                                                                    | |---------------------------------------------------------------------------------------------|----------------------------------------------------------------------------------------------------------------| | <dl> <dt>**NFR\_ANSI**</dt> </dl>    | ANSI structures should be used in [**WM\_NOTIFY**](wm-notify.md) messages sent by the control.<br/>     | | <dl> <dt>**NFR\_UNICODE**</dt> </dl> | Unicode structures should be used in [**WM\_NOTIFY**](wm-notify.md) messages sent by the control. <br/> | | <dl> <dt>**0**</dt> </dl>            | An error occurred.<br/>                                                                                  |</para>
    /// </returns>
    /// <remarks>
    /// <para>When a common control is created, the control sends a **WM\_NOTIFYFORMAT** message to its parent window to determine the type of structures to use in [**WM\_NOTIFY**](wm-notify.md) messages. If the parent window does not handle this message, the [**DefWindowProc**](/windows/desktop/api/winuser/nf-winuser-defwindowproca) function responds according to the type of the parent window. That is, if the parent window is a Unicode window, **DefWindowProc** returns NFR\_UNICODE, and if the parent window is an ANSI window, **DefWindowProc** returns NFR\_ANSI. If the parent window is a dialog box and does not handle this message, the [**DefDlgProc**](/windows/desktop/api/winuser/nf-winuser-defdlgprocw) function similarly responds according to the type of the dialog box (Unicode or ANSI). A parent window can change the type of structures a common control uses in [**WM\_NOTIFY**](wm-notify.md) messages by setting *lParam* to NF\_REQUERY and sending a **WM\_NOTIFYFORMAT** message to the control. This causes the control to send an NF\_QUERY form of the **WM\_NOTIFYFORMAT** message to the parent window. All common controls will send **WM\_NOTIFYFORMAT** messages. However, the standard Windows controls (edit controls, combo boxes, list boxes, buttons, scroll bars, and static controls) do not.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/Controls/wm-notifyformat#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_NOTIFYFORMAT = 85;

    /// <summary>Sent to a window when the SetWindowLong function is about to change one or more of the window's styles.</summary>
    /// <returns>
    /// <para>Type: **LRESULT** An application should return zero if it processes this message.</para>
    /// </returns>
    /// <remarks>
    /// <para><see href="https://learn.microsoft.com/windows/win32/winmsg/wm-stylechanging">Learn more about this API from docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_STYLECHANGING = 124;

    /// <summary>Sent to a window after the SetWindowLong function has changed one or more of the window's styles.</summary>
    /// <returns>
    /// <para>Type: **LRESULT** An application should return zero if it processes this message.</para>
    /// </returns>
    /// <remarks>
    /// <para><see href="https://learn.microsoft.com/windows/win32/winmsg/wm-stylechanged">Learn more about this API from docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_STYLECHANGED = 125;

    /// <summary>The WM\_DISPLAYCHANGE message is sent to all windows when the display resolution has changed.</summary>
    /// <remarks>This message is only sent to top-level windows. For all other windows it is posted.</remarks>
    public const int WM_DISPLAYCHANGE = 126;

    /// <summary>Sent to a window to retrieve a handle to the large or small icon associated with a window. The system displays the large icon in the ALT+TAB dialog, and the small icon in the window caption.</summary>
    /// <returns>
    /// <para>Type: **HICON** The return value is a handle to the large or small icon, depending on the value of *wParam*. When an application receives this message, it can return a handle to a large or small icon, or pass the message to the [**DefWindowProc**](/windows/desktop/api/winuser/nf-winuser-defwindowproca) function.</para>
    /// </returns>
    /// <remarks>
    /// <para>When an application receives this message, it can return a handle to a large or small icon, or pass the message to [**DefWindowProc**](/windows/desktop/api/winuser/nf-winuser-defwindowproca). [**DefWindowProc**](/windows/desktop/api/winuser/nf-winuser-defwindowproca) returns a handle to the large or small icon associated with the window, depending on the value of *wParam*. A window that has no icon explicitly set (with **WM\_SETICON**) uses the icon for the registered window class, and in this case [**DefWindowProc**](/windows/desktop/api/winuser/nf-winuser-defwindowproca) will return 0 for a **WM\_GETICON** message. If sending a **WM\_GETICON** message to a window returns 0, next try calling the [**GetClassLongPtr**](/windows/win32/api/winuser/nf-winuser-getclasslongptra) function for the window. If that returns 0 then try the [**LoadIcon**](/windows/win32/api/winuser/nf-winuser-loadicona) function.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/winmsg/wm-geticon#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_GETICON = 127;

    /// <summary>Associates a new large or small icon with a window. The system displays the large icon in the ALT+TAB dialog box, and the small icon in the window caption.</summary>
    /// <returns>
    /// <para>Type: **LRESULT** The return value is a handle to the previous large or small icon, depending on the value of *wParam*. It is **NULL** if the window previously had no icon of the type indicated by *wParam*.</para>
    /// </returns>
    /// <remarks>The [**DefWindowProc**](/windows/desktop/api/winuser/nf-winuser-defwindowproca) function returns a handle to the previous large or small icon associated with the window, depending on the value of *wParam*.</remarks>
    public const int WM_SETICON = 128;

    /// <summary>Sent prior to the WM\_CREATE message when a window is first created.</summary>
    /// <returns>
    /// <para>Type: **LRESULT** If an application processes this message, it should return **TRUE** to continue creation of the window. If the application returns **FALSE**, the [**CreateWindow**](/windows/win32/api/winuser/nf-winuser-createwindowa) or [**CreateWindowEx**](/windows/win32/api/winuser/nf-winuser-createwindowexa) function will return a **NULL** handle.</para>
    /// </returns>
    /// <remarks>
    /// <para><see href="https://learn.microsoft.com/windows/win32/winmsg/wm-nccreate">Learn more about this API from docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_NCCREATE = 129;

    /// <summary>Notifies a window that its nonclient area is being destroyed. The DestroyWindow function sends the WM\_NCDESTROY message to the window following the WM\_DESTROY message.</summary>
    /// <returns>
    /// <para>Type: **LRESULT** If an application processes this message, it should return zero.</para>
    /// </returns>
    /// <remarks>This message frees any memory internally allocated for the window.</remarks>
    public const int WM_NCDESTROY = 130;

    /// <summary>Sent when the size and position of a window's client area must be calculated. By processing this message, an application can control the content of the window's client area when the size or position of the window changes.</summary>
    /// <returns>
    /// <para>Type: **LRESULT** If the *wParam* parameter is **FALSE**, the application should return zero. If *wParam* is **TRUE**, the application should return zero or a combination of the following values. If *wParam* is **TRUE** and an application returns zero, the old client area is preserved and is aligned with the upper-left corner of the new client area.</para>
    /// <para>| Return code/value                                                                                                                                           | Description                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             | |-------------------------------------------------------------------------------------------------------------------------------------------------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------| | <dl> <dt>**WVR\_ALIGNTOP**</dt> <dt>0x0010</dt> </dl>    | Specifies that the client area of the window is to be preserved and aligned with the top of the new position of the window. For example, to align the client area to the upper-left corner, return the WVR\_ALIGNTOP and **WVR\_ALIGNLEFT** values.<br/>                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          | | <dl> <dt>**WVR\_ALIGNRIGHT**</dt> <dt>0x0080</dt> </dl>  | Specifies that the client area of the window is to be preserved and aligned with the right side of the new position of the window. For example, to align the client area to the lower-right corner, return the **WVR\_ALIGNRIGHT** and WVR\_ALIGNBOTTOM values.<br/>                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              | | <dl> <dt>**WVR\_ALIGNLEFT**</dt> <dt>0x0020</dt> </dl>   | Specifies that the client area of the window is to be preserved and aligned with the left side of the new position of the window. For example, to align the client area to the lower-left corner, return the **WVR\_ALIGNLEFT** and **WVR\_ALIGNBOTTOM** values.<br/>                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             | | <dl> <dt>**WVR\_ALIGNBOTTOM**</dt> <dt>0x0040</dt> </dl> | Specifies that the client area of the window is to be preserved and aligned with the bottom of the new position of the window. For example, to align the client area to the top-left corner, return the WVR\_ALIGNTOP and **WVR\_ALIGNLEFT** values.<br/>                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         | | <dl> <dt>**WVR\_HREDRAW**</dt> <dt>0x0100</dt> </dl>     | Used in combination with any other values, except **WVR\_VALIDRECTS**, causes the window to be completely redrawn if the client rectangle changes size horizontally. This value is similar to [CS\_HREDRAW](about-window-classes.md) class style<br/>                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                               | | <dl> <dt>**WVR\_VREDRAW**</dt> <dt>0x0200</dt> </dl>     | Used in combination with any other values, except **WVR\_VALIDRECTS**, causes the window to be completely redrawn if the client rectangle changes size vertically. This value is similar to [CS\_VREDRAW](about-window-classes.md) class style<br/>                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 | | <dl> <dt>**WVR\_REDRAW**</dt> <dt>0x0300</dt> </dl>      | This value causes the entire window to be redrawn. It is a combination of **WVR\_HREDRAW** and **WVR\_VREDRAW** values.<br/>                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      | | <dl> <dt>**WVR\_VALIDRECTS**</dt> <dt>0x0400</dt> </dl>  | This value indicates that, upon return from [**WM\_NCCALCSIZE**](wm-nccalcsize.md), the rectangles specified by the **rgrc**\[1\] and **rgrc**\[2\] members of the [**NCCALCSIZE\_PARAMS**](/windows/win32/api/winuser/ns-winuser-nccalcsize_params) structure contain valid destination and source area rectangles, respectively. The system combines these rectangles to calculate the area of the window to be preserved. The system copies any part of the window image that is within the source rectangle and clips the image to the destination rectangle. Both rectangles are in parent-relative or screen-relative coordinates. This flag cannot be combined with any other flags. <br/> This return value allows an application to implement more elaborate client-area preservation strategies, such as centering or preserving a subset of the client area.<br/> |</para>
    /// </returns>
    /// <remarks>
    /// <para>The window may be redrawn, depending on whether the [CS\_HREDRAW](about-window-classes.md) or CS\_VREDRAW class style is specified. This is the default, backward-compatible processing of this message by the [**DefWindowProc**](/windows/desktop/api/winuser/nf-winuser-defwindowproca) function (in addition to the usual client rectangle calculation described in the preceding table). When *wParam* is **TRUE**, simply returning 0 without processing the [**NCCALCSIZE\_PARAMS**](/windows/win32/api/winuser/ns-winuser-nccalcsize_params) rectangles will cause the client area to resize to the size of the window, including the window frame. This will remove the window frame and caption items from your window, leaving only the client area displayed. Starting with Windows Vista, removing the standard frame by simply returning 0 when the *wParam* is **TRUE** does not affect frames that are extended into the client area using the [**DwmExtendFrameIntoClientArea**](/windows/win32/api/dwmapi/nf-dwmapi-dwmextendframeintoclientarea) function. Only the standard frame will be removed.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/winmsg/wm-nccalcsize#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_NCCALCSIZE = 131;

    /// <summary>Sent to a window in order to determine what part of the window corresponds to a particular screen coordinate.</summary>
    /// <returns>
    /// <para>The return value of the [**DefWindowProc**](/windows/desktop/api/winuser/nf-winuser-defwindowproca) function is one of the following values, indicating the position of the cursor hot spot.</para>
    /// <para>| Return code/value                                                                                                                                    | Description                                                                                                                                                                                                        | |------------------------------------------------------------------------------------------------------------------------------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------| | <dl> <dt>**HTBORDER**</dt> <dt>18</dt> </dl>      | In the border of a window that does not have a sizing border.<br/>                                                                                                                                           | | <dl> <dt>**HTBOTTOM**</dt> <dt>15</dt> </dl>      | In the lower-horizontal border of a resizable window (the user can click the mouse to resize the window vertically).<br/>                                                                                    | | <dl> <dt>**HTBOTTOMLEFT**</dt> <dt>16</dt> </dl>  | In the lower-left corner of a border of a resizable window (the user can click the mouse to resize the window diagonally).<br/>                                                                              | | <dl> <dt>**HTBOTTOMRIGHT**</dt> <dt>17</dt> </dl> | In the lower-right corner of a border of a resizable window (the user can click the mouse to resize the window diagonally).<br/>                                                                             | | <dl> <dt>**HTCAPTION**</dt> <dt>2</dt> </dl>      | In a title bar.<br/>                                                                                                                                                                                         | | <dl> <dt>**HTCLIENT**</dt> <dt>1</dt> </dl>       | In a client area.<br/>                                                                                                                                                                                       | | <dl> <dt>**HTCLOSE**</dt> <dt>20</dt> </dl>       | In a **Close** button.<br/>                                                                                                                                                                                  | | <dl> <dt>**HTERROR**</dt> <dt>-2</dt> </dl>       | On the screen background or on a dividing line between windows (same as **HTNOWHERE**, except that the [**DefWindowProc**](/windows/desktop/api/winuser/nf-winuser-defwindowproca) function produces a system beep to indicate an error).<br/> | | <dl> <dt>**HTGROWBOX**</dt> <dt>4</dt> </dl>      | In a size box (same as **HTSIZE**).<br/>                                                                                                                                                                     | | <dl> <dt>**HTHELP**</dt> <dt>21</dt> </dl>        | In a **Help** button.<br/>                                                                                                                                                                                   | | <dl> <dt>**HTHSCROLL**</dt> <dt>6</dt> </dl>      | In a horizontal scroll bar.<br/>                                                                                                                                                                             | | <dl> <dt>**HTLEFT**</dt> <dt>10</dt> </dl>        | In the left border of a resizable window (the user can click the mouse to resize the window horizontally).<br/>                                                                                              | | <dl> <dt>**HTMENU**</dt> <dt>5</dt> </dl>         | In a menu.<br/>                                                                                                                                                                                              | | <dl> <dt>**HTMAXBUTTON**</dt> <dt>9</dt> </dl>    | In a **Maximize** button.<br/>                                                                                                                                                                               | | <dl> <dt>**HTMINBUTTON**</dt> <dt>8</dt> </dl>    | In a **Minimize** button.<br/>                                                                                                                                                                               | | <dl> <dt>**HTNOWHERE**</dt> <dt>0</dt> </dl>      | On the screen background or on a dividing line between windows.<br/>                                                                                                                                         | | <dl> <dt>**HTREDUCE**</dt> <dt>8</dt> </dl>       | In a **Minimize** button.<br/>                                                                                                                                                                               | | <dl> <dt>**HTRIGHT**</dt> <dt>11</dt> </dl>       | In the right border of a resizable window (the user can click the mouse to resize the window horizontally).<br/>                                                                                             | | <dl> <dt>**HTSIZE**</dt> <dt>4</dt> </dl>         | In a size box (same as **HTGROWBOX**).<br/>                                                                                                                                                                  | | <dl> <dt>**HTSYSMENU**</dt> <dt>3</dt> </dl>      | In a window menu or in a **Close** button in a child window.<br/>                                                                                                                                            | | <dl> <dt>**HTTOP**</dt> <dt>12</dt> </dl>         | In the upper-horizontal border of a window.<br/>                                                                                                                                                             | | <dl> <dt>**HTTOPLEFT**</dt> <dt>13</dt> </dl>     | In the upper-left corner of a window border.<br/>                                                                                                                                                            | | <dl> <dt>**HTTOPRIGHT**</dt> <dt>14</dt> </dl>    | In the upper-right corner of a window border.<br/>                                                                                                                                                           | | <dl> <dt>**HTTRANSPARENT**</dt> <dt>-1</dt> </dl> | In a window currently covered by another window in the same thread (the message will be sent to underlying windows in the same thread until one of them returns a code that is not **HTTRANSPARENT**).<br/>  | | <dl> <dt>**HTVSCROLL**</dt> <dt>7</dt> </dl>      | In the vertical scroll bar.<br/>                                                                                                                                                                             | | <dl> <dt>**HTZOOM**</dt> <dt>9</dt> </dl>         | In a **Maximize** button.<br/>                                                                                                                                                                               |</para>
    /// </returns>
    /// <remarks>
    /// <para>Use the following code to obtain the horizontal and vertical position:</para>
    /// <para></para>
    /// <para>This doc was truncated.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/inputdev/wm-nchittest#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_NCHITTEST = 132;

    /// <summary>The WM\_NCPAINT message is sent to a window when its frame must be painted.</summary>
    /// <returns>An application returns zero if it processes this message.</returns>
    /// <remarks>
    /// <para>The [**DefWindowProc**](/windows/desktop/api/winuser/nf-winuser-defwindowproca) function paints the window frame. An application can intercept the **WM\_NCPAINT** message and paint its own custom window frame. The clipping region for a window is always rectangular, even if the shape of the frame is altered. The *wParam* value can be passed to [**GetDCEx**](/windows/desktop/api/Winuser/nf-winuser-getdcex) as in the following example.</para>
    /// <para></para>
    /// <para>This doc was truncated.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/gdi/wm-ncpaint#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_NCPAINT = 133;

    /// <summary>Sent to a window when its nonclient area needs to be changed to indicate an active or inactive state.</summary>
    /// <returns>
    /// <para>Type: **LRESULT** When the *wParam* parameter is **FALSE**, an application should return **TRUE** to indicate that the system should proceed with the default processing, or it should return **FALSE** to prevent the change. When *wParam* is **TRUE**, the return value is ignored.</para>
    /// </returns>
    /// <remarks>
    /// <para>Processing messages related to the nonclient area of a standard window is not recommended, because the application must be able to draw all the required parts of the nonclient area for the window. If an application does process this message, it must return **TRUE** to direct the system to complete the change of active window. If the window is minimized when this message is received, the application should pass the message to the [**DefWindowProc**](/windows/desktop/api/winuser/nf-winuser-defwindowproca) function. The [**DefWindowProc**](/windows/desktop/api/winuser/nf-winuser-defwindowproca) function draws the title bar or icon title in its active colors when the *wParam* parameter is **TRUE** and in its inactive colors when *wParam* is **FALSE**.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/winmsg/wm-ncactivate#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_NCACTIVATE = 134;

    /// <summary>Sent to the window procedure associated with a control.</summary>
    /// <returns>
    /// <para>The return value is one or more of the following values, indicating which type of input the application processes.</para>
    /// <para>| Return code/value                                                                                                                                                | Description                                                                                                                | |------------------------------------------------------------------------------------------------------------------------------------------------------------------|----------------------------------------------------------------------------------------------------------------------------| | <dl> <dt>**DLGC\_BUTTON**</dt> <dt>0x2000</dt> </dl>          | Button.<br/>                                                                                                         | | <dl> <dt>**DLGC\_DEFPUSHBUTTON**</dt> <dt>0x0010</dt> </dl>   | Default push button.<br/>                                                                                            | | <dl> <dt>**DLGC\_HASSETSEL**</dt> <dt>0x0008</dt> </dl>       | [**EM\_SETSEL**](/windows/desktop/Controls/em-setsel) messages.<br/>                                                           | | <dl> <dt>**DLGC\_RADIOBUTTON**</dt> <dt>0x0040</dt> </dl>     | Radio button.<br/>                                                                                                   | | <dl> <dt>**DLGC\_STATIC**</dt> <dt>0x0100</dt> </dl>          | Static control.<br/>                                                                                                 | | <dl> <dt>**DLGC\_UNDEFPUSHBUTTON**</dt> <dt>0x0020</dt> </dl> | Non-default push button.<br/>                                                                                        | | <dl> <dt>**DLGC\_WANTALLKEYS**</dt> <dt>0x0004</dt> </dl>     | All keyboard input.<br/>                                                                                             | | <dl> <dt>**DLGC\_WANTARROWS**</dt> <dt>0x0001</dt> </dl>      | Direction keys.<br/>                                                                                                 | | <dl> <dt>**DLGC\_WANTCHARS**</dt> <dt>0x0080</dt> </dl>       | [**WM\_CHAR**](/windows/desktop/inputdev/wm-char) messages.<br/>                                                                      | | <dl> <dt>**DLGC\_WANTMESSAGE**</dt> <dt>0x0004</dt> </dl>     | All keyboard input (the application passes this message in the [**MSG**](/windows/win32/api/winuser/ns-winuser-msg) structure to the control).<br/> | | <dl> <dt>**DLGC\_WANTTAB**</dt> <dt>0x0002</dt> </dl>         | TAB key.<br/>                                                                                                        |</para>
    /// </returns>
    /// <remarks>
    /// <para>Although the [**DefWindowProc**](/windows/desktop/api/winuser/nf-winuser-defwindowproca) function always returns zero in response to the **WM\_GETDLGCODE** message, the window procedure for the predefined control classes return a code appropriate for each class. The **WM\_GETDLGCODE** message and the returned values are useful only with user-defined dialog box controls or standard controls modified by subclassing.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/dlgbox/wm-getdlgcode#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_GETDLGCODE = 135;

    /// <summary>The WM\_SYNCPAINT message is used to synchronize painting while avoiding linking independent GUI threads.</summary>
    /// <returns>An application returns zero if it processes this message.</returns>
    /// <remarks>When a window has been hidden, shown, moved, or sized, the system may determine that it is necessary to send a **WM\_SYNCPAINT** message to the top-level windows of other threads. Applications must pass **WM\_SYNCPAINT** to [**DefWindowProc**](/windows/desktop/api/winuser/nf-winuser-defwindowproca)  for processing. The **DefWindowProc** function will send a [**WM\_NCPAINT**](wm-ncpaint.md) message to the window procedure if the window frame must be painted and send a [**WM\_ERASEBKGND**](../winmsg/wm-erasebkgnd.md) message if the window background must be erased.</remarks>
    public const int WM_SYNCPAINT = 136;

    /// <summary>Posted to a window when the cursor is moved within the nonclient area of the window. This message is posted to the window that contains the cursor. If a window has captured the mouse, this message is not posted.</summary>
    /// <returns>If an application processes this message, it should return zero.</returns>
    /// <remarks>
    /// <para>If it is appropriate to do so, the system sends the [**WM\_SYSCOMMAND**](/windows/desktop/menurc/wm-syscommand) message to the window. You can also use the [**GET\_X\_LPARAM**](/windows/desktop/api/windowsx/nf-windowsx-get_x_lparam) and [**GET\_Y\_LPARAM**](/windows/desktop/api/windowsx/nf-windowsx-get_y_lparam) macros to extract the values of the x- and y- coordinates from *lParam*.</para>
    /// <para></para>
    /// <para>This doc was truncated.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/inputdev/wm-ncmousemove#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_NCMOUSEMOVE = 160;

    /// <summary>Posted when the user presses the left mouse button while the cursor is within the nonclient area of a window. This message is posted to the window that contains the cursor. If a window has captured the mouse, this message is not posted.</summary>
    /// <returns>If an application processes this message, it should return zero.</returns>
    /// <remarks>
    /// <para>The [**DefWindowProc**](/windows/desktop/api/winuser/nf-winuser-defwindowproca) function tests the specified point to find the location of the cursor and performs the appropriate action. If appropriate, **DefWindowProc** sends the [**WM\_SYSCOMMAND**](/windows/desktop/menurc/wm-syscommand) message to the window. You can also use the [**GET\_X\_LPARAM**](/windows/desktop/api/windowsx/nf-windowsx-get_x_lparam) and [**GET\_Y\_LPARAM**](/windows/desktop/api/windowsx/nf-windowsx-get_y_lparam) macros to extract the values of the x- and y- coordinates from *lParam*.</para>
    /// <para></para>
    /// <para>This doc was truncated.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/inputdev/wm-nclbuttondown#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_NCLBUTTONDOWN = 161;

    /// <summary>Posted when the user releases the left mouse button while the cursor is within the nonclient area of a window. This message is posted to the window that contains the cursor. If a window has captured the mouse, this message is not posted.</summary>
    /// <returns>If an application processes this message, it should return zero.</returns>
    /// <remarks>
    /// <para>The [**DefWindowProc**](/windows/desktop/api/winuser/nf-winuser-defwindowproca) function tests the specified point to find out the location of the cursor and performs the appropriate action. If appropriate, **DefWindowProc** sends the [**WM\_SYSCOMMAND**](/windows/desktop/menurc/wm-syscommand) message to the window. You can also use the [**GET\_X\_LPARAM**](/windows/desktop/api/windowsx/nf-windowsx-get_x_lparam) and [**GET\_Y\_LPARAM**](/windows/desktop/api/windowsx/nf-windowsx-get_y_lparam) macros to extract the values of the x- and y- coordinates from *lParam*.</para>
    /// <para></para>
    /// <para>This doc was truncated.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/inputdev/wm-nclbuttonup#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_NCLBUTTONUP = 162;

    /// <summary>Posted when the user double-clicks the left mouse button while the cursor is within the nonclient area of a window. This message is posted to the window that contains the cursor. If a window has captured the mouse, this message is not posted.</summary>
    /// <returns>If an application processes this message, it should return zero.</returns>
    /// <remarks>
    /// <para>You can also use the [**GET\_X\_LPARAM**](/windows/desktop/api/windowsx/nf-windowsx-get_x_lparam) and [**GET\_Y\_LPARAM**](/windows/desktop/api/windowsx/nf-windowsx-get_y_lparam) macros to extract the values of the x- and y- coordinates from *lParam*.</para>
    /// <para></para>
    /// <para>This doc was truncated.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/inputdev/wm-nclbuttondblclk#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_NCLBUTTONDBLCLK = 163;

    /// <summary>Posted when the user presses the right mouse button while the cursor is within the nonclient area of a window. This message is posted to the window that contains the cursor. If a window has captured the mouse, this message is not posted.</summary>
    /// <returns>If an application processes this message, it should return zero.</returns>
    /// <remarks>
    /// <para>You can also use the [**GET\_X\_LPARAM**](/windows/desktop/api/windowsx/nf-windowsx-get_x_lparam) and [**GET\_Y\_LPARAM**](/windows/desktop/api/windowsx/nf-windowsx-get_y_lparam) macros to extract the values of the x- and y- coordinates from *lParam*.</para>
    /// <para></para>
    /// <para>This doc was truncated.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/inputdev/wm-ncrbuttondown#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_NCRBUTTONDOWN = 164;

    /// <summary>Posted when the user releases the right mouse button while the cursor is within the nonclient area of a window. This message is posted to the window that contains the cursor. If a window has captured the mouse, this message is not posted.</summary>
    /// <returns>If an application processes this message, it should return zero.</returns>
    /// <remarks>
    /// <para>You can also use the [**GET\_X\_LPARAM**](/windows/desktop/api/windowsx/nf-windowsx-get_x_lparam) and [**GET\_Y\_LPARAM**](/windows/desktop/api/windowsx/nf-windowsx-get_y_lparam) macros to extract the values of the x- and y- coordinates from *lParam*.</para>
    /// <para></para>
    /// <para>This doc was truncated.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/inputdev/wm-ncrbuttonup#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_NCRBUTTONUP = 165;

    /// <summary>Posted when the user double-clicks the right mouse button while the cursor is within the nonclient area of a window. This message is posted to the window that contains the cursor. If a window has captured the mouse, this message is not posted.</summary>
    /// <returns>If an application processes this message, it should return zero.</returns>
    /// <remarks>
    /// <para>A window need not have the **CS\_DBLCLKS** style to receive **WM\_NCRBUTTONDBLCLK** messages. The system generates a **WM\_NCRBUTTONDBLCLK** message when the user presses, releases, and again presses the right mouse button within the system's double-click time limit. Double-clicking the right mouse button actually generates four messages: [**WM\_NCRBUTTONDOWN**](wm-ncrbuttondown.md), [**WM\_NCRBUTTONUP**](wm-ncrbuttonup.md), **WM\_NCRBUTTONDBLCLK**, and **WM\_NCRBUTTONUP** again. You can also use the [**GET\_X\_LPARAM**](/windows/desktop/api/windowsx/nf-windowsx-get_x_lparam) and [**GET\_Y\_LPARAM**](/windows/desktop/api/windowsx/nf-windowsx-get_y_lparam) macros to extract the values of the x- and y- coordinates from *lParam*.</para>
    /// <para></para>
    /// <para>This doc was truncated.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/inputdev/wm-ncrbuttondblclk#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_NCRBUTTONDBLCLK = 166;

    /// <summary>Posted when the user presses the middle mouse button while the cursor is within the nonclient area of a window. This message is posted to the window that contains the cursor. If a window has captured the mouse, this message is not posted.</summary>
    /// <returns>If an application processes this message, it should return zero.</returns>
    /// <remarks>
    /// <para>You can also use the [**GET\_X\_LPARAM**](/windows/desktop/api/windowsx/nf-windowsx-get_x_lparam) and [**GET\_Y\_LPARAM**](/windows/desktop/api/windowsx/nf-windowsx-get_y_lparam) macros to extract the values of the x- and y- coordinates from *lParam*.</para>
    /// <para></para>
    /// <para>This doc was truncated.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/inputdev/wm-ncmbuttondown#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_NCMBUTTONDOWN = 167;

    /// <summary>Posted when the user releases the middle mouse button while the cursor is within the nonclient area of a window. This message is posted to the window that contains the cursor. If a window has captured the mouse, this message is not posted.</summary>
    /// <returns>If an application processes this message, it should return zero.</returns>
    /// <remarks>
    /// <para>You can also use the [**GET\_X\_LPARAM**](/windows/desktop/api/windowsx/nf-windowsx-get_x_lparam) and [**GET\_Y\_LPARAM**](/windows/desktop/api/windowsx/nf-windowsx-get_y_lparam) macros to extract the values of the x- and y- coordinates from *lParam*.</para>
    /// <para></para>
    /// <para>This doc was truncated.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/inputdev/wm-ncmbuttonup#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_NCMBUTTONUP = 168;

    /// <summary>Posted when the user double-clicks the middle mouse button while the cursor is within the nonclient area of a window. This message is posted to the window that contains the cursor. If a window has captured the mouse, this message is not posted.</summary>
    /// <returns>If an application processes this message, it should return zero.</returns>
    /// <remarks>
    /// <para>A window need not have the **CS\_DBLCLKS** style to receive **WM\_NCMBUTTONDBLCLK** messages. The system generates a **WM\_NCMBUTTONDBLCLK** message when the user presses, releases, and again presses the middle mouse button within the system's double-click time limit. Double-clicking the middle mouse button actually generates four messages: [**WM\_NCMBUTTONDOWN**](wm-ncmbuttondown.md), [**WM\_NCMBUTTONUP**](wm-ncmbuttonup.md), **WM\_NCMBUTTONDBLCLK**, and **WM\_NCMBUTTONUP** again. You can also use the [**GET\_X\_LPARAM**](/windows/desktop/api/windowsx/nf-windowsx-get_x_lparam) and [**GET\_Y\_LPARAM**](/windows/desktop/api/windowsx/nf-windowsx-get_y_lparam) macros to extract the values of the x- and y- coordinates from *lParam*.</para>
    /// <para></para>
    /// <para>This doc was truncated.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/inputdev/wm-ncmbuttondblclk#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_NCMBUTTONDBLCLK = 169;

    /// <summary>Posted when the user presses the first or second X button while the cursor is in the nonclient area of a window. This message is posted to the window that contains the cursor. If a window has captured the mouse, this message is not posted.</summary>
    /// <returns>If an application processes this message, it should return **TRUE**. For more information about processing the return value, see the Remarks section.</returns>
    /// <remarks>
    /// <para>Use the following code to get the information in the *wParam* parameter.</para>
    /// <para></para>
    /// <para>This doc was truncated.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/inputdev/wm-ncxbuttondown#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_NCXBUTTONDOWN = 171;

    /// <summary>Posted when the user releases the first or second X button while the cursor is in the nonclient area of a window. This message is posted to the window that contains the cursor. If a window has captured the mouse, this message is not posted.</summary>
    /// <returns>If an application processes this message, it should return **TRUE**. For more information about processing the return value, see the Remarks section.</returns>
    /// <remarks>
    /// <para>Use the following code to get the information in the *wParam* parameter.</para>
    /// <para></para>
    /// <para>This doc was truncated.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/inputdev/wm-ncxbuttonup#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_NCXBUTTONUP = 172;

    /// <summary>Posted when the user double-clicks the first or second X button while the cursor is in the nonclient area of a window. This message is posted to the window that contains the cursor. If a window has captured the mouse, this message is not posted.</summary>
    /// <returns>If an application processes this message, it should return **TRUE**. For more information about processing the return value, see the Remarks section.</returns>
    /// <remarks>
    /// <para>Use the following code to get the information in the *wParam* parameter.</para>
    /// <para></para>
    /// <para>This doc was truncated.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/inputdev/wm-ncxbuttondblclk#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_NCXBUTTONDBLCLK = 173;

    /// <summary>Sent to the window that registered to receive raw input. A window receives this message through its WindowProc function.</summary>
    /// <returns>If an application processes this message, it should return zero.</returns>
    /// <remarks>
    /// <para><see href="https://learn.microsoft.com/windows/win32/inputdev/wm-input-device-change">Learn more about this API from docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_INPUT_DEVICE_CHANGE = 254;

    /// <summary>Sent to the window that is getting raw input. A window receives this message through its WindowProc function.</summary>
    /// <returns>If an application processes this message, it should return zero.</returns>
    /// <remarks>Raw input is available only when the application calls [**RegisterRawInputDevices**](/windows/win32/api/winuser/nf-winuser-registerrawinputdevices) with valid device specifications.</remarks>
    public const int WM_INPUT = 255;

    public const int WM_KEYFIRST = 256;

    /// <summary>Posted to the window with the keyboard focus when a nonsystem key is pressed. A nonsystem key is a key that is pressed when the ALT key is not pressed.</summary>
    /// <returns>An application should return zero if it processes this message.</returns>
    /// <remarks>
    /// <para>If the F10 key is pressed, the [**DefWindowProc**](/windows/desktop/api/winuser/nf-winuser-defwindowproca) function sets an internal flag. When **DefWindowProc** receives the [**WM\_KEYUP**](wm-keyup.md) message, the function checks whether the internal flag is set and, if so, sends a [**WM\_SYSCOMMAND**](/windows/desktop/menurc/wm-syscommand) message to the top-level window. The **WM\_SYSCOMMAND** parameter of the message is set to SC\_KEYMENU. Because of the autorepeat feature, more than one **WM\_KEYDOWN** message may be posted before a [**WM\_KEYUP**](wm-keyup.md) message is posted. The previous key state (bit 30) can be used to determine whether the **WM\_KEYDOWN** message indicates the first down transition or a repeated down transition. For enhanced 101- and 102-key keyboards, extended keys are the right ALT and CTRL keys on the main section of the keyboard; the INS, DEL, HOME, END, PAGE UP, PAGE DOWN, and arrow keys in the clusters to the left of the numeric keypad; and the divide (/) and ENTER keys in the numeric keypad. Other keyboards may support the extended-key bit in the *lParam* parameter. Applications must pass *wParam* to [**TranslateMessage**](/windows/desktop/api/winuser/nf-winuser-translatemessage) without altering it at all.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/inputdev/wm-keydown#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_KEYDOWN = 256;

    /// <summary>Posted to the window with the keyboard focus when a nonsystem key is released. A nonsystem key is a key that is pressed when the ALT key is not pressed, or a keyboard key that is pressed when a window has the keyboard focus.</summary>
    /// <returns>An application should return zero if it processes this message.</returns>
    /// <remarks>
    /// <para>The [**DefWindowProc**](/windows/desktop/api/winuser/nf-winuser-defwindowproca) function sends a [**WM\_SYSCOMMAND**](/windows/desktop/menurc/wm-syscommand) message to the top-level window if the F10 key or the ALT key was released. The *wParam* parameter of the message is set to SC\_KEYMENU. For enhanced 101- and 102-key keyboards, extended keys are the right ALT and CTRL keys on the main section of the keyboard; the INS, DEL, HOME, END, PAGE UP, PAGE DOWN, and arrow keys in the clusters to the left of the numeric keypad; and the divide (/) and ENTER keys in the numeric keypad. Other keyboards may support the extended-key bit in the *lParam* parameter. Applications must pass *wParam* to [**TranslateMessage**](/windows/desktop/api/winuser/nf-winuser-translatemessage) without altering it at all.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/inputdev/wm-keyup#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_KEYUP = 257;

    /// <summary>Posted to the window with the keyboard focus when a WM\_KEYDOWN message is translated by the TranslateMessage function. The WM\_CHAR message contains the character code of the key that was pressed.</summary>
    /// <returns>An application should return zero if it processes this message.</returns>
    /// <remarks>
    /// <para>The **WM\_CHAR** message uses UTF-16 (16-bit Unicode Transformation Format) code units in its **wParam** if the Unicode version of the [**RegisterClass**](/windows/win32/api/winuser/nf-winuser-registerclassw) function was used to register the window class. Otherwise, the system provides characters in the current process code page, which can be set to UTF-8 in Windows Version 1903 (May 2019 Update) and newer. For more information, see [Registering Window Classes](/windows/win32/intl/registering-window-classes) and [Use UTF-8 code pages in Windows apps](/windows/apps/design/globalizing/use-utf8-code-page). Starting with Windows Vista, **WM\_CHAR** message can send [UTF-16 surrogate pairs](/windows/win32/intl/surrogates-and-supplementary-characters) to Unicode windows. Use the [IS_HIGH_SURROGATE](/windows/win32/api/Winnls/nf-winnls-is_high_surrogate), [IS_LOW_SURROGATE](/windows/win32/api/winnls/nf-winnls-is_low_surrogate), and [IS_SURROGATE_PAIR](/windows/win32/api/winnls/nf-winnls-is_surrogate_pair) macros to detect such cases, if necessary. There is not necessarily a one-to-one correspondence between keys pressed and character messages generated, and so the information in the high-order word of the *lParam* parameter is generally not useful to applications. The information in the high-order word applies only to the most recent [**WM\_KEYDOWN**](wm-keydown.md) message that precedes the posting of the **WM\_CHAR** message. For enhanced 101- and 102-key keyboards, extended keys are the right ALT and the right CTRL keys on the main section of the keyboard; the INS, DEL, HOME, END, PAGE UP, PAGE DOWN and arrow keys in the clusters to the left of the numeric keypad; and the divide (/) and ENTER keys in the numeric keypad. Some other keyboards may support the extended-key bit in the *lParam* parameter. The [**WM\_UNICHAR**](wm-unichar.md) message is the same as **WM\_CHAR**, except it uses UTF-32. It is designed to send or post Unicode characters to ANSI windows, and it can handle Unicode Supplementary Plane characters.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/inputdev/wm-char#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_CHAR = 258;

    /// <summary>Posted to the window with the keyboard focus when a WM\_KEYUP message is translated by the TranslateMessage function.</summary>
    /// <returns>An application should return zero if it processes this message.</returns>
    /// <remarks>
    /// <para>The **WM\_DEADCHAR** message typically is used by applications to give the user feedback about each key pressed. For example, an application can display the accent in the current character position without moving the caret. Because there is not necessarily a one-to-one correspondence between keys pressed and character messages generated, the information in the high-order word of the *lParam* parameter is generally not useful to applications. The information in the high-order word applies only to the most recent [**WM\_KEYDOWN**](wm-keydown.md) message that precedes the posting of the **WM\_DEADCHAR** message. For enhanced 101- and 102-key keyboards, extended keys are the right ALT and the right CTRL keys on the main section of the keyboard; the INS, DEL, HOME, END, PAGE UP, PAGE DOWN and arrow keys in the clusters to the left of the numeric keypad; and the divide (/) and ENTER keys in the numeric keypad. Some other keyboards may support the extended-key bit in the *lParam* parameter.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/inputdev/wm-deadchar#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_DEADCHAR = 259;

    /// <summary>Posted to the window with the keyboard focus when the user presses the F10 key (which activates the menu bar) or holds down the ALT key and then presses another key.</summary>
    /// <returns>An application should return zero if it processes this message.</returns>
    /// <remarks>
    /// <para>The [**DefWindowProc**](/windows/desktop/api/winuser/nf-winuser-defwindowproca) function examines the specified key and generates a [**WM\_SYSCOMMAND**](/windows/desktop/menurc/wm-syscommand) message if the key is either TAB or ENTER. When the context code is zero, the message can be passed to the [**TranslateAccelerator**](/windows/desktop/api/winuser/nf-winuser-translateacceleratora) function, which will handle it as though it were a normal key message instead of a character-key message. This allows accelerator keys to be used with the active window even if the active window does not have the keyboard focus. Because of automatic repeat, more than one **WM\_SYSKEYDOWN** message may occur before a [**WM\_SYSKEYUP**](wm-syskeyup.md) message is sent. The previous key state (bit 30) can be used to determine whether the **WM\_SYSKEYDOWN** message indicates the first down transition or a repeated down transition. For enhanced 101- and 102-key keyboards, enhanced keys are the right ALT and CTRL keys on the main section of the keyboard; the INS, DEL, HOME, END, PAGE UP, PAGE DOWN, and arrow keys in the clusters to the left of the numeric keypad; and the divide (/) and ENTER keys in the numeric keypad. Other keyboards may support the extended-key bit in the *lParam* parameter. This message is also sent whenever the user presses the F10 key without the ALT key.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/inputdev/wm-syskeydown#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_SYSKEYDOWN = 260;

    /// <summary>Posted to the window with the keyboard focus when the user releases a key that was pressed while the ALT key was held down.</summary>
    /// <returns>An application should return zero if it processes this message.</returns>
    /// <remarks>
    /// <para>The [**DefWindowProc**](/windows/desktop/api/winuser/nf-winuser-defwindowproca) function sends a [**WM\_SYSCOMMAND**](/windows/desktop/menurc/wm-syscommand) message to the top-level window if the F10 key or the ALT key was released. The *wParam* parameter of the message is set to **SC\_KEYMENU**. When the context code is zero, the message can be passed to the [**TranslateAccelerator**](/windows/desktop/api/winuser/nf-winuser-translateacceleratora) function, which will handle it as though it were a normal key message instead of a character-key message. This allows accelerator keys to be used with the active window even if the active window does not have the keyboard focus. For enhanced 101- and 102-key keyboards, extended keys are the right ALT and CTRL keys on the main section of the keyboard; the INS, DEL, HOME, END, PAGE UP, PAGE DOWN, and arrow keys in the clusters to the left of the numeric keypad; and the divide (/) and ENTER keys in the numeric keypad. Other keyboards may support the extended-key bit in the *lParam* parameter. For non-U.S. enhanced 102-key keyboards, the right ALT key is handled as a CTRL+ALT key. The following table shows the sequence of messages that result when the user presses and releases this key.</para>
    /// <para>| Message                           | Virtual-key code | |-----------------------------------|------------------| | [**WM\_KEYDOWN**](wm-keydown.md) | **VK\_CONTROL**  | | [**WM\_KEYDOWN**](wm-keydown.md) | **VK\_MENU**     | | [**WM\_KEYUP**](wm-keyup.md)     | **VK\_CONTROL**  | | **WM\_SYSKEYUP**                  | **VK\_MENU**     |</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/inputdev/wm-syskeyup#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_SYSKEYUP = 261;

    /// <summary>Posted to the window with the keyboard focus when a WM\_SYSKEYDOWN message is translated by the TranslateMessage function.</summary>
    /// <returns>An application should return zero if it processes this message.</returns>
    /// <remarks>
    /// <para>When the context code is zero, the message can be passed to the [**TranslateAccelerator**](/windows/desktop/api/Winuser/nf-winuser-translateacceleratora) function, which will handle it as though it were a standard key message instead of a system character-key message. This allows accelerator keys to be used with the active window even if the active window does not have the keyboard focus. For enhanced 101- and 102-key keyboards, extended keys are the right ALT and CTRL keys on the main section of the keyboard; the INS, DEL, HOME, END, PAGE UP, PAGE DOWN and arrow keys in the clusters to the left of the numeric keypad; the PRINT SCRN key; the BREAK key; the NUMLOCK key; and the divide (/) and ENTER keys in the numeric keypad. Other keyboards may support the extended-key bit in the parameter.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/menurc/wm-syschar#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_SYSCHAR = 262;

    /// <summary>Sent to the window with the keyboard focus when a WM\_SYSKEYDOWN message is translated by the TranslateMessage function.</summary>
    /// <returns>An application should return zero if it processes this message.</returns>
    /// <remarks>For enhanced 101- and 102-key keyboards, extended keys are the right ALT and CTRL keys on the main section of the keyboard; the INS, DEL, HOME, END, PAGE UP, PAGE DOWN, and arrow keys in the clusters to the left of the numeric keypad; and the divide (/) and ENTER keys in the numeric keypad. Other keyboards may support the extended-key bit in the *lParam* parameter.</remarks>
    public const int WM_SYSDEADCHAR = 263;

    public const int WM_KEYLAST = 265;

    /// <summary>Sent immediately before the IME generates the composition string as a result of a keystroke. A window receives this message through its WindowProc function.</summary>
    /// <returns>
    /// <para>This message has no parameters.</para>
    /// <para>This message has no return value.</para>
    /// </returns>
    /// <remarks>
    /// <para>This message is a notification to an IME window to open its composition window. An application should process this message if it displays composition characters itself. If an application has created an IME window, it should pass this message to that window. The [**DefWindowProc**](/windows/desktop/api/winuser/nf-winuser-defwindowproca) function processes the message by passing it to the default IME window.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/Intl/wm-ime-startcomposition#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_IME_STARTCOMPOSITION = 269;

    /// <summary>Sent to an application when the IME ends composition. A window receives this message through its WindowProc function.</summary>
    /// <returns>
    /// <para>This message has no parameters.</para>
    /// <para>This message has no return value.</para>
    /// </returns>
    /// <remarks>
    /// <para>An application should process this message if it displays composition characters itself. If the application has created an IME window, it should pass this message to that window. The [**DefWindowProc**](/windows/desktop/api/winuser/nf-winuser-defwindowproca)  function processes this message by passing it to the default IME window.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/Intl/wm-ime-endcomposition#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_IME_ENDCOMPOSITION = 270;

    /// <summary>Sent to an application when the IME changes composition status as a result of a keystroke. A window receives this message through its WindowProc function.</summary>
    /// <returns>This message has no return value.</returns>
    /// <remarks>
    /// <para>An application should process this message if it displays composition characters itself. Otherwise, it should send the message to the IME window. If the application has created an IME window, it should pass this message to that window. The [**DefWindowProc**](/windows/desktop/api/winuser/nf-winuser-defwindowproca)  function processes this message by passing it to the default IME window. The IME window processes this message by updating its appearance based on the change flag specified. An application can call [**ImmGetCompositionString**](/windows/desktop/api/Imm/nf-imm-immgetcompositionstringa) to retrieve the new composition status. If none of the GCS\_ values are set, the message indicates that the current composition has been canceled and applications that draw the composition string should delete the string.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/Intl/wm-ime-composition#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_IME_COMPOSITION = 271;

    public const int WM_IME_KEYLAST = 271;

    /// <summary>Sent to the dialog box procedure immediately before a dialog box is displayed. Dialog box procedures typically use this message to initialize controls and carry out any other initialization tasks that affect the appearance of the dialog box.</summary>
    /// <returns>
    /// <para>The dialog box procedure should return **TRUE** to direct the system to set the keyboard focus to the control specified by *wParam*. Otherwise, it should return **FALSE** to prevent the system from setting the default keyboard focus. The dialog box procedure should return the value directly. The **DWL\_MSGRESULT** value set by the [**SetWindowLong**](/windows/desktop/api/winuser/nf-winuser-setwindowlonga) function is ignored.</para>
    /// </returns>
    /// <remarks>
    /// <para>The control to receive the default keyboard focus is always the first control in the dialog box that is visible, not disabled, and that has the **WS\_TABSTOP** style. When the dialog box procedure returns **TRUE**, the system checks the control to ensure that the procedure has not disabled it. If it has been disabled, the system sets the keyboard focus to the next control that is visible, not disabled, and has the **WS\_TABSTOP**. An application can return **FALSE** only if it has set the keyboard focus to one of the controls of the dialog box.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/dlgbox/wm-initdialog#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_INITDIALOG = 272;

    /// <summary>Sent when the user selects a command item from a menu, when a control sends a notification message to its parent window, or when an accelerator keystroke is translated.</summary>
    /// <returns>If an application processes this message, it should return zero.</returns>
    /// <remarks>
    /// <para>Use of the *wParam* and *lParam* parameters are summarized here.</para>
    /// <para>| Message Source | wParam (high word)                | wParam (low word)                | lParam                       | |----------------|-----------------------------------|----------------------------------|------------------------------| | Menu           | 0                                 | Menu identifier (IDM\_\*)        | 0                            | | Accelerator    | 1                                 | Accelerator identifier (IDM\_\*) | 0                            | | Control        | Control-defined notification code | Control identifier               | Handle to the control window |</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/menurc/wm-command#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_COMMAND = 273;

    /// <summary>A window receives this message when the user chooses a command from the Window menu (formerly known as the system or control menu) or when the user chooses the maximize button, minimize button, restore button, or close button.</summary>
    /// <returns>An application should return zero if it processes this message.</returns>
    /// <remarks>
    /// <para>To obtain the position coordinates in screen coordinates, use the following code:</para>
    /// <para></para>
    /// <para>This doc was truncated.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/menurc/wm-syscommand#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_SYSCOMMAND = 274;

    /// <summary>Posted to the installing thread's message queue when a timer expires. The message is posted by the GetMessage or PeekMessage function.</summary>
    /// <returns>
    /// <para>Type: **LRESULT** An application should return zero if it processes this message.</para>
    /// </returns>
    /// <remarks>
    /// <para>You can process the message by providing a **WM\_TIMER** case in the window procedure. Otherwise, [**DispatchMessage**](/windows/win32/api/winuser/nf-winuser-dispatchmessage) will call the [*TimerProc*](/windows/win32/api/winuser/nc-winuser-timerproc) callback function specified in the call to the [**SetTimer**](/windows/win32/api/winuser/nf-winuser-settimer) function used to install the timer. The **WM\_TIMER** message is a low-priority message. The [**GetMessage**](/windows/win32/api/winuser/nf-winuser-getmessage) and [**PeekMessage**](/windows/win32/api/winuser/nf-winuser-peekmessagea) functions post this message only when no other higher-priority messages are in the thread's message queue.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/winmsg/wm-timer#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_TIMER = 275;

    /// <summary>The WM\_HSCROLL message is sent to a window when a scroll event occurs in the window's standard horizontal scroll bar.</summary>
    /// <returns>If an application processes this message, it should return zero.</returns>
    /// <remarks>
    /// <para>The SB\_THUMBTRACK request code is typically used by applications that provide feedback as the user drags the scroll box. If an application scrolls the content of the window, it must also reset the position of the scroll box by using the [**SetScrollPos**](/windows/desktop/api/Winuser/nf-winuser-setscrollpos) function. Note that the **WM\_HSCROLL** message carries only 16 bits of scroll box position data. Thus, applications that rely solely on **WM\_HSCROLL** (and [**WM\_VSCROLL**](wm-vscroll.md)) for scroll position data have a practical maximum position value of 65,535. However, because the [**SetScrollInfo**](/windows/desktop/api/Winuser/nf-winuser-setscrollinfo), [**SetScrollPos**](/windows/desktop/api/Winuser/nf-winuser-setscrollpos), [**SetScrollRange**](/windows/desktop/api/Winuser/nf-winuser-setscrollrange), [**GetScrollInfo**](/windows/desktop/api/Winuser/nf-winuser-getscrollinfo), [**GetScrollPos**](/windows/desktop/api/Winuser/nf-winuser-getscrollpos), and [**GetScrollRange**](/windows/desktop/api/Winuser/nf-winuser-getscrollrange) functions support 32-bit scroll bar position data, there is a way to circumvent the 16-bit barrier of the **WM\_HSCROLL** and [**WM\_VSCROLL**](wm-vscroll.md) messages. See **GetScrollInfo** for a description of the technique.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/Controls/wm-hscroll#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_HSCROLL = 276;

    /// <summary>The WM\_VSCROLL message is sent to a window when a scroll event occurs in the window's standard vertical scroll bar.</summary>
    /// <returns>If an application processes this message, it should return zero.</returns>
    /// <remarks>
    /// <para>The SB\_THUMBTRACK request code is typically used by applications that provide feedback as the user drags the scroll box. If an application scrolls the content of the window, it must also reset the position of the scroll box by using the [**SetScrollPos**](/windows/desktop/api/Winuser/nf-winuser-setscrollpos) function. Note that the **WM\_VSCROLL** message carries only 16 bits of scroll box position data. Thus, applications that rely solely on **WM\_VSCROLL** (and [**WM\_HSCROLL**](wm-hscroll.md)) for scroll position data have a practical maximum position value of 65,535. However, because the [**SetScrollInfo**](/windows/desktop/api/Winuser/nf-winuser-setscrollinfo), [**SetScrollPos**](/windows/desktop/api/Winuser/nf-winuser-setscrollpos), [**SetScrollRange**](/windows/desktop/api/Winuser/nf-winuser-setscrollrange), [**GetScrollInfo**](/windows/desktop/api/Winuser/nf-winuser-getscrollinfo), [**GetScrollPos**](/windows/desktop/api/Winuser/nf-winuser-getscrollpos), and [**GetScrollRange**](/windows/desktop/api/Winuser/nf-winuser-getscrollrange) functions support 32-bit scroll bar position data, there is a way to circumvent the 16-bit barrier of the [**WM\_HSCROLL**](wm-hscroll.md) and **WM\_VSCROLL** messages. See **GetScrollInfo** for a description of the technique.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/Controls/wm-vscroll#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_VSCROLL = 277;

    /// <summary>Sent when a menu is about to become active.</summary>
    /// <returns>If an application processes this message, it should return zero.</returns>
    /// <remarks>A **WM\_INITMENU** message is sent only when a menu is first accessed; only one **WM\_INITMENU** message is generated for each access. For example, moving the mouse across several menu items while holding down the button does not generate new messages. **WM\_INITMENU** does not provide information about menu items.</remarks>
    public const int WM_INITMENU = 278;

    /// <summary>WM_INITMENUPOPUP message - Sent when a drop-down menu or submenu is about to become active. This allows an application to modify the menu before it is displayed, without changing the entire menu.</summary>
    /// <returns>If an application processes this message, it should return zero.</returns>
    /// <remarks>
    /// <para><see href="https://learn.microsoft.com/windows/win32/menurc/wm-initmenupopup">Learn more about this API from docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_INITMENUPOPUP = 279;

    /// <summary>Passes information about a gesture.</summary>
    /// <returns>
    /// <para>If an application processes this message, it should return 0. If the application does not process the message, it must call [DefWindowProc](/windows/win32/api/winuser/nf-winuser-defwindowproca). Not doing so will cause the application to leak memory because the touch input handle will not be closed and associated process memory will not be freed.</para>
    /// </returns>
    /// <remarks>
    /// <para>The following table lists the supported gesture commands.</para>
    /// <para>| Gesture ID            | Value (*dwID*) | Description                                                                                                                                                                                                                                                                          | |-----------------------|----------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------| | **GID\_BEGIN**        | 1              | Indicates a generic gesture is beginning.                                                                                                                                                                                                                                            | | **GID\_END**          | 2              | Indicates a generic gesture end.                                                                                                                                                                                                                                                     | | **GID\_ZOOM**         | 3              | Indicates zoom start, zoom move, or zoom stop. The first **GID\_ZOOM** command message begins a zoom but does not cause any zooming. The second **GID\_ZOOM** command triggers a zoom relative to the state contained in the first **GID\_ZOOM**.                                    | | **GID\_PAN**          | 4              | Indicates pan move or pan start. The first **GID\_PAN** command indicates a pan start but does not perform any panning. With the second **GID\_PAN** command message, the application will begin panning.                                                                            | | **GID\_ROTATE**       | 5              | Indicates rotate move or rotate start. The first **GID\_ROTATE** command message indicates a rotate move or rotate start but will not rotate. The second **GID\_ROTATE** command message will trigger a rotation operation relative to state contained in the first **GID\_ROTATE**. | | **GID\_TWOFINGERTAP** | 6              | Indicates two-finger tap gesture.                                                                                                                                                                                                                                                    | | **GID\_PRESSANDTAP**  | 7              | Indicates the press and tap gesture.                                                                                                                                                                                                                                                 |</para>
    /// <para>> [!Note] > In order to enable legacy support, messages with the **GID\_BEGIN** and **GID\_END** gesture commands need to be forwarded using [DefWindowProc](/windows/win32/api/winuser/nf-winuser-defwindowproca).</para>
    /// <para>The following table indicates the gesture arguments passed in the *lParam* and *wParam* parameters.</para>
    /// <para>| Gesture ID            | Gesture        | *ullArgument*                                                                                                                                                                                                                                                                                                                                                                                            | *ptsLocation* in [**GestureInfo**](/windows/desktop/api/winuser/nf-winuser-getgestureinfo) structure                                                  | |-----------------------|----------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|-------------------------------------------------------------------------------------------------------------------| | **GID\_ZOOM**         | Zoom In/Out    | Indicates the distance between the two points.                                                                                                                                                                                                                                                                                                                                                           | Indicates the center of the zoom.                                                                                 | | **GID\_PAN**          | Pan            | Indicates the distance between the two points.                                                                                                                                                                                                                                                                                                                                                           | Indicates the current position of the pan.                                                                        | | **GID\_ROTATE**       | Rotate (pivot) | Indicates the angle of rotation if the **GF\_BEGIN** flag is set. Otherwise, this is the angle change since the rotation has started. This is signed to indicate the direction of the rotation. Use the [**GID\_ROTATE\_ANGLE\_FROM\_ARGUMENT**](/windows/desktop/api/winuser/nf-winuser-gid_rotate_angle_from_argument) and [**GID\_ROTATE\_ANGLE\_TO\_ARGUMENT**](/windows/desktop/api/winuser/nf-winuser-gid_rotate_angle_to_argument) macros to get and set the angle value. | This indicates the center of the rotation which is the stationary point that the target object is rotated around. | | **GID\_TWOFINGERTAP** | Two-finger Tap | Indicates the distance between the two fingers.                                                                                                                                                                                                                                                                                                                                                          | Indicates the center of the two fingers.                                                                          | | **GID\_PRESSANDTAP**  | Press and Tap  | Indicates the delta between the first finger and the second finger. This value is stored in the lower 32 bits of the *ullArgument* in a **POINT** structure.                                                                                                                                                                                                                                             | Indicates the position that the first finger comes down on.                                                       |</para>
    /// <para>> [!Note] > All distances and positions are provided in physical screen coordinates.</para>
    /// <para>> [!Note] > The *dwID* and *ullArgument* parameters should only be considered to be accompanying the GID\_\* commands and should not be altered by applications.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/wintouch/wm-gesture#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_GESTURE = 281;

    /// <summary>Gives you a chance to set the gesture configuration.</summary>
    /// <returns>A value should be returned from [DefWindowProc](/windows/win32/api/winuser/nf-winuser-defwindowproca).</returns>
    /// <remarks>
    /// <para>When the **WM\_GESTURENOTIFY** message is received, the application can use [**SetGestureConfig**](/windows/desktop/api/winuser/nf-winuser-setgestureconfig) to specify the gestures to receive. This message should always be bubbled up using the [DefWindowProc](/windows/win32/api/winuser/nf-winuser-defwindowproca) function. > [!Note] > Handling the **WM\_GESTURENOTIFY** message will change the gesture configuration for the lifetime of the Window, not just for the next gesture.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/wintouch/wm-gesturenotify#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_GESTURENOTIFY = 282;

    /// <summary>Sent to a menu's owner window when the user selects a menu item.</summary>
    /// <returns>If an application processes this message, it should return zero.</returns>
    /// <remarks>
    /// <para>If the high-order word of *wParam* contains 0xFFFF and the *lParam* parameter contains **NULL**, the system has closed the menu. Do not use the value  1 for the high-order word of *wParam*, because this value is specified as (**UINT**) [**HIWORD**](/previous-versions/windows/desktop/legacy/ms632657(v=vs.85))(*wParam*). If the value is 0xFFFF, it would be interpreted as 0x0000FFFF, not  1, because of the cast to a **UINT**.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/menurc/wm-menuselect#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_MENUSELECT = 287;

    /// <summary>Sent when a menu is active and the user presses a key that does not correspond to any mnemonic or accelerator key. This message is sent to the window that owns the menu.</summary>
    /// <returns>
    /// <para>An application that processes this message should return one of the following values in the high-order word of the return value.</para>
    /// <para>| Return code/value                                                                                                                                  | Description                                                                                                                                                                              | |----------------------------------------------------------------------------------------------------------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------| | <dl> <dt>**MNC\_CLOSE**</dt> <dt>1</dt> </dl>   | Informs the system that it should close the active menu.<br/>                                                                                                                      | | <dl> <dt>**MNC\_EXECUTE**</dt> <dt>2</dt> </dl> | Informs the system that it should choose the item specified in the low-order word of the return value. The owner window receives a [**WM\_COMMAND**](wm-command.md) message.<br/> | | <dl> <dt>**MNC\_IGNORE**</dt> <dt>0</dt> </dl>  | Informs the system that it should discard the character the user pressed and create a short beep on the system speaker.<br/>                                                       | | <dl> <dt>**MNC\_SELECT**</dt> <dt>3</dt> </dl>  | Informs the system that it should select the item specified in the low-order word of the return value. <br/>                                                                       |</para>
    /// </returns>
    /// <remarks>
    /// <para>The low-order word is ignored if the high-order word contains 0 or 1. An application should process this message when an accelerator is used to select a menu item that displays a bitmap.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/menurc/wm-menuchar#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_MENUCHAR = 288;

    /// <summary>Sent to the owner window of a modal dialog box or menu that is entering an idle state. A modal dialog box or menu enters an idle state when no messages are waiting in its queue after it has processed one or more previous messages.</summary>
    /// <returns>An application should return zero if it processes this message.</returns>
    /// <remarks>You can suppress the **WM\_ENTERIDLE** message for a dialog box by creating the dialog box with the **DS\_NOIDLEMSG** style.</remarks>
    public const int WM_ENTERIDLE = 289;

    /// <summary>Sent when the user releases the right mouse button while the cursor is on a menu item.</summary>
    /// <remarks>The **WM\_MENURBUTTONUP** message allows applications to provide a context-sensitive menu also known as a shortcut menu for the menu item specified in this message. To display a context-sensitive menu for a menu item, call the [**TrackPopupMenuEx**](/windows/desktop/api/Winuser/nf-winuser-trackpopupmenuex) function with **TPM\_RECURSE**.</remarks>
    public const int WM_MENURBUTTONUP = 290;

    /// <summary>Sent to the owner of a drag-and-drop menu when the user drags a menu item.</summary>
    /// <returns>
    /// <para>The application should return one of the following values.</para>
    /// <para>| Return code/value                                                                                                                                   | Description                                                                           | |-----------------------------------------------------------------------------------------------------------------------------------------------------|---------------------------------------------------------------------------------------| | <dl> <dt>**MND\_CONTINUE**</dt> <dt>0</dt> </dl> | Menu should remain active. If the mouse is released, it should be ignored.<br/> | | <dl> <dt>**MND\_ENDMENU**</dt> <dt>1</dt> </dl>  | Menu should be ended.<br/>                                                      |</para>
    /// </returns>
    /// <remarks>
    /// <para>The application can call the [**DoDragDrop**](/windows/win32/api/ole2/nf-ole2-dodragdrop) function in response to this message. To create a drag-and-drop menu, call [**SetMenuInfo**](/windows/desktop/api/Winuser/nf-winuser-setmenuinfo) with **MNS\_DRAGDROP**.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/menurc/wm-menudrag#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_MENUDRAG = 291;

    /// <summary>Sent to the owner of a drag-and-drop menu when the mouse cursor enters a menu item or moves from the center of the item to the top or bottom of the item.</summary>
    /// <returns>
    /// <para>The application should return one of the following values.</para>
    /// <para>| Return code/value                                                                                                                                                | Description                                                                                                            | |------------------------------------------------------------------------------------------------------------------------------------------------------------------|------------------------------------------------------------------------------------------------------------------------| | <dl> <dt>**MNGO\_NOERROR**</dt> <dt>0x00000001</dt> </dl>     | An interface pointer was returned in the **pvObj** member of [**MENUGETOBJECTINFO**](/windows/win32/api/winuser/ns-winuser-menugetobjectinfo)<br/> | | <dl> <dt>**MNGO\_NOINTERFACE**</dt> <dt>0x00000000</dt> </dl> | The interface is not supported.<br/>                                                                             |</para>
    /// </returns>
    /// <remarks>
    /// <para><see href="https://learn.microsoft.com/windows/win32/menurc/wm-menugetobject">Learn more about this API from docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_MENUGETOBJECT = 292;

    /// <summary>Sent when a drop-down menu or submenu has been destroyed.</summary>
    /// <remarks>If an application receives a [**WM\_INITMENUPOPUP**](wm-initmenupopup.md) message, it will receive a **WM\_UNINITMENUPOPUP** message.</remarks>
    public const int WM_UNINITMENUPOPUP = 293;

    /// <summary>Sent when the user makes a selection from a menu.</summary>
    /// <remarks>
    /// <para>The **WM\_MENUCOMMAND** message gives you a handle to the menu so you can access the menu data in the [**MENUINFO**](/windows/win32/api/winuser/ns-winuser-menuinfo) structure and also gives you the index of the selected item, which is typically what applications need. In contrast, the [**WM\_COMMAND**](wm-command.md) message gives you the menu item identifier. The **WM\_MENUCOMMAND** message is sent only for menus that are defined with the **MNS\_NOTIFYBYPOS** flag set in the **dwStyle** member of the [**MENUINFO**](/windows/win32/api/winuser/ns-winuser-menuinfo) structure.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/menurc/wm-menucommand#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_MENUCOMMAND = 294;

    /// <summary>An application sends the WM\_CHANGEUISTATE message to indicate that the UI state should be changed.</summary>
    /// <remarks>
    /// <para>A window should send this message to itself or its parent when it must change the UI state elements of all windows in the same hierarchy. The window procedure must let [**DefWindowProc**](/windows/desktop/api/winuser/nf-winuser-defwindowproca) process this message so that the entire window tree has a consistent UI state. When the top-level window receives the **WM\_CHANGEUISTATE** message, it sends a [**WM\_UPDATEUISTATE**](wm-updateuistate.md) message with the same parameters to all child windows. When the system processes the **WM\_UPDATEUISTATE** message, it makes the change in the UI state. If the low-order word of *wParam* is UIS\_INITIALIZE, the system will send the [**WM\_UPDATEUISTATE**](wm-updateuistate.md) message with a UI state based on the last input event. For example, if the last input came from the mouse, the system will hide the keyboard cues. And, if the last input came from the keyboard, the system will show the keyboard cues. If the state that results from processing **WM\_CHANGEUISTATE** is the same as the old state, [**DefWindowProc**](/windows/desktop/api/winuser/nf-winuser-defwindowproca) does not send this message.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/menurc/wm-changeuistate#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_CHANGEUISTATE = 295;

    /// <summary>An application sends the WM\_UPDATEUISTATE message to change the UI state for the specified window and all its child windows.</summary>
    /// <remarks>
    /// <para>A window should send this message to change the UI state of all its child windows. In contrast to the [**WM\_CHANGEUISTATE**](wm-changeuistate.md) message, which is a notification, when [**DefWindowProc**](/windows/desktop/api/winuser/nf-winuser-defwindowproca) processes the **WM\_UPDATEUISTATE** message it changes the UI state and propagates the changes to all child windows. The [**DefWindowProc**](/windows/desktop/api/winuser/nf-winuser-defwindowproca) function updates the UI state according to the *wParam* value. If the UI state is modified, the function sends the message to all the immediate child windows. **DefWindowProc** also sends this message when it receives a [**WM\_CHANGEUISTATE**](wm-changeuistate.md) message notifying the system that a child window intends to modify the UI state.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/menurc/wm-updateuistate#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_UPDATEUISTATE = 296;

    /// <summary>An application sends the WM\_QUERYUISTATE message to retrieve the UI state for a window.</summary>
    /// <returns>
    /// <para>The return value is **NULL** if the focus indicators and the keyboard accelerators are visible. Otherwise, the return value can be one or more of the following values.</para>
    /// <para>| Return code/value                                                                                                                                       | Description                                                                 | |---------------------------------------------------------------------------------------------------------------------------------------------------------|-----------------------------------------------------------------------------| | <dl> <dt>**UISF\_ACTIVE**</dt> <dt>0x4</dt> </dl>    | A control should be drawn in the style used for active controls.<br/> | | <dl> <dt>**UISF\_HIDEACCEL**</dt> <dt>0x2</dt> </dl> | Keyboard accelerators are hidden.<br/>                                | | <dl> <dt>**UISF\_HIDEFOCUS**</dt> <dt>0x1</dt> </dl> | Focus indicators are hidden.<br/>                                     |</para>
    /// </returns>
    /// <remarks>
    /// <para><see href="https://learn.microsoft.com/windows/win32/menurc/wm-queryuistate">Learn more about this API from docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_QUERYUISTATE = 297;

    public const int WM_CTLCOLORMSGBOX = 306;

    /// <summary>An edit control that is not read-only or disabled sends the WM\_CTLCOLOREDIT message to its parent window when the control is about to be drawn.</summary>
    /// <returns>If an application processes this message, it must return the handle of a brush. The system uses the brush to paint the background of the edit control.</returns>
    /// <remarks>
    /// <para>If the application returns a brush that it created (for example, by using the [**CreateSolidBrush**](/windows/desktop/api/wingdi/nf-wingdi-createsolidbrush) or [**CreateBrushIndirect**](/windows/desktop/api/wingdi/nf-wingdi-createbrushindirect) function), the application must free the brush. If the application returns a system brush (for example, one that was retrieved by the [**GetStockObject**](/windows/desktop/api/wingdi/nf-wingdi-getstockobject) or [**GetSysColorBrush**](/windows/desktop/api/winuser/nf-winuser-getsyscolorbrush) function), the application does not need to free the brush. By default, the [**DefWindowProc**](/windows/desktop/api/winuser/nf-winuser-defwindowproca) function selects the default system colors for the edit control. Read-only or disabled edit controls do not send the **WM\_CTLCOLOREDIT** message; instead, they send the [**WM\_CTLCOLORSTATIC**](wm-ctlcolorstatic.md) message. The **WM\_CTLCOLOREDIT** message is never sent between threads, it is only sent within the same thread. If a dialog box procedure handles this message, it should cast the desired return value to a **INT\_PTR** and return the value directly. If the dialog box procedure returns **FALSE**, then default message handling is performed. The DWL\_MSGRESULT value set by the [**SetWindowLong**](/windows/desktop/api/winuser/nf-winuser-setwindowlonga) function is ignored. **Rich Edit:** This message is not supported. To set the background color for a rich edit control, use the [**EM\_SETBKGNDCOLOR**](em-setbkgndcolor.md) message.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/Controls/wm-ctlcoloredit#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_CTLCOLOREDIT = 307;

    /// <summary>Sent to the parent window of a list box before the system draws the list box. By responding to this message, the parent window can set the text and background colors of the list box by using the specified display device context handle.</summary>
    /// <returns>If an application processes this message, it must return a handle to a brush. The system uses the brush to paint the background of the list box.</returns>
    /// <remarks>
    /// <para>By default, the [**DefWindowProc**](/windows/desktop/api/winuser/nf-winuser-defwindowproca) function selects the default system colors for the list box. The **WM\_CTLCOLORLISTBOX** message is never sent between threads. It is sent only within one thread. If a dialog box procedure handles this message, it should cast the desired return value to a **INT\_PTR** and return the value directly. If the dialog box procedure returns **FALSE**, then default message handling is performed. The **DWL\_MSGRESULT** value set by the [**SetWindowLong**](/windows/desktop/api/winuser/nf-winuser-setwindowlonga) function is ignored.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/Controls/wm-ctlcolorlistbox#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_CTLCOLORLISTBOX = 308;

    /// <summary>The WM\_CTLCOLORBTN message is sent to the parent window of a button before drawing the button. The parent window can change the button's text and background colors. However, only owner-drawn buttons respond to the parent window processing this message.</summary>
    /// <returns>If an application processes this message, it must return a handle to a brush. The system uses the brush to paint the background of the button.</returns>
    /// <remarks>
    /// <para>If the application returns a brush that it created (for example, by using the [**CreateSolidBrush**](/windows/desktop/api/wingdi/nf-wingdi-createsolidbrush) or [**CreateBrushIndirect**](/windows/desktop/api/wingdi/nf-wingdi-createbrushindirect) function), the application must free the brush. If the application returns a system brush (for example, one that was retrieved by the [**GetStockObject**](/windows/desktop/api/wingdi/nf-wingdi-getstockobject) or [**GetSysColorBrush**](/windows/desktop/api/winuser/nf-winuser-getsyscolorbrush) function), the application does not need to free the brush. By default, the [**DefWindowProc**](/windows/desktop/api/winuser/nf-winuser-defwindowproca) function selects the default system colors for the button. Buttons with the [**BS\_PUSHBUTTON**](button-styles.md), [**BS\_DEFPUSHBUTTON**](button-styles.md), or [**BS\_PUSHLIKE**](button-styles.md) styles do not use the returned brush. Buttons with these styles are always drawn with the default system colors. Drawing push buttons requires several different brushes-face, highlight, and shadow-but the **WM\_CTLCOLORBTN** message allows only one brush to be returned. To provide a custom appearance for push buttons, use an owner-drawn button. For more information, see [Creating Owner-Drawn Controls](user-controls-intro.md). The **WM\_CTLCOLORBTN** message is never sent between threads. It is sent only within one thread. The text color of a check box or radio button applies to the box or button, its check mark, and the text. The focus rectangle for these buttons remains the system default color (typically black). The text color of a group box applies to the text but not to the line that defines the box. The text color of a push button applies only to its focus rectangle; it does not affect the color of the text. If a dialog box procedure handles this message, it should cast the desired return value to a **INT\_PTR** and return the value directly. If the dialog box procedure returns **FALSE**, then default message handling is performed. The DWL\_MSGRESULT value set by the [**SetWindowLong**](/windows/desktop/api/winuser/nf-winuser-setwindowlonga) function is ignored.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/Controls/wm-ctlcolorbtn#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_CTLCOLORBTN = 309;

    /// <summary>Sent to a dialog box before the system draws the dialog box. By responding to this message, the dialog box can set its text and background colors using the specified display device context handle.</summary>
    /// <returns>If an application processes this message, it must return a handle to a brush. The system uses the brush to paint the background of the dialog box.</returns>
    /// <remarks>
    /// <para>By default, the [**DefWindowProc**](/windows/desktop/api/winuser/nf-winuser-defwindowproca) function selects the default system colors for the dialog box. The system does not automatically destroy the returned brush. It is the application's responsibility to destroy the brush when it is no longer needed. The **WM\_CTLCOLORDLG** message is never sent between threads. It is sent only within one thread. Note that the **WM\_CTLCOLORDLG** message is sent to the dialog box itself; all of the other **WM\_CTLCOLOR\*** messages are sent to the owner of the control. If a dialog box procedure handles this message, it should cast the desired return value to an **INT\_PTR** and return the value directly. If the dialog box procedure returns **FALSE**, then default message handling is performed. The **DWL\_MSGRESULT** value set by the [**SetWindowLong**](/windows/desktop/api/winuser/nf-winuser-setwindowlonga) function is ignored.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/dlgbox/wm-ctlcolordlg#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_CTLCOLORDLG = 310;

    /// <summary>The WM\_CTLCOLORSCROLLBAR message is sent to the parent window of a scroll bar control when the control is about to be drawn.</summary>
    /// <returns>If an application processes this message, it must return the handle to a brush. The system uses the brush to paint the background of the scroll bar control.</returns>
    /// <remarks>
    /// <para>If the application returns a brush that it created (for example, by using the [**CreateSolidBrush**](/windows/desktop/api/wingdi/nf-wingdi-createsolidbrush) or [**CreateBrushIndirect**](/windows/desktop/api/wingdi/nf-wingdi-createbrushindirect) function), the application must free the brush. If the application returns a system brush (for example, one that was retrieved by the [**GetStockObject**](/windows/desktop/api/wingdi/nf-wingdi-getstockobject) or [**GetSysColorBrush**](/windows/desktop/api/winuser/nf-winuser-getsyscolorbrush) function), the application does not need to free the brush. By default, the [**DefWindowProc**](/windows/desktop/api/winuser/nf-winuser-defwindowproca) function selects the default system colors for the scroll bar control. The **WM\_CTLCOLORSCROLLBAR** message is never sent between threads; it is only sent within the same thread. If a dialog box procedure handles this message, it should cast the desired return value to a **INT\_PTR** and return the value directly. If the dialog box procedure returns **FALSE**, then default message handling is performed. The DWL\_MSGRESULT value set by the [**SetWindowLong**](/windows/desktop/api/winuser/nf-winuser-setwindowlonga) function is ignored. The **WM\_CTLCOLORSCROLLBAR** message is used only by child scroll bar controls. Scrollbars attached to a window (WS\_SCROLL and WS\_VSCROLL) do not generate this message. To customize the appearance of scrollbars attached to a window, use the flat scroll bar functions.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/Controls/wm-ctlcolorscrollbar#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_CTLCOLORSCROLLBAR = 311;

    /// <summary>A static control, or an edit control that is read-only or disabled, sends the WM\_CTLCOLORSTATIC message to its parent window when the control is about to be drawn.</summary>
    /// <returns>If an application processes this message, the return value is a handle to a brush that the system uses to paint the background of the static control.</returns>
    /// <remarks>
    /// <para>If the application returns a brush that it created (for example, by using the [**CreateSolidBrush**](/windows/desktop/api/wingdi/nf-wingdi-createsolidbrush) or [**CreateBrushIndirect**](/windows/desktop/api/wingdi/nf-wingdi-createbrushindirect) function), the application must free the brush. If the application returns a system brush (for example, one that was retrieved by the [**GetStockObject**](/windows/desktop/api/wingdi/nf-wingdi-getstockobject) or [**GetSysColorBrush**](/windows/desktop/api/winuser/nf-winuser-getsyscolorbrush) function), the application does not need to free the brush. By default, the [**DefWindowProc**](/windows/desktop/api/winuser/nf-winuser-defwindowproca) function selects the default system colors for the static control. You can set the text background color of a disabled edit control, but you cannot set the text foreground color. The system always uses COLOR\_GRAYTEXT. Edit controls that are not read-only or disabled do not send the **WM\_CTLCOLORSTATIC** message; instead, they send the [**WM\_CTLCOLOREDIT**](wm-ctlcoloredit.md) message. The **WM\_CTLCOLORSTATIC** message is never sent between threads; it is sent only within the same thread. If a dialog box procedure handles this message, it should cast the desired return value to a **INT\_PTR** and return the value directly. If the dialog box procedure returns **FALSE**, then default message handling is performed. The DWL\_MSGRESULT value set by the [**SetWindowLong**](/windows/desktop/api/winuser/nf-winuser-setwindowlonga) function is ignored.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/Controls/wm-ctlcolorstatic#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_CTLCOLORSTATIC = 312;

    /// <summary>Posted to a window when the cursor moves. If the mouse is not captured, the message is posted to the window that contains the cursor. Otherwise, the message is posted to the window that has captured the mouse.</summary>
    /// <returns>If an application processes this message, it should return zero.</returns>
    /// <remarks>
    /// <para>Use the following code to obtain the horizontal and vertical position:</para>
    /// <para></para>
    /// <para>This doc was truncated.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/inputdev/wm-mousemove#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_MOUSEMOVE = 512;

    /// <summary>Posted when the user presses the left mouse button while the cursor is in the client area of a window.</summary>
    /// <returns>If an application processes this message, it should return zero.</returns>
    /// <remarks>
    /// <para>As noted above, the x-coordinate is in the low-order **short** of the return value; the y-coordinate is in the high-order **short** (both represent *signed* values because they can take negative values on systems with multiple monitors). If the return value is assigned to a variable, you can use the [**MAKEPOINTS**](/windows/desktop/api/wingdi/nf-wingdi-makepoints) macro to obtain a [**POINTS**](/windows/win32/api/windef/ns-windef-points) structure from the return value. You can also use the [**GET\_X\_LPARAM**](/windows/desktop/api/windowsx/nf-windowsx-get_x_lparam) or [**GET\_Y\_LPARAM**](/windows/desktop/api/windowsx/nf-windowsx-get_y_lparam) macro to extract the x- or y-coordinate. > [!IMPORTANT] > Do not use the [**LOWORD**](/previous-versions/windows/desktop/legacy/ms632659(v=vs.85)) or [**HIWORD**](/previous-versions/windows/desktop/legacy/ms632657(v=vs.85)) macros to extract the x- and y- coordinates of the cursor position because these macros return incorrect results on systems with multiple monitors. Systems with multiple monitors can have negative x- and y- coordinates, and **LOWORD** and **HIWORD** treat the coordinates as unsigned quantities.</para>
    /// <para>To detect that the ALT key was pressed, check whether [**GetKeyState**](/windows/win32/api/winuser/nf-winuser-getkeystate) with **VK\_MENU**  0. Note, this must not be [**GetAsyncKeyState**](/windows/win32/api/winuser/nf-winuser-getasynckeystate).</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/inputdev/wm-lbuttondown#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_LBUTTONDOWN = 513;

    /// <summary>Posted when the user releases the left mouse button while the cursor is in the client area of a window.</summary>
    /// <returns>If an application processes this message, it should return zero.</returns>
    /// <remarks>
    /// <para>Use the following code to obtain the horizontal and vertical position:</para>
    /// <para></para>
    /// <para>This doc was truncated.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/inputdev/wm-lbuttonup#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_LBUTTONUP = 514;

    /// <summary>Posted when the user double-clicks the left mouse button while the cursor is in the client area of a window.</summary>
    /// <returns>If an application processes this message, it should return zero.</returns>
    /// <remarks>
    /// <para>Use the following code to obtain the horizontal and vertical position:</para>
    /// <para></para>
    /// <para>This doc was truncated.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/inputdev/wm-lbuttondblclk#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_LBUTTONDBLCLK = 515;

    /// <summary>Posted when the user presses the right mouse button while the cursor is in the client area of a window.</summary>
    /// <returns>If an application processes this message, it should return zero.</returns>
    /// <remarks>
    /// <para>Use the following code to obtain the horizontal and vertical position:</para>
    /// <para></para>
    /// <para>This doc was truncated.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/inputdev/wm-rbuttondown#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_RBUTTONDOWN = 516;

    /// <summary>Posted when the user releases the right mouse button while the cursor is in the client area of a window.</summary>
    /// <returns>If an application processes this message, it should return zero.</returns>
    /// <remarks>
    /// <para>Use the following code to obtain the horizontal and vertical position:</para>
    /// <para></para>
    /// <para>This doc was truncated.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/inputdev/wm-rbuttonup#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_RBUTTONUP = 517;

    /// <summary>Posted when the user double-clicks the right mouse button while the cursor is in the client area of a window.</summary>
    /// <returns>If an application processes this message, it should return zero.</returns>
    /// <remarks>
    /// <para>Only windows that have the **CS\_DBLCLKS** style can receive **WM\_RBUTTONDBLCLK** messages, which the system generates whenever the user presses, releases, and again presses the right mouse button within the system's double-click time limit. Double-clicking the right mouse button actually generates four messages: [**WM\_RBUTTONDOWN**](wm-rbuttondown.md), [**WM\_RBUTTONUP**](wm-rbuttonup.md), **WM\_RBUTTONDBLCLK**, and **WM\_RBUTTONUP** again. Use the following code to obtain the horizontal and vertical position:</para>
    /// <para></para>
    /// <para>This doc was truncated.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/inputdev/wm-rbuttondblclk#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_RBUTTONDBLCLK = 518;

    /// <summary>Posted when the user presses the middle mouse button while the cursor is in the client area of a window.</summary>
    /// <returns>If an application processes this message, it should return zero.</returns>
    /// <remarks>
    /// <para>Use the following code to obtain the horizontal and vertical position:</para>
    /// <para></para>
    /// <para>This doc was truncated.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/inputdev/wm-mbuttondown#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_MBUTTONDOWN = 519;

    /// <summary>Posted when the user releases the middle mouse button while the cursor is in the client area of a window.</summary>
    /// <returns>If an application processes this message, it should return zero.</returns>
    /// <remarks>
    /// <para>Use the following code to obtain the horizontal and vertical position:</para>
    /// <para></para>
    /// <para>This doc was truncated.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/inputdev/wm-mbuttonup#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_MBUTTONUP = 520;

    /// <summary>Posted when the user double-clicks the middle mouse button while the cursor is in the client area of a window.</summary>
    /// <returns>If an application processes this message, it should return zero.</returns>
    /// <remarks>
    /// <para>Use the following code to obtain the horizontal and vertical position:</para>
    /// <para></para>
    /// <para>This doc was truncated.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/inputdev/wm-mbuttondblclk#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_MBUTTONDBLCLK = 521;

    /// <summary>Sent to the focus window when the mouse wheel is rotated.</summary>
    /// <returns>If an application processes this message, it should return zero.</returns>
    /// <remarks>
    /// <para>Use the following code to get the information in the *wParam* parameter:</para>
    /// <para></para>
    /// <para>This doc was truncated.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/inputdev/wm-mousewheel#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_MOUSEWHEEL = 522;

    /// <summary>Posted when the user presses the first or second X button while the cursor is in the client area of a window.</summary>
    /// <returns>If an application processes this message, it should return **TRUE**. For more information about processing the return value, see the Remarks section.</returns>
    /// <remarks>
    /// <para>Use the following code to get the information in the *wParam* parameter:</para>
    /// <para></para>
    /// <para>This doc was truncated.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/inputdev/wm-xbuttondown#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_XBUTTONDOWN = 523;

    /// <summary>Posted when the user releases the first or second X button while the cursor is in the client area of a window.</summary>
    /// <returns>If an application processes this message, it should return **TRUE**. For more information about processing the return value, see the Remarks section.</returns>
    /// <remarks>
    /// <para>Use the following code to get the information in the *wParam* parameter:</para>
    /// <para></para>
    /// <para>This doc was truncated.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/inputdev/wm-xbuttonup#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_XBUTTONUP = 524;

    /// <summary>Posted when the user double-clicks the first or second X button while the cursor is in the client area of a window.</summary>
    /// <returns>If an application processes this message, it should return **TRUE**. For more information about processing the return value, see the Remarks section.</returns>
    /// <remarks>
    /// <para>Use the following code to get the information in the *wParam* parameter:</para>
    /// <para></para>
    /// <para>This doc was truncated.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/inputdev/wm-xbuttondblclk#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_XBUTTONDBLCLK = 525;

    /// <summary>Sent to the active window when the mouse's horizontal scroll wheel is tilted or rotated.</summary>
    /// <returns>If an application processes this message, it should return zero.</returns>
    /// <remarks>
    /// <para>Use the following code to obtain the information in the *wParam* parameter.</para>
    /// <para></para>
    /// <para>This doc was truncated.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/inputdev/wm-mousehwheel#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_MOUSEHWHEEL = 526;

    public const int WM_MOUSELAST = 526;

    /// <summary>Sent to a window when a significant action occurs on a descendant window.</summary>
    /// <returns>
    /// <para>If the application processes this message, it returns zero. If the application does not process this message, it calls [**DefWindowProc**](/windows/win32/api/winuser/nf-winuser-defwindowproca).</para>
    /// </returns>
    /// <remarks>
    /// <para>This message is also sent to all ancestor windows of the child window, including the top-level window. All child windows, except those that have the **WS_EX_NOPARENTNOTIFY** extended window style, send this message to their parent windows. By default, child windows in a dialog box have the **WS_EX_NOPARENTNOTIFY** style, unless the [**CreateWindowEx**](/windows/win32/api/winuser/nf-winuser-createwindowexa) function is called to create the child window without this style. This notification provides the child window's ancestor windows an opportunity to examine the pointer information and, if required, capture the pointer using the pointer capture functions.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/inputmsg/wm-parentnotify#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_PARENTNOTIFY = 528;

    /// <summary>Notifies an application's main window procedure that a menu modal loop has been entered.</summary>
    /// <returns>An application should return zero if it processes this message.</returns>
    /// <remarks>The [**DefWindowProc**](/windows/desktop/api/winuser/nf-winuser-defwindowproca) function returns zero.</remarks>
    public const int WM_ENTERMENULOOP = 529;

    /// <summary>Notifies an application's main window procedure that a menu modal loop has been exited.</summary>
    /// <returns>An application should return zero if it processes this message.</returns>
    /// <remarks>The [**DefWindowProc**](/windows/desktop/api/winuser/nf-winuser-defwindowproca) function returns zero.</remarks>
    public const int WM_EXITMENULOOP = 530;

    /// <summary>Sent to an application when the right or left arrow key is used to switch between the menu bar and the system menu.</summary>
    /// <remarks>In responding to this message, the application can specify the menu to switch to in the **hmenuNext** member of [**MDINEXTMENU**](/windows/win32/api/winuser/ns-winuser-mdinextmenu) and the window to receive the menu notification messages in the **hwndNext** member of the **MDINEXTMENU** structure. You must set both members for the changes to take effect (they are initially **NULL**).</remarks>
    public const int WM_NEXTMENU = 531;

    /// <summary>Sent to a window that the user is resizing. By processing this message, an application can monitor the size and position of the drag rectangle and, if needed, change its size or position.</summary>
    /// <returns>
    /// <para>Type: **LRESULT** An application should return **TRUE** if it processes this message.</para>
    /// </returns>
    /// <remarks>
    /// <para><see href="https://learn.microsoft.com/windows/win32/winmsg/wm-sizing">Learn more about this API from docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_SIZING = 532;

    /// <summary>Sent to the window that is losing the mouse capture.</summary>
    /// <returns>An application should return zero if it processes this message.</returns>
    /// <remarks>
    /// <para>A window receives this message even if it calls [**ReleaseCapture**](/windows/win32/api/winuser/nf-winuser-releasecapture) itself. An application should not attempt to set the mouse capture in response to this message. When it receives this message, a window should redraw itself, if necessary, to reflect the new mouse-capture state.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/inputdev/wm-capturechanged#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_CAPTURECHANGED = 533;

    /// <summary>Sent to a window that the user is moving. By processing this message, an application can monitor the position of the drag rectangle and, if needed, change its position.</summary>
    /// <returns>
    /// <para>Type: **LRESULT** An application should return **TRUE** if it processes this message.</para>
    /// </returns>
    /// <remarks>
    /// <para><see href="https://learn.microsoft.com/windows/win32/winmsg/wm-moving">Learn more about this API from docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_MOVING = 534;

    /// <summary>Notifies applications that a power-management event has occurred.</summary>
    /// <returns>An application should return **TRUE** if it processes this message.</returns>
    /// <remarks>
    /// <para>The system always sends a [PBT\_APMRESUMEAUTOMATIC](pbt-apmresumeautomatic.md) message whenever the system resumes. If the system resumes in response to user input such as pressing a key, the system also sends a **PBT\_APMRESUMESUSPEND** message after sending PBT\_APMRESUMEAUTOMATIC. **WM\_POWERBROADCAST** messages do not distinguish between different low-power states. An application can determine only that the system is entering or has resumed from a low-power state; it cannot determine the specific power state. The system records details about power state transitions in the Windows System event log. To prevent the system from transitioning to a low-power state in Windows Vista, an application must call [**SetThreadExecutionState**](/windows/desktop/api/Winbase/nf-winbase-setthreadexecutionstate) to inform the system that it is in use. The following messages are not supported on any of the operating systems specified in the Requirements section: - PBT_APMQUERYSTANDBY - PBT_APMQUERYSTANDBYFAILED - PBT_APMSTANDBY - PBT_APMRESUMESTANDBY</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/Power/wm-powerbroadcast#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_POWERBROADCAST = 536;

    /// <summary>An application sends the WM\_MDICREATE message to a multiple-document interface (MDI) client window to create an MDI child window.</summary>
    /// <returns>
    /// <para>Type: **HWND** If the message succeeds, the return value is the handle to the new child window. If the message fails, the return value is **NULL**.</para>
    /// </returns>
    /// <remarks>
    /// <para>The MDI child window is created with the [**window style**](window-styles.md) bits **WS\_CHILD**, **WS\_CLIPSIBLINGS**, **WS\_CLIPCHILDREN**, **WS\_SYSMENU**, **WS\_CAPTION**, **WS\_THICKFRAME**, **WS\_MINIMIZEBOX**, and **WS\_MAXIMIZEBOX**, plus additional style bits specified in the [**MDICREATESTRUCT**](/windows/win32/api/winuser/ns-winuser-mdicreatestructa) structure. The system adds the title of the new child window to the window menu of the frame window. An application should use this message to create all child windows of the client window. If an MDI client window receives any message that changes the activation of its child windows while the active child window is maximized, the system restores the active child window and maximizes the newly activated child window. When an MDI child window is created, the system sends the [**WM\_CREATE**](wm-create.md) message to the window. The *lParam* parameter of the **WM\_CREATE** message contains a pointer to a [**CREATESTRUCT**](/windows/win32/api/winuser/ns-winuser-createstructa) structure. The *lpCreateParams* member of this structure contains a pointer to the [**MDICREATESTRUCT**](/windows/win32/api/winuser/ns-winuser-mdicreatestructa) structure passed with the **WM\_MDICREATE** message that created the MDI child window. An application should not send a second **WM\_MDICREATE** message while a **WM\_MDICREATE** message is still being processed. For example, it should not send a **WM\_MDICREATE** message while an MDI child window is processing its **WM\_MDICREATE** message.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/winmsg/wm-mdicreate#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_MDICREATE = 544;

    /// <summary>An application sends the WM\_MDIDESTROY message to a multiple-document interface (MDI) client window to close an MDI child window.</summary>
    /// <returns>
    /// <para>Type: **zero** This message always returns zero.</para>
    /// </returns>
    /// <remarks>
    /// <para>This message removes the title of the MDI child window from the MDI frame window and deactivates the child window. An application should use this message to close all MDI child windows. If an MDI client window receives a message that changes the activation of its child windows and the active MDI child window is maximized, the system restores the active child window and maximizes the newly activated child window.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/winmsg/wm-mdidestroy#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_MDIDESTROY = 545;

    /// <summary>An application sends the WM\_MDIACTIVATE message to a multiple-document interface (MDI) client window to instruct the client window to activate a different MDI child window.</summary>
    /// <returns>
    /// <para>Type: **LRESULT** If an application sends this message to an MDI client window, the return value is zero. An MDI child window should return zero if it processes this message.</para>
    /// </returns>
    /// <remarks>
    /// <para>As the client window processes this message, it sends **WM\_MDIACTIVATE** to the child window being deactivated and to the child window being activated. The message parameters received by an MDI child window are as follows: <dl> <dt> <span id="wParam"></span><span id="wparam"></span><span id="WPARAM"></span>*wParam* </dt> <dd> A handle to the MDI child window being deactivated. </dd> <dt> <span id="lParam"></span><span id="lparam"></span><span id="LPARAM"></span>*lParam* </dt> <dd> A handle to the MDI child window being activated. </dd> </dl> An MDI child window is activated independently of the MDI frame window. When the frame window becomes active, the child window last activated by using the **WM\_MDIACTIVATE** message receives the [**WM\_NCACTIVATE**](wm-ncactivate.md) message to draw an active window frame and title bar; the child window does not receive another **WM\_MDIACTIVATE** message.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/winmsg/wm-mdiactivate#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_MDIACTIVATE = 546;

    /// <summary>An application sends the WM\_MDIRESTORE message to a multiple-document interface (MDI) client window to restore an MDI child window from maximized or minimized size.</summary>
    /// <returns>
    /// <para>Type: **zero** The return value is always zero.</para>
    /// </returns>
    /// <remarks>
    /// <para><see href="https://learn.microsoft.com/windows/win32/winmsg/wm-mdirestore">Learn more about this API from docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_MDIRESTORE = 547;

    /// <summary>An application sends the WM\_MDINEXT message to a multiple-document interface (MDI) client window to activate the next or previous child window.</summary>
    /// <returns>
    /// <para>Type: **zero** The return value is always zero.</para>
    /// </returns>
    /// <remarks>If an MDI client window receives any message that changes the activation of its child windows while the active MDI child window is maximized, the system restores the active child window and maximizes the newly activated child window.</remarks>
    public const int WM_MDINEXT = 548;

    /// <summary>An application sends the WM\_MDIMAXIMIZE message to a multiple-document interface (MDI) client window to maximize an MDI child window.</summary>
    /// <returns>
    /// <para>Type: **zero** The return value is always zero.</para>
    /// </returns>
    /// <remarks>If an MDI client window receives any message that changes the activation of its child windows while the currently active MDI child window is maximized, the system restores the active child window and maximizes the newly activated child window.</remarks>
    public const int WM_MDIMAXIMIZE = 549;

    /// <summary>An application sends the WM\_MDITILE message to a multiple-document interface (MDI) client window to arrange all of its MDI child windows in a tile format.</summary>
    /// <returns>
    /// <para>Type: **BOOL** If the message succeeds, the return value is **TRUE**. If the message fails, the return value is **FALSE**.</para>
    /// </returns>
    /// <remarks>
    /// <para><see href="https://learn.microsoft.com/windows/win32/winmsg/wm-mditile">Learn more about this API from docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_MDITILE = 550;

    /// <summary>An application sends the WM\_MDICASCADE message to a multiple-document interface (MDI) client window to arrange all its child windows in a cascade format.</summary>
    /// <returns>
    /// <para>Type: **BOOL** If the message succeeds, the return value is **TRUE**. If the message fails, the return value is **FALSE**.</para>
    /// </returns>
    /// <remarks>
    /// <para><see href="https://learn.microsoft.com/windows/win32/winmsg/wm-mdicascade">Learn more about this API from docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_MDICASCADE = 551;

    /// <summary>An application sends the WM\_MDIICONARRANGE message to a multiple-document interface (MDI) client window to arrange all minimized MDI child windows. It does not affect child windows that are not minimized.</summary>
    /// <remarks>
    /// <para><see href="https://learn.microsoft.com/windows/win32/winmsg/wm-mdiiconarrange">Learn more about this API from docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_MDIICONARRANGE = 552;

    /// <summary>An application sends the WM\_MDIGETACTIVE message to a multiple-document interface (MDI) client window to retrieve the handle to the active MDI child window.</summary>
    /// <returns>
    /// <para>Type: **HWND** The return value is the handle to the active MDI child window.</para>
    /// </returns>
    /// <remarks>
    /// <para><see href="https://learn.microsoft.com/windows/win32/winmsg/wm-mdigetactive">Learn more about this API from docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_MDIGETACTIVE = 553;

    /// <summary>An application sends the WM\_MDISETMENU message to a multiple-document interface (MDI) client window to replace the entire menu of an MDI frame window, to replace the window menu of the frame window, or both.</summary>
    /// <returns>
    /// <para>Type: **HMENU** If the message succeeds, the return value is the handle to the old frame window menu. If the message fails, the return value is zero.</para>
    /// </returns>
    /// <remarks>
    /// <para>After sending this message, an application must call the [**DrawMenuBar**](/windows/win32/api/winuser/nf-winuser-drawmenubar) function to update the menu bar. If this message replaces the window menu, the MDI child window menu items are removed from the previous window menu and added to the new window menu. If an MDI child window is maximized and this message replaces the MDI frame window menu, the window menu icon and restore icon are removed from the previous frame window menu and added to the new frame window menu.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/winmsg/wm-mdisetmenu#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_MDISETMENU = 560;

    /// <summary>Sent one time to a window after it enters the moving or sizing modal loop.</summary>
    /// <returns>
    /// <para>Type: **LRESULT** An application should return zero if it processes this message.</para>
    /// </returns>
    /// <remarks>
    /// <para><see href="https://learn.microsoft.com/windows/win32/winmsg/wm-entersizemove">Learn more about this API from docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_ENTERSIZEMOVE = 561;

    /// <summary>Sent one time to a window, after it has exited the moving or sizing modal loop.</summary>
    /// <returns>
    /// <para>Type: **LRESULT** An application should return zero if it processes this message.</para>
    /// </returns>
    /// <remarks>
    /// <para><see href="https://learn.microsoft.com/windows/win32/winmsg/wm-exitsizemove">Learn more about this API from docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_EXITSIZEMOVE = 562;

    /// <summary>Sent when the user drops a file on the window of an application that has registered itself as a recipient of dropped files.</summary>
    /// <returns>An application should return zero if it processes this message.</returns>
    /// <remarks>The HDROP handle is declared in Shellapi.h. You must include this header in your build to use **WM\_DROPFILES**. For further discussion of how to use drag-and-drop to transfer Shell data, see [Transferring Shell Data Using Drag-and-Drop or the Clipboard](dragdrop.md).</remarks>
    public const int WM_DROPFILES = 563;

    /// <summary>An application sends the WM\_MDIREFRESHMENU message to a multiple-document interface (MDI) client window to refresh the window menu of the MDI frame window.</summary>
    /// <returns>
    /// <para>Type: **HMENU** If the message succeeds, the return value is the handle to the frame window menu. If the message fails, the return value is **NULL**.</para>
    /// </returns>
    /// <remarks>After sending this message, an application must call the [**DrawMenuBar**](/windows/win32/api/winuser/nf-winuser-drawmenubar) function to update the menu bar.</remarks>
    public const int WM_MDIREFRESHMENU = 564;

    /// <summary>Sent to a window when there is a change in the settings of a monitor that has a digitizer attached to it. This message contains information regarding the scaling of the display mode.</summary>
    /// <returns>
    /// <para>If the application processes this message, it should return zero. If the application does not process this message, it should call [**DefWindowProc**](/windows/win32/api/winuser/nf-winuser-defwindowproca).</para>
    /// </returns>
    /// <remarks>
    /// <para><see href="https://learn.microsoft.com/windows/win32/inputmsg/wm-pointerdevicechange">Learn more about this API from docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_POINTERDEVICECHANGE = 568;

    /// <summary>Sent to a window when a pointer device is detected within range of an input digitizer. This message contains information regarding the device and its proximity.</summary>
    /// <returns>
    /// <para>If the application processes this message, it should return zero. If the application does not process this message, it should call [**DefWindowProc**](/windows/win32/api/winuser/nf-winuser-defwindowproca).</para>
    /// </returns>
    /// <remarks>
    /// <para><see href="https://learn.microsoft.com/windows/win32/inputmsg/wm-pointerdeviceinrange">Learn more about this API from docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_POINTERDEVICEINRANGE = 569;

    /// <summary>Sent to a window when a pointer device has departed the range of an input digitizer. This message contains information regarding the device and its proximity.</summary>
    /// <returns>
    /// <para>If the application processes this message, it should return zero. If the application does not process this message, it should call [**DefWindowProc**](/windows/win32/api/winuser/nf-winuser-defwindowproca).</para>
    /// </returns>
    /// <remarks>
    /// <para><see href="https://learn.microsoft.com/windows/win32/inputmsg/wm-pointerdeviceoutofrange">Learn more about this API from docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_POINTERDEVICEOUTOFRANGE = 570;

    /// <summary>Notifies the window when one or more touch points, such as a finger or pen, touches a touch-sensitive digitizer surface.</summary>
    /// <returns>
    /// <para>If an application processes this message, it should return zero. If the application does not process the message, it must call [DefWindowProc](/windows/win32/api/winuser/nf-winuser-defwindowproca). Not doing so causes the application to leak memory because the touch input handle is not closed and associated process memory is not freed.</para>
    /// </returns>
    /// <remarks>**WM\_TOUCH** messages do not respect **HTTRANSPARENT** regions of windows. If a window returns **HTTRANSPARENT** in response to a **WM\_NCHITTEST** message, mouse messages go to the parent, and **WM\_TOUCH** messages go directly to the window.</remarks>
    public const int WM_TOUCH = 576;

    /// <summary>Posted to provide an update on a pointer that made contact over the non-client area of a window or when a hovering uncaptured contact moves over the non-client area of a window.</summary>
    /// <returns>
    /// <para>If an application processes this message, it should return zero. If the application does not process this message, it should call [**DefWindowProc**](/windows/win32/api/winuser/nf-winuser-defwindowproca).</para>
    /// </returns>
    /// <remarks>If the application does not process this message, [**DefWindowProc**](/windows/win32/api/winuser/nf-winuser-defwindowproca) may perform one or more system actions depending on the hit-test result included in the message. Typically, applications should not need to handle this message.</remarks>
    public const int WM_NCPOINTERUPDATE = 577;

    /// <summary>Posted when a pointer makes contact over the non-client area of a window.</summary>
    /// <returns>
    /// <para>If an application processes this message, it should return zero. If the application does not process this message, it should call [**DefWindowProc**](/windows/win32/api/winuser/nf-winuser-defwindowproca).</para>
    /// </returns>
    /// <remarks>If the application does not process this message, [**DefWindowProc**](/windows/win32/api/winuser/nf-winuser-defwindowproca) may perform one or more system actions depending on the hit-test result included in the message. Typically, applications should not need to handle this message.</remarks>
    public const int WM_NCPOINTERDOWN = 578;

    /// <summary>Posted when a pointer that made contact over the non-client area of a window breaks contact.</summary>
    /// <returns>
    /// <para>If an application processes this message, it should return zero. If the application does not process this message, it should call [**DefWindowProc**](/windows/win32/api/winuser/nf-winuser-defwindowproca).</para>
    /// </returns>
    /// <remarks>If the application does not process this message, [**DefWindowProc**](/windows/win32/api/winuser/nf-winuser-defwindowproca) may perform one or more system actions depending on the hit-test result included in the message. Typically, applications should not need to handle this message.</remarks>
    public const int WM_NCPOINTERUP = 579;

    /// <summary>Posted to provide an update on a pointer that made contact over the client area of a window or on a hovering uncaptured pointer over the client area of a window.</summary>
    /// <returns>
    /// <para>If an application processes this message, it should return zero. If the application does not process this message, it should call [**DefWindowProc**](/windows/win32/api/winuser/nf-winuser-defwindowproca).</para>
    /// </returns>
    /// <remarks>
    /// <para>Each pointer has a unique pointer identifier during its lifetime. The lifetime of a pointer begins when it is first detected. A [**WM_POINTERENTER**](wm-pointerenter.md) message is generated if a hovering pointer is detected. A [**WM_POINTERDOWN**](wm-pointerdown.md) message followed by a **WM_POINTERENTER** message is generated if a non-hovering pointer is detected. During its lifetime, a pointer may generate a series of **WM_POINTERUPDATE** messages while it is hovering or in contact. The lifetime of a pointer ends when it is no longer detected. This generates a [**WM_POINTERLEAVE**](wm-pointerleave.md) message. When a pointer is aborted, [**POINTER_FLAG_CANCELED**](pointer-flags-contants.md) is set. A [**WM_POINTERLEAVE**](wm-pointerleave.md) message may also be generated when a non-captured pointer moves outside the bounds of a window. To obtain the horizontal and vertical position of a pointer, use the following:</para>
    /// <para></para>
    /// <para>This doc was truncated.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/inputmsg/wm-pointerupdate#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_POINTERUPDATE = 581;

    /// <summary>Posted when a pointer makes contact over the client area of a window.</summary>
    /// <returns>
    /// <para>If an application processes this message, it should return zero. If the application does not process this message, it should call [**DefWindowProc**](/windows/win32/api/winuser/nf-winuser-defwindowproca).</para>
    /// </returns>
    /// <remarks>
    /// <para>> ![Important] > When a window loses capture of a pointer and it receives the [**WM_POINTERCAPTURECHANGED**](wm-pointercapturechanged.md) notification, it typically will not receive any further notifications. For this reason, it is important that you not make any assumptions based on evenly paired **WM_POINTERDOWN**/[**WM_POINTERUP**](wm-pointerup.md) or [**WM_POINTERENTER**](wm-pointerenter.md)/[**WM_POINTERLEAVE**](wm-pointerleave.md) notifications.</para>
    /// <para>Each pointer has a unique pointer identifier during its lifetime. The lifetime of a pointer begins when it is first detected. A [**WM_POINTERENTER**](wm-pointerenter.md) message is generated if a hovering pointer is detected. A **WM_POINTERDOWN** message followed by a **WM_POINTERENTER** message is generated if a non-hovering pointer is detected. During its lifetime, a pointer may generate a series of [**WM_POINTERUPDATE**](wm-pointerupdate.md) messages while it is hovering or in contact. The lifetime of a pointer ends when it is no longer detected. This generates a [**WM_POINTERLEAVE**](wm-pointerleave.md) message. When a pointer is aborted, [**POINTER_FLAG_CANCELED**](pointer-flags-contants.md) is set. A [**WM_POINTERLEAVE**](wm-pointerleave.md) message may also be generated when a non-captured pointer moves outside the bounds of a window. To obtain the horizontal and vertical position of a pointer, use the following:</para>
    /// <para></para>
    /// <para>This doc was truncated.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/inputmsg/wm-pointerdown#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_POINTERDOWN = 582;

    /// <summary>Posted when a pointer that made contact over the client area of a window breaks contact.</summary>
    /// <returns>
    /// <para>If an application processes this message, it should return zero. If the application does not process this message, it should call [**DefWindowProc**](/windows/win32/api/winuser/nf-winuser-defwindowproca).</para>
    /// </returns>
    /// <remarks>
    /// <para>> ![Important] > When a window loses capture of a pointer and it receives the [**WM_POINTERCAPTURECHANGED**](wm-pointercapturechanged.md) notification, it typically will not receive any further notifications. For this reason, it is important that you not make any assumptions based on evenly paired [**WM_POINTERDOWN**](wm-pointerdown.md)/**WM_POINTERUP** or [**WM_POINTERENTER**](wm-pointerenter.md)/[**WM_POINTERLEAVE**](wm-pointerleave.md) notifications.</para>
    /// <para>Each pointer has a unique pointer identifier during its lifetime. The lifetime of a pointer begins when it is first detected. A [**WM_POINTERENTER**](wm-pointerenter.md) message is generated if a hovering pointer is detected. A [**WM_POINTERDOWN**](wm-pointerdown.md) message followed by a **WM_POINTERENTER** message is generated if a non-hovering pointer is detected. During its lifetime, a pointer may generate a series of [**WM_POINTERUPDATE**](wm-pointerupdate.md) messages while it is hovering or in contact. The lifetime of a pointer ends when it is no longer detected. This generates a [**WM_POINTERLEAVE**](wm-pointerleave.md) message. When a pointer is aborted, [**POINTER_FLAG_CANCELED**](pointer-flags-contants.md) is set. A [**WM_POINTERLEAVE**](wm-pointerleave.md) message may also be generated when a non-captured pointer moves outside the bounds of a window. To obtain the horizontal and vertical position of a pointer, use the following: Use the following code to obtain the horizontal and vertical position:</para>
    /// <para></para>
    /// <para>This doc was truncated.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/inputmsg/wm-pointerup#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_POINTERUP = 583;

    /// <summary>Sent to a window when a new pointer enters detection range over the window (hover) or when an existing pointer moves within the boundaries of the window.</summary>
    /// <returns>
    /// <para>If an application processes this message, it should return zero. If the application does not process this message, it should call [**DefWindowProc**](/windows/win32/api/winuser/nf-winuser-defwindowproca).</para>
    /// </returns>
    /// <remarks>
    /// <para>The **WM_POINTERENTER** notification can be used by a window to provide feedback to the user while the pointer is over its surface or to otherwise react to the presence of a pointer over its surface. This notification is only sent to the window that is receiving input for the pointer. The following table lists some of the situations in which this notification is sent.</para>
    /// <para>| Action                                                   | Flags Set                                                                                                                                         | Notifications Sent To                                 | |----------------------------------------------------------|---------------------------------------------------------------------------------------------------------------------------------------------------|-------------------------------------------------------| | A new pointer enters detection range (hover).            | [**IS_POINTER_NEW_WPARAM**](/windows/win32/api/winuser/nf-winuser-is_pointer_new_wparam)<br/> [**IS_POINTER_INRANGE_WPARAM**](/windows/win32/api/winuser/nf-winuser-is_pointer_new_wparam)<br/> | Window over which the pointer enters detection range. | | A hovering pointer crosses within the window boundaries. | [**IS_POINTER_INRANGE_WPARAM**](/windows/win32/api/winuser/nf-winuser-is_pointer_inrange_wparam)<br/>                                                                      | Window within which the pointer has crossed.          |</para>
    /// <para>> ![Important] > When a window loses capture of a pointer and it receives the [**WM_POINTERCAPTURECHANGED**](wm-pointercapturechanged.md) notification, it typically will not receive any further notifications. For this reason, it is important that you not make any assumptions based on evenly paired [**WM_POINTERDOWN**](wm-pointerdown.md)/[**WM_POINTERUP**](wm-pointerup.md) or **WM_POINTERENTER**/[**WM_POINTERLEAVE**](wm-pointerleave.md) notifications.</para>
    /// <para>When inputs come from the mouse, as a result of mouse and pointer message integration, **WM_POINTERENTER** is not sent.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/inputmsg/wm-pointerenter#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_POINTERENTER = 585;

    /// <summary>Sent to a window when a pointer leaves detection range over the window (hover) or when a pointer moves outside the boundaries of the window.</summary>
    /// <returns>
    /// <para>If an application processes this message, it should return zero. If the application does not process this message, it should call [**DefWindowProc**](/windows/win32/api/winuser/nf-winuser-defwindowproca).</para>
    /// </returns>
    /// <remarks>
    /// <para>The **WM_POINTERLEAVE** notification can be used by a window to change mode or stop any feedback to the user while the pointer is over the window surface. This notification is only sent to the window that is receiving input for the pointer. The following table lists some of the situations in which this notification is sent.</para>
    /// <para>| Action                                        | Flags Set                                                         | Notifications Sent To                                | |-----------------------------------------------|-------------------------------------------------------------------|------------------------------------------------------| | A hovering pointer crosses window boundaries. | [**IS_POINTER_INRANGE_WPARAM**](/windows/win32/api/winuser/nf-winuser-is_pointer_inrange_wparam) | Window outside of whose boundary the pointer moved.  | | A pointer goes out of detection range.        | N/A                                                               | Window for which the pointer leaves detection range. |</para>
    /// <para>> ![Important] > When a window loses capture of a pointer and it receives the [**WM_POINTERCAPTURECHANGED**](wm-pointercapturechanged.md) notification, it typically will not receive any further notifications. For this reason, it is important that you not make any assumptions based on evenly paired [**WM_POINTERDOWN**](wm-pointerdown.md)/[**WM_POINTERUP**](wm-pointerup.md) or [**WM_POINTERENTER**](wm-pointerenter.md)/**WM_POINTERLEAVE** notifications.</para>
    /// <para>If contact is maintained with the input digitizer and the pointer moves outside the window, **WM_POINTERLEAVE** is not generated. **WM_POINTERLEAVE** is generated only when a hovering pointer crosses window boundaries or contact is terminated. **WM_POINTERLEAVE** is posted to the posted message queue if the input is originated from a mouse device.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/inputmsg/wm-pointerleave#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_POINTERLEAVE = 586;

    /// <summary>Sent to an inactive window when a primary pointer generates a WM_POINTERDOWN over the window.</summary>
    /// <returns>
    /// <para>If an application processes this message, it should return one of the values described in the Remarks section. If the application does not process this message, it should call [**DefWindowProc**](/windows/win32/api/winuser/nf-winuser-defwindowproca).</para>
    /// </returns>
    /// <remarks>
    /// <para>An application can handle this message and return one of the following values to determine how the system processes the activation and the activating input: -   PA_ACTIVATE -   PA_NOACTIVATE It is important to note that, when the user is interacting with the system with multiple simultaneous pointers, the activation opportunity that the **WM_POINTERACTIVATE** message represents is available to applications only for the first of those pointers. Applications should, therefore, be aware that they may still receive input from pointers while they are inactive. If the application does not handle this message, [**DefWindowProc**](/windows/win32/api/winuser/nf-winuser-defwindowproca) passes the message to the parent window.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/inputmsg/wm-pointeractivate#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_POINTERACTIVATE = 587;

    /// <summary>Sent to a window that is losing capture of an input pointer.</summary>
    /// <returns>
    /// <para>If an application processes this message, it should return zero. If the application does not process this message, it should call [**DefWindowProc**](/windows/win32/api/winuser/nf-winuser-defwindowproca).</para>
    /// </returns>
    /// <remarks>
    /// <para>A window should use this notification to stop processing subsequent messages and initiate any cleanup required for the pointer being lost. Processing of gestures associated with the pointer should also be terminated (for example, by calling [**StopInteractionContext**](/windows/win32/api/interactioncontext/nf-interactioncontext-stopinteractioncontext)) and remaining contacts re-associated with the window. Typically, if a window receives the **WM_POINTERCAPTURECHANGED** notification, no subsequent notifications related to the input pointer are received. Because of this, do not depend on paired notifications such as [**WM_POINTERENTER**](wm-pointerenter.md) and [**WM_POINTERLEAVE**](wm-pointerleave.md). **WM_POINTERCAPTURECHANGED** does not include [**POINTER_INFO**](/windows/win32/api/winuser/ns-winuser-pointer_info) data. Other than the [**POINTER_FLAG_CAPTURECHANGED**](pointer-flags-contants.md) flag being set, the data returned by [**GetPointerInfo**](/windows/win32/api/winuser/ns-winuser-pointer_info) (or any variant) is identical to that returned prior to the notification. If the application does not process this notification, [**DefWindowProc**](/windows/win32/api/winuser/nf-winuser-defwindowproca) may generate one or more [**WM_GESTURE**](../wintouch/wm-gesture.md) messages or, if a gesture is not recognized, **DefWindowProc** may generate mouse input. If an application selectively consumes some pointer input and passes the rest to [**DefWindowProc**](/windows/win32/api/winuser/nf-winuser-defwindowproca), the resulting behavior is undefined.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/inputmsg/wm-pointercapturechanged#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_POINTERCAPTURECHANGED = 588;

    /// <summary>Sent to a window on a touch down in order to determine the most probable touch target.</summary>
    /// <returns>
    /// <para>If one or more elements are within the touch contact area, an application should return the result of [**PackTouchHitTestingProximityEvaluation**](/windows/win32/api/winuser/nf-winuser-packtouchhittestingproximityevaluation). If no elements are within the touch contact area, an application should set the value of **score** in [**TOUCH_HIT_TESTING_PROXIMITY_EVALUATION**](/windows/win32/api/winuser/ns-winuser-touch_hit_testing_proximity_evaluation) to [**TOUCH_HIT_TESTING_PROXIMITY_FARTHEST**](/previous-versions/windows/desktop/input_touchhittest/hit-testing-scores) and call [**PackTouchHitTestingProximityEvaluation**](/windows/win32/api/winuser/nf-winuser-packtouchhittestingproximityevaluation) to get the LRESULT return value. If the application does not process this message, it must call [**DefWindowProc**](/windows/win32/api/winuser/nf-winuser-defwindowproca).</para>
    /// </returns>
    /// <remarks>This message is sent to windows that register through the [**RegisterTouchHitTestingWindow**](/windows/win32/api/winuser/nf-winuser-registertouchhittestingwindow) function.</remarks>
    public const int WM_TOUCHHITTESTING = 589;

    /// <summary>Posted to the window with foreground keyboard focus when a scroll wheel is rotated.</summary>
    /// <returns>
    /// <para>If the application processes this message, it should return zero. If the application does not process this message, it should call [**DefWindowProc**](/windows/win32/api/winuser/nf-winuser-defwindowproca).</para>
    /// </returns>
    /// <remarks>
    /// <para>To retrieve the wheel scroll units, use the **inputData** filed of the [**POINTER_INFO**](/windows/win32/api/winuser/ns-winuser-pointer_info) structure returned by calling [**GetPointerInfo**](/windows/win32/api/winuser/ns-winuser-pointer_info) function. This field contains a signed value and is expressed in a multiple of **WHEEL_DELTA**. A positive value indicates a rotation forward and a negative value indicates a rotation backward. Note that the wheel inputs may be delivered even if the mouse cursor is located outside of application s window. The wheel messages are delivered in a way very similar to the keyboard inputs. The focus window of the foregournd message queue receives the wheel messages.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/inputmsg/wm-pointerwheel#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_POINTERWHEEL = 590;

    /// <summary>Posted to the window with foreground keyboard focus when a horizontal scroll wheel is rotated.</summary>
    /// <returns>
    /// <para>If the application processes this message, it should return zero. If the application does not process this message, it should call [**DefWindowProc**](/windows/win32/api/winuser/nf-winuser-defwindowproca).</para>
    /// </returns>
    /// <remarks>
    /// <para>To retrieve the wheel scroll units, use the **inputData** filed of the [**POINTER_INFO**](/windows/win32/api/winuser/ns-winuser-pointer_info) structure returned by calling [**GetPointerInfo**](/windows/win32/api/winuser/ns-winuser-pointer_info) function. This field contains a signed value and is expressed in a multiple of **WHEEL_DELTA**. A positive value indicates a rotation forward and a negative value indicates a rotation backward. Note that the wheel inputs may be delivered even if the mouse cursor is located outside of application s window. The wheel messages are delivered in a way very similar to the keyboard inputs. The focus window of the foregournd message queue receives the wheel messages.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/inputmsg/wm-pointerhwheel#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_POINTERHWHEEL = 591;

    /// <summary>Sent when ongoing pointer input, for an existing pointer ID, transitions from one process to another across content configured for cross-process chaining (AddContentWithCrossProcessChaining).</summary>
    /// <returns>NULL</returns>
    /// <remarks>
    /// <para>This message is not sent when a [**WM_POINTERDOWN**](wm-pointerdown.md) message is posted for a new pointer ID on a different process. A [**WM_POINTERDOWN**](wm-pointerdown.md) message is not sent if a **WM_POINTERROUTEDTO** message is posted first.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/inputmsg/wm-pointerroutedto#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_POINTERROUTEDTO = 593;

    /// <summary>Occurs on the process receiving input when the pointer input is routed to another process.AddContentWithCrossProcessChaining).</summary>
    /// <returns>NULL</returns>
    /// <remarks>This message is not sent with either a [**WM_POINTERUP**](wm-pointerup.md) message or a [**WM_POINTERCAPTURECHANGED**](wm-pointercapturechanged.md) message.</remarks>
    public const int WM_POINTERROUTEDAWAY = 594;

    /// <summary>Sent to all processes (configured for cross-process chaining through AddContentWithCrossProcessChaining and not currently handling pointer input) ever associated with a specific pointer ID, when a WM_POINTERUP message is received on the current process.</summary>
    /// <returns>NULL</returns>
    /// <remarks>
    /// <para><see href="https://learn.microsoft.com/windows/win32/inputmsg/wm-pointerroutedreleased">Learn more about this API from docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_POINTERROUTEDRELEASED = 595;

    /// <summary>Sent to an application when a window is activated. A window receives this message through its WindowProc function.</summary>
    /// <returns>Returns the value returned by [**DefWindowProc**](/windows/desktop/api/winuser/nf-winuser-defwindowproca) or [**ImmIsUIMessage**](/windows/desktop/api/Imm/nf-imm-immisuimessagea).</returns>
    /// <remarks>
    /// <para>If the application has created an IME window, it should call [**ImmIsUIMessage**](/windows/desktop/api/Imm/nf-imm-immisuimessagea). Otherwise, it should pass this message to [**DefWindowProc**](/windows/desktop/api/winuser/nf-winuser-defwindowproca). If the application draws the composition window, the default IME window does not have to show its composition window. In this case, the application must clear the **ISC\_SHOWUICOMPOSITIONWINDOW** value from the *lParam* parameter before passing the message to [**DefWindowProc**](/windows/desktop/api/winuser/nf-winuser-defwindowproca) or [**ImmIsUIMessage**](/windows/desktop/api/Imm/nf-imm-immisuimessagea). To display a certain user interface window, an application should remove the corresponding value so that the IME will not display it.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/Intl/wm-ime-setcontext#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_IME_SETCONTEXT = 641;

    /// <summary>Sent to an application to notify it of changes to the IME window. A window receives this message through its WindowProc function.</summary>
    /// <returns>The return value depends on the command sent.</returns>
    /// <remarks>An application processes this message if it is responsible for managing the IME window.</remarks>
    public const int WM_IME_NOTIFY = 642;

    /// <summary>Sent by an application to direct the IME window to carry out the requested command.</summary>
    /// <returns>The message returns a command-specific value.</returns>
    /// <remarks>
    /// <para><see href="https://learn.microsoft.com/windows/win32/Intl/wm-ime-control">Learn more about this API from docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_IME_CONTROL = 643;

    /// <summary>Sent to an application when the IME window finds no space to extend the area for the composition window. A window receives this message through its WindowProc function.</summary>
    /// <returns>
    /// <para>This message has no parameters.</para>
    /// <para>This message has no return value.</para>
    /// </returns>
    /// <remarks>
    /// <para>The application should use the [IMC\_SETCOMPOSITIONWINDOW](imc-setcompositionwindow.md) command to specify how the window should be displayed. The IME window, instead of the IME, sends this notification message by the [**SendMessage**](/windows/win32/api/winuser/nf-winuser-sendmessage) function.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/Intl/wm-ime-compositionfull#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_IME_COMPOSITIONFULL = 644;

    /// <summary>Sent to an application when the operating system is about to change the current IME. A window receives this message through its WindowProc function.</summary>
    /// <returns>This message has no return value.</returns>
    /// <remarks>
    /// <para>An application that has created an IME window should pass this message to that window so that it can retrieve the keyboard layout handle to the newly selected IME. The [**DefWindowProc**](/windows/desktop/api/winuser/nf-winuser-defwindowproca)  function processes this message by passing the information to the default IME window.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/Intl/wm-ime-select#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_IME_SELECT = 645;

    /// <summary>Sent to an application when the IME gets a character of the conversion result. A window receives this message through its WindowProc function.</summary>
    /// <remarks>
    /// <para>Unlike the [**WM\_CHAR**](../inputdev/wm-char.md) message for a non-Unicode window, this message can include double-byte and single-byte character values. For a Unicode window, this message is the same as WM\_CHAR. For a non-Unicode window, if the WM\_IME\_CHAR message includes a double-byte character and the application passes this message to [**DefWindowProc**](/windows/desktop/api/winuser/nf-winuser-defwindowproca), the IME converts this message into two WM\_CHAR messages, each containing one byte of the double-byte character.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/Intl/wm-ime-char#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_IME_CHAR = 646;

    /// <summary>Sent to an application to provide commands and request information. A window receives this message through its WindowProc function.</summary>
    /// <returns>Returns a command-specific value.</returns>
    /// <remarks>
    /// <para><see href="https://learn.microsoft.com/windows/win32/Intl/wm-ime-request">Learn more about this API from docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_IME_REQUEST = 648;

    /// <summary>Sent to an application by the IME to notify the application of a key press and to keep message order. A window receives this message through its WindowProc function.</summary>
    /// <returns>An application should return 0 if it processes this message.</returns>
    /// <remarks>An application can process this message or pass it to the [**DefWindowProc**](/windows/desktop/api/winuser/nf-winuser-defwindowproca)  function to generate a matching [**WM\_KEYDOWN**](../inputdev/wm-keydown.md) message.</remarks>
    public const int WM_IME_KEYDOWN = 656;

    /// <summary>Sent to an application by the IME to notify the application of a key release and to keep message order. A window receives this message through its WindowProc function.</summary>
    /// <returns>An application should return 0 if it processes this message.</returns>
    /// <remarks>An application can process this message or pass it to the [**DefWindowProc**](/windows/desktop/api/winuser/nf-winuser-defwindowproca)  function to generate a matching [**WM\_KEYUP**](../inputdev/wm-keyup.md) message.</remarks>
    public const int WM_IME_KEYUP = 657;

    /// <summary>Posted to a window when the cursor hovers over the nonclient area of the window for the period of time specified in a prior call to TrackMouseEvent.</summary>
    /// <returns>If an application processes this message, it should return zero.</returns>
    /// <remarks>
    /// <para>Hover tracking stops when this message is generated. The application must call [**TrackMouseEvent**](/windows/win32/api/winuser/nf-winuser-trackmouseevent) again if it requires further tracking of mouse hover behavior. You can also use the [**GET\_X\_LPARAM**](/windows/desktop/api/windowsx/nf-windowsx-get_x_lparam) and [**GET\_Y\_LPARAM**](/windows/desktop/api/windowsx/nf-windowsx-get_y_lparam) macros to extract the values of the x- and y- coordinates from *lParam*.</para>
    /// <para></para>
    /// <para>This doc was truncated.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/inputdev/wm-ncmousehover#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_NCMOUSEHOVER = 672;

    /// <summary>Posted to a window when the cursor leaves the nonclient area of the window specified in a prior call to TrackMouseEvent.</summary>
    /// <returns>If an application processes this message, it should return zero.</returns>
    /// <remarks>All tracking requested by [**TrackMouseEvent**](/windows/win32/api/winuser/nf-winuser-trackmouseevent) is canceled when this message is generated. The application must call **TrackMouseEvent** when the mouse reenters its window if it requires further tracking of mouse hover behavior.</remarks>
    public const int WM_NCMOUSELEAVE = 674;

    public const int WM_WTSSESSION_CHANGE = 689;

    public const int WM_TABLET_FIRST = 704;

    public const int WM_TABLET_LAST = 735;

    /// <summary>Sent when the effective dots per inch (dpi) for a window has changed.</summary>
    /// <returns>If an application processes this message, it should return zero.</returns>
    /// <remarks>
    /// <para>This message is only relevant for **PROCESS\_PER\_MONITOR\_DPI\_AWARE** applications or **DPI\_AWARENESS\_PER\_MONITOR\_AWARE** threads. It may be received on certain DPI changes if your top-level window or process is running as **DPI unaware** or **system DPI aware**, but in those situations it can be safely ignored. For more information about the different types of awareness, see [**PROCESS\_DPI\_AWARENESS**](/windows/desktop/api/ShellScalingApi/ne-shellscalingapi-process_dpi_awareness) and [**DPI\_AWARENESS**](/windows/desktop/api/windef/ne-windef-dpi_awareness). Older versions of Windows required DPI awareness to be tied at the level of an application. Those apps use **PROCESS\_DPI\_AWARENESS**. Currently, DPI awareness is tied to threads and individual windows rather than the entire application. These apps use **DPI\_AWARENESS**. You only need to use either the X-axis or the Y-axis value when scaling your application since they are the same. In order to handle this message correctly, you will need to resize and reposition your window based on the suggestions provided by *lParam* and using [**SetWindowPos**](/windows/desktop/api/winuser/nf-winuser-setwindowpos). If you do not do this, your window will grow or shrink with respect to everything else on the new monitor. For example, if a user is using multiple monitors and drags your window from a 96 DPI monitor to a 192 DPI monitor, your window will appear to be half as large with respect to other items on the 192 DPI monitor. The base value of DPI is defined as **USER\_DEFAULT\_SCREEN\_DPI** which is set to 96. To determine the scaling factor for a monitor, take the DPI value and divide by **USER\_DEFAULT\_SCREEN\_DPI**. The following table provides some sample DPI values and associated scaling factors.</para>
    /// <para>| DPI value | Scaling percentage | |-----------|--------------------| | 96        | 100%               | | 120       | 125%               | | 144       | 150%               | | 192       | 200%               |</para>
    /// <para>The following example provides a sample DPI change handler.</para>
    /// <para></para>
    /// <para>This doc was truncated.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/hidpi/wm-dpichanged#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_DPICHANGED = 736;

    /// <summary>For Per Monitor v2 top-level windows, this message is sent to all HWNDs in the child HWDN tree of the window that is undergoing a DPI change. | WM_DPICHANGED_BEFOREPARENT message (Winuser.h)</summary>
    /// <returns>This value is unused and ignored by the system.</returns>
    /// <remarks>
    /// <para>There is no default handling of this message in [DefWindowProc](/windows/win32/api/winuser/nf-winuser-defwindowproca). This message is only sent when the top-level window has a DPI awareness context of PMv2.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/hidpi/wm-dpichanged-beforeparent#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_DPICHANGED_BEFOREPARENT = 738;

    /// <summary>For Per Monitor v2 top-level windows, this message is sent to all HWNDs in the child HWDN tree of the window that is undergoing a DPI change. | WM_DPICHANGED_AFTERPARENT message (Winuser.h)</summary>
    /// <returns>This value is unused and ignored by the system.</returns>
    /// <remarks>
    /// <para>There is no default handling of this message in [DefWindowProc](/windows/win32/api/winuser/nf-winuser-defwindowproca). This message is only sent when the top-level window has a DPI awareness context of PMv2.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/hidpi/wm-dpichanged-afterparent#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_DPICHANGED_AFTERPARENT = 739;

    /// <summary>This message tells the operating system that the window will be sized to dimensions other than the default.</summary>
    /// <returns>The function returns a BOOL. Returning TRUE indicates that a new size has been computed. Returning FALSE indicates that the message will not be handled, and the default linear DPI scaling will apply to the window.</returns>
    /// <remarks>
    /// <para>This message is only sent to top-level windows which have a DPI awareness context of Per Monitor v2. This event is necessary to facilitate graceful non-linear scaling, and ensures that the windows's position remains constant in relationship to the cursor and when moving back and forth across monitors. There is no specific default handling of this message in [DefWindowProc](/windows/win32/api/winuser/nf-winuser-defwindowproca). As for all messages it does not explicitly handle, [DefWindowProc](/windows/win32/api/winuser/nf-winuser-defwindowproca) will return zero for this message. As noted above, this return tells the system to use the default linear behavior.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/hidpi/wm-getdpiscaledsize#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_GETDPISCALEDSIZE = 740;

    /// <summary>An application sends a WM\_CUT message to an edit control or combo box to delete (cut) the current selection, if any, in the edit control and copy the deleted text to the clipboard in CF\_TEXT format.</summary>
    /// <returns>This message does not return a value.</returns>
    /// <remarks>
    /// <para>The deletion performed by the **WM\_CUT** message can be undone by sending the edit control an [**EM\_UNDO**](../controls/em-undo.md) message. To delete the current selection without placing the deleted text on the clipboard, use the [**WM\_CLEAR**](wm-clear.md) message. When sent to a combo box, the **WM\_CUT** message is handled by its edit control. This message has no effect when sent to a combo box with the [**CBS\_DROPDOWNLIST**](../controls/combo-box-styles.md) style.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/dataxchg/wm-cut#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_CUT = 768;

    /// <summary>An application sends the WM\_COPY message to an edit control or combo box to copy the current selection to the clipboard in CF\_TEXT format.</summary>
    /// <returns>Returns nonzero value on success, else zero.</returns>
    /// <remarks>When sent to a combo box, the **WM\_COPY** message is handled by its edit control. This message has no effect when sent to a combo box with the [**CBS\_DROPDOWNLIST**](../controls/combo-box-styles.md) style.</remarks>
    public const int WM_COPY = 769;

    /// <summary>An application sends a WM\_PASTE message to an edit control or combo box to copy the current content of the clipboard to the edit control at the current caret position. Data is inserted only if the clipboard contains data in CF\_TEXT format.</summary>
    /// <returns>This message does not return a value.</returns>
    /// <remarks>When sent to a combo box, the **WM\_PASTE** message is handled by its edit control. This message has no effect when sent to a combo box with the [**CBS\_DROPDOWNLIST**](../controls/combo-box-styles.md) style.</remarks>
    public const int WM_PASTE = 770;

    /// <summary>An application sends a WM\_CLEAR message to an edit control or combo box to delete (clear) the current selection, if any, from the edit control.</summary>
    /// <returns>This message does not return a value.</returns>
    /// <remarks>
    /// <para>The deletion performed by the **WM\_CLEAR** message can be undone by sending the edit control an [**EM\_UNDO**](../controls/em-undo.md) message. To delete the current selection and place the deleted content on the clipboard, use the [**WM\_CUT**](wm-cut.md) message. When sent to a combo box, the **WM\_CLEAR** message is handled by its edit control. This message has no effect when sent to a combo box with the [**CBS\_DROPDOWNLIST**](../controls/combo-box-styles.md) style.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/dataxchg/wm-clear#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_CLEAR = 771;

    /// <summary>An application sends a WM\_UNDO message to an edit control to undo the last operation. When this message is sent to an edit control, the previously deleted text is restored or the previously added text is deleted.</summary>
    /// <returns>
    /// <para>If the message succeeds, the return value is **TRUE**. If the message fails, the return value is **FALSE**.</para>
    /// </returns>
    /// <remarks>**Rich Edit:** It is recommended that [**EM\_UNDO**](em-undo.md) be used instead of **WM\_UNDO**.</remarks>
    public const int WM_UNDO = 772;

    /// <summary>Sent to the clipboard owner if it has delayed rendering a specific clipboard format and if an application has requested data in that format.</summary>
    /// <returns>If an application processes this message, it should return zero.</returns>
    /// <remarks>When responding to a **WM\_RENDERFORMAT** message, the clipboard owner must not open the clipboard before calling [**SetClipboardData**](/windows/win32/api/winuser/nf-winuser-setclipboarddata). Opening the clipboard is not necessary before placing data in response to **WM\_RENDERFORMAT**, and any attempt to open the clipboard will fail because the clipboard is currently being held open by the application that requested the format to be rendered.</remarks>
    public const int WM_RENDERFORMAT = 773;

    /// <summary>Sent to the clipboard owner before it is destroyed, if the clipboard owner has delayed rendering one or more clipboard formats.</summary>
    /// <returns>If an application processes this message, it should return zero.</returns>
    /// <remarks>
    /// <para>When responding to a **WM\_RENDERALLFORMATS** message, the application must call the [**OpenClipboard**](/windows/win32/api/winuser/nf-winuser-openclipboard) function and then check that it is still the clipboard owner by calling the [**GetClipboardOwner**](/windows/win32/api/winuser/nf-winuser-getclipboardowner) function before calling [**SetClipboardData**](/windows/win32/api/winuser/nf-winuser-setclipboarddata). The application needs to check that it is still the clipboard owner after opening the clipboard because after it receives the **WM\_RENDERALLFORMATS** message, but before it opens the clipboard, another application may have opened and taken ownership of the clipboard, and that application's data should not be overwritten. In most cases, the application should not call the [**EmptyClipboard**](/windows/win32/api/winuser/nf-winuser-emptyclipboard) function before calling [**SetClipboardData**](/windows/win32/api/winuser/nf-winuser-setclipboarddata), since doing so will erase the clipboard formats that the application has already rendered. When the application returns, the system removes any unrendered formats from the list of available clipboard formats. For information about delayed rendering, see [Delayed Rendering](clipboard-operations.md#delayed-rendering).</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/dataxchg/wm-renderallformats#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_RENDERALLFORMATS = 774;

    /// <summary>Sent to the clipboard owner when a call to the EmptyClipboard function empties the clipboard. A window receives this message through its WindowProc function.</summary>
    /// <returns>If an application processes this message, it should return zero.</returns>
    /// <remarks>
    /// <para><see href="https://learn.microsoft.com/windows/win32/dataxchg/wm-destroyclipboard">Learn more about this API from docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_DESTROYCLIPBOARD = 775;

    /// <summary>Sent to the first window in the clipboard viewer chain when the content of the clipboard changes. This enables a clipboard viewer window to display the new content of the clipboard. A window receives this message through its WindowProc function.</summary>
    /// <remarks>
    /// <para>Only clipboard viewer windows receive this message. These are windows that have been added to the clipboard viewer chain by using the [**SetClipboardViewer**](/windows/desktop/api/Winuser/nf-winuser-setclipboardviewer) function. Each window that receives the **WM\_DRAWCLIPBOARD** message must call the [**SendMessage**](/windows/desktop/api/winuser/nf-winuser-sendmessage) function to pass the message on to the next window in the clipboard viewer chain. The handle to the next window in the chain is returned by [**SetClipboardViewer**](/windows/desktop/api/Winuser/nf-winuser-setclipboardviewer), and may change in response to a [**WM\_CHANGECBCHAIN**](wm-changecbchain.md) message.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/dataxchg/wm-drawclipboard#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_DRAWCLIPBOARD = 776;

    /// <summary>Sent to the clipboard owner by a clipboard viewer window when the clipboard contains data in the CF\_OWNERDISPLAY format and the clipboard viewer's client area needs repainting.</summary>
    /// <returns>If an application processes this message, it should return zero.</returns>
    /// <remarks>
    /// <para>To determine whether the entire client area or just a portion of it needs repainting, the clipboard owner must compare the dimensions of the drawing area given in the **rcPaint** member of [**PAINTSTRUCT**](/windows/win32/api/winuser/ns-winuser-paintstruct) to the dimensions given in the most recent [**WM\_SIZECLIPBOARD**](wm-sizeclipboard.md) message. The clipboard owner must use the [**GlobalLock**](/windows/desktop/api/winbase/nf-winbase-globallock) function to lock the memory that contains the [**PAINTSTRUCT**](/windows/win32/api/winuser/ns-winuser-paintstruct) structure. Before returning, the clipboard owner must unlock that memory by using the [**GlobalUnlock**](/windows/desktop/api/winbase/nf-winbase-globalunlock) function.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/dataxchg/wm-paintclipboard#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_PAINTCLIPBOARD = 777;

    /// <summary>Sent to the clipboard owner by a clipboard viewer window when the clipboard contains data in the CF\_OWNERDISPLAY format and an event occurs in the clipboard viewer's vertical scroll bar.</summary>
    /// <returns>If an application processes this message, it should return zero.</returns>
    /// <remarks>The clipboard owner can use the [**ScrollWindow**](https://msdn.microsoft.com/library/Cc410994(v=MSDN.10).aspx) function to scroll the image in the clipboard viewer window and invalidate the appropriate region.</remarks>
    public const int WM_VSCROLLCLIPBOARD = 778;

    /// <summary>Sent to the clipboard owner by a clipboard viewer window when the clipboard contains data in the CF\_OWNERDISPLAY format and the clipboard viewer's client area has changed size.</summary>
    /// <returns>If an application processes this message, it should return zero.</returns>
    /// <remarks>
    /// <para>When the clipboard viewer window is about to be destroyed or resized, a **WM\_SIZECLIPBOARD** message is sent with a null rectangle (0, 0, 0, 0) as the new size. This permits the clipboard owner to free its display resources. The clipboard owner must use the [**GlobalLock**](/windows/desktop/api/winbase/nf-winbase-globallock) function to lock the memory object that contains [**RECT**](/windows/win32/api/windef/ns-windef-rect). Before returning, the clipboard owner must unlock the object by using the [**GlobalUnlock**](/windows/desktop/api/winbase/nf-winbase-globalunlock) function.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/dataxchg/wm-sizeclipboard#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_SIZECLIPBOARD = 779;

    /// <summary>Sent to the clipboard owner by a clipboard viewer window to request the name of a CF\_OWNERDISPLAY clipboard format.</summary>
    /// <returns>If an application processes this message, it should return zero.</returns>
    /// <remarks>
    /// <para>In response to this message, the clipboard owner should copy the name of the owner-display format to the specified buffer, not exceeding the buffer size specified by the *wParam* parameter. A clipboard viewer window sends this message to the clipboard owner to determine the name of the [**CF\_OWNERDISPLAY**](standard-clipboard-formats.md) format   for example, to initialize a menu listing available formats.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/dataxchg/wm-askcbformatname#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_ASKCBFORMATNAME = 780;

    /// <summary>Sent to the first window in the clipboard viewer chain when a window is being removed from the chain. A window receives this message through its WindowProc function.</summary>
    /// <returns>If an application processes this message, it should return zero.</returns>
    /// <remarks>
    /// <para>Each clipboard viewer window saves the handle to the next window in the clipboard viewer chain. Initially, this handle is the return value of the [**SetClipboardViewer**](/windows/desktop/api/Winuser/nf-winuser-setclipboardviewer) function. When a clipboard viewer window receives the **WM\_CHANGECBCHAIN** message, it should call the [**SendMessage**](/windows/desktop/api/winuser/nf-winuser-sendmessage) function to pass the message to the next window in the chain, unless the next window is the window being removed. In this case, the clipboard viewer should save the handle specified by the *lParam* parameter as the next window in the chain.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/dataxchg/wm-changecbchain#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_CHANGECBCHAIN = 781;

    /// <summary>Sent to the clipboard owner by a clipboard viewer window.</summary>
    /// <returns>If an application processes this message, it should return zero.</returns>
    /// <remarks>The clipboard owner can use the [**ScrollWindow**](https://msdn.microsoft.com/library/Cc410994(v=MSDN.10).aspx) function to scroll the image in the clipboard viewer window and invalidate the appropriate region.</remarks>
    public const int WM_HSCROLLCLIPBOARD = 782;

    /// <summary>The WM\_QUERYNEWPALETTE message informs a window that it is about to receive the keyboard focus, giving the window the opportunity to realize its logical palette when it receives the focus.</summary>
    /// <returns>If the window realizes its logical palette, it must return **TRUE**; otherwise, it must return **FALSE**.</returns>
    /// <remarks>
    /// <para><see href="https://learn.microsoft.com/windows/win32/gdi/wm-querynewpalette">Learn more about this API from docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_QUERYNEWPALETTE = 783;

    /// <summary>The WM\_PALETTEISCHANGING message informs applications that an application is going to realize its logical palette.</summary>
    /// <returns>If an application processes this message, it should return zero.</returns>
    /// <remarks>
    /// <para>The application changing its palette does not wait for acknowledgment of this message before changing the palette and sending the [**WM\_PALETTECHANGED**](wm-palettechanged.md) message. As a result, the palette may already be changed by the time an application receives this message. If the application either ignores or fails to process this message and a second application realizes its palette while the first is using palette indexes, there is a strong possibility that the user will see unexpected colors during subsequent drawing operations.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/gdi/wm-paletteischanging#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_PALETTEISCHANGING = 784;

    /// <summary>The WM\_PALETTECHANGED message is sent to all top-level and overlapped windows after the window with the keyboard focus has realized its logical palette, thereby changing the system palette.</summary>
    /// <remarks>
    /// <para>This message must be sent to all top-level and overlapped windows, including the one that changed the system palette. If any child windows use a color palette, this message must be passed on to them as well. To avoid creating an infinite loop, a window that receives this message must not realize its palette, unless it determines that *wParam* does not contain its own window handle.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/gdi/wm-palettechanged#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_PALETTECHANGED = 785;

    /// <summary>Posted when the user presses a hot key registered by the RegisterHotKey function. The message is placed at the top of the message queue associated with the thread that registered the hot key.</summary>
    /// <remarks>**WM\_HOTKEY** is unrelated to the [**WM\_GETHOTKEY**](wm-gethotkey.md) and [**WM\_SETHOTKEY**](wm-sethotkey.md) hot keys. The **WM\_HOTKEY** message is sent for generic hot keys while the **WM\_SETHOTKEY** and **WM\_GETHOTKEY** messages relate to window activation hot keys.</remarks>
    public const int WM_HOTKEY = 786;

    /// <summary>The WM\_PRINT message is sent to a window to request that it draw itself in the specified device context, most commonly in a printer device context.</summary>
    /// <remarks>The [**DefWindowProc**](/windows/desktop/api/winuser/nf-winuser-defwindowproca) function processes this message based on which drawing option is specified: if PRF\_CHECKVISIBLE is specified and the window is not visible, do nothing, if PRF\_NONCLIENT is specified, draw the nonclient area in the specified device context, if PRF\_ERASEBKGND is specified, send the window a [**WM\_ERASEBKGND**](../winmsg/wm-erasebkgnd.md) message, if PRF\_CLIENT is specified, send the window a [**WM\_PRINTCLIENT**](wm-printclient.md) message, if PRF\_CHILDREN is set, send each visible child window a **WM\_PRINT** message, if PRF\_OWNED is set, send each visible owned window a **WM\_PRINT** message.</remarks>
    public const int WM_PRINT = 791;

    /// <summary>Notifies a window that the user generated an application command event, for example, by clicking an application command button using the mouse or typing an application command key on the keyboard.</summary>
    /// <returns>If an application processes this message, it should return **TRUE**. For more information about processing the return value, see the Remarks section.</returns>
    /// <remarks>
    /// <para>[**DefWindowProc**](/windows/desktop/api/winuser/nf-winuser-defwindowproca) generates the **WM\_APPCOMMAND** message when it processes the [**WM\_XBUTTONUP**](wm-xbuttonup.md) or [**WM\_NCXBUTTONUP**](wm-ncxbuttonup.md) message, or when the user types an application command key. If a child window does not process this message and instead calls [**DefWindowProc**](/windows/desktop/api/winuser/nf-winuser-defwindowproca), **DefWindowProc** will send the message to its parent window. If a top level window does not process this message and instead calls **DefWindowProc**, **DefWindowProc** will call a shell hook with the hook code equal to **HSHELL\_APPCOMMAND**. To get the coordinates of the cursor if the message was generated by a mouse click, the application can call [**GetMessagePos**](/windows/desktop/api/winuser/nf-winuser-getmessagepos). An application can test whether the message was generated by the mouse by checking whether *lParam* contains **FAPPCOMMAND\_MOUSE**. Unlike other windows messages, an application should return **TRUE** from this message if it processes it. Doing so will allow software that simulates this message on Windows systems earlier than Windows 2000 to determine whether the window procedure processed the message or called [**DefWindowProc**](/windows/desktop/api/winuser/nf-winuser-defwindowproca) to process it.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/inputdev/wm-appcommand#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_APPCOMMAND = 793;

    /// <summary>Broadcast to every window following a theme change event. Examples of theme change events are the activation of a theme, the deactivation of a theme, or a transition from one theme to another.</summary>
    /// <returns>
    /// <para>Type: **LRESULT** If an application processes this message, it should return zero.</para>
    /// </returns>
    /// <remarks>
    /// <para>A window receives this message through its [**WindowProc**](/previous-versions/windows/desktop/legacy/ms633573(v=vs.85)) function. > [!Note] > This message is posted by the operating system. Applications typically do not send this message.</para>
    /// <para>Themes are specifications for the appearance of controls, so that the visual element of a control is treated separately from its functionality. To release an existing theme handle, call [**CloseThemeData**](/windows/win32/api/uxtheme/nf-uxtheme-closethemedata). To acquire a new theme handle, use [**OpenThemeData**](/windows/win32/api/uxtheme/nf-uxtheme-openthemedata). Following the **WM\_THEMECHANGED** broadcast, any existing theme handles are invalid. A theme-aware window should release and reopen any of its pre-existing theme handles when it receives the **WM\_THEMECHANGED** message. If the [**OpenThemeData**](/windows/win32/api/uxtheme/nf-uxtheme-openthemedata) function returns **NULL**, the window should paint unthemed.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/winmsg/wm-themechanged#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_THEMECHANGED = 794;

    /// <summary>Sent when the contents of the clipboard have changed.</summary>
    /// <returns>If an application processes this message, it should return zero.</returns>
    /// <remarks>To register a window to receive this message, use the [**AddClipboardFormatListener**](/windows/desktop/api/Winuser/nf-winuser-addclipboardformatlistener) function.</remarks>
    public const int WM_CLIPBOARDUPDATE = 797;

    /// <summary>Informs all top-level windows that Desktop Window Manager (DWM) composition has been enabled or disabled.</summary>
    /// <returns>If an application processes this message, it should return zero.</returns>
    /// <remarks>
    /// <para>A window receives this message through its [**WindowProc**](/previous-versions/windows/desktop/legacy/ms633573(v=vs.85)) function. The [**DwmIsCompositionEnabled**](/windows/desktop/api/Dwmapi/nf-dwmapi-dwmiscompositionenabled) function can be used to determine the current composition state.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/dwm/wm-dwmcompositionchanged#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_DWMCOMPOSITIONCHANGED = 798;

    /// <summary>Sent when the non-client area rendering policy has changed.</summary>
    /// <returns>If an application processes this message, it should return zero.</returns>
    /// <remarks>
    /// <para>A window receives this message through its [**WindowProc**](/previous-versions/windows/desktop/legacy/ms633573(v=vs.85)) function. The [**DwmGetWindowAttribute**](/windows/desktop/api/Dwmapi/nf-dwmapi-dwmgetwindowattribute) and [**DwmSetWindowAttribute**](/windows/desktop/api/Dwmapi/nf-dwmapi-dwmsetwindowattribute) functions are used to get or set the non-client rendering policy.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/dwm/wm-dwmncrenderingchanged#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_DWMNCRENDERINGCHANGED = 799;

    /// <summary>Informs all top-level windows that the colorization color has changed.</summary>
    /// <returns>If an application processes this message, it should return zero.</returns>
    /// <remarks>
    /// <para>A window receives this message through its [**WindowProc**](/previous-versions/windows/desktop/legacy/ms633573(v=vs.85)) function. [**DwmGetColorizationColor**](/windows/desktop/api/Dwmapi/nf-dwmapi-dwmgetcolorizationcolor) is used to determine the current color value.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/dwm/wm-dwmcolorizationcolorchanged#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_DWMCOLORIZATIONCOLORCHANGED = 800;

    /// <summary>Sent when a Desktop Window Manager (DWM) composed window is maximized.</summary>
    /// <returns>If an application processes this message, it should return zero.</returns>
    /// <remarks>A window receives this message through its [**WindowProc**](/previous-versions/windows/desktop/legacy/ms633573(v=vs.85)) function.</remarks>
    public const int WM_DWMWINDOWMAXIMIZEDCHANGE = 801;

    /// <summary>Instructs a window to provide a static bitmap to use as a thumbnail representation of that window.</summary>
    /// <returns>If an application processes this message, it should return zero.</returns>
    /// <remarks>
    /// <para>DWM sends this message to a window if all of the following situations are true: -   DWM is displaying an iconic representation of the window. -   The [**DWMWA\_HAS\_ICONIC\_BITMAP**](/windows/desktop/api/Dwmapi/ne-dwmapi-dwmwindowattribute) attribute is set on the window. -   The window did not set a cached bitmap. -   There is room in the cache for another bitmap. The window that receives this message should respond by generating a bitmap that is not larger than the size that is requested in the message parameters. The window then calls the [**DwmSetIconicThumbnail**](/windows/desktop/api/Dwmapi/nf-dwmapi-dwmseticonicthumbnail) function to override the default thumbnail. If the window does not supply a bitmap in a given amount of time, DWM uses its own default iconic representation for the window. The window must belong to the calling process.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/dwm/wm-dwmsendiconicthumbnail#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_DWMSENDICONICTHUMBNAIL = 803;

    /// <summary>Instructs a window to provide a static bitmap to use as a live preview (also known as a Peek preview) of that window.</summary>
    /// <returns>If an application processes this message, it should return zero.</returns>
    /// <remarks>
    /// <para>A *live preview* (also known as a *Peek preview*) of a window appears when a user moves the mouse pointer over the window's thumbnail in the taskbar or gives the thumbnail focus in the ALT+TAB window. This view is a full-sized preview of the window and can be a live snapshot or an iconic representation. Desktop Window Manager (DWM) sends this message to a window if all of the following situations are true: -   Live preview has been invoked on the window. -   The [**DWMWA\_HAS\_ICONIC\_BITMAP**](/windows/desktop/api/Dwmapi/ne-dwmapi-dwmwindowattribute) attribute is set on the window. -   An iconic representation is the only one that exists for this window. The window that receives this message should respond by generating a full-scale bitmap. The window then calls the [**DwmSetIconicLivePreviewBitmap**](/windows/desktop/api/Dwmapi/nf-dwmapi-dwmseticoniclivepreviewbitmap) function to set the live preview. If the window does not set a bitmap in a given amount of time, DWM uses its own default iconic representation for the window.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/dwm/wm-dwmsendiconiclivepreviewbitmap#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_DWMSENDICONICLIVEPREVIEWBITMAP = 806;

    /// <summary>Sent to request extended title bar information. A window receives this message through its WindowProc function.</summary>
    /// <remarks>
    /// <para>The following example shows how the message receiver casts an **LPARAM** value to retrieve the [**TITLEBARINFOEX**](/windows/win32/api/winuser/ns-winuser-titlebarinfoex) structure. `TITLEBARINFOEX *ptinfo = (TITLEBARINFOEX *)lParam;`</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/menurc/wm-gettitlebarinfoex#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_GETTITLEBARINFOEX = 831;

    public const int WM_HANDHELDFIRST = 856;

    public const int WM_HANDHELDLAST = 863;

    public const int WM_AFXFIRST = 864;

    public const int WM_AFXLAST = 895;

    public const int WM_PENWINFIRST = 896;

    public const int WM_PENWINLAST = 911;

    /// <summary>Used to define private messages, usually of the form WM\_APP+x, where x is an integer value.</summary>
    /// <remarks>
    /// <para>The **WM\_APP** constant is used to distinguish between message values that are reserved for use by the system and values that can be used by an application to send messages within a private window class. The following are the ranges of message numbers available.</para>
    /// <para>| Range                                                 | Meaning                                                        | |-------------------------------------------------------|----------------------------------------------------------------| | 0 through [**WM\_USER**](wm-user.md) –1<br/>   | Messages reserved for use by the system.<br/>            | | [**WM\_USER**](wm-user.md) through 0x7FFF<br/> | Integer messages for use by private window classes.<br/> | | **WM\_APP** through 0xBFFF<br/>                 | Messages available for use by applications.<br/>         | | 0xC000 through 0xFFFF<br/>                      | String messages for use by applications.<br/>            | | Greater than 0xFFFF<br/>                        | Reserved by the system.<br/>                             |</para>
    /// <para>Message numbers in the first range (0 through [**WM\_USER**](wm-user.md) –1) are defined by the system. Values in this range that are not explicitly defined are reserved by the system. Message numbers in the second range ([**WM\_USER**](wm-user.md) through 0x7FFF) can be defined and used by an application to send messages within a private window class. These values cannot be used to define messages that are meaningful throughout an application because some predefined window classes already define values in this range. For example, predefined control classes such as **BUTTON**, **EDIT**, **LISTBOX**, and **COMBOBOX** may use these values. Messages in this range should not be sent to other applications unless the applications have been designed to exchange messages and to attach the same meaning to the message numbers. Message numbers in the third range (0x8000 through 0xBFFF) are available for applications to use as private messages. Messages in this range do not conflict with system messages. Message numbers in the fourth range (0xC000 through 0xFFFF) are defined at run time when an application calls the [**RegisterWindowMessage**](/windows/win32/api/winuser/nf-winuser-registerwindowmessagea) function to retrieve a message number for a string. All applications that register the same string can use the associated message number for exchanging messages. The actual message number, however, is not a constant and cannot be assumed to be the same between different sessions. Message numbers in the fifth range (greater than 0xFFFF) are reserved by the system.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/winmsg/wm-app#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_APP = 32768;

    /// <summary>Used to define private messages for use by private window classes, usually of the form WM\_USER+x, where x is an integer value.</summary>
    /// <remarks>
    /// <para>The following are the ranges of message numbers.</para>
    /// <para>| Range                                                        | Meaning                                                        | |--------------------------------------------------------------|----------------------------------------------------------------| | 0 through **WM\_USER** –1<br/>                         | Messages reserved for use by the system.<br/>            | | **WM\_USER** through 0x7FFF<br/>                       | Integer messages for use by private window classes.<br/> | | [**WM\_APP**](wm-app.md) (0x8000) through 0xBFFF<br/> | Messages available for use by applications.<br/>         | | 0xC000 through 0xFFFF<br/>                             | String messages for use by applications.<br/>            | | Greater than 0xFFFF<br/>                               | Reserved by the system.<br/>                             |</para>
    /// <para>Message numbers in the first range (0 through **WM\_USER** –1) are defined by the system. Values in this range that are not explicitly defined are reserved by the system. Message numbers in the second range (**WM\_USER** through 0x7FFF) can be defined and used by an application to send messages within a private window class. These values cannot be used to define messages that are meaningful throughout an application because some predefined window classes already define values in this range. For example, predefined control classes such as **BUTTON**, **EDIT**, **LISTBOX**, and **COMBOBOX** may use these values. Messages in this range should not be sent to other applications unless the applications have been designed to exchange messages and to attach the same meaning to the message numbers. Message numbers in the third range (0x8000 through 0xBFFF) are available for applications to use as private messages. Messages in this range do not conflict with system messages. Message numbers in the fourth range (0xC000 through 0xFFFF) are defined at run time when an application calls the [**RegisterWindowMessage**](/windows/win32/api/winuser/nf-winuser-registerwindowmessagea) function to retrieve a message number for a string. All applications that register the same string can use the associated message number for exchanging messages. The actual message number, however, is not a constant and cannot be assumed to be the same between different sessions. Message numbers in the fifth range (greater than 0xFFFF) are reserved by the system.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/winmsg/wm-user#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_USER = 1024;

    public const int WM_TOOLTIPDISMISS = 837;

    public const int WM_FI_FILENAME = 900;

    public const int WM_CODEC_ONEPASS_CBR = 1;

    public const int WM_CODEC_ONEPASS_VBR = 2;

    public const int WM_CODEC_TWOPASS_CBR = 4;

    public const int WM_CODEC_TWOPASS_VBR_UNCONSTRAINED = 8;

    public const int WM_CODEC_TWOPASS_VBR_PEAKCONSTRAINED = 16;

    public const int WM_CAP_START = 1024;

    public const int WM_CAP_UNICODE_START = 1124;

    public const int WM_CAP_GET_CAPSTREAMPTR = 1025;

    public const int WM_CAP_SET_CALLBACK_ERRORW = 1126;

    public const int WM_CAP_SET_CALLBACK_STATUSW = 1127;

    public const int WM_CAP_SET_CALLBACK_ERRORA = 1026;

    public const int WM_CAP_SET_CALLBACK_STATUSA = 1027;

    /// <summary>The WM\_CAP\_SET\_CALLBACK\_ERROR message sets an error callback function in the client application. AVICap calls this procedure when errors occur. You can send this message explicitly or by using the capSetCallbackOnError macro.</summary>
    /// <returns>
    /// <para><span id="fpProc"></span><span id="fpproc"></span><span id="FPPROC"></span>*fpProc* Pointer to the error callback function, of type [**capErrorCallback**](/windows/desktop/api/Vfw/nc-vfw-caperrorcallbacka). Specify **NULL** for this parameter to disable a previously installed error callback function.</para>
    /// <para>Returns **TRUE** if successful or **FALSE** if streaming capture or a single-frame capture session is in progress.</para>
    /// </returns>
    /// <remarks>
    /// <para>Applications can optionally set an error callback function. If set, AVICap calls the error procedure in the following situations: -   The disk is full. -   A capture window cannot be connected with a capture driver. -   A waveform-audio device cannot be opened. -   The number of frames dropped during capture exceeds the specified percentage. -   The frames cannot be captured due to vertical synchronization interrupt problems.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/Multimedia/wm-cap-set-callback-error#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_CAP_SET_CALLBACK_ERROR = 1126;

    /// <summary>The WM\_CAP\_SET\_CALLBACK\_STATUS message sets a status callback function in the application. AVICap calls this procedure whenever the capture window status changes. You can send this message explicitly or by using the capSetCallbackOnStatus macro.</summary>
    /// <returns>
    /// <para><span id="fpProc"></span><span id="fpproc"></span><span id="FPPROC"></span>*fpProc* Pointer to the status callback function, of type [**capStatusCallback**](/windows/desktop/api/Vfw/nc-vfw-capstatuscallbacka). Specify **NULL** for this parameter to disable a previously installed status callback function.</para>
    /// <para>Returns **TRUE** if successful or **FALSE** if streaming capture or a single-frame capture session is in progress.</para>
    /// </returns>
    /// <remarks>
    /// <para>Applications can optionally set a status callback function. If set, AVICap calls this procedure in the following situations: -   A capture session is completed. -   A capture driver successfully connected to a capture window. -   An optimal palette is created. -   The number of captured frames is reported.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/Multimedia/wm-cap-set-callback-status#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_CAP_SET_CALLBACK_STATUS = 1127;

    /// <summary>The WM\_CAP\_SET\_CALLBACK\_YIELD message sets a callback function in the application. AVICap calls this procedure when the capture window yields during streaming capture. You can send this message explicitly or by using the capSetCallbackOnYield macro.</summary>
    /// <returns>
    /// <para><span id="fpProc"></span><span id="fpproc"></span><span id="FPPROC"></span>*fpProc* Pointer to the yield callback function, of type [**capYieldCallback**](/windows/desktop/api/Vfw/nc-vfw-capyieldcallback). Specify **NULL** for this parameter to disable a previously installed yield callback function.</para>
    /// <para>Returns **TRUE** if successful or **FALSE** if streaming capture or a single-frame capture session is in progress.</para>
    /// </returns>
    /// <remarks>
    /// <para>Applications can optionally set a yield callback function. The yield callback function is called at least once for each video frame captured during streaming capture. If a yield callback function is installed, it will be called regardless of the state of the **fYield** member of the [**CAPTUREPARMS**](/windows/win32/api/vfw/ns-vfw-captureparms) structure. If the yield callback function is used, it must be installed before starting the capture session and it must remain enabled for the duration of the session. It can be disabled after streaming capture ends. Applications typically perform some type of message processing in the callback function consisting of a [PeekMessage](/windows/win32/api/winuser/nf-winuser-peekmessagea), [TranslateMessage](/windows/win32/api/winuser/nf-winuser-translatemessage), [DispatchMessage](/windows/win32/api/winuser/nf-winuser-dispatchmessage) loop, as in the message loop of a [WinMain](/windows/win32/api/winbase/nf-winbase-winmain) function. The yield callback function must also filter and remove messages that can cause reentrancy problems. An application typically returns **TRUE** in the yield procedure to continue streaming capture. If a yield callback function returns **FALSE**, the capture window stops the capture process.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/Multimedia/wm-cap-set-callback-yield#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_CAP_SET_CALLBACK_YIELD = 1028;

    /// <summary>The WM\_CAP\_SET\_CALLBACK\_FRAME message sets a preview callback function in the application. AVICap calls this procedure when the capture window captures preview frames. You can send this message explicitly or by using the capSetCallbackOnFrame macro.</summary>
    /// <returns>
    /// <para><span id="fpProc"></span><span id="fpproc"></span><span id="FPPROC"></span>*fpProc* Pointer to the preview callback function, of type [**capVideoStreamCallback**](/windows/desktop/api/Vfw/nc-vfw-capvideocallback). Specify **NULL** for this parameter to disable a previously installed callback function.</para>
    /// <para>Returns **TRUE** if successful or **FALSE** if streaming capture or a single-frame capture session is in progress.</para>
    /// </returns>
    /// <remarks>The capture window calls the callback function before displaying preview frames. This allows an application to modify the frame if desired. This callback function is not used during streaming video capture.</remarks>
    public const int WM_CAP_SET_CALLBACK_FRAME = 1029;

    /// <summary>The WM\_CAP\_SET\_CALLBACK\_VIDEOSTREAM message sets a callback function in the application.</summary>
    /// <returns>
    /// <para><span id="fpProc"></span><span id="fpproc"></span><span id="FPPROC"></span>*fpProc* Pointer to the video-stream callback function, of type [**capVideoStreamCallback**](/windows/desktop/api/Vfw/nc-vfw-capvideocallback). Specify **NULL** for this parameter to disable a previously installed video-stream callback function.</para>
    /// <para>Returns **TRUE** if successful or **FALSE** if streaming capture or a single-frame capture session is in progress.</para>
    /// </returns>
    /// <remarks>
    /// <para>The capture window calls the callback function before writing the captured frame to disk. This allows applications to modify the frame if desired. If a video stream callback function is used for streaming capture, the procedure must be installed before starting the capture session and it must remain enabled for the duration of the session. It can be disabled after streaming capture ends.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/Multimedia/wm-cap-set-callback-videostream#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_CAP_SET_CALLBACK_VIDEOSTREAM = 1030;

    /// <summary>The WM\_CAP\_SET\_CALLBACK\_WAVESTREAM message sets a callback function in the application.</summary>
    /// <returns>
    /// <para><span id="fpProc"></span><span id="fpproc"></span><span id="FPPROC"></span>*fpProc* Pointer to the wave stream callback function, of type [**capWaveStreamCallback**](/windows/desktop/api/Vfw/nc-vfw-capwavecallback). Specify **NULL** for this parameter to disable a previously installed wave stream callback function.</para>
    /// <para>Returns **TRUE** if successful or **FALSE** if streaming capture or a single-frame capture session is in progress.</para>
    /// </returns>
    /// <remarks>
    /// <para>The capture window calls the procedure before writing the audio buffer to disk. This allows applications to modify the audio buffer if desired. If a wave stream callback function is used, it must be installed before starting the capture session and it must remain enabled for the duration of the session. It can be disabled after streaming capture ends.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/Multimedia/wm-cap-set-callback-wavestream#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_CAP_SET_CALLBACK_WAVESTREAM = 1031;

    /// <summary>The WM\_CAP\_GET\_USER\_DATA message retrieves a LONG\_PTR data value associated with a capture window. You can send this message explicitly or by using the capGetUserData macro.</summary>
    /// <returns>Returns a value previously saved by using the [**WM\_CAP\_SET\_USER\_DATA**](wm-cap-set-user-data.md) message.</returns>
    /// <remarks>
    /// <para><see href="https://learn.microsoft.com/windows/win32/Multimedia/wm-cap-get-user-data">Learn more about this API from docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_CAP_GET_USER_DATA = 1032;

    /// <summary>The WM\_CAP\_SET\_USER\_DATA message associates a LONG\_PTR data value with a capture window. You can send this message explicitly or by using the capSetUserData macro.</summary>
    /// <returns>
    /// <para><span id="lUser"></span><span id="luser"></span><span id="LUSER"></span>*lUser* Data value to associate with a capture window.</para>
    /// <para>Returns **TRUE** if successful or **FALSE** if streaming capture is in progress.</para>
    /// </returns>
    /// <remarks>Typically this message is used to point to a block of data associated with a capture window.</remarks>
    public const int WM_CAP_SET_USER_DATA = 1033;

    /// <summary>The WM\_CAP\_DRIVER\_CONNECT message connects a capture window to a capture driver. You can send this message explicitly or by using the capDriverConnect macro.</summary>
    /// <returns>
    /// <para><span id="iIndex"></span><span id="iindex"></span><span id="IINDEX"></span>*iIndex* Index of the capture driver. The index can range from 0 through 9.</para>
    /// <para>Returns **TRUE** if successful or **FALSE** if the specified capture driver cannot be connected to the capture window.</para>
    /// </returns>
    /// <remarks>Connecting a capture driver to a capture window automatically disconnects any previously connected capture driver.</remarks>
    public const int WM_CAP_DRIVER_CONNECT = 1034;

    /// <summary>The WM\_CAP\_DRIVER\_DISCONNECT message disconnects a capture driver from a capture window. You can send this message explicitly or by using the capDriverDisconnect macro.</summary>
    /// <returns>Returns **TRUE** if successful or **FALSE** if the capture window is not connected to a capture driver.</returns>
    /// <remarks>
    /// <para><see href="https://learn.microsoft.com/windows/win32/Multimedia/wm-cap-driver-disconnect">Learn more about this API from docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_CAP_DRIVER_DISCONNECT = 1035;

    public const int WM_CAP_DRIVER_GET_NAMEA = 1036;

    public const int WM_CAP_DRIVER_GET_VERSIONA = 1037;

    public const int WM_CAP_DRIVER_GET_NAMEW = 1136;

    public const int WM_CAP_DRIVER_GET_VERSIONW = 1137;

    /// <summary>The WM\_CAP\_DRIVER\_GET\_NAME message returns the name of the capture driver connected to the capture window. You can send this message explicitly or by using the capDriverGetName macro.</summary>
    /// <returns>
    /// <para><span id="wSize"></span><span id="wsize"></span><span id="WSIZE"></span>*wSize* Size, in bytes, of the buffer referenced by**szName**.</para>
    /// <para><span id="szName"></span><span id="szname"></span><span id="SZNAME"></span>*szName* Pointer to an application-defined buffer used to return the device name as a null-terminated string.</para>
    /// <para>Returns **TRUE** if successful or **FALSE** if the capture window is not connected to a capture driver.</para>
    /// </returns>
    /// <remarks>The name is a text string retrieved from the driver's resource area. Applications should allocate approximately 80 bytes for this string. If the driver does not contain a name resource, the full path name of the driver listed in the registry or in the SYSTEM.INI file is returned.</remarks>
    public const int WM_CAP_DRIVER_GET_NAME = 1136;

    /// <summary>The WM\_CAP\_DRIVER\_GET\_VERSION message returns the version information of the capture driver connected to a capture window. You can send this message explicitly or by using the capDriverGetVersion macro.</summary>
    /// <returns>
    /// <para><span id="wSize"></span><span id="wsize"></span><span id="WSIZE"></span>*wSize* Size, in bytes, of the application-defined buffer referenced by**szVer**.</para>
    /// <para><span id="szVer"></span><span id="szver"></span><span id="SZVER"></span>*szVer* Pointer to an application-defined buffer used to return the version information as a null-terminated string.</para>
    /// <para>Returns **TRUE** if successful or **FALSE** if the capture window is not connected to a capture driver.</para>
    /// </returns>
    /// <remarks>The version information is a text string retrieved from the driver's resource area. Applications should allocate approximately 40 bytes for this string. If version information is not available, a **NULL** string is returned.</remarks>
    public const int WM_CAP_DRIVER_GET_VERSION = 1137;

    /// <summary>The WM\_CAP\_DRIVER\_GET\_CAPS message returns the hardware capabilities of the capture driver currently connected to a capture window. You can send this message explicitly or by using the capDriverGetCaps macro.</summary>
    /// <returns>
    /// <para><span id="wSize"></span><span id="wsize"></span><span id="WSIZE"></span>*wSize* Size, in bytes, of the structure referenced by**s**.</para>
    /// <para><span id="psCaps"></span><span id="pscaps"></span><span id="PSCAPS"></span>*psCaps* Pointer to the [**CAPDRIVERCAPS**](/windows/win32/api/vfw/ns-vfw-capdrivercaps) structure to contain the hardware capabilities.</para>
    /// <para>Returns **TRUE** if successful or **FALSE** if the capture window is not connected to a capture driver.</para>
    /// </returns>
    /// <remarks>The capabilities returned in [**CAPDRIVERCAPS**](/windows/win32/api/vfw/ns-vfw-capdrivercaps) are constant for a given capture driver. Applications need to retrieve this information once when the capture driver is first connected to a capture window.</remarks>
    public const int WM_CAP_DRIVER_GET_CAPS = 1038;

    public const int WM_CAP_FILE_SET_CAPTURE_FILEA = 1044;

    public const int WM_CAP_FILE_GET_CAPTURE_FILEA = 1045;

    public const int WM_CAP_FILE_SAVEASA = 1047;

    public const int WM_CAP_FILE_SAVEDIBA = 1049;

    public const int WM_CAP_FILE_SET_CAPTURE_FILEW = 1144;

    public const int WM_CAP_FILE_GET_CAPTURE_FILEW = 1145;

    public const int WM_CAP_FILE_SAVEASW = 1147;

    public const int WM_CAP_FILE_SAVEDIBW = 1149;

    /// <summary>The WM\_CAP\_FILE\_SET\_CAPTURE\_FILE message names the file used for video capture. You can send this message explicitly or by using the capFileSetCaptureFile macro.</summary>
    /// <returns>
    /// <para><span id="szName"></span><span id="szname"></span><span id="SZNAME"></span>*szName* Pointer to the null-terminated string that contains the name of the capture file to use.</para>
    /// <para>Returns **TRUE** if successful or **FALSE** if the filename is invalid, or if streaming or single-frame capture is in progress.</para>
    /// </returns>
    /// <remarks>This message stores the filename in an internal structure. It does not create, allocate, or open the specified file. The default capture filename is C:\\CAPTURE.AVI.</remarks>
    public const int WM_CAP_FILE_SET_CAPTURE_FILE = 1144;

    /// <summary>The WM\_CAP\_FILE\_GET\_CAPTURE\_FILE message returns the name of the current capture file. You can send this message explicitly or by using the capFileGetCaptureFile macro.</summary>
    /// <returns>
    /// <para><span id="wSize"></span><span id="wsize"></span><span id="WSIZE"></span>*wSize* Size, in bytes, of the application-defined buffer referenced by**szName**.</para>
    /// <para><span id="szName"></span><span id="szname"></span><span id="SZNAME"></span>*szName* Pointer to an application-defined buffer used to return the name of the capture file as a null-terminated string.</para>
    /// <para>Returns **TRUE** if successful or **FALSE** otherwise.</para>
    /// </returns>
    /// <remarks>The default capture filename is C:\\CAPTURE.AVI.</remarks>
    public const int WM_CAP_FILE_GET_CAPTURE_FILE = 1145;

    /// <summary>The WM\_CAP\_FILE\_SAVEAS message copies the contents of the capture file to another file. You can send this message explicitly or by using the capFileSaveAs macro.</summary>
    /// <returns>
    /// <para><span id="szName"></span><span id="szname"></span><span id="SZNAME"></span>*szName* Pointer to the null-terminated string that contains the name of the destination file used to copy the file.</para>
    /// <para>Returns **TRUE** if successful or **FALSE** otherwise. If an error occurs and an error callback function is set using the [**WM\_CAP\_SET\_CALLBACK\_ERROR**](wm-cap-set-callback-error.md) message, the error callback function is called.</para>
    /// </returns>
    /// <remarks>
    /// <para>This message does not change the name or contents of the current capture file. If the copy operation is unsuccessful due to a disk full error, the destination file is automatically deleted. Typically, a capture file is preallocated for the largest capture segment anticipated and only a portion of it might be used to capture data. This message copies only the portion of the file containing the capture data.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/Multimedia/wm-cap-file-saveas#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_CAP_FILE_SAVEAS = 1147;

    /// <summary>The WM\_CAP\_FILE\_SAVEDIB message copies the current frame to a DIB file. You can send this message explicitly or by using the capFileSaveDIB macro.</summary>
    /// <returns>
    /// <para><span id="szName"></span><span id="szname"></span><span id="SZNAME"></span>*szName* Pointer to the null-terminated string that contains the name of the destination DIB file.</para>
    /// <para>Returns **TRUE** if successful or **FALSE** otherwise. If an error occurs and an error callback function is set using the [**WM\_CAP\_SET\_CALLBACK\_ERROR**](wm-cap-set-callback-error.md) message, the error callback function is called.</para>
    /// </returns>
    /// <remarks>If the capture driver supplies frames in a compressed format, this call attempts to decompress the frame before writing the file.</remarks>
    public const int WM_CAP_FILE_SAVEDIB = 1149;

    /// <summary>The WM\_CAP\_FILE\_ALLOCATE message creates (preallocates) a capture file of a specified size. You can send this message explicitly or by using the capFileAlloc macro.</summary>
    /// <returns>
    /// <para><span id="dwSize"></span><span id="dwsize"></span><span id="DWSIZE"></span>*dwSize* Size, in bytes, to create the capture file.</para>
    /// <para>Returns **TRUE** if successful or **FALSE** otherwise. If an error occurs and an error callback function is set using the [**WM\_CAP\_SET\_CALLBACK\_ERROR**](wm-cap-set-callback-error.md) message, the error callback function is called.</para>
    /// </returns>
    /// <remarks>You can improve streaming capture performance significantly by preallocating a capture file large enough to store an entire video clip and by defragmenting the capture file before capturing the clip.</remarks>
    public const int WM_CAP_FILE_ALLOCATE = 1046;

    /// <summary>The WM\_CAP\_FILE\_SET\_INFOCHUNK message sets and clears information chunks.</summary>
    /// <returns>
    /// <para><span id="lpInfoChunk"></span><span id="lpinfochunk"></span><span id="LPINFOCHUNK"></span>*lpInfoChunk* Pointer to a [**CAPINFOCHUNK**](/windows/win32/api/vfw/ns-vfw-capinfochunk) structure defining the information chunk to be created or deleted.</para>
    /// <para>Returns **TRUE** if successful or **FALSE** otherwise. If an error occurs and an error callback function is set using the [**WM\_CAP\_SET\_CALLBACK\_ERROR**](wm-cap-set-callback-error.md) message, the error callback function is called.</para>
    /// </returns>
    /// <remarks>Multiple registered information chunks can be added to an AVI file. After an information chunk is set, it continues to be added to subsequent capture files until either the entry is cleared or all information chunk entries are cleared. To clear a single entry, specify the information chunk in the **fccInfoID** member and **NULL** in the **lpData** member of the [**CAPINFOCHUNK**](/windows/win32/api/vfw/ns-vfw-capinfochunk) structure. To clear all entries, specify **NULL** in **fccInfoID**.</remarks>
    public const int WM_CAP_FILE_SET_INFOCHUNK = 1048;

    /// <summary>The WM\_CAP\_EDIT\_COPY message copies the contents of the video frame buffer and associated palette to the clipboard. You can send this message explicitly or by using the capEditCopy macro.</summary>
    /// <returns>Returns **TRUE** if successful or **FALSE** otherwise.</returns>
    /// <remarks>
    /// <para><see href="https://learn.microsoft.com/windows/win32/Multimedia/wm-cap-edit-copy">Learn more about this API from docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_CAP_EDIT_COPY = 1054;

    /// <summary>The WM\_CAP\_SET\_AUDIOFORMAT message sets the audio format to use when performing streaming or step capture. You can send this message explicitly or by using the capSetAudioFormat macro.</summary>
    /// <returns>
    /// <para><span id="wSize"></span><span id="wsize"></span><span id="WSIZE"></span>*wSize* Size, in bytes, of the structure referenced by **s**.</para>
    /// <para><span id="psAudioFormat"></span><span id="psaudioformat"></span><span id="PSAUDIOFORMAT"></span>*psAudioFormat* Pointer to a [**WAVEFORMATEX**](/windows/win32/api/mmeapi/ns-mmeapi-waveformatex) or [**PCMWAVEFORMAT**](/windows/win32/api/mmreg/ns-mmreg-pcmwaveformat) structure that defines the audio format.</para>
    /// <para>Returns **TRUE** if successful or **FALSE** otherwise.</para>
    /// </returns>
    /// <remarks>
    /// <para><see href="https://learn.microsoft.com/windows/win32/Multimedia/wm-cap-set-audioformat">Learn more about this API from docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_CAP_SET_AUDIOFORMAT = 1059;

    /// <summary>The WM\_CAP\_GET\_AUDIOFORMAT message obtains the audio format or the size of the audio format. You can send this message explicitly or by using the capGetAudioFormat and capGetAudioFormatSize macros.</summary>
    /// <returns>
    /// <para><span id="wSize"></span><span id="wsize"></span><span id="WSIZE"></span>*wSize* Size, in bytes, of the structure referenced by**s**.</para>
    /// <para><span id="psAudioFormat"></span><span id="psaudioformat"></span><span id="PSAUDIOFORMAT"></span>*psAudioFormat* Pointer to a [**WAVEFORMATEX**](/windows/win32/api/mmeapi/ns-mmeapi-waveformatex) structure, or **NULL**. If the value is **NULL**, the size, in bytes, required to hold the structure is returned.</para>
    /// <para>Returns the size, in bytes, of the audio format.</para>
    /// </returns>
    /// <remarks>Because compressed audio formats vary in size requirements applications must first retrieve the size, then allocate memory, and finally request the audio format data.</remarks>
    public const int WM_CAP_GET_AUDIOFORMAT = 1060;

    /// <summary>The WM\_CAP\_DLG\_VIDEOFORMAT message displays a dialog box in which the user can select the video format.</summary>
    /// <returns>Returns **TRUE** if successful or **FALSE** otherwise.</returns>
    /// <remarks>
    /// <para>After this message returns, applications might need to update the [**CAPSTATUS**](/windows/win32/api/vfw/ns-vfw-capstatus) structure because the user might have changed the image dimensions. The Video Format dialog box is unique for each capture driver. Some capture drivers might not support a Video Format dialog box. Applications can determine if the capture driver supports this message by checking the **fHasDlgVideoFormat** member of [**CAPDRIVERCAPS**](/windows/win32/api/vfw/ns-vfw-capdrivercaps).</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/Multimedia/wm-cap-dlg-videoformat#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_CAP_DLG_VIDEOFORMAT = 1065;

    /// <summary>The WM\_CAP\_DLG\_VIDEOSOURCE message displays a dialog box in which the user can control the video source.</summary>
    /// <returns>Returns **TRUE** if successful or **FALSE** otherwise.</returns>
    /// <remarks>The Video Source dialog box is unique for each capture driver. Some capture drivers might not support a Video Source dialog box. Applications can determine if the capture driver supports this message by checking the **fHasDlgVideoSource** member of the [**CAPDRIVERCAPS**](/windows/win32/api/vfw/ns-vfw-capdrivercaps) structure.</remarks>
    public const int WM_CAP_DLG_VIDEOSOURCE = 1066;

    /// <summary>The WM\_CAP\_DLG\_VIDEODISPLAY message displays a dialog box in which the user can set or adjust the video output.</summary>
    /// <returns>Returns **TRUE** if successful or **FALSE** otherwise.</returns>
    /// <remarks>
    /// <para>The controls in this dialog box do not affect digitized video data; they affect only the output or redisplay of the video signal. The Video Display dialog box is unique for each capture driver. Some capture drivers might not support a Video Display dialog box. Applications can determine if the capture driver supports this message by checking the **fHasDlgVideoDisplay** member of the [**CAPDRIVERCAPS**](/windows/win32/api/vfw/ns-vfw-capdrivercaps) structure.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/Multimedia/wm-cap-dlg-videodisplay#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_CAP_DLG_VIDEODISPLAY = 1067;

    /// <summary>The WM\_CAP\_GET\_VIDEOFORMAT message retrieves a copy of the video format in use or the size required for the video format. You can send this message explicitly or by using the capGetVideoFormat and capGetVideoFormatSize macros.</summary>
    /// <returns>
    /// <para><span id="wSize"></span><span id="wsize"></span><span id="WSIZE"></span>*wSize* Size, in bytes, of the structure referenced by**s**.</para>
    /// <para><span id="psVideoFormat"></span><span id="psvideoformat"></span><span id="PSVIDEOFORMAT"></span>*psVideoFormat* Pointer to a [**BITMAPINFO**](/windows/win32/api/wingdi/ns-wingdi-bitmapinfo) structure. You can also specify **NULL** to retrieve the number of bytes needed.</para>
    /// <para>Returns the size, in bytes, of the video format or zero if the capture window is not connected to a capture driver. For video formats that require a palette, the current palette is also returned.</para>
    /// </returns>
    /// <remarks>Because compressed video formats vary in size requirements applications must first retrieve the size, then allocate memory, and finally request the video format data.</remarks>
    public const int WM_CAP_GET_VIDEOFORMAT = 1068;

    /// <summary>The WM\_CAP\_SET\_VIDEOFORMAT message sets the format of captured video data. You can send this message explicitly or by using the capSetVideoFormat macro.</summary>
    /// <returns>
    /// <para><span id="wSize"></span><span id="wsize"></span><span id="WSIZE"></span>*wSize* Size, in bytes, of the structure referenced by **s**.</para>
    /// <para><span id="psVideoFormat"></span><span id="psvideoformat"></span><span id="PSVIDEOFORMAT"></span>*psVideoFormat* Pointer to a [**BITMAPINFO**](/windows/win32/api/wingdi/ns-wingdi-bitmapinfo) structure.</para>
    /// <para>Returns **TRUE** if successful or **FALSE** otherwise.</para>
    /// </returns>
    /// <remarks>Because video formats are device-specific, applications should check the return value from this function to determine if the format is accepted by the driver.</remarks>
    public const int WM_CAP_SET_VIDEOFORMAT = 1069;

    /// <summary>The WM\_CAP\_DLG\_VIDEOCOMPRESSION message displays a dialog box in which the user can select a compressor to use during the capture process.</summary>
    /// <returns>Returns **TRUE** if successful or **FALSE** otherwise.</returns>
    /// <remarks>
    /// <para>Use this message with capture drivers that provide frames only in the BI\_RGB format. This message is most useful in the step capture operation to combine capture and compression in a single operation. Compressing frames with a software compressor as part of a real-time capture operation is most likely too time-consuming to perform. Compression does not affect the frames copied to the clipboard.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/Multimedia/wm-cap-dlg-videocompression#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_CAP_DLG_VIDEOCOMPRESSION = 1070;

    /// <summary>The WM\_CAP\_SET\_PREVIEW message enables or disables preview mode.</summary>
    /// <returns>
    /// <para><span id="f"></span><span id="F"></span>*f* Preview flag. Specify **TRUE** for this parameter to enable preview mode or **FALSE** to disable it.</para>
    /// <para>Returns **TRUE** if successful or **FALSE** otherwise.</para>
    /// </returns>
    /// <remarks>
    /// <para>The preview mode uses substantial CPU resources. Applications can disable preview or lower the preview rate when another application has the focus. The **fLiveWindow** member of the [**CAPSTATUS**](/windows/win32/api/vfw/ns-vfw-capstatus) structure indicates if preview mode is currently enabled. Enabling preview mode automatically disables overlay mode.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/Multimedia/wm-cap-set-preview#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_CAP_SET_PREVIEW = 1074;

    /// <summary>The WM\_CAP\_SET\_OVERLAY message enables or disables overlay mode. In overlay mode, video is displayed using hardware overlay. You can send this message explicitly or by using the capOverlay macro.</summary>
    /// <returns>
    /// <para><span id="f"></span><span id="F"></span>*f* Overlay flag. Specify **TRUE** for this parameter to enable overlay mode or **FALSE** to disable it.</para>
    /// <para>Returns **TRUE** if successful or **FALSE** otherwise.</para>
    /// </returns>
    /// <remarks>
    /// <para>Using an overlay does not require CPU resources. The **fHasOverlay** member of the [**CAPDRIVERCAPS**](/windows/win32/api/vfw/ns-vfw-capdrivercaps) structure indicates whether the device is capable of overlay. The **fOverlayWindow** member of the [**CAPSTATUS**](/windows/win32/api/vfw/ns-vfw-capstatus) structure indicates whether overlay mode is currently enabled. Enabling overlay mode automatically disables preview mode.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/Multimedia/wm-cap-set-overlay#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_CAP_SET_OVERLAY = 1075;

    /// <summary>The WM\_CAP\_SET\_PREVIEWRATE message sets the frame display rate in preview mode. You can send this message explicitly or by using the capPreviewRate macro.</summary>
    /// <returns>
    /// <para><span id="wMS"></span><span id="wms"></span><span id="WMS"></span>*wMS* Rate, in milliseconds, at which new frames are captured and displayed.</para>
    /// <para>Returns **TRUE** if successful or **FALSE** if the capture window is not connected to a capture driver.</para>
    /// </returns>
    /// <remarks>The preview mode uses substantial CPU resources. Applications can disable preview or lower the preview rate when another application has the focus. During streaming video capture, the previewing task is lower priority than writing frames to disk, and preview frames are displayed only if no other buffers are available for writing.</remarks>
    public const int WM_CAP_SET_PREVIEWRATE = 1076;

    /// <summary>The WM\_CAP\_SET\_SCALE message enables or disables scaling of the preview video images.</summary>
    /// <returns>
    /// <para><span id="f"></span><span id="F"></span>*f* Preview scaling flag. Specify **TRUE** for this parameter to stretch preview frames to the size of the capture window or **FALSE** to display them at their natural size.</para>
    /// <para>Returns **TRUE** if successful or **FALSE** otherwise.</para>
    /// </returns>
    /// <remarks>
    /// <para>Scaling preview images controls the immediate presentation of captured frames within the capture window. It has no effect on the size of the frames saved to file. Scaling has no effect when using overlay to display video in the frame buffer.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/Multimedia/wm-cap-set-scale#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_CAP_SET_SCALE = 1077;

    /// <summary>The WM\_CAP\_GET\_STATUS message retrieves the status of the capture window. You can send this message explicitly or by using the capGetStatus macro.</summary>
    /// <returns>
    /// <para><span id="wSize"></span><span id="wsize"></span><span id="WSIZE"></span>*wSize* Size, in bytes, of the structure referenced by**s**.</para>
    /// <para><span id="s"></span><span id="S"></span>*s* Pointer to a [**CAPSTATUS**](/windows/win32/api/vfw/ns-vfw-capstatus) structure.</para>
    /// <para>Returns **TRUE** if successful or **FALSE** if the capture window is not connected to a capture driver.</para>
    /// </returns>
    /// <remarks>The [**CAPSTATUS**](/windows/win32/api/vfw/ns-vfw-capstatus) structure contains the current state of the capture window. Since this state is dynamic and changes in response to various messages, the application should initialize this structure after sending the [**WM\_CAP\_DLG\_VIDEOFORMAT**](wm-cap-dlg-videoformat.md) message (or using the [**capDlgVideoFormat**](/windows/desktop/api/Vfw/nf-vfw-capdlgvideoformat) macro) and whenever it needs to enable menu items or determine the actual state of the window.</remarks>
    public const int WM_CAP_GET_STATUS = 1078;

    /// <summary>The WM\_CAP\_SET\_SCROLL message defines the portion of the video frame to display in the capture window.</summary>
    /// <returns>
    /// <para><span id="lpP"></span><span id="lpp"></span><span id="LPP"></span>*lpP* Address to contain the desired scroll position.</para>
    /// <para>Returns **TRUE** if successful or **FALSE** otherwise.</para>
    /// </returns>
    /// <remarks>The scroll position affects the image in both preview and overlay modes.</remarks>
    public const int WM_CAP_SET_SCROLL = 1079;

    /// <summary>The WM\_CAP\_GRAB\_FRAME message retrieves and displays a single frame from the capture driver. After capture, overlay and preview are disabled. You can send this message explicitly or by using the capGrabFrame macro.</summary>
    /// <returns>Returns **TRUE** if successful or **FALSE** otherwise.</returns>
    /// <remarks>For information about installing callback functions, see the [**WM\_CAP\_SET\_CALLBACK\_ERROR**](wm-cap-set-callback-error.md) and [**WM\_CAP\_SET\_CALLBACK\_FRAME**](wm-cap-set-callback-frame.md) messages.</remarks>
    public const int WM_CAP_GRAB_FRAME = 1084;

    /// <summary>The WM\_CAP\_GRAB\_FRAME\_NOSTOP message fills the frame buffer with a single uncompressed frame from the capture device and displays it.</summary>
    /// <returns>Returns **TRUE** if successful or **FALSE** otherwise.</returns>
    /// <remarks>For information about installing callback functions, see the [**WM\_CAP\_SET\_CALLBACK\_ERROR**](wm-cap-set-callback-error.md) and [**WM\_CAP\_SET\_CALLBACK\_FRAME**](wm-cap-set-callback-frame.md) messages.</remarks>
    public const int WM_CAP_GRAB_FRAME_NOSTOP = 1085;

    /// <summary>The WM\_CAP\_SEQUENCE message initiates streaming video and audio capture to a file. You can send this message explicitly or by using the capCaptureSequence macro.</summary>
    /// <returns>
    /// <para>Returns **TRUE** if successful or **FALSE** otherwise. If an error occurs and an error callback function is set using the [**WM\_CAP\_SET\_CALLBACK\_ERROR**](wm-cap-set-callback-error.md) message, the error callback function is called.</para>
    /// </returns>
    /// <remarks>
    /// <para>If you want to alter the parameters controlling streaming capture, use the [**WM\_CAP\_SET\_SEQUENCE\_SETUP**](wm-cap-set-sequence-setup.md) message prior to starting the capture. By default, the capture window does not allow other applications to continue running during capture. To override this, either set the **fYield** member of the [**CAPTUREPARMS**](/windows/win32/api/vfw/ns-vfw-captureparms) structure to **TRUE**, or install a yield callback function. During streaming capture, the capture window can optionally issue notifications to your application of specific types of conditions. To install callback procedures for these notifications, use the following messages: -   [**WM\_CAP\_SET\_CALLBACK\_ERROR**](wm-cap-set-callback-error.md) -   [**WM\_CAP\_SET\_CALLBACK\_STATUS**](wm-cap-set-callback-status.md) -   [**WM\_CAP\_SET\_CALLBACK\_YIELD**](wm-cap-set-callback-yield.md) -   [**WM\_CAP\_SET\_CALLBACK\_VIDEOSTREAM**](wm-cap-set-callback-videostream.md) -   [**WM\_CAP\_SET\_CALLBACK\_WAVESTREAM**](wm-cap-set-callback-wavestream.md)</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/Multimedia/wm-cap-sequence#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_CAP_SEQUENCE = 1086;

    /// <summary>The WM\_CAP\_SEQUENCE\_NOFILE message initiates streaming video capture without writing data to a file. You can send this message explicitly or by using the capCaptureSequenceNoFile macro.</summary>
    /// <returns>Returns **TRUE** if successful or **FALSE** otherwise.</returns>
    /// <remarks>
    /// <para>This message is useful in conjunction with video stream or waveform-audio stream callback functions that let your application use the video and audio data directly. If you want to alter the parameters controlling streaming capture, use the [**WM\_CAP\_SET\_SEQUENCE\_SETUP**](wm-cap-set-sequence-setup.md) message prior to starting the capture. By default, the capture window does not allow other applications to continue running during capture. To override this, either set the **fYield** member of the [**CAPTUREPARMS**](/windows/win32/api/vfw/ns-vfw-captureparms) structure to **TRUE**, or install a yield callback function. During streaming capture, the capture window can optionally issue notifications to your application of specific types of conditions. To install callback procedures for these notifications, use the following messages: -   [**WM\_CAP\_SET\_CALLBACK\_ERROR**](wm-cap-set-callback-error.md) -   [**WM\_CAP\_SET\_CALLBACK\_STATUS**](wm-cap-set-callback-status.md) -   [**WM\_CAP\_SET\_CALLBACK\_YIELD**](wm-cap-set-callback-yield.md) -   [**WM\_CAP\_SET\_CALLBACK\_VIDEOSTREAM**](wm-cap-set-callback-videostream.md) -   [**WM\_CAP\_SET\_CALLBACK\_WAVESTREAM**](wm-cap-set-callback-wavestream.md)</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/Multimedia/wm-cap-sequence-nofile#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_CAP_SEQUENCE_NOFILE = 1087;

    /// <summary>The WM\_CAP\_SET\_SEQUENCE\_SETUP message sets the configuration parameters used with streaming capture. You can send this message explicitly or by using the capCaptureSetSetup macro.</summary>
    /// <returns>
    /// <para><span id="wSize"></span><span id="wsize"></span><span id="WSIZE"></span>*wSize* Size, in bytes, of the structure referenced by**s**.</para>
    /// <para><span id="psCapParms"></span><span id="pscapparms"></span><span id="PSCAPPARMS"></span>*psCapParms* Pointer to a [**CAPTUREPARMS**](/windows/win32/api/vfw/ns-vfw-captureparms) structure.</para>
    /// <para>Returns **TRUE** if successful or **FALSE** otherwise.</para>
    /// </returns>
    /// <remarks>For information about the parameters used to control streaming capture, see the [**CAPTUREPARMS**](/windows/win32/api/vfw/ns-vfw-captureparms) structure.</remarks>
    public const int WM_CAP_SET_SEQUENCE_SETUP = 1088;

    /// <summary>The WM\_CAP\_GET\_SEQUENCE\_SETUP message retrieves the current settings of the streaming capture parameters. You can send this message explicitly or by using the capCaptureGetSetup macro.</summary>
    /// <returns>
    /// <para><span id="wSize"></span><span id="wsize"></span><span id="WSIZE"></span>*wSize* Size, in bytes, of the structure referenced by**s**.</para>
    /// <para><span id="s"></span><span id="S"></span>*s* Pointer to a [**CAPTUREPARMS**](/windows/win32/api/vfw/ns-vfw-captureparms) structure.</para>
    /// <para>Returns **TRUE** if successful or **FALSE** otherwise.</para>
    /// </returns>
    /// <remarks>For information about the parameters used to control streaming capture, see the [**CAPTUREPARMS**](/windows/win32/api/vfw/ns-vfw-captureparms) structure.</remarks>
    public const int WM_CAP_GET_SEQUENCE_SETUP = 1089;

    public const int WM_CAP_SET_MCI_DEVICEA = 1090;

    public const int WM_CAP_GET_MCI_DEVICEA = 1091;

    public const int WM_CAP_SET_MCI_DEVICEW = 1190;

    public const int WM_CAP_GET_MCI_DEVICEW = 1191;

    /// <summary>The WM\_CAP\_SET\_MCI\_DEVICE message specifies the name of the MCI video device to be used to capture data. You can send this message explicitly or by using the capSetMCIDeviceName macro.</summary>
    /// <returns>
    /// <para><span id="szName"></span><span id="szname"></span><span id="SZNAME"></span>*szName* Pointer to a null-terminated string containing the name of the device.</para>
    /// <para>Returns **TRUE** if successful or **FALSE** otherwise.</para>
    /// </returns>
    /// <remarks>This message stores the MCI device name in an internal structure. It does not open or access the device. The default device name is **NULL**.</remarks>
    public const int WM_CAP_SET_MCI_DEVICE = 1190;

    /// <summary>The WM\_CAP\_GET\_MCI\_DEVICE message retrieves the name of an MCI device previously set with the WM\_CAP\_SET\_MCI\_DEVICE message. You can send this message explicitly or by using the capGetMCIDeviceName macro.</summary>
    /// <returns>
    /// <para><span id="wSize"></span><span id="wsize"></span><span id="WSIZE"></span>*wSize* Length, in bytes, of the buffer referenced by**szName**.</para>
    /// <para><span id="szName"></span><span id="szname"></span><span id="SZNAME"></span>*szName* Pointer to a null-terminated string that contains the MCI device name.</para>
    /// <para>Returns **TRUE** if successful or **FALSE** otherwise.</para>
    /// </returns>
    /// <remarks>
    /// <para><see href="https://learn.microsoft.com/windows/win32/Multimedia/wm-cap-get-mci-device">Learn more about this API from docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_CAP_GET_MCI_DEVICE = 1191;

    /// <summary>The WM\_CAP\_STOP message stops the capture operation. You can send this message explicitly or by using the capCaptureStop macro.</summary>
    /// <returns>Returns **TRUE** if successful or **FALSE** otherwise.</returns>
    /// <remarks>The capture operation must yield to use this message. Use the [**WM\_CAP\_ABORT**](wm-cap-abort.md) message to abandon the current capture operation.</remarks>
    public const int WM_CAP_STOP = 1092;

    /// <summary>The WM\_CAP\_ABORT message stops the capture operation.</summary>
    /// <returns>Returns **TRUE** if successful or **FALSE** otherwise.</returns>
    /// <remarks>
    /// <para>The capture operation must yield to use this message. Use the [**WM\_CAP\_STOP**](wm-cap-stop.md) message to halt step capture at the current position, and then capture audio.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/Multimedia/wm-cap-abort#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_CAP_ABORT = 1093;

    /// <summary>The WM\_CAP\_SINGLE\_FRAME\_OPEN message opens the capture file for single-frame capturing. Any previous information in the capture file is overwritten. You can send this message explicitly or by using the capCaptureSingleFrameOpen macro.</summary>
    /// <returns>Returns **TRUE** if successful or **FALSE** otherwise.</returns>
    /// <remarks>For information about installing callback functions, see the [**WM\_CAP\_SET\_CALLBACK\_ERROR**](wm-cap-set-callback-error.md) and [**WM\_CAP\_SET\_CALLBACK\_FRAME**](wm-cap-set-callback-frame.md) messages.</remarks>
    public const int WM_CAP_SINGLE_FRAME_OPEN = 1094;

    /// <summary>The WM\_CAP\_SINGLE\_FRAME\_CLOSE message closes the capture file opened by the WM\_CAP\_SINGLE\_FRAME\_OPEN message. You can send this message explicitly or by using the capCaptureSingleFrameClose macro.</summary>
    /// <returns>Returns **TRUE** if successful or **FALSE** otherwise.</returns>
    /// <remarks>For information about installing callback functions, see the [**WM\_CAP\_SET\_CALLBACK\_ERROR**](wm-cap-set-callback-error.md) and [**WM\_CAP\_SET\_CALLBACK\_FRAME**](wm-cap-set-callback-frame.md) messages.</remarks>
    public const int WM_CAP_SINGLE_FRAME_CLOSE = 1095;

    /// <summary>The WM\_CAP\_SINGLE\_FRAME message appends a single frame to a capture file that was opened using the WM\_CAP\_SINGLE\_FRAME\_OPEN message. You can send this message explicitly or by using the capCaptureSingleFrame macro.</summary>
    /// <returns>Returns **TRUE** if successful or **FALSE** otherwise.</returns>
    /// <remarks>
    /// <para><see href="https://learn.microsoft.com/windows/win32/Multimedia/wm-cap-single-frame">Learn more about this API from docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_CAP_SINGLE_FRAME = 1096;

    public const int WM_CAP_PAL_OPENA = 1104;

    public const int WM_CAP_PAL_SAVEA = 1105;

    public const int WM_CAP_PAL_OPENW = 1204;

    public const int WM_CAP_PAL_SAVEW = 1205;

    /// <summary>The WM\_CAP\_PAL\_OPEN message loads a new palette from a palette file and passes it to a capture driver.</summary>
    /// <returns>
    /// <para><span id="szName"></span><span id="szname"></span><span id="SZNAME"></span>*szName* Pointer to a null-terminated string containing the palette filename.</para>
    /// <para>Returns **TRUE** if successful or **FALSE** otherwise. If an error occurs and an error callback function is set using the [**WM\_CAP\_SET\_CALLBACK\_ERROR**](wm-cap-set-callback-error.md) message, the error callback function is called.</para>
    /// </returns>
    /// <remarks>
    /// <para><see href="https://learn.microsoft.com/windows/win32/Multimedia/wm-cap-pal-open">Learn more about this API from docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_CAP_PAL_OPEN = 1204;

    /// <summary>The WM\_CAP\_PAL\_SAVE message saves the current palette to a palette file. Palette files typically use the filename extension .PAL. You can send this message explicitly or by using the capPaletteSave macro.</summary>
    /// <returns>
    /// <para><span id="szName"></span><span id="szname"></span><span id="SZNAME"></span>*szName* Pointer to a null-terminated string containing the palette filename.</para>
    /// <para>Returns **TRUE** if successful or **FALSE** otherwise. If an error occurs and an error callback function is set using the [**WM\_CAP\_SET\_CALLBACK\_ERROR**](wm-cap-set-callback-error.md) message, the error callback function is called.</para>
    /// </returns>
    /// <remarks>
    /// <para><see href="https://learn.microsoft.com/windows/win32/Multimedia/wm-cap-pal-save">Learn more about this API from docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_CAP_PAL_SAVE = 1205;

    /// <summary>The WM\_CAP\_PAL\_PASTE message copies the palette from the clipboard and passes it to a capture driver. You can send this message explicitly or by using the capPalettePaste macro.</summary>
    /// <returns>
    /// <para>Returns **TRUE** if successful or **FALSE** otherwise. If an error occurs and an error callback function is set using the [**WM\_CAP\_SET\_CALLBACK\_ERROR**](wm-cap-set-callback-error.md) message, the error callback function is called.</para>
    /// </returns>
    /// <remarks>A capture driver uses a palette when required by the specified digitized video format.</remarks>
    public const int WM_CAP_PAL_PASTE = 1106;

    /// <summary>The WM\_CAP\_PAL\_AUTOCREATE message requests that the capture driver sample video frames and automatically create a new palette. You can send this message explicitly or by using the capPaletteAuto macro.</summary>
    /// <returns>
    /// <para><span id="iFrames"></span><span id="iframes"></span><span id="IFRAMES"></span>*iFrames* Number of frames to sample.</para>
    /// <para><span id="iColors"></span><span id="icolors"></span><span id="ICOLORS"></span>*iColors* Number of colors in the palette. The maximum value for this parameter is 256.</para>
    /// <para>Returns **TRUE** if successful or **FALSE** otherwise. If an error occurs and an error callback function is set using the [**WM\_CAP\_SET\_CALLBACK\_ERROR**](wm-cap-set-callback-error.md) message, the error callback function is called.</para>
    /// </returns>
    /// <remarks>The sampled video sequence should include all the colors you want in the palette. To obtain the best palette, you might have to sample the whole sequence rather than a portion of it.</remarks>
    public const int WM_CAP_PAL_AUTOCREATE = 1107;

    /// <summary>The WM\_CAP\_PAL\_MANUALCREATE message requests that the capture driver manually sample video frames and create a new palette. You can send this message explicitly or by using the capPaletteManual macro.</summary>
    /// <returns>
    /// <para><span id="fGrab"></span><span id="fgrab"></span><span id="FGRAB"></span>*fGrab* Palette histogram flag. Set this parameter to **TRUE** for each frame included in creating the optimal palette. After the last frame has been collected, set this parameter to **FALSE** to calculate the optimal palette and send it to the capture driver.</para>
    /// <para><span id="iColors"></span><span id="icolors"></span><span id="ICOLORS"></span>*iColors* Number of colors in the palette. The maximum value for this parameter is 256. This value is used only during collection of the first frame in a sequence.</para>
    /// <para>Returns **TRUE** if successful or **FALSE** otherwise. If an error occurs and an error callback function is set using the [**WM\_CAP\_SET\_CALLBACK\_ERROR**](wm-cap-set-callback-error.md) message, the error callback function is called.</para>
    /// </returns>
    /// <remarks>
    /// <para><see href="https://learn.microsoft.com/windows/win32/Multimedia/wm-cap-pal-manualcreate">Learn more about this API from docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_CAP_PAL_MANUALCREATE = 1108;

    /// <summary>The WM\_CAP\_SET\_CALLBACK\_CAPCONTROL message sets a callback function in the application giving it precise recording control. You can send this message explicitly or by using the capSetCallbackOnCapControl macro.</summary>
    /// <returns>
    /// <para><span id="fpProc"></span><span id="fpproc"></span><span id="FPPROC"></span>*fpProc* Pointer to the callback function, of type [**capControlCallback**](/windows/desktop/api/Vfw/nc-vfw-capcontrolcallback). Specify **NULL** for this parameter to disable a previously installed callback function.</para>
    /// <para>Returns **TRUE** if successful or **FALSE** if a streaming capture or a single-frame capture session is in progress.</para>
    /// </returns>
    /// <remarks>A single callback function is used to give the application precise control over the moments that streaming capture begins and completes. The capture window first calls the procedure with *nState* set to CONTROLCALLBACK\_PREROLL after all buffers have been allocated and all other capture preparations have finished. This gives the application the ability to preroll video sources, returning from the callback function at the exact moment recording is to begin. A return value of **TRUE** from the callback function continues capture, and a return value of **FALSE** aborts capture. After capture begins, this callback function will be called frequently with *nState* set to CONTROLCALLBACK\_CAPTURING to allow the application to end capture by returning **FALSE**.</remarks>
    public const int WM_CAP_SET_CALLBACK_CAPCONTROL = 1109;

    public const int WM_CAP_UNICODE_END = 1205;

    public const int WM_CAP_END = 1205;

    public const int WM_SampleExtension_ContentType_Size = 1;

    public const int WM_SampleExtension_PixelAspectRatio_Size = 2;

    public const int WM_SampleExtension_Timecode_Size = 14;

    public const int WM_SampleExtension_SampleDuration_Size = 2;

    public const int WM_SampleExtension_ChromaLocation_Size = 1;

    public const int WM_SampleExtension_ColorSpaceInfo_Size = 3;

    public const int WM_CT_REPEAT_FIRST_FIELD = 16;

    public const int WM_CT_BOTTOM_FIELD_FIRST = 32;

    public const int WM_CT_TOP_FIELD_FIRST = 64;

    public const int WM_CT_INTERLACED = 128;

    public const int WM_CL_INTERLACED420 = 0;

    public const int WM_CL_PROGRESSIVE420 = 1;

    public const int WM_MAX_VIDEO_STREAMS = 63;

    public const int WM_MAX_STREAMS = 63;

    /// <summary>An Active Directory property sheet extension calls the ADsPropGetInitInfo to obtain data about regarding the directory object that the property sheet extension applies to.</summary>
    /// <returns>This message has no return value.</returns>
    /// <remarks>
    /// <para><see href="https://learn.microsoft.com/windows/win32/AD/wm-adsprop-notify-pageinit">Learn more about this API from docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_ADSPROP_NOTIFY_PAGEINIT = 2125;

    /// <summary>An Active Directory directory service property sheet extension calls the ADsPropSetHwnd to inform the notification object of the property page window handle.</summary>
    /// <returns>This message has no return value.</returns>
    /// <remarks>An Active Directory property sheet extension normally calls the [**ADsPropSetHwnd**](/windows/desktop/api/Adsprop/nf-adsprop-adspropsethwnd) function while processing the [**WM\_INITDIALOG**](../dlgbox/wm-initdialog.md) message.</remarks>
    public const int WM_ADSPROP_NOTIFY_PAGEHWND = 2126;

    /// <summary>The WM\_ADSPROP\_NOTIFY\_CHANGE message is used internally by the notification object.</summary>
    /// <returns>This message has no return value.</returns>
    /// <remarks>
    /// <para><see href="https://learn.microsoft.com/windows/win32/AD/wm-adsprop-notify-change">Learn more about this API from docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_ADSPROP_NOTIFY_CHANGE = 2127;

    /// <summary>An Active Directory directory service property sheet extension sends the WM\_ADSPROP\_NOTIFY\_APPLY message to the notification object if the property page PSN\_APPLY handler succeeds.</summary>
    /// <returns>This message has no return value.</returns>
    /// <remarks>When adding pages to the Active Directory Manager MMC snap-in, Active Directory MMC property sheets create the notification objects by a call to the [**ADsPropCreateNotifyObj**](/windows/desktop/api/Adsprop/nf-adsprop-adspropcreatenotifyobj) function, and then passes the notification object handle to each property page.</remarks>
    public const int WM_ADSPROP_NOTIFY_APPLY = 2128;

    /// <summary>The WM\_ADSPROP\_NOTIFY\_SETFOCUS message is used internally by the notification object.</summary>
    /// <returns>This message has no return value.</returns>
    /// <remarks>
    /// <para><see href="https://learn.microsoft.com/windows/win32/AD/wm-adsprop-notify-setfocus">Learn more about this API from docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_ADSPROP_NOTIFY_SETFOCUS = 2129;

    /// <summary>The WM\_ADSPROP\_NOTIFY\_FOREGROUND message is used internally by the notification object.</summary>
    /// <returns>This message has no return value.</returns>
    /// <remarks>
    /// <para><see href="https://learn.microsoft.com/windows/win32/AD/wm-adsprop-notify-foreground">Learn more about this API from docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_ADSPROP_NOTIFY_FOREGROUND = 2130;

    /// <summary>An Active Directory property sheet extension sends the WM\_ADSPROP\_NOTIFY\_EXIT message to the notification object when the notification object is no longer required.</summary>
    /// <returns>This message has no return value.</returns>
    /// <remarks>The notification object will delete itself in response to this message. When this message has been sent, the notification object handle should be considered invalid.</remarks>
    public const int WM_ADSPROP_NOTIFY_EXIT = 2131;

    /// <summary>The WM\_ADSPROP\_NOTIFY\_ERROR message adds an error message to a list of error messages that are displayed by calling the ADsPropShowErrorDialog function.</summary>
    /// <returns>This message has no return value.</returns>
    /// <remarks>
    /// <para>The [**ADsPropSendErrorMessage**](/windows/desktop/api/Adsprop/nf-adsprop-adspropsenderrormessage) function is the preferred method of sending this message. The error messages added by the **WM\_ADSPROP\_NOTIFY\_ERROR** message are accumulated until [**ADsPropShowErrorDialog**](/windows/desktop/api/Adsprop/nf-adsprop-adspropshowerrordialog) is called. **ADsPropShowErrorDialog** combines and displays the accumulated error messages. When the error dialog is dismissed, the accumulated error messages are deleted.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/AD/wm-adsprop-notify-error#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_ADSPROP_NOTIFY_ERROR = 2134;

    /// <summary>The operating system sends a WM\_RASDIALEVENT message to a window procedure when a change of state event occurs during a RAS connection process.</summary>
    /// <returns>If an application processes this message, it should return **TRUE**.</returns>
    /// <remarks>
    /// <para><see href="https://learn.microsoft.com/windows/win32/RRAS/wm-rasdialevent">Learn more about this API from docs.microsoft.com</see>.</para>
    /// </remarks>
    public const int WM_RASDIALEVENT = 52429;

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
