using System.ComponentModel.DataAnnotations.Schema;
using Task1.Models;

namespace Task1.DTOs
{
    public class StringPropertyDto
    {
        public int Length { get; set; }
        public bool Is_palindrome { get; set; }
        public int Unique_characters { get; set; }
        public int Word_count { get; set; }
        public string Sha256_hash { get; set; }
        public Dictionary<string, int> Character_frequency_map { get; set; } = new Dictionary<string, int>();
    }
}
