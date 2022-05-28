using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetReward : MonoBehaviour
{
    public static GetReward Inst { get; private set; }
    [SerializeField] ItemSO itemSO;
    [SerializeField] GameObject cardPrefab;
    [SerializeField] Transform RewardSpawnPoint;

    List<Item> itemBuffer;
    public List<Card> rewardDeck;
    public bool isbattle;
    public Item PopItem()
    {
        if (itemBuffer.Count == 0)
        {
            SetupItemBuffer();
        }
        Item item = itemBuffer[0];
        itemBuffer.RemoveAt(0);
        return item;
    }
    public void SetupItemBuffer()
    {

        itemBuffer = new List<Item>(9);
        for (int i = 0; i < 9; i++)
        {
            int rand = Random.Range(0, itemSO.items.Length);
            itemBuffer.Add(itemSO.items[rand]);
        }
        for (int i = 0; i < itemBuffer.Count; i++)
        {
            int rand = Random.Range(i, itemBuffer.Count);
            Item temp = itemBuffer[i];
            itemBuffer[i] = itemBuffer[rand];
            itemBuffer[rand] = temp;
        }
    }
    public void SetReward()
    {
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
        Vector3 pos = RewardSpawnPoint.position;
        for (int i = 0; i < rewardDeck.Count; i++)
        {
            rewardDeck[i].transform.position = pos;
        }
    }
    void SetOriginOrder()
    {
        int count = rewardDeck.Count;
        for (int i = 0; i < count; i++)
        {
            var targetCard = rewardDeck[i];
            targetCard?.GetComponent<Order>().SetOriginOrder(i);
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
