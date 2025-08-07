using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System; 

namespace API.TUNEFLOW
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            
            var connectionString = builder.Configuration.GetConnectionString("TUNEFLOWContext");

           
            builder.Services.AddDbContext<TUNEFLOWContext>(options =>
            {
              
                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException("La cadena de conexión 'TUNEFLOWContext' no se encontró.");
                }

                
                options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                   
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5, 
                        maxRetryDelay: TimeSpan.FromSeconds(30), 
                        errorCodesToAdd: null 
                    );

                });
            });


            builder.Services.AddCors(options =>
            {
                options.AddPolicy("PermitirMVC", policy =>
                {
                    policy.WithOrigins("https://localhost:7015") 
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });

            builder.Services
                .AddControllers()
                .AddNewtonsoftJson(options =>
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                );

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger(); 
                app.UseSwaggerUI(); 
            }

            app.UseCors("PermitirMVC");

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}