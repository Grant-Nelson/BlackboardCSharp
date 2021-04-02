﻿using System.Collections.Generic;
using Blackboard.Core.Interfaces;
using Blackboard.Core.Bases;

namespace Blackboard.Core.Caps {

    public class MulInt: Nary<int, int> {

        protected override int OnEval(IEnumerable<int> values) {
            int result = 1;
            foreach (int value in values) result *= value;
            return result;
        }

        /// <summary>Gets the string for this node.</summary>
        /// <returns>The debug string for this node.</returns>
        public override string ToString() => "MulInt"+base.ToString();
    }
}
