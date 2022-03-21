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
    /// <summary>
    /// AVL tree whose nodes are linked as a linked-list in increasing order
    /// </summary>
    public partial class LinkedAvlTree<T>
    {
#pragma warning disable 414
        static int _debug = 0;
#pragma warning restore 414

        public class Node
        {
            public T    value;
            public Node prev { get { return _prev; } }
            public Node next { get { return _next; } }

            internal Node _prev;
            internal Node _next;
            internal Node(T value, Node prev, Node next)
            {
                this.value = value;
                this._prev = prev ;
                this._next = next ;
            }

            public override string ToString()
            {
                return value.ToString();
            }
        }

        Node head;
        Node tail;
        BTree.AvlTree<Node> avl;
        Comparison<T> comp;
        int nodecomp(Node x, Node y) { return comp(x.value, y.value); }

        public override string ToString()
        {
            return avl.ToString();
        }
        public string ToStringMathematica()
        {
            return avl.ToStringMathematica();
        }

        public LinkedAvlTree(Comparison<T> comp)
        {
            this.head = null;
            this.tail = null;
            this.comp = comp;
            this.avl  = BTree.AvlTree<Node>.NewAvlTree(nodecomp);
        }
        public static LinkedAvlTree<T> New(Comparison<T> comp)
        {
            return new LinkedAvlTree<T>(comp);
        }
        public Node GetHead()
        {
            return head;
        }
        public Node GetTail()
        {
            return tail;
        }
        public bool IsEmpty()
        {
            if(avl.IsEmpty() == true)
            {
                HDebug.Assert(head == null);
                HDebug.Assert(tail == null);
                return true;
            }
            else
            {
                HDebug.Assert(head != null);
                HDebug.Assert(tail != null);
                return false;
            }
        }
        public bool Contains(T query)
        {
            return (Search(query) != null);
        }
        public Node Search(T query)
        {
            var nodequery = new Node(query, null, null);

            var value = avl.Search(nodequery);

            if(value == null)
                return null;
            return value.Value.value;
        }
        public (Node value, Node parent_value) SearchWithParent(T query)
        {
            var nodequery = new Node(query, null, null);

            var (val, parent_val) = avl.SearchWithParent(nodequery);

            Node value        = null; if(val        != null) value        = val       .Value.value;
            Node parent_value = null; if(parent_val != null) parent_value = parent_val.Value.value;

            return (value, parent_value);
        }
        public (Node value, (Node left_value, Node right_value)) SearchRange(T query, bool doassert=true)
        {
            var nodequery = new Node(query, null, null); ;

            var (val, parent_val) = avl.SearchWithParent(nodequery);

            Node value, left_value, right_value;
            if(val == null)
            {
                HDebug.Assert(parent_val != null);
                Node parent_value = parent_val.Value.value;
                int cmp = comp(query, parent_value.value);
                HDebug.Assert(cmp != 0);
                if(cmp < 0)
                {
                    //HDebug.Assert(false);
                    //  parent->prev < query:null < parent)
                    //return (null, (parent_value.prev, parent_value));
                    value = null;
                    left_value = parent_value.prev;
                    right_value = parent_value;
                }
                else
                {
                    //HDebug.Assert(false);
                    //  parent < query:null < parent->next
                    //return (null, (parent_value, parent_value.next));
                    value = null;
                    left_value = parent_value;
                    right_value = parent_value.next;
                }
            }
            else
            {
                value       = val       .Value.value;
                left_value  = value.prev;
                right_value = value.next;
            }

            if(doassert && HDebug.IsDebuggerAttached)
            {
                if(left_value != null && left_value.value != null &&       value != null) HDebug.Assert(comp(left_value.value,       value.value) <= 0);
                if(     value != null &&      value.value != null && right_value != null) HDebug.Assert(comp(     value.value, right_value.value) <= 0);
                if(left_value != null && left_value.value != null && right_value != null) HDebug.Assert(comp(left_value.value, right_value.value) <= 0);
            }
            return (value, (left_value, right_value));
        }
        public Node Insert(T value)
        {
            Node node = new Node(value, null, null);

            if(avl.IsEmpty() == true)
            {
                var avlnode = avl.AvlInsert(node);
                HDebug.Assert(avlnode.value.value == node);

                node._prev = null;
                node._next = null;
                head = tail = node;
            }
            else
            {
                var avlnode = avl.AvlInsert(node);
                HDebug.Assert(avlnode.value.value == node);

                var avlnode_successor = avlnode.Successor();
                if (avlnode_successor == null)
                {
                    // added to tail
                    HDebug.Assert(avl.AvlSearch(tail).Successor().value.value == node);
                    HDebug.Assert(nodecomp(tail, node) < 0);
                    tail._next = node;
                    node._prev = tail;
                    tail = node;
                }
                else if (avlnode_successor.value.value == head)
                {
                    // added to head
                    HDebug.Assert(nodecomp(node, head) < 0);
                    head._prev = node;
                    node._next = head;
                    head = node;
                }
                else
                {
                    Node node_next = avlnode_successor.value.value;
                    Node node_prev = node_next.prev;
                    HDebug.Assert(node_prev.next == node_next);
                    HDebug.Assert(node_next.prev == node_prev);
                    HDebug.Assert(nodecomp(node_prev, node_next) < 0);
                    HDebug.Assert(nodecomp(node_prev, node     ) < 0);
                    HDebug.Assert(nodecomp(node     , node_next) < 0);
                    node._next = node_next;
                    node._prev = node_prev;
                    node_prev._next = node;
                    node_next._prev = node;
                }
            }
            return node;
        }
        public List<Node> InsertRange(params T[] values)
        {
            return InsertRange(values as IEnumerable<T>);
        }
        public List<Node> InsertRange(IEnumerable<T> values)
        {
            List<Node> nodes = new List<Node>();
            foreach(var value in values)
            {
                Node node = Insert(value);
                nodes.Add(node);
            }
            return nodes;
        }
        public Node Delete(T query)
        {
            var nodequery = new Node(query, null, null);

            var del = avl.Delete(nodequery);
            if(del == null)
                return null;

            Node node = del.Value.value;

            if(avl.IsEmpty())
            {
                HDebug.Assert(node == head);
                HDebug.Assert(node == tail);
                head = tail = null;
            }
            else if(node == head)
            {
                head = node.next;
                head._prev = null;
                node._next = null;
            }
            else if(node == tail)
            {
                tail = node.prev;
                tail._next = null;
                node._prev = null;
            }
            else
            {
                Node node_prev = node.prev;
                Node node_next = node.next;
                node_prev._next = node_next;
                node_next._prev = node_prev;
                node._next = null;
                node._prev = null;
            }

            if(HDebug.IsDebuggerAttached)
            {
                if(head != null) HDebug.Assert(head.prev == null);
                if(tail != null) HDebug.Assert(tail.next == null);
            }
            HDebug.Assert(node.prev == null);
            HDebug.Assert(node.next == null);
            return node;
        }
        public bool Validate()
        {
            return Validate(comp);
        }
        public bool Validate(Comparison<T> comp_validate)
        {
            int nodecomp_validate(Node x, Node y) { return comp_validate(x.value, y.value); }

            // check AVL validate
            if(avl.Validate(nodecomp_validate) == false) return false;

            // check linked-list validate
            Node n = head;
            while(n != null)
            {
                Node n_next = n.next;
                if(n_next != null && (nodecomp_validate(n, n_next) <= 0) == false)
                    return false;
                n = n_next;
            }

            return true;
        }
    }
}
