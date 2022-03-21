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
    public class ProtStruct : IProtStruct
    {
        Vector[] _atoms;      // atom coordinate
        double[] _atom_rad;   // atom vdW radius
        double   _solvgap;    // solvent gap: maximum distance between protein surface and probe sphere center

        public double solvgap   { [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return _solvgap;      } }
        public int    num_atoms { [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return _atoms.Length; } }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public double[] atom_coord(int ia) { return _atoms   [ia]; }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public double   atom_rad  (int ia) { return _atom_rad[ia]; }     // solvent accessible atom sphere
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public double   solv_rad  (int ia) { return _atom_rad[ia] + _solvgap; }     // solvent sphere of atom
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public double solv_rad_max() { return _atom_rad.Max() + _solvgap; }

        public static ProtStruct FromPdb(string filepath, double solvgap)
        {
            /// Example                                                                         
            ///          1         2         3         4         5         6         7         8
            /// 12345678901234567890123456789012345678901234567890123456789012345678901234567890
            /// ATOM     32  N  AARG A  -3      11.281  86.699  94.383  0.50 35.88           N  
            
            string[] lines = System.IO.File.ReadAllLines(filepath);

            List<Vector> atomlst = new List<Vector>();
            List<double> radilst = new List<double>();
            for(int i=0; i<lines.Length; ++i)
            {
                if(lines[i].Substring(0, 6) == "ATOM  ")
                {
                    string _x = lines[i].Substring(31, 8); double x = Double.Parse(_x);
                    string _y = lines[i].Substring(39, 8); double y = Double.Parse(_y);
                    string _z = lines[i].Substring(47, 8); double z = Double.Parse(_z);
                    atomlst.Add(new Vector(x, y, z));

                    string element = lines[i].Substring(77, 2).Trim();
                    switch(element)
                    {
                        case "H": radilst.Add(1.2 ); break;
                        case "N": radilst.Add(1.85); break;
                        case "O": radilst.Add(1.70); break;
                        case "C": radilst.Add(1.90); break;
                        case "S": radilst.Add(2.0 ); break;
                        default:
                            throw new Exception("Undefined atom element...");
                    }
                }
            }

            HDebug.Assert(atomlst.Count == radilst.Count);

            int atom_len = atomlst.Count;
            Vector[] atoms    = new Vector[atom_len];
            double[] atom_rad = new double[atom_len];
            for(int i=0; i<atom_len; ++i)
            {
                atoms[i]    = atomlst[i];
                atom_rad[i] = radilst[i];
            }
            
            return new ProtStruct
            {
                _atoms    = atoms,
                _atom_rad = atom_rad,
                _solvgap  = solvgap,
            };
        }

        public static ProtStruct FromText(string filepath, double solvgap)
        {
            string[] lines = System.IO.File.ReadAllLines(filepath);

            int atom_len = lines.Length;
            Vector[] atoms    = new Vector[atom_len];
            double[] atom_rad = new double[atom_len];
            for(int i=0; i<atom_len; ++i)
            {
                string[] tokens = lines[i].Split(new char[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries);
                HDebug.Assert(tokens.Length == 5);

                double x = Double.Parse(tokens[1]);
                double y = Double.Parse(tokens[2]);
                double z = Double.Parse(tokens[3]);

                atoms   [i] = new Vector(x, y, z);
                atom_rad[i] = Double.Parse(tokens[4]);
            }

            return new ProtStruct
            {
                _atoms    = atoms,
                _atom_rad = atom_rad,
                _solvgap  = solvgap,
            };
        }

        public static ProtStruct SimpleTwoAtoms(Vector coord0, double radii0, Vector coord1, double radii1, double solvgap)
        {
            Vector[] atoms    = { coord0, coord1 };
            double[] atom_rad = { radii0, radii1 };
            int[] atomidxs    = { 0, 1 };

            return new ProtStruct
            {
                _atoms    = atoms,
                _atom_rad = atom_rad,
                _solvgap  = solvgap,
            };
        }
    }
}
