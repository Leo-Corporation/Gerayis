/*
MIT License

Copyright (c) Léo Corporation

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE. 
*/

using Gerayis.Classes;
using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Gerayis.Windows;

/// <summary>
/// Interaction logic for SeeFullBarCode.xaml
/// </summary>
public partial class SeeFullBarCodeWindow : Window
{
	internal BitmapSource BarCode { get; init; }
	public SeeFullBarCodeWindow(BitmapSource barCode)
	{
		InitializeComponent();
		BarCode = barCode;

		InitUI(); // Load the UI
	}

	private void InitUI()
	{
		BarCodeImg.Source = BarCode; // Set image
		StateChanged += (o, e) =>
		{
			MaximizeBtn.Content = WindowState == WindowState.Maximized ? "\uF670" : "\uFA40"; // Set text
				MaximizeBtn.FontSize = WindowState == WindowState.Minimized ? 18 : 14;
			DefineMaximumSize();
		};

		LocationChanged += (o, e) => DefineMaximumSize();
		Loaded += (o, e) => DefineMaximumSize();
	}

	private void MinimizeBtn_Click(object sender, RoutedEventArgs e)
	{
		WindowState = WindowState.Minimized; // Minimize the window
	}

	private void CloseBtn_Click(object sender, RoutedEventArgs e)
	{
		Close(); // Close the window
	}

	private void CopyBtn_Click(object sender, RoutedEventArgs e)
	{
		Clipboard.SetImage(BarCode); // Copy bar code
	}

	private void SaveBtn_Click(object sender, RoutedEventArgs e)
	{
		SaveFileDialog saveFileDialog = new()
		{
			Filter = "PNG|*.png",
			FileName = $"{Properties.Resources.BarCode}.png",
			Title = Properties.Resources.Save
		}; // Create Save file dialog

		if (saveFileDialog.ShowDialog() ?? true)
		{
			Global.SaveImage(saveFileDialog.FileName, BarCode);
		}
	}

	private void MaximizeBtn_Click(object sender, RoutedEventArgs e)
	{
		DefineMaximumSize();

		WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized; // Set
		MaximizeBtn.Content = WindowState == WindowState.Maximized ? "\uF670" : "\uFA40"; // Set text
		MaximizeBtn.FontSize = WindowState == WindowState.Minimized ? 18 : 14;
	}

	private void DefineMaximumSize()
	{
		WindowBorder.Margin = WindowState == WindowState.Maximized ? new(10, 10, 0, 0) : new(10); // Set

		System.Windows.Forms.Screen currentScreen = System.Windows.Forms.Screen.FromHandle(new System.Windows.Interop.WindowInteropHelper(this).Handle); // The current screen

		float dpiX, dpiY;
		double scaling = 100; // Default scaling = 100%

		using (System.Drawing.Graphics graphics = System.Drawing.Graphics.FromHwnd(IntPtr.Zero))
		{
			dpiX = graphics.DpiX; // Get the DPI
			dpiY = graphics.DpiY; // Get the DPI

			scaling = dpiX switch
			{
				96 => 100, // Get the %
				120 => 125, // Get the %
				144 => 150, // Get the %
				168 => 175, // Get the %
				192 => 200, // Get the % 
				_ => 100
			};
		}

		double factor = scaling / 100d; // Calculate factor

		MaxHeight = currentScreen.WorkingArea.Height / factor + 5; // Set max size
		MaxWidth = currentScreen.WorkingArea.Width / factor + 5; // Set max size
	}

}
