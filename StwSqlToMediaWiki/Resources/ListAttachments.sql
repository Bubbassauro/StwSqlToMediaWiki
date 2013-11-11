SELECT [Name], [Page], [Data]
  FROM [Attachment]
union
select [Name], [Directory] as [Page], [Data]
from [File]
order by [Page], [Name]