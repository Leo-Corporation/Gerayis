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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Gerayis.UserControls;

/// <summary>
/// Interaction logic for HistoryItem.xaml
/// </summary>
public partial class HistoryItem : UserControl
{
	internal string ContentText { get; init; }
	WrapPanel WrapPanel { get; init; }
	AppPages AppPages { get; init; }
	internal Barcodes BarcodeType { get; init; }
	private BitmapSource BitmapSource { get; init; }

	public HistoryItem(string value, BitmapSource bitmapSource, WrapPanel wrapPanel, AppPages pages, Barcodes barcodeType = Barcodes.Code128)
	{
		InitializeComponent();
		ContentText = value; // Set
		WrapPanel = wrapPanel; // Set
		AppPages = pages; // Set
		BarcodeType = barcodeType; // Set
		BitmapSource = bitmapSource;

		InitUI();
	}

	private void InitUI()
	{
		HistoryImage.Source = BitmapSource;
		BarCodeTxt.Text = ContentText;
		if (AppPages == AppPages.BarCode) // If the item is a barcode
		{
			BarCodeTypeTxt.Text = BarcodeType switch
			{
				Barcodes.Code11 => Properties.Resources.Code11,
				Barcodes.Code128 => Properties.Resources.Code128,
				Barcodes.ISBN => Properties.Resources.ISBN,
				Barcodes.MSI => Properties.Resources.MSI,
				Barcodes.UPCA => Properties.Resources.UPCA,
				_ => Properties.Resources.Code128
			}; // Set text 
		}
		else
		{
			BarCodeTypeTxt.Visibility = Visibility.Collapsed; // Hide if History Item is from QR code page
		}

		GenerateBtn.Content = AppPages switch
		{
			AppPages.BarCode => "\uF210",
			AppPages.QRCode => "\uF636",
			_ => "\uF210",
		};
	}

	private void GenerateBtn_Click(object sender, RoutedEventArgs e)
	{
		switch (AppPages)
		{
			case AppPages.BarCode:
				Global.BarCodePage.BarCodeStringTxt.Text = ContentText;
				Global.BarCodePage.BarCodeTypeComboBox.SelectedIndex = (int)BarcodeType; // Set selected index
				Global.BarCodePage.GenerateBarCode(ContentText, this, BarcodeType); // Click
				Global.BarCodePage.HistoryScroll.Visibility = Visibility.Collapsed; // Hide
				Global.BarCodePage.Content.Visibility = Visibility.Visible; // Show
				Global.BarCodePage.HistoryBtn.Content = "\uF47F"; // Set text
				break;
			case AppPages.QRCode:
				Global.QRCodePage.QRCodeStringTxt.Text = ContentText;
				Global.QRCodePage.GenerateBtn_Click(this, null); // Click
				Global.QRCodePage.HistoryScroll.Visibility = Visibility.Collapsed; // Hide
				Global.QRCodePage.Content.Visibility = Visibility.Visible; // Show
				Global.QRCodePage.HistoryBtn.Content = "\uF47F"; // Set text
				break;
		}
	}

	private void DeleteBtn_Click(object sender, RoutedEventArgs e)
	{
		WrapPanel.Children.Remove(this); // Remove

		switch (AppPages)
		{
			case AppPages.BarCode:
				Global.BarCodePage.HistoryBtn_Click(this, null);
				break;
			case AppPages.QRCode:
				Global.QRCodePage.HistoryBtn_Click(this, null);
				break;
		}
	}
}
