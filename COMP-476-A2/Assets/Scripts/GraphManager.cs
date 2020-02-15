using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Graph;

public class GraphManager : MonoBehaviour
{
    private Graph<LevelNode> graph;

    private void Awake()
    {
        graph = new Graph<LevelNode>();
    }

    public Graph<LevelNode> Graph
    {
        get { return graph; }
    }

    // Start is called before the first frame update
    void Start()
    {
        LevelNode[] nodes = FindObjectsOfType<LevelNode>();
        foreach(LevelNode n in nodes)
        {
            graph.Add(n.GraphNode);
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
