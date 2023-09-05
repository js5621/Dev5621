using System.Collections;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using static UnityEditor.Progress;
using System.ComponentModel;
using System.Threading;
using System.Linq;
using static System.Random;

public class DeckManager : MonoBehaviour
{
    public static DeckManager Inst { get; private set; }
    private void Awake() => Inst = this;
    [SerializeField] List<DigimonCrd> digimonCrds;
    [SerializeField] GameObject cardPrefab;// ī�庸���ֱ� ��ü
    [SerializeField] GameObject TamaPrefab;// ī�庸���ֱ� ��ü
    [SerializeField] List<DigimonCrd> myCards; // �� ���� ī��
    [SerializeField] List<DigimonCrd> otherCards;//��� �� ī��
    [SerializeField] List<DigimonCrd> mySecu; // �� ���� ī��
    [SerializeField] List<DigimonCrd> othSecu;//��� �� ī��
    [SerializeField] List<DigimonCrd> myTama;//����Ÿ�� �߰� 
    [SerializeField] Transform cardSpawnPoint;
    [SerializeField] Transform cardSpawnPointEnemy;
    [SerializeField] Transform TamaSpawnPoint;
    List<CardData> DigiTamaDeck;//����Ÿ�� ���
    List<CardData> cardDataList;
    List<CardData> DigiCardDeck;// ������ �ʱⵦ 
    List<CardData> DigiCardBkUpDeck;// �ָ��ǿ� �������Ʈ
    List<CardData> DigiCardEnemyBkUpDeck;// ��� ���� �ָ���  
    List<CardData> DigiCardDeckEnemy;
    [SerializeField] Transform myCardLeft; //������ �̴� �� ����
    [SerializeField] Transform myCardRight;// ������ �̴� �� ������
    [SerializeField] Transform otherCardLeft;// ��밡 �̴��� ����
    [SerializeField] Transform otherCardRight;//��밡 �̴��� ������
    [SerializeField] Transform mySecurityOver;//���� ��ť��Ƽ �� ����
    [SerializeField] Transform mySecurityUnder;//���� ��ť��Ƽ �� �Ʒ��� 
    [SerializeField] Transform EnemySecurityOver;//���� ��ť��Ƽ �� ����
    [SerializeField] Transform EnemySecurityUnder;//���� ��ť��Ƽ �� �Ʒ��� 

    public string jsonFilePath = "C:\\Users\\lg\\Documents\\DigimonCard\\data.json";
    string jsonString = "";
    int iDigiDeckCount;//���ż�
    int iDigiTamaCount;//����Ÿ�� ���ż� 
    WaitForSeconds delay05 = new WaitForSeconds(0.5f);
 
    public void Start()
    {
           
        cardDataList= ParseJsonToCardList(jsonString);
        DigiCardDeck = MakingDigiCadDeck(cardDataList);// �÷��� ���� �� ����
        Debug.Log("���� �� �ż� :" +DigiCardDeck.Count);
        DigiCardBkUpDeck = DigiCardDeck.ToList();
        DigiCardDeckEnemy = MakingDigiCadDeck(cardDataList);// ���� ai �� ���� 
        Debug.Log("��� �� �ż� :" + DigiCardDeckEnemy.Count);
        DigiCardEnemyBkUpDeck = DigiCardDeckEnemy.ToList();
        DigiTamaDeck = MakingDigiTamaDeck(cardDataList);
        iDigiDeckCount = DigiCardDeck.Count;
        iDigiTamaCount=DigiTamaDeck.Count;
        TurnManager.OnAddCard += AddCard;
        TurnManager.DoMulligan += DoMulligan;
        TurnManager.OnAddSecurity += AddSecurity;


    }
    private void OnDestroy()
    {
       TurnManager.OnAddCard -= AddCard;
    }

    void CardAlignMent(bool isMine)
    {

        List<PRS> originCardPRSs = new List<PRS>();
        if (isMine)
            originCardPRSs = RoundAlignment(myCardLeft, myCardRight, myCards.Count, 0.5f, Vector3.one * 1.9f);
        else
            originCardPRSs = RoundAlignment(otherCardLeft, otherCardRight, otherCards.Count, 0.5f, Vector3.one * 1.9f);

        var targetCards = isMine ? myCards : otherCards;
        for (int i = 0; i < targetCards.Count; i++)
        {
            var targetCard = targetCards[i];

            targetCard.originPRS = originCardPRSs[i];
            targetCard.MoveTransform(targetCard.originPRS, true, 0.5f);
        }
    }


    void SecurityAlignMent(bool isMine)
    {

        List<PRS> originCardPRSs = new List<PRS>();
        if (isMine)
            originCardPRSs = RoundAlignment(mySecurityOver, mySecurityUnder, myCards.Count, 0.5f, Vector3.one * 1.9f);
        else
            originCardPRSs = RoundAlignment(EnemySecurityOver, EnemySecurityUnder, otherCards.Count, 0.5f, Vector3.one * 1.9f);

        var targetCards = isMine ? mySecu : othSecu;
        for (int i = 0; i < targetCards.Count; i++)
        {
            var targetCard = targetCards[i];

            targetCard.originPRS = originCardPRSs[i];
            targetCard.MoveTransform(targetCard.originPRS, true, 0.5f);
        }
    }
    List<PRS> RoundAlignment(Transform leftTr, Transform rightTr, int objCount, float height, Vector3 scale)
    {
        float[] objLerps = new float[objCount];
        List<PRS> results = new List<PRS>(objCount);
        switch (objCount)
        {
            case 1: objLerps = new float[] { 0.5f }; break;
            case 2: objLerps = new float[] { 0.27f, 0.73f }; break;
            case 3: objLerps = new float[] { 0.1f, 0.5f, 0.9f }; break;
            default:
                float interval = 1f / (objCount - 1);
                for (int i = 0; i < objCount; i++)
                    objLerps[i] = interval * i;
                break;
        }

        for (int i = 0; i < objCount; i++)
        {
            var targetPos = Vector3.Lerp(leftTr.position, rightTr.position, objLerps[i]);
            var targerRot = Quaternion.identity;
            if (objCount >= 4)
            {

               // float curve = Mathf.Sqrt(Mathf.Pow(height, 2) - Mathf.Pow(objLerps[i] - 0.5f, 2));
                //curve = height >= 0 ? curve : -curve;
                //targetPos.y += curve;
                targerRot = Quaternion.Slerp(leftTr.rotation, rightTr.rotation, objLerps[i]);
            }

            results.Add(new PRS(targetPos, targerRot, scale));
        }

        return results;

    }
     void DoMulligan(bool bMullChk)// �ָ��� ���� �Լ�
    {
        if (bMullChk == true)
        {
            for (int i = 0; i < TurnManager.Inst.startCardCount; i++)
            {

                Destroy(myCards[0].gameObject);
                myCards.Remove(myCards[0]);
            }
            DigiCardDeck.Clear();
            DigiCardDeck = ShuffledDeck(DigiCardBkUpDeck.ToList());
        }
        else 
        {
            for (int i = 0; i < TurnManager.Inst.startCardCount; i++)
            {

                Destroy(otherCards[0].gameObject);
                otherCards.Remove(otherCards[0]);
            }

            DigiCardDeckEnemy.Clear();
            DigiCardEnemyBkUpDeck = ShuffledDeck(DigiCardEnemyBkUpDeck.ToList());

        }
    }
    public void Update()
    {
      
        if (Input.GetKeyDown(KeyCode.X)) //�ָ��� ��ư 
        {
            for (int i = 0; i < TurnManager.Inst.startCardCount; i++)
            {
                
                
               
               
                Destroy(otherCards[0].gameObject);
                otherCards.Remove(otherCards[0]);
            }
           
            DigiCardDeck.Clear();
            DigiCardDeck = ShuffledDeck(DigiCardBkUpDeck.ToList());
           
            for (int i = 0; i < TurnManager.Inst.startCardCount; i++)
            {
               
                AddCard(false, cardSpawnPointEnemy);
                
            }

        }
       
        if(Input.GetKeyDown(KeyCode.C))
            AddSecurity(true, cardSpawnPoint);

        if (Input.GetKeyDown(KeyCode.V))
            AddSecurity(false, cardSpawnPointEnemy);



    }
    
    static List<CardData> ShuffledDeck(List<CardData> shuffleDeck)
    {
        var newShuffledDeck = new List<CardData>();
        var listcCount = shuffleDeck.Count;

        for (int i = 0; i < listcCount; i++)
        {
            var randomElementInList = Random.Range(0, shuffleDeck.Count);
            newShuffledDeck.Add(shuffleDeck[randomElementInList]);
            shuffleDeck.Remove(shuffleDeck[randomElementInList]);
        }
        return newShuffledDeck;
    }
    
    void AddCard(bool isMine,Transform cardSpawnPoint) // �ʱ� ��ο� �� ī�� �߰� �Լ� 
    {
        bool isSec = true;
        var cardObject = Instantiate(cardPrefab, cardSpawnPoint.position, Utils.QI);
        var card = cardObject.GetComponent<DigimonCrd>();
        card.Setup(DrawCard(isMine? DigiCardDeck:DigiCardDeckEnemy), isMine,isSec);
        (isMine ? myCards : otherCards).Add(card);

        SetOriginOrder(isMine);

        CardAlignMent(isMine);
    }
    void AddSecurity(bool isMine, Transform cardSpawnPoint) // Security �߰��Լ� 
    {
        bool isSec = false;
        var cardObject = Instantiate(cardPrefab, cardSpawnPoint.position, Utils.QI);
        var card = cardObject.GetComponent<DigimonCrd>();
        card.Setup(DrawCard(isMine ? DigiCardDeck : DigiCardDeckEnemy), isMine, isSec);
        (isMine ? mySecu : othSecu).Add(card);

        SetOriginOrder(isMine);

        SecurityAlignMent(isMine);

    }


    void AddTama(bool isMine) // ����Ÿ�� �߰� �Լ� 
    {

        var cardObject = Instantiate(TamaPrefab, TamaSpawnPoint.position, Utils.QI);
        var card1 = cardObject.GetComponent<DigimonCrd>();
  //      card1.Setup(DrawTama(), isMine);
        myTama.Add(card1);
        SetOriginOrder(true);


    }


    void SetOriginOrder(bool isMine)
    {

        int count = isMine ? myCards.Count : otherCards.Count;
        for (int i = 0; i < count; i++)
        {
            var targetCard = isMine ? myCards[i] : otherCards[i];
            targetCard?.GetComponent<Order>().SetOriginOrder(i);

        }


    }

    public CardData DrawCard(List<CardData> DigiCardDeck)
    {
        if (DigiCardDeck.Count == 0)
            MakingDigiCadDeck(cardDataList);
        CardData digiDrawCard = DigiCardDeck[0];
        DigiCardDeck.RemoveAt(0);
        return digiDrawCard;


    }

    public CardData DrawTama()
    {
        if (DigiCardDeck.Count == 0)
            MakingDigiTamaDeck(cardDataList);
        CardData digiDrawCard = DigiTamaDeck[0];
        DigiCardDeck.RemoveAt(0);
        return digiDrawCard;


    }

    List<CardData> ParseJsonToCardList(string jsonString)
    {   
        jsonString = File.ReadAllText(jsonFilePath);
       
        List<CardData> result = new List<CardData>();
        result = JsonConvert.DeserializeObject<List<CardData>>(jsonString);
       
        return result;
    }

    List<CardData> MakingDigiCadDeck(List<CardData> cardDataList)
    {

        List<CardData> MakingDeck = new List<CardData>();
        for(int i = 0; i < 50;i++)
        {
            int i_card_index_num = 0; // ī������   
            i_card_index_num = Random.Range(1, cardDataList.Count);
            while (cardDataList[i_card_index_num].Card_Num == null || cardDataList[i_card_index_num].card_able == 0
                || cardDataList[i_card_index_num].stage.Equals("�����"))
            {
                i_card_index_num = Random.Range(1, cardDataList.Count);

            }
            cardDataList[i_card_index_num].card_able = cardDataList[i_card_index_num].card_able - 1;
            MakingDeck.Add(cardDataList[i_card_index_num]);
        }

        for (int i = 0; i < cardDataList.Count; i++)
        {
            cardDataList[i].card_able = 4;
        }
        return MakingDeck;
    }

    List<CardData> MakingDigiTamaDeck(List<CardData> DigiTamaList)//����Ÿ�� ������ŷ 
    {

        List<CardData> MakingDeck = new List<CardData>();
        for (int i = 0; i < 4; i++)
        {
           
            MakingDeck.Add(cardDataList[1]);
        }
        return MakingDeck;
    }

    #region MyCard
     public void CardMousOver(DigimonCrd card)
    {
       EnlargeCard(true, card);
    }
    public void CardMouseExit(DigimonCrd card)
    {
        EnlargeCard(false, card);
    }
    #endregion

    void EnlargeCard(bool isEnlarge, DigimonCrd card)
    {
        if (isEnlarge)
        {
            Vector3 enlargePos = new Vector3(card.originPRS.pos.x, -4.8f, -10f);
            card.MoveTransform(new PRS(enlargePos, Utils.QI, Vector3.one * 3.5f), false);
        }
        else
            card.MoveTransform(card.originPRS, false);

        card.GetComponent<Order>().SetMostFrontOrder(isEnlarge);


    }
}


