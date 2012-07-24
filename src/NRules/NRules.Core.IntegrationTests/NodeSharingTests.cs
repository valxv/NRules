﻿using NRules.Core.IntegrationTests.TestAssets;
using NRules.Core.IntegrationTests.TestRules;
using NUnit.Framework;

namespace NRules.Core.IntegrationTests
{
    [TestFixture]
    public class NodeSharingTests : BaseRuleTestFixture
    {
        [Test]
        public void NodeSharing_TwoMatchingFacts_BothRulesFireOnceEach()
        {
            //Arrange
            var fact1 = new FactType1() {TestProperty = "Valid Value"};
            var fact2 = new FactType2() {TestProperty = "Valid Value", JoinReference = fact1};

            Session.Insert(fact1);
            Session.Insert(fact2);

            //Act
            Session.Fire();

            //Assert
            AssertFiredOnce<TwinRuleOne>();
            AssertFiredOnce<TwinRuleTwo>();
        }

        protected override void SetUpRules()
        {
            SetUpRule<TwinRuleOne>();
            SetUpRule<TwinRuleTwo>();
        }
    }
}