using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetReward : MonoBehaviour
{
    public static GetReward Inst { get; private set; }

    [SerializeField] ItemSO itemSO;                 //ItemSO
    [SerializeField] GameObject cardPrefab;         //카드프리팹
    [SerializeField] Transform RewardSpawnPoint;    //보상카드 제시할 위치

    List<Item> itemBuffer;                          //아이템버퍼리스트
    public List<Card> rewardDeck;                   //보상덱
    public bool isbattle;                           //전투가능한지 판단
    public Item PopItem()
    {
        //아이템 리스트가 0이면
        if (itemBuffer.Count == 0)
        {
            //만들어 주기
            SetupItemBuffer();
            //원래는 리스트 바꾸는 걸 원해서 만든 조건문이었습니다
        }
        //아이템리스트에서 하나 뽑고
        Item item = itemBuffer[0];
        //리스트 지우기
        itemBuffer.RemoveAt(0);
        return item;
    }
    public void SetupItemBuffer()
    {
        //그 리스트를 설정하고
        itemBuffer = new List<Item>(9);
        for (int i = 0; i < 9; i++)
        {
            //아이템배열중에 하나 아무거나 가져와서
            int rand = Random.Range(0, itemSO.items.Length);
            //리스트에 추가하기
            itemBuffer.Add(itemSO.items[rand]);
        }
        for (int i = 0; i < itemBuffer.Count; i++)
        {
            //그 후 다시 랜덤하게 정렬
            int rand = Random.Range(i, itemBuffer.Count);
            Item temp = itemBuffer[i];
            itemBuffer[i] = itemBuffer[rand];
            itemBuffer[rand] = temp;
        }
    }
    public void SetReward()
    {
        //addCard와 비슷하게 카드 설정하기
        var cardObject = Instantiate(cardPrefab, RewardSpawnPoint.position, Utils.QI);
        var card = cardObject.GetComponent<Card>();
        for (int i = 0; i < 3; i++)
        {
            card.Setup(PopItem());
            rewardDeck.Add(card);
            SetOriginOrder();
            Alignment();

        }
    }
    public void Alignment()
    {
        //카드 위치를 임의로 지정한 위치로 설정하고
        Vector3 pos = RewardSpawnPoint.position;
        for (int i = 0; i < rewardDeck.Count; i++)
        {
            //그 카드를 해당 위치에 지정시키기
            rewardDeck[i].transform.position = pos;
        }
    }
    void SetOriginOrder()
    {
        //그 덱의 카드는
        int count = rewardDeck.Count;
        for (int i = 0; i < count; i++)
        {
            //order를 설정해서
            var targetCard = rewardDeck[i];
            //다른 오브젝트에 덮이지 않게 설정
            targetCard?.GetComponent<Order>().SetOriginOrder(i);
            //물론 덮일 일이 없긴하지만 만들 당시에는 여러개 추가할
            //생각이었어서 만들어 둔 함수입니다.
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        SetupItemBuffer();
        SetReward();
    }
    void OnDestroy()
    {
        SetupItemBuffer();
    }
    // Update is called once per frame
    void Update()
    {

    }
}
