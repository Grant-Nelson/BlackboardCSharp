﻿using Blackboard.Core.Data.Interfaces;
using Blackboard.Core.Nodes.Bases;
using Blackboard.Core.Nodes.Interfaces;

namespace Blackboard.Core.Nodes.Caps {

    /// <summary>Casts a value node into another value node.</summary>
    sealed public class Cast<T1, T2>: Unary<T1, T2>
        where T1 : IData
        where T2 : IImplicit<T1, T2>, IComparable<T2>, new() {

        /// <summary>Creates a node cast.</summary>
        /// <param name="source">This is the single parent for the source value.</param>
        /// <param name="value">The default value for this node.</param>
        public Cast(IValue<T1> source = null, T2 value = default) :
            base(source, value) { }

        /// <summary>Gets the value to cast from the parent during evaluation.</summary>
        /// <param name="value">The parent value to cast.</param>
        /// <returns>The cast of the parent value.</returns>
        protected override T2 OnEval(T1 value) => this.Value.CastFrom(value);

        /// <summary>Gets the string for this node.</summary>
        /// <returns>The debug string for this node.</returns>
        public override string ToString() => "Cast"+base.ToString();
    }
}
