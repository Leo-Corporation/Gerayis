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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

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
					System.Drawing.Color foreColor = System.Drawing.Color.White; // Foreground
					System.Drawing.Color backColor = System.Drawing.Color.Black; // Background

					if (!string.IsNullOrEmpty(Global.Settings.QRCodeBackgroundColor) && !string.IsNullOrEmpty(Global.Settings.QRCodeForegroundColor))
					{
						string[] fC = Global.Settings.QRCodeForegroundColor.Split(new string[] { ";" }, StringSplitOptions.None); // Split
						string[] bC = Global.Settings.QRCodeBackgroundColor.Split(new string[] { ";" }, StringSplitOptions.None); // Split

						foreColor = System.Drawing.Color.FromArgb((byte)int.Parse(fC[0]), (byte)int.Parse(fC[1]), (byte)int.Parse(fC[2])); // Create new color
						backColor = System.Drawing.Color.FromArgb((byte)int.Parse(bC[0]), (byte)int.Parse(bC[1]), (byte)int.Parse(bC[2])); // Create new color
					}

					QRCodeGenerator qrGenerator = new QRCodeGenerator(); // Create new QRCode generator
					QRCodeData qrCodeData = qrGenerator.CreateQrCode(QRCodeStringTxt.Text, QRCodeGenerator.ECCLevel.Q); // Create QR Code data
					QRCode qrCode = new QRCode(qrCodeData); // Create QR Code
					System.Drawing.Bitmap qrCodeImage = qrCode.GetGraphic(20, foreColor, backColor, true); // Get QR Code bitmap (image)

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
					System.Drawing.Color foreColor = System.Drawing.Color.White; // Foreground
					System.Drawing.Color backColor = System.Drawing.Color.Black; // Background

					if (!string.IsNullOrEmpty(Global.Settings.QRCodeBackgroundColor) && !string.IsNullOrEmpty(Global.Settings.QRCodeForegroundColor))
					{
						string[] fC = Global.Settings.QRCodeForegroundColor.Split(new string[] { ";" }, StringSplitOptions.None); // Split
						string[] bC = Global.Settings.QRCodeBackgroundColor.Split(new string[] { ";" }, StringSplitOptions.None); // Split

						foreColor = System.Drawing.Color.FromArgb((byte)int.Parse(fC[0]), (byte)int.Parse(fC[1]), (byte)int.Parse(fC[2])); // Create new color
						backColor = System.Drawing.Color.FromArgb((byte)int.Parse(bC[0]), (byte)int.Parse(bC[1]), (byte)int.Parse(bC[2])); // Create new color
					}

					QRCodeGenerator qrGenerator = new QRCodeGenerator(); // Create new QRCode generator
					QRCodeData qrCodeData = qrGenerator.CreateQrCode(QRCodeStringTxt.Text, QRCodeGenerator.ECCLevel.Q); // Create QR Code data
					QRCode qrCode = new QRCode(qrCodeData); // Create QR Code
					System.Drawing.Bitmap qrCodeImage = qrCode.GetGraphic(20, foreColor, backColor, true); // Get QR Code bitmap (image)

					IntPtr bmpPt = qrCodeImage.GetHbitmap();
					bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bmpPt, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

					bitmapSource.Freeze();
					QRCodeImg.Source = bitmapSource;

					if (sender is not HistoryItem)
					{
						bool contains = false;
						for (int i = 0; i < QRCodeHistory.Children.Count; i++)
						{
							var historyItem = (HistoryItem)QRCodeHistory.Children[i];
							contains = historyItem.ContentText == QRCodeStringTxt.Text;
						}

						if (!contains && sender is not TextBox)
						{
							QRCodeHistory.Children.Add(new HistoryItem(QRCodeStringTxt.Text, QRCodeHistory, Enums.AppPages.QRCode));
						}
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
				Filter = "PNG|*.png|JPG|*.jpg|JPEG|*.jpeg",
				FileName = $"{QRCodeStringTxt.Text}.png",
				Title = Properties.Resources.Save,
				FilterIndex = (int)Global.Settings.DefaultQRCodeFileExtension.Value + 1
			}; // Create Save file dialog

			if (saveFileDialog.ShowDialog() ?? true)
			{
				Global.SaveImage(saveFileDialog.FileName, bitmapSource, System.IO.Path.GetExtension(saveFileDialog.FileName));
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

		private void QRCodeStringTxt_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (Global.Settings.GenerateQRCodeWhileTyping.Value)
			{
				GenerateBtn_Click(sender, null);
			}
		}

		private void QRCodeImg_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			if (QRCodeImg.Source is not null) // If the image is not empty
			{
				Clipboard.SetImage(bitmapSource); // Copy to clipboard 
			}
		}

		private void CopyGrid_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
		{
			CopyGrid.Visibility = Visibility.Visible; // Show
		}

		private void CopyGrid_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
		{
			CopyGrid.Visibility = Visibility.Collapsed; // Hide
		}
	}
}
