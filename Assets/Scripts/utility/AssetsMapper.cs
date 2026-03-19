using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.logics;
using Assets.Scripts.utility;
using Assets.Scripts.common;

public static class AssetsMapper
{
    public static string GetGroupIcon(BattleGroupType type, bool isBigIcon)
    {
        string str1 = isBigIcon ? "big" : "small";
        string str2 = null;
        switch (type)
        {
            case BattleGroupType.CAVALRY: str2 = "nocircle"; break;

            case BattleGroupType.HEAVY: str2 = "circle"; break;

            case BattleGroupType.MELEE: str2 = "x"; break;

            case BattleGroupType.PROJECTILE: str2 = "arrow"; break;

            case BattleGroupType.FIREARM:
            case BattleGroupType.SIEGE: str2 = "dot"; break;

            case BattleGroupType.SPEAR: str2 = "v"; break;

            case BattleGroupType.SPECIAL: str2 = "m"; break;

            case BattleGroupType.NONE:
            default: break;
        }

        if (str2 == null) return null;
        else return string.Format("images/group/group_icon_{0}/group_{0}_{1}", str1, str2);
    }

    public static string GetGroupIcon(BattleGroupData data, bool isBigIcon, bool needApplyLoss)
    {
        var groupType = data.GetMajorBattleGroupType(needApplyLoss);
        return GetGroupIcon(groupType, isBigIcon);
    }

    public static string GetFontAsset(UIUtils.FontType fontType, UIUtils.LanguageType languageType)
    {
        string fontAssetPath = "";
        switch (languageType)
        {
            case UIUtils.LanguageType.EN:
            case UIUtils.LanguageType.FR:
                {
                    fontAssetPath += "fonts/Inter/static/";
                    switch (fontType)
                    {
                        case UIUtils.FontType.Title:
                            fontAssetPath += "Inter-Bold SDF";
                            break;
                        case UIUtils.FontType.Desc:
                            fontAssetPath += "Inter-Regular SDF";
                            break;
                        default:
                            break;
                    }
                }
                break;

            case UIUtils.LanguageType.CNS:
                {
                    fontAssetPath += "fonts/HanSans/SourceHanSansSC-VF SDF";
                    //switch (fontType)
                    //{
                    //    case UIUtils.FontType.Title:
                    //        fontAsset += "Inter-Bold";
                    //        break;
                    //    case UIUtils.FontType.Desc:
                    //        fontAsset += "Inter-Regular";
                    //        break;
                    //    default:
                    //        break;
                    //}
                }
                break;

            case UIUtils.LanguageType.CNT:
                {
                    fontAssetPath += "fonts/HanSans/SourceHanSansTC-VF SDF";
                    
                }
                break;
            default:
                break;
        }
        return fontAssetPath;
    }

    public static void GetVFXPaths(BattleActionEffectType actionEffectType, out string projectile, out string muzzle, out string hit)
    {
        switch (actionEffectType)
        {
            case BattleActionEffectType.MELEE_ATTACK:
                projectile = "vfx/VFX_Klaus/Prefabs/Stylized Shoot & Hit Vol.2/FX_Shoot_Ice_projectile";
                muzzle = "vfx/VFX_Klaus/Prefabs/Stylized Shoot & Hit Vol.2/FX_Shoot_Ice_muzzle";
                hit = "vfx/VFX_Klaus/Prefabs/Stylized Shoot & Hit Vol.2/FX_Shoot_Ice_hit";
                break;

            case BattleActionEffectType.ARMOUR_PIERCING_ATTACK:
                projectile = "vfx/VFX_Klaus/Prefabs/Stylized Shoot & Hit/FX_Shoot_01_projectile";
                muzzle = "vfx/VFX_Klaus/Prefabs/Stylized Shoot & Hit/FX_Shoot_01_muzzle";
                hit = "vfx/VFX_Klaus/Prefabs/Stylized Shoot & Hit/FX_Shoot_01_hit";
                break;

            case BattleActionEffectType.PROJECTILE_ATTACK:
                projectile = "vfx/VFX_Klaus/Prefabs/Stylized Shoot & Hit Vol.2/FX_Shoot_Arrow_projectile";
                muzzle = "vfx/VFX_Klaus/Prefabs/Stylized Shoot & Hit Vol.2/FX_Shoot_Arrow_muzzle";
                hit = "vfx/VFX_Klaus/Prefabs/Stylized Shoot & Hit Vol.2/FX_Shoot_Arrow_hit";
                break;

            case BattleActionEffectType.PRIOTIZED_ATTACK:
                projectile = "vfx/VFX_Klaus/Prefabs/Stylized Shoot & Hit Vol.2/FX_Shoot_Kunai_projectile";
                muzzle = "vfx/VFX_Klaus/Prefabs/Stylized Shoot & Hit Vol.2/FX_Shoot_Kunai_muzzle";
                hit = "vfx/VFX_Klaus/Prefabs/Stylized Shoot & Hit Vol.2/FX_Shoot_Kunai_hit";
                break;

            case BattleActionEffectType.SIEGE_ATTACK:
                projectile = "vfx/VFX_Klaus/Prefabs/Stylized Shoot & Hit/FX_Shoot_06_projectile";
                muzzle = "vfx/VFX_Klaus/Prefabs/Stylized Shoot & Hit/FX_Shoot_06_muzzle";
                hit = "vfx/VFX_Klaus/Prefabs/Stylized Shoot & Hit/FX_Shoot_06_hit";
                break;

            default:
                projectile = muzzle = hit = null; break;
        }
    }

    public static string GetLandscapeIconPath(string landscapeId)
    {
        return string.Format("images/landscape/icons/" + landscapeId);
    }

    public static string GetLandscapeBgPath(string landscapeId)
    {
        return string.Format("images/landscape/cards/" + landscapeId);
    }

    public static void GetUpdateStatusVFXPaths(BattleActionEffectType actionEffectType, float delta, out string hit)
    {
        // TODO: to update below when the final VFX resources are ready.
        hit = string.Format("vfx/VFX_Klaus/Prefabs/Hyper Casual FX/HCFX_Buff_{0}", delta >= 0 ? "Up" : "Down");
        //switch (actionEffectType)
        //{
        //    case BattleActionEffectType.MORAL:
        //    case BattleActionEffectType.DISCIPLINE:
        //    case BattleActionEffectType.COUNTERATTACK_MODIFIER:
        //        hit = string.Format("vfx/VFX_Klaus/Prefabs/Hyper Casual FX/HCFX_Buff_{0}", delta > 0 ? "Up" : "Down");
        //        break;

        //    default:
        //        hit = null;
        //        break;
        //}
    }

    public static void GetSpecialFunctionVFXPathPlayTime(BattleActionEffectType actionEffectType, out string vfxPath, out float vfxPlayTime)
    {
        // TODO: to update below when the final VFX resources are ready.
        if (Utils.HasAttribute<DrawCardFlag, BattleActionEffectType>(actionEffectType))
        {
            vfxPath = "vfx/VFX_Klaus/Prefabs/Hyper Casual FX/HCFX_Conffeti_Air";
            vfxPlayTime = 1.5f;
        }
        else if (actionEffectType == BattleActionEffectType.BURN_CARD)
        {
            vfxPath = "vfx/VFX_Klaus/Prefabs/Hyper Casual FX Vol.2/HCFX_Energy_04";
            vfxPlayTime = 1.5f;
        }
        else if(actionEffectType == BattleActionEffectType.DISCARD_CARD)
        {
            vfxPath = "vfx/VFX_Klaus/Prefabs/Hyper Casual FX Vol.2/HCFX_Energy_02";
            vfxPlayTime = 1.5f;
        }
        else if(Utils.HasAttribute<ReduceCardCostFlag, BattleActionEffectType>(actionEffectType))
        {
            vfxPath = "vfx/VFX_Klaus/Prefabs/Hyper Casual FX Vol.2/HCFX_Energy_05";
            vfxPlayTime = 1.5f;
        }
        else
        {
            vfxPath = "vfx/VFX_Klaus/Prefabs/Hyper Casual FX/HCFX_Buff_Up";
            vfxPlayTime = 0.5f;
        }
    }
}
