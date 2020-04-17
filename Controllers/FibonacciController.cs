using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using HAN.Demo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace HAN.Demo.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize]
    public class FibonacciController : Controller
    {
        IMemoryCache _cache;

        public FibonacciController(IMemoryCache cache)
        {
            _cache = cache;
        }

        /// <summary>
        /// A recursive and exponential implementation of Fibonacci
        /// </summary>
        /// <param name="i">index to calculate</param>
        /// <returns>The corresponding Fibonacci number</returns>
        public ulong Fibonacci(ulong i)
        {
            if (i < 0)
                throw new ArgumentOutOfRangeException("Fibonacci is only defined for numbers equal or greater than 0");

            if (i == 0)
                return 0;

            if (i <= 2)
                return 1;

            Task.Delay(10).Wait();

            ulong result;
            result = Fibonacci(i - 1) + Fibonacci(i - 2);

            return result;
        }

        public IActionResult Index(int index = 1)
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            
            ulong number = Fibonacci((ulong)index);
            
            stopwatch.Stop();


            return View(new FibonnaciModel()
            {
                Index = index,
                Number = number,
                CalculationTime = stopwatch.Elapsed
            });
        }

        [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new string[] { "index" })]
        public IActionResult ResponseCache(int index = 1)
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            
            ulong number = Fibonacci((ulong)index);
            
            stopwatch.Stop();

            return View(new FibonnaciModel() { 
                Index = index, 
                Number = number,
                CalculationTime = stopwatch.Elapsed 
            });
        }
     
        public IActionResult MemoryCache(int index = 1)
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            ulong number = Fibonacci_cached((ulong)index);

            stopwatch.Stop();

            return View(new FibonnaciModel()
            {
                Index = index,
                Number = number,
                CalculationTime = stopwatch.Elapsed
            });
        }

        /// <summary>
        /// A recursive and exponential implementation of Fibonacci
        /// </summary>
        /// <param name="i">index to calculate</param>
        /// <returns>The corresponding Fibonacci number</returns>
        public ulong Fibonacci_cached(ulong i)
        {
            if (i < 0)
                throw new ArgumentOutOfRangeException("Fibonacci is only defined for numbers equal or greater than 0");

            if (i == 0)
                return 0;

            if (i <= 2)
                return 1;

            ulong result;
            if (_cache.TryGetValue(i, out result))
            {
                return result;
            }

            Task.Delay(10).Wait();

            result = Fibonacci_cached(i - 1) + Fibonacci_cached(i - 2);
            
            _cache.Set(i, result, new MemoryCacheEntryOptions() { SlidingExpiration = new TimeSpan(0, 10, 0) });

            return result;
        }

        /// <summary>
        /// A linear implementation of Fibonacci (with a delay between every iteration) 
        /// </summary>
        /// <param name="i">index to calculate</param>
        /// <param name="m">(do not use)</param>
        /// <param name="n">(do not use)</param>
        /// <returns>The corresponding Fibonacci number</returns>
        public ulong Fibonacci_linear(ulong i, ulong m = 1, ulong n = 0)
        {
            if (i < 0)
                throw new ArgumentOutOfRangeException("Fibonacci is only defined for numbers equal or greater than 0");

            if (i == 0)
                return 0;

            if (i <= 2)
                return 1;

            return m + Fibonacci_linear(i - 1, m + n, m);
        }
    }
}