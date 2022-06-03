using System;//Array �������� ����
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;//using System ����� Random���� ��ȣ�� �����̶� �����߱淡
                                  //Random���� ����Ƽ ���� Random ����
public class CardManager : MonoBehaviour
{
    // Start is called before the first frame update
    public static CardManager Inst { get; private set; }
    void Awake() => Inst = this;

    [SerializeField] ItemSO itemSO;             //ItemSO ����
    [SerializeField] GameObject cardPrefab;     //ī�������� ���� ����
    public List<Card> myCards;                  //���� �� ����Ʈ
    [SerializeField] Transform cardSpawnPoint;  //������ ���� ��ġ
    [SerializeField] Transform myCardLeft;      //ī�� ���� �� �ʿ��� ���� ��ġ
    [SerializeField] Transform myCardRight;     //������ ��ġ
    [SerializeField] ECardState eCardState;     //���� �� ī�� ����


    List<Item> itemBuffer;                      //������ ����
    public List<Item> myDeck;                   //�� �� ������ ����Ʈ
    public List<Card> discardDeck;              //������ �� ����Ʈ
    public List<Card> drawcardDeck;             //��ο� ����Ʈ
    Card selectedCard;                          //���õ� ī��
    bool isMyCardDrag;                          //�巡�� ����
    bool onMyCardArea;                          //�п� ��ġ�� �� �Ǵ�
    bool isOver;                                //�Ѿ�� �� �Ǵ�
    bool canDraw;                               //��ο� �������� �Ǵ�**������������ �߰��Ϸ������� ����
    int errorType;                              //���� Ÿ��
    int myPutCount;                             //��ƼƼ�� ī�� ���� ����
    enum ECardState { Nothing, CanMouseOver,CanMouseDrag}       //ī�� ����:�ƹ��͵� ����, ī�忡 ���콺 �ø�, ī�尡 �巡����
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
    //������ ����Ʈ���� ������ �ϳ� ��������
    public Item PopCard()
    {
        //�� �����۸���Ʈ�� 0�̸�
        if(myDeck.Count==0)
        {
            //�ӽ÷� ������ ī�� 10���� ����
            SetUpMydeck();
        }
        //����Ʈ���� ù��° ������ ��������
        Item card = myDeck[0];
        //������ ����Ʈ������ �����
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

    //�� �п��� ������ ������ ������
    public void SendToDiscard()
    {
        //�� ���� ī�带 ���徿
        discardDeck.Add(myCards[0]);
        //������ ������ ������
        MoveToDiscard();
        myCards.RemoveAt(0);
    }
    public void SendToMyDeck()
    {
        //�� ������ �����ִ� ī�带 ���� ��ο� ���� �ֱ�
        drawcardDeck.Add(discardDeck[0]);
        discardDeck.RemoveAt(0);
    }
    public void MoveToDiscard()
    {
        //������ �� ��ġ��
        Vector3 to = GameObject.Find("DiscardDeck").transform.position;
        //�� ���� ī����� �� ������
        myCards[0].transform.Translate(to);
    }
    void SetUpMydeck()
    {
        myDeck = new List<Item>(100);
        //���Ƿ� ����ī�� 5���
        for(int i=0;i<5;i++)
        {
            myDeck.Add(itemSO.items[0]);
        }
        //����ī�� 5���� �����Ű��
        for (int i = 0; i < 5; i++)
        {
            myDeck.Add(itemSO.items[1]);
        }

        //�׳� �����ϸ� �Ȱ��� ������ ������ ������
        for(int i=0;i<myDeck.Count;i++)
        {
            //�����ϰ� ������ �ο��ؼ� ���� �ٽ� �ֱ�
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
        //�����ϸ� �� �����ϰ�
        SetUpMydeck();
        //�� �������ڸ��� ��ο��ϱ�
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
        //���� ��ο� �����ϴٸ� ��ο�
        if(isMyCardDrag)
            CardDrag();
        //ī�� ���� ����
        DetectedCardArea();
        //ī�� ���� ����
        SetECardState();
    }

    public void Alert(int errorType)
    {
        //errorType 1�� ī���а� �� �� ����
        if(errorType==1)
        {
            GameManager.Inst.Notification("ī�尡 �ʹ� �����ϴ�");
        }
    }
    public void AddCard(bool canDraw)
    {
        //ī�� �����տ��� ī�� �����ϰ�
        var cardObject = Instantiate(cardPrefab, cardSpawnPoint.position, Utils.QI);
        //ī�� ������Ʈ ������
        var card = cardObject.GetComponent<Card>();
        int size = discardDeck.Count;
        //�켱�� �� ������ ����Ʈ���� ī�带 ��������
        if (myDeck.Count != 0)
        {
            //��ο� �����ϴٸ�
            if (canDraw)
            {
                //ī�� pop�ؼ� ���� �����ϱ�
                card.Setup(PopCard());
                //�� �з� ���� ī�� ��������
                myCards.Add(card);
                //order �ּ����� ����
                SetOriginOrder();
                //ī�� ����
                CardAlignment();
            }
            else
            {
                //��ο� �Ұ����ϸ� ���� ���� ȣ��
                Alert(1);
                return;
            }
        }
        //������ ����Ʈ���� ī�� �� ���������
        else
        {
            if(canDraw)
            {
                //��ο� ī�忡 ī�������
                if(drawcardDeck.Count==0)
                {
                    for (int i = 0; i < size; i++)
                    {
                        //������ ������ ī�� �� ��������
                        SendToMyDeck();
                    }
                    //ī�� ��ο��ϱ�
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
            //�� ī�� ��M���� ���ϱ�
            var targetCard = myCards[i];
            targetCard?.GetComponent<Order>().SetOriginOrder(i);
        }
    }
    void CardAlignment()
    {
        //pos,rot,scale ����Ʈ ����
        List<PRS> originCardPRS = new List<PRS>();
        //PRS�� RoundAlignment�� ���� ���� ������
        originCardPRS = RoundAlignment(myCardLeft, myCardRight, myCards.Count, 0.5f, Vector3.one * 1.9f);

        var targetCards = myCards;
        for(int i=0;i<targetCards.Count;i++)
        {
            var targetCard = targetCards[i];
            //������ PRS�� �� ���� ī�忡 ���� ����
            targetCard.originPRS = originCardPRS[i];
            //������ ī����� Dotween�� ���� �������� �� ȿ�������� ǥ��
            targetCard.MoveTransform(targetCard.originPRS, true, 0.7f);
        }
        //��ä�������� ī�� ��ġ��
        List<PRS> RoundAlignment(Transform leftTr,Transform rightTr, int objcount, float height, Vector3 scale)
        {
            //ī�� ���� ��ġ
            float[] objLerps = new float[objcount];
            //��ȯ�� PRS
            List<PRS> results = new List<PRS>(objcount);

            switch(objcount)
            {
                //���� ī�� ������ 3�� ���ϸ� ������ ��ġ�� ����
                case 1: objLerps = new float[] { 0.5f }; break;
                case 2: objLerps = new float[] { 0.27f, 0.73f }; break;
                case 3: objLerps = new float[] { 0.1f, 0.5f, 0.9f }; break;
                //�� �̻��̸� ī�� ��ġ�� 0~1�� �����ϰ� �� ���̿� ī�� �ֵ��� ����
                default:
                    // ������ �������� �־����־���ϹǷ� �յ��ϰ� ������
                    float interval = 1f / (objcount - 1);
                    // ī�� �������� ���� ���ڸ� ���� ��ġ ����
                    for (int i = 0; i < objcount; i++)
                        objLerps[i] = interval * i;
                    break;
            }
            //ī�� ȸ�� ����
            for(int i=0;i<objcount;i++)
            {
                //ī�� 3���̸� ȸ�� ���� �׳� ����
                var targetPos = Vector3.Lerp(leftTr.position, rightTr.position, objLerps[i]);
                var targetRot = Utils.QI;
                //4���̻��̶��
                if(objcount>=4)
                {
                    //���� �������� �̿��� ȸ����ų y ���ϱ�+0.5�� ���� ������ �츮�� ���� ���̸� 0~1�� �����߱� ����
                    float curve = Mathf.Sqrt(Mathf.Pow(height, 2) - Mathf.Pow(objLerps[i] - 0.5f, 2));
                    //curve = height >= 0 ? curve : -curve;
                    //���� ī���� y ������Ű��
                    targetPos.y += curve;
                    //ī���� ȸ���� ����Ƽ ��� �� quaternion.slerp�� ���� ȸ���� ������
                    targetRot = Quaternion.Slerp(leftTr.rotation, rightTr.rotation, objLerps[i]);
                }
                //����� PRS�� ��ȯ
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

    //�� ī�� ����
    #region MyCard
    //���콺�� ī�忡 �ö����
    public void CardMouseOver(Card card)
    {
        //����Ͽ� ī�带 ���� �� ���� ����
        if (eCardState==ECardState.Nothing)
            return;
        //������ ī��� ����
        selectedCard = card;
        //ī�� Ȯ���ϱ�
        EnlargeCard(true, card);
    }

    public void CardMouseExit(Card card)
    {
        //ī�忡�� ������ ī�� �ٽ� ���
        EnlargeCard(false, card);
    }
    public void CardMouseDown()
    {
        //ī�� ��ο찡�� ���� �ƴϸ� �Ѿ��
        if (eCardState != ECardState.CanMouseDrag)
            return;
        //�巡�� �ϱ�
        isMyCardDrag = true;
    }
    //���콺�� ����
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
    //��ο� ������ �� Ȯ��
    public bool CheckCardCount()
    {
        //��� ��Ȳ��
        if (TurnManager.Inst.myTurn)
        {
            //ī�尡 5���̵Ǹ� ��ο� �Ұ�
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
    //ī�� �巡�� �ϱ�
    void CardDrag()
    {
        if (eCardState != ECardState.CanMouseDrag)
            return;
        //�� �� ������ �ƴϸ�
        if (!onMyCardArea)
        {
            //ī�� ��ġ �����ϱ�
            selectedCard.MoveTransform(new PRS(Utils.MousePos, Utils.QI, selectedCard.originPRS.scale), false);
            EntityManager.Inst.InsertMyEmptyEntity(Utils.MousePos.x);
        }
    }
    //ī�� ���� ����
    void DetectedCardArea()
    {
        //���콺 ��ġ�� z�������� ���̸� ���� ������ ���� ����
        RaycastHit2D[] hits = Physics2D.RaycastAll(Utils.MousePos, Vector3.forward);
        //�ٸ� ������Ʈ�� �浹���� �ʰ� �� ī�念�� ���̾ �޾Ƽ�
        int layer = LayerMask.NameToLayer("MyCardArea");
        //����ĳ��Ʈ x�� �浹�� ������Ʈ ���̾ mycardArea layer�� ���� �� �����ϸ� true ��ȯ
        onMyCardArea = Array.Exists(hits, x => x.collider.gameObject.layer == layer);
    }
    //ī�� Ȯ���ϱ�
    void EnlargeCard(bool isEnlarge,Card card)
    {
        //ī�尡 Ȯ�� �����ϴٸ�
        if(isEnlarge)
        {   
            //Ȯ��Ǵ� ��ġ�� �ٽü���,x�� �״�� �ΰ� y�� ��ġ ����
            //�ٵ� z�� �״�� �δϱ� �� ī�� �����ִ� ī�尡 ���õǴ� ���� �߻�
            //�׷��� z���� -10�ؼ� �÷��� ���� �ذ�
            Vector3 enlargePos = new Vector3(card.originPRS.pos.x, -4.8f, -10f);
            //ī�带 �����̴µ� dotween�� �Ⱦ��� ����� ��ġ�� ũ�⸦ �������Ѽ� �ٽ� ����
            card.MoveTransform(new PRS(enlargePos, Utils.QI, Vector3.one * 3.5f), false);
        }
        //���찡 ������Ʈ�� ����� false�� ����Ǹ�
        else
        {
            //ī�� �ٽ� ���� ũ��� ����
            card.MoveTransform(card.originPRS, false);
        }
        //���õǾ Ȯ�� �ƴٴ� ���� ���� �տ��� �������ϱ� ������
        //���Ƿ� ���� �켱������ ������Ŵ
        card.GetComponent<Order>().SetMostFrontOrder(isEnlarge);
    }
    //ī�� ���� ����
    void SetECardState()
    {
        //������� �� ī�� ������ ���峪��
        if (TurnManager.Inst.isLoading)
            //�׳� �ƹ��͵� �ȵǰ� ����
            eCardState = ECardState.Nothing;
        //�� �ʵ尡 �� ���ų� �� ���� �ƴϸ�
        else if (!TurnManager.Inst.myTurn || myPutCount == 1 || EntityManager.Inst.IsFullMyEntities)
            //ī�� �� ���� �ְ� �ϱ�
            eCardState = ECardState.CanMouseOver;
        //�� �Ͽ� ī�� �� �� ������ �巡���ϴµ� �� �� �ִ� ������ ������ �ڽ�Ʈ�� �Ǵ��Ϸ��ߴµ�
        //�ڽ�Ʈ �κ��� �������� ���ؼ� �׳� �п� �ִ� �� �� ������ ��������ϴ�.
        else if (TurnManager.Inst.myTurn && myPutCount == 0)
            //�׷��� �׳� �ʵ尡 full�� �ƴϸ� �巡�״� �׻� �����մϴ�.
            eCardState = ECardState.CanMouseDrag;
    }

    #endregion
}
