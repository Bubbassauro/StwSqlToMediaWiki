select 
	case when g.[Ct] > 1 and isnull(d.[Namespace], '') <> ''
		then d.[Page] + '_' + replace(d.[Namespace], ' ', '_')
		else d.[Page]
	end "Name",
	[Revision],
	-- <mediawiki>
	(select 'en' as "@xml:lang",
		-- <page>
		(select
			-- Use the same title as the last revision to avoid creating multiple pages when the title changes
			case when g.[Ct] > 1 and isnull(d.[Namespace], '') <> ''
				then p.[Title] + ' (' + d.[Namespace] +')'
				else p.[Title]
			end "title",
			-- <revision>
			(select 
				[LastModified] "timestamp", 
				[User] "contributor/username",
				'preserve' as "text/@xml:space",
				-- Include categories at the top of the page content
				isnull((select '[[Category:' + Category + ']]' + char(10) as [text()] from 
				(
					-- Include namespace as a category
					select p.[Namespace] as Category
					union
					-- Page categories
					select Category
					from CategoryBinding b
					where b.[Page] = p.[Page] and p.[Namespace] = b.[Namespace]
					union
					-- Include keywords as categories as well
					select Keyword as Category					
					from PageKeyword as k
					where k.[Page] = p.[Page] and k.[Namespace] = p.[Namespace]
					and k.[Revision] = p.[Revision]
				) c
				for xml path('')), '') +
				-- Actual page content, replace syntax
				Content "text"
			from PageContent "page"
			where [Page] = p.[Page]
				and [Revision] = d.[Revision]
			for xml path('revision'), type, elements)
		from PageContent p
		where p.Revision = -1
			and p.[Page] = d.[Page]
		for xml path('page'), type, elements)
	for xml path('mediawiki')
	) as [Content]
from PageContent d left join 
	(select [Page], Count(distinct [Namespace]) as [Ct]
	from PageContent
	group by [Page]) g on d.[Page] = g.[Page]
order by Name, [LastModified] asc