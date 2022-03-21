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
        ///////////////////////////////////////////////////////////////////////
        /// Binar Search Tree
        ///////////////////////////////////////////////////////////////////////
        public class BST<T>
        {
            public struct RetT
            {
                public T value;
                public static RetT New(T val) { return new RetT { value = val }; }
            }

            internal Node<T> root;
            Comparison<T> _comp;
            public Comparison<T> comp{ get { return _comp; } }
            public void ChangeComp(Comparison<T> comp)
            {
                Comparison<T> comp0 = _comp;
                _comp = comp;
                HDebug.Assert(Validate());
            }

            public static BST<T> NewBST(Comparison<T> comp)
            {
                return new BST<T>
                {
                    root = null,
                    _comp = comp,
                };
            }

            public bool  IsEmpty ()        { return (root == null); }
            public bool  Contains(T query) { return (Search(query) != null); }
            public RetT? Search  (T query) { Node<T> node = BstSearchWithParent(query).ret; if(node == null) return null; return RetT.New(node.value); }
            public RetT? Insert  (T value) { Node<T> node = BstInsert(value);               if(node == null) return null; return RetT.New(node.value); }
            public RetT? Delete  (T query) { var     del  = BstDelete(query);               if(del  == null) return null; return RetT.New(del.Value.value); }
          //public void  MakeACBT()        { DSW(ref root); }
            public bool  Validate()
            {
                if(BstValidateConnection(root) == false) return false;
                if(BstValidateOrder(root, _comp) == false) return false;
                return true;
            }

            internal (Node<T> ret, Node<T> ret_parent) BstSearchWithParent(T query)
            {
                return BTree.BstSearchWithParent(root, null, query, _comp);
            }
            internal Node<T> BstInsert(T value)
            {
                return BTree.BstInsert(ref root, value, _comp);
            }
            internal (T value, Node<T> deleted_parent)? BstDelete(T query)
            {
                (T value, Node<T> deleted_parent)? del = BTree.BstDelete(ref root, query, _comp);
                return del;
            }
            public override string ToString()
            {
                if(root == null)
                    return "()";
                string str = root.ToStringSimple();
                return str;
            }
            public string ToStringMathematica()
            {
                if(root == null)
                    return "TreePlot[{},VertexLabeling->True]";
                string str = root.ToStringMathematica();
                return str;
            }
        }
        public static BST<T> NewBST<T>(Comparison<T> comp)
        {
            return BST<T>.NewBST(comp);
        }
        ///////////////////////////////////////////////////////////////////////
        /// Validate connections
        ///////////////////////////////////////////////////////////////////////
        static bool BstValidateConnection<T>(Node<T> root)
        {
            if(root.parent != null)
                return false;
            if(root.ValidateConnection() == false)
                return false;
            return true;
        }
        ///////////////////////////////////////////////////////////////////////
        /// Validate order
        ///////////////////////////////////////////////////////////////////////
        static bool BstValidateOrder<T>(Node<T> root, Comparison<T> compare)
        {
            if(root == null)
                return true;
            int    count = root.Count();
            Node<T> node = root.MinNode();
            Node<T> next = node.Successor();
            if(next == null)
                return true;
            int num_compare = 0;
            while(next != null)
            {
                if(compare(node.value, next.value) > 0)
                    return false;
                num_compare ++;
                node = next;
                next = next.Successor();
            }
            if(num_compare != count-1)
                return false;
            return true;
        }
        ///////////////////////////////////////////////////////////////////////
        /// BST Search
        ///////////////////////////////////////////////////////////////////////
        static bool BstSearch_selftest = true;
        //public T BstSearch(T query)
        //{
        //    Node node = BstSearch(root, query);
        //    if(node == null)
        //        return default(T);
        //    return node.value;
        //}
        static Node<T> BstSearch<T>(Node<T> node, T query, Comparison<T> compare)
        {
            (Node<T> ret, Node<T> ret_parent) = BstSearchWithParent(node, null, query, compare);
            return ret;
        }
        static (Node<T> ret, Node<T> ret_parent) BstSearchWithParent<T>(Node<T> node, Node<T> node_parent, T query, Comparison<T> compare)
        {
            if(BstSearch_selftest)
            {
                BstSearch_selftest = false;
                Comparison<int> _compare = delegate(int a, int b) { return a - b; };
                Node<int> _root = null;
                BstInsertRange(ref _root, new int[] { 10, 5, 20, 2, 7, 4, 6, 30, 3, 25 }, _compare);
                HDebug.Assert(_root.ToString() == "(((_,2,(3,4,_)),5,(6,7,_)),10,(_,20,(25,30,_)))");
                HDebug.Assert((BstSearchWithParent(_root, null, 10, _compare).ret != null) ==  true);
                HDebug.Assert((BstSearchWithParent(_root, null, 25, _compare).ret != null) ==  true);
                HDebug.Assert((BstSearchWithParent(_root, null,  4, _compare).ret != null) ==  true);
                HDebug.Assert((BstSearchWithParent(_root, null,  7, _compare).ret != null) ==  true);
                HDebug.Assert((BstSearchWithParent(_root, null,  0, _compare).ret != null) == false);
                HDebug.Assert((BstSearchWithParent(_root, null,  9, _compare).ret != null) == false);
                HDebug.Assert((BstSearchWithParent(_root, null, 15, _compare).ret != null) == false);
                HDebug.Assert((BstSearchWithParent(_root, null, 50, _compare).ret != null) == false);
            }

            if(node == null)
                return (null, node_parent);
            int query_node = compare(query, node.value);
            if     (query_node <  0) return BstSearchWithParent(node.left , node, query, compare);
            else if(query_node >  0) return BstSearchWithParent(node.right, node, query, compare);
            else                     return (node, node_parent);
        }
        ///////////////////////////////////////////////////////////////////////
        /// BST Insert
        /// 
        /// 1. Insert value into BST
        /// 2. Return the inserted node
        ///////////////////////////////////////////////////////////////////////
        static bool BstInsert_selftest = true;
        internal static Node<T> BstInsert<T>(ref Node<T> root, T value, Comparison<T> compare)
        {
            HDebug.Assert(root == null || root.IsRoot());
            return BstInsert(null, ref root, value, compare);
        }
        static Node<T> BstInsert<T>(Node<T> parent, ref Node<T> node, T value, Comparison<T> compare)
        {
            if(BstInsert_selftest)
            {
                BstInsert_selftest = false;
                Comparison<int> _compare = delegate(int a, int b) { return a - b; };
                Node<int> _root = null;
                BstInsert(ref _root, 10, _compare); HDebug.Assert(_root.ToStringSimple() == "(10)"                                           );
                BstInsert(ref _root,  5, _compare); HDebug.Assert(_root.ToStringSimple() == "(5,10,_)"                                       );
                BstInsert(ref _root, 20, _compare); HDebug.Assert(_root.ToStringSimple() == "(5,10,20)"                                      );
                BstInsert(ref _root,  2, _compare); HDebug.Assert(_root.ToStringSimple() == "((2,5,_),10,20)"                                );
                BstInsert(ref _root,  7, _compare); HDebug.Assert(_root.ToStringSimple() == "((2,5,7),10,20)"                                );
                BstInsert(ref _root,  4, _compare); HDebug.Assert(_root.ToStringSimple() == "(((_,2,4),5,7),10,20)"                          );
                BstInsert(ref _root,  6, _compare); HDebug.Assert(_root.ToStringSimple() == "(((_,2,4),5,(6,7,_)),10,20)"                    );
                BstInsert(ref _root, 30, _compare); HDebug.Assert(_root.ToStringSimple() == "(((_,2,4),5,(6,7,_)),10,(_,20,30))"             );
                BstInsert(ref _root,  3, _compare); HDebug.Assert(_root.ToStringSimple() == "(((_,2,(3,4,_)),5,(6,7,_)),10,(_,20,30))"       );
                BstInsert(ref _root, 25, _compare); HDebug.Assert(_root.ToStringSimple() == "(((_,2,(3,4,_)),5,(6,7,_)),10,(_,20,(25,30,_)))");
            }

            if(node == null)
            {
                node = Node<T>.New(value, parent, null, null);
                return node;
            }
            if(compare(node.value, value) < 0)
            {
                return BstInsert(node, ref node.right, value, compare);
            }
            else
            {
                return BstInsert(node, ref node.left, value, compare);
            }
        }
        static IEnumerable<Node<T>> BstInsertRange<T>(ref Node<T> root, IEnumerable<T> values, Comparison<T> compare)
        {
            List<Node<T>> nodes = new List<Node<T>>();
            foreach(T value in values)
                nodes.Add(BstInsert(ref root, value, compare));
            return nodes;
        }

        ///////////////////////////////////////////////////////////////////////
        /// BST Delete
        /// 
        /// 1. Delete node whose value is same to query
        /// 2. Return the value in the deleted node
        ///////////////////////////////////////////////////////////////////////
        static (T value, Node<T> deleted_parent)? BstDelete<T>(ref Node<T> root, T query, Comparison<T> compare)
        {
            return BstDeleteImpl(ref root, query, compare);
        }
        //static (T value, Node<T> deleted_parent)? BstDelete<T>(ref Node<T> root, Node<T> node)
        //{
        //    HDebug.ToDo();
        //    if(node == root)
        //        return BstDeleteImpl(ref root);
        //    Node<T> parent = node.parent;
        //    if(parent.left == node)
        //        return BstDeleteImpl(ref parent.left);
        //    else if(parent.right == node)
        //        return BstDeleteImpl(ref parent.right);
        //    else
        //        throw new HException();
        //}
        static (T value, Node<T> deleted_parent)? BstDeleteImpl<T>(ref Node<T> node, T query, Comparison<T> compare)
        {
            // find node to delete
            HDebug.Assert(node != null);
            int query_node = compare(query, node.value);
            if     (query_node <  0) return BstDeleteImpl(ref node.left , query, compare);
            else if(query_node >  0) return BstDeleteImpl(ref node.right, query, compare);
            else if(query_node == 0) return BstDeleteImpl(ref node);
            else                     return null;
        }
        static (T value, Node<T> deleted_parent) BstDeleteImpl<T>(ref Node<T> node)
        {
            if(node.left == null && node.right == null)
            {
                // delete a leaf
                T       value  = node.value;
                Node<T> parent = node.parent;
                node = null;
                return (value, parent);
            }
            else if(node.left != null && node.right == null)
            {
                // has left child
                T       value  = node.value;
                Node<T> parent = node.parent;
                node = node.left;
                node.parent = parent;
                return (value, parent);
            }
            else if(node.left == null && node.right != null)
            {
                // has right child
                T       value  = node.value;
                Node<T> parent = node.parent;
                node = node.right;
                node.parent = parent;
                return (value, parent);
            }
            else
            {
                // has both left and right children
                // 1. find predecessor reference
                ref Node<T> Pred(ref Node<T> lnode)
                {
                    if(lnode.right == null)
                        return ref lnode;
                    return ref Pred(ref lnode.right);
                };
                ref Node<T> pred = ref Pred(ref node.left);

                // 2. backup value to return
                T value = node.value;
                // 3. copy pred.value to node
                node.value = pred.value;
                // 4. node updated
                Node<T> pred_parent = pred.parent;
                // 4. delete pred; since (*pred).right == null, make pred = (*pred).left

                pred = pred.left;
                if(pred != null)
                    pred.parent = pred_parent;

                return (value, pred_parent);
            }
        }
    }
}
