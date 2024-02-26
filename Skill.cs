
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
        Name = "�s���ޯ�";
        Info = "�s���y�z";
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
        Name = "�s������";
        info = "�s���ԭz";
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
        Name = "�s���]";
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

    public static void cul(int value, Effect effect,ChaControl cha)//value = -1����
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
public enum SkillType { DirectAttck��������, Effect�ĪG,PanelControl��������,Recover�ͩR��_,Text�奻,PacageSet�]�w�],PackageDo����]};
public enum EffectType {None, paralysis,Born,Poison,ATKChange,DEFChange,RECChange,Silence,ElementChange,ControlChange,Reflect,FollowUp,CDOrgChange,CDNowChange,Chain,PanelControl}
public enum ElementType { Fired, Water,Light, Grass, Rock, PopUp, Bubble, Poison, Trap,Burn,Timer,All}
public enum ConditionType {�L,�ݩ�,��������,�ĪG,�����q,�Ѿl�q,���v,�ƭ�,��q,�^�X��,�b�e�Y,�b�e�Y�^�X�}�l�e}
public enum Cul {�j��,�p��,����,�[,��,��,��,�l,�]�w��,�S��,��}
[Flags]
public enum Position { �L=0,�@=1, �G=2, �T=4, �|=8, �ĤH=16, �ۨ�=32, ��m���t=64,�H��=128,�P�W�z�ۦP=256}
public enum Group {�L,�ǳN }
public enum PanelControlType {�}�a,���ܱ����v,�ߧY�ܴ�,�������ܴ� }
public enum Reference {�{�������O,������O,�{����_�O,���_�O,�{���ͩR�O,�즳�ͩR�O,�{���N�o,�L}
public enum Accord { �|�h,�^�X,�ƭ�,���v,�L}
[Flags]
public enum ChaSKill{Main=1,ColorA=2,ColorB=4,ColorC=8,BG1=16,BG2=32}

public enum Timing { ���d�}�l,���h�}�l,�^�X�}�l,�ާ@��,�o�ʧޯ�,�ާ@�κ�,�^�X����,���h����}


