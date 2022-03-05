﻿using Blackboard.Core;
using Blackboard.Core.Actions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BlackboardTests.ParserTests {

    [TestClass]
    public class Optimization {

        [TestMethod]
        public void TestParseOptimization_Constants() {
            Slate slate = new();
            Formula formula = slate.LogRead(
                "in A = 21.0 + (1.0 + 3.0*2.0)*3.0;");
            formula.Check(
                "[",
                "  Namespace.A := Input<double>[0] {};",
                "  Input<double>[0] = Literal<double>[42] {};",
                "  Finish",
                "]");
        }

        [TestMethod]
        public void TestParseOptimization_IncorporateParents() {
            Slate slate = new();
            Formula formula = slate.LogRead(
                "in int A, B, C, D;",
                "E := (A + B + 2) + ((C + D) + 3);");
            formula.Check(
                "[",
                "  Namespace.A := Literal<double>[42] {};",
                "  Finish",
                "]");

        }
    }
}
