using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using static UnityEditor.Progress;
using TMPro;
using UnityEngine.Networking;
using DG.Tweening;
using System.Threading;

public class CardData
{
    public string Card_Num;
    public string img;
    public string stage;
    public string Type;
    public string Shape;
    public string DP;
    public string Play_Cost;
    public string Evol_Cost1;
    public string Evol_Cost2;
    public string Effect;
    public string Soure_effect;
    public string Security;
    public string Buying_Source;
    public int card_able = 4;
}
public class DigimonCrd : MonoBehaviour
{
    [SerializeField] SpriteRenderer card;
    [SerializeField] Sprite cardBack;
    [SerializeField] string Card_Num;
    [SerializeField] string img;
    [SerializeField] string stage;
    [SerializeField] string Type;
    [SerializeField] string Shape;
    [SerializeField] string DP;
    [SerializeField] string Play_Cost;
    [SerializeField] string Evol_Cost1;
    [SerializeField] string Evol_Cost2;
    [SerializeField] string Effect;
    [SerializeField] string Soure_effect;
    [SerializeField] string Security;
    [SerializeField] string Buying_Source;
    public PRS originPRS;


    bool isFront;
    void OnMouseOver()
    {
        if (isFront)
            DeckManager.Inst.CardMousOver(this);

    }

    void OnMouseExit()
    {
        if (isFront)
            DeckManager.Inst.CardMouseExit(this);

    }

    

    public CardData getDigCardData(DigimonCrd tmp)
    {
        CardData tmpData = new CardData();
        tmpData.Card_Num = tmp.Card_Num;
        tmpData.img=tmp.img;
        tmpData.Type=tmp.Type;
        tmpData.Shape=tmp.Shape;
        tmpData.DP= tmp.DP;
        tmpData.Play_Cost=tmp.Play_Cost;
        tmpData.Evol_Cost1=tmp.Evol_Cost1;
        tmpData.Evol_Cost2=tmp.Evol_Cost2;
        tmpData.Effect=tmp.Effect;
        tmpData.Soure_effect=tmp.Soure_effect;
        tmpData.Security=tmp.Security;
        tmpData.Buying_Source=tmp.Buying_Source;

        return tmpData;
    }

    public void Setup(CardData cData, bool isFront,bool isSec)
    {


        if (isFront ==true)
        {
            Card_Num = cData.Card_Num;
            Debug.Log(Card_Num);
            Debug.Log(cData.img);
            if (isSec == true)
                card.sprite = LoadImageFromUrl(cData.img, card);
            else
                card.sprite = cardBack;
            stage = cData.stage;
            Type = cData.Type;
            Shape = cData.Shape;
            DP = cData.DP;
            Play_Cost = cData.Play_Cost;
            Evol_Cost1 = cData.Evol_Cost1;
            Evol_Cost2 = cData.Evol_Cost2;
            Effect = cData.Effect;
            Soure_effect = cData.Soure_effect;
            Security = cData.Security;
            Buying_Source = cData.Buying_Source;
        }
        else
        {
         

            card.sprite = cardBack;
            Card_Num = cData.Card_Num;
            stage = cData.stage;
            Type = cData.Type;
            Shape = cData.Shape;
            DP = cData.DP;
            Play_Cost = cData.Play_Cost;
            Evol_Cost1 = cData.Evol_Cost1 ;
            Evol_Cost2 = cData.Evol_Cost2;
            Effect = cData.Effect;
            Soure_effect = cData.Soure_effect;
            Security = cData.Security;
            Buying_Source = cData.Buying_Source;    



        }


        // Start is called before the first frame update

    }



    public void MoveTransform(PRS prs, bool useDotWeen, float dotweenTime = 0)
    {
        if (useDotWeen)
        {
            transform.DOMove(prs.pos, dotweenTime);
            transform.DORotateQuaternion(prs.rot, dotweenTime);
            transform.DOScale(prs.scale, dotweenTime);
        }

        else
        {
            transform.position = prs.pos;
            transform.rotation = prs.rot;
            transform.localScale = prs.scale;



        }





    }

    public Sprite LoadImageFromUrl(string img,SpriteRenderer spriteRenderer)
    {
        
        StartCoroutine(LoadImageCoroutine(img, spriteRenderer));
        return spriteRenderer.sprite;
    }

    private IEnumerator LoadImageCoroutine(string imageUrl, SpriteRenderer spriteRenderer)
    {
        // UnityWebRequest를 사용하여 이미지를 다운로드
        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(imageUrl))
        {
            yield return www.SendWebRequest();


            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Image download failed: " + www.error);
                yield break;
            }

            // 다운로드된 텍스처를 Texture2D로 변환
            Texture2D texture = DownloadHandlerTexture.GetContent(www);
            Texture2D resizedTexture = ResizeTexture(texture, texture.width/5, texture.height/5);//길이 측정 부분

            // Texture2D를 Sprite로 변환
            Sprite sprite = Sprite.Create(resizedTexture, new Rect(0, 0, resizedTexture.width, resizedTexture.height), Vector2.one * 0.5f);
           
            // SpriteRenderer에 표시
            spriteRenderer.sprite = sprite;
           
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
