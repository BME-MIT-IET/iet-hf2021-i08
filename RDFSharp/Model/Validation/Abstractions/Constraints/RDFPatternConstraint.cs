﻿/*
   Copyright 2012-2020 Marco De Salvo

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

     http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using RDFSharp.Query;
using System;
using System.Text;
using System.Text.RegularExpressions;

namespace RDFSharp.Model
{
    /// <summary>
    /// RDFPatternConstraint represents a SHACL constraint on the specified regular expression for a given RDF term
    /// </summary>
    public class RDFPatternConstraint : RDFConstraint {

        #region Properties
        /// <summary>
        /// Regular Expression to be applied on the given RDF term
        /// </summary>
        public Regex RegEx { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a named pattern constraint
        /// </summary>
        public RDFPatternConstraint(RDFResource constraintName, Regex regex) : base(constraintName) {
            if (regex != null) {
                this.RegEx = regex;
            }
            else {
                throw new RDFModelException("Cannot create RDFPatternConstraint because given \"regex\" parameter is null.");
            }
        }

        /// <summary>
        /// Default-ctor to build a blank pattern constraint
        /// </summary>
        public RDFPatternConstraint(Regex regex) : this(new RDFResource(), regex) { }
        #endregion

        #region Methods
        /// <summary>
        /// Evaluates this constraint against the given data graph
        /// </summary>
        internal override RDFValidationReport EvaluateConstraint(RDFShapesGraph shapesGraph,
                                                                 RDFShape shape,
                                                                 RDFGraph dataGraph,
                                                                 RDFResource focusNode,
                                                                 RDFPatternMember valueNode) {
            RDFValidationReport report = new RDFValidationReport(new RDFResource());
            switch (valueNode) {

                //Resource
                case RDFResource valueNodeResource:
                    if (valueNodeResource.IsBlank || !this.RegEx.IsMatch(valueNodeResource.ToString())) {
                        report.AddResult(new RDFValidationResult(shape,
                                                                 RDFVocabulary.SHACL.PATTERN_CONSTRAINT_COMPONENT,
                                                                 focusNode,
                                                                 shape is RDFPropertyShape ? ((RDFPropertyShape)shape).Path : null,
                                                                 valueNode,
                                                                 shape.Messages,
                                                                 new RDFResource(),
                                                                 shape.Severity));
                    }
                    break;

                //Literal
                case RDFLiteral valueNodeLiteral:
                    if (!this.RegEx.IsMatch(valueNodeLiteral.Value)) {
                        report.AddResult(new RDFValidationResult(shape,
                                                                 RDFVocabulary.SHACL.PATTERN_CONSTRAINT_COMPONENT,
                                                                 focusNode,
                                                                 shape is RDFPropertyShape ? ((RDFPropertyShape)shape).Path : null,
                                                                 valueNode,
                                                                 shape.Messages,
                                                                 new RDFResource(),
                                                                 shape.Severity));
                    }
                    break;

            }
            return report;
        }

        /// <summary>
        /// Gets a graph representation of this constraint
        /// </summary>
        internal override RDFGraph ToRDFGraph(RDFShape shape) {
            RDFGraph result = new RDFGraph();
            if (shape != null) {

                //sh:pattern
                result.AddTriple(new RDFTriple(shape, RDFVocabulary.SHACL.PATTERN, new RDFTypedLiteral(this.RegEx.ToString(), RDFModelEnums.RDFDatatypes.XSD_STRING)));

                //sh:flags
                StringBuilder regexFlags = new StringBuilder();
                if (this.RegEx.Options.HasFlag(RegexOptions.IgnoreCase))
                    regexFlags.Append("i");
                if (this.RegEx.Options.HasFlag(RegexOptions.Singleline))
                    regexFlags.Append("s");
                if (this.RegEx.Options.HasFlag(RegexOptions.Multiline))
                    regexFlags.Append("m");
                if (this.RegEx.Options.HasFlag(RegexOptions.IgnorePatternWhitespace))
                    regexFlags.Append("x");
                if (regexFlags.ToString() != String.Empty)
                    result.AddTriple(new RDFTriple(shape, RDFVocabulary.SHACL.FLAGS, new RDFTypedLiteral(regexFlags.ToString(), RDFModelEnums.RDFDatatypes.XSD_STRING)));

            }
            return result;
        }
        #endregion

    }
}