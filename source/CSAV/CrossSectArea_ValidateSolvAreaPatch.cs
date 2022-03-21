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
    public partial class CrossSectArea
    {
        public static bool ValidateSolvAreaPatch(LinkedAvlTree<CircleSegment> tree, double curry, ICollection<(CircleSegment c1, CircleSegment c2)> solvpatches)
        {
            LinkedAvlTree<CircleSegment>.Node node = tree.GetHead();

            // iteratively find its next node in linkedlist 
            int overlapped = 0;
            int patches_count = 0;
            while (node != null)
            {
                CircleSegment cscurr = node.value;

                if(cscurr.IsAtomSegment())
                {
                    // update the number of overlapped atoms
                    if      (cscurr.type == CircleSegment.Type.left ) overlapped--;
                    else if (cscurr.type == CircleSegment.Type.right) overlapped++;
                    else throw new Exception();
                }
                else 
                {
                    HDebug.Assert(cscurr.IsSolventSegment() == true);

                    // update the number of overlapped atoms
                    if      (cscurr.type == CircleSegment.Type.left ) overlapped++;
                    else if (cscurr.type == CircleSegment.Type.right) overlapped--;
                    else throw new Exception();
                }

                HDebug.Assert(node.value.patchval == overlapped);

                if (overlapped == 2 && node.next != null)
                {
                    CircleSegment csnext = node.next.value;
                        
                    //solvpatches.Add((cscurr, csnext));
                    patches_count ++;
                    if(solvpatches.Contains((cscurr, csnext)) == false)
                        return false;
                }

                node = node.next;
            }

            if(overlapped != 0)
                return false;
            if(solvpatches.Count != patches_count)
                return false;
            return true;
        }
    }
}
