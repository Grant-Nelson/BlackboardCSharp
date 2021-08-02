﻿using Blackboard.Core.Nodes.Interfaces;
using System.Collections.Generic;

namespace Blackboard.Core.Nodes.Bases {

    /// <summary>This is a trigger node which has several parents.</summary>
    public abstract class Multitrigger: TriggerNode {

        /// <summary>This is the list of all the parent nodes to listen to.</summary>
        protected List<ITrigger> sources;

        /// <summary>Creates a multi-trigger node.</summary>
        /// <param name="parents">The initial set of parents to use.</param>
        public Multitrigger(params ITrigger[] parents) :
            this(parents as IEnumerable<ITrigger>) { }

        /// <summary>Creates a multi-trigger node.</summary>
        /// <param name="parents">The initial set of parents to use.</param>
        public Multitrigger(IEnumerable<ITrigger> parents = null) {
            this.sources = new List<ITrigger>();
            this.AddParents(parents);
        }

        /// <summary>This adds parents to this node.</summary>
        /// <param name="parents">The set of parents to add.</param>
        public void AddParents(params ITrigger[] parents) =>
            this.AddParents(parents as IEnumerable<ITrigger>);

        /// <summary>This adds parents to this node.</summary>
        /// <param name="parents">The set of parents to add.</param>
        public void AddParents(IEnumerable<ITrigger> parents) {
            this.sources.AddRange(parents);
            foreach (ITrigger parent in parents)
                parent.AddChildren(this);
        }

        /// <summary>This removes the given parents from this node.</summary>
        /// <param name="parents">The set of parents to remove.</param>
        /// <returns>True if any of the parents are removed, false if none were removed.</returns>
        public bool RemoveParents(params ITrigger[] parents) =>
            this.RemoveParents(parents as IEnumerable<ITrigger>);

        /// <summary>This removes the given parents from this node.</summary>
        /// <param name="parents">The set of parents to remove.</param>
        /// <returns>True if any of the parents are removed, false if none were removed.</returns>
        public bool RemoveParents(IEnumerable<ITrigger> parents) {
            bool anyRemoved = false;
            foreach (ITrigger parent in parents) {
                if (this.sources.Remove(parent)) {
                    parent.RemoveChildren(this);
                    anyRemoved = true;
                }
            }
            return anyRemoved;
        }

        /// <summary>The set of parent nodes to this node in the graph.</summary>
        public override IEnumerable<INode> Parents => this.sources;

        /// <summary>
        /// This handles updating this node's value given the
        /// parents' provoked state during evaluation.
        /// </summary>
        /// <remarks>Any null parents are ignored.</remarks>
        /// <param name="provoked">The value from the all the non-null parents.</param>
        /// <returns>The new value for this node.</returns>
        protected abstract bool OnEval(IEnumerable<bool> provoked);

        /// <summary>This updates the trigger during evaluation.</summary>
        /// <returns>True if the value was provoked, false otherwise.</returns>
        protected override bool UpdateTrigger() =>
            this.Provoked = this.OnEval(this.sources.NotNull().Triggers());

        /// <summary>Gets the string for this node.</summary>
        /// <returns>The debug string for this node.</returns>
        public override string ToString() => "("+NodeString(this.sources)+")";
    }
}