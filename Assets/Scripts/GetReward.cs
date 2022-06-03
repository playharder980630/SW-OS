using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetReward : MonoBehaviour
{
    public static GetReward Inst { get; private set; }

    [SerializeField] ItemSO itemSO;                 //ItemSO
    [SerializeField] GameObject cardPrefab;         //ī��������
    [SerializeField] Transform RewardSpawnPoint;    //����ī�� ������ ��ġ

    List<Item> itemBuffer;                          //�����۹��۸���Ʈ
    public List<Card> rewardDeck;                   //����
    public bool isbattle;                           //������������ �Ǵ�
    public Item PopItem()
    {
        //������ ����Ʈ�� 0�̸�
        if (itemBuffer.Count == 0)
        {
            //����� �ֱ�
            SetupItemBuffer();
            //������ ����Ʈ �ٲٴ� �� ���ؼ� ���� ���ǹ��̾����ϴ�
        }
        //�����۸���Ʈ���� �ϳ� �̰�
        Item item = itemBuffer[0];
        //����Ʈ �����
        itemBuffer.RemoveAt(0);
        return item;
    }
    public void SetupItemBuffer()
    {
        //�� ����Ʈ�� �����ϰ�
        itemBuffer = new List<Item>(9);
        for (int i = 0; i < 9; i++)
        {
            //�����۹迭�߿� �ϳ� �ƹ��ų� �����ͼ�
            int rand = Random.Range(0, itemSO.items.Length);
            //����Ʈ�� �߰��ϱ�
            itemBuffer.Add(itemSO.items[rand]);
        }
        for (int i = 0; i < itemBuffer.Count; i++)
        {
            //�� �� �ٽ� �����ϰ� ����
            int rand = Random.Range(i, itemBuffer.Count);
            Item temp = itemBuffer[i];
            itemBuffer[i] = itemBuffer[rand];
            itemBuffer[rand] = temp;
        }
    }
    public void SetReward()
    {
        //addCard�� ����ϰ� ī�� �����ϱ�
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
        //ī�� ��ġ�� ���Ƿ� ������ ��ġ�� �����ϰ�
        Vector3 pos = RewardSpawnPoint.position;
        for (int i = 0; i < rewardDeck.Count; i++)
        {
            //�� ī�带 �ش� ��ġ�� ������Ű��
            rewardDeck[i].transform.position = pos;
        }
    }
    void SetOriginOrder()
    {
        //�� ���� ī���
        int count = rewardDeck.Count;
        for (int i = 0; i < count; i++)
        {
            //order�� �����ؼ�
            var targetCard = rewardDeck[i];
            //�ٸ� ������Ʈ�� ������ �ʰ� ����
            targetCard?.GetComponent<Order>().SetOriginOrder(i);
            //���� ���� ���� ���������� ���� ��ÿ��� ������ �߰���
            //�����̾�� ����� �� �Լ��Դϴ�.
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
