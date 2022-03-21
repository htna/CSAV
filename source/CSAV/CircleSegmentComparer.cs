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
    public class CircleSegmentComparer
    {
        private double y     = double.PositiveInfinity;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetY(double value)
        { 
            HDebug.Assert(y >= value);
            y = value;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double GetY()
        {
            return y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Compare(CircleSegment cs1, CircleSegment cs2, double y)
        {
            if(cs1.id == cs2.id)
                return 0;

            if(cs1.circle.id == cs2.circle.id)
            {
                if(cs1.type == CircleSegment.Type.left)
                {
                    HDebug.Assert(cs1.type == CircleSegment.Type.left );
                    HDebug.Assert(cs2.type == CircleSegment.Type.right);
                    return -1; // left < right  =>  comp(left,right) < 0
                }
                else
                {
                    HDebug.Assert(cs2.type == CircleSegment.Type.left );
                    HDebug.Assert(cs1.type == CircleSegment.Type.right);
                    return +1; // right > left =>  comp(right,left) > 0
                }
            }

            double x1 = cs1.GetX(y);
            double x2 = cs2.GetX(y);

            if(x1 == x2)
            {
                // because cs1.id != cs2.id
                double t1 = cs1.GetTan(x1, y);
                double t2 = cs2.GetTan(x1, y);
                var lr = Geometry.DetermineLeftRight(t1,t2);
                if(lr.lr_0 == 'l')
                {
                    HDebug.Assert(lr.lr_0 == 'l' && lr.lr_1 == 'r');
                    return -1;
                }
                else
                {
                    HDebug.Assert(lr.lr_0 == 'r' && lr.lr_1 == 'l');
                    return 1;
                }
            }

            return x1.CompareTo(x2);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CompareValidate(CircleSegment cs1, CircleSegment cs2, double y)
        {
            Circle circle1 = cs1.circle;
            Circle circle2 = cs2.circle;
            int type1 = (int)cs1.type;
            int type2 = (int)cs2.type;

            if (circle1.id == circle2.id)
            {
                if (type1 != type2)
                    return type1.CompareTo(type2);
                else if (cs1.circle.radii == cs2.circle.radii)
                    return 0;
            }

            double x1 = cs1.GetX(y);
            double x2 = cs2.GetX(y);

            return x1.CompareTo(x2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Compare(CircleSegment cs1, CircleSegment cs2)
        {
            return Compare(cs1, cs2, this.y);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareValidate(CircleSegment cs1, CircleSegment cs2)
        {
            return CompareValidate(cs1, cs2, this.y);
        }
    }
}
