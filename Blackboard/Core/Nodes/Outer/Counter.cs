﻿using Blackboard.Core.Data.Interfaces;
using Blackboard.Core.Extensions;
using Blackboard.Core.Nodes.Bases;
using Blackboard.Core.Nodes.Functions;
using Blackboard.Core.Nodes.Interfaces;
using System.Collections.Generic;

namespace Blackboard.Core.Nodes.Outer {

    /// <summary>Provides a node which can be used to count trigger events.</summary>
    sealed public class Counter<T>: ValueNode<T>, IChild
        where T : IArithmetic<T>, IComparable<T> {

        /// <summary>This is a factory function for creating new instances of this node easily.</summary>
        /// <remarks>This will not initialize any sources except increment, the others can be set later.</remarks>
        static public readonly IFuncDef Factory =
            new Function<ITriggerParent, Counter<T>>((ITriggerParent increment) => new Counter<T>(increment));

        /// <summary>
        /// This is a function to assign a value to the counter node directly.
        /// This will return the counter node on success, or null if value could not be cast.
        /// </summary>
        static public readonly IFuncDef Assign =
            new Function<Counter<T>, IDataNode, Counter<T>>((Counter<T> input, IDataNode node) => {
                T value = node.Data.ImplicitCastTo<T>();
                if (value is null) return null;
                input.SetValue(value);
                return input;
            });

        /// <summary>This is the parent to increment the counter.</summary>
        private ITriggerParent increment;

        /// <summary>This is the parent to decrement the counter.</summary>
        private ITriggerParent decrement;

        /// <summary>This is the parent reset the counter to the reset value.</summary>
        private ITriggerParent reset;

        /// <summary>The value to step during an increment or decrement.</summary>
        private IValueParent<T> delta;

        /// <summary>The value to reset this toggle to when the toggle is reset.</summary>
        private IValueParent<T> resetValue;

        /// <summary>Creates a new node for counting events.</summary>
        /// <param name="increment">The initial parent to trigger an increment.</param>
        /// <param name="decrement">The initial parent to trigger an decrement.</param>
        /// <param name="reset">The initial parent trigger to reset the counter.</param>
        /// <param name="delta">The initial parent for the step value.</param>
        /// <param name="resetValue">The initial reset value parent.</param>
        /// <param name="value">The initial value for this node.</param>
        public Counter(ITriggerParent increment = null, ITriggerParent decrement = null, ITriggerParent reset = null,
            IValueParent<T> delta = null, IValueParent<T> resetValue = null, T value = default) : base(value) {
            this.Increment = increment;
            this.Decrement = decrement;
            this.Reset = reset;
            this.Delta = delta;
            this.ResetValue = resetValue;
        }

        /// <summary>This is the type name of the node.</summary>
        public override string TypeName => "Counter";

        /// <summary>This is the parent to increment the counter.</summary>
        public ITriggerParent Increment {
            get => this.increment;
            set => this.SetParent(ref this.increment, value);
        }

        /// <summary>This is the parent to decrement the counter.</summary>
        public ITriggerParent Decrement {
            get => this.decrement;
            set => this.SetParent(ref this.decrement, value);
        }

        /// <summary>This is the parent reset the toggle to false.</summary>
        public ITriggerParent Reset {
            get => this.reset;
            set => this.SetParent(ref this.reset, value);
        }

        /// <summary>The value to step during an increment or decrement.</summary>
        /// <remarks>If this parent is null then the counter will increment and decrement by one.</remarks>
        public IValueParent<T> Delta {
            get => this.delta;
            set => this.SetParent(ref this.delta, value);
        }

        /// <summary>The value to reset this toggle to when the toggle is reset.</summary>
        /// <remarks>If this parent is null then the toggle is reset to false.</remarks>
        public IValueParent<T> ResetValue {
            get => this.resetValue;
            set => this.SetParent(ref this.resetValue, value);
        }

        /// <summary>The set of parent nodes to this node in the graph.</summary>
        public IEnumerable<IParent> Parents =>
            IChild.EnumerateParents(this.increment, this.decrement, this.reset, this.delta, this.resetValue);

        /// <summary>This sets the value of this node.</summary>
        /// <param name="value">The value to set.</param>
        /// <returns>True if the value has changed, false otherwise.</returns>
        public bool SetValue(T value) => this.UpdateValue(value);

        /// <summary>This is called when the value is evaluated and updated.</summary>
        /// <returns>True if the value was changed, false otherwise.</returns>
        protected override T CalcuateValue() {
            T value = this.Value;
            T delta = this.delta is null ? default(T).Inc() : this.delta.Value;
            if (this.increment?.Provoked ?? false) value = value.Sum(delta);
            if (this.decrement?.Provoked ?? false) value = value.Sub(delta);
            if (this.reset?.Provoked     ?? false) value = this.resetValue is null ? default : this.resetValue.Value;
            return value;
        }
    }
}
