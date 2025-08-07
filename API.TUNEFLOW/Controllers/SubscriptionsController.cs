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
    public class SubscriptionsController : ControllerBase
    {
        
        private readonly IConfiguration _config;
        
        public SubscriptionsController(IConfiguration configuration)
        {
            _config = configuration;
        }

        // GET: api/Suscripciones
        [HttpGet]
        public IEnumerable<Subscription> GetSuscripcion()
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("TUNEFLOWContext"));
            connection.Open();
            var suscripciones = connection.Query<Subscription>("SELECT * FROM \"Subscriptions\"");
            return suscripciones;
        }

        // GET: api/Suscripciones/5
        [HttpGet("{id}")]
        public ActionResult<Subscription> GetSuscripcionById(int id)
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("TUNEFLOWContext"));
            connection.Open();
            var subscription = connection.QuerySingleOrDefault<Subscription>(@"SELECT * FROM ""Subscriptions"" WHERE ""Id"" = @Id", new { Id = id });

            subscription.SubscriptionType = connection.QuerySingleOrDefault<SubscriptionType>(@"SELECT * FROM ""SubscriptionsTypes"" WHERE ""Id"" = @SubscriptionTypeId", new { SubscriptionTypeId = subscription.SubscriptionTypeId });

            if (subscription == null)
            {
                return NotFound();
            }

            return subscription;
        }

        [HttpGet("Codigo/Union/{codigo}")]
        public ActionResult<Subscription> GetSuscripcion(string codigo)
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("TUNEFLOWContext"));
            connection.Open();
            var sql = @"SELECT * FROM ""Subscriptions"" WHERE LOWER(""JoinCode"") = LOWER(@JoinCode)";
            var suscripcion = connection.QuerySingleOrDefault<Subscription>(sql, new { JoinCode = codigo });
            if (suscripcion == null)
            {
                return NotFound(); 
            }
            return suscripcion;
        }

        // PUT: api/Suscripciones/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public IActionResult PutSuscripcion(int id, [FromBody] Subscription subscription)
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("TUNEFLOWContext"));
            connection.Open();
            var affectedRows = connection.Execute(@"UPDATE ""Subscriptions"" SET 
                ""StartDate"" = @StartDate,
                ""JoinCode"" = @JoinCode,
                ""SubscriptionTypeId"" = @SubscriptionTypeId,
                ""NumberMembers"" = @NumberMembers
                 WHERE ""Id"" = @Id",
             new
             {
                 Id = id,
                 StartDate = subscription.StartDate,
                 JoinCode= subscription.JoinCode,
                 SubscriptionTypeId = subscription.SubscriptionTypeId,
                 NumberMembers = subscription.NumberMembers
             }

            );

            if (affectedRows == 0)
                return NotFound(); 

            return NoContent();

        }

        // POST: api/Suscripciones
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public ActionResult<Subscription> PostSuscripcion([FromBody] Subscription subscription)
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("TUNEFLOWContext"));
            connection.Open();
            var sql = @"INSERT INTO ""Subscriptions"" (""StartDate"", ""SubscriptionTypeId"",""NumberMembers"") 
                VALUES (@StartDate, @SubscriptionTypeId, @NumberMembers) 
                RETURNING ""Id"";";

            int idDevuelto = connection.ExecuteScalar<int>(sql, new
            {
                StartDate = subscription.StartDate,
                SubscriptionTypeId = subscription.SubscriptionTypeId,
                NumberMembers = subscription.NumberMembers
            });

            subscription.Id = idDevuelto;

            return CreatedAtAction(nameof(GetSuscripcionById), new { id = idDevuelto }, subscription);
        }
        // DELETE: api/Suscripciones/5
        [HttpDelete("{id}")]
        public void DeleteSuscripcion(int id)
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("TUNEFLOWContext"));
            connection.Open();
            connection.Execute(@"DELETE FROM ""Subscriptions"" WHERE ""Id"" = @Id", new { Id = id });
        }
    }
}
