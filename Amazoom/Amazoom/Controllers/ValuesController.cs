using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace Amazoom.Controllers
{
    /*
    [Route("api/[controller]")]
    [ApiController]

    public class ValuesController : ControllerBase
    {
        [HttpGet]
        public ActionResult<string> Get()
        {
            string text = "";
            Program.MainForm.Invoke(new Action(() =>
            {
                //text = Program.MainForm.textBox1.Text;
            }));
            return text;
        }

        [HttpGet("{id}")]
        public ActionResult Get(string id)
        {
            Program.MainForm.Invoke(new Action(() =>
            {
                Program.MainForm.InitializeItems();
            }));
            return Ok();
        }
    }
    */
    [Route("placeorder")]
    public class PlaceOrderController : Controller
    {
        [HttpGet("{id}")]
        public ActionResult Get(string id)
        {
            string response = "";
            string[] order_parsed = id.Split('-');
            foreach(var strings in order_parsed)
            {
                if(strings.Length <= 0)
                {
                    return NoContent();
                }
                strings.Trim('-');
            }
            Console.WriteLine("Order placed for " + id);
            Program.MainForm.Invoke(new Action(() =>
            {
                response = Program.MainForm.PlaceOrder(order_parsed[0].ToLower(), Int32.Parse(order_parsed[1]), order_parsed[2]); 
            }));
            return Ok(response);
        }

    }

    [Route("updateList")]
    public class updateListController : Controller
    {
        [HttpGet]
        public ActionResult Get()
        {
            string response = "";
            Program.MainForm.Invoke(new Action(() =>
            {
                response = Program.MainForm.UpdateList();
            }));
            return Ok(response);
        }

    }

    [Route("restock")]
    public class RestockController : ControllerBase
    {
        [HttpGet("{id}")]
        public ActionResult Get(string id)
        {
            string[] order_parsed = id.Split('-');
            Console.WriteLine(order_parsed);
            Program.MainForm.Invoke(new Action(() =>
            {
                Program.MainForm.RestockItem(order_parsed[0], Int32.Parse(order_parsed[1]));
            }));
            return Ok("Sick Bro");
        }
    }

    [Route("stocklevel")]
    public class StockLevelController : ControllerBase
    {
        [HttpGet("{id}")]
        public ActionResult Get(string id)
        {
            int stockLevel = 0;
            Program.MainForm.Invoke(new Action(() =>
            {
                stockLevel = Program.MainForm.getItemStockLevel(id);
            }));
            return Ok(stockLevel);
        }

    }


}