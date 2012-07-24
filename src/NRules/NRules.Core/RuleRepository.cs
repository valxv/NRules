﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NRules.Core.Rules;

namespace NRules.Core
{
    public interface IRuleRepository
    {
        void AddRuleSet(Assembly assembly);
    }

    public class RuleRepository : IRuleRepository
    {
        private readonly IList<RuleSet> _ruleSets = new List<RuleSet>();

        private readonly IContainer _diContainer;

        public RuleRepository()
        {
            _diContainer = null;
        }

        public RuleRepository(IContainer diContainer)
        {
            _diContainer = diContainer;
        }

        public void AddRuleSet(Assembly assembly)
        {
            var ruleTypes = assembly.GetTypes().Where(IsRule).ToArray();

            if (!ruleTypes.Any())
                throw new ArgumentException(string.Format("The supplied assembly ({0}) does not contain " +
                                                          "any concrete IRule implementations!",
                                                          assembly.FullName));

            var ruleSet = new RuleSet(ruleTypes);
            _ruleSets.Add(ruleSet);
        }

        public void AddRuleSet(params Type[] types)
        {
            var invalidTypes = types.Where(IsNotRule).ToArray();

            if (invalidTypes.Any())
            {
                var invalidTypesString = String.Join(", ", (string[]) invalidTypes.Select(t => t.FullName));
                throw new ArgumentException(string.Format("The supplied types are not rules {0}",
                                                          invalidTypesString));
            }

            var ruleSet = new RuleSet(types);
            _ruleSets.Add(ruleSet);
        }

        internal IEnumerable<CompiledRule> Compile()
        {
            if (!_ruleSets.Any())
                throw new ArgumentException(
                    "Rules cannot be compiled! No valid rulesets have been added to the rule repository.");

            foreach (RuleSet ruleSet in _ruleSets)
            {
                foreach (Type ruleType in ruleSet.RuleTypes)
                {
                    CompiledRule rule = InstantiateRule(ruleType);
                    yield return rule;
                }
            }
        }

        private CompiledRule InstantiateRule(Type ruleType)
        {
            IRule ruleInstance;

            if (_diContainer == null)
            {
                ruleInstance = BuildRule(ruleType);
            }
            else
            {
                object objectInstance = _diContainer.GetObjectInstance(ruleType);
                ruleInstance = objectInstance as IRule;
                if (ruleInstance == null)
                    throw new ApplicationException(
                        string.Format("Failed to initialize rule of type {0} from dependency injection " +
                                      "container of type {1}.",
                                      ruleType,
                                      _diContainer.GetType()));
            }

            var rule = new CompiledRule(ruleInstance.GetType().FullName);
            var definition = new RuleDefinition(rule);

            ruleInstance.Define(definition);
            return rule;
        }

        private static bool IsNotRule(Type type)
        {
            return !IsRule(type);
        }

        private static bool IsRule(Type type)
        {
            if (IsConcrete(type) &&
                typeof (IRule).IsAssignableFrom(type)) return true;

            return false;
        }

        private static bool IsConcrete(Type type)
        {
            if (type.IsAbstract) return false;
            if (type.IsInterface) return false;
            if (type.IsGenericTypeDefinition) return false;

            return true;
        }

        private static IRule BuildRule(Type type)
        {
            var rule = (IRule) Activator.CreateInstance(type);
            return rule;
        }
    }
}