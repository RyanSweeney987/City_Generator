using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateFactory : MonoBehaviour
{
    public int minWidth = 5;
    public int maxWidth = 8;
    public Material wireMaterial;
    public List<FactoryComponentData> factoryComponents;

    private GameObject factory;

    public void Generate()
    {
        Clear();

        FactoryGenerator factoryGenerator = new FactoryGenerator();
        factoryGenerator.factoryComponents = factoryComponents;
        factoryGenerator.wireMaterial = wireMaterial;
        factory = factoryGenerator.GenerateFactory(minWidth, maxWidth);
        factoryGenerator.CreatePowerLine(factory);
        factory.name = "Factory";

    }

    public void Clear()
    {
        if (Application.isEditor)
        {
            DestroyImmediate(factory);
        }
        else if (Application.isPlaying)
        {
            Destroy(factory);
        }
    }

    
}
