using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Modelos.Tuneflow.User.Administration;
using Modelos.Tuneflow.User.Consumer;
using Npgsql;

namespace API.TUNEFLOW.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArtistsStatisticsController : ControllerBase
    {
        
        private readonly IConfiguration _config;
        public ArtistsStatisticsController(IConfiguration config)
        {
            _config = config;
        }

        // GET: api/EstadisticasArtistas
        [HttpGet]
        public IEnumerable<ArtistStatistics> GetEstadisticasArtista()
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("TUNEFLOWContext"));
            connection.Open();
            var artistStatistics = connection.Query<ArtistStatistics>("SELECT * FROM \"ArtistsStatistics\"");
            return artistStatistics;
        }

        // GET: api/EstadisticasArtistas/5
        [HttpGet("{id}")]
        public ActionResult<ArtistStatistics> GetEstadisticasArtistaById(int id)
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("TUNEFLOWContext"));
            connection.Open();
            var artistStatistics = connection.QuerySingle<ArtistStatistics>(@"SELECT * FROM ""ArtistsStatistics"" WHERE ""Id"" = @Id", new { Id = id });

            if (artistStatistics == null)
            {
                return NotFound();
            }

            return artistStatistics;
        }

        [HttpGet("Estadisticas/Artista/{idArtista}")]
        public ActionResult<ArtistStatistics> GetEstadisticasArtistaByArtistId(int idArtista)
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("TUNEFLOWContext"));
            connection.Open();
            var artistStatistics = connection.QuerySingle<ArtistStatistics>(@"SELECT * FROM ""ArtistsStatistics"" WHERE ""ArtistId"" = @Id", new { Id = idArtista });
            if (artistStatistics == null)
            {
                return NotFound();
            }
            return artistStatistics;
        }

        // PUT: api/EstadisticasArtistas/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEstadisticasArtista(int id, [FromBody] ArtistStatistics artistStatistics)
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("TUNEFLOWContext"));
            connection.Open();
            connection.Execute(@"UPDATE ""ArtistsStatistics"" SET 
                ""ArtistId"" = @ArtistId,
                ""TotalPlays"" = @TotalPlays,
                ""TotalFollowers"" = @TotalFollowers,
                ""PublishedSongs"" = @PublishedSongs,
                ""PublishedAlbums"" = @PublishedAlbums
                WHERE ""Id"" = @Id", new
           {
               Id = id,
               ArtistId = artistStatistics.ArtistId,
               TotalPlays = artistStatistics.TotalPlays,
               TotalFollowers = artistStatistics.TotalFollowers,
               PublishedSongs = artistStatistics.PublishedSongs,
               PublishedAlbums = artistStatistics.PublishedAlbums
           });

            return NoContent();

        }

        // POST: api/EstadisticasArtistas
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public ActionResult<ArtistStatistics> PostEstadisticasArtista([FromBody]ArtistStatistics artistStatistics)
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("TUNEFLOWContext"));
            connection.Open();
            var sql = @"INSERT INTO ""ArtistsStatistics"" 
                (""ArtistId"", ""TotalPlays"", ""TotalFollowers"", ""PublishedSongs"", ""PublishedAlbums"") 
                VALUES (@ArtistId, @TotalPlays, @TotalFollowers, @PublishedSongs, @PublishedAlbums) RETURNING ""Id"";";
            var idReturned = connection.ExecuteScalar<int>(sql, new
            {
                ArtistId = artistStatistics.ArtistId,
                TotalPlays = artistStatistics.TotalPlays,
                TotalFollowers = artistStatistics.TotalFollowers,
                PublishedSongs = artistStatistics.PublishedSongs,
                PublishedAlbums = artistStatistics.PublishedAlbums
            });
            artistStatistics.Id = idReturned;

            return CreatedAtAction(nameof(GetEstadisticasArtistaById), new { id = idReturned }, artistStatistics);
        }

        // DELETE: api/EstadisticasArtistas/5
        [HttpDelete("{id}")]
        public void DeleteEstadisticasArtista(int id)
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("TUNEFLOWContext"));
            connection.Open();
            connection.Execute(@"DELETE FROM ""ArtistsStatistics"" WHERE ""Id"" = @Id", new { Id = id });

           
        }

    }
}
