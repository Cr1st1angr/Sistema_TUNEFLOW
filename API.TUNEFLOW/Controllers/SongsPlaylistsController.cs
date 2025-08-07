using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Modelos.Tuneflow.Playlists;
using Modelos.Tuneflow.Media;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Modelos.Tuneflow.User.Production;
using Npgsql;

namespace API.TUNEFLOW.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SongsPlaylistsController : ControllerBase
    {
        
        private readonly IConfiguration _config;
        public SongsPlaylistsController(IConfiguration config)
        {
            _config = config;
        }

        // GET: api/MusicasPlaylists
        [HttpGet]
        public IEnumerable<SongPlaylist> GetMusicaPlaylist()
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("TUNEFLOWContext"));
            connection.Open();
            var getmusic = connection.Query<SongPlaylist>("SELECT * FROM \"SongsPlaylists\"");
            return getmusic;
        }

        // GET: api/MusicasPlaylists/5
        [HttpGet("{id}")]
        public ActionResult<SongPlaylist> GetMusicaPlaylistById(int id)
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("TUNEFLOWContext"));
            connection.Open();
            var songPlaylist = connection.QuerySingle<SongPlaylist>(@"SELECT * FROM ""SongsPlaylists"" WHERE ""Id"" = @Id", new { Id = id });

            if (songPlaylist == null)
            {
                return NotFound();
            }

            return songPlaylist;
        }

        [HttpGet("ExistSongPlaylist/{SongId}/{PlaylistId}")]
        public ActionResult<int> GetCancionFavoritaPorIdEIdCliente(int SongId, int PlaylistId)
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("TUNEFLOWContext"));
            connection.Open();
            var sql = @"SELECT ""Id"" FROM ""SongsPlaylists"" WHERE ""SongId"" = @IdSong AND ""PlaylistId"" = @IdPlaylist";

            var existId = connection.ExecuteScalar<int?>(sql, new { IdSong = SongId, IdPlaylist = PlaylistId });

            if (existId.HasValue)
                return Ok(new { id = existId.Value });  
            else
                return NotFound();  


        }

        [HttpGet("SongsForPlaylist/{idPlaylist}")]
        public ActionResult<IEnumerable<Song>> ObtenerCancionesPorPlaylist(int idPlaylist)
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("TUNEFLOWContext"));
            connection.Open();
            var sql = @"
                            SELECT 
                                c.""Id"", c.""Title"", c.""Duration"", c.""Genre"", c.""ArtistId"", c.""AlbumId"",
                                c.""FilePath"", c.""ExplicitContent"", c.""ImagePath"",c.""Available"",

                                a.""Id"", a.""StageName"", a.""MusicGenre"",
                                a.""CountryId"", a.""Verified"", a.""UserId"",

                                al.""Id"", al.""Title"", al.""ReleaseDate"", al.""Genre"",
                                al.""CreationDate"", al.""Description"", al.""CoverPath""
                            FROM ""SongsPlaylists"" mp
                            INNER JOIN ""Songs"" c ON mp.""SongId"" = c.""Id""
                            INNER JOIN ""Artists"" a ON c.""ArtistId"" = a.""Id""
                            LEFT JOIN ""Albums"" al ON c.""AlbumId"" = al.""Id""
                            WHERE mp.""PlaylistId"" = @IdPlaylist";

            var songs = connection.Query<Song, Artist, Album, Song>(
                sql,
                (song, artist, album) =>
                {
                    song.Artist = artist;
                    song.Album = album;
                    return song;
                },
                new { IdPlaylist = idPlaylist },
                splitOn: "Id,Id" 
            ).ToList();

            if (!songs.Any())
            {
                return NotFound($"No se encontraron canciones para la playlist con ID {idPlaylist}.");
            }

            return Ok(songs);
        }

        // PUT: api/MusicasPlaylists/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public void PutMusicaPlaylist(int id,[FromBody] SongPlaylist musicPlaylist)
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("TUNEFLOWContext"));
            connection.Open();
            connection.Execute(@"UPDATE ""SongsPlaylists"" SET 
                ""PlaylistId"" = @PlaylistId,
                ""MusicaId"" = @MusicaId
                WHERE ""Id"" = @Id", new
            {
                Id = id,
                PlaylistId = musicPlaylist.PlaylistId,
                SongId = musicPlaylist.SongId
            });
        }

        // POST: api/MusicasPlaylists
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public ActionResult<SongPlaylist> PostMusicaPlaylist([FromBody]SongPlaylist musicPlaylist)
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("TUNEFLOWContext"));
            connection.Open();
            var idReturned = connection.ExecuteScalar<int>(@"INSERT INTO ""SongsPlaylists"" (""SongId"", ""PlaylistId"") 
                VALUES (@SongId, @PlaylistId) RETURNING ""Id"";", new
            {
                SongId = musicPlaylist.SongId,
                PlaylistId = musicPlaylist.PlaylistId
            });

            musicPlaylist.Id = idReturned;

            return CreatedAtAction(nameof(GetMusicaPlaylistById), new { id = idReturned }, musicPlaylist);
        }

        // DELETE: api/MusicasPlaylists/5
        [HttpDelete("{id}")]
        public ActionResult DeleteMusicaPlaylist(int id)
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("TUNEFLOWContext"));
            connection.Open();
            connection.Execute(@"DELETE FROM ""SongsPlaylists"" WHERE ""Id"" = @Id", new { Id = id });
            return NoContent(); 
        }

    }
}
