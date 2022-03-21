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
    public class CircleSegment
    {
        public enum Type { left, right }

        internal readonly Type   type    ;
        public   readonly Circle circle  ;
        internal readonly int    id      ;
        internal          int    patchval;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CircleSegment(Circle circle, Type type)
        {
            this.circle   = circle;
            this.type     = type  ;
            this.id       = GetId(circle, type);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InitData()
        {
            this.patchval = int.MinValue;
            this.cached = (double.MaxValue, double.MaxValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetId(Circle circle, Type type)
        {
            if(type == Type.left) return (circle.id*10);
            else                  return (circle.id*10 + 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSolventSegment()
        {
            return (circle.type == ElementType.solvent);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsAtomSegment()
        {
            return (circle.type == ElementType.atom);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (double x, bool succ) TryGetX(double y)
        {
            double y_centery = y - circle.center_y;
            double radii = circle.radii;
            double temp = radii * radii - y_centery * y_centery;
            if (temp < 0)
            {
                const double tolerance = -0.0000001;
                if(tolerance < temp)
                {
                    return (circle.center_x, true);
                }
                else
                {
                    HDebug.Assert(false);
                    return (circle.center_x, false);
                }
            }
            double width = Math.Sqrt(temp);

            if (type == Type.left)
            {
                return (circle.center_x - width, true);
            }
            else
            {
                return (circle.center_x + width, true);
            }
        }

        (double x, double y) cached = (double.MaxValue, double.MaxValue);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double GetX(double y)
        {
            if(y == cached.y)
                return cached.x;

            double dy = y - circle.center_y;
            double radii = circle.radii;
            double dx2 = radii * radii - dy * dy;
            if (dx2 < 0)
            {
                const double tolerance = -0.0000001;
                if(tolerance < dx2)
                {
                    cached = (circle.center_x,y);
                    return    circle.center_x;
                }
                else
                {
                    HDebug.Assert(false);
                    cached = (circle.center_x,y);
                    return    circle.center_x;
                }
            }
            else
            {
                double dx = Math.Sqrt(dx2);
                if (type == Type.left)
                {
                    cached = ((circle.center_x - dx),y);
                    return    (circle.center_x - dx);
                }
                else
                {
                    cached = ((circle.center_x + dx),y);
                    return    (circle.center_x + dx);
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double GetTan(double y)
        {
            double x = GetX(y);
            return GetTan(x, y);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double GetTan(double x, double y)
        {
            HDebug.Assert(GetX(y) == x);
            var a_b= Geometry.FindCircleTangent(circle.center_x, circle.center_y, x, y);
            return a_b.a;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsOnSegment(double x, double y)
        {
            return IsOnSegment(new Point2(x, y));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsOnSegment(Point2 pt)
        {
            double cx = circle.center_x;
            double cy = circle.center_y;
            double r  = circle.radii;
            if(pt.x==cx)
            {
                if(pt.y == cy+r) return true;  // top is closed
                if(pt.y == cy-r) return false; // bottom if open
                return false;
            }
            // check boundary
            if(pt.y > cy+r) return false;
            if(pt.y < cy-r) return false;
            if(type == Type.left  && pt.x > cx) return false;
            if(type == Type.right && pt.x < cx) return false;
            // check distance
            double dx = pt.x - cx;
            double dy = pt.y - cy;
            double err = Math.Sqrt(dx*dx + dy*dy) - r;
            const double toler = 0.000001;
            if(Math.Abs(err) > toler)
                return false;
            return true;
        }

        public override string ToString()
        {
            //return (center.GetHashCode() + radii.GetHashCode() + type.GetHashCode()).ToString();
            return id.ToString();
            //return id.ToString() + "-" + circle.ToString() + "," + type.ToString() + "," + patchval.ToString();
        }

        //  [MethodImpl(MethodImplOptions.AggressiveInlining)]
        //  public static int Compare((CircleSegment seg, double x) a, (CircleSegment seg, double x) b)
        //  {
        //      if(a.x == b.x)
        //      {
        //          if(a.seg.circle.id == b.seg.circle.id)
        //          {
        //              if(a.seg.type == Type.left)
        //                  return -1;
        //              else
        //                  return 1;
        //          }
        //          return  0;
        //      }
        //      if(a.x <  b.x) return -1;
        //      return  1;
        //  }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetUpdatedPatchval(CircleSegment prev)
        {
            int prev_patchval = (prev == null) ? 0 : prev.patchval;

            bool cs_solv = IsSolventSegment();
            switch((cs_solv,type))
            {
                case ( true,Type.left ): return prev_patchval + 1;
                case ( true,Type.right): return prev_patchval - 1;
                case (false,Type.left ): return prev_patchval - 1;
                case (false,Type.right): return prev_patchval + 1;
                default:
                    throw new Exception();
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UpdatePatchval(CircleSegment prev)
        {
            patchval = GetUpdatedPatchval(prev);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ValidatePatchval(CircleSegment prev)
        {
            return (patchval == GetUpdatedPatchval(prev));
        }
    }
}
