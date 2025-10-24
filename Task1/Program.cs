
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Task1.Data;
using Task1.Filters;
using Task1.Repositories;

namespace Task1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var databaseUrl = builder.Configuration["DATABASE_URL"]
                  ?? builder.Configuration.GetConnectionString("DefaultConnection");

            var connectionString = RailwayDbConnection.ParseConnectionString(databaseUrl);
            builder.Services.AddDbContext<AppDbContext>(option =>
            //option.UseNpgsql(builder.Configuration.GetConnectionString("contextConnection")));
            option.UseNpgsql(connectionString));
            var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
            builder.WebHost.UseUrls($"http://*:{port}");


            builder.Services.AddAutoMapper(typeof(Program));

            builder.Services.AddSingleton<Kernel>(serviceProvider =>
            {
                var apiKey = builder.Configuration["Groq:ApiKey"]
                           ?? throw new InvalidOperationException("Groq API Key not configured");

                var modelId = builder.Configuration["Groq:ModelId"] ?? "mixtral-8x7b-32768";
                var baseUrl = builder.Configuration["Groq:BaseUrl"] ?? "https://api.groq.com/openai/v1/";

                var kernelBuilder = Kernel.CreateBuilder();

                kernelBuilder.AddOpenAIChatCompletion(
                    modelId: modelId,
                    apiKey: apiKey,
                    httpClient: new HttpClient { BaseAddress = new Uri(baseUrl) });

                return kernelBuilder.Build();
            });
            builder.Services.AddScoped<StringRepository>();
            builder.Services.AddScoped<SchemaRepository>();
            builder.Services.AddScoped<SqlQueryGenerator>();
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            //if (app.Environment.IsDevelopment())
            //{
                app.UseSwagger();
                app.UseSwaggerUI();
            //}

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run($"http://0.0.0.0:{port}");
            //app.Run();
        }
    }
}
