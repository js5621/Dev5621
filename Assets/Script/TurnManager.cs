using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
public class TurnManager : MonoBehaviour
{
    // Start is called before the first frame update
    public static TurnManager Inst { get; private set; }
    private void Awake() => Inst = this;
    [Header("Develop")]
    [SerializeField][Tooltip("시작 턴 모드를 정합니다")] ETurnMode eTurnMode;
    [SerializeField][Tooltip("카드 배분이 매우 빨라집니다.")] bool fastMode;
    [SerializeField][Tooltip("유저 멀리건 여부를 체크합니다.")] bool myMulliganChecker;
    [SerializeField][Tooltip("상대방 멀리건 여부를 체크합니다.")] bool enemyMulliganChecker;
    [SerializeField][Tooltip("시작 턴 카드 개수를 정합니다. ")]public int startCardCount;
    [Header("Properties")]

    public bool isLoading;// 게임 끝나면 isLoading을 true로 하면 카드와 엔티티 클릭방지
    public bool myTurn;
    enum ETurnMode { Random, My, Other }
    WaitForSeconds delay05 = new WaitForSeconds(0.5f);
    WaitForSeconds delay07 = new WaitForSeconds(0.7f);
    [SerializeField] public Transform cardSpawnPoint;
    [SerializeField] public Transform cardSpawnPointEnemy;
    public static Action<bool,Transform> OnAddCard;
    public static Action<bool, Transform> OnAddSecurity;
    public static Action<bool,Transform> OnTurnStarted;
    public static Action <bool>DoMulligan;

    void GameSetup()
    {
        if (fastMode)
            delay05 = new WaitForSeconds(0.05f);
        switch (eTurnMode)
        {

            case ETurnMode.Random:
                myTurn = Random.Range(0, 2) == 0;
                break;

            case ETurnMode.My:
                myTurn = true;
                break;
            case ETurnMode.Other:
                myTurn = false;
                break;



        }


    }
    public IEnumerator StartGameCo()//게임 시작 코르틴 
    {
        GameSetup();
        isLoading = true;
        for (int i = 0; i < startCardCount; i++)
        {
            yield return delay05;
            OnAddCard?.Invoke(true,cardSpawnPoint);
            yield return delay05;
            OnAddCard?.Invoke(false,cardSpawnPointEnemy);


        }
        
        StartCoroutine(StartTurnCo());
    }
    IEnumerator MulliganCourtine()
    {
        if (myMulliganChecker)
        {
           
            yield return delay07;
            DoMulligan?.Invoke(true);
            for (int i = 0; i < startCardCount; i++)
            {
               
                OnAddCard?.Invoke(true, cardSpawnPoint);

            }
        }

        if (enemyMulliganChecker)
        {

            yield return delay07;
            DoMulligan?.Invoke(false);
            for (int i = 0; i < startCardCount; i++)
            {
           
                OnAddCard?.Invoke(false, cardSpawnPointEnemy);


            }
        }

        for (int i = 0; i < startCardCount; i++)
        {

            OnAddSecurity?.Invoke(true, cardSpawnPoint);

        }

        for (int i = 0; i < startCardCount; i++)
        {

            OnAddSecurity?.Invoke(false, cardSpawnPointEnemy);

        }
    }
    IEnumerator StartTurnCo()
    {
        StartCoroutine(MulliganCourtine());
        isLoading = true;
        
        yield return delay07;
        OnAddCard?.Invoke(myTurn, cardSpawnPoint);
        yield return delay07;
        isLoading = false;
        OnTurnStarted?.Invoke(false,cardSpawnPointEnemy);

    }
    
    public void EndTurn()
    {
        myTurn = !myTurn;
       StartCoroutine(StartTurnCo());
    }
}
