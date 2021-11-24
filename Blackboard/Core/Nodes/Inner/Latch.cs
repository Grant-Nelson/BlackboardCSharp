﻿using Blackboard.Core.Data.Interfaces;
using Blackboard.Core.Nodes.Bases;
using Blackboard.Core.Nodes.Functions;
using Blackboard.Core.Nodes.Interfaces;
using System.Collections.Generic;

namespace Blackboard.Core.Nodes.Inner {

    /// <summary>This is a latching value node.</summary>
    /// <typeparam name="T">The type of the value for this node.</typeparam>
    sealed public class Latch<T>: ValueNode<T>
        where T : IComparable<T>, new() {

        /// <summary>This is a factory function for creating new instances of this node easily.</summary>
        static public readonly IFuncDef Factory =
            new Function<ITriggerAdopter, IValueParent<T>, Latch<T>>((left, right) => new Latch<T>(left, right));

        /// <summary>This is the first parent node to read from.</summary>
        private ITriggerAdopter source1;

        /// <summary>This is the second parent node to read from.</summary>
        private IValueParent<T> source2;

        /// <summary>Creates a latching value node.</summary>
        /// <remarks>The value is updated right away so the default value may not be used.</remarks>
        /// <param name="source1">This is the parent that indicates the value should be set.</param>
        /// <param name="source2">This is the parent to get the value from when the other is provoked.</param>
        /// <param name="value">The default value for this node.</param>
        public Latch(ITriggerAdopter source1 = null, IValueParent<T> source2 = null, T value = default) : base(value) {
            this.Parent1 = source1;
            this.Parent2 = source2;
            this.UpdateValue();
        }

        /// <summary>The parent node to indicate when the value should be set to the other parent.</summary>
        public ITriggerAdopter Parent1 {
            get => this.source1;
            set {
                this.SetParent(ref this.source1, value);
                this.UpdateValue();
            }
        }

        /// <summary>The parent node to get the source value from if the other parent is provoked.</summary>
        public IValueParent<T> Parent2 {
            get => this.source2;
            set {
                this.SetParent(ref this.source2, value);
                this.UpdateValue();
            }
        }

        /// <summary>The set of parent nodes to this node in the graph.</summary>
        public override IEnumerable<IAdopter> Parents => INode.NotNull<IAdopter>(this.source1, this.source2);

        /// <summary>This updates the value during evaluation.</summary>
        /// <returns>True if the value was changed, false otherwise.</returns>
        protected override bool UpdateValue() =>
            this.source1 is not null &&
            this.source2 is not null &&
            this.source1.Provoked &&
            this.SetNodeValue(this.source2.Value);

        /// <summary>This is the type name of the node.</summary>
        public override string TypeName => "Latch";
    }
}
