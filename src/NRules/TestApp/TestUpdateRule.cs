using System;
using System.Collections.Generic;
using System.Text;
using NRules.Fluent.Dsl;

namespace TestApp
{
    public class TestUpdateRule : Rule
    {
        public override void Define()
        {
            TestFact testFact = null;

            When()
                .Match(() => testFact, f => f.PropOne == "test2" && f.PropTwo == null);

            Then()
                .Do(_ => Actions.UpdateFactProp2Async(testFact))
                .Do(ctx => ctx.Update(testFact))
                .Do(_ => Console.WriteLine("Finished Update rule"));
        }
    }
}
