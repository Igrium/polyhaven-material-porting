using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolyHaven.Util;

public static class RandomCharacters
{
	public static readonly string INDENT_ALLOWED_CHARACTERS = "abcdefghijklmnopqrstuvwxyz-_";
	public static IEnumerable<string> NextStrings(this Random rnd, int length, int count, string allowedChars )
	{
		ISet<string> usedRandomStrings = new HashSet<string>();
		char[] chars = new char[length];
		int setLength = allowedChars.Length;

		while ( count-- > 0 )
		{

			for ( int i = 0; i < length; ++i )
			{
				chars[i] = allowedChars[rnd.Next( length )];
			}

			string randomString = new string( chars, 0, length );

			if ( usedRandomStrings.Add( randomString ) )
			{
				yield return randomString;
			}
			else
			{
				count++;
			}
		}
	}
}
