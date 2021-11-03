using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace AtmWebApi.Controllers
{
    //[Route("api/[controller]")]
    [ApiController]
    public class StockController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;
        private readonly int[] allowedBanknotes;
        public StockController(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;

            if (allowedBanknotes == null)
                allowedBanknotes = _configuration.GetSection("AllowedBanknotes").Get<int[]>();
        }


        [HttpGet]
        [Route("api/stock")]
        public ActionResult<IEnumerable<StockItem>> GetStockItems()
        {
            Dictionary<int, int> retValue = _unitOfWork.Stock.GetAll().ToDictionary(x => x.Id, y => y.Count);
            return Ok(retValue);
        }

        [HttpPost]
        [Route("api/deposit")]
        public ActionResult<int> Deposit(Dictionary<int, int> itemsToDeposit)
        {
            foreach (var itemToDeposit in itemsToDeposit)
            {
                if (!allowedBanknotes.Contains(itemToDeposit.Key))
                    return BadRequest($"Not accepted banknote: {itemToDeposit.Key}");

                var existingItem = _unitOfWork.Stock.GetById(itemToDeposit.Key);

                if (existingItem == null)
                {
                    _unitOfWork.Stock.Add(new StockItem() { Id = itemToDeposit.Key, Count = itemToDeposit.Value });
                }
                else
                {
                    existingItem.Count += itemToDeposit.Value;
                }
            }

            _unitOfWork.Complete();

            return Ok(_unitOfWork.Stock.GetFullStockValue());
        }

        [HttpPost]
        [Route("api/withdrawal")]
        public ActionResult<Dictionary<int, int>> Withdrawal(int withdrawValue)
        {
            return Ok();
        }

    }
}
