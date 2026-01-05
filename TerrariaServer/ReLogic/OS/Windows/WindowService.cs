using System;
using System.Drawing;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;

namespace ReLogic.OS.Windows;

internal class WindowService : IWindowService
{
	public float GetScaling()
	{
		try {
			IntPtr hdc = System.Drawing.Graphics.FromHwnd(IntPtr.Zero).GetHdc();
			int deviceCaps = NativeMethods.GetDeviceCaps(hdc, NativeMethods.DeviceCap.VertRes);
			return (float)NativeMethods.GetDeviceCaps(hdc, NativeMethods.DeviceCap.DesktopVertRes) / (float)deviceCaps;
		}
		catch (Exception) {
			return 1f;
		}
	}

	public void SetQuickEditEnabled(bool enabled)
	{
		IntPtr stdHandle = NativeMethods.GetStdHandle(NativeMethods.StdHandleType.Input);
		if (NativeMethods.GetConsoleMode(stdHandle, out var lpMode)) {
			lpMode = ((!enabled) ? (lpMode & ~NativeMethods.ConsoleMode.QuickEditMode) : (lpMode | NativeMethods.ConsoleMode.QuickEditMode));
			NativeMethods.SetConsoleMode(stdHandle, lpMode);
		}
	}

	public void SetUnicodeTitle(GameWindow window, string title)
	{
		NativeMethods.WndProcCallback d = NativeMethods.DefWindowProc;
		int dwNewLong = NativeMethods.SetWindowLong(window.Handle, -4, (int)Marshal.GetFunctionPointerForDelegate((Delegate)d));
		window.Title = title;
		NativeMethods.SetWindowLong(window.Handle, -4, dwNewLong);
	}

	public void StartFlashingIcon(GameWindow window)
	{
		NativeMethods.FlashInfo flashInfo = NativeMethods.FlashInfo.CreateStart(window.Handle);
		NativeMethods.FlashWindowEx(ref flashInfo);
	}

	public void StopFlashingIcon(GameWindow window)
	{
		NativeMethods.FlashInfo flashInfo = NativeMethods.FlashInfo.CreateStop(window.Handle);
		NativeMethods.FlashWindowEx(ref flashInfo);
	}
}
