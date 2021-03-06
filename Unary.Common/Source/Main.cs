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

using Unary.Common.Interfaces;

using Godot;
using Unary.Common.UI;
using Unary.Common.Utils;

using Unary.Common.Shared;
using Unary.Common.Client;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System;

namespace Unary.Common
{
	public class Main : IBoot
	{
		private Sys Sys;
		

		public class Test
		{
			public void Lol()
			{

			}
		}

		public void AddShared()
		{
			Sys = Sys.Ref;

			Test NewTest = new Test();
			NewTest.Lol();
			NewTest.GetType().GetMethod("Lol").Invoke(NewTest, null);

			System.Type NewType = new Type()
			{
				
			}

			// Adding shared systems
			Sys.Shared.AddObject(new EnvironmentSys());
			Sys.Shared.AddObject(new RegistrySys());
			Sys.Shared.AddNode(new AssemblySys());
			Sys.Shared.AddNode(new EventSys());
			Sys.Shared.AddObject(new ConfigSys());
			Sys.Shared.AddNode(new Shared.SteamSys());
			Sys.Shared.AddObject(new FilesystemSys());
			Sys.Shared.AddObject(new EntriesSys());
			Sys.Shared.AddNode(new InterpreterSys());
			Sys.Shared.AddNode(new RCONSys());
			Sys.Shared.AddObject(new HarmonySys());
			Sys.Shared.AddNode(new OSSys());
			Sys.Shared.AddNode(new RandomSys());
			Sys.Shared.AddObject(new SaveSys());
			//Sys.Shared.Add(new ProfilerManager());
		}

		public void AddClient()
		{
			// Adding client systems
			Sys.Client.AddNode(new Client.DiscordSys());
			Sys.Client.AddObject(new LocaleSys());
			Sys.Client.AddNode(new BindSys());
			Sys.Client.AddNode(new MusicSys());
			Sys.Client.AddUI(new AlertSys(), "Alert");
			Sys.Client.AddNode(new SceneSys());
		}

		public void AddServer()
		{
			Sys.Server.AddNode(new SceneSys());
		}
	}
}
