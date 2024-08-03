using System.Net;
using WebServer;
public class CalculusMethods
{
	public static readonly Func<HttpListenerRequest, ResponseInformation>[] Functions = { CalculusDemo };
	public static ResponseInformation CalculusDemo(HttpListenerRequest req)
	{
		CalculusMethods demo = new CalculusMethods(); 
		return new ResponseInformation(req, demo.response);
	}


	public static float F(float x) => x*x*x;
		
	Dictionary<string, string> response = new Dictionary<string, string>();
	public CalculusMethods()
	{
		Console.WriteLine("Running!");
		response.Add("Function", "x^3");
		response.Add("(Approximate) Derivative at x = 1", ApproxDerive(1).ToString());
		response.Add("(Approximate) Integral of f([0,1])", ApproxIntegrate(0, 1).ToString());
	}

	private static readonly float c = 0.0001f;
	public float ApproxDerive(int x)
    {
		float f1 = F(x + c), f2 = F(x);
		return (f1 - f2) / c;
    }

	public static readonly int NUM_DIVS = 1000;

	/// <summary>
	/// Approximates the integral from a to b. 
	/// </summary>
	/// <param name="a">The lower bound.</param>
	/// <param name="b">The upper bound.</param>
	/// <returns>The approximate integral between a and b.</returns>
	public float ApproxIntegrate(int a, int b)
    {
		// Get Areas when using a left-side bound: 
			// Create a list of values between x and e.
			float[] A1 = new float[NUM_DIVS];
			float LengthOfEachSection = (b - a) / (float) NUM_DIVS;
			for (int i = 0; i < NUM_DIVS; i++)
			{
				A1[i] = F(a + LengthOfEachSection * i) * LengthOfEachSection;
			}

			// Create a list of values between x and e.
			float[] A2 = new float[NUM_DIVS];
			for (int i = 0; i < NUM_DIVS - 1; i++)
			{
				A2[i] = F((a + LengthOfEachSection) + LengthOfEachSection * i) * LengthOfEachSection;
			}

		// Average values.
		float integral = Helpers.SumArray(A1) + Helpers.SumArray(A2);
		return integral / 2f;
    }
	public static float[] CoterminalAngles(float angle)
	{
		List<float> angles = new List<float>();
		float increment = 360 * (angle > 0 ? 1 : -1);
		do
		{
			angles.Add(angle += increment);
		} while (angle <= 360);

		return angles.ToArray();
	}


}

namespace FunctionParser
{
	class FunctionParser
    {
		public static Func<float, float> ParseFunction(string func)
		{
			// Split the function into ranked parts by how soon they must be parsed. (eg, stuff in parenthesis is parsed first.)
			Node node = Node.ParseToNode(func);
			return new Func<float, float>(node.Evaluate);
		}
	}

	class Node
	{
		public Node[] contents;
		public string Value;

		public Node(Node[] contents, string value)
        {
            this.contents = contents;
            Value = value;
        }

		/// <summary>
		/// Evaluates the function at x = a.
		/// </summary>
		/// <param name="a">The x value to be passed.</param>
		/// <returns>The y value at the specified x value.</returns>
		public float Evaluate(float a)
        {
			// Get the values from the contained nodes, and replace that value in as the text, then add/subtract/whatever.
			string ReplacedString = $"{Value}"; // Deep copy
			for (int i = 0; i < contents.Length; i++) ReplacedString.Replace(contents[i].Value.ToString(), contents[i].Evaluate(a).ToString());
			// Put in the value for x and remove all spaces.
			ReplacedString = ReplacedString.Replace("x", a.ToString()).Replace(" ", "");
			Console.WriteLine("Function: " + ReplacedString);

			// Do PEMDAS.
				// Handle addition by adding left to right. (Keep processing until there are no +'s)
			do
			{
				int PlusLocation = Helpers.GetCharacterIndexesInString('+', ReplacedString)[0];
				// Add the two numbers on both sides together. 
				float LeftNumber = GetNumberToLeft(ReplacedString, PlusLocation);
				float RightNumber = GetNumberToRight(ReplacedString, PlusLocation);
				float total = LeftNumber + RightNumber;

				// Replace the whole chunk with the new value.
				string chunk = LeftNumber.ToString() + '+' + RightNumber.ToString();
				ReplacedString = ReplacedString.Replace(chunk, total.ToString());
			} while (Helpers.GetCharacterIndexesInString('+', ReplacedString).Length > 0);

			return float.Parse(TrimParenthesis(ReplacedString));
        }

		private static string TrimParenthesis(string s)
        {
			// Remove all parenthesis and return.
			foreach (char c in ParenthesisEnds) s = s.Replace(c.ToString(), String.Empty);
			foreach (char c in ParenthesisStarts) s = s.Replace(c.ToString(), String.Empty);

			return s;
        }

		//private static float GetNumberToRight(string searchString, int startSearchIndex) => GetNumberToSide(1, searchString, startSearchIndex);
		private static float GetNumberToLeft(string searchString, int startSearchIndex) => float.Parse(Helpers.ReverseString(GetNumberToRight(Helpers.ReverseString(searchString), startSearchIndex).ToString()));

		//TODO: Write this.
		private static float GetNumberToRight(string searchString, int startSearchIndex)
        {
			// Move through the list from the start to the back, until we have all contigious digits.
			string digits = "";
			char[] ValidValues = ".0123456789".ToCharArray();
			for (int i = startSearchIndex + 1; Helpers.ArrayContains(ValidValues, searchString[i], out _); i++)
					digits += searchString[i];

			digits = digits.Trim();
			return float.Parse(digits);
        }


		private static readonly char[] ParenthesisStarts = new char[] { '{', '[', '(' };
		private static readonly char[] ParenthesisEnds = new char[] { '}', ']', ')' };
		public static Node ParseToNode(string node)
		{
			char[] characters = node.ToCharArray();
			// Get text from each parenthetical, call this method on it.
				// Ensure that there are an equal number of parenthesis for starting and ending.
			if (!ParentheticalCounter.IsStringBalanced(node)) throw new Exception("A node is unbalanced!");

			List<Node> InsideNodes = new List<Node>();
			for (int i = 0; i < node.Length; i++)
            {
                if (Helpers.ArrayContains(ParenthesisStarts, characters[i], out int ParenthesisIndexInStartList))
                {
					// This character is the start of a parenthetical, find the end.
					Console.WriteLine($"Index of starter: {ParenthesisIndexInStartList} Starter: {ParenthesisStarts[ParenthesisIndexInStartList]} Ender: {ParenthesisEnds[ParenthesisIndexInStartList]}");
                    char End = ParenthesisEnds[ParenthesisIndexInStartList];
                    FindClosingParenthesis(End, i, node, ParenthesisStarts[ParenthesisIndexInStartList], out int length);
					// Extract the parenthetical.
					Console.WriteLine("Length: " + length);
					string Parenthetical = node.Substring(i, length);
					
					if (!Parenthetical.Equals(node))
						InsideNodes.Add(ParseToNode(Parenthetical));

					Console.WriteLine("Parenthetical: " + Parenthetical);
                }
            }

			return new Node(InsideNodes.ToArray(), node);
		}

		private static int FindClosingParenthesis(char ClosingParenthesis, int startIndex, string searchString, char DepthIncreaseCharacter, out int length)
        {
            char[] NodeText = searchString.ToCharArray();
			int DepthCounter = 0;
			// Increase startIndex by one in order to begin searching at the next character over.
			for (int i = startIndex + 1; i < searchString.Length; i++)
			{
				// Console.WriteLine($"Character: {NodeText[i]}, Depth: {DepthCounter}");
				if (NodeText[i] == ClosingParenthesis && DepthCounter == 0)
				{
					length = i - startIndex + 1;
					return i;
				}
				else if (NodeText[i] == ClosingParenthesis) DepthCounter--;
				else if (NodeText[i] == DepthIncreaseCharacter) DepthCounter++;
			}
			length = -1;
			return -1;
        }

		private class ParentheticalCounter
        {
			public int starts, ends;
			public ParentheticalCounter(char[] characters, string StringToCount)
            {
                starts = 0;
                ends = 0;

				// Determine if the passed string is balanced.
				foreach (char character in StringToCount.ToCharArray())
                {
					if (character == characters[0]) starts++;
					else if (character == characters[1]) ends++; 
                }
            }

			bool IsBalanced() => starts == ends;

            private static readonly char[][] Parenthesis = new char[][] { new char[] { '(', ')' }, new char[] { '[', ']' }, new char[] { '{', '}' } };
			public static bool IsStringBalanced(string node)
            {
				// Run through counters.
				List<ParentheticalCounter> counters = new List<ParentheticalCounter>();
				foreach (char[] chars in Parenthesis) if (!new ParentheticalCounter(chars, node).IsBalanced()) return false;
				return true;
            }
		}
	}
}