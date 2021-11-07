using AtmWebApi.Tools;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace AtmWbApiTests
{
    public class StockManagerTests
    {
        private readonly StockManager _sm;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new Mock<IUnitOfWork>();
        private readonly Mock<IConfigManager> _configMock = new Mock<IConfigManager>();
        public StockManagerTests()
        {
            _sm = new StockManager(_unitOfWorkMock.Object, _configMock.Object);
        }

        [Fact]
        public void Deposit_ShouldThrowException_OnNotAcceptedBanknoteInput()
        {
            Dictionary<int, int> itemsToDeposit = new Dictionary<int, int>() { { 100, 2 }, { 2000, 3 } };
            List<int> acceptedBanknotes = new List<int>() { 20000, 10000, 5000, 2000, 1000 };
            _configMock.Setup(x => x.GetAcceptedBanknotesDescending())
                .Returns(acceptedBanknotes);

            Assert.Throws<Exception>(
                () => _sm.Deposit(itemsToDeposit)
                );
        }

        [Fact]
        public void Deposit_ShouldThrowException_OnNotAcceptedAmountInput()
        {
            Dictionary<int, int> itemsToDeposit = new Dictionary<int, int>() { { 1000, -2 }, { 2000, 3 } };
            List<int> acceptedBanknotes = new List<int>() { 20000, 10000, 5000, 2000, 1000 };
            _configMock.Setup(x => x.GetAcceptedBanknotesDescending())
                .Returns(acceptedBanknotes);

            Assert.Throws<Exception>(
                () => _sm.Deposit(itemsToDeposit)
                );
        }

        [Theory]
        [InlineData(-5)]
        [InlineData(345)]
        public void Withdrawal_ShouldReturnExpeception_WhenWithdrawValueNotAccepted(int withdrawValue)
        {
            Assert.Throws<Exception>(() => _sm.Withdrawal(withdrawValue));
        }

        [Theory]
        [MemberData(nameof(TestDataForSuccess))]
        public void Withdrawal_ShouldReturnExpectedValues(List<StockItem> stock, int withdrawValue, Dictionary<int, int> expected)
        {
            List<int> acceptedBanknotes = new List<int>() { 20000, 10000, 5000, 2000, 1000 };
            _configMock.Setup(x => x.GetAcceptedBanknotesDescending())
                .Returns(acceptedBanknotes);
            _unitOfWorkMock.Setup(x => x.Stock.GetFullStockValue()).Returns(stock.Sum(s => s.Count * s.Id));

            foreach (var item in stock)
            {
                _unitOfWorkMock.Setup(x => x.Stock.GetById(item.Id)).Returns(stock.Find(x => x.Id == item.Id));
            }

            Assert.Equal(expected, _sm.Withdrawal(withdrawValue));
        }

        public static IEnumerable<object[]> TestDataForSuccess()
        {
            List<StockItem> stock = new List<StockItem>()
            { new StockItem()  { Id = 1000, Count = 10 },
               new StockItem() { Id = 5000, Count = 12 },
               new StockItem() { Id = 20000, Count = 1 }
            };

            yield return new object[] { stock, 1000
            , new Dictionary<int,int>() {{ 1000, 1 } }};

            yield return new object[] { stock, 4000
            , new Dictionary<int,int>() {{ 1000, 4 } }};

            yield return new object[] { stock, 6000
            , new Dictionary<int,int>() {{ 5000, 1 },{ 1000, 1 } }};

            yield return new object[] { stock, 47000
            , new Dictionary<int,int>() { { 20000, 1 }, { 5000, 5 },{ 1000, 2 } }};
        }
    }
}
