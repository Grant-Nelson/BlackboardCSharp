﻿using Blackboard.Core.Bases;
using System;

namespace Blackboard.Core.Caps {

    /// <summary>A double value node that gets the absolute value of the parent.</summary>
    public class AbsFloat: Unary<double, double> {

        /// <summary>This will get the absolute value of the parent's value on evaluation.</summary>
        /// <param name="value">The value to get the absolute of.</param>
        /// <returns>The absolute of the given value.</returns>
        protected override double OnEval(double value) => Math.Abs(value);

        /// <summary>Gets the string for this node.</summary>
        /// <returns>The debug string for this node.</returns>
        public override string ToString() => "AbsFloat"+base.ToString();
    }
}
