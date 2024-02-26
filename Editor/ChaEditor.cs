using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;
using System.Linq;
using GluonGui.Dialog;


[CustomEditor(typeof(Chainfo))]
public class ChaEditor : Editor
{

    public Chainfo Cha;
     
    public int NowSelectSkill;
    public string[] SkillName = new string[] {"基本資料","主動技能", "顏色技能A", "顏色技能B", "顏色技能C", "被動技能A", "被動技能B" };
    public string[] SkillValueName = new string[] { "MainSkill", "ColorSkillA", "ColorSkillB", "ColorSkillC", "BackGroundSkillA", "BackGroundSkillB" };
    public List<Skill> skills = new List<Skill>() { };

    SerializedProperty C_Name;
    SerializedProperty C_Element;
    SerializedProperty C_Hp;
    SerializedProperty C_Attack;
    SerializedProperty C_Recover;
    SerializedProperty C_CD;
    SerializedProperty C_Image, C_Icon;

    int skillComponenetIndex = 0;


    public void OnEnable()
    {
   
        Cha= (Chainfo)target;
        skills.Add(Cha.MainSkill);
        skills.Add(Cha.ColorSkillA);
        skills.Add(Cha.ColorSkillB);
        skills.Add(Cha.ColorSkillC);
        skills.Add(Cha.BackGroundSkillA);
        skills.Add(Cha.BackGroundSkillB);
        C_Name = serializedObject.FindProperty("Name");
        C_Element = serializedObject.FindProperty("element");
        C_Hp = serializedObject.FindProperty("Hp").FindPropertyRelative("Org");
        C_Attack = serializedObject.FindProperty("Attack").FindPropertyRelative("Org");
        C_Recover = serializedObject.FindProperty("Recover").FindPropertyRelative("Org");
        C_CD = serializedObject.FindProperty("CD").FindPropertyRelative("Org");
        C_Image = serializedObject.FindProperty("Image");
        C_Icon = serializedObject.FindProperty("Icon");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        NowSelectSkill = GUILayout.Toolbar(NowSelectSkill, SkillName);
        if(NowSelectSkill==0)
            BasicInfoRefresh();
        else         
            SkillRefreach(serializedObject.FindProperty(SkillValueName[NowSelectSkill-1]));
        serializedObject.ApplyModifiedProperties();
        base.OnInspectorGUI();
        
    }

    void BasicInfoRefresh()
    {
        GUILayout.BeginHorizontal();
        {
            GUILayout.BeginVertical();
            {
                GUILayout.Label("名稱");
                EditorGUILayout.PropertyField(C_Name, new GUIContent(""));
            }
            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            {
                GUILayout.Label("屬性");
                EditorGUILayout.PropertyField(C_Element, new GUIContent(""));
            }
            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            {
                GUILayout.Label("冷卻");
                EditorGUILayout.PropertyField(C_CD, new GUIContent(""));
            }
            GUILayout.EndVertical();
        }
        GUILayout.EndHorizontal();
            
        
        GUILayout.BeginHorizontal();
        {
            GUILayout.BeginVertical();
            {
                GUILayout.Label("生命力");
                EditorGUILayout.PropertyField(C_Hp, new GUIContent());
            }
            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            {
                GUILayout.Label("攻擊力");
                EditorGUILayout.PropertyField(C_Attack, new GUIContent());
            }
            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            {
                GUILayout.Label("恢復力");
                EditorGUILayout.PropertyField(C_Recover, new GUIContent());
            }
            GUILayout.EndVertical();

        }
        GUILayout.EndHorizontal();
        EditorGUILayout.PropertyField(C_Image, new GUIContent("全圖"));
        EditorGUILayout.PropertyField(C_Icon, new GUIContent("縮圖"));
    }

    public void SkillRefreach(SerializedProperty K_Base)
    {

        SerializedProperty K_Name = K_Base.FindPropertyRelative("Name");
        EditorGUILayout.PropertyField(K_Name, new GUIContent("技能名稱"));
        SerializedProperty K_Info = K_Base.FindPropertyRelative("Info");
        EditorGUILayout.PropertyField(K_Info, new GUIContent("技能敘述"));

        string[] skillComponenetsName = new string[K_Base.FindPropertyRelative("skillComponenets").arraySize];
        for (int i = 0; i < K_Base.FindPropertyRelative("skillComponenets").arraySize; i++)
        {
            skillComponenetsName[i] = K_Base.FindPropertyRelative("skillComponenets").GetArrayElementAtIndex(i).FindPropertyRelative("Name").stringValue + "\n" + K_Base.FindPropertyRelative("skillComponenets").GetArrayElementAtIndex(i).FindPropertyRelative("info").stringValue;
        }

        skillComponenetIndex = Mathf.Clamp(GUILayout.Toolbar(skillComponenetIndex, skillComponenetsName), 0, skillComponenetsName.Length - 1);
        EditorGUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("新增元件"))skills[NowSelectSkill - 1].skillComponenets.Add(new SkillComponenet());
            if (GUILayout.Button("移除元件")) skills[NowSelectSkill - 1].skillComponenets.RemoveAt(skillComponenetIndex);

        }
        EditorGUILayout.EndHorizontal();
        
        SerializedProperty C_Base = K_Base.FindPropertyRelative("skillComponenets").GetArrayElementAtIndex(skillComponenetIndex);
        SerializedProperty C_Name = C_Base.FindPropertyRelative("Name");
        SerializedProperty C_Info = C_Base.FindPropertyRelative("info");
        EditorGUILayout.PropertyField(C_Name, new GUIContent("元件名稱"));
        EditorGUILayout.PropertyField(C_Info, new GUIContent("元件敘述"));

        GUILayout.Label("假如");
        TypeCheck(C_Base.FindPropertyRelative("Ifcase"));
        GUILayout.Label("則在");
        TypeCheck(C_Base.FindPropertyRelative("TargetSelect"));
        ResultType(C_Base);
    }

    public static void TypeCheck(SerializedProperty TS_Base)
    {
        //SerializedProperty T_Timing = TS_Base.FindPropertyRelative("Timing");
        //EditorGUILayout.PropertyField(T_Timing, new GUIContent("檢查時機點"));
        #region Target          
        SerializedProperty T_Base = TS_Base.FindPropertyRelative("target");
        SerializedProperty T_Position = T_Base.FindPropertyRelative("Position");
        EditorGUILayout.PropertyField(T_Position, new GUIContent("指定位置"));
        #endregion Target
        #region Condition
        SerializedProperty Se_Base = TS_Base.FindPropertyRelative("condition");
        SerializedProperty Se_Type = Se_Base.FindPropertyRelative("type");
        EditorGUILayout.PropertyField(Se_Type, new GUIContent("觸發類別"));
        if (Se_Type.enumValueIndex == (int)ConditionType.屬性 || Se_Type.enumValueIndex == (int)ConditionType.消除量 || Se_Type.enumValueIndex == (int)ConditionType.剩餘量)
        {
            SerializedProperty Se_Block = Se_Base.FindPropertyRelative("Block");
            EditorGUILayout.PropertyField(Se_Block);
        }
        else if (Se_Type.enumValueIndex == (int)ConditionType.效果)
        {
            SerializedProperty Se_Effect = Se_Base.FindPropertyRelative("effect");
            SerializedProperty SeE_Name = Se_Effect.FindPropertyRelative("Name");
            EditorGUILayout.PropertyField(SeE_Name, new GUIContent("效果名稱"));
            
        }
        else if (Se_Type.enumValueIndex == (int)ConditionType.羈絆類型)
        {
            
        }
        else if (Se_Type.enumValueIndex == (int)ConditionType.數值)
        {
            SerializedProperty Se_Ref = Se_Base.FindPropertyRelative("Reference");
            EditorGUILayout.PropertyField(Se_Ref, new GUIContent("參考值"));
        }
        #endregion Condition
        if (Se_Type.enumValueIndex != (int)ConditionType.無)
        {
            SerializedProperty Se_Cul = Se_Base.FindPropertyRelative("Cul");
            EditorGUILayout.PropertyField(Se_Cul, new GUIContent("計算方式"));
            if (Se_Cul.enumValueIndex != (int)Cul.有 && Se_Cul.enumValueIndex != (int)Cul.沒有)
            {
                SerializedProperty TS_Value = TS_Base.FindPropertyRelative("Value");
                EditorGUILayout.PropertyField(TS_Value, new GUIContent("值"));
                SerializedProperty Se_Accord = Se_Base.FindPropertyRelative("Accord");
                EditorGUILayout.PropertyField(Se_Accord, new GUIContent("給"));
            }
        }
    }

    public static void ResultType(SerializedProperty C_Base)
    {
        SerializedProperty C_Type = C_Base.FindPropertyRelative("SkillType");
        EditorGUILayout.PropertyField(C_Type, new GUIContent("結果"));
        if (C_Type.enumValueIndex == (int)SkillType.DirectAttck直接攻擊)
        {
            SerializedProperty D_Base = C_Base.FindPropertyRelative("DirectDamage");
            DirectAttack(D_Base);
        }
        else if (C_Type.enumValueIndex == (int)SkillType.Effect效果)
        {
            SerializedProperty C_Effect = C_Base.FindPropertyRelative("Effect");
            EditorGUILayout.PropertyField(C_Effect, new GUIContent("效果"));
            SerializedProperty C_type = C_Effect.FindPropertyRelative("Type");
            if (C_type.enumValueIndex == (int)EffectType.Silence)
            {
                SerializedProperty C_Silence = C_Effect.FindPropertyRelative("Silence");
                EditorGUILayout.PropertyField(C_Silence, new GUIContent("沉默對象"));
            }
            else if(C_type.enumValueIndex==(int)EffectType.PanelControl)
            {
                SerializedProperty C_PanelControl = C_Base.FindPropertyRelative("PanelControl");
                EditorGUILayout.PropertyField(C_PanelControl);
            }
        }
        else if (C_Type.enumValueIndex == (int)SkillType.PanelControl版面控制)
        {
            SerializedProperty C_PanelControl = C_Base.FindPropertyRelative("PanelControl");
            EditorGUILayout.PropertyField(C_PanelControl);
        }
        else if (C_Type.enumValueIndex == (int)SkillType.Recover生命恢復)
        {
            SerializedProperty D_Base = C_Base.FindPropertyRelative("DirectDamage");
            DirectAttack(D_Base);
        }
        else if(C_Type.enumValueIndex==(int)SkillType.PacageSet設定包|| C_Type.enumValueIndex == (int)SkillType.PackageDo執行包|| C_Type.enumValueIndex == (int)SkillType.Text文本)
        {
            SerializedProperty C_Text = C_Base.FindPropertyRelative("Text");
            EditorGUILayout.PropertyField (C_Text);
        }
    }

    static void DirectAttack(SerializedProperty Base)
    {
        SerializedProperty D_Rate = Base.FindPropertyRelative("Rate");
        SerializedProperty D_Value = Base.FindPropertyRelative("Value");
        SerializedProperty D_Ignore = Base.FindPropertyRelative("IgnoreDefence");
        SerializedProperty D_Ref = Base.FindPropertyRelative("Reference");
        EditorGUILayout.PropertyField(D_Rate, new GUIContent("倍率"));
        EditorGUILayout.PropertyField(D_Value, new GUIContent("值"));
        EditorGUILayout.PropertyField(D_Ignore, new GUIContent("無視防禦(貫穿)"));
        EditorGUILayout.PropertyField(D_Ref, new GUIContent("參考值"));
    }
}


