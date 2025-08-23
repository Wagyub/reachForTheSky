// C#

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.UIElements;

public class SessionsUIController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;

    private readonly List<SessionItem> _items = new();
    private Button _createBtn, _joinBtn, _refreshBtn;

    private ListView _listView;
    private IntegerField _maxField;
    private TextField _nameField;

    private void Awake()
    {
        if (uiDocument == null) uiDocument = GetComponent<UIDocument>();
    }

    private void OnEnable()
    {
        var root = uiDocument.rootVisualElement;

        _listView = root.Q<ListView>("sessions-list");
        _createBtn = root.Q<Button>("create-btn");
        _joinBtn = root.Q<Button>("join-btn");
        _refreshBtn = root.Q<Button>("refresh-btn");
        _nameField = root.Q<TextField>("session-name");
        _maxField = root.Q<IntegerField>("session-max");

        // Init ListView
        _listView.itemsSource = _items;
        _listView.selectionType = SelectionType.Single;
        _listView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
        _listView.fixedItemHeight = 58;
        _listView.makeItem = MakeRow;
        _listView.bindItem = BindRow;

        // Boutons
        _createBtn.clicked += OnCreateClicked;
        _joinBtn.clicked += OnJoinClicked;
        _refreshBtn.clicked += OnRefreshClicked;

        if (_maxField.value <= 0) _maxField.value = 4;

        _ = RefreshAsync();
    }

    private void OnDisable()
    {
        _createBtn.clicked -= OnCreateClicked;
        _joinBtn.clicked -= OnJoinClicked;
        _refreshBtn.clicked -= OnRefreshClicked;
    }

    private VisualElement MakeRow()
    {
        var row = new VisualElement { name = "row" };
        row.style.flexDirection = FlexDirection.Row;
        row.style.justifyContent = Justify.SpaceBetween;

        var title = new Label { name = "title" };
        var count = new Label { name = "count" };

        row.Add(title);
        row.Add(count);
        return row;
    }

    private void BindRow(VisualElement row, int i)
    {
        if (i < 0 || i >= _items.Count) return;
        var s = _items[i];
        row.Q<Label>("title").text = string.IsNullOrEmpty(s.Name) ? s.Id : s.Name;
        row.Q<Label>("count").text = $"{s.CurrentPlayers}/{s.MaxPlayers}";
    }

    private async void OnCreateClicked()
    {
        Debug.Log(_nameField.value.Trim());
        var name = string.IsNullOrWhiteSpace(_nameField.value) ? "MyLobby" : _nameField.value.Trim();
        var max = Mathf.Max(1, _maxField.value);

        try
        {
            var options = new SessionOptions { Name = name, MaxPlayers = max };
            var session = await MultiplayerService.Instance.CreateSessionAsync(options);
            Debug.Log($"Session créée: {session.Id}");
            await RefreshAsync();
        }
        catch (Exception e)
        {
            Debug.LogError($"Échec création: {e.Message}");
        }
    }

    private async void OnJoinClicked()
    {
        if (_listView.selectedIndex < 0 || _listView.selectedIndex >= _items.Count)
        {
            Debug.LogWarning("Sélectionne une session d’abord.");
            return;
        }

        var selected = _items[_listView.selectedIndex];
        try
        {
            var joined = await MultiplayerService.Instance.JoinSessionByIdAsync(selected.Id);
            Debug.Log($"Rejoint: {joined.Id}");
            await RefreshAsync();
        }
        catch (Exception e)
        {
            Debug.LogError($"Échec join: {e.Message}");
        }
    }

    private async void OnRefreshClicked()
    {
        await RefreshAsync();
    }

    private async Task RefreshAsync()
    {
        try
        {
            var result = await MultiplayerService.Instance.QuerySessionsAsync(new QuerySessionsOptions());

            _items.Clear();
            if (result?.Sessions != null)
                foreach (var s in result.Sessions)
                    _items.Add(new SessionItem
                    {
                        Id = s.Id,
                        Name = TryGetString(s, "Name") ?? TryGetString(s, "DisplayName") ?? s.Id,
                        MaxPlayers = TryGetInt(s, "MaxPlayers", 0),
                        AvailableSlots = TryGetInt(s, "AvailableSlots", 0)
                    });

            _listView.Rebuild(); // ou RefreshItems()
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception e)
        {
            Debug.LogError($"Échec refresh: {e.Message}");
            _items.Clear();
            _listView.Rebuild();
        }
    }

    // Helpers robustes si les propriétés varient selon la version du SDK
    private static string TryGetString(object obj, string prop)
    {
        try
        {
            var p = obj.GetType().GetProperty(prop);
            return p?.GetValue(obj) as string;
        }
        catch
        {
            return null;
        }
    }

    private static int TryGetInt(object obj, string prop, int fallback)
    {
        try
        {
            var p = obj.GetType().GetProperty(prop);
            if (p == null) return fallback;
            var v = p.GetValue(obj);
            return v is int i ? i : Convert.ToInt32(v);
        }
        catch
        {
            return fallback;
        }
    }

    private class SessionItem
    {
        public int AvailableSlots;
        public string Id;
        public int MaxPlayers;
        public string Name;
        public int CurrentPlayers => Mathf.Clamp(MaxPlayers - AvailableSlots, 0, MaxPlayers);
    }
}