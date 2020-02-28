using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public abstract class DecisionTool
{
    public DecisionTool() { }

    public abstract bool Evaluate();
}

#region DecisionTree
//a node for the decision tree. It can contain an action to be executed (if its a leaf node) or a condition to evaluate
public abstract class DecisionTreeNode
{
    DecisionTreeNode parent;
    DecisionTreeNode leftChild;
    DecisionTreeNode rightChild;

    public DecisionTreeNode False
    {
        get { return leftChild; }
        set { leftChild = value;
            leftChild.parent = this;
        }
    }

    public DecisionTreeNode True
    {
        get { return rightChild; }
        set { rightChild = value;
            rightChild.parent = this;
        }
    }

    public bool IsLeaf
    {
        get { return False == null && True == null; }
    }

    public DecisionTreeNode()
    {
            
    }

    public abstract bool Evaluate();
}

//class for a condition node
public class CheckNode : DecisionTreeNode
{
    Func<bool> condition;

    public CheckNode(Func<bool> param)
    {
        this.condition = param;
    }

    public override bool Evaluate()
    {
        //return the result of evaluating the condition
        return condition();
    }
}

//class for an action node (leaves)
public class ActionNode : DecisionTreeNode
{
    Action action;

    public ActionNode(Action action)
    {
        this.action = action;
    }

    public override bool Evaluate()
    {
        try
        {
            //if we fail to execute the action return false, otherwise return true
            action();
        }

        catch
        {
            return false;
        }
            
        return true;
    }
}

public class DecisionTree : DecisionTool
{
    DecisionTreeNode root; //the root of the tree (first node evaluated)

    public DecisionTree(DecisionTreeNode root)
    {
        this.root = root;
    }

    public override bool Evaluate()
    {
        bool res = false;
        Evaluate(root, ref res);
        return res;
    }

    //evaluate node by node (going left or right based on the result of condition evaluation) until we reach a leaf where we execute the action
    private void Evaluate(DecisionTreeNode n, ref bool res)
    {
        if (n.IsLeaf)
        {
            res = n.Evaluate();
            return;
        }

        //if evaluation of node is true go right (right for success, left for failure)
        if (n.Evaluate())
            Evaluate(n.True, ref res);

        else
            Evaluate(n.False, ref res);
    }
}

#endregion

#region FiniteStateMachine

//this defines the transitions between nodes
public class FiniteStateMachineTransition
{
    Func<bool> condition;
    int priority;
    FiniteStateMachineNode start;
    FiniteStateMachineNode end;
    string name;

    public int Priority
    {
        get { return priority; }
    }

    public FiniteStateMachineNode End
    {
        get { return end; }
    }

    public FiniteStateMachineTransition(string name, FiniteStateMachineNode start, FiniteStateMachineNode end, Func<bool> condition, int priority)
    {
        this.name = name;
        this.start = start;
        this.end = end;
        this.condition = condition;
        this.priority = priority;
    }

    public bool Evaluate()
    {
        return condition();
    }

    public override string ToString()
    {
        return (name + " (" + priority + ")");
    }
}

//a class for a finite state machine node. this node has an action to be executed when we are on that node (ideally every frame)
public class FiniteStateMachineNode
{
    Action action; //the action to be executed when we are on that node
    List<FiniteStateMachineTransition> transitions;
    string name;

    public List<FiniteStateMachineTransition> Transitions
    {
        get { return transitions; }
    }

    public FiniteStateMachineNode(string name, Action action)
    {
        transitions = new List<FiniteStateMachineTransition>();
        this.action = action;
    }

    //this function will sort the transitions in order of decreasing priority
    private void SortTransitions()
    {
        //insertion sort for the nodes.
        //for each node, we check the nodes before it until we have found one that is less than it. once this happens we remove the node from the position we were currently at
        if (transitions.Count <= 1)
            return;
        
        for(int i = 1; i < transitions.Count; i++)
        {
            int j = i;
            while (j > 0 && transitions[i].Priority > transitions[j].Priority) { j--; };

            FiniteStateMachineTransition temp = transitions[i];

            transitions.RemoveAt(i);
            transitions.Insert(j, temp);
        }
    }

    public void AddTransition(string name, Func<bool> condition, int priority, FiniteStateMachineNode end)
    {
        transitions.Add(new FiniteStateMachineTransition(name, this, end, condition, priority));
        SortTransitions();
    }

    public void Execute()
    {
        action();
    }

    public override string ToString()
    {
        string s = string.Empty;

        foreach(FiniteStateMachineTransition t in transitions)
        {
            s += t.ToString() + " ";
        }

        return s;
    }
}

//class for creating a finite state machine for decision making
public class FiniteStateMachine : DecisionTool
{
    FiniteStateMachineNode currentNode;

    public FiniteStateMachine(FiniteStateMachineNode start_node)
    {
        currentNode = start_node;
    }

    public override bool Evaluate()
    {
        //this method is called to do a potential state transition
        foreach(FiniteStateMachineTransition t in currentNode.Transitions)
        {
            //if the condition is satisfied, we move to that transitions end node
            if(t.Evaluate())
            {
                currentNode = t.End;
                break;
            }
        }

        //then execute the action at the current node
        try
        {
            currentNode.Execute();
        }

        catch
        {
            return false;
        }

        return true;
    }
}

#endregion