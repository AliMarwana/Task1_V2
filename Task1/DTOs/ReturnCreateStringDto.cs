using Task1.Models;

namespace Task1.DTOs
{
    public class ReturnCreateStringDto
    {
        public string Id { get; set; }
        public string Value { get; set; }
        public DateTime Created_at { get; set; } = DateTime.Now;
        public StringPropertyDto? Properties { get; set; }
    }
}
