using System;
using System.Collections.Generic;

namespace CoinKombine
{
    class MainClass
    {


        
        public static void Main(string[] args)
        {
            var start = new List<int>();
            for (var i = 0; i < 200; i++){
                start.Add(1);
            }
            var combiner = new Combiner(200);
            combiner.Combine(start);
            Console.WriteLine(combiner.found.ToString());
            Console.ReadLine();
        }
    }
}
