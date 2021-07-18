﻿using Blackboard.Core.Bases;
using Blackboard.Core.Interfaces;
using System.Collections.Generic;

namespace Blackboard.Core.Caps {

    /// <summary>This is a latching value node.</summary>
    /// <typeparam name="T">The type of the value for this node.</typeparam>
    sealed public class Latch<T>: ValueNode<T> {

        /// <summary>This is the first parent node to read from.</summary>
        private ITrigger source1;

        /// <summary>This is the second parent node to read from.</summary>
        private IValue<T> source2;

        /// <summary>Creates a latching value node.</summary>
        /// <remarks>The value is updated right away so the default value may not be used.</remarks>
        /// <param name="source1">This is the parent that indicates the value should be set.</param>
        /// <param name="source2">This is the parent to get the value from when the other is triggered.</param>
        /// <param name="value">The default value for this node.</param>
        public Latch(ITrigger source1 = null, IValue<T> source2 = null, T value = default) : base(value) {
            this.Parent1 = source1;
            this.Parent2 = source2;
            this.UpdateValue();
        }

        /// <summary>The parent node to indicate when the value should be set to the other parent.</summary>
        public ITrigger Parent1 {
            get => this.source1;
            set {
                this.SetParent(ref this.source1, value);
                this.UpdateValue();
            }
        }

        /// <summary>The parent node to get the source value from if the other parent is triggered.</summary>
        public IValue<T> Parent2 {
            get => this.source2;
            set {
                this.SetParent(ref this.source2, value);
                this.UpdateValue();
            }
        }

        /// <summary>The set of parent nodes to this node in the graph.</summary>
        public override IEnumerable<INode> Parents {
            get {
                if (this.source1 is not null) yield return this.source1;
                if (this.source2 is not null) yield return this.source2;
            }
        }

        /// <summary>This updates the value during evaluation.</summary>
        /// <returns>True if the value was changed, false otherwise.</returns>
        protected override bool UpdateValue() {
            if (this.source1 is null || this.source2 is null) return false;
            if (!this.source1.Triggered) return false;
            T value = this.source2.Value;
            return this.SetNodeValue(value);
        }

        /// <summary>Gets the string for this node.</summary>
        /// <returns>The debug string for this node.</returns>
        public override string ToString() => "Latch("+NodeString(this.source1)+", "+NodeString(this.source2)+")";
    }
}
