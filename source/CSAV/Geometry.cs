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
    public static partial class Geometry
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (char lr_0, char lr_1) DetermineLeftRight(double slope_0, double slope_1)
        {
            ///  left  right                           left right           left  right                      left            right              \\\
            /// -1\   /+1                             +5/   /+3             -3\   \-5                        /               \                  \\\
            ///    \ /                                 / /                       \ \                        /  right     left \                 \\\
            ///     *                                 *                             *                   ---*---             ---*---             \\\
            ///    / \                             / /                               \ \                  /                     \               \\\
            ///   /   \                         /   /                                 \   \              /                       \              \\\
            ///                                                                                                                                 \\\
            /// different sign                  positive signs              negative signe              x-axis              x-axis              \\\
            /// left : negative slope (-1)      left : large slope (+5)     left : large slope (-3)     left : positive     left : 0            \\\
            /// right: positive slope (+1)      right: small slope (+3)     right: small slope (-5)     right: 0            right: negative     \\\
            if(slope_0 == 0 || slope_1 == 0)
            {
                HDebug.Assert(false);
                if(slope_0 > 0) return ('l','r');
                if(slope_0 < 0) return ('r','l');
                if(slope_1 > 0) return ('r','l');
                if(slope_1 < 0) return ('l','r');
                throw new Exception();
            }
            else if(slope_0 * slope_1 < 0)
            {
                if(slope_0 > 0 && slope_1 < 0) return ('r','l');
                if(slope_0 < 0 && slope_1 > 0) return ('l','r');
                throw new Exception();
            }
            else
            {
                if( slope_0 > slope_1 ) return ('l','r');
                else                    return ('r','l');
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (double a, double b) FindCircleTangent(double x0, double y0, double x, double y)
        {
            ///                   \
            ///                  (x,y)
            /// (x0,y0)            |
            /// 
            /// tangent line functions       : y = a x + b
            /// line passing (x0,y0) - (x,y) : Y = (y-y0)/(x-x0) * ( X - x ) + y
            /// perpendicular slope          : -(x-x0)/(y-y0)
            /// tangent line at (x,y)        : Y = -(x-x0)/(y-y0) * ( X - x ) + y
            ///                                  = -(x-x0)/(y-y0) * X   +   y + x*(x-x0)/(y-y0)
            ///                                  = a X                  +   b
            ///                                a = -(x-x0)/(y-y0)
            ///                                b = y + x*(x-x0)/(y-y0) = y - x * -(x-x0)/(y-y0) = y - x * a
            ///                                b = y - x * a
            ///                                y = a x + b
            double a = -(x-x0)/(y-y0);
            double b = y - x * a;
            return (a,b);
        }
            
        /// finds intersections of circles c1 and c2
        /// returns list if all intersetions (max 2, min 1)
        /// this function does not take account whether it is left or right part of the segment
        /// this can be problematic; but it is resolved by having both segments always being handled at the same time
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ((Point2 pt, CircleSegment.Type seg1, CircleSegment.Type seg2) intsec_first, (Point2 pt, CircleSegment.Type seg1, CircleSegment.Type seg2) intsec_second)?
            FindIntersectionCircle(Circle c1, Circle c2)
        {
            HDebug.Assert(c1.id != c2.id);

            double rad1 = c1.radii;
            double rad2 = c2.radii;

            double dist = c1.CenterDistanceWith(c2);
            if(dist == 0) return null;

            if(dist >=          rad1+rad2 ) return null; // two circle are too far 
            if(dist <= Math.Abs(rad1-rad2)) return null; // one circle is enclosed to another

            /// https://mathworld.wolfram.com/Circle-CircleIntersection.html
            double halfd = (rad1 * rad1 - rad2 * rad2 + dist * dist) / (2 * dist);
            double height2 = rad1 * rad1 - halfd * halfd;
            if(height2 < 0) return null;
            double height = Math.Sqrt(height2);

            {
                double c1x = c1.center_x;
                double c1y = c1.center_y;
                double c2x = c2.center_x;
                double c2y = c2.center_y;

                (Point2 pt, CircleSegment.Type seg1, CircleSegment.Type seg2) p1 = default;
                (Point2 pt, CircleSegment.Type seg1, CircleSegment.Type seg2) p2 = default;
                double halfd_dist  = halfd  / dist;
                double height_dist = height / dist;
                double c2x_c1x = (c2x - c1x);
                double c2y_c1y = (c2y - c1y);
                p1.pt.x = (halfd_dist * c2x_c1x + height_dist * c2y_c1y + c1x); // p1.pt.x = (halfd / dist * (c2x - c1x) + height / dist * (c2y - c1y) + c1x);
                p1.pt.y = (halfd_dist * c2y_c1y - height_dist * c2x_c1x + c1y); // p1.pt.y = (halfd / dist * (c2y - c1y) - height / dist * (c2x - c1x) + c1y);
                p2.pt.x = (halfd_dist * c2x_c1x - height_dist * c2y_c1y + c1x); // p2.pt.x = (halfd / dist * (c2x - c1x) - height / dist * (c2y - c1y) + c1x);
                p2.pt.y = (halfd_dist * c2y_c1y + height_dist * c2x_c1x + c1y); // p2.pt.y = (halfd / dist * (c2y - c1y) + height / dist * (c2x - c1x) + c1y);
                p1.seg1 = (p1.pt.x > c1x) ? CircleSegment.Type.right : CircleSegment.Type.left;
                p1.seg2 = (p1.pt.x > c2x) ? CircleSegment.Type.right : CircleSegment.Type.left;
                p2.seg1 = (p2.pt.x > c1x) ? CircleSegment.Type.right : CircleSegment.Type.left;
                p2.seg2 = (p2.pt.x > c2x) ? CircleSegment.Type.right : CircleSegment.Type.left;
                HDebug.Assert(Math.Abs(c1.CenterDistanceWith(p1.pt.x, p1.pt.y) - rad1) < 0.000001);
                HDebug.Assert(Math.Abs(c1.CenterDistanceWith(p2.pt.x, p2.pt.y) - rad1) < 0.000001);
                HDebug.Assert(Math.Abs(c2.CenterDistanceWith(p1.pt.x, p1.pt.y) - rad2) < 0.000001);
                HDebug.Assert(Math.Abs(c2.CenterDistanceWith(p2.pt.x, p2.pt.y) - rad2) < 0.000001);

                if     (p1.pt.y < p2.pt.y)     return (p2, p1);
                else if(p1.pt.y > p2.pt.y)     return (p1, p2);
                else // if firsty == secondy 
                {
                    if     (p1.pt.x < p2.pt.x) return (p1, p2);
                    else if(p1.pt.x > p2.pt.x) return (p2, p1);
                    else               return null;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double CalculateAreaBetweenSegments(CircleSegment c1, CircleSegment c2, double y0, double y1)
        {
            /// area(c1x, c1y, c1r, c2x, c2y, c2r, y0, y1) = +/- area(c1r, y0-c1y, y1-c1y)   --- A
            ///                                              +/- (y1-y0) * (c2x-c1x)         --- B
            ///                                              +/- area(c2r, y0-c2y, y1-c2y)   --- C
            ///     
            /// area(r, y0, y1) = area(r, y1) - area(r, y0)
            /// area(r, y) = 1/2 [y Sqrt[r^2 - y^2] + r^2 ArcTan[y/Sqrt[r^2 - y^2]]
                
            double c1x = c1.circle.center_x; double c1y = c1.circle.center_y; double c1r = c1.circle.radii; CircleSegment.Type c1type = c1.type;
            double c2x = c2.circle.center_x; double c2y = c2.circle.center_y; double c2r = c2.circle.radii; CircleSegment.Type c2type = c2.type;
                
            double area_A = CalculateAreaOfSegment(c1r, y1-c1y) - CalculateAreaOfSegment(c1r, y0-c1y);
            double area_B = (y1 - y0) * (c2x - c1x);
            double area_C = CalculateAreaOfSegment(c2r, y1-c2y) - CalculateAreaOfSegment(c2r, y0-c2y);

            if     (c1type == CircleSegment.Type.left  && c2type == CircleSegment.Type.right)   return  area_A + area_B + area_C;
            else if(c1type == CircleSegment.Type.left  && c2type == CircleSegment.Type.left )   return  area_A + area_B - area_C;
            else if(c1type == CircleSegment.Type.right && c2type == CircleSegment.Type.right)   return -area_A + area_B + area_C;
            else if(c1type == CircleSegment.Type.right && c2type == CircleSegment.Type.left )   return -area_A + area_B - area_C;
                
            HDebug.Assert(false); // error
            return 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double CalculateAreaOfSegment(double r, double y)
        {
            /// area(r, y) = 1/2 [y Sqrt[r^2 - y^2] + r^2 ArcTan[y/Sqrt[r^2 - y^2]]
                
            if(y >  r) y =  r;
            if(y < -r) y = -r;

            double r2 = r * r;
            double sqrt_r2_y2 = Math.Sqrt(r2 - y*y);

            double atan_y_sqrt_r2_y2;
            if     (y >=  r) atan_y_sqrt_r2_y2 =  Math.PI/2;
            else if(y <= -r) atan_y_sqrt_r2_y2 = -Math.PI/2;
            else             atan_y_sqrt_r2_y2 = Math.Atan(y/sqrt_r2_y2);

            double area = 0.5 * (y * sqrt_r2_y2 + r2 * atan_y_sqrt_r2_y2); 
            return area;
        }
    }
}
