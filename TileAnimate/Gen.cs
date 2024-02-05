using Microsoft.Xna.Framework;
using System;
using System.Threading.Tasks;
using Terraria;
using TShockAPI;

namespace TileAnimate;

public class Gen
{

    //// 放置绿砖-方法3
    //tile = Main.tile[col, ry];
    //tile.type = TileID.Platforms;
    //tile.frameX = 90;
    //tile.frameY = 144;
    //tile.active(true);

    //// 放置绿砖-方法1
    //Gen.SetTile(col, ry, TileID.Platforms);

    //// 放置绿砖-方法2
    //WorldGen.PlaceTile(col, ry, TileID.Platforms, true, true, 0, 8);


    //NetMessage.SendTileSquare(-1, col, ry);

    //NetMessage.SendTileSquare(-1, rx, ry, 2);
    //WorldGen.paintTile(col, ry, PaintID.BlackPaint, true);
    //sky Main.worldSurface * 0.3499999940395355


    // 射弹id  85=精灵熔炉 ，188=火焰机关，196=烟雾

    public static void GetHell(int posX = 0, int posY = 0)
    {
        if (posX == 0) posX = Main.spawnTileX;
        if (posY == 0) posY = Main.spawnTileY;

        int hell;
        int xtile;
        for (hell = Main.UnderworldLayer + 10; hell <= Main.maxTilesY - 100; hell++)
        {
            xtile = posX;
            Parallel.For(posX, posX + 8, (cwidth, state) =>
            {
                if (Main.tile[cwidth, hell].active() && !Main.tile[cwidth, hell].lava())
                {
                    state.Stop();
                    xtile = cwidth;
                    return;
                }
            });

            if (!Main.tile[xtile, hell].active()) break;
        }
    }

    public static void ClearEveryting(int x, int y)
    {
        ITile tile = Main.tile[x, y];
        tile.ClearEverything();
    }

    public static ITile SetTile(int x, int y, ushort tileID)
    {
        ITile tile = Main.tile[x, y];
        tile.type = tileID;
        tile.active(true);
        return tile;
    }

    /// <summary>
    /// 清空区域
    /// </summary>
    /// <param name="rect"></param>
    /// <param name="forceUpdate"></param>
    public static void ClearRect(Rectangle rect, bool forceUpdate = false)
    {
        rect = FixRect(rect);
        for (int rx = rect.Left; rx < rect.Right; rx++)
        {
            for (int ry = rect.Top; ry < rect.Bottom; ry++)
            {
                ITile tile = Main.tile[rx, ry];
                tile.ClearEverything();
            }
        }
        if (forceUpdate)
        {
            UpdateRect(rect);
        }
    }


    /// <summary>
    /// 更新区域
    /// </summary>
    /// <param name="rect"></param>
    public static void UpdateRect(Rectangle rect, bool veryStrict = false)
    {
        rect = FixRect(rect);
        var size = 30;
        var pad = size - 1;
        var rawSize = Math.Max(rect.Width, rect.Height);
        if (size > rawSize)
        {
            size = rawSize;
            pad = size - 1;
        }

        if (veryStrict)
        {
            pad = 0;
            size = 2;
        }
        for (int rx = rect.Left; rx < rect.Right + 1; rx++)
        {
            for (int ry = rect.Top; ry < rect.Bottom + 1; ry++)
            {
                NetMessage.SendTileSquare(-1, rx, ry, size);
                ry += pad;
            }
            rx += pad;
        }
    }

    /// <summary>
    /// 更新区域（每隔30x30进行更新）
    /// </summary>
    public static void UpdateRect30(Rectangle rect)
    {
        // 最大size不应超过140
        int size = 31;
        int gapY = 31;
        int countY = rect.Top + 15;
        for (int ry = rect.Top; ry < rect.Bottom; ry++)
        {
            if (ry >= countY)
            {
                NetMessage.SendTileSquare(-1, rect.Center.X, countY, size);
                countY += gapY;
            }
            else
            {
                if (ry >= rect.Bottom - 1)
                {
                    NetMessage.SendTileSquare(-1, rect.Center.X, rect.Bottom - 15, size);
                }
            }
        }
    }


    public static void TestUpdateSquare(TSPlayer op, int type)
    {
        // 填充区域
        Rectangle rect = new(op.TileX, op.TileY, 200, 200);
        rect = FixRect(rect);
        for (int rx = rect.Left; rx < rect.Right; rx++)
        {
            for (int ry = rect.Top; ry < rect.Bottom; ry++)
            {
                ITile tile = Main.tile[rx, ry];
                if (tile != null)
                {
                    tile.type = (ushort)type;
                    if ((rx - rect.Left) % 5 == 0 || (ry - rect.Top) % 5 == 0)
                    {
                        tile.type = (ushort)(type + 1);
                    }
                    tile.active(true);
                }
            }
        }
        UpdateRect(rect);
    }

    public static Rectangle FixRect(Rectangle rect)
    {
        Rectangle r = Utils.CloneRect(rect);
        if (r.X < 10) r.X = 10;
        if (r.Y < 10) r.Y = 10;

        if (r.Right > Main.maxTilesX - 10) r.Width = Main.maxTilesX - 10 - r.Left;
        if (r.Bottom > Main.maxTilesY - 10) r.Width = Main.maxTilesY - 10 - r.Top;

        return r;
    }

    public static bool InWorld(int x, int y)
    {
        return x >= 0 && x < Main.maxTilesX && y >= 0 && y < Main.maxTilesY;
    }

}