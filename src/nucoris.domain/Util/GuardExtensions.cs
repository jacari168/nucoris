using nucoris.domain;
using System;
using System.Linq;
using System.Collections.Generic;

// Extensions on Steve Smith's guard clauses:
// https://github.com/ardalis/GuardClauses

// Using the same namespace as Steve will make sure our code picks up 
// our extensions no matter where the guards are:
namespace Ardalis.GuardClauses
{
    public static class GuardExtensions
    {
        public static void Condition(this IGuardClause clause, bool invalidCondition, string message)
        {
            if( invalidCondition)
            {
                throw new ApplicationException(message);
            }
        }

        public static void InvalidState( this IGuardClause clause, OrderState state, params OrderState[] invalidStates)
        {
            if( invalidStates.Contains(state))
            {
                throw new System.InvalidOperationException($"Order state {state} is not valid for this action");
            }
        }

        public static void OutOfValidStates(this IGuardClause clause, OrderState state, params OrderState[] validStates)
        {
            if (! validStates.Contains(state))
            {
                throw new System.InvalidOperationException(
                    $"Order state {state} is not valid for this action. The order should be in one of the states: {validStates.ToSingleString()}");
            }
        }
    }
}
