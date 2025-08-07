using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Modelos.Tuneflow.Media;
using Modelos.Tuneflow.Models;
using Npgsql;

namespace API.TUNEFLOW.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountriesController : ControllerBase
    {
        
        private readonly IConfiguration _config;
        
        public CountriesController(IConfiguration config)
        {
            _config = config;
        }

        // GET: api/Paises
        [HttpGet]
        public IEnumerable<Country> GetPais()
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("TUNEFLOWContext"));
            connection.Open();
            var paises = connection.Query<Country>("SELECT * FROM \"Countries\"");
            return paises;
        }

        // GET: api/Paises/5
        [HttpGet("{id}")]
        public ActionResult<Country> GetPais(int id)
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("TUNEFLOWContext"));
            connection.Open();
            var country = connection.QuerySingle<Country>(@"SELECT * FROM ""Countries"" WHERE ""Id"" = @Id", new { Id = id });

            if (country == null)
            {
                return NotFound();
            }

            return country;
        }

        // PUT: api/Paises/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public void PutPais(int id, [FromBody] Country country)
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("TUNEFLOWContext"));
            connection.Open();
            connection.Execute(@"UPDATE ""Countrys"" SET 
              ""Name""= @Name,
              "" Continent""= @Continent,
              ""Currency""= @Currency
            WHERE ""Id"" = @Id", new { Id = id, Name = country.Name, Continent = country.Continent, Currency = country.Currency });
        }

        // POST: api/Paises
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public Country PostPais([FromBody] Country country)
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("TUNEFLOWContext"));
            connection.Open();
            connection.Execute(@"INSERT INTO ""Paises"" (""Name"", ""Continent"", ""Currency"") 
            VALUES (@Name, @Continent, @Currency)", new { Name = country.Name, Continent = country.Continent, Currency = country.Currency });
            return country;
        }

        // DELETE: api/Paises/5
        [HttpDelete("{id}")]
        public void DeletePais(int id)
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("TUNEFLOWContext"));
            connection.Open();
            connection.Execute(@"DELETE FROM ""Countrys"" WHERE ""Id"" = @Id", new { Id = id });
        }


        // GET: api/Countries/5/songs
        [HttpGet("{paisId}/songs")]
        public IEnumerable<Song> GetSongsByCountry(int paisId)
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("TUNEFLOWContext"));
            connection.Open();
            // Consulta que une Songs con Artists y filtra por CountryId del artista
            var sql = @"
                    SELECT s.*
                    FROM ""Songs"" s
                    INNER JOIN ""Artists"" a ON s.""ArtistId"" = a.""Id""
                    WHERE a.""CountryId"" = @PaisId
                ";

            var canciones = connection.Query<Song>(sql, new { PaisId = paisId });
            return canciones;
        }

    }
}
