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
using Unary_Common.Structs;
using Unary_Common.Arguments;
using Unary_Common.Arguments.Internal;
using Unary_Common.Utils;
using Unary_Common.Abstract;

using Godot;

using System;
using System.Collections.Generic;

using Steamworks;

namespace Unary_Common.Shared
{
    public class SteamSys : SysNode
    {
        public uint AppID { get; private set; }

        private static ConsoleSys ConsoleSys;
        private EventSys EventSys;

        public List<string> ExistingItems { get; private set; }

        protected SteamAPIWarningMessageHook_t m_SteamAPIWarningMessageHook;
        protected static void SteamAPIDebugTextHook(int nSeverity, System.Text.StringBuilder pchDebugText)
        {
            ConsoleSys.Warning(pchDebugText.ToString());
        }

        private List<ulong> DownloadList;
        private Callback<DownloadItemResult_t> DownloadItemResponse;

        public override void Init()
        {
            if(!FilesystemUtil.SystemFileExists("steam_appid.txt"))
            {
                ConsoleSys.Panic("steam_appid.txt does not exist, could not get AppID.");
                return;
            }

            ConsoleSys = Sys.Ref.ConsoleSys;
            EventSys = Sys.Ref.Shared.GetNode<EventSys>();

            try
            {
                AppID = uint.Parse(FilesystemUtil.SystemFileRead("steam_appid.txt"));

                if (!Packsize.Test())
                {
                    ConsoleSys.Panic("Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.");
                    return;
                }

                if (!DllCheck.Test())
                {
                    ConsoleSys.Panic("DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.");
                    return;
                }

                if (SteamAPI.RestartAppIfNecessary(new AppId_t(AppID)))
                {
                    ConsoleSys.Panic("Steam requested us to restart app");
                    return;
                }
            }
            catch (Exception Exception)
            {
                ConsoleSys.Panic(Exception.Message);
                return;
            }

            if (!SteamAPI.Init())
            {
                ConsoleSys.Panic("SteamAPI_Init() failed. Refer to Valve's documentation or the comment above this line for more information.");
                return;
            }
            else
            {
                m_SteamAPIWarningMessageHook = new SteamAPIWarningMessageHook_t(SteamAPIDebugTextHook);
                SteamClient.SetWarningMessageHook(m_SteamAPIWarningMessageHook);

                ExistingItems = GetModFolders();

                DownloadList = new List<ulong>();
                DownloadItemResponse = Callback<DownloadItemResult_t>.Create(OnDownloadedItem);
            }
        }

        public override void Clear()
        {
            SteamAPI.Shutdown();
        }

        public static IEnumerable<PublishedFileId_t> GetAllSubscribed()
        {
            uint SubCount = SteamUGC.GetNumSubscribedItems();

            PublishedFileId_t[] Items = new PublishedFileId_t[SubCount];

            uint Count = SteamUGC.GetSubscribedItems(Items, SubCount);

            for(uint i = 0; i < Count; ++i)
            {
                PublishedFileId_t pfid = Items[i];
                yield return pfid;
            }

            yield break;
        }

        public List<string> GetModFolders()
        {
            List<string> Result = new List<string>();

            foreach (var Subbed in GetAllSubscribed())
            {
                string Current = GetItemPath(Subbed);

                if(Current != null)
                {
                    Result.Add(Current);
                }
            }

            return Result;
        }

        public void DownloadItems(List<ulong> ItemHandles)
        {
            foreach(var ItemHandle in ItemHandles)
            {
                if (!DownloadList.Contains(ItemHandle))
                {
                    SteamUGC.DownloadItem(new PublishedFileId_t(ItemHandle), true);
                    DownloadList.Add(ItemHandle);
                }
            }
        }

        private string GetItemPath(PublishedFileId_t Item)
        {
            ulong punSizeOnDisk;
            string Result = null;
            uint cchFolderSize = default;
            uint punTimeStamp;

            SteamUGC.GetItemInstallInfo(Item, out punSizeOnDisk, out Result, cchFolderSize, out punTimeStamp);

            return Result;
        }

        public void OnDownloadedItem(DownloadItemResult_t Response)
        {
            if(EnumUtil.GetStringFromKey(Response.m_eResult).Replace("k_EResult", "") == "OK")
            {
                ExistingItems.Add(GetItemPath(Response.m_nPublishedFileId));

                EventSys.Internal.Invoke("Unary_Common.Steam.DownloadedItem", new DownloadItem()
                { ItemHandle = Response.m_nPublishedFileId.m_PublishedFileId });
            }
            else
            {
                DownloadList.Remove(Response.m_nPublishedFileId.m_PublishedFileId);
                EventSys.Internal.Invoke("Unary_Common.Steam.DownloadedItemFailed", new DownloadItem()
                { ItemHandle = Response.m_nPublishedFileId.m_PublishedFileId });
            }

            if(DownloadList.Count == 0)
            {
                EventSys.Internal.Invoke("Unary_Common.Steam.DownloadedAll", null);
            }
        }

        public override void _Process(float delta)
        {
            SteamAPI.RunCallbacks();
        }
    }
}