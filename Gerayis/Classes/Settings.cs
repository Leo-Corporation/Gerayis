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
using Gerayis.Enums;
using LeoCorpLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace Gerayis.Classes
{
	/// <summary>
	/// Settings of Gerayis
	/// </summary>
	public class Settings
	{
		/// <summary>
		/// True if the theme of Gerayis is set to dark.
		/// </summary>
		public bool IsDarkTheme { get; set; }

		/// <summary>
		/// The language of the app (country code). Can be _default, en-US, fr-FR...
		/// </summary>
		public string Language { get; set; }

		/// <summary>
		/// True if Gerayis should check updates on start.
		/// </summary>
		public bool? CheckUpdatesOnStart { get; set; }

		/// <summary>
		/// True if Gerayis should show a notification to the user.
		/// </summary>
		public bool? NotifyUpdates { get; set; }

		/// <summary>
		/// Bar code foreground color.
		/// </summary>
		public string BarCodeForegroundColor { get; set; }

		/// <summary>
		/// Bar code background color.
		/// </summary>
		public string BarCodeBackgroundColor { get; set; }

		/// <summary>
		/// True if Gerayis should generate a bar code on start.
		/// </summary>
		public bool? GenerateBarCodeOnStart { get; set; }

		/// <summary>
		/// True if Gerayis should generate a QR code on start.
		/// </summary>
		public bool? GenerateQRCodeOnStart { get; set; }

		/// <summary>
		/// True if Gerayis should follow the system's theme.
		/// </summary>
		public bool? IsThemeSystem { get; set; }

		/// <summary>
		/// QR code foreground color.
		/// </summary>
		public string QRCodeForegroundColor { get; set; }

		/// <summary>
		/// QR code background color.
		/// </summary>
		public string QRCodeBackgroundColor { get; set; }

		/// <summary>
		/// The default bar code type
		/// </summary>
		public Barcodes? DefaultBarCodeType { get; set; }
	}

	/// <summary>
	/// Class that contains methods that can manage Gerayis' settings.
	/// </summary>
	public static class SettingsManager
	{
		/// <summary>
		/// Loads Gerayis settings.
		/// </summary>
		public static void Load()
		{
			string path = Env.AppDataPath + @"\Léo Corporation\Gerayis\Settings.xml"; // The path of the settings file

			if (File.Exists(path)) // If the file exist
			{
				XmlSerializer xmlSerializer = new XmlSerializer(typeof(Settings)); // XML Serializer
				StreamReader streamReader = new StreamReader(path); // Where the file is going to be read

				Global.Settings = (Settings)xmlSerializer.Deserialize(streamReader); // Read

				streamReader.Dispose();
			}
			else
			{
				Global.Settings = new Settings
				{
					IsDarkTheme = false,
					Language = "_default",
					CheckUpdatesOnStart = true,
					NotifyUpdates = true,
					BarCodeBackgroundColor = "255;255;255",
					BarCodeForegroundColor = "0;0;0",
					GenerateBarCodeOnStart = true,
					GenerateQRCodeOnStart = true,
					IsThemeSystem = false,
					QRCodeBackgroundColor = "255;255;255",
					QRCodeForegroundColor = "0;0;0",
					DefaultBarCodeType = Barcodes.Code128
				}; // Create a new settings file

				Save(); // Save the changes
			}
		}

		/// <summary>
		/// Saves Gerayis settings.
		/// </summary>
		public static void Save()
		{
			string path = Env.AppDataPath + @"\Léo Corporation\Gerayis\Settings.xml"; // The path of the settings file

			XmlSerializer xmlSerializer = new XmlSerializer(typeof(Settings)); // Create XML Serializer

			if (!Directory.Exists(Env.AppDataPath + @"\Léo Corporation\Gerayis")) // If the directory doesn't exist
			{
				Directory.CreateDirectory(Env.AppDataPath + @"\Léo Corporation\"); // Create the directory
				Directory.CreateDirectory(Env.AppDataPath + @"\Léo Corporation\Gerayis"); // Create the directory
			}

			StreamWriter streamWriter = new StreamWriter(path); // The place where the file is going to be written
			xmlSerializer.Serialize(streamWriter, Global.Settings);

			streamWriter.Dispose();
		}

		/// <summary>
		/// Exports current settings.
		/// </summary>
		/// <param name="path">The path where the settings file should be exported.</param>
		public static void Export(string path)
		{
			try
			{
				XmlSerializer xmlSerializer = new XmlSerializer(typeof(Settings)); // Create XML Serializer

				StreamWriter streamWriter = new StreamWriter(path); // The place where the file is going to be written
				xmlSerializer.Serialize(streamWriter, Global.Settings);

				streamWriter.Dispose();

				MessageBox.Show(Properties.Resources.SettingsExportedSucessMsg, Properties.Resources.Gerayis, MessageBoxButton.OK, MessageBoxImage.Information); // Show message
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, Properties.Resources.Gerayis, MessageBoxButton.OK, MessageBoxImage.Error); // Show error message
			}
		}

		/// <summary>
		/// Imports settings.
		/// </summary>
		/// <param name="path">The path to the settings file.</param>
		public static void Import(string path)
		{
			try
			{
				if (File.Exists(path)) // If the file exist
				{
					XmlSerializer xmlSerializer = new XmlSerializer(typeof(Settings)); // XML Serializer
					StreamReader streamReader = new StreamReader(path); // Where the file is going to be read

					Global.Settings = (Settings)xmlSerializer.Deserialize(streamReader); // Read

					streamReader.Dispose();
					Save(); // Save
					MessageBox.Show(Properties.Resources.SettingsImportedMsg, Properties.Resources.Gerayis, MessageBoxButton.OK, MessageBoxImage.Information); // Show error message

					// Restart app
					Process.Start(Directory.GetCurrentDirectory() + @"\Gerayis.exe"); // Start app
					Environment.Exit(0); // Quit
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, Properties.Resources.Gerayis, MessageBoxButton.OK, MessageBoxImage.Error); // Show error message
			}
		}
	}
}
