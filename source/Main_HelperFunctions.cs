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
using System.Runtime.CompilerServices;
using CSAV.HTLib2;

namespace CSAV
{
    using File = System.IO.File;
    using Reader = System.IO.StreamReader;

    partial class Program
    {
        static void DisplayHelp()
        {
            Console.WriteLine("Welcome to the Common Solvent Accessible Volume(CSAV) calculation program.");
            Console.WriteLine("  This program takes a three-dimensional structural data of proteins, such as a PDB file or a text file, a list of");
            Console.WriteLine("  pairs and their involved atoms' protein atom indices, and a parameter file as inputs, and then processes the CSAV,");
            Console.WriteLine("  the overlapping volumes of the solvent between pairs of protein atoms, of user specified indices (1-based).");
            Console.WriteLine("  Please see: https://github.com/htna/CSAV.");
            Console.WriteLine("\nUsage:");
            Console.WriteLine("  csav.exe [-h] | [-i pdb|txt] [-x txt] [-p txt] [-o output-filename]");
            Console.WriteLine("\nInput:");
            Console.WriteLine("  -i, --input pdb, txt\tContains atomic coordinates and radius(txt) or atom type(pdb) information of protein atoms");
            Console.WriteLine("\t\t\t    Format (txt)");
            Console.WriteLine("\t\t\t    line 1:    <index of atom, 1>    <coordinate of atom 1, x y z>    <radius of atom 1, r>");
            Console.WriteLine("\t\t\t    line 2:    <index of atom, 2>    <coordinate of atom 2, x y z>    <radius of atom 2, r>");
            Console.WriteLine("\t\t\t      ...");
            Console.WriteLine("\t\t\t    line n:    <index of atom, n>    <coordinate of atom n, x,y,z>    <radius of atom n, r>");
            Console.WriteLine("  -x, --index txt\tContains the protein atom indices of the pairs and their involved atoms (default=all pairs of protein atoms)");
            Console.WriteLine("\t\t\t    Format (txt)");
            Console.WriteLine("\t\t\t    line 1:    <index of atom, i_1>, <index of atom, j_1>; <list of involved atoms' ids>");
            Console.WriteLine("\t\t\t      ...");
            Console.WriteLine("\t\t\t    line  :    <index of atom, i_m>, <index of atom, j_m>; <list of involved atoms' ids>");
            Console.WriteLine("  -p, --param txt\tContains parameter information");
            Console.WriteLine("\t\t\t    Format (txt)");
            Console.WriteLine("\t\t\t    line 1:    <gap between atom and solvent surface in Angstrom, (default=3.5Å)>");
            Console.WriteLine("\t\t\t    line 2:    <gap between each sweep plane in Angstrom, (default=0.1Å)>");
            Console.WriteLine("\t\t\t    line 3:    <stop earlier when calculation is done, (Y/N, default=N)>");
            Console.WriteLine("\nOptions:");
            Console.WriteLine("  -h, --help\t\tDisplay this help screen and exit.");
            Console.WriteLine("\nOutput:");
            Console.WriteLine("  -o, --output filename\tSave the output results into the specified filename. (default=input-capture.txt)");
            Console.WriteLine("\t\t\t    Format");
            Console.WriteLine("\t\t\t    line 1:    <index pair, 1>: <CSAV>");
            Console.WriteLine("\t\t\t      ...");
            Console.WriteLine("\t\t\t    line m:    <index pair, m>: <CSAV>");
            Console.WriteLine("\nExample:");
            Console.WriteLine("  // Run it on the input files and save the result. Please see the documentation for details.");
            Console.WriteLine("  csav.exe -i prot -x pairs -p params -o output-filename\n");
        }

        static (bool succ, List<(int, int, List<int>)> lstpairs) ReadIdxPairs(string idxpath)
        {
            if (!File.Exists(idxpath))
            {
                throw new Exception();
            }

            string[] lines = File.ReadAllLines(idxpath);

            bool succ = true;

            List<(int, int, List<int>)> lstpairs = new List<(int, int, List<int>)>();
            for (int i = 0; i < lines.Length && succ == true; ++i)
            {
                string[] tokens = lines[i].Split(new char[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);

                int ia, ib;
                if (tokens.Length < 2) { succ = false; break; }
                if (!Int32.TryParse(tokens[0], out ia)) { succ = false; break; }
                if (!Int32.TryParse(tokens[1], out ib)) { succ = false; break; }

                List<int> atomids = new List<int>();
                for(int idx=2; idx < tokens.Length; ++idx)
                {
                    int id;
                    if(!Int32.TryParse(tokens[idx], out id)) { succ = false; break; }
                    atomids.Add(id-1);
                }
                HDebug.Assert(atomids.Count > 0);
                lstpairs.Add((ia-1, ib-1, atomids));
            }

            return (succ, lstpairs);
        }

        static (bool succ, double solvgap, double delt, bool stopAtEvtExitSolv) ReadParam(string parampath)
        {
            if (!File.Exists(parampath))
            {
                throw new Exception();
            }

            string[] lines = File.ReadAllLines(parampath);

            bool succ = (lines.Length == 3);

            double solvgap = 0, delt = 0;
            bool stopAtEvtExitSolv = false;
            if (!succ) return (succ, solvgap, delt, stopAtEvtExitSolv);
            if (!Double.TryParse(lines[0], out solvgap)) succ = false;
            if (!Double.TryParse(lines[1], out delt)) succ = false;
            stopAtEvtExitSolv = lines[2].Length >= 1 && ((lines[2][0] == 'y') || (lines[2][0] == 'Y'));


            return (succ, solvgap, delt, stopAtEvtExitSolv);
        }
    }
}
