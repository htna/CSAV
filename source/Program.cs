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
    partial class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0 || args[0] == "-h" || args[0] == "--help")
            {
                DisplayHelp();
                return;
            }

            string pdbid = "";
            string outpath = "";

            ProtStruct prot = null;
            double solvgap = 3.5, delt = 0.1;
            bool stopAtEvtExitSolv = false;
            List<(int ia, int ib, List<int> atomids)> lstpairs = new List<(int, int, List<int>)>();
            for (int i=0; i<args.Length-1; ++i)
            {
                switch(args[i])
                {
                    case "-i":
                    case "--input":
                        {
                            string protpath = args[++i];
                            int startidx = Math.Max(0, protpath.LastIndexOf('\\')+1);
                            int   endidx = (protpath.LastIndexOf('.') == -1) ? protpath.Length : protpath.LastIndexOf('.');
                            pdbid = protpath.Substring(startidx, endidx - startidx);
                            if(System.IO.Path.GetExtension(protpath) == ".pdb") prot = ProtStruct.FromPdb(protpath, solvgap);
                            else                                                prot = ProtStruct.FromText(protpath, solvgap);
                            break;
                        }
                    case "-x":
                    case "--index":
                        {
                            string idxpath = args[++i];
                            (bool _succ, List<(int, int, List<int>)> _lstpairs) = ReadIdxPairs(idxpath);
                            if (_succ == true)
                            {
                                lstpairs = _lstpairs;
                            }
                            break;
                        }
                    case "-p":
                    case "--param":
                        {
                            string prmpath = args[++i];
                            (bool _succ, double _solvgap, double _delt, bool _stopAtEvtExitSolv) = ReadParam(prmpath);
                            if (_succ == true)
                            {
                                solvgap = _solvgap;
                                delt = _delt;
                                stopAtEvtExitSolv = _stopAtEvtExitSolv;
                            }
                            break;
                        }
                    case "-o":
                    case "--output":
                        {
                            outpath = args[++i];
                            break;
                        }
                    default:
                        throw new Exception();
                }
            }

            if (lstpairs.Count == 0)
            {
                throw new Exception("Pair information is missing");
            }

            // calculate CSAV
            List<string> output = new List<string>();
            string cmd = "// command: csav.exe ";
            foreach(string arg in args)
                cmd += arg + " ";
            output.Add(cmd);
            output.Add("// solvent gap: " + solvgap + ", delta: " + delt + ", stop at event of solvent exit: " + (stopAtEvtExitSolv ? "yes" : "no"));
            output.Add("// total CSAV: ");
            output.Add("");
            output.Add("// index pair: pairwise CSAV");

            double totcsav = 0;
            foreach ((int ia, int ib, List<int> atomids) in lstpairs)
            {
                double csav = CommSolvAccVol.CalcCommSolvAccVolume(prot, ia, ib, atomids, delt, stopAtEvtExitSolv);
              
                totcsav += csav;
                string result = (ia + 1) + ", " + (ib + 1) + ": " + csav;

                output.Add(result);
            }
            output[2] += totcsav;

            // save result to file
            if(outpath == "")
            {
                outpath = pdbid + "-capture.txt";
            }
            System.IO.File.WriteAllLines(outpath, output);
        }
    }
}
