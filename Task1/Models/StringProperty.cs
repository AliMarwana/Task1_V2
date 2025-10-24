using System.ComponentModel.DataAnnotations.Schema;

namespace Task1.Models
{
    public class StringProperty
    {
        public Guid Id  { get; set; }
        public int Length{ get; set; }
        public bool Is_palindrome { get; set; }
        public int Unique_characters { get; set; }
        public int Word_count { get; set; }
        public string Sha256_hash{ get; set; }
        public List<CharacterItem> Character_frequency_map_object { get; set; } = new List<CharacterItem>();
        [NotMapped]
        public Dictionary<string, int> Character_frequency_map { get; set; } = new Dictionary<string, int>();
      
    }
}
