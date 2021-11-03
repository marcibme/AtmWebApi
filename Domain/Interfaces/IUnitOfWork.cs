using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IStockRepository Stock { get; }
        int Complete();
    }
}
