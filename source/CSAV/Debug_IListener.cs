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
using System.Runtime.CompilerServices;

namespace CSAV
{
    using Stopwatch = System.Diagnostics.Stopwatch;

    public static partial class Debug
    {
        public static partial class Listener
        {
            public interface ISweepData
            {
                int     num_circles                     { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; } // include atom circle + solv circle
                int[]   hstgrm_num_circles_intree       { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; } // histogram of [number of circles (segments/2) in tree]
                int     num_evt_enter                   { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
                int     num_evt_exit                    { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
                int     num_evt_intersect               { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
                int[]   hstgrm_num_solvpatches          { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; } // [number of solvent patches in a line         ] -> count in a plane
                int[]   hstgrm_num_intersect_segm       { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; } // [number of intersecting segments of a segment] -> count in a plane
                int[]   hstgrm_num_intersect_circ       { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; } // [number of intersecting circles  of a circle ] -> count in a plane
                int[]   hstgrm_num_swapsegm_event       { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; } // [number of swapping segments     in a event  ] -> count in a plane

                int[]   timesweepplane_elapsed          { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
                int[]   timesweepplane_val2             { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
                int[]   timesweepplane_val3             { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
                int     timesweepplane_elapsed_ticks    { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
                double  timesweepplane_elapsed_millisec { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
            }
            public interface ICsavData
            {
                int                                                            atomi                { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
                int                                                            atomj                { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
                int                                                            num_atoms            { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; } // num atoms in csav (first two are atoms for solvent sphere)
                IEnumerable<ISweepData>                                        list_sweepdata       { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
                double                                                         volume               { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
                double                                                         millisec             { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; } // ElapsedMilliseconds
                (double ia_ib, double iasurf_protsurf, double ibsurf_protsurf) distinfo             { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
                double                                                         info_volume_voxel    { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
                double                                                         info_volume_min      { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
                double                                                         info_volume_max      { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
                double                                                         info_time_numplanes  { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
                double                                                         info_time_avgcircles { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
                double                                                         info_time_extra      { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
            }
            public interface IData: CSAV.HTLib2.IBinarySerializable
            {
                (double,double)        filtercond   { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
                string                 pdbid        { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
                double                 solvgap      { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
                double                 delt         { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
                IEnumerable<ICsavData> csavs        { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

                double[]               atomrad      { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; } // show number of atoms, and distribution of radius
                int                    num_atoms    { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

                double                 total_volume { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
            }
        }
    }
}
