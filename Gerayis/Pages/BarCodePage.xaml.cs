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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Gerayis.Pages
{
	/// <summary>
	/// Logique d'interaction pour BarCodePage.xaml
	/// </summary>
	public partial class BarCodePage : Page
	{
		private System.Drawing.Font BarCodeFont
		{
			get => new System.Drawing.Font(System.Drawing.SystemFonts.DefaultFont.FontFamily, 13.0f);
		}

		public BarCodePage()
		{
			InitializeComponent();
		}

		BitmapSource bitmapSource;
		private void GenerateBtn_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				System.Drawing.Color foreColor = System.Drawing.Color.White; // Foreground
				System.Drawing.Color backColor = System.Drawing.Color.Black; // Background
				
				if (!string.IsNullOrEmpty(Global.Settings.BarCodeBackgroundColor) && !string.IsNullOrEmpty(Global.Settings.BarCodeForegroundColor))
				{
					string[] fC = Global.Settings.BarCodeForegroundColor.Split(new string[] { ";" }, StringSplitOptions.None); // Split
					string[] bC = Global.Settings.BarCodeBackgroundColor.Split(new string[] { ";" }, StringSplitOptions.None); // Split

					foreColor = System.Drawing.Color.FromArgb((byte)int.Parse(fC[0]), (byte)int.Parse(fC[1]), (byte)int.Parse(fC[2])); // Create new color
					backColor = System.Drawing.Color.FromArgb((byte)int.Parse(bC[0]), (byte)int.Parse(bC[1]), (byte)int.Parse(bC[2])); // Create new color
				}

				if (!string.IsNullOrEmpty(BarCodeStringTxt.Text) && !string.IsNullOrWhiteSpace(BarCodeStringTxt.Text))
				{
					BarcodeLib.Barcode barcode = new BarcodeLib.Barcode { IncludeLabel = true, LabelFont = BarCodeFont }; // Create a new barcode generator
					System.Drawing.Image image = barcode.Encode(BarcodeLib.TYPE.CODE128, BarCodeStringTxt.Text, foreColor, backColor, BarCodeStringTxt.Text.Length * 50, 240); // Generate

					var bitmap = new System.Drawing.Bitmap(image);
					IntPtr bmpPt = bitmap.GetHbitmap();
					bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bmpPt, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

					bitmapSource.Freeze();
					BarCodeImg.Source = bitmapSource;
				}
				else
				{
					MessageBox.Show(Properties.Resources.PleaseSpecifyValue, Properties.Resources.Gerayis, MessageBoxButton.OK, MessageBoxImage.Exclamation);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"{Properties.Resources.Error}:\n{Properties.Resources.ErrorCode} {ex.HResult}\n{ex.Message}", $"{Properties.Resources.Error} - {ex.HResult}", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void CopyBtn_Click(object sender, RoutedEventArgs e)
		{
			if (BarCodeImg.Source is not null) // If there is an image
			{
				Clipboard.SetImage(bitmapSource); // Copy to clipboard 
			}
		}
	}
}
