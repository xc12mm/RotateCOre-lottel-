using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "Stage",menuName ="Stage")]
public class Stageinfo : ScriptableObject
{
     public List<EnemyInfo> enemyInfo=new List<EnemyInfo>() { new EnemyInfo()};
}

[System.Serializable]
public class EnemyInfo
{
    public string Name, element;
    public ChaControl chaControl;
    public int Attack, Recover;
    public Vector2 Hp, CD, ATKChange, DefChange, RecChange;
    public Sprite Image, Icon;
    public List<Package> packages=new List<Package>() { new Package("������"),new Package("�^�X����"),new Package("�s���]")};
    public List<Effect> Effects;
    public EnemyInfo()
    {
        Name = "�s���ĤH";
    }
}
