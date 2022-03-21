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
        static bool RotateLeft_selftest = true;
        static void RotateLeft<T>(ref Node<T> grandparent_child)
        {
            if(RotateLeft_selftest)
            {
                RotateLeft_selftest = false;
                Node<string> _root       = Node<string>.New("grandparent_child", null, null, null);
                _root.right              = Node<string>.New("prnt" , null, null, null);
                _root.right.left         = Node<string>.New("T1"     , null, null, null);
                _root.right.right        = Node<string>.New("curr"   , null, null, null);
                _root.right.right.left   = Node<string>.New("T2"     , null, null, null);
                _root.right.right.right  = Node<string>.New("T3"     , null, null, null);
                string _msg;
                _msg = _root.ToStringSimple(); HDebug.Assert(_msg == "(_,grandparent_child,(T1,prnt,(T2,curr,T3)))");
                RotateLeft(ref _root.right);
                _msg = _root.ToStringSimple(); HDebug.Assert(_msg == "(_,grandparent_child,((T1,prnt,T2),curr,T3))");
            }
            ////////////////////////////////////////////////////////////////////////
            // grandparent_child                        grandparent_child         //
            //                  \                                        \        //
            //                   prnt                                     curr    //
            //                  /    \                                   /    \   //
            //                T1      curr        =>                 prnt      T3 //
            //                       /    \                         /    \        //
            //                     T2       T3                     T1     T2      //
            ////////////////////////////////////////////////////////////////////////
            Node<T> prnt = grandparent_child; HDebug.Assert(prnt.right != null);
            Node<T> curr = prnt.right;
            Node<T> t1   = prnt.left;
            Node<T> t2   = curr.left;
            Node<T> t3   = curr.right;
            Node<T> grandparent = prnt.parent;

            grandparent_child = curr;        curr.parent = grandparent;
            curr.left  = prnt;               prnt.parent = curr;
            curr.right = t3;    if(t3 != null) t3.parent = curr;
            prnt.left  = t1;    if(t1 != null) t1.parent = prnt;
            prnt.right = t2;    if(t2 != null) t2.parent = prnt;
        }

        static bool RotateRight_selftest = true;
        static void RotateRight<T>(ref Node<T> grandparent_child)
        {
            if(RotateRight_selftest)
            {
                RotateRight_selftest = false;
                Node<string> _root       = Node<string>.New("grandparent_child", null, null, null);
                _root.right              = Node<string>.New("prnt"   , null, null, null);
                _root.right.left         = Node<string>.New("curr"   , null, null, null);
                _root.right.right        = Node<string>.New("T3"     , null, null, null);
                _root.right.left.left    = Node<string>.New("T1"     , null, null, null);
                _root.right.left.right   = Node<string>.New("T2"     , null, null, null);
                string _msg;
                _msg = _root.ToStringSimple(); HDebug.Assert(_msg == "(_,grandparent_child,((T1,curr,T2),prnt,T3))");
                RotateRight(ref _root.right);
                _msg = _root.ToStringSimple(); HDebug.Assert(_msg == "(_,grandparent_child,(T1,curr,(T2,prnt,T3)))");
            }
            ////////////////////////////////////////////////////////////////////////
            // grandparent_child                  grandparent_child               //
            //                  \                                  \              //
            //                   prnt                               curr          //
            //                  /    \                             /    \         //
            //              curr      T3    =>                   T1      prnt     //
            //             /    \                                       /    \    //
            //            T1     T2                                   T2       T3 //
            ////////////////////////////////////////////////////////////////////////
            Node<T> prnt = grandparent_child; HDebug.Assert(prnt.left != null);
            Node<T> curr = prnt.left;
            Node<T> t1   = curr.left;
            Node<T> t2   = curr.right;
            Node<T> t3   = prnt.right;
            Node<T> grandparent = prnt.parent;

            grandparent_child = curr;        curr.parent = grandparent;
            curr.left  = t1;    if(t1 != null) t1.parent = curr;
            curr.right = prnt;               prnt.parent = curr;
            prnt.left  = t2;    if(t2 != null) t2.parent = prnt;
            prnt.right = t3;    if(t3 != null) t3.parent = prnt;
        }
    }
}
