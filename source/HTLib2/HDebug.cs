/////////////////////////////////////////////////////////////////////////////////////////
//  MIT License                                                                        //
//                                                                                     //
//  Copyright (c) 2022 Hyuntae Na                                                      //
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
using System.Text;
using System.Runtime.CompilerServices;

namespace CSAV.HTLib2
{
    using DEBUG = System.Diagnostics.Debug;

    public partial class HDebug
    {
#if DEBUG
        public static bool True  { get { return true ; } }
        public static bool False { get { return false; } }
#else
        public const bool True  = true ;
        public const bool False = false;
#endif

        public static Random rand = new Random();
        public static readonly HDebug debug = new HDebug();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//      [System.Diagnostics.Conditional("DEBUG")]
//      [System.Diagnostics.DebuggerHiddenAttribute()]
        public static bool Check(bool istrue)
        {
            if(IsDebuggerAttached == false)
                return false;
            return istrue;
        }

        [System.Diagnostics.Conditional("DEBUG")]
//      [System.Diagnostics.DebuggerHiddenAttribute()]
        public static void SetEpsilon(IList<double> values)
        {
            for(int i=0; i<values.Count; i++)
                values[i] = double.Epsilon;
        }

        [System.Diagnostics.Conditional("DEBUG")]
//      [System.Diagnostics.DebuggerHiddenAttribute()]
        public static void SetEpsilon(double[,] values)
        {
            for(int i0=0; i0<values.GetLength(0); i0++)
                for(int i1=0; i1<values.GetLength(1); i1++)
                    values[i0,i1] = double.Epsilon;
        }

        [System.Diagnostics.Conditional("DEBUG")]
        [System.Diagnostics.DebuggerHiddenAttribute()]
        public static void Assert(params bool[] conditions)
        {
            AssertAnd(conditions);
//          System.Diagnostics.Debug.Assert(condition);
        }

        static bool selftest_IsEqualsAll = true;
        public static bool IsEqualsAll<T>(params T[] values)
            where T : IEquatable<T>
        {
            if(IsDebuggerAttached && selftest_IsEqualsAll)
                #region selftest
                {
                    selftest_IsEqualsAll = false;
                    HDebug.Assert(IsEqualsAll(1, 1, 1, 1, 1));
                    HDebug.Assert(IsEqualsAll(1, 1, 1, 1, 2) == false);
                    HDebug.Assert(IsEqualsAll(2, 1, 1, 1, 1) == false);
                }
                #endregion
            for(int i=1; i<values.Length; i++)
            {
                if(values[0].Equals(values[i]) == false)
                    return false;
            }
            return true;
        }
        [System.Diagnostics.Conditional("DEBUG")]
        [System.Diagnostics.DebuggerHiddenAttribute()]
        public static void AssertAllEquals<T>(params T[] values)
            where T : IEquatable<T>
        {
            System.Diagnostics.Debug.Assert(IsEqualsAll(values));
        }

        [System.Diagnostics.Conditional("DEBUG")]
        [System.Diagnostics.DebuggerHiddenAttribute()]
        public static void AssertDouble(params double[] values)
        {
            foreach(double value in values)
            {
                System.Diagnostics.Debug.Assert(double.IsInfinity(value) == false);
                System.Diagnostics.Debug.Assert(double.IsNaN(value) == false);
            }
        }

        [System.Diagnostics.Conditional("DEBUG")]
        [System.Diagnostics.DebuggerHiddenAttribute()]
        public static void AssertTolerance(double tolerance, params double[] values)
        {
            System.Diagnostics.Debug.Assert(CheckTolerance(tolerance, values));
        }
        public static bool CheckTolerance(double tolerance, params double[] values)
        {
            for(int i=0; i<values.Length; i++)
                if(Math.Abs(values[i]) > tolerance)
                    return false;
            //System.Diagnostics.Debug.Assert(assert);
            return true;
        }

        [System.Diagnostics.Conditional("DEBUG")]
        [System.Diagnostics.DebuggerHiddenAttribute()]
        public static void AssertToleranceIf(bool condition, double tolerance, params double[] values)
        {
            if(condition)
            {
                bool assert = true;
                for(int i=0; i<values.Length; i++)
                    assert &= (Math.Abs(values[i]) <= tolerance);
                System.Diagnostics.Debug.Assert(assert);
            }
        }

        [System.Diagnostics.Conditional("DEBUG")]
        [System.Diagnostics.DebuggerHiddenAttribute()]
        public static void AssertTolerance(double tolerance, params double[][] values)
        {
            bool assert = true;
            for(int i=0; i<values.Length; i++)
                for(int j=0; j<values[i].Length; j++)
                    assert &= (Math.Abs(values[i][j]) <= tolerance);
            System.Diagnostics.Debug.Assert(assert);
        }

        [System.Diagnostics.Conditional("DEBUG")]
        [System.Diagnostics.DebuggerHiddenAttribute()]
        public static void AssertTolerance(double tolerance, double[,] values)
        {
            bool assert = CheckTolerance(tolerance, values);
            System.Diagnostics.Debug.Assert(assert);
        }
        public static bool CheckTolerance(double tolerance, double[,] values)
        {
            for(int c=0; c<values.GetLength(0); c++)
                for(int r=0; r<values.GetLength(1); r++)
                {
                    double value = values[c, r];
                    if(Math.Abs(value) > tolerance)
                        return false;
                }
            return true;
        }

        [System.Diagnostics.Conditional("DEBUG")]
        [System.Diagnostics.DebuggerHiddenAttribute()]
        public static void AssertSimilar(double left, double right, double tolerance)
        {
            System.Diagnostics.Debug.Assert(Math.Abs(left-right) <= tolerance);
        }
        [System.Diagnostics.Conditional("DEBUG")]
        [System.Diagnostics.DebuggerHiddenAttribute()]
        public static void AssertSimilar(double[] left, double[] right, double tolerance)
        {
            if(left.Length != right.Length) { DEBUG.Assert(false); return; }
            for(int i=0; i<left.Length; i++)
            {
                if(Math.Abs(left[i]-right[i]) <= tolerance)
                    continue;
                DEBUG.Assert(false);
                return;
            }
        }
        [System.Diagnostics.Conditional("DEBUG")]
        [System.Diagnostics.DebuggerHiddenAttribute()]
        public static void AssertSimilar(double[,] left, double[,] right, double tolerance)
        {
            if(left.GetLength(0) != right.GetLength(0)) { DEBUG.Assert(false); return; }
            if(left.GetLength(1) != right.GetLength(1)) { DEBUG.Assert(false); return; }
            for(int i=0; i<left.GetLength(0); i++)
            {
                for(int j=0; j<left.GetLength(1); j++)
                {
                    if(Math.Abs(left[i, j]-right[i, j]) <= tolerance)
                        continue;
                    DEBUG.Assert(false);
                    return;
                }
            }
        }
        //////////////////////////////////////////////
        [System.Diagnostics.Conditional("DEBUG")]
        [System.Diagnostics.DebuggerHiddenAttribute()]
        public static void AssertNotSimilar(double left, double right, double tolerance)
        {
            System.Diagnostics.Debug.Assert(Math.Abs(left-right) > tolerance);
        }
        [System.Diagnostics.Conditional("DEBUG")]
        [System.Diagnostics.DebuggerHiddenAttribute()]
        public static void AssertNotSimilar(double[] left, double[] right, double tolerance)
        {
            if(left.Length != right.Length) return;
            for(int i=0; i<left.Length; i++)
            {
                if(Math.Abs(left[i]-right[i]) <= tolerance)
                    continue;
                return;
            }
            DEBUG.Assert(false);
        }
        [System.Diagnostics.Conditional("DEBUG")]
        [System.Diagnostics.DebuggerHiddenAttribute()]
        public static void AssertNotSimilar(double[,] left, double[,] right, double tolerance)
        {
            if(left.GetLength(0) != right.GetLength(0)) return;
            if(left.GetLength(1) != right.GetLength(1)) return;
            for(int i=0; i<left.GetLength(0); i++)
            {
                for(int j=0; j<left.GetLength(1); j++)
                {
                    if(Math.Abs(left[i, j]-right[i, j]) <= tolerance)
                        continue;
                    return;
                }
            }
            DEBUG.Assert(false);
        }
        //////////////////////////////////////////////
        [System.Diagnostics.Conditional("DEBUG")]
        [System.Diagnostics.DebuggerHiddenAttribute()]
        public static void AssertNotNull<T>(T value)
            where T : class
        {
            System.Diagnostics.Debug.Assert(value != null);
        }
        [System.Diagnostics.Conditional("DEBUG")]
        [System.Diagnostics.DebuggerHiddenAttribute()]
        public static void AssertNotNull<T>(params T[] value)
            where T : class
        {
            bool assert = true;
            if(value == null)
                assert = false;
            else
                for(int i=0; i<value.Length; i++)
                    if(value[i] == null)
                        assert = false;
            DEBUG.Assert(assert);
        }
        [System.Diagnostics.Conditional("DEBUG")]
        [System.Diagnostics.DebuggerHiddenAttribute()]
        public static void AssertNotNull<T>(T[,] value)
            where T : class
        {
            bool assert = true;
            if(value == null)
                assert = false;
            else
                for(int i=0; i<value.GetLength(0); i++)
                    for(int j=0; j<value.GetLength(1); j++)
                        if(value[i,j] == null)
                            assert = false;
            DEBUG.Assert(assert);
        }
        //////////////////////////////////////////////
        [System.Diagnostics.DebuggerHiddenAttribute()]
        public static void Verify(bool condition)
        {
            System.Diagnostics.Debug.Assert(condition);
        }

        [System.Diagnostics.Conditional("DEBUG")]
        [System.Diagnostics.DebuggerHiddenAttribute()]
        public static void AssertOr(params bool[] conditions)
        {
            foreach(bool condition in conditions)
            {
                if(condition == true)
                {
                    return;
                }
            }
            System.Diagnostics.Debug.Assert(false);
        }

        [System.Diagnostics.Conditional("DEBUG")]
        [System.Diagnostics.DebuggerHiddenAttribute()]
        public static void AssertAnd(params bool[] conditions)
        {
            bool success = true;
            foreach(bool condition in conditions)
            {
                if(condition == false)
                {
                    success = false;
                }
            }
            System.Diagnostics.Debug.Assert(success);
        }

        [System.Diagnostics.Conditional("DEBUG")]
        [System.Diagnostics.DebuggerHiddenAttribute()]
        public static void AssertXor(params bool[] conditions)
        {
            int numsuccess = 0;
            foreach(bool condition in conditions)
            {
                if(condition == true)
                {
                    numsuccess++;
                }
            }
            System.Diagnostics.Debug.Assert(numsuccess == 1);
        }

        [System.Diagnostics.Conditional("DEBUG")]
        [System.Diagnostics.DebuggerHiddenAttribute()]
        public static void AssertIf(bool condition, params bool[] asserts)
        {
            if(condition)
            {
                bool assert = true;
                for(int i=0; i<asserts.Length; i++)
                    assert = assert && asserts[i];
                Assert(assert);
            }
        }

        static Dictionary<int, Dictionary<string, bool>> _ConditionalAssert = new Dictionary<int, Dictionary<string, bool>>();

        // <hashcode_for_name, <name, is_assert>>
        [System.Diagnostics.Conditional("DEBUG")]
        [System.Diagnostics.DebuggerHiddenAttribute()]
        public static void AssertIf(string name, bool initial_condition, bool assert)
        {
            int hash = name.GetHashCode();
            if(_ConditionalAssert.ContainsKey(hash) == false)
            {
                _ConditionalAssert[hash] = new Dictionary<string,bool>();
                _ConditionalAssert[hash][name] = initial_condition;
                AssertIf(initial_condition, assert);
                return;
            }
            if(_ConditionalAssert[hash].ContainsKey(name) == false)
            {
                _ConditionalAssert[hash][name] = initial_condition;
                AssertIf(initial_condition, assert);
                return;
            }
            AssertIf(_ConditionalAssert[hash][name], assert);
        }
        public bool this[string name]
        {
            get
            {
                int hash = name.GetHashCode();
                if(_ConditionalAssert.ContainsKey(hash) == false)
                    return false;
                if(_ConditionalAssert[hash].ContainsKey(name) == false)
                    return false;
                return _ConditionalAssert[hash][name];
            }
            set
            {
                int hash = name.GetHashCode();
                if(_ConditionalAssert.ContainsKey(hash) == false)
                    return;
                if(_ConditionalAssert[hash].ContainsKey(name) == false)
                    return;
                _ConditionalAssert[hash][name] = value;
            }
        }

        //static public bool IsDebuggerAttached
        //{
        //    get
        //    {
        //        return System.Diagnostics.Debugger.IsAttached;
        //    }
        //}
#if DEBUG
        public const bool IsDebuggerAttached = true;
#else
        public const bool IsDebuggerAttached = false;
#endif

        static public bool IsDebuggerAttachedWithProb(double prob)
        {
            if(System.Diagnostics.Debugger.IsAttached)
            {
                HDebug.Assert(0<=prob, prob<=1);
                double nrand = rand.NextDouble();
                return (nrand < prob);
            }
            return false;
        }
        [System.Diagnostics.Conditional("DEBUG")]
        [System.Diagnostics.DebuggerHiddenAttribute()]
        static public void Break()
        {
            System.Diagnostics.Debugger.Break();
        }
        [System.Diagnostics.Conditional("DEBUG")]
        [System.Diagnostics.DebuggerHiddenAttribute()]
        static public void Break(params bool[] conditions)
        {
            BreakOr(conditions);
        }
        [System.Diagnostics.Conditional("DEBUG")]
        [System.Diagnostics.DebuggerHiddenAttribute()]
        static public void BreakAnd(params bool[] conditions)
        {
            if(conditions.Length >= 1)
            {
                bool dobreak = true;
                foreach(bool condition in conditions)
                    dobreak = dobreak && condition;
                if(dobreak)
                    System.Diagnostics.Debugger.Break();
            }
        }
        [System.Diagnostics.Conditional("DEBUG")]
        [System.Diagnostics.DebuggerHiddenAttribute()]
        static public void BreakOr(params bool[] conditions)
        {
            if(conditions.Length >= 1)
            {
                bool dobreak = false;
                foreach(bool condition in conditions)
                    dobreak = dobreak || condition;
                if(dobreak)
                    System.Diagnostics.Debugger.Break();
            }
        }

        [System.Diagnostics.Conditional("DEBUG")]
        [System.Diagnostics.DebuggerHiddenAttribute()]
        static public void ToDo(params string[] todos)
        {
            foreach(string todo in todos)
                System.Console.Error.WriteLine("TODO: " + todo);
            Break();
        }

        [System.Diagnostics.Conditional("DEBUG")]
        [System.Diagnostics.DebuggerHiddenAttribute()]
        static public void Depreciated(params string[] messages)
        {
            foreach(string message in messages)
                System.Console.Error.WriteLine("Depreciated: " + message);
            Break();
        }

        public static class Trace
        {
            public static bool AutoFlush          { set{ System.Diagnostics.Trace.AutoFlush   = value; } get{ return System.Diagnostics.Trace.AutoFlush  ; } }
            public static int  IndentLevel        { set{ System.Diagnostics.Trace.IndentLevel = value; } get{ return System.Diagnostics.Trace.IndentLevel; } }
            public static int  IndentSize         { set{ System.Diagnostics.Trace.IndentSize  = value; } get{ return System.Diagnostics.Trace.IndentSize ; } }
            public static System.Diagnostics.CorrelationManager      CorrelationManager {                get{ return System.Diagnostics.Trace.CorrelationManager; } }
            public static System.Diagnostics.TraceListenerCollection Listeners          {                get{ return System.Diagnostics.Trace.Listeners         ; } }

            [System.Diagnostics.Conditional("TRACE")]    public static void Flush()    { System.Diagnostics.Trace.Flush();    }
            [System.Diagnostics.Conditional("TRACE")]    public static void Indent()   { System.Diagnostics.Trace.Indent();   }
            [System.Diagnostics.Conditional("TRACE")]    public static void Unindent() { System.Diagnostics.Trace.Unindent(); }
                                                        public static void Refresh()  { System.Diagnostics.Trace.Refresh();  }

            [System.Diagnostics.Conditional("TRACE")]    public static void Write(object value)                                          { System.Diagnostics.Trace.Write(value);                              }
            [System.Diagnostics.Conditional("TRACE")]    public static void Write(string message)                                        { System.Diagnostics.Trace.Write(message);                            }
            [System.Diagnostics.Conditional("TRACE")]    public static void Write(object value, string category)                            { System.Diagnostics.Trace.Write(value, category);                    }
            [System.Diagnostics.Conditional("TRACE")]    public static void Write(string message, string category)                        { System.Diagnostics.Trace.Write(message, category);                  }
            [System.Diagnostics.Conditional("TRACE")]    public static void WriteIf(bool condition, object value)                        { System.Diagnostics.Trace.WriteIf(condition, value);                 }
            [System.Diagnostics.Conditional("TRACE")]    public static void WriteIf(bool condition, string message)                        { System.Diagnostics.Trace.WriteIf(condition, message);               }
            [System.Diagnostics.Conditional("TRACE")]    public static void WriteIf(bool condition, object value, string category)        { System.Diagnostics.Trace.WriteIf(condition, value, category);       }
            [System.Diagnostics.Conditional("TRACE")]    public static void WriteIf(bool condition, string message, string category)        { System.Diagnostics.Trace.WriteIf(condition, message, category);     }
            [System.Diagnostics.Conditional("TRACE")]    public static void WriteLine(object value)                                        { System.Diagnostics.Trace.WriteLine(value);                          }
            [System.Diagnostics.Conditional("TRACE")]    public static void WriteLine(string message)                                    { System.Diagnostics.Trace.WriteLine(message);                        }
            [System.Diagnostics.Conditional("TRACE")]    public static void WriteLine(object value, string category)                        { System.Diagnostics.Trace.WriteLine(value, category);                }
            [System.Diagnostics.Conditional("TRACE")]    public static void WriteLine(string message, string category)                    { System.Diagnostics.Trace.WriteLine(message, category);              }
            [System.Diagnostics.Conditional("TRACE")]    public static void WriteLineIf(bool condition, object value)                    { System.Diagnostics.Trace.WriteLineIf(condition, value);             }
            [System.Diagnostics.Conditional("TRACE")]    public static void WriteLineIf(bool condition, string message)                    { System.Diagnostics.Trace.WriteLineIf(condition, message);           }
            [System.Diagnostics.Conditional("TRACE")]    public static void WriteLineIf(bool condition, object value, string category)    { System.Diagnostics.Trace.WriteLineIf(condition, value, category);   }
            [System.Diagnostics.Conditional("TRACE")]    public static void WriteLineIf(bool condition, string message, string category)    { System.Diagnostics.Trace.WriteLineIf(condition, message, category); }
        }
        public static class TraceFile
        {
            static System.IO.StreamWriter writer = System.IO.File.CreateText("TRACE.TXT");
            //[System.Diagnostics.Conditional("TRACE")]    public static void Write(object value)                                          { writer.Write(value);                              writer.Flush(); }
            [System.Diagnostics.Conditional("TRACE")]    public static void Write(string message)                                        { writer.Write(message);                            writer.Flush(); }
            //[System.Diagnostics.Conditional("TRACE")]    public static void Write(object value, string category)                            { writer.Write(value, category);                    writer.Flush(); }
            [System.Diagnostics.Conditional("TRACE")]    public static void Write(string message, string category)                        { writer.Write(message, category);                  writer.Flush(); }
            //[System.Diagnostics.Conditional("TRACE")]    public static void WriteIf(bool condition, object value)                        { writer.WriteIf(condition, value);                 writer.Flush(); }
            //[System.Diagnostics.Conditional("TRACE")]    public static void WriteIf(bool condition, string message)                        { writer.WriteIf(condition, message);               writer.Flush(); }
            //[System.Diagnostics.Conditional("TRACE")]    public static void WriteIf(bool condition, object value, string category)        { writer.WriteIf(condition, value, category);       writer.Flush(); }
            //[System.Diagnostics.Conditional("TRACE")]    public static void WriteIf(bool condition, string message, string category)        { writer.WriteIf(condition, message, category);     writer.Flush(); }
            //[System.Diagnostics.Conditional("TRACE")]    public static void WriteLine(object value)                                        { writer.WriteLine(value);                          writer.Flush(); }
            [System.Diagnostics.Conditional("TRACE")]    public static void WriteLine(string message)                                    { writer.WriteLine(message);                        writer.Flush(); }
            //[System.Diagnostics.Conditional("TRACE")]    public static void WriteLine(object value, string category)                        { writer.WriteLine(value, category);                writer.Flush(); }
            [System.Diagnostics.Conditional("TRACE")]    public static void WriteLine(string message, string category)                    { writer.WriteLine(message, category);              writer.Flush(); }
            //[System.Diagnostics.Conditional("TRACE")]    public static void WriteLineIf(bool condition, object value)                    { writer.WriteLineIf(condition, value);             writer.Flush(); }
            //[System.Diagnostics.Conditional("TRACE")]    public static void WriteLineIf(bool condition, string message)                    { writer.WriteLineIf(condition, message);           writer.Flush(); }
            //[System.Diagnostics.Conditional("TRACE")]    public static void WriteLineIf(bool condition, object value, string category)    { writer.WriteLineIf(condition, value, category);   writer.Flush(); }
            //[System.Diagnostics.Conditional("TRACE")]    public static void WriteLineIf(bool condition, string message, string category)    { writer.WriteLineIf(condition, message, category); writer.Flush(); }
        }
    }
}
