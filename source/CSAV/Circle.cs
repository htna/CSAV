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
    public class Circle
    {
        //internal Point2 center;
        internal readonly ElementType   type     ;
        internal readonly int           id       ;
        public   readonly CircleSegment seg_left ;
        public   readonly CircleSegment seg_right;
        public            Point2        center    { [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return _center   ; } }    Point2 _center   ;
        public            double        center_x  { [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return _center.x ; } }
        public            double        center_y  { [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return _center.y ; } }
        public            double        radii     { [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return _radii    ; } }    double _radii    ;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Circle(ElementType type, int id)
        {
            this.type      = type;
            this.id        = id  ;
            this.seg_left  = new CircleSegment(this, CircleSegment.Type.left );
            this.seg_right = new CircleSegment(this, CircleSegment.Type.right);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InitData(double center_x, double center_y, double radii)
        {
            this._center.x = center_x;
            this._center.y = center_y;
            this._radii     = radii   ;
            seg_left .InitData();
            seg_right.InitData();
        }

        public Point2 TopPt    { [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return new Point2( center_x, center_y+radii ); } }
        public Point2 BottomPt { [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return new Point2( center_x, center_y-radii ); } }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsTopPt(double x, double y)
        {
            if(x != center_x      ) return false;
            if(y != center_y+radii) return false;
            return true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsTopPt(Point2 pt)
        {
            if(center_x       != pt.x) return false;
            if(center_y+radii != pt.y) return false;
            return true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsTopPt(Point2 pt, double toler)
        {
            if(Math.Abs(center_x       - pt.x) > toler) return false;
            if(Math.Abs(center_y+radii - pt.y) > toler) return false;
            return true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsBottomPt(Point2 pt, double toler)
        {
            if(Math.Abs(center_x       - pt.x) > toler) return false;
            if(Math.Abs(center_y-radii - pt.y) > toler) return false;
            return true;
        }

        public double BottomPtY { [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return center_y-radii; } }
        public double TopPtY    { [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return center_y+radii; } }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSolvent()
        {
            return (type == ElementType.solvent);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool InCircle(double x, double y)
        {
            double dx = (x-center_x);
            double dy = (y-center_y);
            double dist2 = dx*dx + dy*dy;
            double radi2 = radii * radii;
            return (dist2 < radi2);
        }

        public override string ToString()
        {
            //return center.ToString() + ", " + radii.ToString() + ", " + type.ToString();
            return (center_x, center_y).ToString() + ", " + radii.ToString() + ", " + type.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsEqual(Circle other)
        {
            if(center_x != other.center_x) return false;
            if(center_y != other.center_y) return false;
            if(radii    != other.radii   ) return false;
            if(type     != other.type    ) return false;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double CenterDistanceWith(Circle other)
        {
            return CenterDistanceWith(other.center_x, other.center_y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double CenterDistanceWith(double x, double y)
        {
            double dx = center_x - x;
            double dy = center_y - y;
            return Math.Sqrt(dx*dx + dy*dy);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Overlap(Circle a, Circle b)
        {
            double dx = (a.center_x - b.center_x);
            double dy = (a.center_y - b.center_y);
            double dist_center = Math.Sqrt( dx*dx + dy*dy );
            if(dist_center >= a.radii + b.radii)
                return false;
            return true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Enclosed(Circle a, Circle b)
        {                                                /// o *---------------------------------------| o.rad
            double dx = (a.center_x - b.center_x);       ///              i *--------------------------| i.rad
            double dy = (a.center_y - b.center_y);       ///   |------------|                            dist_center
            double ab_dist = Math.Sqrt( dx*dx + dy*dy ); ///    dist_center + i.rad <= o.rad
            if(a.radii >= ab_dist + b.radii) return +1; // a > b, a enclose b
            if(a.radii + ab_dist <= b.radii) return -1; // a < b, b enclose a
            return 0; // none of them enclose
        }
    }
}
