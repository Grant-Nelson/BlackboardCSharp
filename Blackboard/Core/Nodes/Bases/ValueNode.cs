﻿using Blackboard.Core.Data.Interfaces;
using Blackboard.Core.Nodes.Interfaces;
using S = System;

namespace Blackboard.Core.Nodes.Bases {

    /// <summary>A base node for a node which has a value.</summary>
    /// <typeparam name="T">The type of the value being held.</typeparam>
    public abstract class ValueNode<T>: Evaluable, IValueParent<T>, S.IEquatable<ValueNode<T>>
        where T : IEquatable<T> {

        /// <summary>Creates a new value node.</summary>
        /// <param name="value">The initial value of the node.</param>
        public ValueNode(T value = default) => this.Value = value ?? default;

        /// <summary>This gets the data being stored in this node.</summary>
        /// <returns>The data being stored.</returns>
        public IData Data => this.Value;

        /// <summary>The value being held by this node.</summary>
        public T Value { get; private set; }

        /// <summary>Sets the given value to this node.</summary>
        ///<param name="value">The new value to set.</param>
        /// <returns>True if the value has changed, false otherwise.</returns>
        protected bool UpdateValue(T value) {
            if (this.Value.Equals(value)) return false;
            this.Value = value;
            return true;
        }

        /// <summary>
        /// This is called when the value is evaluated and updated.
        /// It will determine the new value the node should be set to.
        /// </summary>
        /// <returns>The new value that the node should be set to.</returns>
        abstract protected T CalcuateValue();

        /// <summary>Updates the node's value, usually based on current parents.</summary>
        /// <returns>True indicates that the value has been changed, false otherwise.</returns>
        public override bool Evaluate() => this.UpdateValue(this.CalcuateValue());

        /// <summary>Gets the hash code of the node's value.</summary>
        /// <returns>The node's value's hash code.</returns>
        public override int GetHashCode() => this.Value.GetHashCode();

        /// <summary>Check to see if this node's value is equal to the other object.</summary>
        /// <param name="obj">The other object to compare the value from.</param>
        /// <returns>
        /// True if the other node contains the same value,
        /// false if not a node with a value or the values aren't equal.
        /// </returns>
        public override bool Equals(object obj) => obj is ValueNode<T> val && this.Equals(val);

        /// <summary>Check to see if this node's value is equal to the other object.</summary>
        /// <param name="other">The other node to compare the value from.</param>
        /// <returns>
        /// True if the other node contains the same value,
        /// false if the values aren't equal.
        /// </returns>
        public bool Equals(ValueNode<T> other) => this.Value.Equals(other.Value);
    }
}
