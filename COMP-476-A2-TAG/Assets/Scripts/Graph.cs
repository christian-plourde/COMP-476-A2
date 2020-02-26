using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graph
{
    //this interface is a requirement for the pathfinder graph extension since it does pathfinding with a*. before the algorithm runs, the heuristic costs are evaluated for each of the nodes
    public interface IHeuristic<T>
    {
        double ComputeHeuristic(T goal);

        int getClusterID();
    }

    public enum HEURISTIC_TYPE { NULL, EUCLIDEAN, CLUSTER }

    //a class representing an edge in the graph
    public class GraphEdge<T>
    {
        private GraphNode<T> start_node; //the start node of the edge
        private GraphNode<T> end_node; //the end node of the edge
        private double cost; //the cost of taking that edge

        public GraphNode<T> Start
        {
            get { return start_node; }
            set { start_node = value; }
        }

        public GraphNode<T> End
        {
            get { return end_node; }
            set { end_node = value; }
        }

        public double Cost
        {
            get { return cost; }
            set { cost = value; }
        }

        public GraphEdge(GraphNode<T> start, GraphNode<T> end, double cost)
        {
            this.start_node = start;
            this.end_node = end;
            this.cost = cost;
        }

        public override string ToString()
        {
            return Start.Value + " -> " + End.Value;
        }
    }

    //a class representing a node in the graph
    public class GraphNode<T>
    {
        private T value;
        private LinkedList<GraphEdge<T>> edges;
        private double cost_so_far; //this is the smallest found cost so far
        private GraphEdge<T> connection_edge; //this is the edge that the shortest path so far came from.
        private double heuristic; //the heuristic value for the node
        private double estimated_total_cost; //the estimated total cost to the goal node

        public double EstimatedTotalCost
        {
            get { return estimated_total_cost; }
            set { estimated_total_cost = value; }
        }

        public double Heuristic
        {
            get { return heuristic; }
            set { heuristic = value; }
        }

        public double CostSoFar
        {
            get { return cost_so_far; }
            set { cost_so_far = value; }
        }

        public GraphEdge<T> Connection
        {
            get { return connection_edge; }
            set { connection_edge = value; }
        }

        public LinkedList<GraphEdge<T>> Links
        {
            get { return edges; }
        }

        public List<GraphNode<T>> Neighbors
        {
            get {

                List<GraphNode<T>> neighbors = new List<GraphNode<T>>();
                foreach(GraphEdge<T> e in edges)
                {
                    neighbors.Add(e.End);
                }

                return neighbors;
            
            }
        }

        public GraphNode<T> RandomNeighbor()
        {
            int idx = UnityEngine.Random.Range(0, edges.Count);
            return Neighbors[idx];
        }

        public void AddNeighbor(GraphNode<T> n, double cost)
        {
            this.edges.AddLast(new GraphEdge<T>(this, n, cost));
        }
        
        public T Value
        {
            get { return value; }
            set { this.value = value; }
        }

        public GraphNode(T value)
        {
            this.value = value;
            edges = new LinkedList<GraphEdge<T>>();
            cost_so_far = double.MaxValue;
            estimated_total_cost = double.MaxValue;
        }

        public override string ToString()
        {
            string s = value.ToString() + " -> ";

            int i = 0;
            foreach(GraphEdge<T> e in edges)
            {
                if (i == edges.Count - 1)
                    s += e.End.Value.ToString();

                else
                    s += e.End.Value.ToString() + ", ";

                i++;
            }

            return s;
        }
    }

    //represents a basic graph that does dijkstra shortest path for pathfinding
    public class Graph<T> where T : IHeuristic<T>
    {
        protected LinkedList<GraphNode<T>> nodes; //a list of all nodes currently in the graph
        private GraphNode<T> start_node; //the start node for the current path finding iteration
        protected LinkedList<GraphNode<T>> open_nodes; //a list of all the currently discovered nodes for the pathfinding iteration
        protected LinkedList<GraphNode<T>> closed_nodes; //a list of processed nodes in the current pathfinding iteration
        protected GraphNode<T> current_node; //the node we are currently processing in the pathfinding iteration

        //data for the showing of fill
        //----------------------------------------------------
        protected GraphNode<T> curr_start_node;
        protected GraphNode<T> curr_end_node;
        protected LinkedList<GraphNode<T>> examined_nodes;
        private HEURISTIC_TYPE heuristic_type;
        ClusterLookupTable<double> cluster_table;
        public int cluster_count;

        public GraphNode<T> RandomNode()
        {
            int idx = UnityEngine.Random.Range(0, nodes.Count);

            return nodes.ToArray<GraphNode<T>>()[idx];
        }

        public ClusterLookupTable<double> ClusterTable
        {
            get { return cluster_table; }
        }

        public int ClusterCount
        {
            set { cluster_count = value; }
        }

        public GraphNode<T> CurrentStartNode
        {
            get { return curr_start_node; }
        }

        public GraphNode<T> CurrentEndNode
        {
            get { return curr_end_node; }
        }

        public LinkedList<GraphNode<T>> ExaminedNodes
        {
            get { return examined_nodes; }
        }

        public HEURISTIC_TYPE HeuristicType
        {
            get { return heuristic_type; }
            set { heuristic_type = value; }
        }

        //end data for the showing of fill
        //----------------------------------------------------

        public LinkedList<GraphNode<T>> Nodes
        {
            get { return nodes; }
        }

        protected GraphNode<T> StartNode
        {
            get { return start_node; }
            set { start_node = value; }
        }

        public Graph()
        {
            nodes = new LinkedList<GraphNode<T>>();
            open_nodes = new LinkedList<GraphNode<T>>();
            closed_nodes = new LinkedList<GraphNode<T>>();
            examined_nodes = new LinkedList<GraphNode<T>>();
        }
        
        //this function is used to get the node from the open list that has the smallest cost so far. It will be processed next when pathfinding
        private GraphNode<T> getSmallestCostSoFar()
        {
            double smallest_cost = open_nodes.First.Value.CostSoFar; //set smallest cost to first node
            GraphNode<T> smallest_cost_node = open_nodes.First.Value; // the node with smallest cost is the first node in the list

            //go through each node in the list and if that nodes cost so far is better than the current smallest cost update smallest cost and the node that had the smallest cost
            foreach (GraphNode<T> n in open_nodes)
            {
                if (n.CostSoFar < smallest_cost)
                {
                    smallest_cost = n.CostSoFar;
                    smallest_cost_node = n;
                }

            }

            return smallest_cost_node;
        }

        public void InitializeClusterTable()
        {
            //method to initialize the cluster lookup table used to get the heuristic value in the case where the heuristic type is set to cluster
            //in order to do this, we need to dijkstra evaluate once for each node. This will make the nodes in the graph have their cost so far at the end be the cost to run to that node from the current start node.

            cluster_table = new ClusterLookupTable<double>(cluster_count);

            foreach(GraphNode<T> n in nodes)
            {
                open_nodes.Clear(); //clear both open and closed lists since they will be repopulated in this pathfinding step
                closed_nodes.Clear();
                this.StartNode = n; //start node for pathfinding step

                resetCosts(); //reset all the costs

                start_node.CostSoFar = 0;
                DijkstraEvaluate();

                //once the evaluation has been done we will update the cluster table accordingly
                foreach(GraphNode<T> g in nodes)
                {
                    //if the nodes are in the same cluster the cluster heuristic is 0
                    if (n.Value.getClusterID() == g.Value.getClusterID())
                        cluster_table[n.Value.getClusterID(), g.Value.getClusterID()] = 0;

                    //for each node, compare the costsofar of g with the corresponding entry [cluster id of n][cluster id of g] for the distance from cluster n to cluster g. if it is less than that entry, we update the value
                    else if (g.CostSoFar < cluster_table[n.Value.getClusterID(), g.Value.getClusterID()])
                        cluster_table[n.Value.getClusterID(), g.Value.getClusterID()] = g.CostSoFar;
                }
            }
            examined_nodes.Clear();
        }

        public void Add(GraphNode<T> node)
        {
            nodes.AddLast(node);
        }

        //this is called at the beginning of the pathfindiong step to make sure that the cost so far of each node is reinitialized. The cost is initially as big as possible to make sure that the first time we see a node we dont skip it
        protected void resetCosts()
        {
            foreach (GraphNode<T> n in nodes)
                n.CostSoFar = double.MaxValue;
        }

        public virtual LinkedList<GraphNode<T>> ShortestPath(GraphNode<T> start_node, GraphNode<T> end_node)
        {
            examined_nodes.Clear();
            curr_start_node = start_node;
            curr_end_node = end_node;

            open_nodes.Clear(); //clear both open and closed lists since they will be repopulated in this pathfinding step
            closed_nodes.Clear();
            this.StartNode = start_node; //start node for pathfinding step

            resetCosts(); //reset all the costs
            
            start_node.CostSoFar = 0;
            DijkstraEvaluate();

            //after the evaluation is done, each node has its cost and connections set
            //starting at the end node and appending nodes by following the connections backwards allows us to build and return a list containing the nodes in the shortest path in order.
            GraphNode<T> curr = end_node;
            LinkedList<GraphNode<T>> node_order = new LinkedList<GraphNode<T>>();

            while(curr != start_node)
            {
                node_order.AddFirst(curr);
                curr = curr.Connection.Start;
            }

            node_order.AddFirst(curr);

            return node_order;
        }

        private void DijkstraEvaluate()
        {
            //we evaluate this from the start node
            current_node = start_node;
            open_nodes.AddLast(current_node); //add the current node to open list since we will process it
            examined_nodes.AddLast(current_node);

            //as long as there are still nodes to process we evaluate all of the current nodes neighbors
            while(open_nodes.Count > 0)
            {
                DijkstraEvaluateNeighbors(); 
                if(open_nodes.Count > 0) //after evaluating all neighbors we update the cost so far for the next iteration since we need to know which node to process next
                    current_node = getSmallestCostSoFar();
            }

        }

        private void DijkstraEvaluateNeighbors()
        {
            //we look at each of the neighbors of the current node and update their costs so far and connection values
            foreach(GraphEdge<T> e in current_node.Links)
            {
                bool cost_changed = false;
                //for each edge if the cost so far of the start + the cost of the edge we are on is less than the end
                //current cost so far we should update its connection and its cost so far
                if(e.Start.CostSoFar + e.Cost < e.End.CostSoFar)
                {
                    e.End.CostSoFar = e.Start.CostSoFar + e.Cost;
                    e.End.Connection = e;
                    cost_changed = true;
                }

                //add the end node to the open list
                if(!open_nodes.Contains(e.End) && cost_changed)
                {
                    open_nodes.AddLast(e.End);
                    examined_nodes.AddLast(e.End);
                }
                    
            }

            open_nodes.Remove(current_node);
            closed_nodes.AddLast(current_node);
            
        }

        public override string ToString()
        {
            string s = string.Empty;
            foreach(GraphNode<T> node in nodes)
            {
                s += node.ToString() + "\n";
            }

            return s;
        }
    }

    //This class is the same as the basic graph but it does pathfinding with a*
    public class PathFinderGraph<T> : Graph<T> where T : IHeuristic<T>
    {
        public PathFinderGraph() : base()
        {

        }

        //similar to get smallest cost so far but we compute smallest cost by getting the smallest estimated total cost with the heuristic cost and the cost so far
        private GraphNode<T> getSmallestEstimatedCost()
        {
            double smallest_cost = open_nodes.First.Value.EstimatedTotalCost;
            GraphNode<T> smallest_cost_node = open_nodes.First.Value;

            foreach (GraphNode<T> n in open_nodes)
            {
                if (n.EstimatedTotalCost < smallest_cost)
                {
                    smallest_cost = n.EstimatedTotalCost;
                    smallest_cost_node = n;
                }

            }

            return smallest_cost_node;
        }

        //shortest path using a*
        public override LinkedList<GraphNode<T>> ShortestPath(GraphNode<T> start_node, GraphNode<T> end_node)
        {
            examined_nodes.Clear();
            curr_start_node = start_node;
            curr_end_node = end_node;

            open_nodes.Clear(); //clear open and closed and reset all costs (costs so far)
            closed_nodes.Clear();
            this.StartNode = start_node;
            resetCosts();
            start_node.CostSoFar = 0;

            //for each node compute its heuristic cost
            foreach(GraphNode<T> n in nodes)
            {
                n.Heuristic = n.Value.ComputeHeuristic(end_node.Value);
            }

            //same node path construction as parent class
            Evaluate(end_node);

            GraphNode<T> curr = end_node;
            LinkedList<GraphNode<T>> node_order = new LinkedList<GraphNode<T>>();

            while (curr != start_node)
            {
                node_order.AddFirst(curr);
                curr = curr.Connection.Start;
            }

            node_order.AddFirst(curr);

            return node_order;
        }

        private void Evaluate(GraphNode<T> end_node)
        {
            //we evaluate this from the start node
            current_node = StartNode;
            open_nodes.AddLast(current_node);
            examined_nodes.AddLast(current_node);

            while (open_nodes.Count > 0 && getSmallestEstimatedCost() != end_node)
            {
                EvaluateNeighbors();
                if (open_nodes.Count > 0)
                    current_node = getSmallestEstimatedCost();
            }

        }

        private void EvaluateNeighbors()
        {
            //we look at each of the neighbors of the current node and update their costs so far and connection values
            foreach (GraphEdge<T> e in current_node.Links)
            {
                bool cost_changed = false;
                //for each edge if the cost so far of the start + the cost of the edge we are on is less than the end
                //current cost so far we should update its connection and its cost so far
                if (e.Start.CostSoFar + e.Cost < e.End.CostSoFar)
                {
                    e.End.CostSoFar = e.Start.CostSoFar + e.Cost;
                    e.End.EstimatedTotalCost = e.End.Heuristic + e.End.CostSoFar;
                    e.End.Connection = e;
                    cost_changed = true;
                }

                //add the end node to the open list
                if (!open_nodes.Contains(e.End) && cost_changed)
                {
                    open_nodes.AddLast(e.End);
                    examined_nodes.AddLast(e.End);
                }
                   
            }

            open_nodes.Remove(current_node);
            closed_nodes.AddLast(current_node);

        }

    }

}
