using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using NRules.Extensibility;
using NRules.Rete;
using NRules.RuleModel;
using NRules.Utilities;

namespace NRules
{
    internal interface IRuleAction
    {
        Expression Expression { get; }
        ActionTrigger Trigger { get; }
        object[] GetArguments(IExecutionContext executionContext, IActionContext actionContext);
        void Invoke(IExecutionContext executionContext, IActionContext actionContext, object[] arguments);
        Task InvokeAsync(IExecutionContext executionContext, IActionContext actionContext, object[] arguments, CancellationToken cancellationToken);
    }

    internal class RuleAction : IRuleAction
    {
        private readonly LambdaExpression _expression;
        private readonly IndexMap _tupleFactMap;
        private readonly IndexMap _dependencyFactMap;
        private readonly ActionTrigger _actionTrigger;
        private readonly FastDelegate<Action<IContext, object[]>> _compiledExpression;
        private readonly FastDelegate<Func<IContext, object[], Task>> _compiledAsyncExpression;

        public RuleAction(LambdaExpression expression, FastDelegate<Action<IContext, object[]>> compiledExpression,
            IndexMap tupleFactMap, IndexMap dependencyFactMap, ActionTrigger actionTrigger)
        {
            _expression = expression;
            _tupleFactMap = tupleFactMap;
            _dependencyFactMap = dependencyFactMap;
            _actionTrigger = actionTrigger;
            _compiledExpression = compiledExpression;
        }

        public RuleAction(LambdaExpression expression, FastDelegate<Func<IContext, object[], Task>> compiledExpression,
            IndexMap tupleFactMap, IndexMap dependencyFactMap, ActionTrigger actionTrigger)
        {
            _expression = expression;
            _tupleFactMap = tupleFactMap;
            _dependencyFactMap = dependencyFactMap;
            _actionTrigger = actionTrigger;
            _compiledAsyncExpression = compiledExpression;
        }

        public Expression Expression => _expression;
        public ActionTrigger Trigger => _actionTrigger;

        public object[] GetArguments(IExecutionContext executionContext, IActionContext actionContext)
        {
            var compiledRule = actionContext.CompiledRule;
            var activation = actionContext.Activation;
            var tuple = activation.Tuple;

            var args = new object[_compiledExpression != null ? _compiledExpression.ArrayArgumentCount : _compiledAsyncExpression.ArrayArgumentCount];

            int index = tuple.Count - 1;
            var activationFactMap = activation.FactMap;
            foreach (var fact in tuple.Facts)
            {
                var mappedIndex = _tupleFactMap[activationFactMap[index]];
                IndexMap.SetElementAt(args, mappedIndex, fact.Object);
                index--;
            }

            index = 0;
            var dependencyResolver = executionContext.Session.DependencyResolver;
            var resolutionContext = new ResolutionContext(executionContext.Session, compiledRule.Definition);
            foreach (var dependency in compiledRule.Dependencies)
            {
                var mappedIndex = _dependencyFactMap[index];
                if (mappedIndex >= 0)
                {
                    var resolvedDependency = dependency.Factory(dependencyResolver, resolutionContext);
                    IndexMap.SetElementAt(args, mappedIndex, resolvedDependency);
                }
                index++;
            }

            return args;
        }

        public void Invoke(IExecutionContext executionContext, IActionContext actionContext, object[] arguments)
        {
            Exception exception = null;
            try
            {
                _compiledExpression.Delegate.Invoke(actionContext, arguments);
            }
            catch (Exception e)
            {
                exception = e;
                bool isHandled = false;
                executionContext.EventAggregator.RaiseRhsExpressionFailed(executionContext.Session, e, _expression, arguments, actionContext.Activation, ref isHandled);
                if (!isHandled)
                {
                    throw;
                }
            }
            finally
            {
                executionContext.EventAggregator.RaiseRhsExpressionEvaluated(executionContext.Session, exception, _expression, arguments, actionContext.Activation);
            }
        }

        public async Task InvokeAsync(IExecutionContext executionContext, IActionContext actionContext, object[] arguments, CancellationToken cancellationToken)
        {
            Exception exception = null;
            try
            {
                //TODO: Substitute CancellationToken parameter in expression tree if exists
                // https://www.codeproject.com/Articles/143096/Parameter-Substitution-within-Expression-Trees
                await Task.Run(() => _compiledAsyncExpression.Delegate.Invoke(actionContext, arguments), cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                exception = e;
                bool isHandled = false;
                executionContext.EventAggregator.RaiseRhsExpressionFailed(executionContext.Session, e, _expression, arguments, actionContext.Activation, ref isHandled);
                if (!isHandled)
                {
                    throw;
                }
            }
            finally
            {
                executionContext.EventAggregator.RaiseRhsExpressionEvaluated(executionContext.Session, exception, _expression, arguments, actionContext.Activation);
            }
        }
    }
}
