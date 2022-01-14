﻿using Blackboard.Core.Data.Interfaces;
using Blackboard.Core.Nodes.Bases;
using Blackboard.Core.Nodes.Interfaces;

namespace Blackboard.Core.Nodes.Inner {

    /// <summary>Performs a bitwise NOT of one integer parent.</summary>
    sealed public class BitwiseNot<T>: UnaryValue<T, T>
        where T : IBitwise<T>, IEquatable<T> {

        /// <summary>This is a factory function for creating new instances of this node easily.</summary>
        static public readonly IFuncDef Factory = CreateFactory((value) => new BitwiseNot<T>(value));

        /// <summary>Creates a bitwise NOT value node.</summary>
        /// <param name="source">This is the single parent for the source value.</param>
        public BitwiseNot(IValueParent<T> source = null) : base(source) { }

        /// <summary>This is the type name of the node.</summary>
        public override string TypeName => "BitwiseNot";

        /// <summary>Gets the bitwise NOT of the given value.</summary>
        /// <param name="value">The value to get the bitwise NOT of.</param>
        /// <returns>The bitwise NOT of the given value.</returns>
        protected override T OnEval(T value) => value.BitwiseNot();
    }
}
