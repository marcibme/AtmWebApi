using DataAccess.EFCore.Repositories;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.EFCore
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationContext _context;
        public UnitOfWork(ApplicationContext context)
        {
            _context = context;
            Stock = new StockRepository(_context);
        }
        public IStockRepository Stock { get; private set; }
        public int Complete()
        {
            return _context.SaveChanges();
        }
        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
