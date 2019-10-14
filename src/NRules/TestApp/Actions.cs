using System;
using System.Threading.Tasks;

namespace TestApp
{
    public static class Actions
    {
        public static async Task DoSomethingAsync()
        {
            Console.WriteLine("Started async task DoSomethingAsync");

            await Task.Delay(TimeSpan.FromSeconds(2)).ConfigureAwait(false);

            Console.WriteLine("Finished async task DoSomethingAsync");
        }
    }
}
