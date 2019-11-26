// dllmain.c : 定义 DLL 应用程序的入口点。
#include <Windows.h>
#include <stdio.h>
#include <string.h>
#include <uchar.h>
#include <stdbool.h>

/**
 inputStr: 输入字符串
 outputMaxLen: 存放输出字符串缓存的最大长度（字节数）
 outputStr: 输出字符串
 pOutLength: 输出outputStr输出字符串的实际长度
 @return 输入字符串的长度
*/
__declspec(dllexport)
int APIENTRY MyNativeCPrintLine(const char *inputStr, int outputMaxLen, void *outputStr, void *pOutLength)
{
    const char* srcStr = u8"This is a native C function!!";
    const size_t srcLen = strlen(srcStr);

    strcpy_s(outputStr, outputMaxLen, srcStr);
    int* pLen = pOutLength;
    if (pLen != NULL)
        *pLen = (int)strlen(inputStr);

    printf("The string from C# is: %s\n", inputStr);

    return (int)srcLen;
}


/**
 outputMaxLen: 存放输出字符串缓存的最大长度（字节数）
 outputStr: 存放输出字符串的缓存
 pOutStrLen: 指向实际输出字符串长度变量的指针
 @return 若给定的输出缓存大小小于输出字符串长度，则返回false；否则返回true
 */
__declspec(dllexport)
bool APIENTRY MyNativeModifyString(int outputMaxlen, char16_t* outputStr, int *pOutStrLen)
{
    // 后面 \u 的几个转义字符分别为：“你好世界”
    const char16_t *utf16Str = u"This is a UTF-16 string from native code!! \u4f60\u597d\u4e16\u754c";

    const int utf16StrLen = lstrlenW(utf16Str);
    if (outputMaxlen <= utf16StrLen)
        return false;

    lstrcpyW(outputStr, utf16Str);

    if (pOutStrLen != NULL)
        *pOutStrLen = (int)utf16StrLen;

    return true;
}
