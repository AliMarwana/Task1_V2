namespace Task1.Filters
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.SemanticKernel;
    using Microsoft.SemanticKernel.Connectors.OpenAI;
    using System.Linq;
    //using System.Linq.Dynamic.Core;
    using System.Text.Json;
    using Task1.Data;

    public class KernelEFilter<T>
    {
        private readonly Kernel _kernel;

        public KernelEFilter(Kernel kernel)
        {
            _kernel = kernel;
        }

        //public async Task<List<T>> FilterAsync(DbContext context, string naturalLanguageQuery)
        //{
        //    // Step 1: Get entity schema information
        //    var entityType = context.Model.FindEntityType(typeof(T));
        //    var schema = GetEntitySchema(entityType);

        //    // Step 2: Convert natural language to SQL WHERE clause using AI
        //    var whereClause = await ConvertToWhereClauseAsync(naturalLanguageQuery, schema);

        //    // Step 3: Apply the filter using Dynamic LINQ
        //    var query = await context.Set<T>().AsQueryable();

        //    if (!string.IsNullOrEmpty(whereClause))
        //    {
        //        query = query.Where(whereClause);
        //    }

        //    return await query.ToListAsync();
        //}

        //public async Task<IQueryable<T>> ApplyFilterAsync(IQueryable<T> query, string naturalLanguageQuery)
        //{
        //    var entityType = typeof(T);
        //    var schema = GetEntitySchema(entityType);
        //    var whereClause = await ConvertToWhereClauseAsync(naturalLanguageQuery, schema);

        //    return !string.IsNullOrEmpty(whereClause)
        //        ? query.Where(whereClause)
        //        : query;
        //}

        private async Task<string> ConvertToWhereClauseAsync(string naturalLanguageQuery, string schema)
        {
            var prompt = $"""
        You are a database query assistant. Convert the natural language query into a valid Entity Framework Dynamic LINQ WHERE clause.
        
        Available Entity Properties and Types:
        {schema}
        
        Natural Language Query: "{naturalLanguageQuery}"
        
        Rules:
        1. Return ONLY the Dynamic LINQ expression without any additional text
        2. Use C# property names exactly as shown above
        3. For string comparisons, use case-insensitive contains by default
        4. For dates, use DateTime comparisons
        5. Use proper C# syntax for comparisons
        
        Examples:
        Input: "expensive electronics in stock"
        Output: "Category.Contains(\"Electronics\") && Price > 500 && StockQuantity > 0"
        
        Input: "users from New York who signed up recently"
        Output: "City.Contains(\"New York\") && SignupDate > DateTime.Now.AddMonths(-1)"
        
        Input: "products that need restocking"
        Output: "StockQuantity < 10"
        
        Input: "active premium customers"
        Output: "IsActive == true && MembershipLevel == \"Premium\""
        
        Now convert this query:
        """;

            var result = await _kernel.InvokePromptAsync(prompt);
            return result.ToString().Trim();
        }

        private string GetEntitySchema(Microsoft.EntityFrameworkCore.Metadata.IEntityType entityType)
        {
            var properties = entityType.GetProperties();
            return string.Join("\n", properties.Select(p =>
                $"- {p.Name} ({p.ClrType.Name})"));
        }

        private string GetEntitySchema(Type entityType)
        {
            var properties = entityType.GetProperties();
            return string.Join("\n", properties.Select(p =>
                $"- {p.Name} ({p.PropertyType.Name})"));
        }
    }
}
