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
using Gerayis.Pages;
using LeoCorpLibrary;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Gerayis.Classes
{
	/// <summary>
	/// The <see cref="Global"/> class contains various methods and properties.
	/// </summary>
	public static class Global
	{
		/// <summary>
		/// The current version of Gerayis.
		/// </summary>
		public static string Version => "1.9.0.2112";

		/// <summary>
		/// List of the available languages.
		/// </summary>
		public static List<string> LanguageList => new() { "English (United States)", "Français (France)" };

		/// <summary>
		/// List of the available languages codes.
		/// </summary>
		public static List<string> LanguageCodeList => new() { "en-US", "fr-FR" };

		/// <summary>
		/// GitHub link for the last version (<see cref="string"/>).
		/// </summary>
		public static string LastVersionLink { get => "https://raw.githubusercontent.com/Leo-Corporation/LeoCorp-Docs/master/Liens/Update%20System/Gerayis/Version.txt"; }

		/// <summary>
		/// The <see cref="Pages.BarCodePage"/>.
		/// </summary>
		public static BarCodePage BarCodePage { get; set; }

		/// <summary>
		/// The <see cref="Pages.QRCodePage"/>.
		/// </summary>
		public static QRCodePage QRCodePage { get; set; }

		/// <summary>
		/// The <see cref="Pages.SettingsPage"/>.
		/// </summary>
		public static SettingsPage SettingsPage { get; set; }

		/// <summary>
		/// The <see cref="Classes.Settings"/> of Gerayis
		/// </summary>
		public static Settings Settings { get; set; }

		/// <summary>
		/// Gets the "Hi" sentence message.
		/// </summary>
		public static string GetHiSentence
		{
			get
			{
				if (DateTime.Now.Hour >= 21 && DateTime.Now.Hour <= 7) // If between 9PM & 7AM
				{
					return Properties.Resources.GoodNight + ", " + Environment.UserName + "."; // Return the correct value
				}
				else if (DateTime.Now.Hour >= 7 && DateTime.Now.Hour <= 12) // If between 7AM - 12PM
				{
					return Properties.Resources.Hi + ", " + Environment.UserName + "."; // Return the correct value
				}
				else if (DateTime.Now.Hour >= 12 && DateTime.Now.Hour <= 17) // If between 12PM - 5PM
				{
					return Properties.Resources.GoodAfternoon + ", " + Environment.UserName + "."; // Return the correct value
				}
				else if (DateTime.Now.Hour >= 17 && DateTime.Now.Hour <= 21) // If between 5PM - 9PM
				{
					return Properties.Resources.GoodEvening + ", " + Environment.UserName + "."; // Return the correct value
				}
				else
				{
					return Properties.Resources.Hi + ", " + Environment.UserName + "."; // Return the correct value
				}
			}
		}

		/// <summary>
		/// Changes the application's theme.
		/// </summary>
		public static void ChangeTheme()
		{
			App.Current.Resources.MergedDictionaries.Clear();
			ResourceDictionary resourceDictionary = new(); // Create a resource dictionary

			if (!Settings.IsThemeSystem.HasValue)
			{
				Settings.IsThemeSystem = false;
			}

			if (Settings.IsThemeSystem.Value)
			{
				Settings.IsDarkTheme = IsSystemThemeDark(); // Set
			}

			if (Settings.IsDarkTheme) // If the dark theme is on
			{
				resourceDictionary.Source = new Uri("..\\Themes\\Dark.xaml", UriKind.Relative); // Add source
			}
			else
			{
				resourceDictionary.Source = new Uri("..\\Themes\\Light.xaml", UriKind.Relative); // Add source
			}

			App.Current.Resources.MergedDictionaries.Add(resourceDictionary); // Add the dictionary
		}

		public static bool IsSystemThemeDark()
		{
			if (Env.WindowsVersion != WindowsVersion.Windows10 && Env.WindowsVersion != WindowsVersion.Windows11)
			{
				return false; // Avoid errors on older OSs
			}

			var t = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "SystemUsesLightTheme", "1");
			return t switch
			{
				0 => true,
				1 => false,
				_ => false
			}; // Return
		}

		/// <summary>
		/// Changes the application's language.
		/// </summary>
		public static void ChangeLanguage()
		{
			switch (Global.Settings.Language) // For each case
			{
				case "_default": // No language
					break;
				case "en-US": // English (US)
					Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-US"); // Change
					break;

				case "fr-FR": // French (FR)
					Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("fr-FR"); // Change
					break;
				default: // No language
					break;
			}
		}

		public static void SaveImage(string filePath, BitmapSource bitmapImage, string extension = ".png")
		{
			try
			{
				using (var fileStream = new FileStream(filePath, FileMode.Create))
				{
					BitmapEncoder encoder = extension switch
					{
						".png" => new PngBitmapEncoder(),
						".jpg" => new JpegBitmapEncoder(),
						".jpeg" => new JpegBitmapEncoder(),
						_ => new PngBitmapEncoder()
					};
					encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
					encoder.Save(fileStream);
				}

				MessageBox.Show(Properties.Resources.SaveSucess, Properties.Resources.Save, MessageBoxButton.OK, MessageBoxImage.Information); // Show sucess message
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error); // Show error
			}
		}
	}
}
