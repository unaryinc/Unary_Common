﻿/*
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

using Unary_Common.Interfaces;
using Unary_Common.Shared;
using Unary_Common.Abstract;
using Unary_Common.Structs;
using Unary_Common.Arguments;

using System;
using System.Collections.Generic;

namespace Unary_Common.Server
{
    public class RegistrySys : SysObject
    {
        Shared.RegistrySys RegistrySysShared;
        NetworkSys NetworkSys;

        public override void Init()
        {
            RegistrySysShared = Sys.Ref.Shared.GetObject<Shared.RegistrySys>();
            NetworkSys = Sys.Ref.Server.GetNode<NetworkSys>();
        }

        public override void Clear()
        {
            RegistrySysShared.Synced = false;
        }

        public override Args Sync()
        {
            RegistrySysShared.Synced = true;
            return new RegistrySysSync { Registry = RegistrySysShared.Registry };
        }

        public void AddEntry(string RegistryName, string ModIDEntry)
        {
            NetworkSys.RPCIDAll("Unary_Common.RegistrySys.AddEntry", new RegistrySysEntry()
            {
                RegistryName = RegistryName,
                ModIDEntry = ModIDEntry
            });
        }

        public void RemoveEntry(string RegistryName, string ModIDEntry)
        {
            NetworkSys.RPCIDAll("Unary_Common.RegistrySys.RemoveEntry", new RegistrySysEntry()
            {
                RegistryName = RegistryName,
                ModIDEntry = ModIDEntry
            });
        }

    }
}