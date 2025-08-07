using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Runtime.ExceptionServices;
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
    public class ClientsController : ControllerBase
    {
        private readonly IConfiguration _config;
        public ClientsController(IConfiguration config)
        {
            _config = config;
            
        }

        // GET: api/Clientes
        [HttpGet]
        public IEnumerable<Client> GetCliente()
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("TUNEFLOWContext"));
            connection.Open();
            var cliente= connection.Query<Client>("SELECT * FROM \"Clients\"");
            return cliente;
        }

        // GET: api/Clientes/5
        [HttpGet("{id}")]
        public ActionResult<Client> GetClienteById(int id)
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("TUNEFLOWContext"));
            connection.Open();
            var client = connection.QuerySingleOrDefault<Client>(@"SELECT * FROM ""Clients"" WHERE ""Id"" = @Id", new { Id = id });

            if (client == null)
            {
                return NotFound();
            }

            return client;
        }

        [HttpGet("Usuario/{idUser}")]
        public ActionResult<Client> GetClienteByUsuarioId(string idUser)
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("TUNEFLOWContext"));
            connection.Open();
            var client = connection.QuerySingleOrDefault<Client>(@"SELECT * FROM ""Clients"" WHERE ""UserId"" = @UserId", new { UserId = idUser });
            if (client == null)
            {
                return NotFound();
            }
            return client;
        }

        // PUT: api/Clientes/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public void PutCliente(int id,[FromBody] Client client)
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("TUNEFLOWContext"));
            connection.Open();
            connection.Execute(@"UPDATE ""Clients"" SET 
                ""FirstName"" = @FirstName,
                ""LastName"" = @LastName,
                ""Email"" = @Email,
                ""BirthDate"" = @BirthDate,
                ""CountryId"" = @CountryId,
                ""SubscriptionId"" = @SubscriptionId
                WHERE ""Id"" = @Id", new
            {
                FirstName = client.FirstName,
                LastName = client.LastName,
                Email = client.Email,
                BirthDate = client.BirthDate,
                CountryId = client.CountryId,
                SubscriptionId = client.SubscriptionId,
                Id = id
            });
              
        }

        // POST: api/Clientes
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public ActionResult<Client> PostCliente([FromBody]Client client)
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("TUNEFLOWContext"));
            connection.Open();
            var sql = @"INSERT INTO ""Clients"" (""CountryId"", ""SubscriptionId"", ""FirstName"", ""LastName"", ""Email"", ""Password"",""Phone"",""BirthDate"",""AccountType"",""IsActive"",""RegistrationDate"",""UserId"") 
                VALUES (@CountryId, @SubscriptionId, @FirstName, @LastName, @Email, @Password, @Phone, @BirthDate, @AccountType, @IsActive, @RegistrationDate, @UserId) RETURNING ""Id"";";
            
            var idReturned = connection.ExecuteScalar<int>(sql,new
           {
               CountryId = client.CountryId,
               SubscriptionId = client.SubscriptionId,
               FirstName = client.FirstName,
               LastName = client.LastName,
               Email = client.Email,
               Password = client.Password,
               Phone = client.Phone,
               BirthDate = client.BirthDate,
               AccountType = client.AccountType,
               IsActive = client.IsActive,
               RegistrationDate = client.RegistrationDate,
               UserId = client.UserId

            });

            client.Id = idReturned;

            return CreatedAtAction(nameof(GetClienteById), new { id = idReturned }, client);
        }

        // DELETE: api/Clientes/5
        [HttpDelete("{id}")]
        public void DeleteCliente(int id)
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("TUNEFLOWContext"));
            connection.Open();
            connection.Execute(@"DELETE FROM ""Clients"" WHERE ""Id"" = @Id", new { Id = id });
        }

    }
}
