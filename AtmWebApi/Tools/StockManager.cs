using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AtmWebApi.Tools
{
    public class StockManager : IStockManager
    {
        private static string ACCEPTED_BANKNOTES_SETTING = "AcceptedBanknotes";

        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;
        public StockManager(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        public int Deposit(Dictionary<int, int> itemsToDeposit)
        {
            LogBanknotes(itemsToDeposit, "Banknotes to deposit:");
            foreach (var itemToDeposit in itemsToDeposit)
            {
                if (!IsBanknoteAccepted(itemToDeposit.Key))
                    throw new Exception($"Not accepted banknote: {itemToDeposit.Key}");

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

            var stockValue = _unitOfWork.Stock.GetFullStockValue();
            Console.WriteLine($"Value of stock:{stockValue}");
            return stockValue;
        }

        public Dictionary<int, int> Withdrawal(int withdrawValue)
        {
            Console.WriteLine($"Withdrawing:{withdrawValue}");
            Dictionary<int, int> returnValue = new Dictionary<int, int>();

            if (withdrawValue > _unitOfWork.Stock.GetFullStockValue())
            {
                Console.WriteLine("Insuficcient money in stock.");
                throw new ArgumentException("Insuficcient money in stock.");
            }

            if (withdrawValue % 1000 != 0)
            {
                Console.WriteLine("Smallest unit is 1000.");
                throw new Exception("Smallest unit is 1000.");
            }

            var acceptedBanknotes = GetAcceptedBanknotesDescending();

            foreach (var banknote in acceptedBanknotes)
            {
                if (withdrawValue >= banknote)
                {
                    int requiredCountofBanknote = withdrawValue / banknote;
                    var banknotesAvaiable = _unitOfWork.Stock.GetById(banknote);

                    if (banknotesAvaiable == null)
                        continue;

                    if (banknotesAvaiable.Count < requiredCountofBanknote)
                        requiredCountofBanknote = banknotesAvaiable.Count;

                    banknotesAvaiable.Count -= requiredCountofBanknote;

                    returnValue.Add(banknote, requiredCountofBanknote);
                    withdrawValue -= (banknote * requiredCountofBanknote);
                }

                if (withdrawValue == 0)
                {
                    _unitOfWork.Complete();
                    LogBanknotes(returnValue, "Banknotes to withdraw:");
                    return returnValue;
                }
            }

            Console.WriteLine("Insuficcient money in stock.");
            throw new ArgumentException("Insuficcient money in stock.");
        }

        private bool IsBanknoteAccepted(int bankNote)
        {
            var acceptedBanknotes = _configuration.GetSection(ACCEPTED_BANKNOTES_SETTING).Get<int[]>();
            return acceptedBanknotes.Contains(bankNote);
        }

        private List<int> GetAcceptedBanknotesDescending()
        {
            var acceptedBanknotes = _configuration.GetSection(ACCEPTED_BANKNOTES_SETTING).Get<int[]>();
            return acceptedBanknotes.OrderByDescending(x => x).ToList();
        }

        #region helper functions

        private void LogBanknotes(Dictionary<int, int> items, string message)
        {
            Console.WriteLine(message);
            foreach(var item in items)
            {
                Console.WriteLine($"Banknote:{item.Key} : {item.Value}");
            }
        }

        public Dictionary<int, int> GetStock()
        {
            return _unitOfWork.Stock.GetAll().ToDictionary(x => x.Id, y => y.Count);
        }

        public void ResetStock()
        {
            _unitOfWork.Stock.RemoveRange(_unitOfWork.Stock.GetAll());
            _unitOfWork.Complete();
        }
        #endregion helper functions
    }
}
