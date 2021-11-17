﻿using Blackboard.Core;
using Blackboard.Core.Data.Caps;
using Blackboard.Core.Nodes.Inner;
using Blackboard.Core.Nodes.Interfaces;
using Blackboard.Core.Nodes.Outer;
using Blackboard.Parser.Performers;
using Blackboard.Parser.Preppers;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using PP = PetiteParser;
using S = System;

namespace Blackboard.Parser {

    /// <summary>This will parse the Blackboard language into actions and nodes to apply to the driver.</summary>
    sealed public class Parser {

        /// <summary>The resource file for the Blackboard language definition.</summary>
        private const string resourceName = "Blackboard.Parser.Parser.lang";

        /// <summary>Prepares the parser's static variables before they are used.</summary>
        static Parser() {
            baseParser = PP.Loader.Loader.LoadParser(
                PP.Scanner.Default.FromResource(Assembly.GetExecutingAssembly(), resourceName));
        }

        /// <summary>The Blackboard language base parser lazy singleton.</summary>
        static private readonly PP.Parser.Parser baseParser;

        private readonly Driver driver;
        private readonly Formula formula;
        private Dictionary<string, PP.ParseTree.PromptHandle> prompts;

        private readonly LinkedList<object> stash;
        private readonly LinkedList<IPrepper> stack;

        /// <summary>Creates a new Blackboard language parser.</summary>
        /// <param name="driver">The driver to modify.</param>
        public Parser(Driver driver) {
            this.driver = driver;
            this.formula = new Formula(driver);
            this.prompts = null;

            this.stash = new LinkedList<object>();
            this.stack = new LinkedList<IPrepper>();

            this.initPrompts();
            this.validatePrompts();
        }

        #region Formula Methods...

        /// <summary>Reads the given lines of input Blackline code.</summary>
        /// <remarks>The commands of this input will be added to formula if valid.</remarks>
        /// <param name="input">The input code to parse.</param>
        public void Read(params string[] input) =>
            this.Read(input as IEnumerable<string>);

        /// <summary>Reads the given lines of input Blackline code.</summary>
        /// <remarks>The commands of this input will be added to formula if valid.</remarks>
        /// <param name="input">The input code to parse.</param>
        public void Read(IEnumerable<string> input, string name = "Unnamed") {
            PP.Parser.Result result = baseParser.Parse(new PP.Scanner.Default(input, name));
            if (result.Errors.Length > 0)
                throw new Exception(result.Errors.Join("\n"));
            this.read(result.Tree);
        }

        /// <summary>Reads the given parse tree root for an input Blackline code.</summary>
        /// <param name="node">The parsed tree root node to read from.</param>
        private void read(PP.ParseTree.ITreeNode node) {
            try {
                node.Process(this.prompts);
            } catch(S.Exception ex) {
                throw new Exception("Error occurred while parsing input code.", ex);
            }
        }

        /// <summary>This will dispose of all pending actions.</summary>
        public void Discard() => this.formula.Reset();

        /// <summary>This will perform and apply all pending action to Blackboard.</summary>
        public void Commit() => this.formula.Perform();

        /// <summary>Gets the debug string of the uncommited parse state.</summary>
        /// <returns>A human readable debug string.</returns>
        public string FormulaToString() => this.formula.ToString();

        #endregion
        #region Prompts Setup...

        /// <summary>Initializes the prompts and operators for this parser.</summary>
        private void initPrompts() {
            this.prompts = new Dictionary<string, PP.ParseTree.PromptHandle>() {
                { "clear",         this.handleClear },
                { "pushNamespace", this.handlePushNamespace },
                { "popNamespace",  this.handlePopNamespace },

                { "newTypeInputNoAssign",   this.handleNewTypeInputNoAssign },
                { "newTypeInputWithAssign", this.handleNewTypeInputWithAssign },
                { "newVarInputWithAssign",  this.handleNewVarInputWithAssign },

                { "typeDefine",                this.handleTypeDefine },
                { "varDefine",                 this.handleVarDefine },
                { "provokeTrigger",            this.handleProvokeTrigger },
                { "conditionalProvokeTrigger", this.handleConditionalProvokeTrigger },

                { "assignment",   this.handleAssignment },
                { "cast",         this.handleCast },
                { "memberAccess", this.handleMemberAccess },
                { "startCall",    this.handleStartCall },
                { "addArg",       this.handleAddArg },
                { "pushId",       this.handlePushId },
                { "pushBool",     this.handlePushBool },
                { "pushBin",      this.handlePushBin },
                { "pushOct",      this.handlePushOct },
                { "pushInt",      this.handlePushInt },
                { "pushHex",      this.handlePushHex },
                { "pushDouble",   this.handlePushDouble },
                { "pushString",   this.handlePushString },
                { "pushType",     this.handlePushType },
            };

            this.addProcess(3, "trinary");
            this.addProcess(2, "logicalOr");
            this.addProcess(2, "logicalXor");
            this.addProcess(2, "logicalAnd");
            this.addProcess(2, "or");
            this.addProcess(2, "xor");
            this.addProcess(2, "and");
            this.addProcess(2, "equal");
            this.addProcess(2, "notEqual");
            this.addProcess(2, "greater");
            this.addProcess(2, "less");
            this.addProcess(2, "greaterEqual");
            this.addProcess(2, "lessEqual");
            this.addProcess(2, "shiftRight");
            this.addProcess(2, "shiftLeft");
            this.addProcess(2, "sum");
            this.addProcess(2, "subtract");
            this.addProcess(2, "multiply");
            this.addProcess(2, "divide");
            this.addProcess(2, "modulo");
            this.addProcess(2, "remainder");
            this.addProcess(2, "power");
            this.addProcess(1, "negate");
            this.addProcess(1, "not");
            this.addProcess(1, "invert");
        }

        /// <summary>This adds a prompt for an operator handler.</summary>
        /// <param name="count">The number of values to pop off the stack for this function.</param>
        /// <param name="name">The name of the prompt to add to.</param>
        private void addProcess(int count, string name) {
            INode funcGroup = this.driver.Global.Find(Driver.OperatorNamespace, name);
            this.prompts[name] = (PP.ParseTree.PromptArgs args) => {
                PP.Scanner.Location loc = args.Tokens[^1].End;
                IPrepper[] inputs = this.pop<IPrepper>(count);
                this.push(new FuncPrep(loc, new NoPrep(funcGroup), inputs));
            };
        }
    
        /// <summary>Validates that all prompts in the grammar are handled.</summary>
        private void validatePrompts() {
            // TODO: Move most of this over to PetiteParser.
            HashSet<string> remaining = new(baseParser.Grammar.Prompts.Select((prompt) => prompt.Name));
            HashSet<string> missing = new();
            foreach (string name in this.prompts.Keys) {
                if (remaining.Contains(name)) remaining.Remove(name);
                else missing.Add(name);
            }
            if (remaining.Count > 0 || missing.Count > 0)
                throw new Exception("Blackboard's parser grammer has prompts which do not match prompt handlers.").
                    With("Not handled", remaining.Join(", ")).
                    With("Not in grammer", missing.Join(", "));
        }

        #endregion
        #region Stack Helpers...

        /// <summary>Pushes a prepper onto the stack.</summary>
        /// <param name="prepper">The prepper to push.</param>
        private void push(IPrepper prepper) => this.stack.AddLast(prepper);

        /// <summary>Pops off a prepper is on the top of the stack.</summary>
        /// <typeparam name="T">The type of the prepper to read as.</typeparam>
        /// <returns>The prepper which was on top of the stack.</returns>
        private T pop<T>() where T : class, IPrepper {
            IPrepper item = this.stack.Last.Value;
            this.stack.RemoveLast();
            return item as T;
        }

        /// <summary>Pops one or more prepper off the stack.</summary>
        /// <typeparam name="T">The types of the preppers to read.</typeparam>
        /// <param name="count">The number of preppers to pop.</param>
        /// <returns>The popped preppers in the order oldest to newest.</returns>
        private T[] pop<T>(int count) where T: class, IPrepper {
            T[] items = new T[count];
            for (int i = 0; i < count; i++)
                items[count-1-i] = this.pop<T>();
            return items;
        }

        /// <summary>Pushes an object onto the stash stack.</summary>
        /// <param name="value">The value to push.</param>
        private void stashPush(object value) => this.stash.AddLast(value);

        /// <summary>Pops off an object is on the top of the stash stack.</summary>
        /// <typeparam name="T">The type of the object to read as.</typeparam>
        /// <returns>The object which was on top of the stash stack.</returns>
        private T stashPop<T>() where T : class {
            object value = this.stash.Last.Value;
            this.stash.RemoveLast();
            return value as T;
        }

        #endregion
        #region Prompt Handlers...

        /// <summary>This is called before each statement to prepare and clean up the parser.</summary>
        /// <param name="args">The token information from the parser.</param>
        private void handleClear(PP.ParseTree.PromptArgs args) {
            args.Tokens.Clear();
            this.stash.Clear();
            this.stack.Clear();
        }

        /// <summary>This is called when the namespace has openned.</summary>
        /// <param name="args">The token information from the parser.</param>
        private void handlePushNamespace(PP.ParseTree.PromptArgs args) {
            PP.Tokenizer.Token token = args.Tokens[^1];
            PP.Scanner.Location loc = token.End;
            string name = token.Text;

            IWrappedNode scope = this.formula.CurrentScope;
            IWrappedNode next = scope.ReadField(name);
            if (next is not null) {
                if (next.Type.IsAssignableTo(typeof(Namespace)))
                    throw new Exception("Can not open namespace. Another non-namespace exists by that name.").
                         With("Identifier", name).
                         With("Location", loc);
                scope = next;
            } else {
                // Create a new virtual namespace and a performer to construct the new namespace if this formula is run.
                VirtualNode nextScope = scope.CreateField(name, typeof(Namespace));
                scope = nextScope;
                this.formula.Add(new VirtualNodeWriter(nextScope, new NodeHold(new Namespace())));
            }

            this.formula.PushScope(scope);
        }

        /// <summary>This is called when the namespace had closed.</summary>
        /// <param name="args">The token information from the parser.</param>
        private void handlePopNamespace(PP.ParseTree.PromptArgs args) =>
            this.formula.PopScope();

        /// <summary>This creates a new input node of a specific type without assigning the value.</summary>
        /// <param name="args">The token information from the parser.</param>
        private void handleNewTypeInputNoAssign(PP.ParseTree.PromptArgs args) {
            IdPrep target = this.pop<IdPrep>();
            Type t = this.stashPop<Type>();
            PP.Scanner.Location loc = args.Tokens[^1].End;

            IFuncDef inputFactory =
                t == Type.Bool    ? InputValue<Bool>.Factory :
                t == Type.Int     ? InputValue<Int>.Factory :
                t == Type.Double  ? InputValue<Double>.Factory :
                t == Type.String  ? InputValue<String>.Factory :
                t == Type.Trigger ? InputTrigger.Factory :
                throw new Exception("Unsupported type for new typed input").
                    With("Location", loc).
                    With("Type", t);

            VirtualNode virtualInput = target.CreateNode(formula, inputFactory.ReturnType);
            IPerformer inputPerf = new FuncPrep(loc, new NoPrep(inputFactory)).Prepare(formula);
            this.formula.Add(new VirtualNodeWriter(virtualInput, inputPerf));

            // Push the type back onto the stack for the next assignment.
            this.stashPush(t);
        }

        /// <summary>This creates a new input node of a specific type and assigns it with an initial value.</summary>
        /// <param name="args">The token information from the parser.</param>
        private void handleNewTypeInputWithAssign(PP.ParseTree.PromptArgs args) {
            IPrepper value = this.pop<IPrepper>();
            IdPrep target = this.pop<IdPrep>();
            Type t = this.stashPop<Type>();
            PP.Scanner.Location loc = args.Tokens[^1].End;

            IPerformer valuePerf = value.Prepare(formula, true);
            IFuncDef inputFactory =
                t == Type.Bool    ? InputValue<Bool>.FactoryWithInitialValue :
                t == Type.Int     ? InputValue<Int>.FactoryWithInitialValue :
                t == Type.Double  ? InputValue<Double>.FactoryWithInitialValue :
                t == Type.String  ? InputValue<String>.FactoryWithInitialValue :
                t == Type.Trigger ? InputTrigger.FactoryWithInitialValue :
                throw new Exception("Unsupported type for new typed input with assignment").
                    With("Type", t);

            VirtualNode virtualInput = target.CreateNode(formula, inputFactory.ReturnType);
            Type valueType = Type.FromType(valuePerf.Type);
            if (!t.Match(valueType).IsMatch)
                throw new Exception("May not assign the value to that type of input.").
                    With("Location", loc).
                    With("Input Type", t).
                    With("Value Type", valueType);

            IPerformer inputPerf = new FuncPrep(loc, new NoPrep(inputFactory), new NoPrep(valuePerf)).Prepare(formula);
            this.formula.Add(new VirtualNodeWriter(virtualInput, inputPerf));

            // Push the type back onto the stack for the next assignment.
            this.stashPush(t);
        }

        /// <summary>This creates a new input node and assigns it with an initial value.</summary>
        /// <param name="args">The token information from the parser.</param>
        private void handleNewVarInputWithAssign(PP.ParseTree.PromptArgs args) {
            IPrepper value = this.pop<IPrepper>();
            IdPrep target = this.pop<IdPrep>();
            PP.Scanner.Location loc = args.Tokens[^1].End;

            IPerformer valuePerf = value.Prepare(formula, true);
            Type t = Type.FromType(valuePerf.Type);
            IFuncDef inputFactory =
                t == Type.Bool    ? InputValue<Bool>.FactoryWithInitialValue :
                t == Type.Int     ? InputValue<Int>.FactoryWithInitialValue :
                t == Type.Double  ? InputValue<Double>.FactoryWithInitialValue :
                t == Type.String  ? InputValue<String>.FactoryWithInitialValue :
                t == Type.Trigger ? InputTrigger.FactoryWithInitialValue :
                throw new Exception("Unsupported type for new input").
                    With("Location", loc).
                    With("Type", t);

            VirtualNode virtualInput = target.CreateNode(formula, inputFactory.ReturnType);
            IPerformer inputPerf = new FuncPrep(loc, new NoPrep(inputFactory), new NoPrep(valuePerf)).Prepare(formula);
            this.formula.Add(new VirtualNodeWriter(virtualInput, inputPerf));
        }

        /// <summary>This handles defining a new typed named node.</summary>
        /// <param name="args">The token information from the parser.</param>
        private void handleTypeDefine(PP.ParseTree.PromptArgs args) {
            IPrepper value = this.pop<IPrepper>();
            IdPrep target = this.pop<IdPrep>();
            Type t = this.stashPop<Type>();
            PP.Scanner.Location loc = args.Tokens[^1].End;

            IPerformer valuePerf = value.Prepare(formula, false);
            Type valueType  = Type.FromType(valuePerf.Type);
            TypeMatch match = t.Match(valueType);
            if (!match.IsMatch)
                throw new Exception("May not define the value to that type of input.").
                    With("Location", loc).
                    With("Input Type", t).
                    With("Value Type", valueType);

            VirtualNode virtualInput = target.CreateNode(formula, t.RealType);
            if (match.NeedsCast) {
                INode castGroup =
                    t == Type.Bool    ? driver.Global.Find(Driver.OperatorNamespace, "castBool") :
                    t == Type.Int     ? driver.Global.Find(Driver.OperatorNamespace, "castInt") :
                    t == Type.Double  ? driver.Global.Find(Driver.OperatorNamespace, "castDouble") :
                    t == Type.String  ? driver.Global.Find(Driver.OperatorNamespace, "castString") :
                    t == Type.Trigger ? driver.Global.Find(Driver.OperatorNamespace, "castTrigger") :
                    throw new Exception("Unsupported type for new definition cast").
                        With("Location", loc).
                        With("Type", t);

                IFuncDef castFunc = (castGroup as IFuncGroup).Find(valueType);
                valuePerf = new Function(castFunc, valuePerf);
            }
            this.formula.Add(new VirtualNodeWriter(virtualInput, valuePerf));

            // Push the type back onto the stack for the next definition.
            this.stashPush(t);
        }

        /// <summary>This handles defining a new untyped named node.</summary>
        /// <param name="args">The token information from the parser.</param>
        private void handleVarDefine(PP.ParseTree.PromptArgs args) {
            IPrepper value = this.pop<IPrepper>();
            IdPrep target = this.pop<IdPrep>();

            IPerformer  valuePerf    = value.Prepare(formula, false);
            VirtualNode virtualInput = target.CreateNode(formula, valuePerf.Type);
            this.formula.Add(new VirtualNodeWriter(virtualInput, valuePerf));
        }

        /// <summary>This handles when a trigger is provoked unconditionally.</summary>
        /// <param name="args">The token information from the parser.</param>
        private void handleProvokeTrigger(PP.ParseTree.PromptArgs args) {
            PP.Scanner.Location loc = args.Tokens[^1].End;
            IdPrep target = this.pop<IdPrep>();
            IPerformer targetPerf = target.Prepare(formula, false);

            NoPrep valuePrep = new(Literal.Bool(true)), targetPrep = new(targetPerf), funcPrep = new(InputTrigger.Assign);
            this.formula.Add(new FuncPrep(loc, funcPrep, targetPrep, valuePrep).Prepare(formula));

            // Push the literal true onto the stack for any following trigger pulls.
            this.push(valuePrep);
        }

        /// <summary>This handles when a trigger should only be provoked if a condition returns true.</summary>
        /// <param name="args">The token information from the parser.</param>
        private void handleConditionalProvokeTrigger(PP.ParseTree.PromptArgs args) {
            PP.Scanner.Location loc = args.Tokens[^1].End;
            IdPrep target  = this.pop<IdPrep>();
            IPrepper value = this.pop<IPrepper>();

            IPerformer valuePerf  = value.Prepare(formula, false);
            IPerformer targetPerf = target.Prepare(formula, false);

            Type valueType  = Type.FromType(valuePerf.Type);
            TypeMatch match = Type.Trigger.Match(valueType);
            if (!match.IsMatch)
                throw new Exception("May only conditionally provoke a trigger with a bool or trigger.").
                    With("Location", loc).
                    With("Conditional Type", valueType);

            NoPrep valuePrep = new(valuePerf), targetPrep = new(targetPerf), funcPrep = new(InputTrigger.Assign);
            this.formula.Add(new FuncPrep(loc, funcPrep, targetPrep, valuePrep).Prepare(formula));

            // Push the condition onto the stack for any following trigger pulls.
            this.push(valuePrep);
        }

        /// <summary>This handles assigning the left value to the right value.</summary>
        /// <param name="args">The token information from the parser.</param>
        private void handleAssignment(PP.ParseTree.PromptArgs args) {
            IPrepper value  = this.pop<IPrepper>();
            IPrepper target = this.pop<IPrepper>();
            PP.Scanner.Location loc = args.Tokens[^1].End;

            IPerformer valuePerf  = value.Prepare(formula, false);
            IPerformer targetPerf = target.Prepare(formula, false);

            // Check if the value is an input, this may have to change if we allow assignments to non-input fields.
            if (targetPerf is not WrappedNodeReader targetReader)
                throw new Exception("The target of an assignment must be a wrapped node.").
                    With("Location", loc).
                    With("Target", targetPerf);
            IWrappedNode wrappedTarget = targetReader.WrappedNode;
            if (!wrappedTarget.Type.IsAssignableTo(typeof(IInput)))
                throw new Exception("The target of an assignment must be an input node.").
                    With("Location", loc).
                    With("Type", wrappedTarget.Type).
                    With("Target", wrappedTarget);

            // Check if the base types match. Don't need to check that the type is
            // a data type or trigger since only those can be reduced to constents.
            Type valueType  = Type.FromType(valuePerf.Type);
            Type targetType = Type.FromType(targetPerf.Type);
            if (!valueType.Match(targetType).IsMatch)
                throw new Exception("The value of an assignment must match base types.").
                    With("Location", loc).
                    With("Target", targetPerf).
                    With("Value", valuePerf);

            IFuncDef assignFunc =
                targetType == Type.Bool    ? InputValue<Bool>.Assign :
                targetType == Type.Int     ? InputValue<Int>.Assign :
                targetType == Type.Double  ? InputValue<Double>.Assign :
                targetType == Type.String  ? InputValue<String>.Assign :
                targetType == Type.Trigger ? InputTrigger.Assign :
                throw new Exception("Unsupported type for assignment").
                    With("Location", loc).
                    With("Type", targetType);

            NoPrep valuePrep = new(valuePerf), targetPrep = new(targetPerf), funcPrep = new(assignFunc);
            this.formula.Add(new FuncPrep(loc, funcPrep, targetPrep, valuePrep).Prepare(formula));

            // Push the value back onto the stack for any following assignments.
            this.push(valuePrep);
        }

        /// <summary>This handles performing a type cast of a node.</summary>
        /// <param name="args">The token information from the parser.</param>
        private void handleCast(PP.ParseTree.PromptArgs args) {
            //IPrepper value = this.pop<IPrepper>();
            //Type t = this.stashPop<Type>();

            // TODO: IMPLEMENT

        }

        /// <summary>This handles accessing an identifier to find the receiver for the next identifier.</summary>
        /// <param name="args">The token information from the parser.</param>
        private void handleMemberAccess(PP.ParseTree.PromptArgs args) {
            PP.Tokenizer.Token token = args.Tokens[^1];
            PP.Scanner.Location loc = token.End;
            string name = token.Text;
            IPrepper receiver = this.pop<IPrepper>();
            
            this.push(new IdPrep(loc, receiver, name));
        }

        /// <summary>This handles preparing for a method call.</summary>
        /// <param name="args">The token information from the parser.</param>
        private void handleStartCall(PP.ParseTree.PromptArgs args) {
            IPrepper item = this.pop<IPrepper>();
            PP.Scanner.Location loc = args.Tokens[^1].End;
            this.push(new FuncPrep(loc, item));
        }

        /// <summary>This handles the end of a method call and creates the node for the method.</summary>
        /// <param name="args">The token information from the parser.</param>
        private void handleAddArg(PP.ParseTree.PromptArgs args) {
            IPrepper arg = this.pop<IPrepper>();
            FuncPrep func = this.pop<FuncPrep>();
            func.Arguments.Add(arg);
            this.push(func);
        }

        /// <summary>This handles looking up a node by an id and pushing the node onto the stack.</summary>
        /// <param name="args">The token information from the parser.</param>
        private void handlePushId(PP.ParseTree.PromptArgs args) {
            PP.Tokenizer.Token token = args.Tokens[^1];
            PP.Scanner.Location loc = token.End;
            string name = token.Text;

            this.push(new IdPrep(loc, this.formula.Scopes, name));
        }

        /// <summary>This handles pushing a bool literal value onto the stack.</summary>
        /// <param name="args">The token information from the parser.</param>
        private void handlePushBool(PP.ParseTree.PromptArgs args) {
            PP.Tokenizer.Token token = args.Tokens[^1];
            PP.Scanner.Location loc = token.End;
            string text = token.Text;

            try {
                bool value = bool.Parse(text);
                this.push(LiteralPrep.Bool(value));
            } catch (S.Exception ex) {
                throw new Exception("Failed to parse a bool.", ex).
                    With("Text", text).
                    With("Location", loc);
            }
        }
   
        /// <summary>This handles pushing a binary int literal value onto the stack.</summary>
        /// <param name="args">The token information from the parser.</param>
        private void handlePushBin(PP.ParseTree.PromptArgs args) {
            PP.Tokenizer.Token token = args.Tokens[^1];
            PP.Scanner.Location loc = token.End;
            string text = token.Text;

            try {
                int value = S.Convert.ToInt32(text, 2);
                this.push(LiteralPrep.Int(value));
            } catch (S.Exception ex) {
                throw new Exception("Failed to parse a binary int.", ex).
                    With("Text", text).
                    With("Location", loc);
            }
        }

        /// <summary>This handles pushing an ocatal int literal value onto the stack.</summary>
        /// <param name="args">The token information from the parser.</param>
        private void handlePushOct(PP.ParseTree.PromptArgs args) {
            PP.Tokenizer.Token token = args.Tokens[^1];
            PP.Scanner.Location loc = token.End;
            string text = token.Text;

            try {
                int value = S.Convert.ToInt32(text, 8);
                this.push(LiteralPrep.Int(value));
            } catch (S.Exception ex) {
                throw new Exception("Failed to parse an octal int.", ex).
                    With("Text", text).
                    With("Location", loc);
            }
        }

        /// <summary>This handles pushing a decimal int literal value onto the stack.</summary>
        /// <param name="args">The token information from the parser.</param>
        private void handlePushInt(PP.ParseTree.PromptArgs args) {
            PP.Tokenizer.Token token = args.Tokens[^1];
            PP.Scanner.Location loc = token.End;
            string text = token.Text;

            try {
                int value = int.Parse(text);
                this.push(LiteralPrep.Int(value));
            } catch (S.Exception ex) {
                throw new Exception("Failed to parse a decimal int.", ex).
                    With("Text", text).
                    With("Location", loc);
            }
        }

        /// <summary>This handles pushing a hexadecimal int literal value onto the stack.</summary>
        /// <param name="args">The token information from the parser.</param>
        private void handlePushHex(PP.ParseTree.PromptArgs args) {
            PP.Tokenizer.Token token = args.Tokens[^1];
            PP.Scanner.Location loc = token.End;
            string text = token.Text[2..];

            try {
                int value = int.Parse(text, NumberStyles.HexNumber);
                this.push(LiteralPrep.Int(value));
            } catch (S.Exception ex) {
                throw new Exception("Failed to parse a hex int.", ex).
                    With("Text", text).
                    With("Location", loc);
            }
        }

        /// <summary>This handles pushing a double literal value onto the stack.</summary>
        /// <param name="args">The token information from the parser.</param>
        private void handlePushDouble(PP.ParseTree.PromptArgs args) {
            PP.Tokenizer.Token token = args.Tokens[^1];
            PP.Scanner.Location loc = token.End;
            string text = token.Text;

            try {
                double value = double.Parse(text);
                this.push(LiteralPrep.Double(value));
            } catch (S.Exception ex) {
                throw new Exception("Failed to parse a double.", ex).
                    With("Text", text).
                    With("Location", loc);
            }
        }

        /// <summary>This handles pushing a string literal value onto the stack.</summary>
        /// <param name="args">The token information from the parser.</param>
        private void handlePushString(PP.ParseTree.PromptArgs args) {
            PP.Tokenizer.Token token = args.Tokens[^1];
            PP.Scanner.Location loc = token.End;
            string text = token.Text;

            try {
                string value = PP.Misc.Text.Unescape(text);
                this.push(LiteralPrep.String(value));
            } catch (S.Exception ex) {
                throw new Exception("Failed to decode escaped sequences.", ex).
                    With("Text", text).
                    With("Location", loc);
            }
        }

        /// <summary>This handles pushing a type onto the stack.</summary>
        /// <param name="args">The token information from the parser.</param>
        private void handlePushType(PP.ParseTree.PromptArgs args) {
            PP.Tokenizer.Token token = args.Tokens[^1];
            PP.Scanner.Location loc = token.End;
            string text = token.Text;
            
            Type t = Type.FromName(text);
            if (t is null)
                throw new Exception("Unrecognized type name.").
                    With("Text", text).
                    With("Location", loc);

            this.stashPush(t);
        }

        #endregion
    }
}
