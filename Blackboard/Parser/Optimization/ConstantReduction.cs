﻿using Blackboard.Core.Extensions;
using Blackboard.Core.Inspect;
using Blackboard.Core.Nodes.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Blackboard.Parser.Optimization {

    /// <summary>
    /// This is an optimization rule for finding constant branches and replacing them with literals.
    /// This may leave nodes further down the constant branch in the nodes set.
    /// </summary>
    sealed internal class ConstantReduction: IRule {

        /// <summary>Creates a new constant reduction rule.</summary>
        public ConstantReduction() { }

        /// <summary>
        /// This evaluates down the branch starting from the given node
        /// to get the correct value prior to converting it to a constant.
        /// </summary>
        /// <param name="node">The node to evaluate.</param>
        /// <param name="nodes">The new nodes for a formula.</param>
        private void updateValue(INode node, HashSet<INode> nodes) {
            if (!nodes.Contains(node)) return;
            if (node is IChild child) {
                foreach (INode parent in child.Parents)
                    this.updateValue(parent, nodes);
            }
            if (node is IEvaluable eval)
                eval.Evaluate();
        }

        /// <summary>Recursively finds constant branches and replaces the branch with literals.</summary>
        /// <param name="node">The node of the tree to constant optimize.</param>
        /// <param name="nodes">The formula nodes to optimize.</param>
        /// <param name="logger">The logger to debug and inspect the optimization.</param>
        /// <remarks>The node to replace the given one in the parent or null to not replace.</remarks>
        public INode Perform(INode node, HashSet<INode> nodes, ILogger logger = null) {
            // If this node is not part of the new nodes, just return it.
            if (!nodes.Contains(node)) return null;

            // Check if the node can be turned into a constant.
            if (node.IsConstant()) {
                this.updateValue(node, nodes);
                IConstant con = node.ToConstant();
                if (con is not null && !ReferenceEquals(con, node)) {
                    logger?.Log("Replace {0} with constant {1}", node, con);
                    return con;
                }
            }

            // If the node is not a child just return the node.
            if (node is not IChild child) return null;

            // Check each parent in the child node.
            foreach (IParent parent in child.Parents.ToList()) {
                INode newNode = this.Perform(parent, nodes, logger);
                if (newNode is not null and IParent newParent)
                    child.ReplaceParent(parent, newParent);
            }

            return null;
        }
    }
}
