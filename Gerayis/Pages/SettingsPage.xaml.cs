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
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Gerayis.Pages;

/// <summary>
/// Logique d'interaction pour SettingsPage.xaml
/// </summary>
public partial class SettingsPage : Page
{
	bool isAvailable;
	readonly System.Windows.Forms.NotifyIcon notifyIcon = new();
	public SettingsPage()
	{
		InitializeComponent();
		notifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(AppDomain.CurrentDomain.BaseDirectory + @"\Gerayis.exe");
		notifyIcon.BalloonTipClicked += async (o, e) =>
		{
			string lastVersion = await Update.GetLastVersionAsync(Global.LastVersionLink); // Get last version
			if (MessageBox.Show(Properties.Resources.InstallConfirmMsg, $"{Properties.Resources.InstallVersion} {lastVersion}", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
			{
				Env.ExecuteAsAdmin(Directory.GetCurrentDirectory() + @"\Xalyus Updater.exe"); // Start the updater
				Environment.Exit(0); // Close
			}
		};
		InitUI(); // Load the UI
	}

	private async void InitUI()
	{
		try
		{
			if (string.IsNullOrEmpty(Global.Settings.BarCodeBackgroundColor))
			{
				Global.Settings.BarCodeBackgroundColor = "255;255;255"; // Set
			}

			if (string.IsNullOrEmpty(Global.Settings.BarCodeForegroundColor))
			{
				Global.Settings.BarCodeForegroundColor = "0;0;0"; // Set
			}

			if (string.IsNullOrEmpty(Global.Settings.QRCodeBackgroundColor))
			{
				Global.Settings.QRCodeBackgroundColor = "255;255;255"; // Set
			}

			if (string.IsNullOrEmpty(Global.Settings.QRCodeForegroundColor))
			{
				Global.Settings.QRCodeForegroundColor = "0;0;0"; // Set
			}

			if (!Global.Settings.DefaultBarCodeType.HasValue)
			{
				Global.Settings.DefaultBarCodeType = Barcodes.Code128; // Set default value
			}

			if (!Global.Settings.DefaultBarCodeFileExtension.HasValue)
			{
				Global.Settings.DefaultBarCodeFileExtension = SupportedFileExtensions.PNG; // Set default value
			}

			if (!Global.Settings.DefaultQRCodeFileExtension.HasValue)
			{
				Global.Settings.DefaultQRCodeFileExtension = SupportedFileExtensions.PNG; // Set default value
			}

			if (!Global.Settings.IsFirstRun.HasValue)
			{
				Global.Settings.IsFirstRun = true; // Set default value
			}

			if (!Global.Settings.StartupPage.HasValue)
			{
				Global.Settings.StartupPage = AppPages.BarCode; // Set default value
			}

			// Load RadioButtons
			DarkRadioBtn.IsChecked = Global.Settings.IsDarkTheme; // Change IsChecked property
			LightRadioBtn.IsChecked = !Global.Settings.IsDarkTheme; // Change IsChecked property
			SystemRadioBtn.IsChecked = Global.Settings.IsThemeSystem; // Change IsChecked property
			BarCodePageRadioBtn.IsChecked = (int)Global.Settings.StartupPage == 0;
			QrCodePageRadioBtn.IsChecked = (int)Global.Settings.StartupPage == 1;

			// Borders
			if (DarkRadioBtn.IsChecked.Value)
			{
				CheckedBorder = DarkBorder; // Set
			}
			else if (LightRadioBtn.IsChecked.Value)
			{
				CheckedBorder = LightBorder; // Set
			}
			else if (SystemRadioBtn.IsChecked.Value)
			{
				CheckedBorder = SystemBorder; // Set
			}

			if (BarCodePageRadioBtn.IsChecked.Value)
			{
				PageCheckedBorder = BarCodePageBorder; // Set
			}
			else if (QrCodePageRadioBtn.IsChecked.Value)
			{
				PageCheckedBorder = QrCodePageBorder; // Set
			}
			
			RefreshBorders();
			RefreshStartupBorders();

			// Load checkboxes
			CheckUpdatesOnStartChk.IsChecked = Global.Settings.CheckUpdatesOnStart ?? true; // Set
			NotifyUpdatesChk.IsChecked = Global.Settings.NotifyUpdates ?? true; // Set
			GenerateBarCodeOnStartChk.IsChecked = Global.Settings.GenerateBarCodeOnStart ?? true; // Set
			GenerateQRCodeOnStartChk.IsChecked = Global.Settings.GenerateQRCodeOnStart ?? true; // Set
			GenerateQRCodeTypingChk.IsChecked = Global.Settings.GenerateQRCodeWhileTyping ?? true; // Set

			Global.Settings.GenerateQRCodeWhileTyping = Global.Settings.GenerateQRCodeWhileTyping ?? true; // Set

			// Load LangComboBox
			LangComboBox.Items.Clear(); // Clear
			LangComboBox.Items.Add(Properties.Resources.Default); // Add "default"

			for (int i = 0; i < Global.LanguageList.Count; i++)
			{
				LangComboBox.Items.Add(Global.LanguageList[i]);
			}

			LangComboBox.SelectedIndex = (Global.Settings.Language == "_default") ? 0 : Global.LanguageCodeList.IndexOf(Global.Settings.Language) + 1;

			LangApplyBtn.Visibility = Visibility.Hidden; // Hide
			ThemeApplyBtn.Visibility = Visibility.Hidden; // Hide

			BarCodeTypeComboBox.SelectedIndex = (int)Global.Settings.DefaultBarCodeType.Value; // Select the first item

			BarCodesSaveFormatComboBox.SelectedIndex = (int)Global.Settings.DefaultBarCodeFileExtension.Value; // Select
			QRCodeSaveFormatComboBox.SelectedIndex = (int)Global.Settings.DefaultQRCodeFileExtension.Value; // Select

			// Update the UpdateStatusTxt
			if (Global.Settings.CheckUpdatesOnStart.Value)
			{
				if (await NetworkConnection.IsAvailableAsync())
				{
					isAvailable = Update.IsAvailable(Global.Version, await Update.GetLastVersionAsync(Global.LastVersionLink));

					UpdateStatusTxt.Text = isAvailable ? Properties.Resources.AvailableUpdates : Properties.Resources.UpToDate; // Set the text
					InstallIconTxt.Text = isAvailable ? "\uF152" : "\uF191"; // Set text 
					InstallMsgTxt.Text = isAvailable ? Properties.Resources.Install : Properties.Resources.CheckUpdate; // Set text 

					if (isAvailable && Global.Settings.NotifyUpdates.Value)
					{
						notifyIcon.Visible = true; // Show
						notifyIcon.ShowBalloonTip(5000, Properties.Resources.Gerayis, Properties.Resources.AvailableUpdates, System.Windows.Forms.ToolTipIcon.Info);
						notifyIcon.Visible = false; // Hide
					}
				}
				else
				{
					UpdateStatusTxt.Text = Properties.Resources.UnableToCheckUpdates; // Set the text
					InstallIconTxt.Text = "\uF191"; // Set text 
					InstallMsgTxt.Text = Properties.Resources.CheckUpdate; // Set text
				}
			}
			else
			{
				UpdateStatusTxt.Text = Properties.Resources.CheckUpdatesDisabledOnStart; // Set text
				InstallMsgTxt.Text = Properties.Resources.CheckUpdate; // Set text
				InstallIconTxt.Text = "\uF191"; // Set text 
			}

			// Load Bar code colors
			if (!string.IsNullOrEmpty(Global.Settings.BarCodeForegroundColor))
			{
				string[] foreColor = Global.Settings.BarCodeForegroundColor.Split(new string[] { ";" }, StringSplitOptions.None); // Split
				ForeColorRec.Fill = new SolidColorBrush { Color = (foreColor.Length == 3) ? Color.FromRgb((byte)int.Parse(foreColor[0]), (byte)int.Parse(foreColor[1]), (byte)int.Parse(foreColor[2])) : Color.FromRgb(0, 0, 0) }; // Set color 
			}
			else
			{
				ForeColorRec.Fill = new SolidColorBrush { Color = Color.FromRgb(0, 0, 0) }; // Set color
			}

			if (!string.IsNullOrEmpty(Global.Settings.BarCodeBackgroundColor))
			{
				string[] backColor = Global.Settings.BarCodeBackgroundColor.Split(new string[] { ";" }, StringSplitOptions.None); // Split
				BackColorRec.Fill = new SolidColorBrush { Color = (backColor.Length == 3) ? Color.FromRgb((byte)int.Parse(backColor[0]), (byte)int.Parse(backColor[1]), (byte)int.Parse(backColor[2])) : Color.FromRgb(255, 255, 255) }; // Set color 
			}
			else
			{
				BackColorRec.Fill = new SolidColorBrush { Color = Color.FromRgb(255, 255, 255) }; // Set color
			}

			// Load QR Code colors
			// Load Bar code colors
			if (!string.IsNullOrEmpty(Global.Settings.QRCodeForegroundColor))
			{
				string[] foreColor = Global.Settings.QRCodeForegroundColor.Split(new string[] { ";" }, StringSplitOptions.None); // Split
				QRForeColorRec.Fill = new SolidColorBrush { Color = (foreColor.Length == 3) ? Color.FromRgb((byte)int.Parse(foreColor[0]), (byte)int.Parse(foreColor[1]), (byte)int.Parse(foreColor[2])) : Color.FromRgb(0, 0, 0) }; // Set color 
			}
			else
			{
				QRForeColorRec.Fill = new SolidColorBrush { Color = Color.FromRgb(0, 0, 0) }; // Set color
			}

			if (!string.IsNullOrEmpty(Global.Settings.QRCodeBackgroundColor))
			{
				string[] backColor = Global.Settings.QRCodeBackgroundColor.Split(new string[] { ";" }, StringSplitOptions.None); // Split
				QRBackColorRec.Fill = new SolidColorBrush { Color = (backColor.Length == 3) ? Color.FromRgb((byte)int.Parse(backColor[0]), (byte)int.Parse(backColor[1]), (byte)int.Parse(backColor[2])) : Color.FromRgb(255, 255, 255) }; // Set color 
			}
			else
			{
				QRBackColorRec.Fill = new SolidColorBrush { Color = Color.FromRgb(255, 255, 255) }; // Set color
			}

			VersionTxt.Text = Global.Version; // Set text

			SettingsManager.Save(); // Save changes
		}
		catch (Exception ex)
		{
			MessageBox.Show(ex.Message, ex.StackTrace, MessageBoxButton.OK, MessageBoxImage.Error); // Show error
		}
	}

	private async void RefreshInstallBtn_Click(object sender, RoutedEventArgs e)
	{
		if (Global.Settings.CheckUpdatesOnStart.Value)
		{
			if (await NetworkConnection.IsAvailableAsync()) // If there is Internet
			{
				if (isAvailable) // If there is updates
				{
					string lastVersion = await Update.GetLastVersionAsync(Global.LastVersionLink); // Get last version
					if (MessageBox.Show(Properties.Resources.InstallConfirmMsg, $"{Properties.Resources.InstallVersion} {lastVersion}", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
					{
						Env.ExecuteAsAdmin(Directory.GetCurrentDirectory() + @"\Xalyus Updater.exe"); // Start the updater
						Environment.Exit(0); // Close
					}
				}
				else
				{
					// Update the UpdateStatusTxt
					isAvailable = Update.IsAvailable(Global.Version, await Update.GetLastVersionAsync(Global.LastVersionLink));

					UpdateStatusTxt.Text = isAvailable ? Properties.Resources.AvailableUpdates : Properties.Resources.UpToDate; // Set the text
					InstallIconTxt.Text = isAvailable ? "\uF152" : "\uF191"; // Set text 
					InstallMsgTxt.Text = isAvailable ? Properties.Resources.Install : Properties.Resources.CheckUpdate; // Set text

					if (isAvailable && Global.Settings.NotifyUpdates.Value)
					{
						notifyIcon.Visible = true; // Show
						notifyIcon.ShowBalloonTip(5000, Properties.Resources.Gerayis, Properties.Resources.AvailableUpdates, System.Windows.Forms.ToolTipIcon.Info);
						notifyIcon.Visible = false; // Hide
					}
				}
			}
			else
			{
				UpdateStatusTxt.Text = Properties.Resources.UnableToCheckUpdates; // Set the text
				InstallIconTxt.Text = "\uF191"; // Set text 
				InstallMsgTxt.Text = Properties.Resources.CheckUpdate; // Set text
			}
		}
		else
		{
			UpdateStatusTxt.Text = Properties.Resources.CheckUpdatesDisabledOnStart; // Set text
			InstallMsgTxt.Text = Properties.Resources.CheckUpdate; // Set text
			InstallIconTxt.Text = "\uF191"; // Set text 
		}
	}

	private void LangComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		LangApplyBtn.Visibility = Visibility.Visible; // Show the LangApplyBtn button
	}

	private void ThemeApplyBtn_Click(object sender, RoutedEventArgs e)
	{
		Global.Settings.IsDarkTheme = DarkRadioBtn.IsChecked.Value; // Set the settings
		Global.Settings.IsThemeSystem = SystemRadioBtn.IsChecked; // Set the settings

		SettingsManager.Save(); // Save the changes
		ThemeApplyBtn.Visibility = Visibility.Hidden; // Hide
		DisplayRestartMessage();
	}

	private void LangApplyBtn_Click(object sender, RoutedEventArgs e)
	{
		Global.Settings.Language = LangComboBox.Text switch
		{
			"English (United States)" => Global.LanguageCodeList[0], // Set the settings value
			"Français (France)" => Global.LanguageCodeList[1], // Set the settings value
			"中文（简体）" => Global.LanguageCodeList[2], // Set the settings value
			_ => "_default" // Set the settings value
		};
		SettingsManager.Save(); // Save the changes
		LangApplyBtn.Visibility = Visibility.Hidden; // Hide
		DisplayRestartMessage();
	}

	/// <summary>
	/// Restarts Gerayis.
	/// </summary>
	private void DisplayRestartMessage()
	{
		if (MessageBox.Show(Properties.Resources.NeedRestartToApplyChanges, Properties.Resources.Gerayis, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
		{
			Process.Start(Directory.GetCurrentDirectory() + @"\Gerayis.exe"); // Start
			Environment.Exit(0); // Close
		}
	}

	private void LightRadioBtn_Checked(object sender, RoutedEventArgs e)
	{
		ThemeApplyBtn.Visibility = Visibility.Visible; // Show the ThemeApplyBtn button
	}

	private void DarkRadioBtn_Checked(object sender, RoutedEventArgs e)
	{
		ThemeApplyBtn.Visibility = Visibility.Visible; // Show the ThemeApplyBtn button
	}

	private void CheckUpdatesOnStartChk_Checked(object sender, RoutedEventArgs e)
	{
		Global.Settings.CheckUpdatesOnStart = CheckUpdatesOnStartChk.IsChecked; // Set
		SettingsManager.Save(); // Save changes
	}

	private void NotifyUpdatesChk_Checked(object sender, RoutedEventArgs e)
	{
		Global.Settings.NotifyUpdates = NotifyUpdatesChk.IsChecked; // Set
		SettingsManager.Save(); // Save changes
	}

	private void ForeColorRec_MouseDown(object sender, MouseButtonEventArgs e)
	{
		System.Windows.Forms.ColorDialog colorDialog = new()
		{
			AllowFullOpen = true,
		}; // Create color picker/dialog

		if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) // If the user selected a color
		{
			Global.Settings.BarCodeForegroundColor = $"{colorDialog.Color.R};{colorDialog.Color.G};{colorDialog.Color.B}";
			SettingsManager.Save(); // Save changes

			ForeColorRec.Fill = new SolidColorBrush { Color = Color.FromRgb(colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B) }; // Set color
		}
	}

	private void BackColorRec_MouseDown(object sender, MouseButtonEventArgs e)
	{
		System.Windows.Forms.ColorDialog colorDialog = new()
		{
			AllowFullOpen = true,
		}; // Create color picker/dialog

		if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) // If the user selected a color
		{
			Global.Settings.BarCodeBackgroundColor = $"{colorDialog.Color.R};{colorDialog.Color.G};{colorDialog.Color.B}";
			SettingsManager.Save(); // Save changes

			BackColorRec.Fill = new SolidColorBrush { Color = Color.FromRgb(colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B) }; // Set color
		}
	}

	private void GenerateBarCodeOnStartChk_Checked(object sender, RoutedEventArgs e)
	{
		Global.Settings.GenerateBarCodeOnStart = GenerateBarCodeOnStartChk.IsChecked; // Set
		SettingsManager.Save(); // Save changes
	}

	private void GenerateQRCodeOnStartChk_Checked(object sender, RoutedEventArgs e)
	{
		Global.Settings.GenerateQRCodeOnStart = GenerateQRCodeOnStartChk.IsChecked; // Set
		SettingsManager.Save(); // Save changes
	}

	private void ImportBtn_Click(object sender, RoutedEventArgs e)
	{
		OpenFileDialog openFileDialog = new()
		{
			Filter = "XML|*.xml",
			Title = Properties.Resources.Export
		}; // Create file dialog

		if (openFileDialog.ShowDialog() ?? true)
		{
			SettingsManager.Import(openFileDialog.FileName); // Import games
		}
	}

	private void ExportBtn_Click(object sender, RoutedEventArgs e)
	{
		SaveFileDialog saveFileDialog = new()
		{
			FileName = "GerayisSettings.xml",
			Filter = "XML|*.xml",
			Title = Properties.Resources.Export
		}; // Create file dialog

		if (saveFileDialog.ShowDialog() ?? true)
		{
			SettingsManager.Export(saveFileDialog.FileName); // Export games
		}
	}

	private void BtnEnter(object sender, MouseEventArgs e)
	{
		Button button = (Button)sender; // Create button
		button.Foreground = new SolidColorBrush { Color = (Color)ColorConverter.ConvertFromString(App.Current.Resources["WindowButtonsHoverForeground1"].ToString()) }; // Set the foreground
	}

	private void BtnLeave(object sender, MouseEventArgs e)
	{
		Button button = (Button)sender; // Create button
		button.Foreground = new SolidColorBrush { Color = (Color)ColorConverter.ConvertFromString(App.Current.Resources["Foreground1"].ToString()) }; // Set the foreground 
	}

	private void SystemRadioBtn_Checked(object sender, RoutedEventArgs e)
	{
		ThemeApplyBtn.Visibility = Visibility.Visible; // Show
	}

	private void LightBorder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		LightRadioBtn.IsChecked = true; // Set IsChecked
		CheckedBorder = LightBorder; // Set
		RefreshBorders();
	}

	Border CheckedBorder { get; set; }
	private void Border_MouseEnter(object sender, MouseEventArgs e)
	{
		Border border = (Border)sender;
		border.BorderBrush = new SolidColorBrush() { Color = (Color)ColorConverter.ConvertFromString(App.Current.Resources["AccentColor"].ToString()) }; // Set color
	}

	private void Border_MouseLeave(object sender, MouseEventArgs e)
	{
		Border border = (Border)sender;
		if (border != CheckedBorder && border != PageCheckedBorder)
		{
			border.BorderBrush = new SolidColorBrush() { Color = Colors.Transparent }; // Set color 
		}
	}

	private void DarkBorder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		DarkRadioBtn.IsChecked = true; // Set IsChecked
		CheckedBorder = DarkBorder; // Set
		RefreshBorders();
	}

	private void SystemBorder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		SystemRadioBtn.IsChecked = true; // Set IsChecked
		CheckedBorder = SystemBorder; // Set
		RefreshBorders();
	}

	private void RefreshBorders()
	{
		LightBorder.BorderBrush = new SolidColorBrush() { Color = Colors.Transparent }; // Set color 
		DarkBorder.BorderBrush = new SolidColorBrush() { Color = Colors.Transparent }; // Set color 
		SystemBorder.BorderBrush = new SolidColorBrush() { Color = Colors.Transparent }; // Set color 

		CheckedBorder.BorderBrush = new SolidColorBrush() { Color = (Color)ColorConverter.ConvertFromString(App.Current.Resources["AccentColor"].ToString()) }; // Set color
	}

	private void QRForeColorRec_MouseDown(object sender, MouseButtonEventArgs e)
	{
		System.Windows.Forms.ColorDialog colorDialog = new()
		{
			AllowFullOpen = true,
		}; // Create color picker/dialog

		if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) // If the user selected a color
		{
			Global.Settings.QRCodeForegroundColor = $"{colorDialog.Color.R};{colorDialog.Color.G};{colorDialog.Color.B}";
			SettingsManager.Save(); // Save changes

			QRForeColorRec.Fill = new SolidColorBrush { Color = Color.FromRgb(colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B) }; // Set color
		}
	}

	private void QRBackColorRec_MouseDown(object sender, MouseButtonEventArgs e)
	{
		System.Windows.Forms.ColorDialog colorDialog = new()
		{
			AllowFullOpen = true,
		}; // Create color picker/dialog

		if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) // If the user selected a color
		{
			Global.Settings.QRCodeBackgroundColor = $"{colorDialog.Color.R};{colorDialog.Color.G};{colorDialog.Color.B}";
			SettingsManager.Save(); // Save changes

			QRBackColorRec.Fill = new SolidColorBrush { Color = Color.FromRgb(colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B) }; // Set color
		}
	}

	private void BarCodeTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		Global.Settings.DefaultBarCodeType = (Barcodes)BarCodeTypeComboBox.SelectedIndex; // Set the default type
		SettingsManager.Save(); // Save changes
	}

	private void GenerateQRCodeTypingChk_Checked(object sender, RoutedEventArgs e)
	{
		Global.Settings.GenerateQRCodeWhileTyping = GenerateQRCodeTypingChk.IsChecked; // Set the default type
		SettingsManager.Save(); // Save changes
	}

	private void ResetColorsLink_Click(object sender, RoutedEventArgs e)
	{
		Global.Settings.BarCodeForegroundColor = "0;0;0"; // Set black
		Global.Settings.BarCodeBackgroundColor = "255;255;255"; // Set white

		SettingsManager.Save(); // Save changes
		InitUI(); // Refresh
	}

	private void QRResetColorsLink_Click(object sender, RoutedEventArgs e)
	{
		Global.Settings.QRCodeForegroundColor = "0;0;0"; // Set black
		Global.Settings.QRCodeBackgroundColor = "255;255;255"; // Set white

		SettingsManager.Save(); // Save changes
		InitUI(); // Refresh
	}

	private void ResetSettingsLink_Click(object sender, RoutedEventArgs e)
	{
		if (MessageBox.Show(Properties.Resources.ResetSettingsConfirmMsg, Properties.Resources.Settings, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
		{
			Global.Settings = new()
			{
				CheckUpdatesOnStart = true,
				IsDarkTheme = false,
				Language = "_default",
				NotifyUpdates = true,
				BarCodeBackgroundColor = "255;255;255",
				BarCodeForegroundColor = "0;0;0",
				GenerateBarCodeOnStart = true,
				GenerateQRCodeOnStart = true,
				IsThemeSystem = true,
				QRCodeBackgroundColor = "255;255;255",
				QRCodeForegroundColor = "0;0;0",
				DefaultBarCodeType = Barcodes.Code128,
				GenerateQRCodeWhileTyping = true,
				DefaultBarCodeFileExtension = SupportedFileExtensions.PNG,
				DefaultQRCodeFileExtension = SupportedFileExtensions.PNG,
				IsFirstRun = false,
				StartupPage = AppPages.BarCode,
			}; // Create default settings

			SettingsManager.Save(); // Save the changes
			InitUI(); // Reload the page

			MessageBox.Show(Properties.Resources.SettingsReset, Properties.Resources.Gerayis, MessageBoxButton.OK, MessageBoxImage.Information);
			Process.Start(Directory.GetCurrentDirectory() + @"\Gerayis.exe");
			Environment.Exit(0); // Quit
		}
	}

	private void SeeLicensesLink_Click(object sender, RoutedEventArgs e)
	{
		MessageBox.Show($"{Properties.Resources.Licenses}\n\n" +
			"Fluent System Icons - MIT License - © 2020 Microsoft Corporation\n" +
			"QRCoder - MIT License - © 2013-2018 Raffael Herrmann\n" +
			"barcodelib - Apache License - Version 2.0, January 2004 - © Brad Barnhill\n" +
			"LeoCorpLibrary - MIT License - © 2020-2022 Léo Corporation\n" +
			"Gerayis - MIT License - © 2021-2022 Léo Corporation", $"{Properties.Resources.Gerayis} - {Properties.Resources.Licenses}", MessageBoxButton.OK, MessageBoxImage.Information);
	}

	private void BarCodesSaveFormatComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		Global.Settings.DefaultBarCodeFileExtension = (SupportedFileExtensions)BarCodesSaveFormatComboBox.SelectedIndex; // Set the default file extension
		SettingsManager.Save(); // Save changes
	}

	private void QRCodeSaveFormatComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		Global.Settings.DefaultQRCodeFileExtension = (SupportedFileExtensions)QRCodeSaveFormatComboBox.SelectedIndex; // Set the default file extension
		SettingsManager.Save(); // Save changes
	}

	Border PageCheckedBorder { get; set; }
	private void RefreshStartupBorders()
	{
		BarCodePageBorder.BorderBrush = new SolidColorBrush() { Color = Colors.Transparent }; // Set color 
		QrCodePageBorder.BorderBrush = new SolidColorBrush() { Color = Colors.Transparent }; // Set color

		PageCheckedBorder.BorderBrush = new SolidColorBrush() { Color = (Color)ColorConverter.ConvertFromString(App.Current.Resources["AccentColor"].ToString()) }; // Set color
	}

	private void BarCodePageBorder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		PageCheckedBorder = BarCodePageBorder; // Set
		BarCodePageRadioBtn.IsChecked = true;
		RefreshStartupBorders(); // Refresh

		Global.Settings.StartupPage = AppPages.BarCode; // Set
		SettingsManager.Save(); // Save changes
	}

	private void QrCodePageBorder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		PageCheckedBorder = QrCodePageBorder; // Set
		QrCodePageRadioBtn.IsChecked = true;
		RefreshStartupBorders(); // Refresh

		Global.Settings.StartupPage = AppPages.QRCode; // Set
		SettingsManager.Save(); // Save changes
	}
}
