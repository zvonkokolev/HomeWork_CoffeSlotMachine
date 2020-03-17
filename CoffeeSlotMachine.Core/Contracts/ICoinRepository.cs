using CoffeeSlotMachine.Core.Entities;
using System.Collections.Generic;

namespace CoffeeSlotMachine.Core.Contracts
{
    public interface ICoinRepository
    {
      public IEnumerable<Coin> GetAllCoins();
      public void AddCoins(IDictionary<int, int> coins);
      public void PayBackCoins(IDictionary<int, int> coins);
    }
}