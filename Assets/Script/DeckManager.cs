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
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;

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
    [SerializeField] GameObject CardinfoDialog; // ī�弳��â
    [SerializeField] Image CardImageExplain; // ī�弳���̹���
    [SerializeField] TextMeshProUGUI CardExplainText; // ī�弳���̹���


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


        if (Input.GetKeyDown(KeyCode.T))
            AddTama(true);

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
        card.Setup(DrawCard(isMine? DigiCardDeck:DigiCardDeckEnemy), isMine,isSec,false);
        (isMine ? myCards : otherCards).Add(card);

        SetOriginOrder(isMine);

        CardAlignMent(isMine);
    }
    void AddSecurity(bool isMine, Transform cardSpawnPoint) // Security �߰��Լ� 
    {
        bool isSec = false;
        var cardObject = Instantiate(cardPrefab, cardSpawnPoint.position, Utils.QI);
        var card = cardObject.GetComponent<DigimonCrd>();
        card.Setup(DrawCard(isMine ? DigiCardDeck : DigiCardDeckEnemy), isMine, isSec,false);
        (isMine ? mySecu : othSecu).Add(card);

        SetOriginOrder(isMine);

        SecurityAlignMent(isMine);

    }


    void AddTama(bool isMine) // ����Ÿ�� �߰� �Լ� 
    {

        var cardObject = Instantiate(TamaPrefab, TamaSpawnPoint.position, Utils.QI);
        var card1 = cardObject.GetComponent<DigimonCrd>();
        card1.Setup(DrawTama(), isMine,true,true);
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
        print("CardMouseOVer");
        CardData tmpCardData;
        CardinfoDialog.SetActive(true);
        string strTmpExpl;// �����ؽ�Ʈ ���� 
        tmpCardData=card.getDigCardData(card);
        strTmpExpl = "[ī���] : " +tmpCardData.Card_Num+"\n"+
                     "[DP] : " + tmpCardData.DP.ToString() + "\n"
                   + "[���� �ڽ�Ʈ] : " + tmpCardData.Play_Cost + "\n"
                   + "[��ȭ �ڽ�Ʈ] : " + tmpCardData.Evol_Cost1 + "\n"
                   + "[ȿ��] : " + tmpCardData.Effect + "\n"
                   + "[��ȭ�� ȿ��] : " + tmpCardData.Soure_effect;
        CardExplainText.text = strTmpExpl;
        CardImageExplain.sprite = card.GetComponent<SpriteRenderer>().sprite; 
       //LoadImageFromUrl(tmpCardData.img,CardImageExplain);


    }
    public void CardMouseExit(DigimonCrd card)
    {
        print("CardMouseExit");
        CardinfoDialog.SetActive(false);

        if (CardImageExplain != null)
        {
            CardImageExplain.sprite = null; // �̹����� ����ϴ�.
        }
        //EnlargeCard(false, card);
    }
    #endregion

    void EnlargeCard(bool isEnlarge, DigimonCrd card)
    {
        if (isEnlarge)
        {
            Vector3 enlargePos = new Vector3(card.originPRS.pos.x, -1.8f, -10f);
            card.MoveTransform(new PRS(enlargePos, Utils.QI, Vector3.one * 3.5f), false);
        }
        else
            card.MoveTransform(card.originPRS, false);

        card.GetComponent<Order>().SetMostFrontOrder(isEnlarge);


    }
    /// <summary>
    /// �̹��� ���� 
    /// </summary>
    /// <param name="img"></param>
    /// <param name="spriteRenderer"></param>
    /// <returns></returns>
    public void LoadImageFromUrl(string img, Image exImage)
    {

        StartCoroutine(LoadImageCoroutine(img));
        
    }

    IEnumerator LoadImageCoroutine(string img)
    {
        using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(img))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Failed to download image: " + webRequest.error);
            }
            else
            {
                // �̹��� �ٿ�ε尡 �����ϸ� Texture2D�� ��ȯ�մϴ�.
                Texture2D texture = DownloadHandlerTexture.GetContent(webRequest);

                // Texture2D�� Sprite�� ��ȯ�մϴ�.
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                if (CardImageExplain != null)
                {
                    CardImageExplain.sprite = sprite;
                }

            }
        }
    }
    private Texture2D ResizeTexture(Texture2D originalTexture, int newWidth, int newHeight)
    {
        Texture2D resizedTexture = new Texture2D(newWidth, newHeight);
        for (int y = 0; y < newHeight; y++)
        {
            for (int x = 0; x < newWidth; x++)
            {
                Color newColor = originalTexture.GetPixelBilinear((float)x / newWidth, (float)y / newHeight);
                resizedTexture.SetPixel(x, y, newColor);
            }
        }
        resizedTexture.Apply();
        return resizedTexture;
    }
    private void ResizeSpriteSize(SpriteRenderer spriteRenderer, float newWidth, float newHeight)
    {
        spriteRenderer.transform.localScale = new Vector3(newWidth / spriteRenderer.sprite.bounds.size.x,
                                                         newHeight / spriteRenderer.sprite.bounds.size.y,
                                                         1.0f);
    }
}


