using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class FrameSet
{
    public static FrameSet NeiGuan = new FrameSet();
    public static FrameSet Players = new FrameSet();
    public static FrameSet Wings = new FrameSet();

    public int startFrameIndex = -1;
    public int resMaxFrame;
    public Dictionary<MirAction, Frame> Frames = new Dictionary<MirAction, Frame>();
    public List<Frame> EffectFrames = new List<Frame>();

    static FrameSet()
    {
        #region Player Frames
        Players.Frames.Add(MirAction.Standing, new Frame(0, 4, 4, 64));
        Players.Frames.Add(MirAction.Walking, new Frame(64, 6, 2, 64));
        Players.Frames.Add(MirAction.Running, new Frame(128, 6, 2, 64));
        Players.Frames.Add(MirAction.BeiZhan, new Frame(192, 1, 0, 8));
        Players.Frames.Add(MirAction.Attack1, new Frame(200, 6, 2, 64));
        Players.Frames.Add(MirAction.Attack2, new Frame(264, 6, 2, 64));
        Players.Frames.Add(MirAction.Attack3, new Frame(328, 8, 0, 64));
        Players.Frames.Add(MirAction.ShiFa, new Frame(392, 6, 2, 64));
        Players.Frames.Add(MirAction.ShiQu, new Frame(456, 2, 0, 16));
        Players.Frames.Add(MirAction.BeiJi, new Frame(472, 3, 5, 64));
        Players.Frames.Add(MirAction.Die, new Frame(536, 4, 4, 64));
        Players.resMaxFrame = Players.Frames[MirAction.Die].FrameIndex;
        #endregion

        #region Wings Frames
        Wings.Frames.Add(MirAction.Standing, new Frame(0, 8, 0, 64));
        Wings.Frames.Add(MirAction.Walking, new Frame(64, 6, 2, 64));
        Wings.Frames.Add(MirAction.Running, new Frame(128, 6, 2, 64));
        Wings.Frames.Add(MirAction.BeiZhan, new Frame(192, 1, 0, 8));
        Wings.Frames.Add(MirAction.Attack1, new Frame(200, 6, 2, 64));
        Wings.Frames.Add(MirAction.Attack2, new Frame(264, 6, 2, 64));
        Wings.Frames.Add(MirAction.Attack3, new Frame(328, 8, 0, 64));
        Wings.Frames.Add(MirAction.ShiFa, new Frame(392, 6, 2, 64));
        Wings.Frames.Add(MirAction.ShiQu, new Frame(456, 2, 0, 16));
        Wings.Frames.Add(MirAction.BeiJi, new Frame(472, 3, 5, 64));
        Wings.Frames.Add(MirAction.Die, new Frame(536, 4, 4, 64));
        Wings.resMaxFrame = Wings.Frames[MirAction.Die].FrameIndex;
        #endregion

        NeiGuan.Frames.Add(MirAction.NeiGuan, new Frame(0, 60, 0, 60, false, false, MirDirection.UpRight));
    }
}

public class Frame
{
    public int Start, Count, Skip, MaxCount;
    public bool Alpha = false;
    public bool EffectAlpha = false;
    public MirDirection direction;

    public int OffSet
    {
        get { return Count + Skip; }
    }

    public int FrameIndex
    {
        get { return Start + MaxCount; }
    }

    public Frame(int start, int count, int skip, int maxCount, bool alpha = false, bool effectAlpha = false, MirDirection mirDirection = MirDirection.UpLeft)
    {
        Start = start;
        Count = count;
        Skip = skip;
        MaxCount = maxCount;
        Alpha = alpha;
        EffectAlpha = effectAlpha;
        direction = mirDirection;
    }
}

