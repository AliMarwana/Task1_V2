namespace Task1.DTOs
{
    public class InterpretedQuery
    {
        public  string? Original { get; set; }
        public Dictionary<string, object>? ParsedFilters { get; set; }
    }
}
