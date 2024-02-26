
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;


[System.Serializable]
public class Skill 
{
    public string Name;
    public string Info;
    public List<SkillComponenet> skillComponenets=new List<SkillComponenet>();
    public Skill()
    {
        Name = "新的技能";
        Info = "新的描述";
        skillComponenets.Add(new SkillComponenet());
    }
    
}
[System.Serializable]
public class SkillComponenet
{
    public string Name;
    public SkillType SkillType;
    public string info,AutoInfo;
    public TargetSelect Ifcase,TargetSelect;
    public DirectDamage DirectDamage;
    public Effect Effect;
    public PanelControl PanelControl;
    [Multiline(4)] public string Text;
    public static void EffectStack(Effect effect,List<ChaControl> cha)
    {
        foreach (ChaControl a in cha)
        {
            if (!effect.Stack)
            {
                Debug.Log("GOB");
                goto B;
            }
            foreach (Effect b in a.effects)
            {
                if (b.Name == effect.Name)
                {                   
                    b.Turn = effect.Turn;
                    if (!b.Overlay)
                    {
                        Effect.cul(-1, b, a);
                        b.Layer = Math.Clamp(b.Layer + effect.Layer, b.LayerMin, b.LayerMax);
                        Effect.cul(1, b, a);
                    }
                    else
                    {
                        Effect.cul(-1, b, a);
                        a.effects.Remove(b);
                        Debug.Log("overB");
                        goto B; 
                    
                    }
                    
                    if (b.Layer == 0)
                    {
                        Effect.cul(-1, b, a);
                        a.effects.Remove(b);
                        Debug.Log("layer0");
                    }
                    goto A;
                }
                
            }
            B:;

            Effect g= Effect.Copy(effect);
            Debug.Log("ADDG"+g.Name);
            a.effects.Add(g);
            Effect.cul(1,g,a);

        A:;
            Debug.Log("GOA");
        }
    }
    public SkillComponenet()
    {
        Name = "新的元件";
        info = "新的敘述";
    }

}

[System.Serializable]
public class Package
{
    public string Name;
    public bool Random,Lock;
    public List<Skill> skills=new List<Skill>();  
    public Package(string Name)
    {
        this.Name = Name;
        skills.Add(new Skill());
    }
    public Package()
    {
        Name = "新的包";
        skills.Add(new Skill());
    }
}


[System.Serializable]
public class DirectDamage
{
    public float Rate;
    public int Value;
    public bool IgnoreDefence;
    public Reference Reference;
    [HideInInspector] public List<ChaControl> Target;
}
public class Heal
{
    public float Rate;
    public int Value;
    public Reference Reference;
    [HideInInspector] public List<ChaControl> Target;
}
[System.Serializable]
public class PanelControl
{
    public PanelControlType PanelControlType;
    public float value;
    public List<ElementType> Org;
    public ElementType Trans;
    public PanelControl(PanelControlType Type,float Value,List<ElementType> OrgEle,ElementType TransEle)
    {
        PanelControlType = Type;
        value = Value;
        Org = OrgEle; Trans = TransEle;
    }
}

[System.Serializable]
public class TargetSelect
{
    [HideInInspector]public Timing Timing;
    public Target target;
    public Condition condition;
    public bool Sus;
    public float Value;
    public DirectDamage DirectDamage;
    public Effect Effect;
    [System.Serializable]
    public class Target
    {
        public Position Position;
    }
    [System.Serializable]
    public class Condition
    {
        public ConditionType type;
        public List<BlockCheck> Block;
        public Effect effect;
        public Cul Cul;
        public Reference Reference;
        public Accord Accord;
        [System.Serializable]
        public class BlockCheck
        {
            public ElementType ElementType;
            public Cul cul;
            public int Value;
        }

    }

}
[System.Serializable]
public class Effect
{
    public string Name,Info;
    public bool Overlay,Stack;
    public int Turn,LayerMin, Layer, LayerMax,Reduce;
    public float Value, Rate=1;
    [HideInInspector]public List<ChaControl> Target;
    [HideInInspector] public PanelControl Panel;

    public EffectType Type;
    [HideInInspector]public ChaSKill Silence;
    public static void Active(ChaControl cha)
    {
        for (int i = 0; i < cha.effects.Count; i++)
        {
            if (cha.effects[i].Turn > 0)
            {
                cha.effects[i].Turn--;
                

            }
            if (cha.effects[i].Reduce > 0)
            {
                cul(-1, cha.effects[i], cha);
                cha.effects[i].Layer = Math.Clamp(cha.effects[i].Layer - cha.effects[i].Reduce, cha.effects[i].LayerMin, cha.effects[i].LayerMax);
                cul(1, cha.effects[i], cha);
            }
            if (cha.effects[i].Turn == 0 || cha.effects[i].Layer == 0)
            {
                cul(-1, cha.effects[i], cha);
                cha.effects.Remove(cha.effects[i]);
            }
        }  
    }

    public static Effect Copy(Effect Sourse)
    {
        Effect Cop = new Effect();
        Cop.Overlay = Sourse.Overlay;
        Cop.Layer = Sourse.Layer;
        Cop.LayerMin = Sourse.LayerMin;
        Cop.LayerMax = Sourse.LayerMax;
        Cop.Value = Sourse.Value;
        Cop.Info = Sourse.Info;
        Cop.Name = Sourse.Name;
        Cop.Rate = Sourse.Rate;
        Cop.Reduce = Sourse.Reduce;
        Cop.Silence = Sourse.Silence;
        Cop.Turn = Sourse.Turn;
        Cop.Type = Sourse.Type;
        Cop.Stack = Sourse.Stack;
        Cop.Panel = Sourse.Panel;
        return Cop;
    }

    public static void cul(int value, Effect effect,ChaControl cha)//value = -1移除
    {
        Chainfo chainfo = cha.Chainfo;
        switch (effect.Type)
        {
            case EffectType.ATKChange:
                chainfo.Attack.Rate += effect.Rate*effect.Layer*value;
                chainfo.Attack.Value += (int)effect.Value * effect.Layer * value;
               chainfo.Attack.Cul();
                break;
            case EffectType.RECChange:
                chainfo.Recover.Value += (int)effect.Value * effect.Layer * value;
                chainfo.Recover.Rate += effect.Rate * effect.Layer * value;
                chainfo.Recover.Cul();
                break;
            case EffectType.ControlChange:
                chainfo.Control.Org =Mathf.Clamp(value+ chainfo.Control.Org, 1,5);
                chainfo.Control.Now = Mathf.Clamp(value + chainfo.Control.Now, 0, 5);
                for (int i = 0; i < GameSystem.Script.UI.player.Head.Chainfo.Control.Now; i++)
                    GameSystem.Script.UI.player.RotateIcon[i].color = Color.white;
                //Debug.Log(chainfo.Control.Org);
                break;
            case EffectType.DEFChange:
                cha.Def.Rate += effect.Rate * value;
                cha.Def.Value += (int)effect.Value * value;
                break;
            case EffectType.CDNowChange:
                chainfo.CD.Now += (int)effect.Value;
                chainfo.CD.Now = Mathf.Clamp(chainfo.CD.Now, 0, chainfo.CD.Org);
                break;
            case EffectType.CDOrgChange:
                chainfo.CD.Value += (int)effect.Value * value;
                chainfo.CD.Rate += value==1? effect.Rate:0;
                break;
            case EffectType.Reflect:
                cha.Reflect.Rate += effect.Rate * value;
                cha.Reflect.Value += (int)effect.Value * value;
                break;
            case EffectType.FollowUp:
                cha.FollowUp.Rate += effect.Rate * value;
                cha.FollowUp.Value += (int)effect.Value * value;
                break;
            case EffectType.Born:
                cha.Beattacked(Mathf.RoundToInt(effect.Rate*GameSystem.Script.UI.player.Hp.Org),false,ElementType.Burn,null);
                break;
            case EffectType.Poison:
                cha.Beattacked(Mathf.RoundToInt(effect.Layer * effect.Rate * GameSystem.Script.UI.player.Hp.Org), false, ElementType.Burn, null) ;
                break;
            case EffectType.Chain:
                cha.Chain[(int)effect.Rate].Now += value * (int)effect.Value;
                break;
            case EffectType.PanelControl:
                    Debug.Log(effect.Panel.PanelControlType);
                    GameSystem.Script.PanelControll(effect.Panel);
                break;
            default:
                break;
        }
    }
}
public enum SkillType { DirectAttck直接攻擊, Effect效果,PanelControl版面控制,Recover生命恢復,Text文本,PacageSet設定包,PackageDo執行包};
public enum EffectType {None, paralysis,Born,Poison,ATKChange,DEFChange,RECChange,Silence,ElementChange,ControlChange,Reflect,FollowUp,CDOrgChange,CDNowChange,Chain,PanelControl}
public enum ElementType { Fired, Water,Light, Grass, Rock, PopUp, Bubble, Poison, Trap,Burn,Timer,All}
public enum ConditionType {無,屬性,羈絆類型,效果,消除量,剩餘量,倍率,數值,血量,回合數,在前頭,在前頭回合開始前}
public enum Cul {大於,小於,等於,加,減,乘,除,餘,設定為,沒有,有}
[Flags]
public enum Position { 無=0,一=1, 二=2, 三=4, 四=8, 敵人=16, 自身=32, 位置偏差=64,隨機=128,與上述相同=256}
public enum Group {無,學術 }
public enum PanelControlType {破壞,改變掉落率,立即變換,掉落時變換 }
public enum Reference {現有攻擊力,原攻擊力,現有恢復力,原恢復力,現有生命力,原有生命力,現有冷卻,無}
public enum Accord { 疊層,回合,數值,倍率,無}
[Flags]
public enum ChaSKill{Main=1,ColorA=2,ColorB=4,ColorC=8,BG1=16,BG2=32}

public enum Timing { 關卡開始,階層開始,回合開始,操作後,發動技能,操作用盡,回合結束,階層結束}


