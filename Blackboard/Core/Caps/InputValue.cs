﻿using System.Collections.Generic;
using Blackboard.Core.Interfaces;
using Blackboard.Core.Bases;
using System.Linq;
using System;

namespace Blackboard.Core.Caps {

    public class InputValue<T>: ValueNode<T>, IValueInput<T> {

        private string name;
        private INamespace scope;

        public InputValue(string name = "Input", INamespace scope = null, T value = default): base(value) {
            this.Name = name;
            this.Scope = scope;
        }

        /// <summary>Gets or sets the name for the node.</summary>
        public string Name {
            get => this.name;
            set => Namespace.SetName(this, value);
        }

        /// <summary>Gets or sets the containing scope for this name or null.</summary>
        public INamespace Scope {
            get => this.scope;
            set {
                if (value?.Exists(this.name) ?? false)
                    throw Exception.RenameDuplicateInScope(this.name, value);
                this.scope?.RemoveChildren(this);
                this.scope = value;
                this.scope?.AddChildren(this);
            }
        }

        /// <summary>The set of parent nodes to this node in the graph.</summary>
        public override IEnumerable<INode> Parents {
            get {
                if (!(this.scope is null)) yield return this.scope;
            }
        }

        public bool SetValue(T value) => this.SetNodeValue(value);

        protected override bool UpdateValue() => true;

        /// <summary>Gets the string for this node.</summary>
        /// <returns>The debug string for this node.</returns>
        public override string ToString() =>
            (this.scope is null ? "" : this.Scope.ToString()+".")+this.Name;
    }
}
