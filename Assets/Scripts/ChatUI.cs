using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChatUI : MonoBehaviour
{
    [Header("聊天面板")]
    public GameObject chatPanel;
    public Button toggleChatButton;

    [Header("消息显示")]
    public ScrollRect chatScrollRect;
    public TextMeshProUGUI chatContent;
    public GameObject messagePrefab;
    public Transform messageContainer;

    [Header("输入区域")]
    public TMP_InputField messageInput;
    public Button sendButton;
    public TMP_Dropdown channelDropdown;

    [Header("玩家列表")]
    public GameObject playerListPanel;
    public Transform playerListContainer;
    public GameObject playerListItemPrefab;

    [Header("设置")]
    public bool autoScroll = true;
    public bool showChatOnMessage = true;
    public float fadeOutTime = 5f;

    private float lastMessageTime;
    private bool isChatVisible = true;

    void Start()
    {
        SetupUI();
        SubscribeToEvents();
    }

    void SetupUI()
    {
        if (toggleChatButton != null)
            toggleChatButton.onClick.AddListener(ToggleChat);

        if (sendButton != null)
            sendButton.onClick.AddListener(SendMessage);

        if (messageInput != null)
            messageInput.onSubmit.AddListener(OnInputSubmit);

        if (channelDropdown != null)
        {
            channelDropdown.ClearOptions();
            channelDropdown.AddOptions(new System.Collections.Generic.List<string>
            {
                "全局",
                "团队",
                "悄悄话"
            });
        }
    }

    void SubscribeToEvents()
    {
        if (ChatSystem.instance != null)
        {
            ChatSystem.instance.OnMessageReceived += OnMessageReceived;
            ChatSystem.instance.OnPlayerNameChanged += OnPlayerNameChanged;
        }
    }

    void Update()
    {
        HandleInput();
        HandleFadeOut();
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (!messageInput.isFocused)
            {
                messageInput.ActivateInputField();
                messageInput.Select();
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (messageInput.isFocused)
            {
                messageInput.DeactivateInputField();
            }
            else
            {
                ToggleChat();
            }
        }
    }

    void HandleFadeOut()
    {
        if (!isChatVisible) return;

        if (Time.time - lastMessageTime > fadeOutTime && !messageInput.isFocused)
        {
            if (chatPanel != null)
            {
                CanvasGroup canvasGroup = chatPanel.GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, 0.3f, Time.deltaTime * 2f);
                }
            }
        }
    }

    // ===== 聊天控制 =====
    public void ToggleChat()
    {
        isChatVisible = !isChatVisible;

        if (chatPanel != null)
        {
            chatPanel.SetActive(isChatVisible);
        }

        if (playerListPanel != null)
        {
            playerListPanel.SetActive(isChatVisible);
        }
    }

    public void ShowChat()
    {
        isChatVisible = true;

        if (chatPanel != null)
        {
            chatPanel.SetActive(true);
            CanvasGroup canvasGroup = chatPanel.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }
        }

        lastMessageTime = Time.time;
    }

    // ===== 发送消息 =====
    void SendMessage()
    {
        if (string.IsNullOrEmpty(messageInput.text)) return;

        string message = messageInput.text.Trim();
        messageInput.text = "";

        if (ChatSystem.instance == null)
        {
            Debug.LogWarning("ChatSystem not found");
            return;
        }

        int channelIndex = channelDropdown != null ? channelDropdown.value : 0;

        switch (channelIndex)
        {
            case 0:
                ChatSystem.instance.SendGlobalMessage(message);
                break;
            case 1:
                ChatSystem.instance.SendTeamMessage(message);
                break;
            case 2:
                // 悄悄话需要选择目标玩家
                break;
        }

        messageInput.ActivateInputField();
    }

    void OnInputSubmit(string text)
    {
        if (!string.IsNullOrEmpty(text))
        {
            SendMessage();
        }
    }

    // ===== 消息接收 =====
    void OnMessageReceived(ChatMessage message)
    {
        AddMessageToUI(message);

        if (showChatOnMessage)
        {
            ShowChat();
        }
    }

    void AddMessageToUI(ChatMessage message)
    {
        if (messageContainer != null && messagePrefab != null)
        {
            GameObject messageObj = Instantiate(messagePrefab, messageContainer);
            TextMeshProUGUI messageText = messageObj.GetComponent<TextMeshProUGUI>();

            if (messageText != null)
            {
                messageText.text = ChatSystem.instance.FormatMessage(message);
            }
        }

        if (chatContent != null)
        {
            chatContent.text += ChatSystem.instance.FormatMessage(message) + "\n";
        }

        if (autoScroll)
        {
            ScrollToBottom();
        }

        lastMessageTime = Time.time;
    }

    void ScrollToBottom()
    {
        if (chatScrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            chatScrollRect.verticalNormalizedPosition = 0f;
        }
    }

    // ===== 玩家列表 =====
    void OnPlayerNameChanged(ulong playerId, string playerName)
    {
        UpdatePlayerList();
    }

    void UpdatePlayerList()
    {
        if (ChatSystem.instance == null || playerListContainer == null) return;

        // 清除现有列表
        foreach (Transform child in playerListContainer)
        {
            Destroy(child.gameObject);
        }

        // 添加新玩家
        var playerNames = ChatSystem.instance.GetAllPlayerNames();
        foreach (var kvp in playerNames)
        {
            if (playerListItemPrefab != null)
            {
                GameObject playerObj = Instantiate(playerListItemPrefab, playerListContainer);
                TextMeshProUGUI playerText = playerObj.GetComponentInChildren<TextMeshProUGUI>();

                if (playerText != null)
                {
                    playerText.text = kvp.Value;
                }

                // 添加点击事件
                Button playerButton = playerObj.GetComponent<Button>();
                if (playerButton != null)
                {
                    ulong playerId = kvp.Key;
                    playerButton.onClick.AddListener(() => OnPlayerClick(playerId));
                }
            }
        }
    }

    void OnPlayerClick(ulong playerId)
    {
        if (channelDropdown != null)
        {
            channelDropdown.value = 2;
        }
    }

    // ===== 公共方法 =====
    public void ClearChat()
    {
        if (chatContent != null)
        {
            chatContent.text = "";
        }

        if (messageContainer != null)
        {
            foreach (Transform child in messageContainer)
            {
                Destroy(child.gameObject);
            }
        }

        if (ChatSystem.instance != null)
        {
            ChatSystem.instance.ClearChatHistory();
        }
    }

    public void LoadHistory()
    {
        if (ChatSystem.instance == null) return;

        var history = ChatSystem.instance.GetChatHistory();
        foreach (var message in history)
        {
            AddMessageToUI(message);
        }
    }

    void OnDestroy()
    {
        if (ChatSystem.instance != null)
        {
            ChatSystem.instance.OnMessageReceived -= OnMessageReceived;
            ChatSystem.instance.OnPlayerNameChanged -= OnPlayerNameChanged;
        }
    }
}
