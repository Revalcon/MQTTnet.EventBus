using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace MQTTnet.EventBus.Reflection
{
    internal class TopicPatternVisitor : ExpressionVisitor
    {
        private readonly StringBuilder _constBuilder;
        private readonly List<string> _members;

        public TopicPatternVisitor(StringBuilder builder, List<string> members)
        {
            _constBuilder = builder;
            _members = members;
        }

        public override Expression Visit(Expression node)
        {
            if (node != null)
            {
                if (node.NodeType == ExpressionType.Constant)
                {
                    _constBuilder.Append(((ConstantExpression)node).Value);
                }
                else if (node.NodeType == ExpressionType.MemberAccess)
                {
                    _members.Add("{" + ((MemberExpression)node).Member.Name + "}");
                }
            }

            return base.Visit(node);
        }
    }
}
