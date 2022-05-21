using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Order : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] Renderer[] backRenders;
    [SerializeField] Renderer[] middleRenders;
    [SerializeField] string sortingLayerName;
    int originOrder;
    public void SetOriginOrder(int originOrder)
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

        foreach(var renderer in backRenders)
        {
            renderer.sortingLayerName = sortingLayerName;
            renderer.sortingOrder = mulOrder;
        }
        foreach (var renderer in middleRenders)
        {
            renderer.sortingLayerName = sortingLayerName;
            renderer.sortingOrder = mulOrder + 1;
        }
    }
    
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
