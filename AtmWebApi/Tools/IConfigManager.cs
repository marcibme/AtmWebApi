using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AtmWebApi.Tools
{
    public interface IConfigManager
    {
        bool IsBanknoteAccepted(int bankNote);
        List<int> GetAcceptedBanknotesDescending();
    }
}
