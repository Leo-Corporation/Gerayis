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
using Gerayis.Windows;
using Microsoft.Win32;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Gerayis.Pages;

/// <summary>
/// Logique d'interaction pour BarCodePage.xaml
/// </summary>
public partial class BarCodePage : Page
{
	private static System.Drawing.Font BarCodeFont
	{
		get => new(System.Drawing.SystemFonts.DefaultFont.FontFamily, 13.0f);
	}

	internal string Error { get; set; }

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
			border.Background = new SolidColorBrush { Color = (Color)ColorConverter.ConvertFromString(App.Current.Resources["LightAccentColor"].ToString()) }; // Set the background
			BorderIconTxt.Foreground = new SolidColorBrush { Color = (Color)ColorConverter.ConvertFromString(App.Current.Resources["AccentColor"].ToString()) }; // Set the foreground
			BorderMsgTxt.Foreground = new SolidColorBrush { Color = (Color)ColorConverter.ConvertFromString(App.Current.Resources["AccentColor"].ToString()) }; // Set the foreground

			BorderIconTxt.Text = "\uF299"; // Set icon
			BorderMsgTxt.Text = Properties.Resources.SuccessBarCodeGenerated; // Set text

			System.Drawing.Color foreColor = System.Drawing.Color.White; // Foreground
			System.Drawing.Color backColor = System.Drawing.Color.Black; // Background

			ShowErrorBtn.Visibility = Visibility.Collapsed; // Hide

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
				BarcodeLib.Barcode barcode = new() { IncludeLabel = true, LabelFont = BarCodeFont }; // Create a new barcode generator
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
						BarCodeHistory.Children.Add(new HistoryItem(text, bitmapSource, BarCodeHistory, AppPages.BarCode, barcodeType));
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
			border.Background = new SolidColorBrush { Color = (Color)ColorConverter.ConvertFromString(App.Current.Resources["Red"].ToString()) }; // Set the background
			BorderIconTxt.Foreground = new SolidColorBrush { Color = (Color)ColorConverter.ConvertFromString(App.Current.Resources["DarkRed"].ToString()) }; // Set the foreground
			BorderMsgTxt.Foreground = new SolidColorBrush { Color = (Color)ColorConverter.ConvertFromString(App.Current.Resources["DarkRed"].ToString()) }; // Set the foreground

			BorderIconTxt.Text = "\uF36E"; // Set icon
			BorderMsgTxt.Text = Properties.Resources.NoUseSpecialChars; // Set text

			Error = ex.Message; // Set error message
			ShowErrorBtn.Visibility = Visibility.Visible; // Show
		}
	}

	internal void CopyBtn_Click(object sender, RoutedEventArgs e)
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
			Filter = "PNG|*.png|JPG|*.jpg|JPEG|*.jpeg",
			FileName = $"{BarCodeStringTxt.Text}",
			Title = Properties.Resources.Save,
			FilterIndex = (int)Global.Settings.DefaultBarCodeFileExtension.Value + 1
		}; // Create Save file dialog

		if (saveFileDialog.ShowDialog() ?? true)
		{
			Global.SaveImage(saveFileDialog.FileName, bitmapSource, System.IO.Path.GetExtension(saveFileDialog.FileName));
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

	private void BarCodeTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (BarCodeStringTxt.Text == Properties.Resources.Gerayis ||
			BarCodeStringTxt.Text == "456146121546" ||
			BarCodeStringTxt.Text == "978146121546" ||
			BarCodeStringTxt.Text == "163657455245" ||
			BarCodeStringTxt.Text == "12659456240")
		{
			BarCodeStringTxt.Text = (Barcodes)BarCodeTypeComboBox.SelectedIndex switch
			{
				Barcodes.Code128 => Properties.Resources.Gerayis, // Text
				Barcodes.Code11 => "456146121546", // Code11
				Barcodes.ISBN => "978146121546", // ISBN starts with 978
				Barcodes.MSI => "163657455245", // MSI
				Barcodes.UPCA => "12659456240", // UPC-A
				_ => Properties.Resources.Gerayis // Default value
			};
		}

		UpdateValidIcon(); // Update valid icon
	}

	private void ShowErrorBtn_Click(object sender, RoutedEventArgs e)
	{
		MessageBox.Show(Error, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
	}

	private void CopyGrid_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
	{
		CopyGrid.Visibility = Visibility.Visible; // Show
	}

	private void CopyGrid_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
	{
		CopyGrid.Visibility = Visibility.Collapsed; // Hide
	}

	private void BarCodeImg_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
	{
		if (BarCodeImg.Source is not null) // If the image is not empty
		{
			Clipboard.SetImage(bitmapSource); // Copy to clipboard 
		}
	}

	private void SeeFullBarCodeBtn_Click(object sender, RoutedEventArgs e)
	{
		if (BarCodeImg.Source is not null)
		{
			new SeeFullBarCodeWindow(bitmapSource, AppPages.BarCode).Show(); // Show bar code
		}
	}

	bool infoPanelToggled = false;
	private void BarCodeInfoBtn_Click(object sender, RoutedEventArgs e)
	{
		if (infoPanelToggled)
		{
			InfoPanel.Visibility = Visibility.Collapsed; // Hide
			Content.Visibility = Visibility.Visible; // Show
			infoPanelToggled = false; // Set to false

			BarCodeInfoBtn.Content = "\uF4A4"; // Set text
		}
		else
		{
			InfoPanel.Visibility = Visibility.Visible; // Show
			Content.Visibility = Visibility.Collapsed; // Hide
			infoPanelToggled = true; // Set to true

			BarCodeInfoBtn.Content = "\uF36B"; // Set text
			LoadInfoPanel((Barcodes)BarCodeTypeComboBox.SelectedIndex); // Load info panel
		}
	}

	private void LoadInfoPanel(Barcodes barcode)
	{
		switch (barcode)
		{
			case Barcodes.Code11:
				// Update icons
				NumbersIconTxt.Text = "\uF295"; // Set text
				CharsIconTxt.Text = "\uF36A"; // Set text
				MinLengthIconTxt.Text = "\uF36A"; // Set text

				// Update colors
				NumbersIconTxt.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(App.Current.Resources["Green"].ToString())); // Set color
				CharsIconTxt.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(App.Current.Resources["Red2"].ToString())); // Set color
				MinLengthIconTxt.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(App.Current.Resources["Red2"].ToString())); // Set color
				break;
			case Barcodes.Code128:
				// Update icons
				NumbersIconTxt.Text = "\uF295"; // Set text
				CharsIconTxt.Text = "\uF295"; // Set text
				MinLengthIconTxt.Text = "\uF36A"; // Set text

				// Update colors
				NumbersIconTxt.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(App.Current.Resources["Green"].ToString())); // Set color
				CharsIconTxt.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(App.Current.Resources["Green"].ToString())); // Set color
				MinLengthIconTxt.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(App.Current.Resources["Red2"].ToString())); // Set color
				break;
			case Barcodes.ISBN:
				// Update icons
				NumbersIconTxt.Text = "\uF295"; // Set text
				CharsIconTxt.Text = "\uF36A"; // Set text
				MinLengthIconTxt.Text = "\uF295"; // Set text

				// Update text
				MinLengthTxt.Text = $"{Properties.Resources.MinLength} - 9/10/12/13"; // Set text

				// Update colors
				NumbersIconTxt.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(App.Current.Resources["Green"].ToString())); // Set color
				CharsIconTxt.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(App.Current.Resources["Red2"].ToString())); // Set color
				MinLengthIconTxt.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(App.Current.Resources["Green"].ToString())); // Set color
				break;
			case Barcodes.MSI:
				// Update icons
				NumbersIconTxt.Text = "\uF295"; // Set text
				CharsIconTxt.Text = "\uF36A"; // Set text
				MinLengthIconTxt.Text = "\uF36A"; // Set text

				// Update colors
				NumbersIconTxt.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(App.Current.Resources["Green"].ToString())); // Set color
				CharsIconTxt.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(App.Current.Resources["Red2"].ToString())); // Set color
				MinLengthIconTxt.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(App.Current.Resources["Red2"].ToString())); // Set color
				break;
			case Barcodes.UPCA:
				// Update icons
				NumbersIconTxt.Text = "\uF295"; // Set text
				CharsIconTxt.Text = "\uF36A"; // Set text
				MinLengthIconTxt.Text = "\uF295"; // Set text

				// Update text
				MinLengthTxt.Text = $"{Properties.Resources.MinLength} - 11/12"; // Set text

				// Update colors
				NumbersIconTxt.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(App.Current.Resources["Green"].ToString())); // Set color
				CharsIconTxt.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(App.Current.Resources["Red2"].ToString())); // Set color
				MinLengthIconTxt.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(App.Current.Resources["Green"].ToString())); // Set color
				break;
		}

		// Update Example text
		ExampleTxt.Text = barcode switch
		{
			Barcodes.Code128 => $"{Properties.Resources.Example} - {Properties.Resources.Gerayis}", // Text
			Barcodes.Code11 => $"{Properties.Resources.Example} - 456146121546", // Code11
			Barcodes.ISBN => $"{Properties.Resources.Example} - 978146121546", // ISBN starts with 978
			Barcodes.MSI => $"{Properties.Resources.Example} - 163657455245", // MSI
			Barcodes.UPCA => $"{Properties.Resources.Example} - 12659456240", // UPC-A
			_ => Properties.Resources.Gerayis // Default value
		}; // Set text depending on the bar code type
	}

	private void BarCodeStringTxt_TextChanged(object sender, TextChangedEventArgs e)
	{
		UpdateValidIcon();
	}

	private void UpdateValidIcon()
	{
		Regex regex = new((Barcodes)BarCodeTypeComboBox.SelectedIndex switch
		{
			Barcodes.Code11 => "^[0-9-]+$", // Code11 ('-' character is allowed)
			Barcodes.Code128 => "^.[^éèàçùµ¤]+$", // Code128
			Barcodes.ISBN => "^(?:[0-9]{9}|[0-9]{10}|[0-9]{12}|[0-9]{13})$", // ISBN
			Barcodes.MSI => "^[0-9]+$", // MSI
			Barcodes.UPCA => "^[0-9]{11,12}$", // UPC-A
			_ => "^.[^éèàçùµ¤]+$" // Default value
		});

		// Check if the string is valid
		ValidIconTxt.Text = regex.IsMatch(BarCodeStringTxt.Text) ? "\uF295" : "\uF36A"; // Set icon depending on the result
		ValidIconTxt.Foreground = regex.IsMatch(BarCodeStringTxt.Text) ? new SolidColorBrush((Color)ColorConverter.ConvertFromString(App.Current.Resources["Green"].ToString())) : new SolidColorBrush((Color)ColorConverter.ConvertFromString(App.Current.Resources["Red2"].ToString())); // Set color depending on the result

	}
}
