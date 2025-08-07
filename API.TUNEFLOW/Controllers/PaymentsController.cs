using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Modelos.Tuneflow.Payments;
using Modelos.Tuneflow.User.Consumer;
using Npgsql;

namespace API.TUNEFLOW.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        
        private readonly IConfiguration _config;
        
        public PaymentsController(IConfiguration config)
        {
            _config = config;
        }

        // GET: api/Pagos
        [HttpGet]
        public IEnumerable<Payment> GetPago()
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("TUNEFLOWContext"));
            connection.Open();
            var payment = connection.Query<Payment>("SELECT * FROM \"Payments\"");
            return payment;
        }

        // GET: api/Pagos/5
        [HttpGet("{id}")]
        public ActionResult<Payment> GetPagoById(int id)
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("TUNEFLOWContext"));
            connection.Open();
            var payment = connection.QuerySingleOrDefault<Payment>(@"SELECT * FROM ""Payments"" WHERE ""Id"" = @Id", new { Id = id });

            if (payment == null)
            {
                return NotFound();
            }

            return payment;
        }

        // PUT: api/Pagos/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public void PutPago(int id, [FromBody] Payment payment)
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("TUNEFLOWContext"));
            connection.Open();
            connection.Execute(@"UPDATE ""Payments"" SET 
                ""ClientId"" = @ClientId,
                ""PaymentDate"" = @PaymentDate,
                ""Amount"" = @Amount,
                ""PaymentMethod"" = @PaymentMethod
                WHERE ""Id"" = @Id", new
            {
                Id = id,
                ClientId = payment.ClientId,
                PaymentDate = payment.PaymentDate,
                Amount = payment.Amount,
                PaymentMethod = payment.PaymentMethod
            });
        }

        // POST: api/Pagos
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public ActionResult<Payment> PostPago([FromBody]Payment payment)
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("TUNEFLOWContext"));
            connection.Open();
            var sql = @"INSERT INTO ""Payments"" (""ClientId"", ""PaymentDate"", ""Amount"", ""PaymentMethod"") 
                VALUES (@ClientId, @PaymentDate, @Amount, @PaymentMethod) RETURNING ""Id""";
            
            var idDevuelto = connection.ExecuteScalar<int>(sql,new
            {
                ClientId = payment.ClientId,
                PaymentDate = payment.PaymentDate,
                Amount = payment.Amount,
                PaymentMethod = payment.PaymentMethod
            });

            payment.Id = idDevuelto; // Set the Id of the newly created payment
            return CreatedAtAction(nameof(GetPagoById), new { id = idDevuelto }, payment);
        }

        // DELETE: api/Pagos/5
        [HttpDelete("{id}")]
        public void DeletePago(int id)
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("TUNEFLOWContext"));
            connection.Open();
            connection.Execute(@"DELETE FROM ""Payments"" WERE ""Id"" = @Id", new { Id = id });
           
        }

    }
}
