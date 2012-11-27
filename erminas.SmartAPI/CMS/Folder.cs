﻿/*
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
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using erminas.SmartAPI.Utils;
using erminas.Utilities;

namespace erminas.SmartAPI.CMS
{
    public class Folder : PartialRedDotObject
    {
        #region ComparisonFileAttribute enum

        public enum ComparisonFileAttribute
        {
            Width,
            Heigth,
            Depth,
            Size
        }

        #endregion

        #region ComparisonOperator enum

        public enum ComparisonOperator
        {
            Equal,
            Less,
            Greater,
            LessEqual,
            GreaterEqual
        }

        #endregion

        /// <summary>
        ///   RQL for listing files for the folder with guid {0}. No parameters
        /// </summary>
        private const string LIST_FILE_ATTRIBUTES =
            @"<MEDIA><FOLDER guid=""{0}""><FILE sourcename=""{1}""><FILEATTRIBUTES action=""list""/></FILE></FOLDER></MEDIA>";

        /// <summary>
        ///   RQL for listing files for the folder with guid {0}. No parameters
        /// </summary>
        private const string LIST_FILES_IN_FOLDER =
            @"<PROJECT><MEDIA><FOLDER guid=""{0}"" subdirguid=""{0}""><FILES action=""list"" view=""thumbnail"" orderby=""name"" maxcount=""1000"" attributeguid="""" searchtext=""*"" /></FOLDER></MEDIA></PROJECT>";

        /// <summary>
        ///   RQL for listing files for the folder with guid {0}. No parameters
        /// </summary>
        private const string LIST_FILES_IN_FOLDER_PARTIAL =
            @"<PROJECT><MEDIA><FOLDER guid=""{0}"" subdirguid=""{0}""><FILES action=""list"" view=""thumbnail"" orderby=""name"" maxcount=""1000""  searchtext=""*"" pattern="""" startcount=""{1}"" sectioncount=""{2}""/></FOLDER></MEDIA></PROJECT>";

        /// <summary>
        ///   RQL for listing files for the folder with guid {0} and the filtertext {1}. No parameters
        /// </summary>
        private const string FILTER_FILES_BY_TEXT =
            @"<MEDIA><FOLDER  guid=""{0}""><FILES action=""list"" view=""thumbnail"" maxfilesize=""0""  searchtext=""{1}"" pattern="""" startcount=""1"" orderby=""name""/></FOLDER></MEDIA>";

        /// <summary>
        ///   RQL for listing files for the folder with guid {0} by the creator with guid {1}. No parameters
        /// </summary>
        private const string FILTER_FILES_BY_CREATOR =
            @"<MEDIA><FOLDER  guid=""{0}""><FILES action=""list"" view=""thumbnail"" maxfilesize=""0"" createguid=""{1}"" pattern="""" startcount=""1"" orderby=""name""/></FOLDER></MEDIA>";

        /// <summary>
        ///   RQL for listing files for the folder with guid {0} changed by a user with guid {1}. No parameters
        /// </summary>
        private const string FILTER_FILES_BY_CHANGEAUTHOR =
            @"<MEDIA><FOLDER  guid=""{0}""><FILES action=""list"" view=""thumbnail"" maxfilesize=""0"" changeguid=""{1}"" pattern="""" startcount=""1"" orderby=""name""/></FOLDER></MEDIA>";

        /// <summary>
        ///   RQL for listing files for the folder with guid {0} which match the command {1} with the operator {2} and value {3}. No parameters
        /// </summary>
        private const string FILTER_FILES_BY_COMMAND =
            @"<MEDIA><FOLDER  guid=""{0}"" ><FILES action=""list"" view=""thumbnail"" sectioncount=""30"" maxfilesize=""0""  command=""{1}"" op=""{2}"" value=""{3}""  startcount=""1"" orderby=""name""/></FOLDER></MEDIA>";

        /// <summary>
        ///   RQL for saving a file {1} in a folder {0}. IMPORTANT: For {1} Create a File by using String FILE_TO_SAVE to insert 1...n files and fill in required values No parameters
        /// </summary>
        private const string SAVE_FILES_IN_FOLDER =
            @"<MEDIA><FOLDER guid=""{0}"">{1}</FOLDER></MEDIA>";

        /// <summary>
        ///   RQL for a file to be saved. Has to be inserted in SAVE_FILES_IN_FOLDER. No parameters
        /// </summary>
        private const string FILE_TO_SAVE =
            @"<FILE action=""save"" sourcename=""{0}"" sourcepath=""{1}""/>";

        /// <summary>
        ///   RQL for updating files {0} from source in a folder. No parameters
        /// </summary>
        private const string UPDATE_FILES_IN_FOLDER =
            @"<MEDIA><FOLDER guid=""{0}"">{1}</FOLDER></MEDIA>";

        /// <summary>
        ///   RQL for a file to be updated. Has to be inserted in UPDATE_FILES_IN_FOLDER No parameters
        /// </summary>
        private const string FILE_TO_UPDATE =
            @"<FILE action=""update"" sourcename=""{0}""/>";


        /// <summary>
        ///   RQL for deleting files for the folder with guid {0}. {1} List of Files to be deleted. Can contain mor than one <FILE>
        /// </summary>
        private const string DELETE_FILES =
            @"<MEDIA><FOLDER guid=""{0}""><FILES action=""deletefiles"">{1}</FILES></FOLDER></MEDIA>";

        /// <summary>
        ///   RQL for a files for the folder with the sourcename {0} to be inserted in e.g. DELETE_FILES deletereal=0: Prior to deleting, a message is sent back if the file is already being used. (Default setting).
        /// </summary>
        private const string FILE_TO_DELETE_IF_UNUSED = @"<FILE deletereal=""0"" sourcename=""{0}""/>";

        /// <summary>
        ///   RQL for a files for the folder with the sourcename {0} to be inserted in e.g. DELETE_FILES deletereal=1: The file is deleted regardless of whether it is being used in a project or not.
        /// </summary>
        private const string FORCE_FILE_TO_BE_DELETED = @"<FILE deletereal=""1"" sourcename=""{0}""/>";


        private bool? _isAssetManagerFolder;
        private Folder _linkedFolder;
        private string _name;

        public Folder(Project project, XmlNode xmlNode)
            : base(xmlNode)
        {
            Project = project;
            LoadXml(xmlNode);
            Init();
        }

        public Folder(Project project, Guid guid)
            : base(guid)
        {
            Project = project;
            Init();
        }

        public override string Name
        {
            get { return LazyLoad(ref _name); }
        }

        public Project Project { get; set; }

        public bool IsAssetManagerFolder
        {
            get { return LazyLoad(ref _isAssetManagerFolder).Value; }
        }

        public Folder LinkedFolder
        {
            get { return LazyLoad(ref _linkedFolder); }
        }

        public ICachedList<File> AllFiles { get; private set; }

        private void Init()
        {
            AllFiles = new CachedList<File>(GetAllFiles, Caching.Enabled);
        }

        protected override void LoadXml(XmlNode xmlNode)
        {
            InitIfPresent(ref _name, "name", x => x);
            InitIfPresent(ref _isAssetManagerFolder, "catalog", x => x == "1");

            Guid linkedProjectGuid;
            if (xmlNode.TryGetGuid("linkedprojectguid", out linkedProjectGuid))
            {
                _linkedFolder = new Folder(Project.Session.Projects.GetByGuid(linkedProjectGuid),
                                           xmlNode.GetGuid("linkedfolderguid"));
            }
        }

        protected override XmlNode RetrieveWholeObject()
        {
            const string LOAD_FOLDER = @"<PROJECT><FOLDER action=""load"" guid=""{0}""/></PROJECT>";

            XmlDocument xmlDoc = Project.ExecuteRQL(String.Format(LOAD_FOLDER, Guid.ToRQLString()));
            XmlNodeList folders = xmlDoc.GetElementsByTagName("FOLDER");
            if (folders.Count != 1)
            {
                throw new Exception(String.Format("No folder with guid {0} found.", Guid.ToRQLString()));
            }
            return folders[0];
        }

        private List<File> RetrieveFiles(string rqlString)
        {
            XmlDocument xmlDoc = Project.ExecuteRQL(rqlString);
            XmlNodeList xmlNodes = xmlDoc.GetElementsByTagName("FILE");

            return (from XmlNode xmlNode in xmlNodes select new File(Project, xmlNode)).ToList();
        }

        public List<File> GetSubListOfFiles(int startCount, int fileCount)
        {
            string rqlString = String.Format(LIST_FILES_IN_FOLDER_PARTIAL, Guid.ToRQLString(), startCount, fileCount);

            return RetrieveFiles(rqlString);
        }

        public List<File> GetFilesByNamePattern(string searchText)
        {
            string rqlString = String.Format(FILTER_FILES_BY_TEXT, Guid.ToRQLString(), searchText);
            return RetrieveFiles(rqlString);
        }

        public List<File> GetFilesByAuthor(Guid authorGuid)
        {
            string rqlString = String.Format(FILTER_FILES_BY_CREATOR, Guid.ToRQLString(), authorGuid.ToRQLString());
            return RetrieveFiles(rqlString);
        }

        public List<File> GetFilesByLastModifier(Guid lastModifierGuid)
        {
            string rqlString = String.Format(FILTER_FILES_BY_CHANGEAUTHOR, Guid.ToRQLString(),
                                             lastModifierGuid.ToRQLString());
            return RetrieveFiles(rqlString);
        }

        /// <summary>
        ///   Returns List of files that match a predicate on an attribute
        /// </summary>
        /// <param name="attribute"> Attribute which values get checked in the predicate </param>
        /// <param name="operator"> Opreator e.g. "le" (less equal), "ge" (greater equal), "lt"(less than), "gt" (greater than) or "eq" (equal) </param>
        /// <param name="value"> Value e.g. 50 pixel/ 24 bit, etc. </param>
        /// <returns> </returns>
        public List<File> GetFilesByAttributeComparison(ComparisonFileAttribute attribute, ComparisonOperator @operator,
                                                        int value)
        {
            string rqlString = String.Format(FILTER_FILES_BY_COMMAND, Guid.ToRQLString(), AttributeToString(attribute),
                                             ComparisonOperatorToString(@operator),
                                             value);
            return RetrieveFiles(rqlString);
        }

        private object ComparisonOperatorToString(ComparisonOperator @operator)
        {
            switch (@operator)
            {
                case ComparisonOperator.Greater:
                    return "gt";
                case ComparisonOperator.Less:
                    return "lt";
                case ComparisonOperator.LessEqual:
                    return "le";
                case ComparisonOperator.GreaterEqual:
                    return "ge";
                case ComparisonOperator.Equal:
                    return "eq";
                default:
                    throw new ArgumentException(string.Format("Unknown comparison operator: {0}", @operator));
            }
        }

        public static string AttributeToString(ComparisonFileAttribute attribute)
        {
            switch (attribute)
            {
                case ComparisonFileAttribute.Width:
                    return "width";
                case ComparisonFileAttribute.Heigth:
                    return "height";
                case ComparisonFileAttribute.Size:
                    return "size";
                case ComparisonFileAttribute.Depth:
                    return "depth";
                default:
                    throw new ArgumentException(string.Format("Unknown file attribute: {0}", attribute));
            }
        }


        public FileAttribute FileInfos(String fileName)
        {
            XmlDocument xmlDoc = Project.ExecuteRQL(String.Format(LIST_FILE_ATTRIBUTES, Guid.ToRQLString(), fileName));
            XmlNode node = xmlDoc.GetElementsByTagName("EXTERNALATTRIBUTES")[0];
            return new FileAttribute(node);
        }

        public void SaveFiles(List<FileSource> sources)
        {
            var filesToSave = new List<string>();

            foreach (FileSource fileSource in sources)
            {
                string fileToUpload = string.Format(FILE_TO_SAVE, fileSource.Sourcename, fileSource.Sourcepath);
                filesToSave.Add(fileToUpload);
            }

            XmlDocument xmlDoc =
                Project.ExecuteRQL(String.Format(SAVE_FILES_IN_FOLDER, Guid.ToRQLString(),
                                                 string.Join(string.Empty, filesToSave)));
            XmlNodeList xmlNodes = xmlDoc.GetElementsByTagName("FILE");
            if (xmlNodes.Count == 0)
            {
                throw new ArgumentException("Could not save Files.");
            }
        }


        public void UpdateFiles(List<FileSource> files)
        {
            // Add 1..n file update Strings in UPDATE_FILES_IN_FOLDER string and execute RQL-Query
            var filesToUpdate = new List<string>();

            foreach (FileSource file in files)
            {
                string fileToUpload = string.Format(FILE_TO_UPDATE, file.Sourcename);
                filesToUpdate.Add(fileToUpload);
            }

            XmlDocument xmlDoc =
                Project.ExecuteRQL(string.Format(UPDATE_FILES_IN_FOLDER, Guid.ToRQLString(),
                                                 string.Join(string.Empty, filesToUpdate)));
            XmlNodeList xmlNodes = xmlDoc.GetElementsByTagName("FILE");
            if (xmlNodes.Count == 0)
            {
                throw new ArgumentException("Could not update Files.");
            }
        }

        // New Version to delete Files
        public void DeleteFiles(List<string> filenames, bool forceDelete)
        {
            // Add 1..n file update Strings in UPDATE_FILES_IN_FOLDER string and execute RQL-Query
            var filesToDelete = new List<string>();

            foreach (string filename in filenames)
            {
                string fileToUpload = string.Format(forceDelete ? FORCE_FILE_TO_BE_DELETED : FILE_TO_DELETE_IF_UNUSED,
                                                    filename);

                filesToDelete.Add(fileToUpload);
            }

            XmlDocument xmlDoc =
                Project.ExecuteRQL(string.Format(DELETE_FILES, Guid.ToRQLString(),
                                                 string.Join(string.Empty, filesToDelete)));
            XmlNodeList xmlNodes = xmlDoc.GetElementsByTagName("IODATA");
            if (xmlNodes.Count == 0)
            {
                throw new ArgumentException("Could not delete Files.");
            }
        }

        private List<File> GetAllFiles()
        {
            string rqlString = String.Format(LIST_FILES_IN_FOLDER, Guid.ToRQLString());

            return RetrieveFiles(rqlString);
        }

        #region Nested type: FileSource

        public class FileSource
        {
            public string Sourcename;
            public string Sourcepath;

            public FileSource(string sourcename, string sourcepath)
            {
                Sourcename = sourcename;
                Sourcepath = sourcepath;
            }
        }

        #endregion
    }
}