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
    public string[] SkillName = new string[] {"�򥻸��","�D�ʧޯ�", "�C��ޯ�A", "�C��ޯ�B", "�C��ޯ�C", "�Q�ʧޯ�A", "�Q�ʧޯ�B" };
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
                GUILayout.Label("�W��");
                EditorGUILayout.PropertyField(C_Name, new GUIContent(""));
            }
            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            {
                GUILayout.Label("�ݩ�");
                EditorGUILayout.PropertyField(C_Element, new GUIContent(""));
            }
            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            {
                GUILayout.Label("�N�o");
                EditorGUILayout.PropertyField(C_CD, new GUIContent(""));
            }
            GUILayout.EndVertical();
        }
        GUILayout.EndHorizontal();
            
        
        GUILayout.BeginHorizontal();
        {
            GUILayout.BeginVertical();
            {
                GUILayout.Label("�ͩR�O");
                EditorGUILayout.PropertyField(C_Hp, new GUIContent());
            }
            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            {
                GUILayout.Label("�����O");
                EditorGUILayout.PropertyField(C_Attack, new GUIContent());
            }
            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            {
                GUILayout.Label("��_�O");
                EditorGUILayout.PropertyField(C_Recover, new GUIContent());
            }
            GUILayout.EndVertical();

        }
        GUILayout.EndHorizontal();
        EditorGUILayout.PropertyField(C_Image, new GUIContent("����"));
        EditorGUILayout.PropertyField(C_Icon, new GUIContent("�Y��"));
    }

    public void SkillRefreach(SerializedProperty K_Base)
    {

        SerializedProperty K_Name = K_Base.FindPropertyRelative("Name");
        EditorGUILayout.PropertyField(K_Name, new GUIContent("�ޯ�W��"));
        SerializedProperty K_Info = K_Base.FindPropertyRelative("Info");
        EditorGUILayout.PropertyField(K_Info, new GUIContent("�ޯ�ԭz"));

        string[] skillComponenetsName = new string[K_Base.FindPropertyRelative("skillComponenets").arraySize];
        for (int i = 0; i < K_Base.FindPropertyRelative("skillComponenets").arraySize; i++)
        {
            skillComponenetsName[i] = K_Base.FindPropertyRelative("skillComponenets").GetArrayElementAtIndex(i).FindPropertyRelative("Name").stringValue + "\n" + K_Base.FindPropertyRelative("skillComponenets").GetArrayElementAtIndex(i).FindPropertyRelative("info").stringValue;
        }

        skillComponenetIndex = Mathf.Clamp(GUILayout.Toolbar(skillComponenetIndex, skillComponenetsName), 0, skillComponenetsName.Length - 1);
        EditorGUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("�s�W����"))skills[NowSelectSkill - 1].skillComponenets.Add(new SkillComponenet());
            if (GUILayout.Button("��������")) skills[NowSelectSkill - 1].skillComponenets.RemoveAt(skillComponenetIndex);

        }
        EditorGUILayout.EndHorizontal();
        
        SerializedProperty C_Base = K_Base.FindPropertyRelative("skillComponenets").GetArrayElementAtIndex(skillComponenetIndex);
        SerializedProperty C_Name = C_Base.FindPropertyRelative("Name");
        SerializedProperty C_Info = C_Base.FindPropertyRelative("info");
        EditorGUILayout.PropertyField(C_Name, new GUIContent("����W��"));
        EditorGUILayout.PropertyField(C_Info, new GUIContent("����ԭz"));

        GUILayout.Label("���p");
        TypeCheck(C_Base.FindPropertyRelative("Ifcase"));
        GUILayout.Label("�h�b");
        TypeCheck(C_Base.FindPropertyRelative("TargetSelect"));
        ResultType(C_Base);
    }

    public static void TypeCheck(SerializedProperty TS_Base)
    {
        //SerializedProperty T_Timing = TS_Base.FindPropertyRelative("Timing");
        //EditorGUILayout.PropertyField(T_Timing, new GUIContent("�ˬd�ɾ��I"));
        #region Target          
        SerializedProperty T_Base = TS_Base.FindPropertyRelative("target");
        SerializedProperty T_Position = T_Base.FindPropertyRelative("Position");
        EditorGUILayout.PropertyField(T_Position, new GUIContent("���w��m"));
        #endregion Target
        #region Condition
        SerializedProperty Se_Base = TS_Base.FindPropertyRelative("condition");
        SerializedProperty Se_Type = Se_Base.FindPropertyRelative("type");
        EditorGUILayout.PropertyField(Se_Type, new GUIContent("Ĳ�o���O"));
        if (Se_Type.enumValueIndex == (int)ConditionType.�ݩ� || Se_Type.enumValueIndex == (int)ConditionType.�����q || Se_Type.enumValueIndex == (int)ConditionType.�Ѿl�q)
        {
            SerializedProperty Se_Block = Se_Base.FindPropertyRelative("Block");
            EditorGUILayout.PropertyField(Se_Block);
        }
        else if (Se_Type.enumValueIndex == (int)ConditionType.�ĪG)
        {
            SerializedProperty Se_Effect = Se_Base.FindPropertyRelative("effect");
            SerializedProperty SeE_Name = Se_Effect.FindPropertyRelative("Name");
            EditorGUILayout.PropertyField(SeE_Name, new GUIContent("�ĪG�W��"));
            
        }
        else if (Se_Type.enumValueIndex == (int)ConditionType.��������)
        {
            
        }
        else if (Se_Type.enumValueIndex == (int)ConditionType.�ƭ�)
        {
            SerializedProperty Se_Ref = Se_Base.FindPropertyRelative("Reference");
            EditorGUILayout.PropertyField(Se_Ref, new GUIContent("�Ѧҭ�"));
        }
        #endregion Condition
        if (Se_Type.enumValueIndex != (int)ConditionType.�L)
        {
            SerializedProperty Se_Cul = Se_Base.FindPropertyRelative("Cul");
            EditorGUILayout.PropertyField(Se_Cul, new GUIContent("�p��覡"));
            if (Se_Cul.enumValueIndex != (int)Cul.�� && Se_Cul.enumValueIndex != (int)Cul.�S��)
            {
                SerializedProperty TS_Value = TS_Base.FindPropertyRelative("Value");
                EditorGUILayout.PropertyField(TS_Value, new GUIContent("��"));
                SerializedProperty Se_Accord = Se_Base.FindPropertyRelative("Accord");
                EditorGUILayout.PropertyField(Se_Accord, new GUIContent("��"));
            }
        }
    }

    public static void ResultType(SerializedProperty C_Base)
    {
        SerializedProperty C_Type = C_Base.FindPropertyRelative("SkillType");
        EditorGUILayout.PropertyField(C_Type, new GUIContent("���G"));
        if (C_Type.enumValueIndex == (int)SkillType.DirectAttck��������)
        {
            SerializedProperty D_Base = C_Base.FindPropertyRelative("DirectDamage");
            DirectAttack(D_Base);
        }
        else if (C_Type.enumValueIndex == (int)SkillType.Effect�ĪG)
        {
            SerializedProperty C_Effect = C_Base.FindPropertyRelative("Effect");
            EditorGUILayout.PropertyField(C_Effect, new GUIContent("�ĪG"));
            SerializedProperty C_type = C_Effect.FindPropertyRelative("Type");
            if (C_type.enumValueIndex == (int)EffectType.Silence)
            {
                SerializedProperty C_Silence = C_Effect.FindPropertyRelative("Silence");
                EditorGUILayout.PropertyField(C_Silence, new GUIContent("�I�q��H"));
            }
            else if(C_type.enumValueIndex==(int)EffectType.PanelControl)
            {
                SerializedProperty C_PanelControl = C_Base.FindPropertyRelative("PanelControl");
                EditorGUILayout.PropertyField(C_PanelControl);
            }
        }
        else if (C_Type.enumValueIndex == (int)SkillType.PanelControl��������)
        {
            SerializedProperty C_PanelControl = C_Base.FindPropertyRelative("PanelControl");
            EditorGUILayout.PropertyField(C_PanelControl);
        }
        else if (C_Type.enumValueIndex == (int)SkillType.Recover�ͩR��_)
        {
            SerializedProperty D_Base = C_Base.FindPropertyRelative("DirectDamage");
            DirectAttack(D_Base);
        }
        else if(C_Type.enumValueIndex==(int)SkillType.PacageSet�]�w�]|| C_Type.enumValueIndex == (int)SkillType.PackageDo����]|| C_Type.enumValueIndex == (int)SkillType.Text�奻)
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
        EditorGUILayout.PropertyField(D_Rate, new GUIContent("���v"));
        EditorGUILayout.PropertyField(D_Value, new GUIContent("��"));
        EditorGUILayout.PropertyField(D_Ignore, new GUIContent("�L�����m(�e��)"));
        EditorGUILayout.PropertyField(D_Ref, new GUIContent("�Ѧҭ�"));
    }
}


