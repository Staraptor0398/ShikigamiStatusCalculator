using ScenarioRunner.Automation.Model;
using System;
using System.Runtime.InteropServices;

namespace ScenarioRunner.Automation.Interop
{
	public static class WindowFrameHelper
	{
		private const int DWMWA_EXTENDED_FRAME_BOUNDS = 9;
		private const int DWMWA_VISIBLE_FRAME_BORDER_THICKNESS = 37;

		private const int GWL_STYLE = -16;
		private const int GWL_EXSTYLE = -20;

		private const int SM_CXSIZEFRAME = 32;
		private const int SM_CYSIZEFRAME = 33;
		private const int SM_CXPADDEDBORDER = 92;

		private const int SW_SHOWNORMAL = 1;
		private const int SW_SHOWMINIMIZED = 2;
		private const int SW_SHOWMAXIMIZED = 3;

		private const int ERROR = 0;

		private const uint MONITOR_DEFAULTTONEAREST = 2;

		[DllImport("user32.dll")]
		private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MonitorInfo lpmi);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool IsZoomed(IntPtr hwnd);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool IsIconic(IntPtr hwnd);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool IsWindowVisible(IntPtr hwnd);

		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool GetWindowRect(IntPtr hwnd, out Rect lpRect);

		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool GetClientRect(IntPtr hwnd, out Rect lpRect);

		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool ClientToScreen(IntPtr hwnd, ref Point lpPoint);

		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool GetWindowInfo(IntPtr hwnd, ref WindowInfo pwi);

		[DllImport("user32.dll", SetLastError = true)]
		private static extern int GetWindowRgnBox(IntPtr hwnd, out Rect lprc);

		[DllImport("user32.dll", SetLastError = true)]
		private static extern uint GetDpiForWindow(IntPtr hwnd);

		[DllImport("user32.dll", SetLastError = true)]
		private static extern int GetSystemMetricsForDpi(int nIndex, uint dpi);

		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool AdjustWindowRectExForDpi(ref Rect lpRect, uint dwStyle, [MarshalAs(UnmanagedType.Bool)] bool bMenu, uint dwExStyle, uint dpi);

		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool GetWindowPlacement(IntPtr hwnd, ref WindowPlacement lpwndpl);

		[DllImport("dwmapi.dll")]
		private static extern int DwmGetWindowAttribute(IntPtr hwnd, int dwAttribute, out Rect pvAttribute, int cbAttribute);

		[DllImport("dwmapi.dll")]
		private static extern int DwmGetWindowAttribute(IntPtr hwnd, int dwAttribute, out uint pvAttribute, int cbAttribute);

		[DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW", SetLastError = true)]
		private static extern IntPtr GetWindowLongPtr64(IntPtr hwnd, int nIndex);

		[DllImport("user32.dll", EntryPoint = "GetWindowLongW", SetLastError = true)]
		private static extern IntPtr GetWindowLongPtr32(IntPtr hwnd, int nIndex);

		public static WindowBounds GetAdjustedBounds(IntPtr windowHandle, WindowBounds targetVisibleBounds)
		{
			validateWindowHandle(windowHandle);

			if (targetVisibleBounds == null)
			{
				throw new ArgumentNullException(nameof(targetVisibleBounds));
			}

			WindowFrameInfo frameInfo = getWindowFrameInfo(windowHandle);

			WindowFrameInsets invisibleFrame = frameInfo.InvisibleFrame;

			return new WindowBounds(
			 targetVisibleBounds.X - invisibleFrame.Left,
			 targetVisibleBounds.Y - invisibleFrame.Top,
			 targetVisibleBounds.Width
			  + invisibleFrame.Left
			  + invisibleFrame.Right,
			 targetVisibleBounds.Height
			  + invisibleFrame.Top
			  + invisibleFrame.Bottom);
		}

		public static string GetDiagnostics(IntPtr windowHandle)
		{
			validateWindowHandle(windowHandle);

			WindowFrameInfo frameInfo = getWindowFrameInfo(windowHandle);

			WindowInfo windowInfo = getWindowInfo(windowHandle);

			WindowBounds windowInfoBounds = toWindowBounds(windowInfo.WindowRect);

			WindowBounds windowInfoClientBounds = toWindowBounds(windowInfo.ClientRect);

			uint visibleFrameBorderThickness;
			bool hasVisibleFrameBorderThickness = tryGetVisibleFrameBorderThickness(windowHandle, out visibleFrameBorderThickness);

			WindowBounds windowRegionBounds;
			bool hasWindowRegion = tryGetWindowRegionBounds(windowHandle, out windowRegionBounds);

			uint dpi;
			bool hasDpi = tryGetDpi(windowHandle, out dpi);

			uint style = unchecked((uint)getWindowLongPtr(windowHandle, GWL_STYLE).ToInt64());

			uint exStyle = unchecked((uint)getWindowLongPtr(windowHandle, GWL_EXSTYLE).ToInt64());

			string visibleFrameBorderThicknessText = hasVisibleFrameBorderThickness ? visibleFrameBorderThickness.ToString() : "Unsupported";

			string windowRegionText = hasWindowRegion ? formatBounds(windowRegionBounds) : "Unsupported";

			string dpiText = hasDpi ? dpi.ToString() : "Unsupported";

			string systemMetricsText = getSystemMetricsDiagnostics(dpi);

			string adjustedRectText = getAdjustedWindowRectDiagnostics(frameInfo.ClientBounds, style, exStyle, dpi);

			string placementText = getWindowPlacementDiagnostics(windowHandle);

			WindowFrameInsets invisibleFrame = frameInfo.InvisibleFrame;

			return
			 $"GetWindowRect: {formatBounds(frameInfo.WindowBounds)}\r\n" +
			 $"DWM Frame: {formatBounds(frameInfo.DwmFrameBounds)}\r\n" +
			 $"DWM Visible Frame Border Thickness: {visibleFrameBorderThicknessText}\r\n" +
			 $"Client Screen: {formatBounds(frameInfo.ClientBounds)}\r\n" +
			 $"WINDOWINFO Window: {formatBounds(windowInfoBounds)}\r\n" +
			 $"WINDOWINFO Client: {formatBounds(windowInfoClientBounds)}\r\n" +
			 $"WINDOWINFO Border: X={frameInfo.WindowBorderWidth}, Y={frameInfo.WindowBorderHeight}\r\n" +
			 $"Window Region: {windowRegionText}\r\n" +
			 $"Invisible Frame: Left={invisibleFrame.Left}, Top={invisibleFrame.Top}, Right={invisibleFrame.Right}, Bottom={invisibleFrame.Bottom}\r\n" +
			 $"\r\n" +
			 $"DPI: {dpiText}\r\n" +
			 $"Style: 0x{style:X8}\r\n" +
			 $"ExStyle: 0x{exStyle:X8}\r\n" +
			 $"{systemMetricsText}\r\n" +
			 $"{adjustedRectText}\r\n" +
			 $"{placementText}\r\n" +
			 $"IsZoomed: {IsZoomed(windowHandle)}\r\n" +
			 $"IsIconic: {IsIconic(windowHandle)}\r\n" +
			 $"IsWindowVisible: {IsWindowVisible(windowHandle)}\r\n" +
			 $"{getMonitorDiagnostics(windowHandle)}";
		}

		private static WindowBounds getWindowBounds(IntPtr windowHandle)
		{
			validateWindowHandle(windowHandle);

			if (!GetWindowRect(windowHandle, out Rect rect))
			{
				int errorCode = Marshal.GetLastWin32Error();
				throw new InvalidOperationException(
				 $"Failed to get window bounds. Win32 error: {errorCode}");
			}

			return toWindowBounds(rect);
		}

		private static WindowBounds getDwmFrameBounds(IntPtr windowHandle)
		{
			validateWindowHandle(windowHandle);

			int result = DwmGetWindowAttribute(windowHandle, DWMWA_EXTENDED_FRAME_BOUNDS, out Rect rect, Marshal.SizeOf(typeof(Rect)));

			if (result != 0)
			{
				throw new InvalidOperationException(
				 $"Failed to get DWM frame bounds. HRESULT: 0x{result:X8}");
			}

			return toWindowBounds(rect);
		}

		private static WindowBounds getClientBounds(IntPtr windowHandle)
		{
			validateWindowHandle(windowHandle);

			if (!GetClientRect(windowHandle, out Rect clientRect))
			{
				int errorCode = Marshal.GetLastWin32Error();
				throw new InvalidOperationException(
				 $"Failed to get client bounds. Win32 error: {errorCode}");
			}

			Point topLeft = new Point
			{
				X = clientRect.Left,
				Y = clientRect.Top
			};

			Point bottomRight = new Point
			{
				X = clientRect.Right,
				Y = clientRect.Bottom
			};

			if (!ClientToScreen(windowHandle, ref topLeft))
			{
				int errorCode = Marshal.GetLastWin32Error();
				throw new InvalidOperationException(
				 $"Failed to convert client top-left coordinates. Win32 error: {errorCode}");
			}

			if (!ClientToScreen(windowHandle, ref bottomRight))
			{
				int errorCode = Marshal.GetLastWin32Error();
				throw new InvalidOperationException(
				 $"Failed to convert client bottom-right coordinates. Win32 error: {errorCode}");
			}

			return new WindowBounds(
			 topLeft.X,
			 topLeft.Y,
			 bottomRight.X - topLeft.X,
			 bottomRight.Y - topLeft.Y);
		}

		private static WindowFrameInsets getInvisibleFrameInsets(WindowBounds windowBounds, WindowBounds dwmFrameBounds)
		{
			int left = Math.Max(0, dwmFrameBounds.X - windowBounds.X);

			int top = Math.Max(0, dwmFrameBounds.Y - windowBounds.Y);

			int right = Math.Max(0, (windowBounds.X + windowBounds.Width) - (dwmFrameBounds.X + dwmFrameBounds.Width));

			int bottom = Math.Max(0, (windowBounds.Y + windowBounds.Height) - (dwmFrameBounds.Y + dwmFrameBounds.Height));

			return new WindowFrameInsets(left, top, right, bottom);
		}

		private static WindowFrameInfo getWindowFrameInfo(IntPtr windowHandle)
		{
			validateWindowHandle(windowHandle);

			WindowBounds windowBounds = getWindowBounds(windowHandle);

			WindowBounds dwmFrameBounds = getDwmFrameBounds(windowHandle);

			WindowBounds clientBounds = getClientBounds(windowHandle);

			WindowInfo windowInfo = getWindowInfo(windowHandle);

			WindowFrameInsets invisibleFrame = getInvisibleFrameInsets(windowBounds, dwmFrameBounds);

			return new WindowFrameInfo(windowBounds, dwmFrameBounds, clientBounds, invisibleFrame, (int)windowInfo.WindowBordersX, (int)windowInfo.WindowBordersY);
		}

		private static bool tryGetVisibleFrameBorderThickness(IntPtr windowHandle, out uint thickness)
		{
			validateWindowHandle(windowHandle);

			int result = DwmGetWindowAttribute(windowHandle, DWMWA_VISIBLE_FRAME_BORDER_THICKNESS, out thickness, Marshal.SizeOf(typeof(uint)));

			return result == 0;
		}

		private static bool tryGetWindowRegionBounds(IntPtr windowHandle, out WindowBounds regionBounds)
		{
			validateWindowHandle(windowHandle);

			int result = GetWindowRgnBox(windowHandle, out Rect regionRect);

			if (result == ERROR)
			{
				regionBounds = null;
				return false;
			}

			WindowBounds windowBounds = getWindowBounds(windowHandle);

			regionBounds = new WindowBounds(
			 windowBounds.X + regionRect.Left,
			 windowBounds.Y + regionRect.Top,
			 regionRect.Right - regionRect.Left,
			 regionRect.Bottom - regionRect.Top);

			return true;
		}

		private static bool tryGetDpi(IntPtr windowHandle, out uint dpi)
		{
			validateWindowHandle(windowHandle);

			try
			{
				dpi = GetDpiForWindow(windowHandle);
				return dpi != 0;
			}
			catch (EntryPointNotFoundException)
			{
				dpi = 0;
				return false;
			}
		}

		private static string getSystemMetricsDiagnostics(uint dpi)
		{
			if (dpi == 0)
			{
				return "System Metrics For DPI: Unsupported";
			}

			try
			{
				int sizeFrameX = GetSystemMetricsForDpi(SM_CXSIZEFRAME, dpi);
				int sizeFrameY = GetSystemMetricsForDpi(SM_CYSIZEFRAME, dpi);
				int paddedBorder = GetSystemMetricsForDpi(SM_CXPADDEDBORDER, dpi);

				return
				 $"System Metrics For DPI: " +
				 $"CXSizeFrame={sizeFrameX}, " +
				 $"CYSizeFrame={sizeFrameY}, " +
				 $"CXPaddedBorder={paddedBorder}, " +
				 $"ResizeBorderX={sizeFrameX + paddedBorder}, " +
				 $"ResizeBorderY={sizeFrameY + paddedBorder}";
			}
			catch (EntryPointNotFoundException)
			{
				return "System Metrics For DPI: Unsupported";
			}
		}

		private static string getAdjustedWindowRectDiagnostics(WindowBounds clientBounds, uint style, uint exStyle, uint dpi)
		{
			if (dpi == 0)
			{
				return "AdjustWindowRectExForDpi: Unsupported";
			}

			Rect rect = new Rect
			{
				Left = 0,
				Top = 0,
				Right = clientBounds.Width,
				Bottom = clientBounds.Height
			};

			try
			{
				if (!AdjustWindowRectExForDpi(ref rect, style, false, exStyle, dpi))
				{
					int errorCode = Marshal.GetLastWin32Error();

					return $"AdjustWindowRectExForDpi: Failed, Win32 error: {errorCode}";
				}

				return
				 $"AdjustWindowRectExForDpi: " +
				 $"Left={rect.Left}, " +
				 $"Top={rect.Top}, " +
				 $"Right={rect.Right}, " +
				 $"Bottom={rect.Bottom}, " +
				 $"Width={rect.Right - rect.Left}, " +
				 $"Height={rect.Bottom - rect.Top}";
			}
			catch (EntryPointNotFoundException)
			{
				return "AdjustWindowRectExForDpi: Unsupported";
			}
		}

		private static string getWindowPlacementDiagnostics(IntPtr windowHandle)
		{
			WindowPlacement placement = new WindowPlacement
			{
				Length = Marshal.SizeOf(typeof(WindowPlacement))
			};

			if (!GetWindowPlacement(windowHandle, ref placement))
			{
				int errorCode = Marshal.GetLastWin32Error();

				return
				 $"Window Placement: Failed, Win32 error: {errorCode}";
			}

			string state;

			switch (placement.ShowCommand)
			{
				case SW_SHOWNORMAL:
					state = "Normal";
					break;
				case SW_SHOWMINIMIZED:
					state = "Minimized";
					break;
				case SW_SHOWMAXIMIZED:
					state = "Maximized";
					break;
				default:
					state = placement.ShowCommand.ToString();
					break;
			}

			WindowBounds normalBounds = toWindowBounds(placement.NormalPosition);

			return
			 $"Window Placement: " +
			 $"State={state}, " +
			 $"NormalPosition={formatBounds(normalBounds)}";
		}

		private static WindowInfo getWindowInfo(IntPtr windowHandle)
		{
			validateWindowHandle(windowHandle);

			WindowInfo windowInfo = new WindowInfo
			{
				Size = (uint)Marshal.SizeOf(typeof(WindowInfo))
			};

			if (!GetWindowInfo(windowHandle, ref windowInfo))
			{
				int errorCode = Marshal.GetLastWin32Error();

				throw new InvalidOperationException(
				 $"Failed to get window information. Win32 error: {errorCode}");
			}

			return windowInfo;
		}

		private static IntPtr getWindowLongPtr(IntPtr windowHandle, int index)
		{
			if (IntPtr.Size == 8)
			{
				return GetWindowLongPtr64(windowHandle, index);
			}

			return GetWindowLongPtr32(windowHandle, index);
		}

		private static string getMonitorDiagnostics(IntPtr windowHandle)
		{
			IntPtr monitorHandle = MonitorFromWindow(windowHandle, MONITOR_DEFAULTTONEAREST);

			if (monitorHandle == IntPtr.Zero)
			{
				return "Monitor: Failed";
			}

			MonitorInfo monitorInfo = new MonitorInfo
			{
				Size = (uint)Marshal.SizeOf(typeof(MonitorInfo))
			};

			if (!GetMonitorInfo(monitorHandle, ref monitorInfo))
			{
				int errorCode = Marshal.GetLastWin32Error();

				return $"Monitor: Failed, Win32 error: {errorCode}";
			}

			WindowBounds monitorBounds = toWindowBounds(monitorInfo.MonitorRect);

			WindowBounds workAreaBounds = toWindowBounds(monitorInfo.WorkRect);

			return
			 $"Monitor Bounds: {formatBounds(monitorBounds)}\r\n" +
			 $"Monitor Work Area: {formatBounds(workAreaBounds)}";
		}

		private static void validateWindowHandle(IntPtr windowHandle)
		{
			if (windowHandle == IntPtr.Zero)
			{
				throw new ArgumentException("Window handle is invalid.", nameof(windowHandle));
			}
		}

		private static WindowBounds toWindowBounds(Rect rect)
		{
			return new WindowBounds(
			 rect.Left,
			 rect.Top,
			 rect.Right - rect.Left,
			 rect.Bottom - rect.Top);
		}

		private static string formatBounds(WindowBounds bounds)
		{
			return
			 $"X={bounds.X}, Y={bounds.Y}, " +
			 $"Width={bounds.Width}, Height={bounds.Height}";
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct Point
		{
			public int X;
			public int Y;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct Rect
		{
			public int Left;
			public int Top;
			public int Right;
			public int Bottom;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct WindowInfo
		{
			public uint Size;
			public Rect WindowRect;
			public Rect ClientRect;
			public uint Style;
			public uint ExStyle;
			public uint WindowStatus;
			public uint WindowBordersX;
			public uint WindowBordersY;
			public ushort WindowType;
			public ushort CreatorVersion;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct WindowPlacement
		{
			public int Length;
			public int Flags;
			public int ShowCommand;
			public Point MinPosition;
			public Point MaxPosition;
			public Rect NormalPosition;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		private struct MonitorInfo
		{
			public uint Size;
			public Rect MonitorRect;
			public Rect WorkRect;
			public uint Flags;
		}
	}
}
