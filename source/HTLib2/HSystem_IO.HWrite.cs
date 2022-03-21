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
using System.Security.AccessControl;
using System.Text;
using System.Runtime.CompilerServices;
using System.IO;

namespace CSAV.HTLib2
{
    using IList       = System.Collections.IList;
    using IDictionary = System.Collections.IDictionary;
    public static partial class HSystem_IO
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void HWrite(this BinaryWriter writer, double value) { writer.Write(value); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void HWrite(this BinaryWriter writer, int    value) { writer.Write(value); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void HWrite(this BinaryWriter writer, string value) { writer.Write(value); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void HWrite(this BinaryWriter writer, bool   value) { writer.Write(value); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void HWrite(this BinaryWriter writer, double[] values) { writer.Write(values.Length); for(int i=0; i<values.Length; i++) writer.Write(values[i]); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void HWrite(this BinaryWriter writer, int   [] values) { writer.Write(values.Length); for(int i=0; i<values.Length; i++) writer.Write(values[i]); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void HWrite(this BinaryWriter writer, string[] values) { writer.Write(values.Length); for(int i=0; i<values.Length; i++) writer.Write(values[i]); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void HWrite(this BinaryWriter writer, bool  [] values) { writer.Write(values.Length); for(int i=0; i<values.Length; i++) writer.Write(values[i]); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void HWrite(this BinaryWriter writer, List<double> values) { writer.Write(values.Count); for(int i=0; i<values.Count; i++) writer.Write(values[i]); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void HWrite(this BinaryWriter writer, List<int   > values) { writer.Write(values.Count); for(int i=0; i<values.Count; i++) writer.Write(values[i]); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void HWrite(this BinaryWriter writer, List<string> values) { writer.Write(values.Count); for(int i=0; i<values.Count; i++) writer.Write(values[i]); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void HWrite(this BinaryWriter writer, List<bool  > values) { writer.Write(values.Count); for(int i=0; i<values.Count; i++) writer.Write(values[i]); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void HWrite<T  >(this BinaryWriter writer, T               value ) { _HWrite          (writer, value ); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void HWrite<T  >(this BinaryWriter writer, List<T>         values) { _HWriteList      (writer, values); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void HWrite<T  >(this BinaryWriter writer, T[]             values) { _HWriteArray     (writer, values); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void HWrite<T,U>(this BinaryWriter writer, Dictionary<T,U> dict  ) { _HWriteDictionary(writer, dict  ); }

        static void _HWriteDouble(BinaryWriter writer, object value) { writer.Write((double)value); }
        static void _HWriteInt   (BinaryWriter writer, object value) { writer.Write((int   )value); }
        static void _HWriteString(BinaryWriter writer, object value) { writer.Write((string)value); }
        static void _HWriteBool  (BinaryWriter writer, object value) { writer.Write((bool  )value); }
        static void _HWriteBinarySerializable(BinaryWriter writer, object value)
        {
            if((value is IBinarySerializable) == false)
                throw new Exception();
            string type_name = value.GetType().AssemblyQualifiedName;
            writer.Write(type_name);
            ((IBinarySerializable)value).Serialize(writer);
        }
        static void _HWriteList(BinaryWriter writer, object value)
        {
            if((value is IList) == false)
                throw new Exception();
            IList values = (IList)value;
            writer.Write(values.Count);
            for(int i=0; i<values.Count; i++)
                _HWrite(writer, values[i]);
        }
        static void _HWriteArray(BinaryWriter writer, object value)
        {
            if((value is Array) == false)
                throw new Exception();
            Array values = (Array)value;
            writer.Write(values.Length);
            for(int i=0; i<values.Length; i++)
                _HWrite(writer, values.GetValue(i));
        }
        public static void _HWriteDictionary(this BinaryWriter writer, object value)
        {
            if((value is IDictionary) == false)
                throw new Exception();
            IDictionary dict = (IDictionary)value;
            writer.Write(dict.Count);
            var dict_enum = dict.GetEnumerator();
            //foreach(var key_val in dictenum)
            int cnt = 0;
            while(dict_enum.MoveNext())
            {
                cnt ++;
                _HWrite(writer, dict_enum.Key  );
                _HWrite(writer, dict_enum.Value);
            }
            HDebug.Assert(cnt == dict.Count);
        }
        static void _HWrite(BinaryWriter writer, object value)
        {
            //string type_name = value.GetType()AssemblyQualifiedName;
            if(value is IBinarySerializable) { _HWriteBinarySerializable(writer, value); return; }
            if(value is double             ) { _HWriteDouble            (writer, value); return; }
            if(value is int                ) { _HWriteInt               (writer, value); return; }
            if(value is string             ) { _HWriteString            (writer, value); return; }
            if(value is bool               ) { _HWriteBool              (writer, value); return; }
            if(value is IList              ) { _HWriteList              (writer, value); return; }
            if(value is Array              ) { _HWriteArray             (writer, value); return; }
            if(value is IDictionary        ) { _HWriteDictionary        (writer, value); return; }
            throw new Exception();
        }
    }
}
