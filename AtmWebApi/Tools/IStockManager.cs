using System.Collections.Generic;

namespace AtmWebApi.Tools
{
    public interface IStockManager
    {
        int Deposit(Dictionary<int, int> itemsToDeposit);
        Dictionary<int, int> Withdrawal(int withdrawValue);

#region helper functions
        Dictionary<int, int> GetStock();
        void ResetStock();
#endregion helper functions
    }
}
