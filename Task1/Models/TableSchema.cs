namespace Task1.Models
{
    public class TableSchema
    {
        public string TableName { get; set; }
        public string? Schema { get; set; }
        public List<TableColumn> Columns { get; set; } = new List<TableColumn>();
    }
}
