# TouchScreenSharp
Touch screen support on Linux for .Net  
.NET Linux上的触摸屏支持

[![NuGet version (TouchScreenSharp)](https://img.shields.io/nuget/v/TouchScreenSharp.svg?style=flat)](https://www.nuget.org/packages/TouchScreenSharp/)

## What is this 是什么
This is a touchscreen adapter for C# to do basic touch inputs. 
这是为C#设计的基础触摸输入库，可以完成基础的触摸输入。

## Setup 安装
Just import the nuget package of this repo into your dotnet project.
把这个项目的nuget包导入到你的.net项目里就完成了。

## How to use 如何使用
```csharp
static void Main(string[] args)
{
  // Open touch input device 打开触摸输入设备
  TouchScreen t = new TouchScreen("/dev/input/event1");
  
  // Bind events 绑定事件
  t.OnPress += T_OnPress;
  t.OnMove += T_OnMove;
  t.OnRelease += T_OnRelease;
}

private static void T_OnPress(object arg1, TouchEventArgs arg2)
{
  // arg2.ID Tell you which object on screen triggered this event.
  // Say you put two fingers on screen, then the first finger is labeled 0,
  // the second one is labeled 1.
  // arg2.ID 用于检测是哪个接触屏幕的物体触发了事件.
  // 例如，你将两个手指放到屏幕上，第一个手指会被标注为0,第二个标注为1.
  Console.WriteLine(arg2.ID + " - " + arg2.X + "," + arg2.Y + " Press");
}

private static void T_OnRelease(object arg1, TouchEventArgs arg2)
{
  Console.WriteLine(arg2.ID + " - " + arg2.X + "," + arg2.Y + " Release");
}

private static void T_OnMove(object arg1, TouchEventArgs arg2)
{
  Console.WriteLine(arg2.ID + " - " + arg2.X + "," + arg2.Y + " Move");
}

```
