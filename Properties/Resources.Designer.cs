﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.5446
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace LH.Apps.RajceDownloader.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "2.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("LH.Apps.RajceDownloader.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to .
        /// </summary>
        internal static string Parser_EndTag {
            get {
                return ResourceManager.GetString("Parser_EndTag", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;div id=&quot;photoList&quot;&gt;.
        /// </summary>
        internal static string Parser_StartTag {
            get {
                return ResourceManager.GetString("Parser_StartTag", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to var storage\s*=\s*&quot;(?&lt;storage&gt;.*)&quot;\s*;.
        /// </summary>
        internal static string Parser_StorageRegex {
            get {
                return ResourceManager.GetString("Parser_StorageRegex", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Stahuji stránku alba....
        /// </summary>
        internal static string Status_DownloadingPage {
            get {
                return ResourceManager.GetString("Status_DownloadingPage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Připraven.
        /// </summary>
        internal static string Status_Ready {
            get {
                return ResourceManager.GetString("Status_Ready", resourceCulture);
            }
        }
    }
}
