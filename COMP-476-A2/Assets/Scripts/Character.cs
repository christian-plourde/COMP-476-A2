using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Graph;
using System.Linq;

//a class used for an npc character
public class Character : NPC
{
    public GameObject startNode; //the node that the chracter should start at
    private GraphNode<LevelNode> current_node; //the node that the chracter is currently at in the graph
    private Graph<LevelNode> graph; //a reference to the graph that is used for setting the movement path for the chracter
    public Camera cam;
    private GraphNode<LevelNode>[] path = new GraphNode<LevelNode>[0]; //this is a list containing the nodes in the current chracters path
    private int current_path_node_index = 0; //the step of the path the character s currently executing
    private GraphNode<LevelNode> currentTarget;

    public GraphNode<LevelNode>[] Path
    {
        get { return path; }
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        //initialize the node that the character is at to the graph node of the level node that he was placed at to begin
        current_node = startNode.GetComponent<LevelNode>().GraphNode;

        //set the position of the character to the position of the current node
        transform.position = current_node.Value.transform.position;

        graph = FindObjectOfType<GraphManager>().Graph;
        Movement.Target = current_node.Value.transform.position;

        MaxVelocity = 100 * MaxVelocity;
    }

    // Update is called once per frame
    protected override void Update()
    {
        if (Input.GetKey(KeyCode.Mouse0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if(hit.transform.gameObject.GetComponent<LevelNode>())
                {

                    current_path_node_index = 0;
                    path = graph.ShortestPath(current_node, hit.transform.gameObject.GetComponent<LevelNode>().GraphNode).ToArray();

                    //check to make sure the node we are going to is in the path. if its not we need to go back to the start to avoid clipping through the graph
                    if(!path.Contains(currentTarget))
                    {
                        Movement.Target = current_node.Value.transform.position;
                        currentTarget = current_node;
                    }

                }
            }

        }

        try
        {
            if (Movement.HasArrived)
            {
                current_node = path[current_path_node_index];
                Movement.Target = path[++current_path_node_index].Value.transform.position;
                currentTarget = path[current_path_node_index];
            }
        }

        catch
        { }
        

        base.Update();
    }
}
