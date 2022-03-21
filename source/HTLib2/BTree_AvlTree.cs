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
    //using T = System.Int32;
    public static partial class BTree
    {
#pragma warning disable 414
        static int _debug = 0;
#pragma warning restore 414

        public static AvlTree<T> NewAvlTree<T>(Comparison<T> comp=null)
        {
            return AvlTree<T>.NewAvlTree(comp);
        }
        ///////////////////////////////////////////////////////////////////////
        /// AVL Tree
        ///////////////////////////////////////////////////////////////////////
        public class AvlTree<T>
        {
            public struct RetT
            {
                public T value;
                public static RetT New(T val) { return new RetT { value = val }; }
            }

            internal struct AvlNodeInfo
            {
                public T   value;
                public int left_height;
                public int right_height;
                public int height { get { return Math.Max(left_height, right_height) + 1; } }
                public int bf     { get { return right_height - left_height; } }
                public override string ToString() { return value.ToString(); }
            }
            Node<AvlNodeInfo> root;
            Comparison<T> _comp;
            public Comparison<T> comp { get { return _comp; } }
            int avlcomp(AvlNodeInfo x, AvlNodeInfo y) { return _comp(x.value, y.value); }

            public void ChangeComp(Comparison<T> comp)
            {
                Comparison<T> comp0 = _comp;
                _comp = comp;
                HDebug.Assert(Validate());
            }


            public static AvlTree<T> NewAvlTree(Comparison<T> comp)
            {
                if(comp == null)
                {
                    var compr = Comparer<T>.Default;
                    comp = delegate(T x, T y)
                    {
                        return compr.Compare(x,y);
                    };
                }

                return new AvlTree<T>
                {
                    root = null,
                    _comp = comp,
                };
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
            
            ///////////////////////////////////////////////////////////////////////
            /// AVL Search
            ///////////////////////////////////////////////////////////////////////
            public bool IsEmpty()
            {
                return (root == null);
            }
            public bool Contains(T query)
            {
                return (Search(query) != null);
            }
            public RetT? Search(T query)
            {
                Node<AvlNodeInfo> node = AvlSearch(query);
                if(node == null)
                    return null;
                return RetT.New(node.value.value);
            }
            public (RetT? val, RetT? parent_val) SearchWithParent(T query)
            {
                (Node<AvlNodeInfo> node, Node<AvlNodeInfo> node_parent) = AvlSearchWithParent(query);

                RetT? val        = null; if(node        != null) val        = RetT.New(node       .value.value);
                RetT? parent_val = null; if(node_parent != null) parent_val = RetT.New(node_parent.value.value);

                return (val, parent_val);
            }
            //public (RetT? val,(RetT? left, RetT? right)) SearchRange(T query)
            //{
            //    (Node<AvlNodeInfo> node, Node<AvlNodeInfo> node_parent) = AvlSearch(query);
            //    if (node == null)
            //        return null;
            //    return RetT.New(node.value.value);
            //}
            internal Node<AvlNodeInfo> AvlSearch(T query)
            {
                Node<AvlNodeInfo> node = BstSearch<AvlNodeInfo>(root, new AvlNodeInfo{value = query}, avlcomp);
                return node;
            }
            internal (Node<AvlNodeInfo> node, Node<AvlNodeInfo> node_parent) AvlSearchWithParent(T query)
            {
                (Node<AvlNodeInfo> node, Node<AvlNodeInfo> node_parent) = BstSearchWithParent<AvlNodeInfo>(root, null, new AvlNodeInfo { value = query }, avlcomp);
                return (node, node_parent);
            }

            ///////////////////////////////////////////////////////////////////////
            /// AVL Validate
            ///////////////////////////////////////////////////////////////////////
            public bool Validate()
            {
                return Validate(_comp);
            }
            public bool Validate(Comparison<T> comp_validate)
            {
                if(root == null)
                    return true;

                int avlcomp_validate(AvlNodeInfo x, AvlNodeInfo y) { return comp_validate(x.value, y.value); }

                if(ValidateBalance() == false) return false;
                if(BTree.BstValidateConnection(root) == false) return false;
                if(BTree.BstValidateOrder(root, avlcomp_validate) == false) return false;
                return true;
            }
            bool ValidateBalance()
            {
                if(ValidateBalance(root) == null)
                    return false;
                return true;

                int? ValidateBalance(Node<AvlNodeInfo> node)
                {
                    if(node == null)
                        return -1;
                    int? lh = ValidateBalance(node.left);
                    int? rh = ValidateBalance(node.right);
                    if(lh == null) return null;
                    if(rh == null) return null;
                    if(lh.Value != node.value.left_height ) { HDebug.Assert(false); return null; }
                    if(rh.Value != node.value.right_height) { HDebug.Assert(false); return null; }
                    if(Math.Abs(rh.Value - lh.Value) >= 2 ) { HDebug.Assert(false); return null; }
                    int h = Math.Max(lh.Value, rh.Value) + 1;
                    return h;
                }
            }

            ///////////////////////////////////////////////////////////////////////
            /// AVL Insert
            /// 
            /// 1. Insert value into BST
            /// 2. Rebalance
            /// 3. Return the inserted node
            ///////////////////////////////////////////////////////////////////////
            static bool Insert_selftest = HDebug.IsDebuggerAttached;
            public static void InsertSelftest()
            {
                if(Insert_selftest == false)
                    return;

                Insert_selftest = false;
                {
                    var avltree = BTree.NewAvlTree<int>();
                    HDebug.Assert(avltree.Validate()); 
                    avltree.Insert( 1); HDebug.Assert(avltree.Validate()); HDebug.Assert(avltree.ToString() == "(1)");
                    avltree.Insert( 2); HDebug.Assert(avltree.Validate()); HDebug.Assert(avltree.ToString() == "(_,1,2)");
                    avltree.Insert( 3); HDebug.Assert(avltree.Validate()); HDebug.Assert(avltree.ToString() == "(1,2,3)");
                }
                {
                    var avltree = BTree.NewAvlTree<int>();
                    HDebug.Assert(avltree.Validate()); 
                    avltree.Insert( 4); HDebug.Assert(avltree.Validate()); HDebug.Assert(avltree.ToString() == "(4)");
                    avltree.Insert( 3); HDebug.Assert(avltree.Validate()); HDebug.Assert(avltree.ToString() == "(3,4,_)");
                    avltree.Insert( 9); HDebug.Assert(avltree.Validate()); HDebug.Assert(avltree.ToString() == "(3,4,9)");
                    avltree.Insert( 2); HDebug.Assert(avltree.Validate()); HDebug.Assert(avltree.ToString() == "((2,3,_),4,9)");
                    avltree.Insert(11); HDebug.Assert(avltree.Validate()); HDebug.Assert(avltree.ToString() == "((2,3,_),4,(_,9,11))");
                    avltree.Insert( 0); HDebug.Assert(avltree.Validate()); HDebug.Assert(avltree.ToString() == "((0,2,3),4,(_,9,11))");
                    avltree.Insert(15); HDebug.Assert(avltree.Validate()); HDebug.Assert(avltree.ToString() == "((0,2,3),4,(9,11,15))");
                    avltree.Insert(17); HDebug.Assert(avltree.Validate()); HDebug.Assert(avltree.ToString() == "((0,2,3),4,(9,11,(_,15,17)))");
                    avltree.Insert(14); HDebug.Assert(avltree.Validate()); HDebug.Assert(avltree.ToString() == "((0,2,3),4,(9,11,(14,15,17)))");
                    avltree.Insert(12); HDebug.Assert(avltree.Validate()); HDebug.Assert(avltree.ToString() == "((0,2,3),4,((9,11,12),14,(_,15,17)))");
                }
                {
                    var avltree = BTree.NewAvlTree<int>();
                    HDebug.Assert(avltree.Validate()); 
                    avltree.Insert( 4); HDebug.Assert(avltree.Validate()); HDebug.Assert(avltree.ToString() == "(4)");
                    avltree.Insert( 3); HDebug.Assert(avltree.Validate()); HDebug.Assert(avltree.ToString() == "(3,4,_)");
                    avltree.Insert( 9); HDebug.Assert(avltree.Validate()); HDebug.Assert(avltree.ToString() == "(3,4,9)");
                    avltree.Insert( 2); HDebug.Assert(avltree.Validate()); HDebug.Assert(avltree.ToString() == "((2,3,_),4,9)");
                    avltree.Insert(11); HDebug.Assert(avltree.Validate()); HDebug.Assert(avltree.ToString() == "((2,3,_),4,(_,9,11))");
                    avltree.Insert(-1); HDebug.Assert(avltree.Validate()); HDebug.Assert(avltree.ToString() == "((-1,2,3),4,(_,9,11))");
                    avltree.Insert(15); HDebug.Assert(avltree.Validate()); HDebug.Assert(avltree.ToString() == "((-1,2,3),4,(9,11,15))");
                    avltree.Insert( 0); HDebug.Assert(avltree.Validate()); HDebug.Assert(avltree.ToString() == "(((_,-1,0),2,3),4,(9,11,15))");
                    avltree.Insert(-2); HDebug.Assert(avltree.Validate()); HDebug.Assert(avltree.ToString() == "(((-2,-1,0),2,3),4,(9,11,15))");
                    avltree.Insert( 1); HDebug.Assert(avltree.Validate()); HDebug.Assert(avltree.ToString() == "(((-2,-1,_),0,(1,2,3)),4,(9,11,15))");
                }
            }
            public bool Insert(T value)
            {
                Node<AvlNodeInfo> node = AvlInsert(value);
                if(node == null)
                    return false;
                return true;
            }
            public bool[] InsertRange(params T[] values)
            {
                return InsertRange(values as IEnumerable<T>);
            }
            public bool[] InsertRange(IEnumerable<T> values)
            {
                List<Node<AvlNodeInfo>> inserteds = AvlInsertRange(values);
                bool[] results = new bool[inserteds.Count];
                for(int i=0; i<results.Length; i++)
                    results[i] = (inserteds[i] != null);
                return results;
            }
            List<Node<AvlNodeInfo>> AvlInsertRange(IEnumerable<T> values)
            {
                List<Node<AvlNodeInfo>> inserteds = new List<Node<AvlNodeInfo>>();
                foreach(var value in values)
                {
                    Node<AvlNodeInfo> node = AvlInsert(value);
                    inserteds.Add(node);
                }
                return inserteds;
            }
            internal Node<AvlNodeInfo> AvlInsert(T value)
            {
                HDebug.Assert(root == null || root.IsRoot());
                AvlNodeInfo avlvalue = new AvlNodeInfo
                {
                    value  = value,
                    left_height  = -1, // height of null node is -1
                    right_height = -1, // height of null node is -1
                };

                if(root == null)
                {
                    Node<AvlNodeInfo> node = BstInsert<AvlNodeInfo>(null, ref root, avlvalue, avlcomp);
                    HDebug.Assert(root == node);
                    HDebug.Assert(root.left  == null);
                    HDebug.Assert(root.right == null);
                    HDebug.Assert(root.value.height == 0);
                    return node;
                }
                else
                {
                    Node<AvlNodeInfo> node = BstInsert<AvlNodeInfo>(null, ref root, avlvalue, avlcomp);
                    HDebug.Assert(node.value.height == 0);
                    UpdateBalance(node, ref root);
                    return node;
                }
            }
            //[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)] 
            //static void UpdateParentHeight(Node<AvlNodeInfo> node)
            //{
            //    Node<AvlNodeInfo> parent = node.parent;
            //    HDebug.Assert(parent != null);
            //    HDebug.Assert((node == parent.left ) || (node == parent.right));
            //
            //    if(node == parent.left ) parent.value.left_height  = node.value.height;
            //    if(node == parent.right) parent.value.right_height = node.value.height;
            //}
            ///////////////////////////////////////////////////////////////////////
            /// AVL Update height
            /// AVL Update Balance (rebalance)
            ///////////////////////////////////////////////////////////////////////
            static void UpdateHeightRec(Node<AvlNodeInfo> node)
            {
                if(node == null) return;
                if(node.left  != null) UpdateHeightRec(node.left );
                if(node.right != null) UpdateHeightRec(node.right);
                UpdateHeight(node);
            }
            static void UpdateHeight(Node<AvlNodeInfo> node)
            {
                node.value.left_height  = (node.left  == null) ? -1 : node.left .value.height;
                node.value.right_height = (node.right == null) ? -1 : node.right.value.height;
            }
            static void UpdateBalance(Node<AvlNodeInfo> node, ref Node<AvlNodeInfo> root)
            {
                Node<AvlNodeInfo> parent = node.parent;
                while(parent != null)
                {
                    (node, parent) = UpdateBalance(node, parent, ref root);
                    node   = parent;
                    parent = node.parent;
                }
            }
            // update balance of (node, parent), and return (redetermined node, redetermined parent)
            static (Node<AvlNodeInfo> nnode, Node<AvlNodeInfo> nparent) UpdateBalance(Node<AvlNodeInfo> node, Node<AvlNodeInfo> parent, ref Node<AvlNodeInfo> root)
            {
                HDebug.Assert(node   != null);
                HDebug.Assert(parent != null);
                HDebug.Assert(parent == node.parent);

                UpdateHeight(parent);

                int   node_bf =   node.value.bf;
                int parent_bf = parent.value.bf;
                HDebug.Assert(Math.Abs(parent_bf) <= 2);

                {
                    if(parent_bf == 2)
                    {
                        node = parent.right;
                        node_bf = node.value.bf;
                    }
                    else if(parent_bf == -2)
                    {
                        node = parent.left;
                        node_bf = node.value.bf;
                    }
                }

                Node<AvlNodeInfo> nnode   = null;
                Node<AvlNodeInfo> nparent = null;

                if((Math.Abs(node_bf) <= 1) && (Math.Abs(parent_bf) <= 1))
                {
                    nnode   = node  ;
                    nparent = parent;
                }
                else if( ((node_bf == -1) && (parent_bf == -2)) || ((node_bf == 0) && (parent_bf == -2)) )
                {
                    //HDebug.Assert(parent_bf == -2);
                    ref Node<AvlNodeInfo> parent_ref = ref parent.GetThisRef(ref root);
                    BTree.RotateRight<AvlNodeInfo>(ref parent_ref);
                    UpdateHeight(parent);
                    UpdateHeight(node  );
                    HDebug.Assert(parent.parent == node);

                    nnode   = parent;
                    nparent = node;
                }
                else if((node_bf == 1) && (parent_bf == -2))
                {
                    //HDebug.Assert(parent_bf == -2);
                    ref Node<AvlNodeInfo> node_ref = ref node.GetThisRef(ref root);
                    BTree.RotateLeft<AvlNodeInfo>(ref node_ref);
                    ref Node<AvlNodeInfo> parent_ref = ref parent.GetThisRef(ref root);
                    BTree.RotateRight<AvlNodeInfo>(ref parent_ref);
                    HDebug.Assert(node.parent == parent.parent);
                    UpdateHeight(parent);
                    UpdateHeight(node  );
                    UpdateHeight(node.parent);

                    nnode   = node;
                    nparent = node.parent;
                }
                else if( ((node_bf == 1) && (parent_bf == 2)) || ((node_bf == 0) && (parent_bf == 2)) )
                {
                    //HDebug.Assert(parent_bf == 2);
                    ref Node<AvlNodeInfo> parent_ref = ref parent.GetThisRef(ref root);
                    BTree.RotateLeft<AvlNodeInfo>(ref parent_ref);
                    UpdateHeight(parent);
                    UpdateHeight(node  );
                    HDebug.Assert(parent.parent == node);

                    nnode   = parent;
                    nparent = node;
                }
                else if((node_bf == -1) && (parent_bf == 2))
                {
                    //HDebug.Assert(parent_bf == 2);
                    ref Node<AvlNodeInfo> node_ref = ref node.GetThisRef(ref root);
                    BTree.RotateRight<AvlNodeInfo>(ref node_ref);
                    ref Node<AvlNodeInfo> parent_ref = ref parent.GetThisRef(ref root);
                    BTree.RotateLeft<AvlNodeInfo>(ref parent_ref);
                    HDebug.Assert(node.parent == parent.parent);
                    UpdateHeight(parent);
                    UpdateHeight(node  );
                    UpdateHeight(node.parent);

                    nnode   = node;
                    nparent = node.parent;
                }
                else
                {
                    throw new NotImplementedException();
                }

                HDebug.Assert(nnode   != null);
                HDebug.Assert(nparent != null);
                HDebug.Assert(nnode.parent == nparent);
                return (nnode, nparent);
            }
            ///////////////////////////////////////////////////////////////////////
            /// AVL Delete
            /// 
            /// 1. Delete node from BST
            /// 2. Rebalance
            /// 3. Return the deleted value
            ///////////////////////////////////////////////////////////////////////
            static bool Delete_selftest = HDebug.IsDebuggerAttached;
            public static void DeleteSelftest()
            {
                if(Delete_selftest == false)
                    return;

                Delete_selftest = false;
                {
                    var avltree = BTree.NewAvlTree<int>();
                    HDebug.Assert(avltree.Validate()); 
                    avltree.InsertRange( 10, 5, 17, 2, 8, 20, 3, 7 );
                                        HDebug.Assert(avltree.Validate()); HDebug.Assert(avltree.ToString() == "(((_,2,3),5,(7,8,_)),10,(_,17,20))");
                    avltree.Delete(10); HDebug.Assert(avltree.Validate()); HDebug.Assert(avltree.ToString() == "(((_,2,3),5,7),8,(_,17,20))");
                    avltree.Delete( 8); HDebug.Assert(avltree.Validate()); HDebug.Assert(avltree.ToString() == "((2,3,5),7,(_,17,20))");
                    avltree.Delete( 2); HDebug.Assert(avltree.Validate()); HDebug.Assert(avltree.ToString() == "((_,3,5),7,(_,17,20))");
                    avltree.Delete( 7); HDebug.Assert(avltree.Validate()); HDebug.Assert(avltree.ToString() == "(3,5,(_,17,20))");
                    avltree.Delete( 3); HDebug.Assert(avltree.Validate()); HDebug.Assert(avltree.ToString() == "(5,17,20)");
                    avltree.Delete(17); HDebug.Assert(avltree.Validate()); HDebug.Assert(avltree.ToString() == "(_,5,20)");
                    avltree.Delete( 5); HDebug.Assert(avltree.Validate()); HDebug.Assert(avltree.ToString() == "(20)");
                    avltree.Delete(20); HDebug.Assert(avltree.Validate()); HDebug.Assert(avltree.ToString() == "()");
                }
            }
            public RetT? Delete(T query)
            {
                AvlNodeInfo avlquery = new AvlNodeInfo
                {
                    value  = query,
                };
                var del = BstDelete<AvlNodeInfo>(ref root, avlquery, avlcomp);
                if(del == null)
                    return null;
                
                AvlNodeInfo       value          = del.Value.value;
                Node<AvlNodeInfo> deleted_parent = del.Value.deleted_parent;

                if(root != null)
                {
                    if(deleted_parent != null)
                    {
                        if(deleted_parent.left == null && deleted_parent.right == null)
                        {
                            // need to update hight of parent, then update balance
                            UpdateHeight(deleted_parent);
                            UpdateBalance(deleted_parent, ref root);
                        }
                        else if(deleted_parent.left != null && deleted_parent.right == null)
                        {
                            // do not need to update hight of sibling
                            UpdateBalance(deleted_parent.left, ref root);
                        }
                        else if(deleted_parent.left == null && deleted_parent.right != null)
                        {
                            // do not need to update hight of sibling
                            UpdateBalance(deleted_parent.right, ref root);
                        }
                        else
                        {
                            HDebug.Assert(deleted_parent.left != null && deleted_parent.right != null);
                            // case of deleting predecessor, and the predecessor has one left child, and the predecessor put parent.right
                            // Both parent.left and parent.right are balanced, and their height is correct
                            // However, maybe parent.left could be highter than parent.right (just deleted one node)
                            // Therefore, balance starts from parent.left
                            UpdateBalance(deleted_parent.left, ref root);
                        }
                    }
                    else
                    {
                        // This is the case that tree has only root, or only two elements
                        HDebug.Assert(root.Count() <= 2);
                        UpdateHeightRec(root);
                    }
                }

                return RetT.New(value.value);
            }
        }
    }
}
