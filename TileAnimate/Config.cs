using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace TileAnimate;

// 插件配置
public class Config
{
    /// <summary>
    /// 动画名称
    /// </summary>
    public string title = "♪《Bad Apple》♪";

    /// <summary>
    /// 装饰标题
    /// </summary>
    public string mark = "⇆ ◁ ❚❚ ▷ ↻";


    /// <summary>
    /// 四重奏
    /// </summary>
    public bool duet = false;

    /// <summary>
    /// 十六联屏
    /// </summary>
    public bool superDuet = false;

    /// <summary>
    /// 自动朝向
    /// </summary>
    public bool autoDirection = false;

    /// <summary>
    /// 站脚点
    /// </summary>
    public bool stand = false;

    /// <summary>
    /// 宽度
    /// </summary>
    public int width = 120;

    /// <summary>
    /// 高度
    /// </summary>
    public int height = 68;

    /// <summary>
    /// 开启边缘填充
    /// </summary>
    public bool border = false;

    /// <summary>
    /// 间隔多少帧
    /// </summary>
    public int delay = 15;

    /// <summary>
    /// 最大帧数
    /// </summary>
    public int total = 4345;


    /// <summary>
    /// 默认图格
    /// </summary>
    public ushort id = 0;

    /// <summary>
    /// 图格类型
    /// </summary>
    public int type = 1;


    /// <summary>
    /// 图格规划
    /// </summary>
    public List<TilePlan> tilePlans = new();

    /// <summary>
    /// 传送规划
    /// </summary>
    public List<TPPlan> tpPlans = new();

    /// <summary>
    /// 召唤规划
    /// </summary>
    public List<SpawnMobPlan> spawnMobPlans = new();


    /// <summary>
    /// 释放弹力巨石
    /// </summary>
    public List<int> bouncyBoulder = new();

    /// <summary>
    /// 清理射弹
    /// </summary>
    public List<int> clearProjectiles = new();

    /// <summary>
    /// 图格映射
    /// </summary>
    public List<TileMapping> mapping = new();



    public void Init()
    {
        // 开头 红色
        // 骑扫帚 黑色 金
        // 粉 书
        // 女仆 紫色
        // 翅膀 彩色 红 刀
        // 青色 刀 花 仙人掌
        // 蓝色 樱花
        // 红船 镰刀
        // 红色 枫叶
        // 扇子 镒砖
        // 

        // 土块
        tilePlans.Add(new TilePlan(0, 146, 0));

        // 红苹果
        tilePlans.Add(new TilePlan(245, 297, 259));

        // 魔理沙 土块
        tilePlans.Add(new TilePlan(298, 439, 0));

        // 红魔馆 粉砖
        tilePlans.Add(new TilePlan(440, 506, Terraria.ID.TileID.PinkDungeonBrick));

        // 陶瓷 光面大理石
        tilePlans.Add(new TilePlan(843, 868, 357));

        // 彩虹转
        tilePlans.Add(new TilePlan(1004, 1125, Terraria.ID.TileID.RainbowBrick));

        // 火焰 活火块
        tilePlans.Add(new TilePlan(1752, 1889, Terraria.ID.TileID.LivingFire));

        // 红眼睛
        tilePlans.Add(new TilePlan(2806, 2842, 259));

        // 扇子 镒砖
        tilePlans.Add(new TilePlan(2930, 2946, 667));

        // 太极 金币堆
        tilePlans.Add(new TilePlan(4199, 4270, 332));

        // 扇子 镒砖
        tilePlans.Add(new TilePlan(4308, 4343, 259));
    }

    /// <summary>
    /// 加载配置文件
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static Config Load(string path)
    {
        if (File.Exists(path))
        {
            return JsonConvert.DeserializeObject<Config>(File.ReadAllText(path), new JsonSerializerSettings()
            {
                Error = (sender, error) => error.ErrorContext.Handled = true
            });
        }
        else
        {
            var c = new Config();
            c.Init();

            // 保存配置
            File.WriteAllText(path, JsonConvert.SerializeObject(c, Formatting.Indented, new JsonSerializerSettings()
            {
            }));
            return c;
        }
    }

}


/// <summary>
/// 图格规划
/// </summary>
public class TilePlan
{
    /// <summary>
    /// 起始帧
    /// </summary>
    public int start = 1;

    /// <summary>
    /// 结束帧
    /// </summary>
    public int end = 10;

    /// <summary>
    /// 图格id
    /// 是液体时，1=水，2=岩浆，3=蜂蜜，4=微光
    /// </summary>
    public ushort id = 0;

    /// <summary>
    /// 图格类型，1=图格,2=墙,3=液体
    /// </summary>
    public int type = 1;

    public TilePlan(int _start, int _end, ushort _id, int _type=1)
    {
        start = _start;
        end = _end;
        id = _id;
        type = _type;
    }
}


/// <summary>
/// 召唤规划
/// </summary>
public class SpawnMobPlan
{
    // npc id
    public int id = 0;

    // 目标帧
    public int start = 1;

    // 数量
    public int num = 1;
}



/// <summary>
/// tp规划
/// </summary>
public class TPPlan
{
    public int x = 0;
    public int y = 0;
    public int start = 0;
}

/// <summary>
/// 图格映射
/// </summary>
public class TileMapping
{
    public int num = 0;
    public int id = 0;
}
