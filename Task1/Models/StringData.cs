namespace Task1.Models
{
    public class StringData
    {
        public Guid Id { get; set; }
        public string Value { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public StringProperty? Properties { get; set; } = new StringProperty();
    }
}
