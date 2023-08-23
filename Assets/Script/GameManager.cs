using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
   
    // Start is called before the first frame update
    void Start()
    {
        StartGame();
    }

    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR
        InputCheatKey();
#endif


    }
    void InputCheatKey()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
           
            TurnManager.OnAddCard?.Invoke(true,TurnManager.Inst.cardSpawnPoint);
            
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
           
           
            TurnManager.OnAddCard?.Invoke(false, TurnManager.Inst.cardSpawnPointEnemy);
            
        }



    }
    public void StartGame()
    {
       StartCoroutine(TurnManager.Inst.StartGameCo());

    }
}
