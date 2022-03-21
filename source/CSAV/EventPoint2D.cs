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
    class EventPoint2DComparer : IComparer<EventPoint2D>
    {
        // compares y-coordinate of each CEventPoint, used in sorting/initializing Queue
        // since it is used for Sets, it must check equality for all elements
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Compare(EventPoint2D pt1, EventPoint2D pt2)
        {
            return EventPoint2D.CompareTo(pt1,pt2);
        }
    }
    class EventPoint2DComparerReverse : IComparer<EventPoint2D>
    {
        // compares y-coordinate of each CEventPoint, used in sorting/initializing Queue
        // since it is used for Sets, it must check equality for all elements
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Compare(EventPoint2D pt1, EventPoint2D pt2)
        {
            return -1 * EventPoint2D.CompareTo(pt1,pt2);
        }
    }

    public class EventPoint2D
    {
#if DEBUG
        internal int _debug = -1;
#endif
        public enum Type
        {
            /// value is used in CompareTo to return in order of intersect(3) -> exit(2) -> enter(1)
            enter     = 1,
            exit      = 2,
            intersect = 3,
        }

        public  readonly Type   type;
        public  readonly double x;
        public  readonly double y;
        private readonly CircleSegment involved0; private int involved0_id { get { return involved0.id; } }
        private readonly CircleSegment involved1; private int involved1_id { get { return involved1.id; } }

        public CircleSegment involved_intersectL { [MethodImpl(MethodImplOptions.AggressiveInlining)] get { HDebug.Assert(type == Type.intersect); return involved0; } }
        public CircleSegment involved_intersectR { [MethodImpl(MethodImplOptions.AggressiveInlining)] get { HDebug.Assert(type == Type.intersect); return involved1; } }
        public CircleSegment involved_enterL     { [MethodImpl(MethodImplOptions.AggressiveInlining)] get { HDebug.Assert(type == Type.enter    ); return involved0; } }
        public CircleSegment involved_enterR     { [MethodImpl(MethodImplOptions.AggressiveInlining)] get { HDebug.Assert(type == Type.enter    ); return involved1; } }
        public CircleSegment involved_exitL      { [MethodImpl(MethodImplOptions.AggressiveInlining)] get { HDebug.Assert(type == Type.exit     ); return involved0; } }
        public CircleSegment involved_exitR      { [MethodImpl(MethodImplOptions.AggressiveInlining)] get { HDebug.Assert(type == Type.exit     ); return involved1; } }

        public EventPoint2D
            ( double x
            , double y
            , CircleSegment involved0
            , CircleSegment involved1
            , Type type
            )
        {
            HDebug.AssertIf(type == Type.enter, involved0.type == CircleSegment.Type.left && involved1.type == CircleSegment.Type.right);
            HDebug.AssertIf(type == Type.exit , involved0.type == CircleSegment.Type.left && involved1.type == CircleSegment.Type.right);

            this.x = x;
            this.y = y;

            this.involved0 = involved0;
            this.involved1 = involved1;

            this.type      = type     ;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ValidateIntersect()
        {
            return ValidateIntersect
            ( involved_intersectL
            , involved_intersectR
            , x, y
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ValidateIntersect
            ( CircleSegment intersectL
            , CircleSegment intersectR
            , double coord0, double coord1
            )
        {
            if(intersectL.IsOnSegment(coord0, coord1) == false) return false;
            if(intersectR.IsOnSegment(coord0, coord1) == false) return false;
            return true;
        }

        public override string ToString()
        {
            string str = (x, y).ToString() + ", " + type.ToString() + ", " + (involved0,involved1).ToString();
#if DEBUG
            str += ", _debug:" + _debug.ToString();
#endif
            return str;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            return involved0_id.GetHashCode()
                    + involved1_id.GetHashCode()
                    + type        .GetHashCode();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CompareTo(EventPoint2D a, EventPoint2D b)
        {
            int comp_y = a.y.CompareTo(b.y);
            if (comp_y != 0)
                return comp_y;

            int comp_type = ((int)a.type).CompareTo((int)b.type);
            if(comp_type != 0)
                return comp_type;
            //  /// value is used in CompareTo to return in order of intersect(3) -> exit(2) -> enter(1)
            //  switch((type,other.type))
            //  {
            //      case (Type.enter    ,Type.enter    ): break    ; // (1,1) => (1 = 1) =>    0   =>  0
            //      case (Type.enter    ,Type.exit     ): return -1; // (1,2) => (1 < 2) => -1     => -1
            //      case (Type.enter    ,Type.intersect): return -1; // (1,3) => (1 < 3) => -1     => -1
            //      case (Type.exit     ,Type.enter    ): return +1; // (2,1) => (2 > 1) =>     +1 => +1
            //      case (Type.exit     ,Type.exit     ): break    ; // (2,2) => (2 = 2) =>    0   =>  0
            //      case (Type.exit     ,Type.intersect): return -1; // (2,3) => (2 < 3) => -1     => -1
            //      case (Type.intersect,Type.enter    ): return +1; // (3,1) => (3 > 1) =>     +1 => +1
            //      case (Type.intersect,Type.exit     ): return +1; // (3,2) => (3 > 2) =>     +1 => +1
            //      case (Type.intersect,Type.intersect): break    ; // (3,3) => (3 = 3) =>    0   =>  0
            //      default: throw new Exception();
            //  }

            // compare x
            int comp_x = a.x.CompareTo(b.x);
            if (comp_x != 0)
                return comp_x;

            int comp_inv0 = a.involved0.id.CompareTo(b.involved0.id);
            if(comp_inv0 != 0)
                return comp_inv0;

            int comp_inv1 = a.involved1.id.CompareTo(b.involved1.id);
            return comp_inv1;
        }
    }
}
