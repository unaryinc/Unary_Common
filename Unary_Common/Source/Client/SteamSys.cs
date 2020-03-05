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
using Unary_Common.Arguments;
using Unary_Common.Shared;

using System;

using Steamworks;

namespace Unary_Common.Client
{
    public class SteamSys : Godot.Object, IClient
    {
        private EventSys EventSys;

        public int AuthTicketSize { get; set; } = 1024;
        public HAuthTicket AuthTicket { get; set; }

        private Callback<GameOverlayActivated_t> CallbackGameOverlayActivated;

        public void Init()
        {
            EventSys = Sys.Ref.GetSharedNode<EventSys>();
            CallbackGameOverlayActivated = Callback<GameOverlayActivated_t>.Create(OnOverlayActivated);

            EventSys.SubscribeEvent(this, nameof(OnJoin), "Unary_Common.CJoin");
            EventSys.SubscribeEvent(this, nameof(OnLeave), "Unary_Common.CLeave");
        }

        public void Clear()
        {

        }

        private void OnOverlayActivated(GameOverlayActivated_t Callback)
        {
            OverlayActivated NewResponse = new OverlayActivated();
            NewResponse.Target = Callback;
            EventSys.InvokeEvent("Unary_Common.COverlay", NewResponse);
        }

        public string GetPersonaName()
        {
            return SteamFriends.GetPersonaName();
        }

        public byte[] GetAuthTicket()
        {
            byte[] TempTicket = new byte[AuthTicketSize];

            uint TicketSize;

            AuthTicket = SteamUser.GetAuthSessionTicket(TempTicket, AuthTicketSize, out TicketSize);

            byte[] ResultTicket = new byte[TicketSize];

            Array.Copy(TempTicket, 0, ResultTicket, 0, TicketSize);

            return ResultTicket;
        }

        public void CancelAuthTicket()
        {
            SteamUser.CancelAuthTicket(AuthTicket);
        }

        public void OnJoin()
        {

        }

        public void OnLeave()
        {

        }
    }
}