using Microsoft.Xna.Framework;
using System;
using System.IO;
using System.Threading.Tasks;
using Terraria;
using TShockAPI;

namespace TileAnimate;

public class Plan
{
    private int current = 1;
    private int interval = 0;
    private bool playing = false;
    private int scence = 1;


    private int tileID = -1;
    private string planName = "apple";
    private Config config;


    public Plan(string name, Config c)
    {
        planName = name;

        config = c;
    }


    /// <summary>
    /// 是否正在播放
    /// </summary>
    public bool IsPlaying { get { return playing; } }

    /// <summary>
    /// 当前帧
    /// </summary>
    public int CurrentFrame { get { return current; } }

    /// <summary>
    /// 总帧数
    /// </summary>
    public int TotalFrames { get { return config.total; } }

    /// <summary>
    /// 动画简述
    /// </summary>
    /// <returns></returns>
    public string Summary()
    {
        var mark = config.border ? ", 填充边缘" : "";
        var total = (config.delay * config.total) / 60;
        if (config.duet) total *= 4;
        if (config.superDuet) total *= 16;

        DateTime currentTime = DateTime.Now;
        DateTime futureTime = currentTime.AddSeconds(total);
        string hours = futureTime.Hour.ToString("00");
        string minutes = futureTime.Minute.ToString("00");
        string seconds = futureTime.Second.ToString("00");
        int scence = config.duet ? 4 : (config.superDuet ? 16 : 1);

        TimeSpan time = TimeSpan.FromSeconds(total);
        string timeReport = time.Hours > 0
            ? string.Format("{0:%h}小时{0:%m}分钟{0:%s}秒", time)
            : string.Format("{0:%m}分钟{0:%s}秒", time);

        return $"{config.width}x{config.height}{mark} {config.total}帧, {scence}个场景, 耗时{timeReport}，预计{hours}:{minutes}:{seconds}播完";
    }


    /// <summary>
    /// 播放
    /// </summary>
    public void Play()
    {
        interval = 0;
        playing = true;
    }

    /// <summary>
    /// 暂停
    /// </summary>
    public void Pause()
    {
        playing = false;
    }

    /// <summary>
    /// 播放暂停
    /// </summary>
    public void Toggle()
    {
        if (playing) { Pause(); }
        else { Play(); }
    }

    /// <summary>
    /// 从指定帧，开始播放
    /// </summary>
    /// <param name="frame"></param>
    public void GoToAndPlay(int frame)
    {
        if (frame > 0 && frame <= config.total)
        {
            current = frame;
        }
        interval = 0;
        playing = true;
    }

    /// <summary>
    /// 转到指定帧，并停止播放
    /// </summary>
    /// <param name="frame"></param>
    public void GoToAndStop(int frame)
    {
        if (frame > 0 && frame <= config.total)
        {
            current = frame;
        }
        playing = false;
    }


    /// <summary>
    /// 停止
    /// </summary>
    public void Stop()
    {
        current = 1;
        playing = false;

        // 清理绘制区域
        int rawW = config.width;
        int rawH = config.height;
        int w = config.duet ? rawW / 2 : (config.superDuet ? rawW / 4 : rawW);
        int h = config.duet ? rawH / 2 : (config.superDuet ? rawH / 4 : rawH);
        int padX = w / 2 - 1;
        int padY = h / 2 + 1;

        Rectangle rect = new(Main.spawnTileX - padX, Main.spawnTileY - padY, w, h);
        if (config.border)
        {
            Gen.UpdateRect(ClearDarw(rect));
        }
        else
        {
            Gen.UpdateRect(rect);
        }
    }

    /// <summary>
    /// 更新画面（enterframe）
    /// </summary>
    /// <returns></returns>
    public async Task UpdateAsync()
    {
        if (!playing) return;

        interval--;
        if (interval < 1)
        {
            interval = config.delay;
            await AsyncDraw();
        }
    }

    /// <summary>
    /// 异步绘制
    /// </summary>
    /// <returns></returns>
    Task AsyncDraw()
    {
        return Task.Run(() => { Draw(); });
    }

    /// <summary>
    /// 绘制图格
    /// </summary>
    void Draw()
    {
        int col = config.duet ? 2 : (config.superDuet ? 4 : 1);
        if (current < 1 || current > config.total)
        {
            if (config.duet || config.superDuet)
            {
                if (current > config.total)
                {
                    scence++;
                    current = 1;

                    if ((config.duet && scence > 4) || (config.superDuet && scence > 16))
                    {
                        scence = 1;
                        playing = false;
                        Utils.Log("已播完");
                        return;
                    }

                    if (config.autoDirection) SetDirection(scence % col == 0);
                    Utils.Log($"场景{scence}");
                }
            }
            else
            {
                playing = false;
                current = 1;
                Utils.Log("已播完");
                return;
            }

        }

        int rawW = config.width;
        int rawH = config.height;
        int w = config.duet ? rawW / 2 : (config.superDuet ? rawW / 4 : rawW);
        int h = config.duet ? rawH / 2 : (config.superDuet ? rawH / 4 : rawH);
        int padX = w / 2 - 1;
        int padY = h / 2 + 1;
        Rectangle rect = new(Main.spawnTileX - padX, Main.spawnTileY - padY, w, h);
        Rectangle finalRect = Utils.CloneRect(rect);

        // 当前帧所填充的图格
        ushort id = 0;
        int type = 1;
        SelectTile(current, ref id, ref type);

        // 外围填充图格
        if (config.border)
        {
            finalRect = FillBorder(rect, id, type);
        }

        // 使用 StreamReader 打开文件并读取内容
        string fileName = Path.Combine(Utils.GetPlanDir(planName), "txt", $"{current:0000}.txt");
        using (StreamReader sr = new(fileName))
        {
            int si = scence - 1;
            int startX = (si % col) * w;
            int startY = (int)Math.Floor((double)si / col) * h;
            int endX = startX + w;
            int endY = startY + h;

            for (int row = 0; row < config.height; row++)
            {
                string line = sr.ReadLine();
                if (line == null) continue;
                if (row < startY || row >= endY) continue;

                for (col = 0; col < config.width; col++)
                {
                    if (col >= line.Length) break;
                    if (col < startX || col >= endX) continue;

                    bool hasPixel = false;
                    string pixelValue = line[col].ToString();
                    if (pixelValue == "0")
                    {
                        hasPixel = true;
                    }

                    // 图格映射
                    foreach (var m in config.mapping)
                    {
                        if (pixelValue == m.num.ToString())
                        {
                            if (m.id == 1000)
                            {
                                hasPixel = false;
                            }
                            else
                            {
                                id = (ushort)m.id;
                                type = 1;
                                hasPixel = true;
                            }
                            //Utils.Log($"{hasPixel} {id}");
                            break;
                        }
                    }

                    var rx = rect.X + col - startX;
                    var ry = rect.Y + row - startY;
                    if (config.stand && rx == Main.spawnTileX && (ry >= Main.spawnTileY - 1 && ry <= Main.spawnTileY))
                    {
                        if (ry == Main.spawnTileY)
                        {
                            //WorldGen.PlaceTile(rx, ry, 19, false, true, -1, 14);
                            SetTile(rx, ry, id, type);
                        }
                        continue;
                    }

                    if (hasPixel)
                    {
                        SetTile(rx, ry, id, type);
                    }
                    else
                    {
                        //ClearTile(rx, ry);
                        ClearEveryting(rx, ry);
                    }

                }

            }
        }

        Gen.UpdateRect(finalRect);



        // 释放弹力巨石
        if (config.bouncyBoulder.Contains(current))
        {
            Basketball();
        }


        // 清除射弹
        if (config.clearProjectiles.Contains(current))
        {
            ClearProjectiles();
        }

        ProcessMob(current);
        ProcessTP(current);


        current++;
    }


    // 填充边框
    static Rectangle FillBorder(Rectangle rawRect, ushort tileID, int type)
    {
        Rectangle rect = Utils.CloneRect(rawRect);

        int num = 4;
        rect.X -= num;
        rect.Y -= num;
        rect.Width += num * 2;
        rect.Height += num * 2;

        for (int ry = rect.Y; ry < rect.Bottom; ry++)
        {
            for (int rx = rect.X; rx < rect.Right; rx++)
            {
                if (!rawRect.Contains(rx, ry))
                {
                    ClearEveryting(rx, ry);
                    SetTile(rx, ry, tileID, type);
                }
            }
        }

        return rect;
    }

    /// <summary>
    /// 清理绘制区域
    /// </summary>
    /// <param name="rawRect"></param>
    /// <returns></returns>
    static Rectangle ClearDarw(Rectangle rawRect)
    {
        Rectangle rect = Utils.CloneRect(rawRect);

        int num = 4;
        rect.X -= num;
        rect.Y -= num;
        rect.Width += num * 2;
        rect.Height += num * 2;

        for (int ry = rect.Y; ry < rect.Bottom; ry++)
        {
            for (int rx = rect.X; rx < rect.Right; rx++)
            {
                ClearEveryting(rx, ry);
            }
        }

        return rect;
    }

    void SelectTile(int current, ref ushort id, ref int type)
    {
        if (tileID == -1)
        {
            foreach (var p in config.tilePlans)
            {
                if (current >= p.start && current <= p.end)
                {
                    id = p.id;
                    type = p.type;
                    return;
                }
            }
        }

        id = config.id;
        type = config.type;
    }

    /// <summary>
    /// 处理NPC召唤
    /// </summary>
    /// <param name="current"></param>
    void ProcessMob(int current)
    {

        foreach (var p in config.spawnMobPlans)
        {
            if (current == p.start)
            {
                NPC npc = new();
                npc.SetDefaults(p.id);
                TSPlayer.Server.SpawnNPC(npc.type, npc.FullName, 1, Main.spawnTileX, Main.spawnTileY, 5, 2);
                return;
            }
        }
    }

    /// <summary>
    /// 处理tp
    /// </summary>
    /// <param name="current"></param>
    void ProcessTP(int current)
    {
        foreach (var plan in config.tpPlans)
        {
            if (current == plan.start)
            {
                Main.spawnTileX = plan.x;
                Main.spawnTileY = plan.y;
                foreach (TSPlayer op in TShock.Players)
                {
                    if ((op != null) && (op.Active))
                    {
                        op.Teleport(Main.spawnTileX * 16, (Main.spawnTileY * 16) - 48);
                    }
                }
                return;
            }
        }
    }


    /// <summary>
    /// 更改玩家转向（未实现）
    /// </summary>
    /// <param name="isLeft"></param>
    static void SetDirection(bool isLeft)
    {
        foreach (TSPlayer op in TShock.Players)
        {
            if ((op != null) && (op.Active))
            {
                if (isLeft)
                    op.TPlayer.direction = -1;
                else
                    op.TPlayer.direction = 1;
                NetMessage.SendData(13, -1, -1, null, op.Index);
            }
        }
    }

    protected static void ClearEveryting(int x, int y)
    {
        ITile tile = Main.tile[x, y];
        tile.ClearEverything();
    }

    protected static void ClearTile(int x, int y)
    {
        ITile tile = Main.tile[x, y];
        tile.ClearTile();
    }

    public static ITile SetTile(int x, int y, ushort id, int type = 1)
    {
        ITile tile = Main.tile[x, y];
        switch (type)
        {
            // 墙
            case 2:
                tile.wall = id;
                NetMessage.SendTileSquare(-1, x, y);
                break;

            // 液体
            case 3:
                FillLiquid(x, y, id);
                //if (id == 2) tile.lava(true);
                //else if (id == 3) tile.honey(true);
                //else if (id == 4) tile.shimmer(true);
                //else
                //{
                //    tile.lava(false);
                //    tile.honey(false);
                //    tile.shimmer(false);
                //}

                //WorldGen.SquareTileFrame(x, y);
                //NetMessage.SendTileSquare(-1, x, y);
                break;

            default:
                tile.type = id;
                tile.active(true);
                break;
        }
        return tile;
    }

    void Basketball()
    {
        int total = 40;
        Vector2 pos;
        Vector2 vel;
        int pIndex;
        int projID = 1013;
        for (int i = 0; i < total; i++)
        {
            // 1013 弹力巨石
            // 1021 月亮巨石
            pos = new(Main.spawnTileX * 16 + (60 - i * 3) * 16, Main.spawnTileY * 16 - 16 * (Main.rand.Next(10, 50)));
            vel = new(Main.rand.Next(-10, 10), Main.rand.Next(2, 20));
            pIndex = Projectile.NewProjectile(Projectile.GetNoneSource(), pos, vel, projID, 0, 0f);
            NetMessage.SendData(27, -1, -1, null, pIndex);
        }
    }

    static void ClearProjectiles()
    {
        for (int i = 0; i < Main.maxProjectiles; i++)
        {
            if (Main.projectile[i].active)
            {
                Main.projectile[i].active = false;
                Main.projectile[i].type = 0;
                NetMessage.SendData(27, -1, -1, null, i);
            }
        }
    }

    public void SetTileID(int id)
    {
        tileID = id;
    }

    /// <summary>
    /// 填充液体
    /// </summary>
    private static void FillLiquid(int x, int y, int type)
    {
        ITile tile = Main.tile[x, y];
        // if (tile.active() || tile.liquid > 0) return;

        int id = -1;
        switch (type)
        {
            // Tile.Liquid_Water
            case 1: id = 0; break;
            case 2: id = 1; break;
            case 3: id = 2; break;
            case 4: id = 3; break;
        }
        if (id != -1)
        {
            tile.liquid = byte.MaxValue;
            tile.liquidType(id);
            //WorldGen.SquareTileFrame(x, y);
            //NetMessage.SendTileSquare(-1, x, y);
        }
    }

    public void Dispose()
    {
        playing = false;
    }

}