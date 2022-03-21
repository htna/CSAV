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
        public static double CalcCommSolvAccVolume
            ( IProtStruct prot
            , int atom_a
            , int atom_b
            , List<int> atoms_involved
            , double delt       // = 0.1
            , bool stopAtEvtExitSolv
            )
        {
            EventQueue eventqueue = new EventQueue();

            HDebug.Assert(atoms_involved.Contains(atom_a));
            HDebug.Assert(atoms_involved.Contains(atom_b));
            
            // determine list of (z, atom_id, top/down) sorted from top->down
            List<(int idx, double z, int id, TopDown type)> lstAtomSorted;
            {
                lstAtomSorted = new List<(int idx, double z, int id, TopDown type)>((atoms_involved.Count+2)*2);
                //foreach(int id in atoms_involved)
                for(int idx=0; idx<atoms_involved.Count; idx++)
                {
                    int    id   = atoms_involved[idx];
                    double id_z = prot.atom_coord(id)[2];
                    double rad  = prot.atom_rad  (id);
                    lstAtomSorted.Add((idx, id_z+rad, id, TopDown.top));
                    lstAtomSorted.Add((idx, id_z-rad, id, TopDown.down));
                }
                static int CompTopDown((int idx, double z, int id, TopDown type) x, (int idx, double z, int id, TopDown type) y)
                {
                    if (x.z < y.z) return -1;
                    if (x.z > y.z) return +1;

                    if (x.id < y.id) return -1;
                    if (x.id > y.id) return +1;

                    HDebug.Assert(x.idx  == y.idx );
                    HDebug.Assert(x.z    == y.z   );
                    HDebug.Assert(x.id   == y.id  );
                    HDebug.Assert(x.type == y.type);
                    return 0;
                }
                lstAtomSorted.Sort(CompTopDown);
                if(HDebug.IsDebuggerAttached)
                {
                    for(int i=1; i<lstAtomSorted.Count; i++)
                        HDebug.Assert(lstAtomSorted[i-1].z <= lstAtomSorted[i].z);
                }
            }

            double minz, maxz;
            {
                double a_zcoord = prot.atom_coord(atom_a)[2]; double a_rad = prot.atom_rad(atom_a); double a_solvrad = prot.solv_rad(atom_a);
                double b_zcoord = prot.atom_coord(atom_b)[2]; double b_rad = prot.atom_rad(atom_b); double b_solvrad = prot.solv_rad(atom_b);
                double maxsolvrad = Math.Max(a_solvrad, b_solvrad);

                minz = Math.Min(a_zcoord, b_zcoord) - maxsolvrad;
                maxz = Math.Max(a_zcoord, b_zcoord) + maxsolvrad;
            }

            List<(int idx, int id)> setAtomInPlane = new List<(int idx, int id)>(atoms_involved.Count);
            void Move_lstAtomSorted_to_setAtomInPlane(double z)
            {
                while(lstAtomSorted.Count != 0)
                {
                    var last = lstAtomSorted.HLast();
                     if(z > last.z)
                        break;
                    if(last.type == TopDown.top) { HDebug.Assert(setAtomInPlane.Contains((last.idx, last.id)) == false); setAtomInPlane.Add   ((last.idx, last.id)); }
                    else                         { HDebug.Assert(setAtomInPlane.Contains((last.idx, last.id)) == true ); setAtomInPlane.Remove((last.idx, last.id)); }
                    lstAtomSorted.HRemoveLast();
                }
            }

            Circle       circle_a_solv = NewCircle(atom_a, true);
            Circle       circle_b_solv = NewCircle(atom_b, true);
            List<Circle> circle_involved = new List<Circle>(atoms_involved.Count);
            foreach(int atom in atoms_involved)
                circle_involved.Add(NewCircle(atom, false));

            List<Circle> circles = new List<Circle>(atoms_involved.Count+2);
            Debug._debug_circles = circles;
            double tot_vol = 0;
            int iz = 0;
            for(double z = maxz; z >= minz; z -= delt)
            {
                iz ++;

                Move_lstAtomSorted_to_setAtomInPlane(z);

                int dxyi = 0;
                if(InitCircle(prot, circle_a_solv, z, delt, ++dxyi) == false) continue;
                if(InitCircle(prot, circle_b_solv, z, delt, ++dxyi) == false) continue;
                if(Circle.Overlap(circle_a_solv, circle_b_solv) == false)
                    continue;
                circles.Clear();
                circles.Add(circle_a_solv);
                circles.Add(circle_b_solv);

                foreach (var atom in setAtomInPlane)
                {
                    Circle circle = circle_involved[atom.idx];
                    if(InitCircle(prot, circle, z, delt, ++dxyi) == false)
                        continue;

                    bool add = false;
                    if(atom.id == atom_a) add = true;
                    if(atom.id == atom_b) add = true;
                    if(Circle.Overlap(circle, circle_a_solv) && Circle.Overlap(circle, circle_b_solv)) add = true;

                    if(add == false)
                        continue;
                    circles.Add(circle);
                }
                {
                    // remove enclosed circles
                    for(int ci=2; ci<circles.Count; ci++)
                    {
                        if(circles[ci] == null) continue;
                        for(int cj=ci+1; cj<circles.Count; cj++)
                        {
                            if(circles[ci] == null) break;
                            if(circles[cj] == null) continue;
                            int enclosed = Circle.Enclosed(circles[ci], circles[cj]);
                            if(enclosed == 0) continue;
                            if(enclosed == 1) { HDebug.Assert(enclosed == +1); circles[cj] = null; } /// ci ≥ cj  =>  cj is enclosed by ci  =>  remove cj
                            else              { HDebug.Assert(enclosed == -1); circles[ci] = null; } /// ci ≤ cj  =>  ci is enclosed by cj  =>  remove ci
                        }
                    }
                    HDebug.Assert(circles[0] != null);
                    HDebug.Assert(circles[1] != null);
                    int num_removed_circles = circles.RemoveAll(IsNull);
                    static bool IsNull(Circle obj) { return (obj == null); }
                }
                try
                {
                    Debug._debug_z = z;
                    Debug._debug_iaibz_numcircles = circles.Count;
                    var area = CrossSectArea.CalcCrossSectAreaOfCSAV(circles, eventqueue, stopAtEvtExitSolv);
                    tot_vol = tot_vol + area * delt;
                }
                catch(Exception e)
                {
                    System.Console.WriteLine("atom index a: " + atom_a);
                    System.Console.WriteLine("atom index b: " + atom_b);
                    System.Console.WriteLine("z-coord: " + z + " (z-idx " + iz.ToString() + ")");
                    System.Console.WriteLine("circles: " + circles.Count);
                    System.Console.WriteLine("\n_deubg: " + Debug._debug + "\n\n");
                    System.Console.WriteLine(e.ToString());
                    throw e;
                }
            }

            return tot_vol;
        }
    }
}
