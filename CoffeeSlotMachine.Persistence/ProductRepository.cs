﻿using CoffeeSlotMachine.Core.Contracts;
using CoffeeSlotMachine.Core.Entities;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace CoffeeSlotMachine.Persistence
{
	public class ProductRepository : IProductRepository
	{
		private readonly ApplicationDbContext _dbContext;

		public ProductRepository(ApplicationDbContext dbContext)
		{
			_dbContext = dbContext;
		}
		public IEnumerable<Product> GetAllProductsSortedByName() => _dbContext.Products
														.OrderBy(n => n.Name)
														.ToArray();
		public Product GetProductById(int id) => _dbContext.Products.Find(id);
	}
}
