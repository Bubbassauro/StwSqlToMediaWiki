using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Text.RegularExpressions;
using System.IO;
using System.Net;
using System.Collections.Specialized;
using System.Xml;
using System.Web;
using System.Diagnostics;

namespace StwSqlToMediaWiki
{
    public class Program
    {
        static void Main(string[] args)
        {
            string destination = ConfigurationManager.AppSettings["destination"];
            string attachmentsFolder = Path.Combine(destination, "attachments");
            string pagesFolder = Path.Combine(destination, "pages");

            // Files
            if (!Directory.Exists(attachmentsFolder))
            {
                Directory.CreateDirectory(attachmentsFolder);
                ExportFiles(attachmentsFolder);
            }
            else
            {
                Console.WriteLine("There is an attachments folder at " + destination + ". Please make sure the folder is empty.");
            }

            if (Directory.GetFiles(attachmentsFolder).Count() > 0)
            {
                ImportAttachmentsToMediaWiki(attachmentsFolder);
            }
            else
            {
                Console.WriteLine("No attachments to import.");
            }

            // Pages
            if (!Directory.Exists(pagesFolder))
            {
                Directory.CreateDirectory(pagesFolder);
                ExportPages(pagesFolder);
            }
            else
            {
                Console.WriteLine("There is a pages folder at " + destination + ". Please make sure the folder is empty.");
            }

            if (Directory.GetFiles(pagesFolder).Count() > 0)
            {
                ImportPagesToMediaWiki(pagesFolder);
            }
            else
            {
                Console.WriteLine("No pages to import.");
            }

            RebuildRecentChanges();
            Console.WriteLine("Finished.");
        }

        static void ExportFiles(string attachmentsFolder)
        {
            using (SqlConnection cnn = new SqlConnection(ConfigurationManager.ConnectionStrings["wiki"].ConnectionString))
            {
                var cmd = cnn.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = Scripts.ListAttachments;
                cmd.Connection.Open();
                var drPages = cmd.ExecuteReader();

                while (drPages.Read())
                {
                    string file = drPages["Name"].ToString();
                    string pageName = drPages["Page"].ToString();
                    byte[] data = (byte[])drPages["Data"];

                    string fileName = Path.Combine(attachmentsFolder, file);

                    Console.WriteLine("Processing file: " + fileName);
                    if (!File.Exists(fileName))
                    {
                        // Save file for import
                        FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write);
                        fs.Write(data, 0, data.Length);
                        fs.Position = 0;
                        fs.Close();
                    }
                    else
                    {
                        Console.WriteLine("File " + fileName + " already exists.");
                        Trace.WriteLine("File " + fileName + " already exists.");

                        string folder = Path.Combine(Path.GetDirectoryName(fileName), "failed", pageName.Replace("/", "_"));
                        if (!Directory.Exists(folder))
                            Directory.CreateDirectory(folder);

                        // Save file to import manually later
                        string failedFileName = Path.Combine(folder, Path.GetFileName(fileName));
                        FileStream fs = new FileStream(failedFileName, FileMode.Create, FileAccess.Write);
                        fs.Write(data, 0, data.Length);
                        fs.Position = 0;
                        fs.Close();
                    }
                }
                cmd.Connection.Close();
            }
        }

        static void ExportPages(string pagesFolder)
        {
            using (SqlConnection cnn = new SqlConnection(ConfigurationManager.ConnectionStrings["wiki"].ConnectionString))
            {
                // Check database version
                var versionCmd = cnn.CreateCommand();
                versionCmd.CommandText = "select Version from Version where Component = 'Pages'";
                versionCmd.Connection.Open();
                int versionNumber = int.Parse(versionCmd.ExecuteScalar().ToString());
                versionCmd.Connection.Close();

                var cmd = cnn.CreateCommand();
                cmd.CommandType = CommandType.Text;
                if (versionNumber >= 4000)
                {
                    cmd.CommandText = Scripts.ListPagesV4;
                }
                else
                {
                    cmd.CommandText = Scripts.ListPagesV3;
                }
                cmd.Connection.Open();
                var drPages = cmd.ExecuteReader();

                int pageNumber = 0;

                while (drPages.Read())
                {
                    pageNumber++;
                    string pageName = drPages["Name"].ToString();
                    int revision = int.Parse(drPages["Revision"].ToString());
                    string xml = drPages["Content"].ToString();

                    string fileName = Path.Combine(pagesFolder, String.Format("{0:000000000}_{1}.{2}.xml", pageNumber, pageName, revision));

                    Console.WriteLine("Processing file: " + fileName);
                    xml = TranslateMarkup(xml);

                    // Save xml for import
                    StreamWriter sw = new StreamWriter(fileName, false, Encoding.UTF8);
                    sw.Write(xml);
                    sw.Close();
                }
                cmd.Connection.Close();
            }
        }

        public static string TranslateMarkup(string xml)
        {
            string imageExtensions = ConfigurationManager.AppSettings["imageExtensions"];
            string documentExtensions = ConfigurationManager.AppSettings["documentExtensions"];

            // Remove special character for carriage return (to avoid errors on import)
            xml = xml.Replace("&#x0D;", "");

            // Cleanup link caret ^ from links (symbol to open in new window is not understoood by MediaWiki)
            xml = Regex.Replace(xml, @"\[[^]]*?\^[^]]*\]", m => m.Value.Replace("^", ""));

            // Replace anchors [anchor|#foo] becomes <div id="foo"/>
            xml = Regex.Replace(xml, @"\[anchor[^]#]+#(?<anchor>[^]]+)\]", "&lt;div id=\"${anchor}\"/&gt;");

            // Cleanup artificial line breaks
            xml = Regex.Replace(xml, @"{br}", "");

            // Cleanup table of contents (created automatically on mediawiki)
            xml = Regex.Replace(xml, @"{TOC}", "");

            // Cleanup namespace references
            xml = Regex.Replace(xml, @"\^*{UP[^}]*}", "");

            // Replace characters that cause the import to fail
            xml = Regex.Replace(xml, "[‘’]", "'");

            // Replace code sections @@code@@ becomes <pre>code</pre> for boxed code
            xml = Regex.Replace(xml, @"\@\@([^@]+)\@\@", "&lt;pre&gt;$1&lt;/pre&gt;");

            // Replace code sections {{code}} becomes <code>code</code> for inline code
            xml = Regex.Replace(xml, @"{{(?<code>.+?)}}", "&lt;code&gt;${code}&lt;/code&gt;");

            // Replace image references [image||myimage.jpg] becomes [[file:myimage.jpg]]
            xml = Regex.Replace(xml, @"\[image\w*\|\|([^]]+/)*(?<file>[^]/.]+\.\w{3,4})\]", m => "[[file:" + m.Groups["file"].Value.Replace(" ", "_") + "]]"); ;
            xml = Regex.Replace(xml, @"\[image\w*\|[^]]+[/|}](?<file>[^]/.]+\.\w{3,4})\]", m => "[[file:" + m.Groups["file"].Value.Replace(" ", "_") + "]]");

            // Replace other file references with descriptions
            xml = Regex.Replace(xml, @"(?<!\[)\[(?<file>[^]|.]+\.(doc|xls|mpp|pdf|ppt|docx|xlsx|pptx|ps|odt|ods|odp|odg|txt))\|(?<name>[^]]+)\](?!\])", m => "[[media:" + m.Groups["file"].Value.Replace(" ", "_").Substring(m.Groups["file"].Value.LastIndexOf("/") + 1) + "|" + m.Groups["name"].Value + "]]");

            // Other files without description
            xml = Regex.Replace(xml, @"(?<!\[)\[([^]]+/)*(?<file>[^]/.]+\.(doc|xls|mpp|pdf|ppt|docx|xlsx|pptx|ps|odt|ods|odp|odg|txt))\](?!\])", m => "[[media:" + m.Groups["file"].Value.Replace(" ", "_") + "]]");

            // UNC paths
            xml = Regex.Replace(xml, @"\[\^*(?<path>\\\\[^]|]+)\|(?<name>[^]]*)\]", @"{{unc | ${path} | ${name} }}");
            xml = Regex.Replace(xml, @"\[\^*(?<path>\\\\[^]|]+)\]", @"{{unc | ${path} | ${path} }}");

            // Page links with description
            xml = Regex.Replace(xml, @"(?<!\[)\[(?!http)([^].]+\.)*(?<page>[^]/}|]+)\|(?<name>[^]]+)\](?!\])", m => "[[" + m.Groups["page"].Value.Replace("-", "_") + "|" + m.Groups["name"].Value + "]]");

            // Page links without description
            xml = Regex.Replace(xml, @"(?<!\[)\[(?!http)(?<page>[^][|]+)\](?!\])", m => "[[" + m.Groups["page"].Value.Replace("-", "_") + "]]");

            // Links with description
            xml = Regex.Replace(xml, @"\[\^*(?<url>http[^]]+)\|(?<name>[^]]+)\]", "[${url} ${name}]");

            // Links without description
            xml = Regex.Replace(xml, @"\[\^*(?<url>http[^]]+)\]", "[${url}]");

            // Html links
            xml = Regex.Replace(xml, @"&lt;[^/]+?href=""(?<url>[^""]+)""[^/]*?&gt;(?<name>.*?)&lt;/[aA]&gt;", "[${url} ${name}]");

            // Box text (no box text in the MediaWiki default install, uses blockquote instead)
            xml = Regex.Replace(xml, @"\({3}(?<text>.*)?\){3}", "&lt;blockquote&gt;${text}&lt;/blockquote&gt;", RegexOptions.Singleline);

            return xml;
        }

        static void ImportPagesToMediaWiki(string pagesFolder)
        {
            string scriptFolder = ConfigurationManager.AppSettings["scriptFolder"];
            string phpExe = ConfigurationManager.AppSettings["phpExe"];

            foreach (var fileName in Directory.GetFiles(pagesFolder))
            {
                string arguments = String.Format("{0} \"{1}\"", Path.Combine(scriptFolder, "importDump.php"), fileName);
                Console.WriteLine(arguments);

                Process compiler = new Process();
                compiler.StartInfo.FileName = phpExe;
                compiler.StartInfo.Arguments = arguments;
                compiler.StartInfo.UseShellExecute = false;
                compiler.StartInfo.RedirectStandardOutput = true;
                compiler.Start();

                string result = compiler.StandardOutput.ReadToEnd();
                if (result.IndexOf("Done!") < 0)
                {
                    Trace.WriteLine("Error processing file " + fileName);
                    Trace.WriteLine(result);
                    MoveToSubfolder(fileName, "failed");
                }
                else
                {
                    MoveToSubfolder(fileName, "done");
                }
                Console.WriteLine(result);
                compiler.WaitForExit();
            }
        }

        static void ImportAttachmentsToMediaWiki(string attachmentsFolder)
        {
            string scriptFolder = ConfigurationManager.AppSettings["scriptFolder"];
            string phpExe = ConfigurationManager.AppSettings["phpExe"];
            string arguments = Path.Combine(scriptFolder, "importImages.php " + attachmentsFolder);

            Console.WriteLine("Bulk importing files...");
            Process compiler = new Process();
            compiler.StartInfo.FileName = phpExe;
            compiler.StartInfo.Arguments = arguments;
            compiler.StartInfo.UseShellExecute = false;
            compiler.StartInfo.RedirectStandardOutput = true;
            compiler.Start();

            string result = compiler.StandardOutput.ReadToEnd();
            Console.WriteLine(result);
            compiler.WaitForExit();
        }

        /// <summary>
        /// This is to make the imported pages appear on Special:RecentChanges
        /// </summary>
        static void RebuildRecentChanges()
        {
            string scriptFolder = ConfigurationManager.AppSettings["scriptFolder"];
            string phpExe = ConfigurationManager.AppSettings["phpExe"];
            string arguments = Path.Combine(scriptFolder, "rebuildRecentChanges.php");

            Process compiler = new Process();
            compiler.StartInfo.FileName = phpExe;
            compiler.StartInfo.Arguments = arguments;
            compiler.StartInfo.UseShellExecute = false;
            compiler.StartInfo.RedirectStandardOutput = true;
            compiler.Start();

            string result = compiler.StandardOutput.ReadToEnd();
            Console.WriteLine(result);
            compiler.WaitForExit();
        }

        /// <summary>
        /// Moves a file to a relative subfolder. Creates the subfolder if it doesn't exist
        /// </summary>
        /// <param name="fileName">Full file name</param>
        /// <param name="subfolderName">Subfolder name (partial)</param>
        static void MoveToSubfolder(string fileName, string subfolderName)
        {
            string subfolder = Path.Combine(Path.GetDirectoryName(fileName), subfolderName);
            if (!Directory.Exists(subfolder))
            {
                Directory.CreateDirectory(subfolder);
            }

            string destFileName = Path.Combine(subfolder, Path.GetFileName(fileName));
            File.Move(fileName, destFileName);
        }

    }
}
