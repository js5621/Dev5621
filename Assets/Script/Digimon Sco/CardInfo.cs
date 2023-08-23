using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;


    [System.Serializable]   
    public class DigiCard
    {
        public string card_num;// 카드번호 
        public string card_name;//카드 이미지 
        public string img_url;//웹이미지 주소 
        public string stage;// 디지몬 레벨 (성장기, 유년기)
        public string Type;// 디지몬 종(ex. 백신,바이러스,데이터)
        public string Shape;//디지몬 형태( ex) 파충류형,공룡형,거조형
        public int Play_Cost;// 등장 코스트 
        public int Evol_Cost1;// 색 1의 진화 코스트 
        public int Evol_Cost2;//  색 2의 진화 코스트 
        public string Effect;// 디지몬 효과 
        public string Soure_effect;// 디지몬 진화원 효과 
        public string Security;//시큐리티 효과 
        public string Buying_Source;// 구매출처
      
    }

    [CreateAssetMenu(fileName ="CardInfo", menuName ="scriptable Object/CardInfo")]
    public class CardInfo :ScriptableObject
    {
         public DigiCard[] digiCards;

        
    }

