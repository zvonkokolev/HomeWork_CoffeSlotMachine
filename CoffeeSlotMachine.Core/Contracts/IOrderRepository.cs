using CoffeeSlotMachine.Core.Entities;
using System.Collections.Generic;

namespace CoffeeSlotMachine.Core.Contracts
{
	public interface IOrderRepository
	{
		public IEnumerable<Order> GetAllWithProduct();
		public void AddOrderIntoDB(Order order);
	}
}