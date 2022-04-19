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
using Gerayis.Windows;
using LeoCorpLibrary;
using System.IO;
using System.Windows;

namespace Gerayis;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
	protected override void OnStartup(StartupEventArgs e)
	{
		SettingsManager.Load(); // Load settings

		Global.ChangeTheme(); // Change the theme
		Global.ChangeLanguage(); // Change the language

		Global.SettingsPage = new(); // Create a new SettingsPage
		Global.BarCodePage = new(); // Create a new BarCodePage
		Global.QRCodePage = new(); // Create a new QRCodePage

		if (!File.Exists(Env.AppDataPath + @"\Léo Corporation\Gerayis\JumpList.txt")) // Checks if the jumplists have already been generated
		{
			Global.CreateJumpLists(); // Create the JumpLists
		}

		if (Global.Settings.IsFirstRun.Value)
		{
			new FirstRunWindow().Show(); // Show the "First run" window
		}
		else
		{
			if (e.Args.Length > 1)
			{
				switch ($"{e.Args[0]} {e.Args[1]}")
				{
					case "/page 0":
						new MainWindow(Enums.AppPages.BarCode).Show(); // Launch Gerayis with the BarCode page
						break;
					case "/page 1":
						new MainWindow(Enums.AppPages.QRCode).Show(); // Launch Gerayis with the QRCode page
						break;
					default:
						new MainWindow().Show(); // Launch Gerayis with the default page
						break;
				}
			}
			else
			{
				new MainWindow().Show(); // Launch Gerayis with the default page
			}
		}
	}
}
