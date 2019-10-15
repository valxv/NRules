﻿using System;
using NRules.Fluent.Dsl;

namespace TestApp
{
    public class TestRule : Rule
    {
        public override void Define()
        {
            When()
                .Match<TestFact>(f => f.PropOne == "test1" && f.PropTwo == null);

            Then()
                .Do(_ => Actions.DoSomethingAsync())
                .Do(_ => Console.WriteLine("Finished processing"));
        }
    }
}
