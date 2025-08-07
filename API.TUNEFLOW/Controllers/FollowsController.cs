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
using Npgsql;

namespace API.TUNEFLOW.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FollowsController : ControllerBase
    {
        
        private readonly IConfiguration _config;
        
        public FollowsController(IConfiguration config)
        {
            _config = config;
            
        }

        // GET: api/Seguimientos
        [HttpGet]
        public IEnumerable<Follow> GetSeguimiento()
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("TUNEFLOWContext"));
            connection.Open();
            var follows = connection.Query<Follow>("SELECT * FROM \"Follows\"");
            return follows;
        }

        // GET: api/Seguimientos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Follow>> GetSeguimientoById(int id)
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("TUNEFLOWContext"));
            connection.Open();
            var follow = connection.QuerySingle<Follow>(@"SELECT * FROM ""Follows"" WHERE ""Id"" = @Id", new { Id = id });

            if (follow == null)
            {
                return NotFound();
            }

            return follow;
        }

        [HttpGet("FollowsByCliemnte/{idCliente}")]
        public ActionResult<IEnumerable<Follow>> GetSeguimientoByCliente(int idCliente)
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("TUNEFLOWContext"));
            connection.Open();
            var sql = @"
                        SELECT f.*, a.*
                        FROM ""Follows"" f
                        INNER JOIN ""Artists"" a ON f.""ArtistId"" = a.""Id""
                        WHERE f.""ClientId"" = @idCliente;
                    ";

            var follows = connection.Query<Follow, Artist, Follow>(
                    sql,
                    (follow, artist) =>
                    {
                        follow.Artist = artist;
                        return follow;
                    },
                    new { idCliente },
                    splitOn: "Id"
                ).ToList();

                return Ok(follows);
            
        }

        [HttpGet("ObtenerIsFollowed/{idClient}/{idArtist}")]
        public async Task<ActionResult<int>> GetIdSeguimientoExist(int idClient, int idArtist)
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("TUNEFLOWContext"));
            connection.Open();
            var sql = @"SELECT ""Id"" FROM ""Follows"" WHERE ""ClientId"" = @IdClient AND ""ArtistId""=@IdArtist";

            var followId = connection.QuerySingleOrDefault<int?>(sql, new { IdClient = idClient, IdArtist = idArtist });

            if (followId.HasValue)
            {
                return Ok(followId.Value);
            }
            else
            {
                return Ok(0); // o -1, o NotFound() si prefieres
            }

        }

        // PUT: api/Seguimientos/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public void PutSeguimiento(int id,[FromBody] Follow follow)
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("TUNEFLOWContext"));
            connection.Open();
            connection.Execute(@"UPDATE ""Follows"" SET
                ""ClientId""=@ClientId,
                ""ArtistId""=@ArtistId WERE ""Id""=@Id", new
            {
                ClientId = follow.ClientId,
                ArtistId =  follow.ArtistId
            });

        }

        // POST: api/Seguimientos
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public ActionResult<Follow>  PostSeguimiento([FromBody]Follow follow)
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("TUNEFLOWContext"));
            connection.Open();
            var sql = @"INSERT INTO ""Follows"" (""ClientId"", ""ArtistId"") 
                VALUES (@ClientId, @ArtistId) RETURNING ""Id""";

            var idResult = connection.ExecuteScalar<int>(sql, new
            {
                ClientId = follow.ClientId,
                ArtistId = follow.ArtistId
            });
            follow.Id = idResult;

            return CreatedAtAction(nameof(GetSeguimientoById), new { id = idResult }, follow);
        }

        // DELETE: api/Seguimientos/5
        [HttpDelete("{id}")]
        public void DeleteSeguimiento(int id)
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("TUNEFLOWContext"));
            connection.Open();
            connection.Execute(@"DELETE FROM ""Follows"" WHERE ""Id"" = @Id", new { Id = id });
            
        }
    }
}
