using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Amazoom_UI_with_DB.Data;
using Amazoom_UI_with_DB.Models;
using Microsoft.AspNetCore.Authorization;
using System.Net;
using System.IO;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;
using System.Web;
using System.Collections.Specialized;

namespace Amazoom_UI_with_DB.Controllers
{
    public class MerchandisesController : Controller
    {
        private static readonly HttpClient client = new HttpClient();
        private readonly ApplicationDbContext _context;

        public MerchandisesController(ApplicationDbContext context)
        {
            _context = context;
        }




        // GET: Merchandises
        public async Task<IActionResult> Index()
        {

            var responseString = await client.GetStringAsync("http://localhost:5000/updateList");
            string[] itemQuantityPairs = responseString.Split('%');

            var table = _context.Merchandise.Where(j => j.Name.Length >= 0).ToList();
            foreach (var entry in table)
            {
                _context.Merchandise.Remove(entry);
            }
            await _context.SaveChangesAsync();

            foreach (string itemQuantity in itemQuantityPairs)
            {
                Merchandise newMerchandise = new Merchandise();
                string[] splitPairs = itemQuantity.Split('-');
                if (splitPairs.Count() == 2)
                {
                    newMerchandise.Name = splitPairs[0];
                    newMerchandise.qty = Int32.Parse(splitPairs[1]);
                }

                if (_context.Merchandise.Where(j => j.Name.Contains(newMerchandise.Name)).ToList().Count == 0 && newMerchandise.Name != null)
                {
                    _context.Add(newMerchandise);
                    await _context.SaveChangesAsync();
                }

            }

            return View(await _context.Merchandise.Where(j => j.Name.Length >= 0).ToListAsync());
            //                 ^ the _context retrieves the data from the db, converts it to a list, then displays it.
        }





        // GET: Merchandises/ShowSearchForm
        public async Task<IActionResult> ShowSearchForm()
        {
            return View();
        }




        // POST: Merchandises/ShowSearchResults
        public async Task<IActionResult> ShowSearchResults(String SearchPhrase)
        {
            // var responseString = await client.GetStringAsync("http://localhost:5000/api/values/bye");
            return View("Index", await _context.Merchandise.Where(j => j.Name.Contains(SearchPhrase)).ToListAsync());
        }











        // POST: Purchase an item.

        [Authorize]
        public async Task<ActionResult> PurchaseAsync(int id, [Bind("Id,Name,qty")] Merchandise merchandise)
        {
            // var responseJSONstring = JsonConvert.SerializeObject(merchandise);
            //NameValueCollection queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);

            //queryString.Add("MerchandiseID", merchandise.Id.ToString());
            //queryString.Add("qty", merchandise.qty.ToString());

            string queryString = "item";

            try
            {
                var responseString = await client.GetStringAsync("http://localhost:5000/placeorder/" + queryString + id + "-1"); // "item02-qty" = qty order of item02
                return View("OrderPlaced");

            }
            catch (Exception)
            {

                return View("OrderFailed");
                throw;
            }
        }





        // Purchase Multiple Items.
        [Authorize]
        public async Task<ActionResult> MultiplePurchasesAsync(int id, String purchaseQuantityNumber, [Bind("Id,Name,qty")] Merchandise merchandise)
        {
            // var responseJSONstring = JsonConvert.SerializeObject(merchandise);
            //NameValueCollection queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);

            //queryString.Add("MerchandiseID", merchandise.Id.ToString());
            //queryString.Add("qty", merchandise.qty.ToString());

            var table = _context.Merchandise.Where(j => j.Id == merchandise.Id).ToList();
            
            string queryString = table[0].Name; 

            string userInfo = "";
            userInfo = User.Identity.Name;

            try
            {
                string responseString = await client.GetStringAsync("http://localhost:5000/placeorder/" + queryString + "-" + purchaseQuantityNumber + "-" + userInfo); // "item02-qty" = qty order of item02
                if(responseString == "Order Successfully Placed")
                {
                    return View("OrderPlaced");
                }
                ErrorString err = new ErrorString();
                err.Error = responseString;
                return View("OrderFailed",err);

            }
            catch (Exception)
            {
                
                return View("OrderFailed");
                throw;
            }
        }




        // GET: Merchandises/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var merchandise = await _context.Merchandise
                .FirstOrDefaultAsync(m => m.Id == id);
            if (merchandise == null)
            {
                return NotFound();
            }

            return View(merchandise);
        }







        [Authorize]
        // GET: Merchandises/Create
        public IActionResult Create()
        {
            return View();
        }






        // POST: Merchandises/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,qty")] Merchandise merchandise)
        {
            if (ModelState.IsValid)
            {
                _context.Add(merchandise);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(merchandise);
        }






        // GET: Merchandises/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var merchandise = await _context.Merchandise.FindAsync(id);
            if (merchandise == null)
            {
                return NotFound();
            }
            return View(merchandise);
        }

        // POST: Merchandises/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,qty")] Merchandise merchandise)
        {
            if (id != merchandise.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(merchandise);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MerchandiseExists(merchandise.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(merchandise);
        }






        // GET: Merchandises/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var merchandise = await _context.Merchandise
                .FirstOrDefaultAsync(m => m.Id == id);
            if (merchandise == null)
            {
                return NotFound();
            }

            return View(merchandise);
        }

        // POST: Merchandises/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var merchandise = await _context.Merchandise.FindAsync(id);
            _context.Merchandise.Remove(merchandise);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }




        private bool MerchandiseExists(int id)
        {
            return _context.Merchandise.Any(e => e.Id == id);
        }
    }
}
