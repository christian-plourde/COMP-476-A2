using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Graph;

public class LevelNode : MonoBehaviour, IHeuristic<LevelNode>
{
    GraphNode<LevelNode> node; //this is node in the actual graph that will be used for pathfinding
    public GameObject[] linked_nodes; //this is a list of node objects (in the scene) that the current node is connected to. This information will be used to construct the graph
    public GameObject lineRendererPrefab; //this is a prefab game object that contains a line renderer. This is important because we will need to instantiate a separate line renderer for each game object
    List<LineRenderer> lineRenderers; //this is the list of line renderers. we will instantiate one for each connection that the node has to draw the paths in the scene
    public Material start_node_mat;
    public Material end_node_mat;
    public Material default_node_mat;
    public Material examined_node_mat;
    public Material path_edge_mat;
    private Graph<LevelNode> graph;
    public GameObject character;
    public int cluster_id;

    public int getClusterID()
    {
        return cluster_id;
    }

    public Graph<LevelNode> Graph
    {
        set { graph = value; }
    }

    public GraphNode<LevelNode> GraphNode
    {
        get { return node; }
    }

    public double ComputeHeuristic(LevelNode goal)
    {
        if (graph.HeuristicType == HEURISTIC_TYPE.EUCLIDEAN)
            return (goal.GraphNode.Value.transform.position - node.Value.transform.position).magnitude;
        if (graph.HeuristicType == HEURISTIC_TYPE.CLUSTER)
            return (graph.ClusterTable[this.cluster_id, goal.cluster_id]);
        else
            return 0;
    }

    private void Awake()
    {
        //let's set the graph node that its connected to 
        node = new GraphNode<LevelNode>(this);
    }

    public void Initialize()
    {
        //the graph node needs to be connected to all of it neighbors as well
        foreach (GameObject o in linked_nodes)
        {
            node.AddNeighbor(o.GetComponent<LevelNode>().GraphNode, (o.transform.position - this.transform.position).magnitude);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Initialize();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
