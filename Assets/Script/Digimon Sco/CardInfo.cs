using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;


    [System.Serializable]   
    public class DigiCard
    {
        public string card_num;// ī���ȣ 
        public string card_name;//ī�� �̹��� 
        public string img_url;//���̹��� �ּ� 
        public string stage;// ������ ���� (�����, �����)
        public string Type;// ������ ��(ex. ���,���̷���,������)
        public string Shape;//������ ����( ex) �������,������,������
        public int Play_Cost;// ���� �ڽ�Ʈ 
        public int Evol_Cost1;// �� 1�� ��ȭ �ڽ�Ʈ 
        public int Evol_Cost2;//  �� 2�� ��ȭ �ڽ�Ʈ 
        public string Effect;// ������ ȿ�� 
        public string Soure_effect;// ������ ��ȭ�� ȿ�� 
        public string Security;//��ť��Ƽ ȿ�� 
        public string Buying_Source;// ������ó
      
    }

    [CreateAssetMenu(fileName ="CardInfo", menuName ="scriptable Object/CardInfo")]
    public class CardInfo :ScriptableObject
    {
         public DigiCard[] digiCards;

        
    }

