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
using System.Windows;
using System.Windows.Media.Imaging;

namespace Gerayis.Windows
{
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
			};
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
			WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized; // Set
			MaximizeBtn.Content = WindowState == WindowState.Maximized ? "\uF670" : "\uFA40"; // Set text
			MaximizeBtn.FontSize = WindowState == WindowState.Minimized ? 18 : 14;
		}
	}
}
