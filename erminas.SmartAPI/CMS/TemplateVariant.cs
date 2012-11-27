/*
 * Smart API - .Net programatical access to RedDot servers
 * Copyright (C) 2012  erminas GbR 
 *
 * This program is free software: you can redistribute it and/or modify it 
 * under the terms of the GNU General Public License as published by the Free Software Foundation,
 * either version 3 of the License, or (at your option) any later version.
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the GNU General Public License for more details. 
 *
 * You should have received a copy of the GNU General Public License along with this program.
 * If not, see <http://www.gnu.org/licenses/>. 
 */

using System;
using System.Web;
using System.Xml;
using erminas.SmartAPI.Utils;
using erminas.Utilities;

namespace erminas.SmartAPI.CMS
{
    //TODO templatevariant auf attributes umstellen
    /// <summary>
    ///   Represents a single template on the RedDot server
    /// </summary>
    public class TemplateVariant : PartialRedDotObject
    {
        #region State enum

        /// <summary>
        ///   State of the template.
        /// </summary>
        public enum State
        {
            Draft,
            WaitsForRelease,
            Released
        }

        #endregion

        private DateTime _changeDate;
        private User _changeUser;
        private User _createUser;
        private DateTime _creationDate;
        private string _data;
        private string _description;
        private string _fileExtension;
        private bool? _hasContainerPageReference;
        private bool? _isLocked;
        private bool? _isStylesheetIncluded;
        private string _name;
        private bool? _noStartEndMarkers;
        private string _pdfOrientation;
        private State? _status;


        public TemplateVariant(ContentClass contentClass, Guid guid)
            : base(guid)
        {
            ContentClass = contentClass;
        }

        public TemplateVariant(ContentClass contentClass, XmlNode xmlNode)
            : base(xmlNode)
        {
            ContentClass = contentClass;
            LoadXml(xmlNode);
        }

        //TODO mit reddotobjecthandle ersetzen
        public TemplateVariantHandle Handle
        {
            get { return new TemplateVariantHandle {Name = Name, Guid = Guid}; }
        }

        /// <summary>
        ///   Name of the template
        /// </summary>
        public override string Name
        {
            get { return LazyLoad(ref _name); }
            set { _name = value; }
        }

        public bool HasContainerPageReference
        {
            get { return LazyLoad(ref _hasContainerPageReference).Value; }
        }

        /// <summary>
        ///   Content data of the template (template text)
        /// </summary>
        public string Data
        {
            get { return LazyLoad(ref _data); }
            set
            {
                const string SAVE_DATA =
                    @"<TEMPLATE action=""save"" guid=""{0}""><TEMPLATEVARIANT guid=""{1}"">{2}</TEMPLATEVARIANT></TEMPLATE>";
                XmlDocument result =
                    ContentClass.Project.ExecuteRQL(
                        String.Format(SAVE_DATA, ContentClass.Guid.ToRQLString(), Guid.ToRQLString(),
                                      HttpUtility.HtmlEncode(value)),
                        Project.RqlType.SessionKeyInProject);
                if (!result.DocumentElement.InnerText.Contains(ContentClass.Guid.ToRQLString()))
                {
                    var e =
                        new Exception("Could not save templatevariant '" + Name + "' for content class '" +
                                      ContentClass.Name);
                    e.Data.Add("query_result", result);
                }
                _data = value;
            }
        }

        /// <summary>
        ///   Timestamp of the last change to the template
        /// </summary>
        public DateTime LastChangeDate
        {
            get { return LazyLoad(ref _changeDate); }
        }

        /// <summary>
        ///   Timestamp of the creation of the template
        /// </summary>
        public DateTime CreationDate
        {
            get { return LazyLoad(ref _creationDate); }
        }

        /// <summary>
        ///   User who created the template
        /// </summary>
        public User CreationUser
        {
            get { return LazyLoad(ref _createUser); }
        }

        /// <summary>
        ///   User who last changed the template
        /// </summary>
        public User LastChangeUser
        {
            get { return LazyLoad(ref _changeUser); }
        }

        /// <summary>
        ///   Current release status of the template
        /// </summary>
        public State ReleaseStatus
        {
            get { return LazyLoad(ref _status).Value; }
        }

        /// <summary>
        ///   Denoting whether or not a stylesheet should be automatically built into the header area of a page.
        /// </summary>
        public bool IsStylesheetIncludedInHeader
        {
            get { return LazyLoad(ref _isStylesheetIncluded).Value; }
        }

        /// <summary>
        /// </summary>
        public bool ContainsAreaMarksInPage
        {
            get { return !LazyLoad(ref _noStartEndMarkers).Value; }
        }

        /// <summary>
        ///   Description of the template
        /// </summary>
        public string Description
        {
            get { return LazyLoad(ref _description); }
        }

        public string FileExtension
        {
            get { return LazyLoad(ref _fileExtension); }
        }

        //TODO implement as enum
        public string PDFOrientation
        {
            get { return LazyLoad(ref _pdfOrientation); }
            set { _pdfOrientation = value; }
        }

        public bool IsLocked
        {
            get { return LazyLoad(ref _isLocked).Value; }
        }

        public ContentClass ContentClass { get; private set; }

        /// <summary>
        ///   Assign this template to a specific project variant
        /// </summary>
        public void AssignToProjectVariant(ProjectVariant variant, bool doNotPublish, bool doNotUseTidy)
        {
            const string ASSIGN_PROJECT_VARIANT =
                @"<TEMPLATE guid=""{0}""><TEMPLATEVARIANTS> <TEMPLATEVARIANT guid=""{1}"">
                                                    <PROJECTVARIANTS action=""assign""><PROJECTVARIANT donotgenerate=""{3}"" donotusetidy=""{4}"" guid=""{2}"" />
                                                    </PROJECTVARIANTS></TEMPLATEVARIANT></TEMPLATEVARIANTS></TEMPLATE>";

            ContentClass.Project.ExecuteRQL(string.Format(ASSIGN_PROJECT_VARIANT, ContentClass.Guid.ToRQLString(),
                                                          Guid.ToRQLString(), variant.Guid.ToRQLString(),
                                                          doNotPublish.ToRQLString(),
                                                          doNotUseTidy.ToRQLString()));
        }

        protected override void LoadXml(XmlNode node)
        {
            var element = ((XmlElement) node);
            if (!String.IsNullOrEmpty(element.InnerText))
            {
                _data = element.InnerText;
            }
            InitIfPresent(ref _creationDate, "createdate", DateTimeConvert);
            InitIfPresent(ref _changeDate, "changeddate", DateTimeConvert);
            InitIfPresent(ref _description, "description", x => x);
            InitIfPresent(ref _createUser, "createuserguid",
                          x =>
                          new User(ContentClass.Project.Session.CmsClient, Guid.Parse(x))
                              {Name = node.GetAttributeValue("createusername")});
            InitIfPresent(ref _changeUser, "changeduserguid",
                          x =>
                          new User(ContentClass.Project.Session.CmsClient, Guid.Parse(x))
                              {Name = node.GetAttributeValue("changedusername")});
            InitIfPresent(ref _name, "name", x => x);
            InitIfPresent(ref _fileExtension, "fileextension", x => x);
            InitIfPresent(ref _pdfOrientation, "pdforientation", x => x);
            InitIfPresent(ref _isStylesheetIncluded, "insertstylesheetinpage", NullableBoolConvert);
            InitIfPresent(ref _noStartEndMarkers, "nostartendmarkers", NullableBoolConvert);
            InitIfPresent(ref _isLocked, "lock", NullableBoolConvert);
            InitIfPresent(ref _hasContainerPageReference, "containerpagereference", NullableBoolConvert);

            if (BoolConvert(node.GetAttributeValue("draft")))
            {
                _status = State.Draft;
            }
            else
            {
                _status = BoolConvert(node.GetAttributeValue("waitforrelease")) ? State.WaitsForRelease : State.Released;
            }
        }

        /// <summary>
        ///   Copy this template over to another content class
        /// </summary>
        /// <param name="target"> </param>
        public void CopyToContentClass(ContentClass target)
        {
            const string ADD_TEMPLATE_VARIANT =
                @"<TEMPLATE action=""assign"" guid=""{0}"">
                    <TEMPLATEVARIANTS action=""addnew"">
                        <TEMPLATEVARIANT name=""{1}"" description=""{2}"" code=""{3}"" fileextension=""{4}"" insertstylesheetinpage=""{5}"" nostartendmarkers=""{6}"" containerpagereference=""{7}""  pdforientation=""{8}"">
                        {3}
                        </TEMPLATEVARIANT>
                    </TEMPLATEVARIANTS>
                </TEMPLATE>";
            XmlDocument xmlDoc =
                target.Project.ExecuteRQL(
                    string.Format(ADD_TEMPLATE_VARIANT, target.Guid.ToRQLString(), HttpUtility.HtmlEncode(Name),
                                  HttpUtility.HtmlEncode(Description), HttpUtility.HtmlEncode(Data),
                                  HttpUtility.HtmlEncode(FileExtension),
                                  IsStylesheetIncludedInHeader.ToRQLString(),
                                  ContainsAreaMarksInPage.ToRQLString(),
                                  HasContainerPageReference.ToRQLString(), PDFOrientation),
                    Project.RqlType.SessionKeyInProject);
            if (xmlDoc.DocumentElement.InnerText.Trim().Length == 0)
            {
                return;
            }
            string errorMsg = string.Format("Error during addition of template variant '{0}' to content class '{1}'.",
                                            Name, target.Name);
            //sometimes it's <IODATA><ERROR>Reason</ERROR></IODATA> and sometimes just <IODATA>ERROR</IODATA>
            XmlNodeList errorElements = xmlDoc.GetElementsByTagName("ERROR");
            if (errorElements.Count > 0)
            {
                throw new Exception(errorMsg + string.Format(" Reason: {0}.", errorElements[0].FirstChild.Value));
            }
            throw new Exception(errorMsg);
        }

        protected override XmlNode RetrieveWholeObject()
        {
            const string LOAD_TEMPLATEVARIANT =
                @"<TEMPLATE><TEMPLATEVARIANT action=""load"" readonly=""1"" guid=""{0}"" /></TEMPLATE>";
            XmlDocument xmlDoc = ContentClass.Project.ExecuteRQL(string.Format(LOAD_TEMPLATEVARIANT, Guid.ToRQLString()));

            return xmlDoc.GetElementsByTagName("TEMPLATEVARIANT")[0];
        }

        #region Nested type: TemplateVariantHandle

        public struct TemplateVariantHandle
        {
            public Guid Guid;
            public string Name;
        }

        #endregion
    }
}