using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Graph;
using UnityEngine.UI;

//responsible for creating and maintaining the graph structure
public class GraphManager : MonoBehaviour
{
    private Graph<LevelNode> graph;
    public HEURISTIC_TYPE heuristic_type;
    public int cluster_count;

    private void Awake()
    {
        if (heuristic_type == HEURISTIC_TYPE.NULL)
            graph = new Graph<LevelNode>();
        else
            graph = new PathFinderGraph<LevelNode>();
    }

    public Graph<LevelNode> Graph
    {
        get { return graph; }
    }

    private void initialize_nodes()
    {
        LevelNode[] nodes = FindObjectsOfType<LevelNode>(); //find all the level nodes in the scene and add them to the graph
        foreach (LevelNode n in nodes)
        {
            graph.Add(n.GraphNode);
            n.Graph = graph;
        }
        graph.HeuristicType = heuristic_type;

        if (graph.HeuristicType == HEURISTIC_TYPE.CLUSTER)
        {
            graph.ClusterCount = cluster_count;
            graph.InitializeClusterTable();
        }
            
    }

    // Start is called before the first frame update
    void Start()
    {
        initialize_nodes();
    }
}
