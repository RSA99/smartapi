// Smart API - .Net programmatic access to RedDot servers
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

namespace erminas.SmartAPI.CMS.CCElements.Attributes
{
    public class SyllableXmlNodeAttribute : AbstractGuidXmlNodeAttribute<Syllable>
    {
        public SyllableXmlNodeAttribute(ContentClass element, string name)
            : base(element.Project.Session, element, name)
        {
        }

        protected override string GetTypeDescription()
        {
            return "prefix/suffix";
        }

        protected override Syllable RetrieveByGuid(Guid guid)
        {
            return ((ContentClass) Parent).Project.Syllables.GetByGuid(guid);
        }

        protected override Syllable RetrieveByName(string name)
        {
            return ((ContentClass) Parent).Project.Syllables[name];
        }
    }
}