﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18408
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace StwSqlToMediaWiki {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Scripts {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Scripts() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("StwSqlToMediaWiki.Scripts", typeof(Scripts).Assembly);
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
        ///   Looks up a localized string similar to SELECT [Name], [Page], [Data]
        ///  FROM [Attachment]
        ///union
        ///select [Name], [Directory] as [Page], [Data]
        ///from [File]
        ///order by [Page], [Name].
        /// </summary>
        internal static string ListAttachments {
            get {
                return ResourceManager.GetString("ListAttachments", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to select 
        ///	case when g.[Ct] &gt; 1 and isnull(d.[Namespace], &apos;&apos;) &lt;&gt; &apos;&apos;
        ///		then d.[Page] + &apos;_&apos; + replace(d.[Namespace], &apos; &apos;, &apos;_&apos;)
        ///		else d.[Page]
        ///	end &quot;Name&quot;,
        ///	[Revision],
        ///	-- &lt;mediawiki&gt;
        ///	(select &apos;en&apos; as &quot;@xml:lang&quot;,
        ///		-- &lt;page&gt;
        ///		(select
        ///			case when g.[Ct] &gt; 1 and isnull(d.[Namespace], &apos;&apos;) &lt;&gt; &apos;&apos;
        ///				then d.[Title] + &apos; (&apos; + d.[Namespace] +&apos;)&apos;
        ///				else d.[Title]
        ///			end &quot;title&quot;,
        ///			-- &lt;revision&gt;
        ///			(select 
        ///				[LastModified] &quot;timestamp&quot;, 
        ///				[User] &quot;contributor/username&quot;,
        ///				&apos;preserve&apos; as &quot;tex [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string ListPagesV3 {
            get {
                return ResourceManager.GetString("ListPagesV3", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to select 
        ///	case when g.[Ct] &gt; 1 and isnull(d.[Namespace], &apos;&apos;) &lt;&gt; &apos;&apos;
        ///		then d.[Name] + &apos;_&apos; + replace(d.[Namespace], &apos; &apos;, &apos;_&apos;)
        ///		else d.[Name]
        ///	end &quot;Name&quot;,
        ///	[Revision],
        ///	-- &lt;mediawiki&gt;
        ///	(select &apos;en&apos; as &quot;@xml:lang&quot;,
        ///		-- &lt;page&gt;
        ///		(select
        ///			case when g.[Ct] &gt; 1 and isnull(d.[Namespace], &apos;&apos;) &lt;&gt; &apos;&apos;
        ///				then d.[Title] + &apos; (&apos; + d.[Namespace] +&apos;)&apos;
        ///				else d.[Title]
        ///			end &quot;title&quot;,
        ///			-- &lt;revision&gt;
        ///			(select 
        ///				[LastModified] &quot;timestamp&quot;, 
        ///				[User] &quot;contributor/username&quot;,
        ///				&apos;preserve&apos; as &quot;tex [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string ListPagesV4 {
            get {
                return ResourceManager.GetString("ListPagesV4", resourceCulture);
            }
        }
    }
}