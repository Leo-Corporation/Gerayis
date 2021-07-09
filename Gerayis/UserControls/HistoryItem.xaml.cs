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
using LeoCorpLibrary;
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

namespace Gerayis.UserControls
{
	/// <summary>
	/// Interaction logic for HistoryItem.xaml
	/// </summary>
	public partial class HistoryItem : UserControl
	{
		string ContentText { get; init; }
		StackPanel StackPanel { get; init; }
		AppPages AppPages { get; init; }
		public HistoryItem(string value, StackPanel stackPanel, AppPages pages)
		{
			InitializeComponent();
			ContentText = value; // Set
			StackPanel = stackPanel; // Set
			AppPages = pages; // Set

			InitUI();
		}

		private void InitUI()
		{
			BarCodeTxt.Text = ContentText;

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
					Global.BarCodePage.GenerateBtn_Click(this, null); // Click
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
			StackPanel.Children.Remove(this); // Remove

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
}
