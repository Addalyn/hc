using I2.Loc;
using LobbyGameClientMessages;
using System;
using System.Collections.Generic;
using System.Threading;

public class TextConsole
{
    public struct Message
    {
        public string Text;

        public ConsoleMessageType MessageType;

        public CharacterType CharacterType;

        public bool DisplayDevTag;

        public long SenderAccountId;

        public string SenderHandle;

        public Team SenderTeam;

        public string RecipientHandle;

        public Team RestrictVisibiltyToTeam;
    }

    public struct AllowedEmojis
    {
        public List<int> emojis;
    }

    private static TextConsole s_instance;

    public string LastWhisperSenderHandle
    {
        get;
        private set;
    }

    private Action<Message, AllowedEmojis> OnMessageHolder;
    public event Action<Message, AllowedEmojis> OnMessage
    {
        add
        {
            Action<Message, AllowedEmojis> action = this.OnMessageHolder;
            Action<Message, AllowedEmojis> action2;
            do
            {
                action2 = action;
                action = Interlocked.CompareExchange(ref this.OnMessageHolder, (Action<Message, AllowedEmojis>)Delegate.Combine(action2, value), action);
            }
            while ((object)action != action2);
            while (true)
            {
                return;
            }
        }
        remove
        {
            Action<Message, AllowedEmojis> action = this.OnMessageHolder;
            Action<Message, AllowedEmojis> action2;
            do
            {
                action2 = action;
                action = Interlocked.CompareExchange(ref this.OnMessageHolder, (Action<Message, AllowedEmojis>)Delegate.Remove(action2, value), action);
            }
            while ((object)action != action2);
        }
    }

    public TextConsole()
    {
        this.OnMessageHolder = delegate
        {
        };

        ClientGameManager.Get().OnChatNotification += HandleChatNotification;
    }

    public static TextConsole Get()
    {
        return s_instance;
    }

    public static void Instantiate()
    {
        s_instance = new TextConsole();
    }

    public void Write(Message message, List<int> EmojisAllowed = null)
    {
        AllowedEmojis allowedEmojis = default(AllowedEmojis);
        allowedEmojis.emojis = EmojisAllowed;
        this.OnMessageHolder(message, allowedEmojis);
        UITextConsole.StoreMessage(message, allowedEmojis);
    }

    public void Write(string text, ConsoleMessageType messageType = ConsoleMessageType.SystemMessage)
    {
        Message message = default(Message);
        message.MessageType = messageType;
        message.Text = text;
        Write(message);
    }

    public string RemoveRichTextTags(string theString)
    {
        string text = theString;
        if (text.IndexOf('<') != -1)
        {
            if (text.IndexOf('>') != -1)
            {
                text = text.Replace("<", "< ");
            }
        }
        return text;
    }

    public void OnInputSubmitted(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return;
        }
        bool isInGame = GameManager.Get() != null
                    && GameManager.Get().GameInfo != null
                    && GameManager.Get().GameInfo.GameStatus != GameStatus.Stopped;
        input = ChatEmojiManager.Get().UnlocalizeEmojis(input);
#if VANILLA
		input = RemoveRichTextTags(input);
#else
        ClientGameManager clientGameManager = ClientGameManager.Get();

        if (!HydrogenConfig.Get().AllowChatTags || clientGameManager == null || clientGameManager.ClientAccessLevel != ClientAccessLevel.Admin)
        {
            input = RemoveRichTextTags(input);
        }
#endif
        string arguments;
        string command;
        if (input[0] == '/')
        {
            string[] array = input.Split((string[])null, 2, StringSplitOptions.RemoveEmptyEntries);
            if (array.Length >= 2)
            {
                command = array[0];
                arguments = array[1];
            }
            else
            {
                command = input;
                arguments = string.Empty;
            }
        }
        else
        {
            command = isInGame ? "/team" : "/global";
            arguments = input;
        }

        command = command.Trim();
        if (SlashCommands.Get().RunSlashCommand(command, arguments))
        {
            return;
        }
#if VANILLA
        if (DebugCommands.Get() != null)
        {
            DebugCommands.Get().RunDebugCommand(command, arguments);
        }
#else
        if (DebugCommands.Get() != null)
        {
            if (!(clientGameManager == null))
            {
                if (clientGameManager.ClientAccessLevel == ClientAccessLevel.Admin)
                {
                    DebugCommands.Get().RunDebugCommand(command, arguments);
                }
            }
        }
#endif
    }

    public void HandleSetDevTagResponse(SetDevTagResponse response)
    {
        string empty = string.Empty;
        if (response.Success)
        {
            empty = "Success";
        }
        else
        {
            empty = "Failed";
        }
        Write(new Message
        {
            Text = empty,
            MessageType = ConsoleMessageType.SystemMessage
        });
    }

    public void HandleChatNotification(ChatNotification notification)
    {
        if (Options_UI.Get() != null)
        {
            if (Options_UI.Get().GetEnableProfanityFilter() && notification.ConsoleMessageType != ConsoleMessageType.BroadcastMessage)
            {
                notification.Text = BannedWords.Get().FilterPhrase(notification.Text, LocalizationManager.CurrentLanguageCode);
            }
        }
        Write(new Message
        {
            Text = ((notification.LocalizedText == null) ? notification.Text : notification.LocalizedText.ToString()),
            MessageType = notification.ConsoleMessageType,
            SenderAccountId = notification.SenderAccountId,
            SenderHandle = notification.SenderHandle,
            SenderTeam = notification.SenderTeam,
            RecipientHandle = notification.RecipientHandle,
            CharacterType = notification.CharacterType,
            DisplayDevTag = notification.DisplayDevTag
        }, notification.EmojisAllowed);
        if (notification.ConsoleMessageType == ConsoleMessageType.WhisperChat)
        {
            ClientGameManager clientGameManager = ClientGameManager.Get();
            if (!(clientGameManager == null))
            {
                if (!(clientGameManager.Handle != notification.SenderHandle))
                {
                    goto IL_0148;
                }
            }
            LastWhisperSenderHandle = notification.SenderHandle;
        }
        goto IL_0148;
    IL_0148:
        if (notification.ConsoleMessageType == ConsoleMessageType.BroadcastMessage)
        {
            SystemMenuBroadcast.Get().DisplaySystemMessage(notification);
        }
    }
}
