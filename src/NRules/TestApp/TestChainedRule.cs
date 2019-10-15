using System;
using System.Collections.Generic;
using System.Text;
using NRules.Fluent.Dsl;

namespace TestApp
{
    public class TestChainedRule : Rule
    {
        public override void Define()
        {
            TestFact testFact = null;

            When()
                .Match(() => testFact, f => f.PropOne != null && f.PropTwo != null);

            Then()
                .Do(_ => testFact.ShowProperties());
        }
    }
}
