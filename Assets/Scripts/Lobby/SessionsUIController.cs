// C#

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.UIElements;

public class SessionsUIController : MonoBehaviour
{
    private const float _minRefreshIntervalSeconds = 1.5f;
    [SerializeField] private UIDocument uiDocument;

    private readonly List<SessionItem> _items = new();
    private Button _createBtn, _joinBtn, _refreshBtn;
    private bool _isCreating; // Empêche la création concurrente
    private bool _isJoining; // Empêche les joins concurrents
    private bool _isLeaving;

    // Anti-bombardement de refresh: single-flight + throttling
    private bool _isRefreshing;
    private float _lastRefreshTime = -999f;

    private ListView _listView;
    private IntegerField _maxField;
    private TextField _nameField;
    private ISession currentJoinedSession;
    private string prevSessionId;
    private string prevSessionName;

    private void Awake()
    {
        if (uiDocument == null) uiDocument = GetComponent<UIDocument>();
    }

    private void Update()
    {
        // Éviter de muter l’UI ici; tout se fait dans BindRow/Refresh
    }

    private async void OnEnable()
    {
        var root = uiDocument.rootVisualElement;

        _listView = root.Q<ListView>("sessions-list");
        _createBtn = root.Q<Button>("create-btn");
        _refreshBtn = root.Q<Button>("refresh-btn");
        _nameField = root.Q<TextField>("session-name");
        _maxField = root.Q<IntegerField>("session-max");

        // Init ListView
        _listView.itemsSource = _items;
        _listView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
        _listView.fixedItemHeight = 58;
        // Si un itemTemplate est assigné dans l’UI Builder, on l’utilise pour créer les items.
        _listView.makeItem = () =>
        {
            var tpl = _listView.itemTemplate;
            return tpl != null ? tpl.Instantiate() : new VisualElement();
        };
        _listView.bindItem = BindRow;

        // Boutons
        _createBtn.clicked += OnCreateClicked;
        _refreshBtn.clicked += OnRefreshClicked;

        if (_maxField.value <= 0) _maxField.value = 4;

        UpdateCreateButtonState();

        // Premier remplissage
        await RefreshAsync(true);
    }

    private void OnDisable()
    {
        _createBtn.clicked -= OnCreateClicked;
        _refreshBtn.clicked -= OnRefreshClicked;
    }

    private void BindRow(VisualElement row, int i)
    {
        if (i < 0 || i >= _items.Count || row == null) return;

        var s = _items[i];

        var title = row.Q<Label>("title");
        var count = row.Q<Label>("count");
        var joinBtn = row.Q<Button>("join-btn");
        var leaveBtn = row.Q<Button>("leave-btn");

        if (title != null) title.text = string.IsNullOrEmpty(s.Name) ? s.Id : s.Name;
        if (count != null) count.text = $"{s.CurrentPlayers}/{s.MaxPlayers}";

        // IMPORTANT: ne pas empiler les handlers. Remplacer la Clickable à chaque bind.
        if (joinBtn != null)
            joinBtn.clickable = new Clickable(() =>
            {
                if (_isLeaving || _isRefreshing || _isJoining) return;
                OnJoinClicked(row, s.Id);
            });

        if (leaveBtn != null)
            leaveBtn.clickable = new Clickable(() =>
            {
                if (_isLeaving || _isRefreshing || _isJoining) return;
                if (currentJoinedSession != null && currentJoinedSession.Id == s.Id)
                    _ = leaveSession(row, currentJoinedSession);
            });

        // État des boutons: dépend uniquement de la session join actuelle (pas du refresh)
        var isCurrent = currentJoinedSession != null && currentJoinedSession.Id == s.Id;

        if (joinBtn != null)
        {
            joinBtn.text = isCurrent ? "Joined" : "Join";
            // Ne pas utiliser _isRefreshing pour l'état visuel; l'action sera bloquée dans le handler
            joinBtn.SetEnabled(!isCurrent && !_isJoining && !_isLeaving /* && !_isRefreshing */);
        }

        if (leaveBtn != null)
            // Laisser Leave activé visuellement quand on est dans cette session
            leaveBtn.SetEnabled(isCurrent && !_isLeaving);

        // Le bouton Create est global: activé si aucune session n’est rejointe
        UpdateCreateButtonState();
    }

    private void updatePostJoinButtons(VisualElement row, ISession joinedSession)
    {
        if (row == null || joinedSession == null) return;

        // Ne manipuler l'UI que si l'élément est toujours attaché
        if (row.panel == null) return;

        var container = _listView.Q<ScrollView>()?.contentContainer;
        if (container != null && container.panel != null)
            foreach (var visualElement in container.Children())
            {
                var otherJoin = visualElement.Q<Button>("join-btn");
                otherJoin?.SetEnabled(false);
            }

        var joinedBtn = row.Q<Button>("join-btn");
        var leaveBtn = row.Q<Button>("leave-btn");

        if (joinedBtn != null)
        {
            joinedBtn.text = "Joined";
            joinedBtn.SetEnabled(false);
        }

        if (leaveBtn != null)
        {
            leaveBtn.SetEnabled(true);
            // Remplacer le handler pour éviter l'empilement
            leaveBtn.clickable = new Clickable(() => { _ = leaveSession(row, joinedSession); });
        }

        // Pendant que l’on est dans une session, Create est désactivé
        _createBtn?.SetEnabled(false);
    }

    private void UpdateCreateButtonState()
    {
        var enableCreate = currentJoinedSession == null && !_isLeaving && !_isJoining && !_isCreating && !_isRefreshing;
        _createBtn?.SetEnabled(enableCreate);
        _refreshBtn?.SetEnabled(!_isRefreshing && !_isJoining && !_isLeaving);
    }

    private async Task leaveSession(VisualElement row, ISession joinedSession)
    {
        if (_isLeaving || joinedSession == null) return;

        _isLeaving = true;
        UpdateCreateButtonState();

        try
        {
            // Mémoriser avant l’appel réseau
            prevSessionName = joinedSession.Name;
            prevSessionId = joinedSession.Id;

            // Désactiver temporairement les boutons de la ligne (si encore attachée)
            if (row != null && row.panel != null)
            {
                var leaveBtn0 = row.Q<Button>("leave-btn");
                var joinBtn0 = row.Q<Button>("join-btn");
                leaveBtn0?.SetEnabled(false);
                joinBtn0?.SetEnabled(false);
            }

            // Appel réseau: quitter la session
            await joinedSession.LeaveAsync();

            // Remettre l’état local immédiatement
            currentJoinedSession = null;

            Debug.Log("session left");

            // Rafraîchir la liste après le leave (forcer le refresh pour bypasser le throttling)
            await RefreshAsync(true);
        }
        catch (Exception e)
        {
            Debug.LogError($"Échec leave: {e.Message}");
            // Réactiver le bouton leave si la row est toujours attachée
            if (row != null && row.panel != null)
            {
                var leaveBtn = row.Q<Button>("leave-btn");
                leaveBtn?.SetEnabled(true);
            }
        }
        finally
        {
            _isLeaving = false;
            UpdateCreateButtonState();
        }
    }

    private async void OnCreateClicked()
    {
        if (_isCreating || _isJoining || _isLeaving || _isRefreshing) return;

        _isCreating = true;
        UpdateCreateButtonState();

        var name = string.IsNullOrWhiteSpace(_nameField.value) ? "MyLobby" : _nameField.value.Trim();
        var max = Mathf.Max(1, _maxField.value);

        try
        {
            if (_items.Any(item => item.Name == name))
            {
                Debug.Log("Lobby avec le meme nom existe deja");
            }
            else
            {
                var options = new SessionOptions { Name = name, MaxPlayers = max };
                var session = await MultiplayerService.Instance.CreateSessionAsync(options);
                currentJoinedSession = session;
                Debug.Log($"Session créée: {session.Id}");
                await RefreshAsync(true);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Échec création: {e.Message}");
        }
        finally
        {
            _isCreating = false;
            UpdateCreateButtonState();
        }
    }

    private async void OnJoinClicked(VisualElement row, string id)
    {
        if (_isJoining || _isLeaving || _isRefreshing) return;

        _isJoining = true;
        // Désactive rapidement le bouton sur la ligne si encore attachée
        if (row != null && row.panel != null)
        {
            var joinBtn = row.Q<Button>("join-btn");
            joinBtn?.SetEnabled(false);
        }

        UpdateCreateButtonState();

        try
        {
            var joinedSession = await MultiplayerService.Instance.JoinSessionByIdAsync(id);
            currentJoinedSession = joinedSession;
            Debug.Log($"Rejoint: {joinedSession.Id}");
            updatePostJoinButtons(row, joinedSession); // retour visuel immédiat (protégé par panel != null)
            await RefreshAsync(true);
        }
        catch (Exception e)
        {
            Debug.LogError($"Échec join: {e.Message}");
        }
        finally
        {
            _isJoining = false;
            UpdateCreateButtonState();
        }
    }

    private async void OnRefreshClicked()
    {
        await RefreshAsync();
    }

    private async Task RefreshAsync(bool force = false)
    {
        if (_isRefreshing) return;

        var now = Time.unscaledTime;
        if (!force && now - _lastRefreshTime < _minRefreshIntervalSeconds) return;

        _isRefreshing = true;
        UpdateCreateButtonState();

        try
        {
            // On “réserve” le créneau pour amortir le spam
            _lastRefreshTime = now;

            var result = await MultiplayerService.Instance.QuerySessionsAsync(new QuerySessionsOptions());

            // Détacher temporairement la source pour éviter les conflits de virtualisation
            if (_listView != null) _listView.itemsSource = null;

            _items.Clear();
            if (result?.Sessions != null)
                foreach (var s in result.Sessions)
                    _items.Add(new SessionItem
                    {
                        Id = s.Id,
                        Name = TryGetString(s, "Name") ?? TryGetString(s, "DisplayName") ?? s.Id,
                        MaxPlayers = TryGetInt(s, "MaxPlayers", 0),
                        AvailableSlots = TryGetInt(s, "AvailableSlots", 0),
                        data = s
                    });

            if (_listView != null)
            {
                _listView.itemsSource = _items;
                // Préférer RefreshItems pour limiter les reconstructions profondes pendant la traversée de styles
                _listView.RefreshItems();
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception e)
        {
            Debug.LogError($"Échec refresh: {e.Message}");
        }
        finally
        {
            _isRefreshing = false;
            UpdateCreateButtonState();
            // Important: rebind après la fin du refresh pour réévaluer l'état des boutons
            _listView?.RefreshItems();
        }
    }

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
        public ISessionInfo data;
        public string Id;
        public int MaxPlayers;
        public string Name;
        public int CurrentPlayers => Mathf.Clamp(MaxPlayers - AvailableSlots, 0, MaxPlayers);
    }
}