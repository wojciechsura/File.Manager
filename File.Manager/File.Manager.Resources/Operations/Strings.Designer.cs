﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Ten kod został wygenerowany przez narzędzie.
//     Wersja wykonawcza:4.0.30319.42000
//
//     Zmiany w tym pliku mogą spowodować nieprawidłowe zachowanie i zostaną utracone, jeśli
//     kod zostanie ponownie wygenerowany.
// </auto-generated>
//------------------------------------------------------------------------------

namespace File.Manager.Resources.Operations {
    using System;
    
    
    /// <summary>
    ///   Klasa zasobu wymagająca zdefiniowania typu do wyszukiwania zlokalizowanych ciągów itd.
    /// </summary>
    // Ta klasa została automatycznie wygenerowana za pomocą klasy StronglyTypedResourceBuilder
    // przez narzędzie, takie jak ResGen lub Visual Studio.
    // Aby dodać lub usunąć składową, edytuj plik ResX, a następnie ponownie uruchom narzędzie ResGen
    // z opcją /str lub ponownie utwórz projekt VS.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Strings {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Strings() {
        }
        
        /// <summary>
        /// Zwraca buforowane wystąpienie ResourceManager używane przez tę klasę.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("File.Manager.Resources.Operations.Strings", typeof(Strings).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Przesłania właściwość CurrentUICulture bieżącego wątku dla wszystkich
        ///   przypadków przeszukiwania zasobów za pomocą tej klasy zasobów wymagającej zdefiniowania typu.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        /// Wyszukuje zlokalizowany ciąg podobny do ciągu Copied files: {0} ({1}). Elapsed: {2}{3}.
        /// </summary>
        public static string CopyMove_Info_PartialDescription {
            get {
                return ResourceManager.GetString("CopyMove_Info_PartialDescription", resourceCulture);
            }
        }
        
        /// <summary>
        /// Wyszukuje zlokalizowany ciąg podobny do ciągu Copied files: {0} / {1} ({2} / {3}). Elapsed: {4}, left: {5}{6}.
        /// </summary>
        public static string CopyMove_Info_TotalDescription {
            get {
                return ResourceManager.GetString("CopyMove_Info_TotalDescription", resourceCulture);
            }
        }
        
        /// <summary>
        /// Wyszukuje zlokalizowany ciąg podobny do ciągu Cannot change attributes of the destination file &quot;{2}&quot;..
        /// </summary>
        public static string CopyMove_Question_CannotChangeFileAttributes {
            get {
                return ResourceManager.GetString("CopyMove_Question_CannotChangeFileAttributes", resourceCulture);
            }
        }
        
        /// <summary>
        /// Wyszukuje zlokalizowany ciąg podobny do ciągu Cannot check if file &quot;{2}&quot; exists in location &quot;{1}&quot;..
        /// </summary>
        public static string CopyMove_Question_CannotCheckIfDestinationFileExists {
            get {
                return ResourceManager.GetString("CopyMove_Question_CannotCheckIfDestinationFileExists", resourceCulture);
            }
        }
        
        /// <summary>
        /// Wyszukuje zlokalizowany ciąg podobny do ciągu Cannot check if folder &quot;{2}&quot; exists in location &quot;{1}&quot;..
        /// </summary>
        public static string CopyMove_Question_CannotCheckIfDestinationFolderExists {
            get {
                return ResourceManager.GetString("CopyMove_Question_CannotCheckIfDestinationFolderExists", resourceCulture);
            }
        }
        
        /// <summary>
        /// Wyszukuje zlokalizowany ciąg podobny do ciągu Cannot check if file &quot;{2}&quot; exists in location &quot;{0}&quot;..
        /// </summary>
        public static string CopyMove_Question_CannotCheckIfSourceFileExists {
            get {
                return ResourceManager.GetString("CopyMove_Question_CannotCheckIfSourceFileExists", resourceCulture);
            }
        }
        
        /// <summary>
        /// Wyszukuje zlokalizowany ciąg podobny do ciągu Cannot check if folder &quot;{2}&quot; exists in location &quot;{0}&quot;..
        /// </summary>
        public static string CopyMove_Question_CannotCheckIfSourceFolderExists {
            get {
                return ResourceManager.GetString("CopyMove_Question_CannotCheckIfSourceFolderExists", resourceCulture);
            }
        }
        
        /// <summary>
        /// Wyszukuje zlokalizowany ciąg podobny do ciągu Cannot check if subfolder &quot;{2}&quot; in location &quot;{0}&quot; is empty..
        /// </summary>
        public static string CopyMove_Question_CannotCheckIfSubfolderIsEmpty {
            get {
                return ResourceManager.GetString("CopyMove_Question_CannotCheckIfSubfolderIsEmpty", resourceCulture);
            }
        }
        
        /// <summary>
        /// Wyszukuje zlokalizowany ciąg podobny do ciągu Cannot close destination stream for file &quot;{2}&quot; in location &quot;{1}&quot;..
        /// </summary>
        public static string CopyMove_Question_CannotCloseDestinationStream {
            get {
                return ResourceManager.GetString("CopyMove_Question_CannotCloseDestinationStream", resourceCulture);
            }
        }
        
        /// <summary>
        /// Wyszukuje zlokalizowany ciąg podobny do ciągu Cannot close source stream for file &quot;{2}&quot; in location &quot;{0}&quot;..
        /// </summary>
        public static string CopyMove_Question_CannotCloseSourceStream {
            get {
                return ResourceManager.GetString("CopyMove_Question_CannotCloseSourceStream", resourceCulture);
            }
        }
        
        /// <summary>
        /// Wyszukuje zlokalizowany ciąg podobny do ciągu Cannot copy file &quot;{0}&quot; from &quot;{1}&quot; to &quot;{2}&quot;..
        /// </summary>
        public static string CopyMove_Question_CannotCopyFile {
            get {
                return ResourceManager.GetString("CopyMove_Question_CannotCopyFile", resourceCulture);
            }
        }
        
        /// <summary>
        /// Wyszukuje zlokalizowany ciąg podobny do ciągu Cannot create folder &quot;{2}&quot; in location &quot;{1}&quot;..
        /// </summary>
        public static string CopyMove_Question_CannotCreateDestinationFolder {
            get {
                return ResourceManager.GetString("CopyMove_Question_CannotCreateDestinationFolder", resourceCulture);
            }
        }
        
        /// <summary>
        /// Wyszukuje zlokalizowany ciąg podobny do ciągu Cannot delete folder &quot;{2}&quot; in location &quot;{0}&quot;..
        /// </summary>
        public static string CopyMove_Question_CannotDeleteEmptyFolder {
            get {
                return ResourceManager.GetString("CopyMove_Question_CannotDeleteEmptyFolder", resourceCulture);
            }
        }
        
        /// <summary>
        /// Wyszukuje zlokalizowany ciąg podobny do ciągu Cannot open folder &quot;{2}&quot; in location &quot;{1}&quot;..
        /// </summary>
        public static string CopyMove_Question_CannotEnterDestinationFolder {
            get {
                return ResourceManager.GetString("CopyMove_Question_CannotEnterDestinationFolder", resourceCulture);
            }
        }
        
        /// <summary>
        /// Wyszukuje zlokalizowany ciąg podobny do ciągu Cannot open folde &quot;{2}&quot; in location &quot;{0}&quot;..
        /// </summary>
        public static string CopyMove_Question_CannotEnterSourceFolder {
            get {
                return ResourceManager.GetString("CopyMove_Question_CannotEnterSourceFolder", resourceCulture);
            }
        }
        
        /// <summary>
        /// Wyszukuje zlokalizowany ciąg podobny do ciągu Cannot get attributes of file &quot;{2}&quot; in location &quot;{0}&quot;..
        /// </summary>
        public static string CopyMove_Question_CannotGetSourceFileAttributes {
            get {
                return ResourceManager.GetString("CopyMove_Question_CannotGetSourceFileAttributes", resourceCulture);
            }
        }
        
        /// <summary>
        /// Wyszukuje zlokalizowany ciąg podobny do ciągu Cannot get attributes of file &quot;{2}&quot; in location &quot;{1}&quot;..
        /// </summary>
        public static string CopyMove_Question_CannotGetTargetFileAttributes {
            get {
                return ResourceManager.GetString("CopyMove_Question_CannotGetTargetFileAttributes", resourceCulture);
            }
        }
        
        /// <summary>
        /// Wyszukuje zlokalizowany ciąg podobny do ciągu Cannot list source folder contents: &quot;{0}&quot;.
        /// </summary>
        public static string CopyMove_Question_CannotListSourceFolder {
            get {
                return ResourceManager.GetString("CopyMove_Question_CannotListSourceFolder", resourceCulture);
            }
        }
        
        /// <summary>
        /// Wyszukuje zlokalizowany ciąg podobny do ciągu Cannot open destination file &quot;{2}&quot; in location &quot;{0}&quot;..
        /// </summary>
        public static string CopyMove_Question_CannotOpenDestinationFile {
            get {
                return ResourceManager.GetString("CopyMove_Question_CannotOpenDestinationFile", resourceCulture);
            }
        }
        
        /// <summary>
        /// Wyszukuje zlokalizowany ciąg podobny do ciągu Cannot open source file &quot;{2}&quot; in location &quot;{0}&quot;..
        /// </summary>
        public static string CopyMove_Question_CannotOpenSourceFile {
            get {
                return ResourceManager.GetString("CopyMove_Question_CannotOpenSourceFile", resourceCulture);
            }
        }
        
        /// <summary>
        /// Wyszukuje zlokalizowany ciąg podobny do ciągu Cannot delete source file &quot;{0}&quot; from location &quot;{1}&quot;..
        /// </summary>
        public static string CopyMove_Question_CannotRemoveSourceFile {
            get {
                return ResourceManager.GetString("CopyMove_Question_CannotRemoveSourceFile", resourceCulture);
            }
        }
        
        /// <summary>
        /// Wyszukuje zlokalizowany ciąg podobny do ciągu Cannot set attributes for file &quot;{2}&quot; in location &quot;{1}&quot;..
        /// </summary>
        public static string CopyMove_Question_CannotSetTargetFileAttributes {
            get {
                return ResourceManager.GetString("CopyMove_Question_CannotSetTargetFileAttributes", resourceCulture);
            }
        }
        
        /// <summary>
        /// Wyszukuje zlokalizowany ciąg podobny do ciągu Destination file &quot;{2}&quot; already exists in location &quot;{1}&quot;..
        /// </summary>
        public static string CopyMove_Question_DestinationFileAlreadyExists {
            get {
                return ResourceManager.GetString("CopyMove_Question_DestinationFileAlreadyExists", resourceCulture);
            }
        }
        
        /// <summary>
        /// Wyszukuje zlokalizowany ciąg podobny do ciągu Destination file &quot;{2}&quot; is read only..
        /// </summary>
        public static string CopyMove_Question_DestinationFileIsReadOnly {
            get {
                return ResourceManager.GetString("CopyMove_Question_DestinationFileIsReadOnly", resourceCulture);
            }
        }
        
        /// <summary>
        /// Wyszukuje zlokalizowany ciąg podobny do ciągu Destination file &quot;{2}&quot; is a system file..
        /// </summary>
        public static string CopyMove_Question_DestinationFileIsSystem {
            get {
                return ResourceManager.GetString("CopyMove_Question_DestinationFileIsSystem", resourceCulture);
            }
        }
        
        /// <summary>
        /// Wyszukuje zlokalizowany ciąg podobny do ciągu Cannot copy file &quot;{0}&quot;: location &quot;{1}&quot; already contains folder with the same name..
        /// </summary>
        public static string CopyMove_Question_DestinationFolderAlreadyExists {
            get {
                return ResourceManager.GetString("CopyMove_Question_DestinationFolderAlreadyExists", resourceCulture);
            }
        }
        
        /// <summary>
        /// Wyszukuje zlokalizowany ciąg podobny do ciągu You try to copy or move a file into itself..
        /// </summary>
        public static string CopyMove_Question_FileCopiedIntoItself {
            get {
                return ResourceManager.GetString("CopyMove_Question_FileCopiedIntoItself", resourceCulture);
            }
        }
        
        /// <summary>
        /// Wyszukuje zlokalizowany ciąg podobny do ciągu You try to copy or move a folder into itself..
        /// </summary>
        public static string CopyMove_Question_FolderCopiedIntoItself {
            get {
                return ResourceManager.GetString("CopyMove_Question_FolderCopiedIntoItself", resourceCulture);
            }
        }
        
        /// <summary>
        /// Wyszukuje zlokalizowany ciąg podobny do ciągu Renamed file name &quot;{2}&quot; is invalid..
        /// </summary>
        public static string CopyMove_Question_InvalidRenamedFilename {
            get {
                return ResourceManager.GetString("CopyMove_Question_InvalidRenamedFilename", resourceCulture);
            }
        }
        
        /// <summary>
        /// Wyszukuje zlokalizowany ciąg podobny do ciągu Source file &quot;{2}&quot; does not exist in location &quot;{0}&quot;..
        /// </summary>
        public static string CopyMove_Question_SourceFileDoesNotExist {
            get {
                return ResourceManager.GetString("CopyMove_Question_SourceFileDoesNotExist", resourceCulture);
            }
        }
        
        /// <summary>
        /// Wyszukuje zlokalizowany ciąg podobny do ciągu Copying files.
        /// </summary>
        public static string CopyMove_Title_CopyingFiles {
            get {
                return ResourceManager.GetString("CopyMove_Title_CopyingFiles", resourceCulture);
            }
        }
        
        /// <summary>
        /// Wyszukuje zlokalizowany ciąg podobny do ciągu Moving files.
        /// </summary>
        public static string CopyMove_Title_MovingFiles {
            get {
                return ResourceManager.GetString("CopyMove_Title_MovingFiles", resourceCulture);
            }
        }
        
        /// <summary>
        /// Wyszukuje zlokalizowany ciąg podobny do ciągu Deleted files: {0} ({1}). Elapsed: {2}.
        /// </summary>
        public static string Delete_Info_PartialDescription {
            get {
                return ResourceManager.GetString("Delete_Info_PartialDescription", resourceCulture);
            }
        }
        
        /// <summary>
        /// Wyszukuje zlokalizowany ciąg podobny do ciągu Deleted files: {0} / {1} ({2} / {3}). Elapsed: {4}, left: {5}{6}.
        /// </summary>
        public static string Delete_Info_TotalDescription {
            get {
                return ResourceManager.GetString("Delete_Info_TotalDescription", resourceCulture);
            }
        }
        
        /// <summary>
        /// Wyszukuje zlokalizowany ciąg podobny do ciągu Cannot check if file &quot;{1}&quot; exists in location &quot;{0}&quot;..
        /// </summary>
        public static string Delete_Question_CannotCheckIfFileExists {
            get {
                return ResourceManager.GetString("Delete_Question_CannotCheckIfFileExists", resourceCulture);
            }
        }
        
        /// <summary>
        /// Wyszukuje zlokalizowany ciąg podobny do ciągu Cannot check if subfolder &quot;{1}&quot; in location &quot;{0}&quot; is empty..
        /// </summary>
        public static string Delete_Question_CannotCheckIfSubfolderIsEmpty {
            get {
                return ResourceManager.GetString("Delete_Question_CannotCheckIfSubfolderIsEmpty", resourceCulture);
            }
        }
        
        /// <summary>
        /// Wyszukuje zlokalizowany ciąg podobny do ciągu Cannot delete file &quot;{1}&quot; in location &quot;{0}&quot;..
        /// </summary>
        public static string Delete_Question_CannotDeleteFile {
            get {
                return ResourceManager.GetString("Delete_Question_CannotDeleteFile", resourceCulture);
            }
        }
        
        /// <summary>
        /// Wyszukuje zlokalizowany ciąg podobny do ciągu Cannot delete folder &quot;{1}&quot; in location &quot;{0}&quot;..
        /// </summary>
        public static string Delete_Question_CannotDeleteFolder {
            get {
                return ResourceManager.GetString("Delete_Question_CannotDeleteFolder", resourceCulture);
            }
        }
        
        /// <summary>
        /// Wyszukuje zlokalizowany ciąg podobny do ciągu Cannot open folder &quot;{1}&quot; in location &quot;{0}&quot;..
        /// </summary>
        public static string Delete_Question_CannotEnterFolder {
            get {
                return ResourceManager.GetString("Delete_Question_CannotEnterFolder", resourceCulture);
            }
        }
        
        /// <summary>
        /// Wyszukuje zlokalizowany ciąg podobny do ciągu Cannot get attributes of file &quot;{1}&quot; in location &quot;{0}&quot;..
        /// </summary>
        public static string Delete_Question_CannotGetFileAttributes {
            get {
                return ResourceManager.GetString("Delete_Question_CannotGetFileAttributes", resourceCulture);
            }
        }
        
        /// <summary>
        /// Wyszukuje zlokalizowany ciąg podobny do ciągu Cannot list folder contents: &quot;{0}&quot;.
        /// </summary>
        public static string Delete_Question_CannotListFolderContents {
            get {
                return ResourceManager.GetString("Delete_Question_CannotListFolderContents", resourceCulture);
            }
        }
        
        /// <summary>
        /// Wyszukuje zlokalizowany ciąg podobny do ciągu Cannot set attributes of file &quot;{1}&quot; in location &quot;{0}&quot;..
        /// </summary>
        public static string Delete_Question_CannotSetFileAttributes {
            get {
                return ResourceManager.GetString("Delete_Question_CannotSetFileAttributes", resourceCulture);
            }
        }
        
        /// <summary>
        /// Wyszukuje zlokalizowany ciąg podobny do ciągu File &quot;{1}&quot; in location &quot;{0}&quot; is hidden..
        /// </summary>
        public static string Delete_Question_DeletedFileIsHidden {
            get {
                return ResourceManager.GetString("Delete_Question_DeletedFileIsHidden", resourceCulture);
            }
        }
        
        /// <summary>
        /// Wyszukuje zlokalizowany ciąg podobny do ciągu File &quot;{1}&quot; in location &quot;{0}&quot; is read only..
        /// </summary>
        public static string Delete_Question_DeletedFileIsReadOnly {
            get {
                return ResourceManager.GetString("Delete_Question_DeletedFileIsReadOnly", resourceCulture);
            }
        }
        
        /// <summary>
        /// Wyszukuje zlokalizowany ciąg podobny do ciągu File &quot;{1}&quot; in location &quot;{0}&quot; is system..
        /// </summary>
        public static string Delete_Question_DeletedFileIsSystem {
            get {
                return ResourceManager.GetString("Delete_Question_DeletedFileIsSystem", resourceCulture);
            }
        }
        
        /// <summary>
        /// Wyszukuje zlokalizowany ciąg podobny do ciągu File &quot;{1}&quot; is no longer in location &quot;{0}&quot;..
        /// </summary>
        public static string Delete_Question_FileDoesNotExist {
            get {
                return ResourceManager.GetString("Delete_Question_FileDoesNotExist", resourceCulture);
            }
        }
    }
}
