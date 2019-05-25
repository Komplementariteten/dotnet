using System;
using System.Collections.Generic;

namespace CoinKombine
{
    public class Combiner
    {
		private List<int> coins = new List<int>() { 1, 2, 5, 10, 20, 50, 100, 200 };
		private Dictionary<string, List<int>> pairs = new Dictionary<string, List<int>>();
		private int CurrentCoin = 1;
        private int position = 0;
        private int maxSum = 0;
        public int found = 1;

        private int SumCoins(List<int> coinList){
            var sum = 0;
            foreach(var coin in coinList){
                sum += coin;
            }
            return sum;
        }

        private void FillMissing(List<int> lastList, int curPosi, int coin) {
            
        }

        private List<int> NextCoinKombination(List<int> lastList){
            var count = 0;
            var newList = new List<int>();
            for (var i = 0; i < lastList.Count; i++)
            {
                if (i == position)
                {
                    count += coins[CurrentCoin];
                    newList.Add(coins[CurrentCoin]);
                }
                else if(count < maxSum)
                {
                    count += lastList[i];
                    newList.Add(lastList[i]);
                }
                else {
                    position++;
                    found++;
                    Console.WriteLine(String.Join(", ", newList));
                    return newList;
                }
            }
            throw new Exception("This should never happen!");
        }

		public void Combine(List<int> lastList)
		{
			if ((maxSum / coins[CurrentCoin]) == lastList.Count)
			{
				CurrentCoin++;
				position = 0;
			}
			if (coins.Count == CurrentCoin)
			{
				return;
			}
            var newList = NextCoinKombination(lastList);

            Combine(newList);
		}

        public Combiner(int sum)
        {
            maxSum = sum;
        }
    }
}
