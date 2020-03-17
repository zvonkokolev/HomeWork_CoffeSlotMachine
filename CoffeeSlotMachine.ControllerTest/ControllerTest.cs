using CoffeeSlotMachine.Core.Logic;
using CoffeeSlotMachine.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace CoffeeSlotMachine.ControllerTest
{
	[TestClass]
	public class ControllerTest
	{
		[TestInitialize]
		public void MyTestInitialize()
		{
			using (ApplicationDbContext applicationDbContext = new ApplicationDbContext())
			{
				applicationDbContext.Database.EnsureDeleted();
				applicationDbContext.Database.Migrate();
			}
		}


		[TestMethod]
		public void T01_GetCoinDepot_CoinTypesCount_ShouldReturn6Types_3perType_SumIs1155Cents()
		{
			using (OrderController controller = new OrderController())
			{
				var depot = controller.GetCoinDepot().ToArray();
				Assert.AreEqual(6, depot.Count(), "Sechs Münzarten im Depot");
				foreach (var coin in depot)
				{
					Assert.AreEqual(3, coin.Amount, "Je Münzart sind drei Stück im Depot");
				}

				int sumOfCents = depot.Sum(coin => coin.CoinValue * coin.Amount);
				Assert.AreEqual(1155, sumOfCents, "Beim Start sind 1155 Cents im Depot");
			}
		}

		[TestMethod]
		public void T02_GetProducts_9Products_FromCappuccinoToRistretto()
		{
			using (OrderController statisticsController = new OrderController())
			{
				var products = statisticsController.GetProducts().ToArray();
				Assert.AreEqual(9, products.Length, "Neun Produkte wurden erzeugt");
				Assert.AreEqual("Cappuccino", products[0].Name);
				Assert.AreEqual("Ristretto", products[8].Name);
			}
		}

		[TestMethod]
		public void T03_BuyOneCoffee_OneCoinIsEnough_CheckCoinsAndOrders()
		{
			using (OrderController controller = new OrderController())
			{
				var products = controller.GetProducts();
				var product = products.Single(p => p.Name == "Cappuccino");
				var order = controller.OrderCoffee(product);
				bool isFinished = controller.InsertCoin(order, 100);
				Assert.AreEqual(true, isFinished, "100 Cent genügen");
				Assert.AreEqual(100, order.ThrownInCents, "Einwurf stimmt nicht");
				Assert.AreEqual(100 - product.PriceInCents, order.ReturnCents);
				Assert.AreEqual(0, order.DonationCents);
				Assert.AreEqual("20;10;5", order.ReturnCoinValues);

				// Depot überprüfen
				var coins = controller.GetCoinDepot().ToArray();
				int sumOfCents = coins.Sum(c => c.CoinValue * c.Amount);
				Assert.AreEqual(1220, sumOfCents, "Beim Start sind 1155 Cents + 65 Cents für Cappuccino");
				Assert.AreEqual("3*200 + 4*100 + 3*50 + 2*20 + 2*10 + 2*5", controller.GetCoinDepotString());

				var orders = controller.GetAllOrdersWithProduct().ToArray();
				Assert.AreEqual(1, orders.Length, "Es ist genau eine Bestellung");
				Assert.AreEqual(0, orders[0].DonationCents, "Keine Spende");
				Assert.AreEqual(100, orders[0].ThrownInCents, "100 Cents wurden eingeworfen");
				Assert.AreEqual("Cappuccino", orders[0].Product.Name, "Produktname Cappuccino");
			}
		}

		[TestMethod]
		public void T04_BuyOneCoffee_ExactThrowInOneCoin_CheckCoinsAndOrders()
		{
			using (OrderController controller = new OrderController())
			{
				var products = controller.GetProducts();
				var product = products.Single(p => p.Name == "Espresso");
				var order = controller.OrderCoffee(product);
				bool isFinished = controller.InsertCoin(order, 50);
				Assert.AreEqual(true, isFinished, "50 Cent genügen");
				Assert.AreEqual(50, order.ThrownInCents, "Einwurf stimmt nicht");
				Assert.AreEqual(50 - product.PriceInCents, order.ReturnCents);
				Assert.AreEqual(0, order.DonationCents);
				Assert.AreEqual("", order.ReturnCoinValues);

				// Depot überprüfen
				var coins = controller.GetCoinDepot().ToArray();
				int sumOfCents = coins.Sum(c => c.CoinValue * c.Amount);
				Assert.AreEqual(1205, sumOfCents, "Beim Start sind 1155 Cents + 50 Cents für Cappuccino");
				Assert.AreEqual("3*200 + 3*100 + 4*50 + 3*20 + 3*10 + 3*5", controller.GetCoinDepotString());

				var orders = controller.GetAllOrdersWithProduct().ToArray();
				Assert.AreEqual(1, orders.Length, "Es ist genau eine Bestellung");
				Assert.AreEqual(0, orders[0].DonationCents, "Keine Spende");
				Assert.AreEqual(50, orders[0].ThrownInCents, "50 Cents wurden eingeworfen");
				Assert.AreEqual("Espresso", orders[0].Product.Name, "Produktname Espresso");
			}
		}

		[TestMethod]
		public void T05_BuyOneCoffee_MoreCoins_CheckCoinsAndOrders()
		{
			using (OrderController controller = new OrderController())
			{
				var products = controller.GetProducts();
				var product = products.Single(p => p.Name == "Doppio");
				var order = controller.OrderCoffee(product);
				controller.InsertCoin(order, 50);
				controller.InsertCoin(order, 20);
				bool isFinished = controller.InsertCoin(order, 10);
				Assert.AreEqual(true, isFinished, "80 Cent genügen");
				Assert.AreEqual(80, order.ThrownInCents, "Einwurf stimmt nicht");
				Assert.AreEqual(80 - product.PriceInCents, order.ReturnCents);
				Assert.AreEqual(0, order.DonationCents);
				Assert.AreEqual("", order.ReturnCoinValues);

				// Depot überprüfen
				var coins = controller.GetCoinDepot().ToArray();
				int sumOfCents = coins.Sum(c => c.CoinValue * c.Amount);
				Assert.AreEqual(1235, sumOfCents, "Beim Start sind 1155 Cents + 80 Cents für Cappuccino");
				Assert.AreEqual("3*200 + 3*100 + 4*50 + 4*20 + 4*10 + 3*5", controller.GetCoinDepotString());

				var orders = controller.GetAllOrdersWithProduct().ToArray();
				Assert.AreEqual(1, orders.Length, "Es ist genau eine Bestellung");
				Assert.AreEqual(0, orders[0].DonationCents, "Keine Spende");
				Assert.AreEqual(80, orders[0].ThrownInCents, "80 Cents wurden eingeworfen");
				Assert.AreEqual("Doppio", orders[0].Product.Name, "Produktname Doppio");
			}
		}


		[TestMethod()]
		public void T06_BuyMoreCoffees_OneCoins_CheckCoinsAndOrders()
		{
			using (OrderController controller = new OrderController())
			{
				var products = controller.GetProducts();

				var product = products.Single(p => p.Name == "Latte");
				var order = controller.OrderCoffee(product);
				bool isFinished = controller.InsertCoin(order, 50);

				product = products.Single(p => p.Name == "Espresso");
				order = controller.OrderCoffee(product);
				isFinished = controller.InsertCoin(order, 50);

				product = products.Single(p => p.Name == "Latte");
				order = controller.OrderCoffee(product);
				isFinished = controller.InsertCoin(order, 50);
				
				Assert.AreEqual(50, order.ThrownInCents, "Einwurf stimmt nicht");
				Assert.AreEqual(0, order.DonationCents);
				Assert.AreEqual("", order.ReturnCoinValues);

				// Depot überprüfen
				var coins = controller.GetCoinDepot().ToArray();
				int sumOfCents = coins.Sum(c => c.CoinValue * c.Amount);
				Assert.AreEqual(1305, sumOfCents, "Beim Start sind 1155 Cents + 150 Cents für Cappuccino");
				Assert.AreEqual("3*200 + 3*100 + 6*50 + 3*20 + 3*10 + 3*5", controller.GetCoinDepotString());

				var orders = controller.GetAllOrdersWithProduct().ToArray();
				Assert.AreEqual(3, orders.Length, "Es sind 3 Bestellung");
				Assert.AreEqual(0, orders[0].DonationCents, "Keine Spende");
			}
		}


		[TestMethod()]
		public void T07_BuyMoreCoffees_UntilDonation_CheckCoinsAndOrders()
		{
			using (OrderController controller = new OrderController())
			{
				var products = controller.GetProducts();

				var product = products.Single(p => p.Name == "Lungo");
				var order = controller.OrderCoffee(product);
				controller.InsertCoin(order, 50);
				bool isFinished = controller.InsertCoin(order, 20);

				product = products.Single(p => p.Name == "Lungo");
				order = controller.OrderCoffee(product);
				controller.InsertCoin(order, 50);
				controller.InsertCoin(order, 10);
				isFinished = controller.InsertCoin(order, 10);
				Assert.AreEqual(0, order.DonationCents);
				Assert.AreEqual("5", order.ReturnCoinValues);

				product = products.Single(p => p.Name == "Lungo");
				order = controller.OrderCoffee(product);
				controller.InsertCoin(order, 50);
				controller.InsertCoin(order, 10);
				isFinished = controller.InsertCoin(order, 10);

				product = products.Single(p => p.Name == "Lungo");
				order = controller.OrderCoffee(product);
				controller.InsertCoin(order, 50);
				controller.InsertCoin(order, 10);
				isFinished = controller.InsertCoin(order, 10);

				Assert.AreEqual(70, order.ThrownInCents, "Einwurf stimmt nicht");
				Assert.AreEqual(5, order.DonationCents);
				Assert.AreEqual("", order.ReturnCoinValues);


				// Depot überprüfen
				var coins = controller.GetCoinDepot().ToArray();
				int sumOfCents = coins.Sum(c => c.CoinValue * c.Amount);
				Assert.AreEqual(1420, sumOfCents, "Beim Start sind 1155 Cents + 265 Cents für 4 mal Lungo + 5 Cent Spende");
				Assert.AreEqual("3*200 + 3*100 + 7*50 + 4*20 + 9*10 + 0*5", controller.GetCoinDepotString());

				var orders = controller.GetAllOrdersWithProduct().ToArray();
				Assert.AreEqual(4, orders.Length, "Es sind 4 Bestellung");
			}
		}

	}
}
