namespace Task1.DTOs
{
    public class FilteredStringsDto
    {
        public List<ReturnCreateStringDto> Data { get; set; } = new List<ReturnCreateStringDto>();
        public int Count{ get; set; }
       public Dictionary<string, object>? Filters_applied { get; set; }

    }
}
