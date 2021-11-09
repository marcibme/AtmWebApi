using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Interfaces
{
    public interface IStockRepository : IGenericRepository<StockItem>
    {
        int GetFullStockValue();
        List<StockItem> GetItemsCountGreaterThanZeroDescendingById();
    }
}
