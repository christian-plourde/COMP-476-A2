using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Graph;
using System.Linq;
using UnityEngine.AI;

public enum BEHAVIOUR_TYPE { WANDER, MOVE_TO_LAST_SPOTTER, FLANK, TAGGING, EVADE, TURN_AROUND, NULL }

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
    private bool is_tag = false;
    private DecisionTree decisionTree; //dictates the behaviour when the character is not the tag
    private FiniteStateMachine finiteStateMachine; //dictates behaviour when character is not the tag
    private TagManager tag_manager;
    private float line_of_sight_y_offset = 0.3f;
    private BEHAVIOUR_TYPE behaviour;
    private BEHAVIOUR_TYPE previous_behaviour = BEHAVIOUR_TYPE.NULL;
    private float tag_radius = 0.5f; //radius within which we can tag the it player
    public string name;
    public Material normal_material;
    public Material it_material;
    public Character last_player_seen; //used for running away from a player when the character is it

    public override float MaxVelocity
    {
        get { if (IsIt)
                return 2.5f * base.MAX_VELOCITY;
            else
                return base.MAX_VELOCITY;
        }
        set { base.MAX_VELOCITY = value; }
    }

    public GraphNode<LevelNode>[] Path
    {
        get { return path; }
    }

    public bool IsIt
    {
        get { return is_tag; }
        set { is_tag = value;

            if (IsIt)
            {
                this.GetComponentInChildren<SkinnedMeshRenderer>().material = it_material;
            }

            else
            {
                this.GetComponentInChildren<SkinnedMeshRenderer>().material = normal_material;
            }
                
        }
    }

    public TagManager Manager
    {
        get { return tag_manager; }
        set { tag_manager = value; }
    }

    #region Decision Tree Checks
    //this will return true if the it player is visible from this characters position and false otherwise.
    public bool ItPlayerVisible()
    {
        Ray ray = new Ray(new Vector3(Position.x, Position.y + line_of_sight_y_offset, Position.z), this.transform.forward);
        Debug.DrawRay(new Vector3(Position.x, Position.y + line_of_sight_y_offset, Position.z), this.transform.forward * 1.5f, Color.red);
        RaycastHit hit;

        if(Physics.Raycast(ray, out hit))
        {
            if(hit.transform.gameObject.GetComponent<Character>())
            {
                if (hit.transform.gameObject.GetComponent<Character>() == Manager.ItPlayer)
                {
                    //Debug.Log(this.gameObject + " saw " + hit.transform.gameObject);
                    return true;
                }
                    
            }
        }

        return false;
    }

    //tells us if the it player is in the proximity of our character
    public bool ItPlayerInProximity()
    {
        if(!IsIt && (Manager.ItPlayer.Position - Position).magnitude < tag_radius)
        {
            return true;
        }

        return false;
    }

    private bool ItPlayerStillVisible()
    {
        return Manager.LastSpotter.ItPlayerVisible();
    }

    #endregion

    #region Decision Tree Actions
    private void SetWander()
    {
        behaviour = BEHAVIOUR_TYPE.WANDER;   
    }

    private void SetMoveToLastSpotter()
    {
        behaviour = BEHAVIOUR_TYPE.MOVE_TO_LAST_SPOTTER;
    }

    private void SetFlank()
    {
        behaviour = BEHAVIOUR_TYPE.FLANK;
    }

    private void SetTagging()
    {
        behaviour = BEHAVIOUR_TYPE.TAGGING;
    }

    #endregion

    #region Finite State Machine Transitions

    private bool PlayerInLineOfSight()
    {
        //we need to check if any of the other players are visible from our point of view as the it player

        Ray ray = new Ray(new Vector3(Position.x, Position.y + line_of_sight_y_offset, Position.z), this.transform.forward);
        Debug.DrawRay(new Vector3(Position.x, Position.y + line_of_sight_y_offset, Position.z), this.transform.forward * 1.5f, Color.red);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform.gameObject.GetComponent<Character>())
            {
                last_player_seen = hit.transform.gameObject.GetComponent<Character>();
                return true;
            }
        }

        return false;
    }

    private bool AtDeadEnd()
    {
        if (current_node.Neighbors.Count == 1)
            return true;

        return false;
    }

    private bool NotAtDeadEnd()
    {
        return !AtDeadEnd();
    }

    private bool PlayerNotInLineOfSight()
    {
        return !PlayerInLineOfSight();
    }

    #endregion

    #region Finite State Machine Actions

    private void SetRunAway()
    {
        behaviour = BEHAVIOUR_TYPE.EVADE;
    }

    private void SetTurnAround()
    {
        behaviour = BEHAVIOUR_TYPE.TURN_AROUND;
    }

    private void SetStayPut()
    {
        behaviour = BEHAVIOUR_TYPE.NULL;
    }

    #endregion

    private void SetDecisionTree()
    {
        //we need to create a decision tree for the behaviour of the non-tag npc's
        //first the check node to see if the it player is visible by us
        CheckNode it_player_visible = new CheckNode(ItPlayerVisible);

        //------------------- LEFT SUBTREE --------------------//
        //next a node to tell us if someone else can see the it player
        CheckNode it_player_spotted = new CheckNode(Manager.ItPlayerSpotted);
        //next a node to tell us if the last person to see the it player can still see him
        CheckNode it_player_still_visible = new CheckNode(ItPlayerStillVisible);

        //now the action nodes
        ActionNode Wander = new ActionNode(SetWander);
        ActionNode MoveToLastSpotter = new ActionNode(SetMoveToLastSpotter);
        ActionNode Flank = new ActionNode(SetFlank);

        //------------------- RIGHT SUBTREE ------------------//
        CheckNode close_enough_to_tag = new CheckNode(ItPlayerInProximity);
        ActionNode ItPlayerNotClose = new ActionNode(() => { }); //we do nothing in this case
        ActionNode ItPlayerClose = new ActionNode(SetTagging);

        //now setting the relationships
        it_player_visible.False = it_player_spotted;
        it_player_visible.True = close_enough_to_tag;

        it_player_spotted.False = Wander;
        it_player_spotted.True = it_player_still_visible;

        it_player_still_visible.False = MoveToLastSpotter;
        it_player_still_visible.True = Flank;

        close_enough_to_tag.False = ItPlayerNotClose;
        close_enough_to_tag.True = ItPlayerClose;

        decisionTree = new DecisionTree(it_player_visible);
    }

    private void SetFiniteStateMachine()
    {
        FiniteStateMachineNode wander = new FiniteStateMachineNode("wander", SetWander);
        FiniteStateMachineNode turnAround = new FiniteStateMachineNode("turn around", SetTurnAround);
        FiniteStateMachineNode runAway = new FiniteStateMachineNode("run away", SetRunAway);
        FiniteStateMachineNode stayPut = new FiniteStateMachineNode("stay put", SetStayPut);

        wander.AddTransition("Player in line of sight", PlayerInLineOfSight, 2, runAway);
        wander.AddTransition("At Dead End", AtDeadEnd, 1, turnAround);

        runAway.AddTransition("Player not in line of sight", PlayerNotInLineOfSight, 1, wander);

        turnAround.AddTransition("Not at dead end", NotAtDeadEnd, 1, wander);
        turnAround.AddTransition("Player in line of sight", PlayerInLineOfSight, 2, stayPut);

        finiteStateMachine = new FiniteStateMachine(wander);
    }

    //this will return the furthest neighbor (straight line distance) to the current node from the current node of the character passed as an argument. used for evading a chaser
    public GraphNode<LevelNode> FurthestNeighborFromCharacter(Character chaser)
    {
        //for each of our neighbors, compute the shortest path to the current node of the chaser
        //then check the cost so far of the last node in the path. if that cost is greater than the smallest cost so far for the neighbors update the node that we will return

        GraphNode<LevelNode> furthestNeighbor = this.current_node.Neighbors[0];

        double furthest_distance = 0;

        foreach (GraphNode<LevelNode> n in this.current_node.Neighbors)
        {
            if ((n.Value.transform.position - chaser.currentTarget.Value.transform.position).magnitude > furthest_distance)
            {
                furthest_distance = (n.Value.transform.position - chaser.currentTarget.Value.transform.position).magnitude;
                furthestNeighbor = n;
            }
        }

        return furthestNeighbor;
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
        SetDecisionTree();

        SetFiniteStateMachine();
    }

    #region Behaviour Update Functions

    private void MoveToLastSpotterUpdate()
    {
        if (Manager.NavMesh)
        {
            this.gameObject.GetComponent<NavMeshAgent>().SetDestination(Manager.LastSpotter.current_node.Value.transform.position);
        }

        else
        {
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
            {
                current_path_node_index = 0;
                path = graph.ShortestPath(current_node, Manager.LastSpotter.current_node).ToArray();

                if (!path.Contains(currentTarget))
                {
                    Movement.Target = current_node.Value.transform.position;
                    currentTarget = current_node;
                }
            }

            base.Update();
        }
    }

    private void WanderUpdate()
    {
        if(Manager.NavMesh)
        {
            this.gameObject.GetComponent<NavMeshAgent>().SetDestination(graph.RandomNode().Value.transform.position);
        }

        else
        {
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
            {
                current_path_node_index = 0;
                path = graph.ShortestPath(current_node, graph.RandomNode()).ToArray();

                if (!path.Contains(currentTarget))
                {
                    Movement.Target = current_node.Value.transform.position;
                    currentTarget = current_node;
                }
            }

            base.Update();
        }
        
    }

    private void FlankUpdate()
    {
        if (Manager.NavMesh)
        {
            this.gameObject.GetComponent<NavMeshAgent>().SetDestination(Manager.ItPlayer.current_node.RandomNeighbor().Value.transform.position);
        }

        else
        {
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
            {
                current_path_node_index = 0;
                path = graph.ShortestPath(current_node, Manager.ItPlayer.current_node.RandomNeighbor()).ToArray();

                if (!path.Contains(currentTarget))
                {
                    Movement.Target = current_node.Value.transform.position;
                    currentTarget = current_node;
                }
            }

            base.Update();
        }
        
    }

    private void TaggingUpdate()
    {
        Movement.Target = Manager.ItPlayer.current_node.Value.transform.position;
    }

    private void RunAwayUpdate()
    {
        if (Manager.NavMesh)
        {
            this.gameObject.GetComponent<NavMeshAgent>().SetDestination(FurthestNeighborFromCharacter(last_player_seen).Value.transform.position);
        }

        else
        {

            current_path_node_index = 0;
            path = graph.ShortestPath(current_node, FurthestNeighborFromCharacter(last_player_seen)).ToArray();

            if (!path.Contains(currentTarget))
            {
                Movement.Target = current_node.Value.transform.position;
                currentTarget = current_node;
            }

            base.Update();
        }
    }

    private void TurnAroundUpdate()
    {
        if (Manager.NavMesh)
        {
            this.gameObject.GetComponent<NavMeshAgent>().SetDestination(current_node.Neighbors[0].Value.transform.position);
        }

        else
        {
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
            {
                current_path_node_index = 0;
                path = graph.ShortestPath(current_node, current_node.Neighbors[0]).ToArray();

                if (!path.Contains(currentTarget))
                {
                    Movement.Target = current_node.Value.transform.position;
                    currentTarget = current_node;
                }
            }

            base.Update();
        }
    }

    #endregion

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "ItPlayer")
        {
            Manager.ResetGame();
        }
    }

    private void DebugDecision()
    {
        string motion = "";

        switch(behaviour)
        {
            case BEHAVIOUR_TYPE.WANDER: motion = "Wandering"; break;
            case BEHAVIOUR_TYPE.MOVE_TO_LAST_SPOTTER: motion = "Moving to Last Spotter"; break;
            case BEHAVIOUR_TYPE.FLANK: motion = "Flanking"; break;
            case BEHAVIOUR_TYPE.TAGGING: motion = "Tagging"; break;
        }

        Debug.Log(name + ": " + motion);
    }

    // Update is called once per frame
    protected override void Update()
    {
        if (!IsIt)
            decisionTree.Evaluate();

        else
            finiteStateMachine.Evaluate();

        if(Manager.NavMesh)
        {
            if (behaviour != previous_behaviour || this.GetComponent<NavMeshAgent>().velocity.magnitude == 0)
            {
                previous_behaviour = behaviour;

                //DebugDecision();

                switch (behaviour)
                {
                    case BEHAVIOUR_TYPE.WANDER: WanderUpdate(); break;
                    case BEHAVIOUR_TYPE.MOVE_TO_LAST_SPOTTER: MoveToLastSpotterUpdate(); break;
                    case BEHAVIOUR_TYPE.FLANK: FlankUpdate(); break;
                    case BEHAVIOUR_TYPE.TAGGING: TaggingUpdate(); break;
                    case BEHAVIOUR_TYPE.EVADE: RunAwayUpdate(); break;
                    case BEHAVIOUR_TYPE.TURN_AROUND: TurnAroundUpdate(); break;
                }
            }
        }

        else
        {
            switch (behaviour)
            {
                case BEHAVIOUR_TYPE.WANDER: WanderUpdate(); break;
                case BEHAVIOUR_TYPE.MOVE_TO_LAST_SPOTTER: MoveToLastSpotterUpdate(); break;
                case BEHAVIOUR_TYPE.FLANK: FlankUpdate(); break;
                case BEHAVIOUR_TYPE.TAGGING: TaggingUpdate(); break;
                case BEHAVIOUR_TYPE.EVADE: RunAwayUpdate(); break;
                case BEHAVIOUR_TYPE.TURN_AROUND: TurnAroundUpdate(); break;
            }
        }
    }
}
