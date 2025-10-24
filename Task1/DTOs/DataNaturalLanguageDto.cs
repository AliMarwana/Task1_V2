namespace Task1.DTOs
{
    public class DataNaturalLanguageDto
    {
        public List<ReturnCreateStringDto> Data { get; set; } = new List<ReturnCreateStringDto>();
        public int Count { get; set; }

        public InterpretedQuery? InterpretedQuery { get; set; }
    }
}
