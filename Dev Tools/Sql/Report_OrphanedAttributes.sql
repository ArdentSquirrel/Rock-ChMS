-- report orphaned block attributes
declare cAttribs cursor for 
select distinct EntityTypeQualifierColumn [ColumnName], SUBSTRING(EntityTypeQualifierColumn, 1, len(EntityTypeQualifierColumn)-2) [TableName]
from attribute where isnull(EntityTypeQualifierColumn, '') like '%Id'

declare
  @columnName nvarchar(max),
  @tableName nvarchar(max),
  @statement nvarchar(max)

set @statement = '
SELECT 
    [a].[Id], a.EntityTypeQualifierColumn into #orphanedAttributes
FROM 
    [Attribute] [a]
  where 1=0'

open cAttribs
fetch next from cAttribs into @columnName, @tableName

while @@FETCH_STATUS = 0
begin
  set @statement += 'or
  (  a.EntityTypeQualifierColumn = ''' + @columnName + '''
  and 
    [a].[EntityTypeQualifierValue] not in (select Id from [' + @tableName + '])
  )'

  fetch next from cAttribs into @columnName, @tableName
end

--CLOSE cAttribs;
DEALLOCATE cAttribs;

set @statement += '
select * from Attribute where Id in (select id from #orphanedAttributes)
select * from AttributeQualifier where AttributeId in (select id from #orphanedAttributes)
select * from AttributeValue where AttributeId in (select id from #orphanedAttributes)
'

execute sp_executesql @statement 



