/////////////////////////////////////////////////////////////////////////////////////////
//  MIT License                                                                        //
//                                                                                     //
//  Copyright (c) 2022 Hyuntae Na and In Jung Kim                                      //
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
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using CSAV.HTLib2;

namespace CSAV
{
    public partial class CrossSectArea
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void SolvPatches_RemovePatch
            ( List<(CircleSegment c1, CircleSegment c2)> solvpatches
            , LinkedAvlTree<CircleSegment>.Node patch_c1
            , LinkedAvlTree<CircleSegment>.Node patch_c2
            )
        {
            if(patch_c1 == null) return;
            if(patch_c2 == null) return;
            (CircleSegment c1, CircleSegment c2) patch = (patch_c1.value, patch_c2.value);
            if(patch.c1.patchval == 2)
                solvpatches.Remove(patch);
            else
                HDebug.Assert(solvpatches.Contains(patch) == false);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void SolvPatches_RemovePatch_NodeFrom_NodeTo
            ( LinkedAvlTree<CircleSegment> tree
            , List<(CircleSegment c1, CircleSegment c2)> solvpatches
            , LinkedAvlTree<CircleSegment>.Node node_from
            , LinkedAvlTree<CircleSegment>.Node node_to
            )
        {
            if(node_from == tree.GetTail()) return;
            if(node_to   == tree.GetHead()) return;
            if(node_from == null) node_from = tree.GetHead();
            if(node_to   == null) node_to   = tree.GetTail();
                
            LinkedAvlTree<CircleSegment>.Node node_from_next = node_from.next;
            if(node_from_next == node_to)
            {
                CircleSegment c1 = node_from.value;
                CircleSegment c2 = node_from_next.value;
                if(c1.patchval == 2) solvpatches.Remove((c1,c2));
                else HDebug.Assert(solvpatches.Contains((c1,c2)) == false);
                return;
            }
            LinkedAvlTree<CircleSegment>.Node node_from_next_next = node_from_next.next;
            if(node_from_next_next == node_to)
            {
                CircleSegment c1 = node_from.value;
                CircleSegment c2 = node_from_next.value;
                CircleSegment c3 = node_from_next_next.value;
                if(c1.patchval == 2) solvpatches.Remove((c1,c2)); else HDebug.Assert(solvpatches.Contains((c1,c2)) == false);
                if(c2.patchval == 2) solvpatches.Remove((c2,c3)); else HDebug.Assert(solvpatches.Contains((c2,c3)) == false);
                return;
            }
            if(node_from_next_next.next == node_to)
            {
                CircleSegment c1 = node_from.value;
                CircleSegment c2 = node_from_next.value;
                CircleSegment c3 = node_from_next_next.value;
                CircleSegment c4 = node_to.value;
                if(c1.patchval == 2) solvpatches.Remove((c1,c2)); else HDebug.Assert(solvpatches.Contains((c1,c2)) == false);
                if(c2.patchval == 2) solvpatches.Remove((c2,c3)); else HDebug.Assert(solvpatches.Contains((c2,c3)) == false);
                if(c3.patchval == 2) solvpatches.Remove((c3,c4)); else HDebug.Assert(solvpatches.Contains((c3,c4)) == false);
                return;
            }

            SolvPatches_RemovePatch_NodeFrom_NodeTo_Iter(tree, solvpatches, node_from, node_to);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void SolvPatches_RemovePatch_NodeFrom_NodeTo_Iter
            ( LinkedAvlTree<CircleSegment> tree
            , List<(CircleSegment c1, CircleSegment c2)> solvpatches
            , LinkedAvlTree<CircleSegment>.Node node_from   // node_left_prev
            , LinkedAvlTree<CircleSegment>.Node node_to     // node_right_next
            )
        {
            ////////////////////////////////////////////////////////////////////////////
            /// remove solv patches between (node_from, node_from.next) - ... - (node_to.prev, node_to)
            LinkedAvlTree<CircleSegment>.Node node_patchdel_from = (node_from != null) ? node_from : tree.GetHead();
            LinkedAvlTree<CircleSegment>.Node node_patchdel_to   = (node_to   != null) ? node_to   : tree.GetTail();
            LinkedAvlTree<CircleSegment>.Node i0 = node_patchdel_from;
            LinkedAvlTree<CircleSegment>.Node i1 = i0.next;
            while(i0 != node_patchdel_to)
            {
                HDebug.Assert(i0 != null);
                HDebug.Assert(i1 != null);
                CircleSegment c0 = i0.value;
                if(c0.patchval == 2)
                {
                    (CircleSegment, CircleSegment) patch = (c0, i1.value);
                    HDebug.Assert(solvpatches.Contains(patch));
                    solvpatches.Remove(patch);
                }
                if(HDebug.IsDebuggerAttached)
                {
                    (CircleSegment, CircleSegment) patch = (c0, i1.value);
                    HDebug.Assert(solvpatches.Contains(patch) == false);
                }
                i0 = i1;
                i1 = i1.next;
            }
            HDebug.Assert(i0 == node_patchdel_to);
        }
        ////////////////////////////////////////////////////////////////////////////
        /// update patch value between                  node_from.next  - ... -  node_to.prev
        /// check  patch value no-change at                                                    node_to
        /// add solv patches between        (node_from, node_from.next) - ... - (node_to.prev, node_to)
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void SolvPatches_UpdatePatchval_AddPatch_NodeFrom_NodeTo
            ( LinkedAvlTree<CircleSegment> tree
            , List<(CircleSegment c1, CircleSegment c2)> solvpatches
            , LinkedAvlTree<CircleSegment>.Node node_from
            , LinkedAvlTree<CircleSegment>.Node node_to
            )
        {
            if(node_from == null)
            {
                if(node_to == null)
                {
                    HDebug.Assert(node_from == null && node_to == null);
                    ////////////////////////////////////////////////////////////////////////////
                    /// update patch value between           node_from.next  - ... -  node_to.prev
                    /// check  patch value no-change at                                             node_to
                    /// add solv patches between (node_from, node_from.next) - ... - (node_to.prev, node_to)
                    LinkedAvlTree<CircleSegment>.Node head = tree.GetHead();
                    LinkedAvlTree<CircleSegment>.Node tail = tree.GetTail();
                    if(head == null)
                        return;
                    if(head.next == tail)
                    {
                        // ( null, head, tail, null )
                        //CircleSegment c0 = null;
                        CircleSegment c1 = head.value;  c1.UpdatePatchval(null); HDebug.Assert(c1.patchval != 2);
                        CircleSegment c2 = tail.value;  c2.UpdatePatchval(c1  ); HDebug.Assert(c2.patchval != 2);
                        return;
                    }
                    else
                    {
                        SolvPatches_UpdatePatchval_AddPatch_All(tree, solvpatches);
                        return;
                    }
                }
                else
                {
                    HDebug.Assert(node_from == null && node_to != null);
                    ////////////////////////////////////////////////////////////////////////////
                    /// update patch value between           node_from.next  - ... -  node_to.prev
                    /// check  patch value no-change at                                             node_to
                    /// add solv patches between (node_from, node_from.next) - ... - (node_to.prev, node_to)
                    LinkedAvlTree<CircleSegment>.Node node_from_next = tree.GetHead();
                    if(node_from_next == node_to)
                    {
                        // ( null, node_to )
                        HDebug.Assert(node_to.value.GetUpdatedPatchval(null) == node_to.value.patchval);
                        return;
                    }
                    LinkedAvlTree<CircleSegment>.Node node_from_next_next = node_from_next.next;
                    if(node_from_next_next == node_to)
                    {
                        // ( null, node_from_next, node_to )
                        //CircleSegment c1 = null;
                        CircleSegment c2 = node_from_next.value;        c2.    UpdatePatchval(null);
                        CircleSegment c3 = node_to.value; HDebug.Assert(c3.GetUpdatedPatchval(c2) == c3.patchval);
                        HDebug.Assert(c2.patchval != 2); HDebug.Assert(solvpatches.Contains((c2,c3)) == false);
                        return;
                    }
                    if(node_from_next_next.next == node_to)
                    {
                        // ( null, node_from_next, node_from_next_next, node_to )
                        //CircleSegment c1 = null;
                        CircleSegment c2 = node_from_next.value;        c2.    UpdatePatchval(null);
                        CircleSegment c3 = node_from_next_next.value;   c3.    UpdatePatchval(c2);
                        CircleSegment c4 = node_to.value; HDebug.Assert(c4.GetUpdatedPatchval(c3) == c4.patchval);
                        HDebug.Assert(c2.patchval != 2); HDebug.Assert(solvpatches.Contains((c2,c3)) == false);
                        HDebug.Assert(solvpatches.Contains((c3,c4)) == false); if(c3.patchval == 2) solvpatches.Add((c3,c4));
                        return;
                    }
                }
            }
            else
            {
                if(node_to == null)
                {
                    HDebug.Assert(node_from != null && node_to == null);
                    ////////////////////////////////////////////////////////////////////////////
                    /// update patch value between           node_from.next  - ... -  node_to.prev
                    /// check  patch value no-change at                                             node_to
                    /// add solv patches between (node_from, node_from.next) - ... - (node_to.prev, node_to)
                    LinkedAvlTree<CircleSegment>.Node node_from_next = node_from.next;
                    if(node_from_next == node_to)
                    {
                        // ( node_from, null )
                        //CircleSegment c1 = node_from.value;
                        //CircleSegment c2 = null;
                        HDebug.Assert(node_from.value.patchval == 0);
                        return;
                    }
                    LinkedAvlTree<CircleSegment>.Node node_from_next_next = node_from_next.next;
                    if(node_from_next_next == node_to)
                    {
                        // ( node_from, node_from_next, null )
                        CircleSegment c1 = node_from.value;
                        CircleSegment c2 = node_from_next.value; c2.UpdatePatchval(c1);
                        //CircleSegment c3 = null;
                        HDebug.Assert(solvpatches.Contains((c1,c2)) == false); if(c1.patchval == 2) solvpatches.Add((c1,c2));
                        HDebug.Assert(c2.patchval == 0);
                        return;
                    }
                    if(node_from_next_next.next == node_to)
                    {
                        // ( node_from, node_from_next, node_from_next_next, null )
                        CircleSegment c1 = node_from.value;
                        CircleSegment c2 = node_from_next.value;        c2.UpdatePatchval(c1);
                        CircleSegment c3 = node_from_next_next.value;   c3.UpdatePatchval(c2);
                        //CircleSegment c4 = null;
                        HDebug.Assert(solvpatches.Contains((c1,c2)) == false); if(c1.patchval == 2) solvpatches.Add((c1,c2));
                        HDebug.Assert(solvpatches.Contains((c2,c3)) == false); if(c2.patchval == 2) solvpatches.Add((c2,c3));
                        HDebug.Assert(c3.patchval == 0);
                        return;
                    }
                }
                else
                {
                    HDebug.Assert(node_from != null && node_to != null);
                    ////////////////////////////////////////////////////////////////////////////
                    /// update patch value between           node_from.next  - ... -  node_to.prev
                    /// check  patch value no-change at                                             node_to
                    /// add solv patches between (node_from, node_from.next) - ... - (node_to.prev, node_to)
                    LinkedAvlTree<CircleSegment>.Node node_from_next = node_from.next;
                    if(node_from_next == node_to)
                    {
                        // ( node_from, node_to )
                        CircleSegment c1 = node_from.value;
                        CircleSegment c2 = node_to.value; HDebug.Assert(c2.GetUpdatedPatchval(c1) == c2.patchval);
                        HDebug.Assert(solvpatches.Contains((c1,c2)) == false); if(c1.patchval == 2) solvpatches.Add((c1,c2));
                        return;
                    }
                    LinkedAvlTree<CircleSegment>.Node node_from_next_next = node_from_next.next;
                    if(node_from_next_next == node_to)
                    {
                        // ( node_from, node_from_next, node_to )
                        CircleSegment c1 = node_from.value;
                        CircleSegment c2 = node_from_next.value;        c2.    UpdatePatchval(c1);
                        CircleSegment c3 = node_to.value; HDebug.Assert(c3.GetUpdatedPatchval(c2) == c3.patchval);
                        HDebug.Assert(solvpatches.Contains((c1,c2)) == false); if(c1.patchval == 2) solvpatches.Add((c1,c2));
                        HDebug.Assert(solvpatches.Contains((c2,c3)) == false); if(c2.patchval == 2) solvpatches.Add((c2,c3));
                        return;
                    }
                    if(node_from_next_next.next == node_to)
                    {
                        // ( node_from, node_from_next, node_to )
                        CircleSegment c1 = node_from.value;
                        CircleSegment c2 = node_from_next.value;        c2.    UpdatePatchval(c1);
                        CircleSegment c3 = node_from_next_next.value;   c3.    UpdatePatchval(c2);
                        CircleSegment c4 = node_to.value; HDebug.Assert(c4.GetUpdatedPatchval(c3) == c4.patchval);
                        HDebug.Assert(solvpatches.Contains((c1,c2)) == false); if(c1.patchval == 2) solvpatches.Add((c1,c2));
                        HDebug.Assert(solvpatches.Contains((c2,c3)) == false); if(c2.patchval == 2) solvpatches.Add((c2,c3));
                        HDebug.Assert(solvpatches.Contains((c3,c4)) == false); if(c3.patchval == 2) solvpatches.Add((c3,c4));
                        return;
                    }
                }
            }

            SolvPatches_UpdatePatchval_AddPatch_NodeFrom_NodeTo_Iter(tree, solvpatches, node_from, node_to);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void SolvPatches_UpdatePatchval_AddPatch_All
            ( LinkedAvlTree<CircleSegment> tree
            , List<(CircleSegment c1, CircleSegment c2)> solvpatches
            )
        {
            ////////////////////////////////////////////////////////////////////////////
            /// Special case
            ///      node_from                      node_to
            ///     ( null    , head) - ... - (tail, null  )
            ///     
            /// update patch value  between      head - ... - tail
            ///    add solv patches between      head - ... - tail
            LinkedAvlTree<CircleSegment>.Node head = tree.GetHead();
            LinkedAvlTree<CircleSegment>.Node tail = tree.GetTail();
            head.value.UpdatePatchval(null);
            LinkedAvlTree<CircleSegment>.Node i0 = head;
            LinkedAvlTree<CircleSegment>.Node i1 = head.next;
            while(i0 != tail)
            {
                CircleSegment c0 = i0.value;
                CircleSegment c1 = i1.value;
                c1.UpdatePatchval(c0);
                if(c0 != null && c0.patchval == 2)
                {
                    (CircleSegment, CircleSegment) patch = (c0, c1);
                    HDebug.Assert(solvpatches.Contains(patch) == false);
                    solvpatches.Add(patch);
                }
                else
                {
                    if(HDebug.IsDebuggerAttached)
                    {
                        (CircleSegment, CircleSegment) patch = (c0, c1);
                        HDebug.Assert(solvpatches.Contains(patch) == false);
                    }
                }
                i0 = i1;
                i1 = i1.next;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void SolvPatches_UpdatePatchval_AddPatch_NodeFrom_NodeTo_Iter
            ( LinkedAvlTree<CircleSegment> tree
            , List<(CircleSegment c1, CircleSegment c2)> solvpatches
            , LinkedAvlTree<CircleSegment>.Node node_from
            , LinkedAvlTree<CircleSegment>.Node node_to
            )
        {
            ////////////////////////////////////////////////////////////////////////////
            /// update patch value between           node_from.next  - ... -  node_to.prev
            /// check  patch value no-change at                                             node_to
            /// add solv patches between (node_from, node_from.next) - ... - (node_to.prev, node_to)
            {
                LinkedAvlTree<CircleSegment>.Node i0 = node_from;
                LinkedAvlTree<CircleSegment>.Node i1 = (i0 != null) ? i0.next : tree.GetHead();
                CircleSegment c0 = (i0 != null)? i0.value : null;
                CircleSegment c1 = i1.value;
                while(i0 != node_to)
                {
                    if(i1 != node_to)
                        c1.UpdatePatchval(c0);
                    if(c0 != null && c0.patchval == 2)
                    {
                        (CircleSegment, CircleSegment) patch = (c0, c1);
                        HDebug.Assert(solvpatches.Contains(patch) == false);
                        solvpatches.Add(patch);
                    }
                    else
                    {
                        if(HDebug.IsDebuggerAttached)
                        {
                            (CircleSegment, CircleSegment) patch = (c0, c1);
                            HDebug.Assert(solvpatches.Contains(patch) == false);
                        }
                    }
                    if(i1 == null)
                        break;
                    i0 = i1;
                    i1 = i1.next;
                    c0 = c1;
                    c1 = (i1 != null)? i1.value : null;
                }
                if(HDebug.IsDebuggerAttached)
                {
                    if(node_to != null && node_to.prev != null)
                    {
                        CircleSegment c_to      = node_to.value;
                        CircleSegment c_to_prev = node_to.prev.value;
                        int c_to_patchval = c_to.GetUpdatedPatchval(c_to_prev);
                        HDebug.Assert(c_to.patchval == c_to_patchval);
                    }
                }
            }
        }
    }
}
