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
        public static void DrawCircles(string filepath, List<Circle> circles)
        {
            var lines = DrawCircles(circles);
            System.IO.File.WriteAllLines(filepath, lines);
        }
        public static void DrawCircles(string filepath, List<CircleSegment> segments)
        {
            var lines = DrawCircles(segments);
            System.IO.File.WriteAllLines(filepath, lines);
        }
        public static string ToMathematicaStringCircles(List<CircleSegment> segments)
        {
            StringBuilder sb = new StringBuilder();
            var lines = DrawCircles(segments);
            foreach(var line in lines)
                sb.AppendLine(line);
            return sb.ToString();
        }
        public static string[] DrawCircles(List<CircleSegment> segments)
        {
            List<Circle> circles = new List<Circle>();
            foreach(var segment in segments)
                circles.Add(segment.circle);
            return DrawCircles(circles);
        }
        public static string[] DrawCircles(string filepath, List<Circle> circles, bool color_solvent = true)
        {
            List<ValueTuple<double, double, double>> atom_data = new List<(double, double, double)>();
            List<ValueTuple<double, double, double>> solv_data = new List<(double, double, double)>();
            foreach(var circle in circles)
            {
                if(circle.type == ElementType.atom)
                    atom_data.Add((circle.center_x, circle.center_y, circle.radii));
                else
                    solv_data.Add((circle.center_x, circle.center_y, circle.radii));
            }
                
            string adata = Mathematica.ToString2(atom_data);
            string sdata = Mathematica.ToString2(solv_data);

            string[] lines = new string[]
            {
                "adata = " + adata + ";",
                "sdata = " + sdata + ";",
                "g = Graphics[{{}",
                ",      Table[ Circle[{c[[1]],c[[2]]}, c[[3]]], {c, adata}] (* atom circles *)",
                ", Red, Table[ Circle[{c[[1]],c[[2]]}, c[[3]]], {c, sdata}] (* solvent circles *)",
                "}",
                "]",
            };

            if(filepath != null)
                System.IO.File.WriteAllLines(filepath, lines);

            return lines;
        }
        public static string[] DrawCircles(List<Circle> circles)
        {
            List<ValueTuple<double, double, double, int>> data = new List<(double, double, double, int)>();
            foreach(var circle in circles)
                data.Add((circle.center_x, circle.center_y, circle.radii, circle.id));
            string sdata = Mathematica.ToString2(data);

            string[] lines = new string[]
            {
                "data = " + sdata + ";",
                "linex = {Min[Table[c[[1]]-c[[3]],{c,data}]],Max[Table[c[[1]]+c[[3]],{c,data}]]};",
                "liney = 0;",
                "g = Graphics[{{}",
                ", Table[ Tooltip[Circle[{c[[1]],c[[2]]}, c[[3]]], c[[4]]], {c,data}] (* circles *)",
                ", Table[ Point[{c[[1]],c[[2]]+ c[[3]]}], {c, data}] (* begins *)",
                ", Table[ Point[{c[[1]],c[[2]]- c[[3]]}], {c, data}] (* ends *)",
                ", Red, Line[{{linex[[1]],liney},{linex[[2]],liney}}]",
                "}",
                "]",
            };

            string filepath = null;
            if(filepath != null)
                System.IO.File.WriteAllLines(filepath, lines);

            return lines;
        }
        public static string ToMathematicaStringCircles()
        {
            StringBuilder sb = new StringBuilder();
            var lines = DrawCirclesEventPoint(_debug_circles,_debug_currep);
            foreach(var line in lines)
                sb.AppendLine(line);
            return sb.ToString();
        }
        public static string[] DrawCirclesEventPoint()
        {
            return DrawCirclesEventPoint(_debug_circles,_debug_currep);
        }
        public static string[] DrawCirclesEventPoint
            ( List<Circle> circles
            , EventPoint2D currep
            )
        {
            List<ValueTuple<double, double, double, int>> data = new List<(double, double, double, int)>();
            foreach(var circle in circles)
                data.Add((circle.center_x, circle.center_y, circle.radii, circle.id));
            string sdata = Mathematica.ToString2(data);

            string[] lines = new string[]
            {
                "data = " + sdata + ";",
                "evtp = {"+currep.x+","+currep.y+"};",
                "linex = {Min[Table[c[[1]]-c[[3]],{c,data}]],Max[Table[c[[1]]+c[[3]],{c,data}]]};",
                "liney = "+currep.y+";",
                "g = Graphics[{{}",
                ", Table[ Tooltip[Circle[{c[[1]],c[[2]]}, c[[3]]], c[[4]]], {c,data}] (* circles *)",
                ", Table[ Point[{c[[1]],c[[2]]+ c[[3]]}], {c, data}] (* begins *)",
                ", Table[ Point[{c[[1]],c[[2]]- c[[3]]}], {c, data}] (* ends *)",
                ", Red, Line[{{linex[[1]],evtp[[2]]},{linex[[2]],evtp[[2]]}}]",
                ", Red, Point[evtp]",
                "}",
                "]",
            };

            string filepath = null;
            if(filepath != null)
                System.IO.File.WriteAllLines(filepath, lines);

            return lines;
        }
        //  public static string[] DrawSpheres(string filepath, List<Sphere> spheres, bool color_solvent = true)
        //  {
        //      List<ValueTuple<double, double, double, double>> atom_data = new List<(double, double, double, double)>();
        //      List<ValueTuple<double, double, double, double>> solv_data = new List<(double, double, double, double)>();
        //      foreach(var sphere in spheres)
        //      {
        //          if(sphere.type == ElementType.atom)
        //              atom_data.Add((sphere.center[0], sphere.center[1], sphere.center[2], sphere.radii));
        //          else
        //              solv_data.Add((sphere.center[0], sphere.center[1], sphere.center[2], sphere.radii));
        //      }
        //      
        //      string adata = Mathematica.ToString2(atom_data);
        //      string sdata = Mathematica.ToString2(solv_data);
        //  
        //      string[] lines = new string[]
        //      {
        //          "adata = " + adata + ";",
        //          "sdata = " + sdata + ";",
        //          "g = Graphics3D",
        //  
        //          "}",
        //          "]",
        //      };
        //  
        //      if(filepath != null)
        //          System.IO.File.WriteAllLines(filepath, lines);
        //  
        //      return lines;
        //  }

        public static bool ValidateTree(LinkedAvlTree<CircleSegment> tree, Comparison<CircleSegment> treecs_CompareValidate, double yy)
        {
            if(double.IsNaN(yy))
                return true;
            if(HDebug.True)
                return tree.Validate(treecs_CompareValidate);

            List<CircleSegment> lstseg = ListTreeXSeg(tree);
            var lstNodes = new List<((double x, bool succ) getx, string node, CircleSegment seg, bool assert)>();
            bool validate = true;
            for(int i=0; i<lstseg.Count; i++)
            {
                var seg = lstseg[i];
                lstNodes.Add((seg.TryGetX(yy), seg.ToString(), seg, true));
                if(lstNodes.Count >= 2)
                {
                    bool assert = CircleSegmentComparer.Compare(lstseg[i-1], lstseg[i], yy) < 0;
                    if(assert == false)
                    {
                        validate = false;
                        lstNodes[i] = (seg.TryGetX(yy), seg.ToString(), seg, false);
                    }
                }
            }
            return validate;
        }

        public static List<CircleSegment> ListTreeXSeg(LinkedAvlTree<CircleSegment> tree)
        {
            var head = tree.GetHead();
            List<CircleSegment> lstXSeg = new List<CircleSegment>();

            int count = 0;

            while (head != null)
            {
                var    cs = head.value;
                lstXSeg.Add(cs);

                head = head.next;
                count++;
            }

            return lstXSeg;
        }
    }
}
