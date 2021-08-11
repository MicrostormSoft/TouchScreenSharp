using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace TouchScreenSharp
{
    public class TouchScreen : IDisposable
    {
        Stream devicestream;
        Dictionary<int, TouchEventArgs> touches;
        int current = 0;

        public Thread Worker;

        public event Action<object, TouchEventArgs> OnMove;
        public event Action<object, TouchEventArgs> OnPress;
        public event Action<object, TouchEventArgs> OnRelease;

        public bool Debug = false;

        private bool flag_run = true;



        public TouchScreen(string device = "/dev/input/event1", bool buildinthread = true)
        {
            devicestream = File.OpenRead(device);
            touches = new Dictionary<int, TouchEventArgs>();
            touches.Add(current, new TouchEventArgs());
            Worker = new Thread(new ThreadStart(() =>
            {
                while (flag_run) run();
            }));
            if (buildinthread) Worker.Start();
        }

        private void DebugLog(string log)
        {
            if (Debug) Console.WriteLine(log);
        }

        public void run()
        {
            AfterallEventType type = AfterallEventType.Move;
            while (true)
            {
                var eve = ReadEvent();

                switch (eve.Type)
                {
                    case EventType.EV_ABS:
                        DebugLog("ABS");
                        if (!touches.ContainsKey(0))
                        {
                            touches.Add(0, new TouchEventArgs());
                            DebugLog("# Default Newslot ");
                        }
                        switch (eve.Code)
                        {
                            case KeyCode.ABS_MT_SLOT:
                                current = eve.Value;
                                DebugLog("# SwitchSlot " + current);
                                if (!touches.ContainsKey(current))
                                {
                                    touches.Add(current, new TouchEventArgs() { ID = current });

                                    type = AfterallEventType.Press;
                                    DebugLog("# NewSlot ");
                                }
                                break;
                            case KeyCode.ABS_MT_POSITION_X:
                            case KeyCode.ABS_X:
                                DebugLog("# SetXPos " + eve.Value);
                                touches[current].X = eve.Value;
                                break;

                            case KeyCode.ABS_MT_POSITION_Y:
                            case KeyCode.ABS_Y:
                                DebugLog("# SetYPos " + eve.Value);
                                touches[current].Y = eve.Value;
                                break;

                            case KeyCode.ABS_MT_TOUCH_MAJOR:
                                DebugLog("# TouchMajor " + eve.Value);
                                touches[current].TouchMajor = eve.Value;
                                break;

                            case KeyCode.ABS_MT_WIDTH_MAJOR:
                                DebugLog("# WidthMajor " + eve.Value);
                                touches[current].WidthMajor = eve.Value;
                                break;

                            case KeyCode.ABS_MT_TRACKING_ID:
                                DebugLog("# Tracking ID " + eve.Value);
                                if (eve.Value == -1)
                                {
                                    DebugLog("# ReleaseSlot " + current);
                                    type = AfterallEventType.Release;
                                }
                                break;
                        }
                        break;
                    case EventType.EV_KEY:
                        switch (eve.Code)
                        {
                            case KeyCode.BTN_TOUCH:
                                if (eve.Value == 1) type = AfterallEventType.Press;
                                else type = AfterallEventType.Release;
                                DebugLog("# TOUCH " + type);
                                break;
                        }
                        break;
                    case EventType.EV_SYN:
                        switch (type)
                        {
                            case AfterallEventType.Move:
                                OnMove?.Invoke(this, touches[current]);
                                break;
                            case AfterallEventType.Press:
                                OnPress?.Invoke(this, touches[current]);
                                break;
                            case AfterallEventType.Release:
                                OnRelease?.Invoke(this, touches[current]);
                                touches.Remove(current);
                                break;
                        }
                        type = AfterallEventType.Move;
                        break;
                }
            }
        }

        public NativeTouchEventArgs ReadEvent()
        {
            return new NativeTouchEventArgs
            {
                Second = ReadInt32(),
                USecond = ReadInt32(),
                Type = (EventType)ReadInt16(),
                Code = (KeyCode)ReadInt16(),
                Value = ReadInt32()
            };
        }

        public int ReadInt32()
        {
            return ReadInt16() + (ReadInt16() << 16);
        }

        public int ReadInt16()
        {
            byte buf1 = (byte)devicestream.ReadByte();
            byte buf2 = (byte)devicestream.ReadByte();
            return (buf2 << 8) + buf1;
        }

        public void Dispose()
        {
            flag_run = false;
            devicestream.Close();
        }
    }
    public struct NativeTouchEventArgs
    {
        public int Second, USecond;
        public EventType Type;
        public KeyCode Code;
        public int Value;
    }

    public class TouchEventArgs
    {
        public int X, Y;
        public int Pressure;
        public int ID;
        public int TouchMajor, WidthMajor;
    }


    /*
    #define EV_SYN			0x00
    #define EV_KEY			0x01
    #define EV_REL			0x02
    #define EV_ABS			0x03
    #define EV_MSC			0x04
    #define EV_SW			0x05
    #define EV_LED			0x11
    #define EV_SND			0x12
    #define EV_REP			0x14
    #define EV_FF			0x15
    #define EV_PWR			0x16
    #define EV_FF_STATUS		0x17
    #define EV_MAX			0x1f
    #define EV_CNT			(EV_MAX+1)
     */
    public enum EventType
    {
        EV_SYN = 0x00, EV_KEY = 0x01, EV_REL = 0x02, EV_ABS = 0x03,
        EV_MSC = 0x04, EV_SW = 0x05, EV_LED = 0x11, EV_SND = 0x12,
        EV_REP = 0x14, EV_FF = 0x15, EV_PWR = 0x16, EV_FF_STATUS = 0x17,
        EV_MAX = 0x1f, EV_CNT = 0x1f + 1
    }

    public enum KeyCode
    {
        NOTHING = 0x00,
        ABS_X = 0x00, ABS_Y = 0x01,
        ABS_PRESSURE = 0x18, BTN_TOUCH = 0x14A,
        ABS_MT_POSITION_X = 0x35, ABS_MT_POSITION_Y = 0x36,
        ABS_MT_SLOT = 0x2f,//切换手指
        ABS_MT_TRACKING_ID = 0x39,
        ABS_MT_TOUCH_MAJOR = 0x30,//Touching直径
        ABS_MT_WIDTH_MAJOR = 0x32,//Approaching直径

    }

    public enum AfterallEventType
    {
        Move, Press, Release
    }
}
