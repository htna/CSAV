# CSAV software

<!--
## License
Copyright (c) 2022, Hyuntae Na and In Jung Kim  
All rights reserved. 

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:  

1. Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
3. Neither the name of the copyright holder nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.
  
THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
-->

## Overview
This repository contains a tutorial on calculating the common solvent accessible volume (CSAV) of a protein based on a sweep-line algorithm. The term CSAV here measures the amount of solvent interacting with two proteins' atoms at the same time and thus provides a possible means to measure the protein-solvent-protein interactions. Detailed information can be found in the paper [An efficient algorithm calculating common solvent accessible volume]().

## Getting Started
This program takes a three-dimensional structural data of proteins, such as a PDB file or a text file, a list of pairs and their involved atoms' protein atom indices, and an optional parameter file as inputs, and then processes the CSAV calculation of user specified indices (1-based). 

### Data
- [xyzr](./dataset/xyzr/) contains the xyz-coordinates and the radius information about the dataset
- [pair](./dataset/pair/) contains the pair information about the dataset 
- [result](./dataset/result/) contains the sample output of the dataset

### Usage
```
Usage:
  csav.exe [-h] | [-i pdb|txt] [-x txt] [-p txt] [-o output-filename]
Input:
  -i, --input pdb, txt      Contains atomic coordinates and radius(txt) or atom type(pdb) information of protein atoms
                                Format (txt)
                                line 1:    <index of atom, 1>    <coordinate of atom 1, x y z>    <radius of atom 1, r>
                                line 2:    <index of atom, 2>    <coordinate of atom 2, x y z>    <radius of atom 2, r>
                                  ...
                                line n:    <index of atom, n>    <coordinate of atom n, x,y,z>    <radius of atom n, r>
  -x, --index txt           Contains the protein atom indices of the pairs and their involved atoms (default=all pairs of protein atoms)
                                Format (txt)
                                line 1:    <index of atom, i_1>, <index of atom, j_1>; <list of involved atoms' ids>
                                  ...
                                line m:    <index of atom, i_m>, <index of atom, j_m>; <list of involved atoms' ids>
  -p, --param txt           Contains parameter information
                                Format (txt)
                                line 1:    <gap between atom and solvent surface in Angstrom, (default=3.5Å)>
                                line 2:    <gap between each sweep plane in Angstrom, (default=0.1Å)>
                                line 3:    <stop earlier when calculation is done, (Y/N, default=N)>
Options:
  -h, --help                Display this help screen and exit.
Output:
  -o, --output filename     Save the output results into the specified filename. (default=input-capture.txt)
                                Format
                                line 1:    <index pair, 1>: <CSAV>
                                  ...
                                line m:    <index pair, m>: <CSAV>
```

## License
This project is licensed under the MIT License. See LICENSE for more details
