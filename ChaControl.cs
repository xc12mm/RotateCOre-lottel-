using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ChaControl : MonoBehaviour
{
    public Chainfo Chainfo;
    public ValuePack Def,Reflect,FollowUp;
    public List<ValuePack> Chain;
    public List<Effect> effects;
    public List<Animation> animations;
    public Animation ani,DamageAni;
    public bool TurnEnd,Enemy;
    public int number;
    public List<object> Queue =new List<object>();//effect directattack

    public GameObject DamageView,DamageText;
    public Text CD,AttackDamage;
    // Start is called before the first frame update

    void Awake()
    {
        DamageView = transform.GetChild(1).gameObject;
    }
    void Start()
    {
       
        CD = GetComponentInChildren<Text>();    }

    // Update is called once per frame
    void Update()
    {

    }
    public void Turn()
    {
        if (Chainfo!=null)
        {
            
            if (GameSystem.Script.UI.player.Head == this)
            {
                GameSystem.Script.TurnEnd(Chainfo.ColorSkillA, this);
                GameSystem.Script.TurnEnd(Chainfo.ColorSkillB, this);
                GameSystem.Script.TurnEnd(Chainfo.ColorSkillC, this);
                GameSystem.Script.UI.player.Head.Chainfo.Control.Now = Chainfo.Control.Now;
                //GameSystem.Script.UI.player.IsHead.transform.position = this.transform.position + new Vector3(7, 7,0);
            }
            GameSystem.Script.TurnEnd(Chainfo.BackGroundSkillA, this);
            GameSystem.Script.TurnEnd(Chainfo.BackGroundSkillB, this);
            Chainfo.CD.Now -= Chainfo.CD.Now>0? 1:0;
            CD.text = Chainfo.CD.Now.ToString();
            if (effects.Count!=0)
                Effect.Active(this);

                StartCoroutine(Queue_());

        }
        
        StartCoroutine(DamageAnimation());
    }
    public IEnumerator Queue_()
    {
        //Debug.Log(Queue.Count);
        if (Queue.Count > 0)
        {
            for (int i = 0; i < Queue.Count; i++)
            {
                GameSystem.QueueDo(this, Queue[i]);
                yield return new WaitForSeconds(0.2f);
            }
            Queue.Clear();
        }
    }

    public void Press()
    {
        GameSystem.Script.UI.Cha.SkillBars[0].GetComponentInChildren<Text>().text = Chainfo.MainSkill.Info;
        GameSystem.Script.UI.Cha.SkillBars[1].GetComponentInChildren<Text>().text = Chainfo.BackGroundSkillA.Info;
        GameSystem.Script.UI.Cha.SkillBars[2].GetComponentInChildren<Text>().text = Chainfo.BackGroundSkillB.Info;
        GameSystem.Script.UI.Cha.SkillBars[3].GetComponentInChildren<Text>().text = Chainfo.ColorSkillA.Info;
        GameSystem.Script.UI.Cha.SkillBars[4].GetComponentInChildren<Text>().text = Chainfo.ColorSkillB.Info;
        GameSystem.Script.UI.Cha.SkillBars[5].GetComponentInChildren<Text>().text = Chainfo.ColorSkillC.Info;
        GameSystem.Script.UI.Cha.BasicBar.SetActive(true);
    }

    public void Click()
    {
        GameSystem.Script.UI.Cha.SkillName.text = Chainfo.MainSkill.Name;
        GameSystem.Script.UI.Cha.SkillInfo.text = Chainfo.MainSkill.Info;
        GameSystem.Script.UI.Cha.UseSkill.gameObject.SetActive(Chainfo.CD.Now == 0 ? true : false);
        GameSystem.Script.UI.player.Use = this;
    }
    public void Beattacked(int Damage,bool Defence,ElementType Element,ChaControl cha)
    {

            if (!Enemy)
            {
            int D = Mathf.Max(Defence ? Mathf.RoundToInt((Damage - (int)Def.Value) * Def.Rate) : Damage, 1);
            GameObject ru = Instantiate(DamageText, Vector3.zero, Quaternion.identity, DamageView.transform);
                ru.GetComponentInChildren<Text>().text = Defence ? Mathf.RoundToInt((Damage - (int)Def.Value) * Def.Rate).ToString() : Damage.ToString();
                EnemySystem.Script.enemyUI.Animation.clip = EnemySystem.Script.enemyUI.Animation.GetClip("ATKpos" + number);
                EnemySystem.Script.enemyUI.Animation.Play();
            GameSystem.Script.UI.player.Hp.Now -= D;
                GameSystem.Script.UI.player.HpPivot.transform.localScale = new Vector2((float)GameSystem.Script.UI.player.Hp.Now / (float)GameSystem.Script.UI.player.Hp.Org, 1);
                GameSystem.Script.UI.player.HpText.text = GameSystem.Script.UI.player.Hp.Now.ToString() + "/" + GameSystem.Script.UI.player.Hp.Org.ToString();
            }
            else
            {
            int D = Mathf.Max(Defence ? Mathf.RoundToInt((Damage - (int)Def.Value) * Def.Rate) : Damage, 1);          
            if(cha!=null)
            {
                cha.DamageAni.Play();
                StartCoroutine(cha.JumpDamage(D, Element));
            }

                EnemySystem.Script.enemyInfo.Hp.x -= D;
                Statistics.EnemyTakenDamage += D;
                EnemySystem.Script.enemyUI.TakenDamage.text=Statistics.EnemyTakenDamage.ToString();
                EnemySystem.Script.enemyInfo.Hp.x = Mathf.Clamp(EnemySystem.Script.enemyInfo.Hp.x, 0, EnemySystem.Script.enemyInfo.Hp.y);
                EnemySystem.Script.enemyUI.HpPivot.transform.localScale = new Vector3((float)EnemySystem.Script.enemyInfo.Hp.x / EnemySystem.Script.enemyInfo.Hp.y, 1, 1);
                EnemySystem.Script.enemyUI.HpText.text = (100 * Math.Round((float)EnemySystem.Script.enemyInfo.Hp.x / EnemySystem.Script.enemyInfo.Hp.y, 4)).ToString() + "%";
                
        }
            if (Defence || cha != null)
            {
                if (Reflect.Value != 0 || Reflect.Rate != 0)
                {
                    DirectDamage DD = new DirectDamage();
                    DD.IgnoreDefence = true;
                    DD.Rate = Reflect.Rate;
                    DD.Value = Reflect.Value;
                    DD.Reference = Reference.²{¦³§ðÀ»¤O;
                    DD.Target = new List<ChaControl> {cha};
                    Queue.Add(DD);
                }

                    //Queue.Add()
                   // cha.Beattacked((int)(Chainfo.Attack.Now * Reflect.Rate + Reflect.Value), true, Chainfo.element, null);
            }
        
    }

    IEnumerator JumpDamage(int Damage,ElementType element)
    {
        List<Color> c = new List<Color>() {Color.red,Color.blue,Color.yellow,Color.green,Color.gray };
        AttackDamage.color= c[(int)element>3?4:(int)element];
        for(int i=0;i<23;i++)
        {
            if(i>12)
            {
                AttackDamage.text = Mathf.Lerp(0, Damage, (i - 12) * 0.1f).ToString();
                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    IEnumerator DamageAnimation()
    {           
        if (DamageView.transform.childCount != 0)
        {
            Destroy(DamageView.transform.GetChild(0).gameObject);
            yield return new WaitForSeconds(0.3f);
            StartCoroutine(DamageAnimation());
        }
        yield return null;
    }

}







