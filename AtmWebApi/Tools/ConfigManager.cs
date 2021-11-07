using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AtmWebApi.Tools
{
    public class ConfigManager : IConfigManager
    {
        private readonly IConfiguration _configuration;
        public static string ACCEPTED_BANKNOTES_SETTING = "AcceptedBanknotes";

        public ConfigManager(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public List<int> GetAcceptedBanknotesDescending()
        {
            var acceptedBanknotes = _configuration.GetSection(ACCEPTED_BANKNOTES_SETTING).Get<int[]>();
            return acceptedBanknotes.OrderByDescending(x => x).ToList();
        }

        public bool IsBanknoteAccepted(int bankNote)
        {
            var acceptedBanknotes = _configuration.GetSection(ACCEPTED_BANKNOTES_SETTING).Get<int[]>();
            return acceptedBanknotes.Contains(bankNote);
        }
    }
}
