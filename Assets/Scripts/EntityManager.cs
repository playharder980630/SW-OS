using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public class EntityManager : MonoBehaviour
{
    public static EntityManager Inst { get; private set; }
    void Awake() => Inst = this;

    [SerializeField] GameObject entityPrefab;
    [SerializeField] List<Entity> myEntities; //내 카드배열
    [SerializeField] List<Entity> otherEntities; //상대 필드 몬스터 배열
    [SerializeField] GameObject TargetPicker;
    [SerializeField] GameObject damagePrefab;
    [SerializeField] Entity myEmptyEntity;
    [SerializeField] Entity myBossEntity;
    [SerializeField] Entity otherBossEntity;
    [SerializeField] Sprite[] sprites;
    const int MAX_ENTITY_COUNT = 6; // 최대 스폰카드 수
    public bool IsFullMyEntities => myEntities.Count >= MAX_ENTITY_COUNT && !ExistMyEmptyEntity;
    public int thief = 0;
    public int warrior = 0;
    bool IsFullOtherEntities => otherEntities.Count >= MAX_ENTITY_COUNT;
    bool ExistTargetPickEntity => targetPickEntity != null;
    bool ExistMyEmptyEntity => myEntities.Exists(x => x == myEmptyEntity);
    int MyEmptyEntityIndex => myEntities.FindIndex(x => x == myEmptyEntity);
    int BossAttackorDefence=0;
    int Class;

    bool CanMouseInput => TurnManager.Inst.myTurn && !TurnManager.Inst.isLoading; // 내 턴일때만 작동 
    Entity selectEntity;
    Entity targetPickEntity;
    WaitForSeconds delay1 = new WaitForSeconds(1);
    WaitForSeconds delay2 = new WaitForSeconds(2);
    void Start()
    {
        TurnManager.OnTurnStarted += OnTurnStarted;

        var targetBossEntity1 = otherBossEntity;
        targetBossEntity1.BossUpdate(0,Change.StageNumberChange );
    }

    void OnDestroy()
    {
        TurnManager.OnTurnStarted -= OnTurnStarted;
    }

    void OnTurnStarted(bool myTurn)
    {
        AttackableReset(myTurn);

        if (!myTurn)
            StartCoroutine(AICo());
    }
    void EntityAlignment(bool isMine)
    {
        float targetY = isMine ? -4.35f : 4.15f;                // isMine에 따라 보스위치 내 위치에 카드 소환
        var targetEntities = isMine ? myEntities : otherEntities; // isMine에 따라 targetEntities에 myentities or otherentities 할당

        for (int i = 0; i < targetEntities.Count; i++)
        {
        //Entites의 카운트에 따라 Entity들의 위치 조정
            float targetX = (targetEntities.Count - 1) * -3.4f + i * 6.8f; //6.8 간격에 따라 중앙부터 정렬

            var targetEntity = targetEntities[i];
            targetEntity.originPos = new Vector3(targetX, targetY, 0);
            targetEntity.MoveTransform(targetEntity.originPos, true, 0.5f); //부드럽게 이동하기 위해
            targetEntity.GetComponent<Order>()?.SetOriginOrder(i);
        }
    }
    public void InsertMyEmptyEntity(float xPos)
    {
        // mouse를 드래그 중일 때 계속 호출하며 entity들의 순서를 정렬해주는 함수
        // Card드래그 함수와 호환해 myEmptyEntity를 채운상태에서 작동한다
        if (IsFullMyEntities) // 엔티티가 풀로 차 있으면 정렬하지 않는다
            return;
        // 내 엔티티가 없으면 내 엔티티의 엠티엔티티를 추가
        if (!ExistMyEmptyEntity) 
            myEntities.Add(myEmptyEntity);
        
        Vector3 emptyEntityPos = myEmptyEntity.transform.position;
        emptyEntityPos.x = xPos;
        myEmptyEntity.transform.position = emptyEntityPos;
        // 엔티티 x좌표를 마우스xPos로 실시간으로 따온다.
        int _emptyEntityIndex = MyEmptyEntityIndex;
        myEntities.Sort((entity1, entity2) => entity1.transform.position.x.CompareTo(entity2.transform.position.x)); 
        if (MyEmptyEntityIndex != _emptyEntityIndex)
            EntityAlignment(true);
        // 엔티티 x좌표에 따라 순서를 정해주고 정렬함수 호출. 
    }
    public void RemoveMyEmptyEntity()
    {
        // 카드드래그가 내 카드로 왔을 때 MyEmptyEntity를 지워주는 함수
        if (!ExistMyEmptyEntity)
            return;

        myEntities.RemoveAt(MyEmptyEntityIndex);
        EntityAlignment(true);
    }
    public bool SpawnEntity(bool isMine, Item item, Vector3 spawnPos)
    {
        //카드를 실제로 필드에 추가하는 함수.
        if (isMine)
        {
        //나의 엔티티가 풀이면 리턴false.
            if (IsFullMyEntities || !ExistMyEmptyEntity)
                return false;
        }
        else
        {
            if (IsFullOtherEntities)
                return false;
        }

        var entityObject = Instantiate(entityPrefab, spawnPos, Utils.QI);
        var entity = entityObject.GetComponent<Entity>(); //빈 엔티티오브젝트에 엔티티의 정보를 업데이트하고 엔티티에 넣어주기

        if (isMine)
        { 
            myEntities[MyEmptyEntityIndex] = entity; // 나의 엔티티리스트에 엔티티 추가
        }
        
        entity.isMine = isMine;
        entity.Setup(item);
        AddClass(entity.Class);
        EntityAlignment(isMine);
        //엔티티의 정보를 업데이트, 정렬
        return true;
    }
    public void EntityMouseDown(Entity entity)
    {   
    //엔티티 누를시 엔티티를 선택해 selectEntity에 넣어주기 -> Entity의 마우스다운에서 누른entity를 받아와 selectEntity에넣기

        if (!CanMouseInput)
            return;
    
        selectEntity = entity;
    }

    public void EntityMouseUp()
    {
    
        if (!CanMouseInput)
            return;
        if (selectEntity && targetPickEntity && selectEntity.attackable) //마우스를 놨을때 타겟,섹렉트,어택어블이 모두 정상이면 어택
            Attack(selectEntity, targetPickEntity);

        selectEntity = null;
        targetPickEntity = null; // 타겟픽과 셀렉트엔티티 
    }

    public void EntityMouseDrag()
    {

        if (!CanMouseInput|| selectEntity == null)
            return;
        bool existTarget = false;
        foreach (var hit in Physics2D.RaycastAll(Utils.MousePos, Vector3.forward))
        {
            Entity entity = hit.collider?.GetComponent<Entity>(); // 마우스 위치에 콜라이더로 entity가 존재하면 entity정보 가져오기
            if (entity != null  && !entity.isMine && selectEntity.attackable)
            {
                targetPickEntity = entity; //타켓픽엔티티에 엔티티넣어주기
                existTarget = true;
                break;
            }
        }
        if (!existTarget)
            targetPickEntity = null;
    }
    void Attack(Entity attacker, Entity defender)
    {
        //공격자와 수비자를 넣어서 damage함수 호출후 tmp와 health를 업데이트하고 attack 모션을 만드는 함수.
        attacker.attackable = false; // 한번 공격시 또 공격 못하게
        attacker.GetComponent<Order>().SetMostFrontOrder(true); //공격자의 orderinlayer를 가장 높게 만들어주기

        Sequence sequence = DOTween.Sequence()
            .Append(attacker.transform.DOMove(defender.originPos, 0.4f)).SetEase(Ease.InSine) //공격하러 가고
            .AppendCallback(() =>
            {
                //공격자와 수비자 모두 damge 처리 해주고 damage뜨게
                attacker.Damaged(defender.attack);
                defender.Damaged(attacker.attack);
                SpawnDamage(defender.attack, attacker.transform);
                SpawnDamage(attacker.attack, defender.transform);
            })
            .Append(attacker.transform.DOMove(attacker.originPos, 0.4f)).SetEase(Ease.OutSine) //다시 되돌아오고
            .OnComplete(() => AttackCallback(attacker, defender));
    }

    void AttackCallback(params Entity[] entities)
    {
        // 죽을 사람 골라서 죽음 처리
        entities[0].GetComponent<Order>().SetMostFrontOrder(false); //attacker의 orderinlayer를 정상화

        foreach (var entity in entities)
        {
            if (!entity.isDie || entity.isBossOrEmpty)
                continue;
            //엔티티의 isDie를 체크 죽으면 넘어가고 아닐시 엔티티s배열에서 제거해줘야한다.
            if (entity.isMine)
            {
                myEntities.Remove(entity);
                SubClass(entity.Class);
            }
            else
                otherEntities.Remove(entity);

            Sequence sequence = DOTween.Sequence()
                .Append(entity.transform.DOShakePosition(1.3f)) //죽는 모션을 표현 흔들기
                .Append(entity.transform.DOScale(Vector3.zero, 0.3f)).SetEase(Ease.OutCirc) //죽는 모션을 표현
                .OnComplete(() =>
                {
                    EntityAlignment(entity.isMine); // 죽은 엔티티를 빼고 정렬
                    Destroy(entity.gameObject); //게임 오브젝트 파괴
                });
        }
        StartCoroutine(CheckBossDie());
    }

    IEnumerator AICo()
    {
        CardManager.Inst.TryPutCard(false); //카드를 낼 수 없다
        yield return delay1;
        var attackers = otherBossEntity;  // 보스의 엔티티들을 attacker로
       
        var defenders = new List<Entity>(myEntities); // 내 엔티티들을 디펜더 배열로
        defenders.Add(myBossEntity); // 내 보스도 추가
        int rand = UnityEngine.Random.Range(0, defenders.Count);
        if (BossAttackorDefence == 0 && Change.StageNumberChange != 7&&Change.StageNumberChange !=5)
        {
            Attack(attackers, defenders[rand]);  // 난수를 발생시켜 랜덤으로 공격하게 만든다
            BossAttackorDefence = 1;  // 보스가 한번 공격하면 한번 체력회복하며 쉬게끔 단 stage5는 쉬지않는다
        }
        else if (Change.StageNumberChange == 5)
        {
             // stage5보스의 개별패턴
            for (int i = 0; i < 3; i++)
            {
                rand = UnityEngine.Random.Range(0, defenders.Count);
                yield return delay1;
                Attack(attackers, defenders[rand]);
            }

        }
        else
        {
            // 보스가 한번 공격후 쉬는 패턴
            otherBossEntity.health = otherBossEntity.health + otherBossEntity.attack;
            BossAttackorDefence = 0;
        }
        var targetBossEntity1 = otherBossEntity;
        targetBossEntity1.Damaged(0); // 보스의 체력회복시 TMP업데이트
        if (TurnManager.Inst.isLoading)
                yield break;

            yield return delay2;
        
        TurnManager.Inst.EndTurn();
    }
    void Update()
    {
        ShowTargetPicker(ExistTargetPickEntity); // 공격표시
    }


    void ShowTargetPicker(bool isShow)
    {
        TargetPicker.SetActive(isShow);
        if (ExistTargetPickEntity)
            TargetPicker.transform.position = targetPickEntity.transform.position;
    }
    public void AttackableReset(bool isMine)
    {
        //나 또는 상대의 모든 엔티티들의 attackable을 초기화
        var targetEntites = isMine ? myEntities : otherEntities;
        targetEntites.ForEach(x => x.attackable = true);
    }
    void SpawnDamage(int damage, Transform tr)
    {
        //damage 오브젝트를 스폰하는 함수
        if (damage <= 0)
            return;

        var damageComponent = Instantiate(damagePrefab).GetComponent<Damage>();
        damageComponent.SetupTransform(tr);
        damageComponent.Damaged(damage);
    }
    IEnumerator CheckBossDie()
    {
        // 나와 상대 보스의 죽음을 체크 isDie로 게임오버 or 승리 여부를 판단.
        yield return delay2;

        if (myBossEntity.isDie)
            StartCoroutine(GameManager.Inst.GameOver(false));

        if (otherBossEntity.isDie)
        {
            StartCoroutine(GameManager.Inst.GameOver(true));
        }
    }
    public void DamageBoss(bool isMine, int damage)
    {
    //gamemanager 치트키 함수
        var targetBossEntity = isMine ? myBossEntity : otherBossEntity;
        targetBossEntity.Damaged(damage);
        StartCoroutine(CheckBossDie());
    }
    public void CheckClass(int Class)
    {
        
    }
    public void AddClass(int Class)
    {
        switch (Class)
        {
            case 1:
                thief++;
                if (thief >= 3&&thief<4)
                {
                    for (int i = 0; i < myEntities.Count; i++)
                        if (myEntities[i].Class == 1)
                        {
                            myEntities[i].attack += 2;
                        }
                }
                break;
            case 2:
                warrior++;
                if (warrior >= 2&& warrior<3)
                {
                    for (int i = 0; i < myEntities.Count; i++)
                    {
                        myEntities[i].health += 2;
                    }
                }
                break;
        }
    }
    public void SubClass(int Class)
    {
        switch (Class)
        {
            case 1:
                thief--;
                if (thief <= 2&&thief>1)
                {
                    for (int i = 0; i < myEntities.Count; i++)
                        if (myEntities[i].Class == 1)
                        {
                            myEntities[i].attack -= 2;
                        }
                }
                break;
            case 2:
                warrior--;
                if (warrior < 2&&warrior>=1)
                {
                    for (int i = 0; i < myEntities.Count; i++)
                    {
                        myEntities[i].health -= 2;
                    }
                }
                break;
        }
    }
}
