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

        private readonly IConfigManager _configManager;
        private readonly IUnitOfWork _unitOfWork;
        public StockManager(IUnitOfWork unitOfWork, IConfigManager configManager)
        {
            _unitOfWork = unitOfWork;
            _configManager = configManager;
        }

        public int Deposit(Dictionary<int, int> itemsToDeposit)
        {
            LogBanknotes(itemsToDeposit, "Banknotes to deposit:");
            foreach (var itemToDeposit in itemsToDeposit)
            {
                if (itemsToDeposit.Count < 0)
                {
                    ThrowExceptionWithConsoleLog(new Exception($"Not accepted amount: {itemToDeposit.Key}:{itemsToDeposit.Count}"));
                }

                if (!_configManager.IsBanknoteAccepted(itemToDeposit.Key))
                {
                    ThrowExceptionWithConsoleLog(new Exception($"Not accepted banknote: {itemToDeposit.Key}"));
                }

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

            if (withdrawValue <= 0)
            {
                ThrowExceptionWithConsoleLog(new Exception("Withdraw value incorrect."));
            }

            if (withdrawValue % 1000 != 0)
            {
                ThrowExceptionWithConsoleLog(new Exception("Smallest unit is 1000."));
            }

            if (withdrawValue > _unitOfWork.Stock.GetFullStockValue())
            {
                ThrowExceptionWithConsoleLog(new ArgumentException("Insuficcient money in stock."));
            }

            var itemsToWithdraw = RetrieveBanknotesFromStockToWithdraw(withdrawValue);

            if (itemsToWithdraw != null)
                return itemsToWithdraw;

            Console.WriteLine("Insuficcient money in stock.");
            throw new ArgumentException("Insuficcient money in stock.");
        }

        private Dictionary<int, int> RetrieveBanknotesFromStockToWithdraw(int withdrawValue)
        {
            var availableBanknotes = _unitOfWork.Stock.GetItemsCountGreaterThanZeroDescendingById();

            Dictionary<int, int> returnValue = new Dictionary<int, int>();
            foreach (var banknote in availableBanknotes)
            {
                if (withdrawValue >= banknote.Id)
                {
                    int requiredCountofBanknote = withdrawValue / banknote.Id;

                    if (banknote.Count < requiredCountofBanknote)
                        requiredCountofBanknote = banknote.Count;

                    banknote.Count -= requiredCountofBanknote;

                    returnValue.Add(banknote.Id, requiredCountofBanknote);
                    withdrawValue -= (banknote.Id * requiredCountofBanknote);
                }

                if (withdrawValue == 0)
                {
                    _unitOfWork.Complete();
                    LogBanknotes(returnValue, "Banknotes to withdraw:");
                    return returnValue;
                }
            }
            return null;
        }

        #region helper functions

        private void LogBanknotes(Dictionary<int, int> items, string message)
        {
            Console.WriteLine(message);
            foreach (var item in items)
            {
                Console.WriteLine($"Banknote:{item.Key} : {item.Value}");
            }
        }

        private void ThrowExceptionWithConsoleLog(Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw ex;
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
