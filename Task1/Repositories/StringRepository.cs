using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Task1.Data;
using Task1.DTOs;
using Task1.Models;
using static System.Net.Mime.MediaTypeNames;

namespace Task1.Repositories
{
    public class StringRepository
    {
        AppDbContext _appDbContext;
        private readonly HttpClient _httpClient;
        private readonly string _modelName;
        private readonly string _apiKey;
        //IMapper _mapper;
        public StringRepository(AppDbContext appDbContext, IConfiguration configuration)
        {
            _appDbContext = appDbContext;
            //_apiKey = configuration["HuggingFace:ApiKey"]
            //    ?? throw new InvalidOperationException("HuggingFace API Key not configured");

            //_modelName = configuration["HuggingFace:ModelName"] ?? "microsoft/DialoGPT-large";

            //_httpClient = new HttpClient
            //{
            //    BaseAddress = new Uri("https://api-inference.huggingface.co/")
            //};

            //_httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            //_mapper = mapper;
        }

         public bool GetConditionForFilter(StringData stringData, bool? is_palindrome = null, int? min_length = null,
    int? max_length = null, int? word_count = null, string? contains_character = null)
  {
      var condition = true;
      if (is_palindrome != null)
      {
          condition = condition && (bool)is_palindrome;
      }
      if(min_length != null)
      {
          condition = condition && stringData.Properties.Length >= (int)min_length;
      }
      if(max_length != null)
      {
          condition = condition && stringData.Properties.Length <= (int)max_length;
      }
      if(word_count != null)
      {
          condition = condition && stringData.Properties.Word_count == (int)word_count;
      }
      if(contains_character != null)
      {
          condition = condition && stringData.Value.Where(char.IsLetterOrDigit)
                .Distinct().ToList().Contains(Char.Parse((string)contains_character));
      }
      return condition;

  }
        public async Task<List<StringData>> GetStringDatasFromNaturalLanguage(Dictionary<string, object> filters)
        {
            var allStringDatas = await _appDbContext.StringDatas.Include(p => p.Properties).ThenInclude(p => p.Character_frequency_map_object).ToListAsync();
            var stringFiltered = allStringDatas.Where(p => GetStringsCondition(p, filters));
            return stringFiltered.ToList();
        }
        public bool GetStringsCondition(StringData stringData, Dictionary<string, object> filters)
        {
            var condition = true;
            foreach (var item in filters)
            {
                if (item.Key == "is_palindrome")
                {
                     condition = condition && IsPalindromeIgnoreCase(stringData.Value) && bool.Parse(item.Value.ToString());
                }
                if(item.Key == "min_length")
                {
                    condition = condition && stringData.Properties.Length >= int.Parse(item.Value.ToString());
                }
                if(item.Key == "max_length")
                {
                    condition = condition && stringData.Properties.Length <= int.Parse(item.Value.ToString());
                }
                if(item.Key == "word_count")
                {
                    condition = condition && stringData.Properties.Word_count == int.Parse(item.Value.ToString());
                }
                if(item.Key == "contains_character")
                {
                    condition = condition && stringData.Value.Where(char.IsLetterOrDigit)
                      .Distinct().ToList().Contains(Char.Parse(item.Value.ToString()));
                }
            }
            return condition;
        }
              public async Task<List<StringData>> GetStringsFiltered(bool? is_palindrome = null, int? min_length = null,
            int? max_length = null, int? word_count = null, string? contains_character = null)
        {

            var allStringDatas = await _appDbContext.StringDatas.Include(p => p.Properties).ThenInclude(p => p.Character_frequency_map_object).ToListAsync();
            var allStringsFiltered = allStringDatas.Where(p => GetConditionForFilter(p, is_palindrome, min_length,
                max_length, word_count, contains_character)).ToList();
            return allStringsFiltered;

        }
                public Dictionary<string, object> GetGlobalFilters(bool? is_palindrome = null, int? min_length = null,
        int? max_length = null, int? word_count = null, string? contains_character = null)
        {
            Dictionary<string, object> filters = new Dictionary<string, object>();
            if (is_palindrome != null)
            {
                filters.Add("is_palindrome", is_palindrome);
            }
            if (min_length != null)
            {
                filters.Add("min_length", min_length);
            }
            if(max_length != null)
            {
                filters.Add("max_length", max_length);
            }
            if(word_count != null)
            {
                filters.Add("word_count", word_count);
            }
            if(contains_character != null)
            {
                filters.Add("contains_character", contains_character);
            }
            return filters;
        }
     
           public FilteredStringsDto GetReturnStringsFiltered(List<StringData> stringDatas, Dictionary<string, object> filters)
       {
           var returnValues = stringDatas.Select(stringData =>
           {
               var returnValue = GetReturnCreateStringDto(stringData);
               return returnValue;
           }).ToList();
           var filteredStringsDto = new FilteredStringsDto
           {
               Data = returnValues,
               Count = returnValues.Count(),
               Filters_applied = filters
           };
           return filteredStringsDto;
       }
        public List<ReturnCreateStringDto> GetAllReturnValueDtos(List<StringData> stringDatas)
        {
            var returnValues = stringDatas.Select(stringData =>
            {
                var returnValue = GetReturnCreateStringDto(stringData);
                return returnValue;
            }).ToList();
            return returnValues;
        }
      

        public ReturnCreateStringDto GetReturnCreateStringDto(StringData stringData)
        {
            var returnCreateStringDto = new ReturnCreateStringDto
            {
                //Id = "sha256_hash_value",
                Id = stringData.Properties.Sha256_hash,
                Value = stringData.Value,
                Properties = new StringPropertyDto
                {
                    Length = stringData.Properties.Length,
                    Is_palindrome = stringData.Properties.Is_palindrome,
                    Unique_characters = stringData.Properties.Unique_characters,
                    Word_count = stringData.Properties.Word_count,
                    Sha256_hash = stringData.Properties.Sha256_hash,
                    Character_frequency_map = ConvertCharacterItemsToDictionary(GetOccurencesOfCharacters(stringData.Value))  
                }
                ,
                //_mapper.Map<StringPropertyDto>(stringData.Properties),
                Created_at = stringData.CreatedAt
            };
            return returnCreateStringDto;   
        }

        public Dictionary<string, int> ConvertCharacterItemsToDictionary(List<CharacterItem> characterItems)
        {
            Dictionary<string, int> keyValuePairs = new Dictionary<string, int>();
            foreach (CharacterItem characterItem in characterItems)
            {
                keyValuePairs.Add(characterItem.Character, characterItem.Occurence);
            }
            return keyValuePairs;
        }
        public async Task<bool> IsStringExists(string value)
        {
            var strings = await _appDbContext.StringDatas.ToListAsync();
            if(strings.FirstOrDefault(p => p.Value == value) == null )
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public StringData AssignStringData(string input)
        {
            var stringData = new StringData
            {
                Id = Guid.NewGuid(),
                Value = input,
                Properties = new StringProperty
                {
                    Id = Guid.NewGuid(),
                    Length = input.Count(char.IsLetterOrDigit),
                    Is_palindrome = IsPalindromeIgnoreCase(input),
                    Unique_characters = input.Where(char.IsLetterOrDigit)
                      .Distinct()
                      .Count(),
                    Word_count = CountWords(input),
                    Sha256_hash = ComputeSha256Hash(input),
                    Character_frequency_map_object = GetOccurencesOfCharacters(input),
                    Character_frequency_map = ConvertCharacterItemsToDictionary(GetOccurencesOfCharacters(input))
                }
            };
            return stringData;
        }
        public List<CharacterItem> GetOccurencesOfCharacters(string input)
        {
            var characterItems = new List<CharacterItem>();
            var characters = input.Where(char.IsLetterOrDigit).ToList();
            foreach (var character in characters)
            {
                if(characterItems.FirstOrDefault(p => p.Character == character.ToString()) == null)
                {
                    var newCharacterItem = new CharacterItem
                    {
                        Character = character.ToString(),
                        Occurence = 1
                    };
                    characterItems.Add(newCharacterItem);
                }
                else
                {
                    characterItems = characterItems.Select(p =>
                    {
                        if (p.Character == character.ToString())
                        {
                            p.Occurence++;
                        }
                        return p;
                    }).ToList();
                }

            }
            return characterItems;
        }
        public  string ComputeSha256Hash(string rawData)
    {
        // Create a SHA256
         using (SHA256 sha256Hash = SHA256.Create())
        {
            // ComputeHash - returns byte array
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

            // Convert byte array to a string
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2")); // x2 = hexadecimal format
            }
            return builder.ToString();
        }
    }
        public  bool IsPalindromeIgnoreCase(string input)
        {
            if (string.IsNullOrEmpty(input))
                return true;

            input = input.ToLower();
            int left = 0;
            int right = input.Length - 1;

            while (left < right)
            {
                if (input[left] != input[right])
                    return false;

                left++;
                right--;
            }

            return true;
        }
        public  int CountWords(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return 0;
            return Regex.Matches(input, @"[^\s]+").Count;
        }
    }
}
