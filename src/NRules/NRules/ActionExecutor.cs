using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using NRules.RuleModel;
using NRules.Utilities;

namespace NRules
{
    internal interface IActionExecutor
    {
        void Execute(IExecutionContext executionContext, IActionContext actionContext);

        Task ExecuteAsync(IExecutionContext executionContext, IActionContext actionContext, CancellationToken cancellationToken);
    }

    internal class ActionExecutor : IActionExecutor
    {
        public void Execute(IExecutionContext executionContext, IActionContext actionContext)
        {
            ISession session = executionContext.Session;
            Activation activation = actionContext.Activation;

            var invocations = CreateInvocations(executionContext, actionContext);

            executionContext.EventAggregator.RaiseRuleFiring(session, activation);
            var interceptor = session.ActionInterceptor;
            if (interceptor != null)
            {
                interceptor.Intercept(actionContext, invocations);
            }
            else
            {
                foreach (var invocation in invocations)
                {
                    try
                    {
                        invocation.Invoke();
                    }
                    catch (Exception e)
                    {
                        throw new RuleRhsExpressionEvaluationException("Failed to evaluate rule action",
                            actionContext.Rule.Name, invocation.Expression.ToString(), e);
                    }
                }
            }
            executionContext.EventAggregator.RaiseRuleFired(session, activation);
        }

        public async Task ExecuteAsync(IExecutionContext executionContext,
            IActionContext actionContext, CancellationToken cancellationToken)
        {
            ISession session = executionContext.Session;
            Activation activation = actionContext.Activation;

            var invocations = CreateInvocations(executionContext, actionContext);

            executionContext.EventAggregator.RaiseRuleFiring(session, activation);
            var interceptor = session.ActionInterceptor;
            if (interceptor != null)
            {
                interceptor.Intercept(actionContext, invocations);
            }
            else
            {
                foreach (var invocation in invocations)
                {
                    try
                    {
                        if (((LambdaExpression)invocation.Expression).ReturnType == typeof(Task))
                            await invocation.InvokeAsync(cancellationToken).ConfigureAwait(false);
                        else
                            invocation.Invoke();
                    }
                    catch (Exception e)
                    {
                        throw new RuleRhsExpressionEvaluationException("Failed to evaluate rule action",
                            actionContext.Rule.Name, invocation.Expression.ToString(), e);
                    }
                }
            }
            executionContext.EventAggregator.RaiseRuleFired(session, activation);
        }

        private IEnumerable<ActionInvocation> CreateInvocations(IExecutionContext executionContext, IActionContext actionContext)
        {
            ICompiledRule compiledRule = actionContext.CompiledRule;
            MatchTrigger trigger = actionContext.Activation.Trigger;
            var invocations = new List<ActionInvocation>();
            foreach (IRuleAction action in compiledRule.Actions)
            {
                if (!trigger.Matches(action.Trigger)) continue;

                var args = action.GetArguments(executionContext, actionContext);
                var invocation = new ActionInvocation(executionContext, actionContext, action, args);
                invocations.Add(invocation);
            }
            return invocations;
        }
    }
}
