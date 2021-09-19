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
using Gerayis.Enums;
using Gerayis.UserControls;
using Microsoft.Win32;
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
			InitUI(); // Load UI
		}

		private void InitUI()
		{
			BarCodeTypeComboBox.SelectedIndex = (int)Global.Settings.DefaultBarCodeType.Value; // Select the first item

			if (Global.Settings.GenerateBarCodeOnStart.Value)
			{
				BarCodeStringTxt.Text = Global.Settings.DefaultBarCodeType switch
				{
					Barcodes.Code128 => Properties.Resources.Gerayis, // Text
					Barcodes.Code11 => "456146121546", // Code11
					Barcodes.ISBN => "978146121546", // ISBN starts with 978
					Barcodes.MSI => "163657455245", // MSI
					Barcodes.UPCA => "12659456240", // UPC-A
					_ => Properties.Resources.Gerayis // Default value
				}; // Set text depending on the bar code type

				GenerateBarCode(BarCodeStringTxt.Text, null, Global.Settings.DefaultBarCodeType.Value); // Generate bar code
			}
		}

		BitmapSource bitmapSource;
		internal void GenerateBtn_Click(object sender, RoutedEventArgs e)
		{
			GenerateBarCode(BarCodeStringTxt.Text, sender, (Barcodes)BarCodeTypeComboBox.SelectedIndex); // Generate
		}

		internal void GenerateBarCode(string text, object sender, Barcodes barcodeType)
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

				if (!string.IsNullOrEmpty(text) && !string.IsNullOrWhiteSpace(text))
				{
					// Find the barcode type
					BarcodeLib.TYPE barType = barcodeType switch
					{
						Barcodes.Code128 => BarcodeLib.TYPE.CODE128, // Code128
						Barcodes.Code11 => BarcodeLib.TYPE.CODE11, // Code11
						Barcodes.UPCA => BarcodeLib.TYPE.UPCA, // UPC-A
						Barcodes.MSI => BarcodeLib.TYPE.MSI_Mod10, // MSI
						Barcodes.ISBN => BarcodeLib.TYPE.ISBN, // ISBN
						_ => BarcodeLib.TYPE.CODE128 // Default value
					}; // Get

					// Generate bar code
					BarcodeLib.Barcode barcode = new BarcodeLib.Barcode { IncludeLabel = true, LabelFont = BarCodeFont }; // Create a new barcode generator
					System.Drawing.Image image = barcode.Encode(barType, text, foreColor, backColor, BarCodeStringTxt.Text.Length * 50, 240); // Generate

					// Create and set image
					var bitmap = new System.Drawing.Bitmap(image);
					IntPtr bmpPt = bitmap.GetHbitmap();
					bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bmpPt, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

					bitmapSource.Freeze();
					BarCodeImg.Source = bitmapSource;

					if (sender is not HistoryItem)
					{
						bool contains = false;

						for (int i = 0; i < BarCodeHistory.Children.Count; i++)
						{
							var historyItem = (HistoryItem)BarCodeHistory.Children[i];
							contains = historyItem.ContentText == text && historyItem.BarcodeType == barcodeType;
						}

						if (!contains)
						{
							BarCodeHistory.Children.Add(new HistoryItem(text, BarCodeHistory, AppPages.BarCode, barcodeType));
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
			if (BarCodeImg.Source is not null) // If there is an image
			{
				Clipboard.SetImage(bitmapSource); // Copy to clipboard 
			}
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
				Global.SaveImage(saveFileDialog.FileName, bitmapSource);
			}
		}

		internal void HistoryBtn_Click(object sender, RoutedEventArgs e)
		{
			if (BarCodeHistory.Children.Count > 0)
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
