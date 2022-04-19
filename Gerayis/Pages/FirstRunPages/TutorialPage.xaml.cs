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

using QRCoder;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Gerayis.Pages.FirstRunPages;
/// <summary>
/// Interaction logic for TutorialPage.xaml
/// </summary>
public partial class TutorialPage : Page
{
	private static System.Drawing.Font BarCodeFont
	{
		get => new(System.Drawing.SystemFonts.DefaultFont.FontFamily, 13.0f);
	}

	public TutorialPage()
	{
		InitializeComponent();
	}

	BitmapSource bitmapSource;
	BitmapSource bitmapSource2;
	private void BarCodeStringTxt_TextChanged(object sender, TextChangedEventArgs e)
	{
		try
		{
			if (BarCodeStringTxt.Text.Length > 0)
			{
				// Generate bar code
				BarcodeLib.Barcode barcode = new() { IncludeLabel = true, LabelFont = BarCodeFont }; // Create a new barcode generator
				System.Drawing.Image image = barcode.Encode(BarcodeLib.TYPE.CODE128, BarCodeStringTxt.Text, System.Drawing.Color.Black, System.Drawing.Color.White, BarCodeStringTxt.Text.Length * 50, 240); // Generate

				// Create and set image
				var bitmap = new System.Drawing.Bitmap(image);
				IntPtr bmpPt = bitmap.GetHbitmap();
				bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bmpPt, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

				bitmapSource.Freeze();
				BarCodeImg.Source = bitmapSource;

				QRCodeGenerator qrGenerator = new(); // Create new QRCode generator
				QRCodeData qrCodeData = qrGenerator.CreateQrCode(BarCodeStringTxt.Text, QRCodeGenerator.ECCLevel.Q); // Create QR Code data
				QRCode qrCode = new(qrCodeData); // Create QR Code
				System.Drawing.Bitmap qrCodeImage = qrCode.GetGraphic(20, System.Drawing.Color.Black, System.Drawing.Color.White, true); // Get QR Code bitmap (image)

				IntPtr bmpPt2 = qrCodeImage.GetHbitmap();
				bitmapSource2 = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bmpPt2, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

				bitmapSource2.Freeze();
				QRCodeImg.Source = bitmapSource2;
			}
		}
		catch { }
	}
}
