using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Graph;
using System.Linq;

public class Character : NPC
{
    public GameObject startNode;
    private GraphNode<LevelNode> current_node;
    private Graph<LevelNode> graph;
    public Camera cam;
    private GraphNode<LevelNode>[] path = new GraphNode<LevelNode>[0];
    private int current_path_node_index = 0;

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

        MaxVelocity = 10 * MaxVelocity;
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
                    if(Movement.HasArrived)
                    {
                        if (path.Length == 0 || current_path_node_index == path.Length)
                        {
                            current_path_node_index = 0;
                            path = graph.ShortestPath(current_node, hit.transform.gameObject.GetComponent<LevelNode>().GraphNode).ToArray();
                        }
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
            }
        }

        catch
        { }
        

        base.Update();
    }
}
