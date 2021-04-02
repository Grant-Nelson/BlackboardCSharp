﻿using System.Collections.Generic;
using Blackboard.Core.Interfaces;
using Blackboard.Core.Bases;

namespace Blackboard.Core.Caps {

    public class SumFloat: Nary<double, double> {

        protected override double OnEval(IEnumerable<double> values) {
            double result = 0.0;
            foreach (double value in values) result += value;
            return result;
        }

        /// <summary>Gets the string for this node.</summary>
        /// <returns>The debug string for this node.</returns>
        public override string ToString() => "SumFloat"+base.ToString();
    }
}
