using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class GameSystem : MonoBehaviour
{
    public static GameSystem Script;
    public GameUI UI;
    public Team Team;
    public List<Vector3Int> FallRate;//rate/Count/max
    public GameObject[,] GamePanel = new GameObject[6, 6];
    public List<Vector2Int> BeChecked = new List<Vector2Int>(), SamePos = new List<Vector2Int>();
    public List<GameObject> WaitObject, RotObject;
    public List<Sprite> BlockIcon;
    float timer;
    public Vector2Int LeftControlTimes;
    public int Turn = -1;

    List<GameObject> SameElement = new List<GameObject>();
    int count;
    float BreakCount;

    public Stageinfo stageinfo;
    public ChaControl Enemy;

    // Start is called before the first frame update
    void Start()
    {
        Input.multiTouchEnabled = false;
        FallRateChange(ElementType.Fired, 0, 0);
        FallRateChange(ElementType.Rock, 0, 0);
        RotObject = new List<GameObject>() { null, null, null, null };
        Script = this;
        Team.TeamMemberSet();
        EnemySystem.Script.LoadEnemy(stageinfo.enemyInfo[0]);
        UI.player.Head = Team.ChaControls[0];
        Invoke("PanelFall", 1f);
        Invoke("NextTurn", 2f);
    }
    
    // Update is called once per frame
    void Update()
    {
        Touch touch=new Touch();
        try {touch = Input.GetTouch(0); } catch { }


        timer += Time.deltaTime;
        if ((Input.GetMouseButtonUp(0)||touch.phase==TouchPhase.Began) && UI.Cha.BasicBar.activeSelf)
        {
            UI.Cha.BasicBar.SetActive(false);
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            for (int i = 4; i > -1; i--)
            {
                Debug.Log(i + "-->" + GamePanel[0, i].name.Split(',')[0] + " ||||  " + GamePanel[1, i].name.Split(',')[0] + " ||||  " + GamePanel[2, i].name.Split(',')[0] + " ||||  " + GamePanel[3, i].name.Split(',')[0] + " ||||  " + GamePanel[4, i].name.Split(',')[0] + " ||||  " + GamePanel[5, i].name.Split(',')[0] + " ||||  ");
            }
        }
        if(Input.GetKeyDown(KeyCode.S))
        {
            for (int i = 0; i < Statistics.PanelElement.Count; i++)
                Debug.Log((ElementType)i + Statistics.PanelElement[i].ToString());
        }
        if (Input.GetKeyDown(KeyCode.R))
        { PanelFall(); }
    }

    public void Button()
    {
        GameSystem.Script.PanelControll(new PanelControl(PanelControlType.破壞, 30, new List<ElementType>() { ElementType.Fired, ElementType.Water, ElementType.Light, ElementType.Grass }, ElementType.All));
    }
    public void UseSkill()
    {
        EnemySystem.SkillLoad(UI.player.Use.Chainfo.MainSkill, UI.player.Use);
        StartCoroutine(UI.player.Use.Queue_());
        UI.player.Use.Chainfo.CD.Now = UI.player.Use.Chainfo.CD.Org;
    }
    public void Heal(float value)
    {
        UI.player.Hp.Now = Mathf.Clamp(UI.player.Hp.Now + (int)value, 0, UI.player.Hp.Org);
        GameSystem.Script.UI.player.HpPivot.transform.localScale = new Vector2((float)GameSystem.Script.UI.player.Hp.Now / (float)GameSystem.Script.UI.player.Hp.Org, 1);
        GameSystem.Script.UI.player.HpText.text = GameSystem.Script.UI.player.Hp.Now.ToString() + "/" + GameSystem.Script.UI.player.Hp.Org.ToString();
    }
    public void NextTurn()
    {
        Debug.Log("NextTurn");
        LeftControlTimes.x = LeftControlTimes.y;
        Statistics.PhaseTurn++;
        Statistics.TotalTurn++;
        Turn++;
        if (Turn % 2 == 0)
        {
            foreach (ChaControl x in Team.ChaControls)
            {
                x.Queue_();
            }
            if (UI.player.Head != null)
            {
                UI.player.Hp.Now = Mathf.Clamp(UI.player.Hp.Now + UI.player.Head.Chainfo.Recover.Now / 5 * Statistics.BreaKElement[((int)(UI.player.Head.Chainfo.element) + 3) % 4], 0, UI.player.Hp.Org);
                Enemy.Beattacked(UI.player.Head.Chainfo.Attack.Now / 5 * Statistics.BreaKElement[((int)(UI.player.Head.Chainfo.element))], true, UI.player.Head.Chainfo.element, UI.player.Head);
                UI.player.HpPivot.transform.localScale = new Vector2((float)GameSystem.Script.UI.player.Hp.Now / (float)GameSystem.Script.UI.player.Hp.Org, 1);
                UI.player.HpText.text = GameSystem.Script.UI.player.Hp.Now.ToString() + "/" + GameSystem.Script.UI.player.Hp.Org.ToString();
            }
            EnemySystem.Script.StartCoroutine(EnemySystem.Script.Turn(0.8f));
        }
        else
        {
            Enemy.Queue_();
            foreach (ChaControl x in Team.ChaControls)
            {
                x.Turn();
            }
            UI.player.Head = Team.ChaControls[((Turn) / 2) % Team.Members.Count];
            UI.player.IsHead.transform.position = UI.player.Head.transform.position;
            UI.player.Head.Chainfo.Control.Now = UI.player.Head.Chainfo.Control.Org;
            //Debug.Log("ControlTimes"+UI.player.Head.Chainfo.Control.Now);
            for (int i = 0; i < UI.player.Head.Chainfo.Control.Now; i++)
                UI.player.RotateIcon[i].color = Color.white;
            Statistics.Clear();
            for(int i=0;i<FallRate.Count;i++)
            {
                if (i < 4)
                    FallRate[i]=new Vector3Int(25,0,25*(1+i));
                else
                    FallRate[i] = new Vector3Int(0, 0, 100);
            }
        }
    }

    public void TurnEnd(Skill skill, ChaControl chaControl)
    {
        EnemySystem.SkillLoad(skill, chaControl);
    }

    void PanelBreak()
    {
        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                foreach (Vector2Int x in BeChecked)
                {
                    if (i == x.x && j == x.y)
                    { goto a; }
                }
                List<string> element = new List<string>() { GamePanel[i, j].name.Split(',')[0] };
                if (element[0] == "Rock" && j == 0)
                    DestroyBlock(i, j);
                if ((int)Enum.Parse<ElementType>(element[0]) > 3)
                    goto a;


                count = 1;
                SameElement.Add(GamePanel[i, j]);
                SamePos.Add(new Vector2Int(i, j));
                BeChecked.Add(new Vector2Int(i, j));
                BreakCheck(new Vector2Int(i, j), element);

                int ele = (int)Enum.Parse<ElementType>(GamePanel[i, j].name.Split(',')[0]);
                if (count >= UI.player.Head.Chain[ele].Now && ele < 4)
                {
                    for (int q = 0; q < SamePos.Count; q++)
                    {
                        DestroyBlock(SamePos[q].x, SamePos[q].y);
                        List<Vector2Int> a = new List<Vector2Int>() { new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(-1, 0), new Vector2Int(0, -1) };
                        foreach (Vector2Int x in a)
                        {
                            try {
                                if (GamePanel[SamePos[q].x + x.x, SamePos[q].y + x.y].name.Split(',')[0] == "Bubble")
                                {
                                    DestroyBlock(SamePos[q].x + x.x, SamePos[q].y + x.y);
                                    BeChecked.Add(new Vector2Int(SamePos[q].x + x.x, SamePos[q].y + x.y));
                                }
                            }
                            catch { }
                        }
                    }
                }
            a:
                SameElement.Clear();
                SamePos.Clear();
            }
        }

        Invoke("PanelFall", 0.08f);
    }
    void DestroyBlock(int x, int y)
    {
        WaitObject.Add(GamePanel[x, y]);
        GamePanel[x, y].transform.position += new Vector3(-1000, 0, 0);
        Statistics.BreaKElement[(int)Enum.Parse<ElementType>(GamePanel[x, y].name.Split(',')[0])]++;
        Statistics.PanelElement[(int)Enum.Parse<ElementType>(GamePanel[x, y].name.Split(',')[0])]--;
        GamePanel[x, y] = null;
    }
    void BreakCheck(Vector2Int pos, List<string> element)
    {
        List<Vector2Int> a = new List<Vector2Int>() { new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(-1, 0), new Vector2Int(0, -1) };
        foreach (Vector2Int q in a)
            foreach (string p in element)
            {
                //Debug.Log(pos + "/" + q);
                if (pos.x + q.x < 6 && pos.y + q.y < 5 && pos.x + q.x >= 0 && pos.y + q.y >= 0)
                {
                    foreach (Vector2Int s in BeChecked)
                    {
                        if (s == new Vector2Int(pos.x + q.x, pos.y + q.y))
                            goto a;
                    }
                    try
                    {
                        if (GamePanel[pos.x + q.x, pos.y + q.y].name.Split(',')[0] == p)
                        {
                            SameElement.Add(GamePanel[pos.x + q.x, pos.y + q.y]);
                            SamePos.Add(new Vector2Int(pos.x + q.x, pos.y + q.y));
                            BeChecked.Add(new Vector2Int(pos.x + q.x, pos.y + q.y));
                            BreakCheck(new Vector2Int(pos.x + q.x, pos.y + q.y), element);
                            count++;
                        }
                    }
                    catch { };
                a:;
                }
            }
    }

    void PanelFall()
    {
        bool Again = false;
        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                if (j != 0 && GamePanel[i, j] != null && GamePanel[i, j - 1] == null)
                {
                    Again = true;
                    StartCoroutine(BlockFall(GamePanel[i, j].transform.localPosition, GamePanel[i, j]));
                    GamePanel[i, j - 1] = GamePanel[i, j];
                    GamePanel[i, j] = null;
                }
                if (j == 4 && GamePanel[i, j] == null)
                {
                    GamePanel[i, j] = WaitObject[0];
                    WaitObject[0].GetComponent<Image>().sprite = BlockIcon[FallChoose()];//CHANGE
                    WaitObject[0].name = WaitObject[0].GetComponent<Image>().sprite.name + ",1";
                    WaitObject[0].transform.localPosition = new Vector2(120 * i - 310, 310);
                    StartCoroutine(BlockFall(WaitObject[0].transform.localPosition, WaitObject[0]));
                    WaitObject.Remove(WaitObject[0]);
                    Again = true;
                }
                if (GamePanel[i, j] != null && j == 0 && GamePanel[i, j].name.Split(',')[0] == "Rock")
                {
                    WaitObject.Add(GamePanel[i, j]);
                    GamePanel[i, j].transform.position += new Vector3(-1000, 0, 0);
                    BreakCount++;
                    Statistics.BreaKElement[(int)Enum.Parse<ElementType>(GamePanel[i, j].name.Split(',')[1])]++;
                    GamePanel[i, j] = null;
                    Again = true;
                }
            }
        }
        if (Again)
            Invoke("PanelFall", 0.08f);

    }

    public int FallChoose()
    {
        int r = UnityEngine.Random.Range(0, FallRate[FallRate.Count - 1].z);
        int p = -1;
        for (int i = 0; i < FallRate.Count; i++)
        {
            if (FallRate[i].y > 0)
            {
                FallRate[i] -= new Vector3Int(0, 1, 0);
                p=i;
                goto b;
            }
        }        
        for (int i = 0; i < FallRate.Count; i++)
        {
            if (r < FallRate[i].z)
            {
                p = i;
                goto b;
            }
        }
        b:
        p =(p== -1 ? (int)Element.Bubble : p);
        Statistics.PanelElement[p]++;
        return p;
    }

    public void FallRateChange(ElementType element, int DropCount, int Rate)
    {
        int Count = 0;

        if ((int)element < 4)
        {
            for (int i = 0; i < 4; i++)
            {
                int Add = i == (int)element ? Rate : Rate / -3;
                try { Count = FallRate[i - 1].z
                    + Add + FallRate[i].x; } catch { Count = FallRate[i].x + Add; }
                FallRate[i] = new Vector3Int(FallRate[i].x + Add, FallRate[i].y, Count);

            }
        }
        else
        {
            for (int i = 4; i < FallRate.Count; i++)
            {
                int Add = i == (int)element ? Rate : 0;
                Count = FallRate[i - 1].z + Add + FallRate[i].x;
                FallRate[i] = new Vector3Int(FallRate[i].x + Add, FallRate[i].y, Count);
            }
        }
    }
    public void PanelControll(PanelControl panelControl)
    {
        if(panelControl.PanelControlType==PanelControlType.改變掉落率)
        {
            int Count = 0;
            ElementType element = panelControl.Trans;
            int Rate = (int)panelControl.value;
            if ((int)element < 4)
            {
                for (int i = 0; i < 4; i++)
                {
                    int Add = i == (int)element ? Rate : Rate / -3;
                    try
                    {
                        Count = FallRate[i - 1].z
                        + Add + FallRate[i].x;
                    }
                    catch { Count = FallRate[i].x + Add; }
                    FallRate[i] = new Vector3Int(FallRate[i].x + Add, FallRate[i].y, Count);

                }
            }
            else
            {
                for (int i = 4; i < FallRate.Count; i++)
                {
                    int Add = i == (int)element ? Rate : 0;
                    Count = FallRate[i - 1].z + Add + FallRate[i].x;
                    FallRate[i] = new Vector3Int(FallRate[i].x + Add, FallRate[i].y, Count);
                }
            }
            return;
        }
        if(panelControl.PanelControlType==PanelControlType.掉落時變換)
        {
            FallRate[(int)panelControl.Trans] += new Vector3Int(0,(int)panelControl.value,0);
            return;
        }
        List<GameObject> Target = new List<GameObject>();
        List<Vector2Int> TargetPos = new List<Vector2Int>();
        for (int i = 0; i < 6; i++)
            for (int j = 0; j < 5; j++)
            {
                foreach (ElementType coloR in panelControl.Org)
                    if (GamePanel[i, j].name.Split(',')[0] == coloR.ToString())
                    {
                        Target.Add(GamePanel[i, j]);
                        TargetPos.Add(new Vector2Int(i, j));
                    }
                
            }
        if (panelControl.value > 0)
            while (Target.Count > panelControl.value)
            {
                int random = UnityEngine.Random.Range(0, Target.Count - 1);
                Target.RemoveAt(random);
                TargetPos.RemoveAt(random);
                
            }
        foreach (GameObject x in Target)
        {
            if(panelControl.PanelControlType == PanelControlType.破壞)
            {
                WaitObject.Add(x);
                x.transform.position += new Vector3(-1000, 0, 0);
                GamePanel[TargetPos[0].x, TargetPos[0].y] = null;
                TargetPos.RemoveAt(0);
                Statistics.PanelElement[(int)(Enum.Parse(typeof(ElementType), x.name.Split(",")[0]))]--;

            }
            else if (panelControl.PanelControlType == PanelControlType.立即變換)
            {
                Statistics.PanelElement[(int)(Enum.Parse(typeof(ElementType), x.name.Split(",")[0]))]--;
                Statistics.PanelElement[(int)panelControl.Trans]++;
                x.name = panelControl.Trans.ToString() + "," + x.name.Split(",")[1];
                x.GetComponent<Image>().sprite = BlockIcon[(int)panelControl.Trans];
            }
        }
        PanelFall();
    }

    public void ThumbtackRotate(GameObject game)
    {
        if (UI.player.Head.Chainfo.Control.Now == 0)
            return;
        BeChecked.Clear();
        game.transform.LookAt(Input.mousePosition);
        UI.ThumbtackMid.transform.position = game.transform.position;
        float LeftConner = float.Parse(game.name);
        Vector2Int LC = new Vector2Int((int)LeftConner, Mathf.RoundToInt(10 * (LeftConner - (int)LeftConner)));
        GamePanel[LC.x, LC.y].transform.SetParent(UI.ThumbtackMid.transform);
        GamePanel[LC.x + 1, LC.y].transform.SetParent(UI.ThumbtackMid.transform);
        GamePanel[LC.x, LC.y + 1].transform.SetParent(UI.ThumbtackMid.transform);
        GamePanel[LC.x + 1, LC.y + 1].transform.SetParent(UI.ThumbtackMid.transform);

        RotObject[0] = GamePanel[(int)LC.x, (int)LC.y];
        RotObject[1] = GamePanel[(int)LC.x, (int)LC.y + 1];
        RotObject[2] = GamePanel[(int)LC.x + 1, (int)LC.y + 1];
        RotObject[3] = GamePanel[(int)LC.x + 1, (int)LC.y];
        float angle = game.transform.rotation.eulerAngles.y - 180 > 0 ? game.transform.rotation.eulerAngles.x - 270 : 270 - game.transform.rotation.eulerAngles.x;
        UI.ThumbtackMid.transform.rotation = Quaternion.Euler(0, 0, angle);
    }
    public void ThumbtackEnd(GameObject game)
    {
        if (UI.player.Head.Chainfo.Control.Now == 0)
            return;
        float LeftConner = float.Parse(game.name);
        Vector2Int LC = new Vector2Int((int)LeftConner, Mathf.RoundToInt(10 * (LeftConner - (int)LeftConner)));
        int rot = 0;
        float angle = 0;
        angle = UI.ThumbtackMid.transform.rotation.eulerAngles.z + (UI.ThumbtackMid.transform.rotation.eulerAngles.z < 0 ? 360 : 0);
        if (Input.mousePosition.y > 780)
        {
            UI.ThumbtackMid.transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else if (angle > 225 && angle <= 315)
            rot = 3;
        else if (angle > 135)
            rot = 2;
        else if (angle > 45)
            rot = 1;
        if (rot != 0)
        {
            LeftControlTimes.x -= 1;
            GamePanel[LC.x, LC.y] = RotObject[(rot) < 4 ? rot : rot - 4];
            GamePanel[LC.x, LC.y + 1] = RotObject[(1 + rot) < 4 ? 1 + rot : 1 + rot - 4];
            GamePanel[LC.x + 1, LC.y + 1] = RotObject[(2 + rot) < 4 ? 2 + rot : 2 + rot - 4];
            GamePanel[LC.x + 1, LC.y] = RotObject[(3 + rot) < 4 ? 3 + rot : 3 + rot - 4];
            PanelBreak();
            UI.player.Head.Chainfo.Control.Now -= 1;
            UI.player.RotateIcon[UI.player.Head.Chainfo.Control.Now].color = Color.gray;
            if (UI.player.Head.Chainfo.Control.Now == 0)
            {
                Invoke("NextTurn", 0.8f);

            }
        }
        UI.ThumbtackMid.transform.rotation = Quaternion.Euler(0, 0, 90 * rot);
        foreach (GameObject x in RotObject)
        {
            x.transform.SetParent(UI.GamePanel.transform);
            x.transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        UI.ThumbtackMid.transform.rotation = Quaternion.Euler(0, 0, 0);
        game.transform.rotation = Quaternion.identity;
    }
    IEnumerator BlockFall(Vector2 origin, GameObject gameObject)
    {
        timer = 0;
        for (int i = 0; i < 10; i++)
        {
            gameObject.transform.localPosition = Vector2.Lerp(origin, origin - new Vector2(0, 120), 0.1f * i);
            yield return new WaitForSeconds(0.004f);
        }
        gameObject.transform.localPosition = origin - new Vector2(0, 120);
        yield return 0;
    }
    public void newSKillActive(SkillComponenet x, ChaControl cha)
    {
        bool active=false;
        List<ChaControl> target = new List<ChaControl>();
        float Value = 0, Rate = 0;
        int Layer = 0, Turn = 0;
        if (x.Ifcase.target.Position.HasFlag(Position.自身)||(int)x.Ifcase.target.Position==0)
        {
            target.Add(cha);
        }
        else if (x.Ifcase.target.Position.HasFlag(Position.隨機) )
        {
            Debug.Log("隨機");
            List<int> ints = new List<int>() { 0, 1, 2, 3 };
            for (int i = 0; i < 4; i++)
                if (x.Ifcase.target.Position.HasFlag((Position)(2^i)))
                {
                    int r = UnityEngine.Random.Range(0, 4);
                    target.Add(Team.ChaControls[i]);
                    ints.RemoveAt(i);
                }
        }
        else if ((int)x.Ifcase.target.Position != 0)
        {
            for (int i = 0; i < 5; i++)
            {
                if (x.Ifcase.target.Position.HasFlag((Position)(1 << i)))
                {
                    target.Add(i < 4 ? Team.ChaControls[i] : EnemySystem.Script.enemyInfo.chaControl);
                }
            }
            if (target.Count == 0)
                target.Add(cha);
        }
        for (int i=0;i<target.Count;i++)
        {
            ChaControl a = target[i]; 
            switch (x.Ifcase.condition.type)
            {
                case ConditionType.無:
                    active=true;
                    break;
                case ConditionType.血量:
                    float hp = (a == EnemySystem.Script.enemyInfo.chaControl) ? EnemySystem.Script.enemyInfo.Hp.x / EnemySystem.Script.enemyInfo.Hp.y : (float)UI.player.Hp.Now/(float)UI.player.Hp.Org;
                    active = MathCul(x.Ifcase.condition.Cul, hp, x.Ifcase.Value)>=1?true:false;
                    break;
                case ConditionType.屬性:
                    foreach (TargetSelect.Condition.BlockCheck b in x.Ifcase.condition.Block)
                    {
                        if(!active)
                        active = MathCul(b.cul, (int)a.Chainfo.element, (int)b.ElementType) >= 1 ? true : false;
                    }
                    break;
                case ConditionType.效果:
                    try
                    {
                        active = MathCul(x.Ifcase.condition.Cul, a.effects.FindLast(f => f.Name == x.Ifcase.condition.effect.Name).Layer, x.Ifcase.Value)>=1?true:false;
                    }
                    catch { active = x.Ifcase.condition.Cul == Cul.沒有 ? true : false;                       
                    }
                    break;
                case ConditionType.消除量:
                    foreach (TargetSelect.Condition.BlockCheck b in x.Ifcase.condition.Block)
                    {
                        switch (x.Ifcase.condition.Accord)
                        {
                            case Accord.疊層: Layer = (int)MathCul(x.Ifcase.condition.Cul, Statistics.BreaKElement[(int)b.ElementType], x.Ifcase.Value); break;
                            case Accord.回合: Turn = (int)MathCul(x.Ifcase.condition.Cul, Statistics.BreaKElement[(int)b.ElementType], x.Ifcase.Value); break;
                            case Accord.數值: Value = MathCul(x.Ifcase.condition.Cul, Statistics.BreaKElement[(int)b.ElementType], x.Ifcase.Value); break;
                            case Accord.倍率: Rate = MathCul(x.Ifcase.condition.Cul, Statistics.BreaKElement[(int)b.ElementType], x.Ifcase.Value); break;
                        }
                        if(!active)
                            if (Value != 0 || Turn != 0 || Rate != 0 || Layer != 0)
                                active = true;
                    }
                    break;
                case ConditionType.剩餘量:
                    foreach (TargetSelect.Condition.BlockCheck b in x.Ifcase.condition.Block)
                    {
                        switch (x.Ifcase.condition.Accord)
                        {
                            case Accord.疊層: Layer = (int)MathCul(x.Ifcase.condition.Cul, Statistics.BreaKElement[(int)b.ElementType], x.Ifcase.Value); break;
                            case Accord.回合: Turn = (int)MathCul(x.Ifcase.condition.Cul, Statistics.BreaKElement[(int)b.ElementType], x.Ifcase.Value); break;
                            case Accord.數值: Value = MathCul(x.Ifcase.condition.Cul, Statistics.BreaKElement[(int)b.ElementType], x.Ifcase.Value); break;
                            case Accord.倍率: Rate = MathCul(x.Ifcase.condition.Cul, Statistics.BreaKElement[(int)b.ElementType], x.Ifcase.Value); break;
                        }
                    }
                    if (!active)
                        if (Value != 0 || Turn != 0 || Rate != 0 || Layer != 0)
                            active = true;
                    break;
                case ConditionType.在前頭:
                    active = UI.player.Head ==a ? true : false;
                    break;
                case ConditionType.在前頭回合開始前:
                    active= UI.player.Head == GameSystem.Script.Team.ChaControls[(a.number - 1 % 4)] ? true : false;
                    break;
                case ConditionType.數值:
                    float ReferenceValue = ReferenceCul(x.Ifcase.condition.Reference, a);
                    active = true;
                    switch (x.Ifcase.condition.Accord)
                    {
                        case Accord.疊層: Layer = (int)MathCul(x.Ifcase.condition.Cul, ReferenceValue, x.Ifcase.Value); break;
                        case Accord.回合: Turn = (int)MathCul(x.Ifcase.condition.Cul, ReferenceValue, x.Ifcase.Value); break;
                        case Accord.數值: Value = MathCul(x.Ifcase.condition.Cul, ReferenceValue, x.Ifcase.Value); break;
                        case Accord.倍率: Rate = MathCul(x.Ifcase.condition.Cul, ReferenceValue, x.Ifcase.Value); break;
                        case Accord.無: active = (MathCul(x.Ifcase.condition.Cul, ReferenceValue, x.Ifcase.Value) >= 1) ? true : false; break;
                    }
                    if (!active)
                        if (Value != 0 || Turn != 0 || Rate != 0 || Layer != 0)
                            active = true;
                    break;
                case ConditionType.羈絆類型://未寫
                    break;
            }

            if (Value == 0 && Turn == 0 && Rate == 0 && Layer == 0 && !active)
                target.RemoveAt(i);
            active = false;
        }
        if (target.Count == 0)  goto A;
        float setvalue = Value + 0; Value = 0;
        if ((int)x.TargetSelect.target.Position!=256)
        {
            target.Clear() ;
            if (x.TargetSelect.target.Position.HasFlag(Position.自身)||(int)x.TargetSelect.target.Position==0)
            {
                target.Add(cha);
            }
            else if (x.TargetSelect.target.Position.HasFlag(Position.隨機))
            {
                List<int> ints = new List<int>() { 0, 1, 2, 3 };
                for (int i = 0; i < 4; i++)
                {
                    if (x.TargetSelect.target.Position.HasFlag((Position)(1 << i)))
                    {
                        int r = UnityEngine.Random.Range(0, 4);
                        target.Add(Team.ChaControls[r]);
                        ints.Remove(r);
                    }
                }
            }
            else if((int)x.TargetSelect.target.Position!=0)
            {
                for (int i = 0; i < 5; i++)
                    if (x.TargetSelect.target.Position.HasFlag((Position)(1 << i)))
                    {
                        target.Add(i < 4 ? Team.ChaControls[i] : EnemySystem.Script.enemyInfo.chaControl);
                    }
            }
        }
        for (int i = 0; i < target.Count; i++)
        {
            ChaControl a = target[i];
            switch (x.TargetSelect.condition.type)
            {
                case ConditionType.無:
                    active = true;
                    break;
                case ConditionType.血量:
                    float hp = (a == EnemySystem.Script.enemyInfo.chaControl) ? EnemySystem.Script.enemyInfo.Hp.x / EnemySystem.Script.enemyInfo.Hp.y : (float)UI.player.Hp.Now / (float)UI.player.Hp.Org;
                    active = MathCul(x.TargetSelect.condition.Cul, hp, x.TargetSelect.Value) >= 1 ? true : false;
                    break;
                case ConditionType.屬性:
                    foreach (TargetSelect.Condition.BlockCheck b in x.TargetSelect.condition.Block)
                    {
                        active = MathCul(b.cul, (int)a.Chainfo.element, (int)b.ElementType)>=1?true:false;
                    }
                    break;
                case ConditionType.效果:      
                    try
                    {
                        float vel = MathCul(x.TargetSelect.condition.Cul, a.effects.FindLast(f => f.Name == x.TargetSelect.condition.effect.Name).Layer, x.TargetSelect.Value);
                        active =(vel!=0?true:false);
                    }
                    catch {
                        active = x.TargetSelect.condition.Cul == Cul.沒有 ? true : false; }
                    break;
                case ConditionType.消除量:
                    foreach (TargetSelect.Condition.BlockCheck b in x.TargetSelect.condition.Block)
                    {
                        switch (x.TargetSelect.condition.Accord)
                        {
                            case Accord.疊層: Layer = (int)MathCul(x.TargetSelect.condition.Cul, Statistics.BreaKElement[(int)b.ElementType], x.TargetSelect.Value); break;
                            case Accord.回合: Turn = (int)MathCul(x.TargetSelect.condition.Cul, Statistics.BreaKElement[(int)b.ElementType], x.TargetSelect.Value); break;
                            case Accord.數值: Value = MathCul(x.TargetSelect.condition.Cul, Statistics.BreaKElement[(int)b.ElementType], x.TargetSelect.Value); break;
                            case Accord.倍率: Rate = MathCul(x.TargetSelect.condition.Cul, Statistics.BreaKElement[(int)b.ElementType], x.TargetSelect.Value); break;
                                
                        }
                    }
                    break;
                case ConditionType.剩餘量:
                    foreach (TargetSelect.Condition.BlockCheck b in x.TargetSelect.condition.Block)
                    {
                        switch (x.TargetSelect.condition.Accord)
                        {
                            case Accord.疊層: Layer = (int)MathCul(x.TargetSelect.condition.Cul, Statistics.BreaKElement[(int)b.ElementType], x.TargetSelect.Value); break;
                            case Accord.回合: Turn = (int)MathCul(x.TargetSelect.condition.Cul, Statistics.BreaKElement[(int)b.ElementType], x.TargetSelect.Value); break;
                            case Accord.數值: Value = MathCul(x.TargetSelect.condition.Cul, Statistics.BreaKElement[(int)b.ElementType], x.TargetSelect.Value); break;
                            case Accord.倍率: Rate = MathCul(x.TargetSelect.condition.Cul, Statistics.BreaKElement[(int)b.ElementType], x.TargetSelect.Value); break;

                        }
                    }
                    break;
                case ConditionType.在前頭:
                    active = UI.player.Head == a ? true : false;
                    break;
                case ConditionType.在前頭回合開始前:
                    active = UI.player.Head == GameSystem.Script.Team.ChaControls[(a.number - 1 % 4)] ? true : false;

                    break;
                case ConditionType.數值:
                    active = true;
                    float ReferenceValue = ReferenceCul(x.TargetSelect.condition.Reference, a);
                    switch (x.TargetSelect.condition.Accord)
                    {
                        case Accord.疊層: Layer += (int)MathCul(x.TargetSelect.condition.Cul, ReferenceValue, x.TargetSelect.Value); break;
                        case Accord.回合: Turn += (int)MathCul(x.TargetSelect.condition.Cul, ReferenceValue, x.TargetSelect.Value); break;
                        case Accord.數值: Value += MathCul(x.TargetSelect.condition.Cul, ReferenceValue, x.TargetSelect.Value); break;
                        case Accord.倍率: Rate += MathCul(x.TargetSelect.condition.Cul, ReferenceValue, x.TargetSelect.Value); break;
                        case Accord.無:active=(MathCul(x.TargetSelect.condition.Cul,ReferenceValue,x.TargetSelect.Value)>=1)?true:false;break;
                    }
                    break;
                case ConditionType.羈絆類型://未寫
                    break;
            }
            if (Value == 0 && Turn == 0 && Rate == 0 && Layer == 0 && !active) 
            {              
                target.Remove(target[i]);
            }
            active = false;
        }

        Accord accord = x.TargetSelect.condition.Accord;
        switch (x.SkillType)
        {
            case SkillType.PacageSet設定包:
                if (target.Count == 0) break;
                EnemySystem.Script.NowPackage = EnemySystem.Script.enemyInfo.packages[int.Parse( x.Text)];
                EnemySystem.Script.Now = 0;
                break;
            case SkillType.DirectAttck直接攻擊:
                if (target.Count == 0) break;
                DirectDamage DD = new DirectDamage();
                DD.Rate = Rate + x.DirectDamage.Rate;
                DD.Value = (int)MathF.Round(Value + x.DirectDamage.Value+setvalue);
                DD.Target = target;
                DD.Reference = x.DirectDamage.Reference;
                DD.IgnoreDefence = x.DirectDamage.IgnoreDefence;
                cha.Queue.Add(DD);
                break;
            case SkillType.Effect效果:
                
                if (target.Count == 0) break;
                Effect copy = Effect.Copy(x.Effect);
                copy.Value =x.Effect.Value + setvalue +Value ;
                copy.Rate = x.Effect.Rate + Rate;
                copy.Turn = (int)MathF.Round(x.Effect.Turn + Turn);
                copy.Layer = Mathf.Clamp((int)MathF.Round(x.Effect.Layer + Layer),copy.LayerMin,copy.LayerMax);
                copy.Target = target;
                if(copy.Type==EffectType.PanelControl)
                {
                    copy.Panel = x.PanelControl;
                }
                SkillComponenet.EffectStack(copy, target);
                cha.Queue.Add(copy);
                break;
            case SkillType.Recover生命恢復:
                if (target.Count == 0) break;
                Heal HL = new Heal();
                HL.Rate = Rate+x.DirectDamage.Rate;
                HL.Value = (int)MathF.Round(Value + x.DirectDamage.Value + setvalue);
                HL.Reference = x.DirectDamage.Reference;
                HL.Target = target;
                Debug.Log("heal" + target[0]);
                cha.Queue.Add(HL);
                break;
            case SkillType.PanelControl版面控制:
                cha.Queue.Add(x.PanelControl);
                break;
            case SkillType.Text文本:
                if(target.Count!=0) cha.Queue.Add(x.Text);
                break;
        }
    A:;
    }

    public static void QueueDo(ChaControl cha,object Object)
    {
        switch (Object)
        {
            case DirectDamage x:
                foreach (ChaControl y in x.Target)
                {               
                    y.Beattacked(Mathf.RoundToInt(x.Value + GameSystem.Script.ReferenceCul(x.Reference, cha) * x.Rate), x.IgnoreDefence, cha.Chainfo.element, cha);
                }
                break;
            case Effect x:
                cha.transform.GetChild(2).GetComponent<Animation>().Play();
                break;
            case Heal x:
                foreach (ChaControl y in x.Target)
                {
                    GameSystem.Script.Heal(x.Value+ GameSystem.Script.ReferenceCul(x.Reference, y) * x.Rate);                   
                }
                break;
            case PanelControl x:
                GameSystem.Script.PanelControll(x);
                break;
            case string x:
                GameSystem.Script.UI.textBar.text.text = x;
                GameSystem.Script.UI.textBar.animation.Play();
                break;


        }


    }
    public float ReferenceCul(Reference refe,ChaControl a)
    {
        float ReferenceValue = 0;
        switch (refe)
        {
            case Reference.現有攻擊力:
                ReferenceValue = a.Chainfo.Attack.Now; break;
            case Reference.原攻擊力:
                ReferenceValue = a.Chainfo.Attack.Org; Debug.Log("Org"+a.Chainfo.Attack.Org); break;
            case Reference.現有恢復力:
                ReferenceValue = a.Chainfo.Recover.Now; break;
            case Reference.原恢復力:
                ReferenceValue = a.Chainfo.Recover.Org; break;
            case Reference.現有冷卻:
                ReferenceValue = a.Chainfo.CD.Now; break;
            case Reference.現有生命力:
                ReferenceValue = a == EnemySystem.Script.enemyInfo.chaControl ? Enemy.Chainfo.Hp.Now : UI.player.Hp.Now;
                break;
            case Reference.原有生命力:
                ReferenceValue = a == EnemySystem.Script.enemyInfo.chaControl ? Enemy.Chainfo.Hp.Org : UI.player.Hp.Org;
                break;
            case Reference.無:ReferenceValue = 0;break;
        }
        return (ReferenceValue);
    }

    public float MathCul(Cul cul,float value1,float value2)
    {
        switch (cul)
        {
            case Cul.乘:return value1*value2;
            case Cul.除:return value1/value2;
            case Cul.加:return value2+value1;
            case Cul.減:return value1 - value2;
            case Cul.大於: return value1 > value2 ? 1: 0 ;
            case Cul.小於:return value1 < value2 ? 1: 0 ;
            case Cul.等於:return (value1 == value2 )? 1 : 0;
            case Cul.設定為:return value2;
            case Cul.有:return value1!=0? value1 : 0 ;
            case Cul.沒有:return value1==0? 1 : 0 ;
                default: return 0;
        }
        
    }
}
[System.Serializable]
public class Team
{
    public List<Chainfo> Members;
    public List<ChaControl> ChaControls;
    public void TeamMemberSet()
    {
        for (int i = 0; i < Members.Count; i++)
        {
            GameSystem.Script.UI.Cha.Image[i].sprite = Members[i].Icon;
            ChaControls[i].Chainfo = Members[i];
            GameSystem.Script.UI.player.Hp.Org += ChaControls[i].Chainfo.Hp.Org;
            GameSystem.Script.UI.player.Hp.Now += ChaControls[i].Chainfo.Hp.Org;
            ChaControls[i].Chainfo.Control.Org = 3;
            ChaControls[i].Chainfo.Control.Now = 0;
            ChaControls[i].Chainfo.Attack.Zero();
            ChaControls[i].Chainfo.Recover.Zero();
            ChaControls[i].Chainfo.CD.Zero();
        }
        GameSystem.Script.UI.player.HpText.text = GameSystem.Script.UI.player.Hp.Now.ToString() + "/"+GameSystem.Script.UI.player.Hp.Org.ToString();
    }
}
[System.Serializable]
public class GameUI
{
    [System.Serializable]

    public class ChaUI
    {
        public List<Image> Image;
        public List<GameObject> SkillBars,EffectBars;
        public GameObject EffectBar,BasicBar,SkillCheckBar;
        public Text SkillName, SkillInfo;
        public Button UseSkill;
    }
    public ChaUI Cha;
    public PlayerUI player;
    public TextBar textBar;
    public GameObject GamePanelSheet, ThumbtackMid,GamePanel;

    [System.Serializable]
    public class PlayerUI
    {
        public ValuePack Hp;
        public List<Image>RotateIcon;
        public GameObject HpPivot,SkillviewPanel,SKillviewBar;
        public Text HpText;
        public ChaControl Head,Use;
        public Image IsHead;

    }
    [System.Serializable]
    public class TextBar
    {
        public Text text;
        public GameObject Bar;
        public Animation animation;
    }
}

public class Block
{
    public Image Icon;
    public string Name;
}

public static class Statistics
{

        public static List<int> BreaKElement= new List<int>() { 0,0,0,0,0,0,0,0,0,0,0};
        public static List<int> PanelElement= new List<int>() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        public static int EnemyTakenDamage, PlayerTakenDamage,TotalTurn,PhaseTurn; 
        public static void Clear()
        {
            for (int i = 0; i< BreaKElement.Count; i++)
            {
                BreaKElement[i] = 0;
            }
        EnemyTakenDamage = 0;
        PlayerTakenDamage = 0;
        }
    

    public static Vector2Int Recover, Hit, Damage;
}