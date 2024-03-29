# VC# Call C Functions
VC#调用C函数样例详解

<br />

当前，网上对于C#如何调用C语言函数的博文已经有不少了。不过针对C#到C语言的类型映射相关的文章还比较少，因此本文将比较详细地描述从C#端如何给C语言端传参，C语言端又如何把数据再送往C#端的一些知识点。

微软官方的MSDN中有讲解到从托管代码调用本地函数的主题（Calling Native Functions from Managed Code），这其中有一张本地C语言类型到公共语言运行时类型的映射表，个人认为需要先拿出来。由于我们这里主要关注C#与C语言，因此我将这个表中的公共语言运行时类型直接改为C#所支持的类型。

 wtypes.h | MSC | C#
 ---- | ----- | ------
 HANDLE | void \* | IntPtr, UIntPtr
 BYTE | unsigned char | Byte
 SHORT | short | Int16
 WORD | unsigned short | UInt16
 INT | int | Int32
 UINT | unsigned int | UInt32
 LONG | long | Int32
 BOOL | long | Boolean
 DWORD | unsigned long | UInt32
 ULONG | unsigned long | UInt32
 CHAR | char | Char
 LPSTR | char \* | String [in], StringBuilder [in, out]
 LPCSTR | const char \* | String
 LPWSTR | wchar_t \*, char16_t \* | String [in], StringBuilder [in, out]
 LPCWSTR | const wchar_t \*, const char16_t \* | String
 FLOAT | float | Single
 DOUBLE | double | Double

有了上述这张表之后，我们就能从容判断在C#端所声明的本地C函数的返回类型以及参数类型了，并且也能判断出如何合理地安排参数与返回类型来向本地C语言端输出/获取相关数据。

由于C#需要运行在.NET运行时环境，.NET相当于一个虚拟机，因此如果C#想要与本地C函数进行交互的话需要将本地C代码打包成动态连接库（在Windows系统上为 **dll** 文件，在类Unix系统上为 **so** 文件，在macOS系统上则为 **dylib** 文件）的形式，然后让C#程序在运行时加载动态连接库指定的函数名加以执行。由于笔者这里的系统环境为Windows 10，开发工具为Visual Studio 2019 Community，因此为了方便叙述，后面直接将“动态连接库”称为“dll文件”了。

本仓库提供了两个工程项目，上面那个“CSharpInvokeNativeCFunction”是C#工程，下面那个“MyDLL”则是基于C语言的DLL工程，各位可以直接拿来使用。这里考虑到有些新手对Visual Studio用得可能不是很习惯，因此这里笔者将详细地描述DLL工程的创建过程。

我们首先在Windows开始菜单中找到Visual Studio Installer，打开它，如下图所示。

<br />

![1](https://github.com/zenny-chen/VCSharp-Call-C-Functions/blob/master/images/1.JPG)

<br />

点击“修改”按钮之后，我们进入到插件选择界面。这里我们选择Windows下的三套组件，如下图所示。

<br />

![2](https://github.com/zenny-chen/VCSharp-Call-C-Functions/blob/master/images/2.JPG)

<br />

最后，如果我们需要在本地端使用Clang编译工具链的话，可以在右侧栏，找到“使用C++的桌面开发”一栏，展开，然后勾选上“适用于Windows的C++ Clang工具”，如下图所示。这可使得我们使用同时兼容于GNU语法扩展以及Windows独有语法扩展的C语言！

<br />

![3](https://github.com/zenny-chen/VCSharp-Call-C-Functions/blob/master/images/3.JPG)

<br />

全都选完之后，我们点击右下角的“下载安装”即可。完成安装之后，我们打开Visual Studio 2019，来到欢迎界面。点击右侧“开始使用”下面的“创建新项目”，如下图所示。

<br />

![4](https://github.com/zenny-chen/VCSharp-Call-C-Functions/blob/master/images/4.JPG)

<br />

随后，在右侧栏仔细寻找“**动态链接库（DLL）**”，并且底下标签分别有：**C++**、 **Windows**、 **库**，如下图所示：

<br />

![5](https://github.com/zenny-chen/VCSharp-Call-C-Functions/blob/master/images/5.JPG)

<br />

点击“下一步”按钮，开始设置项目名与目录，如下图所示。

<br />

![6](https://github.com/zenny-chen/VCSharp-Call-C-Functions/blob/master/images/6.JPG)

<br />

最后点击“创建按钮”，Visual Studio将会把新建的项目创建在指定的目录上，并立即打开默认初始的C++文件。

由于我们这里并不需要臃肿不堪的C艹，而只需用C即可完成任务，因此我们这里先将默认生成的dllmain.cpp文件从项目中移除出去，如下图所示。

<br />

![7](https://github.com/zenny-chen/VCSharp-Call-C-Functions/blob/master/images/7.JPG)

<br />

我们在右边的项目导航栏中展开“源文件”，然后鼠标右键点击“dllmain.cpp”，选择“移除”，弹出提示对话框之后再点击“移除”按钮，先将该文件从当前工程项目中移除，我们后续还要使用。

完成之后，我们打开文件资源管理器，找到当前DLL项目工程中所存放的dllmain.cpp文件，将它改名为“dllmain.c”，然后为了能方便跨平台浏览编辑，我们将此文件转用UTF-8编码格式。方法为：用记事本打开它，然后在菜单栏选择“文件”，点击“另存为”，在底下“编码”那一栏中选择“UTF-8”，点击保存覆盖原来的即可，如下图所示。

<br />

![8](https://github.com/zenny-chen/VCSharp-Call-C-Functions/blob/master/images/8.JPG)

<br />

完成之后我们回到Visual Studio，鼠标右键点击项目导航栏中的“源文件”，然后选择“添加”，再选择“现有项”，选中dllmain.c，将此文件添加回来，如下图所示。

<br />

![9](https://github.com/zenny-chen/VCSharp-Call-C-Functions/blob/master/images/9.JPG)

<br />

最后，我们将工具栏中的“解决方案配置”改为“Release”，将“解决方案平台”改为“x64”即可，如下图所示。完成之后，我们就可以正式编辑代码了。

<br />

![10](https://github.com/zenny-chen/VCSharp-Call-C-Functions/blob/master/images/10.JPG)

<br />

下面我们可以直接将本仓库中MyDLL项目中的dllmain.c代码复制黏贴到自己的C文件中，然后这里需要讲解几个点。

我们先看一下“MyNativeCPrintLine”函数的声明：

```c
/**
 inputStr: 输入字符串
 outputMaxLen: 存放输出字符串缓存的最大长度（字节数）
 outputStr: 输出字符串
 pOutLength: 输出outputStr输出字符串的实际长度
 @return 输入字符串的长度
*/
__declspec(dllexport)
int APIENTRY MyNativeCPrintLine(const char *inputStr, int outputMaxLen, void *outputStr, void *pOutLength)
```

这里，`__declspec(dllexport)`是MSVC特有的声明符，指示当前所声明的函数作为DLL外部可见的函数。Windows系统的dll文件格式与类Unix系统的so文件格式有所不同。默认情况下，so动态链接库中所有外部符号都能被其他应用程序加载，除非显式使用私有（private）或内部（internal）可见性属性（visibility）进行声明。而Windows系统下的dll则不同，它需要显式指定当前的外部函数是否能被其他程序加载，这里就需要通过`__declspec(dllexport)`进行声明。

这里的`APIENTRY`宏表示的是`__stdcall`函数调用约定，这对于x86_64模式下是不用去关心的，但考虑到通用性以及可移植性，我们这里还是把它保留比较好。在32位的x86模式下，`__stdcall`函数调用约定是Windows系统下公共的、可跨语言调用的一种函数调用约定，因此也被称为是API调用约定。

而在下面的“MyNativeModifyString”函数中，由于MSVC的字符编码集变幻莫测，对于不同系统语言环境会采用不同的编码集，因此这里笔者直接对中文字符使用了Unicode转义字符。

```c
    // 后面 \u 的几个转义字符分别为：“你好世界”
    const char16_t *utf16Str = u"This is a UTF-16 string from native code!! \u4f60\u597d\u4e16\u754c";
```

关于C语言中采用Unicode转义字符可参考这篇博文：[C11中的通用字符名（Universal Character Names）](https://www.jianshu.com/p/0edabe77a5a1)。

代码编写完毕后，我们就可以点击工具栏中绿色小箭头进行构建，这里VS肯定会提示报错，称找到可执行文件，我们无需理睬。生成完之后，我们可以在当前工程项目目录中的`x64/Release/`找到一个dll和一个lib文件，我们可以先把这两个文件复制出来，待会儿要用。

<br />

接着，我们开始创建C#控制台项目工程。我们可以先关闭一下Visual Studio，然后再重新打开回到欢迎界面，点击“创建新项目”按钮，然后这里需要选择“控制台应用（.NET Core）”，并且底下标签栏含有“ **C#** ”的工程项目，如下图所示。

<br />

![11](https://github.com/zenny-chen/VCSharp-Call-C-Functions/blob/master/images/11.JPG)

<br />

创建完之后，我们可以将本仓库中“CSharpInvokeNativeCFunction”项目中的“Program.cs”文件中的内容复制到自己的C#源文件中。然后我们可以点击工具栏上的绿色小箭头编译运行一把。当然，这次编译不会有问题，但运行时会报错，由于找不到所需要的DLL文件。此时，我们可以将刚从DLL项目中所生成的dll文件和lib文件复制到当前C#工程项目的`bin/Debug/netcoreapp3.0/`目录下，此目录正好也存放着当前C#项目所生成的exe文件。再次运行就能顺利跑通了。

下面，我们开始详细讨论C#代码内容。

我们先看在C#端要对本地C函数进行调用的函数声明：

```cs
[DllImport("MyDLL.dll", EntryPoint = "MyNativeCPrintLine", CharSet = CharSet.Ansi)]
private static extern int MyNativeCPrintLine(string inputStr, int outputMaxLen, IntPtr outputStr, IntPtr pOutLength);

        
[DllImport("MyDLL.dll", EntryPoint = "MyNativeModifyString", CharSet = CharSet.Unicode)]
private static extern bool MyNativeModifyString(int outputMaxlen, StringBuilder outputStr, IntPtr pOutStrLen);
```

我们后面在 **Main** 静态方法中将要调用本地函数“MyNativeCPrintLine”和“MyNativeModifyString”，因此这里需要对这两个外部函数进行`DllImport`的外部声明。C#中的`DllImport`特性拥有非常丰富的字段、属性以及方法，详细信息可参考微软官方提供的[DllImportAttribute Class](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.dllimportattribute?redirectedfrom=MSDN&view=netframework-4.8)介绍。而我们这里则介绍最基本且常用的字段。

DllImport的第一个参数总是为指明当前所要加载的dll文件名的字符串。后面的字段不需要特定次序。其中，`EntryPoint`指定当前所要加载本地代码的入口点，直接将它指定为所声明的C函数名即可。这个特性可使得.NET运行时可高效地定位所加载的必要代码，而无需一股脑地把所指定的动态链接库的内容全都加载进来。`CharSet`特性则指定当前本地函数所使用的字符串的编码格式。这个特性也非常重要，它灵活地给出了C#端字符串要为本地C函数提供怎样的字符编码接口。我们从本文一开始的类型映射表上可以看到，无论是`char*`还是`char16_t*`都能映射到C#端的`String`或`StringBuilder`类型，因此到底这俩类型提供给native怎样的编码操作，则可以通过此`CharSet`字段进行指定。其中，`CharSet.Ansi`表示使用ASCII或多字节编码格式，包括UTF-8；而`CharSet.Unicode`则表示使用双字节的UTF-16编码格式。由于在“MyNativeCPrintLine”函数实现中，我们用char类型去获取输入字符串内容，因此这里使用`CharSet.Ansi`字符编码；而在“MyNativeModifyString”函数中，我们需要向C#端输出16位的UTF-16字符串内容，因此需要使用`CharSet.Unicode`字符编码进行指定。

由于我们这里只在当前类调用“MyNativeCPrintLine”和“MyNativeModifyString”这两个函数，因此它们都用`private`访问权限，如果你有其他需求，也可以改用其他访问权限。然后，我们可以通过本文一开始给出的类型映射表来观察C#端对本地C函数的声明中返回类型与参数类型的映射。

下面介绍一个比较重要的知识点：C#端如何从本地C函数端获取其输出数据。我们已经知道，C#端基于.NET运行时框架，其存储器模型都是受托管的，此时就需要有一种接口能向本地C函数暴露其平凡的存储器缓存地址。C#引入了`GCHandle`类，可以允许从一个不受托管的存储器访问一个受托管的一个对象，一个`GCHandle`对象也可称作为一个面向本地的（不受托管的）“指针”对象。通过调用`GCHandle.Alloc`方法，我们可以为指定的受托管的C#对象建立一个“指针”句柄。其第一个参数用于指定要给不受托管的本地端所暴露的C#端受托管的对象；第二个参数用于指定句柄类型，我们这里需要将它指定为`GCHandleType.Pinned`，因为我们后续需要获取该句柄所指向的本地存储器所存放的数据内容。如果一个`GCHandle`对象使用了`GCHandleType.Pinned`进行分配，那么它在用完后必须使用`Free()`成员方法进行显式释放。

下面我们参考这几行代码：

```cs
var outputBytes = new sbyte[256];
var outputCharBufferPtr = GCHandle.Alloc(outputBytes, GCHandleType.Pinned);
var outLenPtr = GCHandle.Alloc(0, GCHandleType.Pinned);
var len = MyNativeCPrintLine("Hello, world", outputBytes.Length, outputCharBufferPtr.AddrOfPinnedObject(), outLenPtr.AddrOfPinnedObject());

outputCharBufferPtr.Free();
```

上述代码中，`outputCharBufferPtr.AddrOfPinnedObject()`即为`IntPtr`类型，表示当前句柄所指向的C#端托管对象给本地端所暴露的底层地址。这样在C语言端就能直接把字节流数据放到以此地址作为起始地址的存储器缓存中了。

另外我们需要当心C#中传值传引用的问题。由于C#对于基本类型采取的是“传值”机制，因此如果我们就想对一个`int`数据或`float`数据的C#对象暴露给本地C函数端作为输出结果的话，我们不能单独建一个基本类型对象，然后将它传递给`GCHandle.Alloc`，因为它传递的是值，而不是该类型对象自身的引用，因此，所分配的GCHandle对象不会指向该基本类型对象。此时，我们可以直接用`GCHandle.Alloc`分配自己想要的基本数据类型的句柄，然后直接给出其地址即可。然后可以通过GCHandle对象的`Target`属性来获取它所指向的基本数据类型对象的值。我们可以参考以下代码：

```cs
var outLenPtr = GCHandle.Alloc(0, GCHandleType.Pinned);
var len = MyNativeCPrintLine("Hello, world", outputBytes.Length, outputCharBufferPtr.AddrOfPinnedObject(), outLenPtr.AddrOfPinnedObject());

Console.WriteLine("The input string length is: " + outLenPtr.Target);

outLenPtr.Free();
```

如此一来，我们只要根据本文一开始所列出的类型映射表，我们就可以将任意数据结构在C#端与本地C语言端进行传递了，这样在接口协定上确实比JNI要简洁不少。

<br />

最后，本demo还简单介绍了如何将C#端的类方法传递给本地C语言端做回调。首先我们先定义要传递的C#类方法的委托类型，然后用这些委托类型去声明类属性做全局初始化，使得这些委托对象能被一直保持住而不会被释放掉。随后，我们需要在本地C语言端定义一个注册函数，该函数将被C#端调用，使得C#端能把本地C语言端想要回调的类方法传递给C语言端。在C语言端保持住通过注册函数所传递过来的来自C#端的类方法之后，我们就可以随时进行调用了。


