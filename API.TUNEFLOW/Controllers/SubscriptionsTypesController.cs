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
using Npgsql;

namespace API.TUNEFLOW.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubscriptionsTypesController : ControllerBase
    {
        
        
        private readonly IConfiguration _config;
        public SubscriptionsTypesController(IConfiguration configuration)
        {
            _config = configuration;
        }
        // GET: api/TiposSuscripciones
        [HttpGet]
        public IEnumerable<SubscriptionType> GetTipoSuscripcion()
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("TUNEFLOWContext"));
            connection.Open();
            var subscriptionTypes = connection.Query<SubscriptionType>("SELECT * FROM \"SubscriptionsTypes\"");
            return subscriptionTypes;
        }

        // GET: api/TiposSuscripciones/5
        [HttpGet("{id}")]
        public ActionResult<SubscriptionType> GetTipoSuscripcion(int id)
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("TUNEFLOWContext"));
            connection.Open();
            var subscriptionType = connection.QuerySingleOrDefault<SubscriptionType>(@"SELECT * FROM ""SubscriptionsTypes"" WHERE ""Id"" = @Id", new { Id = id });

            if (subscriptionType == null)
            {
                return NotFound();
            }

            return subscriptionType;
        }

        // PUT: api/TiposSuscripciones/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public void PutTipoSuscripcion(int id,[FromBody] SubscriptionType subscriptionType)
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("TUNEFLOWContext"));
            connection.Open();
            connection.Execute(@"UPDATE ""SubscriptionsTypes"" SET 
                ""Name"" = @Name,
                ""Price"" = @Price,
                ""MemberLimit"" = @MemberLimit,
                WHERE ""Id"" = @Id", new
            {
                Id = id,
                Name = subscriptionType.Name,
                Price = subscriptionType.Price,
                MemberLimit= subscriptionType.MemberLimit
            });
        }

        // POST: api/TiposSuscripciones
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public SubscriptionType PostTipoSuscripcion([FromBody]SubscriptionType subscriptionType)
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("TUNEFLOWContext"));
            connection.Open();
            connection.Execute(@"INSERT INTO ""SubscriptionsTypes"" (""Name"", ""Price"", ""MembersLimit"") 
                VALUES (@Name, @Price, @MembersLimit)", new
           {
               Name = subscriptionType.Name,
               Price = subscriptionType.Price,
               MemberLimit = subscriptionType.MemberLimit
           });
            return subscriptionType;
        }

        // DELETE: api/TiposSuscripciones/5
        [HttpDelete("{id}")]
        public void DeleteTipoSuscripcion(int id)
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("TUNEFLOWContext"));
            connection.Open();
            connection.Execute(@"DELETE FROM ""SubscriptionsTypes"" WHERE ""Id"" = @Id", new { Id = id });
         
        }
    }
}
