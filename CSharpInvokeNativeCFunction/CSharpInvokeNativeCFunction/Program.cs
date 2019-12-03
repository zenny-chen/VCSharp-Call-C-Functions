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

        /// <summary>
        /// 声明一个委托，用于将指定类方法传递给本地C语言函数进行回调
        /// </summary>
        /// <param name="array">指向一个整型数组</param>
        /// <param name="length">指定整型数组的长度</param>
        private delegate void ArrayFuncDelegate(IntPtr array, int length);

        [DllImport("MyDLL.dll", EntryPoint = "RegisterCallbackMethods")]
        private static extern void RegisterCallbackMethods(ArrayFuncDelegate csharpMethod);


        [DllImport("MyDLL.dll", EntryPoint = "TestCSharpMethodCallback")]
        private static extern void TestCSharpMethodCallback();

        /// <summary>
        /// 这里全局初始化类属性theFuncDelegate，使得它能被一直hold住，而不被释放
        /// </summary>
        private static ArrayFuncDelegate theFuncDelegate = new ArrayFuncDelegate(CSharpMethod);

        /// <summary>
        /// 将在本地C语言端进行回调的C#类方法
        /// </summary>
        /// <param name="array">指向一个整型数组</param>
        /// <param name="length">指定整型数组的长度</param>
        private static void CSharpMethod(IntPtr array, int length)
        {
            int sum = 0;
            unsafe
            {
                var arr = (int*)array.ToPointer();
                for (int i = 0; i < length; i++)
                    sum += arr[i];
            }

            Console.WriteLine("In the C# method, the sum is: " + sum);
        }

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

            Console.WriteLine("\n");

            // 测试从本地C语言端回调C#的类方法
            // 首先需要将C#端要被回调的类方法注册给本地C语言端
            RegisterCallbackMethods(theFuncDelegate);

            // 然后，调用本地C语言端的测试函数，它将会回调自己所注册的C#端的类方法
            TestCSharpMethodCallback();
        }
    }
}

