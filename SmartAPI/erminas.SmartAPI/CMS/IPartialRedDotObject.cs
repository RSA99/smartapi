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
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml;
using erminas.SmartAPI.CMS.Project;
using erminas.SmartAPI.CMS.Project.ContentClasses;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS
{
    public interface IPartialRedDotObject : IRedDotObject
    {
        void EnsureInitialization();
        void Refresh();
    }

    public interface IPartialRedDotProjectObject : IPartialRedDotObject, IProjectObject
    {
    }

    //public interface ILanguageDependentPartialRedDotObject : IPartialRedDotProjectObject,
    //                                                         ILanguageDependentXmlBasedObject
    //{

    //}

    /// <summary>
    ///     Base class for all RedDotObject that can be initialized with a guid only and then retrieve other information on demand.
    /// </summary>
    /// <remarks>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 As the object can be partially initalized (guid only), the other properties have to
    ///                 call
    ///                 <see cref="LazyLoad{T}" />
    ///                 , to ensure complete initialization upon access.
    ///                 <example>
    ///                     <code>private string _description;
    ///             public override string Description { get { return LazyLoad(ref _description); } }</code>
    ///                 </example>
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 You have to initialize the attributes/properties in the LoadXml method, which is called
    ///                 by
    ///                 <see cref="LazyLoad{T}" />
    ///             </description>
    ///         </item>
    ///     </list>
    /// </remarks>
    internal abstract class PartialRedDotObject : RedDotObject, IPartialRedDotObject
    {
        /// <summary>
        ///     Create a new PartialRedDotObject and initialize it with an XmlNode. The object is completly initialized afterwards (
        ///     <see
        ///         cref="IsInitialized" />
        ///     )
        /// </summary>
        protected PartialRedDotObject(ISession session, XmlElement xmlElement) : base(session, xmlElement)
        {
            IsInitialized = true;
        }

        /// <summary>
        ///     Create a new PartialRedDotObject with a guid only The object is only partially initialized afterwards (
        ///     <see
        ///         cref="IsInitialized" />
        ///     )
        /// </summary>
        protected PartialRedDotObject(ISession session, Guid guid) : base(session, guid)
        {
            IsInitialized = false;
        }

        protected PartialRedDotObject(ISession session) : base(session)
        {
            IsInitialized = false;
        }

        /// <summary>
        ///     Indicates, wether the object is already completly initialized (true) or not (false).
        /// </summary>
        public bool IsInitialized { get; set; }

        /// <summary>
        ///     If the object or a variable already is initialized, returns the variable value, otherwise calls <code>LoadXml(RetrieveWholeObject())</code> to initialized the object. And returns the variable value afterwards;
        /// </summary>
        /// <typeparam name="T"> TypeId of the variable </typeparam>
        /// <param name="value"> variable </param>
        /// <returns> Value of the variable, after initialization was ensured </returns>
        protected T LazyLoad<T>(ref T value)
        {
            if (IsInitialized || !Equals(value, default(T)))
            {
                return value;
            }
            Refresh();
            return value;
        }

        protected abstract void LoadWholeObject();

        /// <summary>
        ///     Returns an XmlNode with which contains the complete information on this object. This gets called, if the object is only partially initialized and other information is needed.
        /// </summary>
        protected abstract XmlElement RetrieveWholeObject();

        #region IPartialRedDotObject Members

        public void EnsureInitialization()
        {
            if (!IsInitialized)
            {
                Refresh();
            }
        }

        public override sealed string Name
        {
            get { return LazyLoad(ref _name); }
            internal set { base.Name = value; }
        }

        public virtual void Refresh()
        {
            XmlElement = (XmlElement) RetrieveWholeObject().Clone();
            InitGuidAndName();
            LoadWholeObject();

            IsInitialized = true;
        }

        public override sealed XmlElement XmlElement
        {
            get
            {
                EnsureInitialization();
                return base.XmlElement;
            }
            protected internal set
            {
                base.XmlElement = value;
                _readWriteWrapper = new XmlReadWriteWrapper(_xmlElement, new Dictionary<string, string>());
            }
        }

        protected override sealed T GetAttributeValue<T>([CallerMemberName] string callerName = "")
        {
            EnsureInitialization();
// ReSharper disable ExplicitCallerInfoArgument
            return base.GetAttributeValue<T>(callerName);
// ReSharper restore ExplicitCallerInfoArgument
        }

        protected override sealed void SetAttributeValue<T>(T value, [CallerMemberName] string callerName = "")
        {
            EnsureInitialization();
// ReSharper disable ExplicitCallerInfoArgument
            base.SetAttributeValue(value, callerName);
// ReSharper restore ExplicitCallerInfoArgument
        }

        #endregion
    }

    internal abstract class LanguageDependentPartialRedDotProjectObject : PartialRedDotProjectObject,
                                                                          ILanguageDependentXmlBasedObject
    {
        //protected readonly Dictionary<string, object> _languageDependendValues = new Dictionary<string, object>();

        //private readonly Dictionary<string, XmlElement> _languageDependentWriteXmlElements =
        //    new Dictionary<string, XmlElement>();

        //private readonly Dictionary<string, XmlElement> _languageDependentXmlElements =
        //    new Dictionary<string, XmlElement>();

        private readonly Dictionary<string, XmlReadWriteWrapper> _languageDepdentXmlElements =
            new Dictionary<string, XmlReadWriteWrapper>();

        protected LanguageDependentPartialRedDotProjectObject(IProject project, XmlElement xmlElement)
            : base(project, xmlElement)
        {
        }

        protected LanguageDependentPartialRedDotProjectObject(IProject project, Guid guid) : base(project, guid)
        {
        }

        protected LanguageDependentPartialRedDotProjectObject(IProject project) : base(project)
        {
        }

        public IXmlReadWriteWrapper GetElementForLanguage(string languageAbbreviation)
        {
            var element = _languageDepdentXmlElements.GetOrAdd(languageAbbreviation, () =>
                {
                    using (new LanguageContext(Project.LanguageVariants[languageAbbreviation]))
                    {
                        var read = (XmlElement) RetrieveWholeObject().Clone();
                        return new XmlReadWriteWrapper(read, _readWriteWrapper.WrittenValues);
                    }
                });

            return element;
        }

        public override void Refresh()
        {
            _languageDepdentXmlElements.Clear();
            base.Refresh();
        }

        //public XmlElement XmlElementForCurrentLanguage
        //{
        //    get { return GetXmlElementForLanguage(Project.LanguageVariants.Current.Abbreviation); }
        //}

        //public XmlElement XmlElementForMainLanguage
        //{
        //    get { return GetXmlElementForLanguage(Project.LanguageVariants.Main.Abbreviation); }
        //}

        //private static XmlElement GetWriteMergedIntoRead(XmlElement writeElement, XmlElement readElement)
        //{
        //    var mergedElement = (XmlElement) readElement.Clone();
        //    foreach (XmlAttribute curAttribute in writeElement.Attributes)
        //    {
        //        mergedElement.SetAttributeValue(curAttribute.Name, curAttribute.Value);
        //    }

        //    return mergedElement;
        //}
    }
}