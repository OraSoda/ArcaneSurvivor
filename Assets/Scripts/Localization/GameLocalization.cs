
using System;
using System.Collections.Generic;
using UnityEngine;

public enum GameLanguage
{
    Chinese = 0,
    Japanese = 1,
    English = 2
}

public static class GameLocalization
{
    private const string SaveKey = "ArcaneSurvivor_Language";

    public static event Action LanguageChanged;

    public static GameLanguage Language { get; private set; } =
        (GameLanguage)Mathf.Clamp(
            PlayerPrefs.GetInt(SaveKey, 2), 0, 2);

    private static readonly Dictionary<string, string[]> Texts = new()
    {
        { "title", new[] { "奥术幸存者", "アルケイン・サバイバー", "ARCANE SURVIVOR" } },
        { "subtitle", new[] { "魔法生存挑战", "魔法サバイバル", "MAGICAL SURVIVAL" } },
        { "start", new[] { "开始游戏", "ゲーム開始", "START GAME" } },
        { "resume", new[] { "继续游戏", "ゲームに戻る", "RESUME" } },
        { "pause", new[] { "暂停", "ポーズ", "PAUSED" } },
        { "settings", new[] { "设置", "設定", "SETTINGS" } },
        { "language", new[] { "语言", "言語", "LANGUAGE" } },
        { "quit", new[] { "退出游戏", "ゲーム終了", "QUIT" } },
        { "title_return", new[] { "返回主菜单", "タイトルへ戻る", "MAIN MENU" } },
        { "choose", new[] { "选择一项强化", "強化を選択", "CHOOSE AN UPGRADE" } },
        { "choose_hint", new[] { "选择一项，继续战斗", "一つ選んで戦闘を続行", "CHOOSE ONE TO CONTINUE" } },
        { "victory", new[] { "胜利！", "勝利！", "VICTORY!" } },
        { "gameover", new[] { "游戏结束", "ゲームオーバー", "GAME OVER" } },
        { "results", new[] { "本局结算", "リザルト", "RUN COMPLETE" } },
        { "endless", new[] { "进入无限模式", "エンドレスモードへ", "CONTINUE - ENDLESS" } },
        { "finish", new[] { "结束本局", "今回のプレイを終了", "FINISH RUN" } },
        { "restart", new[] { "重新开始", "リスタート", "RESTART" } },
        { "level", new[] { "等级", "レベル", "LEVEL" } },
        { "health", new[] { "生命", "HP", "HP" } },
        { "attack", new[] { "攻击", "攻撃", "ATK" } },
        { "kills", new[] { "击杀", "撃破", "KILLS" } },
        { "time", new[] { "生存时间", "生存時間", "SURVIVAL" } },
        { "xp", new[] { "经验", "経験値", "XP" } },
        { "upgrades", new[] { "强化", "強化", "UPGRADES" } },
        { "none", new[] { "无", "なし", "NONE" } },
        { "locked", new[] { "未解锁", "未解放", "LOCKED" } },
        { "ready", new[] { "可使用", "使用可能", "READY" } },
        { "casting", new[] { "释放中", "発動中", "CASTING" } },
        { "total_xp", new[] { "累计经验", "累計経験値", "TOTAL XP" } },
        { "controls", new[] { "WASD 移动  ·  鼠标瞄准  ·  Space 技能", "WASD 移動・マウス照準・Space スキル", "WASD MOVE  ·  MOUSE AIM  ·  SPACE SKILL" } },

        { "Multishot", new[] { "多重射击", "マルチショット", "Multishot" } },
        { "Split", new[] { "分裂", "分裂弾", "Split" } },
        { "BurstFire", new[] { "三连发", "バースト射撃", "Burst Fire" } },
        { "LargerProjectile", new[] { "弹丸增大", "弾サイズ拡大", "Larger Projectile" } },
        { "ImpactExplosion", new[] { "命中爆炸", "着弾爆発", "Impact Explosion" } },
        { "Pierce", new[] { "无限穿透", "無制限貫通", "Infinite Pierce" } },
        { "DoubleFireRate", new[] { "两倍射速", "攻撃速度２倍", "Double Fire Rate" } },
        { "AreaFreeze", new[] { "范围冻结", "範囲凍結", "Area Freeze" } },
        { "AreaSlow", new[] { "范围减速", "範囲減速", "Area Slow" } },
        { "DamageZone", new[] { "持续伤害区域", "継続ダメージ領域", "Damage Zone" } },
        { "ChainDamage", new[] { "连锁伤害", "連鎖ダメージ", "Chain Damage" } },
        { "NuclearDamage", new[] { "核弹伤害", "超高火力", "Nuclear Damage" } },

        { "desc_Multishot", new[] { "同时朝三个方向射击", "同時に３方向へ発射", "Fire three projectiles at once" } },
        { "desc_Split", new[] { "直接命中后生成三颗小弹丸", "直撃時に小弾を３発生成", "Create three child shots on direct hit" } },
        { "desc_BurstFire", new[] { "每次攻击连续发射三颗弹丸", "１回の攻撃で３連射", "Fire three shots in quick succession" } },
        { "desc_LargerProjectile", new[] { "弹丸与碰撞范围扩大 1.5 倍", "弾と当たり判定が1.5倍", "Projectile size and hitbox x1.5" } },
        { "desc_ImpactExplosion", new[] { "直接命中触发范围爆炸", "直撃時に範囲爆発", "Direct hits cause area explosions" } },
        { "desc_Pierce", new[] { "贯穿不同敌人，直至弹丸消失", "弾が消えるまで敵を貫通", "Pierce enemies until projectile expires" } },
        { "desc_DoubleFireRate", new[] { "普通攻击频率提高一倍", "通常攻撃速度が２倍", "Double normal attack frequency" } },
        { "desc_AreaFreeze", new[] { "命中范围冻结 0.8 秒", "範囲内を0.8秒凍結", "Freeze nearby enemies for 0.8 sec" } },
        { "desc_AreaSlow", new[] { "范围减速 50%，持续 4 秒", "範囲内を４秒間50％減速", "Slow nearby enemies 50% for 4 sec" } },
        { "desc_DamageZone", new[] { "命中后留下持续伤害区域", "着弾時に継続ダメージ領域を生成", "Leave a damaging area on direct hit" } },
        { "desc_ChainDamage", new[] { "额外攻击附近最多五名敌人", "周囲の敵最大５体に追加ダメージ", "Damage up to five additional enemies" } },
        { "desc_NuclearDamage", new[] { "弹丸伤害提高至五倍", "弾のダメージが５倍", "Multiply projectile damage by five" } }
    };

    public static string Get(string key)
    {
        if (Texts.TryGetValue(key, out string[] values))
            return values[(int)Language];

        return key;
    }

    public static string UpgradeName(UpgradeId id)
    {
        return Get(id.ToString());
    }

    public static string UpgradeDescription(UpgradeId id)
    {
        return Get("desc_" + id);
    }

    public static void SetLanguage(GameLanguage language)
    {
        if (Language == language)
            return;

        Language = language;
        PlayerPrefs.SetInt(SaveKey, (int)language);
        PlayerPrefs.Save();
        LanguageChanged?.Invoke();
    }
}
