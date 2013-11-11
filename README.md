StwSqlToMediaWiki
=================

This command line application imports pages from a ScrewTurnWiki SQL database to MediaWiki. If you used ScrewTurnWiki with the file system provider, take a look at https://github.com/Cyberitas/ScrewturnToMediawiki

It works with both ScrewTurnWiki v3 and v4 databases.

This is a simple application that:

1. Reads all attachments from the SQL database to a folder for import
2. Uses the MediaWiki php maintenance script to import all attachments to MediaWiki
3. Reads all pages and revisions from the ScrewTurnWiki database
4. Translates some of the markup from ScrewTurnWiki to MediaWiki
5. Saves each page revision as an XML file in a format that MediaWiki can import
6. Imports each page revision into MediaWiki using maintenance scripts

There are significant differences between the way pages are structured in ScrewTurnWiki and MediaWiki, for more information: 

Important notes
---------------
* The pages are imported with the current title. Previous revisions are also imported with the same title (even if it changed over time)  otherwise MediaWiki will think that the revisions with a different title are a different page.
* The application does not import to different namespaces, if a page exists in two or more namespaces it will be imported with the namespace in parentheses. The namespace is also added as a category tag to the page.
* Categories and keywords are added with the special [[Category:]] tag on the page body. Categories appear on the page footer and you can easily create an index for each category.

Before importing, check the following MediaWiki settings
--------------------------------------------------------

If you are importing images and attachments, make sure you disable caching in MediaWiki (mediawiki\LocalSettings.php) before running the import as the caching mechanism may prevent the import process from running properly.
For example, comment the caching settings before you run the import:

	## Shared memory settings
	#$wgMainCacheType = CACHE_ACCEL;
	#$wgMemCachedServers = array();

You also must specify the file types that can be uploaded to MediaWiki, by adding the accepted file extensions to LocalSettings.php:

	$wgFileExtensions = array('png','gif','jpg','jpeg','doc','xls','mpp','pdf','ppt','tiff','bmp','docx', 'xlsx', 'pptx','ps','odt','ods','odp','odg');

Make sure the images folder is not read-only and that the execution account has permission to write to mediawiki\images folder.

How to Use
----------

Get the code

	 git clone https://github.com/Bubbassauro/StwSqlToMediaWiki.git

Update the .config file

* destination: Where XML files will be saved for import
* phpExe: Location of your PHP installation
* scriptFolder: Location of your MediaWiki installation
* connectionString to read from your ScrewTurnWiki database

Run

	StwSqlToMediaWiki

Markup Differences
------------------

These are some of the main markup differences I had to translate. This does not cover all the markup differences and after migration you may need more manual adjustments or you can add your own rules to the method TranslateMarkup in Program.cs

<table>
<tbody>
<tr>
<td></td>
<td><strong>ScrewTurnWiki</strong></td>
<td><strong>MediaWiki</strong></td>
<td><strong>Notes</strong></td>
</tr>
<tr>
<td>Images</td>
<td>[image||image.jpg]</td>
<td>[[file:image.jpg]]</td>
<td></td>
</tr>
<tr>
<td>Downloadable documents/media files</td>
<td>[myfile.doc||My File]</td>
<td>[[media:myfile.doc|My File]]</td>
<td>Spaces in the file name should be replaced with underscores because MediaWiki does that by default when you upload a file.</td>
</tr>
<tr>
<td>External links with description</td>
<td>[<a href="http://hellolink">http://hellolink</a> | Hello Link]</td>
<td>[<a href="http://hellolink">http://hellolink</a> Hello Link]</td>
<td>Space between link and description instead of pipe.</td>
</tr>
<tr>
<td>Wiki links</td>
<td>[Another-Page]</td>
<td>[[Another Page]]</td>
<td>Page names in Media Wiki have spaces separating the words instead of dashes.</td>
</tr>
<tr>
<td>UNC paths</td>
<td>[\\my_network_share | Share]</td>
<td>{{ unc | \\my_share | Share }}</td>
<td>This one is a bit tricky, and requires enabling an extension and setting up a template. Please refer to <a href="http://www.mediawiki.org/wiki/UNC_links">http://www.mediawiki.org/wiki/UNC_links</a></td>
</tr>
<tr>
<td></td>
<td></td>
<td></td>
<td>Tag is not applicable, just use normal line breaks</td>
</tr>
<tr>
<td>Boxed code sections</td>
<td>@@my code@@</td>
<td>&lt;pre&gt;my code&lt;/pre&gt;</td>
<td></td>
</tr>
<tr>
<td>Inline code sections</td>
<td>{{my code}}</td>
<td>&lt;code&gt;my code&lt;/code&gt;</td>
<td></td>
</tr>
<tr>
<td>Boxed text</td>
<td>(((Some text in
a box)))</td>
<td>&lt;blockquote&gt;Some text in
a box&lt;/blockquote&gt;</td>
<td>No box on MediaWiki default install</td>
</tr>
</tbody>
</table>


Things that MediaWiki does not understand (so the script removes them):

<table></colgroup>
<tbody>
<tr>
<td>
Line breaks
</td>
<td>
{br}
</td>
<td>
Tag is not applicable, just use normal line breaks
</td>
</tr>
<tr>
<td>
Open link in new window
</td>
<td>
^
</td>
<td>
MediaWiki does not understand this notation and has a setting to open external links in a new window: <a href="http://www.mediawiki.org/wiki/Manual:Opening_external_links_in_a_new_window">http://www.mediawiki.org/wiki/Manual:Opening_external_links_in_a_new_window</a>
</td>
</tr>
<tr>
<td>
Table of contents
</td>
<td>
{TOC}
</td>
<td>
MediaWiki automatically includes a Table of Contents at the top of each page, linking to each header section (no need to use the {TOC} tag)
</td>
</tr>
<tr>
<td>
Parent reference
</td>
<td>
{UP}
</td>
<td>
The script removes namespace references and {UP} links to a different namespace, leaving just the link to the page.
</td>
</tr>
</tbody>
</table>