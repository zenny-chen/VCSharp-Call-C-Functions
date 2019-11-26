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
