using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.SemanticKernel;
using Task1.Data;
using Task1.Filters;
using Task1.Models;

namespace Task1.Repositories
{
    public class SchemaRepository
    {
        private AppDbContext _appDbContext;
        public SchemaRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }
        // Method 1: Get schema for a specific entity type
        public TableSchema GetTableSchema<T>() where T : class
        {
            var entityType = _appDbContext.Model.FindEntityType(typeof(T));

            if (entityType == null)
                throw new InvalidOperationException($"Entity type {typeof(T).Name} not found in DbContext");

            return new TableSchema
            {
                TableName = entityType.GetTableName(),
                Schema = entityType.GetSchema(),
                Columns = GetColumnsFromEntityType(entityType)
            };
        }
        // Method 2: Get schema by entity type name
        public TableSchema GetTableSchema(string entityName)
        {
            var entityType = _appDbContext.Model.GetEntityTypes()
                .FirstOrDefault(e => e.ClrType.Name == entityName || e.GetTableName() == entityName);

            if (entityType == null)
                throw new InvalidOperationException($"Entity {entityName} not found in DbContext");

            return new TableSchema
            {
                TableName = entityType.GetTableName(),
                Schema = entityType.GetSchema(),
                Columns = GetColumnsFromEntityType(entityType)
            };
        }
        // Method 3: Get all table schemas in the context
        public List<TableSchema> GetAllTableSchemas()
        {
            return _appDbContext.Model.GetEntityTypes()
                .Select(entityType => new TableSchema
                {
                    TableName = entityType.GetTableName(),
                    Schema = entityType.GetSchema(),
                    Columns = GetColumnsFromEntityType(entityType)
                })
                .ToList();
        }
        private List<TableColumn> GetColumnsFromEntityType(IEntityType entityType)
        {
            return entityType.GetProperties().Select(property => new TableColumn
            {
                Name = property.GetColumnName(),
                Type = property.GetColumnType(),
                IsNullable = property.IsNullable,
                IsPrimaryKey = property.IsPrimaryKey(),
          
            }).ToList();
        }

    }
}
