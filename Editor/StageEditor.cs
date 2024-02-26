using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[CustomEditor(typeof(Stageinfo))]
public class StageEditor : Editor
{
    public Stageinfo stageinfo;
    public int Phase;
    public string[] PhaseEnemyName =new string[] {} ;

    public int Package;
    public string[] PackageName = new string[] { };

    public int Skill;
    public string[] SkillName = new string[] { };

    public int SKillCompoment;
    public string[] SkillCompomentName = new string[] { };

    //分割線
    bool IsDrag;
    float spilitterPos;
    Rect spilitterRect;

    SerializedProperty S_Name;
    SerializedProperty S_Element;
    SerializedProperty S_Attack;
    SerializedProperty S_Recover;
    SerializedProperty S_Hp;
    SerializedProperty S_CD;
    SerializedProperty S_Enemy;
    SerializedProperty S_Package;
    SerializedProperty S_Skill;   
    SerializedProperty S_Skillcompoment;
    public void OnEnable()
    {
        stageinfo = (Stageinfo)target;
        S_Enemy = serializedObject.FindProperty("Enemy");      
    }


    public override void OnInspectorGUI()
    {
        
        serializedObject.Update();
        Array.Resize(ref PhaseEnemyName, stageinfo.enemyInfo.Count);
        for (int i = 0; i < stageinfo.enemyInfo.Count; i++)
        {
            // Debug.Log(i);
            PhaseEnemyName[i] = stageinfo.enemyInfo[i].Name;
        }
        
        Array.Resize(ref PackageName, stageinfo.enemyInfo[Phase].packages.Count+1);
        PackageName[0] = "基本數值";
        for (int i = 0; i < stageinfo.enemyInfo[Phase].packages.Count;i++)
        {
            PackageName[i+1] = stageinfo.enemyInfo[Phase].packages[i].Name;
        }       
        Phase = GUILayout.Toolbar(Phase, PhaseEnemyName);
        EditorGUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("新增敵人"))
            {
                stageinfo.enemyInfo.Insert(Phase, new EnemyInfo());
                Phase = Math.Min(Phase + 1, stageinfo.enemyInfo.Count-1) ;
            }
            if (stageinfo.enemyInfo.Count > 1)
            {
                if (GUILayout.Button("移除敵人"))
                {
                    stageinfo.enemyInfo.RemoveAt(Phase - 1);
                    Phase = Math.Max(0, Phase - 1);
                }
            }
        }
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(10);
        Package = GUILayout.Toolbar(Package, PackageName);
       
        S_Enemy = serializedObject.FindProperty("enemyInfo").GetArrayElementAtIndex(Phase);


        EditorGUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("新增包"))
            {
                stageinfo.enemyInfo[Phase].packages.Insert(Mathf.Min(Package ,2), new Package());
                Package = Math.Min(Package+1 , stageinfo.enemyInfo[Phase].packages.Count+1);
            }
            if (Package > 2)
            {
                if (GUILayout.Button("移除包"))
                {
                    stageinfo.enemyInfo[Phase].packages.RemoveAt(Package-1);
                    Package = Math.Max(0, Package - 1);
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        if (Package==0)  
            InfoRefresh();
        else 
        {
            S_Package = S_Enemy.FindPropertyRelative("packages").GetArrayElementAtIndex(Mathf.Max(Package - 1,0));
            SerializedProperty P_Name = S_Package.FindPropertyRelative("Name");
            EditorGUILayout.PropertyField(P_Name, new GUIContent("名稱"));
            EditorGUILayout.BeginHorizontal();
            
            SerializedProperty P_Random = S_Package.FindPropertyRelative("Random");
            SerializedProperty P_Lock= S_Package.FindPropertyRelative("Lock");
            
            EditorGUILayout.PropertyField(P_Lock, new GUIContent("保護"));
            EditorGUILayout.PropertyField(P_Random, new GUIContent("隨機"));
            EditorGUILayout.EndHorizontal();
           
            DrawSkill(stageinfo.enemyInfo[Phase].packages[Package-1]);
            SerializedProperty C_Base = S_Skillcompoment;
            GUILayout.Label("假如");
            ChaEditor.TypeCheck(C_Base.FindPropertyRelative("Ifcase"));
            GUILayout.Label("則在");
            ChaEditor.TypeCheck(C_Base.FindPropertyRelative("TargetSelect"));
            ChaEditor.ResultType(C_Base);
        }
        serializedObject.ApplyModifiedProperties();
        base.OnInspectorGUI();
    }
    public void Call()
    {
       
    }
    public void InfoRefresh()
    {
        S_Name = S_Enemy.FindPropertyRelative("Name");
        S_Element = S_Enemy.FindPropertyRelative("element");
        S_Attack = S_Enemy.FindPropertyRelative("Attack");
        S_Recover = S_Enemy.FindPropertyRelative("Recover");
        S_Hp = S_Enemy.FindPropertyRelative("Hp");
        S_CD = S_Enemy.FindPropertyRelative("CD");

        EditorGUILayout.PropertyField(S_Name, new GUIContent("名稱"));
        EditorGUILayout.PropertyField(S_Element, new GUIContent("屬性"));
        EditorGUILayout.PropertyField(S_Attack, new GUIContent("攻擊力"));
        EditorGUILayout.PropertyField(S_Recover, new GUIContent("恢復力"));
        EditorGUILayout.PropertyField(S_Hp, new GUIContent("生命力"));
        EditorGUILayout.PropertyField(S_CD, new GUIContent("CD"));
    }
    public void DrawSkill(Package package)
    {       
        Array.Resize(ref SkillName, package.skills.Count);
        GUILayout.Space(20);
        for (int i= 0;i< package.skills.Count;i++)
        {
            SkillName[i] = package.skills[i].Name + "\n" + package.skills[i].Info;
        }
        if (Skill > package.skills.Count-1) Skill = 0;
        Skill = GUILayout.Toolbar(Skill, SkillName);
        S_Skill = S_Package.FindPropertyRelative("skills").GetArrayElementAtIndex(Skill);
        
        EditorGUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("新增技能"))
            {
                stageinfo.enemyInfo[Phase].packages[Package - 1].skills.Insert(Skill + 1, new Skill());
                Skill++;
            }
            if (stageinfo.enemyInfo[Phase].packages[Package-1].skills.Count!=1)
            {
                if (GUILayout.Button("移除技能"))
                {
                    stageinfo.enemyInfo[Phase].packages[Package - 1].skills.RemoveAt(Skill);
                    Skill = Math.Max(0, Skill - 1);
                }
            }
        }
        EditorGUILayout.EndHorizontal();
        SerializedProperty K_Name = S_Skill.FindPropertyRelative("Name");
        SerializedProperty K_Info = S_Skill.FindPropertyRelative("Info");
        EditorGUILayout.PropertyField(K_Name, new GUIContent("技能名稱"));
        EditorGUILayout.PropertyField(K_Info, new GUIContent("技能敘述"));

        DrawCompoment(package.skills[Skill]);
    }

    public void DrawCompoment(Skill skill)
    {
        Array.Resize(ref SkillCompomentName, skill.skillComponenets.Count);
        GUILayout.Space(20);
        for (int i = 0; i < skill.skillComponenets.Count; i++)
        {
            SkillCompomentName[i] = skill.skillComponenets[i].Name + "\n" + skill.skillComponenets[i].info;
        }
        if (SKillCompoment > skill.skillComponenets.Count - 1) SKillCompoment = 0;
        SKillCompoment = GUILayout.Toolbar(SKillCompoment, SkillCompomentName);
        S_Skillcompoment = S_Skill.FindPropertyRelative("skillComponenets").GetArrayElementAtIndex(SKillCompoment);
        
        EditorGUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("新增元件"))
            {
                stageinfo.enemyInfo[Phase].packages[Package - 1].skills[Skill].skillComponenets.Insert(SKillCompoment + 1, new SkillComponenet());
                SKillCompoment++;
            }
            if (stageinfo.enemyInfo[Phase].packages[Package - 1].skills[Skill].skillComponenets.Count != 1)
            {
                if (GUILayout.Button("移除元件"))
                {
                    stageinfo.enemyInfo[Phase].packages[Package - 1].skills[Skill].skillComponenets.RemoveAt(SKillCompoment);
                    SKillCompoment = Math.Max(0, Skill - 1);
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        SerializedProperty C_Name = S_Skillcompoment.FindPropertyRelative("Name");
        SerializedProperty C_Info = S_Skillcompoment.FindPropertyRelative("info");
        EditorGUILayout.PropertyField(C_Name, new GUIContent("元件名稱"));
        EditorGUILayout.PropertyField(C_Info, new GUIContent("元件敘述"));

    }
}
