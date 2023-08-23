using System.Collections;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using static UnityEditor.Progress;
using System.ComponentModel;
using System.Threading;
using System.Linq;

public class DeckManager : MonoBehaviour
{
    public static DeckManager Inst { get; private set; }
    private void Awake() => Inst = this;
    [SerializeField] List<DigimonCrd> digimonCrds;
    [SerializeField] GameObject cardPrefab;// 카드보여주기 객체
    [SerializeField] GameObject TamaPrefab;// 카드보여주기 객체
    [SerializeField] List<DigimonCrd> myCards;
    [SerializeField] List<DigimonCrd> otherCards;
    [SerializeField] List<DigimonCrd> myTama;//디지타마 추가 
    [SerializeField] Transform cardSpawnPoint;
    [SerializeField] Transform cardSpawnPointEnemy;
    [SerializeField] Transform TamaSpawnPoint;
    List<CardData> DigiTamaDeck;//디지타마 목록
    List<CardData> cardDataList;
    List<CardData> DigiCardDeck;// 디지몬 초기덱 
    List<CardData> DigiCardBkUpDeck;
    List<CardData> DigiCardDeckEnemy;
    [SerializeField] Transform myCardLeft;
    [SerializeField] Transform myCardRight;
    [SerializeField] Transform otherCardLeft;
    [SerializeField] Transform otherCardRight;
    public string jsonFilePath = "C:\\Users\\lg\\Documents\\DigimonCard\\data.json";
    string jsonString = "";
    int iDigiDeckCount;//덱매수
    int iDigiTamaCount;//디지타마 덱매수 
    WaitForSeconds delay05 = new WaitForSeconds(0.5f);
 
    public void Start()
    {
           
        cardDataList= ParseJsonToCardList(jsonString);
        DigiCardDeck = MakingDigiCadDeck(cardDataList);
        DigiCardBkUpDeck = DigiCardDeck.ToList();
        Debug.Log("백업: " + DigiCardBkUpDeck[1].Card_Num);
        DigiCardDeckEnemy = MakingDigiCadDeck(cardDataList);
        DigiTamaDeck = MakingDigiTamaDeck(cardDataList);
        iDigiDeckCount = DigiCardDeck.Count;
        iDigiTamaCount=DigiTamaDeck.Count;
        TurnManager.OnAddCard += AddCard;
       
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
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            for (int i = 0; i < TurnManager.Inst.startCardCount; i++)
            {
                
                
               
               
                Destroy(myCards[0].gameObject);
                myCards.Remove(myCards[0]);
            }
           
            DigiCardDeck.Clear();
            DigiCardDeck = DigiCardBkUpDeck.ToList();
           
            for (int i = 0; i < TurnManager.Inst.startCardCount; i++)
            {
               
                AddCard(true, cardSpawnPoint);
                
            }

        }
    }
    
    void AddCard(bool isMine,Transform cardSpawnPoint)
    {
        
        var cardObject = Instantiate(cardPrefab, cardSpawnPoint.position, Utils.QI);
        var card = cardObject.GetComponent<DigimonCrd>();
        card.Setup(DrawCard(isMine? DigiCardDeck:DigiCardDeckEnemy), isMine);
        (isMine ? myCards : otherCards).Add(card);

        SetOriginOrder(isMine);

        CardAlignMent(isMine);
    }
   
    void AddTama(bool isMine)
    {

        var cardObject = Instantiate(TamaPrefab, TamaSpawnPoint.position, Utils.QI);
        var card1 = cardObject.GetComponent<DigimonCrd>();
        card1.Setup(DrawTama(), isMine);
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
            int i_card_index_num = 0; // 카드정보   
            i_card_index_num = Random.Range(1, cardDataList.Count);
            if (cardDataList[i_card_index_num].Card_Num == null || cardDataList[i_card_index_num].card_able == 0
                || cardDataList[i_card_index_num].stage.Equals("유년기"))
            {
                continue;

            }
            cardDataList[i_card_index_num].card_able = cardDataList[i_card_index_num].card_able - 1;
            MakingDeck.Add(cardDataList[i_card_index_num]);
        }
        return MakingDeck;
    }

    List<CardData> MakingDigiTamaDeck(List<CardData> DigiTamaList)//디지타마 덱메이킹 
    {

        List<CardData> MakingDeck = new List<CardData>();
        for (int i = 0; i < 4; i++)
        {
           
            MakingDeck.Add(cardDataList[1]);
        }
        return MakingDeck;
    }
}
