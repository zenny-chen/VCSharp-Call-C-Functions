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

而在下面的“MyNativeModifyString”函数中，由于MSVC的字符编码集变换莫测，对于不同系统语言环境会采用不同的编码集，因此这里笔者直接对中文字符使用了Unicode转义字符。

```c
    // 后面 \u 的几个转义字符分别为：“你好世界”
    const char16_t *utf16Str = u"This is a UTF-16 string from native code!! \u4f60\u597d\u4e16\u754c";
```

关于C语言中采用Unicode转义字符可参考这篇博文：[C11中的通用字符名（Universal Character Names）](https://www.jianshu.com/p/0edabe77a5a1)

