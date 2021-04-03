﻿using Blackboard.Core.Bases;

namespace Blackboard.Core.Caps {

    /// <summary>Gets the difference between the two parent values.</summary>
    public class SubFloat: Binary<double, double, double> {

        /// <summary>Gets the difference of the parents during evaluation.</summary>
        /// <param name="value1">The first value to be subtracted from.</param>
        /// <param name="value2">The second value to subtract from the first.</param>
        /// <returns>The difference of the two values.</returns>
        protected override double OnEval(double value1, double value2) => value1-value2;

        /// <summary>Gets the string for this node.</summary>
        /// <returns>The debug string for this node.</returns>
        public override string ToString() => "SubFloat"+base.ToString();
    }
}
