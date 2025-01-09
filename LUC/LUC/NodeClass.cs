using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LUC
{
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

        public void AddChild(string data)
        {
            children.AddFirst(new TreeNode<string>(data));
        }

        public void AddChild(TreeNode<string> tree)
        {
            children.AddFirst(tree);
        }

        public TreeNode<string> GetChild(int i)
        {
            foreach (TreeNode<string> n in children)
                if (--i == 0)
                    return n;
            return null;
        }

        public List<TreeNode<string>> GetAllChild()
        {
            List<TreeNode<string>> alltrees = new List<TreeNode<string>> { };

            foreach (TreeNode<string> n in children)
            {
                alltrees.Add(n);
            }

            return alltrees;
        }

        public void Traverse(TreeNode<string> node, TreeVisitor<string> visitor)
        {
            visitor(node.data);
            foreach (TreeNode<string> kid in node.children)
                Traverse(kid, visitor);
        }

        public string Data
        {
            get { return data; }
        }
    }
}
