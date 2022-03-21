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
    public static partial class Debug
    {
#if DEBUG
        static public int                                      _debug = 0;
        static public List<Circle>                             _debug_circles = null;
        static public EventPoint2D                             _debug_currep = null;
        static public EventQueue                               _debug_eventqueue = null;
        static public LinkedAvlTree<CircleSegment>             _debug_tree = null;
        static public List<(Tuple<double, double>, double)>    _debug_area_lst_y = null;
        static public string                                   _debug_pdbid            = null;
        static public int                                      _debug_ia               = -1;
        static public int                                      _debug_ib               = -1;
        static public double                                   _debug_z                = double.NaN;
        static public int                                      _debug_iaib_numatoms    = -1;
        static public int                                      _debug_iaibz_numcircles = -1;
#else
        static public int                                      _debug                  { [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return int.MinValue; } [MethodImpl(MethodImplOptions.AggressiveInlining)] set { } }
        static public List<Circle>                             _debug_circles          { [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return null;         } [MethodImpl(MethodImplOptions.AggressiveInlining)] set { } }
        static public EventPoint2D                             _debug_currep           { [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return null;         } [MethodImpl(MethodImplOptions.AggressiveInlining)] set { } }
        static public EventQueue                               _debug_eventqueue       { [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return null;         } [MethodImpl(MethodImplOptions.AggressiveInlining)] set { } }
        static public LinkedAvlTree<CircleSegment>             _debug_tree             { [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return null;         } [MethodImpl(MethodImplOptions.AggressiveInlining)] set { } }
        static public List<(Tuple<double, double>, double)>    _debug_area_lst_y       { [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return null;         } [MethodImpl(MethodImplOptions.AggressiveInlining)] set { } }
        static public string                                   _debug_pdbid            { [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return null;         } [MethodImpl(MethodImplOptions.AggressiveInlining)] set { } }
        static public int                                      _debug_ia               { [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return int.MinValue; } [MethodImpl(MethodImplOptions.AggressiveInlining)] set { } }
        static public int                                      _debug_ib               { [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return int.MinValue; } [MethodImpl(MethodImplOptions.AggressiveInlining)] set { } }
        static public double                                   _debug_z                { [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return double.NaN;   } [MethodImpl(MethodImplOptions.AggressiveInlining)] set { } }
        static public int                                      _debug_iaib_numatoms    { [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return int.MinValue; } [MethodImpl(MethodImplOptions.AggressiveInlining)] set { } }
        static public int                                      _debug_iaibz_numcircles { [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return int.MinValue; } [MethodImpl(MethodImplOptions.AggressiveInlining)] set { } }
#endif
    }
}
