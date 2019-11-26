using System;
using System.Runtime.InteropServices;
using System.Text;

namespace CSharpInvokeNativeCFunction
{
    class Program
    {
        [DllImport("MyDLL.dll", EntryPoint = "MyNativeCPrintLine", CharSet = CharSet.Ansi)]
        private static extern int MyNativeCPrintLine(string inputStr, int outputMaxLen, IntPtr outputStr, IntPtr pOutLength);

        
        [DllImport("MyDLL.dll", EntryPoint = "MyNativeModifyString", CharSet = CharSet.Unicode)]
        private static extern bool MyNativeModifyString(int outputMaxlen, StringBuilder outputStr, IntPtr pOutStrLen);

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            // 测试MyNativeCPrintLine
            var outputBytes = new sbyte[256];
            var outputCharBufferPtr = GCHandle.Alloc(outputBytes, GCHandleType.Pinned);
            var outLenPtr = GCHandle.Alloc(0, GCHandleType.Pinned);
            var len = MyNativeCPrintLine("Hello, world", outputBytes.Length, outputCharBufferPtr.AddrOfPinnedObject(), outLenPtr.AddrOfPinnedObject());

            outputCharBufferPtr.Free();

            var sb = new StringBuilder();
            for (int i = 0; i < len; i++)
                sb.Append((char)outputBytes[i]);

            Console.WriteLine("The input string length is: " + outLenPtr.Target);

            outLenPtr.Free();

            Console.WriteLine("The string from native C is: " + sb);

            Console.WriteLine("The output string length is: " + len);

            Console.WriteLine("\n");

            // 测试MyNativeModifyString
            var outputStrLen = new int[1];
            var outputStrLenPtr = GCHandle.Alloc(outputStrLen, GCHandleType.Pinned);
            var sb2 = new StringBuilder(256);
            if(MyNativeModifyString(256, sb2, outputStrLenPtr.AddrOfPinnedObject()))
            {
                Console.WriteLine("sb2 content is: " + sb2);
                Console.WriteLine("sb2 len = " + sb2.Length + ", and outputStrLen = " + outputStrLen[0]);
            }
            else
                Console.WriteLine("MyNativeModifyString calling failed!!");

            outputStrLenPtr.Free();
        }
    }
}

