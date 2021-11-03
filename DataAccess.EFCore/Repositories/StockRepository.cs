using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Domain.Interfaces;
using System.Linq;

namespace DataAccess.EFCore.Repositories
{
    public class StockRepository : GenericRepository<StockItem>, IStockRepository
    {
        public StockRepository(ApplicationContext context) : base(context)
        {
        }
        public int GetFullStockValue()
        {
            return _context.StockItems.ToList().Sum(x => x.Count * x.Id);
        }
    }
}
