using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemySystem : MonoBehaviour
{

    public static EnemySystem Script;
    public EnemyInfo enemyInfo;
    public Package NowPackage;
    public EnemyUI enemyUI;
    public List<Skill> Plan;
    public int Now,Phase;
    public float WaitNextTime;
    public bool SwitchPhase = true;
    List<Skill> NextSkill;
    // Start is called before the first frame update
    void Start()
    {        
        enemyInfo.chaControl = GetComponent<ChaControl>();
        Script = this;
        enemyInfo.chaControl.Chainfo.Attack.Zero();
        enemyInfo.chaControl.Chainfo.Recover.Zero();

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void LateUpdate()
    {
        if (enemyInfo.Hp.x == 0 &&Statistics.PhaseTurn%2==1)
        {
            GameSystem.Script.UI.player.Head.Chainfo.Control.Now = 0;
            if (GameSystem.Script.Team.ChaControls[0].Queue.Count==0&& GameSystem.Script.Team.ChaControls[1].Queue.Count == 0&& GameSystem.Script.Team.ChaControls[2].Queue.Count == 0&& GameSystem.Script.Team.ChaControls[3].Queue.Count == 0)
            {                
                GameSystem.Script.NextTurn();
                EnemySystem.Script.SwitchPhase = true;
            }
        }
    }

    public void LoadEnemy(EnemyInfo x)
    {
        Statistics.PhaseTurn = 0;
        enemyInfo.Effects.Clear();
        enemyInfo = x;
        enemyInfo.chaControl = GetComponent<ChaControl>();
        enemyInfo.CD.x = (int)enemyInfo.CD.y;
        enemyInfo.Hp.x = enemyInfo.Hp.y;
        Now = 0;
        enemyInfo.chaControl.Chainfo.Attack.Org = enemyInfo.Attack;
        enemyInfo.chaControl.Chainfo.Attack.Now= enemyInfo.Attack;
    }

    public IEnumerator Turn(float t)
    {
        Statistics.EnemyTakenDamage = 0;
        yield return new WaitForSeconds(t);
        WaitNextTime = 0.8f;
        Debug.Log("EnemyTurn");
        if(enemyInfo.Hp.x<=0)
        {
            enemyInfo.chaControl.Queue.Clear();
            SwitchPhase = true;
        }
        if (SwitchPhase)
        {
            SwitchPhase = false;
            enemyUI.Phase.text = "Phase  " +(1+ Phase) + "/" + GameSystem.Script.stageinfo.enemyInfo.Count ;
            enemyUI.PhaseAni.Play();
            LoadEnemy(GameSystem.Script.stageinfo.enemyInfo[Phase]);
            Phase++;
            enemyInfo.Hp.x = (int)enemyInfo.Hp.y;
            enemyInfo.Hp.y = (int)enemyInfo.Hp.y;
            PackageLoad(enemyInfo.packages[0]);
        }   
        else 
        {
            enemyInfo.CD.x -= 1;
            if (enemyInfo.packages[1].skills.Count > 0)
                PackageLoad(enemyInfo.packages[1]);
            if (enemyInfo.CD.x <= 0)
            {
                if (Plan.Count != 0)
                {
                    foreach (Skill a in Plan)
                    {
                        SkillLoad(a, enemyInfo.chaControl);
                        WaitNextTime += 0.2f;
                    }
                    Plan.Clear();
                }
                else
                {             
                    if (NowPackage.Random)
                        SkillLoad(NowPackage.skills[UnityEngine.Random.Range(0, NowPackage.skills.Count)], enemyInfo.chaControl);
                    else
                    {
                        SkillLoad(NowPackage.skills[Now], enemyInfo.chaControl);
                        Now++;
                    }

                    WaitNextTime += 0.2f;
                }
                enemyInfo.CD.x = enemyInfo.CD.y;
            }
        }
        enemyUI.CD.text = "CD "+enemyInfo.CD.x.ToString();
        if (enemyInfo.chaControl.Queue.Count>0)
        {
           
            for (int i = 0; i < enemyInfo.chaControl.Queue.Count; i++)
            {
                GameSystem.QueueDo(enemyInfo.chaControl, enemyInfo.chaControl.Queue[i]);
                yield return new WaitForSeconds(0.2f);
            }
            enemyInfo.chaControl.Queue.Clear();
        }
        GameSystem.Script.Invoke("NextTurn", WaitNextTime);
        yield return 0;
    }
    void PackageLoad(Package package)
    {
        foreach (Skill skill in package.skills)
        {
            SkillLoad(skill, enemyInfo.chaControl);
            WaitNextTime += 0.2f*skill.skillComponenets.Count;
        }
    }

    public static void SkillLoad(Skill skill,ChaControl y)
    {
        Debug.Log("read  :  "+skill.Name);
        for (int i = 0; i < skill.skillComponenets.Count; i++)
        {
            GameSystem.Script.newSKillActive(skill.skillComponenets[i], y);            
        }
    }

    
}
[System.Serializable]
public class EnemyUI
{
    public GameObject HpPivot;
    public Text HpText, CD,Phase,TakenDamage;
    public Animation Animation,PhaseAni;
}
public class dir
{
    public int Damage;
    public bool Defence;
    public ElementType element;
    public ChaControl source;
}
