using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeSnake.Mother3.Text
{
    public class ReverseLookup
    {
        private class LookupNode
        {
            internal Dictionary<Context, short> Values = new Dictionary<Context, short>();
            internal Dictionary<char, LookupNode> Children = new Dictionary<char, LookupNode>();
            internal LookupNode Parent = null;
            internal int Depth = 0;
        }

        private Dictionary<char, LookupNode> tree = new Dictionary<char, LookupNode>();

        public ReverseLookup(IEnumerable<KeyValuePair<short, ContextString>> charLookup)
        {
            var unique = charLookup.Where(cs => cs.Value.String.Length > 0)
                .GroupBy(cs => cs.Value)
                .Select(g => g.First());

            foreach (var kv in unique)
            {
                short value = kv.Key;
                ContextString cs = kv.Value;
                char first = cs.String[0];

                LookupNode root;

                if (tree.ContainsKey(first))
                {
                    root = tree[first];
                }
                else
                {
                    root = new LookupNode();
                    tree.Add(cs.String[0], root);
                }

                root.Depth = 1;

                int currentIndex = 1;

                while (currentIndex < cs.String.Length)
                {
                    char currentChar = cs.String[currentIndex];

                    LookupNode node;

                    if (root.Children.ContainsKey(currentChar))
                    {
                        node = root.Children[currentChar];
                    }
                    else
                    {
                        node = new LookupNode();
                        root.Children.Add(currentChar, node);
                    }

                    node.Parent = root;
                    root = node;
                    currentIndex++;
                    root.Depth = currentIndex;
                }

                root.Values.Add(cs.Context, value);
            }
        }

        public short? Find(string str, int startingIndex, Context context, out int matchedLength)
        {
            matchedLength = 0;

            if (startingIndex >= str.Length)
                return null;

            if (!tree.ContainsKey(str[startingIndex]))
                return null;

            LookupNode node = tree[str[startingIndex++]];
            LookupNode bestMatch;

            if (node.Values.Count > 0 &&
                (node.Values.ContainsKey(context) || node.Values.ContainsKey(Context.None)))
            {
                bestMatch = node;
            }
            else
            {
                bestMatch = null;
            }

            matchedLength++;

            while (startingIndex < str.Length &&
                node.Children.ContainsKey(str[startingIndex]))
            {
                node = node.Children[str[startingIndex++]];

                if (node.Values.Count > 0 &&
                    (node.Values.ContainsKey(context) || node.Values.ContainsKey(Context.None)))
                {
                    bestMatch = node;
                }
            }

            if (bestMatch != null)
            {
                if (bestMatch.Values.Count > 0)
                {
                    if (bestMatch.Values.ContainsKey(context))
                    {
                        matchedLength = bestMatch.Depth;
                        return bestMatch.Values[context];
                    }
                    else if (bestMatch.Values.ContainsKey(Context.None))
                    {
                        matchedLength = bestMatch.Depth;
                        return bestMatch.Values[Context.None];
                    }
                }
            }

            matchedLength = 0;
            return null;
        }
    }
}
