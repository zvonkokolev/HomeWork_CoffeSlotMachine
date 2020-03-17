using CoffeeSlotMachine.Core.Contracts;
using CoffeeSlotMachine.Core.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoffeeSlotMachine.Persistence
{
	public class CoinRepository : ICoinRepository
	{
		private readonly ApplicationDbContext _dbContext;

		public CoinRepository(ApplicationDbContext dbContext)
		{
			_dbContext = dbContext;
		}
		public IEnumerable<Coin> GetAllCoins() => _dbContext.Coins;

		public void AddCoins(IDictionary<int, int> coins)
		{
			foreach (var item in coins)
			{
				var c = _dbContext.Coins.SingleOrDefault(c => c.CoinValue == item.Key);
				c.Amount += item.Value;	
			}
			_dbContext.SaveChanges();
		}
		public void PayBackCoins(IDictionary<int, int> coins)
		{
			foreach (var item in coins)
			{
				var c = _dbContext.Coins.SingleOrDefault(c => c.CoinValue == item.Key);
				c.Amount -= item.Value;
			}
			_dbContext.SaveChanges();
		}
	}
}
