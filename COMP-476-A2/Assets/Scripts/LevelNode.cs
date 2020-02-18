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

        lineRenderers = new List<LineRenderer>(); //instantiate the list of line renderers
        foreach (GameObject o in linked_nodes)
        {
            GameObject lineDrawer = Instantiate(lineRendererPrefab) as GameObject; //this is required to draw the path lines
            lineRenderers.Add(lineDrawer.GetComponent<LineRenderer>()); //for each of the conenctions the node has, we create a new line renderer to draw the connection path
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
        //set the material for the node appropriately
        if (node == graph.CurrentStartNode)
            this.gameObject.GetComponent<MeshRenderer>().material = start_node_mat;

        else if (node == graph.CurrentEndNode)
            this.gameObject.GetComponent<MeshRenderer>().material = end_node_mat;

        else if (graph.ExaminedNodes.Contains(node))
            this.gameObject.GetComponent<MeshRenderer>().material = examined_node_mat;

        else
            this.gameObject.GetComponent<MeshRenderer>().material = default_node_mat;

        //this code will draw the paths using the line renderers
        int i = 0;

        foreach(GameObject o in linked_nodes)
        {
            if (character.GetComponent<Character>().Path.Contains<GraphNode<LevelNode>>(o.GetComponent<LevelNode>().GraphNode) && character.GetComponent<Character>().Path.Contains<GraphNode<LevelNode>>(GraphNode))
            {
                lineRenderers[i].material = path_edge_mat;
            }
                

            else if (graph.ExaminedNodes.Contains(GraphNode) && graph.ExaminedNodes.Contains(o.GetComponent<LevelNode>().GraphNode))
                lineRenderers[i].material = examined_node_mat;

            else
                lineRenderers[i].material = default_node_mat;

            lineRenderers[i].SetPosition(0, this.transform.position); //draw a line from the start to the end of the connection
            lineRenderers[i].SetPosition(1, o.transform.position);
            i++;
        }
    }
}
