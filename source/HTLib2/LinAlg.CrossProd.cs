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
using System.Text;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace CSAV.HTLib2
{
    public static partial class LinAlg
	{
        public static double DotProd(Vector l, Vector r)
        {
            return VtV(l, r);
        }
        public static double VtV(Vector l, Vector r)
        {
            HDebug.Assert(l.Size == r.Size);
            int size = l.Size;
            double result = 0;
            for(int i=0; i < size; i++)
                result += l[i] * r[i];
            return result;
        }
        public static Vector CrossProd(Vector l, Vector r)
        {
            HDebug.Assert(l.Size == r.Size);
            if(l.Size == 3) return CrossProd3(l, r);
            HDebug.Assert(false);
            return null;
        }
        public static bool CrossProd3_SelfTest = HDebug.IsDebuggerAttached;
        public static Vector CrossProd3(Vector v1, Vector v2)
        {
            Vector cross = new double[3];
            CrossProd3(v1, v2, cross);
            return cross;
        }
        public static void CrossProd3(Vector v1, Vector v2, Vector cross)
        {
            if(CrossProd3_SelfTest)
            {
                CrossProd3_SelfTest = false;
                Vector tv1 = new double[] { 1, 2, 3 };
                Vector tv2 = new double[] { 4, 5, 6 };
                Vector tv  = CrossProd3(tv1, tv2);
                HDebug.AssertTolerance(0, tv - new Vector(-3, 6, -3));
            }
            // http://en.wikipedia.org/wiki/Cross_product
            HDebug.Assert(v1.Size == 3);
            HDebug.Assert(v2.Size == 3);
            //Vector cross = new double[3];
            HDebug.Assert(cross.Size == 3);
            cross[0] = v1[1]*v2[2] - v1[2]*v2[1];
            cross[1] = v1[2]*v2[0] - v1[0]*v2[2];
            cross[2] = v1[0]*v2[1] - v1[1]*v2[0];

            //HDebug.Assert(left.Size == 3, right.Size == 3);
            //return new double[]{
            //                left[1]*right[2] - left[2]*right[1],
            //                left[2]*right[0] - left[0]*right[2],
            //                left[0]*right[1] - left[1]*right[0]
            //                };
        }
    }
}
