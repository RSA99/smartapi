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
using System.Globalization;
using System.IO;
using System.Xml;
using erminas.SmartAPI.CMS;
using erminas.SmartAPI.Exceptions;

namespace erminas.SmartAPI.Utils
{
    public static class XmlUtil
    {
        /// <summary>
        ///     Creates an attribute via the owner document of <see cref="xmlElement" /> , sets its value and appends it to
        ///     <see
        ///         cref="xmlElement" />
        ///     .
        /// </summary>
        /// <param name="xmlElement"> The node, the attribute gets added to </param>
        /// <param name="attributeName"> Name of the attribute </param>
        /// <param name="value"> Value of the attribute </param>
        public static void AddAttribute(this XmlElement xmlElement, string attributeName, string value)
        {
            XmlAttribute attr = xmlElement.OwnerDocument.CreateAttribute(attributeName);
            attr.Value = value;
            xmlElement.Attributes.Append(attr);
        }

        /// <summary>
        ///     Creates an <see cref="XmlElement" /> and appends it as child to the XmlNode
        /// </summary>
        /// <param name="node"> The parent node </param>
        /// <param name="name"> Name of the newly created element </param>
        /// <returns> </returns>
        public static XmlElement AddElement(this XmlNode node, string name)
        {
            var doc = node as XmlDocument;
            doc = doc ?? node.OwnerDocument;
            XmlElement element = doc.CreateElement(name);
            node.AppendChild(element);
            return element;
        }

        /// <summary>
        ///     Gets the value of an attribute. If the attribute does not exists, null is returned.
        /// </summary>
        /// <param name="xmlElement"> The node </param>
        /// <param name="attributeName"> Name of the attribute </param>
        /// <returns> Value of the attribute, null, if attribute doesn't exist </returns>
        public static string GetAttributeValue(this XmlElement xmlElement, string attributeName)
        {
            XmlAttribute attr = xmlElement.Attributes[attributeName];
            return attr == null ? null : attr.Value;
        }

        public static bool? GetBoolAttributeValue(this XmlElement xmlElement, string attributeName)
        {
            int? value = xmlElement.GetIntAttributeValue(attributeName);
            if (value == null)
            {
                return null;
            }
            if (value == 1)
            {
                return true;
            }
            if (value == 0)
            {
                return false;
            }
            throw new SmartAPIException((ServerLogin) null,
                                        string.Format(
                                            "Could not convert value '{0}' of attribute '{1}' to a boolean value", value,
                                            attributeName));
        }

        public static double? GetDoubleAttributeValue(this XmlElement xmlElement, string attributeName)
        {
            XmlAttribute attr = xmlElement.Attributes[attributeName];
            return attr == null ? (double?) null : Double.Parse(attr.Value, CultureInfo.InvariantCulture);
        }

        public static Guid GetGuid(this XmlElement xmlElement)
        {
            return xmlElement.GetGuid("guid");
        }

        public static Guid GetGuid(this XmlElement xmlElement, String attributeName)
        {
            return Guid.Parse(xmlElement.GetAttributeValue(attributeName));
        }

        public static int? GetIntAttributeValue(this XmlElement xmlElement, string attributeName)
        {
            XmlAttribute attr = xmlElement.Attributes[attributeName];
            return attr == null ? (int?) null : int.Parse(attr.Value);
        }

        public static string GetName(this XmlElement xmlElement)
        {
            return xmlElement.GetAttributeValue("name");
        }

        public static DateTime? GetOADate(this XmlElement element, string attributeName = "date")
        {
            string strValue = element.GetAttributeValue(attributeName);
            if (String.IsNullOrEmpty(strValue))
            {
                return null;
            }

            return strValue.ToOADate();
        }

        public static XmlElement GetSingleElement(this XmlDocument doc, string tagName)
        {
            var nodes = doc.GetElementsByTagName(tagName);
            if (nodes.Count != 1)
            {
                throw new SmartAPIInternalException(
                    string.Format("Invalid number of {0} elements in XML reply from server. Expected: 1 actual: {1}",
                                  tagName, nodes.Count));
            }

            return (XmlElement) nodes[0];
        }

        public static bool IsAttributeSet(this XmlElement xmlElement, ISessionObject session, string attributeName)
        {
            var strValue = xmlElement.GetAttributeValue(attributeName);

            return !string.IsNullOrEmpty(strValue) && strValue != session.Session.SessionKey;
        }

        public static bool IsContainingOk(this XmlDocument xmlDoc)
        {
            return xmlDoc.InnerText.Contains("ok");
        }

        /// <summary>
        ///     Creates a string representation of an <see cref="XmlNode" />
        /// </summary>
        /// <param name="xmlElement"> The node </param>
        /// <returns>
        ///     string representation of <see cref="xmlElement" />
        /// </returns>
        public static string NodeToString(this XmlElement xmlElement)
        {
            var sw = new StringWriter();
            var xw = new XmlTextWriter(sw);
            xmlElement.WriteTo(xw);

            return sw.ToString();
        }

        /// <summary>
        ///     Sets an attribute to a value. If no fitting <see cref="XmlAttribute" /> exists, a new one is created/appended and its value set.
        /// </summary>
        /// <param name="xmlElement"> The node </param>
        /// <param name="attributeName"> Name of the attribute </param>
        /// <param name="value"> Value to set the attribute to </param>
        public static void SetAttributeValue(this XmlElement xmlElement, string attributeName, string value)
        {
            XmlAttribute attr = xmlElement.Attributes[attributeName];
            if (attr == null)
            {
                AddAttribute(xmlElement, attributeName, value);
            }
            else
            {
                attr.Value = value;
            }
        }

        public static DateTime ToOADate(this string value)
        {
            string valueNormalizedToInvariantCulture = value.Replace(",", ".");
            return DateTime.FromOADate(Double.Parse(valueNormalizedToInvariantCulture, CultureInfo.InvariantCulture));
        }

        public static bool TryGetGuid(this XmlElement xmlElement, out Guid guid)
        {
            return TryGetGuid(xmlElement, "guid", out guid);
        }

        public static bool TryGetGuid(this XmlElement xmlElement, string attributeName, out Guid guid)
        {
            string strValue = xmlElement.GetAttributeValue(attributeName);
            if (string.IsNullOrEmpty(strValue))
            {
                guid = Guid.Empty;
                return false;
            }

            return Guid.TryParse(strValue, out guid);
        }
    }
}