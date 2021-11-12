using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using AtmWebApi.Tools;

namespace AtmWebApi.Controllers
{}
    [ApiController]
    public class StockController : ControllerBase
    {
        private readonly IStockManager _stockManager;

        public StockController(IStockManager stockManager)
        {
            _stockManager = stockManager;
        }

        [HttpPost]
        [Route("api/deposit")]
        public ActionResult<int> Deposit(Dictionary<int, int> itemsToDeposit)
        {
            try
            {
                return Ok(_stockManager.Deposit(itemsToDeposit));
            }
            catch(Exception)
            {
                return BadRequest();
            }
        }


        [HttpPost]
        [Route("api/withdrawal")]
        public ActionResult<Dictionary<int, int>> Withdrawal([FromBody] int withdrawValue)
        {
            try
            {
                return Ok(_stockManager.Withdrawal(withdrawValue));
            }
            catch(ArgumentException)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable);
            }
            catch(Exception)
            {
                return BadRequest();
            }
        }

#region helper functions
        [HttpGet]
        [Route("api/stock")]
        public ActionResult<IEnumerable<StockItem>> GetStockItems()
        {
            try
            {
                return Ok(_stockManager.GetStock());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("api/reset")]
        public ActionResult ResetStock()
        {
            try
            {
                _stockManager.ResetStock();
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
#endregion helper functions

    }
}
