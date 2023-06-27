﻿using Blackboard.Core.Data.Interfaces;
using Blackboard.Core.Nodes.Bases;
using Blackboard.Core.Nodes.Interfaces;

namespace Blackboard.Core.Nodes.Outer;

/// <summary>An external node as a placeholder for value node.</summary>
/// <typeparam name="T">The type of the value to hold.</typeparam>
sealed public class ExternValue<T> : ValueNode<T>, IValueExtern<T>
    where T : struct, IData, IEquatable<T> {

    /// <summary>Creates a new extern value node.</summary>
    public ExternValue() { }

    /// <summary>Creates a new extern value node.</summary>
    /// <param name="value">The initial value for this node.</param>
    public ExternValue(T value = default) => this.SetValue(value);
    
    /// <summary>Creates a new instance of this node with no parents but similar configuration.</summary>
    /// <returns>The new instance of this node.</returns>
    public override INode NewInstance() => new ExternValue<T>();

    /// <summary>This is the type name of the node.</summary>
    /// <remarks>Doesn't use nameof since this has both trigger and value nodes.</remarks>
    public override string TypeName => "Extern";

    /// <summary>Always return the initial value from the extern.</summary>
    /// <returns>The initial value.</returns>
    protected override T CalculateValue() => this.Value;

    /// <summary>This sets the value of this node.</summary>
    /// <param name="value">The value to set.</param>
    /// <returns>True if the value has changed, false otherwise.</returns>
    public bool SetValue(T value) => this.UpdateValue(value);
}
