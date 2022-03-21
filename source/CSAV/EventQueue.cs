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
    public class EventQueue
    {
        HPriorityQueue<EventPoint2D> eventqueue;
        List<EventPoint2D>      events_removed;
        HashSet<(int,int)>           intersect_added; // circle pairs processed for inserting intersections

        public class SegsIntsectpt
        {
            public Point2             pt  ;
            public CircleSegment.Type seg1;
            public CircleSegment.Type seg2;
            public SegsIntsectpt      next;
            public SegsIntsectpt(Point2 pt, CircleSegment.Type seg1, CircleSegment.Type seg2, SegsIntsectpt next)
            {
                this.pt   = pt  ;
                this.seg1 = seg1;
                this.seg2 = seg2;
                this.next = next;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void eventqueue_Add(EventPoint2D evt)
        {
            //HDebug.Assert(eventqueue_Contains(evt) == false);
            eventqueue.Push(evt);
            Debug.Listener.evtpts_add(evt);
        }
        //  [MethodImpl(MethodImplOptions.AggressiveInlining)]
        //  bool eventqueue_Contains(EventPoint2D evt)
        //  {
        //      // do not check Contains
        //      throw new Exception();
        //  }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        EventPoint2D eventqueue_PopTop()
        {
            //HDebug.Assert(eventqueue_Contains(evt) == true);
            EventPoint2D top = eventqueue.Top;
            eventqueue.Pop();
            events_removed.Add(top);
            return top;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EventQueue()
        {
            const int max_events = 300;
            eventqueue     = new HPriorityQueue<EventPoint2D>(max_events, new EventPoint2DComparerReverse());
            events_removed = new List          <EventPoint2D>(max_events);
            intersect_added= new HashSet<(int, int)>(max_events*2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            while(eventqueue.Count != 0)
                eventqueue.Pop();
            events_removed.Clear();
            intersect_added.Clear();
        }

        public EventPoint2D Top                 { [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return eventqueue.Top; } }
        public int Count                        { [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return eventqueue.Count; } }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddCircleStartEndEvent(Circle circle)
        {
            var enterpt = circle.TopPt;
            var exitpt  = circle.BottomPt;

            EventPoint2D enter = new EventPoint2D( enterpt.x, enterpt.y, circle.seg_left , circle.seg_right, EventPoint2D.Type.enter );
            EventPoint2D exit  = new EventPoint2D( exitpt .x, exitpt .y, circle.seg_left , circle.seg_right, EventPoint2D.Type.exit  );

            eventqueue_Add(enter);
            eventqueue_Add(exit);
        }

        /// Finds intersections of c1 and c2, if it exists
        /// Insert intersecting event points into queue
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EventPoint2D AddSegmentIntersectionEvent(CircleSegment c_left, CircleSegment c_right, EventPoint2D pt)
        {
            // check if they are of same circle
            if (c_left.circle.id == c_right.circle.id)
                return null;

            {
                HDebug.Assert(c_left.circle != c_right.circle);
                Circle circ1 = c_left .circle;
                Circle circ2 = c_right.circle;

                (int,int) circs = (circ1.id < circ2.id) ? (circ1.id, circ2.id) : (circ2.id, circ1.id);
                HDebug.Assert(circs.Item1 < circs.Item2);
                if(intersect_added.Contains(circs))
                    return null;
                intersect_added.Add(circs);

                var intsecs = Geometry.FindIntersectionCircle(circ1, circ2);
                if(intsecs == null)
                    return null;

                (Point2 pt, CircleSegment.Type seg1, CircleSegment.Type seg2) intsec1 = intsecs.Value.intsec_first ;
                (Point2 pt, CircleSegment.Type seg1, CircleSegment.Type seg2) intsec2 = intsecs.Value.intsec_second;
                foreach(EventPoint2D ep in EnumEventPointIntsecs(circ1, circ2, intsec1, intsec2))
                {
                    HDebug.Assert(pt.y >= ep.y);
                    HDebug.Assert(ep.ValidateIntersect());
                    //HDebug.Assert(eventqueue_Contains(ep) == false);
                    eventqueue_Add(ep);
                }
                return null;

                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                /// Supporting functions
                IEnumerable<EventPoint2D> EnumEventPointIntsecs
                    ( Circle circ1, Circle circ2
                    , (Point2 pt, CircleSegment.Type seg1, CircleSegment.Type seg2) intsec1
                    , (Point2 pt, CircleSegment.Type seg1, CircleSegment.Type seg2) intsec2
                    )
                {
                    foreach(EventPoint2D ep in EnumEventPointIntsec(circ1, circ2 ,intsec1)) yield return ep;
                    foreach(EventPoint2D ep in EnumEventPointIntsec(circ1, circ2 ,intsec2)) yield return ep;
                }
                static IEnumerable<EventPoint2D> EnumEventPointIntsec(Circle circ1, Circle circ2, (Point2 pt, CircleSegment.Type seg1, CircleSegment.Type seg2) intsec)
                {
                    bool stop = false;
                    const double tol_topbottom = 0.000001;    /// tolerance for hole at circle bottom

                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    // return (do not add point) if intsec.pt is at bottom of c1.circle or c2.circle
                    if(stop == false)
                    {
                        if(circ1.IsBottomPt(intsec.pt, tol_topbottom))
                            // pt is at c1.circle bottom => hole => no intersection
                            stop = true;
                            //return; 
                    }
                    if(stop == false)
                    {
                        if(circ2.IsBottomPt(intsec.pt, tol_topbottom))
                            // pt is at c2.circle bottom => hole => no intersection
                            stop = true;
                            //return; 
                    }
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    //  if intersection point is close to the top of circle
                    //  then make the intersection point as the top of circle
                    //  =>
                    //  
                    //                                       /
                    //  insert circle at * ==>    ___---*---#__        
                    //                         ---         /   ---
                    //                        /           /       \
                    //                       A           C  ^      B
                    //                                      |
                    //                                  intersection event
                    //                                    
                    //  In this case, segment order of adding * is (A,B,C).
                    //  However, since the circle point move very fast at near top point, when handling intersection at #
                    //    tree order is (A,B,C), but the x-point order can become (A,C,B).
                    //  Because this inconsistency between tree order and x-point order,
                    //    when handling intersection at #, a segment B or C cannot be found in tree
                    //  If make the intersection point as * (rather than #),
                    //    then this not-finding problem does not happens because B or C is going to be searched at *
                    if(stop == false)
                    {
                        if(circ1.IsTopPt(intsec.pt, tol_topbottom))
                        {
                            CircleSegment seg2 = (intsec.seg2 == CircleSegment.Type.left) ? circ2.seg_left : circ2.seg_right;
                            Point2 toppt = circ1.TopPt;
                            yield return CreateEventPointIntersect(toppt, circ1.seg_left , seg2);
                            yield return CreateEventPointIntersect(toppt, seg2, circ1.seg_right);
                            stop = true;
                        }
                    }
                    if(stop == false)
                    {
                        if(circ2.IsTopPt(intsec.pt, tol_topbottom))
                        {
                            CircleSegment seg1 = (intsec.seg1 == CircleSegment.Type.left) ? circ1.seg_left : circ1.seg_right;
                            Point2 toppt = circ2.TopPt;
                            yield return CreateEventPointIntersect(toppt, circ2.seg_left , seg1);
                            yield return CreateEventPointIntersect(toppt, seg1, circ2.seg_right);
                            stop = true;
                        }
                    }
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    // return intersection at intsec.pt
                    if(stop == false)
                    {
                        CircleSegment seg1 = (intsec.seg1 == CircleSegment.Type.left) ? circ1.seg_left : circ1.seg_right;
                        CircleSegment seg2 = (intsec.seg2 == CircleSegment.Type.left) ? circ2.seg_left : circ2.seg_right;
                        yield return CreateEventPointIntersect(intsec.pt, seg1, seg2);
                    }
                }
                static EventPoint2D CreateEventPointIntersect(Point2 pt, CircleSegment seg1, CircleSegment seg2)
                {
                    // check left/right between seg1 and seg2
                    HDebug.Assert(seg1.IsOnSegment(pt));
                    HDebug.Assert(seg2.IsOnSegment(pt));
                    (double seg1a, double seg1b) = Geometry.FindCircleTangent(seg1.circle.center_x, seg1.circle.center_y, pt.x, pt.y);
                    (double seg2a, double seg2b) = Geometry.FindCircleTangent(seg2.circle.center_x, seg2.circle.center_y, pt.x, pt.y);
                    if((seg1a == 0) || (seg2a == 0))
                    {
                        // if one of them is horizontal line, check order by y coord
                        HDebug.Assert(CircleSegmentComparer.Compare(seg1, seg2, pt.y) != 0);
                        if(CircleSegmentComparer.Compare(seg1, seg2, pt.y) < 0) return new EventPoint2D( pt.x, pt.y, seg1, seg2, EventPoint2D.Type.intersect);
                        else                                                    return new EventPoint2D( pt.x, pt.y, seg2, seg1, EventPoint2D.Type.intersect);
                    }
                    else
                    {
                        (char  seg1lr, char  seg2lr) = Geometry.DetermineLeftRight(seg1a, seg2a);
                        // make intsect event point
                        if(seg1lr=='l') { HDebug.Assert(seg1lr=='l' && seg2lr=='r'); return new EventPoint2D( pt.x, pt.y, seg1, seg2, EventPoint2D.Type.intersect); }
                        if(seg1lr=='r') { HDebug.Assert(seg2lr=='l' && seg1lr=='r'); return new EventPoint2D( pt.x, pt.y, seg2, seg1, EventPoint2D.Type.intersect); }
                        throw new Exception();
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EventPoint2D PopTop()
        {
            var maxevent = eventqueue_PopTop();
#if DEBUG
            maxevent._debug = Debug._debug;
#endif
            return maxevent;
        }
    }
}
