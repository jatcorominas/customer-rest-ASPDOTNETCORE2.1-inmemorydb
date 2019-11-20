using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Customers.Models;

namespace Customers.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly CustomerContext _context;

        public CustomersController(CustomerContext context)
        {
            _context = context;
            
        }

        // GET: api/Customers
        [HttpGet]
        public async Task<ActionResult> GetCustomers()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            IEnumerable<Customer> customers =  await _context.Customers.ToListAsync();

            if (customers == null)
            {
                return NotFound();
            }

            return Ok(customers);
        }

        // GET: api/Customers/5
        [HttpGet("{id}")]
        public async Task<ActionResult> GetCustomerById([FromRoute] long id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var customer = await _context.Customers.FindAsync(id);

            if (customer == null)
            {
                return NotFound();
            }

            return Ok(customer);
        }

        // GET: api/Customers/age/30
        [HttpGet("age/{age}")]
        public async Task<ActionResult> GetCustomerByAge([FromRoute] int? age)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var query = _context.Customers as IQueryable<Customer>;
            if ( age != null )
                query = query.Where(x => x.age == age);

            IEnumerable<Customer> customers = await query.ToListAsync();

            if (customers == null)
            {
                return NotFound();
            }

            return Ok(customers);
        }

        // PUT: api/Customers/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCustomer([FromRoute] long id, [FromBody] Customer customer)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!CustomerExists(id))
            {
                return NotFound();
            }

            // Get the Customer
            Customer customerToBeUpdated = await _context.Customers.FindAsync(id);
            customerToBeUpdated.name = customer.name;
            customerToBeUpdated.active = customer.active;
            customerToBeUpdated.age = customer.age;

            _context.Entry(customerToBeUpdated).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CustomerExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok();
        }

        // POST: api/Customers/seed
        [HttpPost("seed")]
        public async Task<IActionResult> SeedDatabase()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            // Create a new Customer if collection is empty,
            // which means you can't delete all Customers.
            Customer customer = new Customer { name = "Mason", age = 30, active = false };
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();    
            
            return CreatedAtAction(nameof(GetCustomerById), new { id = customer.id }, customer);
        }

        // POST: api/Customers/create
        [HttpPost("create")]
        public async Task<IActionResult> PostCustomer([FromBody] Customer customer)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCustomer", new { id = customer.id }, customer);
        }

        // DELETE: api/Customers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer([FromRoute] long id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();

            return Ok("Successfully deleted customer");
        }

        // DELETE: api/Customers/delete
        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteAllCustomers()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            IEnumerable<Customer> customers = await _context.Customers.ToListAsync();
            if( customers == null)
            {
                return NotFound();
            }

            _context.Customers.RemoveRange(customers);
            await _context.SaveChangesAsync();
            return Ok("Deleted All Customers");

        }

        private bool CustomerExists(long id)
        {
            return _context.Customers.Any(e => e.id == id);
        }
    }
}