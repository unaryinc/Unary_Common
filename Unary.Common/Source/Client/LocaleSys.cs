/*
MIT License

Copyright (c) 2020 Unary Incorporated

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

using Unary.Common.Utils;
using Unary.Common.Interfaces;
using Unary.Common.Shared;
using Unary.Common.Structs;
using Unary.Common.Abstract;

using Godot;

using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;

namespace Unary.Common.Client
{
	public class LocaleSys : SysObject
	{
		private Dictionary<string, string> LocaleList;

		private bool SelectedIsFallback = false;

		private string SelectedLocale;
		private Dictionary<string, string> SelectedLocaleEntries;

		private string FallbackLocale;
		private Dictionary<string, string> FallbackLocaleEntries;

		private ConsoleSys ConsoleSys;

		public override void Init()
		{
			ConsoleSys = Sys.Ref.ConsoleSys;

			if (!FilesystemUtil.GD.FileExists("res://Unary.Common/Locales/Locales.json"))
			{
				ConsoleSys.Error("Failed to find Locales.json, LocaleSys will likely crash everything");
				return;
			}

			string LocalesManifest = FilesystemUtil.GD.FileRead("res://Unary.Common/Locales/Locales.json");

			if (LocalesManifest == null)
			{
				ConsoleSys.Error("Failed to read Locales.json, LocaleSys will likely crash everything");
				return;
			}

			try
			{
				LocaleList = JsonConvert.DeserializeObject<Dictionary<string, string>>(LocalesManifest);
			}
			catch (Exception Exception)
			{
				ConsoleSys.Error("Failed to parse Locales.json, LocaleSys will likely crash everything");
				ConsoleSys.Error(Exception.Message);
				return;
			}

			SelectedLocaleEntries = new Dictionary<string, string>();
			FallbackLocaleEntries = new Dictionary<string, string>();

			SelectedLocale = Sys.Ref.Shared.GetObject<ConfigSys>().Shared.Get<string>("Unary.Common.Locale");
			FallbackLocale = Sys.Ref.Shared.GetObject<ConfigSys>().Shared.Get<string>("Unary.Common.Locale.Fallback");

			if (SelectedLocale == FallbackLocale)
			{
				SelectedIsFallback = true;
			}

			if (!LocaleList.ContainsKey(SelectedLocale))
			{
				SelectedLocale = "en";
			}

			if (!LocaleList.ContainsKey(FallbackLocale))
			{
				FallbackLocale = "en";
			}

			LoadLocale("Unary.Common", SelectedLocale, true);
			if (!SelectedIsFallback)
			{
				LoadLocale("Unary.Common", FallbackLocale, false);
			}
		}

		public override void Clear()
		{
			LocaleList.Clear();
			SelectedLocaleEntries.Clear();
			FallbackLocaleEntries.Clear();
		}

		public override void ClearMod(Mod Mod)
		{
			foreach (var Entry in SelectedLocaleEntries.ToList())
			{
				if (Entry.Key.StartsWith(Mod.ModID + '.'))
				{
					SelectedLocaleEntries.Remove(Entry.Key);
				}
			}

			foreach (var Entry in FallbackLocaleEntries.ToList())
			{
				if (Entry.Key.StartsWith(Mod.ModID + '.'))
				{
					FallbackLocaleEntries.Remove(Entry.Key);
				}
			}
		}

		private void LoadLocale(string ModID, string LocaleIndex, bool Selected)
		{
			string LocaleDir = "res://" + ModID + "/Locales/" + LocaleIndex;

			if(!FilesystemUtil.GD.DirExists(LocaleDir))
			{
				ConsoleSys.Error("Failed to load locale at " + LocaleDir + " because folder does not exist");
				return;
			}

			List<string> LocaleFiles = FilesystemUtil.GD.DirGetFiles(LocaleDir);

			for(int i = LocaleFiles.Count - 1; i >= 0; --i)
			{
				if(!LocaleFiles[i].EndsWith(".json"))
				{
					LocaleFiles.RemoveAt(i);
				}
			}

			for (int i = LocaleFiles.Count - 1; i >= 0; --i)
			{
				string LocaleManifest = FilesystemUtil.GD.FileRead(LocaleFiles[i]);

				try
				{
					Dictionary<string, string> LocaleEntries = JsonConvert.DeserializeObject<Dictionary<string, string>>(LocaleManifest);

					foreach (var Entry in LocaleEntries)
					{
						if(!ModIDUtil.Validate(Entry.Key))
						{
							ConsoleSys.Error("Locale entry " + Entry.Key + " is not a valid ModIDEntry");
							continue;
						}

						if (Selected)
						{
							SelectedLocaleEntries[Entry.Key] = Entry.Value;
						}
						else
						{
							FallbackLocaleEntries[Entry.Key] = Entry.Value;
						}
					}
				}
				catch (Exception Exception)
				{
					ConsoleSys.Error("Failed to parse locale manifest at " + LocaleFiles[i]);
					ConsoleSys.Error(Exception.Message);
					continue;
				}
			}
		}

		public string Translate(string ModIDEntry)
		{
			if(!ModIDUtil.Validate(ModIDEntry))
			{
				ConsoleSys.Error("Failed to validate " + ModIDEntry);
				return default;
			}

			if(SelectedLocaleEntries.ContainsKey(ModIDEntry))
			{
				return SelectedLocaleEntries[ModIDEntry];
			}
			else if(FallbackLocaleEntries.ContainsKey(ModIDEntry))
			{
				ConsoleSys.Warning("Failed to localize " + ModIDEntry + ", using fallback instead");
				return FallbackLocaleEntries[ModIDEntry];
			}
			else
			{
				ConsoleSys.Error("Failed to localize " + ModIDEntry + ", using ModIDEntry instead");
				return ModIDEntry;
			}
		}

		public string Translate(string ModIDEntry, params object[] Arguments)
		{
			if (!ModIDUtil.Validate(ModIDEntry))
			{
				ConsoleSys.Error("Failed to validate " + ModIDEntry);
				return default;
			}

			if (SelectedLocaleEntries.ContainsKey(ModIDEntry))
			{
				try
				{
					return string.Format(SelectedLocaleEntries[ModIDEntry], Arguments);
				}
				catch(Exception Exception)
				{
					ConsoleSys.Error("Failed to format " + ModIDEntry + ", using ModIDEntry instead");
					ConsoleSys.Error(Exception.Message);
					return ModIDEntry;
				}
			}
			else if (FallbackLocaleEntries.ContainsKey(ModIDEntry))
			{
				ConsoleSys.Warning("Failed to localize " + ModIDEntry + ", using fallback instead");

				try
				{
					return string.Format(FallbackLocaleEntries[ModIDEntry], Arguments);
				}
				catch (Exception Exception)
				{
					ConsoleSys.Error("Failed to format " + ModIDEntry + ", using ModIDEntry instead");
					ConsoleSys.Error(Exception.Message);
					return ModIDEntry;
				}
			}
			else
			{
				ConsoleSys.Error("Failed to localize " + ModIDEntry + ", using ModIDEntry instead");
				return ModIDEntry;
			}
		}

		public override void InitCore(Mod Mod)
		{
			LoadLocale(Mod.ModID, SelectedLocale, true);
			if (!SelectedIsFallback)
			{
				LoadLocale(Mod.ModID, FallbackLocale, false);
			}
		}

		public override void InitMod(Mod Mod)
		{
			LoadLocale(Mod.ModID, SelectedLocale, true);
			if (!SelectedIsFallback)
			{
				LoadLocale(Mod.ModID, FallbackLocale, false);
			}
		}
	}
}
