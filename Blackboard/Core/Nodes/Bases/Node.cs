﻿using Blackboard.Core.Nodes.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Blackboard.Core.Nodes.Bases {

    /// <summary>This is the base node for all data in the blackboard graph.</summary>
    public abstract class Node: INode {

        /// <summary>Gets a string for the given node even if the node is null.</summary>
        /// <param name="node">The node to get a string for.</param>
        /// <returns>The string for the given node.</returns>
        static public string NodeString(INode node) =>
            node is null ? "null" : node.ToString();

        /// <summary>Gets a string got the given set of nodes comma separated.</summary>
        /// <param name="nodes">The set of nodes which may contain nulls.</param>
        /// <returns>The string for the given nodes.</returns>
        static public string NodeString(IEnumerable<INode> nodes) =>
            string.Join(", ", nodes.Select((INode node) => NodeString(node)));

        /// <summary>This will check if from the given root node any of the given target nodes can be reachable.</summary>
        /// <param name="root">The root to start checking from.</param>
        /// <param name="targets">The target nodes to try to reach.</param>
        /// <returns>True if any of the targets can be reached, false otherwise.</returns>
        static public bool CanReachAny(INode root, IEnumerable<INode> targets) {
            // TODO:  Could the loop check be improved using the depth?
            List<INode> touched = new();
            Queue<INode> pending = new();
            pending.Enqueue(root);
            while (pending.Count > 0) {
                INode node = pending.Dequeue();
                if (node is null) continue;
                touched.Add(node);
                if (targets.Contains(node)) return true;
                foreach (INode parent in node.Parents) {
                    if (!touched.Contains(parent)) pending.Enqueue(parent);
                }
            }
            return false;
        }

        /// <summary>This updates the depth values of the given pending nodes.</summary>
        /// <param name="pending">The initial set of nodes which are pending depth update.</param>
        static public void UpdateDepths(LinkedList<INode> pending) {
            while (pending.Count > 0) {
                INode node = pending.TakeFirst();
                int depth = node.Parents.MaxDepth() + 1;
                if (node.Depth != depth) {
                    (node as Node).Depth = depth;
                    pending.SortInsertUnique(node.Children);
                }
            }
        }

        /// <summary>The collection of children nodes to this node.</summary>
        private List<INode> children;

        /// <summary>Creates a new node.</summary>
        protected Node() {
            this.children = new List<INode>();
            this.Depth = 0;
        }

        /// <summary>The depth in the graph from the furthest input of this node.</summary>
        public int Depth { get; private set; }

        /// <summary>Determines if the node is constant or if all of it's parents are constant.</summary>
        /// <returns>True if constant, false otherwise.</returns>
        public bool IsConstant() {
            if (this is IConstant) return true;
            if (this is IInput)    return false;
            if (this is ITrigger)  return false;
            foreach (INode parent in this.Parents) {
                if (parent is not IConstant) return false;
            }
            return true;
        }

        /// <summary>Converts this node to a literal.</summary>
        /// <returns>A literal of this node, itself if already literal, or null if it can't become a literal.</returns>
        public abstract INode ToLiteral();

        /// <summary>Evaluates this node and updates it.</summary>
        /// <returns>
        /// The set of children that should be updated based on the results of this update.
        /// If this evaluation made no change then typically no children will be returned.
        /// Usually the entire set of children are returned on change, but it is not required.
        /// </returns>
        public abstract IEnumerable<INode> Eval();

        /// <summary>The set of parent nodes to this node in the graph.</summary>
        public abstract IEnumerable<INode> Parents { get; }

        /// <summary>The set of children nodes to this node in the graph.</summary>
        public IEnumerable<INode> Children => this.children;

        /// <summary>This is a helper method for setting a parent to the node.</summary>
        /// <typeparam name="T">The node type for the parent.</typeparam>
        /// <param name="parent">The parent variable being set.</param>
        /// <param name="newParent">The new parent being set, or null</param>
        protected void SetParent<T>(ref T parent, T newParent) where T : INode {
            if (ReferenceEquals(parent, newParent)) return;
            parent?.RemoveChildren(this);
            parent = newParent;
            newParent?.AddChildren(this);
        }

        /// <summary>Adds children nodes onto this node.</summary>
        /// <remarks>This will always check for loops.</remarks>
        /// <param name="children">The children to add.</param>
        public void AddChildren(params INode[] children) =>
            this.AddChildren(children as IEnumerable<INode>);

        /// <summary>Adds children nodes onto this node.</summary>
        /// <param name="children">The children to add.</param>
        /// <param name="checkedForLoops">Indicates if loops in the graph should be checked for.</param>
        public void AddChildren(IEnumerable<INode> children, bool checkedForLoops = true) {
            if (checkedForLoops && CanReachAny(this, children))
                throw Exception.NodeLoopDetected();
            LinkedList<INode> needsDepthUpdate = new();
            foreach (Node child in children) {
                if (child is not null && !this.children.Contains(child)) {
                    this.children.Add(child);
                    needsDepthUpdate.SortInsertUnique(child);
                }
            }
            UpdateDepths(needsDepthUpdate);
        }

        /// <summary>Removes all the given children from this node if they exist.</summary>
        /// <param name="children">The children to remove.</param>
        public void RemoveChildren(params INode[] children) =>
            this.RemoveChildren(children as IEnumerable<INode>);

        /// <summary>Removes all the given children from this node if they exist.</summary>
        /// <param name="children">The children to remove.</param>
        public void RemoveChildren(IEnumerable<INode> children) {
            LinkedList<INode> needsDepthUpdate = new();
            foreach (Node child in children) {
                if (child is not null) {
                    int index = this.children.IndexOf(child);
                    if (index >= 0) {
                        this.children.RemoveAt(index);
                        needsDepthUpdate.SortInsertUnique(child);
                    }
                }
            }
            UpdateDepths(needsDepthUpdate);
        }
    }
}