using System;//Array 쓰기위해 설정
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;//using System 썼더니 Random에서 모호한 설정이란 오류뜨길래
                                  //Random역시 유니티 엔진 Random 설정
public class CardManager : MonoBehaviour
{
    // Start is called before the first frame update
    public static CardManager Inst { get; private set; }
    void Awake() => Inst = this;

    [SerializeField] ItemSO itemSO;             //ItemSO 변수
    [SerializeField] GameObject cardPrefab;     //카드프리팹 받을 변수
    public List<Card> myCards;                  //나의 패 리스트
    [SerializeField] Transform cardSpawnPoint;  //프리팹 생성 위치
    [SerializeField] Transform myCardLeft;      //카드 정렬 시 필요한 왼쪽 위치
    [SerializeField] Transform myCardRight;     //오른쪽 위치
    [SerializeField] ECardState eCardState;     //현재 내 카드 상태


    List<Item> itemBuffer;                      //아이템 버퍼
    public List<Item> myDeck;                   //내 덱 아이템 리스트
    public List<Card> discardDeck;              //버리는 덱 리스트
    public List<Card> drawcardDeck;             //드로우 리스트
    Card selectedCard;                          //선택된 카드
    bool isMyCardDrag;                          //드래그 상태
    bool onMyCardArea;                          //패에 위치한 지 판단
    bool isOver;                                //넘어갔는 지 판단
    bool canDraw;                               //드로우 가능한지 판단**보스패턴으로 추가하려했으나 실패
    int errorType;                              //오류 타입
    int myPutCount;                             //엔티티에 카드 놓을 갯수
    enum ECardState { Nothing, CanMouseOver,CanMouseDrag}       //카드 상태:아무것도 안함, 카드에 마우스 올림, 카드가 드래그함
    public Item PopItem()
    {
        if(itemBuffer.Count==0)
        {
            SetupItemBuffer();
        }
        Item item = itemBuffer[0];
        itemBuffer.RemoveAt(0);
        return item;
    }
    //아이템 리스트에서 아이템 하나 가져오기
    public Item PopCard()
    {
        //내 아이템리스트가 0이면
        if(myDeck.Count==0)
        {
            //임시로 지정한 카드 10장을 저장
            SetUpMydeck();
        }
        //리스트에서 첫번째 아이템 가져오기
        Item card = myDeck[0];
        //아이템 리스트에서는 지우기
        myDeck.RemoveAt(0);
        return card;
    }
    public Card PopHand()
    {
        if(myCards.Count==0)
        {
            TurnManager.Inst.StartCoroutine(TurnManager.Inst.StartTurnCo());
        }
        Card hand = myCards[0];
        myCards.RemoveAt(0);
        return hand;
    }

    //내 패에서 버리는 덱으로 보내기
    public void SendToDiscard()
    {
        //내 패의 카드를 한장씩
        discardDeck.Add(myCards[0]);
        //버리는 덱으로 보내기
        MoveToDiscard();
        myCards.RemoveAt(0);
    }
    public void SendToMyDeck()
    {
        //내 버리는 덱에있는 카드를 전부 드로우 덱에 넣기
        drawcardDeck.Add(discardDeck[0]);
        discardDeck.RemoveAt(0);
    }
    public void MoveToDiscard()
    {
        //버리는 덱 위치로
        Vector3 to = GameObject.Find("DiscardDeck").transform.position;
        //내 패의 카드들을 다 보내기
        myCards[0].transform.Translate(to);
    }
    void SetUpMydeck()
    {
        myDeck = new List<Item>(100);
        //임의로 도적카드 5장과
        for(int i=0;i<5;i++)
        {
            myDeck.Add(itemSO.items[0]);
        }
        //전사카드 5장을 저장시키고
        for (int i = 0; i < 5; i++)
        {
            myDeck.Add(itemSO.items[1]);
        }

        //그냥 저장하면 똑같은 순서로 나오기 때문에
        for(int i=0;i<myDeck.Count;i++)
        {
            //랜덤하게 순서를 부여해서 덱에 다시 넣기
            int rand = Random.Range(i, myDeck.Count);
            Item temp = myDeck[i];
            myDeck[i] = myDeck[rand];
            myDeck[rand] = temp;
        }
    }
    void SetupItemBuffer()
    {
        itemBuffer = new List<Item>(100);
        for(int i=0;i<itemSO.items.Length;i++)
        {
            Item item = itemSO.items[i];
            for(int j=0;j<item.percent;j++)
            {
                itemBuffer.Add(item);
            }
        }
        for(int i=0;i<itemBuffer.Count;i++)
        {
            int rand = Random.Range(i, itemBuffer.Count);
            Item temp = itemBuffer[i];
            itemBuffer[i] = itemBuffer[rand];
            itemBuffer[rand] = temp;
        }
    }
    void Start()
    {
        //시작하면 덱 설정하고
        SetUpMydeck();
        //턴 시작하자마자 드로우하기
        TurnManager.OnAddCard += AddCard;
        TurnManager.OnTurnStarted += OnTurnStarted;
    }
    void OnDestroy()
    {
        SetUpMydeck();
        TurnManager.OnAddCard -= AddCard;
        TurnManager.OnTurnStarted -= OnTurnStarted;
    }
    void OnTurnStarted(bool myTurn)
    {
        if (myTurn)
            myPutCount = 0;

    }
    // Update is called once per frame
    void Update()
    {
        //내가 드로우 가능하다면 드로우
        if(isMyCardDrag)
            CardDrag();
        //카드 영역 감지
        DetectedCardArea();
        //카드 상태 지정
        SetECardState();
    }

    public void Alert(int errorType)
    {
        //errorType 1은 카드패가 다 찬 상태
        if(errorType==1)
        {
            GameManager.Inst.Notification("카드가 너무 많습니다");
        }
    }
    public void AddCard(bool canDraw)
    {
        //카드 프리팹에서 카드 생성하고
        var cardObject = Instantiate(cardPrefab, cardSpawnPoint.position, Utils.QI);
        //카드 컴포넌트 얻어오기
        var card = cardObject.GetComponent<Card>();
        int size = discardDeck.Count;
        //우선은 내 아이템 리스트에서 카드를 가져오기
        if (myDeck.Count != 0)
        {
            //드로우 가능하다면
            if (canDraw)
            {
                //카드 pop해서 정보 설정하기
                card.Setup(PopCard());
                //내 패로 만든 카드 가져오기
                myCards.Add(card);
                //order 최선위로 설정
                SetOriginOrder();
                //카드 정렬
                CardAlignment();
            }
            else
            {
                //드로우 불가능하면 오류 문구 호출
                Alert(1);
                return;
            }
        }
        //아이템 리스트에서 카드 다 만들었으면
        else
        {
            if(canDraw)
            {
                //드로우 카드에 카드없으면
                if(drawcardDeck.Count==0)
                {
                    for (int i = 0; i < size; i++)
                    {
                        //버리는 덱에서 카드 다 가져오기
                        SendToMyDeck();
                    }
                    //카드 드로우하기
                    myCards.Add(drawcardDeck[0]);
                    drawcardDeck.RemoveAt(0);
                    SetOriginOrder();
                    CardAlignment();
                }
                myCards.Add(drawcardDeck[0]);
                drawcardDeck.RemoveAt(0);
                SetOriginOrder();
                CardAlignment();
            }
            else
            {
                Alert(1);
                return;
            }
        }
    }
    void SetOriginOrder()
    {
        int count = myCards.Count;
        for(int i=0;i<count;i++)
        {
            //내 카드 우숸순위 정하기
            var targetCard = myCards[i];
            targetCard?.GetComponent<Order>().SetOriginOrder(i);
        }
    }
    void CardAlignment()
    {
        //pos,rot,scale 리스트 생성
        List<PRS> originCardPRS = new List<PRS>();
        //PRS는 RoundAlignment에 의해 새로 설정됨
        originCardPRS = RoundAlignment(myCardLeft, myCardRight, myCards.Count, 0.5f, Vector3.one * 1.9f);

        var targetCards = myCards;
        for(int i=0;i<targetCards.Count;i++)
        {
            var targetCard = targetCards[i];
            //설정된 PRS를 내 패의 카드에 각각 설정
            targetCard.originPRS = originCardPRS[i];
            //설정된 카드들은 Dotween을 통해 움직임을 더 효과적으로 표현
            targetCard.MoveTransform(targetCard.originPRS, true, 0.7f);
        }
        //부채형식으로 카드 펼치기
        List<PRS> RoundAlignment(Transform leftTr,Transform rightTr, int objcount, float height, Vector3 scale)
        {
            //카드 놓을 위치
            float[] objLerps = new float[objcount];
            //반환할 PRS
            List<PRS> results = new List<PRS>(objcount);

            switch(objcount)
            {
                //만약 카드 개수가 3개 이하면 지정한 위치로 설정
                case 1: objLerps = new float[] { 0.5f }; break;
                case 2: objLerps = new float[] { 0.27f, 0.73f }; break;
                case 3: objLerps = new float[] { 0.1f, 0.5f, 0.9f }; break;
                //그 이상이면 카드 위치를 0~1로 설정하고 그 사이에 카드 넣도록 설정
                default:
                    // 일정한 간격으로 멀어져있어야하므로 균등하게 나누고
                    float interval = 1f / (objcount - 1);
                    // 카드 순서별로 나눈 숫자를 곱해 위치 설정
                    for (int i = 0; i < objcount; i++)
                        objLerps[i] = interval * i;
                    break;
            }
            //카드 회전 설정
            for(int i=0;i<objcount;i++)
            {
                //카드 3장이면 회전 없이 그냥 설정
                var targetPos = Vector3.Lerp(leftTr.position, rightTr.position, objLerps[i]);
                var targetRot = Utils.QI;
                //4장이상이라면
                if(objcount>=4)
                {
                    //원의 방정식을 이용해 회전시킬 y 구하기+0.5를 빼는 이유는 우리가 패의 길이를 0~1로 설정했기 때문
                    float curve = Mathf.Sqrt(Mathf.Pow(height, 2) - Mathf.Pow(objLerps[i] - 0.5f, 2));
                    //curve = height >= 0 ? curve : -curve;
                    //현재 카드의 y 증가시키기
                    targetPos.y += curve;
                    //카드의 회전은 유니티 기능 중 quaternion.slerp를 통해 회전을 실행함
                    targetRot = Quaternion.Slerp(leftTr.rotation, rightTr.rotation, objLerps[i]);
                }
                //변경된 PRS를 반환
                results.Add(new PRS(targetPos, targetRot, scale));
            }
            return results;
        }
    }
    public bool TryPutCard(bool isMine)
    {
        if (isMine && myPutCount >= 1)
            return false;

        if (!isMine )
            return false;

        Card card = selectedCard;
        var spawnPos =  Utils.MousePos ;
        var targetCards = myCards ;

        if (EntityManager.Inst.SpawnEntity(isMine, card.item, spawnPos))
        {
            targetCards.Remove(card);
            DestroyImmediate(card.gameObject);
            if (isMine)
            {
                //myPutCount++;
            }
            return true;
        }
        else
        {
            targetCards.ForEach(x => x.GetComponent<Order>().SetMostFrontOrder(false));
            return false;
        }
    }

    //내 카드 영역
    #region MyCard
    //마우스가 카드에 올라오면
    public void CardMouseOver(Card card)
    {
        //상대턴에 카드를 만질 수 없게 설정
        if (eCardState==ECardState.Nothing)
            return;
        //선택할 카드로 설정
        selectedCard = card;
        //카드 확대하기
        EnlargeCard(true, card);
    }

    public void CardMouseExit(Card card)
    {
        //카드에서 나오면 카드 다시 축소
        EnlargeCard(false, card);
    }
    public void CardMouseDown()
    {
        //카드 드로우가능 상태 아니면 넘어가기
        if (eCardState != ECardState.CanMouseDrag)
            return;
        //드래그 하기
        isMyCardDrag = true;
    }
    //마우스를 떼면
    public void CardMouseUp()
    {

        isMyCardDrag = false;

        if (eCardState != ECardState.CanMouseDrag)
            return;
        if (onMyCardArea)
            EntityManager.Inst.RemoveMyEmptyEntity();
        else
            TryPutCard(true);

    }
    //드로우 가능한 지 확인
    public bool CheckCardCount()
    {
        //모든 상황에
        if (TurnManager.Inst.myTurn)
        {
            //카드가 5장이되면 드로우 불가
            if (myCards.Count >= 5)
                return false;
        }
        else
        {
            if (myCards.Count >= 5)
                return false;
        }
        return true;
    }
    //카드 드래그 하기
    void CardDrag()
    {
        if (eCardState != ECardState.CanMouseDrag)
            return;
        //내 패 영역이 아니면
        if (!onMyCardArea)
        {
            //카드 위치 변경하기
            selectedCard.MoveTransform(new PRS(Utils.MousePos, Utils.QI, selectedCard.originPRS.scale), false);
            EntityManager.Inst.InsertMyEmptyEntity(Utils.MousePos.x);
        }
    }
    //카드 영역 감지
    void DetectedCardArea()
    {
        //마우스 위치에 z방향으로 레이를 쏴서 적중한 정보 저장
        RaycastHit2D[] hits = Physics2D.RaycastAll(Utils.MousePos, Vector3.forward);
        //다른 오브젝트와 충돌나지 않게 내 카드영역 레이어를 받아서
        int layer = LayerMask.NameToLayer("MyCardArea");
        //레이캐스트 x가 충돌한 오브젝트 레이어가 mycardArea layer와 같은 게 존재하면 true 반환
        onMyCardArea = Array.Exists(hits, x => x.collider.gameObject.layer == layer);
    }
    //카드 확대하기
    void EnlargeCard(bool isEnlarge,Card card)
    {
        //카드가 확대 가능하다면
        if(isEnlarge)
        {   
            //확대되는 위치로 다시설정,x는 그대로 두고 y만 위치 증가
            //근데 z를 그대로 두니까 이 카드 옆에있는 카드가 선택되는 오류 발생
            //그래서 z역시 -10해서 올려서 오류 해결
            Vector3 enlargePos = new Vector3(card.originPRS.pos.x, -4.8f, -10f);
            //카드를 움직이는데 dotween은 안쓰고 변경된 위치와 크기를 증가시켜서 다시 적용
            card.MoveTransform(new PRS(enlargePos, Utils.QI, Vector3.one * 3.5f), false);
        }
        //마우가 오브젝트를 벗어나서 false로 변경되면
        else
        {
            //카드 다시 원래 크기로 변경
            card.MoveTransform(card.originPRS, false);
        }
        //선택되어서 확대 됐다는 것은 제일 앞에서 보여야하기 때문에
        //임의로 가장 우선순위로 설정시킴
        card.GetComponent<Order>().SetMostFrontOrder(isEnlarge);
    }
    //카드 상태 설정
    void SetECardState()
    {
        //대기중일 때 카드 만지면 고장나서
        if (TurnManager.Inst.isLoading)
            //그냥 아무것도 안되게 설정
            eCardState = ECardState.Nothing;
        //내 필드가 다 차거나 내 턴이 아니면
        else if (!TurnManager.Inst.myTurn || myPutCount == 1 || EntityManager.Inst.IsFullMyEntities)
            //카드 볼 수만 있게 하기
            eCardState = ECardState.CanMouseOver;
        //내 턴에 카드 낼 수 있으면 드래그하는데 낼 수 있는 조건을 원래는 코스트로 판단하려했는데
        //코스트 부분을 구현하지 못해서 그냥 패에 있는 거 다 내도록 만들었습니다.
        else if (TurnManager.Inst.myTurn && myPutCount == 0)
            //그래서 그냥 필드가 full이 아니면 드래그는 항상 가능합니다.
            eCardState = ECardState.CanMouseDrag;
    }

    #endregion
}
