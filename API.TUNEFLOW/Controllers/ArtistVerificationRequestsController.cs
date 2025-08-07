using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Modelos.Tuneflow.Models;
using Npgsql;

namespace API.TUNEFLOW.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArtistVerificationRequestsController : ControllerBase
    {
        private readonly IConfiguration _config;

        public ArtistVerificationRequestsController(IConfiguration config)
        {
            _config= config;
        }

        // GET: api/ArtistVerificationRequests
        [HttpGet]
        public IEnumerable<ArtistVerificationRequest> Get()
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("TUNEFLOWContext"));
            connection.Open();
            var artistVerificationRequests = connection.Query<ArtistVerificationRequest>("SELECT * FROM \"ArtistVerificationRequests\"");
            return artistVerificationRequests;
        }

        // GET: api/ArtistVerificationRequests/5
        [HttpGet("{id}")]
        public ActionResult<ArtistVerificationRequest> GetArtistVerificationRequest(int id)
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("TUNEFLOWContext"));
            connection.Open();
            var artistVerificationRequest = connection.QuerySingleOrDefault<ArtistVerificationRequest>(
                @"SELECT * FROM ""ArtistVerificationRequests"" WHERE ""Id"" = @Id", new { Id = id });

            if (artistVerificationRequest == null)
            {
                return NotFound();
            }

            return artistVerificationRequest;
        }

        // PUT: api/ArtistVerificationRequests/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public void PutArtistVerificationRequest(int id, ArtistVerificationRequest artistVerificationRequest)
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("TUNEFLOWContext"));
            connection.Open();
            connection.Execute(@"UPDATE ""ArtistVerificationRequests"" SET 
                ""ArtistId"" = @ArtistId, 
                ""RequestDate"" = @RequestDate 
                WHERE ""Id"" = @Id",
                new { artistVerificationRequest.ArtistId, artistVerificationRequest.RequestDate, Id = id });

        }

        // POST: api/ArtistVerificationRequests
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public ActionResult<ArtistVerificationRequest> PostArtistVerificationRequest(ArtistVerificationRequest artistVerificationRequest)
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("TUNEFLOWContext"));
            connection.Open();
            var id = connection.ExecuteScalar<int>(@"INSERT INTO ""ArtistVerificationRequests"" (""ArtistId"", ""RequestDate"") 
                VALUES (@ArtistId, @RequestDate) RETURNING ""Id""",
                new { artistVerificationRequest.ArtistId, artistVerificationRequest.RequestDate });
            artistVerificationRequest.Id = id;
            return CreatedAtAction("GetArtistVerificationRequest", new { id = artistVerificationRequest.Id }, artistVerificationRequest);
        }

        // DELETE: api/ArtistVerificationRequests/5
        [HttpDelete("{id}")]
        public void DeleteArtistVerificationRequest(int id)
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("TUNEFLOWContext"));
            connection.Open();
            connection.Execute(@"DELETE FROM ""ArtistVerificationRequests"" WHERE ""Id"" = @Id", new { Id = id });
        }

    }
}
