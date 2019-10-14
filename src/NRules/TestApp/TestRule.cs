using System;
using NRules.Fluent.Dsl;

namespace TestApp
{
    public class TestRule : Rule
    {
        public override void Define()
        {
            TestFact testFact = null;

            When()
                .Match<TestFact>(() => testFact, f => !string.IsNullOrEmpty(f.PropOne) && f.PropTwo == null);

            Then()
                .Do(_ => Actions.DoSomethingAsync())
                .Do(_ => Console.WriteLine("Finished processing"));
        }
    }
}
