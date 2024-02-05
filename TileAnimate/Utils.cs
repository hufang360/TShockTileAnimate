using Microsoft.Xna.Framework;
using System;
using System.IO;
using Terraria;
using TShockAPI;

namespace TileAnimate;

public class Utils
{
    public static readonly string SaveDir = Path.Combine(TShock.SavePath, "TileAnimate");

    public static string GetPlanDir(string name)
    {
        return Path.Combine(SaveDir, name);
    }

    /// <summary>
    /// 获取当前时间的 unix时间戳（毫秒）
    /// </summary>
    public static int GetUnixTimestamp
    {
        get
        {
            return (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
        }
    }

    public static string RectangleToString(Rectangle rect)
    {
        return $"{rect.X},{rect.Y} {rect.Width}x{rect.Height}";
    }
    public static void LogRectangle(Rectangle rect)
    {
        TShock.Log.ConsoleInfo($"x={rect.X}, y={rect.Y}, w={rect.Width}, h={rect.Height}");
    }

    public static int HalfFloor(int value)
    {
        return (int)Math.Floor((float)(value / 2));
    }


    /// <summary>
    /// 基地范围
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public static bool IsBase(int x, int y)
    {
        int sw = 122;
        int sh = 68;
        Rectangle area = new(Main.spawnTileX - sw / 2, Main.spawnTileY - sh / 2, sw, sh);
        return area.Contains(x, y);
    }

    /// <summary>
    /// 是否为全图范围
    /// </summary>
    /// <param name="rect"></param>
    /// <returns></returns>
    public static bool IsWorldArea(Rectangle rect)
    {
        return rect.Width == Main.maxTilesX && rect.Height == Main.maxTilesY;
    }

    /// <summary>
    /// 玩家所在一屏区域
    /// </summary>
    /// <param name="op"></param>
    public static Rectangle GetScreen(TSPlayer op) { return GetScreen(op.TileX, op.TileY); }

    /// <summary>
    /// 玩家所在一屏区域
    /// </summary>
    /// <param name="playerX"></param>
    /// <param name="playerY"></param>
    public static Rectangle GetScreen(int playerX, int playerY) { return new Rectangle(playerX - 59, playerY - 35 + 3, 120, 68); }

    /// <summary>
    /// 整个地图区域
    /// </summary>
    public static Rectangle GetWorldArea() { return new Rectangle(0, 0, Main.maxTilesX, Main.maxTilesY); }

    /// <summary>
    /// 基地所在一屏区域
    /// </summary>
    /// <returns></returns>
    public static Rectangle GetBaseArea() { return new Rectangle(Main.spawnTileX - 59, Main.spawnTileY - 35 + 3, 120, 68); }

    /// <summary>
    /// 克隆 Rectangle 对象
    /// </summary>
    /// <param name="rect"></param>
    /// <returns></returns>
    public static Rectangle CloneRect(Rectangle rect) { return new Rectangle(rect.X, rect.Y, rect.Width, rect.Height); }

    /// <summary>
    /// 是否在区域内
    /// </summary>
    /// <param name="rect"></param>
    /// <param name="point"></param>
    /// <returns></returns>
    public static bool InArea(Rectangle rect, Rectangle point)
    {
        return InArea(rect, point.X, point.Y);
    }
    /// <summary>
    /// 是否在区域内
    /// </summary>
    /// <param name="rect"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public static bool InArea(Rectangle rect, int x, int y) //overloaded with x,y
    {
        /*
        DO NOT CHANGE TO Area.Contains(x, y)!
        Area.Contains does not account for the right and bottom 'border' of the rectangle,
        which results in regions being trimmed.
        */
        return x >= rect.X && x <= rect.X + rect.Width && y >= rect.Y && y <= rect.Y + rect.Height;
    }

    /// <summary>
    /// 输出日志
    /// </summary>
    public static void Log(object obj) { TShock.Log.ConsoleInfo($"[ani]{obj}"); }
}