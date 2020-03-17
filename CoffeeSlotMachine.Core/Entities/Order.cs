using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Text;

namespace CoffeeSlotMachine.Core.Entities
{
	/// <summary>
	/// Bestellung verwaltet das bestellte Produkt, die eingeworfenen Münzen und
	/// die Münzen die zurückgegeben werden.
	/// </summary>
	public class Order : EntityObject
	{
		private int _thrownInCents;
		private int _retunCents;
		private int _donationsCents;
		private StringBuilder sb = new StringBuilder();
		private List<int> _validCoins = new List<int>{ 5, 10, 20, 50, 100, 200 };
		private int[] _actualCoinsInput = new int[6];
		public IDictionary<int, int> _coins = new Dictionary<int, int>
		{
			{   5, 0 },
			{  10, 0 },
			{  20, 0 },
			{  50, 0 },
			{ 100, 0 },
			{ 200, 0 }
		};
		public IDictionary<int, int> _retourCoins = new Dictionary<int, int>
		{
			{   5, 0 },
			{  10, 0 },
			{  20, 0 },
			{  50, 0 },
			{ 100, 0 },
			{ 200, 0 }
		};

		/// <summary>
		/// Datum und Uhrzeit der Bestellung
		/// </summary>
		public DateTime Time { get; set; }

		/// <summary>
		/// Werte der eingeworfenen Münzen als Text. Die einzelnen 
		/// Münzwerte sind durch ; getrennt (z.B. "10;20;10;50")
		/// </summary>
		public String ThrownInCoinValues { get; set; }

		/// <summary>
		/// Zurückgegebene Münzwerte mit ; getrennt
		/// </summary>
		public String ReturnCoinValues { get; set; }

		/// <summary>
		/// Summe der eingeworfenen Cents.
		/// </summary>
		public int ThrownInCents => _thrownInCents;

		/// <summary>
		/// Summe der Cents die zurückgegeben werden
		/// </summary>
		public int ReturnCents => _retunCents;

		public int ProductId { get; set; }

		[ForeignKey(nameof(ProductId))]
		public Product Product { get; set; }

		/// <summary>
		/// Kann der Automat mangels Kleingeld nicht
		/// mehr herausgeben, wird der Rest als Spende verbucht
		/// </summary>
		public int DonationCents => _donationsCents;
		/// <summary>
		/// Münze wird eingenommen.
		/// </summary>
		/// <param name="coinValue"></param>
		/// <returns>isFinished ist true, wenn der Produktpreis zumindest erreicht wurde</returns>
		public bool InsertCoin(int coinValue)
		{
			bool check = false;
			if (coinValue == 1 || coinValue == 2)
			{  // converts 1 & 2 Euro coins in cents
				coinValue *= 100;
			}  // sum actual in depot
			_thrownInCents += coinValue;
			if (ThrownInCents < Product.PriceInCents)
			{
				_coins[coinValue] ++;
				// converts digits into character
				ThrownInCoinValues = AddIntToNumbersText(coinValue);
				return check;
			}
			else
			{	// increment amount of coins per value - Muenzwerte in aktuellen einwuerf um Eins erhoehen
				_coins[coinValue]++;
				// converts digits into character
				ThrownInCoinValues = AddIntToNumbersText(coinValue);
				check = true;
			}
			return check;
		}
		public string AddIntToNumbersText(int coinValue)
		{
			if(sb.Length == 0)
				sb.Append(coinValue.ToString());
			else
				sb.AppendJoin(';', coinValue.ToString());
			return sb.ToString();
		}

		/// <summary>
		/// Übernahme des Einwurfs in das Münzdepot.
		/// Rückgabe des Retourgeldes aus der Kasse. Staffelung des Retourgeldes
		/// hängt vom Inhalt der Kasse ab.
		/// </summary>
		/// <param name="coins">Aktueller Zustand des Münzdepots</param>
		public void FinishPayment(IEnumerable<Coin> coins)
		{
			StringBuilder sb1 = new StringBuilder();
			_donationsCents = 0;
			// calculate rest of money in cents
			_retunCents = ThrownInCents - Product.PriceInCents;
			int temp = ReturnCents;
			foreach (var item in coins)
			{
				if(item.Amount != 0)
				{
					if (temp / item.CoinValue >= 1)
					{  //search and sort rest of money array
						while (temp >= item.CoinValue)
						{
							if (sb1.Length == 0)
								sb1.Append(item.CoinValue.ToString());
							else
								sb1.Append(';' + item.CoinValue.ToString());
							temp -= item.CoinValue;
							_retourCoins[item.CoinValue]++;
						}
					}
				}
			}
			ReturnCoinValues = sb1.ToString();
			_donationsCents = temp;
		}
	}
}
