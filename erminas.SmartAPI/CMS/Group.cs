﻿// Smart API - .Net programmatic access to RedDot servers
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

using System.Xml;
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS
{
    /// <summary>
    ///     Group data:
    ///     <pre>
    ///         <GROUP action="load" guid="[!guid_group!]" name="group_name" email="name@company.com" />
    ///     </pre>
    /// </summary>
    public class Group : RedDotObject
    {
        public Group()
        {
            //Users = new List<User>();
        }

        /// <summary>
        ///     Reads group data from XML-Element "GROUP" like:
        ///     <pre>
        ///         <GROUP action="load" guid="[!guid_group!]" name="group_name"
        ///             email="name@company.com" />
        ///     </pre>
        /// </summary>
        /// <exception cref="FileDataException">Thrown if element doesn't contain valid data.</exception>
        /// <param name="xmlElement"> </param>
        public Group(XmlElement xmlElement) : base(xmlElement)
        {
            LoadXml();
        }

        public string Email { get; set; }

        // /// <summary>
        // ///     All users within a group. Set to "null" if not loaded yet. If there is no user in this group, Users is set to an empty list.
        // /// </summary>
        // public IEnumerable<User> Users { get; set; }

        private void LoadXml()
        {
            Email = XmlElement.GetAttributeValue("email");
        }
    }
}