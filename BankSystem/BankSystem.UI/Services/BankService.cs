using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BankSystem.BankClient.Models;
using System.Net.Http.Json;

namespace BankSystem.BankClient.Services
{
    public class BankService : IBankService
    {
        private readonly HttpClient _httpClient;

        public BankService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<Bank>> GetBanksAsync()
        {
            var response = await _httpClient.GetFromJsonAsync<List<Bank>>("api/banks");
            return response ?? new List<Bank>();
        }
    }
}
