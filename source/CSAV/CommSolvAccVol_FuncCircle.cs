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
    public partial class CommSolvAccVol
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Circle NewCircle(int id, bool isSolvent)
        {
            int         circleid = (isSolvent) ? -(id+1) : (id+1);
            ElementType ctype    = (isSolvent == false) ? ElementType.atom : ElementType.solvent;
            Circle      circle   = new Circle ( ctype, circleid );
            return circle;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool InitCircle(IProtStruct prot, Circle circle, double z, double delt, int dxyi)
        {
            HDebug.Assert(circle.id != 0);
            bool isSolvent = (circle.type == ElementType.solvent);
            int  id        = Math.Abs(circle.id) - 1;

            Vector coord  = prot.atom_coord(id);
            double atmrad = prot.atom_rad  (id);
            double slvrad = prot.solv_rad  (id);

            double sradii = (isSolvent == false) ? (atmrad) : (slvrad);

            double dz = Math.Abs(z - coord[2]);
            if (dz > sradii)
                return false;

            if(sradii < dz)
                return false;

            double cradii = Math.Sqrt(sradii * sradii - dz * dz);
            // if circle is not in range of csav
            if (cradii < delt)
                return false;

            // tilt (x,y) location slightly to avoid degenerate cases such as
            // two circle's x coordinates are exactly same
            // since all coordinates will be divided by 0.001
            double dxy = dxyi*0.0000000001;
            circle.InitData( coord[0]+dxy, coord[1]+dxy, cradii );
            return true;
        }
    }
}
