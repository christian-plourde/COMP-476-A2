using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Graph;

public class LevelNode : MonoBehaviour
{
    GraphNode<LevelNode> node; //this is node in the actual graph that will be used for pathfinding
    public GameObject[] linked_nodes; //this is a list of node objects (in the scene) that the current node is connected to. This information will be used to construct the graph
    public GameObject lineRendererPrefab; //this is a prefab game object that contains a line renderer. This is important because we will need to instantiate a separate line renderer for each game object
    List<LineRenderer> lineRenderers; //this is the list of line renderers. we will instantiate one for each connection that the node has to draw the paths in the scene

    public GraphNode<LevelNode> GraphNode
    {
        get { return node; }
    }

    private void Awake()
    {
        //let's set the graph node that its connected to 
        node = new GraphNode<LevelNode>(this);
    }

    // Start is called before the first frame update
    void Start()
    {
        //the graph node needs to be connected to all of it neighbors as well
        foreach (GameObject o in linked_nodes)
        {
            node.AddNeighbor(o.GetComponent<LevelNode>().GraphNode, (o.transform.position - this.transform.position).magnitude);
        }

        lineRenderers = new List<LineRenderer>(); //instantiate the list of line renderers
        foreach(GameObject o in linked_nodes)
        {
            GameObject lineDrawer = Instantiate(lineRendererPrefab) as GameObject; //this is required to draw the path lines
            lineRenderers.Add(lineDrawer.GetComponent<LineRenderer>()); //for each of the conenctions the node has, we create a new line renderer to draw the connection path
        }

    }

    // Update is called once per frame
    void Update()
    {
        //this code will draw the paths using the line renderers
        int i = 0;

        foreach(GameObject o in linked_nodes)
        {
            lineRenderers[i].SetPosition(0, this.transform.position); //draw a line from the start to the end of the connection
            lineRenderers[i].SetPosition(1, o.transform.position);
            i++;
        }
    }
}
