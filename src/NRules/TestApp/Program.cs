using System;
using System.Threading.Tasks;
using NRules;
using NRules.Fluent;

namespace TestApp
{
    static class Program
    {
        static void Main(string[] args)
        {
            MainAsync().GetAwaiter().GetResult();
        }

        static Task MainAsync()
        {
            var repository = new RuleRepository();
            repository.Load(x => x.From(typeof(TestRule).Assembly));

            var factory = repository.Compile();

            var session = factory.CreateSession();

            var testFact = new TestFact { PropOne = "test" };

            session.Insert(testFact);

            return session.FireAsync();
        }
    }
}
