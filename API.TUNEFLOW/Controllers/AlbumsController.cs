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
using Modelos.Tuneflow.Playlists;
using Modelos.Tuneflow.User.Administration;
using Modelos.Tuneflow.User.Production;
using Npgsql;

namespace API.TUNEFLOW.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AlbumsController : ControllerBase
    {
        
        private readonly IConfiguration _config;
        public AlbumsController(IConfiguration config)
        {
            _config = config;
        }

        // GET: api/Albums
        [HttpGet]
        public IEnumerable<Album> Get()
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("TUNEFLOWContext"));
            var albums = connection.Query<Album>("SELECT * FROM \"Albums\"");
            return albums;
        }

        // GET: api/Albums/5
        [HttpGet("{id}")]
        public ActionResult<Album> Get(int id)
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("TUNEFLOWContext"));
            var albums = connection.QuerySingle<Album>(@"SELECT * FROM ""Albums"" WHERE ""Id""=@Id", new {Id= id });

            if (albums == null)
            {
                return NotFound();
            }

            return albums;
        }

        // PUT: api/Albums/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public ActionResult Put(int id, [FromBody] Album album)
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("TUNEFLOWContext"));
            connection.Open(); 
            Console.WriteLine($"Actualizando álbum con ID: {id} y título: {album.Title}");
            connection.Execute(
                @"UPDATE ""Albums"" SET 
                ""Title"" = @Title, 
                ""ReleaseDate"" = @ReleaseDate, 
                ""Genre"" = @Genre, 
               ""CreationDate"" = @CreationDate, 
                ""Description"" = @Description, 
                ""CoverPath"" = @CoverPath,
                ""ArtistId"" = @ArtistId
                WHERE ""Id"" = @Id",
                new
                {
                    Title= album.Title,
                    ReleaseDate=album.ReleaseDate,
                    Genre = album.Genre,
                    CreationDate= album.CreationDate,
                    Description = album.Description,
                    CoverPath = album.CoverPath,
                    ArtistId = album.ArtistId,
                    Id = id

                });

            return NoContent();
        }

        // POST: api/Albums
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [HttpPost]
        public async Task<ActionResult<Album>> Post([FromBody] Album album)
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("TUNEFLOWContext"));
            await connection.OpenAsync();
            using var transaction = await connection.BeginTransactionAsync();

            try
            {
                var albumQuery = @"
            INSERT INTO ""Albums"" (""Title"", ""ReleaseDate"", ""Genre"", ""CreationDate"", ""Description"", ""CoverPath"", ""ArtistId"") 
            VALUES (@Title, @ReleaseDate, @Genre, @CreationDate, @Description, @CoverPath, @ArtistId)
            RETURNING ""Id"";";

                album.Id = await connection.ExecuteScalarAsync<int>(albumQuery, album, transaction);

                var songQuery = @"
            INSERT INTO ""Songs"" (""Title"", ""Duration"", ""Genre"", ""ArtistId"", ""AlbumId"", ""FilePath"", ""ExplicitContent"", ""ImagePath"", ""ReleaseDate"", ""Available"") 
            VALUES (@Title, @Duration, @Genre, @ArtistId, @AlbumId, @FilePath, @ExplicitContent, @ImagePath, @ReleaseDate, @Available)
            RETURNING ""Id"";";

                foreach (var song in album.Songs)
                {
                    song.AlbumId = album.Id;
                    song.Id = await connection.ExecuteScalarAsync<int>(songQuery, song, transaction);
                }
                 await transaction.CommitAsync();
                return Ok(album);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Error al crear álbum con canciones: {ex.Message}");
            }
        }



        // DELETE: api/Albums/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("TUNEFLOWContext"));
            connection.Open();
            connection.Execute(@"DELETE FROM ""Albums"" WHERE ""Id"" = @Id", new { Id = id });
        }
        [HttpGet("{albumId}/songs")]
        public ActionResult<IEnumerable<Song>> ObtenerCancionesPorAlbum(int albumId)
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("TUNEFLOWContext"));
            connection.Open();

            var sql = @"
        SELECT 
            s.""Id"", s.""Title"", s.""Duration"", s.""Genre"", s.""ArtistId"", s.""AlbumId"",
            s.""FilePath"", s.""ExplicitContent"", s.""ImagePath"", s.""Available"",
            
            a.""Id"", a.""StageName"", a.""MusicGenre"", a.""CountryId"", a.""Verified"", a.""UserId"",
            
            al.""Id"", al.""Title"", al.""ReleaseDate"", al.""Genre"",
            al.""CreationDate"", al.""Description"", al.""CoverPath"", al.""ArtistId""

        FROM ""Songs"" s
        INNER JOIN ""Artists"" a ON s.""ArtistId"" = a.""Id""
        LEFT JOIN ""Albums"" al ON s.""AlbumId"" = al.""Id""
        WHERE s.""AlbumId"" = @AlbumId
    ";

            var canciones = connection.Query<Song, Artist, Album, Song>(
                sql,
                (song, artist, album) =>
                {
                    song.Artist = artist;
                    song.Album = album;
                    return song;
                },
                new { AlbumId = albumId },
                splitOn: "Id,Id" 
            ).ToList();

            if (!canciones.Any())
                return NotFound($"No se encontraron canciones para el álbum con ID {albumId}.");

            return Ok(canciones);
        }

    }
}
