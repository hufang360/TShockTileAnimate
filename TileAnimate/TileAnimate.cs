using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Terraria;
using Terraria.Localization;
using TerrariaApi.Server;
using TShockAPI;

namespace TileAnimate;

[ApiVersion(2, 1)]
public class TileAnimate : TerrariaPlugin
{
    public override string Name => "图格动画";
    public override string Author => "hufang360";
    public override string Description => "用图格来显示动画";
    public override Version Version => Assembly.GetExecutingAssembly().GetName().Version;

    Plan plan;
    Config config;
    int tileID = -1;
    string planName = "apple";


    public TileAnimate(Main game) : base(game)
    {
    }

    public override void Initialize()
    {
        Commands.ChatCommands.Add(new Command(Manage, "animate", "ani") { HelpText = "图格动画" });
        ServerApi.Hooks.GameUpdate.Register(this, OnGameUpdate);

        if (!Directory.Exists(Utils.SaveDir))
        {
            Directory.CreateDirectory(Utils.SaveDir);
        }
    }

    void Manage(CommandArgs args)
    {
        if (args.Parameters.Count == 0)
        {
            Toggle(args.Player);
            return;
        }

        TSPlayer op = args.Player;

        void Help()
        {
            var li = new List<string>() {
                "/animate, 开始/暂停播放（默认《Bad Apple》）",
                "/animate [动画名称], 加载指定动画",
                "/animate stop, 停止播放",
            };
            args.Player.SendInfoMessage(string.Join("\n", li));
        }

        switch (args.Parameters[0].ToLowerInvariant())
        {
            case "stop":
                Stop(args.Player);
                break;

            case "gotoandplay":
            case "gap":
                if (args.Parameters.Count > 1)
                {
                    _ = int.TryParse(args.Parameters[1], out int frame);
                    plan!?.GoToAndPlay(frame);
                }
                else
                {
                    args.Player.SendErrorMessage("请输入要跳转的帧数");
                }
                break;

            case "gotoandstop":
            case "gas":
                if (args.Parameters.Count > 1)
                {
                    _ = int.TryParse(args.Parameters[1], out int frame);
                    plan!?.GoToAndStop(frame);
                }
                else
                {
                    args.Player.SendErrorMessage("请输入要跳转的帧数");
                }
                break;

            case "help": case "h": Help(); return;

            // 脚下垫方块
            case "sit":
                Sit(op);
                return;

            default:
                var p1 = args.Parameters[0];
                // 图格id
                if (int.TryParse(args.Parameters[0], out int id))
                {
                    tileID = id;
                    plan!?.SetTileID(tileID);
                    args.Player.SendSuccessMessage("已将图格id改为 " + id);
                }

                // 加载动画
                else
                {
                    var path = Path.Combine(Utils.SaveDir, args.Parameters[0]);
                    if (Directory.Exists(path))
                    {
                        planName = p1;

                        path = Path.Combine(Utils.GetPlanDir(planName), "config.json");
                        if (File.Exists(path))
                        {
                            args.Player.SendSuccessMessage($"已启用 {p1}");
                        }
                        else
                        {
                            args.Player.SendErrorMessage($"{p1} 目录下没有找到 config.json 配置文件！");
                        }
                    }
                    else
                    {
                        args.Player.SendErrorMessage("输入 /animate help 查看指令用法");
                    }
                }
                break;
        }
    }

    static void Sit(TSPlayer op)
    {
        if (op == null) return;
        Plan.SetTile(Main.spawnTileX, Main.spawnTileY, 0);
        op.Teleport(Main.spawnTileX * 16, (Main.spawnTileY * 16) - 48);
        op.TPlayer.direction =1;
        //NetMessage.SendData(4, -1, -1, NetworkText.FromLiteral(op.Name), op.Index, 0f, 0f, 0f, 0);
        //NetMessage.SendData(4, op.Index, -1, NetworkText.FromLiteral(op.Name), op.Index, 0f, 0f, 0f, 0);
    }

    /// <summary>
    /// 切换播放状态
    /// </summary>
    /// <param name="op"></param>
    void Toggle(TSPlayer op)
    {
        if (plan != null)
        {
            plan.Toggle();
            if (!plan.IsPlaying)
            {
                op.SendInfoMessage("已暂停");
            }
        }
        else
        {
            Run(op);
        }
    }

    void Run(TSPlayer op)
    {
        var path = Path.Combine(Utils.GetPlanDir(planName), "config.json");
        if (!File.Exists(path))
        {
            op.SendErrorMessage($"{planName} 目录下没有名为 config.json 的配置文件");
            return;
        }

        config = Config.Load(path);
        plan = new Plan(planName, config);
        plan.SetTileID(tileID);
        plan.Play();
        op.SendInfoMessage(config.title);
        if (config.mark != "")
        {
            op.SendInfoMessage(config.mark);
        }

        Utils.Log(plan.Summary());
    }


    void Stop(TSPlayer op = null)
    {
        plan!?.Stop();
        op?.SendInfoMessage("正在停止……");
        plan = null;
    }


    void OnGameUpdate(EventArgs args)
    {
        plan!?.UpdateAsync();
    }

    void ManualDispose()
    {
        ServerApi.Hooks.GameUpdate.Deregister(this, OnGameUpdate);
        plan!?.Dispose();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            ManualDispose();
        }
        base.Dispose(disposing);
    }
}