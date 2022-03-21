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

//#define COLLECT_DATA
//#define COLLECT_TIME
//#define COLLECT_VOXEL_VALIDATE
//#define COLLECT_TIME_SWEEPPLANE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using CSAV.HTLib2;

namespace CSAV
{
    using Stopwatch = System.Diagnostics.Stopwatch;

    public static partial class Debug
    {
        public static IEnumerable<int> HEnumNumCircles(this IEnumerable<Listener.ISweepData> list)
        {
            foreach(var item in list)
                yield return item.num_circles;
        }
        public static IEnumerable<int> HEnumNumEvents(this IEnumerable<Listener.ISweepData> list)
        {
            foreach(var item in list)
                yield return (item.num_evt_enter + item.num_evt_exit + item.num_evt_intersect);
        }
        public static IEnumerable<int> HEnumNumEventIntersects(this IEnumerable<Listener.ISweepData> list)
        {
            foreach(var item in list)
                yield return item.num_evt_intersect;
        }
        public static IEnumerable<double> HEnumAvgTreeSize(this IEnumerable<Listener.ISweepData> list)
        {
            foreach(var item in list)
            {
                yield return item.hstgrm_num_circles_intree.Average();
            }
        }
        public static IEnumerable<double> HListMillisec(this IEnumerable<Listener.ICsavData> csavs)
        {
            foreach(var csav in csavs)
                yield return csav.millisec;
        }
        public static IEnumerable<double> HListNumAtoms(this IEnumerable<Listener.ICsavData> csavs)
        {
            foreach(var csav in csavs)
                yield return csav.num_atoms;
        }

        public static partial class Listener
        {
            /// solv radius, delt
            /// radius 3.5
            /// hist of num atoms                         in CSAV           ,   max of num atoms        in CSAV
            /// hist of num circles                       in sweep plane    ,   max of num circles      in sweep plane
            /// hist of num (ins, del, swap) event points in sweep plane    ,   max of num event points in sweep plane
            /// hist of num patches                       in sweep line     ,   max of num patches      in sweep line 
            /// hist of num circle segments               in sweep line     ,   max of num segments     in sweep line 
            /// 
            const int ver = 5;
            // ver 0 : initial
            // ver 1 : add hstgrm_num_intersect_segm
            // ver 2 : add hstgrm_num_intersect_circ
            // ver 3 : fix serialization bug (not recording CsavData.atomi, CsavData.atomj)
            //         remove atomi_atomj_volume_millisec
            // ver 4 : atomi_atomj_csav -> csavs
            // ver 5 : add hstgrm_num_swapsegm_event
            static int readver = -1;
            [Serializable]
            public class SweepData : ISweepData, IBinarySerializable
            {
                int     ISweepData.num_circles                     { get { return num_circles                    ; } } // include atom circle + solv circle
                int[]   ISweepData.hstgrm_num_circles_intree       { get { return hstgrm_num_circles_intree      ; } } // histogram of [number of circles (segments/2) in tree]
                int     ISweepData.num_evt_enter                   { get { return num_evt_enter                  ; } }
                int     ISweepData.num_evt_exit                    { get { return num_evt_exit                   ; } }
                int     ISweepData.num_evt_intersect               { get { return num_evt_intersect              ; } }
                int[]   ISweepData.hstgrm_num_solvpatches          { get { return hstgrm_num_solvpatches         ; } } // [number of solvent patches in a line         ] -> count in a plane
                int[]   ISweepData.hstgrm_num_intersect_segm       { get { return hstgrm_num_intersect_segm      ; } } // [number of intersecting segments of a segment] -> count in a plane
                int[]   ISweepData.hstgrm_num_intersect_circ       { get { return hstgrm_num_intersect_circ      ; } } // [number of intersecting circles  of a circle ] -> count in a plane
                int[]   ISweepData.hstgrm_num_swapsegm_event       { get { return hstgrm_num_swapsegm_event      ; } } // [number of swapping segments     in a event  ] -> count in a plane
                int[]   ISweepData.timesweepplane_elapsed          { get { return timesweepplane_elapsed         ; } }
                int[]   ISweepData.timesweepplane_val2             { get { return timesweepplane_val2            ; } }
                int[]   ISweepData.timesweepplane_val3             { get { return timesweepplane_val3            ; } }
                int     ISweepData.timesweepplane_elapsed_ticks    { get { return timesweepplane_elapsed_ticks   ; } }
                double  ISweepData.timesweepplane_elapsed_millisec { get { return timesweepplane_elapsed_millisec; } }

                public int   num_circles;       // include atom circle + solv circle
                public int[] hstgrm_num_circles_intree;   // histogram of [number of circles (segments/2) in tree]
                public int   num_evt_enter    ;
                public int   num_evt_exit     ;
                public int   num_evt_intersect;
                public int[] hstgrm_num_solvpatches   ;   // [number of solvent patches in a line         ] -> count in a plane
                public int[] hstgrm_num_intersect_segm;   // [number of intersecting segments of a segment] -> count in a plane
                public int[] hstgrm_num_intersect_circ;   // [number of intersecting circles  of a circle ] -> count in a plane
                public int[] hstgrm_num_swapsegm_event;   // [number of swapping segments     in a event  ] -> count in a plane

                public int[] timesweepplane_elapsed   { get { return hstgrm_num_solvpatches   ; } set { hstgrm_num_solvpatches    = value; } }
                public int[] timesweepplane_val2      { get { return hstgrm_num_intersect_segm; } set { hstgrm_num_intersect_segm = value; } }
                public int[] timesweepplane_val3      { get { return hstgrm_num_intersect_circ; } set { hstgrm_num_intersect_circ = value; } }
                public int    timesweepplane_elapsed_ticks    { get { HDebug.Assert(hstgrm_num_solvpatches.Length == 2); return hstgrm_num_solvpatches[0]; } }
                public double timesweepplane_elapsed_millisec { get { return TimeSpan.FromTicks(timesweepplane_elapsed_ticks).TotalMilliseconds; } }

                public void Serialize(System.IO.BinaryWriter writer)
                {
                    writer.HWrite(num_circles     );
                    writer.HWrite(hstgrm_num_circles_intree);
                    writer.HWrite(num_evt_enter    );
                    writer.HWrite(num_evt_exit     );
                    writer.HWrite(num_evt_intersect);
                    writer.HWrite(hstgrm_num_solvpatches);
                    writer.HWrite(hstgrm_num_intersect_segm);
                    writer.HWrite(hstgrm_num_intersect_circ);
                    writer.HWrite(hstgrm_num_swapsegm_event);
                }
                public void Deserialize(System.IO.BinaryReader reader)
                {
                    reader.HRead(out num_circles     );
                    reader.HRead(out hstgrm_num_circles_intree);
                    reader.HRead(out num_evt_enter    );
                    reader.HRead(out num_evt_exit     );
                    reader.HRead(out num_evt_intersect);
                    reader.HRead(out hstgrm_num_solvpatches);
                    reader.HRead(out hstgrm_num_intersect_segm);
                    reader.HRead(out hstgrm_num_intersect_circ);
        if(ver >= 5) reader.HRead(out hstgrm_num_swapsegm_event);
                }
                public override string ToString()
                {
                    string str 
                        = $"circs:{num_circles}"
                        + $", treesizes:{hstgrm_num_circles_intree.Length}"
                        + $", itx:{num_evt_intersect}, ent:{num_evt_enter}, ext:{num_evt_exit}";
                    return str;
                }
            }
            [Serializable]
            public class CsavData : ICsavData, IBinarySerializable
            {
                int                                                            ICsavData.atomi                { get { return atomi               ; } }
                int                                                            ICsavData.atomj                { get { return atomj               ; } }
                int                                                            ICsavData.num_atoms            { get { return num_atoms           ; } }
                IEnumerable<ISweepData>                                        ICsavData.list_sweepdata       { get { return list_sweepdata      ; } }
                double                                                         ICsavData.volume               { get { return volume              ; } }
                double                                                         ICsavData.millisec             { get { return millisec            ; } }
                (double ia_ib, double iasurf_protsurf, double ibsurf_protsurf) ICsavData.distinfo             { get { return distinfo            ; } }
                double                                                         ICsavData.info_volume_voxel    { get { return info_volume_voxel   ; } }
                double                                                         ICsavData.info_volume_min      { get { return info_volume_min     ; } }
                double                                                         ICsavData.info_volume_max      { get { return info_volume_max     ; } }
                double                                                         ICsavData.info_time_numplanes  { get { return info_time_numplanes ; } }
                double                                                         ICsavData.info_time_avgcircles { get { return info_time_avgcircles; } }
                double                                                         ICsavData.info_time_extra      { get { return info_time_extra     ; } }

                public int             atomi;
                public int             atomj;
                public int             num_atoms;       // num atoms in csav (first two are atoms for solvent sphere)
                public List<SweepData> list_sweepdata;
                public double          volume;
                public double          millisec;        // ElapsedMilliseconds
                public (double ia_ib, double iasurf_protsurf, double ibsurf_protsurf) distinfo;
                public double info_volume_voxel    { [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return distinfo.ia_ib          ; } [MethodImpl(MethodImplOptions.AggressiveInlining)] set { distinfo.ia_ib           = value; } }
                public double info_volume_min      { [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return distinfo.iasurf_protsurf; } [MethodImpl(MethodImplOptions.AggressiveInlining)] set { distinfo.iasurf_protsurf = value; } }
                public double info_volume_max      { [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return distinfo.ibsurf_protsurf; } [MethodImpl(MethodImplOptions.AggressiveInlining)] set { distinfo.ibsurf_protsurf = value; } }
                public double info_time_numplanes  { [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return distinfo.ia_ib          ; } [MethodImpl(MethodImplOptions.AggressiveInlining)] set { distinfo.ia_ib           = value; } }
                public double info_time_avgcircles { [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return distinfo.iasurf_protsurf; } [MethodImpl(MethodImplOptions.AggressiveInlining)] set { distinfo.iasurf_protsurf = value; } }
                public double info_time_extra      { [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return distinfo.ibsurf_protsurf; } [MethodImpl(MethodImplOptions.AggressiveInlining)] set { distinfo.ibsurf_protsurf = value; } }

                public void Serialize(System.IO.BinaryWriter writer)
                {
                    writer.HWrite(atomi         );
                    writer.HWrite(atomj         );
                    writer.HWrite(num_atoms     );
                    writer.HWrite(list_sweepdata);
                    writer.HWrite(volume        );
                    writer.HWrite(millisec      );
                    writer.HWrite(distinfo.ia_ib          );
                    writer.HWrite(distinfo.iasurf_protsurf);
                    writer.HWrite(distinfo.ibsurf_protsurf);
                }
                public void Deserialize(System.IO.BinaryReader reader)
                {
                    reader.HRead(out atomi         );
                    reader.HRead(out atomj         );
                    reader.HRead(out num_atoms     );
                    reader.HRead(out list_sweepdata);
                    reader.HRead(out volume        );
                    reader.HRead(out millisec      );
                    reader.HRead(out distinfo.ia_ib          );
                    reader.HRead(out distinfo.iasurf_protsurf);
                    reader.HRead(out distinfo.ibsurf_protsurf);
                }
                public override string ToString()
                {
                    string str 
                        = $"atomij({atomi},{atomj})"
                        + $", #atm:{num_atoms}"
                        + $", #sweep:{list_sweepdata.Count}"
                        + $", vol:{volume}"
                        + $", msec:{millisec}"
                        ;
                    return str;
                }
            }
            [Serializable]
            public class Data : IData, HTLib2.IBinarySerializable
            {
                (double,double)        IData.filtercond   { get { return filtercond  ; } }
                string                 IData.pdbid        { get { return pdbid       ; } }
                double                 IData.solvgap      { get { return solvgap     ; } }
                double                 IData.delt         { get { return delt        ; } }
                IEnumerable<ICsavData> IData.csavs        { get { return csavs       ; } }
                double[]               IData.atomrad      { get { return atomrad     ; } }
                int                    IData.num_atoms    { get { return num_atoms   ; } }
                double                 IData.total_volume { get { return total_volume; } }

                public (double,double) filtercond;
                public string pdbid;
                public double solvgap;
                public double delt;
                public List<CsavData> csavs;

                public double[] atomrad;    // show number of atoms, and distribution of radius
                public int      num_atoms { get { return atomrad.Length; } }

                public double total_volume
                {
                    get
                    {
                        double volume = 0;
                        foreach(var csav in csavs)
                        {
                            volume += csav.volume;
                        }
                        return volume;
                    }
                }

                public void Serialize(System.IO.BinaryWriter writer)
                {
                    writer.HWrite(ver);
                    writer.HWrite(filtercond.Item1);
                    writer.HWrite(filtercond.Item2);
                    writer.HWrite(pdbid  );
                    writer.HWrite(solvgap);
                    writer.HWrite(delt   );
                    writer.HWrite(csavs  );
                    writer.HWrite(atomrad);
                    writer.HWrite("**end**");
                }
                public void Deserialize(System.IO.BinaryReader reader)
                {
                    HDebug.Assert(readver == -1);
                    reader.HRead(out readver);
                    reader.HRead(out filtercond.Item1);
                    reader.HRead(out filtercond.Item2);
                    reader.HRead(out pdbid  );
                    reader.HRead(out solvgap);
                    reader.HRead(out delt   );
                    reader.HRead(out csavs  );
                    reader.HRead(out atomrad);
    string __end__; reader.HRead(out __end__); if(__end__ != "**end**") throw new Exception();
                    readver = -1;
                }
            }
            public static Data data = null;

#if COLLECT_DATA
            public const string mode = "collect data";
            public struct CurrSweepData
            {
                public CsavData  csav;
                public Stopwatch stopwatch;
                public int       num_circles;       // working sweepdata, include atom circle + solv circle
                public int       treecnt;           // working sweepdata
                public List<int> hstgrm_num_circles_intree;//working sweepdata
                public int       cnt_evt_enter    ; // working sweepdata
                public int       cnt_evt_exit     ; // working sweepdata
                public int       cnt_evt_intersect; // working sweepdata
                public List<int> hstgrm_num_solvpatches;  // working sweepdata, history of num solv patches
                public List<int> hstgrm_num_swapsegm_event;
                public Dictionary<int,int> segid_cntintersect;
                public Dictionary<int,HashSet<int>> circid_intersectCircIds;
            }
            public static CurrSweepData curr;

            public static void prot_start((double,double) filtercond, IProtStruct prot, string pdbid, double delt)
            {
                HDebug.Assert(data == null);

                double[] atomrad = new double[prot.num_atoms];
                for(int ia=0; ia<atomrad.Length; ia++)
                    atomrad[ia] = prot.atom_rad(ia);

                data = new Data
                {
                    filtercond = filtercond,
                    pdbid   = pdbid,
                    solvgap = prot.solvgap,
                    delt    = delt,
                    csavs   = new List<CsavData>(100 * prot.num_atoms),
                    atomrad = atomrad,
                };
            }
            public static void prot_end(string serializepath)
            {
                HDebug.Assert(data != null);
                serialize_log(serializepath);
                data = null;
            }
            public static void csav_start
                ( int atomi, int atomj
                , double delt
                , int num_atoms_in_csav
                , (double ia_ib, double iasurf_protsurf, double ibsurf_protsurf) distinfo
                )
            {
                (int,int) atomij = (atomi < atomj) ? (atomi,atomj) : (atomj,atomi);
                atomi = atomij.Item1;
                atomj = atomij.Item2;

                CsavData csav = new CsavData();
                data.csavs.Add(csav);

                curr.csav = csav;
                curr.csav.atomi          = atomi;
                curr.csav.atomj          = atomj;
                curr.csav.num_atoms      = num_atoms_in_csav;
                curr.csav.list_sweepdata = new List<SweepData>();
                curr.csav.distinfo       = distinfo;
                curr.stopwatch           = Stopwatch.StartNew();
            }
            public static void csav_end(double csav_volume, object opt1=null, object opt2=null, object opt3=null)
            {
                curr.stopwatch.Stop();
                double millisec = curr.stopwatch.ElapsedMilliseconds;
                curr.csav.millisec = millisec;
                curr.csav.volume = csav_volume;
                curr.csav = null;
            }
            public static void sweepline_start(int num_circles)
            {
                HDebug.Assert(curr.csav != null);
                curr.num_circles      = num_circles;
                curr.treecnt          = 0;
                curr.hstgrm_num_circles_intree = new List<int>(500);
                curr.cnt_evt_enter    = 0;
                curr.cnt_evt_exit     = 0;
                curr.cnt_evt_intersect= 0;
                curr.hstgrm_num_solvpatches = new List<int>(10);
                curr.segid_cntintersect = new Dictionary<int, int>();
                curr.circid_intersectCircIds = new Dictionary<int, HashSet<int>>();
                curr.hstgrm_num_swapsegm_event = new List<int>(5);
            }
            public static void sweepline_end()
            {
                List<int> hstgrm_num_intersect_segm = new List<int>();
                foreach(var cntintersect in curr.segid_cntintersect.Values)
                    update_hist(hstgrm_num_intersect_segm, cntintersect);
                List<int> hstgrm_num_intersect_circ = new List<int>();
                foreach(var intersectCircIds in curr.circid_intersectCircIds.Values)
                    update_hist(hstgrm_num_intersect_circ, intersectCircIds.Count);

                // create sweepdata
                SweepData sweepdata = new SweepData
                {
                    num_circles      = curr.num_circles,
                    hstgrm_num_circles_intree = curr.hstgrm_num_circles_intree.ToArray(),
                    num_evt_enter    = curr.cnt_evt_enter    ,
                    num_evt_exit     = curr.cnt_evt_exit     ,
                    num_evt_intersect= curr.cnt_evt_intersect,
                    hstgrm_num_solvpatches = curr.hstgrm_num_solvpatches.ToArray(),
                    hstgrm_num_intersect_segm = hstgrm_num_intersect_segm.ToArray(),
                    hstgrm_num_intersect_circ = hstgrm_num_intersect_circ.ToArray(),
                    hstgrm_num_swapsegm_event = curr.hstgrm_num_swapsegm_event.ToArray(),
                };

                // add sweepdata into csav
                curr.csav.list_sweepdata.Add(sweepdata);
                // init working sweepdata
                curr.num_circles      = 0;
                curr.treecnt          = 0;
                curr.hstgrm_num_circles_intree = null;
                curr.cnt_evt_enter    = 0;
                curr.cnt_evt_exit     = 0;
                curr.cnt_evt_intersect= 0;
                curr.hstgrm_num_solvpatches = null;
                curr.segid_cntintersect = null;
                curr.circid_intersectCircIds = null;
                curr.hstgrm_num_swapsegm_event = null;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void update_hist(List<int> hist, int val)
            {
                while(hist.Count <= val+1)
                        hist.Add(0);
                hist[val] ++;
            }
            public static void tree_add(int numitem) { curr.treecnt ++; update_hist(curr.hstgrm_num_circles_intree, curr.treecnt); }
            public static void tree_del(int numitem) { curr.treecnt --; update_hist(curr.hstgrm_num_circles_intree, curr.treecnt); }
            public static void tree_swp(int numitem) {                  update_hist(curr.hstgrm_num_circles_intree, curr.treecnt); update_hist(curr.hstgrm_num_swapsegm_event, numitem); }
            public static void solvpatches(List<(CircleSegment c1, CircleSegment c2)> solvpatches) { update_hist(curr.hstgrm_num_solvpatches, solvpatches.Count); }
            public static void evtpts_execute(EventPoint2D evt) // used for event points that is executed only
            {
                // if(evt.type == EventPoint2D.Type.enter    ) { curr.cnt_evt_enter     ++; return; }
                // if(evt.type == EventPoint2D.Type.exit     ) { curr.cnt_evt_exit      ++; return; }
                // if(evt.type == EventPoint2D.Type.intersect) { curr.cnt_evt_intersect ++; return; }
                // throw new Exception();
            }
            public static void evtpts_add(EventPoint2D evt) // used for all event points
            {
                if(evt.type == EventPoint2D.Type.exit)
                {
                    curr.cnt_evt_exit ++;
                    return;
                }
                if(evt.type == EventPoint2D.Type.enter)
                {
                    curr.cnt_evt_enter ++;
                    curr.segid_cntintersect.Add(evt.involved_enterL.id,0);
                    curr.segid_cntintersect.Add(evt.involved_enterR.id,0);
                    int circid = evt.involved_enterL.circle.id;
                    HDebug.Assert(circid == evt.involved_enterL.circle.id);
                    HDebug.Assert(circid == evt.involved_enterR.circle.id);
                    curr.circid_intersectCircIds.Add(circid, new HashSet<int>());
                    return;
                }
                if(evt.type == EventPoint2D.Type.intersect)
                {
                    curr.cnt_evt_intersect ++;
                    curr.segid_cntintersect[evt.involved_intersectL.id]++;
                    curr.segid_cntintersect[evt.involved_intersectR.id]++;
                    int circid1 = evt.involved_intersectL.circle.id;
                    int circid2 = evt.involved_intersectR.circle.id;
                    curr.circid_intersectCircIds[circid1].Add(circid2);
                    curr.circid_intersectCircIds[circid2].Add(circid1);
                    return;
                }
                throw new Exception();
            }

            public static void serialize_log(string serializepath)
            {
                if(                      serializepath  == null) return;
                if(System.IO.File.Exists(serializepath) == true) return;
                HSerialize.SerializeBinary(serializepath, null, data);
            }
#elif COLLECT_TIME
            public const string mode = "collect time";
            public struct CurrSweepData
            {
                public CsavData  csav;
                public Stopwatch stopwatch;
                public int num_planes;
                public int sum_num_circles;
            }
            public static CurrSweepData curr;

            public static void prot_start((double,double) filtercond, IProtStruct prot, string pdbid, double delt)
            {
                HDebug.Assert(data == null);

                double[] atomrad = new double[prot.num_atoms];
                for(int ia=0; ia<atomrad.Length; ia++)
                    atomrad[ia] = prot.atom_rad(ia);

                data = new Data
                {
                    filtercond = filtercond,
                    pdbid   = pdbid,
                    solvgap = prot.solvgap,
                    delt    = delt,
                    csavs   = new List<CsavData>(100 * prot.num_atoms),
                    atomrad = atomrad,
                };
            }
            public static void prot_end(string serializepath)
            {
                HDebug.Assert(data != null);
                serialize_log(serializepath);
                data = null;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void csav_start
                ( int atomi, int atomj
                , double delt
                , int num_atoms_in_csav
                , (double ia_ib, double iasurf_protsurf, double ibsurf_protsurf) distinfo
                )
            {
                (int,int) atomij = (atomi < atomj) ? (atomi,atomj) : (atomj,atomi);
                atomi = atomij.Item1;
                atomj = atomij.Item2;

                CsavData csav = new CsavData();
                data.csavs.Add(csav);

                curr.csav = csav;
                curr.csav.atomi          = atomi;
                curr.csav.atomj          = atomj;
                curr.csav.num_atoms      = num_atoms_in_csav;
                curr.csav.list_sweepdata = new List<SweepData>();
                //curr.csav.distinfo       = distinfo;
                curr.csav.info_time_numplanes  = double.NaN;
                curr.csav.info_time_avgcircles = double.NaN;
                curr.csav.info_time_extra      = double.NaN;
                curr.num_planes      = 0;
                curr.sum_num_circles = 0;
                curr.stopwatch       = Stopwatch.StartNew();
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void csav_end(double csav_volume, object opt1=null, object opt2=null, object opt3=null)
            {
                curr.stopwatch.Stop();
                double millisec = curr.stopwatch.ElapsedMilliseconds;
                curr.csav.millisec = millisec;
                curr.csav.volume = csav_volume;
                curr.csav.info_time_numplanes  = curr.num_planes;
                curr.csav.info_time_avgcircles = ((double)curr.sum_num_circles) / curr.num_planes;
                curr.csav.info_time_extra      = 0;
                curr.csav = null;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void sweepline_start(int num_circles)
            {
                curr.num_planes ++;
                curr.sum_num_circles += num_circles;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void sweepline_end  () { }
            [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void tree_add(int numitem) { }
            [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void tree_del(int numitem) { }
            [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void tree_swp(int numitem) { }
            [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void solvpatches(List<(CircleSegment c1, CircleSegment c2)> solvpatches) { }
            [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void evtpts_execute(EventPoint2D evt) { }
            [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void evtpts_add(EventPoint2D evt) { }

            public static void serialize_log(string serializepath)
            {
                if(                      serializepath  == null) return;
                if(System.IO.File.Exists(serializepath) == true) return;
                HSerialize.SerializeBinary(serializepath, null, data);
            }
#elif COLLECT_VOXEL_VALIDATE
            public const string mode = "voxel validate";

            public static CsavData curr;

            public static void prot_start((double,double) filtercond, IProtStruct prot, string pdbid, double delt)
            {
                HDebug.Assert(data == null);

                double[] atomrad = new double[prot.num_atoms];
                for(int ia=0; ia<atomrad.Length; ia++)
                    atomrad[ia] = prot.atom_rad(ia);

                data = new Data
                {
                    filtercond = filtercond,
                    pdbid   = pdbid,
                    solvgap = prot.solvgap,
                    delt    = delt,
                    csavs   = new List<CsavData>(100 * prot.num_atoms),
                    atomrad = atomrad,
                };
            }
            public static void prot_end(string serializepath)
            {
                HDebug.Assert(data != null);
                serialize_log(serializepath);
                data = null;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void csav_start
                ( int atomi, int atomj
                , double delt
                , int num_atoms_in_csav
                , (double ia_ib, double iasurf_protsurf, double ibsurf_protsurf) distinfo
                )
            {
                (int,int) atomij = (atomi < atomj) ? (atomi,atomj) : (atomj,atomi);
                atomi = atomij.Item1;
                atomj = atomij.Item2;

                CsavData csav = new CsavData();
                data.csavs.Add(csav);

                curr = csav;
                curr.atomi          = atomi;
                curr.atomj          = atomj;
                curr.num_atoms      = num_atoms_in_csav;
                curr.list_sweepdata = new List<SweepData>();
                curr.distinfo       = distinfo;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void csav_end(double csav_volume, object opt1=null, object opt2=null, object opt3=null)
            {
                curr.millisec = Convert.ToDouble(opt3); // elapsed time for csav_volume only
                curr.volume = csav_volume;
                curr.distinfo.ia_ib = csav_volume;
                curr.distinfo.iasurf_protsurf = Convert.ToDouble(opt1);  // min
                curr.distinfo.ibsurf_protsurf = Convert.ToDouble(opt2);  // max
                curr = null;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void sweepline_start(int num_circles) { }
            [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void sweepline_end  () { }
            [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void tree_add(int numitem) { }
            [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void tree_del(int numitem) { }
            [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void tree_swp(int numitem) { }
            [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void solvpatches(List<(CircleSegment c1, CircleSegment c2)> solvpatches) { }
            [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void evtpts_execute(EventPoint2D evt) { }
            [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void evtpts_add(EventPoint2D evt) { }

            public static void serialize_log(string serializepath)
            {
                if(                      serializepath  == null) return;
                if(System.IO.File.Exists(serializepath) == true) return;
                HSerialize.SerializeBinary(serializepath, null, data);
            }
#elif COLLECT_TIME_SWEEPPLANE
            public const string mode = "collect time - sweep plane";
            public struct CurrSweepData
            {
                public CsavData  csav;
                public Stopwatch csav_stopwatch;

                public int       num_circles               ; // working sweepdata, include atom circle + solv circle
                public int       num_evt_enter             ; // working sweepdata
                public int       num_evt_exit              ; // working sweepdata
                public int       num_evt_intersect         ; // working sweepdata
                public List<int> history_num_circles_intree; // working sweepdata
                public int       plane_num_circles_intree  ; // working sweepdata
                public Stopwatch plane_stopwatch           ;
            }
            public static CurrSweepData curr;

            public static void prot_start((double,double) filtercond, IProtStruct prot, string pdbid, double delt)
            {
                HDebug.Assert(data == null);

                double[] atomrad = new double[prot.num_atoms];
                for(int ia=0; ia<atomrad.Length; ia++)
                    atomrad[ia] = prot.atom_rad(ia);

                data = new Data
                {
                    filtercond = filtercond,
                    pdbid   = pdbid,
                    solvgap = prot.solvgap,
                    delt    = delt,
                    csavs   = new List<CsavData>(100 * prot.num_atoms),
                    atomrad = atomrad,
                };
            }
            public static void prot_end(string serializepath)
            {
                HDebug.Assert(data != null);
                serialize_log(serializepath);
                data = null;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void csav_start
                ( int atomi, int atomj
                , double delt
                , int num_atoms_in_csav
                , (double ia_ib, double iasurf_protsurf, double ibsurf_protsurf) distinfo
                )
            {
                (int,int) atomij = (atomi < atomj) ? (atomi,atomj) : (atomj,atomi);
                atomi = atomij.Item1;
                atomj = atomij.Item2;

                CsavData csav = new CsavData();
                data.csavs.Add(csav);

                curr.csav = csav;
                curr.csav.atomi          = atomi;
                curr.csav.atomj          = atomj;
                curr.csav.num_atoms      = num_atoms_in_csav;
                curr.csav.list_sweepdata = new List<SweepData>();
                curr.csav.distinfo       = distinfo;
                curr.csav_stopwatch      = new Stopwatch();
                curr.plane_stopwatch     = new Stopwatch();
                curr.history_num_circles_intree = new List<int>(10000); // 50*20*10

                curr.csav_stopwatch.Restart();
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void csav_end(double csav_volume, object opt1=null, object opt2=null, object opt3=null)
            {
                curr.csav_stopwatch.Stop();

                curr.csav.millisec = curr.csav_stopwatch.Elapsed.TotalMilliseconds;
                curr.csav.volume   = csav_volume;
                curr.csav          = null;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void sweepline_start(int num_circles)
            {
                HDebug.Assert(curr.csav != null);
                curr.num_circles              = num_circles;
                curr.num_evt_enter            = 0;
                curr.num_evt_exit             = 0;
                curr.num_evt_intersect        = 0;
                curr.plane_num_circles_intree = 0;
                curr.history_num_circles_intree.Clear();
                curr.plane_stopwatch.Restart();
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void sweepline_end()
            {
                curr.plane_stopwatch.Stop();
                // create sweepdata
                SweepData sweepdata = new SweepData
                {
                    num_circles                 = curr.num_circles,
                    hstgrm_num_circles_intree   = curr.history_num_circles_intree.ToArray(),
                    num_evt_enter               = curr.num_evt_enter    ,
                    num_evt_exit                = curr.num_evt_exit     ,
                    num_evt_intersect           = curr.num_evt_intersect,
                    timesweepplane_elapsed      = new int[]
                    {
                        (int)curr.plane_stopwatch.Elapsed.Ticks,
                        (int)curr.plane_stopwatch.Elapsed.TotalMilliseconds,
                    },
                    timesweepplane_val2         = new int[0],
                    timesweepplane_val3         = new int[0],
                };

                // add sweepdata into csav
                curr.csav.list_sweepdata.Add(sweepdata);
                // init working sweepdata
                curr.num_circles                = int.MinValue;
                curr.plane_num_circles_intree   = int.MinValue;
                curr.num_evt_enter              = int.MinValue;
                curr.num_evt_exit               = int.MinValue;
                curr.num_evt_intersect          = int.MinValue;
                curr.history_num_circles_intree.Clear();
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void tree_add(int numitem) { HDebug.Assert(curr.plane_num_circles_intree != int.MinValue); curr.plane_num_circles_intree ++; curr.history_num_circles_intree.Add(curr.plane_num_circles_intree); }
            [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void tree_del(int numitem) { HDebug.Assert(curr.plane_num_circles_intree != int.MinValue); curr.plane_num_circles_intree --; curr.history_num_circles_intree.Add(curr.plane_num_circles_intree); }
            [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void tree_swp(int numitem) { HDebug.Assert(curr.plane_num_circles_intree != int.MinValue);           curr.history_num_circles_intree.Add(curr.plane_num_circles_intree); }
            [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void solvpatches(List<(CircleSegment c1, CircleSegment c2)> solvpatches) { }
            [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void evtpts_execute(EventPoint2D evt) { }
            [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void evtpts_add(EventPoint2D evt)
            {
                switch(evt.type)
                {
                    case EventPoint2D.Type.exit     : HDebug.Assert(curr.num_evt_exit      != int.MinValue); curr.num_evt_exit      ++; return;
                    case EventPoint2D.Type.enter    : HDebug.Assert(curr.num_evt_enter     != int.MinValue); curr.num_evt_enter     ++; return;
                    case EventPoint2D.Type.intersect: HDebug.Assert(curr.num_evt_intersect != int.MinValue); curr.num_evt_intersect ++; return;
                }
                throw new Exception();
            }

            public static void serialize_log(string serializepath)
            {
                if(                      serializepath  == null) return;
                if(System.IO.File.Exists(serializepath) == true) return;
                HDebug.Assert(data != null);
                HSerialize.SerializeBinary(serializepath, null, data);
            }
#else // COLLECT_NONE
            public const string mode = null;
            [MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool prot_start((double,double) filtercond, IProtStruct prot, string pdbid, double delt) { return true; }
            [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void prot_end(string serializepath) { }
            [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void csav_start(int atomi, int atomj, double delt, int num_atoms_in_csav, (double ia_ib, double iasurf_protsurf, double ibsurf_protsurf) distinfo) { }
            [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void csav_end(double csav, object opt=null, object opt2=null, object opt3=null) { }
            [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void sweepline_start(int num_circles) { }
            [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void sweepline_end  () { }
            [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void tree_add(int numitem) { }
            [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void tree_del(int numitem) { }
            [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void tree_swp(int numitem) { }
            [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void evtpts_execute(EventPoint2D evt) { }
            [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void evtpts_add(EventPoint2D evt) { }
            [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void solvpatches(List<(CircleSegment c1, CircleSegment c2)> solvpatches) { }
#endif // COLLECT_*
            //public static bool has_log(string serializepath) //, (double,double) filtercond, string pdbid, double solvgap, double delt)
            //{
            //    //string path = getpath_log(serializebase, filtercond, pdbid, solvgap, delt);
            //
            //    //if(HDebug.False)
            //    //{
            //    //    Data test;
            //    //    HSerialize.DeserializeBinary(path, null, out test);
            //    //}
            //
            //    if(HFile.Exists(serializepath) == true)
            //        return true;
            //    return false;
            //}
            public static IData deserialize_log(string serializepath)
            {
                if(System.IO.File.Exists(serializepath) == false)
                    return null;

                IData data;
                HSerialize.DeserializeBinary(serializepath, null, out data);
                return data;
            }
        }
    }
}
