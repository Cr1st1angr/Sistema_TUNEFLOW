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
using Modelos.Tuneflow.User.Profiles;
using Npgsql;

namespace API.TUNEFLOW.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FavoriteSongsController : ControllerBase
    {
        
        private readonly IConfiguration _config;
        

        public FavoriteSongsController(IConfiguration config)
        {
            _config = config;
        }
        // GET: api/CancionesFavoritas
        [HttpGet]
        public IEnumerable<FavoriteSong> GetCancionFavorita()
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("TUNEFLOWContext"));
            connection.Open();
            var cancionesfav = connection.Query<FavoriteSong>("SELECT * FROM \"FavoritesSongs\"");
            return cancionesfav;
        }

        // GET: api/CancionesFavoritas/5
        [HttpGet("{id}")]
        public ActionResult<FavoriteSong> GetCancionFavoritaById(int id)
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("TUNEFLOWContext"));
            connection.Open();
            var favoriteSong = connection.QuerySingle<FavoriteSong>(@"SELECT * FROM ""FavoritesSongs"" WERE ""Id""= @Id", new { Id = id });

            if (favoriteSong == null)
            {
                return NotFound();
            }

            return favoriteSong;
        }

        [HttpGet("IsFavorite/{id}/{idClient}")]
        public ActionResult<int> GetCancionFavoritaPorIdEIdCliente(int id, int idClient)
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("TUNEFLOWContext"));
            connection.Open();
            var sql = @"SELECT ""Id"" FROM ""FavoritesSongs"" WHERE ""ClientId"" = @IdClient AND ""SongId"" = @Id";

            var existId = connection.ExecuteScalar<int?>(sql, new { IdClient = idClient, Id = id });

            if (existId.HasValue)
                return Ok(new { id = existId.Value });  // Devuelve el Id encontrado
            else
                return NotFound();  // O puedes devolver algo distinto si no existe


        }

        // PUT: api/CancionesFavoritas/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public void PutCancionFavorita(int id,[FromBody] FavoriteSong favoriteSong)
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("TUNEFLOWContext"));
            connection.Open();
            connection.Execute(@"UPDATE ""FavoritesSongs"" SET " +
                "\"ClientId\" = @ClientId, " +
                "\"SongsId\" = @SongsId, " +
                "\"DateAdded\" = @DateAdded " +
                "WHERE \"Id\" = @Id",new
                {
                    ClientId=favoriteSong.ClientId,
                    SongId=favoriteSong.SongId,
                    DateAdded = favoriteSong.DateAdded,
                    Id = id
                } );
        }

        // POST: api/CancionesFavoritas
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public ActionResult<FavoriteSong> PostCancionFavorita([FromBody]FavoriteSong favoriteSong)
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("TUNEFLOWContext"));
            connection.Open();
            Console.WriteLine("Entro al post");
            var idReturned = connection.ExecuteScalar<int>(@"INSERT INTO ""FavoritesSongs"" (""ClientId"", ""SongId"", ""DateAdded"") 
                                                VALUES (@ClientId, @SongId, @DateAdded) RETURNING ""Id"";",
                new
                {
                    ClientId = favoriteSong.ClientId,
                    SongId = favoriteSong.SongId,
                    DateAdded = favoriteSong.DateAdded
                });
            favoriteSong.Id = idReturned;

            return CreatedAtAction(nameof(GetCancionFavoritaById), new { id = idReturned }, favoriteSong);
        }

        // DELETE: api/CancionesFavoritas/5
        [HttpDelete("{id}")]
        public ActionResult DeleteCancionFavorita(int id)
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("TUNEFLOWContext"));
            connection.Open();
            connection.Execute(@"DELETE FROM ""FavoritesSongs"" WHERE ""Id"" = @Id", new { Id = id });
            return NoContent(); // Código 204
        }

    }
}
