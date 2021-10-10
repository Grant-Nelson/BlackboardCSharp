﻿using Blackboard.Core.Data.Caps;
using Blackboard.Core.Data.Interfaces;
using Blackboard.Core.Nodes.Functions;
using Blackboard.Core.Nodes.Bases;
using Blackboard.Core.Nodes.Interfaces;

namespace Blackboard.Core.Nodes.Inner {

    /// <summary>This will return the value of one of two parents based on a boolean parent.</summary>
    /// <remarks>This functions just like a typical ternary statement.</remarks>
    /// <typeparam name="T">The type of input for the two value providing parents.</typeparam>
    sealed public class Select<T>: Ternary<Bool, T, T, T>
        where T : IComparable<T>, new() {

        /// <summary>This is a factory function for creating new instances of this node easily.</summary>
        static public readonly IFuncGroup Factory =
            new Function<IValueAdopter<Bool>, IValueAdopter<T>, IValueAdopter<T>, Select<T>>(
                (test, left, right) => new Select<T>(test, left, right));

        /// <summary>Creates a selection value node.</summary>
        /// <param name="source1">This is the first parent for the source value.</param>
        /// <param name="source2">This is the second parent for the source value.</param>
        /// <param name="source3">This is the third parent for the source value.</param>
        /// <param name="value">The default value for this node.</param>
        public Select(IValueAdopter<Bool> source1 = null, IValueAdopter<T> source2 = null,
            IValueAdopter<T> source3 = null, T value = default) :
            base(source1, source2, source3, value) { }

        /// <summary>Selects the value to return during evaluation.</summary>
        /// <param name="value1">
        /// The parent to select with,
        /// true will return the second parent's value,
        /// false will return the third parent's value.
        /// </param>
        /// <param name="value2">The value to return if the first parent is true.</param>
        /// <param name="value3">The value to return if the first parent is false.</param>
        /// <returns>The selected value to set to this node.</returns>
        protected override T OnEval(Bool value1, T value2, T value3) => value1.Value ? value2 : value3;

        /// <summary>Gets the string for this node.</summary>
        /// <returns>The debug string for this node.</returns>
        public override string ToString() => "Select"+base.ToString();
    }
}