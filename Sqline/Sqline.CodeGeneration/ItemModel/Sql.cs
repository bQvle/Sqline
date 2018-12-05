// Authors="Daniel Jonas Møller, Anders Eggers-Krag" License="New BSD License http://sqline.codeplex.com/license"
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Sqline.Base;

namespace Sqline.CodeGeneration.ViewModel {
    public class SQLWithCondition {
        private string FValue;
        private string FCondition;

        public SQLWithCondition(string value, string condition) {
            FValue = value;
            FCondition = condition;
        }

        public string Value {
            get {
                return FValue;
            }
        }

        public string Condition {
            get {
                return FCondition;
            }
        }
    }


    public class Sql {
        private IOwner FOwner;
        private string FStatement;
        private List<SQLWithCondition> FSQLWithConditions;


        public Sql(IOwner owner, XElement element) {
            FOwner = owner;
            FStatement = element.Value.Replace("\"", "\"\"").Trim().NormalizeLineEndings();
            FSQLWithConditions = new List<SQLWithCondition>();

            foreach (XNode node in element.Nodes()) {
                if (node.NodeType == System.Xml.XmlNodeType.Element && ((XElement)node).Name == ItemFile.XmlNamespace + "condition") {
                    FSQLWithConditions.Add(new SQLWithCondition(
                        ((XElement)node).Value.Replace("\"", "\"\"").Trim().NormalizeLineEndings(),
                        ((XElement)node).Attribute("parameter").Value.ToCamelCase()
                    ));
                }
                else if (node.NodeType == System.Xml.XmlNodeType.Text) {
                    FSQLWithConditions.Add(new SQLWithCondition(
                        node.ToString().Replace("\"", "\"\"").Trim().NormalizeLineEndings(),
                        null
                    ));
                }
            }


        }

        public bool Conditional {
            get {
                return SQLWithConditions.Any(x => x.Condition != null);
            }
        }

        public List<SQLWithCondition> SQLWithConditions {
            get {
                return FSQLWithConditions;
            }
        }

        public string Statement {
            get {
                return FStatement;
            }
        }




    }
}
