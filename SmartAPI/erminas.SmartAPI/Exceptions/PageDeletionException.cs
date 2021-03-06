﻿// SmartAPI - .Net programmatic access to RedDot servers
//  
// Copyright (C) 2013 erminas GbR
// 
// This program is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License along with this program.
// If not, see <http://www.gnu.org/licenses/>.

using System;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.Exceptions
{
    public enum PageDeletionError
    {
        Unknown = 0,
        NoRightToDeletePage,
        ElementsOfPageStillGetReferenced
    }

    [Serializable]
    public class PageDeletionException : SmartAPIException
    {
        public readonly PageDeletionError Error;

        internal PageDeletionException(RQLException e) : base(e.Server, e.Message, e)
        {
            switch (e.ErrorCode)
            {
                case ErrorCode.RDError2910:
                    Error = PageDeletionError.ElementsOfPageStillGetReferenced;
                    break;
                case ErrorCode.RDError15805:
                    Error = PageDeletionError.NoRightToDeletePage;
                    break;
                default:
                    Error = PageDeletionError.Unknown;
                    break;
            }
        }

        internal PageDeletionException(ServerLogin login, string message) : base(login, message)
        {
            Error = PageDeletionError.Unknown;
        }
    }
}