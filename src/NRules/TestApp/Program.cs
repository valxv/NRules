using System;
using System.Collections.Generic;
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

            return Task.WhenAll(
                Task.Run(() => RunSession1(factory)),
                Task.Run(() => RunSession2(factory)));
        }

        private static Task RunSession1(ISessionFactory factory)
        {
            var session = factory.CreateSession();

            var testFact = new TestFact { PropOne = "test1" };

            session.Insert(testFact);

            return session.FireAsync();
        }

        private static Task RunSession2(ISessionFactory factory)
        {
            var session = factory.CreateSession();

            var testFact = new TestFact { PropOne = "test2" };

            session.Insert(testFact);

            return session.FireAsync();
        }
    }
}
