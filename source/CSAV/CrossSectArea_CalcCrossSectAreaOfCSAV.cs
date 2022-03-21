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
        public static double CalcCrossSectAreaOfCSAV
            ( List<Circle> circles
            , EventQueue eventqueue
            , bool stopAtEvtExitSolv
            )
        {
            // for each circles, initialize eventqueue
            Debug._debug_eventqueue = eventqueue;
            eventqueue.Clear();

            var treecompcs  = new CircleSegmentComparer();
            var tree        = new LinkedAvlTree<CircleSegment>(treecompcs.Compare); // AVL tree sorted according to y
            var solvpatches = new List<(CircleSegment c1, CircleSegment c2)>(10);

            Debug._debug++;
            Debug._debug_tree = tree;
            if(HDebug.IsDebuggerAttached)
            {
                if(Debug._debug_area_lst_y == null)
                    Debug._debug_area_lst_y = new List<(Tuple<double, double>, double)>(1000);
                Debug._debug_area_lst_y.Clear();
            }

            Debug.Listener.sweepline_start(circles.Count);
            double area;
            {
                foreach (var circle in circles)
                {
                    eventqueue.AddCircleStartEndEvent(circle);
                }

                area = CalcCrossSectAreaOfCSAV
                    ( eventqueue
                    , treecompcs
                    , tree 
                    , solvpatches
                    , stopAtEvtExitSolv
                    );
            }
            Debug.Listener.sweepline_end();

            return area;
        }
        public static double CalcCrossSectAreaOfCSAV
            ( EventQueue eventqueue
            , CircleSegmentComparer circsegcomp
            , LinkedAvlTree<CircleSegment> tree // AVL tree sorted according to y
            , List<(CircleSegment c1, CircleSegment c2)> solvpatches
            , bool stopAtEvtExitSolv // = false
            ) 
        {
            double sumofarea = 0.0;

            double prevy = double.PositiveInfinity;
            double curry = double.MaxValue;
                
            while (eventqueue.Count > 0)
            {
                Debug._debug ++;
                //HDebug.Break(_debug == 128609);

                EventPoint2D currep = eventqueue.PopTop(); Debug._debug_currep = currep; Debug.Listener.evtpts_execute(currep);

                bool hasGapBwPrevyCurry = false;
                // update prevy
                {
                    HDebug.Assert(prevy > curry);
                    if(curry != currep.y)
                    {
                        hasGapBwPrevyCurry = true;
                        prevy = curry;
                        curry = currep.y;

                        // verify tree (do not execute when there's multiple eps with same y)
                        //double y1 = Mean(prevy ,curry);
                        double y1;
                        {
                            // mean of (prevy and curry)
                            if(prevy == curry)  y1 = prevy;
                            else                y1 = (prevy + curry) / 2;
                            // if (y1 is above prevy) or (y1 is below curry), then y1 <- prevy
                            if(y1 > prevy || y1 <= curry)
                                y1 = prevy;
                        }

                        circsegcomp.SetY( y1 );
                        HDebug.Assert(Debug.ValidateTree(tree, circsegcomp.CompareValidate, (prevy+curry)/2));
                    }
                    HDebug.Assert(prevy > curry);
                }
                HDebug.Assert(Double.IsNaN(curry) == false);


                //////////////////////////////////////////////////////////////////////////////////////////////////////
                // calculate the area
                // Calculate areas of solv patches
                if(HDebug.IsDebuggerAttached)
                {
                    HDebug.Assert(ValidateSolvAreaPatch(tree, curry, solvpatches));
                }
                /// 1. Calculate areas of solv patches
                Debug.Listener.solvpatches(solvpatches);
                if(solvpatches.Count > 0 && hasGapBwPrevyCurry)
                {
                    double area_band = 0;
                    foreach(var patch in solvpatches)
                    {
                        double area_patch = Geometry.CalculateAreaBetweenSegments(patch.c1, patch.c2, curry, prevy);
                        area_band += area_patch;
                    }
                    sumofarea = sumofarea + area_band;

                    if(Debug._debug_area_lst_y != null)
                        Debug._debug_area_lst_y.Add((new Tuple<double, double>(prevy, curry), area_band));
                }
                // calculate the area
                //////////////////////////////////////////////////////////////////////////////////////////////////////


                //////////////////////////////////////////////////////////////////////////////////////////////////////
                // Handle event points
                //////////////////////////////////////////////////////////////////////////////////////////////////////
                // if entering point of the circle, add both of the segment (left & right)
                if (currep.type == EventPoint2D.Type.enter)
                {
                    // find its pair 
                    CircleSegment c_left  = currep.involved_enterL;
                    CircleSegment c_right = currep.involved_enterR;
                    HDebug.Assert(c_left.circle.id == c_right.circle.id);
                    HDebug.Assert(c_left.TryGetX(curry).x <= c_right.TryGetX(curry).x);
                    
                    // perform insert
                    circsegcomp.SetY( curry );
                    HandleEventCircleStart(eventqueue, tree, solvpatches, c_left, c_right, currep);
                }
                //////////////////////////////////////////////////////////////////////////////////////////////////////
                // Handle event points
                //////////////////////////////////////////////////////////////////////////////////////////////////////
                // if exiting point of the circle, delete both of the segment (left & right) 
                else if (currep.type == EventPoint2D.Type.exit)
                {
                    CircleSegment c_left  = currep.involved_exitL;
                    CircleSegment c_right = currep.involved_exitR;
                    HDebug.Assert(c_left.circle.id == c_right.circle.id);

                    if (c_left.IsSolventSegment() && c_right.IsSolventSegment()) 
                    {
                        if(stopAtEvtExitSolv)
                            return sumofarea;
                    }
                    
                    // perform delete
                    HandleEventCircleEnd(eventqueue, tree, solvpatches, c_left, c_right, currep);
                }
                //////////////////////////////////////////////////////////////////////////////////////////////////////
                // Handle event points
                //////////////////////////////////////////////////////////////////////////////////////////////////////
                // if intersecting (swapping) event point, 
                else
                {
                    HDebug.Assert(currep.type == EventPoint2D.Type.intersect);

                    CircleSegment c1 = currep.involved_intersectL; // segment that begins intersection
                    CircleSegment c2 = currep.involved_intersectR; // segment that ends   intersection
                    HDebug.Assert(currep.ValidateIntersect());
                    HDebug.Assert(CircleSegmentComparer.Compare(c1,c2,circsegcomp.GetY()) < 0);

                    // 1. pop top event points as long as its coordinate is same to the current event point
                    // 2. keep c1 as the left most segments and
                    //         c2 as the right most segments among segments in the top event points
                    EventPoint2D eventqueue_Max = eventqueue.Top;
                    if(eventqueue_Max.type == EventPoint2D.Type.intersect)
                    {
                        double treecs_y = circsegcomp.GetY();
                        while(    eventqueue_Max.type == EventPoint2D.Type.intersect 
                            && eventqueue_Max.x  == currep.x 
                            && eventqueue_Max.y  == currep.y)
                        {
                            currep = eventqueue.PopTop(); Debug._debug_currep = currep;

                            CircleSegment tc1 = currep.involved_intersectL;
                            CircleSegment tc2 = currep.involved_intersectR;
                            HDebug.Assert(currep.ValidateIntersect());
                            HDebug.Assert(CircleSegmentComparer.Compare(tc1,tc2,circsegcomp.GetY()) < 0);

                            if(CircleSegmentComparer.Compare(c1,tc1,treecs_y) > 0) c1 = tc1; // keep c1 smallest
                            if(CircleSegmentComparer.Compare(c2,tc2,treecs_y) < 0) c2 = tc2; // keep c2 largest

                            eventqueue_Max = eventqueue.Top;
                        }
                    }
                    HDebug.Assert(eventqueue_Max == eventqueue.Top);

                    HandleEventSegmentIntersect(eventqueue, tree, solvpatches, c1, c2, currep);
                }
                // finished event point handling
                //////////////////////////////////////////////////////////////////////////////////////////////////////
                // Handle event points
                //////////////////////////////////////////////////////////////////////////////////////////////////////
            }
                
            return sumofarea;
        }
    }
}
