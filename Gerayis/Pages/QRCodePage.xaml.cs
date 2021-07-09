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
using Gerayis.UserControls;
using Microsoft.Win32;
using QRCoder;
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
	/// Logique d'interaction pour QRCodePage.xaml
	/// </summary>
	public partial class QRCodePage : Page
	{
		public QRCodePage()
		{
			InitializeComponent();
			InitUI(); // Load the UI
		}

		private void InitUI()
		{
			if (Global.Settings.GenerateQRCodeOnStart.Value)
			{
				QRCodeStringTxt.Text = Properties.Resources.Gerayis; // Set text

				if (!string.IsNullOrEmpty(QRCodeStringTxt.Text) && !string.IsNullOrWhiteSpace(QRCodeStringTxt.Text))
				{
					QRCodeGenerator qrGenerator = new QRCodeGenerator(); // Create new QRCode generator
					QRCodeData qrCodeData = qrGenerator.CreateQrCode(QRCodeStringTxt.Text, QRCodeGenerator.ECCLevel.Q); // Create QR Code data
					QRCode qrCode = new QRCode(qrCodeData); // Create QR Code
					System.Drawing.Bitmap qrCodeImage = qrCode.GetGraphic(20); // Get QR Code bitmap (image)

					IntPtr bmpPt = qrCodeImage.GetHbitmap();
					bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bmpPt, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

					bitmapSource.Freeze();
					QRCodeImg.Source = bitmapSource;

					QRCodeHistory.Children.Add(new HistoryItem(QRCodeStringTxt.Text, QRCodeHistory, Enums.AppPages.QRCode));
				}
			}
		}

		BitmapSource bitmapSource;
		internal void GenerateBtn_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (!string.IsNullOrEmpty(QRCodeStringTxt.Text) && !string.IsNullOrWhiteSpace(QRCodeStringTxt.Text))
				{
					QRCodeGenerator qrGenerator = new QRCodeGenerator(); // Create new QRCode generator
					QRCodeData qrCodeData = qrGenerator.CreateQrCode(QRCodeStringTxt.Text, QRCodeGenerator.ECCLevel.Q); // Create QR Code data
					QRCode qrCode = new QRCode(qrCodeData); // Create QR Code
					System.Drawing.Bitmap qrCodeImage = qrCode.GetGraphic(20); // Get QR Code bitmap (image)

					IntPtr bmpPt = qrCodeImage.GetHbitmap();
					bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bmpPt, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

					bitmapSource.Freeze();
					QRCodeImg.Source = bitmapSource;

					if (sender is not HistoryItem)
					{
						QRCodeHistory.Children.Add(new HistoryItem(QRCodeStringTxt.Text, QRCodeHistory, Enums.AppPages.QRCode));
					}
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
			if (QRCodeImg.Source is not null) // If the image is not empty
			{
				Clipboard.SetImage(bitmapSource); // Copy to clipboard 
			}
		}

		private void SaveBtn_Click(object sender, RoutedEventArgs e)
		{
			SaveFileDialog saveFileDialog = new()
			{
				Filter = "PNG|*.png",
				FileName = $"{Properties.Resources.QRCode}.png",
				Title = Properties.Resources.Save
			}; // Create Save file dialog

			if (saveFileDialog.ShowDialog() ?? true)
			{
				Global.SaveImage(saveFileDialog.FileName, bitmapSource);
			}
		}

		internal void HistoryBtn_Click(object sender, RoutedEventArgs e)
		{
			if (QRCodeHistory.Children.Count > 0)
			{
				if (sender is not HistoryItem)
				{
					if (Content.Visibility == Visibility.Visible)
					{
						Content.Visibility = Visibility.Collapsed; // Hide
						HistoryScroll.Visibility = Visibility.Visible; // Show

						HistoryBtn.Content = "\uF36A"; // Set text 
					}
					else
					{
						Content.Visibility = Visibility.Visible; // Show
						HistoryScroll.Visibility = Visibility.Collapsed; // Hide

						HistoryBtn.Content = "\uF47F"; // Set text
					}
				}
			}
			else
			{
				Content.Visibility = Visibility.Visible; // Show
				HistoryScroll.Visibility = Visibility.Collapsed; // Hide

				HistoryBtn.Content = "\uF47F"; // Set text

				if (sender is not HistoryItem)
				{
					MessageBox.Show(Properties.Resources.HistoryEmpty, Properties.Resources.Gerayis, MessageBoxButton.OK, MessageBoxImage.Information); // Show
				}
			}
		}
	}
}
