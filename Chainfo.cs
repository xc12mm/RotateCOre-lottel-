using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Cha",menuName ="Cha")]
public class Chainfo : ScriptableObject
{
    [HideInInspector] public string Name;
    [HideInInspector] public ElementType element;
    //public string Description;
    [HideInInspector]public ValuePack Hp, Attack, Recover, CD,Control;
    [HideInInspector] public Sprite Image,Icon;
    public Skill MainSkill=new Skill(), ColorSkillA = new Skill(), ColorSkillB = new Skill(), ColorSkillC = new Skill(), BackGroundSkillA = new Skill(), BackGroundSkillB = new Skill();
}
[System.Serializable]
public class ValuePack
{
    public int Org, Now, Value;
    public float Rate=1;
    public void Cul()
    {
        Now=Mathf.RoundToInt((Org*Rate)+Value);
    }

    public void Zero()
    {
        Value = 0;
        Rate = 1;
        Now = Org;
    }
}

