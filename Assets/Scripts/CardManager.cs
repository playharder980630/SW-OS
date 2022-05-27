using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
public class CardManager : MonoBehaviour
{
    // Start is called before the first frame update
    public static CardManager Inst { get; private set; }
    void Awake() => Inst = this;

    [SerializeField] ItemSO itemSO;
    [SerializeField] GameObject cardPrefab;
    public List<Card> myCards;
    [SerializeField] Transform cardSpawnPoint;
    [SerializeField] Transform myCardLeft;
    [SerializeField] Transform myCardRight;
    [SerializeField] ECardState eCardState;


    List<Item> itemBuffer;
    public List<Item> myDeck;
    public List<Card> discardDeck;
    public List<Card> drawcardDeck;
    Card selectedCard;
    bool isMyCardDrag;
    bool onMyCardArea;
    bool isOver;
    bool canDraw;
    int errorType;
    int myPutCount;
    enum ECardState { Nothing, CanMouseOver,CanMouseDrag}
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
    public Item PopCard()
    {
        if(myDeck.Count==0)
        {
            SetUpMydeck();
        }
        Item card = myDeck[0];
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
    public void SendToDiscard()
    {
        discardDeck.Add(myCards[0]);
        MoveToDiscard();
        myCards.RemoveAt(0);
    }
    public void SendToMyDeck()
    {
        drawcardDeck.Add(discardDeck[0]);
        discardDeck.RemoveAt(0);
    }
    public void MoveToDiscard()
    {
        Vector3 to = GameObject.Find("DiscardDeck").transform.position;
        myCards[0].transform.Translate(to);
    }
    void SetUpMydeck()
    {
        myDeck = new List<Item>(100);
        for(int i=0;i<5;i++)
        {
            myDeck.Add(itemSO.items[0]);
        }
        for (int i = 0; i < 5; i++)
        {
            myDeck.Add(itemSO.items[1]);
        }
        for(int i=0;i<myDeck.Count;i++)
        {
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
        SetUpMydeck();
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
        if(isMyCardDrag)
            CardDrag();

        DetectedCardArea();
        SetECardState();
    }

    public void Alert(int errorType)
    {
        if(errorType==1)
        {
            GameManager.Inst.Notification("카드가 너무 많습니다");
        }
    }
    public void AddCard(bool canDraw)
    {
        var cardObject = Instantiate(cardPrefab, cardSpawnPoint.position, Utils.QI);
        var card = cardObject.GetComponent<Card>();
        int size = discardDeck.Count;

        if (myDeck.Count != 0)
        {
            if (canDraw)
            {
                card.Setup(PopCard());
                myCards.Add(card);
                SetOriginOrder();
                CardAlignment();
            }
            else
            {
                Alert(1);
                return;
            }
        }
        else
        {
            if(canDraw)
            {
                if(drawcardDeck.Count==0)
                {
                    for (int i = 0; i < size; i++)
                    {
                        SendToMyDeck();
                    }
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
            var targetCard = myCards[i];
            targetCard?.GetComponent<Order>().SetOriginOrder(i);
        }
    }
    void CardAlignment()
    {
        List<PRS> originCardPRS = new List<PRS>();
        
        originCardPRS = RoundAlignment(myCardLeft, myCardRight, myCards.Count, 0.5f, Vector3.one * 1.9f);

        var targetCards = myCards;
        for(int i=0;i<targetCards.Count;i++)
        {
            var targetCard = targetCards[i];

            targetCard.originPRS = originCardPRS[i];
            targetCard.MoveTransform(targetCard.originPRS, true, 0.7f);
        }

        List<PRS> RoundAlignment(Transform leftTr,Transform rightTr, int objcount, float height, Vector3 scale)
        {
            float[] objLerps = new float[objcount];
            List<PRS> results = new List<PRS>(objcount);

            switch(objcount)
            {
                case 1: objLerps = new float[] { 0.5f }; break;
                case 2: objLerps = new float[] { 0.27f, 0.73f }; break;
                case 3: objLerps = new float[] { 0.1f, 0.5f, 0.9f }; break;
                default:
                    float interval = 1f / (objcount - 1);
                    for (int i = 0; i < objcount; i++)
                        objLerps[i] = interval * i;
                    break;
            }

            for(int i=0;i<objcount;i++)
            {
                var targetPos = Vector3.Lerp(leftTr.position, rightTr.position, objLerps[i]);
                var targetRot = Utils.QI;
                if(objcount>=4)
                {
                    float curve = Mathf.Sqrt(Mathf.Pow(height, 2) - Mathf.Pow(objLerps[i] - 0.5f, 2));
                    curve = height >= 0 ? curve : -curve;
                    targetPos.y += curve;
                    targetRot = Quaternion.Slerp(leftTr.rotation, rightTr.rotation, objLerps[i]);
                }
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
                myPutCount++;
            }
            return true;
        }
        else
        {
            targetCards.ForEach(x => x.GetComponent<Order>().SetMostFrontOrder(false));
            return false;
        }
    }


    #region MyCard

    public void CardMouseOver(Card card)
    {
        if (eCardState==ECardState.Nothing)
            return;
        selectedCard = card;
        EnlargeCard(true, card);
    }

    public void CardMouseExit(Card card)
    {
        EnlargeCard(false, card);
    }
    public void CardMouseDown()
    {
        if (eCardState != ECardState.CanMouseDrag)
            return;
        isMyCardDrag = true;
    }
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
    public bool CheckCardCount()
    {
        if (TurnManager.Inst.myTurn)
        {
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
    void CardDrag()
    {
        if (eCardState != ECardState.CanMouseDrag)
            return;

        if (!onMyCardArea)
        {
            selectedCard.MoveTransform(new PRS(Utils.MousePos, Utils.QI, selectedCard.originPRS.scale), false);
            EntityManager.Inst.InsertMyEmptyEntity(Utils.MousePos.x);
        }
    }
    void DetectedCardArea()
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(Utils.MousePos, Vector3.forward);
        int layer = LayerMask.NameToLayer("MyCardArea");
        onMyCardArea = Array.Exists(hits, x => x.collider.gameObject.layer == layer);
    }
    void EnlargeCard(bool isEnlarge,Card card)
    {
        if(isEnlarge)
        {   
            Vector3 enlargePos = new Vector3(card.originPRS.pos.x, -4.8f, -10f);
            card.MoveTransform(new PRS(enlargePos, Utils.QI, Vector3.one * 3.5f), false);
        }
        else
        {
            card.MoveTransform(card.originPRS, false);
        }
        card.GetComponent<Order>().SetMostFrontOrder(isEnlarge);
    }

    void SetECardState()
    {
        if (TurnManager.Inst.isLoading)
            eCardState = ECardState.Nothing;

        else if (!TurnManager.Inst.myTurn || myPutCount == 1 || EntityManager.Inst.IsFullMyEntities)
            eCardState = ECardState.CanMouseOver;

        else if (TurnManager.Inst.myTurn && myPutCount == 0)
            eCardState = ECardState.CanMouseDrag;
    }

    #endregion
}
