/////////////////////////////////////////////////////////////////////////////////////////
//  MIT License                                                                        //
//                                                                                     //
//  Copyright (c) 2022 Hyuntae Na                                                      //
//                                                                                     //
//  Permission is hereby granted, free of charge, to any person obtaining a copy       //
//  of this software and associated documentation files (the "Software"), to deal      //
//  in the Software without restriction, including without limitation the rights       //
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell          //
//  copies of the Software, and to permit persons to whom the Software is              //
//  furnished to do so, subject to the following conditions:                           //
//                                                                                     //
//  The above copyright notice and this permission notice shall be included in all     //
//  copies or substantial portions of the Software.                                    //
//                                                                                     //
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR         //
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,           //
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE        //
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER             //
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,      //
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE      //
//  SOFTWARE.                                                                          //
/////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSAV.HTLib2
{
    public partial class BTree
    {
        public class Node
        {
        }
        public class Node<T> : Node
        {
            public T       value ;
            public Node<T> parent;
            public Node<T> left  ;
            public Node<T> right ;

            public static Node<T> New(T value, Node<T> parent, Node<T> left, Node<T> right)
            {
                return new Node<T>
                {
                    value  = value ,
                    parent = parent,
                    left   = left  ,
                    right  = right ,
                };
            }
            public Node<T> MaxNode()
            {
                HDebug.Assert(parent != null);
                if(right == null)
                    return this;
                return right.MaxNode();
            }
            public Node<T> MinNode()
            {
                Node<T> node = this;
                while(node.left != null)
                    node = node.left;
                return node;
                //HDebug.Assert(parent != null);
                //if(left == null)
                //    return this;
                //return left.MinNode();
            }
            public Node<T> Successor()
            {
                return Successor(this);

                Node<T> Successor(Node<T> node)
                {
                    if(node.right != null)
                        return node.right.MinNode();
                    while(true)
                    {
                        if(node == null)
                            return null;
                        if(node.parent == null)
                            return null;
                        if(node.parent.left == node)
                            return node.parent;
                        node = node.parent;
                    }
                }
            }

            public bool IsRoot()
            {
                return (parent == null);
            }
            public bool IsLeaf()
            {
                return (left == null && right == null);
            }
            public int Count()
            {
                int lc = (left  == null) ? 0 : left .Count();
                int rc = (right == null) ? 0 : right.Count();
                return (1 + lc + rc);
            }
            public int Height()
            {
                int lh = (left  == null) ? 0 : left .Height();
                int rh = (right == null) ? 0 : right.Height();
                return Math.Max(lh, rh) + 1;
            }

            public bool IsBalanced()
            {
                return IsBalanced(this).balanced;

                (bool balanced, int height) IsBalanced(Node<T> n)
                {
                    if(n == null)
                        return (true, -1);
                    var lb = IsBalanced(n.left );
                    var rb = IsBalanced(n.right);
                    int height = Math.Max(lb.height, rb.height) + 1;
                    int diff   = left.Height() - right.Height();
                    bool balanced = lb.balanced && rb.balanced && (Math.Abs(diff) <= 1);
                    return (balanced, height);
                }
            }

            public ref Node<T> GetThisRef(ref Node<T> root)
            {
                if(this == root        ) return ref root;
                if(this == parent.left ) return ref parent.left ;
                if(this == parent.right) return ref parent.right;
                HDebug.Assert(false);
                throw new Exception();
            }

            public bool ValidateConnection()
            {
                if(left != null)
                {
                    if(left.ValidateConnection() == false) return false;
                    if(this != left.parent) return false;
                }
                if(right != null)
                {
                    if(right.ValidateConnection() == false) return false;
                    if(this != right.parent) return false;
                }
                return true;
            }
            ///////////////////////////////////////////////////////////////////////
            /// ToString()
            ///////////////////////////////////////////////////////////////////////
            public override string ToString()
            {
                return ToStringSimple();
            }
            public string ToStringDetail()
            {
                StringBuilder sb = new StringBuilder();
                ToString(sb, this);
                sb.Insert(0, "val:" + value + ", cnt:" + Count().ToString() + ", ");
                return sb.ToString();
            }
            public string ToStringSimple()
            {
                StringBuilder sb = new StringBuilder();
                ToString(sb, this);
                return sb.ToString();
            }
            public static void ToString(StringBuilder sb, Node<T> node)
            {
                if (node == null)
                {
                    sb.Append("()");
                }
                else
                {
                    ToStringRec(sb, node);
                    if (node.IsLeaf())
                    {
                        sb.Insert(0, "(");
                        sb.Append(")");
                    }
                }
            }
            static void ToStringRec(StringBuilder sb, Node<T> node)
            {
                if (node == null)
                {
                    sb.Append("_");
                    return;
                }
                else if (node.IsLeaf())
                {
                    sb.Append(node.value.ToString());
                    return;
                }
                else
                {
                    sb.Append("(");
                    ToStringRec(sb, node.left);
                    sb.Append(",");
                    sb.Append(node.value.ToString());
                    sb.Append(",");
                    ToStringRec(sb, node.right);
                    sb.Append(")");
                    return;
                }
            }
            public string ToStringMathematica()
            {
                StringBuilder sb = new StringBuilder();
                ToStringMathematica(sb, this);
                return sb.ToString();
            }
            public void ToStringMathematica(StringBuilder sb, Node<T> root)
            {
                if (root == null)
                {
                    sb.Append("TreePlot[{},VertexLabeling->True]");
                    return;
                }

                List<string> edges = new List<string>();
                if(root.IsLeaf())
                {
                    string edge
                        = "\"root\"->"
                        + root.value.ToString();
                    edges.Add(edge);
                }
                else
                {
                    ToStringMathematicaRec(edges, root);
                }

                sb.Append("TreePlot[{");
                for(int i=0; i<edges.Count; i++)
                {
                    if(i != 0)
                        sb.Append(",");
                    sb.Append(edges[i]);
                }
                sb.Append("},VertexLabeling->True]");
            }
            static void ToStringMathematicaRec(List<string> edges, Node<T> node)
            {
                if (node == null)
                    return;
                if(node.parent != null)
                {
                    string edge
                        = node.parent.value.ToString()
                        + "->"
                        + node.value.ToString();
                    edges.Add(edge);
                }
                ToStringMathematicaRec(edges, node.left );
                ToStringMathematicaRec(edges, node.right);
            }
        }
    }
}
