namespace TMPro
{
	public struct TMP_LinkInfo
	{
		public TMP_Text textComponent;

		public int hashCode;

		public int linkIdFirstCharacterIndex;

		public int linkIdLength;

		public int linkTextfirstCharacterIndex;

		public int linkTextLength;

		internal char[] linkID;

		internal void SetLinkID(char[] text, int startIndex, int length)
		{
			if (linkID != null)
			{
				while (true)
				{
					switch (3)
					{
					case 0:
						continue;
					}
					break;
				}
				if (1 == 0)
				{
					/*OpCode not supported: LdMemberToken*/;
				}
				if (linkID.Length >= length)
				{
					goto IL_003c;
				}
				while (true)
				{
					switch (6)
					{
					case 0:
						continue;
					}
					break;
				}
			}
			linkID = new char[length];
			goto IL_003c;
			IL_003c:
			for (int i = 0; i < length; i++)
			{
				linkID[i] = text[startIndex + i];
			}
			while (true)
			{
				switch (1)
				{
				default:
					return;
				case 0:
					break;
				}
			}
		}

		public string GetLinkText()
		{
			string text = string.Empty;
			TMP_TextInfo textInfo = textComponent.textInfo;
			for (int i = linkTextfirstCharacterIndex; i < linkTextfirstCharacterIndex + linkTextLength; i++)
			{
				text += textInfo.characterInfo[i].character;
			}
			while (true)
			{
				switch (6)
				{
				case 0:
					continue;
				}
				if (1 == 0)
				{
					/*OpCode not supported: LdMemberToken*/;
				}
				return text;
			}
		}

		public string GetLinkID()
		{
			if (textComponent == null)
			{
				while (true)
				{
					switch (2)
					{
					case 0:
						break;
					default:
						if (1 == 0)
						{
							/*OpCode not supported: LdMemberToken*/;
						}
						return string.Empty;
					}
				}
			}
			return new string(linkID, 0, linkIdLength);
		}
	}
}
