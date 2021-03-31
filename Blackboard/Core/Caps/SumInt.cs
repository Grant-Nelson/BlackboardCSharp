﻿using System.Collections.Generic;
using Blackboard.Core.Interfaces;

namespace Blackboard.Core.Caps {

    public class SumInt: Nary<int, int> {

        public SumInt(params IValue<int>[] sources) :
            this(sources as IEnumerable<IValue<int>>) { }

        public SumInt(IEnumerable<IValue<int>> sources = null, int value = default) :
            base(sources, value) { }

        protected override int OnEval(int[] values) {
            int result = 0;
            foreach (int value in values) result += value;
            return result;
        }

        public override string ToString() => "SumInt"+base.ToString();
    }
}
