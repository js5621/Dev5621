using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Order : MonoBehaviour
{
    // Start is called before the first frame update 
    [SerializeField] Renderer[] backRenderers;
    [SerializeField] Renderer[] middleRenderers;
    [SerializeField] string sortingLayerName;
    int originOrder;
    public void SetOriginOrder(int originOrder)//맨앞정하기 
    {
        this.originOrder = originOrder;
        SetOrder(originOrder);
    }
    public void SetMostFrontOrder(bool isMostFront)
    {

        SetOrder(isMostFront ? 100 : originOrder);
    }


    public void SetOrder(int order)
    {
        int mulOrder = order * 10;
        foreach (var renderer in backRenderers)
        {

            renderer.sortingLayerName = sortingLayerName;
            renderer.sortingOrder = mulOrder;
        }

        foreach (var renderer in middleRenderers)
        {

            renderer.sortingLayerName = sortingLayerName;
            renderer.sortingOrder = mulOrder + 1;
        }


    }
}
