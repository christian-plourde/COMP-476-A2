using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public abstract class DecisionTreeNode
{
    DecisionTreeNode parent;
    DecisionTreeNode leftChild;
    DecisionTreeNode rightChild;

    public DecisionTreeNode Left
    {
        get { return leftChild; }
        set { leftChild = value;
            leftChild.parent = this;
        }
    }

    public DecisionTreeNode Right
    {
        get { return rightChild; }
        set { rightChild = value;
            rightChild.parent = this;
        }
    }

    public bool IsLeaf
    {
        get { return Left == null && Right == null; }
    }

    public DecisionTreeNode()
    {
            
    }

    public abstract bool Evaluate();
}

public class CheckNode : DecisionTreeNode
{
    Func<bool> condition;

    public CheckNode(Func<bool> param)
    {
        this.condition = param;
    }

    public override bool Evaluate()
    {
        return condition();
    }
}

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
            action();
        }

        catch
        {
            return false;
        }
            
        return true;
    }
}

public class DecisionTree
{
    DecisionTreeNode root;

    public DecisionTree(DecisionTreeNode root)
    {
        this.root = root;
    }

    public bool Evaluate()
    {
        bool res = false;
        Evaluate(root, ref res);
        return res;
    }

    private void Evaluate(DecisionTreeNode n, ref bool res)
    {
        if (n.IsLeaf)
        {
            res = n.Evaluate();
            return;
        }

        //if evaluation of node is true go right (right for success, left for failure)
        if (n.Evaluate())
            Evaluate(n.Right, ref res);

        else
            Evaluate(n.Left, ref res);
    }
}
