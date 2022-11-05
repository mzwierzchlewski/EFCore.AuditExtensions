namespace EFCore.AuditExtensions.Common.SharedModels;

public class AuditedEntityKeyProperty
{
    public string ColumnName { get; }
        
    public AuditColumnType ColumnType { get; }
    
    public int? MaxLength { get; }
        
    public AuditedEntityKeyProperty(string columnName, AuditColumnType columnType, int? maxLength = null)
    {
        ColumnName = columnName;
        ColumnType = columnType;
        MaxLength = maxLength;
    }
}