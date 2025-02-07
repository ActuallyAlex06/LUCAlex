using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LUC
{
    //Class to create, maintain and call the syntax tree used in the syntax analysis.
    delegate void TreeVisitor<String>(string nodeData);

    class TreeNode<String>
    {
        private string data;
        private LinkedList<TreeNode<string>> children;

        public TreeNode(string data)
        {
            this.data = data;
            children = new LinkedList<TreeNode<string>>();
        }

        //Children can be strings and Treenodes, Treenodes are used for Nonterminals
        public void AddChild(string data)
        {
            children.AddLast(new TreeNode<string>(data));
        }

        public void AddChild(TreeNode<string> tree)
        {
            children.AddLast(tree);
        }

        //Call als children of a spcific root and returns them
        public List<TreeNode<string>> GetAllChild()
        {
            List<TreeNode<string>> alltrees = new List<TreeNode<string>> { };

            foreach (TreeNode<string> n in children)
            {
                alltrees.Add(n);
            }

            return alltrees;
        }

        public string Data
        {
            get { return data; }
        }
    }
}
