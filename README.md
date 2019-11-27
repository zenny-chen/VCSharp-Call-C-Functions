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

![1](https://github.com/zenny-chen/VCSharp-Call-C-Functions/blob/master/images/1.JPG)

点击“修改”按钮之后，我们进入到插件选择界面。这里我们选择Windows下的三套组件，如下图所示。

![2](https://github.com/zenny-chen/VCSharp-Call-C-Functions/blob/master/images/2.JPG)

最后，如果我们需要在本地端使用Clang编译工具链的话，可以在右侧栏，找到“使用C++的桌面开发”一栏，展开，然后勾选上“适用于Windows的C++ Clang工具”，如下图所示。这可使得我们使用同时兼容于GNU语法扩展以及Windows独有语法扩展的C语言！

![3](https://github.com/zenny-chen/VCSharp-Call-C-Functions/blob/master/images/3.JPG)

全都选完之后，我们点击右下角的“下载安装”即可。完成安装之后，我们打开Visual Studio 2019，来到欢迎界面。点击右侧“开始使用”下面的“创建新项目”，如下图所示。
