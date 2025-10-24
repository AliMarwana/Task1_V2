using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using System.Formats.Asn1;
using System.Net;
using System.Reflection;
using Task1.Data;
using Task1.DTOs;
using Task1.Filters;
using Task1.Models;
using Task1.Repositories;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Task1.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class StringsController : ControllerBase
    {
        StringRepository _stringRepository;
        SchemaRepository _schemaRepository;
        AppDbContext _appDbContext;
        private readonly Filters.SqlQueryGenerator _sqlGenerator;
        public StringsController(StringRepository stringRepository, SchemaRepository schemaRepository, AppDbContext appDbContext, SqlQueryGenerator sqlQueryGenerator)
        {
            _stringRepository = stringRepository;
            _appDbContext = appDbContext;
            _sqlGenerator = sqlQueryGenerator;
            _schemaRepository = schemaRepository;
        }

        [HttpPost]
        public async Task<IActionResult> CreateString([FromBody]CreateStringDto stringDto)
        {
            try
            {
                if (String.IsNullOrEmpty(stringDto.Value))
                {
                    return BadRequest("Invalid request body or missing \"value\" field");
                }
                var isStringInDb = await _stringRepository.IsStringExists(stringDto.Value);
                if (isStringInDb)
                {
                    return Conflict("String already exists in the system");
                }
                if (stringDto.Value.GetType() != typeof(string))
                {
                    return UnprocessableEntity(new ProblemDetails
                    {
                        Title = "Invalid data type",
                        Detail = "The 'Name' field must be a string.",
                        Status = 422,
                        Instance = HttpContext.Request.Path

                    });
                }

                var stringDataCreated = _stringRepository.AssignStringData(stringDto.Value);
                await _appDbContext.StringDatas.AddAsync(stringDataCreated);
                await _appDbContext.SaveChangesAsync();
               var returnCreateStringDto = _stringRepository.GetReturnCreateStringDto(stringDataCreated);
                return CreatedAtAction("CreateString", new { id = returnCreateStringDto.Id }, returnCreateStringDto);
            }
            catch (Exception ex)
            {
                return BadRequest("Invalid request body or missing \"value\" field");
            }


        }

        [HttpGet("{value}")]
        public async Task<IActionResult> GetString(string value)
        {
            var stringVarDb = await _appDbContext.StringDatas
                .Include(p => p.Properties)
                .ThenInclude(p => p.Character_frequency_map_object)
                .FirstOrDefaultAsync(p => p.Value == value);
            if (stringVarDb == null)
            {
                return NotFound("String does not exist in the system");
              
            }
            else
            {
                var response = _stringRepository.GetReturnCreateStringDto(stringVarDb);
                return Ok(response);
            }
        }

        [HttpGet("{is_palindrome}&{min_length}&{max_length}&{word_count}&{contains_character}")]
        public async Task<IActionResult> GetAllStringsWithFiltering(bool is_palindrome, 
            int min_length, int max_length, int word_count, string contains_character)
        {
            try
            {
                var stringDatas = await _stringRepository.GetStringsFiltered(is_palindrome, min_length, max_length, word_count, contains_character);
                var filteredReturnStrings = _stringRepository.GetReturnStringsFiltered(stringDatas, is_palindrome, min_length,
                    max_length, word_count, contains_character);
                return Ok(filteredReturnStrings);
            }
            catch (Exception ex)
            {
                return BadRequest("Invalid query parameter values or types");
            }

        }

        [HttpGet("filter-by-natural-language")]
        public async Task<IActionResult> GetStringsFromNaturalLanguage([FromQuery]string query)
        {
            //var canBeParsed = await _sqlGenerator.CanParseQueryAsync(naturalLanguage, _schemaRepository.GetTableSchema<StringData>());
          
            //if(!canBeParsed)
            //{
            //    return BadRequest("Unable to parse natural language query");
            //}
            //var hasConficlingFilters = await _sqlGenerator.HasConflictingFiltersAsync(naturalLanguage, _schemaRepository.GetTableSchema<StringData>());
            //if(hasConficlingFilters)
            //{
            //    return UnprocessableEntity(new ProblemDetails
            //    {
            //        Title = "Unprocessable Entity",
            //        Detail = "Query parsed but resulted in conflicting filters",
            //        Status = 422,
            //        Instance = HttpContext.Request.Path

            //    });
            //}
            var filters = await _sqlGenerator.GenerateSqlQueryAsync(
             query, "StringDatas", _schemaRepository.GetTableSchema<StringData>());
            if (filters.Values.Count() > 0)
            {
                if (filters.Keys.Contains("error"))
                {
                    var value = filters.GetValueOrDefault("error");
                    if (value == "400")
                    {
                        return BadRequest("Unable to parse natural language query");
                    }
                    if (value == "422")
                    {
                        return UnprocessableEntity(new ProblemDetails
                        {
                            Title = "Unprocessable Entity",
                            Detail = "Query parsed but resulted in conflicting filters",
                            Status = 422,
                            Instance = HttpContext.Request.Path

                        });
                    }
                }
            }
            var stringFiltered = await _stringRepository.GetStringDatasFromNaturalLanguage(filters);
            var stringsToReturnFiltered = _stringRepository.GetAllReturnValueDtos(stringFiltered);
            var dataToReturn = new DataNaturalLanguageDto
            {
                Data = stringsToReturnFiltered,
                Count = stringsToReturnFiltered.Count(),
                InterpretedQuery = new InterpretedQuery
                {
                    Original = query,
                    ParsedFilters = filters
                }
            };
            //var stringDatas = await _appDbContext.StringDatas
            //   .FromSqlRaw(sql)
            //   .ToListAsync();
            
            return Ok(dataToReturn);
        }

        [HttpDelete("{string_value}")]
        public async Task<IActionResult> DeleteString(string string_value)
        {
            var stringValue = await _appDbContext.StringDatas.
                Include(p => p.Properties).
                FirstOrDefaultAsync(p => p.Value == string_value)
                ;
  
            if(stringValue == null)
            {
                return NotFound("String does not exist in the system");
            }
            else
            {
                _appDbContext.Remove(stringValue);
                await _appDbContext.SaveChangesAsync();
                return NoContent();
            }
        }
    }
}

