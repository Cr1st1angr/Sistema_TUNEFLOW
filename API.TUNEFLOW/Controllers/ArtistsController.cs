using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Modelos.Tuneflow.User.Consumer;
using Modelos.Tuneflow.User.Production;
using Modelos.Tuneflow.Models;
using Npgsql;

namespace API.TUNEFLOW.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArtistsController : ControllerBase
    {
        
        private readonly IConfiguration _config;

        // GET: api/Artistas
        public ArtistsController(IConfiguration config)
        {
            _config = config;
        }
        [HttpGet]
        public IEnumerable<Artist> GetArtista()
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("TUNEFLOWContext"));
            connection.Open();
            var artistas = connection.Query<Artist>("SELECT * FROM \"Artists\"");
            return artistas;
        }

        // GET: api/Artistas/5
        [HttpGet("{id}")]
        public ActionResult<Artist> GetArtistaById(int id)
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("TUNEFLOWContext"));
            connection.Open();
            var artista = connection.QuerySingleOrDefault<Artist>(@"SELECT * FROM ""Artists"" WHERE ""Id"" = @Id", new { Id = id });

            if (artista == null)
            {
                return NotFound();
            }

            return artista;
        }

        [HttpGet("UsuarioArtista/{idUser}")]
        public ActionResult<Artist> GetArtistaByUsuarioId(string idUser)
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("TUNEFLOWContext"));
            connection.Open();
            var artista = connection.QuerySingleOrDefault<Artist>(@"SELECT * FROM ""Artists"" WHERE ""UserId"" = @UserId", new { UserId = idUser });
            
            
            if (artista == null)
            {
                return NotFound();
            }
            return artista;
        }

        [HttpGet("Comprobar/StageName/{stageName}")]
        public async Task<ActionResult<bool>> ComprobarStageName(string stageName)
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("TUNEFLOWContext"));
            connection.Open();

            var exists = await connection.ExecuteScalarAsync<int>(
                @"SELECT COUNT(1) FROM ""Artists"" WHERE LOWER(""StageName"") = LOWER(@StageName)",
                new { StageName = stageName });

            return Ok(!(exists > 0));
        }

        // PUT: api/Artistas/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public void  PutArtista(int id,[FromBody] Artist artist)
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("TUNEFLOWContext"));
            connection.Open();
            connection.Execute(@"UPDATE ""Artists"" SET 
                ""StageName"" = @StageName, 
                ""MusicGenre"" = @MusicGenre,  
                ""CountryId"" = @CountryId, 
                ""Verified"" = @Verified
                WHERE ""Id"" = @Id", new
           {
               artist.StageName,
               artist.MusicGenre,
               artist.CountryId,
               artist.Verified,
               Id = id
           });
        }

        // POST: api/Artistas
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public ActionResult<Artist> PostArtista([FromBody]Artist artist)
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("TUNEFLOWContext"));
            connection.Open();

            var sql = @"INSERT INTO ""Artists"" 
                (""StageName"",""MusicGenre"",""CountryId"",""Verified"",""UserId"",""FirstName"",""LastName"",""Email"",""Password"",""Phone"",""BirthDate"",""AccountType"",""IsActive"",""RegistrationDate"") 
                VALUES (@StageName, @MusicGenre, @CountryId, @Verified,@UserId, @FirstName, @LastName, @Email, @Password, @Phone, @BirthDate, @AccountType, @IsActive, @RegistrationDate) RETURNING ""Id"";";
                
            var idreturned = connection.ExecuteScalar<int>(sql,new
                {
                    StageName = artist.StageName,
                    MusicGenre = artist.MusicGenre,
                    CountryId = artist.CountryId,
                    Verified = artist.Verified,
                    UserId = artist.UserId,
                    FirstName = artist.FirstName,
                    LastName= artist.LastName,
                    Email = artist.Email,
                    Password = artist.Password,
                    Phone = artist.Phone,
                    BirthDate = artist.BirthDate,
                    AccountType = artist.AccountType,
                    IsActive = artist.IsActive,
                    RegistrationDate = artist.RegistrationDate
            });

            artist.Id = idreturned;

            return CreatedAtAction(nameof(GetArtistaById), new { id = idreturned }, artist);
        }

        // DELETE: api/Artistas/5
        [HttpDelete("{id}")]
        public void DeleteArtista(int id)
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("TUNEFLOWContext"));
            connection.Open();
            connection.Execute(@"DELETE FROM ""Artistas"" WHERE ""Id"" = @Id", new { Id = id });

        }
    }
}
