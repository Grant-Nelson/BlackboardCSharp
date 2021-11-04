﻿using Blackboard.Core.Data.Interfaces;
using Blackboard.Core.Nodes.Interfaces;
using System.Collections.Generic;

namespace Blackboard.Core.Nodes.Bases {

    /// <summary>This is a value node which has several parents as the source of the value.</summary>
    /// <typeparam name="TIn">The type of the all the parents' value for this node.</typeparam>
    /// <typeparam name="TResult">The type of value this node holds.</typeparam>
    public abstract class Nary<TIn, TResult>: ValueNode<TResult>
        where TIn : IData
        where TResult : IComparable<TResult>, new() {

        /// <summary>This is the list of all the parent nodes to read from.</summary>
        private List<IValueAdopter<TIn>> sources;

        /// <summary>Creates a N-ary value node.</summary>
        /// <param name="parents">The initial set of parents to use.</param>
        public Nary(params IValueAdopter<TIn>[] parents) :
            this(parents as IEnumerable<IValueAdopter<TIn>>) { }

        /// <summary>Creates a N-ary value node.</summary>
        /// <remarks>The value is updated right away so the default value may not be used.</remarks>
        /// <param name="parents">The initial set of parents to use.</param>
        /// <param name="value">The default value for this node.</param>
        public Nary(IEnumerable<IValueAdopter<TIn>> parents = null, TResult value = default) : base(value) {
            this.sources = new List<IValueAdopter<TIn>>();
            this.AddParents(parents);
            // UpdateValue already called by AddParents.
        }

        /// <summary>This adds parents to this node.</summary>
        /// <remarks>The value is updated after these parents are added.</remarks>
        /// <param name="parents">The set of parents to add.</param>
        public void AddParents(params IValueAdopter<TIn>[] parents) =>
            this.AddParents(parents as IEnumerable<IValueAdopter<TIn>>);

        /// <summary>This adds parents to this node.</summary>
        /// <remarks>The value is updated after these parents are added.</remarks>
        /// <param name="parents">The set of parents to add.</param>
        public void AddParents(IEnumerable<IValueAdopter<TIn>> parents) {
            parents = parents.NotNull();
            this.sources.AddRange(parents);
            foreach (IValueAdopter<TIn> parent in parents)
                parent.AddChildren(this);
            this.UpdateValue();
        }

        /// <summary>This removes the given parents from this node.</summary>
        /// <remarks>The value is updated after these parents are removed.</remarks>
        /// <param name="parents">The set of parents to remove.</param>
        /// <returns>True if any of the parents are removed, false if none were removed.</returns>
        public bool RemoveParents(params IValueAdopter<TIn>[] parents) =>
            this.RemoveParents(parents as IEnumerable<IValueAdopter<TIn>>);

        /// <summary>This removes the given parents from this node.</summary>
        /// <remarks>The value is updated after these parents are removed.</remarks>
        /// <param name="parents">The set of parents to remove.</param>
        /// <returns>True if any of the parents are removed, false if none were removed.</returns>
        public bool RemoveParents(IEnumerable<IValueAdopter<TIn>> parents) {
            bool anyRemoved = false;
            foreach (IValueAdopter<TIn> parent in parents) {
                if (this.sources.Remove(parent)) {
                    parent.RemoveChildren(this);
                    anyRemoved = true;
                }
            }
            if (anyRemoved) this.UpdateValue();
            return anyRemoved;
        }

        /// <summary>The set of parent nodes to this node in the graph.</summary>
        public override IEnumerable<INode> Parents => this.sources;

        /// <summary>This handles updating this node's value given the parents' values during evaluation.</summary>
        /// <remarks>Any null parents are ignored.</remarks>
        /// <param name="values">The value from the all the non-null parents.</param>
        /// <returns>The new value for this node.</returns>
        protected abstract TResult OnEval(IEnumerable<TIn> values);

        /// <summary>This updates the value during evaluation.</summary>
        /// <returns>True if the value was changed, false otherwise.</returns>
        protected override bool UpdateValue() =>
            this.SetNodeValue(this.OnEval(this.sources.NotNull().Values()));

        /// <summary>Creates a pretty string for this node.</summary>
        /// <param name="scopeName">The name of this node from a parent namespace or empty for no name.</param>
        /// <param name="nodeDepth">The depth of the nodes to get the string for.</param>
        /// <returns>The pretty string for debugging and testing this node.</returns>
        public override string PrettyString(string scopeName = "", int nodeDepth = int.MaxValue) {
            string name = string.IsNullOrEmpty(scopeName) ? this.TypeName : scopeName;
            string tail = nodeDepth > 0 ?
                INode.NodePrettyString(this.sources, scopeName, nodeDepth-1) :
                this.Value.ValueString;
            return name + "<" + this.Value.TypeName + ">(" + tail + ")";
        }
    }
}
