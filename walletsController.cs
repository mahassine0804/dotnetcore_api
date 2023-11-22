using System.Collections.Generic;
using System.Threading.Tasks;
using dbcontext_wallet_test.data;
using dbcontext_wallet_test.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace dbcontext_wallet_test.Controllers
{

    [Route("api/[controller]")]
    [ApiController]

    
    public class walletsController : ControllerBase
    {
        private readonly Mydbcontext _context;

        public walletsController(Mydbcontext context)
        {
            _context = context;
        }
        [HttpGet]
        //this is a function to get all the wallets from the database without any filter or goupby 
        public async Task<ActionResult<IEnumerable<wallets>>> GetWallets()
        {
            if (_context.wallets == null)
            {
                return NotFound();
            }
            try
            {
                return await _context.wallets.ToListAsync();

            }
            catch (Exception e)
            {
                return NotFound(e);
            }



        }
        [HttpGet("sortbybalance")]
        // this one is a function that get you the wallets from the database sorted by balance and grouped by name
        public async Task<ActionResult<IEnumerable<wallets>>> GetWalletssortebybalance()
        {
            if (_context.wallets == null)
            {
                return NotFound();
            }

            try
            {
                var groupedWallets = await _context.wallets
                    .GroupBy(x => x.name)
                    .Select(g => new wallets { name = g.Key, id = g.First().id, balance = g.Sum(w=> w.balance) /* Add other properties if needed */ })
                    .OrderByDescending(x=>x.balance)
                    .ToListAsync();

                return Ok(groupedWallets);
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        [HttpGet("{id}")]
        //this one is a function to get you a wallet  from the database with a specific id you give in parameters
        public async Task<ActionResult<wallets>> GetWallets(int id)
        {
            var wallet = await _context.wallets.FindAsync(id);

            if (wallet == null)
            {
                return NotFound();
            }

            return wallet;
        }
        [HttpGet("byname/{name}")]
        //this function get you wallets wallet with the specific name you gave in the parameters
        public async Task<ActionResult<wallets>> GetWalletsByName(string name)
        {
            var wallet = await _context.wallets.FirstOrDefaultAsync(w => w.name == name);

            if (wallet == null)
            {
                return NotFound();
            }

            return wallet;
        }

        [HttpPost]
        //this is function allow us to add new wallets to the database
            public async Task<ActionResult<wallets>> CreateUser([FromBody] wallets newwallet)
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        _context.wallets.Add(newwallet);
                        await _context.SaveChangesAsync();

                        return CreatedAtAction(nameof(newwallet), new { id = newwallet.id }, newwallet);
                    }
                     catch (Exception ex)
                    {
                        return BadRequest(ex.Message);
                    }
                }
                else
                {
                return BadRequest(ModelState);
                }
            }
        //this a function to check if this id exist already in the database or not its booleen function
        private bool Walletexist(int id)
        {
            return (_context.wallets?.Any(x => x.id == id)).GetValueOrDefault();

        }
        [HttpPut("{id}")]
        //this function allow us to modify an exesting wallet in the database
        public async Task<ActionResult<wallets>> UpdateWallet(int id, [FromBody] wallets updatedWallet)
        {
            if (id != updatedWallet.id)
            {
                return BadRequest("ID in the URL does not match ID in the data.");
            }

            var existingWallet = await _context.wallets.FindAsync(id);

            if (existingWallet == null)
            {
                return NotFound("Wallet not found.");
            }

            existingWallet.name = updatedWallet.name;
            existingWallet.balance = updatedWallet.balance;

            try
            {
                _context.wallets.Update(existingWallet);
                await _context.SaveChangesAsync();
                return Ok(existingWallet);
            }
            catch (DbUpdateConcurrencyException)
            {
                return StatusCode(500, "Concurrency issue occurred.");
            }
        }
         [HttpDelete("{id}")]
         // this function allow us to delete a wallets by its id we give in the parameters
         public async Task<ActionResult> deletewallet(int id)
         {
            if(_context.wallets == null)
            {
                return NotFound();  
            }
            var wallet = await _context.wallets.FindAsync(id);
            if(wallet == null)
            {
                return NotFound();
            }
            _context.wallets.Remove(wallet);
            await _context.SaveChangesAsync();
            return Ok();
         }
        [HttpPut("transaction/{receiverId}/{senderId}/{amount}")]
        //this is a function to make a transaction to send money from wallet to another wallet we need to provide the senderidwallet and receiveridwallet and also the amount

        public async Task<ActionResult<wallets>> Maketransaction(int receiverId, int senderId, int amount)
        {
            using var transaction = _context.Database.BeginTransaction();

            // Fetch sender and receiver wallets from the database based on their IDs
            var senderWallet = _context.wallets.Single(x => x.id == senderId);
            var receiverWallet = _context.wallets.Single(x => x.id == receiverId);

            if(senderWallet.balance < amount)
            {
                string notenough = "solde est insifusant";
                return BadRequest(notenough);
            }
            
            senderWallet.balance -= amount;
            receiverWallet.balance += amount;

            try
            { 
                // Save changes to the database within the transaction
                await _context.SaveChangesAsync();

                // Commit the transaction if everything succeeds
                await transaction.CommitAsync();

                // Return some response indicating the transaction was successful
                return Ok("Transaction completed successfully.");
            }
            catch (Exception)
            {
                // Rollback the transaction if an exception occurs
                await transaction.RollbackAsync();
                return StatusCode(500, "Transaction failed. Rollback performed.");
            }
        }
        //this is a http method to get wallets with balance less or greater than a int we choose 
        [HttpGet("filterbalance")]
        
        public async Task<ActionResult<IEnumerable<wallets>>> GetfilterWallets(string? akbar,string? asghar)
        {
            if(!string.IsNullOrEmpty(akbar) && !string.IsNullOrEmpty(asghar)){
                return BadRequest("enter only one provider please");
            }
            if(string.IsNullOrEmpty(asghar) && string.IsNullOrEmpty(akbar))
            {

                return BadRequest("provide at least one parameter");
            }

            if (!string.IsNullOrEmpty(akbar) && int.TryParse(akbar, out int akbarInt))
            {
                return   await _context.wallets.Where(x => x.balance >= akbarInt).ToListAsync();
                 
                 
            }
            else if (!string.IsNullOrEmpty(asghar) && int.TryParse(asghar, out int asgharInt))
            {
                return await _context.wallets.Where(x=> x.balance <= asgharInt).ToListAsync();
            }return Ok();
        }
    }
}