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

        public static async Task UpdateFactProp2Async(TestFact testFact)
        {
            Console.WriteLine($"Started async task {nameof(UpdateFactProp2Async)}");

            await Task.Delay(TimeSpan.FromSeconds(2)).ConfigureAwait(false);

            testFact.PropTwo = "testprop2";

            Console.WriteLine($"Finished async task {nameof(UpdateFactProp2Async)}");
        }
    }
}
