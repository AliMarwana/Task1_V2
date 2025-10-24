namespace Task1.Filters
{
    using Microsoft.SemanticKernel;
    using Microsoft.SemanticKernel.Connectors.OpenAI;
    using System.Text.Json;
    using Task1.Models;

    public class SqlQueryGenerator
    {
        private readonly Kernel _kernel;

        public SqlQueryGenerator(Kernel kernel)
        {
            _kernel = kernel;
        }


        public async Task<bool> HasConflictingFiltersAsync(string sqlQuery, TableSchema tableSchema)
        {
            var prompt = $"""
        Analyze this SQL query for conflicting filter conditions.
        Return ONLY "true" if conflicts exist, "false" if no conflicts.
        
        Table Schema:
        {FormatSchemaForPrompt(tableSchema)}
        
        SQL Query: {sqlQuery}
        
        Look for:
        - Contradictory conditions on same column (Price > 100 AND Price < 50)
        - Mutually exclusive filters
        - Impossible combinations
        Answer:
        """;
            var result = await _kernel.InvokePromptAsync(prompt);
            var response = result.ToString().Trim().ToLower();

            return response == "true" || response == "yes";
        }

        public async Task<bool> CanParseQueryAsync(string naturalLanguageQuery, TableSchema tableSchema)
        {
            try
            {
                var prompt = $"""
            Can this natural language query be converted to SQL WHERE clauses for the given table?
            Answer with ONLY "true" or "false" - no explanations.

            Table Schema:
            {FormatSchemaForPrompt(tableSchema)}

            Query: "{naturalLanguageQuery}"

            Answer:
            """;

                var result = await _kernel.InvokePromptAsync(prompt);
                var response = result.ToString().Trim().ToLower();

                return response == "true" || response == "yes";
            
            }
            catch(Exception ex)
            {
                return false;
            }
            }
             private string FormatSchemaForPrompt(TableSchema schema)
        {
            return string.Join(", ", schema.Columns.Select(c => $"{c.Name} ({c.Type})"));
        }

        public async Task<Dictionary<string, object>> GenerateFiltersAsync(string naturalLanguageQuery)
        {
            var prompt = $$"""
        Convert the natural language query into a dictionary of filters for string analysis.
        c
        """;

            try
            {
                var result = await _kernel.InvokePromptAsync(prompt);
                var json = result.ToString().Trim();

                // Parse the JSON into a dictionary
                var filters = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                return filters ?? new Dictionary<string, object>();
            }
            catch (Exception)
            {
                return new Dictionary<string, object>();
            }
        }
        public async Task<Dictionary<string, object>> GenerateSqlQueryAsync(
            string naturalLanguageQuery,
            string tableName,
            TableSchema tableSchema,
            string operation = "SELECT")
        {
            var prompt = $$"""
       
        
        Table Name: {{tableName}}
        
        Table Schema:
        {{FormatTableSchema(tableSchema)}}
        
        Natural Language Query: "{{naturalLanguageQuery}}"
        
        
        Query: "{{naturalLanguageQuery}}"
        
        Available filters:
        - is_palindrome: boolean (true/false)
        - min_length: integer (minimum string length)
        - max_length: integer (maximum string length) 
        - word_count: integer (exact word count)
        - min_word_count: integer (minimum word count)
        - max_word_count: integer (maximum word count)
        - contains_character: string (single character to contain)
        - contains_string: string (substring to contain)
        - starts_with: string (prefix)
        - ends_with: string (suffix)
        
        Return ONLY a JSON object with the filters. If no filters apply, return empty object {}.
        
        Examples:
        Input: "all single word palindromic strings" 
        Output: {"word_count": 1, "is_palindrome": true}
        
        Input: "strings longer than 10 characters"
        Output: {"min_length": 11}
        
        Input: "palindromic strings that contain the first vowel"
        Output: {"is_palindrome": true, "contains_character": "a"}
        
        Input: "strings containing the letter z"
        Output: {"contains_character": "z"}
        
        Input: "words between 5 and 10 characters with exactly 2 words"
        Output: {"min_length": 5, "max_length": 10, "word_count": 2}
        
        Input: "strings that start with hello and end with world"
        Output: {"starts_with": "hello", "ends_with": "world"}
        
        Now convert the query by giving the filters only in output like the exampls I gave you.
        Don't add anything else in your answer.
        If you are unable to parse the query, send {"error": 400}.
        If the query parsed but resulted in conflicting filters, send {"error": 422}.
        
       
        """;

          
        try
        {
            var result = await _kernel.InvokePromptAsync(prompt);
            var json = result.ToString().Trim();
            
            // Parse the JSON into a dictionary
            var filters = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
            return filters ?? new Dictionary<string, object>();
        }
        catch (Exception)
        {
            return new Dictionary<string, object>();
        }
        }
        //public async Task<string> GenerateSqlQueryAsync(
        //    string naturalLanguageQuery,
        //    string tableName,
        //    TableSchema tableSchema,
        //    string operation = "SELECT")
        //{
        //    var prompt = $$"""
        //You are a SQL expert. Convert the natural language query into a valid SQL {{operation}} statement.
        
        //Table Name: {{tableName}}
        
        //Table Schema:
        //{{FormatTableSchema(tableSchema)}}
        
        //Natural Language Query: "{{naturalLanguageQuery}}"
        
        
        //Query: "{{naturalLanguageQuery}}"
        
        //Available filters:
        //- is_palindrome: boolean (true/false)
        //- min_length: integer (minimum string length)
        //- max_length: integer (maximum string length) 
        //- word_count: integer (exact word count)
        //- min_word_count: integer (minimum word count)
        //- max_word_count: integer (maximum word count)
        //- contains_character: string (single character to contain)
        //- contains_string: string (substring to contain)
        //- starts_with: string (prefix)
        //- ends_with: string (suffix)
        
        //Return ONLY a JSON object with the filters. If no filters apply, return empty object {}.
        
        //Examples:
        //Input: "all single word palindromic strings" 
        //Output: {"word_count": 1, "is_palindrome": true}
        
        //Input: "strings longer than 10 characters"
        //Output: {"min_length": 11}
        
        //Input: "palindromic strings that contain the first vowel"
        //Output: {"is_palindrome": true, "contains_character": "a"}
        
        //Input: "strings containing the letter z"
        //Output: {"contains_character": "z"}
        
        //Input: "words between 5 and 10 characters with exactly 2 words"
        //Output: {"min_length": 5, "max_length": 10, "word_count": 2}
        
        //Input: "strings that start with hello and end with world"
        //Output: {"starts_with": "hello", "ends_with": "world"}
        
        //Now convert this query:
        
        //Now convert this query for table {{tableName}}:
        //""";

        //    var result = await _kernel.InvokePromptAsync(prompt);

        //    return CleanSqlOutput(result.ToString());
        //}

        public async Task<string> GenerateFilterClauseAsync(
            string naturalLanguageQuery,
            string tableName,
            TableSchema tableSchema)
        {
            var prompt = $"""
        Convert the natural language query into a SQL WHERE clause only.
        
        Table: {tableName}
        Schema:
        {FormatTableSchema(tableSchema)}
        
        Query: "{naturalLanguageQuery}"
        
        Return ONLY the WHERE clause starting with "WHERE" or an empty string if no filters.
        Examples:
        Input: "expensive electronics"
        Output: "WHERE Category = 'Electronics' AND Price > 500"
        
        Input: "all products"
        Output: ""
        
        Convert this:
        """;

            var result = await _kernel.InvokePromptAsync(prompt);
            return CleanSqlOutput(result.ToString());
        }

        private string FormatTableSchema(TableSchema schema)
        {
            return string.Join("\n", schema.Columns.Select(c =>
                $"- {c.Name} ({c.Type}){(c.IsNullable ? " NULL" : " NOT NULL")}"));
        }

        private string CleanSqlOutput(string sql)
        {
            // Remove markdown code blocks if present
            sql = sql.Replace("```sql", "").Replace("```", "").Trim();

            // Ensure it ends with semicolon
            if (!sql.EndsWith(";") && !string.IsNullOrEmpty(sql))
                sql += ";";

            return sql;
        }
    }

    

}
