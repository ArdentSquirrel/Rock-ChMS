/* List DefinedTypeValues*/
SELECT
    t.Category, 
	t.Id as [Type ID],
    t.Name as [DefinedType.Name],
	v.Id as [Value ID],
    v.Name as Value, 
    v.Description,
    t.Guid [DefinedType.Guid],
    v.Guid [DefinedValue.Guid]
FROM            
    DefinedValue AS v 
INNER JOIN
    DefinedType AS t ON t.Id = v.DefinedTypeId
ORDER BY t.Category, [DefinedType.Name], Value