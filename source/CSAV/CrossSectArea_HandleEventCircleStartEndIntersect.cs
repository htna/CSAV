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
using System.Runtime.CompilerServices;
using CSAV.HTLib2;

namespace CSAV
{
    public partial class CrossSectArea
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void HandleEventCircleStart
            ( EventQueue eventqueue
            , LinkedAvlTree<CircleSegment> tree
            , List<(CircleSegment c1, CircleSegment c2)> solvpatches
            , CircleSegment c_left
            , CircleSegment c_right
            , EventPoint2D pt
            )
        {
            // insert segments and do appropriate operations
            HDebug.Assert(c_left .type == CircleSegment.Type.left );
            HDebug.Assert(c_right.type == CircleSegment.Type.right);

            // insert the node
            Debug.Listener.tree_add(2);
            LinkedAvlTree<CircleSegment>.Node node_left       = tree.Insert(c_left );
            LinkedAvlTree<CircleSegment>.Node node_right      = tree.Insert(c_right);
            LinkedAvlTree<CircleSegment>.Node node_left_prev  = node_left .prev;
            LinkedAvlTree<CircleSegment>.Node node_left_next  = node_left .next;
            LinkedAvlTree<CircleSegment>.Node node_right_prev = node_right.prev;
            LinkedAvlTree<CircleSegment>.Node node_right_next = node_right.next;

            ////////////////////////////////////////////////////////////////////////////
            /// remove patches between ( node_left_prev ,         , node_left_next, ... , node_right_prev ,          , node_right_next )
            ///               excluding                  node_left                                         node_right
            {
                if((node_left.next == node_right) || (node_left == node_right.prev))
                {
                    // there is no gap between (node_left, node_right)
                    HDebug.Assert(node_left.next == node_right);
                    HDebug.Assert(node_left == node_right.prev);
                    //Patches_RemovePatch(solvpatches, node_left_prev, node_right_next);
                    if((node_left_prev != null) && (node_right_next != null))
                    {
                        if(node_left_prev.value.patchval == 2) solvpatches.Remove  ((node_left_prev.value, node_right_next.value));
                        HDebug.Assert(                false == solvpatches.Contains((node_left_prev.value, node_right_next.value)));
                    }
                }
                else
                {
                    //Patches_RemovePatch(solvpatches, node_left_prev , node_left_next );
                    if((node_left_prev != null) && (node_left_next != null))
                    {
                        if(node_left_prev.value.patchval == 2) solvpatches.Remove  ((node_left_prev.value, node_left_next.value));
                        HDebug.Assert(                false == solvpatches.Contains((node_left_prev.value, node_left_next.value)));
                    }
                        
                    //Patches_RemovePatch(solvpatches, node_right_prev, node_right_next);
                    if((node_right_prev != null) && (node_right_next != null))
                    {
                        if(node_right_prev.value.patchval == 2) solvpatches.Remove  ((node_right_prev.value, node_right_next.value));
                        HDebug.Assert(                 false == solvpatches.Contains((node_right_prev.value, node_right_next.value)));
                    }

                    if(node_left_next != node_right_prev)
                    {
                        SolvPatches_RemovePatch_NodeFrom_NodeTo
                        ( tree
                        , solvpatches
                        , node_left_next 
                        , node_right_prev
                        );
                    }
                }
            }
            ////////////////////////////////////////////////////////////////////////////
            /// delete c_left and c_right in tree
            if(node_left_prev != null)
            {
                CircleSegment c_left_prev = node_left_prev.value;
                EventPoint2D newpt = eventqueue.AddSegmentIntersectionEvent(c_left_prev, c_left, pt);
                HDebug.Assert((newpt != null && c_left.circle.IsTopPt(newpt.x, newpt.y)) == false);
            }
            if(node_right_next != null)
            {
                CircleSegment c_right_next = node_right_next.value;
                EventPoint2D newpt = eventqueue.AddSegmentIntersectionEvent(c_right, c_right_next, pt);
                HDebug.Assert((newpt != null && c_left.circle.IsTopPt(newpt.x, newpt.y)) == false);
            }
            if(node_left_next != node_right)
            {
                // if left.next != right,
                // then this means that there is a segment seg such as (left, seg, right)
                // => add intersections between (left,seg) and (seg,right)
                //    as if seg passes through the top of circle
                HDebug.Assert(node_left_next != node_right     );
                HDebug.Assert(node_left      != node_right_prev);
                {
                    CircleSegment c_left_next = node_left_next.value;
                    EventPoint2D newpt = eventqueue.AddSegmentIntersectionEvent(c_left, c_left_next, pt);
                    if(newpt != null && c_left.circle.IsTopPt(newpt.x, newpt.y))
                    {
                        // add intersect(c_left_prev, c_right) into event queue
                        EventPoint2D newpt2 = eventqueue.AddSegmentIntersectionEvent(c_left_next, c_right, pt);
                        HDebug.Assert(newpt2 != null);
                    }
                }
                {
                    CircleSegment c_right_prev = node_right_prev.value;
                    EventPoint2D newpt = eventqueue.AddSegmentIntersectionEvent(c_right, c_right_prev, pt);
                    if(newpt != null && c_left.circle.IsTopPt(newpt.x, newpt.y))
                    {
                        // add intersect(c_left_prev, c_left) into event queue
                        EventPoint2D newpt2 = eventqueue.AddSegmentIntersectionEvent(c_left, c_right_prev, pt);
                        HDebug.Assert(newpt2 != null);
                    }
                }
            }
            ////////////////////////////////////////////////////////////////////////////
            /// update segment.patchval
            /// add patches between ( node_left_prev , ... , node_right_next )
            SolvPatches_UpdatePatchval_AddPatch_NodeFrom_NodeTo
            ( tree
            , solvpatches
            , node_left_prev 
            , node_right_next
            );
            ////////////////////////////////////////////////////////////////////////////
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void HandleEventCircleEnd
            ( EventQueue eventqueue
            , LinkedAvlTree<CircleSegment> tree
            , List<(CircleSegment c1, CircleSegment c2)> solvpatches
            , CircleSegment c_left
            , CircleSegment c_right
            , EventPoint2D pt
            )
        {
            HDebug.Assert(c_left .type == CircleSegment.Type.left );
            HDebug.Assert(c_right.type == CircleSegment.Type.right);

            LinkedAvlTree<CircleSegment>.Node node_left       = tree.Search(c_left );
            LinkedAvlTree<CircleSegment>.Node node_right      = tree.Search(c_right);
            LinkedAvlTree<CircleSegment>.Node node_left_prev  = node_left .prev;
            LinkedAvlTree<CircleSegment>.Node node_left_next  = node_left .next;
            LinkedAvlTree<CircleSegment>.Node node_right_prev = node_right.prev;
            LinkedAvlTree<CircleSegment>.Node node_right_next = node_right.next;


            ////////////////////////////////////////////////////////////////////////////
            /// remove patches between ( node_left_prev , ... , node_right_next )
            SolvPatches_RemovePatch_NodeFrom_NodeTo
            ( tree
            , solvpatches
            , node_left_prev 
            , node_right_next
            );
            ////////////////////////////////////////////////////////////////////////////
            /// delete c_left and c_right in tree
            /// 
            /// Following if statment means as follows:
            /// 1. there exists one omitted segment that intersects with either c1 or c2 at exact same point, Or
            ///     -> find new intersect with c1.prev and c2.next
            /// 2. there exists intersecting event point(s) at exact same point
            ///     -> no need to concern as new intersecting event points will be covered when swap operation executes
            ///     -> the segments that intersects c1 and c2 at this point must be the second intersecting point (since it is exiting event points of c1 and c2)
            /// thus, in either case, simply handle new intersecting evnet points betweeb node_left_prev, node_left_next, node_right_next
            if((node_left_next == node_right) || (node_left == node_right_prev))
            {
                HDebug.Assert(node_left_next == node_right     );
                HDebug.Assert(node_left      == node_right_prev);
                if(node_left_prev != null && node_right_next != null)
                    eventqueue.AddSegmentIntersectionEvent(node_left_prev.value, node_right_next.value, pt);
            }
            else // meaning that node_left_next is not the deleted node
            {
                if(node_left_prev != null)
                {
                    eventqueue.AddSegmentIntersectionEvent(node_left_prev.value, node_left_next.value, pt);
                }
                if(node_right_next != null)
                {
                    eventqueue.AddSegmentIntersectionEvent(node_right_prev.value, node_right_next.value, pt);
                }
            }
            tree.Delete(c_left );
            tree.Delete(c_right);
            Debug.Listener.tree_del(2);
            ////////////////////////////////////////////////////////////////////////////
            /// update segment.patchval
            /// add patches between ( node_left_prev , ... , node_right_next )
            SolvPatches_UpdatePatchval_AddPatch_NodeFrom_NodeTo
            ( tree
            , solvpatches
            , node_left_prev 
            , node_right_next
            );
            ////////////////////////////////////////////////////////////////////////////
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void HandleEventSegmentIntersect
            ( EventQueue eventqueue
            , LinkedAvlTree<CircleSegment> tree
            , List<(CircleSegment c1, CircleSegment c2)> solvpatches
            , CircleSegment c1
            , CircleSegment c2
            , EventPoint2D pt
            )
        {
            LinkedAvlTree<CircleSegment>.Node node_c1 = tree.Search(c1);
            LinkedAvlTree<CircleSegment>.Node node_c2 = tree.Search(c2);
            HDebug.Assert(node_c1.value == c1);
            HDebug.Assert(node_c2.value == c2);

            LinkedAvlTree<CircleSegment>.Node node_c0 = node_c1.prev;
            LinkedAvlTree<CircleSegment>.Node node_c3 = node_c2.next;

            ////////////////////////////////////////////////////////////////////////////
            /// remove patches between ( node_c1_prev , ... , node_c2_next )
            SolvPatches_RemovePatch_NodeFrom_NodeTo
            ( tree
            , solvpatches
            , node_c0 
            , node_c3
            );
            ////////////////////////////////////////////////////////////////////////////
            /// swap nodes 
            ///   from (c0, c1, c2, c3)
            ///     to (c0, c2, c1, c3)
            if(node_c1.next == node_c2)
            {
                HDebug.Assert(node_c1.next == node_c2);
                HDebug.Assert(node_c2.prev == node_c1);
                // (c1_prev, c1, c2, c2_next)
                CircleSegment tmp = node_c1.value;
                node_c1.value = node_c2.value;
                node_c2.value = tmp;
                Debug.Listener.tree_swp(2);
                // (c1_prev, c2, c1, c2_next)
                if (node_c0 != null)
                {
                    CircleSegment c0 = node_c0.value;
                    eventqueue.AddSegmentIntersectionEvent(c0, c2, pt);
                }
                if (node_c3 != null)
                {
                    CircleSegment c3 = node_c3.value;
                    eventqueue.AddSegmentIntersectionEvent(c1, c3, pt);
                }
                eventqueue.AddSegmentIntersectionEvent(c2, c1, pt);
            }
            else
            {
                /// 1. collect segments (c1, c1.next, ..., c2.prev, c2)
                /// 2. determine potentially updated eventqueue.max.y
                ///      by checking intersections bt pairs in between {c1, c1.next, ..., c2.prev, c2}
                /// 3. sort (c1, c1.next, ..., c2.prev, c2)
                ///      using (pt.y + potential.max.y)/2
                ///      which will be actual sorted list after adding additional intersections
                /// 4. add intersection between (c0, c1)
                /// 5. add intersection between (c2, c3)
                /// 6. add intersections between (c1, c1.next), ..., (c2.prev, c2)
                /// 7. update segments in tree by sorted (c1, c1.next, ..., c2.prev, c2)
                /// 
                /// The reason of (insert intersect) -> (swap node) is because
                ///   it is possible that the new added intersection can be the top,
                ///   and its location will help to correctly sort segments between (c1, ..., c2)
                HDebug.Assert(node_c1.value == c1);
                HDebug.Assert(node_c2.value == c2);
                // cs <- segments between c1 and c2
                List<CircleSegment> cs = new List<CircleSegment>();
                LinkedAvlTree<CircleSegment>.Node node_ci;
                for( node_ci = node_c1; node_ci != node_c2.next; node_ci = node_ci.next )
                {
                    cs.Add(node_ci.value);
                }
                Debug.Listener.tree_swp(cs.Count);
                // determine updated max_y
                double eventpoints_max_y = eventqueue.Top.y;
                // sort cs with (currep.y + eventpoints.max.y)/2
                {
                    double y = (pt.y + eventpoints_max_y)/2;
                    int Compare(CircleSegment a, CircleSegment b)
                    {
                        return CircleSegmentComparer.Compare(a, b, y);
                    }
                    cs.Sort(Compare);
                }
                // add intersections between (c0, c1), (c0, c1.next), ..., (c0,c2)
                if (node_c0 != null)
                {
                    CircleSegment c0 = node_c0.value;
                    eventqueue.AddSegmentIntersectionEvent(c0, cs.First(), pt);
                }
                // add intersections between (c1,c3), (c1.next,c3), ..., (c2,c3)
                if (node_c3 != null)
                {
                    CircleSegment c3 = node_c3.value;
                    eventqueue.AddSegmentIntersectionEvent(cs.Last(), c3, pt);
                }
                // add intersections between sorted (c1, c1.next), ..., (c2.prev, c2)
                for(int i=1; i<cs.Count; i++)
                {
                    CircleSegment ci1 = cs[i-1];
                    CircleSegment ci2 = cs[i  ];
                    eventqueue.AddSegmentIntersectionEvent(ci1, ci2, pt);
                }
                // update segments with the sorted cs
                for( node_ci = node_c1; node_ci != node_c2.next; node_ci = node_ci.next )
                {
                    node_ci.value = cs[0];
                    cs.RemoveAt(0);
                }
                HDebug.Assert(cs.Count == 0);
            }
            ////////////////////////////////////////////////////////////////////////////
            /// update segment.patchval
            /// add patches between ( node_left_prev , ... , node_right_next )
            SolvPatches_UpdatePatchval_AddPatch_NodeFrom_NodeTo
            ( tree
            , solvpatches
            , node_c0 
            , node_c3
            );
            ////////////////////////////////////////////////////////////////////////////
        }
    }
}
